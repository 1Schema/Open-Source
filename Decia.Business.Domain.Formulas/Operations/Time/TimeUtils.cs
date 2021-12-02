using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;

namespace Decia.Business.Domain.Formulas.Operations.Time
{
    public static class TimeUtils
    {
        public const string TimeCategoryName = "Time";
        public const bool DefaultAllowShiftBeforeTimeFrame = true;
        public const bool DefaultAllowShiftAfterTimeFrame = true;

        #region TimePeriod Methods

        public static List<TimePeriod> GenerateRelevantTimePeriods(this ITimeDimension timeDimension, TimePeriod? currentPrimaryPeriod)
        {
            var timePeriods = new List<TimePeriod>();
            if (!timeDimension.HasTimeValue)
            {
                timePeriods.Add(TimePeriod.ForeverPeriod);
                return timePeriods;
            }

            timePeriods = timeDimension.GeneratePeriodsForTimeDimension().ToList();
            if (!currentPrimaryPeriod.HasValue)
            { return timePeriods; }

            timePeriods = timePeriods.Where(x => currentPrimaryPeriod.Value.ContainsOrEquals(x)).ToList();
            return timePeriods;
        }

        public static int GetCurrentPeriodIndex(this IList<TimePeriod> periods, TimePeriod currentPeriod)
        {
            if (!periods.Contains(currentPeriod))
            { throw new InvalidOperationException("The current Period must be in the list of Periods to compute its index."); }

            return periods.IndexOf(currentPeriod);
        }

        #endregion

        #region Introspection Methods

        public static bool GetTimeDimensionSet_ForIntrospection(ICurrentState currentState, IDictionary<int, OperationMember> inputs, Parameter? valueParameter, out ITimeDimensionSet resultingTimeDimensionality)
        {
            var valueToUse = (ITimeDimensionSet)null;
            if (valueParameter.HasValue && (inputs.Values.Count > valueParameter.Value.Index))
            {
                valueToUse = inputs.Values.ElementAt(valueParameter.Value.Index).TimeDimesionality;
            }

            if (valueToUse != (object)null)
            {
                resultingTimeDimensionality = new TimeDimensionSet(valueToUse.PrimaryTimeDimension, valueToUse.SecondaryTimeDimension);
                return true;
            }

            var primaryTimeDimension = (currentState.PrimaryPeriodType.HasValue) ? new TimeDimension(TimeDimensionType.Primary, TimeValueType.PeriodValue, currentState.PrimaryPeriodType, currentState.ModelStartDate, currentState.ModelEndDate) : (ITimeDimension)null;
            var secondaryTimeDimension = (currentState.SecondaryPeriodType.HasValue) ? new TimeDimension(TimeDimensionType.Secondary, TimeValueType.PeriodValue, currentState.SecondaryPeriodType, currentState.ModelStartDate, currentState.ModelEndDate) : (ITimeDimension)null;

            resultingTimeDimensionality = new TimeDimensionSet(primaryTimeDimension, secondaryTimeDimension);
            return true;
        }

        #endregion

        #region Discrete Match Methods

        public static TimePeriod GetDesiredPeriodForMatchingValue(this ITimeDimension timeDimension, TimePeriod currentPeriod, int matchingValueAsIndex, ICurrentState currentState)
        {
            var periods = timeDimension.GeneratePeriodsForTimeDimension(currentState).ToList();
            return periods.GetDesiredPeriodForRetrievalValue(timeDimension.TimePeriodType, matchingValueAsIndex);
        }

        public static TimePeriod GetDesiredPeriodForMatchingValue(this ITimeDimension timeDimension, TimePeriod currentPeriod, DateTime matchingValueAsDate, ICurrentState currentState)
        {
            var periods = timeDimension.GeneratePeriodsForTimeDimension(currentState).ToList();
            return periods.GetDesiredPeriodForRetrievalValue(timeDimension.TimePeriodType, matchingValueAsDate);
        }

        #endregion

        #region Relative Offset Methods

        public static TimePeriod GetDesiredPeriodForOffsetAmount(this IList<TimePeriod> periods, TimePeriodType periodType, TimePeriod currentPeriod, int offsetAsPeriodCount)
        {
            int currentIndex = periods.IndexOf(currentPeriod);
            int adjustedIndex = currentIndex + offsetAsPeriodCount;

            return periods.GetDesiredPeriodForRetrievalValue(periodType, adjustedIndex);
        }

        public static TimePeriod GetDesiredPeriodForOffsetAmount(this IList<TimePeriod> periods, TimePeriodType periodType, TimePeriod currentPeriod, TimeSpan offsetAsTimeSpan, double periodUsageRatio)
        {
            DateTime desiredContainedDate;
            if (currentPeriod.TimeValueType == TimeValueType.SpotValue)
            {
                desiredContainedDate = currentPeriod.StartDate.Add(offsetAsTimeSpan);
            }
            else
            {
                double ratioSpanInMs = (currentPeriod.EndDate - currentPeriod.StartDate).TotalMilliseconds * periodUsageRatio;
                DateTime currentStartDate = currentPeriod.StartDate.AddMilliseconds(ratioSpanInMs);
                desiredContainedDate = currentStartDate.Add(offsetAsTimeSpan);
            }

            return periods.GetDesiredPeriodForRetrievalValue(periodType, desiredContainedDate);
        }

        #endregion

        #region Helper Methods

        public static TimePeriod GetDesiredPeriodForRetrievalValue(this IList<TimePeriod> periods, TimePeriodType periodType, int indexToRetrieve)
        {
            int minIndex = periods.Select(p => p.Index).Min();
            int maxIndex = periods.Select(p => p.Index).Max();

            if ((!DefaultAllowShiftBeforeTimeFrame && (indexToRetrieve < minIndex))
                || (!DefaultAllowShiftAfterTimeFrame && (indexToRetrieve > maxIndex)))
            {
                indexToRetrieve = (indexToRetrieve < minIndex) ? minIndex : maxIndex;
                return periods[indexToRetrieve];
            }

            if ((minIndex <= indexToRetrieve) && (indexToRetrieve <= maxIndex))
            { return periods.Where(p => p.Index == indexToRetrieve).First(); }

            var zeroIndexPeriod = periods.Where(p => p.Index == 0).First();
            var adjustedStartDate = periodType.GetStartDateForPeriodIndex(zeroIndexPeriod.StartDate, indexToRetrieve);
            var adjustedEndDate = periodType.GetEndDateForPeriodIndex(zeroIndexPeriod.StartDate, indexToRetrieve);

            return new TimePeriod(adjustedStartDate, adjustedEndDate, indexToRetrieve);
        }

        public static TimePeriod GetDesiredPeriodForRetrievalValue(this IList<TimePeriod> periods, TimePeriodType periodType, DateTime dateToRetrieve)
        {
            TimePeriod minPeriod = periods.Min();
            TimePeriod maxPeriod = periods.Max();

            TimePeriod desiredContainedPeriod = new TimePeriod(dateToRetrieve, dateToRetrieve);
            var matchingPeriods = periods.GetContainingPeriods(desiredContainedPeriod);

            Nullable<TimePeriod> matchingPeriod = null;
            if (matchingPeriods.Count > 0)
            { matchingPeriod = matchingPeriods.First(); }

            if (!matchingPeriod.HasValue)
            {
                if ((!DefaultAllowShiftBeforeTimeFrame && (dateToRetrieve <= minPeriod.StartDate))
                || (!DefaultAllowShiftAfterTimeFrame && (dateToRetrieve >= maxPeriod.EndDate)))
                {
                    matchingPeriod = (dateToRetrieve < minPeriod.StartDate) ? minPeriod : maxPeriod;
                    return matchingPeriod.Value;
                }
                else
                {

                    if (dateToRetrieve < minPeriod.StartDate)
                    {
                        var newPeriodStartDate = minPeriod.StartDate;
                        int count = 0;

                        while (dateToRetrieve <= newPeriodStartDate)
                        {
                            newPeriodStartDate = periodType.GetPreviousStartDate(newPeriodStartDate);
                            count++;
                        }

                        matchingPeriod = new TimePeriod(newPeriodStartDate, periodType.GetCurrentEndDate(newPeriodStartDate), minPeriod.Index - count);
                    }
                    else
                    {
                        var newPeriodStartDate = maxPeriod.StartDate;
                        int count = 0;

                        while (dateToRetrieve > periodType.GetCurrentEndDate(newPeriodStartDate))
                        {
                            newPeriodStartDate = periodType.GetNextStartDate(newPeriodStartDate);
                            count++;
                        }

                        matchingPeriod = new TimePeriod(newPeriodStartDate, periodType.GetCurrentEndDate(newPeriodStartDate), maxPeriod.Index + count);
                    }
                }
            }

            if (!matchingPeriod.HasValue)
            { throw new InvalidOperationException("Could not find matching Period."); }

            return matchingPeriod.Value;
        }

        #endregion
    }
}