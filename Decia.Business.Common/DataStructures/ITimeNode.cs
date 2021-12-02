using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.DataStructures
{
    public interface ITimeNode<T>
         where T : struct, IComparable
    {
        TimePeriod Period { get; }
        T Node { get; }
    }

    public class TimeNodeEqualityComparer<T> : IEqualityComparer<ITimeNode<T>>
       where T : struct, IComparable
    {
        protected IEqualityComparer<T> m_NodeComparer;

        public TimeNodeEqualityComparer()
            : this(null)
        { }

        public TimeNodeEqualityComparer(IEqualityComparer<T> nodeComparer)
        {
            m_NodeComparer = nodeComparer;
        }

        public bool Equals(ITimeNode<T> x, ITimeNode<T> y)
        {
            if (!x.Period.Equals(y.Period))
            { return false; }
            if (m_NodeComparer == null)
            {
                if (!x.Node.Equals(y.Node))
                { return false; }
            }
            else
            {
                if (!m_NodeComparer.Equals(x.Node, y.Node))
                { return false; }
            }
            return true;
        }

        public int GetHashCode(ITimeNode<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}