using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public class MethodBasedComparer : IComparer
    {
        protected Func<object, object, int> m_CompareMethod;

        public MethodBasedComparer()
        {
            m_CompareMethod = null;
        }

        public MethodBasedComparer(Func<object, object, int> compareMethod)
        {
            m_CompareMethod = compareMethod;
        }

        public Func<object, object, int> CompareMethod { get { return m_CompareMethod; } }

        public int Compare(object x, object y)
        {
            if (m_CompareMethod == null)
            {
                var defaultComparer = new DefaultComparer();
                return defaultComparer.Compare(x, y);
            }

            return m_CompareMethod(x, y);
        }
    }

    public class MethodBasedComparer<T> : MethodBasedComparer, IComparer<T>
    {
        public MethodBasedComparer()
            : base()
        { }

        public MethodBasedComparer(Func<object, object, int> compareMethod)
            : base(compareMethod)
        { }

        public int Compare(T x, T y)
        {
            object xAsObj = (object)x;
            object yAsObj = (object)y;
            return Compare(xAsObj, yAsObj);
        }
    }
}