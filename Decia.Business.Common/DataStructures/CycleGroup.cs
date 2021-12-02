using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.DataStructures
{
    public struct CycleGroup<T> : ICycleGroup<T>
        where T : struct, IComparable
    {
        private ReadOnlyHashSet<T> m_NodesIncluded;
        private ReadOnlyHashSet<IEdge<T>> m_EdgesIncluded;

        public CycleGroup(IEnumerable<T> nodesIncluded)
            : this(nodesIncluded, (IEqualityComparer<T>)null)
        { }

        public CycleGroup(IEnumerable<T> nodesIncluded, IEqualityComparer<T> nodeComparer)
            : this(nodesIncluded, new List<IEdge<T>>(), nodeComparer)
        { }

        public CycleGroup(IEnumerable<T> nodesIncluded, IEnumerable<IEdge<T>> edgesIncluded)
            : this(nodesIncluded, edgesIncluded, (IEqualityComparer<T>)null)
        { }

        public CycleGroup(IEnumerable<T> nodesIncluded, IEnumerable<IEdge<T>> edgesIncluded, IEqualityComparer<T> nodeComparer)
        {
            if ((nodesIncluded == null) || (edgesIncluded == null))
            { throw new InvalidOperationException("The \"nodesIncluded\" and \"edgesIncluded\" must not be null."); }

            var edgeComparer = new EdgeEqualityComparer<T>(nodeComparer);

            m_NodesIncluded = new ReadOnlyHashSet<T>(nodesIncluded, nodeComparer);
            m_EdgesIncluded = new ReadOnlyHashSet<IEdge<T>>(edgesIncluded, edgeComparer);
        }

        public ICollection<T> NodesIncluded
        { get { return m_NodesIncluded; } }

        public ICollection<IEdge<T>> EdgesIncluded
        { get { return m_EdgesIncluded; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ICycleGroup<T>))
            { return false; }

            ICycleGroup<T> otherCycleGroup = (ICycleGroup<T>)obj;
            ICollection<T> otherNodesInCycle = otherCycleGroup.NodesIncluded;

            if (m_NodesIncluded.Count != otherNodesInCycle.Count)
            { return false; }

            for (int i = 0; i < m_NodesIncluded.Count; i++)
            {
                T thisNode = m_NodesIncluded.ElementAt(i);
                T otherNode = otherNodesInCycle.ElementAt(i);

                if (!thisNode.Equals(otherNode))
                { return false; }
            }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public static readonly string NodeList_Prefix = KeyProcessingModeUtils.GetModalDebugText("Nodes In Cycle Group: ", "G:");

        public override string ToString()
        {
            string stringValue = NodeList_Prefix;

            for (int i = 0; i < m_NodesIncluded.Count; i++)
            {
                T node = m_NodesIncluded.ElementAt(i);

                if (i == 0)
                {
                    stringValue += node.ToString();
                }
                else
                {
                    stringValue += ", " + node.ToString();
                }
            }

            return stringValue;
        }

        public static bool operator ==(CycleGroup<T> a, ICycleGroup<T> b)
        { return a.Equals(b); }

        public static bool operator !=(CycleGroup<T> a, ICycleGroup<T> b)
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
    }
}