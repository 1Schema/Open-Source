using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Rendering;
using RenderingUtils = Decia.Business.Domain.Reporting.ReportRenderingEngineUtils;

namespace Decia.Business.Domain.Reporting
{
    public class ReportRenderingEngine : IReportRenderingEngine
    {
        public static readonly IEqualityComparer<ReportElementId> EqualityComparer_ReportElementId = new ReportElementId_ReportScopedComparer();
        public static readonly IEqualityComparer<RenderingKey> EqualityComparer_RenderingKey = new RenderingKey_ReportScopedComparer();

        private IFormulaProcessingEngine m_FormulaProcessingEngine;
        private IReportingDataProvider m_DataProvider;

        private ProcessingState m_InitializationState;
        private ProcessingState m_ValidationState;
        private ProcessingState m_RenderForDesignState;
        private ProcessingState m_RenderForProductionState;

        private Nullable<ModelObjectReference> m_ModelTemplateRef;
        private IReport m_Report;
        private Dictionary<ReportElementId, IReportElement> m_ReportElements;
        private List<ReportElementId> m_MissingReportElementIds;
        private List<ReportElementId> m_InaccessibleReportElementIds;

        private ICurrentState m_ModelTemplateState;
        private CachedTree<ReportElementId> m_ElementTree;
        private CachedTree<ReportElementId> m_StyleTree;
        private RenderingState m_RenderingState;
        private Nullable<bool> m_IsReportValid;
        private List<ReportElementId> m_InvalidReportElementIds;

        private RenderedReport m_RenderedReport_Design;

        private Dictionary<ModelObjectReference, ICurrentState> m_ModelInstanceStates;
        private Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, RenderedReport>> m_RenderedReports_Production;

        public ReportRenderingEngine(IFormulaProcessingEngine formulaProcessingEngine)
            : this(formulaProcessingEngine, (formulaProcessingEngine.DataProvider as IReportingDataProvider))
        { }

        public ReportRenderingEngine(IFormulaProcessingEngine formulaProcessingEngine, IReportingDataProvider alternateDataProvider)
        {
            if (alternateDataProvider == null)
            { throw new InvalidOperationException("The FormulaProcessingEngine's DataProvider must not be null."); }

            m_FormulaProcessingEngine = formulaProcessingEngine;
            m_DataProvider = alternateDataProvider;

            m_FormulaProcessingEngine.DataProvider_Anonymous = m_DataProvider;

            m_InitializationState = new ProcessingState();
            m_ValidationState = new ProcessingState(new IProcessingState[] { m_InitializationState });
            m_RenderForDesignState = new ProcessingState(new IProcessingState[] { m_InitializationState, m_ValidationState });
            m_RenderForProductionState = new ProcessingState(new IProcessingState[] { m_InitializationState, m_ValidationState });

            m_ModelTemplateRef = null;
            m_Report = null;
            m_ReportElements = null;
            m_MissingReportElementIds = null;
            m_InaccessibleReportElementIds = null;

            m_ModelTemplateState = null;
            m_ElementTree = null;
            m_StyleTree = null;
            m_RenderingState = null;
            m_IsReportValid = null;
            m_InvalidReportElementIds = null;

            m_RenderedReport_Design = null;

            m_ModelInstanceStates = null;
            m_RenderedReports_Production = null;
        }

        public IFormulaProcessingEngine FormulaProcessingEngine
        {
            get { return m_FormulaProcessingEngine; }
        }

        public IReportingDataProvider DataProvider
        {
            get { return m_DataProvider; }
        }

        public ProjectMemberId ProjectId
        {
            get { return DataProvider.ProjectId; }
        }

        public void SetInitializationInputs(ModelObjectReference modelTemplateRef, IReport report, IEnumerable<IReportElement> reportElements)
        {
            if (m_InitializationState.Started)
            { throw new InvalidOperationException("Initialization has already been started."); }

            if (m_ModelTemplateRef.HasValue)
            { throw new InvalidOperationException("The ModelTemplateRef is already set."); }
            if (!DataProvider.IsValid(modelTemplateRef))
            { throw new InvalidOperationException("The specified ModelTemplateRef is not valid."); }

            m_ModelTemplateRef = modelTemplateRef;
            m_Report = report.Copy();
            m_ReportElements = reportElements.ToDictionary(x => x.Key, x => x.Copy(), ReportRenderingEngine.EqualityComparer_ReportElementId);

            FormulaProcessingEngine.AnonymousVariableGroupId = report.Key.ReportGuid;
        }

        public ModelObjectReference ModelTemplateRef
        {
            get { return m_ModelTemplateRef.Value; }
        }

        public IReport Report
        {
            get { return m_Report.Copy(); }
        }

        public IDictionary<ReportElementId, IReportElement> ReportElements
        {
            get { return m_ReportElements.ToDictionary(x => x.Key, x => x.Value.Copy(), ReportRenderingEngine.EqualityComparer_ReportElementId); }
        }

        public void SetValidationInputs(ICurrentState modelTemplateState)
        {
            if (!m_InitializationState.Finished)
            { throw new InvalidOperationException("Initialization has not finished."); }
            if (!m_InitializationState.Succeeded)
            { throw new InvalidOperationException("Initialization did not succeed."); }
            if (m_ValidationState.Started)
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

        public void SetDesignInputs()
        {
            if (!m_InitializationState.Finished)
            { throw new InvalidOperationException("Initialization has not finished."); }
            if (!m_InitializationState.Succeeded)
            { throw new InvalidOperationException("Initialization did not succeed."); }
            if (!m_ValidationState.Finished)
            { throw new InvalidOperationException("Validation has not finished."); }
            if (!m_ValidationState.Succeeded)
            { throw new InvalidOperationException("Validation did not succeed."); }
            if (m_RenderForDesignState.Started)
            { throw new InvalidOperationException("Design Rendering has already been started."); }
        }

        public void SetProductionInputs(IDictionary<ModelObjectReference, ICurrentState> modelInstanceStates)
        {
            if (!m_InitializationState.Finished)
            { throw new InvalidOperationException("Initialization has not finished."); }
            if (!m_InitializationState.Succeeded)
            { throw new InvalidOperationException("Initialization did not succeed."); }
            if (!m_ValidationState.Finished)
            { throw new InvalidOperationException("Validation has not finished."); }
            if (!m_ValidationState.Succeeded)
            { throw new InvalidOperationException("Validation did not succeed."); }
            if (m_RenderForProductionState.Started)
            { throw new InvalidOperationException("Production Rendering has already been started."); }

            if (m_ModelInstanceStates != null)
            { throw new InvalidOperationException("The ModelInstanceStates are already set."); }
            if (modelInstanceStates == null)
            { throw new InvalidOperationException("The ModelInstanceStates cannot be set to null."); }
            if (modelInstanceStates.Values.Contains(null))
            { throw new InvalidOperationException("The ModelInstanceStates cannot be set to null."); }

            m_ModelInstanceStates = modelInstanceStates.ToDictionary(x => x.Key, x => x.Value);
        }

        public ICollection<ModelObjectReference> ModelInstanceRefs
        {
            get { return m_ModelInstanceStates.Keys.ToList(); }
        }

        public IDictionary<ModelObjectReference, ICurrentState> ModelInstanceStates
        {
            get { return m_ModelInstanceStates.ToDictionary(x => x.Key, x => x.Value); }
        }

        public IProcessingState InitializationState
        {
            get { return m_InitializationState; }
        }

        public RenderingResult Initialize()
        {
            var result = new RenderingResult(ModelTemplateRef, null, ProcessingAcivityType.Initialization);

            var reportState = GetReportInitializationState();
            var renderingState = new RenderingState(m_Report, m_ReportElements, reportState, m_FormulaProcessingEngine);

            var missingReportElementIds = new List<ReportElementId>();
            var inaccessibleReportElementIds = new List<ReportElementId>();
            bool errorFound = false;

            try
            {
                m_InitializationState.Start();
                var reportResult = m_Report.Initialize(DataProvider, renderingState);

                if (!reportResult.IsValid)
                {
                    result.SetErrorState(reportResult);
                    errorFound = true;
                }

                foreach (IReportElement reportElement in m_ReportElements.Values)
                {
                    var reportElementResult = reportElement.Initialize(DataProvider, renderingState);

                    if (!reportElementResult.IsValid)
                    {
                        if (reportElementResult.ResultType == RenderingResultType.ReportElementNotFound)
                        { inaccessibleReportElementIds.Add(reportElement.Key); }
                        else if (reportElementResult.ResultType == RenderingResultType.ParentReportElementNotFound)
                        { missingReportElementIds.Add(reportElement.ParentElementId.Value); }

                        result.SetErrorState(reportElementResult);
                        errorFound = true;
                    }
                }

                if (errorFound)
                {
                    m_InitializationState.Finish(false);
                }
                else
                {
                    result.SetModelInitializedState();
                    m_InitializationState.Finish(true);
                }
            }
            catch (Exception exception)
            {
                result.SetErrorState(RenderingResultType.ModelLevelErrorOccurred, "Initialization Failed: " + exception.Message);
                m_InitializationState.Finish(false);
            }
            finally
            {
                m_MissingReportElementIds = missingReportElementIds.ToList();
                m_InaccessibleReportElementIds = inaccessibleReportElementIds.ToList();
            }
            return result;
        }

        public ICollection<ReportElementId> MissingReportElementIds
        {
            get { return m_MissingReportElementIds.ToList(); }
        }

        public ICollection<ReportElementId> InaccessibleReportElementIds
        {
            get { return m_InaccessibleReportElementIds.ToList(); }
        }

        public IProcessingState ValidationState
        {
            get { return m_ValidationState; }
        }

        public RenderingResult Validate()
        {
            RenderingResult result = new RenderingResult(ModelTemplateRef, null, ProcessingAcivityType.Validation);

            var reportState = GetReportTemplateState();
            var renderingState = new RenderingState(m_Report, m_ReportElements, reportState, m_FormulaProcessingEngine);

            CachedTree<ReportElementId> elementTree = null;
            CachedTree<ReportElementId> styleTree = null;
            bool isReportValid = false;
            List<ReportElementId> invalidReportElementIds = new List<ReportElementId>();
            bool errorFound = false;

            try
            {
                m_ValidationState.Start();
                DataProvider.ClearAnonymousVariable_ReportingStates(m_Report.Key);

                var elementChildParents = new Dictionary<ReportElementId, ReportElementId?>(ReportRenderingEngine.EqualityComparer_ReportElementId);
                var styleChildParents = new Dictionary<ReportElementId, ReportElementId?>(ReportRenderingEngine.EqualityComparer_ReportElementId);

                foreach (IReportElement reportElement in m_ReportElements.Values.OrderBy(x => x.ZOrder).ToList())
                {
                    elementChildParents.Add(reportElement.Key, reportElement.ParentElementId);
                    styleChildParents.Add(reportElement.Key, reportElement.StyleInheritanceElementId);
                }

                elementTree = new CachedTree<ReportElementId>(elementChildParents, ReportRenderingEngine.EqualityComparer_ReportElementId);
                styleTree = new CachedTree<ReportElementId>(styleChildParents, ReportRenderingEngine.EqualityComparer_ReportElementId);

                foreach (var elementId in elementTree.Nodes)
                {
                    var element = m_ReportElements[elementId];

                    elementTree.SetCachedValue(elementId, element);
                    styleTree.SetCachedValue(elementId, element);
                }

                IEnumerable<ReportElementId> elementCycleIds = null;
                IEnumerable<ReportElementId> styleCycleIds = null;
                bool hasElementCycle = elementTree.HasCycles(false, out elementCycleIds);
                bool hasStyleCycle = styleTree.HasCycles(true, out styleCycleIds);

                if (hasElementCycle)
                {
                    invalidReportElementIds.AddRange(elementCycleIds);
                    result.SetErrorState(RenderingResultType.LayoutCycleExists, "Some Report Elements are involved in a Layout Cycle.");
                    errorFound = true;
                    return result;
                }
                if (hasStyleCycle)
                {
                    invalidReportElementIds.AddRange(styleCycleIds);
                    result.SetErrorState(RenderingResultType.StyleCycleExists, "Some Report Elements are involved in a Style Cycle.");
                    errorFound = true;
                    return result;
                }

                if (!hasElementCycle)
                {
                    var reportResult = m_Report.Validate(DataProvider, renderingState, elementTree);

                    if (!reportResult.IsValid)
                    {
                        result.SetErrorState(reportResult);
                        errorFound = true;
                    }
                    else
                    {
                        isReportValid = true;
                    }
                }

                var formulaInitializationResult = FormulaProcessingEngine.Initialize();
                var formulaValidationResult = FormulaProcessingEngine.Validate();
                errorFound = (errorFound || !formulaInitializationResult.IsValid || !formulaValidationResult.IsValid);

                if (errorFound)
                {
                    m_ValidationState.Finish(false);
                }
                else
                {
                    result.SetModelValidatedState();
                    m_ValidationState.Finish(true);
                }
            }
            catch (Exception exception)
            {
                result.SetErrorState(RenderingResultType.ModelLevelErrorOccurred, "Validation Failed: " + exception.Message);
                m_ValidationState.Finish(false);
            }
            finally
            {
                m_ElementTree = elementTree;
                m_StyleTree = styleTree;
                m_RenderingState = renderingState;
                m_IsReportValid = isReportValid;
                m_InvalidReportElementIds = invalidReportElementIds.Distinct().ToList();
            }
            return result;
        }

        public ITree<ReportElementId> ElementTree
        {
            get { return m_ElementTree; }
        }

        public ITree<ReportElementId> StyleTree
        {
            get { return m_StyleTree; }
        }

        public IRenderingState RenderingState
        {
            get { return m_RenderingState; }
        }

        public bool IsValid
        {
            get { return (IsReportValid && AreReportElementsValid); }
        }

        public bool IsReportValid
        {
            get { return m_IsReportValid.Value; }
        }

        public bool AreReportElementsValid
        {
            get { return (m_InvalidReportElementIds.Count < 1); }
        }

        public ICollection<ReportElementId> InvalidReportElementIds
        {
            get { return m_InvalidReportElementIds.ToList(); }
        }

        public IProcessingState RenderForDesignState
        {
            get { return m_RenderForDesignState; }
        }

        public RenderingResult RenderForDesign()
        {
            RenderingResult result = new RenderingResult(ModelTemplateRef, null, ProcessingAcivityType.DesignRendering);

            RenderedReport renderedReport = null;
            bool errorFound = false;

            try
            {
                m_RenderForDesignState.Start();
                m_Report.SetRenderingMode(m_ReportElements.Values, RenderingMode.Design);

                var reportUsesRatio = (Report.ReportAreaLayout.DimensionLayout_X.SizeMode_Value == SizeMode.Ratio) || (Report.ReportAreaLayout.DimensionLayout_Y.SizeMode_Value == SizeMode.Ratio);
                var xElementsUsingRatio = ReportElements.Values.Where(x => x.ElementLayout.DimensionLayout_X.SizeMode_Value == SizeMode.Ratio).ToList().Count;
                var yElementsUsingRatio = ReportElements.Values.Where(x => x.ElementLayout.DimensionLayout_Y.SizeMode_Value == SizeMode.Ratio).ToList().Count;

                if (reportUsesRatio || (xElementsUsingRatio > 0) || (yElementsUsingRatio > 0))
                { throw new InvalidOperationException("Currently, the \"Ratio\" SizeMode is not supported."); }


                var breadthFirstElementIds = m_ElementTree.GetBreadthFirstTraversalFromRoot();
                var renderedLayouts = new Dictionary<RenderingKey, RenderedLayout>(EqualityComparer_RenderingKey);

                var reportResult = m_Report.RenderForDesign(DataProvider, m_RenderingState, m_ElementTree, renderedLayouts);

                if (!reportResult.IsValid)
                {
                    result.SetErrorState(reportResult);
                    errorFound = true;
                }

                var reportRenderingKey = renderedLayouts.Values.First().RenderingKey;
                var reportLayout = renderedLayouts[reportRenderingKey];

                foreach (var reportElementId in m_ElementTree.RootNodes)
                {
                    var reportElement = m_ReportElements[reportElementId];
                    var reportElementResult = reportElement.RenderForDesign(DataProvider, m_RenderingState, m_ElementTree, renderedLayouts, reportRenderingKey);

                    if (!reportElementResult.IsValid)
                    {
                        result.SetErrorState(reportElementResult);
                        errorFound = true;
                    }
                }

                foreach (var renderingKey in renderedLayouts.Keys)
                {
                    if (renderingKey.StructuralPoint.HasValue)
                    { throw new InvalidOperationException("Layout Structural Points must not be initialized in Design Mode."); }
                }

                renderedReport = CreateReportFromLayouts(reportLayout);

                if (errorFound)
                {
                    m_RenderForDesignState.Finish(false);
                }
                else
                {
                    result.SetModelRenderedState();
                    m_RenderForDesignState.Finish(true);
                }
            }
            catch (Exception exception)
            {
                result.SetErrorState(RenderingResultType.ModelLevelErrorOccurred, "Design Rendering Failed: " + exception.Message);
                m_RenderForDesignState.Finish(false);
            }
            finally
            {
                m_RenderedReport_Design = renderedReport;
            }
            return result;
        }

        public RenderedReport DesignReport
        {
            get { return m_RenderedReport_Design; }
        }

        public IProcessingState RenderForProductionState
        {
            get { return m_RenderForProductionState; }
        }

        public RenderingResult RenderForProduction(ModelObjectReference selectedStructuralInstanceRef)
        {
            ICollection<ModelObjectReference> selectedStructuralInstanceRefs = new ModelObjectReference[] { selectedStructuralInstanceRef };
            var result = RenderForProduction(selectedStructuralInstanceRefs).First().Value.First().Value;
            return result;
        }

        public IDictionary<ModelObjectReference, IDictionary<ModelObjectReference, RenderingResult>> RenderForProduction()
        {
            ICollection<ModelObjectReference> selectedStructuralInstanceRefs = null;
            return RenderForProduction(selectedStructuralInstanceRefs);
        }

        public IDictionary<ModelObjectReference, IDictionary<ModelObjectReference, RenderingResult>> RenderForProduction(ICollection<ModelObjectReference> selectedStructuralInstanceRefs)
        {
            var results = new Dictionary<ModelObjectReference, IDictionary<ModelObjectReference, RenderingResult>>();
            var renderedReports = new Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, RenderedReport>>();
            var structuralTypeRef = Report.StructuralTypeRef;
            var variableTemplateRef = DataProvider.DependencyMap.GetIdVariableTemplate(structuralTypeRef);

            var latestModelInstanceRef = (Nullable<ModelObjectReference>)null;
            var latestStructuralInstanceRef = (Nullable<ModelObjectReference>)null;

            try
            {
                m_RenderForProductionState.Start();
                m_Report.SetRenderingMode(m_ReportElements.Values, RenderingMode.Production);

                foreach (var modelInstanceRef in m_ModelInstanceStates.Keys)
                {
                    latestModelInstanceRef = modelInstanceRef;

                    results.Add(modelInstanceRef, new Dictionary<ModelObjectReference, RenderingResult>());
                    renderedReports.Add(modelInstanceRef, new Dictionary<ModelObjectReference, RenderedReport>());

                    var modelInstanceState = m_ModelInstanceStates[modelInstanceRef];
                    var structuralInstanceRefs = DataProvider.StructuralMap.GetStructuralInstancesForType(modelInstanceRef, structuralTypeRef);

                    if (selectedStructuralInstanceRefs != null)
                    { structuralInstanceRefs = structuralInstanceRefs.Where(x => selectedStructuralInstanceRefs.Contains(x)).ToList(); }

                    foreach (var structuralInstanceRef in structuralInstanceRefs)
                    {
                        latestStructuralInstanceRef = structuralInstanceRef;

                        bool errorFound = false;
                        RenderingResult result = new RenderingResult(ModelTemplateRef, modelInstanceRef, structuralTypeRef, structuralInstanceRef, ProcessingAcivityType.ProductionRendering);

                        results[modelInstanceRef].Add(structuralInstanceRef, result);

                        var variableInstanceRef = DataProvider.GetChildVariableInstanceReference(modelInstanceRef, variableTemplateRef, structuralInstanceRef);
                        var productionState = new CurrentState(modelInstanceState, structuralTypeRef, variableTemplateRef, structuralInstanceRef, variableInstanceRef);
                        var structuralPositions = DataProvider.StructuralMap.GetRelativeStructuralPoints(modelInstanceRef, m_RenderingState.CurrentTimeKey.NullablePrimaryTimePeriod, structuralInstanceRef, productionState.UseExtendedStructure);

                        if (structuralPositions.Count != 1)
                        { throw new InvalidOperationException("The Report requires a single Structural Point to begin Production Rendering."); }

                        m_RenderingState.SetToProductionState(DataProvider, productionState, structuralPositions.First());

                        var breadthFirstElementIds = m_ElementTree.GetBreadthFirstTraversalFromRoot();
                        var renderedLayouts = new Dictionary<RenderingKey, RenderedLayout>(EqualityComparer_RenderingKey);

                        var reportResult = m_Report.RenderForProduction(DataProvider, m_RenderingState, m_ElementTree, renderedLayouts);

                        if (!reportResult.IsValid)
                        {
                            result.SetErrorState(reportResult);
                            errorFound = true;
                        }

                        var reportRenderingKey = renderedLayouts.Values.First().RenderingKey;
                        var reportLayout = renderedLayouts[reportRenderingKey];

                        foreach (var reportElementId in m_ElementTree.RootNodes)
                        {
                            var reportElement = m_ReportElements[reportElementId];
                            var reportElementResult = reportElement.RenderForProduction(DataProvider, m_RenderingState, m_ElementTree, renderedLayouts, reportRenderingKey);

                            if (!reportElementResult.IsValid)
                            {
                                result.SetErrorState(reportElementResult);
                                errorFound = true;
                            }
                        }

                        foreach (var renderingKey in renderedLayouts.Keys)
                        {
                            if (!renderingKey.StructuralPoint.HasValue)
                            { throw new InvalidOperationException("Layout Structural Points must be initialized in Production Mode."); }
                        }

                        if (errorFound)
                        {
                            if (!m_RenderForProductionState.Finished)
                            { m_RenderForProductionState.Finish(false); }
                            else if (m_RenderForProductionState.Succeeded)
                            { throw new InvalidOperationException("Production Rendering succeeded prematurely."); }
                        }
                        else
                        {
                            var renderedReport = CreateReportFromLayouts(reportLayout);

                            result.SetModelRenderedState();
                            renderedReports[modelInstanceRef].Add(structuralInstanceRef, renderedReport);
                        }

                        var formulaComputationResultForInstance = FormulaProcessingEngine.CompleteAnonymousComputation_ForInstance(modelInstanceRef, structuralInstanceRef);
                        if (!formulaComputationResultForInstance.IsValid)
                        {
                            results[latestModelInstanceRef.Value][latestStructuralInstanceRef.Value].SetErrorState(RenderingResultType.ModelLevelErrorOccurred, "Failed to complete Anonymous Computation.");
                            m_RenderForProductionState.Finish(false);
                        }
                    }
                }

                var formulaComputationResultForTemplate = FormulaProcessingEngine.CompleteAnonymousComputation_ForTemplate();
                if (!formulaComputationResultForTemplate.IsValid)
                {
                    results[latestModelInstanceRef.Value][latestStructuralInstanceRef.Value].SetErrorState(RenderingResultType.ModelLevelErrorOccurred, "Failed to complete Anonymous Computation.");
                    m_RenderForProductionState.Finish(false);
                }

                if (!m_RenderForProductionState.Finished)
                { m_RenderForProductionState.Finish(true); }
            }
            catch (Exception exception)
            {
                results[latestModelInstanceRef.Value][latestStructuralInstanceRef.Value].SetErrorState(RenderingResultType.ModelLevelErrorOccurred, "Production Rendering Failed: " + exception.Message);
                m_RenderForProductionState.Finish(false);
            }
            finally
            {
                m_RenderedReports_Production = renderedReports;
            }
            return results;
        }

        public IDictionary<ModelObjectReference, IDictionary<ModelObjectReference, RenderedReport>> ProductionReports
        {
            get
            {
                var results = new Dictionary<ModelObjectReference, IDictionary<ModelObjectReference, RenderedReport>>();
                foreach (var modelInstanceRef in m_RenderedReports_Production.Keys)
                {
                    var modelInstanceResults = m_RenderedReports_Production[modelInstanceRef].ToDictionary(x => x.Key, x => x.Value);
                    results.Add(modelInstanceRef, modelInstanceResults);
                }
                return results;
            }
        }

        private static RenderedReport CreateReportFromLayouts(RenderedLayout reportLayout)
        {
            var renderingObjectsByFastId = new Dictionary<Guid, object>();
            var renderingObjectsByKey = new Dictionary<RenderingKey, object>(EqualityComparer_RenderingKey);

            renderingObjectsByFastId.Add(reportLayout.FastId, reportLayout);

            var dimensionsToProcess = reportLayout.TemplateLayout.Dimensions;
            var renderingTrees = new SortedDictionary<Dimension, ITree<Guid>>();

            foreach (var dimension in dimensionsToProcess)
            {
                var renderingHierarchy = new Dictionary<Guid, Guid?>();
                renderingHierarchy.Add(reportLayout.FastId, null);

                RenderingUtils.CreateRenderingGroups(reportLayout, dimension, renderingObjectsByFastId, renderingHierarchy);

                var renderingTree = new CachedTree<Guid>(renderingHierarchy);

                foreach (var renderingKey in renderingObjectsByFastId.Keys)
                { renderingTree.SetCachedValue(renderingKey, renderingObjectsByFastId[renderingKey]); }

                renderingTrees.Add(dimension, renderingTree);
            }

            foreach (var renderingObject in renderingObjectsByFastId.Values)
            {
                if (renderingObject is RenderingGroup)
                {
                    var group = (RenderingGroup)renderingObject;
                    renderingObjectsByKey.Add(group.RenderingKey, group);
                }
                else
                {
                    var layout = (RenderedLayout)renderingObject;
                    renderingObjectsByKey.Add(layout.RenderingKey, layout);
                }
            }

            foreach (var dimension in dimensionsToProcess)
            {
                var renderingTree = renderingTrees[dimension];

                var bfsTraversal = renderingTree.GetBreadthFirstTraversalFromRoot();
                var layoutOrder = bfsTraversal.Reverse().ToList();

                var delayedSizedKeys = new HashSet<Guid>();
                var intializedKeys = new HashSet<Guid>();

                foreach (var renderingKey in layoutOrder)
                {
                    var renderingObject = renderingTree.GetCachedValue(renderingKey);

                    if (renderingObject is RenderingGroup)
                    {
                        var group = (RenderingGroup)renderingObject;

                        if (group.IsDelayedSized())
                        { delayedSizedKeys.Add(renderingKey); }
                        else
                        { RenderingUtils.HandleRenderingGroup(group, dimension, null, renderingTree, intializedKeys); }
                    }
                    else
                    {
                        var layout = (RenderedLayout)renderingObject;

                        if (layout.IsDelayedSized())
                        { delayedSizedKeys.Add(renderingKey); }
                        else
                        { RenderingUtils.HandleRenderedLayout(layout, dimension, null, renderingTree, intializedKeys); }
                    }
                }

                foreach (var renderingKey in delayedSizedKeys)
                {
                    var renderingObject = renderingTree.GetCachedValue(renderingKey);

                    if (renderingObject is RenderingGroup)
                    {
                        var group = (RenderingGroup)renderingObject;
                        var sizeToUse = group.GetGroupSizeUsingDelayedSizing(dimension, renderingTree, renderingObjectsByKey);
                        RenderingUtils.HandleRenderingGroup(group, dimension, sizeToUse, renderingTree, intializedKeys);
                    }
                    else
                    {
                        var layout = (RenderedLayout)renderingObject;
                        RenderingUtils.HandleRenderedLayout(layout, dimension, null, renderingTree, intializedKeys);
                    }
                }
            }


            var renderedReport = new RenderedReport(reportLayout);
            var layoutsToProcess = reportLayout.NestedLayouts.Select(x => new KeyValuePair<RenderedLayout, RenderedLayout>(x, reportLayout)).ToList();

            var renderedObjectsByLayout = new Dictionary<RenderedLayout, RenderedObjectBase<RenderedReport, RenderedRange>>();
            renderedObjectsByLayout.Add(reportLayout, renderedReport);

            while (layoutsToProcess.Count > 0)
            {
                var currentItem = layoutsToProcess[0];
                var currentLayout = currentItem.Key;
                var parentLayout = currentItem.Value;
                var parentRange = renderedObjectsByLayout[parentLayout];

                layoutsToProcess.RemoveAt(0);

                var groupOffset = new Point(0, 0);

                foreach (var groupDimension in parentLayout.ContainerDimensions)
                {
                    var groupFound = false;

                    foreach (var group in parentLayout.NestedGroups[groupDimension].Values)
                    {
                        if (groupFound)
                        { continue; }

                        if (group.ContainedLayouts.Values.Contains(currentLayout))
                        { groupFound = true; }
                        else
                        {
                            groupOffset.X = (groupDimension == Dimension.X) ? groupOffset.X + group.ActualSize.Width : groupOffset.X;
                            groupOffset.Y = (groupDimension == Dimension.Y) ? groupOffset.Y + group.ActualSize.Height : groupOffset.Y;
                        }
                    }

                    if (!groupFound)
                    { throw new InvalidOperationException("A required RenderingGroup could not be found."); }
                }

                var currentRange = new RenderedRange(currentLayout, groupOffset);
                parentRange.AddChild(currentRange);
                renderedObjectsByLayout.Add(currentLayout, currentRange);

                foreach (var childLayout in currentLayout.NestedLayouts.OrderBy(x => x.ZOrder))
                {
                    layoutsToProcess.Add(new KeyValuePair<RenderedLayout, RenderedLayout>(childLayout, currentLayout));
                }
            }
            return renderedReport;
        }

        #region Helper Methods

        private ICurrentState GetReportInitializationState()
        {
            ICurrentState reportState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, ModelTemplateRef, DeciaDataTypeUtils.MinDateTime, DeciaDataTypeUtils.MaxDateTime);
            return reportState;
        }

        private ICurrentState GetReportTemplateState()
        {
            var structuralTypeRef = m_Report.StructuralTypeRef;
            var variableTemplateRef = DataProvider.DependencyMap.GetIdVariableTemplate(structuralTypeRef);

            ICurrentState reportState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, ModelTemplateState.ModelTemplateRef, structuralTypeRef, variableTemplateRef);
            reportState.ModelStartDate = ModelTemplateState.ModelStartDate;
            reportState.ModelEndDate = ModelTemplateState.ModelEndDate;

            return reportState;
        }

        private ICurrentState GetReportInstanceState(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef)
        {
            var structuralTypeRef = m_Report.StructuralTypeRef;
            var variableTemplateRef = DataProvider.DependencyMap.GetIdVariableTemplate(structuralTypeRef);
            var variableInstanceRef = DataProvider.GetChildVariableInstanceReference(modelInstanceRef, variableTemplateRef, structuralInstanceRef);

            var modelInstanceState = ModelInstanceStates[modelInstanceRef];

            ICurrentState reportState = new CurrentState(ProjectId.ProjectGuid, ProjectId.RevisionNumber_NonNull, modelInstanceState.ModelTemplateRef, structuralTypeRef, variableTemplateRef, modelInstanceRef, structuralInstanceRef, variableInstanceRef);
            reportState.ModelStartDate = modelInstanceState.ModelStartDate;
            reportState.ModelEndDate = modelInstanceState.ModelEndDate;

            return reportState;
        }

        #endregion
    }
}