using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Structure
{
    public class JoinPath
    {
        public const bool UseExtendedStructure = true;
        public const int Default_AncestorDistance = 0;
        public const int Default_DescendantDistance = int.MaxValue;

        #region Constructors

        internal JoinPath(IStructuralMap structuralMap, ModelObjectReference mainStructuralTypeRef, ModelObjectReference relatedStructuralTypeRef)
            : this(structuralMap, mainStructuralTypeRef, new ModelObjectReference[] { relatedStructuralTypeRef })
        { }

        internal JoinPath(IStructuralMap structuralMap, ModelObjectReference mainStructuralTypeRef, IEnumerable<ModelObjectReference> relatedStructuralTypeRefs)
        {
            StructuralMap = structuralMap;

            MainStructuralTypeRef = mainStructuralTypeRef;
            MainSpace_Original = structuralMap.GetStructuralSpace(mainStructuralTypeRef, UseExtendedStructure);
            MainSpace_Reduced = MainSpace_Original.GetReduced(structuralMap, UseExtendedStructure);
            MainSpace_Maximal = MainSpace_Original.GetMaximal(structuralMap, UseExtendedStructure);

            RelatedTypeRefs = new HashSet<ModelObjectReference>(relatedStructuralTypeRefs, ModelObjectReference.DimensionalComparer);
            if (RelatedTypeRefs.Contains(MainStructuralTypeRef))
            { RelatedTypeRefs.Remove(MainStructuralTypeRef); }

            var totalKeyAndSpace = structuralMap.GetRelativeStructuralRefsAndSpace(mainStructuralTypeRef, relatedStructuralTypeRefs, UseExtendedStructure);
            IsValidPath = (totalKeyAndSpace.HasValue);
            if (!IsValidPath)
            { return; }

            TotalSpace_TypeRefJoinOrder = totalKeyAndSpace.Value.Key.KeyPartItems.ToList();
            TotalSpace_Original = totalKeyAndSpace.Value.Value;
            TotalSpace_Reduced = TotalSpace_Original.GetReduced(structuralMap, UseExtendedStructure);
            TotalSpace_Maximal = TotalSpace_Original.GetMaximal(structuralMap, UseExtendedStructure);

            var entityTypesNeeded = new Dictionary<ModelObjectReference, bool>(ModelObjectReference.DimensionalComparer);

            if (MainStructuralType == ModelObjectType.EntityType)
            { entityTypesNeeded.Add(MainStructuralTypeRef, true); }
            if (MainStructuralType == ModelObjectType.RelationType)
            {
                var mainRelationSpace = structuralMap.GetStructuralSpace(MainStructuralTypeRef, UseExtendedStructure);
                foreach (var dimension in mainRelationSpace.Dimensions)
                { entityTypesNeeded.Add(dimension.EntityTypeRefWithDimNum, false); }
            }

            foreach (var relatedStructuralTypeRef in TotalSpace_TypeRefJoinOrder)
            {
                if (ModelObjectReference.DimensionalComparer.Equals(MainStructuralTypeRef, relatedStructuralTypeRef))
                { continue; }

                if (relatedStructuralTypeRef.ModelObjectType == ModelObjectType.EntityType)
                {
                    if (entityTypesNeeded.ContainsKey(relatedStructuralTypeRef))
                    { entityTypesNeeded[relatedStructuralTypeRef] = true; }
                    else
                    { entityTypesNeeded.Add(relatedStructuralTypeRef, true); }
                }
                else if (relatedStructuralTypeRef.ModelObjectType == ModelObjectType.RelationType)
                {
                    var relatedRelationSpace_Base = structuralMap.GetBaseStructuralSpace(relatedStructuralTypeRef);
                    var relatedRelationSpace_Extd = structuralMap.GetExtendedStructuralSpace(relatedStructuralTypeRef);

                    var baseDimensions = relatedRelationSpace_Base.Dimensions.ToHashSet();

                    foreach (var dimension in relatedRelationSpace_Extd.Dimensions)
                    {
                        var entityTypeRef = dimension.EntityTypeRefWithDimNum;

                        if (entityTypesNeeded.ContainsKey(entityTypeRef))
                        { entityTypesNeeded[entityTypeRef] = true; }
                        else
                        { entityTypesNeeded.Add(entityTypeRef, false); }
                    }
                }
                else
                { continue; }
            }

            var iterationValues = entityTypesNeeded.Keys.ToList();
            EntityTypeRef_SubPaths_FromAncestor = new Dictionary<ModelObjectReference, IList<ModelObjectReference>>(ModelObjectReference.DimensionalComparer);

            for (int i = 0; i < iterationValues.Count; i++)
            {
                var ancestor = iterationValues.ElementAt(i);

                for (int j = 0; j < iterationValues.Count; j++)
                {
                    if (i == j)
                    { continue; }

                    var descendant = iterationValues.ElementAt(j);

                    if (ancestor.NonNullAlternateDimensionNumber != descendant.NonNullAlternateDimensionNumber)
                    { continue; }

                    var isUsed = (entityTypesNeeded[ancestor] || entityTypesNeeded[descendant]);
                    var path = structuralMap.EntityTypeExtendedNetwork.GetPath(ancestor, descendant);

                    if (path == null)
                    { continue; }

                    var pathForAltDim = path.Select(x => x.ToAlternateDimension(ancestor.AlternateDimensionNumber)).ToList();

                    foreach (var pathMember in pathForAltDim)
                    { entityTypesNeeded[pathMember] = isUsed; }

                    if (!EntityTypeRef_SubPaths_FromAncestor.ContainsKey(ancestor))
                    {
                        EntityTypeRef_SubPaths_FromAncestor.Add(ancestor, pathForAltDim);
                    }
                    else
                    {
                        var oldPath = EntityTypeRef_SubPaths_FromAncestor[ancestor];
                        var oldPathPosition = (int?)null;

                        for (int x = 0; x < pathForAltDim.Count; x++)
                        {
                            if (!oldPathPosition.HasValue)
                            {
                                if (oldPath[0] == pathForAltDim[x])
                                { oldPathPosition = 0; }
                            }
                            else
                            {
                                oldPathPosition++;

                                if (oldPathPosition.Value >= oldPath.Count)
                                { continue; }
                                if (oldPath[oldPathPosition.Value] != pathForAltDim[x])
                                { throw new InvalidOperationException("Path fragments do not match."); }
                            }
                        }

                        EntityTypeRef_SubPaths_FromAncestor[ancestor] = pathForAltDim;
                    }
                }
            }

            EntityTypeRefs_Used = entityTypesNeeded.Where(x => x.Value).Select(x => x.Key).ToHashSet(ModelObjectReference.DimensionalComparer);

            AllTypeRefs_Ordered = TotalSpace_TypeRefJoinOrder.ToList();
            if (!AllTypeRefs_Ordered.Contains(MainStructuralTypeRef, ModelObjectReference.DimensionalComparer))
            { AllTypeRefs_Ordered.Insert(0, MainStructuralTypeRef); }

            AllTypeRefs_Included = new HashSet<ModelObjectReference>(AllTypeRefs_Ordered, ModelObjectReference.DimensionalComparer);
            GlobalTypeRefs_Included = new HashSet<ModelObjectReference>(AllTypeRefs_Included.Where(x => x.ModelObjectType == ModelObjectType.GlobalType), ModelObjectReference.DimensionalComparer);
            EntityTypeRefs_Included = new HashSet<ModelObjectReference>(AllTypeRefs_Included.Where(x => x.ModelObjectType == ModelObjectType.EntityType), ModelObjectReference.DimensionalComparer);
            RelationTypeRefs_Included = new HashSet<ModelObjectReference>(AllTypeRefs_Included.Where(x => x.ModelObjectType == ModelObjectType.RelationType), ModelObjectReference.DimensionalComparer);
        }

        #endregion

        #region Properties

        public IStructuralMap StructuralMap { get; protected set; }
        public bool IsValidPath { get; protected set; }

        public ModelObjectReference MainStructuralTypeRef { get; protected set; }
        public ModelObjectType MainStructuralType { get { return MainStructuralTypeRef.ModelObjectType; } }
        public StructuralSpace MainSpace_Original { get; protected set; }
        public StructuralSpace MainSpace_Reduced { get; protected set; }
        public StructuralSpace MainSpace_Maximal { get; protected set; }

        public HashSet<ModelObjectReference> RelatedTypeRefs { get; protected set; }
        public List<ModelObjectReference> TotalSpace_TypeRefJoinOrder { get; protected set; }
        public StructuralSpace TotalSpace_Original { get; protected set; }
        public StructuralSpace TotalSpace_Reduced { get; protected set; }
        public StructuralSpace TotalSpace_Maximal { get; protected set; }

        public HashSet<ModelObjectReference> EntityTypeRefs_Used { get; protected set; }

        public List<ModelObjectReference> AllTypeRefs_Ordered { get; protected set; }
        public HashSet<ModelObjectReference> AllTypeRefs_Included { get; protected set; }
        public HashSet<ModelObjectReference> GlobalTypeRefs_Included { get; protected set; }
        public HashSet<ModelObjectReference> EntityTypeRefs_Included { get; protected set; }
        public HashSet<ModelObjectReference> RelationTypeRefs_Included { get; protected set; }

        public Dictionary<ModelObjectReference, IList<ModelObjectReference>> EntityTypeRef_SubPaths_FromAncestor { get; protected set; }

        #endregion

        #region Methods

        public List<JoinStep> GetOrderedJoinSteps()
        {
            var mainSpace = StructuralMap.GetStructuralSpace(MainStructuralTypeRef, UseExtendedStructure);

            var handledDimensions = new Dictionary<StructuralDimension, JoinedDimensionValue>();
            var joinedDimensionValues = new Dictionary<StructuralDimension, JoinedDimensionValue>();
            foreach (var dimension in mainSpace.Dimensions)
            {
                var dimValue = new JoinedDimensionValue(MainStructuralTypeRef);
                dimValue.Dimension = dimension;
                dimValue.KeySourceRef = MainStructuralTypeRef;
                dimValue.ExtendedJoinPath = null;

                joinedDimensionValues.Add(dimension, dimValue);
                handledDimensions.Add(dimension, dimValue);
            }
            var handledDimensions_Original = handledDimensions.Keys.ToHashSet();

            var mainJoinStep = new JoinStep(JoinStep.Start_JoinOrder, MainStructuralTypeRef, joinedDimensionValues, null);
            mainJoinStep.IsUsed = true;

            var joinStepDict = new Dictionary<ModelObjectReference, JoinStep>(ModelObjectReference.DimensionalComparer);
            joinStepDict.Add(mainJoinStep.StructuralTypeRef, mainJoinStep);

            var structuralTypeRefs = AllTypeRefs_Ordered.Union(EntityTypeRefs_Used, ModelObjectReference.DimensionalComparer).ToHashSet(ModelObjectReference.DimensionalComparer);
            var remainingStructuralTypeRefsAsList = GroupByObjectId(structuralTypeRefs);
            remainingStructuralTypeRefsAsList.Remove(MainStructuralTypeRef, ModelObjectReference.DimensionalComparer);

            var requiresGlobal = ((MainStructuralTypeRef != ModelObjectReference.GlobalTypeReference) && remainingStructuralTypeRefsAsList.Contains(ModelObjectReference.GlobalTypeReference));
            if (requiresGlobal)
            {
                var globalJoinStep = new JoinStep(JoinStep.Global_JoinOrder, ModelObjectReference.GlobalTypeReference, new Dictionary<StructuralDimension, JoinedDimensionValue>(), mainJoinStep);
                globalJoinStep.IsUsed = true;
                joinStepDict.Add(globalJoinStep.StructuralTypeRef, globalJoinStep);
            }
            remainingStructuralTypeRefsAsList.Remove(ModelObjectReference.GlobalTypeReference);
            var remainingStructuralTypeRefsAsHashSet = remainingStructuralTypeRefsAsList.ToHashSet(ModelObjectReference.DimensionalComparer);


            if (MainStructuralType == ModelObjectType.GlobalType)
            {
                ExploreAccessibleStructuralTypeRefs(joinStepDict, mainJoinStep, handledDimensions, handledDimensions_Original, remainingStructuralTypeRefsAsHashSet, null);
            }
            else
            {
                ExploreAccessibleStructuralTypeRefs(joinStepDict, mainJoinStep, handledDimensions, handledDimensions_Original, remainingStructuralTypeRefsAsHashSet, false);

                if (remainingStructuralTypeRefsAsHashSet.Count > 0)
                {
                    ExploreAccessibleStructuralTypeRefs(joinStepDict, mainJoinStep, handledDimensions, handledDimensions_Original, remainingStructuralTypeRefsAsHashSet, true);
                }
            }

            if (remainingStructuralTypeRefsAsHashSet.Count > 0)
            { return null; }

            var joinSteps = JoinStep.GetAsList_BreadthFirst(mainJoinStep, true);
            return joinSteps;
        }

        private void ExploreAccessibleStructuralTypeRefs(Dictionary<ModelObjectReference, JoinStep> joinStepDict, JoinStep currentJoinStep, Dictionary<StructuralDimension, JoinedDimensionValue> handledDimensions, HashSet<StructuralDimension> handledDimensions_Original, HashSet<ModelObjectReference> remainingStructuralTypeRefs, bool? exploreExtendedNodes)
        {
            var currentStructuralTypeRef = currentJoinStep.StructuralTypeRef;

            var nestedStructuralRefsToProcess = GetOrderedStructuralTypesToProcess(MainStructuralTypeRef, currentStructuralTypeRef, remainingStructuralTypeRefs.ToList(), false);
            if ((exploreExtendedNodes == true) || (!exploreExtendedNodes.HasValue))
            {
                var nestedStructuralRefsToProcess_Ext = GetOrderedStructuralTypesToProcess(MainStructuralTypeRef, currentStructuralTypeRef, remainingStructuralTypeRefs.ToList(), true);
                nestedStructuralRefsToProcess.AddRange(nestedStructuralRefsToProcess_Ext);

                if ((!exploreExtendedNodes.HasValue) && (currentJoinStep.PreviousJoinStep == null))
                { nestedStructuralRefsToProcess = nestedStructuralRefsToProcess.Where(x => remainingStructuralTypeRefs.Contains(x)).ToList(); }
            }
            if (currentStructuralTypeRef.ModelObjectType == ModelObjectType.EntityType)
            { nestedStructuralRefsToProcess = nestedStructuralRefsToProcess.Select(x => (x.ModelObjectType == ModelObjectType.EntityType) ? new ModelObjectReference(x, currentStructuralTypeRef.NonNullAlternateDimensionNumber) : x).ToList(); }

            var existingNestedSteps = currentJoinStep.DependentJoinSteps.OrderBy(x => x.JoinOrder).ToList();

            for (int index = 0; index < nestedStructuralRefsToProcess.Count; index++)
            {
                if (remainingStructuralTypeRefs.Count < 1)
                { break; }

                var nestedStructuralTypeRef = nestedStructuralRefsToProcess[index];
                var joinedDimensionValues = new Dictionary<StructuralDimension, JoinedDimensionValue>();
                var nestedJoinStep = (JoinStep)null;

                if (JoinStep.IsRefAlreadyHandled(nestedStructuralTypeRef, currentJoinStep))
                { continue; }

                if (joinStepDict.ContainsKey(nestedStructuralTypeRef))
                {
                    nestedJoinStep = joinStepDict[nestedStructuralTypeRef];
                }
                else
                {
                    var structuralSpace = StructuralMap.GetStructuralSpace(nestedStructuralTypeRef, UseExtendedStructure);

                    foreach (var dimension in structuralSpace.Dimensions)
                    {
                        if (handledDimensions.ContainsKey(dimension))
                        {
                            var dimValue = handledDimensions[dimension];
                            dimValue = new JoinedDimensionValue(nestedStructuralTypeRef, dimValue);
                            joinedDimensionValues.Add(dimension, dimValue);
                        }
                        else
                        {
                            var path = (IList<ModelObjectReference>)null;
                            var wasCreatedByMainRef = handledDimensions_Original.Contains(dimension);

                            if (!wasCreatedByMainRef)
                            {
                                if (nestedStructuralTypeRef.ModelObjectType == ModelObjectType.EntityType)
                                {
                                    var possiblePaths = EntityTypeRef_SubPaths_FromAncestor.Values.Where(x => (x.Contains(nestedStructuralTypeRef, ModelObjectReference.DimensionalComparer) && x.Contains(dimension.EntityTypeRefWithDimNum, ModelObjectReference.DimensionalComparer))).ToList();
                                    path = possiblePaths.FirstOrDefault();
                                }
                                else if (nestedStructuralTypeRef.ModelObjectType == ModelObjectType.RelationType)
                                {
                                    path = new List<ModelObjectReference>();
                                    path.Add(nestedStructuralTypeRef);
                                    path.Add(dimension.EntityTypeRefWithDimNum);
                                }
                                else
                                { throw new InvalidOperationException("The requested Path cannot be created."); }
                            }

                            var dimValue = new JoinedDimensionValue(nestedStructuralTypeRef);
                            dimValue.Dimension = dimension;
                            dimValue.KeySourceRef = nestedStructuralTypeRef;
                            dimValue.ExtendedJoinPath = path;

                            joinedDimensionValues.Add(dimension, dimValue);
                            handledDimensions.Add(dimension, dimValue);
                        }
                    }

                    nestedJoinStep = new JoinStep(index, nestedStructuralTypeRef, joinedDimensionValues, currentJoinStep);
                    joinStepDict.Add(nestedJoinStep.StructuralTypeRef, nestedJoinStep);
                }

                if (remainingStructuralTypeRefs.Contains(nestedStructuralTypeRef))
                {
                    nestedJoinStep.IsUsed = true;
                    remainingStructuralTypeRefs.Remove(nestedStructuralTypeRef);
                }

                ExploreAccessibleStructuralTypeRefs(joinStepDict, nestedJoinStep, handledDimensions, handledDimensions_Original, remainingStructuralTypeRefs, exploreExtendedNodes);
            }

            foreach (var existingNestedStep in existingNestedSteps)
            {
                ExploreAccessibleStructuralTypeRefs(joinStepDict, existingNestedStep, handledDimensions, handledDimensions_Original, remainingStructuralTypeRefs, exploreExtendedNodes);
            }
        }

        private List<ModelObjectReference> GetOrderedStructuralTypesToProcess(ModelObjectReference mainStructuralTypeRef, ModelObjectReference currentStructuralTypeRef, List<ModelObjectReference> remainingStructuralTypeRefs, bool current_UseExtendedNetwork)
        {
            if (currentStructuralTypeRef.ModelObjectType == ModelObjectType.GlobalType)
            {
                if (mainStructuralTypeRef.ModelObjectType != ModelObjectType.GlobalType)
                { return new List<ModelObjectReference>(new ModelObjectReference[] { currentStructuralTypeRef }); }
            }

            var remainingGlobalTypes = remainingStructuralTypeRefs.Where(x => (x.ModelObjectType == ModelObjectType.GlobalType)).ToList();
            var remainingRelationTypes = remainingStructuralTypeRefs.Where(x => (x.ModelObjectType == ModelObjectType.RelationType)).ToList();

            if (remainingGlobalTypes.Count == remainingStructuralTypeRefs.Count)
            { return remainingGlobalTypes; }

            var accessibleEntityTypes = remainingStructuralTypeRefs.Where(x => (x.ModelObjectType == ModelObjectType.EntityType) && (StructuralMap.IsAccessible(currentStructuralTypeRef, x, current_UseExtendedNetwork))).ToHashSet(ModelObjectReference.DimensionalComparer);
            var uniquelyAccessibleEntityTypes = accessibleEntityTypes.Where(x => StructuralMap.IsUnique(currentStructuralTypeRef, x, current_UseExtendedNetwork)).ToHashSet(ModelObjectReference.DimensionalComparer);
            var nonUniquelyAccessibleEntityTypes = accessibleEntityTypes.Where(x => !uniquelyAccessibleEntityTypes.Contains(x)).ToHashSet(ModelObjectReference.DimensionalComparer);

            var ancestorsWithDistances = (!current_UseExtendedNetwork) ? StructuralMap.EntityTypeTree.GetAncestorsWithDistances(currentStructuralTypeRef) : StructuralMap.EntityTypeExtendedNetwork.GetAncestorsWithDistances(currentStructuralTypeRef);
            if ((currentStructuralTypeRef.ModelObjectType == ModelObjectType.RelationType) && (remainingStructuralTypeRefs.Count > 0))
            {
                var structuralSpace = StructuralMap.GetStructuralSpace(currentStructuralTypeRef, current_UseExtendedNetwork);
                ancestorsWithDistances = structuralSpace.Dimensions.ToDictionary(x => x.EntityTypeRefWithDimNum, x => 1, ModelObjectReference.DimensionalComparer);
                uniquelyAccessibleEntityTypes = ancestorsWithDistances.Keys.ToHashSet(ModelObjectReference.DimensionalComparer);
            }

            var descendantsWithDistances = (!current_UseExtendedNetwork) ? StructuralMap.EntityTypeTree.GetDecendantsWithDistances(currentStructuralTypeRef) : StructuralMap.EntityTypeExtendedNetwork.GetDecendantsWithDistances(currentStructuralTypeRef);
            if ((currentStructuralTypeRef.ModelObjectType == ModelObjectType.GlobalType) && (nonUniquelyAccessibleEntityTypes.Count > 0))
            {
                var firstET = nonUniquelyAccessibleEntityTypes.First();
                var ancestorsOfFirst = StructuralMap.EntityTypeExtendedNetwork.GetAncestorsWithDistances(firstET);

                if (ancestorsOfFirst.Count > 0)
                {
                    var maxDistance = ancestorsOfFirst.Values.Max();
                    var maxAncestor = ancestorsOfFirst.Where(x => x.Value == maxDistance).First().Key;
                    descendantsWithDistances = StructuralMap.EntityTypeExtendedNetwork.GetDecendantsWithDistances(maxAncestor);
                }
            }

            var uniquelyAccessibleEntityTypes_Ordered = uniquelyAccessibleEntityTypes.OrderBy(x => ancestorsWithDistances.ContainsKey(x) ? ancestorsWithDistances[x] : Default_AncestorDistance).ToList();
            var structuralTypesToProcess_Ordered = new List<ModelObjectReference>();

            if (mainStructuralTypeRef.ModelObjectType == ModelObjectType.GlobalType)
            {
                var nonUniquelyAccessibleEntityTypes_Ordered = nonUniquelyAccessibleEntityTypes.OrderBy(x => descendantsWithDistances.ContainsKey(x) ? descendantsWithDistances[x] : Default_DescendantDistance).ToList();

                structuralTypesToProcess_Ordered.AddRange(uniquelyAccessibleEntityTypes_Ordered);
                structuralTypesToProcess_Ordered.AddRange(nonUniquelyAccessibleEntityTypes_Ordered);
                structuralTypesToProcess_Ordered.AddRange(remainingRelationTypes);
            }
            else
            {
                var nonUniquelyAccessibleEntityTypes_Ordered = descendantsWithDistances.OrderBy(x => x.Value).Select(x => x.Key).ToList();

                var wholeEntityTypeChain = new List<ModelObjectReference>();
                wholeEntityTypeChain.AddRange(uniquelyAccessibleEntityTypes_Ordered);
                if (currentStructuralTypeRef.ModelObjectType == ModelObjectType.EntityType)
                { wholeEntityTypeChain.Add(currentStructuralTypeRef); }
                wholeEntityTypeChain.AddRange(nonUniquelyAccessibleEntityTypes_Ordered);

                var insertionsAfterIndex = new Dictionary<int, List<ModelObjectReference>>();

                if ((remainingRelationTypes.Count > 0) && (currentStructuralTypeRef.ModelObjectType != ModelObjectType.RelationType))
                {
                    var entityTypeRef = currentStructuralTypeRef;
                    var entityDimension = new StructuralDimension(entityTypeRef.ModelObjectIdAsInt, entityTypeRef.NonNullAlternateDimensionNumber, StructuralDimensionType.NotSet, null);
                    var i = wholeEntityTypeChain.IndexOf(entityTypeRef);

                    foreach (var relationTypeRef in remainingRelationTypes.ToList())
                    {
                        var relationSpace = StructuralMap.GetStructuralSpace(relationTypeRef, UseExtendedStructure);

                        if (relationSpace.Dimensions.Contains(entityDimension))
                        {
                            if (!insertionsAfterIndex.ContainsKey(i))
                            { insertionsAfterIndex.Add(i, new List<ModelObjectReference>()); }

                            insertionsAfterIndex[i].Add(relationTypeRef);
                            remainingRelationTypes.Remove(relationTypeRef);
                        }
                    }
                }

                var newOrderedList = new List<ModelObjectReference>();
                for (int i = 0; i < wholeEntityTypeChain.Count; i++)
                {
                    var entityTypeRef = wholeEntityTypeChain[i];

                    if (!ModelObjectReference.DimensionalComparer.Equals(entityTypeRef, currentStructuralTypeRef))
                    { newOrderedList.Add(entityTypeRef); }
                    if (insertionsAfterIndex.ContainsKey(i))
                    { newOrderedList.AddRange(insertionsAfterIndex[i]); }
                }
                structuralTypesToProcess_Ordered = newOrderedList;
            }
            return structuralTypesToProcess_Ordered;
        }

        private List<ModelObjectReference> GroupByObjectId(HashSet<ModelObjectReference> structuralTypeRefs)
        {
            var remaining = structuralTypeRefs.ToHashSet(ModelObjectReference.DimensionalComparer);
            var result = new List<ModelObjectReference>();

            while (remaining.Count > 0)
            {
                var currentRef = remaining.First();

                remaining.Remove(currentRef);
                result.Add(currentRef);

                var similars = remaining.Where(x => x.ModelObjectId == currentRef.ModelObjectId).ToList();

                foreach (var similar in similars)
                {
                    remaining.Remove(similar);
                    result.Add(similar);
                }
            }
            return result;
        }

        public HashSet<ModelObjectReference> GetHashSetWithoutMainRef(IEnumerable<ModelObjectReference> originalCollection)
        {
            var newHashSet = new HashSet<ModelObjectReference>(originalCollection, ModelObjectReference.DimensionalComparer);

            if (!newHashSet.Contains(MainStructuralTypeRef))
            { return newHashSet; }

            newHashSet.Remove(MainStructuralTypeRef);
            return newHashSet;
        }

        #endregion
    }
}