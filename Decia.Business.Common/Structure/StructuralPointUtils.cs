using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using SDT = Decia.Business.Common.Structure.StructuralDimensionType;
using SDTUtils = Decia.Business.Common.Structure.StructuralDimensionTypeUtils;

namespace Decia.Business.Common.Structure
{
    public static class StructuralPointUtils
    {
        public const bool DefaultFavorMatching = false;
        public const bool DefaultAllowAsGeneralToReturnTrue = StructuralSpaceUtils.DefaultAllowAsGeneralToReturnTrue;

        public static StructuralPoint GetMissing(this StructuralPoint point, StructuralPoint otherPoint, out Nullable<bool> hasMatches)
        {
            return GetMissing(point, otherPoint, DefaultFavorMatching, out hasMatches);
        }

        public static StructuralPoint GetMissing(this StructuralPoint point, StructuralPoint otherPoint, bool favorMatching, out Nullable<bool> hasMatches)
        {
            HashSet<StructuralCoordinate> missing = new HashSet<StructuralCoordinate>();
            hasMatches = null;

            IDictionary<StructuralDimension, StructuralCoordinate> otherCoordinatesByDimension = otherPoint.CoordinatesByDimension;
            foreach (StructuralCoordinate coordinate in point.Coordinates)
            {
                StructuralDimension dimension = coordinate.Dimension;
                if (!otherCoordinatesByDimension.ContainsKey(dimension))
                {
                    missing.Add(coordinate);
                    continue;
                }

                if (!otherCoordinatesByDimension.Values.Contains(coordinate))
                {
                    if (!hasMatches.HasValue || (hasMatches.HasValue && !favorMatching))
                    { hasMatches = false; }
                }
                else
                {
                    if (!hasMatches.HasValue || (hasMatches.HasValue && favorMatching))
                    { hasMatches = true; }
                }
            }
            return new StructuralPoint(missing);
        }

        public static StructuralPoint GetReduced(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, bool useExtendedStructure)
        {
            return GetReduced(point, structuralMap, modelInstanceRef, null, useExtendedStructure);
        }

        public static StructuralPoint GetReduced(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, bool useExtendedStructure)
        {
            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }

            HashSet<StructuralCoordinate> reducedCoordinates = new HashSet<StructuralCoordinate>(point.Coordinates);
            HashSet<StructuralCoordinate> coordinatesToProcess = new HashSet<StructuralCoordinate>(reducedCoordinates);

            while (coordinatesToProcess.Count > 0)
            {
                StructuralCoordinate currentCoordinate = coordinatesToProcess.First();
                IEnumerable<ModelObjectReference> currentDecendants = null;
                SDT descendantDimensionType = SDTUtils.DefaultDimensionType;

                ITree<ModelObjectReference> entityInstanceTree = structuralMap.GetEntityInstanceTree(modelInstanceRef);
                ITimeNetwork<ModelObjectReference> entityInstanceExtendedNetwork = structuralMap.GetEntityInstanceExtendedTimeNetwork(modelInstanceRef);

                if (!useExtendedStructure || (entityInstanceExtendedNetwork == null))
                {
                    currentDecendants = entityInstanceTree.GetDecendants(currentCoordinate.EntityInstanceRef).ToList();
                }
                else
                {
                    currentDecendants = entityInstanceExtendedNetwork.GetDecendants(desiredPeriod.Value, currentCoordinate.EntityInstanceRef).ToList();
                }

                HashSet<StructuralCoordinate> currentDecendantCoordinates = new HashSet<StructuralCoordinate>();
                foreach (ModelObjectReference descendantInstanceRef in currentDecendants)
                {
                    ModelObjectReference descendantTypeRef = structuralMap.GetStructuralTypeForInstance(modelInstanceRef, descendantInstanceRef);
                    StructuralCoordinate descendantCoordinate = new StructuralCoordinate(descendantTypeRef.ModelObjectIdAsInt, currentCoordinate.EntityDimensionNumber, descendantInstanceRef.ModelObjectId, descendantDimensionType);
                    currentDecendantCoordinates.Add(descendantCoordinate);
                }

                foreach (StructuralCoordinate decendantCoordinate in currentDecendantCoordinates)
                {
                    if (!reducedCoordinates.Contains(decendantCoordinate))
                    { continue; }

                    reducedCoordinates.Remove(currentCoordinate);
                    break;
                }

                coordinatesToProcess.Remove(currentCoordinate);
            }
            return new StructuralPoint(reducedCoordinates);
        }

        public static StructuralPoint GetMaximal(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, bool useExtendedStructure)
        {
            return GetMaximal(point, structuralMap, modelInstanceRef, null, useExtendedStructure);
        }

        public static StructuralPoint GetMaximal(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, bool useExtendedStructure)
        {
            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }

            var entityInstanceTree = structuralMap.GetEntityInstanceTree(modelInstanceRef);
            var entityInstanceExtendedNetwork = structuralMap.GetEntityInstanceExtendedTimeNetwork(modelInstanceRef);
            var hasExtendedNetwork = (useExtendedStructure && (entityInstanceExtendedNetwork != null));

            var maximalCoordinates = new HashSet<StructuralCoordinate>(point.Coordinates);
            var coordinatesToProcess = new HashSet<StructuralCoordinate>(maximalCoordinates);

            while (coordinatesToProcess.Count > 0)
            {
                StructuralCoordinate currentCoordinate = coordinatesToProcess.First();
                IEnumerable<ModelObjectReference> currentAncestors = null;
                SDT ancestorDimensionType = SDTUtils.DefaultDimensionType;

                if (!hasExtendedNetwork)
                {
                    currentAncestors = entityInstanceTree.GetAncestors(currentCoordinate.EntityInstanceRef).ToList();
                }
                else
                {
                    currentAncestors = entityInstanceExtendedNetwork.GetAncestors(desiredPeriod.Value, currentCoordinate.EntityInstanceRef).ToList();
                }

                var currentAncestorCoordinates = new HashSet<StructuralCoordinate>();

                foreach (ModelObjectReference ancestorInstanceRef in currentAncestors)
                {
                    ModelObjectReference ancestorTypeRef = structuralMap.GetStructuralTypeForInstance(modelInstanceRef, ancestorInstanceRef);
                    StructuralCoordinate ancestorCoordinate = new StructuralCoordinate(ancestorTypeRef.ModelObjectIdAsInt, currentCoordinate.EntityDimensionNumber, ancestorInstanceRef.ModelObjectId, ancestorDimensionType);
                    currentAncestorCoordinates.Add(ancestorCoordinate);
                }

                foreach (StructuralCoordinate ancestorCoordinate in currentAncestorCoordinates)
                {
                    if (maximalCoordinates.Contains(ancestorCoordinate))
                    { continue; }

                    maximalCoordinates.Add(ancestorCoordinate);
                    coordinatesToProcess.Add(ancestorCoordinate);
                }

                coordinatesToProcess.Remove(currentCoordinate);
            }
            return new StructuralPoint(maximalCoordinates);
        }

        public static bool IsRelatedAndMoreGeneral(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, StructuralPoint otherPoint, bool useExtendedStructure)
        {
            return IsRelatedAndMoreGeneral(point, structuralMap, modelInstanceRef, null, otherPoint, useExtendedStructure, DefaultAllowAsGeneralToReturnTrue);
        }

        public static bool IsRelatedAndMoreGeneral(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, StructuralPoint otherPoint, bool useExtendedStructure, bool allowAsGeneral)
        {
            return IsRelatedAndMoreGeneral(point, structuralMap, modelInstanceRef, null, otherPoint, useExtendedStructure, allowAsGeneral);
        }

        public static bool IsRelatedAndMoreGeneral(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, StructuralPoint otherPoint, bool useExtendedStructure)
        {
            return IsRelatedAndMoreGeneral(point, structuralMap, modelInstanceRef, desiredPeriod, otherPoint, useExtendedStructure, DefaultAllowAsGeneralToReturnTrue);
        }

        public static bool IsRelatedAndMoreGeneral(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, StructuralPoint otherPoint, bool useExtendedStructure, bool allowAsGeneral)
        {
            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }

            StructuralSpace thisSpace = point.Space;
            StructuralSpace otherSpace = otherPoint.Space;

            if (point.Equals(otherPoint))
            { return allowAsGeneral; }

            if (!point.Overlaps(structuralMap, modelInstanceRef, desiredPeriod, otherPoint, useExtendedStructure))
            { return false; }

            return thisSpace.IsRelatedAndMoreGeneral(structuralMap, otherSpace, useExtendedStructure);
        }

        public static bool Overlaps(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, StructuralPoint otherPoint, bool useExtendedStructure)
        {
            return Overlaps(point, structuralMap, modelInstanceRef, null, otherPoint, useExtendedStructure);
        }

        public static bool Overlaps(this StructuralPoint point, IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, StructuralPoint otherPoint, bool useExtendedStructure)
        {
            if ((point == StructuralPoint.GlobalStructuralPoint)
                 || (otherPoint == StructuralPoint.GlobalStructuralPoint))
            { return true; }

            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }

            Nullable<bool> hasMatches = null;
            StructuralPoint pointDifferences = point.GetMissing(otherPoint, out hasMatches);
            if (hasMatches.HasValue)
            { return hasMatches.Value; }

            StructuralPoint otherPointDifferences = otherPoint.GetMissing(point, out hasMatches);
            StructuralPoint maximalDifferences = pointDifferences.GetMaximal(structuralMap, modelInstanceRef, desiredPeriod, useExtendedStructure);
            StructuralPoint maximalOtherDifferences = otherPointDifferences.GetMaximal(structuralMap, modelInstanceRef, desiredPeriod, useExtendedStructure);

            IDictionary<StructuralDimension, StructuralCoordinate> maximalOtherCoordinatesByDimension = maximalOtherDifferences.CoordinatesByDimension;
            bool overlapFound = false;
            foreach (StructuralCoordinate coordinate in maximalDifferences.Coordinates)
            {
                StructuralDimension dimension = coordinate.Dimension;
                if (!maximalOtherCoordinatesByDimension.ContainsKey(dimension))
                { continue; }

                overlapFound = true;
                if (!maximalOtherCoordinatesByDimension.Values.Contains(coordinate))
                { return false; }
            }
            return overlapFound;
        }

        public static StructuralPoint Merge(this StructuralPoint basePoint, StructuralPoint otherPoint)
        {
            return Merge(basePoint, otherPoint.Coordinates);
        }

        public static StructuralPoint Merge(this StructuralPoint basePoint, IEnumerable<StructuralCoordinate> otherCoordinates)
        {
            HashSet<StructuralCoordinate> otherCoordinatesToUse = new HashSet<StructuralCoordinate>();

            foreach (StructuralCoordinate otherCoordinate in otherCoordinates)
            {
                if (basePoint.CoordinatesById.ContainsKey(otherCoordinate.CoordinateId))
                { continue; }

                otherCoordinatesToUse.Add(otherCoordinate);
            }

            return new StructuralPoint(basePoint, otherCoordinatesToUse);
        }

        public static IEnumerable<StructuralCoordinate> PruneNullRefs(this IEnumerable<StructuralCoordinate> originalCoordinates)
        {
            var reducedCoordinates = originalCoordinates.Where(x => !x.IsNull).ToList();
            return reducedCoordinates;
        }

        public static StructuralPoint PruneNullRefs(this StructuralPoint originalPoint)
        {
            var reducedCoordinates = originalPoint.Coordinates.PruneNullRefs();
            var reducedPoint = new StructuralPoint(reducedCoordinates);
            return reducedPoint;
        }

        public static IEnumerable<StructuralPoint> PruneNullRefs(this IEnumerable<StructuralPoint> originalPoints)
        {
            var reducedPoints = new List<StructuralPoint>();
            foreach (var originalPoint in originalPoints)
            {
                var reducedPoint = originalPoint.PruneNullRefs();
                reducedPoints.Add(reducedPoint);
            }
            return reducedPoints;
        }

        public static IDictionary<K, StructuralPoint> PruneNullRefs<K>(this IDictionary<K, StructuralPoint> originalPoints)
        {
            var reducedPoints = new Dictionary<K, StructuralPoint>(originalPoints.GetComparer());
            foreach (var originalBucket in originalPoints)
            {
                var reducedPoint = originalBucket.Value.PruneNullRefs();
                reducedPoints.Add(originalBucket.Key, reducedPoint);
            }
            return reducedPoints;
        }
    }
}