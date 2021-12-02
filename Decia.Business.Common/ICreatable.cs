using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public interface ICreatable
    {
        UserId CreatorId { get; }
        DateTime CreationDate { get; }
    }

    public static class ICreatableUtils
    {
        #region Static Methods - Asserts

        public static void AssertCreatorIsNotAnonymous(this ICreatable obj)
        {
            obj.CreatorId.AssertIsNotAnonymous("The current object's Creator must not be anonymous.");
        }

        #endregion
    }
}