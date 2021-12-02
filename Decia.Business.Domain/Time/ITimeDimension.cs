using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public interface ITimeDimension : ITimeComparable<ITimeDimension>
    {
        TimeDimensionType TimeDimensionType { get; }
        bool HasTimeValue { get; }
        TimeValueType TimeValueType { get; }
        bool UsesTimePeriods { get; }
        TimePeriodType TimePeriodType { get; }
        DateTime FirstPeriodStartDate { get; }
        DateTime LastPeriodEndDate { get; }

        Nullable<TimeValueType> NullableTimeValueType { get; }
        Nullable<TimePeriodType> NullableTimePeriodType { get; }
        Nullable<DateTime> NullableFirstPeriodStartDate { get; }
        Nullable<DateTime> NullableLastPeriodEndDate { get; }
    }
}