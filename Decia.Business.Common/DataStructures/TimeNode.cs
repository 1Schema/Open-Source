using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.DataStructures
{
    public struct TimeNode<T> : ITimeNode<T>
        where T : struct, IComparable
    {
        private TimePeriod m_Period;
        private T m_Node;

        public TimeNode(T node)
            : this(new TimePeriod(true), node)
        { }

        public TimeNode(TimePeriod period, T node)
        {
            m_Period = period;
            m_Node = node;
        }

        public TimePeriod Period
        { get { return m_Period; } }

        public T Node
        { get { return m_Node; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            if (this.m_Period.IsForever && (obj is T))
            {
                T otherNode = (T)obj;
                return (this.Node.Equals(otherNode));
            }
            else
            {
                if (!(obj is ITimeNode<T>))
                { return false; }

                ITimeNode<T> otherNode = (ITimeNode<T>)obj;
                bool areEqual = ((Period.Equals(otherNode.Period))
                    && (Node.Equals(otherNode.Node)));
                return areEqual;
            }
        }

        public override int GetHashCode()
        {
            if (this.m_Period.IsForever)
            {
                return this.Node.GetHashCode();
            }
            else
            {
                return this.ToString().GetHashCode();
            }
        }

        public static readonly string TextFormat = KeyProcessingModeUtils.GetModalDebugText("Period: {0}, Node: {1}", "P:{0},N:{1}");

        public override string ToString()
        {
            string value = string.Format(TextFormat, Period.ToString(), Node.ToString());
            return value;
        }

        public static bool operator ==(TimeNode<T> a, ITimeNode<T> b)
        { return a.Equals(b); }

        public static bool operator ==(TimeNode<T> a, T b)
        { return a.Equals(b); }

        public static bool operator !=(TimeNode<T> a, ITimeNode<T> b)
        { return !(a == b); }

        public static bool operator !=(TimeNode<T> a, T b)
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

            ITimeNode<T> otherTimeNode = (ITimeNode<T>)obj;

            IComparable thisPeriod = (this.Period as IComparable);
            IComparable thisNode = (this.Node as IComparable);
            IComparable otherPeriod = (otherTimeNode.Period as IComparable);
            IComparable otherNode = (otherTimeNode.Node as IComparable);

            int periodResult = thisPeriod.CompareTo(otherPeriod);
            if (periodResult != 0)
            { return periodResult; }

            return thisNode.CompareTo(otherNode);
        }

        #endregion
    }
}