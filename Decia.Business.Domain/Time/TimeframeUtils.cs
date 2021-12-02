using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public static class TimeframeUtils
    {
        public const TimeDimensionType Default_TimeDimensionType = TimeDimensionType.Primary;

        public const TimeValueType Default_TimeValueType = TimeValueType.PeriodValue;
        public const TimePeriodType Default_TimePeriodType = TimePeriodType.Years;
        public const int Default_YearSpan = 10;
        public static readonly DateTime Default_FirstPeriodStartDate = new DateTime(2012, 1, 1);
        public static readonly DateTime Default_LastPeriodEndDate = Default_FirstPeriodStartDate.AddYears(Default_YearSpan);

        #region Methods - Create

        public static TimeDimension CreateDefaultTimeframe()
        {
            return CreateDefaultTimeframe(Default_TimeDimensionType);
        }

        public static TimeDimension CreateDefaultTimeframe(this TimeDimensionType timeDimensionType)
        {
            return new TimeDimension(timeDimensionType, Default_TimeValueType, Default_TimePeriodType, Default_FirstPeriodStartDate, Default_LastPeriodEndDate);
        }

        public static TimeDimension CreateEmptyTimeDimension(this TimeDimensionType timeDimensionType)
        {
            return new TimeDimension(timeDimensionType, null, null, null, null);
        }

        #endregion

        #region Methods - Assert

        public static void AssertTimeframeIsValidDefault(this ITimeDimension timeDimension)
        {
            AssertTimeframeIsNotEmpty(timeDimension);

            if (timeDimension.TimeValueType != Default_TimeValueType)
            { throw new InvalidOperationException("The specified Timeframe is not Period-based."); }
        }

        public static void AssertTimeframeIsNotEmpty(this ITimeDimension timeDimension)
        {
            if (!timeDimension.HasTimeValue)
            { throw new InvalidOperationException("The specified Timeframe does not have TimeValue."); }
        }

        public static void AssertTimeframeIsEmpty(this ITimeDimension timeDimension)
        {
            if (!timeDimension.HasTimeValue)
            { throw new InvalidOperationException("The specified Timeframe has TimeValue."); }
        }

        #endregion
    }
}