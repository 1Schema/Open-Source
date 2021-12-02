using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public class DefaultComparer : IComparer
    {
        public DefaultComparer()
        { }

        public int Compare(object x, object y)
        {
            var xType = x.GetType();
            var yType = y.GetType();

            if (xType != yType)
            { throw new InvalidOperationException("The types being compared do not match."); }

            if (!(x is IComparable))
            { throw new InvalidOperationException("The specified type is not Comparable."); }

            var xAsIComparable = (IComparable)x;
            return xAsIComparable.CompareTo(y);
        }
    }

    public class DefaultComparer<T> : DefaultComparer, IComparer<T>
    {
        public DefaultComparer()
            : base()
        { }

        public int Compare(T x, T y)
        {
            object xAsObj = (object)x;
            object yAsObj = (object)y;
            return Compare(xAsObj, yAsObj);
        }
    }
}