using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Time
{
    public static class MultiTimePeriodKeyUtils
    {
        #region Methods - Get Relevant Keys (checks list TPs contain desired TP)

        public static ICollection<MultiTimePeriodKey> GetRelevantTimeKeys(this IEnumerable<MultiTimePeriodKey> allDefinedTimeKeys, Nullable<TimePeriod> desiredPrimaryPeriod, Nullable<TimePeriod> desiredSecondaryPeriod)
        {
            MultiTimePeriodKey timeKey = new MultiTimePeriodKey(desiredPrimaryPeriod, desiredSecondaryPeriod);
            return GetRelevantTimeKeys(allDefinedTimeKeys, timeKey);
        }

        public static ICollection<MultiTimePeriodKey> GetRelevantTimeKeys(this IEnumerable<MultiTimePeriodKey> allDefinedTimeKeys, MultiTimePeriodKey desiredTimeKey)
        {
            if (desiredTimeKey.HasPrimaryTimePeriod && desiredTimeKey.HasSecondaryTimePeriod)
            {
                return GetRelevantTimeKeysForPrimaryAndSecondaryTimeDimensions(allDefinedTimeKeys, desiredTimeKey.PrimaryTimePeriod, desiredTimeKey.SecondaryTimePeriod);
            }
            else if (desiredTimeKey.HasPrimaryTimePeriod && !desiredTimeKey.HasSecondaryTimePeriod)
            {
                return GetRelevantTimeKeysForPrimaryTimeDimensions(allDefinedTimeKeys, desiredTimeKey.PrimaryTimePeriod);
            }
            else if (!desiredTimeKey.HasPrimaryTimePeriod && desiredTimeKey.HasSecondaryTimePeriod)
            {
                return GetRelevantTimeKeysForSecondaryTimeDimensions(allDefinedTimeKeys, desiredTimeKey.SecondaryTimePeriod);
            }
            else if (!desiredTimeKey.HasPrimaryTimePeriod && !desiredTimeKey.HasSecondaryTimePeriod)
            {
                if (!allDefinedTimeKeys.Contains(MultiTimePeriodKey.DimensionlessTimeKey))
                { return new List<MultiTimePeriodKey>(); }
                return new List<MultiTimePeriodKey>(new MultiTimePeriodKey[] { MultiTimePeriodKey.DimensionlessTimeKey });
            }
            else
            { throw new InvalidOperationException("Invalid Time Key encountered."); }
        }

        private static ICollection<MultiTimePeriodKey> GetRelevantTimeKeysForPrimaryAndSecondaryTimeDimensions(this IEnumerable<MultiTimePeriodKey> timeKeys, TimePeriod primaryPeriod, TimePeriod secondaryPeriod)
        {
            return timeKeys.Where(tk => tk.PrimaryTimePeriod.ContainsOrEquals(primaryPeriod) && tk.SecondaryTimePeriod.ContainsOrEquals(secondaryPeriod)).ToList();
        }

        private static ICollection<MultiTimePeriodKey> GetRelevantTimeKeysForPrimaryTimeDimensions(IEnumerable<MultiTimePeriodKey> timeKeys, TimePeriod primaryPeriod)
        {
            return timeKeys.Where(tk => tk.PrimaryTimePeriod.ContainsOrEquals(primaryPeriod)).ToList();
        }

        private static ICollection<MultiTimePeriodKey> GetRelevantTimeKeysForSecondaryTimeDimensions(IEnumerable<MultiTimePeriodKey> timeKeys, TimePeriod secondaryPeriod)
        {
            return timeKeys.Where(tk => tk.SecondaryTimePeriod.ContainsOrEquals(secondaryPeriod)).ToList();
        }

        #endregion

        #region Methods - Get Corresponding Key (checks either TP contains the other)

        public static MultiTimePeriodKey? GetCorrespondingTimeKey(this IEnumerable<MultiTimePeriodKey> allDefinedTimeKeys, Nullable<TimePeriod> desiredPrimaryPeriod, Nullable<TimePeriod> desiredSecondaryPeriod)
        {
            MultiTimePeriodKey timeKey = new MultiTimePeriodKey(desiredPrimaryPeriod, desiredSecondaryPeriod);
            return GetCorrespondingTimeKey(allDefinedTimeKeys, timeKey);
        }

        public static MultiTimePeriodKey? GetCorrespondingTimeKey(this IEnumerable<MultiTimePeriodKey> allDefinedTimeKeys, MultiTimePeriodKey desiredTimeKey)
        {
            foreach (MultiTimePeriodKey definedTimeKey in allDefinedTimeKeys)
            {
                if (definedTimeKey.HasPrimaryTimePeriod)
                {
                    if (!desiredTimeKey.HasPrimaryTimePeriod)
                    { continue; }

                    TimePeriod currentTimePeriod = definedTimeKey.PrimaryTimePeriod;
                    if (currentTimePeriod.TimeValueType == TimeValueType.PeriodValue)
                    {
                        var definedTimePeriodType = definedTimeKey.PrimaryTimePeriod.InferredPeriodType.Value;
                        var desiredTimePeriodType = desiredTimeKey.PrimaryTimePeriod.InferredPeriodType.Value;
                        var timeComparison = definedTimePeriodType.CompareTimeTo(desiredTimePeriodType);

                        if (timeComparison == TimeComparisonResult.ThisIsAsGranular)
                        {
                            if (!currentTimePeriod.Equals(desiredTimeKey.PrimaryTimePeriod))
                            { continue; }
                        }
                        else if (timeComparison == TimeComparisonResult.ThisIsLessGranular)
                        {
                            if (!currentTimePeriod.Contains(desiredTimeKey.PrimaryTimePeriod, true, true))
                            { continue; }
                        }
                        else if (timeComparison == TimeComparisonResult.ThisIsMoreGranular)
                        {
                            if (!desiredTimeKey.PrimaryTimePeriod.Contains(currentTimePeriod, true, true))
                            { continue; }
                        }
                        else
                        { throw new InvalidOperationException("The TimePeriods are not comparable."); }
                    }
                    else
                    {
                        if (!currentTimePeriod.Equals(desiredTimeKey.PrimaryTimePeriod))
                        { continue; }
                    }
                }
                if (definedTimeKey.HasSecondaryTimePeriod)
                {
                    if (!desiredTimeKey.HasSecondaryTimePeriod)
                    { continue; }

                    TimePeriod currentTimePeriod = definedTimeKey.SecondaryTimePeriod;
                    if (currentTimePeriod.TimeValueType == TimeValueType.PeriodValue)
                    {
                        var definedTimePeriodType = definedTimeKey.SecondaryTimePeriod.InferredPeriodType.Value;
                        var desiredTimePeriodType = desiredTimeKey.SecondaryTimePeriod.InferredPeriodType.Value;
                        var timeComparison = definedTimePeriodType.CompareTimeTo(desiredTimePeriodType);

                        if (timeComparison == TimeComparisonResult.ThisIsAsGranular)
                        {
                            if (!currentTimePeriod.Equals(desiredTimeKey.SecondaryTimePeriod))
                            { continue; }
                        }
                        else if (timeComparison == TimeComparisonResult.ThisIsLessGranular)
                        {
                            if (!currentTimePeriod.Contains(desiredTimeKey.SecondaryTimePeriod, true, true))
                            { continue; }
                        }
                        else if (timeComparison == TimeComparisonResult.ThisIsMoreGranular)
                        {
                            if (!desiredTimeKey.SecondaryTimePeriod.Contains(currentTimePeriod, true, true))
                            { continue; }
                        }
                        else
                        { throw new InvalidOperationException("The TimePeriods are not comparable."); }
                    }
                    else
                    {
                        if (!currentTimePeriod.Equals(desiredTimeKey.SecondaryTimePeriod))
                        { continue; }
                    }
                }

                return definedTimeKey;
            }
            return null;
        }

        #endregion

        #region Methods - Merge

        public static MultiTimePeriodKey MergeTimeKeys(this MultiTimePeriodKey firstTimeKey, MultiTimePeriodKey secondTimeKey)
        {
            if (firstTimeKey.HasPrimaryTimePeriod && secondTimeKey.HasPrimaryTimePeriod)
            {
                if (firstTimeKey.PrimaryTimePeriod != secondTimeKey.PrimaryTimePeriod)
                { throw new InvalidOperationException("The Time Keys have conflicting values, so they cannot be merged."); }
            }
            if (firstTimeKey.HasSecondaryTimePeriod && secondTimeKey.HasSecondaryTimePeriod)
            {
                if (firstTimeKey.SecondaryTimePeriod != secondTimeKey.SecondaryTimePeriod)
                { throw new InvalidOperationException("The Time Keys have conflicting values, so they cannot be merged."); }
            }

            var mergedTimeKey = new MultiTimePeriodKey(firstTimeKey.NullablePrimaryTimePeriod, firstTimeKey.NullableSecondaryTimePeriod);

            if (secondTimeKey.HasPrimaryTimePeriod)
            { mergedTimeKey = new MultiTimePeriodKey(secondTimeKey.PrimaryTimePeriod, mergedTimeKey.NullableSecondaryTimePeriod); }
            if (secondTimeKey.HasSecondaryTimePeriod)
            { mergedTimeKey = new MultiTimePeriodKey(mergedTimeKey.NullablePrimaryTimePeriod, secondTimeKey.SecondaryTimePeriod); }
            return mergedTimeKey;
        }

        #endregion
    }
}