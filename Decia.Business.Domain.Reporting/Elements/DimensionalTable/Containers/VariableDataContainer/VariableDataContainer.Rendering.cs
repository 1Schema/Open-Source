using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.Reporting.Dimensionality;
using Decia.Business.Domain.Reporting.Dimensionality.Production;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public partial class VariableDataContainer
    {
        public override RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            ICurrentState reportState = renderingState.ReportState;
            DimensionalRepeatGroup parentRepeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.ParentElementOrReportId];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.Validation);

            if (!m_CommonTitleContainerNumber.HasValue)
            { throw new InvalidOperationException("The related CommonTitleContainer has not been set."); }
            if (!m_VariableTitleContainerNumber.HasValue)
            { throw new InvalidOperationException("The related VariableTitleContainer has not been set."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_CommonTitleContainerNumber.Value)))
            { throw new InvalidOperationException("The related CommonTitleContainer does not exist in the current Report."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_VariableTitleContainerNumber.Value)))
            { throw new InvalidOperationException("The related VariableTitleContainer does not exist in the current Report."); }

            var currrentRepeatGroupName = this.Name;
            var currentRepeatGroup = new DimensionalRepeatGroup(currrentRepeatGroupName, parentRepeatGroup.DesignState, parentRepeatGroup.DesignContext, parentRepeatGroup.AdditionalStructuralTypeRefs, parentRepeatGroup.RelevantTimeDimensions);
            parentRepeatGroup.AddNestedRepeatGroup(currentRepeatGroup);
            currentRepeatGroup.AddElementToGroup(this);

            var commonTitleContainer = elementTree.GetCachedValue<ICommonTitleContainer>(this.CommonTitleContainerId);
            var commonTitleContainerGroup = renderingState.GroupingState.ElementRepeatGroups[commonTitleContainer.Key];

            if (commonTitleContainerGroup.NestedRepeatGroups.Count != 1)
            { throw new InvalidOperationException("Encountered unexpected number of Repeat Groups nested in CommonTitleContainer's Repeat Group."); }
            var nestedCommonTitleGroup = commonTitleContainerGroup.NestedRepeatGroups.Values.First();

            var variableTitleContainer = elementTree.GetCachedValue<IVariableTitleContainer>(this.VariableTitleContainerId);
            var variableTitleContainerGroup = renderingState.GroupingState.ElementRepeatGroups[variableTitleContainer.Key];

            var variableDataBoxIds = elementTree.GetChildren(this.Key);
            var variableDataBoxes = variableDataBoxIds.Select(x => elementTree.GetCachedValue<IVariableDataBox>(x)).ToDictionary(x => x.Key, x => x, ReportRenderingEngine.EqualityComparer_ReportElementId);

            var orderedNestedTitleGroups = TraverseGroupTree(renderingState.GroupingState, variableTitleContainerGroup.NestedRepeatGroups.Values);

            foreach (var nestedVariableTitleGroup in orderedNestedTitleGroups)
            {
                var additionalStructuralTypeRefs = nestedVariableTitleGroup.AdditionalStructuralTypeRefs.ToList();
                var relevantTimeDimensions = nestedVariableTitleGroup.RelevantTimeDimensions.ToDictionary(x => x.Key, x => x.Value);
                IStructuralContext structuralContext = null;

                additionalStructuralTypeRefs.AddRange(nestedCommonTitleGroup.AdditionalStructuralTypeRefs);

                foreach (var timeDimension in nestedCommonTitleGroup.RelevantTimeDimensions)
                {
                    if (!relevantTimeDimensions.ContainsKey(timeDimension.Key))
                    { relevantTimeDimensions.Add(timeDimension.Key, timeDimension.Value); }
                    else if (timeDimension.Value.HasValue)
                    { relevantTimeDimensions[timeDimension.Key] = timeDimension.Value; }
                }

                var structuralContexts = DimensioningUtils.BuildStructuralContexts(dataProvider, reportState, additionalStructuralTypeRefs);
                if (structuralContexts.Count != 1)
                { throw new InvalidOperationException("The current Report Element does not have a valid Structural Context."); }
                structuralContext = structuralContexts.First().Value;

                var nestedDesignState = nestedVariableTitleGroup.DesignState;

                var nestedVariableDataGroupName = nestedVariableTitleGroup.Name.Replace("Title", "Data");
                var nestedVariableDataGroup = new DimensionalRepeatGroup(nestedVariableDataGroupName, nestedDesignState, structuralContext, additionalStructuralTypeRefs, relevantTimeDimensions);

                currentRepeatGroup.AddNestedRepeatGroup(nestedVariableDataGroup);

                foreach (IVariableTitleBox variableTitleBox in nestedVariableTitleGroup.GroupedElements.Values.Where(x => (x is IVariableTitleBox)))
                {
                    var relevantVariableDataBoxes = variableDataBoxes.Values.Where(x => x.RelatedVariableTitleBoxNumber == variableTitleBox.Key.ReportElementNumber).ToList();

                    foreach (var variableDataBox in relevantVariableDataBoxes)
                    {
                        nestedVariableDataGroup.AddElementToGroup(variableDataBox);
                    }
                }
            }

            bool hasNestedElements = false;
            foreach (var nestedRepeatGroup in currentRepeatGroup.NestedRepeatGroups.Values)
            {
                foreach (var elementId in nestedRepeatGroup.GroupedElements.Keys)
                {
                    if (!renderingState.GroupingState.ProcessedElementIds.Contains(elementId))
                    { throw new InvalidOperationException("Encountered a Report Element that was not added to a Repeat Group."); }

                    var element = nestedRepeatGroup.GroupedElements[elementId];
                    var elementResult = element.Validate(dataProvider, renderingState, elementTree);

                    if (!elementResult.IsValid)
                    {
                        result.SetErrorState(elementResult);
                        return result;
                    }
                    else
                    { result.SetValidatedState(elementResult); }
                }
            }

            if (!hasNestedElements)
            { result.SetValidatedState(); }
            return result;
        }

        private List<DimensionalRepeatGroup> TraverseGroupTree(DimensionalGroupingState groupingState, IEnumerable<DimensionalRepeatGroup> currentRepeatGroups)
        {
            List<DimensionalRepeatGroup> results = new List<DimensionalRepeatGroup>();

            foreach (var repeatGroup in currentRepeatGroups)
            {
                results.Add(repeatGroup);

                var nestedResults = TraverseGroupTree(groupingState, repeatGroup.NestedRepeatGroups.Values);
                results.AddRange(nestedResults);
            }
            return results;
        }

        public override RenderingResult RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            var result = ReportElementUtils.RenderContainerForDesign(this, dataProvider, renderingState, elementTree, layoutResults, parentRenderingKey);
            return result;
        }

        public override RenderingResult RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            ICurrentState reportState = renderingState.ReportState;
            IReport report = renderingState.Report;
            DimensionalRepeatGroup repeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.Key];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.ModelInstanceRef, reportState.StructuralTypeRef, reportState.StructuralInstanceRef, this.Key, ProcessingAcivityType.ProductionRendering);

            var currentLayout = new RenderedLayout();
            currentLayout.Report = report;
            currentLayout.Element = this;
            currentLayout.StructuralSpace = repeatGroup.DesignContext.ResultingSpace;
            currentLayout.StructuralPoint = renderingState.CurrentStructuralPosition.Value;
            currentLayout.TimeKey = renderingState.CurrentTimeBindings;
            currentLayout.OverriddenContentGroups = new SortedDictionary<Dimension, int>(renderingState.OverriddenContentGroups);
            currentLayout.ActualSize = this.ElementLayout.GetDesiredSize(currentLayout.ActualSize);
            layoutResults.Add(currentLayout.RenderingKey, currentLayout);

            var parentLayout = layoutResults[parentRenderingKey];
            parentLayout.NestedLayouts.Add(currentLayout);

            if ((renderingState.DimensionalTable_VariableHeaders.Count < 1) || (renderingState.DimensionalTable_CommonHeaders.Count < 1))
            {
                renderingState.DimensionalTable_CommonHeaders = null;
                renderingState.DimensionalTable_VariableHeaders = null;

                result.SetRenderedState();
                return result;
            }

            var variableDataBoxes = this.VariableDataBoxIds.Select(x => (IVariableDataBox)renderingState.ReportElements[x]).ToList();
            var variableDataBoxesByTitleId = variableDataBoxes.ToDictionary(x => x.RelatedVariableTitleBoxId, x => x, ReportRenderingEngine.EqualityComparer_ReportElementId);

            foreach (var varResult in renderingState.DimensionalTable_VariableHeaders)
            {
                foreach (var comResult in renderingState.DimensionalTable_CommonHeaders)
                {
                    var varSpaceResult = varResult.SpaceResult;
                    var comSpaceResult = comResult.SpaceResult;

                    var varDimResults = varSpaceResult.DimensionResults;
                    var comDimResults = comSpaceResult.DimensionResults;

                    var dimensionResults = varDimResults;
                    foreach (var dimensionIndex in comDimResults.Keys)
                    {
                        var newDimensionIndex = varDimResults.Count + dimensionIndex;
                        var dimensionValue = comDimResults[dimensionIndex];
                        dimensionResults.Add(newDimensionIndex, dimensionValue);
                    }

                    var nestedRenderingState = new RenderingState(renderingState);

                    var structuralValues = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);
                    var variableValues = new Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, ModelObjectReference>>(ModelObjectReference.DimensionalComparer);
                    var timeKey = MultiTimePeriodKey.DimensionlessTimeKey;

                    foreach (var dimensionIndex in dimensionResults.Keys)
                    {
                        var dimensionResult = dimensionResults[dimensionIndex];

                        if (dimensionResult.DimensionType == DimensionType.Structure)
                        {
                            var structuralTypeRef = dimensionResult.DimensionRef;
                            var structuralInstanceRef = dimensionResult.StructuralInstanceRef.Value;

                            structuralValues[structuralTypeRef] = structuralInstanceRef;

                            var variableTemplateRef = dimensionResult.ParentEnumerator.NameVariableRef.Value;
                            var variableInstanceRef = dimensionResult.VariableInstanceRef.Value;

                            variableValues[structuralTypeRef] = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);
                            variableValues[structuralTypeRef][variableTemplateRef] = variableInstanceRef;
                        }
                        else if (dimensionResult.DimensionType == DimensionType.Time)
                        {
                            var timeDimensionType = dimensionResult.TimeDimensionType.Value;
                            var timePeriod = dimensionResult.PeriodToDisplay.Value;

                            if (timeDimensionType == TimeDimensionType.Primary)
                            { timeKey = new MultiTimePeriodKey(timePeriod, timeKey.NullableSecondaryTimePeriod); }
                            else if (timeDimensionType == TimeDimensionType.Secondary)
                            { timeKey = new MultiTimePeriodKey(timeKey.NullablePrimaryTimePeriod, timePeriod); }
                            else
                            { throw new InvalidOperationException(); }
                        }
                        else
                        { throw new InvalidOperationException(); }
                    }

                    nestedRenderingState.CurrentStructuralBindings = structuralValues;
                    nestedRenderingState.CurrentVariableBindings = variableValues;
                    nestedRenderingState.CurrentTimeBindings = timeKey;
                    nestedRenderingState.UpdateProductionStateFromBindings();

                    nestedRenderingState.OverriddenContentGroups[this.StackingDimension] = varResult.OrderNumber;
                    nestedRenderingState.OverriddenContentGroups[this.CommonDimension] = comResult.OrderNumber;

                    var childElement = variableDataBoxesByTitleId[varResult.VariableTitleBox.Key];
                    this.AssertChildMatchesParentSettings(childElement);

                    var childElementResult = childElement.RenderForProduction(dataProvider, nestedRenderingState, elementTree, layoutResults, currentLayout.RenderingKey);

                    if (!childElementResult.IsValid)
                    {
                        result.SetErrorState(childElementResult);
                        return result;
                    }
                    else
                    { result.SetRenderedState(childElementResult); }
                }
            }

            renderingState.DimensionalTable_CommonHeaders = null;
            renderingState.DimensionalTable_VariableHeaders = null;

            return result;
        }
    }
}