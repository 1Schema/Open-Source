using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public static class ITextPersistableUtils
    {
        public static bool IsTextPersistable(this Type typeToPersist)
        {
            var interfacesForType = typeToPersist.GetInterfaces();
            var isTextPersistable = interfacesForType.Any(x => (x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITextPersistable<>)));
            return isTextPersistable;
        }

        public static void AssertIsTextPersistable(this Type typeToPersist)
        {
            if (!typeToPersist.IsTextPersistable())
            { throw new InvalidOperationException("The specified type does not implement ITextPersistable."); }
        }
    }
}