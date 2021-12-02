using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public static class IModelMemberUtils
    {
        #region Static Members

        public static readonly string ProjectGuid_PropName = IProjectMemberUtils.ProjectGuid_PropName;
        public static readonly string IsRevisionSpecific_PropName = IProjectMemberUtils.IsRevisionSpecific_PropName;
        public static readonly string RevisionNumber_PropName = IProjectMemberUtils.RevisionNumber_PropName;
        public static readonly string RevisionNumber_NonNull_PropName = IProjectMemberUtils.RevisionNumber_NonNull_PropName;
        public static readonly string IsInstance_PropName = ClassReflector.GetPropertyName((IModelMember x) => x.IsInstance);
        public static readonly string ModelTemplateNumber_PropName = ClassReflector.GetPropertyName((IModelMember x) => x.ModelTemplateNumber);
        public static readonly string ModelInstanceGuid_PropName = ClassReflector.GetPropertyName((IModelMember x) => x.ModelInstanceGuid);

        public static readonly string ProjectGuid_Prefix = IProjectMemberUtils.ProjectGuid_Prefix;
        public static readonly string IsRevisionSpecific_Prefix = IProjectMemberUtils.IsRevisionSpecific_Prefix;
        public static readonly string RevisionNumber_Prefix = IProjectMemberUtils.RevisionNumber_Prefix;
        public static readonly string RevisionNumber_NonNull_Prefix = IProjectMemberUtils.RevisionNumber_NonNull_Prefix;
        public static readonly string IsInstance_Prefix = KeyProcessingModeUtils.GetModalDebugText(IsInstance_PropName);
        public static readonly string ModelTemplateNumber_Prefix = KeyProcessingModeUtils.GetModalDebugText(ModelTemplateNumber_PropName);
        public static readonly string ModelInstanceGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(ModelInstanceGuid_PropName);

        public static readonly long RevisionNumber_Min = IProjectMemberUtils.RevisionNumber_Min;
        public static readonly long RevisionNumber_Max = IProjectMemberUtils.RevisionNumber_Max;

        public static readonly Guid ProjectGuid_Empty = IProjectMemberUtils.ProjectGuid_Empty;
        public static readonly long RevisionNumber_Empty = IProjectMemberUtils.RevisionNumber_Empty;
        public static readonly int ModelTemplateNumber_Empty = 0;
        public static readonly int ModelTemplateNumber_Singleton = 1;
        public static readonly Nullable<Guid> ModelInstanceGuid_Null = null;
        public static readonly Guid ModelInstanceGuid_Empty = Guid.Empty;

        #endregion

        #region Static Methods - Gets

        public static ModelMemberId GetModelMemberId(this IModelMember modelMember)
        {
            if (!modelMember.IsInstance)
            { return new ModelMemberId(modelMember.ProjectGuid, modelMember.RevisionNumber, modelMember.ModelTemplateNumber); }
            else
            { return new ModelMemberId(modelMember.ProjectGuid, modelMember.RevisionNumber, modelMember.ModelTemplateNumber, modelMember.ModelInstanceGuid.Value); }
        }

        public static ModelMemberId GetModelMemberId_ForTemplate(this IModelMember modelMember)
        {
            return new ModelMemberId(modelMember.ProjectGuid, modelMember.RevisionNumber, modelMember.ModelTemplateNumber);
        }

        public static ModelObjectDescriptor GetKeyedModelObjectValue(this IModelMember_Orderable modelMember)
        {
            var value = new ModelObjectDescriptor(modelMember.ModelObjectRef, modelMember.OrderNumber, modelMember.OrderValue, modelMember);
            return value;
        }

        #endregion

        #region Static Methods - Checks

        public static bool AreModelMemberIdsEqual(this IModelMember first, IModelMember second)
        {
            return AreModelMemberIdsEqual(first, new IModelMember[] { second });
        }

        public static bool AreModelMemberIdsEqual(this IModelMember first, IModelMember second, IModelMember third)
        {
            return AreModelMemberIdsEqual(first, new IModelMember[] { second, third });
        }

        public static bool AreModelMemberIdsEqual(this IModelMember main, IEnumerable<IModelMember> others)
        {
            foreach (var other in others)
            {
                var mainId = main.GetModelMemberId();
                var otherId = other.GetModelMemberId();

                if (!mainId.Equals(otherId))
                { return false; }
            }
            return true;
        }

        #endregion

        #region Static Methods - Asserts

        public static void AssertModelMemberIdsAreEqual(this IModelMember first, IModelMember second)
        {
            AssertModelMemberIdsAreEqual(first, new IModelMember[] { second });
        }

        public static void AssertModelMemberIdsAreEqual(this IModelMember first, IModelMember second, IModelMember third)
        {
            AssertModelMemberIdsAreEqual(first, new IModelMember[] { second, third });
        }

        public static void AssertModelMemberIdsAreEqual(this IModelMember main, IEnumerable<IModelMember> others)
        {
            if (!AreModelMemberIdsEqual(main, others))
            { throw new InvalidOperationException("The objects provided do not have the same ModelMemberId values."); }
        }

        #endregion
    }
}