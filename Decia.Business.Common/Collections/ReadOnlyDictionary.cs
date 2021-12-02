using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace Decia.Business.Common.Collections
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    [ComVisible(false)]
    public class ReadOnlyDictionary<TKey, TValue> : IReadOnly, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        private bool m_IsReadOnly;
        private Dictionary<TKey, TValue> m_Dictionary;

        #region Constructors

        public ReadOnlyDictionary()
        {
            m_IsReadOnly = false;
            m_Dictionary = new Dictionary<TKey, TValue>();
        }

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            m_IsReadOnly = false;
            m_Dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        public ReadOnlyDictionary(IEqualityComparer<TKey> comparer)
        {
            m_IsReadOnly = false;
            m_Dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ReadOnlyDictionary(int capacity)
        {
            m_IsReadOnly = false;
            m_Dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            m_IsReadOnly = false;
            m_Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public ReadOnlyDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            m_IsReadOnly = false;
            m_Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        #endregion

        #region Properties

        public IEqualityComparer<TKey> Comparer { get { return m_Dictionary.Comparer; } }
        public int Count { get { return m_Dictionary.Count; } }
        bool ICollection.IsSynchronized { get { return (m_Dictionary as ICollection).IsSynchronized; } }
        bool IDictionary.IsFixedSize { get { return (m_Dictionary as IDictionary).IsFixedSize; } }
        object ICollection.SyncRoot { get { return (m_Dictionary as ICollection).SyncRoot; } }

        public Dictionary<TKey, TValue>.KeyCollection Keys { get { return m_Dictionary.Keys; } }
        public Dictionary<TKey, TValue>.ValueCollection Values { get { return m_Dictionary.Values; } }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys { get { return m_Dictionary.Keys; } }
        ICollection<TValue> IDictionary<TKey, TValue>.Values { get { return m_Dictionary.Values; } }

        ICollection IDictionary.Keys { get { return m_Dictionary.Keys; } }
        ICollection IDictionary.Values { get { return m_Dictionary.Values; } }

        public bool IsReadOnly
        {
            get { return m_IsReadOnly; }
            set { m_IsReadOnly = value; }
        }

        #endregion

        #region Indexers

        public TValue this[TKey key]
        {
            get { return m_Dictionary[key]; }
            set
            {
                AssertIsNotReadOnly();
                m_Dictionary[key] = value;
            }
        }

        object IDictionary.this[object key]
        {
            get { return (m_Dictionary as IDictionary)[key]; }
            set
            {
                AssertIsNotReadOnly();
                (m_Dictionary as IDictionary)[key] = value;
            }
        }

        #endregion

        #region Methods - Local

        public void AssertIsNotReadOnly()
        {
            IReadOnlyUtils.AssertIsNotReadOnly(this);
        }

        #endregion

        #region Methods - To mirror Dictionary<TKey, TValue>

        public void Add(TKey key, TValue value)
        {
            AssertIsNotReadOnly();
            m_Dictionary.Add(key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            AssertIsNotReadOnly();
            (m_Dictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        }

        void IDictionary.Add(object key, object value)
        {
            AssertIsNotReadOnly();
            (m_Dictionary as IDictionary).Add(key, value);
        }

        public void Clear()
        {
            AssertIsNotReadOnly();
            m_Dictionary.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        { return (m_Dictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item); }

        bool IDictionary.Contains(object key)
        { return (m_Dictionary as IDictionary).Contains(key); }

        public bool ContainsKey(TKey key)
        { return m_Dictionary.ContainsKey(key); }

        public bool ContainsValue(TValue value)
        { return m_Dictionary.ContainsValue(value); }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        { (m_Dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex); }

        void ICollection.CopyTo(Array array, int index)
        { (m_Dictionary as ICollection).CopyTo(array, index); }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        { return m_Dictionary.GetEnumerator(); }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        { return this.GetEnumerator(); }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        { return this.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator()
        { return this.GetEnumerator(); }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            AssertIsNotReadOnly();
            m_Dictionary.GetObjectData(info, context);
        }

        public virtual void OnDeserialization(object sender)
        { m_Dictionary.OnDeserialization(sender); }

        public bool Remove(TKey key)
        {
            AssertIsNotReadOnly();
            return m_Dictionary.Remove(key);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            AssertIsNotReadOnly();
            return (m_Dictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }

        void IDictionary.Remove(object key)
        {
            AssertIsNotReadOnly();
            (m_Dictionary as IDictionary).Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        { return m_Dictionary.TryGetValue(key, out value); }

        #endregion
    }
}