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
    public partial class CommonTitleContainer
    {
        public override RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            ICurrentState reportState = renderingState.ReportState;
            DimensionalRepeatGroup parentRepeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.ParentElementOrReportId];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.Validation);

            if (!m_VariableDataContainerNumber.HasValue)
            { throw new InvalidOperationException("The related VariableDataContainer has not been set."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_VariableDataContainerNumber.Value)))
            { throw new InvalidOperationException("The related VariableDataContainer does not exist in the current Report."); }

            var currrentRepeatGroupName = this.Name;
            var currentRepeatGroup = new DimensionalRepeatGroup(currrentRepeatGroupName, parentRepeatGroup.DesignState, parentRepeatGroup.DesignContext, parentRepeatGroup.AdditionalStructuralTypeRefs, parentRepeatGroup.RelevantTimeDimensions);
            parentRepeatGroup.AddNestedRepeatGroup(currentRepeatGroup);
            currentRepeatGroup.AddElementToGroup(this);

            var commonTitleBoxes = renderingState.ReportElements.Values.Where(x => this.CommonTitleBoxNumbers.Contains(x.Key.ReportElementNumber)).Select(x => (x as ICommonTitleBox)).ToList();
            var orderedCommonTitleBoxesDict = commonTitleBoxes.OrderBy(x => x.CommonOrder).ToDictionary(x => x.CommonOrder, x => x);

            IStructuralContext structuralContext = null;
            var additionalStructuralTypeRefs = new List<ModelObjectReference>();
            var relevantTimeDimensions = new Dictionary<TimeDimensionType, TimePeriodType?>();

            if (commonTitleBoxes.Count > 0)
            {
                var maximalCommonTitleBox = (ICommonTitleBox)null;
                foreach (var commonTitleBox in commonTitleBoxes)
                {
                    if (maximalCommonTitleBox == null)
                    { maximalCommonTitleBox = commonTitleBox; }
                    else
                    {
                        if ((commonTitleBox.ContainedStructuralTitleRangeIds.Count > maximalCommonTitleBox.ContainedStructuralTitleRangeIds.Count)
                            || (commonTitleBox.ContainedTimeTitleRangeIds.Count > maximalCommonTitleBox.ContainedTimeTitleRangeIds.Count))
                        { maximalCommonTitleBox = commonTitleBox; }
                    }
                }

                this.ValidateDimensionsForContainer(maximalCommonTitleBox, renderingState.ReportElements);

                bool hasAdditionalNestedStructure = (maximalCommonTitleBox.ContainedStructuralTitleRangeIds.Count > 0);
                bool hasAdditionalNestedTime = (maximalCommonTitleBox.ContainedTimeTitleRangeIds.Count > 0);

                additionalStructuralTypeRefs = currentRepeatGroup.AdditionalStructuralTypeRefs.ToList();
                relevantTimeDimensions = currentRepeatGroup.RelevantTimeDimensions.ToDictionary(x => x.Key, x => x.Value);

                if (hasAdditionalNestedStructure)
                {
                    foreach (var elementId in maximalCommonTitleBox.ContainedStructuralTitleRangeIds)
                    {
                        var range = elementTree.GetCachedValue<IStructuralTitleRange>(elementId);
                        additionalStructuralTypeRefs.Add(range.EntityTypeRef);
                    }
                }

                if (hasAdditionalNestedTime)
                {
                    foreach (var elementId in maximalCommonTitleBox.ContainedTimeTitleRangeIds)
                    {
                        var range = elementTree.GetCachedValue<ITimeTitleRange>(elementId);

                        var timePeriodType = relevantTimeDimensions[range.TimeDimensionType];

                        if (!timePeriodType.HasValue)
                        { relevantTimeDimensions[range.TimeDimensionType] = range.TimePeriodType; }
                        else
                        {
                            if (timePeriodType != range.TimePeriodType)
                            { throw new InvalidOperationException("Currently only a single TimePeriodType per Time Dimension is supported in Reports."); }
                        }
                    }
                }
            }

            var structuralContexts = DimensioningUtils.BuildStructuralContexts(dataProvider, reportState, additionalStructuralTypeRefs);
            if (structuralContexts.Count != 1)
            {
                result.SetErrorState(RenderingResultType.StructuralReferenceIsInvalid, "The current Report Element does not have a valid Structural Context.");
                return result;
            }
            structuralContext = structuralContexts.First().Value;

            var nestedRepeatGroupName = DimensionalRepeatGroup.GetDefaultGroupName(this.Name);
            var nestedRepeatGroup = new DimensionalRepeatGroup(nestedRepeatGroupName, currentRepeatGroup.DesignState, structuralContext, additionalStructuralTypeRefs, relevantTimeDimensions);
            currentRepeatGroup.AddNestedRepeatGroup(nestedRepeatGroup);

            var childElements = renderingState.ReportElements.Values.Where(x => this.ChildNumbers.Contains(x.Key.ReportElementNumber)).ToList();

            foreach (var element in childElements)
            {
                if (renderingState.GroupingState.ProcessedElementIds.Contains(element.Key))
                { throw new InvalidOperationException("Encountered a Report Element that was handled out of order."); }

                nestedRepeatGroup.AddElementToGroup(element);
                var elementResult = element.Validate(dataProvider, renderingState, elementTree);

                if (!elementResult.IsValid)
                {
                    result.SetErrorState(elementResult);
                    return result;
                }
                else
                { result.SetValidatedState(elementResult); }
            }

            if (childElements.Count <= 0)
            { result.SetValidatedState(); }
            return result;
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

            var childElementIds = elementTree.GetChildren(this.Key);
            var childCommonTitleBoxes = childElementIds.Select(x => renderingState.ReportElements[x]).Where(x => (x is ICommonTitleBox)).Select(x => (x as ICommonTitleBox)).OrderBy(x => x.CommonOrder).ToList();

            var firstCommonTitleBox = childCommonTitleBoxes.First();
            var childDimensionalRanges = firstCommonTitleBox.ChildIds.Select(x => renderingState.ReportElements[x]).Where(x => (x is IDimensionalRange)).ToList();

            var spaceEnumerator = new SpaceEnumerator(this, elementTree);
            var spaceResults = spaceEnumerator.EnumerateResults(dataProvider, reportState);

            var maxIterationCount = spaceResults.Count;
            var hasNoDimensions = (spaceEnumerator.ReducedNestedElements.Count <= 0);
            var hasNoResults = !hasNoDimensions && (maxIterationCount <= 0);

            if (hasNoResults)
            {
                renderingState.DimensionalTable_CommonHeaders = new List<CommonHeaderResult>();
                result.SetRenderedState();
                return result;
            }

            if (hasNoDimensions)
            {
                ElementRenderingUtils.SetMinSizeToShowValues(this.ElementLayout, this.CommonDimension);

                var valueHeaderResult = new CommonHeaderResult(repeatGroup);

                renderingState.DimensionalTable_CommonHeaders = (new CommonHeaderResult[] { valueHeaderResult }).ToList();
                result.SetRenderedState();
                return result;
            }

            var commonHeaderEnumerator = new CommonHeaderEnumerator(renderingState, this, childCommonTitleBoxes, spaceResults);
            var commonHeaderResults = commonHeaderEnumerator.EnumerateResults(elementTree);

            renderingState.DimensionalTable_CommonHeaders = commonHeaderResults;

            foreach (var comResult in commonHeaderResults)
            {
                var spaceResult = comResult.SpaceResult;
                var dimensionResults = spaceResult.DimensionResults;
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

                    nestedRenderingState.CurrentStructuralBindings = structuralValues;
                    nestedRenderingState.CurrentVariableBindings = variableValues;
                    nestedRenderingState.CurrentTimeBindings = timeKey;
                    nestedRenderingState.UpdateProductionStateFromBindings();
                }

                nestedRenderingState.OverriddenContentGroups[this.CommonDimension] = comResult.OrderNumber;

                var childElement = comResult.CommonTitleBox;
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
            return result;
        }
    }
}