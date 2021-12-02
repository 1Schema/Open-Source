using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class VariableHeaderEnumerator
    {
        protected IRenderingState m_RenderingState;
        protected IVariableTitleContainer m_RootContainer;
        protected Dictionary<ReportElementId, IVariableTitleBox> m_VariableTitleBoxes;
        protected SortedDictionary<int, SpaceResult> m_SpaceResults;

        protected ITree<Guid> m_GroupTree;
        protected Dictionary<Guid, SortedDictionary<int, IterationBlock>> m_GroupIterationBlocks;

        public VariableHeaderEnumerator(IRenderingState renderingState, IVariableTitleContainer rootContainer, IEnumerable<IVariableTitleBox> variableTitleBoxes, IEnumerable<SpaceResult> spaceResults)
        {
            m_RenderingState = renderingState;
            m_RootContainer = rootContainer;
            m_VariableTitleBoxes = variableTitleBoxes.ToDictionary(x => x.Key, x => x, ReportRenderingEngine.EqualityComparer_ReportElementId);
            m_SpaceResults = new SortedDictionary<int, SpaceResult>(spaceResults.ToDictionary(x => x.OriginalResultIndex, x => x));

            m_GroupTree = null;
            m_GroupIterationBlocks = null;
        }

        public IRenderingState RenderingState
        {
            get { return m_RenderingState; }
        }

        public ReportElementId RootContainerId
        {
            get { return m_RootContainer.Key; }
        }

        public List<SpaceResult> SpaceResults
        {
            get { return m_SpaceResults.Values.ToList(); }
        }

        public bool HasStartedEnumeration
        {
            get { return (m_GroupTree != null); }
        }

        public List<VariableHeaderResult> EnumerateResults(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            var rootContainerGroup = RenderingState.GroupingState.ElementRepeatGroups[RootContainerId];
            var nestedTitleGroups = rootContainerGroup.NestedRepeatGroups.Values.ToList();

            var groupSubTreeIds = new Dictionary<Guid, Guid?>();
            var relevantGroups = new Dictionary<Guid, DimensionalRepeatGroup>();

            ProcessGroups(rootContainerGroup, groupSubTreeIds, relevantGroups);
            m_GroupTree = new CachedTree<Guid>(groupSubTreeIds);

            foreach (var groupId in relevantGroups.Keys)
            {
                m_GroupTree.SetCachedValue(groupId, relevantGroups[groupId]);
            }

            var groupIterationBlocks = new Dictionary<Guid, SortedDictionary<int, IterationBlock>>();
            BuildIterationBlocks(dataProvider, renderingState, SpaceResults, nestedTitleGroups, null, groupIterationBlocks);
            m_GroupIterationBlocks = groupIterationBlocks;

            var orderedTitleBoxes = m_VariableTitleBoxes.Values.OrderBy(x => x.StackingOrder).ToList();

            int currentPosition = 0;
            var processedGroupIds = new List<Guid>();
            var varResults = new List<VariableHeaderResult>();

            var orderedRootGroups = m_GroupTree.RootNodes.Select(x => m_GroupTree.GetCachedValue<DimensionalRepeatGroup>(x)).OrderBy(x => x.Number).ToList();

            foreach (var rootGroup in orderedRootGroups)
            {
                var rootGroupVarResults = new List<VariableHeaderResult>();
                var handledBlockTitleRangeIds = new Dictionary<MultiPartKey<Guid, int>, List<ReportElementId>>();

                var rootBlocks = m_GroupIterationBlocks[rootGroup.Id];
                var hasBlocks = (rootBlocks.Count > 0);

                if (hasBlocks)
                {
                    foreach (var iterationBlock in rootBlocks)
                    {
                        GenerateResultsForGroupTree(dataProvider, elementTree, rootGroup, iterationBlock.Value, ref currentPosition, orderedTitleBoxes, handledBlockTitleRangeIds, rootGroupVarResults);
                    }
                }
                else if (!hasBlocks && rootGroup.Get_IsGlobalGroup(dataProvider))
                {
                    var spaceResult = new SpaceResult(null, 0);
                    var iterationBlock = new IterationBlock(rootGroup.Id, 0, null, new SpaceResult[] { spaceResult });
                    GenerateResultsForGroupTree(dataProvider, elementTree, rootGroup, iterationBlock, ref currentPosition, orderedTitleBoxes, handledBlockTitleRangeIds, rootGroupVarResults);
                }

                varResults.AddRange(rootGroupVarResults);
            }
            return varResults;
        }

        private void GenerateResultsForGroupTree(IReportingDataProvider dataProvider, ITree<ReportElementId> elementTree, DimensionalRepeatGroup currentGroup, IterationBlock currentBlock, ref int currentPosition, List<IVariableTitleBox> parentTitleBoxes, Dictionary<MultiPartKey<Guid, int>, List<ReportElementId>> handledBlockTitleRangeIds, List<VariableHeaderResult> varResults)
        {
            var containedElementIds = GetContainedElements(currentGroup);
            var currentTitleBoxes = parentTitleBoxes.Where(x => containedElementIds.ContainsKey(x.Key)).OrderBy(x => x.StackingOrder).ToList();
            var directTitleBoxes = currentGroup.GroupedElements.Values.Where(x => (x is IVariableTitleBox)).Select(x => (x as IVariableTitleBox)).ToList();
            var orderedNestedGroups = currentGroup.NestedRepeatGroups.Values.OrderBy(x => x.Number).ToList();

            var groupStructuralSpace = currentGroup.DesignContext.ResultingSpace;
            var groupTimeSpace = currentGroup.RelevantTimeDimensions;
            var uniqueSpaceResults = new List<SpaceResult>();

            foreach (var spaceResult in currentBlock.Results.Values)
            {
                if (IsResultRepeat(groupStructuralSpace, groupTimeSpace, spaceResult, uniqueSpaceResults))
                { continue; }
                uniqueSpaceResults.Add(spaceResult);
            }

            foreach (var spaceResult in uniqueSpaceResults)
            {
                foreach (var titleBox in currentTitleBoxes)
                {
                    if (currentGroup.GroupedElementIds.Contains(titleBox.Key))
                    {
                        var resultId = new MultiPartKey<Guid, int>(currentBlock.BlockId, spaceResult.OriginalResultIndex);

                        if (handledBlockTitleRangeIds.ContainsKey(resultId))
                        {
                            if (handledBlockTitleRangeIds[resultId].Contains(titleBox.Key))
                            { continue; }
                        }

                        var varResult = new VariableHeaderResult(currentPosition, titleBox, elementTree, currentGroup, currentBlock, spaceResult);
                        varResults.Add(varResult);
                        currentPosition++;

                        if (!handledBlockTitleRangeIds.ContainsKey(resultId))
                        { handledBlockTitleRangeIds.Add(resultId, new List<ReportElementId>()); }
                        handledBlockTitleRangeIds[resultId].Add(titleBox.Key);

                    }
                    else
                    {
                        var nestedGroupId = containedElementIds[titleBox.Key];
                        var nestedGroup = m_GroupTree.GetCachedValue<DimensionalRepeatGroup>(nestedGroupId);

                        var nestedBlocks = m_GroupIterationBlocks[nestedGroupId].ToList();
                        nestedBlocks = nestedBlocks.Where(x => x.Value.ParentBlock == currentBlock).ToList();
                        var hasBlocks = (nestedBlocks.Count > 0);

                        if (hasBlocks)
                        {
                            foreach (var nestedBlock in nestedBlocks)
                            {
                                GenerateResultsForGroupTree(dataProvider, elementTree, nestedGroup, nestedBlock.Value, ref currentPosition, currentTitleBoxes, handledBlockTitleRangeIds, varResults);
                            }
                        }
                        else if (!hasBlocks && (currentGroup.Get_IsGlobalGroup(dataProvider) && nestedGroup.Get_IsGlobalGroup(dataProvider)))
                        {
                            var iterationBlock = new IterationBlock(nestedGroup.Id, 0, null, new SpaceResult[] { spaceResult });
                            GenerateResultsForGroupTree(dataProvider, elementTree, nestedGroup, iterationBlock, ref currentPosition, currentTitleBoxes, handledBlockTitleRangeIds, varResults);
                        }
                    }
                }
            }
        }

        #region Static Methods

        private static void ProcessGroups(DimensionalRepeatGroup repeatGroup, IDictionary<Guid, Guid?> childParentIds, IDictionary<Guid, DimensionalRepeatGroup> groups)
        {
            foreach (var nestedRepeatGroup in repeatGroup.NestedRepeatGroups.Values)
            {
                var parentGroupId = childParentIds.ContainsKey(repeatGroup.Id) ? repeatGroup.Id : (Nullable<Guid>)null;

                childParentIds.Add(nestedRepeatGroup.Id, parentGroupId);
                groups.Add(nestedRepeatGroup.Id, nestedRepeatGroup);

                ProcessGroups(nestedRepeatGroup, childParentIds, groups);
            }
        }

        private static void BuildIterationBlocks(IReportingDataProvider dataProvider, IRenderingState renderingState, List<SpaceResult> spaceResults, IEnumerable<DimensionalRepeatGroup> repeatGroupsToHandle, IterationBlock parentGroupIterationBlock, Dictionary<Guid, SortedDictionary<int, IterationBlock>> groupIterationBlocks)
        {
            var iterationStartIndex = (parentGroupIterationBlock == null) ? 0 : parentGroupIterationBlock.StartIndex;
            var maxIterationIndex = (parentGroupIterationBlock == null) ? (spaceResults.Count - 1) : (parentGroupIterationBlock.Count - 1);

            var allEnumerators = new SortedDictionary<int, DimensionEnumerator>();
            if (spaceResults.Count > 0)
            { allEnumerators = spaceResults.First().ParentEnumerator.DimensionEnumerators; }

            foreach (var currentGroup in repeatGroupsToHandle)
            {
                if (!groupIterationBlocks.ContainsKey(currentGroup.Id))
                { groupIterationBlocks.Add(currentGroup.Id, new SortedDictionary<int, IterationBlock>()); }

                var groupSpace = currentGroup.DesignContext.ResultingSpace;
                var groupEntityTypeRefs = groupSpace.Dimensions.Select(x => x.EntityTypeRefWithDimNum).ToList();
                var groupTimeDims = currentGroup.RelevantTimeDimensions.Where(x => x.Value.HasValue).Select(x => x.Key).ToList();
                var groupEnumerators = new SortedDictionary<int, DimensionEnumerator>();

                foreach (var eumeratorIndex in allEnumerators.Keys)
                {
                    var enumerator = allEnumerators[eumeratorIndex];

                    if (enumerator.DimensionType == DimensionType.Structure)
                    {
                        if (groupEntityTypeRefs.Contains(enumerator.DimensionRef))
                        {
                            groupEnumerators.Add(groupEnumerators.Count, enumerator);
                        }
                    }
                    else if (enumerator.DimensionType == DimensionType.Time)
                    {
                        if (groupTimeDims.Contains(enumerator.TimeDimensionType.Value))
                        {
                            groupEnumerators.Add(groupEnumerators.Count, enumerator);
                        }
                    }
                    else
                    { throw new InvalidOperationException("Unsupported Dimension Type encountered."); }
                }

                var groupOrderedResults = (parentGroupIterationBlock == null) ? spaceResults : parentGroupIterationBlock.Results.Values.ToList();

                var groupOrderComparer = new SpaceResultComparer();
                groupOrderedResults = groupOrderedResults.Select(x => x.GetReorganizedResult(groupEnumerators)).ToList();
                groupOrderedResults = groupOrderedResults.OrderBy(x => x, groupOrderComparer).ToList();

                var iterationIndex = iterationStartIndex;
                var blockStartIndex = iterationStartIndex;

                var blockResults = new List<SpaceResult>();
                var blockNumber = (groupIterationBlocks[currentGroup.Id].Count < 1) ? 0 : groupIterationBlocks[currentGroup.Id].Keys.Max() + 1;

                while (iterationIndex <= maxIterationIndex)
                {
                    if (iterationIndex < blockStartIndex)
                    { throw new InvalidOperationException("The Iteration Index should never be less than the Block Start Index."); }

                    var currentResult = groupOrderedResults[iterationIndex];
                    var iterationBlock = (IterationBlock)null;

                    bool isValid = ValidationUtils.ValidateSpaceResultForGroup(dataProvider, renderingState, currentGroup.GroupedStructuralTypeRefs, currentResult);

                    if (!isValid)
                    {
                        if ((iterationIndex == maxIterationIndex) && (blockResults.Count > 0))
                        {
                            iterationBlock = new IterationBlock(currentGroup.Id, blockNumber, parentGroupIterationBlock, blockResults);
                            groupIterationBlocks[currentGroup.Id].Add(blockNumber, iterationBlock);

                            blockResults = new List<SpaceResult>();
                            blockNumber++;
                        }
                    }
                    else if (iterationIndex == maxIterationIndex)
                    {
                        blockResults.Add(currentResult);
                        iterationBlock = new IterationBlock(currentGroup.Id, blockNumber, parentGroupIterationBlock, blockResults);
                        groupIterationBlocks[currentGroup.Id].Add(blockNumber, iterationBlock);

                        blockResults = new List<SpaceResult>();
                        blockNumber++;
                    }
                    else
                    {
                        var nextResult = groupOrderedResults[iterationIndex + 1];
                        var requiresNewIterationBlock = false;

                        var currentResultDimensions = currentResult.DimensionResults.Values.ToDictionary(x => x.DimensionRef, ModelObjectReference.DimensionalComparer);
                        var nextResultDimensions = nextResult.DimensionResults.Values.ToDictionary(x => x.DimensionRef, ModelObjectReference.DimensionalComparer);

                        foreach (var structuralDimension in groupSpace.Dimensions)
                        {
                            if (!currentResultDimensions.ContainsKey(structuralDimension.EntityTypeRef))
                            { continue; }
                            if (!nextResultDimensions.ContainsKey(structuralDimension.EntityTypeRef))
                            { continue; }

                            var currentResultDimension = currentResultDimensions[structuralDimension.EntityTypeRef];
                            var nextResultDimension = nextResultDimensions[structuralDimension.EntityTypeRef];

                            if (currentResultDimension.StructuralInstanceRef != nextResultDimension.StructuralInstanceRef)
                            { requiresNewIterationBlock = true; }
                        }

                        if (!requiresNewIterationBlock)
                        {
                            var timeDimensions = currentGroup.RelevantTimeDimensions;
                            foreach (var timeDimension in timeDimensions.Keys)
                            {
                                if (!timeDimensions[timeDimension].HasValue)
                                { continue; }

                                var timeDimensionRef = new ModelObjectReference(timeDimension);

                                var currentResultDimension = currentResultDimensions[timeDimensionRef];
                                var nextResultDimension = nextResultDimensions[timeDimensionRef];

                                if (currentResultDimension.PeriodToDisplay != nextResultDimension.PeriodToDisplay)
                                { requiresNewIterationBlock = true; }
                            }
                        }

                        blockResults.Add(currentResult);

                        if (requiresNewIterationBlock)
                        {
                            iterationBlock = new IterationBlock(currentGroup.Id, blockNumber, parentGroupIterationBlock, blockResults);
                            groupIterationBlocks[currentGroup.Id].Add(blockNumber, iterationBlock);

                            blockResults = new List<SpaceResult>();
                            blockNumber++;
                        }
                    }

                    if (iterationBlock != null)
                    {
                        BuildIterationBlocks(dataProvider, renderingState, groupOrderedResults, currentGroup.NestedRepeatGroups.Values, iterationBlock, groupIterationBlocks);
                    }

                    iterationIndex++;
                }
            }
        }

        private static Dictionary<ReportElementId, Guid> GetContainedElements(DimensionalRepeatGroup currentGroup)
        {
            var elementIds = new Dictionary<ReportElementId, Guid>(ReportRenderingEngine.EqualityComparer_ReportElementId);
            GetContainedElements(currentGroup, elementIds);
            return elementIds;
        }

        private static void GetContainedElements(DimensionalRepeatGroup currentGroup, Dictionary<ReportElementId, Guid> elementIds)
        {
            foreach (var elementId in currentGroup.GroupedElementIds)
            {
                elementIds.Add(elementId, currentGroup.Id);
            }

            foreach (var nestedGroup in currentGroup.NestedRepeatGroups.Values)
            {
                var nestedElementIds = GetContainedElements(nestedGroup);
                foreach (var elementId in nestedElementIds.Keys)
                {
                    elementIds.Add(elementId, nestedGroup.Id);
                }
            }
        }

        private static bool IsResultRepeat(StructuralSpace structuralSpace, IDictionary<TimeDimensionType, TimePeriodType?> timeSpace, SpaceResult currentResult, IEnumerable<SpaceResult> previousResults)
        {
            foreach (var previousResult in previousResults)
            {
                if (IsResultRepeat(structuralSpace, timeSpace, currentResult, previousResult))
                { return true; }
            }
            return false;
        }

        private static bool IsResultRepeat(StructuralSpace structuralSpace, IDictionary<TimeDimensionType, TimePeriodType?> timeSpace, SpaceResult currentResult, SpaceResult previousResult)
        {
            var previousDimensionResults = previousResult.DimensionResults.Values.ToList();

            foreach (var dimensionResult in currentResult.DimensionResults.Values)
            {
                if (dimensionResult.DimensionType == DimensionType.Structure)
                {
                    var isRelevant = false;
                    foreach (var dimension in structuralSpace.Dimensions)
                    {
                        if (ModelObjectReference.DimensionalComparer.Equals(dimension.EntityTypeRefWithDimNum, dimensionResult.DimensionRef))
                        {
                            isRelevant = true;
                            break;
                        }
                    }

                    if (!isRelevant)
                    { continue; }

                    foreach (var previousDimensionResult in previousDimensionResults)
                    {
                        if (previousDimensionResult.DimensionType != DimensionType.Structure)
                        { continue; }
                        if (!ModelObjectReference.DimensionalComparer.Equals(dimensionResult.DimensionRef, previousDimensionResult.DimensionRef))
                        { continue; }

                        var same_NoDim = ModelObjectReference.DefaultComparer.Equals(dimensionResult.StructuralInstanceRef.Value, previousDimensionResult.StructuralInstanceRef.Value);
                        var same_WithDim = ModelObjectReference.DimensionalComparer.Equals(dimensionResult.StructuralInstanceRef.Value, previousDimensionResult.StructuralInstanceRef.Value);

                        if (same_NoDim ^ same_WithDim)
                        { throw new InvalidOperationException("Alternate Dimensionality should not change result."); }

                        if (!same_WithDim)
                        { return false; }

                        previousDimensionResults.Remove(previousDimensionResult);
                        break;
                    }
                }
                else if (dimensionResult.DimensionType == DimensionType.Time)
                {
                    var isRelevant = dimensionResult.TimeDimensionType.HasValue ? timeSpace.ContainsKey(dimensionResult.TimeDimensionType.Value) : false;
                    if (isRelevant)
                    { isRelevant = timeSpace[dimensionResult.TimeDimensionType.Value].HasValue; }

                    if (!isRelevant)
                    { continue; }

                    foreach (var previousDimensionResult in previousDimensionResults)
                    {
                        if (previousDimensionResult.DimensionType != DimensionType.Time)
                        { continue; }
                        if (dimensionResult.TimeDimensionType != previousDimensionResult.TimeDimensionType)
                        { continue; }

                        if (dimensionResult.PeriodToDisplay != previousDimensionResult.PeriodToDisplay)
                        { return false; }

                        previousDimensionResults.Remove(previousDimensionResult);
                        break;
                    }
                }
                else
                { throw new InvalidOperationException("The specified Dimension Type is not yet supported."); }
            }
            return true;
        }

        #endregion
    }
}