using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.DataStructures
{
    public struct Edge<T> : IEdge<T>
        where T : struct, IComparable
    {
        private T m_OutgoingNode;
        private T m_IncomingNode;

        public Edge(T outgoingNode, T incomingNode)
        {
            m_OutgoingNode = outgoingNode;
            m_IncomingNode = incomingNode;
        }

        public T OutgoingNode
        { get { return m_OutgoingNode; } }

        public T IncomingNode
        { get { return m_IncomingNode; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IEdge<T>))
            { return false; }

            IEdge<T> otherEdge = (IEdge<T>)obj;
            bool areEqual = ((OutgoingNode.Equals(otherEdge.OutgoingNode)) && (IncomingNode.Equals(otherEdge.IncomingNode)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public static readonly string TextFormat = KeyProcessingModeUtils.GetModalDebugText("Outgoing: {0}, Incoming: {1}", "O:{0},I:{1}");

        public override string ToString()
        {
            string value = string.Format(TextFormat, OutgoingNode.ToString(), IncomingNode.ToString());
            return value;
        }

        public static bool operator ==(Edge<T> a, IEdge<T> b)
        { return a.Equals(b); }

        public static bool operator !=(Edge<T> a, IEdge<T> b)
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

            IEdge<T> otherEdge = (IEdge<T>)obj;

            IComparable thisOutgoingNode = (this.OutgoingNode as IComparable);
            IComparable thisIncomingNode = (this.IncomingNode as IComparable);
            IComparable otherOutgoingNode = (otherEdge.OutgoingNode as IComparable);
            IComparable otherIncomingNode = (otherEdge.IncomingNode as IComparable);

            int outgoingResult = thisOutgoingNode.CompareTo(otherOutgoingNode);
            if (outgoingResult != 0)
            { return outgoingResult; }

            return thisIncomingNode.CompareTo(otherIncomingNode);
        }

        #endregion
    }
}