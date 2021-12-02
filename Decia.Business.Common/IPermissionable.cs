using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Common
{
    public interface IPermissionable : ICreatable
    {
        SiteActorId OwnerId { get; }
        SiteActorType OwnerType { get; }
        Guid OwnerGuid { get; }

        bool IsOwnerEditable();
        void SetOwner(Nullable<SiteActorId> newOwnerId);
    }

    public static class IPermissionableUtils
    {
        #region Static Methods - Asserts

        public static void AssertOwnerIsNotAnonymous(this IPermissionable obj)
        {
            obj.CreatorId.AssertIsNotAnonymous("The current object's Owner must not be anonymous.");
        }

        #endregion
    }
}