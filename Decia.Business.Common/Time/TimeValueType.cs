using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Time
{
    public enum TimeValueType
    {
        [Description("Spot Value")]
        SpotValue = 1,
        [Description("Period Value")]
        PeriodValue
    }

    public static class TimeValueTypeUtils
    {
        public static TimeComparisonResult CompareTimeTo(this TimeValueType first, TimeValueType second)
        {
            if (first == second)
            { return TimeComparisonResult.ThisIsAsGranular; }
            else if ((first == TimeValueType.PeriodValue) && (first == TimeValueType.SpotValue))
            { return TimeComparisonResult.ThisIsLessGranular; }
            else if ((first == TimeValueType.SpotValue) && (first == TimeValueType.PeriodValue))
            { return TimeComparisonResult.ThisIsMoreGranular; }
            else
            { throw new InvalidOperationException("Unexpected condition encountered."); }
        }
    }
}