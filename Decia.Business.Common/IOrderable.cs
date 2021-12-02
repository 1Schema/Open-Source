using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public interface IOrderable
    {
        Nullable<long> OrderNumber { get; }

        string OrderValue { get; }
    }

    public class Orderable<T> : IOrderable
    {
        private T m_ParentObject;
        private Func<T, long?> m_OrderNumberGetter;
        private Func<T, string> m_OrderValueGetter;

        public Orderable(T parentObject, Func<T, long?> orderNumberGetter, Func<T, string> orderValueGetter)
        {
            if ((parentObject == null) || (orderNumberGetter == null) || (orderValueGetter == null))
            { throw new InvalidOperationException("Construction parameters must not be null."); }

            m_ParentObject = parentObject;
            m_OrderNumberGetter = orderNumberGetter;
            m_OrderValueGetter = orderValueGetter;
        }

        public T ParentObject { get { return m_ParentObject; } }
        public long? OrderNumber { get { return m_OrderNumberGetter(m_ParentObject); } }
        public string OrderValue { get { return m_OrderValueGetter(m_ParentObject); } }
    }

    public static class IOrderableUtils
    {
        public const long OrderNumber_Max = long.MaxValue;

        public static long GetOrderNumber_NonNull<T>(this T orderable)
            where T : IOrderable
        {
            if (!orderable.OrderNumber.HasValue)
            { return OrderNumber_Max; }
            return orderable.OrderNumber.Value;
        }

        public static List<T> GetOrderedList<T>(this IEnumerable<T> items)
            where T : IOrderable
        {
            var itemsByOrderNumber = new SortedDictionary<long, List<T>>();
            var sortedItems = new List<T>();

            foreach (T item in items)
            {
                long orderNumber = item.GetOrderNumber_NonNull();

                if (!itemsByOrderNumber.ContainsKey(orderNumber))
                { itemsByOrderNumber.Add(orderNumber, new List<T>()); }

                itemsByOrderNumber[orderNumber].Add(item);
            }

            foreach (var orderNumber in itemsByOrderNumber.Keys)
            {
                var bucketItems = itemsByOrderNumber[orderNumber];
                var sortedBucketItems = bucketItems.OrderBy(i => i.OrderValue);

                sortedItems.AddRange(sortedBucketItems);
            }
            return sortedItems;
        }
    }
}