using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Sql;

namespace Decia.Business.Common.Time
{
    public enum TimePeriodType
    {
        [Description("Years")]
        Years = 1,
        [Description("Half-Years")]
        HalfYears,
        [Description("Quarters")]
        QuarterYears,
        [Description("Months")]
        Months,
        [Description("Half-Months")]
        HalfMonths,
        [Description("Quarter-Months")]
        QuarterMonths,
        [Description("Days")]
        Days
    }

    public static class TimePeriodTypeUtils
    {
        public const TimePeriodType LeastGranular_TimePeriodType = TimePeriodType.Years;

        public const int SqlServerMsAccuracy = 3;
        public static readonly TimeSpan SqlServerMsAccuracy_TimeSpan = new TimeSpan(0, 0, 0, 0, SqlServerMsAccuracy);

        public static readonly DateTime SqlServerMinDate_SmallDateTime = new DateTime(1900, 1, 1);
        public static readonly DateTime SqlServerMaxDate_SmallDateTime = new DateTime(2079, 6, 6);

        public static readonly TimeSpan SqlServerMaxDuration_SmallDateTime = (SqlServerMaxDate_SmallDateTime - SqlServerMinDate_SmallDateTime);

        public static TimePeriodType GetMostGranular(this TimePeriodType? timePeriodType)
        {
            if (!timePeriodType.HasValue)
            { return LeastGranular_TimePeriodType; }
            return timePeriodType.Value;
        }

        public static TimeSpan GetEndDateAdjustment(this TimePeriodType periodType)
        {
            return new TimeSpan(0, 0, 0, 0, SqlServerMsAccuracy);
        }

        private static DateTime GetRelativeStartDate(this TimePeriodType periodType, DateTime currentStartDate, bool moveForward)
        {
            int multiplier = (moveForward) ? 1 : -1;

            if (periodType == TimePeriodType.Years)
            { return currentStartDate.AddYears(multiplier * 1); }
            if (periodType == TimePeriodType.HalfYears)
            { return currentStartDate.AddMonths(multiplier * 6); }
            if (periodType == TimePeriodType.QuarterYears)
            { return currentStartDate.AddMonths(multiplier * 3); }
            if (periodType == TimePeriodType.Months)
            { return currentStartDate.AddMonths(multiplier * 1); }
            if (periodType == TimePeriodType.HalfMonths)
            {
                DateTime thisMonthsStartDate = new DateTime(currentStartDate.Year, currentStartDate.Month, 1);
                DateTime nextMonthsStartDate = thisMonthsStartDate.AddMonths(multiplier * 1);
                TimeSpan totalDuration = (moveForward) ? (nextMonthsStartDate - thisMonthsStartDate) : (thisMonthsStartDate - nextMonthsStartDate);
                double halfDurationInMs = totalDuration.TotalMilliseconds / 2.0;

                return currentStartDate.AddMilliseconds(multiplier * halfDurationInMs);
            }
            if (periodType == TimePeriodType.QuarterMonths)
            {
                DateTime thisMonthsStartDate = new DateTime(currentStartDate.Year, currentStartDate.Month, 1);
                DateTime nextMonthsStartDate = thisMonthsStartDate.AddMonths(multiplier * 1);
                TimeSpan totalDuration = (moveForward) ? (nextMonthsStartDate - thisMonthsStartDate) : (thisMonthsStartDate - nextMonthsStartDate);
                double quarterDurationInMs = totalDuration.TotalMilliseconds / 4.0;

                return currentStartDate.AddMilliseconds(multiplier * quarterDurationInMs);
            }
            if (periodType == TimePeriodType.Days)
            { return currentStartDate.AddDays(multiplier * 1); }
            throw new InvalidOperationException("Unrecognized TimePeriodType encountered.");
        }

        public static DateTime GetNextStartDate(this TimePeriodType periodType, DateTime currentStartDate)
        {
            return periodType.GetRelativeStartDate(currentStartDate, true);
        }

        public static DateTime GetPreviousStartDate(this TimePeriodType periodType, DateTime currentStartDate)
        {
            return periodType.GetRelativeStartDate(currentStartDate, false);
        }

        public static DateTime GetCurrentEndDate(this TimePeriodType periodType, DateTime currentStartDate)
        {
            DateTime nextStartDate = periodType.GetNextStartDate(currentStartDate);
            TimeSpan endDateAdjustment = periodType.GetEndDateAdjustment();

            return nextStartDate.Subtract(endDateAdjustment);
        }

        public static DateTime GetCurrentProportionalDate(this TimePeriodType periodType, DateTime currentStartDate, double proportionOfPeriod)
        {
            if ((proportionOfPeriod < 0) || (proportionOfPeriod > 1))
            { throw new InvalidOperationException("The specified proportion is not between [0,1]."); }

            DateTime currentEndDate = periodType.GetCurrentEndDate(currentStartDate);
            TimeSpan currentPeriodDuration = currentEndDate - currentStartDate;

            int relevantDurationInMs = (int)(currentPeriodDuration.TotalMilliseconds * proportionOfPeriod);
            return currentStartDate.AddMilliseconds(relevantDurationInMs);
        }

        public static DateTime GetStartDateForPeriodIndex(this TimePeriodType periodType, DateTime zeroIndexStartDate, int index)
        {
            DateTime currentPeriodStartDate = zeroIndexStartDate;

            if (index >= 0)
            {
                for (int i = 0; i < index; i++)
                { currentPeriodStartDate = periodType.GetNextStartDate(currentPeriodStartDate); }
            }
            else
            {
                for (int i = 0; i > index; i--)
                { currentPeriodStartDate = periodType.GetPreviousStartDate(currentPeriodStartDate); }
            }

            return currentPeriodStartDate;
        }

        public static DateTime GetEndDateForPeriodIndex(this TimePeriodType periodType, DateTime zeroIndexStartDate, int index)
        {
            DateTime currentPeriodStartDate = periodType.GetStartDateForPeriodIndex(zeroIndexStartDate, index);
            DateTime currentPeriodEndDate = periodType.GetCurrentEndDate(currentPeriodStartDate);
            return currentPeriodEndDate;
        }

        public static DateTime GetProportionalDateForPeriodIndex(this TimePeriodType periodType, DateTime zeroIndexStartDate, int index, double proportionOfPeriod)
        {
            DateTime currentStartDate = periodType.GetStartDateForPeriodIndex(zeroIndexStartDate, index);
            return periodType.GetCurrentProportionalDate(currentStartDate, proportionOfPeriod);
        }

        public static TimeComparisonResult CompareTimeTo(this TimePeriodType? first, TimePeriodType? second)
        {
            if (first == second)
            { return TimeComparisonResult.ThisIsAsGranular; }
            else if (first.HasValue && !second.HasValue)
            { return TimeComparisonResult.ThisIsMoreGranular; }
            else if (!first.HasValue && second.HasValue)
            { return TimeComparisonResult.ThisIsLessGranular; }
            else
            { return CompareTimeTo(first.Value, second.Value); }
        }

        public static TimeComparisonResult CompareTimeTo(this TimePeriodType first, TimePeriodType second)
        {
            if (first == second)
            { return TimeComparisonResult.ThisIsAsGranular; }
            else if (first == TimePeriodType.Years)
            {
                TimePeriodType[] moreGranular = new TimePeriodType[] { TimePeriodType.HalfYears, TimePeriodType.QuarterYears, TimePeriodType.Months, TimePeriodType.HalfMonths, TimePeriodType.QuarterMonths, TimePeriodType.Days };
                return (moreGranular.Contains(second)) ? TimeComparisonResult.ThisIsLessGranular : TimeComparisonResult.ThisIsMoreGranular;
            }
            else if (first == TimePeriodType.HalfYears)
            {
                TimePeriodType[] moreGranular = new TimePeriodType[] { TimePeriodType.QuarterYears, TimePeriodType.Months, TimePeriodType.HalfMonths, TimePeriodType.QuarterMonths, TimePeriodType.Days };
                return (moreGranular.Contains(second)) ? TimeComparisonResult.ThisIsLessGranular : TimeComparisonResult.ThisIsMoreGranular;
            }
            else if (first == TimePeriodType.QuarterYears)
            {
                TimePeriodType[] moreGranular = new TimePeriodType[] { TimePeriodType.Months, TimePeriodType.HalfMonths, TimePeriodType.QuarterMonths, TimePeriodType.Days };
                return (moreGranular.Contains(second)) ? TimeComparisonResult.ThisIsLessGranular : TimeComparisonResult.ThisIsMoreGranular;
            }
            else if (first == TimePeriodType.Months)
            {
                TimePeriodType[] moreGranular = new TimePeriodType[] { TimePeriodType.HalfMonths, TimePeriodType.QuarterMonths, TimePeriodType.Days };
                return (moreGranular.Contains(second)) ? TimeComparisonResult.ThisIsLessGranular : TimeComparisonResult.ThisIsMoreGranular;
            }
            else if (first == TimePeriodType.HalfMonths)
            {
                TimePeriodType[] moreGranular = new TimePeriodType[] { TimePeriodType.QuarterMonths, TimePeriodType.Days };
                return (moreGranular.Contains(second)) ? TimeComparisonResult.ThisIsLessGranular : TimeComparisonResult.ThisIsMoreGranular;
            }
            else if (first == TimePeriodType.QuarterMonths)
            {
                TimePeriodType[] moreGranular = new TimePeriodType[] { TimePeriodType.Days };
                return (moreGranular.Contains(second)) ? TimeComparisonResult.ThisIsLessGranular : TimeComparisonResult.ThisIsMoreGranular;
            }
            else if (first == TimePeriodType.Days)
            {
                TimePeriodType[] moreGranular = new TimePeriodType[] { };
                return (moreGranular.Contains(second)) ? TimeComparisonResult.ThisIsLessGranular : TimeComparisonResult.ThisIsMoreGranular;
            }
            else
            { throw new InvalidOperationException("Unexpected condition encountered."); }
        }

        public static void GetDatePart(this TimePeriodType periodType, SqlDb_TargetType dbType, out string datePartValue, out double datePartMultiplier)
        {
            if (periodType == TimePeriodType.Years)
            {
                datePartValue = "year";
                datePartMultiplier = 1.0;
            }
            else if (periodType == TimePeriodType.HalfYears)
            {
                datePartValue = "quarter";
                datePartMultiplier = 2.0;
            }
            else if (periodType == TimePeriodType.QuarterYears)
            {
                datePartValue = "quarter";
                datePartMultiplier = 1.0;
            }
            else if (periodType == TimePeriodType.Months)
            {
                datePartValue = "month";
                datePartMultiplier = 1.0;
            }
            else if (periodType == TimePeriodType.HalfMonths)
            {
                datePartValue = "week";
                datePartMultiplier = 2.0;
            }
            else if (periodType == TimePeriodType.QuarterMonths)
            {
                datePartValue = "week";
                datePartMultiplier = 1.0;
            }
            else if (periodType == TimePeriodType.Days)
            {
                datePartValue = "day";
                datePartMultiplier = 1.0;
            }
            else
            { throw new InvalidOperationException("Unrecognized TimePeriodType encountered."); }
        }

        public static void GetDatePart(this TimePeriodType periodType, NoSqlDb_TargetType dbType, out string datePartValue, out double datePartMultiplier)
        {
            if (periodType == TimePeriodType.Years)
            {
                datePartValue = "year";
                datePartMultiplier = 1.0;
            }
            else if (periodType == TimePeriodType.HalfYears)
            {
                datePartValue = "month";
                datePartMultiplier = 6.0;
            }
            else if (periodType == TimePeriodType.QuarterYears)
            {
                datePartValue = "month";
                datePartMultiplier = 3.0;
            }
            else if (periodType == TimePeriodType.Months)
            {
                datePartValue = "month";
                datePartMultiplier = 1.0;
            }
            else if (periodType == TimePeriodType.HalfMonths)
            {
                datePartValue = "month";
                datePartMultiplier = 0.5;
            }
            else if (periodType == TimePeriodType.QuarterMonths)
            {
                datePartValue = "month";
                datePartMultiplier = 0.25;
            }
            else if (periodType == TimePeriodType.Days)
            {
                datePartValue = "day";
                datePartMultiplier = 1.0;
            }
            else
            { throw new InvalidOperationException("Unrecognized TimePeriodType encountered."); }
        }
    }
}