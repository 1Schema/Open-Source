using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Collections
{
    public enum UnrecognizedValueMode
    {
        OrderFirst,
        OrderLast
    }

    public interface IOrderManager<T>
    {
        bool IsOrderingEnabled { get; }
        UnrecognizedValueMode UnrecognizedValueMode { get; }

        void OrderListInPlace(IList<T> list);
        void OrderSetInPlace(ISet<T> set);
        void OrderDictionaryInPlace<TValue>(IDictionary<T, TValue> dictionary);

        IList<T> ToOrderedList(IEnumerable<T> enumerable);
        ISet<T> ToOrderedSet(IEnumerable<T> enumerable);
        ISet<T> ToOrderedSet(IEnumerable<T> enumerable, IEqualityComparer<T> comparer);
        IDictionary<T, TValue> ToOrderedDictionary<TValue>(IDictionary<T, TValue> dictionary);
        IDictionary<T, TValue> ToOrderedDictionary<TValue>(IDictionary<T, TValue> dictionary, IEqualityComparer<T> comparer);
    }
}