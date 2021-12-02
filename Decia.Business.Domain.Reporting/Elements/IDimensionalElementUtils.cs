using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Reporting
{
    public static class IDimensionalElementUtils
    {
        public static void ValidateDimensionsForContainer(this IDimensionalContainer container, IDimensionalBox box, IDictionary<ReportElementId, IReportElement> allElements)
        {
            ValidateDimensionsForContainer(container, new List<IDimensionalBox>() { box }, allElements);
        }

        public static void ValidateDimensionsForContainer(this IDimensionalContainer container, IEnumerable<IDimensionalBox> boxes, IDictionary<ReportElementId, IReportElement> allElements)
        {
            var dimensionSortOrders = container.DimensionSortOrders;
            var dimensionsEncountered = dimensionSortOrders.ToDictionary(x => x.Key, x => false, ModelObjectReference.DimensionalComparer);

            foreach (var box in boxes)
            {
                var dimensionalRanges = new List<IDimensionalRange>();
                dimensionalRanges.AddRange(box.ContainedStructuralTitleRangeIds.Select(x => (IDimensionalRange)allElements[x]).ToList());
                dimensionalRanges.AddRange(box.ContainedTimeTitleRangeIds.Select(x => (IDimensionalRange)allElements[x]).ToList());

                foreach (var dimensionalRange in dimensionalRanges)
                {
                    var dimensionTypeRef = dimensionalRange.DimensionalTypeRef;

                    if (dimensionsEncountered.ContainsKey(dimensionTypeRef))
                    {
                        dimensionsEncountered[dimensionTypeRef] = true;
                        continue;
                    }

                    container.SetToMaxSortOrder(dimensionTypeRef);
                }
            }

            foreach (var dimensionTypeRef in dimensionsEncountered.Keys)
            {
                if (dimensionsEncountered[dimensionTypeRef])
                { continue; }

                container.RemoveSortOrder(dimensionTypeRef);
            }
        }

        public static bool DoContainersHaveOverlappingDimensions(this IDimensionalContainer firstContainer, IDimensionalContainer secondContainer)
        {
            var refComparer = ModelObjectReference.DimensionalComparer;

            var firstDimensions = new HashSet<ModelObjectReference>(firstContainer.DimensionSortOrders.Keys, refComparer);
            var secondDimensions = new HashSet<ModelObjectReference>(secondContainer.DimensionSortOrders.Keys, refComparer);
            var allDimensions = new HashSet<ModelObjectReference>(firstDimensions.Union(secondDimensions, refComparer), refComparer);

            var expectedDimensionCount = (firstDimensions.Count + secondDimensions.Count);
            var actualDimensionCount = allDimensions.Count;

            return (expectedDimensionCount != actualDimensionCount);
        }

        public static void AssertContainersDoNotHaveOverlappingDimensions(this IDimensionalContainer firstContainer, IDimensionalContainer secondContainer)
        {
            if (DoContainersHaveOverlappingDimensions(firstContainer, secondContainer))
            { throw new InvalidOperationException("Dimensions must not be present in both DimensionalContainers."); }
        }
    }
}