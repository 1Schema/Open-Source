using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace Decia.Business.Common.Collections
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class ReadOnlyList<T> : IReadOnly, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private bool m_IsReadOnly;
        private List<T> m_List;

        #region Constructors

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public ReadOnlyList()
        {
            m_IsReadOnly = false;
            m_List = new List<T>();
        }

        public ReadOnlyList(IEnumerable<T> collection)
        {
            m_IsReadOnly = false;
            m_List = new List<T>(collection);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public ReadOnlyList(int capacity)
        {
            m_IsReadOnly = false;
            m_List = new List<T>(capacity);
        }

        #endregion

        #region Properties

        public int Capacity { get { return m_List.Capacity; } }
        public int Count { get { return m_List.Count; } }
        bool ICollection.IsSynchronized { get { return (m_List as ICollection).IsSynchronized; } }
        bool IList.IsFixedSize { get { return (m_List as IList).IsFixedSize; } }
        object ICollection.SyncRoot { get { return (m_List as ICollection).SyncRoot; } }

        public bool IsReadOnly
        {
            get { return m_IsReadOnly; }
            set { m_IsReadOnly = value; }
        }

        #endregion

        #region Indexers

        public T this[int index]
        {
            get { return m_List[index]; }
            set
            {
                AssertIsNotReadOnly();
                m_List[index] = value;
            }
        }

        object IList.this[int index]
        {
            get { return (m_List as IList)[index]; }
            set
            {
                AssertIsNotReadOnly();
                (m_List as IList)[index] = value;
            }
        }

        #endregion

        #region Methods - Local

        public void AssertIsNotReadOnly()
        {
            IReadOnlyUtils.AssertIsNotReadOnly(this);
        }

        #endregion

        #region Methods - To mirror List<T>

        public void Add(T item)
        {
            AssertIsNotReadOnly();
            m_List.Add(item);
        }

        int IList.Add(object value)
        {
            AssertIsNotReadOnly();
            return (m_List as IList).Add(value);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            AssertIsNotReadOnly();
            m_List.AddRange(collection);
        }

        public ReadOnlyCollection<T> AsReadOnly()
        { return m_List.AsReadOnly(); }

        public int BinarySearch(T item)
        { return m_List.BinarySearch(item); }

        public int BinarySearch(T item, IComparer<T> comparer)
        { return m_List.BinarySearch(item, comparer); }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        { return m_List.BinarySearch(index, count, item, comparer); }

        public void Clear()
        {
            AssertIsNotReadOnly();
            m_List.Clear();
        }

        public bool Contains(T item)
        { return m_List.Contains(item); }

        bool IList.Contains(object value)
        { return (m_List as IList).Contains(value); }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        { return m_List.ConvertAll<TOutput>(converter); }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void CopyTo(T[] array)
        { m_List.CopyTo(array); }

        public void CopyTo(T[] array, int arrayIndex)
        { m_List.CopyTo(array, arrayIndex); }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        { m_List.CopyTo(index, array, arrayIndex, count); }

        void ICollection.CopyTo(Array array, int index)
        { (m_List as ICollection).CopyTo(array, index); }

        public bool Exists(Predicate<T> match)
        { return m_List.Exists(match); }

        public T Find(Predicate<T> match)
        { return m_List.Find(match); }

        public List<T> FindAll(Predicate<T> match)
        { return m_List.FindAll(match); }

        public int FindIndex(Predicate<T> match)
        { return m_List.FindIndex(match); }

        public int FindIndex(int startIndex, Predicate<T> match)
        { return m_List.FindIndex(startIndex, match); }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        { return m_List.FindIndex(startIndex, count, match); }

        public T FindLast(Predicate<T> match)
        { return m_List.FindLast(match); }

        public int FindLastIndex(Predicate<T> match)
        { return m_List.FindLastIndex(match); }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        { return m_List.FindLastIndex(startIndex, match); }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        { return m_List.FindLastIndex(startIndex, count, match); }

        public void ForEach(Action<T> action)
        { m_List.ForEach(action); }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public List<T>.Enumerator GetEnumerator()
        { return m_List.GetEnumerator(); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        { return this.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator()
        { return this.GetEnumerator(); }

        public List<T> GetRange(int index, int count)
        { return m_List.GetRange(index, count); }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOf(T item)
        { return m_List.IndexOf(item); }

        public int IndexOf(T item, int index)
        { return m_List.IndexOf(item, index); }

        public int IndexOf(T item, int index, int count)
        { return m_List.IndexOf(item, index, count); }

        int IList.IndexOf(object value)
        { return (m_List as IList).IndexOf(value); }

        public void Insert(int index, T item)
        {
            AssertIsNotReadOnly();
            m_List.Insert(index, item);
        }

        void IList.Insert(int index, object value)
        {
            AssertIsNotReadOnly();
            (m_List as IList).Insert(index, value);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            AssertIsNotReadOnly();
            m_List.InsertRange(index, collection);
        }

        public int LastIndexOf(T item)
        { return m_List.LastIndexOf(item); }

        public int LastIndexOf(T item, int index)
        { return m_List.LastIndexOf(item, index); }

        public int LastIndexOf(T item, int index, int count)
        { return m_List.LastIndexOf(item, index, count); }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public bool Remove(T item)
        {
            AssertIsNotReadOnly();
            return m_List.Remove(item);
        }

        void IList.Remove(object value)
        {
            AssertIsNotReadOnly();
            (m_List as IList).Remove(value);
        }

        public int RemoveAll(Predicate<T> match)
        {
            AssertIsNotReadOnly();
            return m_List.RemoveAll(match);
        }

        public void RemoveAt(int index)
        {
            AssertIsNotReadOnly();
            m_List.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            AssertIsNotReadOnly();
            m_List.RemoveRange(index, count);
        }

        public void Reverse()
        {
            AssertIsNotReadOnly();
            m_List.Reverse();
        }

        public void Reverse(int index, int count)
        {
            AssertIsNotReadOnly();
            m_List.Reverse(index, count);
        }

        public void Sort()
        {
            AssertIsNotReadOnly();
            m_List.Sort();
        }

        public void Sort(Comparison<T> comparison)
        {
            AssertIsNotReadOnly();
            m_List.Sort(comparison);
        }

        public void Sort(IComparer<T> comparer)
        {
            AssertIsNotReadOnly();
            m_List.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            AssertIsNotReadOnly();
            m_List.Sort(index, count, comparer);
        }

        public T[] ToArray()
        { return m_List.ToArray(); }

        public void TrimExcess()
        {
            AssertIsNotReadOnly();
            m_List.TrimExcess();
        }

        public bool TrueForAll(Predicate<T> match)
        { return m_List.TrueForAll(match); }

        #endregion
    }
}