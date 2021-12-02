using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common
{
    public static class IWorkgroupUtils
    {
        #region Static Members

        public static readonly string WorkgroupGuid_PropName = ClassReflector.GetPropertyName((IWorkgroup x) => x.WorkgroupGuid);
        public static readonly string WorkgroupGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(WorkgroupGuid_PropName);

        public const SiteActorType ActorType = SiteActorType.Workgroup;
        public static readonly Guid EmptyWorkgroupGuid = ISiteActorUtils.EmptyActorGuid;

        #endregion

        #region Static Methods - Gets

        public static SiteActorId GetAsSiteActorId(this IWorkgroup workgroup)
        {
            return new SiteActorId(workgroup);
        }

        #endregion

        #region Static Methods - Asserts

        public static void AssertIsAnonymous(this IWorkgroup workgroup)
        {
            workgroup.AssertIsAnonymous("The current Workgroup must be anonymous.");
        }

        public static void AssertIsNotAnonymous(this IWorkgroup workgroup)
        {
            workgroup.AssertIsNotAnonymous("The current Workgroup must not be anonymous.");
        }

        #endregion
    }
}