using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.DataStructures
{
    public interface ITimeEdge<T> : IEdge<T>
         where T : struct, IComparable
    {
        TimePeriod Period { get; }
        ITimeNode<T> OutgoingTimeNode { get; }
        ITimeNode<T> IncomingTimeNode { get; }
        IEdge<T> Edge { get; }
    }

    public class TimeEdgeEqualityComparer<T> : IEqualityComparer<ITimeEdge<T>>
       where T : struct, IComparable
    {
        protected IEqualityComparer<T> m_NodeComparer;
        protected EdgeEqualityComparer<T> m_EdgeComparer;

        public TimeEdgeEqualityComparer()
            : this(null)
        { }

        public TimeEdgeEqualityComparer(IEqualityComparer<T> nodeComparer)
        {
            m_NodeComparer = nodeComparer;
            m_EdgeComparer = new EdgeEqualityComparer<T>(m_NodeComparer);
        }

        public bool Equals(ITimeEdge<T> x, ITimeEdge<T> y)
        {
            if (!x.Period.Equals(y.Period))
            { return false; }
            if (!m_EdgeComparer.Equals(x.Edge, y.Edge))
            { return false; }
            return true;
        }

        public int GetHashCode(ITimeEdge<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}