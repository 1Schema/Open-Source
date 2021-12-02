using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Collections
{
    public class EqualityComparerForComparer<T> : IEqualityComparer<T>, IComparer<T>
    {
        protected IComparer<T> m_Comparer;

        public EqualityComparerForComparer(IComparer<T> comparer)
        {
            if (comparer == null)
            { throw new InvalidOperationException("The Comparer passed-in must not be null."); }

            m_Comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return m_Comparer.Compare(x, y);
        }

        public bool Equals(T x, T y)
        {
            return (Compare(x, y) == 0);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}