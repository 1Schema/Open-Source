using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Time
{
    public static class TimePeriodUtils
    {
        public const bool DefaultRequireStrictMatchForZeroDate = true;
        public const string DefaultForeverText = "Forever";

        public static readonly char PeriodDateSeparator = ConversionUtils.ItemPartSeparator;
        public static readonly string PeriodIdFormat = "{0}" + PeriodDateSeparator + "{1}";
        public const string FriendlyName_Format = "{0} - {1}";

        public static ICollection<TimePeriod> GeneratePeriodsForTimeDimension(this TimePeriodType periodType, DateTime firstPeriodStartDate, DateTime lastPeriodEndDate)
        {
            return periodType.GeneratePeriodsForTimeDimension(firstPeriodStartDate, lastPeriodEndDate, null);
        }

        public static ICollection<TimePeriod> GeneratePeriodsForTimeDimension(this TimePeriodType periodType, DateTime firstPeriodStartDate, DateTime lastPeriodEndDate, Nullable<DateTime> zeroIndexDate)
        {
            return periodType.GeneratePeriodsForTimeDimension(firstPeriodStartDate, lastPeriodEndDate, zeroIndexDate, DefaultRequireStrictMatchForZeroDate);
        }

        public static ICollection<TimePeriod> GeneratePeriodsForTimeDimension(this TimePeriodType periodType, DateTime firstPeriodStartDate, DateTime lastPeriodEndDate, Nullable<DateTime> zeroIndexDate, bool requireStrictMatchForZeroDate)
        {
            DateTime zeroIndexNonNullDate = zeroIndexDate.HasValue ? zeroIndexDate.Value : firstPeriodStartDate;
            bool minDateIsFirstStart = (firstPeriodStartDate <= zeroIndexNonNullDate);

            int indexOfFirstStartDate = 0;
            DateTime indexLoopStartDate = minDateIsFirstStart ? firstPeriodStartDate : zeroIndexNonNullDate;
            DateTime indexLoopTerminationDate = minDateIsFirstStart ? zeroIndexNonNullDate : firstPeriodStartDate;

            while (indexLoopStartDate < indexLoopTerminationDate)
            {
                if (!requireStrictMatchForZeroDate)
                {
                    DateTime indexLoopEndDate = periodType.GetCurrentEndDate(indexLoopStartDate);
                    if (indexLoopTerminationDate <= indexLoopEndDate)
                    { break; }
                }

                if (minDateIsFirstStart)
                { indexOfFirstStartDate++; }
                else
                { indexOfFirstStartDate--; }

                indexLoopStartDate = periodType.GetNextStartDate(indexLoopStartDate);
            }


            var periods = new HashSet<TimePeriod>();
            DateTime periodStartDate = firstPeriodStartDate;
            int currentIndex = indexOfFirstStartDate;

            while (periodStartDate < lastPeriodEndDate)
            {
                DateTime periodEndDate = periodType.GetCurrentEndDate(periodStartDate);
                TimePeriod period = new TimePeriod(periodStartDate, periodEndDate, currentIndex);
                periods.Add(period);

                periodStartDate = periodType.GetNextStartDate(periodStartDate);
                currentIndex++;
            }
            return periods;
        }

        public static ICollection<TimePeriod> GetRelevantOrContainedPeriods(this IEnumerable<TimePeriod> allDefinedPeriods, TimePeriod desiredPeriod)
        {
            List<TimePeriod> relevantPeriods = allDefinedPeriods.Where(p => desiredPeriod.Contains(p.StartDate, true, false)).ToList();

            if (relevantPeriods.Count <= 0)
            { relevantPeriods = allDefinedPeriods.Where(p => p.ContainsOrEquals(desiredPeriod, true, true)).ToList(); }

            return relevantPeriods;
        }

        public static ICollection<TimePeriod> GetRelevantPeriods(this IEnumerable<TimePeriod> allDefinedPeriods, TimePeriod desiredPeriod)
        {
            List<TimePeriod> relevantPeriods = allDefinedPeriods.Where(p => desiredPeriod.Contains(p.StartDate, true, false)).ToList();
            return relevantPeriods;
        }

        public static ICollection<TimePeriod> GetContainingPeriods(this IEnumerable<TimePeriod> allDefinedPeriods, TimePeriod desiredPeriod)
        {
            List<TimePeriod> relevantPeriods = allDefinedPeriods.Where(p => p.ContainsOrEquals(desiredPeriod)).ToList();
            return relevantPeriods;
        }

        public static TimePeriod ConvertToPeriod(this DateTime date)
        {
            return new TimePeriod(date, date);
        }

        #region String to TimePeriod Conversion

        public static string GetTimePeriodId(this TimePeriod timePeriod)
        {
            if (timePeriod.IsForever)
            { return string.Empty; }
            return GetTimePeriodId(timePeriod.StartDate, timePeriod.EndDate);
        }

        public static string GetTimePeriodId(this DateTime startDate, DateTime endDate)
        {
            var startDateAsOADate = startDate.ToOADate();
            var endDateAsOADate = endDate.ToOADate();
            var timePeriodId = string.Format(PeriodIdFormat, startDateAsOADate, endDateAsOADate);
            return timePeriodId;
        }

        public static TimePeriod ConvertStringToTimePeriod(this string timePeriodId)
        {
            TimePeriod timePeriod;
            var success = ConvertStringToTimePeriod(timePeriodId, out timePeriod);

            if (!success)
            { throw new InvalidOperationException("The TimePeriodId is not in the correct format."); }

            return timePeriod;
        }

        public static bool ConvertStringToTimePeriod(this string timePeriodId, out TimePeriod timePeriod)
        {
            if (string.IsNullOrWhiteSpace(timePeriodId))
            {
                timePeriod = TimePeriod.ForeverPeriod;
                return true;
            }

            timePeriodId = timePeriodId.Replace("\"", "");
            var datesAsDoubles = timePeriodId.Split(PeriodDateSeparator);

            if (datesAsDoubles.Count() != 2)
            {
                timePeriod = TimePeriod.ForeverPeriod;
                return false;
            }

            double startDateAsDouble;
            double endDateAsDouble;

            bool startDateParseSuccess = double.TryParse(datesAsDoubles[0], out startDateAsDouble);
            bool endDateParseSuccess = double.TryParse(datesAsDoubles[1], out endDateAsDouble);

            if (!startDateParseSuccess || !endDateParseSuccess)
            {
                timePeriod = TimePeriod.ForeverPeriod;
                return false;
            }

            DateTime startDate = DateTime.FromOADate(startDateAsDouble);
            DateTime endDate = DateTime.FromOADate(endDateAsDouble);

            timePeriod = new TimePeriod(startDate, endDate);
            return true;
        }

        public static string ToFriendlyName(this TimePeriod timePeriod, Nullable<TimePeriodType> timePeriodType)
        {
            if (timePeriod.IsForever)
            { return DefaultForeverText; }

            var inferredPeriodType = timePeriodType.HasValue ? timePeriodType : timePeriod.InferredPeriodType;
            var startDate = timePeriod.StartDate;
            var endDate = timePeriod.EndDate;

            var startDateAsText = startDate.ToShortDateString();
            var endDateAsText = endDate.ToShortDateString();

            if (inferredPeriodType == TimePeriodType.Years)
            { return "" + endDate.Year; }
            else if (inferredPeriodType == TimePeriodType.HalfYears)
            {
                if (endDate.Month <= 7)
                { return "1H " + endDate.Year; }
                else
                { return "2H " + endDate.Year; }
            }
            else if (inferredPeriodType == TimePeriodType.QuarterYears)
            {
                if (endDate.Month <= 4)
                { return "1Q " + endDate.Year; }
                else if (endDate.Month <= 7)
                { return "2Q " + endDate.Year; }
                else if (endDate.Month <= 10)
                { return "3Q " + endDate.Year; }
                else
                { return "4Q " + endDate.Year; }
            }
            else if (inferredPeriodType == TimePeriodType.Months)
            { return "" + endDate.Month + "-" + endDate.Year; }
            else if (inferredPeriodType == TimePeriodType.HalfMonths)
            {
                if (endDate.Day <= 15)
                { return "1H " + endDate.Month + "-" + endDate.Year; }
                else
                { return "2H " + endDate.Month + "-" + endDate.Year; }
            }
            else if (inferredPeriodType == TimePeriodType.QuarterMonths)
            {
                if (endDate.Month <= 8)
                { return "1Q " + endDate.Month + "-" + endDate.Year; }
                else if (endDate.Month <= 15)
                { return "2Q " + endDate.Month + "-" + endDate.Year; }
                else if (endDate.Month <= 22)
                { return "3Q " + endDate.Month + "-" + endDate.Year; }
                else
                { return "4Q " + endDate.Month + "-" + endDate.Year; }
            }

            return string.Format(FriendlyName_Format, startDateAsText, endDateAsText);
        }

        #endregion
    }
}