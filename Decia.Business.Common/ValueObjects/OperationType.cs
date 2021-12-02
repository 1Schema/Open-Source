using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.ValueObjects
{
    public enum OperationType
    {
        SimpleComputation = 0x0000000,
        Filter = 0x0000001,
        Aggregation = 0x0000010,
        Shift = 0x0000100,
        Introspection = 0x0001000
    }

    public static class OperationTypeUtils
    {
        public const OperationType DefaultOperationType = OperationType.SimpleComputation;

        public static IList<T> SortByOperationType<T>(this IEnumerable<T> unsorted, Func<T, OperationType> operationTypeGetter)
        {
            return unsorted.OrderBy(t => (-1 * ((int)operationTypeGetter(t)))).ToList();
        }

        public static bool IsFilter(this OperationType operationType)
        {
            return (operationType == (operationType | OperationType.Filter));
        }

        public static bool IsAggregation(this OperationType operationType)
        {
            return (operationType == (operationType | OperationType.Aggregation));
        }

        public static bool IsShift(this OperationType operationType)
        {
            return (operationType == (operationType | OperationType.Shift));
        }

        public static bool IsIntrospection(this OperationType operationType)
        {
            return (operationType == (operationType | OperationType.Introspection));
        }
    }
}