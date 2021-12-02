using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Collections
{
    public interface IReadOnly
    {
        bool IsReadOnly { get; }
    }

    public static class IReadOnlyUtils
    {
        public static void AssertIsNotReadOnly<T>(this T readOnlyObj)
            where T : IReadOnly
        {
            if (readOnlyObj.IsReadOnly)
            {
                var type = readOnlyObj.GetType();
                var message = string.Format("The {0} cannot be edited when it is in Read-Only mode.", type.Name);
                throw new InvalidOperationException(message);
            }
        }
    }
}