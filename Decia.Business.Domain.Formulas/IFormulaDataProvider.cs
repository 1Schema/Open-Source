using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Dependencies;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;

namespace Decia.Business.Domain.Formulas
{
    public interface IFormulaDataProvider : IProjectMember
    {
        bool AllowSchemaErrors { get; }
        ProjectMemberId ProjectId { get; }
        ModelObjectReference ModelTemplateRef { get; }
        IStructuralMap StructuralMap { get; }
        IDependencyMap DependencyMap { get; }

        bool IsValid(ModelObjectReference reference);
        bool IsNavigationVariable(ModelObjectReference variableTemplateReference);
        ModelObjectReference GetParentVariableTemplateReference(ModelObjectReference modelInstanceReference, ModelObjectReference variableInstanceReference);
        IDictionary<ModelObjectReference, ModelObjectReference> GetChildVariableInstanceReferences(ModelObjectReference modelInstanceReference, ModelObjectReference variableTemplateReference);
        ModelObjectReference GetChildVariableInstanceReference(ModelObjectReference modelInstanceReference, ModelObjectReference variableTemplateReference, ModelObjectReference structuralInstanceRef);
        IDictionary<StructuralPoint, ModelObjectReference> GetRelatedChildVariableInstanceReferences(ModelObjectReference modelInstanceReference, Nullable<TimePeriod> navigationPeriod, ModelObjectReference relatedVariableTemplateReference, IStructuralContext structuralContext);
        IDictionary<StructuralPoint, ModelObjectReference> GetRelatedChildVariableInstanceReferences(ModelObjectReference modelInstanceReference, Nullable<TimePeriod> navigationPeriod, ModelObjectReference relatedVariableTemplateReference, IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> pointsNeededByKey);

        Nullable<bool> GetModelIsInitialized(ModelObjectReference modelTemplateRef);
        void SetModelIsInitialized(ModelObjectReference modelTemplateRef, bool result);
        Nullable<bool> GetModelIsValidated(ModelObjectReference modelTemplateRef);
        void SetModelIsValidated(ModelObjectReference modelTemplateRef, bool result);
        Nullable<bool> GetModelIsComputed(ModelObjectReference modelInstanceRef);
        void SetModelIsComputed(ModelObjectReference modelInstanceRef, bool result);

        string GetObjectName(ModelObjectReference reference);
        ModelObjectDescriptor GetObjectDescriptor(ModelObjectReference reference);
        ICollection<ModelObjectDescriptor> GetObjectDescriptorsForName(string name);
        ICollection<ModelObjectDescriptor> GetObjectDescriptorsForName(string name, IEnumerable<ModelObjectType> validModelObjectTypes);

        VariableType GetVariableType(ModelObjectReference reference);
        Nullable<bool> GetIsInitialized(ModelObjectReference reference);
        void SetIsInitialized(ModelObjectReference reference, bool result);
        Nullable<bool> GetIsValidated(ModelObjectReference reference);
        void SetIsValidated(ModelObjectReference reference, bool result);

        Formula GetFormula(FormulaId formulaId);
        Formula GetFormula(ModelObjectReference variableTemplateReference);

        ModelObjectReference GetStructuralType(ModelObjectReference variableTemplateReference);

        DeciaDataType GetAssessedDataType(ModelObjectReference reference);
        Nullable<DeciaDataType> GetValidatedDataType(ModelObjectReference variableTemplateReference);
        void SetValidatedDataType(ModelObjectReference variableTemplateReference, Nullable<DeciaDataType> dataType);

        ITimeDimensionSet GetAssessedTimeDimensions(ModelObjectReference reference);
        ITimeDimensionSet GetValidatedTimeDimensions(ModelObjectReference variableTemplateReference);
        void SetValidatedTimeDimensions(ModelObjectReference variableTemplateReference, ITimeDimensionSet timeDimensions);

        CompoundUnit GetAssessedUnit(ModelObjectReference reference);
        CompoundUnit GetValidatedUnit(ModelObjectReference variableTemplateReference);
        void SetValidatedUnit(ModelObjectReference variableTemplateReference, CompoundUnit unit);

        ITimeDimensionSet GetComputationTimeDimensions(ModelObjectReference modelInstanceReference, ModelObjectReference variableTemplateReference);
        ChronometricValue GetDefaultTimeMatrix(ModelObjectReference modelInstanceReference, ModelObjectReference variableInstanceReference);
        bool InitializeComputedTimeMatrix(ModelObjectReference modelInstanceReference, ModelObjectReference variableInstanceReference);
        void InitializeAllValidComputedTimeMatrices(ModelObjectReference modelInstanceReference);
        ChronometricValue GetAssessedTimeMatrix(ModelObjectReference modelInstanceReference, ModelObjectReference variableInstanceReference);
        ChronometricValue GetComputedTimeMatrix(ModelObjectReference modelInstanceReference, ModelObjectReference variableInstanceReference);
        void SetComputedTimeValues(ModelObjectReference modelInstanceReference, ModelObjectReference variableInstanceReference, ChronometricValue timeMatrixToCopyFrom, ICurrentState currentState);
    }
}