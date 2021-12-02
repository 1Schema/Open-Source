using System;
using System.Collections;
using System.Collections.Generic;
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
    public class ReadOnlyHashSet<T> : IReadOnly, ISerializable, IDeserializationCallback, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private bool m_IsReadOnly;
        private HashSet<T> m_HashSet;

        #region Constructors

        public ReadOnlyHashSet()
        {
            m_IsReadOnly = false;
            m_HashSet = new HashSet<T>();
        }

        public ReadOnlyHashSet(IEnumerable<T> collection)
        {
            m_IsReadOnly = false;
            m_HashSet = new HashSet<T>(collection);
        }

        public ReadOnlyHashSet(IEqualityComparer<T> comparer)
        {
            m_IsReadOnly = false;
            m_HashSet = new HashSet<T>(comparer);
        }

        public ReadOnlyHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            m_IsReadOnly = false;
            m_HashSet = new HashSet<T>(collection, comparer);
        }

        #endregion

        #region Properties

        public IEqualityComparer<T> Comparer { get { return m_HashSet.Comparer; } }
        public int Count { get { return m_HashSet.Count; } }

        public bool IsReadOnly
        {
            get { return m_IsReadOnly; }
            set { m_IsReadOnly = value; }
        }

        #endregion

        #region Methods - Local

        public void AssertIsNotReadOnly()
        {
            IReadOnlyUtils.AssertIsNotReadOnly(this);
        }

        #endregion

        #region Methods - To mirror HashSet<T>

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public bool Add(T item)
        {
            AssertIsNotReadOnly();
            return m_HashSet.Add(item);
        }

        void ICollection<T>.Add(T item)
        { this.Add(item); }

        public void Clear()
        {
            AssertIsNotReadOnly();
            m_HashSet.Clear();
        }

        public bool Contains(T item)
        { return m_HashSet.Contains(item); }

        public void CopyTo(T[] array)
        { m_HashSet.CopyTo(array); }

        public void CopyTo(T[] array, int arrayIndex)
        { m_HashSet.CopyTo(array, arrayIndex); }

        public void CopyTo(T[] array, int arrayIndex, int count)
        { m_HashSet.CopyTo(array, arrayIndex, count); }

        public static IEqualityComparer<HashSet<T>> CreateSetComparer()
        { return HashSet<T>.CreateSetComparer(); }

        public void ExceptWith(IEnumerable<T> other)
        {
            AssertIsNotReadOnly();
            m_HashSet.ExceptWith(other);
        }

        public HashSet<T>.Enumerator GetEnumerator()
        { return m_HashSet.GetEnumerator(); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        { return this.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator()
        { return this.GetEnumerator(); }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            AssertIsNotReadOnly();
            m_HashSet.GetObjectData(info, context);
        }

        [SecurityCritical]
        public void IntersectWith(IEnumerable<T> other)
        {
            AssertIsNotReadOnly();
            m_HashSet.IntersectWith(other);
        }

        [SecurityCritical]
        public bool IsProperSubsetOf(IEnumerable<T> other)
        { return m_HashSet.IsProperSubsetOf(other); }

        [SecurityCritical]
        public bool IsProperSupersetOf(IEnumerable<T> other)
        { return m_HashSet.IsProperSupersetOf(other); }

        [SecurityCritical]
        public bool IsSubsetOf(IEnumerable<T> other)
        { return m_HashSet.IsSubsetOf(other); }

        public bool IsSupersetOf(IEnumerable<T> other)
        { return m_HashSet.IsSupersetOf(other); }

        public virtual void OnDeserialization(object sender)
        { m_HashSet.OnDeserialization(sender); }

        public bool Overlaps(IEnumerable<T> other)
        { return m_HashSet.Overlaps(other); }

        public bool Remove(T item)
        {
            AssertIsNotReadOnly();
            return m_HashSet.Remove(item);
        }

        public int RemoveWhere(Predicate<T> match)
        {
            AssertIsNotReadOnly();
            return m_HashSet.RemoveWhere(match);
        }

        [SecurityCritical]
        public bool SetEquals(IEnumerable<T> other)
        { return m_HashSet.SetEquals(other); }

        [SecurityCritical]
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            AssertIsNotReadOnly();
            m_HashSet.SymmetricExceptWith(other);
        }

        public void TrimExcess()
        {
            AssertIsNotReadOnly();
            m_HashSet.TrimExcess();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            AssertIsNotReadOnly();
            m_HashSet.UnionWith(other);
        }

        #endregion
    }
}