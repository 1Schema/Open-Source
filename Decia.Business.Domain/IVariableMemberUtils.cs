using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public static class IVariableMemberUtils
    {
        #region Static Members

        public static readonly string ProjectGuid_PropName = IModelMemberUtils.ProjectGuid_PropName;
        public static readonly string IsRevisionSpecific_PropName = IModelMemberUtils.IsRevisionSpecific_PropName;
        public static readonly string RevisionNumber_PropName = IModelMemberUtils.RevisionNumber_PropName;
        public static readonly string RevisionNumber_NonNull_PropName = IModelMemberUtils.RevisionNumber_NonNull_PropName;
        public static readonly string IsInstance_PropName = IModelMemberUtils.IsInstance_PropName;
        public static readonly string ModelTemplateNumber_PropName = IModelMemberUtils.ModelTemplateNumber_PropName;
        public static readonly string ModelInstanceGuid_PropName = IModelMemberUtils.ModelInstanceGuid_PropName;
        public static readonly string VariableTemplateNumber_PropName = ClassReflector.GetPropertyName((IVariableMember x) => x.VariableTemplateNumber);
        public static readonly string VariableInstanceGuid_PropName = ClassReflector.GetPropertyName((IVariableMember x) => x.VariableInstanceGuid);

        public static readonly string ProjectGuid_Prefix = IModelMemberUtils.ProjectGuid_Prefix;
        public static readonly string IsRevisionSpecific_Prefix = IModelMemberUtils.IsRevisionSpecific_Prefix;
        public static readonly string RevisionNumber_Prefix = IModelMemberUtils.RevisionNumber_Prefix;
        public static readonly string RevisionNumber_NonNull_Prefix = IModelMemberUtils.RevisionNumber_NonNull_Prefix;
        public static readonly string IsInstance_Prefix = IModelMemberUtils.IsInstance_Prefix;
        public static readonly string ModelTemplateNumber_Prefix = IModelMemberUtils.ModelTemplateNumber_Prefix;
        public static readonly string ModelInstanceGuid_Prefix = IModelMemberUtils.ModelInstanceGuid_Prefix;
        public static readonly string VariableTemplateNumber_Prefix = KeyProcessingModeUtils.GetModalDebugText(VariableTemplateNumber_PropName);
        public static readonly string VariableInstanceGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(VariableInstanceGuid_PropName);

        public static readonly long RevisionNumber_Min = IModelMemberUtils.RevisionNumber_Min;
        public static readonly long RevisionNumber_Max = IModelMemberUtils.RevisionNumber_Max;

        public static readonly Guid ProjectGuid_Empty = IModelMemberUtils.ProjectGuid_Empty;
        public static readonly long RevisionNumber_Empty = IModelMemberUtils.RevisionNumber_Empty;
        public static readonly int ModelTemplateNumber_Empty = IModelMemberUtils.ModelTemplateNumber_Empty;
        public static readonly Nullable<Guid> ModelInstanceGuid_Null = IModelMemberUtils.ModelInstanceGuid_Null;
        public static readonly Guid ModelInstanceGuid_Empty = IModelMemberUtils.ModelInstanceGuid_Empty;
        public static readonly int VariableTemplateNumber_Empty = ModelTemplateNumber_Empty;
        public static readonly Nullable<Guid> VariableInstanceGuid_Null = ModelInstanceGuid_Null;
        public static readonly Guid VariableInstanceGuid_Empty = ModelInstanceGuid_Empty;

        #endregion

        #region Static Methods - Gets

        public static VariableMemberId GetVariableMemberId(this IVariableMember variableMember)
        {
            if (!variableMember.IsInstance)
            { return new VariableMemberId(variableMember.ProjectGuid, variableMember.RevisionNumber, variableMember.ModelTemplateNumber, variableMember.VariableTemplateNumber); }
            else
            { return new VariableMemberId(variableMember.ProjectGuid, variableMember.RevisionNumber, variableMember.ModelTemplateNumber, variableMember.ModelInstanceGuid.Value, variableMember.VariableTemplateNumber, variableMember.VariableInstanceGuid.Value); }
        }

        public static VariableMemberId GetVariableMemberId_ForTemplate(this IVariableMember variableMember)
        {
            return new VariableMemberId(variableMember.ProjectGuid, variableMember.RevisionNumber, variableMember.ModelTemplateNumber, variableMember.VariableTemplateNumber);
        }

        #endregion

        #region Static Methods - Checks

        public static bool AreVariableMemberIdsEqual(this IVariableMember first, IVariableMember second)
        {
            return AreVariableMemberIdsEqual(first, new IVariableMember[] { second });
        }

        public static bool AreVariableMemberIdsEqual(this IVariableMember first, IVariableMember second, IVariableMember third)
        {
            return AreVariableMemberIdsEqual(first, new IVariableMember[] { second, third });
        }

        public static bool AreVariableMemberIdsEqual(this IVariableMember main, IEnumerable<IVariableMember> others)
        {
            foreach (var other in others)
            {
                var mainId = main.GetVariableMemberId();
                var otherId = other.GetVariableMemberId();

                if (!mainId.Equals(otherId))
                { return false; }
            }
            return true;
        }

        #endregion

        #region Static Methods - Asserts

        public static void AssertVariableMemberIdsAreEqual(this IVariableMember first, IVariableMember second)
        {
            AssertVariableMemberIdsAreEqual(first, new IVariableMember[] { second });
        }

        public static void AssertVariableMemberIdsAreEqual(this IVariableMember first, IVariableMember second, IVariableMember third)
        {
            AssertVariableMemberIdsAreEqual(first, new IVariableMember[] { second, third });
        }

        public static void AssertVariableMemberIdsAreEqual(this IVariableMember main, IEnumerable<IVariableMember> others)
        {
            if (!AreVariableMemberIdsEqual(main, others))
            { throw new InvalidOperationException("The objects provided do not have the same VariableMemberId values."); }
        }

        #endregion
    }
}