using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Formulas
{
    public interface IFormulaProcessingEngine
    {
        ProjectMemberId ProjectId { get; }
        IFormulaDataProvider DataProvider { get; }
        bool HasAnonymousDataProvider { get; }
        IFormulaDataProvider_Anonymous DataProvider_Anonymous { get; set; }

        bool IsInAnonymousMode { get; }
        Nullable<Guid> AnonymousVariableGroupId { get; set; }
        IEnumerable<ModelObjectReference> CurrentVariableTemplates { get; }
        FormulaProcessingState CurrentProcessingState { get; }
        void ResetAnonymousProcessingState();

        void SetInitializationInputs(ModelObjectReference modelTemplateRef);
        ModelObjectReference ModelTemplateRef { get; }

        void SetValidationInputs(ICurrentState modelTemplateState);
        ICurrentState ModelTemplateState { get; }

        void SetComputationInputs(IDictionary<ModelObjectReference, ICurrentState> modelInstanceStates);
        ICollection<ModelObjectReference> ModelInstanceRefs { get; }
        IDictionary<ModelObjectReference, ICurrentState> ModelInstanceStates { get; }

        IProcessingState InitializationState { get; }
        ComputationResult Initialize();
        INetwork<Guid> ProcessingNetwork { get; }
        IDictionary<Guid, IComputationGroup> ComputationGroups { get; }
        IDictionary<Guid, IComputationGroup> ComputationGroupsWithCycles { get; }
        IDictionary<Guid, IComputationGroup> ComputationGroupsWithFalseCycles { get; }
        IDictionary<Guid, IComputationGroup> ComputationGroupsWithRealCycles { get; }
        IDictionary<int, IComputationGroup> OrderedComputationGroups { get; }
        IComputationGroup GetComputationGroupForReference(ModelObjectReference variableTemplateRef);

        bool RequiresComputationByPeriod(ModelObjectReference variableTemplateRef);
        bool RequiresComputationByPeriod(IComputationGroup computationGroup);

        IProcessingState ValidationState { get; }
        ComputationResult Validate();

        IProcessingState ComputationState { get; }
        ComputationResult Compute();
        ComputationResult ComputeAnonymousValue(ModelObjectReference modelInstanceRef, ModelObjectReference variableTemplateRef, ModelObjectReference rootStructuralInstanceRef, StructuralPoint structuralPoint, MultiTimePeriodKey timeKey);
        ComputationResult CompleteAnonymousComputation_ForInstance(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef);
        ComputationResult CompleteAnonymousComputation_ForTemplate();
    }
}