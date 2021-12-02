using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Time
{
    public enum TimeComparisonResult
    {
        NotComparable,
        ThisIsLessGranular,
        ThisIsAsGranular,
        ThisIsMoreGranular
    }
}