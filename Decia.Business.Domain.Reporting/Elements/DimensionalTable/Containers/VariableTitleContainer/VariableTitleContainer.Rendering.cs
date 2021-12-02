using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
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
    public partial class VariableTitleContainer
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

            var variableTitleBoxes = renderingState.ReportElements.Values.Where(x => this.VariableTitleBoxNumbers.Contains(x.Key.ReportElementNumber)).Select(x => (x as IVariableTitleBox)).ToList();
            var orderedVariableTitleBoxesList = variableTitleBoxes.OrderBy(x => x.StackingOrder).ToList();
            var orderedVariableTitleBoxesDict = orderedVariableTitleBoxesList.ToDictionary(x => orderedVariableTitleBoxesList.IndexOf(x), x => x);

            this.ValidateDimensionsForContainer(variableTitleBoxes, renderingState.ReportElements);

            var groupDatasById = new SortedDictionary<Guid, RepeatGroupData>();
            var groupDatasByIndex = new SortedDictionary<int, RepeatGroupData>();
            var previousGroupData = (RepeatGroupData)null;

            foreach (var titleOrder in orderedVariableTitleBoxesDict.Keys)
            {
                var titleBox = orderedVariableTitleBoxesDict[titleOrder];
                var titleGroupName = titleBox.RepeatGroup;

                var parentGroupData = (RepeatGroupData)null;
                bool addNewGroup = true;

                if (previousGroupData == null)
                {
                    parentGroupData = CreateGroupDataSubTreeUpToCurrent(groupDatasById, groupDatasByIndex, titleGroupName, previousGroupData);
                }
                else
                {
                    if (previousGroupData.DoesGroupNameMatch(titleGroupName))
                    {
                        previousGroupData.AddVariableTitleBox(titleBox);
                        addNewGroup = false;
                    }
                    else if (previousGroupData.IsGroupNameAncestor(titleGroupName))
                    {
                        parentGroupData = previousGroupData;

                        while (!RepeatGroupData.DoGroupNamesMatch(titleGroupName, parentGroupData.GroupName))
                        {
                            if (!parentGroupData.ParentGroupId.HasValue)
                            { throw new InvalidOperationException("Unexpected root Group encountered."); }

                            parentGroupData = groupDatasById[parentGroupData.ParentGroupId.Value];
                        }

                        parentGroupData.AddVariableTitleBox(titleBox);
                        previousGroupData = parentGroupData;
                        addNewGroup = false;
                    }
                    else if (previousGroupData.IsGroupNameDescendant(titleGroupName))
                    {
                        parentGroupData = CreateGroupDataSubTreeUpToCurrent(groupDatasById, groupDatasByIndex, titleGroupName, previousGroupData);
                    }
                    else if (previousGroupData.IsGroupNameRelated(titleGroupName))
                    {
                        var firstDifferenceIndex = previousGroupData.GetGroupNameRelationSeparationIndex(titleGroupName).Value;
                        var desiredAncestorDepth = (firstDifferenceIndex - 1);
                        var ancestorGroupData = RepeatGroupData.GetAncesorAtDepth(previousGroupData, groupDatasById, desiredAncestorDepth);

                        parentGroupData = CreateGroupDataSubTreeUpToCurrent(groupDatasById, groupDatasByIndex, titleGroupName, ancestorGroupData);
                    }
                }

                if (!addNewGroup)
                { continue; }

                var currentGroupIndex = groupDatasByIndex.Count() + 1;
                var currentGroupData = new RepeatGroupData(titleGroupName, currentGroupIndex, parentGroupData);
                currentGroupData.AddVariableTitleBox(titleBox);

                groupDatasById.Add(currentGroupData.GroupId, currentGroupData);
                groupDatasByIndex.Add(currentGroupData.GroupIndex, currentGroupData);

                previousGroupData = currentGroupData;
            }

            var usedGroupNames = new List<string>();
            var createdGroups = new List<DimensionalRepeatGroup>();

            CreateRepeatGroups(dataProvider, renderingState, elementTree, currentRepeatGroup, null, groupDatasByIndex, usedGroupNames, createdGroups);

            bool hasNestedElements = false;
            foreach (var nestedRepeatGroup in createdGroups)
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

        private static RepeatGroupData CreateGroupDataSubTreeUpToCurrent(SortedDictionary<Guid, RepeatGroupData> groupDatasById, SortedDictionary<int, RepeatGroupData> groupDatasByIndex, string titleGroupName, RepeatGroupData existingAncestorGroupData)
        {
            var parentGroupData = existingAncestorGroupData;
            bool hasEncounteredParent = (existingAncestorGroupData == null);

            var partialGroupName = string.Empty;
            var fullGroupName = string.Empty;

            var groupNames = RepeatGroupData.GetStandardizedGroupName(titleGroupName).Split(DimensionalRepeatGroup.ValidGroupNameSeparators);

            for (int i = 0; i < (groupNames.Length - 1); i++)
            {
                partialGroupName = groupNames[i];
                fullGroupName += string.IsNullOrWhiteSpace(fullGroupName) ? partialGroupName : DimensionalRepeatGroup.DefaultGroupNameSeparator + partialGroupName;

                if (hasEncounteredParent)
                {
                    var parentGroupIndex = groupDatasByIndex.Count() + 1;
                    parentGroupData = new RepeatGroupData(fullGroupName, parentGroupIndex, parentGroupData);

                    groupDatasById.Add(parentGroupData.GroupId, parentGroupData);
                    groupDatasByIndex.Add(parentGroupData.GroupIndex, parentGroupData);
                }
                else
                {
                    hasEncounteredParent = RepeatGroupData.DoGroupNamesMatch(fullGroupName, existingAncestorGroupData.GroupName);
                }
            }

            if ((parentGroupData == null) && (groupNames.Length > 1))
            { throw new InvalidOperationException("Invalid existing ancestor RepeatGroupData specified."); }

            return parentGroupData;
        }

        private void CreateRepeatGroups(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, DimensionalRepeatGroup parentRepeatGroup, Nullable<Guid> currentParentDataId, SortedDictionary<int, RepeatGroupData> groupDatasByIndex, List<string> usedGroupNames, List<DimensionalRepeatGroup> createdGroups)
        {
            ICurrentState reportState = renderingState.ReportState;

            foreach (var groupIndex in groupDatasByIndex.Keys)
            {
                var groupData = groupDatasByIndex[groupIndex];

                if (groupData.ParentGroupId != currentParentDataId)
                { continue; }

                var structuralTitleRangeIds = new List<ReportElementId>();
                var timeTitleRangeIds = new List<ReportElementId>();
                var groupedStructuralTypeRefs = new List<ModelObjectReference>();

                foreach (var rangeId in groupData.GroupedBoxes.Keys)
                {
                    if (renderingState.GroupingState.ProcessedElementIds.Contains(rangeId))
                    { throw new InvalidOperationException("Encountered a Report Element that was handled out of order."); }

                    var range = groupData.GroupedBoxes[rangeId];

                    structuralTitleRangeIds.AddRange(range.ContainedStructuralTitleRangeIds);
                    timeTitleRangeIds.AddRange(range.ContainedTimeTitleRangeIds);

                    var groupedStructuralTypeRef = dataProvider.GetStructuralType(range.VariableTemplateRef);
                    groupedStructuralTypeRefs.Add(groupedStructuralTypeRef);
                }

                bool hasAdditionalNestedStructure = (structuralTitleRangeIds.Count > 0);
                bool hasAdditionalNestedTime = (timeTitleRangeIds.Count > 0);

                var additionalStructuralTypeRefs = parentRepeatGroup.AdditionalStructuralTypeRefs.ToList();
                var relevantTimeDimensions = parentRepeatGroup.RelevantTimeDimensions.ToDictionary(x => x.Key, x => x.Value);
                IStructuralContext structuralContext = null;

                if (hasAdditionalNestedStructure)
                {
                    foreach (var elementId in structuralTitleRangeIds)
                    {
                        var range = elementTree.GetCachedValue<IStructuralTitleRange>(elementId);
                        additionalStructuralTypeRefs.Add(range.EntityTypeRef);
                    }
                }

                if (hasAdditionalNestedTime)
                {
                    foreach (var elementId in timeTitleRangeIds)
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

                var structuralContexts = DimensioningUtils.BuildStructuralContexts(dataProvider, reportState, additionalStructuralTypeRefs);
                if (structuralContexts.Count != 1)
                { throw new InvalidOperationException("The current Report Element does not have a valid Structural Context."); }
                structuralContext = structuralContexts.First().Value;

                var nestedRepeatGroupName = string.Empty;
                if (string.IsNullOrWhiteSpace(groupData.GroupName))
                { nestedRepeatGroupName = DimensionalRepeatGroup.GetDefaultGroupName(this.Name, usedGroupNames); }
                else
                { nestedRepeatGroupName = DimensionalRepeatGroup.GetGroupName(this.Name, groupData.GroupName, usedGroupNames); }

                var nestedRepeatGroup = new DimensionalRepeatGroup(nestedRepeatGroupName, parentRepeatGroup.DesignState, structuralContext, additionalStructuralTypeRefs, relevantTimeDimensions, groupedStructuralTypeRefs);

                parentRepeatGroup.AddNestedRepeatGroup(nestedRepeatGroup);
                usedGroupNames.Add(nestedRepeatGroupName);
                createdGroups.Add(nestedRepeatGroup);

                foreach (var rangeId in groupData.GroupedBoxes.Keys)
                {
                    if (renderingState.GroupingState.ProcessedElementIds.Contains(rangeId))
                    { throw new InvalidOperationException("Encountered a Report Element that was handled out of order."); }

                    var range = groupData.GroupedBoxes[rangeId];
                    nestedRepeatGroup.AddElementToGroup(range);
                }

                CreateRepeatGroups(dataProvider, renderingState, elementTree, nestedRepeatGroup, groupData.GroupId, groupDatasByIndex, usedGroupNames, createdGroups);
            }
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
            var childVariableTitleBoxes = childElementIds.Select(x => renderingState.ReportElements[x]).Where(x => (x is IVariableTitleBox)).Select(x => (x as IVariableTitleBox)).OrderBy(x => x.StackingOrder).ToList();

            var spaceEnumerator = new SpaceEnumerator(this, elementTree);
            var spaceResults = spaceEnumerator.EnumerateResults(dataProvider, reportState);

            var expansionDimension = this.StackingDimension;
            var hasNoDimensions = (spaceEnumerator.ReducedNestedElements.Count <= 0);
            var hasNoResults = !hasNoDimensions && (spaceResults.Count <= 0);

            var variableTitleContainerGroup = renderingState.GroupingState.ElementRepeatGroups[this.Key];
            var initialIterationGroups = variableTitleContainerGroup.NestedRepeatGroups;
            var hasGlobalGroups = (variableTitleContainerGroup.NestedRepeatGroups.Where(x => x.Value.Get_IsGlobalGroup(dataProvider)).Count() > 0);

            if (hasNoResults && !hasGlobalGroups)
            {
                renderingState.DimensionalTable_VariableHeaders = new List<VariableHeaderResult>();
                result.SetRenderedState();
                return result;
            }

            if (hasNoDimensions && !hasGlobalGroups)
            {
                ElementRenderingUtils.SetMinSizeToShowValues(this.ElementLayout, expansionDimension);

                renderingState.DimensionalTable_VariableHeaders = new List<VariableHeaderResult>();
                result.SetRenderedState();
                return result;
            }

            var variableHeaderEnumerator = new VariableHeaderEnumerator(renderingState, this, childVariableTitleBoxes, spaceResults);
            var variableHeaderResults = variableHeaderEnumerator.EnumerateResults(dataProvider, renderingState, elementTree);

            renderingState.DimensionalTable_VariableHeaders = variableHeaderResults;

            if (false)
            {
                var varResultsAsText = string.Empty;
                foreach (var varResult in variableHeaderResults)
                {
                    var varResultAsText = string.Empty;

                    varResultAsText += varResult.VariableTitleBox.Name;

                    for (int i = varResultAsText.Length; i < 50; i++)
                    { varResultAsText += " "; }

                    foreach (var i in varResult.SpaceResult.DimensionResults.Keys)
                    { varResultAsText += varResult.SpaceResult.DimensionResults[i].ObjectToOrderBy + ",   "; }

                    varResultsAsText += varResultAsText + Environment.NewLine;
                }
            }

            foreach (var varResult in variableHeaderResults)
            {
                var spaceResult = varResult.SpaceResult;
                var dimensionResults = spaceResult.DimensionResults;
                var variableTitleRange = varResult.VariableTitleBox;
                var nestedRenderingState = new RenderingState(renderingState);

                var structuralValues = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);
                var variableValues = new Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, ModelObjectReference>>(ModelObjectReference.DimensionalComparer);
                var timeKey = MultiTimePeriodKey.DimensionlessTimeKey;

                if (varResult.RepeatGroup.Get_IsGlobalGroup(dataProvider) && spaceResult.IsGlobal)
                {
                    var structuralTypeRef = ModelObjectReference.GlobalTypeReference;
                    var structuralInstanceRef = dataProvider.StructuralMap.GetStructuralInstancesForType(reportState.NullableModelInstanceRef.Value, structuralTypeRef).First();

                    structuralValues[structuralTypeRef] = structuralInstanceRef;

                    var variableTemplateRef = dataProvider.DependencyMap.GetNameVariableTemplate(structuralTypeRef);
                    var variableInstanceRef = dataProvider.GetChildVariableInstanceReference(reportState.ModelInstanceRef, variableTemplateRef, structuralInstanceRef);

                    variableValues[structuralTypeRef] = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);
                    variableValues[structuralTypeRef][variableTemplateRef] = variableInstanceRef;
                }
                else
                {
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
                }

                nestedRenderingState.CurrentStructuralBindings = structuralValues;
                nestedRenderingState.CurrentVariableBindings = variableValues;
                nestedRenderingState.CurrentTimeBindings = timeKey;
                nestedRenderingState.UpdateProductionStateFromBindings();

                nestedRenderingState.OverriddenContentGroups[expansionDimension] = varResult.OrderNumber;

                var childElement = varResult.VariableTitleBox;
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