using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Collections
{
    public class NoOpOrderManager<T> : IOrderManager<T>
    {
        public NoOpOrderManager()
        { }

        public bool IsOrderingEnabled
        {
            get { return false; }
        }

        public UnrecognizedValueMode UnrecognizedValueMode
        {
            get { return Collections.UnrecognizedValueMode.OrderLast; }
        }

        public void OrderListInPlace(IList<T> list)
        { }

        public void OrderSetInPlace(ISet<T> set)
        { }

        public void OrderDictionaryInPlace<TValue>(IDictionary<T, TValue> dictionary)
        { }

        public IList<T> ToOrderedList(IEnumerable<T> enumerable)
        {
            if (enumerable is IList<T>)
            { return (enumerable as IList<T>); }

            return enumerable.ToList();
        }

        public ISet<T> ToOrderedSet(IEnumerable<T> enumerable)
        {
            return ToOrderedSet(enumerable, enumerable.GetComparer());
        }

        public ISet<T> ToOrderedSet(IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
        {
            if (enumerable is ISet<T>)
            { return (enumerable as ISet<T>); }

            return enumerable.ToHashSet(comparer);
        }

        public IDictionary<T, TValue> ToOrderedDictionary<TValue>(IDictionary<T, TValue> dictionary)
        {
            return ToOrderedDictionary(dictionary, dictionary.GetComparer());
        }

        public IDictionary<T, TValue> ToOrderedDictionary<TValue>(IDictionary<T, TValue> dictionary, IEqualityComparer<T> comparer)
        {
            if (dictionary is IDictionary<T, TValue>)
            { return (dictionary as IDictionary<T, TValue>); }

            return dictionary.ToDictionary(x => x.Key, x => x.Value, comparer);
        }
    }
}