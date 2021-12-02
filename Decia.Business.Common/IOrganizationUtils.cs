using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common
{
    public static class IOrganizationUtils
    {
        #region Static Members

        public static readonly string OrganizationGuid_PropName = ClassReflector.GetPropertyName((IOrganization x) => x.OrganizationGuid);
        public static readonly string OrganizationGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(OrganizationGuid_PropName);

        public const SiteActorType ActorType = SiteActorType.Organization;
        public static readonly Guid EmptyOrganizationGuid = ISiteActorUtils.EmptyActorGuid;

        #endregion

        #region Static Methods - Gets

        public static SiteActorId GetAsSiteActorId(this IOrganization organization)
        {
            return new SiteActorId(organization);
        }

        #endregion

        #region Static Methods - Asserts

        public static void AssertIsAnonymous(this IOrganization organization)
        {
            organization.AssertIsAnonymous("The current Organization must be anonymous.");
        }

        public static void AssertIsNotAnonymous(this IOrganization organization)
        {
            organization.AssertIsNotAnonymous("The current Organization must not be anonymous.");
        }

        #endregion
    }
}