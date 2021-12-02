using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;

namespace Decia.Business.Domain
{
    public class ProjectMemberComparer_Default<T> : IEqualityComparer<T>, IStringGenerator<T>
        where T : IProjectMember
    {
        public bool Equals(T x, T y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        public string ToString(T obj)
        {
            return obj.ToString();
        }
    }

    public class ProjectMemberComparer_Revisionless<T> : IEqualityComparer<T>, IStringGenerator<T>
        where T : IProjectMember_Revisionless
    {
        public bool Equals(T x, T y)
        {
            return x.Equals_Revisionless(y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode_Revisionless();
        }

        public string ToString(T obj)
        {
            return obj.ToString_Revisionless();
        }
    }
}