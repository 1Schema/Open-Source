using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Collections
{
    public class PresetOrderManager<T> : IOrderManager<T>
    {
        private bool m_OrderingEnabled;
        private UnrecognizedValueMode m_UnrecognizedValueMode;

        private Dictionary<T, int> m_ValueOrders;
        private SortedDictionary<int, T> m_OrderedValues;

        public PresetOrderManager(IEnumerable<T> orderedValues)
            : this(orderedValues, null)
        { }

        public PresetOrderManager(IEnumerable<T> orderedValues, IEqualityComparer<T> comparer)
        {
            m_OrderingEnabled = true;
            m_UnrecognizedValueMode = UnrecognizedValueMode.OrderLast;

            m_ValueOrders = new Dictionary<T, int>(comparer);
            m_OrderedValues = new SortedDictionary<int, T>();

            for (int i = 0; i < orderedValues.Count(); i++)
            {
                var value = orderedValues.ElementAt(i);

                m_ValueOrders.Add(value, i);
                m_OrderedValues.Add(i, value);
            }
        }

        public bool IsOrderingEnabled
        {
            get { return m_OrderingEnabled; }
            set { m_OrderingEnabled = value; }
        }

        public UnrecognizedValueMode UnrecognizedValueMode
        {
            get { return m_UnrecognizedValueMode; }
            set { m_UnrecognizedValueMode = value; }
        }

        public int OrderByMethod(T value)
        {
            if (!m_ValueOrders.ContainsKey(value))
            { return (UnrecognizedValueMode == UnrecognizedValueMode.OrderLast) ? int.MaxValue : int.MinValue; }
            return m_ValueOrders[value];
        }

        public void OrderListInPlace(IList<T> list)
        {
            if (!IsOrderingEnabled)
            { (new NoOpOrderManager<T>()).OrderListInPlace(list); }

            var orderedList = list.OrderBy(x => OrderByMethod(x)).ToList();
            list.Clear();

            foreach (var element in orderedList)
            { list.Add(element); }
        }

        public void OrderSetInPlace(ISet<T> set)
        {
            if (!IsOrderingEnabled)
            { (new NoOpOrderManager<T>()).OrderSetInPlace(set); }

            var orderedSet = set.OrderBy(x => OrderByMethod(x)).ToList();
            set.Clear();

            foreach (var element in orderedSet)
            { set.Add(element); }
        }

        public void OrderDictionaryInPlace<TValue>(IDictionary<T, TValue> dictionary)
        {
            if (!IsOrderingEnabled)
            { (new NoOpOrderManager<T>()).OrderDictionaryInPlace(dictionary); }

            var orderedBuckets = dictionary.OrderBy(x => OrderByMethod(x.Key)).ToList();
            dictionary.Clear();

            foreach (var bucket in orderedBuckets)
            { dictionary.Add(bucket.Key, bucket.Value); }
        }

        public IList<T> ToOrderedList(IEnumerable<T> enumerable)
        {
            if (!IsOrderingEnabled)
            { return (new NoOpOrderManager<T>()).ToOrderedList(enumerable); }

            return enumerable.OrderBy(x => OrderByMethod(x)).ToList();
        }

        public ISet<T> ToOrderedSet(IEnumerable<T> enumerable)
        {
            return ToOrderedSet(enumerable, enumerable.GetComparer());
        }

        public ISet<T> ToOrderedSet(IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
        {
            if (!IsOrderingEnabled)
            { return (new NoOpOrderManager<T>()).ToOrderedSet(enumerable); }

            return enumerable.OrderBy(x => OrderByMethod(x)).ToHashSet();
        }

        public IDictionary<T, TValue> ToOrderedDictionary<TValue>(IDictionary<T, TValue> dictionary)
        {
            return ToOrderedDictionary(dictionary, dictionary.GetComparer());
        }

        public IDictionary<T, TValue> ToOrderedDictionary<TValue>(IDictionary<T, TValue> dictionary, IEqualityComparer<T> comparer)
        {
            if (!IsOrderingEnabled)
            { return (new NoOpOrderManager<T>()).ToOrderedDictionary(dictionary); }

            return dictionary.OrderBy(x => OrderByMethod(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}