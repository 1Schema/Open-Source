using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Common.Modeling
{
    public static class ModelObjectTypeUtils
    {
        public static readonly IEnumerable<ModelObjectType> ModelRootLevelTypeValues = new ModelObjectType[] { ModelObjectType.ModelTemplate, ModelObjectType.EntityTypesList, ModelObjectType.RelationTypesList, ModelObjectType.ScenarioTypesList };
        public static readonly IEnumerable<ModelObjectType> StructuralTypeListValues = new ModelObjectType[] { ModelObjectType.EntityTypesList, ModelObjectType.RelationTypesList, ModelObjectType.ScenarioTypesList };

        public static readonly IEnumerable<ModelObjectType> ModelMemberTypeValues = new ModelObjectType[] { ModelObjectType.ModelTemplate, ModelObjectType.GlobalType, ModelObjectType.EntityType, ModelObjectType.RelationType, ModelObjectType.VariableTemplate };
        public static readonly IEnumerable<ModelObjectType> ModelMemberInstanceValues = new ModelObjectType[] { ModelObjectType.ModelInstance, ModelObjectType.GlobalInstance, ModelObjectType.EntityInstance, ModelObjectType.RelationInstance, ModelObjectType.VariableInstance };

        public static readonly IEnumerable<ModelObjectType> TimeTypeValues = new ModelObjectType[] { ModelObjectType.TimeType };
        public static readonly IEnumerable<ModelObjectType> TimeInstanceValues = new ModelObjectType[] { ModelObjectType.TimeInstance };

        public static readonly IEnumerable<ModelObjectType> StructuralTypeValues = new ModelObjectType[] { ModelObjectType.GlobalType, ModelObjectType.EntityType, ModelObjectType.RelationType };
        public static readonly IEnumerable<ModelObjectType> StructuralInstanceValues = new ModelObjectType[] { ModelObjectType.GlobalInstance, ModelObjectType.EntityInstance, ModelObjectType.RelationInstance };

        public static readonly IEnumerable<ModelObjectType> VariableTypeValues = new ModelObjectType[] { ModelObjectType.VariableTemplate, ModelObjectType.AnonymousVariableTemplate };
        public static readonly IEnumerable<ModelObjectType> VariableInstanceValues = new ModelObjectType[] { ModelObjectType.VariableInstance, ModelObjectType.AnonymousVariableInstance };

        public static readonly IEnumerable<ModelObjectType> DimensionableTypeValues = new ModelObjectType[] { ModelObjectType.TimeType, ModelObjectType.EntityType, ModelObjectType.VariableTemplate, ModelObjectType.AnonymousVariableTemplate };
        public static readonly IEnumerable<ModelObjectType> DimensionableInstanceValues = new ModelObjectType[] { ModelObjectType.TimeInstance, ModelObjectType.EntityInstance, ModelObjectType.VariableInstance, ModelObjectType.AnonymousVariableInstance };

        public static readonly IEnumerable<ModelObjectType> AnonymousValues = new ModelObjectType[] { ModelObjectType.AnonymousVariableTemplate, ModelObjectType.AnonymousVariableInstance };

        #region Validity Checks and Assertions

        public static bool IsModelRootLevelType(this ModelObjectType modelObjectType)
        {
            return ModelRootLevelTypeValues.Contains(modelObjectType);
        }

        public static bool IsModelRootLevelType(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsModelRootLevelType();
        }

        public static bool IsStructuralListType(this ModelObjectType modelObjectType)
        {
            return StructuralTypeListValues.Contains(modelObjectType);
        }

        public static bool IsStructuralListType(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsStructuralListType();
        }

        public static bool IsModelMemberType(this ModelObjectType modelObjectType)
        {
            return ModelMemberTypeValues.Contains(modelObjectType);
        }

        public static bool IsModelMemberType(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsModelMemberType();
        }

        public static bool IsModelMemberInstance(this ModelObjectType modelObjectType)
        {
            return ModelMemberInstanceValues.Contains(modelObjectType);
        }

        public static bool IsModelMemberInstance(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsModelMemberInstance();
        }

        public static bool IsTimeType(this ModelObjectType modelObjectType)
        {
            return TimeTypeValues.Contains(modelObjectType);
        }

        public static bool IsTimeType(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsTimeType();
        }

        public static bool IsTimeInstance(this ModelObjectType modelObjectType)
        {
            return TimeInstanceValues.Contains(modelObjectType);
        }

        public static bool IsTimeInstance(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsTimeInstance();
        }

        public static bool IsStructuralType(this ModelObjectType modelObjectType)
        {
            return StructuralTypeValues.Contains(modelObjectType);
        }

        public static bool IsStructuralType(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsStructuralType();
        }

        public static bool IsStructuralInstance(this ModelObjectType modelObjectType)
        {
            return StructuralInstanceValues.Contains(modelObjectType);
        }

        public static bool IsStructuralInstance(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsStructuralInstance();
        }

        public static bool IsVariableType(this ModelObjectType modelObjectType)
        {
            return VariableTypeValues.Contains(modelObjectType);
        }

        public static bool IsVariableType(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsVariableType();
        }

        public static bool IsVariableInstance(this ModelObjectType modelObjectType)
        {
            return VariableInstanceValues.Contains(modelObjectType);
        }

        public static bool IsVariableInstance(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsVariableInstance();
        }

        public static bool IsDimensionableType(this ModelObjectType modelObjectType)
        {
            return DimensionableTypeValues.Contains(modelObjectType);
        }

        public static bool IsDimensionableType(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsDimensionableType();
        }

        public static bool IsDimensionableInstance(this ModelObjectType modelObjectType)
        {
            return DimensionableInstanceValues.Contains(modelObjectType);
        }

        public static bool IsDimensionableInstance(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsDimensionableInstance();
        }

        public static bool IsAnonymous(this ModelObjectType modelObjectType)
        {
            return AnonymousValues.Contains(modelObjectType);
        }

        public static bool IsAnonymous(this ModelObjectReference modelObjectRef)
        {
            return modelObjectRef.ModelObjectType.IsAnonymous();
        }

        #endregion

        #region Conversion Methods

        public static IEnumerable<ModelObjectReference> ToEnumerable(this ModelObjectReference modelObjectRef)
        {
            return new ModelObjectReference[] { modelObjectRef };
        }

        public static ModelObjectType GetInstanceForType(this ModelObjectType structuralType)
        {
            if (structuralType == ModelObjectType.ModelTemplate)
            { return ModelObjectType.ModelInstance; }
            else if (structuralType == ModelObjectType.TimeType)
            { return ModelObjectType.TimeInstance; }
            else if (structuralType == ModelObjectType.GlobalType)
            { return ModelObjectType.GlobalInstance; }
            else if (structuralType == ModelObjectType.EntityType)
            { return ModelObjectType.EntityInstance; }
            else if (structuralType == ModelObjectType.RelationType)
            { return ModelObjectType.RelationInstance; }
            else if (structuralType == ModelObjectType.VariableTemplate)
            { return ModelObjectType.VariableInstance; }
            else
            { throw new InvalidOperationException("Unrecognized ModelObjectType encountered."); }
        }

        public static ModelObjectType GetTypeForInstance(this ModelObjectType structuralType)
        {
            if (structuralType == ModelObjectType.ModelInstance)
            { return ModelObjectType.ModelTemplate; }
            else if (structuralType == ModelObjectType.TimeInstance)
            { return ModelObjectType.TimeType; }
            else if (structuralType == ModelObjectType.GlobalInstance)
            { return ModelObjectType.GlobalType; }
            else if (structuralType == ModelObjectType.EntityInstance)
            { return ModelObjectType.EntityType; }
            else if (structuralType == ModelObjectType.RelationInstance)
            { return ModelObjectType.RelationType; }
            else if (structuralType == ModelObjectType.VariableInstance)
            { return ModelObjectType.VariableTemplate; }
            else
            { throw new InvalidOperationException("Unrecognized ModelObjectType encountered."); }
        }

        public static ModelObjectType GetModelObjectType(this StructuralTypeOption structuralType)
        {
            if (structuralType == StructuralTypeOption.GlobalType)
            { return ModelObjectType.GlobalType; }
            else if (structuralType == StructuralTypeOption.EntityType)
            { return ModelObjectType.EntityType; }
            else if (structuralType == StructuralTypeOption.RelationType)
            { return ModelObjectType.RelationType; }
            else
            { throw new InvalidOperationException("Unrecognized StructuralTypeOption encountered."); }
        }

        public static ModelObjectType GetModelObjectType(this StructuralInstanceOption structuralInstance)
        {
            if (structuralInstance == StructuralInstanceOption.GlobalInstance)
            { return ModelObjectType.GlobalInstance; }
            else if (structuralInstance == StructuralInstanceOption.EntityInstance)
            { return ModelObjectType.EntityInstance; }
            else if (structuralInstance == StructuralInstanceOption.RelationInstance)
            { return ModelObjectType.RelationInstance; }
            else
            { throw new InvalidOperationException("Unrecognized StructuralInstanceOption encountered."); }
        }

        public static StructuralTypeOption GetStructuralType(this ModelObjectType objectType)
        {
            if (objectType == ModelObjectType.GlobalType)
            { return StructuralTypeOption.GlobalType; }
            else if (objectType == ModelObjectType.EntityType)
            { return StructuralTypeOption.EntityType; }
            else if (objectType == ModelObjectType.RelationType)
            { return StructuralTypeOption.RelationType; }
            else
            { throw new InvalidOperationException("Unrecognized ModelObjectType encountered."); }
        }

        public static StructuralTypeOption GetStructuralType(this StructuralInstanceOption instanceOption)
        {
            if (instanceOption == StructuralInstanceOption.GlobalInstance)
            { return StructuralTypeOption.GlobalType; }
            else if (instanceOption == StructuralInstanceOption.EntityInstance)
            { return StructuralTypeOption.EntityType; }
            else if (instanceOption == StructuralInstanceOption.RelationInstance)
            { return StructuralTypeOption.RelationType; }
            else
            { throw new InvalidOperationException("Unrecognized StructuralInstanceOption encountered."); }
        }

        public static StructuralInstanceOption GetStructuralInstance(this ModelObjectType objectType)
        {
            if (objectType == ModelObjectType.GlobalInstance)
            { return StructuralInstanceOption.GlobalInstance; }
            else if (objectType == ModelObjectType.EntityInstance)
            { return StructuralInstanceOption.EntityInstance; }
            else if (objectType == ModelObjectType.RelationInstance)
            { return StructuralInstanceOption.RelationInstance; }
            else
            { throw new InvalidOperationException("Unrecognized ModelObjectType encountered."); }
        }

        public static StructuralInstanceOption GetStructuralInstance(this StructuralTypeOption typeOption)
        {
            if (typeOption == StructuralTypeOption.GlobalType)
            { return StructuralInstanceOption.GlobalInstance; }
            else if (typeOption == StructuralTypeOption.EntityType)
            { return StructuralInstanceOption.EntityInstance; }
            else if (typeOption == StructuralTypeOption.RelationType)
            { return StructuralInstanceOption.RelationInstance; }
            else
            { throw new InvalidOperationException("Unrecognized StructuralTypeOption encountered."); }
        }

        public static SortedDictionary<string, string> GetAvailableTimeDimensions()
        {
            SortedDictionary<string, string> timeDimensions = new SortedDictionary<string, string>();
            timeDimensions.Add(ConversionUtils.ConvertComplexIdToString(ModelObjectType.TimeType, 0), "No Time Dimension");
            timeDimensions.Add(ConversionUtils.ConvertComplexIdToString(ModelObjectType.TimeType, 1), "First Dimension");
            timeDimensions.Add(ConversionUtils.ConvertComplexIdToString(ModelObjectType.TimeType, 2), "Second Dimension");
            return timeDimensions;
        }

        #endregion

        #region Analysis Methods

        public static Dictionary<ModelObjectReference, int> GetStructuralTypeCounts(this IEnumerable<ModelObjectReference> structuralTypeRefs)
        {
            var structuralTypeCounts = new Dictionary<ModelObjectReference, int>();
            foreach (var structuralTypeRef in structuralTypeRefs)
            {
                if (!structuralTypeCounts.ContainsKey(structuralTypeRef))
                { structuralTypeCounts[structuralTypeRef] = ModelObjectReference.MinimumAlternateDimensionNumber; }
                else
                { structuralTypeCounts[structuralTypeRef]++; }
            }
            return structuralTypeCounts;
        }

        #endregion

        #region Persistence Validation Methods

        public static void AssertActionIsAllowed_ForRevisionType(this ModelObjectType? modelObjectType, AutoGeneratedRevisionType? revisionType, RepositoryActionType actionType)
        {
            if (actionType == RepositoryActionType.Read)
            { return; }
            else if (actionType == RepositoryActionType.Add)
            { AssertCreateIsAllowed_ForRevisionType(modelObjectType, revisionType); }
            else if (actionType == RepositoryActionType.Update)
            { AssertUpdateIsAllowed_ForRevisionType(modelObjectType, revisionType); }
            else if (actionType == RepositoryActionType.Remove)
            { AssertDeleteIsAllowed_ForRevisionType(modelObjectType, revisionType); }
            else
            { throw new InvalidOperationException("Unsupported RepositoryActionType encountered."); }
        }

        public static void AssertCreateIsAllowed_ForRevisionType(this ModelObjectType? modelObjectType, AutoGeneratedRevisionType? revisionType)
        {
            if (!IsCreateAllowed_ForRevisionType(modelObjectType, revisionType))
            { throw new InvalidOperationException("\"Create\" is not supported for the current ObjectType and RevisionType."); }
        }

        public static void AssertUpdateIsAllowed_ForRevisionType(this ModelObjectType? modelObjectType, AutoGeneratedRevisionType? revisionType)
        {
            if (!IsUpdateAllowed_ForRevisionType(modelObjectType, revisionType))
            { throw new InvalidOperationException("\"Update\" is not supported for the current ObjectType and RevisionType."); }
        }

        public static void AssertDeleteIsAllowed_ForRevisionType(this ModelObjectType? modelObjectType, AutoGeneratedRevisionType? revisionType)
        {
            if (!IsDeleteAllowed_ForRevisionType(modelObjectType, revisionType))
            { throw new InvalidOperationException("\"Delete\" is not supported for the current ObjectType and RevisionType."); }
        }

        public static bool IsActionAllowed_ForRevisionType(this ModelObjectType? modelObjectType, AutoGeneratedRevisionType? revisionType, RepositoryActionType actionType)
        {
            if (actionType == RepositoryActionType.Read)
            { return true; }
            else if (actionType == RepositoryActionType.Add)
            { return IsCreateAllowed_ForRevisionType(modelObjectType, revisionType); }
            else if (actionType == RepositoryActionType.Update)
            { return IsUpdateAllowed_ForRevisionType(modelObjectType, revisionType); }
            else if (actionType == RepositoryActionType.Remove)
            { return IsDeleteAllowed_ForRevisionType(modelObjectType, revisionType); }
            else
            { throw new InvalidOperationException("Unsupported RepositoryActionType encountered."); }
        }

        public static bool IsCreateAllowed_ForRevisionType(this ModelObjectType? modelObjectType, AutoGeneratedRevisionType? revisionType)
        {
            if (revisionType == AutoGeneratedRevisionType.ComputationRevision)
            {
                if (!modelObjectType.HasValue)
                { return true; }
                if (modelObjectType == ModelObjectType.Unit)
                { return true; }
                if (modelObjectType == ModelObjectType.TimeMatrix)
                { return true; }
                if (modelObjectType == ModelObjectType.Formula)
                { return true; }
                if (modelObjectType == ModelObjectType.BaseUnitType)
                { return true; }
                if (ModelObjectTypeUtils.ModelMemberTypeValues.Contains(modelObjectType.Value))
                { return true; }
                if (ModelObjectTypeUtils.ModelMemberInstanceValues.Contains(modelObjectType.Value))
                { return true; }
                return false;
            }
            return true;
        }

        public static bool IsUpdateAllowed_ForRevisionType(this ModelObjectType? modelObjectType, AutoGeneratedRevisionType? revisionType)
        {
            if (revisionType == AutoGeneratedRevisionType.ComputationRevision)
            {
                if (!modelObjectType.HasValue)
                { return true; }

                return true;
            }
            return true;
        }

        public static bool IsDeleteAllowed_ForRevisionType(this ModelObjectType? modelObjectType, AutoGeneratedRevisionType? revisionType)
        {
            return false;
        }

        #endregion

        #region Export Methods

        public static bool IsExportable(this ModelObjectType modelObjectType)
        {
            if (modelObjectType == ModelObjectType.ModelTemplate)
            { return true; }
            if (modelObjectType == ModelObjectType.BaseUnitType)
            { return true; }
            if (modelObjectType == ModelObjectType.TimeType)
            { return true; }
            if (modelObjectType == ModelObjectType.GlobalType)
            { return true; }
            if (modelObjectType == ModelObjectType.EntityType)
            { return true; }
            if (modelObjectType == ModelObjectType.RelationType)
            { return true; }
            if (modelObjectType == ModelObjectType.VariableTemplate)
            { return true; }
            return false;
        }

        #endregion
    }
}