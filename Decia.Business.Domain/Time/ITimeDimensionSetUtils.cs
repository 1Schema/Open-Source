using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public static class ITimeDimensionSetUtils
    {
        public static IList<MultiTimePeriodKey> GenerateTimeKeysForTimeDimensionSet(this ITimeDimensionSet timeDimensionSet)
        {
            ICollection<MultiTimePeriodKey> storedKeys = null;
            return timeDimensionSet.GenerateTimeKeysForTimeDimensionSet(storedKeys);
        }

        public static IList<MultiTimePeriodKey> GenerateTimeKeysForTimeDimensionSet(this ITimeDimensionSet timeDimensionSet, ICollection<MultiTimePeriodKey> storedKeys)
        {
            if (timeDimensionSet == null)
            { throw new InvalidOperationException("The ChronometricValue argument must be non-null."); }
            if (storedKeys == null)
            { storedKeys = new List<MultiTimePeriodKey>(); }

            ITimeDimension primaryTimeDimesion = timeDimensionSet.PrimaryTimeDimension;
            ITimeDimension secondaryTimeDimesion = timeDimensionSet.SecondaryTimeDimension;

            if (!primaryTimeDimesion.HasTimeValue && !secondaryTimeDimesion.HasTimeValue)
            {
                if (storedKeys.Contains(MultiTimePeriodKey.DimensionlessTimeKey))
                { return new List<MultiTimePeriodKey>(storedKeys); }

                IList<MultiTimePeriodKey> result = new List<MultiTimePeriodKey>();
                result.Add(MultiTimePeriodKey.DimensionlessTimeKey);
                return result;
            }
            else if (primaryTimeDimesion.HasTimeValue && !secondaryTimeDimesion.HasTimeValue)
            {
                if (primaryTimeDimesion.TimeValueType == TimeValueType.SpotValue)
                { return new List<MultiTimePeriodKey>(storedKeys); }
                else if (primaryTimeDimesion.TimeValueType == TimeValueType.PeriodValue)
                {
                    ICollection<TimePeriod> primaryTimePeriods = primaryTimeDimesion.GeneratePeriodsForTimeDimension();
                    List<MultiTimePeriodKey> result = new List<MultiTimePeriodKey>(storedKeys);

                    foreach (TimePeriod primaryTimePeriod in primaryTimePeriods)
                    {
                        MultiTimePeriodKey timeKey = new MultiTimePeriodKey(primaryTimePeriod, null);
                        if (!result.Contains(timeKey))
                        { result.Add(timeKey); }
                    }
                    return result;
                }
                else
                { throw new InvalidOperationException("Unexpected TimeValueType encountered."); }
            }
            else if (!primaryTimeDimesion.HasTimeValue && secondaryTimeDimesion.HasTimeValue)
            {
                if (secondaryTimeDimesion.TimeValueType == TimeValueType.SpotValue)
                { return new List<MultiTimePeriodKey>(storedKeys); }
                else if (secondaryTimeDimesion.TimeValueType == TimeValueType.PeriodValue)
                {
                    ICollection<TimePeriod> secondaryTimePeriods = secondaryTimeDimesion.GeneratePeriodsForTimeDimension();
                    List<MultiTimePeriodKey> result = new List<MultiTimePeriodKey>(storedKeys);

                    foreach (TimePeriod secondaryTimePeriod in secondaryTimePeriods)
                    {
                        MultiTimePeriodKey timeKey = new MultiTimePeriodKey(null, secondaryTimePeriod);
                        if (!result.Contains(timeKey))
                        { result.Add(timeKey); }
                    }
                    return result;
                }
                else
                { throw new InvalidOperationException("Unexpected TimeValueType encountered."); }
            }
            else if (primaryTimeDimesion.HasTimeValue && secondaryTimeDimesion.HasTimeValue)
            {
                if ((primaryTimeDimesion.TimeValueType == TimeValueType.SpotValue)
                    || (secondaryTimeDimesion.TimeValueType == TimeValueType.SpotValue))
                { return new List<MultiTimePeriodKey>(storedKeys); }
                else if (primaryTimeDimesion.TimeValueType == TimeValueType.PeriodValue)
                {
                    ICollection<TimePeriod> primaryTimePeriods = primaryTimeDimesion.GeneratePeriodsForTimeDimension();
                    ICollection<TimePeriod> secondaryTimePeriods = secondaryTimeDimesion.GeneratePeriodsForTimeDimension();
                    List<MultiTimePeriodKey> result = new List<MultiTimePeriodKey>(storedKeys);

                    foreach (TimePeriod primaryTimePeriod in primaryTimePeriods)
                    {
                        foreach (TimePeriod secondaryTimePeriod in secondaryTimePeriods)
                        {
                            MultiTimePeriodKey timeKey = new MultiTimePeriodKey(primaryTimePeriod, secondaryTimePeriod);
                            if (!result.Contains(timeKey))
                            { result.Add(timeKey); }
                        }
                    }
                    return result;
                }
                else
                { throw new InvalidOperationException("Unexpected TimeValueType encountered."); }
            }
            else
            { throw new InvalidOperationException("Unexpected Time Dimensioning encountered."); }
        }

        public static MultiTimePeriodKey ConvertTimeKeyToTimeDimensionSet(this ITimeDimensionSet timeDimensionSet, MultiTimePeriodKey originalKey)
        {
            if (timeDimensionSet == null)
            { throw new InvalidOperationException("The ChronometricValue argument must be non-null."); }

            ITimeDimension primaryTimeDimesion = timeDimensionSet.PrimaryTimeDimension;
            ITimeDimension secondaryTimeDimesion = timeDimensionSet.SecondaryTimeDimension;

            if (!primaryTimeDimesion.HasTimeValue && !secondaryTimeDimesion.HasTimeValue)
            {
                return MultiTimePeriodKey.DimensionlessTimeKey;
            }
            else if (primaryTimeDimesion.HasTimeValue && !secondaryTimeDimesion.HasTimeValue)
            {
                if (primaryTimeDimesion.TimeValueType == TimeValueType.SpotValue)
                {
                    return new MultiTimePeriodKey(originalKey.NullablePrimaryTimePeriod, null);
                }
                else if (primaryTimeDimesion.TimeValueType == TimeValueType.PeriodValue)
                {
                    return new MultiTimePeriodKey(originalKey.NullablePrimaryTimePeriod, null);
                }
                else
                { throw new InvalidOperationException("Unexpected TimeValueType encountered."); }
            }
            else if (!primaryTimeDimesion.HasTimeValue && secondaryTimeDimesion.HasTimeValue)
            {
                if (secondaryTimeDimesion.TimeValueType == TimeValueType.SpotValue)
                {
                    return new MultiTimePeriodKey(null, originalKey.NullableSecondaryTimePeriod);
                }
                else if (secondaryTimeDimesion.TimeValueType == TimeValueType.PeriodValue)
                {
                    return new MultiTimePeriodKey(null, originalKey.NullableSecondaryTimePeriod);
                }
                else
                { throw new InvalidOperationException("Unexpected TimeValueType encountered."); }
            }
            else if (primaryTimeDimesion.HasTimeValue && secondaryTimeDimesion.HasTimeValue)
            {
                if ((primaryTimeDimesion.TimeValueType == TimeValueType.SpotValue)
                    || (secondaryTimeDimesion.TimeValueType == TimeValueType.SpotValue))
                {
                    return originalKey;
                }
                else if (primaryTimeDimesion.TimeValueType == TimeValueType.PeriodValue)
                {
                    return originalKey;
                }
                else
                { throw new InvalidOperationException("Unexpected TimeValueType encountered."); }
            }
            else
            { throw new InvalidOperationException("Unexpected Time Dimensioning encountered."); }
        }

        public static bool Equals(this ITimeDimensionSet first, ITimeDimensionSet second)
        {
            if (first == null)
            { throw new InvalidOperationException("The first ITimeDimension to compare must be non-null."); }
            if (second == null)
            { throw new InvalidOperationException("The second ITimeDimension to compare must be non-null."); }


            if (!ITimeDimensionUtils.Equals(first.PrimaryTimeDimension, second.PrimaryTimeDimension))
            { return false; }
            if (!ITimeDimensionUtils.Equals(first.SecondaryTimeDimension, second.SecondaryTimeDimension))
            { return false; }
            return true;
        }

        public static TimeComparisonResult CompareTimeTo(this ITimeDimensionSet first, ITimeDimensionSet second)
        {
            if (first == null)
            { throw new InvalidOperationException("The first ITimeDimension to compare must be non-null."); }
            if (second == null)
            { throw new InvalidOperationException("The second ITimeDimension to compare must be non-null."); }

            TimeComparisonResult primaryResult = ITimeDimensionUtils.CompareTimeTo(first.PrimaryTimeDimension, second.PrimaryTimeDimension);
            TimeComparisonResult secondaryResult = ITimeDimensionUtils.CompareTimeTo(first.SecondaryTimeDimension, second.SecondaryTimeDimension);

            if (primaryResult != TimeComparisonResult.ThisIsAsGranular)
            { return primaryResult; }
            return secondaryResult;
        }

        public static ITimeDimensionSet GetDimensionsForResult(this ITimeDimensionSet first, ITimeDimensionSet second)
        {
            if ((first == null) || (second == null))
            { throw new InvalidOperationException("The ITimeDimensionSet arguments must be non-null."); }

            TimeComparisonResult primaryResult = ITimeDimensionUtils.CompareTimeTo(first.PrimaryTimeDimension, second.PrimaryTimeDimension);
            TimeComparisonResult secondaryResult = ITimeDimensionUtils.CompareTimeTo(first.SecondaryTimeDimension, second.SecondaryTimeDimension);

            ITimeDimension resultingPrimaryDimension = null;
            ITimeDimension resultingSecondaryDimension = null;

            if (primaryResult == TimeComparisonResult.ThisIsLessGranular)
            { resultingPrimaryDimension = new TimeDimension(second.PrimaryTimeDimension); }
            else if ((primaryResult == TimeComparisonResult.ThisIsAsGranular) || (primaryResult == TimeComparisonResult.ThisIsMoreGranular))
            { resultingPrimaryDimension = new TimeDimension(first.PrimaryTimeDimension); }
            else
            { throw new InvalidOperationException("Unexpected TimeComparisonResult encountered."); }

            if (secondaryResult == TimeComparisonResult.ThisIsLessGranular)
            { resultingSecondaryDimension = new TimeDimension(second.SecondaryTimeDimension); }
            else if ((secondaryResult == TimeComparisonResult.ThisIsAsGranular) || (secondaryResult == TimeComparisonResult.ThisIsMoreGranular))
            { resultingSecondaryDimension = new TimeDimension(first.SecondaryTimeDimension); }
            else
            { throw new InvalidOperationException("Unexpected TimeComparisonResult encountered."); }

            return new TimeDimensionSet(resultingPrimaryDimension, resultingSecondaryDimension);
        }

        public static ITimeDimensionSet GetDimensionsForResults(this IEnumerable<ITimeDimensionSet> sets)
        {
            var result = (ITimeDimensionSet)TimeDimensionSet.EmptyTimeDimensionSet;

            if (sets.Count() < 1)
            { return new TimeDimensionSet(result.PrimaryTimeDimension, result.SecondaryTimeDimension); }

            foreach (var set in sets)
            {
                if (set == null)
                { continue; }

                result = GetDimensionsForResult(result, set);
            }
            return result;
        }

        public static ITimeDimensionSet GetDimensionsForGroup(this IEnumerable<ITimeDimensionSet> groupSets, Nullable<DateTime> groupStartDate, Nullable<DateTime> groupEndDate)
        {
            if (groupSets == null)
            { throw new InvalidOperationException("The ITimeDimensionSet arguments must be non-null."); }
            if (groupSets.Contains(null))
            { throw new InvalidOperationException("The ITimeDimensionSet arguments must be non-null."); }

            var groupSetsAsList = new List<ITimeDimensionSet>(groupSets);

            if (groupSetsAsList.Count < 1)
            { return TimeDimensionSet.EmptyTimeDimensionSet; }

            ITimeDimensionSet combinedSet = groupSetsAsList[0];
            groupSetsAsList.Remove(combinedSet);

            while (groupSetsAsList.Count > 0)
            {
                var nextSet = groupSetsAsList[0];
                combinedSet = combinedSet.GetDimensionsForResult(nextSet);
                groupSetsAsList.Remove(nextSet);
            }

            if (!groupStartDate.HasValue && !groupEndDate.HasValue)
            { return combinedSet; }

            var updatedTimeDimesions = new List<ITimeDimension>();

            for (int dimNumber = combinedSet.MinimumDimensionNumber; dimNumber <= combinedSet.MaximumDimensionNumber; dimNumber++)
            {
                var timeDimesion = combinedSet.GetTimeDimension(dimNumber);

                if (!timeDimesion.HasTimeValue)
                {
                    updatedTimeDimesions.Add(timeDimesion);
                    continue;
                }

                DateTime startDate = groupStartDate.HasValue ? groupStartDate.Value : timeDimesion.FirstPeriodStartDate;
                DateTime endDate = groupEndDate.HasValue ? groupEndDate.Value : timeDimesion.LastPeriodEndDate;

                var updatedTimeDimesion = new TimeDimension(timeDimesion.TimeDimensionType, timeDimesion.NullableTimeValueType, timeDimesion.NullableTimePeriodType, startDate, endDate);
                updatedTimeDimesions.Add(updatedTimeDimesion);
            }

            var result = new TimeDimensionSet(updatedTimeDimesions[0], updatedTimeDimesions[1]);
            return result;
        }

        public static ITimeDimensionSet GetMostGranularTimeDimensions<T>(this IEnumerable<T> timeValuedObjects, Func<T, int> tdCountGetter, Func<T, TimeValueType> tvtGetter_Primary, Func<T, TimePeriodType> tptGetter_Primary, Func<T, TimeValueType> tvtGetter_Secondary, Func<T, TimePeriodType> tptGetter_Secondary, ITimeDimension defaultTimeDimension)
        {
            var defaultTimeDimension_Primary = new TimeDimension(TimeDimensionType.Primary, defaultTimeDimension);
            var defaultTimeDimension_Secondary = new TimeDimension(TimeDimensionType.Secondary, defaultTimeDimension);
            var defaultTimeDimensionSet = new TimeDimensionSet(defaultTimeDimension_Primary, defaultTimeDimension_Secondary);
            return GetMostGranularTimeDimensions<T>(timeValuedObjects, tdCountGetter, tvtGetter_Primary, tptGetter_Primary, tvtGetter_Secondary, tptGetter_Secondary, defaultTimeDimensionSet);
        }

        public static ITimeDimensionSet GetMostGranularTimeDimensions<T>(this IEnumerable<T> timeValuedObjects, Func<T, int> tdCountGetter, Func<T, TimeValueType> tvtGetter_Primary, Func<T, TimePeriodType> tptGetter_Primary, Func<T, TimeValueType> tvtGetter_Secondary, Func<T, TimePeriodType> tptGetter_Secondary, ITimeDimensionSet defaultTimeDimensionSet)
        {
            var defaultTimeDimension_Primary = defaultTimeDimensionSet.PrimaryTimeDimension;
            var defaultTimeDimension_Secondary = defaultTimeDimensionSet.SecondaryTimeDimension;

            var timePeriodType_Primary = (TimePeriodType?)null;
            var startDate_Primary = (DateTime?)null;
            var endDate_Primary = (DateTime?)null;

            var timePeriodType_Secondary = (TimePeriodType?)null;
            var startDate_Secondary = (DateTime?)null;
            var endDate_Secondary = (DateTime?)null;

            foreach (var variableTemplate in timeValuedObjects)
            {
                var tdCount = tdCountGetter(variableTemplate);

                if (tdCount >= 1)
                {
                    if (tvtGetter_Primary(variableTemplate) == TimeValueType.SpotValue)
                    { continue; }

                    var tpt_Primary = tptGetter_Primary(variableTemplate);

                    if (!timePeriodType_Primary.HasValue)
                    {
                        timePeriodType_Primary = tpt_Primary;
                        startDate_Primary = defaultTimeDimension_Primary.FirstPeriodStartDate;
                        endDate_Primary = defaultTimeDimension_Primary.LastPeriodEndDate;
                    }
                    else
                    {
                        var compareResult = timePeriodType_Primary.Value.CompareTimeTo(tpt_Primary);

                        if (compareResult == TimeComparisonResult.ThisIsLessGranular)
                        { timePeriodType_Primary = tpt_Primary; }
                        else if (compareResult == TimeComparisonResult.NotComparable)
                        { throw new InvalidOperationException("The Primary TimePeriodTypes are not comparable."); }
                    }
                }
                if (tdCount >= 2)
                {
                    if (tvtGetter_Secondary(variableTemplate) == TimeValueType.SpotValue)
                    { continue; }

                    var tpt_Secondary = tptGetter_Secondary(variableTemplate);

                    if (!timePeriodType_Secondary.HasValue)
                    {
                        timePeriodType_Secondary = tpt_Secondary;
                        startDate_Secondary = defaultTimeDimension_Secondary.FirstPeriodStartDate;
                        endDate_Secondary = defaultTimeDimension_Secondary.LastPeriodEndDate;
                    }
                    else
                    {
                        var compareResult = timePeriodType_Secondary.Value.CompareTimeTo(tpt_Secondary);

                        if (compareResult == TimeComparisonResult.ThisIsLessGranular)
                        { timePeriodType_Secondary = tpt_Secondary; }
                        else if (compareResult == TimeComparisonResult.NotComparable)
                        { throw new InvalidOperationException("The Secondary TimePeriodTypes are not comparable."); }
                    }
                }
            }

            var timeDimension_Primary = new TimeDimension(TimeDimensionType.Primary, null, null, null, null);
            if (timePeriodType_Primary.HasValue)
            { timeDimension_Primary = new TimeDimension(TimeDimensionType.Primary, TimeValueType.PeriodValue, timePeriodType_Primary, startDate_Primary, endDate_Primary); }

            var timeDimension_Secondary = new TimeDimension(TimeDimensionType.Secondary, null, null, null, null);
            if (timePeriodType_Secondary.HasValue)
            { timeDimension_Secondary = new TimeDimension(TimeDimensionType.Secondary, TimeValueType.PeriodValue, timePeriodType_Secondary, startDate_Secondary, endDate_Secondary); }

            return new TimeDimensionSet(timeDimension_Primary, timeDimension_Secondary);
        }
    }
}