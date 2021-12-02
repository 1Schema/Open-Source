using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;

namespace Decia.Business.Domain.Formulas
{
    public interface IFormulaDataProvider_Anonymous : IFormulaDataProvider
    {
        ICollection<ModelObjectReference> ComputedModelInstanceRefs { get; }
        ICollection<Guid> AnonymousVariableGroupIds { get; }

        ICollection<ModelObjectReference> GetAnonymousVariableTemplateRefs(Guid anonymousVariableGroupId);
        bool IsAnonymousVariable(ModelObjectReference variableTemplateRef);

        Nullable<bool> GetAnonymousGroupIsInitialized(Guid anonymousVariableGroupId);
        void SetAnonymousGroupIsInitialized(Guid anonymousVariableGroupId, bool result);
        Nullable<bool> GetAnonymousGroupIsValidated(Guid anonymousVariableGroupId);
        void SetAnonymousGroupIsValidated(Guid anonymousVariableGroupId, bool result);
        Nullable<bool> GetAnonymousGroupIsComputed(Guid anonymousVariableGroupId, ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef);
        void SetAnonymousGroupIsComputed(Guid anonymousVariableGroupId, ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef, bool result);

        Nullable<bool> GetAnonymousVariableIsInitialized(ModelObjectReference variableTemplateRef);
        void SetAnonymousVariableIsInitialized(ModelObjectReference variableTemplateRef, bool result);
        Nullable<bool> GetAnonymousVariableIsValidated(ModelObjectReference variableTemplateRef);
        void SetAnonymousVariableIsValidated(ModelObjectReference variableTemplateRef, bool result);

        Formula GetAnonymousFormula(FormulaId formulaId);
        Formula GetAnonymousFormula(ModelObjectReference variableTemplateRef);

        void InitializeAnonymousVariableSettings(ModelObjectReference variableTemplateRef, StructuralSpace structuralSpace, ITimeDimensionSet timeDimensions, DeciaDataType dataType, DynamicValue defaultValue);
        ModelObjectReference GetAnonymousVariableRootStructuralTypeRef(ModelObjectReference variableTemplateRef);
        StructuralSpace GetAnonymousVariableStructuralSpace(ModelObjectReference variableTemplateRef);
        ITimeDimensionSet GetAnonymousVariableTimeDimensions(ModelObjectReference variableTemplateRef);
        DeciaDataType GetAnonymousVariableDataType(ModelObjectReference variableTemplateRef);
        DynamicValue GetAnonymousVariableDefaultValue(ModelObjectReference variableTemplateRef);

        ChronometricValue GetDefaultAnonymousVariableTimeMatrix(ModelObjectReference modelInstanceRef, ModelObjectReference variableTemplateRef);

        void InitializeAnonymousVariableValue(ModelObjectReference modelInstanceRef, ModelObjectReference variableTemplateRef, StructuralPoint structuralPoint, MultiTimePeriodKey timeKey);
        DynamicValue GetComputedAnonymousVariableValue(ModelObjectReference modelInstanceRef, ModelObjectReference variableTemplateRef, StructuralPoint structuralPoint, MultiTimePeriodKey timeKey);
        void SetComputedAnonymousVariableValue(ModelObjectReference modelInstanceRef, ModelObjectReference variableTemplateRef, StructuralPoint structuralPoint, MultiTimePeriodKey timeKey, DynamicValue value);
    }
}