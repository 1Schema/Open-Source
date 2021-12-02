using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Operations;
using Decia.Business.Domain.Formulas.Operations.Time.Search;

namespace Decia.Business.Domain.Formulas
{
    public class FormulaProcessingEngine : IFormulaProcessingEngine
    {
        private IFormulaDataProvider m_DataProvider;
        private IFormulaDataProvider_Anonymous m_DataProvider_Anonymous;
        private Nullable<Guid> m_AnonymousVariableGroupId;

        private FormulaProcessingState m_ProcessingState;
        private Dictionary<Guid, FormulaProcessingState> m_AnonymousProcessingStates;

        private Nullable<ModelObjectReference> m_ModelTemplateRef;
        private INetwork<Guid> m_ProcessingNetwork;
        private HashSet<Guid> m_GroupsWithFalseCycles;
        private HashSet<Guid> m_GroupsWithRealCycles;

        private ICurrentState m_ModelTemplateState;
        private Dictionary<ModelObjectReference, bool> m_VariableRequiresComputationByPeriod;

        private Dictionary<ModelObjectReference, ICurrentState> m_ModelInstanceStates;

        public FormulaProcessingEngine(IFormulaDataProvider dataProvider)
        {
            if (dataProvider == null)
            { throw new InvalidOperationException("The FormulaProcessingEngine's DataProvider must not be null."); }

            m_DataProvider = dataProvider;
            m_DataProvider_Anonymous = null;
            m_AnonymousVariableGroupId = null;

            m_ProcessingState = new FormulaProcessingState();
            m_AnonymousProcessingStates = new Dictionary<Guid, FormulaProcessingState>();

            m_ModelTemplateRef = null;
            m_ProcessingNetwork = null;
            m_GroupsWithFalseCycles = new HashSet<Guid>();
            m_GroupsWithRealCycles = new HashSet<Guid>();
            m_ModelTemplateState = null;
            m_VariableRequiresComputationByPeriod = new Dictionary<ModelObjectReference, bool>();
            m_ModelInstanceStates = null;
        }

        public ProjectMemberId ProjectId
        {
            get { return DataProvider.ProjectId; }
        }

        public IFormulaDataProvider DataProvider
        {
            get { return m_DataProvider; }
        }

        public bool HasAnonymousDataProvider
        {
            get { return (DataProvider_Anonymous != null); }
        }

        public IFormulaDataProvider_Anonymous DataProvider_Anonymous
        {
            get
            {
                if (m_DataProvider_Anonymous != null)
                { return m_DataProvider_Anonymous; }
                else if (m_DataProvider is IFormulaDataProvider_Anonymous)
                { return (m_DataProvider as IFormulaDataProvider_Anonymous); }
                else
                { return null; }
            }
            set
            { m_DataProvider_Anonymous = value; }
        }

        public bool IsInAnonymousMode
        {
            get
            {
                if (!HasAnonymousDataProvider)
                { return false; }
                return AnonymousVariableGroupId.HasValue;
            }
        }

        public Nullable<Guid> AnonymousVariableGroupId
        {
            get { return m_AnonymousVariableGroupId; }
            set
            {
                if (!value.HasValue)
                { m_AnonymousVariableGroupId = null; }
                else
                {
                    if (!HasAnonymousDataProvider)
                    { throw new InvalidOperationException("The current DataProvider does not support Anonymous Computation."); }
                    if (!DataProvider_Anonymous.AnonymousVariableGroupIds.Contains(value.Value))
                    { throw new InvalidOperationException("The specified Anonymous VariableGroup does not exist in the DataProvider."); }

                    m_AnonymousVariableGroupId = value.Value;
                }
            }
        }

        protected Guid AnonymousVariableGroupId_NonNull
        {
            get
            {
                if (!AnonymousVariableGroupId.HasValue)
                { throw new InvalidOperationException("The Anonymous VariableGroup is not set."); }

                return AnonymousVariableGroupId.Value;
            }
        }

        public IEnumerable<ModelObjectReference> CurrentVariableTemplates
        {
            get
            {
                if (!IsInAnonymousMode)
                { return DataProvider.DependencyMap.VariableTemplateRefs; }
                else
                { return DataProvider_Anonymous.GetAnonymousVariableTemplateRefs(AnonymousVariableGroupId_NonNull); }
            }
        }

        public FormulaProcessingState CurrentProcessingState
        {
            get
            {
                if (!IsInAnonymousMode)
                { return m_ProcessingState; }
                else
                {
                    if (!m_AnonymousProcessingStates.ContainsKey(AnonymousVariableGroupId_NonNull))
                    { m_AnonymousProcessingStates.Add(AnonymousVariableGroupId_NonNull, new FormulaProcessingState()); }

                    return m_AnonymousProcessingStates[AnonymousVariableGroupId_NonNull];
                }
            }
        }

        public void ResetAnonymousProcessingState()
        {
            if (!AnonymousVariableGroupId.HasValue)
            { return; }

            if (!m_AnonymousProcessingStates.ContainsKey(AnonymousVariableGroupId_NonNull))
            { return; }

            m_AnonymousProcessingStates.Remove(AnonymousVariableGroupId_NonNull);
        }

        public void SetInitializationInputs(ModelObjectReference modelTemplateRef)
        {
            if (InitializationState.Started)
            { throw new InvalidOperationException("Initialization has already been started."); }

            if (m_ModelTemplateRef.HasValue)
            { throw new InvalidOperationException("The ModelTemplateRef is already set."); }
            if (!DataProvider.IsValid(modelTemplateRef))
            { throw new InvalidOperationException("The specified ModelTemplateRef is not valid."); }

            m_ModelTemplateRef = modelTemplateRef;
        }

        public ModelObjectReference ModelTemplateRef
        {
            get { return m_ModelTemplateRef.Value; }
        }

        public void SetValidationInputs(ICurrentState modelTemplateState)
        {
            if (!IsInAnonymousMode)
            {
                if (!InitializationState.Finished)
                { throw new InvalidOperationException("Initialization has not finished."); }
                if (!InitializationState.Succeeded)
                { throw new InvalidOperationException("Initialization did not succeed."); }
            }
            if (ValidationState.Started)
            { throw new InvalidOperationException("Validation has already been started."); }

            if (m_ModelTemplateState != null)
            { throw new InvalidOperationException("The ModelTemplateState is already set."); }
            if (modelTemplateState == null)
            { throw new InvalidOperationException("The ModelTemplateState cannot be set to null."); }

            m_ModelTemplateState = modelTemplateState;
        }

        public ICurrentState ModelTemplateState
        {
            get { return m_ModelTemplateState; }
        }

        public void SetComputationInputs(IDictionary<ModelObjectReference, ICurrentState> modelInstanceStates)
        {
            if (!InitializationState.Finished)
            { throw new InvalidOperationException("Initialization has not finished."); }
            if (!InitializationState.Succeeded)
            { throw new InvalidOperationException("Initialization did not succeed."); }
            if (!ValidationState.Finished)
            { throw new InvalidOperationException("Validation has not finished."); }
            if (!ValidationState.Succeeded)
            { throw new InvalidOperationException("Validation did not succeed."); }
            if (ComputationState.Started)
            { throw new InvalidOperationException("Computation has already been started."); }

            if (m_ModelInstanceStates != null)
            { throw new InvalidOperationException("The ModelInstanceStates are already set."); }
            if (modelInstanceStates == null)
            { throw new InvalidOperationException("The ModelInstanceStates cannot be set to null."); }

            foreach (var modelInstanceRef in modelInstanceStates.Keys)
            {
                if (!DataProvider.IsValid(modelInstanceRef))
                { throw new InvalidOperationException("The specified ModelInstanceRef is not valid."); }
            }

            m_ModelInstanceStates = new Dictionary<ModelObjectReference, ICurrentState>(modelInstanceStates);
        }

        public ICollection<ModelObjectReference> ModelInstanceRefs
        {
            get { return m_ModelInstanceStates.Keys.ToList(); }
        }

        public IDictionary<ModelObjectReference, ICurrentState> ModelInstanceStates
        {
            get { return new Dictionary<ModelObjectReference, ICurrentState>(m_ModelInstanceStates); }
        }

        public IProcessingState InitializationState
        {
            get { return CurrentProcessingState.InitializationState; }
        }

        public ComputationResult Initialize()
        {
            if (!IsInAnonymousMode)
            { return Initialize_Normal(); }
            else
            { return Initialize_Anonymous(); }
        }

        protected ComputationResult Initialize_Normal()
        {
            ComputationResult result = new ComputationResult(ModelTemplateRef, null, ProcessingAcivityType.Initialization);

            try
            {
                if (IsInAnonymousMode)
                { throw new InvalidOperationException("The ProcessingEngine is currently in an incompatible Processing Mode."); }

                CurrentProcessingState.InitializationState_Editable.Start();

                foreach (var variableTemplateRef in CurrentVariableTemplates)
                {
                    var variableType = DataProvider.GetVariableType(variableTemplateRef);
                    var variableTemplateName = DataProvider.GetObjectName(variableTemplateRef);
                    var structuralTypeRef = DataProvider.GetStructuralType(variableTemplateRef);

                    CurrentState currentState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, ModelTemplateRef, structuralTypeRef, variableTemplateRef);

                    if (variableType == VariableType.Input)
                    {
                        DataProvider.SetIsInitialized(variableTemplateRef, true);
                    }
                    else if ((variableType == VariableType.BasicFormula) || (variableType.IsAggregationFormula()))
                    {
                        Formula variableTemplateFormula = DataProvider.GetFormula(variableTemplateRef);
                        var formulaResult = variableTemplateFormula.Initialize(DataProvider, currentState);

                        if (!formulaResult.IsValid)
                        {
                            result.SetErrorState(formulaResult);
                            CurrentProcessingState.ValidationState_Editable.Finish(false);
                            return result;
                        }
                    }
                }

                DataProvider.DependencyMap.AddImpliedNavigationDependencies(DataProvider.StructuralMap, CurrentState.DefaultUseExtendedStructure);
                m_ProcessingNetwork = DataProvider.DependencyMap.GetComputationNetwork();

                foreach (var group in ComputationGroupsWithCycles.Values)
                {
                    var cycleType = group.AnalyzeCycle(DataProvider);

                    if (cycleType == CycleType.None)
                    { /* do nothing */}
                    else if (cycleType == CycleType.PastCycle)
                    { m_GroupsWithFalseCycles.Add(group.Id); }
                    else
                    { m_GroupsWithRealCycles.Add(group.Id); }
                }

                if (m_GroupsWithRealCycles.Count > 0)
                {
                    string errorMessage = "Cycles are not currently supported.";

                    result.SetErrorState(ComputationResultType.ModelLevelErrorOccurred, "Initialization Failed: " + errorMessage);
                    CurrentProcessingState.InitializationState_Editable.Finish(false);
                    return result;
                }

                DataProvider.SetModelIsInitialized(ModelTemplateRef, true);
                result.SetModelInitializedState();
                CurrentProcessingState.InitializationState_Editable.Finish(true);
            }
            catch (Exception exception)
            {
                m_ProcessingNetwork = null;
                m_GroupsWithFalseCycles = new HashSet<Guid>();
                m_GroupsWithRealCycles = new HashSet<Guid>();

                result.SetErrorState(ComputationResultType.ModelLevelErrorOccurred, "Initialization Failed: " + exception.Message);
                CurrentProcessingState.InitializationState_Editable.Finish(false);
            }
            return result;
        }

        protected ComputationResult Initialize_Anonymous()
        {
            ComputationResult result = new ComputationResult(ModelTemplateRef, null, ProcessingAcivityType.Initialization);

            try
            {
                if (!IsInAnonymousMode)
                { throw new InvalidOperationException("The ProcessingEngine is currently in an incompatible Processing Mode."); }

                var isInitialized = DataProvider.GetModelIsInitialized(ModelTemplateRef);
                if (!isInitialized.HasValue)
                { throw new InvalidOperationException("The ModelTemplate has not yet been Initialized."); }
                if (!isInitialized.Value)
                { throw new InvalidOperationException("The ModelTemplate failed Initialization."); }

                if (!AnonymousVariableGroupId.HasValue)
                { throw new InvalidOperationException("The Anonymous VariableGroup is not set."); }

                CurrentProcessingState.InitializationState_Editable.Start();

                foreach (var variableTemplateRef in CurrentVariableTemplates)
                {
                    var rootStructuralTypeRef = DataProvider_Anonymous.GetAnonymousVariableRootStructuralTypeRef(variableTemplateRef);
                    var structuralSpace = DataProvider_Anonymous.GetAnonymousVariableStructuralSpace(variableTemplateRef);

                    CurrentState currentState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, ModelTemplateRef, rootStructuralTypeRef, structuralSpace, variableTemplateRef);

                    Formula variableTemplateFormula = DataProvider_Anonymous.GetAnonymousFormula(variableTemplateRef);
                    var formulaResult = variableTemplateFormula.Initialize(DataProvider_Anonymous, currentState);

                    if (!formulaResult.IsValid)
                    {
                        result.SetErrorState(formulaResult);
                        CurrentProcessingState.ValidationState_Editable.Finish(false);
                        return result;
                    }
                }

                DataProvider_Anonymous.SetAnonymousGroupIsInitialized(AnonymousVariableGroupId_NonNull, true);
                result.SetModelInitializedState();
                CurrentProcessingState.InitializationState_Editable.Finish(true);
            }
            catch (Exception exception)
            {
                result.SetErrorState(ComputationResultType.ModelLevelErrorOccurred, "Initialization Failed: " + exception.Message);
                CurrentProcessingState.InitializationState_Editable.Finish(false);
            }
            return result;
        }

        public INetwork<Guid> ProcessingNetwork
        {
            get { return m_ProcessingNetwork; }
        }

        public IDictionary<Guid, IComputationGroup> ComputationGroups
        {
            get
            {
                if (m_ProcessingNetwork == null)
                { return null; }

                var groups = new Dictionary<Guid, IComputationGroup>();

                foreach (Guid node in m_ProcessingNetwork.Nodes)
                {
                    var group = m_ProcessingNetwork.GetCachedValue<IComputationGroup>(node);
                    groups.Add(node, group);
                }
                return groups;
            }
        }

        public IDictionary<Guid, IComputationGroup> ComputationGroupsWithCycles
        {
            get { return ComputationGroups.Values.Where(grp => grp.HasCycle).ToDictionary(grp => grp.Id, grp => grp); }
        }

        public IDictionary<Guid, IComputationGroup> ComputationGroupsWithFalseCycles
        {
            get { return ComputationGroups.Values.Where(grp => m_GroupsWithFalseCycles.Contains(grp.Id)).ToDictionary(grp => grp.Id, grp => grp); }
        }

        public IDictionary<Guid, IComputationGroup> ComputationGroupsWithRealCycles
        {
            get { return ComputationGroups.Values.Where(grp => m_GroupsWithRealCycles.Contains(grp.Id)).ToDictionary(grp => grp.Id, grp => grp); }
        }

        public IDictionary<int, IComputationGroup> OrderedComputationGroups
        {
            get
            {
                if (!IsInAnonymousMode)
                {
                    if (m_ProcessingNetwork == null)
                    { return null; }

                    var orderedGroups = new SortedDictionary<int, IComputationGroup>();
                    var orderedNodes = m_ProcessingNetwork.GetDependencyOrderedTraversalFromRoot();

                    for (int index = 0; index < orderedNodes.Count; index++)
                    {
                        var node = orderedNodes.ElementAt(index);
                        var group = m_ProcessingNetwork.GetCachedValue<IComputationGroup>(node);
                        orderedGroups.Add(index, group);
                    }
                    return orderedGroups;
                }
                else
                {
                    var anonymousVariableGroup = new ComputationGroup(CurrentVariableTemplates);

                    var orderedGroups = new SortedDictionary<int, IComputationGroup>();
                    orderedGroups.Add(1, anonymousVariableGroup);

                    return orderedGroups;
                }
            }
        }

        public IComputationGroup GetComputationGroupForReference(ModelObjectReference variableTemplateRef)
        {
            foreach (var group in ComputationGroups.Values)
            {
                if (group.NodesIncluded.Contains(variableTemplateRef))
                { return group; }
            }
            return null;
        }

        public bool RequiresComputationByPeriod(ModelObjectReference variableTemplateRef)
        {
            if (m_VariableRequiresComputationByPeriod == null)
            { throw new InvalidOperationException("The Validate method has not been called yet or has not completed successfully."); }

            if (!m_VariableRequiresComputationByPeriod.ContainsKey(variableTemplateRef))
            { return false; }

            return m_VariableRequiresComputationByPeriod[variableTemplateRef];
        }

        public bool RequiresComputationByPeriod(IComputationGroup computationGroup)
        {
            foreach (var node in computationGroup.NodesIncluded)
            {
                if (RequiresComputationByPeriod(node))
                { return true; }
            }
            return false;
        }

        public IProcessingState ValidationState
        {
            get { return CurrentProcessingState.ValidationState; }
        }

        public IDictionary<ModelObjectReference, bool> VariableRequiresComputationByPeriod
        {
            get
            {
                return new Dictionary<ModelObjectReference, bool>(m_VariableRequiresComputationByPeriod);
            }
        }

        public ComputationResult Validate()
        {
            if (!IsInAnonymousMode)
            { return Validate_Normal(); }
            else
            { return Validate_Anonymous(); }
        }

        protected ComputationResult Validate_Normal()
        {
            ComputationResult result = new ComputationResult(ModelTemplateRef, null, ProcessingAcivityType.Validation);

            try
            {
                CurrentProcessingState.ValidationState_Editable.Start();

                var orderedComputationGroups = OrderedComputationGroups;
                var orderedVariableTemplateNames = GetOrderedGroupNames(DataProvider, orderedComputationGroups);

                foreach (int orderIndex in orderedComputationGroups.Keys)
                {
                    IComputationGroup group = orderedComputationGroups[orderIndex];
                    var orderedNodesInGroup = (group.HasStrictTimeOrdering) ? group.TimeOrderedNodes : group.NodesIncluded;

                    foreach (var variableTemplateRef in orderedNodesInGroup)
                    {
                        var variableType = DataProvider.GetVariableType(variableTemplateRef);
                        var variableTemplateName = DataProvider.GetObjectName(variableTemplateRef);
                        var structuralTypeRef = DataProvider.GetStructuralType(variableTemplateRef);

                        CurrentState currentState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, ModelTemplateRef, structuralTypeRef, variableTemplateRef);
                        currentState.ModelStartDate = ModelTemplateState.ModelStartDate;
                        currentState.ModelEndDate = ModelTemplateState.ModelEndDate;
                        currentState.ParentComputationGroup = group;

                        if (variableType == VariableType.Input)
                        {
                            var assessedDataType = DataProvider.GetAssessedDataType(variableTemplateRef);
                            var assessedTimeDimensionSet = DataProvider.GetAssessedTimeDimensions(variableTemplateRef);
                            var assessedUnit = DataProvider.GetAssessedUnit(variableTemplateRef);

                            var validatedUnit = (CompoundUnit)null;
                            if (assessedUnit != null)
                            {
                                if (assessedUnit.RevisionNumber_NonNull == DataProvider.RevisionNumber_NonNull)
                                { validatedUnit = assessedUnit.Copy(); }
                                else
                                { validatedUnit = assessedUnit.CopyForRevision(DataProvider.RevisionNumber_NonNull); }
                            }

                            DataProvider.SetIsValidated(variableTemplateRef, true);
                            DataProvider.SetValidatedDataType(variableTemplateRef, assessedDataType);
                            DataProvider.SetValidatedTimeDimensions(variableTemplateRef, assessedTimeDimensionSet);
                            DataProvider.SetValidatedUnit(variableTemplateRef, validatedUnit);
                        }
                        else if ((variableType == VariableType.BasicFormula) || (variableType.IsAggregationFormula()))
                        {
                            Formula variableTemplateFormula = DataProvider.GetFormula(variableTemplateRef);
                            bool requiresComputationByPeriod;

                            var formulaResult = variableTemplateFormula.Validate(DataProvider, currentState, out requiresComputationByPeriod);
                            m_VariableRequiresComputationByPeriod.Add(variableTemplateRef, requiresComputationByPeriod);

                            if (!formulaResult.IsValid)
                            {
                                result.SetErrorState(formulaResult);
                                CurrentProcessingState.ValidationState_Editable.Finish(false);
                                return result;
                            }
                        }
                    }
                }

                DataProvider.SetModelIsValidated(ModelTemplateRef, true);
                result.SetModelValidatedState();
                CurrentProcessingState.ValidationState_Editable.Finish(true);
            }
            catch (Exception exception)
            {
                m_VariableRequiresComputationByPeriod = new Dictionary<ModelObjectReference, bool>();

                result.SetErrorState(ComputationResultType.ModelLevelErrorOccurred, "Validation Failed: " + exception.Message);
                CurrentProcessingState.ValidationState_Editable.Finish(false);
            }
            return result;
        }

        protected ComputationResult Validate_Anonymous()
        {
            ComputationResult result = new ComputationResult(ModelTemplateRef, null, ProcessingAcivityType.Validation);

            try
            {
                var isValidated = DataProvider.GetModelIsValidated(ModelTemplateRef);

                if (!DataProvider.AllowSchemaErrors)
                {
                    if (!isValidated.HasValue)
                    { throw new InvalidOperationException("The ModelTemplate has not yet been Validated."); }
                    if (!isValidated.Value)
                    { throw new InvalidOperationException("The ModelTemplate failed Validation."); }
                }

                CurrentProcessingState.ValidationState_Editable.Start();

                var group = new ComputationGroup(CurrentVariableTemplates);
                var orderedNodesInGroup = (group.HasStrictTimeOrdering) ? group.TimeOrderedNodes : group.NodesIncluded;

                foreach (var variableTemplateRef in orderedNodesInGroup)
                {
                    var rootStructuralTypeRef = DataProvider_Anonymous.GetAnonymousVariableRootStructuralTypeRef(variableTemplateRef);
                    var structuralSpace = DataProvider_Anonymous.GetAnonymousVariableStructuralSpace(variableTemplateRef);

                    CurrentState currentState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, ModelTemplateRef, rootStructuralTypeRef, structuralSpace, variableTemplateRef);
                    currentState.ModelStartDate = ModelTemplateState.ModelStartDate;
                    currentState.ModelEndDate = ModelTemplateState.ModelEndDate;
                    currentState.ParentComputationGroup = group;

                    Formula variableTemplateFormula = DataProvider_Anonymous.GetAnonymousFormula(variableTemplateRef);
                    var formulaResult = variableTemplateFormula.Validate(DataProvider_Anonymous, currentState);

                    if (!formulaResult.IsValid)
                    {
                        result.SetErrorState(formulaResult);
                        CurrentProcessingState.ValidationState_Editable.Finish(false);
                        return result;
                    }
                }

                DataProvider_Anonymous.SetAnonymousGroupIsValidated(AnonymousVariableGroupId_NonNull, true);
                result.SetModelValidatedState();
                CurrentProcessingState.ValidationState_Editable.Finish(true);
            }
            catch (Exception exception)
            {
                result.SetErrorState(ComputationResultType.ModelLevelErrorOccurred, "Validation Failed: " + exception.Message);
                CurrentProcessingState.ValidationState_Editable.Finish(false);
            }
            return result;
        }

        public IProcessingState ComputationState
        {
            get { return CurrentProcessingState.ComputationState; }
        }

        public ComputationResult ComputeAnonymousValue(ModelObjectReference modelInstanceRef, ModelObjectReference variableTemplateRef, ModelObjectReference rootStructuralInstanceRef, StructuralPoint structuralPoint, MultiTimePeriodKey timeKey)
        {
            ComputationResult result = new ComputationResult(ModelTemplateRef, modelInstanceRef, ProcessingAcivityType.Computation);

            try
            {
                if (!IsInAnonymousMode)
                { throw new InvalidOperationException("The current method must be called in AnonymousMode."); }

                var modelInstanceRefHasState = (m_ModelInstanceStates.ContainsKey(modelInstanceRef) && (m_ModelInstanceStates[modelInstanceRef] != null));
                if (!modelInstanceRefHasState)
                { throw new InvalidOperationException("There is no Computation State for the current ModelInstance."); }

                var isComputed = DataProvider.GetModelIsComputed(modelInstanceRef);
                if (!isComputed.HasValue)
                { throw new InvalidOperationException("The ModelInstance has not yet been Computed."); }
                if (!isComputed.Value)
                { throw new InvalidOperationException("The ModelInstance failed Computation."); }

                if (!CurrentProcessingState.ComputationState_Editable.Started)
                { CurrentProcessingState.ComputationState_Editable.Start(); }

                var group = new ComputationGroup(CurrentVariableTemplates);

                var modelInstanceState = m_ModelInstanceStates[modelInstanceRef];
                var rootStructuralTypeRef = DataProvider_Anonymous.GetAnonymousVariableRootStructuralTypeRef(variableTemplateRef);
                var structuralSpace = DataProvider_Anonymous.GetAnonymousVariableStructuralSpace(variableTemplateRef);

                var fakeVariableInstanceRef = new ModelObjectReference(ModelObjectType.VariableInstance, Guid.NewGuid());

                CurrentState currentState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, ModelTemplateRef, rootStructuralTypeRef, structuralSpace, variableTemplateRef, modelInstanceRef, rootStructuralInstanceRef, structuralPoint, fakeVariableInstanceRef);
                currentState.ModelStartDate = modelInstanceState.ModelStartDate;
                currentState.ModelEndDate = modelInstanceState.ModelEndDate;
                currentState.ParentComputationGroup = group;

                if (timeKey != MultiTimePeriodKey.DimensionlessTimeKey)
                { currentState = new CurrentState(currentState, timeKey); }

                Formula variableTemplateFormula = DataProvider_Anonymous.GetAnonymousFormula(variableTemplateRef);
                var formulaResult = variableTemplateFormula.Compute(DataProvider_Anonymous, currentState);

                if (!formulaResult.IsValid)
                {
                    result.SetErrorState(formulaResult);
                    CurrentProcessingState.ComputationState_Editable.Finish(false);
                }
                else
                {
                    result.SetComputedState(formulaResult);
                }
            }
            catch (Exception exception)
            {
                result.SetErrorState(ComputationResultType.ModelLevelErrorOccurred, "Computation Failed: " + exception.Message);
                CurrentProcessingState.ComputationState_Editable.Finish(false);
            }
            return result;
        }

        public ComputationResult CompleteAnonymousComputation_ForInstance(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef)
        {
            ComputationResult result = new ComputationResult(ModelTemplateRef, modelInstanceRef, ProcessingAcivityType.Computation);

            if (!CurrentProcessingState.ComputationState_Editable.Started)
            { CurrentProcessingState.ComputationState_Editable.Start(); }

            DataProvider_Anonymous.SetAnonymousGroupIsComputed(AnonymousVariableGroupId_NonNull, modelInstanceRef, structuralInstanceRef, true);

            result.SetModelComputedState();
            return result;
        }

        public ComputationResult CompleteAnonymousComputation_ForTemplate()
        {
            ComputationResult result = new ComputationResult(ModelTemplateRef, null, ProcessingAcivityType.Computation);

            if (!CurrentProcessingState.ComputationState_Editable.Started)
            { CurrentProcessingState.ComputationState_Editable.Start(); }

            CurrentProcessingState.ComputationState_Editable.Finish(true);

            result.SetModelComputedState();
            return result;
        }

        public ComputationResult Compute()
        {
            ComputationResult result = new ComputationResult(ModelTemplateRef, null, ProcessingAcivityType.Computation);

            try
            {
                var modelInstanceRefsToCompute = m_ModelInstanceStates.Keys.ToHashSet();

                if (IsInAnonymousMode)
                { throw new InvalidOperationException("Anonymous Computation must be performed by StructuralPoint."); }

                CurrentProcessingState.ComputationState_Editable.Start();

                var orderedComputationGroups = OrderedComputationGroups;
                var orderedVariableTemplateNames = GetOrderedGroupNames(DataProvider, orderedComputationGroups);

                foreach (var modelInstanceRef in modelInstanceRefsToCompute)
                {
                    ICurrentState modelInstanceState = m_ModelInstanceStates[modelInstanceRef];

                    DataProvider.InitializeAllValidComputedTimeMatrices(modelInstanceRef);


                    foreach (int orderIndex in orderedComputationGroups.Keys)
                    {
                        IComputationGroup group = orderedComputationGroups[orderIndex];

                        var orderedNodesInGroup = (group.HasStrictTimeOrdering) ? group.TimeOrderedNodes : group.NodesIncluded;
                        var requiresPeriodBasedComputation = RequiresComputationByPeriod(group);
                        var timeAggregationNodes = group.NodesIncluded.Where(x => VariableTypeUtils.VariableTypes_WithFormulas.Contains(DataProvider.GetVariableType(x))).Where(x => DataProvider.GetFormula(x).IsTimeAggregation).ToList();
                        var nodesForTimeDimensionSet = new List<ModelObjectReference>(group.NodesIncluded);

                        foreach (var timeAggregationNode in timeAggregationNodes)
                        {
                            var precedentsHaveMorePrimaryTimeDimensionality = false;
                            var precedentsHaveMoreSecondaryTimeDimensionality = false;

                            var timeDimensionSet = DataProvider.GetValidatedTimeDimensions(timeAggregationNode);
                            var precedents = DataProvider.DependencyMap.VariableTypeNetwork.GetAncestors(timeAggregationNode);

                            foreach (var precedent in precedents)
                            {
                                var precedentTimeDimensionSet = DataProvider.GetValidatedTimeDimensions(precedent);

                                precedentsHaveMorePrimaryTimeDimensionality = (precedentTimeDimensionSet.PrimaryTimeDimension.HasTimeValue && !timeDimensionSet.PrimaryTimeDimension.HasTimeValue) ? true : precedentsHaveMorePrimaryTimeDimensionality;
                                precedentsHaveMoreSecondaryTimeDimensionality = (precedentTimeDimensionSet.SecondaryTimeDimension.HasTimeValue && !timeDimensionSet.SecondaryTimeDimension.HasTimeValue) ? true : precedentsHaveMoreSecondaryTimeDimensionality;
                            }

                            if (!precedentsHaveMorePrimaryTimeDimensionality && !precedentsHaveMoreSecondaryTimeDimensionality)
                            { nodesForTimeDimensionSet.AddRange(precedents); }
                        }

                        var includedTimeDimensionSets = nodesForTimeDimensionSet.ToHashSet().Select(n => DataProvider.GetComputationTimeDimensions(modelInstanceRef, n)).ToList();
                        var combinedTimeDimensionSet = includedTimeDimensionSets.GetDimensionsForGroup(modelInstanceState.ModelStartDate, modelInstanceState.ModelEndDate);

                        var timeKeysToLoopOn = new List<MultiTimePeriodKey>();
                        if (!requiresPeriodBasedComputation)
                        { timeKeysToLoopOn.Add(MultiTimePeriodKey.DimensionlessTimeKey); }
                        else
                        { timeKeysToLoopOn.AddRange(combinedTimeDimensionSet.GenerateTimeKeysForTimeDimensionSet()); }


                        foreach (var timeKey in timeKeysToLoopOn)
                        {
                            foreach (var variableTemplateRef in orderedNodesInGroup)
                            {
                                var variableTemplateName = DataProvider.GetObjectName(variableTemplateRef);
                                var structuralTypeRef = DataProvider.GetStructuralType(variableTemplateRef);

                                bool success = ComputeForTimeKey(modelInstanceState, structuralTypeRef, variableTemplateRef, group, timeKey, result);

                                if (!success)
                                { return result; }
                            }
                        }
                    }

                    DataProvider.SetModelIsComputed(modelInstanceRef, true);
                }

                result.SetModelComputedState();
                CurrentProcessingState.ComputationState_Editable.Finish(true);
            }
            catch (Exception exception)
            {
                result.SetErrorState(ComputationResultType.ModelLevelErrorOccurred, "Computation Failed: " + exception.Message);
                CurrentProcessingState.ComputationState_Editable.Finish(false);
            }
            return result;
        }

        private bool ComputeForTimeKey(ICurrentState modelInstanceState, ModelObjectReference structuralTypeRef, ModelObjectReference variableTemplateRef, IComputationGroup group, Nullable<MultiTimePeriodKey> timeKeyToCompute, ComputationResult result)
        {
            var variableInstanceRefsByStructuralInstanceRef = DataProvider.GetChildVariableInstanceReferences(modelInstanceState.ModelInstanceRef, variableTemplateRef);


            foreach (var structuralInstanceRef in variableInstanceRefsByStructuralInstanceRef.Keys)
            {
                var variableInstanceRef = variableInstanceRefsByStructuralInstanceRef[structuralInstanceRef];

                CurrentState currentState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, ModelTemplateRef, structuralTypeRef, variableTemplateRef, modelInstanceState.ModelInstanceRef, structuralInstanceRef, variableInstanceRef);
                currentState.ModelStartDate = modelInstanceState.ModelStartDate;
                currentState.ModelEndDate = modelInstanceState.ModelEndDate;
                currentState.ParentComputationGroup = group;
                currentState.DebugHelperText = DataProvider.GetObjectName(currentState.VariableTemplateRef);

                if (timeKeyToCompute.HasValue)
                {
                    if (timeKeyToCompute.Value.HasPrimaryTimePeriod || timeKeyToCompute.Value.HasSecondaryTimePeriod)
                    { currentState = new CurrentState(currentState, timeKeyToCompute.Value); }
                }


                var variableType = DataProvider.GetVariableType(variableTemplateRef);
                if (variableType == VariableType.Input)
                {
                    var assessedTimeMatrix = DataProvider.GetAssessedTimeMatrix(modelInstanceState.ModelInstanceRef, variableInstanceRef);

                    DataProvider.SetComputedTimeValues(modelInstanceState.ModelInstanceRef, variableInstanceRef, assessedTimeMatrix, currentState);
                }
                else if ((variableType == VariableType.BasicFormula) || (variableType.IsAggregationFormula()))
                {
                    Formula variableTemplateFormula = DataProvider.GetFormula(variableTemplateRef);

                    var formulaResult = variableTemplateFormula.Compute(DataProvider, currentState);

                    if (!formulaResult.IsValid)
                    {
                        result.SetErrorState(formulaResult);
                        CurrentProcessingState.ComputationState_Editable.Finish(false);
                        return false;
                    }
                }
            }
            return true;
        }

        #region Helper Methods


        protected static IDictionary<int, Dictionary<ModelObjectReference, string>> GetOrderedVariableTemplateNames(IFormulaDataProvider DataProvider, IDictionary<int, IComputationGroup> orderedComputationGroups)
        {
            var names = new SortedDictionary<int, Dictionary<ModelObjectReference, string>>();

            foreach (int orderIndex in orderedComputationGroups.Keys)
            {
                IComputationGroup group = orderedComputationGroups[orderIndex];

                names.Add(orderIndex, new Dictionary<ModelObjectReference, string>());

                foreach (var variableTemplateRef in group.NodesIncluded)
                {
                    var variableTemplateName = DataProvider.GetObjectName(variableTemplateRef);
                    names[orderIndex].Add(variableTemplateRef, variableTemplateName);
                }
            }
            return names;
        }

        protected static IDictionary<int, string> GetOrderedGroupNames(IFormulaDataProvider DataProvider, IDictionary<int, IComputationGroup> orderedComputationGroups)
        {
            var names = new SortedDictionary<int, string>();

            foreach (int orderIndex in orderedComputationGroups.Keys)
            {
                IComputationGroup group = orderedComputationGroups[orderIndex];
                string groupName = string.Empty;

                foreach (var variableTemplateRef in group.NodesIncluded)
                {
                    var variableTemplateName = DataProvider.GetObjectName(variableTemplateRef);

                    if (groupName == string.Empty)
                    { groupName += variableTemplateName; }
                    else
                    { groupName += ", " + variableTemplateName; }
                }

                names.Add(orderIndex, groupName);
            }
            return names;
        }

        #endregion
    }
}