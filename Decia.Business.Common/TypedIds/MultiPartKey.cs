using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.TypedIds
{
    public struct MultiPartKey<S1, S2> : IConvertible, IComparable
        where S1 : struct
        where S2 : struct
    {
        private S1 m_KeyPart1;
        private S2 m_KeyPart2;

        public MultiPartKey(S1 keyPart1, S2 keyPart2)
        {
            m_KeyPart1 = keyPart1;
            m_KeyPart2 = keyPart2;
        }

        public S1 KeyPart1
        { get { return m_KeyPart1; } }

        public S2 KeyPart2
        { get { return m_KeyPart2; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            MultiPartKey<S1, S2> otherKey = (MultiPartKey<S1, S2>)obj;
            bool areEqual = ((KeyPart1.Equals(otherKey.KeyPart1)) && (KeyPart2.Equals(otherKey.KeyPart2)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string format = "{0}, {1}";
            string value = string.Format(format, KeyPart1.ToString(), KeyPart2.ToString());
            return value;
        }

        public static bool operator ==(MultiPartKey<S1, S2> a, MultiPartKey<S1, S2> b)
        { return a.Equals(b); }

        public static bool operator !=(MultiPartKey<S1, S2> a, MultiPartKey<S1, S2> b)
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

            MultiPartKey<S1, S2> otherKey = (MultiPartKey<S1, S2>)obj;
            IComparable thisPart1 = (this.KeyPart1 as IComparable);
            IComparable thisPart2 = (this.KeyPart2 as IComparable);
            IComparable otherPart1 = (otherKey.KeyPart1 as IComparable);
            IComparable otherPart2 = (otherKey.KeyPart2 as IComparable);

            if (thisPart1.CompareTo(otherPart1) != 0)
            { return thisPart1.CompareTo(otherPart1); }
            return thisPart2.CompareTo(otherPart2);
        }

        #endregion
    }
}