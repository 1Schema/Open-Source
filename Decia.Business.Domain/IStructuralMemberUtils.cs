using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public static class IStructuralMemberUtils
    {
        #region Static Members

        public static readonly string ProjectGuid_PropName = IModelMemberUtils.ProjectGuid_PropName;
        public static readonly string IsRevisionSpecific_PropName = IModelMemberUtils.IsRevisionSpecific_PropName;
        public static readonly string RevisionNumber_PropName = IModelMemberUtils.RevisionNumber_PropName;
        public static readonly string RevisionNumber_NonNull_PropName = IModelMemberUtils.RevisionNumber_NonNull_PropName;
        public static readonly string IsInstance_PropName = IModelMemberUtils.IsInstance_PropName;
        public static readonly string ModelTemplateNumber_PropName = IModelMemberUtils.ModelTemplateNumber_PropName;
        public static readonly string ModelInstanceGuid_PropName = IModelMemberUtils.ModelInstanceGuid_PropName;
        public static readonly string StructuralTypeOption_PropName = ClassReflector.GetPropertyName((IStructuralMember x) => x.StructuralTypeOption);
        public static readonly string StructuralTypeNumber_PropName = ClassReflector.GetPropertyName((IStructuralMember x) => x.StructuralTypeNumber);
        public static readonly string StructuralInstanceGuid_PropName = ClassReflector.GetPropertyName((IStructuralMember x) => x.StructuralInstanceGuid);

        public static readonly string ProjectGuid_Prefix = IModelMemberUtils.ProjectGuid_Prefix;
        public static readonly string IsRevisionSpecific_Prefix = IModelMemberUtils.IsRevisionSpecific_Prefix;
        public static readonly string RevisionNumber_Prefix = IModelMemberUtils.RevisionNumber_Prefix;
        public static readonly string RevisionNumber_NonNull_Prefix = IModelMemberUtils.RevisionNumber_NonNull_Prefix;
        public static readonly string IsInstance_Prefix = IModelMemberUtils.IsInstance_Prefix;
        public static readonly string ModelTemplateNumber_Prefix = IModelMemberUtils.ModelTemplateNumber_Prefix;
        public static readonly string ModelInstanceGuid_Prefix = IModelMemberUtils.ModelInstanceGuid_Prefix;
        public static readonly string StructuralTypeOption_Prefix = KeyProcessingModeUtils.GetModalDebugText(StructuralTypeOption_PropName);
        public static readonly string StructuralTypeNumber_Prefix = KeyProcessingModeUtils.GetModalDebugText(StructuralTypeNumber_PropName);
        public static readonly string StructuralInstanceGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(StructuralInstanceGuid_PropName);

        public static readonly long RevisionNumber_Min = IModelMemberUtils.RevisionNumber_Min;
        public static readonly long RevisionNumber_Max = IModelMemberUtils.RevisionNumber_Max;

        public static readonly Guid ProjectGuid_Empty = IModelMemberUtils.ProjectGuid_Empty;
        public static readonly long RevisionNumber_Empty = IModelMemberUtils.RevisionNumber_Empty;
        public static readonly int ModelTemplateNumber_Empty = IModelMemberUtils.ModelTemplateNumber_Empty;
        public static readonly Nullable<Guid> ModelInstanceGuid_Null = IModelMemberUtils.ModelInstanceGuid_Null;
        public static readonly Guid ModelInstanceGuid_Empty = IModelMemberUtils.ModelInstanceGuid_Empty;
        public static readonly StructuralTypeOption StructuralTypeOption_Empty = StructuralTypeOption.GlobalType;
        public static readonly int StructuralTypeNumber_Empty = ModelTemplateNumber_Empty;
        public static readonly Nullable<Guid> StructuralInstanceGuid_Null = ModelInstanceGuid_Null;
        public static readonly Guid StructuralInstanceGuid_Empty = ModelInstanceGuid_Empty;

        #endregion

        #region Static Methods - Gets

        public static StructuralMemberId GetStructuralMemberId(this IStructuralMember structuralMember)
        {
            if (!structuralMember.IsInstance)
            { return new StructuralMemberId(structuralMember.ProjectGuid, structuralMember.RevisionNumber, structuralMember.ModelTemplateNumber, structuralMember.StructuralTypeOption, structuralMember.StructuralTypeNumber); }
            else
            { return new StructuralMemberId(structuralMember.ProjectGuid, structuralMember.RevisionNumber, structuralMember.ModelTemplateNumber, structuralMember.ModelInstanceGuid.Value, structuralMember.StructuralTypeOption, structuralMember.StructuralTypeNumber, structuralMember.StructuralInstanceGuid.Value); }
        }

        public static StructuralMemberId GetStructuralId_ForType(this IStructuralMember structuralMember)
        {
            return new StructuralMemberId(structuralMember.ProjectGuid, structuralMember.RevisionNumber, structuralMember.ModelTemplateNumber, structuralMember.StructuralTypeOption, structuralMember.StructuralTypeNumber);
        }

        #endregion

        #region Static Methods - Checks

        public static bool AreStructuralMemberIdsEqual(this IStructuralMember first, IStructuralMember second)
        {
            return AreStructuralMemberIdsEqual(first, new IStructuralMember[] { second });
        }

        public static bool AreStructuralMemberIdsEqual(this IStructuralMember first, IStructuralMember second, IStructuralMember third)
        {
            return AreStructuralMemberIdsEqual(first, new IStructuralMember[] { second, third });
        }

        public static bool AreStructuralMemberIdsEqual(this IStructuralMember main, IEnumerable<IStructuralMember> others)
        {
            foreach (var other in others)
            {
                var mainId = main.GetStructuralMemberId();
                var otherId = other.GetStructuralMemberId();

                if (!mainId.Equals(otherId))
                { return false; }
            }
            return true;
        }

        #endregion

        #region Static Methods - Asserts

        public static void AssertStructuralMemberIdsAreEqual(this IStructuralMember first, IStructuralMember second)
        {
            AssertStructuralMemberIdsAreEqual(first, new IStructuralMember[] { second });
        }

        public static void AssertStructuralMemberIdsAreEqual(this IStructuralMember first, IStructuralMember second, IStructuralMember third)
        {
            AssertStructuralMemberIdsAreEqual(first, new IStructuralMember[] { second, third });
        }

        public static void AssertStructuralMemberIdsAreEqual(this IStructuralMember main, IEnumerable<IStructuralMember> others)
        {
            if (!AreStructuralMemberIdsEqual(main, others))
            { throw new InvalidOperationException("The objects provided do not have the same StructuralMemberId values."); }
        }

        #endregion

        #region Static Methods - StructuralSpace

        public static StructuralSpace GetStructuralSpace_WithVariableTemplateRefs(this StructuralSpace originalSpace, IDictionary<StructuralDimension, ModelObjectReference> variableTemplateRefsByDimension)
        {
            var updatedDimensions = new List<StructuralDimension>();

            foreach (var originalDimension in originalSpace.Dimensions)
            {
                if (!variableTemplateRefsByDimension.ContainsKey(originalDimension))
                { throw new InvalidOperationException("At least one VariableTemplate reference is missing."); }

                var variableTemplateRef = variableTemplateRefsByDimension[originalDimension];

                if (variableTemplateRef.ModelObjectType != ModelObjectType.VariableTemplate)
                { throw new InvalidOperationException("The VariableTemplate reference must refer to a VariableTemplate."); }

                var updatedDimension = new StructuralDimension(originalDimension.EntityTypeNumber, originalDimension.EntityDimensionNumber, originalDimension.DimensionType, variableTemplateRef);
                updatedDimensions.Add(updatedDimension);
            }
            return new StructuralSpace(updatedDimensions);
        }

        #endregion
    }
}