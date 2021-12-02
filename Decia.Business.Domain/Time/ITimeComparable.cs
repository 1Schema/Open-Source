using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public interface ITimeComparable<T>
        where T : ITimeComparable<T>
    {
        TimeComparisonResult CompareTimeTo(T other);
    }
}