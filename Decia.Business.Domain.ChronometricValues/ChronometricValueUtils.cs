using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues.TimeDimensions;

namespace Decia.Business.Domain.ChronometricValues
{
    public static class ChronometricValueUtils
    {
        public static object[] GetValuesAsObjects(this ChronometricValue chronometricValue)
        {
            if (chronometricValue == ChronometricValue.NullInstanceAsObject)
            { throw new InvalidOperationException("The ChronometricValue argument must be non-null."); }

            var timeKeys = chronometricValue.TimeKeys;
            object[] valuesAsObjects = new object[timeKeys.Count];

            for (int i = 0; i < timeKeys.Count; i++)
            {
                valuesAsObjects[i] = chronometricValue.GetValue(timeKeys[i]).GetValue();
            }
            return valuesAsObjects;
        }

        public static IList<MultiTimePeriodKey> GetImpliedTimeKeys(ChronometricValue chronometricValue)
        {
            if (chronometricValue == ChronometricValue.NullInstanceAsObject)
            { throw new InvalidOperationException("The ChronometricValue argument must be non-null."); }

            return chronometricValue.TimeDimensionSet.GenerateTimeKeysForTimeDimensionSet(chronometricValue.StoredTimeKeys);
        }

        public static IList<MultiTimePeriodKey> GetTimeKeysForIteration(ChronometricValue first, ChronometricValue second)
        {
            if ((first == ChronometricValue.NullInstanceAsObject) || (second == ChronometricValue.NullInstanceAsObject))
            { throw new InvalidOperationException("The ChronometricValue arguments must be non-null."); }

            TimeComparisonResult primaryTimeDimesionResult = first.PrimaryTimeDimension.CompareTimeTo(second.PrimaryTimeDimension);
            TimeComparisonResult secondaryTimeDimesionResult = first.SecondaryTimeDimension.CompareTimeTo(second.SecondaryTimeDimension);

            if ((primaryTimeDimesionResult == TimeComparisonResult.ThisIsLessGranular)
                && (secondaryTimeDimesionResult == TimeComparisonResult.ThisIsLessGranular))
            {
                return second.TimeKeys.ToList();
            }
            else if ((primaryTimeDimesionResult == TimeComparisonResult.ThisIsAsGranular)
            && (secondaryTimeDimesionResult == TimeComparisonResult.ThisIsAsGranular))
            {
                return first.TimeKeys.ToList();
            }
            else if ((primaryTimeDimesionResult == TimeComparisonResult.ThisIsMoreGranular)
                && (secondaryTimeDimesionResult == TimeComparisonResult.ThisIsMoreGranular))
            {
                return first.TimeKeys.ToList();
            }
            else if (((primaryTimeDimesionResult == TimeComparisonResult.ThisIsLessGranular) || (primaryTimeDimesionResult == TimeComparisonResult.ThisIsAsGranular))
                && ((secondaryTimeDimesionResult == TimeComparisonResult.ThisIsAsGranular) || (secondaryTimeDimesionResult == TimeComparisonResult.ThisIsMoreGranular)))
            {
                HashSet<TimePeriod> primaryPeriods = new HashSet<TimePeriod>();
                HashSet<TimePeriod> secondaryPeriods = new HashSet<TimePeriod>();

                foreach (MultiTimePeriodKey firstTimeKey in first.TimeKeys)
                {
                    if (firstTimeKey.HasSecondaryTimePeriod)
                    { secondaryPeriods.Add(firstTimeKey.SecondaryTimePeriod); }
                }
                foreach (MultiTimePeriodKey secondTimeKey in second.TimeKeys)
                {
                    if (secondTimeKey.HasPrimaryTimePeriod)
                    { primaryPeriods.Add(secondTimeKey.PrimaryTimePeriod); }
                }

                return MultiTimePeriodKey.GetKeyCombinationsForPeriods(primaryPeriods, secondaryPeriods);
            }
            else if (((primaryTimeDimesionResult == TimeComparisonResult.ThisIsAsGranular) || (primaryTimeDimesionResult == TimeComparisonResult.ThisIsMoreGranular))
                && ((secondaryTimeDimesionResult == TimeComparisonResult.ThisIsLessGranular) || (secondaryTimeDimesionResult == TimeComparisonResult.ThisIsAsGranular)))
            {
                HashSet<TimePeriod> primaryPeriods = new HashSet<TimePeriod>();
                HashSet<TimePeriod> secondaryPeriods = new HashSet<TimePeriod>();

                foreach (MultiTimePeriodKey firstTimeKey in first.TimeKeys)
                {
                    if (firstTimeKey.HasPrimaryTimePeriod)
                    { primaryPeriods.Add(firstTimeKey.PrimaryTimePeriod); }
                }
                foreach (MultiTimePeriodKey secondTimeKey in second.TimeKeys)
                {
                    if (secondTimeKey.HasSecondaryTimePeriod)
                    { secondaryPeriods.Add(secondTimeKey.SecondaryTimePeriod); }
                }

                return MultiTimePeriodKey.GetKeyCombinationsForPeriods(primaryPeriods, secondaryPeriods);
            }
            else
            { throw new InvalidOperationException("Unexpected TimeComparisonResult encountered."); }
        }

        public static ITimeDimensionSet GetDimensionsForResult(ChronometricValue first, ChronometricValue second)
        {
            if ((first == ChronometricValue.NullInstanceAsObject) || (second == ChronometricValue.NullInstanceAsObject))
            { throw new InvalidOperationException("The ChronometricValue arguments must be non-null."); }

            return ITimeDimensionSetUtils.GetDimensionsForResult(first.TimeDimensionSet, second.TimeDimensionSet);
        }

        public static ChronometricValue CreateDimensionedResult(ChronometricValue first, ChronometricValue second, DeciaDataType resultingDataType)
        {
            if ((first == ChronometricValue.NullInstanceAsObject) || (second == ChronometricValue.NullInstanceAsObject))
            { throw new InvalidOperationException("The ChronometricValue arguments must be non-null."); }

            var projectMemberId = first.GetProjectMemberId_MostSpecific(second);
            ITimeDimensionSet resultingDimensions = GetDimensionsForResult(first, second);

            ChronometricValue newResult = new ChronometricValue(projectMemberId.ProjectGuid, projectMemberId.RevisionNumber_NonNull);
            newResult.DataType = resultingDataType;
            newResult.ReDimension(resultingDimensions);

            return newResult;
        }
    }
}