using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.ValueObjects
{
    public enum CycleType
    {
        None = 0x0000,
        PastCycle = 0x0001,
        PresentCycle = 0x0010,
        FutureCycle = 0x0100,
    }

    public static class CycleTypeUtils
    {
        public const CycleType DefaultCycleType = CycleType.None;

        public static IList<T> SortByOperationType<T>(this IEnumerable<T> unsorted, Func<T, CycleType> operationTypeGetter)
        {
            return unsorted.OrderBy(t => (-1 * ((int)operationTypeGetter(t)))).ToList();
        }

        public static bool IsPastCycle(this CycleType cycleType)
        {
            return (cycleType == (cycleType | CycleType.PastCycle));
        }

        public static bool IsPresentCycle(this CycleType cycleType)
        {
            return (cycleType == (cycleType | CycleType.PresentCycle));
        }

        public static bool IsFutureCycle(this CycleType cycleType)
        {
            return (cycleType == (cycleType | CycleType.FutureCycle));
        }
    }
}