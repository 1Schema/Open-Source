using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.DataStructures
{
    public interface IEdge<T> : IConvertible, IComparable
        where T : struct, IComparable
    {
        T OutgoingNode { get; }
        T IncomingNode { get; }
    }

    public class EdgeEqualityComparer<T> : IEqualityComparer<IEdge<T>>
       where T : struct, IComparable
    {
        protected IEqualityComparer<T> m_NodeComparer;

        public EdgeEqualityComparer()
            : this(null)
        { }

        public EdgeEqualityComparer(IEqualityComparer<T> nodeComparer)
        {
            m_NodeComparer = nodeComparer;
        }

        public bool Equals(IEdge<T> x, IEdge<T> y)
        {
            if (m_NodeComparer == null)
            {
                if (!x.OutgoingNode.Equals(y.OutgoingNode))
                { return false; }
                if (!x.IncomingNode.Equals(y.IncomingNode))
                { return false; }
            }
            else
            {
                if (!m_NodeComparer.Equals(x.OutgoingNode, y.OutgoingNode))
                { return false; }
                if (!m_NodeComparer.Equals(x.IncomingNode, y.IncomingNode))
                { return false; }
            }
            return true;
        }

        public int GetHashCode(IEdge<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}