using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common
{
    public static class IUserUtils
    {
        #region Static Members

        public static readonly string UserGuid_PropName = ClassReflector.GetPropertyName((IUser x) => x.UserGuid);
        public static readonly string UserGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(UserGuid_PropName);

        public const SiteActorType ActorType = SiteActorType.User;
        public static readonly Guid EmptyUserGuid = ISiteActorUtils.EmptyActorGuid;

        #endregion

        #region Static Methods - Gets

        public static SiteActorId GetAsSiteActorId(this IUser user)
        {
            return new SiteActorId(user);
        }

        #endregion

        #region Static Methods - Asserts

        public static void AssertIsAnonymous(this IUser user)
        {
            user.AssertIsAnonymous("The current User must be anonymous.");
        }

        public static void AssertIsNotAnonymous(this IUser user)
        {
            user.AssertIsNotAnonymous("The current User must not be anonymous.");
        }

        #endregion
    }
}