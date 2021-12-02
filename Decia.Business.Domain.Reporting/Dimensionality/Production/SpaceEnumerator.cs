using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class SpaceEnumerator
    {
        public SpaceEnumerator(IDimensionalContainer sourceElement, ITree<ReportElementId> elementTree)
        {
            SourceElement = sourceElement;
            NestedElements = GetNestedElements(sourceElement, elementTree);
            DimensionEnumerators = new SortedDictionary<int, DimensionEnumerator>();

            var nestedElementDimensions = new Dictionary<ReportElementId, ModelObjectReference>(ReportRenderingEngine.EqualityComparer_ReportElementId);
            DuplicateNestedElements = new Dictionary<ModelObjectReference, List<ReportElementId>>(ModelObjectReference.DimensionalComparer);

            foreach (IDimensionalRange nestedElement in NestedElements.Values)
            {
                var dimensionTypeRef = nestedElement.DimensionalTypeRef;
                nestedElementDimensions.Add(nestedElement.Key, dimensionTypeRef);

                if (!DuplicateNestedElements.ContainsKey(dimensionTypeRef))
                { DuplicateNestedElements.Add(dimensionTypeRef, new List<ReportElementId>()); }

                DuplicateNestedElements[dimensionTypeRef].Add(nestedElement.Key);
            }

            var wasDimensionHandled = new HashSet<ModelObjectReference>(ModelObjectReference.DimensionalComparer);
            ReducedNestedElements = new Dictionary<ReportElementId, IDimensionalRange>(ReportRenderingEngine.EqualityComparer_ReportElementId);

            var dimensionalOrder = SourceElement.SortOrderedDimensions;

            foreach (var sortOrder in dimensionalOrder.Keys)
            {
                var dimensionTypeRef = dimensionalOrder[sortOrder];

                if (wasDimensionHandled.Contains(dimensionTypeRef))
                { continue; }

                var nestedElementIds = DuplicateNestedElements[dimensionTypeRef];

                if (nestedElementIds.Count < 1)
                { throw new InvalidOperationException("Ordered Dimensions must have corresponging DimensionalRanges."); }

                var nestedElementId = nestedElementIds.First();
                var nestedElement = elementTree.GetCachedValue<IDimensionalRange>(nestedElementId);

                var dimensionEnumerator = new DimensionEnumerator(nestedElement, dimensionTypeRef, sortOrder);

                ReducedNestedElements.Add(nestedElement.Key, nestedElement);
                DimensionEnumerators.Add(sortOrder, dimensionEnumerator);

                wasDimensionHandled.Add(dimensionTypeRef);
            }
        }

        public IDimensionalContainer SourceElement { get; protected set; }
        public Dictionary<ReportElementId, IDimensionalRange> NestedElements { get; protected set; }

        public Dictionary<ModelObjectReference, List<ReportElementId>> DuplicateNestedElements { get; protected set; }
        public Dictionary<ReportElementId, IDimensionalRange> ReducedNestedElements { get; protected set; }

        public SortedDictionary<int, DimensionEnumerator> DimensionEnumerators { get; protected set; }

        public ReportId ReportId { get { return ElementId.ReportId; } }
        public ReportElementId ElementId { get { return SourceElement.Key; } }

        private static Dictionary<ReportElementId, IDimensionalRange> GetNestedElements(IDimensionalContainer sourceElement, ITree<ReportElementId> elementTree)
        {
            if (sourceElement is ICommonTitleContainer)
            {
                var sourceElementAsCommonTitleContainer = (sourceElement as ICommonTitleContainer);
                var childElementIds = sourceElementAsCommonTitleContainer.ChildIds.ToList();
                var childElements = childElementIds.Select(x => elementTree.GetCachedValue<IReportElement>(x)).Where(x => (x is ICommonTitleBox)).ToList();

                var nestedElements = new Dictionary<ReportElementId, IDimensionalRange>(ReportRenderingEngine.EqualityComparer_ReportElementId);
                foreach (ICommonTitleBox childElement in childElements)
                {
                    var childNestedElementIds = new List<ReportElementId>();
                    childNestedElementIds.AddRange(childElement.ContainedStructuralTitleRangeIds);
                    childNestedElementIds.AddRange(childElement.ContainedTimeTitleRangeIds);

                    var childNestedElements = childNestedElementIds.Select(x => elementTree.GetCachedValue<IDimensionalRange>(x)).ToList();

                    foreach (var nestedElement in childNestedElements)
                    { nestedElements.Add(nestedElement.Key, nestedElement); }
                }
                return nestedElements;
            }
            else if (sourceElement is IVariableTitleContainer)
            {
                var sourceElementAsVariableTitleContainer = (sourceElement as IVariableTitleContainer);
                var childElementIds = sourceElementAsVariableTitleContainer.ChildIds.ToList();
                var childElements = childElementIds.Select(x => elementTree.GetCachedValue<IReportElement>(x)).Where(x => (x is IVariableTitleBox)).ToList();

                var nestedElements = new Dictionary<ReportElementId, IDimensionalRange>(ReportRenderingEngine.EqualityComparer_ReportElementId);
                foreach (IVariableTitleBox childElement in childElements)
                {
                    var childNestedElementIds = new List<ReportElementId>();
                    childNestedElementIds.AddRange(childElement.ContainedStructuralTitleRangeIds);
                    childNestedElementIds.AddRange(childElement.ContainedTimeTitleRangeIds);

                    var childNestedElements = childNestedElementIds.Select(x => elementTree.GetCachedValue<IDimensionalRange>(x)).ToList();

                    foreach (var nestedElement in childNestedElements)
                    { nestedElements.Add(nestedElement.Key, nestedElement); }
                }
                return nestedElements;
            }
            else
            { throw new InvalidOperationException("Unrecognized IDimensionalContainer encountered."); }
        }

        public List<SpaceResult> EnumerateResults(IReportingDataProvider dataProvider, ICurrentState reportState)
        {
            var dimensionalResults = new SortedDictionary<int, List<DimensionResult>>();
            var dimensionalIndices = new SortedDictionary<int, int>();
            var dimensionalDivisors = new Dictionary<int, int>();
            var spaceResults = new List<SpaceResult>();

            if (DimensionEnumerators.Count < 1)
            { return spaceResults; }

            foreach (int sortOrder in DimensionEnumerators.Keys)
            {
                var dimensionEnumerator = DimensionEnumerators[sortOrder];
                var dimensionalResult = dimensionEnumerator.EnumerateResults(dataProvider, reportState);

                dimensionalResult = dimensionalResult.OrderBy(x => x.ObjectToOrderBy, dimensionEnumerator.GetTypedComparer<object>()).ToList();

                dimensionalResults.Add(sortOrder, dimensionalResult);
                dimensionalIndices.Add(sortOrder, 0);
            }

            var divisor = 1;
            var reversedDimensionKeys = DimensionEnumerators.Keys.Reverse().ToList();
            var firstDimensionKey = DimensionEnumerators.Keys.First();
            var firstDivisor = divisor;

            foreach (var dimensionKey in reversedDimensionKeys)
            {
                dimensionalDivisors.Add(dimensionKey, divisor);

                if (dimensionKey == firstDimensionKey)
                { firstDivisor = divisor; }
                else
                { divisor *= dimensionalResults[dimensionKey].Count; }
            }

            var maxIterationCount = (firstDivisor * dimensionalResults[firstDimensionKey].Count);

            var iterationIndex = 0;
            var structuralValues = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);
            var timeKey = MultiTimePeriodKey.DimensionlessTimeKey;

            while (iterationIndex < maxIterationCount)
            {
                var iterationResult = new SortedDictionary<int, DimensionResult>();

                foreach (var dimensionKey in dimensionalIndices.Keys.ToList())
                {
                    var dimensionalEnumerator = DimensionEnumerators[dimensionKey];

                    var currentDivisor = dimensionalDivisors[dimensionKey];
                    var currentIndex = iterationIndex / currentDivisor;
                    dimensionalIndices[dimensionKey] = currentIndex;

                    var dimensionResults = dimensionalResults[dimensionKey];
                    var updatedIndex = currentIndex % dimensionResults.Count;
                    var dimensionResult = dimensionResults[updatedIndex];

                    iterationResult.Add(dimensionKey, dimensionResult);
                }

                var spaceResult = new SpaceResult(this, iterationIndex, iterationResult.Values);
                spaceResults.Add(spaceResult);
                iterationIndex++;
            }

            return spaceResults;
        }
    }
}