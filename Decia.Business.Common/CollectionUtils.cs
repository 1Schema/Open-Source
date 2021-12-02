using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;

namespace Decia.Business.Common
{
    public static class CollectionUtils
    {
        #region Dictionary Methods

        public static void AddRange<K, V>(this IDictionary<K, V> mainDictionary, IDictionary<K, V> dictionaryToAdd)
        {
            foreach (var bucket in dictionaryToAdd)
            { mainDictionary.Add(bucket.Key, bucket.Value); }
        }

        #endregion

        #region HashSet Methods

        public const string ExpectedComparerPropName = "Comparer";

        public static IEqualityComparer<T> GetComparer<T>(this IEnumerable<T> collection)
        {
            var comparer = (IEqualityComparer<T>)null;

            if (collection is HashSet<T>)
            {
                var typedCollection = (collection as HashSet<T>);
                comparer = typedCollection.Comparer;
            }
            else if (collection is ReadOnlyHashSet<T>)
            {
                var typedCollection = (collection as ReadOnlyHashSet<T>);
                comparer = typedCollection.Comparer;
            }
            else
            {
                comparer = GetComparerFromObject<T>(collection);
            }
            return comparer;
        }

        public static IEqualityComparer<TKey> GetComparer<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            var comparer = (IEqualityComparer<TKey>)null;

            if (dictionary is Dictionary<TKey, TValue>)
            {
                var typedCollection = (dictionary as Dictionary<TKey, TValue>);
                comparer = typedCollection.Comparer;
            }
            else if (dictionary is ReadOnlyDictionary<TKey, TValue>)
            {
                var typedCollection = (dictionary as ReadOnlyDictionary<TKey, TValue>);
                comparer = typedCollection.Comparer;
            }
            else if (dictionary is SortedDictionary<TKey, TValue>)
            {
                var typedCollection = (dictionary as SortedDictionary<TKey, TValue>);
                comparer = new EqualityComparerForComparer<TKey>(typedCollection.Comparer);
            }
            else if (dictionary is ReadOnlySortedDictionary<TKey, TValue>)
            {
                var typedCollection = (dictionary as ReadOnlySortedDictionary<TKey, TValue>);
                comparer = new EqualityComparerForComparer<TKey>(typedCollection.Comparer);
            }
            else
            {
                comparer = GetComparerFromObject<TKey>(dictionary);
            }
            return comparer;
        }

        private static IEqualityComparer<T> GetComparerFromObject<T>(object collection)
        {
            var type = collection.GetType();
            foreach (var propInfo in type.GetProperties())
            {
                if (propInfo.Name.Contains(ExpectedComparerPropName))
                {
                    var comparer = propInfo.GetValue(collection, null);

                    if (comparer is IEqualityComparer<T>)
                    { return (IEqualityComparer<T>)comparer; }
                    else if (comparer is IComparer<T>)
                    { return new EqualityComparerForComparer<T>((IComparer<T>)comparer); }
                }
            }
            return null;
        }

        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> collectionToAdd)
        {
            foreach (var item in collectionToAdd)
            { hashSet.Add(item); }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            var comparer = collection.GetComparer();
            return ToHashSet(collection, comparer);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            { return new HashSet<T>(collection); }
            else
            { return new HashSet<T>(collection, comparer); }
        }

        public static HashSet<TKey> KeysToHashSet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            var comparer = dictionary.GetComparer();
            return KeysToHashSet(dictionary, comparer);
        }

        public static HashSet<TKey> KeysToHashSet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            return ToHashSet(dictionary.Keys, comparer);
        }

        #endregion

        #region String Methods

        public static string SubstringOrRemaining(this string str, int startIndex, int length)
        {
            var str_Length = str.Length;
            if ((startIndex + length) > str_Length)
            { return str.Substring(startIndex, (str_Length - startIndex)); }
            return str.Substring(startIndex, length);
        }

        public static string SubStringForIndices(this string str, KeyValuePair<int, int> subLocation)
        {
            return SubStringForIndices(str, subLocation.Key, subLocation.Value);
        }

        public static string SubStringForIndices(this string str, int startIndex, int endIndex)
        {
            int length = ((endIndex - startIndex) + 1);
            return str.Substring(startIndex, length);
        }

        #endregion

        public static IEnumerable<T> GetSubCollection<T>(this IEnumerable<T> collection, int startIndex, int length)
        {
            var collectionCount = collection.Count();
            var stopCount = (startIndex + length);
            var minCount = Math.Min(collectionCount, stopCount);

            var subCollection = new List<T>();
            for (int i = startIndex; i < minCount; i++)
            {
                subCollection.Add(collection.ElementAt(i));
            }
            return subCollection;
        }

        public static void Remove<T>(this List<T> list, T itemToRemove, IEqualityComparer<T> comparer)
        {
            int indexToRemove = list.FindIndex(it => comparer.Equals(it, itemToRemove));
            list.RemoveAt(indexToRemove);
        }

        public static SortedDictionary<int, T> GetAsSortedDictionary<T>(this IEnumerable<T> list)
        {
            return new SortedDictionary<int, T>(list.GetAsDictionary());
        }

        public static SortedDictionary<int, CT> GetAsSortedDictionary<OT, CT>(this IEnumerable<OT> list, Func<OT, CT> converter)
        {
            return new SortedDictionary<int, CT>(list.GetAsDictionary(converter));
        }

        public static Dictionary<int, T> GetAsDictionary<T>(this IEnumerable<T> list)
        {
            return GetAsDictionary(list, (T original) => original);
        }

        public static Dictionary<int, CT> GetAsDictionary<OT, CT>(this IEnumerable<OT> list, Func<OT, CT> converter)
        {
            Dictionary<int, CT> dictionary = new Dictionary<int, CT>();

            for (int i = 0; i < list.Count(); i++)
            {
                OT original = list.ElementAt(i);
                CT converted = converter(original);
                dictionary.Add(i, converted);
            }

            return dictionary;
        }

        public static List<T> GetAsList<T>(this IDictionary<int, T> dictionary)
        {
            return GetAsList(dictionary, (T original) => original);
        }

        public static List<CT> GetAsList<OT, CT>(this IDictionary<int, OT> dictionary, Func<OT, CT> converter)
        {
            List<CT> list = new List<CT>();

            foreach (KeyValuePair<int, OT> bucket in dictionary.OrderBy(kvp => kvp.Key))
            {
                OT original = bucket.Value;
                CT converted = converter(original);
                list.Add(converted);
            }

            return list;
        }

        public static ICollection<T> GetNullableAsCollection<T>(this Nullable<T> nullable)
            where T : struct
        {
            if (!nullable.HasValue)
            { return new T[] { }; }
            return new T[] { nullable.Value };
        }

        public static bool HasContents<T>(this IEnumerable<T> enumerable)
        {
            return (enumerable.Count() > 0);
        }

        public static void AssertHasContents<T>(this IEnumerable<T> enumerable)
        {
            if (!enumerable.HasContents())
            { throw new InvalidOperationException("The IEnumerable contains no elements."); }
        }

        public static void AssertDoesNotHaveContents<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable.HasContents())
            { throw new InvalidOperationException("The IEnumerable contains at least one element already."); }
        }
    }
}