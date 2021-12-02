using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using SDT = Decia.Business.Common.Structure.StructuralDimensionType;
using SDTUtils = Decia.Business.Common.Structure.StructuralDimensionTypeUtils;

namespace Decia.Business.Common.Structure
{
    public static class StructuralSpaceUtils
    {
        public const bool DefaultAllowAsGeneralToReturnTrue = true;

        public static StructuralSpace GetMissing(this StructuralSpace space, StructuralSpace otherSpace, out bool hasMatches)
        {
            HashSet<StructuralDimension> missing = new HashSet<StructuralDimension>();
            hasMatches = false;

            foreach (StructuralDimension dimension in space.Dimensions)
            {
                if (!otherSpace.Dimensions.Contains(dimension))
                { missing.Add(dimension); }
                else
                { hasMatches = true; }
            }
            return new StructuralSpace(missing);
        }

        public static StructuralSpace GetReduced(this StructuralSpace space, IStructuralMap structuralMap, bool useExtendedStructure)
        {
            HashSet<StructuralDimension> reducedDimensions = new HashSet<StructuralDimension>(space.Dimensions);
            HashSet<StructuralDimension> dimensionsToProcess = new HashSet<StructuralDimension>(reducedDimensions);

            while (dimensionsToProcess.Count > 0)
            {
                StructuralDimension currentDimension = dimensionsToProcess.First();
                var currentDescendants = structuralMap.GetDescendantsWithConnectingDimensions(currentDimension, useExtendedStructure);

                IEnumerable<StructuralDimension> currentDescendantDimensions = currentDescendants.Keys.Select(descrf => new StructuralDimension(descrf.ModelObjectIdAsInt, currentDimension.EntityDimensionNumber, currentDescendants[descrf].DimensionType, currentDescendants[descrf].CorrespondingVariableRef)).ToList();

                foreach (StructuralDimension descendantDimension in currentDescendantDimensions)
                {
                    if (!reducedDimensions.Contains(descendantDimension))
                    { continue; }

                    reducedDimensions.Remove(currentDimension);
                    break;
                }

                dimensionsToProcess.Remove(currentDimension);
            }
            return new StructuralSpace(reducedDimensions);
        }

        public static StructuralSpace GetMaximal(this StructuralSpace space, IStructuralMap structuralMap, bool useExtendedStructure)
        {
            HashSet<StructuralDimension> maximalDimensions = new HashSet<StructuralDimension>(space.Dimensions);
            HashSet<StructuralDimension> dimensionsToProcess = new HashSet<StructuralDimension>(maximalDimensions);

            while (dimensionsToProcess.Count > 0)
            {
                StructuralDimension currentDimension = dimensionsToProcess.First();
                var currentAnscestors = structuralMap.GetAncestorsWithConnectingDimensions(currentDimension, useExtendedStructure);

                IEnumerable<StructuralDimension> currentAncestorDimensions = currentAnscestors.Keys.Select(ancrf => new StructuralDimension(ancrf.ModelObjectIdAsInt, currentDimension.EntityDimensionNumber, currentAnscestors[ancrf].DimensionType, currentAnscestors[ancrf].CorrespondingVariableRef)).ToList();

                foreach (StructuralDimension ancestorDimension in currentAncestorDimensions)
                {
                    if (maximalDimensions.Contains(ancestorDimension))
                    { continue; }

                    maximalDimensions.Add(ancestorDimension);
                    dimensionsToProcess.Add(ancestorDimension);
                }

                dimensionsToProcess.Remove(currentDimension);
            }
            return new StructuralSpace(maximalDimensions);
        }

        public static Nullable<StructuralSpace> GetBridge(this StructuralSpace space, IStructuralMap structuralMap, ModelObjectReference mainRef, ModelObjectReference relatedRef, bool useExtendedStructure)
        {
            var relatedRefs = new ModelObjectReference[] { relatedRef };
            return GetBridge(space, structuralMap, mainRef, relatedRefs, useExtendedStructure);
        }

        public static Nullable<StructuralSpace> GetBridge(this StructuralSpace space, IStructuralMap structuralMap, ModelObjectReference mainRef, IEnumerable<ModelObjectReference> relatedRefs, bool useExtendedStructure)
        {
            StructuralSpace reducedSpace = space.GetReduced(structuralMap, useExtendedStructure);

            if (structuralMap.IsDirectlyAccessibleUsingSpace(mainRef, relatedRefs, reducedSpace, useExtendedStructure))
            { return reducedSpace; }

            HashSet<StructuralDimension> dimensionsToProcess = new HashSet<StructuralDimension>(reducedSpace.Dimensions);
            HashSet<StructuralDimension> handledDimensions = new HashSet<StructuralDimension>();
            StructuralSpace lastSpace = reducedSpace;
            List<StructuralSpace> bridgingSpaces = new List<StructuralSpace>();

            ExpandBridgeByDimension(structuralMap, mainRef, relatedRefs, useExtendedStructure, dimensionsToProcess, handledDimensions, lastSpace, bridgingSpaces);

            if (bridgingSpaces.Count < 1)
            { return null; }

            var minSpace = bridgingSpaces.OrderBy(bs => bs.Dimensions.Count).First();
            return minSpace;
        }

        private static void ExpandBridgeByDimension(IStructuralMap structuralMap, ModelObjectReference mainRef, IEnumerable<ModelObjectReference> relatedRefs, bool useExtendedStructure, HashSet<StructuralDimension> dimensionsToProcess, HashSet<StructuralDimension> handledDimensions, StructuralSpace lastSpace, List<StructuralSpace> bridgingSpaces)
        {
            foreach (var currentDimension in dimensionsToProcess)
            {
                if (handledDimensions.Contains(currentDimension))
                { continue; }

                StructuralSpace tempSpace = lastSpace;

                if (!lastSpace.Dimensions.Contains(currentDimension))
                {
                    tempSpace = lastSpace.Merge(currentDimension);

                    if (structuralMap.IsDirectlyAccessibleUsingSpace(mainRef, relatedRefs, tempSpace, useExtendedStructure))
                    {
                        bridgingSpaces.Add(tempSpace);
                        return;
                    }
                }

                var currentAnscestors = structuralMap.GetAncestorsWithConnectingDimensions(currentDimension, useExtendedStructure);
                IEnumerable<StructuralDimension> currentAncestorDimensions = currentAnscestors.Keys.Select(ancrf => new StructuralDimension(ancrf.ModelObjectIdAsInt, currentDimension.EntityDimensionNumber, currentAnscestors[ancrf].DimensionType, currentAnscestors[ancrf].CorrespondingVariableRef, currentAnscestors[ancrf].EntityTypeEnumType, currentAnscestors[ancrf].EntityInstanceEnumType)).ToList();

                foreach (StructuralDimension ancestorDimension in currentAncestorDimensions)
                {
                    if (lastSpace.Dimensions.Contains(ancestorDimension))
                    { continue; }

                    tempSpace = tempSpace.Merge(ancestorDimension);

                    if (structuralMap.IsDirectlyAccessibleUsingSpace(mainRef, relatedRefs, tempSpace, useExtendedStructure))
                    {
                        bridgingSpaces.Add(tempSpace);
                        return;
                    }

                    HashSet<StructuralDimension> tempDimensionsToProcess = new HashSet<StructuralDimension>(handledDimensions);
                    tempDimensionsToProcess.Add(ancestorDimension);

                    HashSet<StructuralDimension> tempHandledDimensions = new HashSet<StructuralDimension>(handledDimensions);
                    tempHandledDimensions.Add(currentDimension);

                    ExpandBridgeByDimension(structuralMap, mainRef, relatedRefs, useExtendedStructure, tempDimensionsToProcess, tempHandledDimensions, tempSpace, bridgingSpaces);
                }
            }
        }

        public static bool IsRelatedAndMoreGeneral(this StructuralSpace space, IStructuralMap structuralMap, StructuralSpace otherSpace, bool useExtendedStructure)
        {
            return IsRelatedAndMoreGeneral(space, structuralMap, otherSpace, useExtendedStructure, DefaultAllowAsGeneralToReturnTrue);
        }

        public static bool IsRelatedAndMoreGeneral(this StructuralSpace space, IStructuralMap structuralMap, StructuralSpace otherSpace, bool useExtendedStructure, bool allowAsGeneral)
        {
            if (!allowAsGeneral)
            {
                if ((space == StructuralSpace.GlobalStructuralSpace) && (otherSpace == StructuralSpace.GlobalStructuralSpace))
                { return false; }
            }
            if (space == StructuralSpace.GlobalStructuralSpace)
            { return true; }

            bool hasMatches = false;
            StructuralSpace spaceDifferences = space.GetMissing(otherSpace, out hasMatches);
            StructuralSpace otherSpaceDifferences = new StructuralSpace();

            if (!allowAsGeneral)
            {
                otherSpaceDifferences = otherSpace.GetMissing(space, out hasMatches);

                if ((spaceDifferences.Dimensions.Count <= 0) && (otherSpaceDifferences.Dimensions.Count <= 0))
                { return false; }
            }
            if (spaceDifferences.Dimensions.Count <= 0)
            { return true; }

            StructuralSpace maximalSpace = space.GetMaximal(structuralMap, useExtendedStructure);
            StructuralSpace maximalOtherSpace = otherSpace.GetMaximal(structuralMap, useExtendedStructure);

            StructuralSpace maximalSpaceDifferences = maximalSpace.GetMissing(maximalOtherSpace, out hasMatches);
            StructuralSpace maximalOtherSpaceDifferences = new StructuralSpace();

            if (!allowAsGeneral)
            {
                maximalOtherSpaceDifferences = maximalOtherSpace.GetMissing(maximalSpace, out hasMatches);

                if ((maximalSpaceDifferences.Dimensions.Count <= 0) && (maximalOtherSpaceDifferences.Dimensions.Count <= 0))
                { return false; }
            }
            if (maximalSpaceDifferences.Dimensions.Count <= 0)
            { return true; }

            return false;
        }

        public static bool Overlaps(this StructuralSpace space, IStructuralMap structuralMap, StructuralSpace otherSpace, bool useExtendedStructure)
        {
            if ((space == StructuralSpace.GlobalStructuralSpace)
                || (otherSpace == StructuralSpace.GlobalStructuralSpace))
            { return true; }

            bool hasMatches = false;
            StructuralSpace spaceDifferences = space.GetMissing(otherSpace, out hasMatches);

            if (hasMatches)
            { return true; }

            StructuralSpace otherSpaceDifferences = otherSpace.GetMissing(space, out hasMatches);
            StructuralSpace maximalDifferences = spaceDifferences.GetMaximal(structuralMap, useExtendedStructure);
            StructuralSpace maximalOtherDifferences = otherSpaceDifferences.GetMaximal(structuralMap, useExtendedStructure);

            foreach (StructuralDimension dimension in maximalDifferences.Dimensions)
            {
                foreach (StructuralDimension otherDimension in maximalOtherDifferences.Dimensions)
                {
                    if (otherDimension == dimension)
                    { return true; }
                }
            }

            return false;
        }

        public static StructuralSpace Merge(this StructuralSpace baseSpace, StructuralSpace otherSpace)
        {
            return Merge(baseSpace, otherSpace.Dimensions);
        }

        public static StructuralSpace Merge(this StructuralSpace baseSpace, StructuralDimension otherDimension)
        {
            var otherDimensions = new StructuralDimension[] { otherDimension };
            return Merge(baseSpace, otherDimensions);
        }

        public static StructuralSpace Merge(this StructuralSpace baseSpace, IEnumerable<StructuralDimension> otherDimensions)
        {
            return Merge(baseSpace, otherDimensions, null);
        }

        public static StructuralSpace Merge(this StructuralSpace baseSpace, IEnumerable<StructuralDimension> otherDimensions, Nullable<int> alternateDimensionNumber)
        {
            IDictionary<int, StructuralDimension> thisDims = baseSpace.DimensionsById;
            IDictionary<int, StructuralDimension> otherDims = otherDimensions.ToDictionary(d => d.DimensionId, d => d);

            HashSet<StructuralDimension> mergedDims = new HashSet<StructuralDimension>();

            foreach (StructuralDimension thisDim in thisDims.Values)
            {
                if (!otherDims.ContainsKey(thisDim.DimensionId))
                {
                    mergedDims.Add(thisDim);
                }
                else
                {
                    StructuralDimension otherDim = otherDims[thisDim.DimensionId];
                    StructuralDimension mergedDim = thisDim.Merge(otherDim);
                    mergedDims.Add(mergedDim);
                }
            }

            foreach (StructuralDimension otherDim in otherDims.Values)
            {
                if (!thisDims.ContainsKey(otherDim.DimensionId))
                {
                    mergedDims.Add(otherDim);
                }
            }

            return new StructuralSpace(mergedDims, alternateDimensionNumber);
        }

        public static bool KeysMatch(this StructuralSpace baseSpace, StructuralSpace otherSpace)
        {
            var baseKeys = baseSpace.Dimensions.Where(d => d.DimensionType.IsKeyMember()).ToList();
            var otherKeys = otherSpace.Dimensions.Where(d => d.DimensionType.IsKeyMember()).ToList();

            if (baseKeys.Count != otherKeys.Count)
            { return false; }

            var unionKeys = baseKeys.Union(otherKeys).ToList();

            return (baseKeys.Count == unionKeys.Count);
        }

        public static bool ContainsAllKeysFrom(this StructuralSpace baseSpace, StructuralSpace otherSpace)
        {
            var otherKeyDimensions = otherSpace.Dimensions.Where(d => d.DimensionType.IsKeyMember()).ToList();

            foreach (var otherKeyDimension in otherKeyDimensions)
            {
                var matchingDimensions = baseSpace.Dimensions.Where(d => (d.EntityTypeNumber == otherKeyDimension.EntityTypeNumber)).ToList();

                if (matchingDimensions.Count < 1)
                { return false; }
            }
            return true;
        }
    }
}