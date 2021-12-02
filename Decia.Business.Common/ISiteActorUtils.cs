using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common
{
    public static class ISiteActorUtils
    {
        #region Static Members

        public static readonly string ActorType_PropName = ClassReflector.GetPropertyName((ISiteActor x) => x.ActorType);
        public static readonly string ActorGuid_PropName = ClassReflector.GetPropertyName((ISiteActor x) => x.ActorGuid);

        public static readonly string ActorType_Prefix = KeyProcessingModeUtils.GetModalDebugText(ActorType_PropName);
        public static readonly string ActorGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(ActorGuid_PropName);

        public static readonly Guid AnonymousSiteActorGuid = Guid.Empty;

        public const SiteActorType EmptyActorType = ((SiteActorType)(-1));
        public static readonly Guid EmptyActorGuid = AnonymousSiteActorGuid;

        #endregion

        #region Static Methods - Gets

        public static UserId GetAsUserId(this ISiteActor siteActor)
        {
            if (siteActor.ActorType != SiteActorType.User)
            { throw new InvalidOperationException("The specified SiteActor is not a User."); }

            return new UserId(siteActor.ActorGuid);
        }

        public static WorkgroupId GetAsWorkgroupId(this ISiteActor siteActor)
        {
            if (siteActor.ActorType != SiteActorType.Workgroup)
            { throw new InvalidOperationException("The specified SiteActor is not a Workgroup."); }

            return new WorkgroupId(siteActor.ActorGuid);
        }

        public static OrganizationId GetAsOrganizationId(this ISiteActor siteActor)
        {
            if (siteActor.ActorType != SiteActorType.Organization)
            { throw new InvalidOperationException("The specified SiteActor is not a Organization."); }

            return new OrganizationId(siteActor.ActorGuid);
        }

        #endregion

        #region Static Methods - Checks

        public static bool IsAnonymous(this ISiteActor siteActor)
        {
            return (siteActor.ActorGuid == AnonymousSiteActorGuid);
        }

        public static bool IsNotAnonymous(this ISiteActor siteActor)
        {
            return !siteActor.IsAnonymous();
        }

        #endregion

        #region Static Methods - Asserts

        public static void AssertIsAnonymous(this ISiteActor siteActor, string failureMessage)
        {
            if (siteActor.IsNotAnonymous())
            { throw new InvalidOperationException(failureMessage); }
        }

        public static void AssertIsNotAnonymous(this ISiteActor siteActor, string failureMessage)
        {
            if (siteActor.IsAnonymous())
            { throw new InvalidOperationException(failureMessage); }
        }

        #endregion
    }
}