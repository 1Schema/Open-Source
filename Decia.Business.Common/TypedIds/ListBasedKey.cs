using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;

namespace Decia.Business.Common.TypedIds
{
    public struct ListBasedKey<T> : IConvertible, IComparable
        where T : struct, IComparable
    {
        private ReadOnlyList<T> m_KeyPartItems;
        private IEqualityComparer<T> m_EqualityComparer;
        private IStringGenerator<T> m_StringGenerator;

        public ListBasedKey(T keyPart)
            : this(keyPart, null)
        { }

        public ListBasedKey(T keyPart, IEqualityComparer<T> equalityComparer)
            : this(new T[] { keyPart }, equalityComparer)
        { }

        public ListBasedKey(IEnumerable<T> keyParts)
            : this(keyParts, null)
        { }

        public ListBasedKey(IEnumerable<T> keyParts, IEqualityComparer<T> equalityComparer)
        {
            m_KeyPartItems = new ReadOnlyList<T>(keyParts);
            m_KeyPartItems.IsReadOnly = true;
            m_EqualityComparer = equalityComparer;
            m_StringGenerator = equalityComparer as IStringGenerator<T>;
        }

        public ListBasedKey(ListBasedKey<T> previousKey, T additionalKeyPart)
            : this(previousKey, new T[] { additionalKeyPart })
        { }

        public ListBasedKey(ListBasedKey<T> previousKey, IEnumerable<T> additionalKeyParts)
        {
            List<T> allKeyParts = new List<T>();
            allKeyParts.AddRange(previousKey.KeyPartItems);
            allKeyParts.AddRange(additionalKeyParts);

            m_KeyPartItems = new ReadOnlyList<T>(allKeyParts);
            m_KeyPartItems.IsReadOnly = true;
            m_EqualityComparer = previousKey.m_EqualityComparer;
            m_StringGenerator = previousKey.m_StringGenerator;
        }

        public IList<T> KeyPartItems
        { get { return m_KeyPartItems; } }

        public bool HasEqualityComparer
        { get { return (m_EqualityComparer != null); } }

        public IEqualityComparer<T> EqualityComparer
        { get { return m_EqualityComparer; } }

        public bool HasStringGenerator
        { get { return (m_StringGenerator != null); } }

        public IStringGenerator<T> StringGenerator
        { get { return m_StringGenerator; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            ListBasedKey<T> otherKey = (ListBasedKey<T>)obj;
            for (int i = 0; i < m_KeyPartItems.Count; i++)
            {
                T thisKeyPartItem = m_KeyPartItems[i];
                T otherKeyPartItem = otherKey.m_KeyPartItems[i];

                if (HasEqualityComparer)
                {
                    if (!EqualityComparer.Equals(otherKeyPartItem, thisKeyPartItem))
                    { return false; }
                }
                else
                {
                    if (!otherKeyPartItem.Equals(thisKeyPartItem))
                    { return false; }
                }
            }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string thisAsString = string.Empty;

            for (int i = 0; i < m_KeyPartItems.Count; i++)
            {
                T thisKeyPartItem = m_KeyPartItems[i];
                string thisKeyPartItemAsString = HasStringGenerator ? StringGenerator.ToString(thisKeyPartItem) : thisKeyPartItem.ToString();

                if (string.IsNullOrWhiteSpace(thisAsString))
                { thisAsString += thisKeyPartItemAsString; }
                else
                { thisAsString += "," + thisKeyPartItemAsString; }
            }
            return thisAsString;
        }

        public static bool operator ==(ListBasedKey<T> a, ListBasedKey<T> b)
        { return a.Equals(b); }

        public static bool operator !=(ListBasedKey<T> a, ListBasedKey<T> b)
        { return !(a == b); }

        #region IConvertible Implementation

        public TypeCode GetTypeCode()
        { return TypeCode.Object; }

        public bool ToBoolean(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public byte ToByte(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public char ToChar(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public DateTime ToDateTime(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public decimal ToDecimal(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public double ToDouble(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public short ToInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public int ToInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public long ToInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public sbyte ToSByte(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public float ToSingle(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public string ToString(IFormatProvider provider)
        { return this.ToString(); }

        public object ToType(Type conversionType, IFormatProvider provider)
        { return ToString(provider); }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion

        #region IComparable Implementation

        public int CompareTo(object obj)
        {
            if (obj == null)
            { return -1; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return -1; }

            ListBasedKey<T> otherKey = (ListBasedKey<T>)obj;
            for (int i = 0; i < m_KeyPartItems.Count; i++)
            {
                T thisKeyPartItem = m_KeyPartItems[i];
                T otherKeyPartItem = otherKey.m_KeyPartItems[i];

                int compResult = thisKeyPartItem.CompareTo(otherKeyPartItem);

                if (compResult != 0)
                { return compResult; }
            }
            return 0;
        }

        #endregion
    }
}