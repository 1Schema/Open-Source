using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public static class ITimeDimensionUtils
    {
        public static ICollection<TimePeriod> GeneratePeriodsForTimeDimension(this ITimeDimension timeDimension)
        {
            return timeDimension.GeneratePeriodsForTimeDimension(timeDimension.FirstPeriodStartDate, timeDimension.LastPeriodEndDate);
        }

        public static ICollection<TimePeriod> GeneratePeriodsForTimeDimension(this ITimeDimension timeDimension, ICurrentState currentState)
        {
            return timeDimension.GeneratePeriodsForTimeDimension(currentState.ModelStartDate, currentState.ModelEndDate);
        }

        public static ICollection<TimePeriod> GeneratePeriodsForTimeDimension(this ITimeDimension timeDimension, DateTime firstPeriodStartDate, DateTime lastPeriodEndDate)
        {
            return TimePeriodUtils.GeneratePeriodsForTimeDimension(timeDimension.TimePeriodType, firstPeriodStartDate, lastPeriodEndDate);
        }

        public static bool Equals(this ITimeDimension first, ITimeDimension second)
        {
            bool checkDates = true;
            return Equals(first, second, checkDates);
        }

        public static bool Equals(this ITimeDimension first, ITimeDimension second, bool checkDates)
        {
            if (first == null)
            { throw new InvalidOperationException("The first ITimeDimension to compare must be non-null."); }
            if (second == null)
            { throw new InvalidOperationException("The second ITimeDimension to compare must be non-null."); }

            if (first.TimeDimensionType != second.TimeDimensionType)
            { return false; }
            if (first.HasTimeValue != second.HasTimeValue)
            { return false; }

            if (first.HasTimeValue)
            {
                if (first.TimeValueType != second.TimeValueType)
                { return false; }

                if (first.TimeValueType == TimeValueType.PeriodValue)
                {
                    if (first.TimePeriodType != second.TimePeriodType)
                    { return false; }
                    if (checkDates)
                    {
                        if (first.FirstPeriodStartDate != second.FirstPeriodStartDate)
                        { return false; }
                        if (first.LastPeriodEndDate != second.LastPeriodEndDate)
                        { return false; }
                    }
                }
            }
            return true;
        }

        public static TimeComparisonResult CompareTimeTo(this ITimeDimension first, ITimeDimension second)
        {
            if (first == null)
            { throw new InvalidOperationException("The first ITimeDimension to compare must be non-null."); }
            if (second == null)
            { throw new InvalidOperationException("The second ITimeDimension to compare must be non-null."); }

            if (!first.HasTimeValue && !second.HasTimeValue)
            { return TimeComparisonResult.ThisIsAsGranular; }
            if (first.HasTimeValue != second.HasTimeValue)
            {
                if (!first.HasTimeValue)
                { return TimeComparisonResult.ThisIsLessGranular; }
                else
                { return TimeComparisonResult.ThisIsMoreGranular; }
            }
            if ((first.TimeValueType != second.TimeValueType)
                || (first.TimeValueType == TimeValueType.SpotValue)
                || (second.TimeValueType == TimeValueType.SpotValue))
            {
                return first.TimeValueType.CompareTimeTo(second.TimeValueType);
            }
            if (first.TimePeriodType != second.TimePeriodType)
            {
                return first.TimePeriodType.CompareTimeTo(second.TimePeriodType);
            }
            return TimeComparisonResult.ThisIsAsGranular;
        }


        public static bool TryAssertTimeDimensionTypeIsValid(TimeDimensionType timeDimensionType)
        {
            return TimeDimensionTypeUtils.TryAssertTimeDimensionTypeIsDirectlyUsable(timeDimensionType);
        }

        public static void AssertTimeDimensionTypeIsValid(TimeDimensionType timeDimensionType)
        {
            TimeDimensionTypeUtils.AssertTimeDimensionTypeIsDirectlyUsable(timeDimensionType);
        }

        public static bool TryAssertSecondaryTimeDimensionIsValid(TimeDimensionType timeDimensionType, Nullable<TimeValueType> timeValueType)
        {
            if ((timeDimensionType == TimeDimensionType.Secondary) && (timeValueType == TimeValueType.SpotValue))
            { return false; }
            return true;
        }

        public static void AssertSecondaryTimeDimensionIsValid(TimeDimensionType timeDimensionType, Nullable<TimeValueType> timeValueType)
        {
            if (!TryAssertSecondaryTimeDimensionIsValid(timeDimensionType, timeValueType))
            { throw new InvalidOperationException("The secondary TimeDimension currently only supports period values."); }
        }

        public static bool TryAssertTimeValueTypeIsValid(Nullable<TimeValueType> timeValueType, Nullable<TimePeriodType> timePeriodType, ref string message)
        {
            if (!timeValueType.HasValue && timePeriodType.HasValue)
            {
                message = "The TimePeriodType must be null when the TimeValueType is null.";
                return false;
            }
            if ((timeValueType == TimeValueType.SpotValue) && timePeriodType.HasValue)
            {
                message = "The TimePeriodType must be null when the TimeValueType is SpotValue.";
                return false;
            }
            return true;
        }

        public static void AssertTimeValueTypeIsValid(Nullable<TimeValueType> timeValueType, Nullable<TimePeriodType> timePeriodType)
        {
            string message = string.Empty;
            if (!TryAssertTimeValueTypeIsValid(timeValueType, timePeriodType, ref message))
            { throw new InvalidOperationException(message); }
        }
    }
}