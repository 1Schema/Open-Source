using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.DataStructures
{
    public struct TimeEdge<T> : ITimeEdge<T>
        where T : struct, IComparable
    {
        private TimePeriod m_Period;
        private T m_OutgoingNode;
        private T m_IncomingNode;

        public TimeEdge(IEdge<T> edge)
            : this(edge.OutgoingNode, edge.IncomingNode)
        { }

        public TimeEdge(T outgoingNode, T incomingNode)
            : this(new TimePeriod(true), outgoingNode, incomingNode)
        { }

        public TimeEdge(TimePeriod period, IEdge<T> edge)
            : this(period, edge.OutgoingNode, edge.IncomingNode)
        { }

        public TimeEdge(TimePeriod period, T outgoingNode, T incomingNode)
        {
            m_Period = period;
            m_OutgoingNode = outgoingNode;
            m_IncomingNode = incomingNode;
        }

        public TimePeriod Period
        { get { return m_Period; } }

        public ITimeNode<T> OutgoingTimeNode
        { get { return new TimeNode<T>(m_Period, m_OutgoingNode); } }

        public ITimeNode<T> IncomingTimeNode
        { get { return new TimeNode<T>(m_Period, m_IncomingNode); } }

        public IEdge<T> Edge
        { get { return new Edge<T>(OutgoingNode, IncomingNode); } }

        public T OutgoingNode
        { get { return m_OutgoingNode; } }

        public T IncomingNode
        { get { return m_IncomingNode; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            if (this.m_Period.IsForever && (obj is IEdge<T>))
            {
                IEdge<T> otherEdge = (IEdge<T>)obj;
                return (this.Edge.Equals(otherEdge));
            }
            else
            {
                if (!(obj is ITimeEdge<T>))
                { return false; }

                ITimeEdge<T> otherEdge = (ITimeEdge<T>)obj;
                bool areEqual = ((Period.Equals(otherEdge.Period))
                    && (OutgoingNode.Equals(otherEdge.OutgoingNode))
                    && (IncomingNode.Equals(otherEdge.IncomingNode)));
                return areEqual;
            }
        }

        public override int GetHashCode()
        {
            if (this.m_Period.IsForever)
            {
                return this.Edge.GetHashCode();
            }
            else
            {
                return this.ToString().GetHashCode();
            }
        }

        public static readonly string TextFormat = KeyProcessingModeUtils.GetModalDebugText("Period: {0}, Outgoing: {1}, Incoming: {2}", "P:{0},O:{1},I:{2}");

        public override string ToString()
        {
            string value = string.Format(TextFormat, Period.ToString(), OutgoingNode.ToString(), IncomingNode.ToString());
            return value;
        }

        public static bool operator ==(TimeEdge<T> a, ITimeEdge<T> b)
        { return a.Equals(b); }

        public static bool operator ==(TimeEdge<T> a, IEdge<T> b)
        { return a.Equals(b); }

        public static bool operator !=(TimeEdge<T> a, ITimeEdge<T> b)
        { return !(a == b); }

        public static bool operator !=(TimeEdge<T> a, IEdge<T> b)
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

            ITimeEdge<T> otherEdge = (ITimeEdge<T>)obj;

            IComparable thisPeriod = (this.Period as IComparable);
            IComparable thisOutgoingNode = (this.OutgoingNode as IComparable);
            IComparable thisIncomingNode = (this.IncomingNode as IComparable);
            IComparable otherPeriod = (otherEdge.Period as IComparable);
            IComparable otherOutgoingNode = (otherEdge.OutgoingNode as IComparable);
            IComparable otherIncomingNode = (otherEdge.IncomingNode as IComparable);

            int periodResult = thisPeriod.CompareTo(otherPeriod);
            if (periodResult != 0)
            { return periodResult; }

            int outgoingResult = thisOutgoingNode.CompareTo(otherOutgoingNode);
            if (outgoingResult != 0)
            { return outgoingResult; }

            return thisIncomingNode.CompareTo(otherIncomingNode);
        }

        #endregion
    }
}