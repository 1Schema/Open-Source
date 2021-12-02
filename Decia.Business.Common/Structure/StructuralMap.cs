using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Node = Decia.Business.Common.Modeling.ModelObjectReference;
using IEdge = Decia.Business.Common.DataStructures.IEdge<Decia.Business.Common.Modeling.ModelObjectReference>;
using Edge = Decia.Business.Common.DataStructures.Edge<Decia.Business.Common.Modeling.ModelObjectReference>;
using ReferenceListKey = Decia.Business.Common.TypedIds.ListBasedKey<Decia.Business.Common.Modeling.ModelObjectReference>;

namespace Decia.Business.Common.Structure
{
    public partial class StructuralMap : IStructuralMap
    {
        public static readonly Func<ModelObjectReference, ModelObjectReference?> nullableConverter = ((ModelObjectReference nonNull) => nonNull);

        public static readonly Nullable<ModelObjectReference> NullModelObjectReference = null;
        public static readonly Nullable<StructuralCoordinate> NullCoordinate = null;
        public static readonly Nullable<StructuralDimension> NullDimension = null;
        public static readonly bool DefaultTypeRefIsBound = true;

        #region Members

        private ModelObjectReference m_ModelTemplateRef;
        private HashSet<ModelObjectReference> m_ModelInstanceRefs;

        private CachedTree<ModelObjectReference> m_EntityTypeTree;
        private CachedNetwork<ModelObjectReference> m_EntityTypeExtendedNetwork;
        private Dictionary<ModelObjectReference, CachedTree<ModelObjectReference>> m_EntityInstanceTrees;
        private Dictionary<ModelObjectReference, CachedTimeNetwork<ModelObjectReference>> m_EntityInstanceExtendedTimeNetworks;

        private KeyValuePair<ModelObjectReference, StructuralSpace> m_GlobalTypeSpaces;
        private Dictionary<ModelObjectReference, StructuralSpace> m_EntityTypeSpaces;
        private Dictionary<ModelObjectReference, StructuralSpace> m_RelationTypeSpaces;

        private Nullable<KeyValuePair<ModelObjectReference, HashSet<StructuralDimension>>> m_GlobalTypeExtendedDimensions;
        private Dictionary<ModelObjectReference, HashSet<StructuralDimension>> m_EntityTypeExtendedDimensions;
        private Dictionary<ModelObjectReference, HashSet<StructuralDimension>> m_RelationTypeExtendedDimensions;

        private Dictionary<ModelObjectReference, CachedTree<ModelObjectReference>> m_TypeInstanceTreesByModel;

        private Dictionary<ModelObjectReference, KeyValuePair<ModelObjectReference, StructuralPoint>> m_GlobalInstancePointsByModel;
        private Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, StructuralPoint>> m_EntityInstancePointsByModel;
        private Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, StructuralPoint>> m_RelationInstancePointsByModel;

        private Dictionary<ModelObjectReference, Dictionary<TimePeriod, Nullable<KeyValuePair<ModelObjectReference, HashSet<StructuralCoordinate>>>>> m_GlobalInstanceExtendedCoordinatesByModelPeriod;
        private Dictionary<ModelObjectReference, Dictionary<TimePeriod, Dictionary<ModelObjectReference, HashSet<StructuralCoordinate>>>> m_EntityInstanceExtendedCoordinatesByModelPeriod;
        private Dictionary<ModelObjectReference, Dictionary<TimePeriod, Dictionary<ModelObjectReference, HashSet<StructuralCoordinate>>>> m_RelationInstanceExtendedCoordinatesByModelPeriod;

        #endregion

        #region Constructors

        public StructuralMap(ModelObjectReference modelTemplateRef, ModelObjectReference globalTypeRef, IDictionary<ModelObjectReference, StructuralSpace> entityTypeSpaces, IDictionary<ModelObjectReference, StructuralSpace> relationTypeSpaces)
        {
            Dictionary<ModelObjectReference, ModelObjectReference?> childParents = new Dictionary<ModelObjectReference, ModelObjectReference?>();
            foreach (ModelObjectReference entityTypeRef in entityTypeSpaces.Keys)
            {
                IEnumerable<StructuralDimension> parentDimensions = entityTypeSpaces[entityTypeRef].Dimensions.Where(sa => sa.EntityTypeNumber != entityTypeRef.ModelObjectIdAsInt).ToList();
                Nullable<ModelObjectReference> parentEntityTypeRef = null;

                if (parentDimensions.Count() > 0)
                {
                    StructuralDimension parentDimension = parentDimensions.First();
                    parentEntityTypeRef = parentDimension.EntityTypeRef;
                }

                childParents.Add(entityTypeRef, parentEntityTypeRef);
            }

            m_EntityTypeTree = new CachedTree<ModelObjectReference>(childParents);
            m_ModelTemplateRef = modelTemplateRef;
            m_ModelInstanceRefs = new HashSet<ModelObjectReference>();

            m_EntityTypeExtendedNetwork = null;
            m_EntityInstanceTrees = new Dictionary<ModelObjectReference, CachedTree<ModelObjectReference>>();
            m_EntityInstanceExtendedTimeNetworks = new Dictionary<ModelObjectReference, CachedTimeNetwork<ModelObjectReference>>();

            m_GlobalTypeSpaces = new KeyValuePair<ModelObjectReference, StructuralSpace>(globalTypeRef, StructuralSpace.GlobalStructuralSpace);
            m_EntityTypeSpaces = entityTypeSpaces.ToDictionary(et => et.Key, et => et.Value);
            m_RelationTypeSpaces = relationTypeSpaces.ToDictionary(rt => rt.Key, rt => rt.Value);

            m_GlobalTypeExtendedDimensions = null;
            m_EntityTypeExtendedDimensions = new Dictionary<ModelObjectReference, HashSet<StructuralDimension>>();
            m_RelationTypeExtendedDimensions = new Dictionary<ModelObjectReference, HashSet<StructuralDimension>>();

            m_TypeInstanceTreesByModel = new Dictionary<ModelObjectReference, CachedTree<ModelObjectReference>>();

            m_GlobalInstancePointsByModel = new Dictionary<ModelObjectReference, KeyValuePair<ModelObjectReference, StructuralPoint>>();
            m_EntityInstancePointsByModel = new Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, StructuralPoint>>();
            m_RelationInstancePointsByModel = new Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, StructuralPoint>>();

            m_GlobalInstanceExtendedCoordinatesByModelPeriod = new Dictionary<Node, Dictionary<TimePeriod, KeyValuePair<Node, HashSet<StructuralCoordinate>>?>>();
            m_EntityInstanceExtendedCoordinatesByModelPeriod = new Dictionary<Node, Dictionary<TimePeriod, Dictionary<Node, HashSet<StructuralCoordinate>>>>();
            m_RelationInstanceExtendedCoordinatesByModelPeriod = new Dictionary<Node, Dictionary<TimePeriod, Dictionary<Node, HashSet<StructuralCoordinate>>>>();
        }

        #endregion

        #region Add Extended Structural Types

        public void AddExtendedDimensions(IDictionary<ModelObjectReference, IEnumerable<StructuralDimension>> entityTypeExtendedDimensions, IDictionary<ModelObjectReference, IEnumerable<StructuralDimension>> relationTypeExtendedDimensions)
        {
            if ((m_TypeInstanceTreesByModel.Count > 0) || (m_EntityInstancePointsByModel.Count > 0) || (m_RelationInstancePointsByModel.Count > 0)
                || (m_EntityInstanceExtendedCoordinatesByModelPeriod.Count > 0) || (m_RelationInstanceExtendedCoordinatesByModelPeriod.Count > 0))
            { throw new InvalidOperationException("Cannot add extended dimensions if Model Instance values already have been added."); }

            HashSet<ModelObjectReference> nodes = new HashSet<ModelObjectReference>();
            HashSet<IEdge> edges = new HashSet<IEdge>();

            foreach (ModelObjectReference entityTypeRef in m_EntityTypeSpaces.Keys)
            {
                nodes.Add(entityTypeRef);

                StructuralSpace baseSpace = m_EntityTypeSpaces[entityTypeRef];
                IEnumerable<StructuralDimension> extendedDimensions = new HashSet<StructuralDimension>();

                if (entityTypeExtendedDimensions.ContainsKey(entityTypeRef))
                { extendedDimensions = entityTypeExtendedDimensions[entityTypeRef]; }

                StructuralSpace extendedSpace = baseSpace.Merge(extendedDimensions);
                foreach (StructuralDimension parentDimension in extendedSpace.Dimensions)
                {
                    ModelObjectReference parentEntityTypeRef = parentDimension.EntityTypeRef;

                    if (entityTypeRef == parentEntityTypeRef)
                    { continue; }

                    edges.Add(new Edge(parentEntityTypeRef, entityTypeRef));
                }
            }

            m_EntityTypeExtendedNetwork = new CachedNetwork<ModelObjectReference>(nodes, edges);

            foreach (ModelObjectReference entityTypeRef in entityTypeExtendedDimensions.Keys)
            {
                StructuralSpace structuralSpace = m_EntityTypeSpaces[entityTypeRef];
                IEnumerable<StructuralDimension> extendedDimensions = entityTypeExtendedDimensions[entityTypeRef];

                foreach (StructuralDimension extendedDimension in extendedDimensions)
                {
                    if (structuralSpace.Dimensions.Contains(extendedDimension))
                    { continue; }

                    if (!m_EntityTypeExtendedDimensions.ContainsKey(entityTypeRef))
                    { m_EntityTypeExtendedDimensions.Add(entityTypeRef, new HashSet<StructuralDimension>()); }

                    m_EntityTypeExtendedDimensions[entityTypeRef].Add(extendedDimension);
                }
            }

            foreach (ModelObjectReference relationTypeRef in relationTypeExtendedDimensions.Keys)
            {
                StructuralSpace structuralSpace = m_RelationTypeSpaces[relationTypeRef];
                IEnumerable<StructuralDimension> extendedDimensions = relationTypeExtendedDimensions[relationTypeRef];

                foreach (StructuralDimension extendedDimension in extendedDimensions)
                {
                    if (structuralSpace.Dimensions.Contains(extendedDimension))
                    { continue; }

                    if (!m_RelationTypeExtendedDimensions.ContainsKey(relationTypeRef))
                    { m_RelationTypeExtendedDimensions.Add(relationTypeRef, new HashSet<StructuralDimension>()); }

                    m_RelationTypeExtendedDimensions[relationTypeRef].Add(extendedDimension);
                }
            }
        }

        #endregion

        #region Add Basic Structural Instances

        public void AddModelInstance(ModelObjectReference modelInstanceRef, IDictionary<ModelObjectReference, ModelObjectReference> instanceTypeRefs, ModelObjectReference globalInstanceRef, IDictionary<ModelObjectReference, StructuralPoint> entityInstancePoints, IDictionary<ModelObjectReference, StructuralPoint> relationInstancePoints)
        {
            AddModelInstance(modelInstanceRef, instanceTypeRefs, globalInstanceRef, entityInstancePoints, relationInstancePoints, new Dictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>>(), new Dictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>>());
        }

        public void AddModelInstance(ModelObjectReference modelInstanceRef, IDictionary<ModelObjectReference, ModelObjectReference> instanceTypeRefs, ModelObjectReference globalInstanceRef, IDictionary<ModelObjectReference, StructuralPoint> entityInstancePoints, IDictionary<ModelObjectReference, StructuralPoint> relationInstancePoints, Dictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>> entityInstanceForeverExtendedCoordinates, Dictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>> relationInstanceForeverExtendedCoordinates)
        {
            if ((entityInstanceForeverExtendedCoordinates.Count > 0) || (relationInstanceForeverExtendedCoordinates.Count > 0))
            {
                if (m_EntityTypeExtendedNetwork == null)
                { throw new InvalidOperationException("Cannot add extended dimensions if extended type tree is null."); }
            }
            if (m_ModelInstanceRefs.Contains(modelInstanceRef))
            { throw new InvalidOperationException("A Model Instance can only be added to the Structural Map once."); }

            var instanceTypes = instanceTypeRefs.ToList().ToDictionary(kvp => kvp.Key, kvp => nullableConverter(kvp.Value));
            instanceTypes.Add(m_GlobalTypeSpaces.Key, null);
            foreach (var entityType in m_EntityTypeSpaces.Keys)
            { instanceTypes.Add(entityType, null); }
            foreach (var relationType in m_RelationTypeSpaces.Keys)
            { instanceTypes.Add(relationType, null); }

            var childParents = new Dictionary<ModelObjectReference, ModelObjectReference?>();
            foreach (ModelObjectReference entityInstanceRef in entityInstancePoints.Keys)
            {
                IEnumerable<StructuralCoordinate> parentCoordinates = entityInstancePoints[entityInstanceRef].Coordinates.Where(sa => sa.EntityInstanceGuid != entityInstanceRef.ModelObjectId).ToList();
                if (parentCoordinates.Count() <= 0)
                {
                    childParents.Add(entityInstanceRef, null);
                }
                else
                {
                    StructuralCoordinate parentCoordinate = parentCoordinates.First();
                    ModelObjectReference parentEntityInstanceRef = new ModelObjectReference(ModelObjectType.EntityInstance, parentCoordinate.EntityInstanceGuid);
                    childParents.Add(entityInstanceRef, parentEntityInstanceRef);
                }
            }

            CachedTree<ModelObjectReference> tempRelationalTree = new CachedTree<ModelObjectReference>(instanceTypes);
            CachedTree<ModelObjectReference> tempStructuralTree = new CachedTree<ModelObjectReference>(childParents);
            CachedTimeNetwork<ModelObjectReference> tempStructuralNetwork = (m_EntityTypeExtendedNetwork != null) ? new CachedTimeNetwork<ModelObjectReference>() : null;

            m_ModelInstanceRefs.Add(modelInstanceRef);
            m_TypeInstanceTreesByModel.Add(modelInstanceRef, tempRelationalTree);
            m_EntityInstanceTrees.Add(modelInstanceRef, tempStructuralTree);
            m_EntityInstanceExtendedTimeNetworks.Add(modelInstanceRef, tempStructuralNetwork);

            m_GlobalInstancePointsByModel.Add(modelInstanceRef, new KeyValuePair<ModelObjectReference, StructuralPoint>(globalInstanceRef, StructuralPoint.GlobalStructuralPoint));
            m_EntityInstancePointsByModel.Add(modelInstanceRef, new Dictionary<ModelObjectReference, StructuralPoint>(entityInstancePoints));
            m_RelationInstancePointsByModel.Add(modelInstanceRef, new Dictionary<ModelObjectReference, StructuralPoint>(relationInstancePoints));

            m_GlobalInstanceExtendedCoordinatesByModelPeriod.Add(modelInstanceRef, null);
            m_EntityInstanceExtendedCoordinatesByModelPeriod.Add(modelInstanceRef, new Dictionary<TimePeriod, Dictionary<Node, HashSet<StructuralCoordinate>>>());
            m_RelationInstanceExtendedCoordinatesByModelPeriod.Add(modelInstanceRef, new Dictionary<TimePeriod, Dictionary<Node, HashSet<StructuralCoordinate>>>());

            if (m_EntityTypeExtendedNetwork != null)
            { AddExtendedCoordinates(modelInstanceRef, TimePeriod.ForeverPeriod, entityInstanceForeverExtendedCoordinates, relationInstanceForeverExtendedCoordinates); }
        }

        #endregion

        #region Add Extended Structural Instances

        public void AddExtendedCoordinates(ModelObjectReference modelInstanceRef, IDictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>> entityInstanceExtendedCoordinates, IDictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>> relationInstanceExtendedCoordinates)
        {
            AddExtendedCoordinates(modelInstanceRef, null, entityInstanceExtendedCoordinates, relationInstanceExtendedCoordinates);
        }

        public void AddExtendedCoordinates(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, IDictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>> entityInstanceExtendedCoordinates, IDictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>> relationInstanceExtendedCoordinates)
        {
            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }
            if (entityInstanceExtendedCoordinates == null)
            { entityInstanceExtendedCoordinates = new Dictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>>(); }
            if (relationInstanceExtendedCoordinates == null)
            { relationInstanceExtendedCoordinates = new Dictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>>(); }

            HashSet<Node> currentNodes = new HashSet<Node>();
            HashSet<IEdge> currentEdges = new HashSet<IEdge>();

            foreach (ModelObjectReference entityInstanceRef in m_EntityInstancePointsByModel[modelInstanceRef].Keys)
            {
                currentNodes.Add(entityInstanceRef);

                StructuralPoint basePoint = m_EntityInstancePointsByModel[modelInstanceRef][entityInstanceRef];
                IEnumerable<StructuralCoordinate> extendedCoordinates = new HashSet<StructuralCoordinate>();

                if (m_EntityInstanceExtendedCoordinatesByModelPeriod.ContainsKey(modelInstanceRef))
                {
                    if (m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value))
                    {
                        if (m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].ContainsKey(entityInstanceRef))
                        {
                            extendedCoordinates = extendedCoordinates.Union(m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value][entityInstanceRef]);
                        }
                    }
                }
                if (entityInstanceExtendedCoordinates.ContainsKey(entityInstanceRef))
                { extendedCoordinates = extendedCoordinates.Union(entityInstanceExtendedCoordinates[entityInstanceRef]); }

                StructuralPoint extendedPoint = basePoint.Merge(extendedCoordinates);
                foreach (StructuralCoordinate parentCoordinate in extendedPoint.Coordinates)
                {
                    ModelObjectReference parentEntityInstanceRef = parentCoordinate.EntityInstanceRef;

                    if (entityInstanceRef == parentEntityInstanceRef)
                    { continue; }

                    currentEdges.Add(new Edge(parentEntityInstanceRef, entityInstanceRef));
                }
            }


            CachedTimeNetwork<ModelObjectReference> existingNetwork = m_EntityInstanceExtendedTimeNetworks[modelInstanceRef];

            if (existingNetwork == null)
            { return; }

            bool alreadyHasValuesForTimePeriod = (existingNetwork.GetRelevantPeriods(desiredPeriod.Value).Count > 0);
            ICollection<ModelObjectReference> existingNodesForPeriod = new List<ModelObjectReference>();
            ICollection<IEdge<ModelObjectReference>> existingEdgesForPeriod = new List<IEdge<ModelObjectReference>>();

            if (alreadyHasValuesForTimePeriod)
            {
                existingNodesForPeriod = existingNetwork.GetAllNodesForPeriod(desiredPeriod.Value);
                existingEdgesForPeriod = existingNetwork.GetAllEdgesForPeriod(desiredPeriod.Value);
            }

            List<Node> nodesToAdd = currentNodes.Where(n => !existingNodesForPeriod.Contains(n)).Distinct().ToList();
            List<IEdge> edgesToAdd = currentEdges.Where(e => !existingEdgesForPeriod.Contains(e)).Distinct().ToList();
            List<Node> nodesToRemove = existingNodesForPeriod.Where(n => !currentNodes.Contains(n)).Distinct().ToList();
            List<IEdge> edgesToRemove = existingEdgesForPeriod.Where(e => !currentEdges.Contains(e)).Distinct().ToList();

            existingNetwork.UpdateValues(desiredPeriod.Value, nodesToAdd, edgesToAdd, nodesToRemove, edgesToRemove);


            foreach (ModelObjectReference entityInstanceRef in entityInstanceExtendedCoordinates.Keys)
            {
                StructuralPoint structuralPoint = m_EntityInstancePointsByModel[modelInstanceRef][entityInstanceRef];
                IEnumerable<StructuralCoordinate> extendedCoordinates = entityInstanceExtendedCoordinates[entityInstanceRef];

                foreach (StructuralCoordinate extendedCoordinate in extendedCoordinates)
                {
                    if (structuralPoint.Coordinates.Contains(extendedCoordinate))
                    { continue; }

                    if (!m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value))
                    { m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].Add(desiredPeriod.Value, new Dictionary<Node, HashSet<StructuralCoordinate>>()); }

                    if (!m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].ContainsKey(entityInstanceRef))
                    { m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].Add(entityInstanceRef, new HashSet<StructuralCoordinate>()); }

                    m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value][entityInstanceRef].Add(extendedCoordinate);
                }
            }

            foreach (ModelObjectReference relationInstanceRef in relationInstanceExtendedCoordinates.Keys)
            {
                StructuralPoint structuralPoint = m_RelationInstancePointsByModel[modelInstanceRef][relationInstanceRef];
                IEnumerable<StructuralCoordinate> extendedCoordinates = relationInstanceExtendedCoordinates[relationInstanceRef];

                foreach (StructuralCoordinate extendedCoordinate in extendedCoordinates)
                {
                    if (structuralPoint.Coordinates.Contains(extendedCoordinate))
                    { continue; }

                    if (!m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value))
                    { m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].Add(desiredPeriod.Value, new Dictionary<Node, HashSet<StructuralCoordinate>>()); }

                    if (!m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].ContainsKey(relationInstanceRef))
                    { m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].Add(relationInstanceRef, new HashSet<StructuralCoordinate>()); }

                    m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value][relationInstanceRef].Add(extendedCoordinate);
                }
            }
        }

        public void UpdateExtendedCoordinates(ModelObjectReference modelInstanceRef, IDictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>> entityInstanceExtendedCoordinatesByDimension, IDictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>> relationInstanceExtendedCoordinatesByDimension)
        {
            UpdateExtendedCoordinates(modelInstanceRef, null, entityInstanceExtendedCoordinatesByDimension, relationInstanceExtendedCoordinatesByDimension);
        }

        public void UpdateExtendedCoordinates(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, IDictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>> entityInstanceExtendedCoordinatesByDimension, IDictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>> relationInstanceExtendedCoordinatesByDimension)
        {
            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }
            if (entityInstanceExtendedCoordinatesByDimension == null)
            { entityInstanceExtendedCoordinatesByDimension = new Dictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>>(); }
            if (relationInstanceExtendedCoordinatesByDimension == null)
            { relationInstanceExtendedCoordinatesByDimension = new Dictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>>(); }

            HashSet<Node> currentNodes = new HashSet<Node>();
            HashSet<IEdge> currentEdges = new HashSet<IEdge>();

            foreach (ModelObjectReference entityInstanceRef in m_EntityInstancePointsByModel[modelInstanceRef].Keys)
            {
                currentNodes.Add(entityInstanceRef);

                StructuralPoint basePoint = m_EntityInstancePointsByModel[modelInstanceRef][entityInstanceRef];
                Dictionary<StructuralDimension, StructuralCoordinate> extendedCoordinatesByDimention = new Dictionary<StructuralDimension, StructuralCoordinate>();

                if (m_EntityInstanceExtendedCoordinatesByModelPeriod.ContainsKey(modelInstanceRef))
                {
                    if (m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value))
                    {
                        if (m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].ContainsKey(entityInstanceRef))
                        {
                            extendedCoordinatesByDimention = m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value][entityInstanceRef].ToDictionary(x => x.Dimension, x => x);
                        }
                    }
                }
                if (entityInstanceExtendedCoordinatesByDimension.ContainsKey(entityInstanceRef))
                {
                    var updatedCoordinatesByDimension = entityInstanceExtendedCoordinatesByDimension[entityInstanceRef];

                    foreach (var updatedStructuralDimension in updatedCoordinatesByDimension.Keys)
                    {
                        var updatedStructuralCoordinate = updatedCoordinatesByDimension[updatedStructuralDimension];

                        if (!updatedStructuralCoordinate.HasValue)
                        {
                            if (extendedCoordinatesByDimention.ContainsKey(updatedStructuralDimension))
                            { extendedCoordinatesByDimention.Remove(updatedStructuralDimension); }
                        }
                        else
                        {
                            extendedCoordinatesByDimention[updatedStructuralDimension] = updatedStructuralCoordinate.Value;
                        }
                    }
                }

                StructuralPoint extendedPoint = basePoint.Merge(extendedCoordinatesByDimention.Values);
                foreach (StructuralCoordinate parentCoordinate in extendedPoint.Coordinates)
                {
                    ModelObjectReference parentEntityInstanceRef = parentCoordinate.EntityInstanceRef;

                    if (entityInstanceRef == parentEntityInstanceRef)
                    { continue; }

                    currentEdges.Add(new Edge(parentEntityInstanceRef, entityInstanceRef));
                }
            }


            CachedTimeNetwork<ModelObjectReference> existingNetwork = m_EntityInstanceExtendedTimeNetworks[modelInstanceRef];

            if (existingNetwork == null)
            { return; }

            bool alreadyHasValuesForTimePeriod = (existingNetwork.GetRelevantPeriods(desiredPeriod.Value).Count > 0);
            ICollection<ModelObjectReference> existingNodesForPeriod = new List<ModelObjectReference>();
            ICollection<IEdge<ModelObjectReference>> existingEdgesForPeriod = new List<IEdge<ModelObjectReference>>();

            if (alreadyHasValuesForTimePeriod)
            {
                existingNodesForPeriod = existingNetwork.GetAllNodesForPeriod(desiredPeriod.Value);
                existingEdgesForPeriod = existingNetwork.GetAllEdgesForPeriod(desiredPeriod.Value);
            }

            List<Node> nodesToAdd = currentNodes.Where(n => !existingNodesForPeriod.Contains(n)).Distinct().ToList();
            List<IEdge> edgesToAdd = currentEdges.Where(e => !existingEdgesForPeriod.Contains(e)).Distinct().ToList();
            List<Node> nodesToRemove = existingNodesForPeriod.Where(n => !currentNodes.Contains(n)).Distinct().ToList();
            List<IEdge> edgesToRemove = existingEdgesForPeriod.Where(e => !currentEdges.Contains(e)).Distinct().ToList();

            existingNetwork.UpdateValues(desiredPeriod.Value, nodesToAdd, edgesToAdd, nodesToRemove, edgesToRemove);


            foreach (ModelObjectReference entityInstanceRef in entityInstanceExtendedCoordinatesByDimension.Keys)
            {
                var existingStructuralPoint = m_EntityInstancePointsByModel[modelInstanceRef][entityInstanceRef];
                var updatedExtendedCoordinatesByDimension = entityInstanceExtendedCoordinatesByDimension[entityInstanceRef];

                foreach (var extendedDimension in updatedExtendedCoordinatesByDimension.Keys)
                {
                    var extendedCoordinate = updatedExtendedCoordinatesByDimension[extendedDimension];
                    var removeCoordinate = false;
                    var updateCoordinate = false;

                    if (existingStructuralPoint.CoordinatesByDimension.Keys.Contains(extendedDimension))
                    {
                        if (!extendedCoordinate.HasValue)
                        { removeCoordinate = true; }
                        else if (existingStructuralPoint.Coordinates.Contains(extendedCoordinate.Value))
                        { continue; }
                        else if (existingStructuralPoint.CoordinatesByDimension.ContainsKey(extendedDimension))
                        { updateCoordinate = true; }
                    }

                    if (!m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value))
                    { m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].Add(desiredPeriod.Value, new Dictionary<Node, HashSet<StructuralCoordinate>>()); }

                    if (!m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].ContainsKey(entityInstanceRef))
                    { m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].Add(entityInstanceRef, new HashSet<StructuralCoordinate>()); }

                    if (removeCoordinate || updateCoordinate)
                    {
                        var storedCoordinates = m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value][entityInstanceRef];
                        var coordinateToRemoves = storedCoordinates.Where(x => x.Dimension == extendedDimension).ToList();

                        if (coordinateToRemoves.Count != 1)
                        { throw new InvalidOperationException(); }

                        storedCoordinates.Remove(coordinateToRemoves.First());
                    }
                    if (extendedCoordinate.HasValue)
                    {
                        m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value][entityInstanceRef].Add(extendedCoordinate.Value);
                    }
                }
            }

            foreach (ModelObjectReference relationInstanceRef in relationInstanceExtendedCoordinatesByDimension.Keys)
            {
                var existingStructuralPoint = m_RelationInstancePointsByModel[modelInstanceRef][relationInstanceRef];
                var updatedExtendedCoordinatesByDimension = relationInstanceExtendedCoordinatesByDimension[relationInstanceRef];

                foreach (var extendedDimension in updatedExtendedCoordinatesByDimension.Keys)
                {
                    var extendedCoordinate = updatedExtendedCoordinatesByDimension[extendedDimension];
                    var removeCoordinate = false;
                    var updateCoordinate = false;

                    if (existingStructuralPoint.CoordinatesByDimension.Keys.Contains(extendedDimension))
                    {
                        if (!extendedCoordinate.HasValue)
                        { removeCoordinate = true; }
                        else if (existingStructuralPoint.Coordinates.Contains(extendedCoordinate.Value))
                        { continue; }
                        else if (existingStructuralPoint.CoordinatesByDimension.ContainsKey(extendedDimension))
                        { updateCoordinate = true; }
                    }

                    if (!m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value))
                    { m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].Add(desiredPeriod.Value, new Dictionary<Node, HashSet<StructuralCoordinate>>()); }

                    if (!m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].ContainsKey(relationInstanceRef))
                    { m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].Add(relationInstanceRef, new HashSet<StructuralCoordinate>()); }

                    if (removeCoordinate || updateCoordinate)
                    {
                        var storedCoordinates = m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value][relationInstanceRef];
                        var coordinateToRemoves = storedCoordinates.Where(x => x.Dimension == extendedDimension).ToList();

                        if (coordinateToRemoves.Count != 1)
                        { throw new InvalidOperationException(); }

                        storedCoordinates.Remove(coordinateToRemoves.First());
                    }
                    if (extendedCoordinate.HasValue)
                    {
                        m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value][relationInstanceRef].Add(extendedCoordinate.Value);
                    }
                }
            }
        }

        public void RemoveExtendedCoordinates(ModelObjectReference modelInstanceRef, IEnumerable<ModelObjectReference> entityInstanceRefs, IEnumerable<ModelObjectReference> relationInstanceRefs)
        {
            RemoveExtendedCoordinates(modelInstanceRef, null, entityInstanceRefs, relationInstanceRefs);
        }

        public void RemoveExtendedCoordinates(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, IEnumerable<ModelObjectReference> entityInstanceRefs, IEnumerable<ModelObjectReference> relationInstanceRefs)
        {
            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }
            if (entityInstanceRefs == null)
            { entityInstanceRefs = new List<ModelObjectReference>(); }
            if (relationInstanceRefs == null)
            { relationInstanceRefs = new List<ModelObjectReference>(); }

            foreach (ModelObjectReference entityInstanceRef in entityInstanceRefs)
            {
                if (m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value))
                {
                    if (m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].ContainsKey(entityInstanceRef))
                    { m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].Remove(entityInstanceRef); }
                }
            }
            foreach (ModelObjectReference relationInstanceRef in relationInstanceRefs)
            {
                if (m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value))
                {
                    if (m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].ContainsKey(relationInstanceRef))
                    { m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].Remove(relationInstanceRef); }
                }
            }
            AddExtendedCoordinates(modelInstanceRef, desiredPeriod, null, null);
        }

        public void ClearExtendedCoordinates(ModelObjectReference modelInstanceRef)
        {
            ClearExtendedCoordinates(modelInstanceRef, null);
        }

        public void ClearExtendedCoordinates(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod)
        {
            IEnumerable<ModelObjectReference> entityInstanceRefs = m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].Keys;
            IEnumerable<ModelObjectReference> relationInstanceRefs = m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value].Keys;
            RemoveExtendedCoordinates(modelInstanceRef, desiredPeriod, entityInstanceRefs, relationInstanceRefs);
        }

        #endregion

        #region Properties and Getter Methods

        public bool IsTypeMapValid
        {
            get
            {
                if (m_EntityTypeTree.HasCycles(false))
                { return false; }
                if (m_EntityTypeExtendedNetwork == null)
                { return true; }
                if (m_EntityTypeExtendedNetwork.HasCycles(true))
                { return false; }
                return true;
            }
        }

        public ModelObjectReference ModelTemplateRef
        {
            get { return m_ModelTemplateRef; }
        }

        public ModelObjectReference GlobalTypeRef
        {
            get { return m_GlobalTypeSpaces.Key; }
        }

        public IEnumerable<ModelObjectReference> EntityTypeRefs
        {
            get { return m_EntityTypeSpaces.Keys; }
        }

        public IEnumerable<ModelObjectReference> RelationTypeRefs
        {
            get { return m_RelationTypeSpaces.Keys; }
        }

        public ITree<ModelObjectReference> EntityTypeTree
        {
            get { return m_EntityTypeTree; }
        }

        public INetwork<ModelObjectReference> EntityTypeExtendedNetwork
        {
            get { return m_EntityTypeExtendedNetwork; }
        }

        public IEnumerable<ModelObjectReference> ModelInstanceRefs
        {
            get { return m_ModelInstanceRefs.ToList(); }
        }

        public ICollection<TimePeriod> GetDefinedPeriodsForExtendedStructure(ModelObjectReference modelInstanceRef)
        {
            if (!m_EntityInstanceExtendedTimeNetworks.ContainsKey(modelInstanceRef))
            { return new HashSet<TimePeriod>(); }
            if (m_EntityInstanceExtendedTimeNetworks[modelInstanceRef] == null)
            { return new HashSet<TimePeriod>(); }

            var timeNetwork = m_EntityInstanceExtendedTimeNetworks[modelInstanceRef];
            var validPeriods = timeNetwork.TimePeriodsPlusForever;
            return validPeriods;
        }

        public bool GetIsInstanceMapValid(ModelObjectReference modelInstanceRef)
        {
            if (!m_EntityInstanceTrees.ContainsKey(modelInstanceRef))
            { return false; }

            CachedTree<ModelObjectReference> entityInstanceTree = m_EntityInstanceTrees[modelInstanceRef];
            CachedTimeNetwork<ModelObjectReference> entityInstanceExtendedTimeNetwork = null;

            if (m_EntityInstanceExtendedTimeNetworks.ContainsKey(modelInstanceRef))
            { entityInstanceExtendedTimeNetwork = m_EntityInstanceExtendedTimeNetworks[modelInstanceRef]; }

            if (entityInstanceTree.HasCycles(false))
            { return false; }
            if ((m_EntityTypeExtendedNetwork == null) && (entityInstanceExtendedTimeNetwork == null))
            { return true; }
            if (entityInstanceExtendedTimeNetwork == null)
            { return false; }
            foreach (TimePeriod assessedPeriod in entityInstanceExtendedTimeNetwork.TimePeriodsPlusForever)
            {
                if (entityInstanceExtendedTimeNetwork.HasCycles(assessedPeriod, true))
                { return false; }
            }
            return true;
        }

        public ModelObjectReference GetGlobalInstanceRef(ModelObjectReference modelInstanceRef)
        {
            return m_GlobalInstancePointsByModel[modelInstanceRef].Key;
        }

        public IEnumerable<ModelObjectReference> GetEntityInstanceRefs(ModelObjectReference modelInstanceRef)
        {
            return m_EntityInstancePointsByModel[modelInstanceRef].Keys;
        }

        public IEnumerable<ModelObjectReference> GetRelationInstanceRefs(ModelObjectReference modelInstanceRef)
        {
            return m_RelationInstancePointsByModel[modelInstanceRef].Keys;
        }

        public ITree<ModelObjectReference> GetEntityInstanceTree(ModelObjectReference modelInstanceRef)
        {
            return m_EntityInstanceTrees[modelInstanceRef];
        }

        public ITimeNetwork<ModelObjectReference> GetEntityInstanceExtendedTimeNetwork(ModelObjectReference modelInstanceRef)
        {
            return m_EntityInstanceExtendedTimeNetworks[modelInstanceRef];
        }

        public ICollection<ModelObjectReference> StructuralTypeRefs
        {
            get
            {
                var structuralTypeRefs = new List<ModelObjectReference>();
                structuralTypeRefs.Add(m_GlobalTypeSpaces.Key);
                structuralTypeRefs.AddRange(m_EntityTypeSpaces.Keys);
                structuralTypeRefs.AddRange(m_RelationTypeSpaces.Keys);
                return structuralTypeRefs.ToHashSet();
            }
        }

        public IDictionary<ModelObjectReference, ICollection<ModelObjectReference>> StructuralInstanceRefsByModelInstanceRef
        {
            get
            {
                var structuralInstanceRefsDict = new SortedDictionary<ModelObjectReference, ICollection<ModelObjectReference>>();

                foreach (var modelInstanceRef in ModelInstanceRefs)
                {
                    var structuralInstanceRefs = new List<ModelObjectReference>();
                    structuralInstanceRefs.Add(m_GlobalInstancePointsByModel[modelInstanceRef].Key);
                    structuralInstanceRefs.AddRange(m_EntityInstancePointsByModel[modelInstanceRef].Keys);
                    structuralInstanceRefs.AddRange(m_RelationInstancePointsByModel[modelInstanceRef].Keys);

                    structuralInstanceRefsDict.Add(modelInstanceRef, structuralInstanceRefs.ToHashSet());
                }
                return structuralInstanceRefsDict;
            }
        }

        #endregion

        #region Structural Path Validator Methods

        public StructuralPathValidator CreatePathValidator()
        {
            var pathValidator = new StructuralPathValidator(m_EntityTypeTree, m_EntityTypeExtendedNetwork);
            return pathValidator;
        }

        #endregion

        #region Structural Space Methods

        public StructuralSpace GetBaseStructuralSpace(ModelObjectReference structuralTypeRef)
        {
            if (ModelObjectTypeUtils.StructuralInstanceValues.Contains(structuralTypeRef.ModelObjectType))
            { structuralTypeRef = GetStructuralTypeForInstance(structuralTypeRef); }

            if (!ModelObjectTypeUtils.StructuralTypeValues.Contains(structuralTypeRef.ModelObjectType))
            { throw new InvalidOperationException("The supplied reference must point to a structural value."); }

            if (structuralTypeRef.ModelObjectType == ModelObjectType.GlobalType)
            {
                return m_GlobalTypeSpaces.Value;
            }
            else if (structuralTypeRef.ModelObjectType == ModelObjectType.EntityType)
            {
                StructuralSpace entityTypeSpace = m_EntityTypeSpaces[structuralTypeRef];
                entityTypeSpace = new StructuralSpace(entityTypeSpace, structuralTypeRef.AlternateDimensionNumber);
                return entityTypeSpace;
            }
            else if (structuralTypeRef.ModelObjectType == ModelObjectType.RelationType)
            {
                return m_RelationTypeSpaces[structuralTypeRef];
            }
            else
            { throw new ApplicationException("The specified Structural Type does not exist in the current Model Template."); }
        }

        public StructuralSpace GetExtendedStructuralSpace(ModelObjectReference structuralTypeRef)
        {
            if (ModelObjectTypeUtils.StructuralInstanceValues.Contains(structuralTypeRef.ModelObjectType))
            { structuralTypeRef = GetStructuralTypeForInstance(structuralTypeRef); }

            if (!ModelObjectTypeUtils.StructuralTypeValues.Contains(structuralTypeRef.ModelObjectType))
            { throw new InvalidOperationException("The supplied reference must point to a structural value."); }

            if (structuralTypeRef.ModelObjectType == ModelObjectType.GlobalType)
            {
                return m_GlobalTypeSpaces.Value;
            }
            else if (structuralTypeRef.ModelObjectType == ModelObjectType.EntityType)
            {
                StructuralSpace baseSpace = m_EntityTypeSpaces[structuralTypeRef];
                IEnumerable<StructuralDimension> extendedDimensions = new HashSet<StructuralDimension>();

                if (m_EntityTypeExtendedDimensions.ContainsKey(structuralTypeRef))
                { extendedDimensions = m_EntityTypeExtendedDimensions[structuralTypeRef]; }

                return baseSpace.Merge(extendedDimensions, structuralTypeRef.AlternateDimensionNumber);
            }
            else if (structuralTypeRef.ModelObjectType == ModelObjectType.RelationType)
            {
                StructuralSpace baseSpace = m_RelationTypeSpaces[structuralTypeRef];
                IEnumerable<StructuralDimension> extendedDimensions = new HashSet<StructuralDimension>();

                if (m_RelationTypeExtendedDimensions.ContainsKey(structuralTypeRef))
                { extendedDimensions = m_RelationTypeExtendedDimensions[structuralTypeRef]; }

                return baseSpace.Merge(extendedDimensions);
            }
            else
            { throw new ApplicationException("The specified Structural Type does not exist in the current Model Template."); }
        }

        public StructuralSpace GetStructuralSpace(ModelObjectReference structuralTypeRef, bool useExtendedStructure)
        {
            if (!useExtendedStructure)
            { return GetBaseStructuralSpace(structuralTypeRef); }
            else
            { return GetExtendedStructuralSpace(structuralTypeRef); }
        }

        #endregion

        #region Structural Point Methods

        public StructuralPoint GetBaseStructuralPoint(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef)
        {
            var structuralInstanceRefs = new ModelObjectReference[] { structuralInstanceRef };
            var results = GetBaseStructuralPoints(modelInstanceRef, ModelObjectReference.EmptyReference, structuralInstanceRefs);

            if (!results.ContainsKey(structuralInstanceRef))
            { throw new InvalidOperationException("The specified StructuralInstanceRef does not exist."); }

            return results[structuralInstanceRef];
        }

        public IDictionary<ModelObjectReference, StructuralPoint> GetBaseStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs)
        {
            if (!m_ModelInstanceRefs.Contains(modelInstanceRef))
            { throw new InvalidOperationException("The specified Model Instance has not been loaded into the StructuralMap."); }

            ModelObjectType? modelObjectType = null;
            var results = new Dictionary<ModelObjectReference, StructuralPoint>(ModelObjectReference.DimensionalComparer);

            var modelObjectTypes = structuralInstanceRefs.Select(x => x.ModelObjectType).Distinct().ToList();
            if (modelObjectTypes.Count > 1)
            { throw new InvalidOperationException("All Instances must be of same Type."); }
            if (modelObjectTypes.Count == 1)
            {
                modelObjectType = modelObjectTypes.First();

                if (!ModelObjectTypeUtils.StructuralInstanceValues.Contains(modelObjectType.Value))
                { throw new InvalidOperationException("The supplied reference must point to a structural value."); }
            }

            if (modelObjectType == ModelObjectType.GlobalInstance)
            {
                if (structuralInstanceRefs.Count() > 1)
                { throw new InvalidOperationException("Only one Global Instance is supported per Model Instance."); }

                results.Add(structuralInstanceRefs.First(), m_GlobalInstancePointsByModel[modelInstanceRef].Value);
            }
            else if (modelObjectType == ModelObjectType.EntityInstance)
            {
                var pointsForModel = m_EntityInstancePointsByModel[modelInstanceRef];

                foreach (var structuralInstanceRef in structuralInstanceRefs)
                {
                    var entityTypePoint = pointsForModel[structuralInstanceRef];
                    entityTypePoint = new StructuralPoint(entityTypePoint, structuralInstanceRef.AlternateDimensionNumber);
                    results.Add(structuralInstanceRef, entityTypePoint);
                }
            }
            else if (modelObjectType == ModelObjectType.RelationInstance)
            {
                var pointsForModel = m_RelationInstancePointsByModel[modelInstanceRef];

                foreach (var structuralInstanceRef in structuralInstanceRefs)
                {
                    var relationTypePoint = pointsForModel[structuralInstanceRef];
                    results.Add(structuralInstanceRef, relationTypePoint);
                }
            }
            else if (modelObjectType.HasValue)
            {
                throw new ApplicationException("The specified Structural Type does not exist in the current Model Instance.");
            }
            return results;
        }

        public StructuralPoint GetExtendedStructuralPoint(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef)
        {
            var structuralInstanceRefs = new ModelObjectReference[] { structuralInstanceRef };
            var results = GetExtendedStructuralPoints(modelInstanceRef, ModelObjectReference.EmptyReference, structuralInstanceRefs);

            if (!results.ContainsKey(structuralInstanceRef))
            { throw new InvalidOperationException("The specified StructuralInstanceRef does not exist."); }

            return results[structuralInstanceRef];
        }

        public IDictionary<ModelObjectReference, StructuralPoint> GetExtendedStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs)
        {
            return GetExtendedStructuralPoints(modelInstanceRef, null, structuralTypeRef, structuralInstanceRefs);
        }

        public StructuralPoint GetExtendedStructuralPoint(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralInstanceRef)
        {
            var structuralInstanceRefs = new ModelObjectReference[] { structuralInstanceRef };
            var results = GetExtendedStructuralPoints(modelInstanceRef, desiredPeriod, ModelObjectReference.EmptyReference, structuralInstanceRefs);

            if (!results.ContainsKey(structuralInstanceRef))
            { throw new InvalidOperationException("The specified StructuralInstanceRef does not exist."); }

            return results[structuralInstanceRef];
        }

        public IDictionary<ModelObjectReference, StructuralPoint> GetExtendedStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs)
        {
            if (structuralInstanceRefs.Count() < 1)
            { return new Dictionary<ModelObjectReference, StructuralPoint>(ModelObjectReference.DimensionalComparer); }

            if (!m_ModelInstanceRefs.Contains(modelInstanceRef))
            { throw new InvalidOperationException("The specified Model Instance has not been loaded into the StructuralMap."); }

            var typeInstanceTreeForModel = m_TypeInstanceTreesByModel[modelInstanceRef];
            var firstStructuralInstanceRef = structuralInstanceRefs.First();
            var firstStructuralTypeRef = typeInstanceTreeForModel.GetParent(firstStructuralInstanceRef);
            if (!firstStructuralTypeRef.HasValue)
            { throw new InvalidOperationException("The supplied reference must point to a Structural value."); }

            structuralTypeRef = ModelObjectTypeUtils.StructuralInstanceValues.Contains(structuralTypeRef.ModelObjectType) ? structuralTypeRef : firstStructuralTypeRef.Value;
            if (!ModelObjectTypeUtils.StructuralTypeValues.Contains(structuralTypeRef.ModelObjectType))
            { throw new InvalidOperationException("The supplied reference must point to a Structural value."); }

            desiredPeriod = desiredPeriod.HasValue ? desiredPeriod : TimePeriod.ForeverPeriod;
            if (!GetDefinedPeriodsForExtendedStructure(modelInstanceRef).Contains(desiredPeriod.Value))
            { return GetBaseStructuralPoints(modelInstanceRef, structuralTypeRef, structuralInstanceRefs); }

            var instanceObjectTypes = structuralInstanceRefs.Select(x => x.ModelObjectType).Distinct().ToList();
            if (instanceObjectTypes.Count > 1)
            { throw new InvalidOperationException("All Structural Instances must be of same Structural Type."); }

            var firstInstanceObjectType = instanceObjectTypes.First();
            if (firstInstanceObjectType != structuralTypeRef.ModelObjectType.GetInstanceForType())
            { throw new InvalidOperationException("The Structural Instances are of the wrong Structural Type."); }

            var useExtendedStructure = true;
            var structuralTypeSpace = GetStructuralSpace(structuralTypeRef, useExtendedStructure);
            var results = new Dictionary<ModelObjectReference, StructuralPoint>(ModelObjectReference.DimensionalComparer);


            if (firstInstanceObjectType == ModelObjectType.GlobalInstance)
            {
                if (structuralInstanceRefs.Count() > 1)
                { throw new InvalidOperationException("Only one Global Instance is supported per Model Instance."); }

                results.Add(structuralInstanceRefs.First(), m_GlobalInstancePointsByModel[modelInstanceRef].Value);
            }
            else if (firstInstanceObjectType == ModelObjectType.EntityInstance)
            {
                var pointsForModel = m_EntityInstancePointsByModel[modelInstanceRef];
                var coordsForModel = m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value) ? m_EntityInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value] : null;

                foreach (var structuralInstanceRef in structuralInstanceRefs)
                {
                    var resultingPoint = GetMergedStructuralPointForInstance(structuralTypeRef, structuralTypeSpace, structuralInstanceRef, typeInstanceTreeForModel, pointsForModel, coordsForModel);
                    results.Add(structuralInstanceRef, resultingPoint);
                }
            }
            else if (firstInstanceObjectType == ModelObjectType.RelationInstance)
            {
                var pointsForModel = m_RelationInstancePointsByModel[modelInstanceRef];
                var coordsForModel = m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef].ContainsKey(desiredPeriod.Value) ? m_RelationInstanceExtendedCoordinatesByModelPeriod[modelInstanceRef][desiredPeriod.Value] : null;

                foreach (var structuralInstanceRef in structuralInstanceRefs)
                {
                    var resultingPoint = GetMergedStructuralPointForInstance(structuralTypeRef, structuralTypeSpace, structuralInstanceRef, typeInstanceTreeForModel, pointsForModel, coordsForModel);
                    results.Add(structuralInstanceRef, resultingPoint);
                }
            }
            else
            { throw new ApplicationException("The specified Structural Type does not exist in the current Model Instance."); }

            return results;
        }

        private static StructuralPoint GetMergedStructuralPointForInstance(ModelObjectReference structuralTypeRef, StructuralSpace structuralTypeSpace, ModelObjectReference structuralInstanceRef, ITree<ModelObjectReference> typeInstanceTreeForModel, Dictionary<ModelObjectReference, StructuralPoint> pointsForModel, Dictionary<ModelObjectReference, HashSet<StructuralCoordinate>> coordsForModel)
        {
            var sourceTypeRef = typeInstanceTreeForModel.GetParent(structuralInstanceRef);

            if (!sourceTypeRef.HasValue)
            { throw new InvalidOperationException("The Structural Instance is not of a known Structural Type."); }
            if (!ModelObjectReference.DimensionalComparer.Equals(structuralTypeRef, sourceTypeRef.Value))
            { throw new InvalidOperationException("The Structural Instance is of the wrong Structural Type."); }

            var basePoint = pointsForModel[structuralInstanceRef];
            var extendedCoordinates = new HashSet<StructuralCoordinate>();

            if (coordsForModel != null)
            {
                if (coordsForModel.ContainsKey(structuralInstanceRef))
                { extendedCoordinates = coordsForModel[structuralInstanceRef]; }
            }

            var expectedCoordCount = structuralTypeSpace.Dimensions.Count;
            var actualCoordCount = basePoint.Coordinates.Count + extendedCoordinates.Count();

            if (expectedCoordCount != actualCoordCount)
            {
                var fixedDims = basePoint.CoordinatesByDimension.Keys.Union(extendedCoordinates.Select(x => x.Dimension)).Distinct().ToList();

                foreach (var requiredDim in structuralTypeSpace.Dimensions)
                {
                    if (fixedDims.Contains(requiredDim))
                    { continue; }

                    extendedCoordinates.Add(new StructuralCoordinate(requiredDim.EntityTypeNumber, requiredDim.EntityDimensionNumber, Guid.Empty, requiredDim.DimensionType, requiredDim.UsesTimeDimension));
                }
            }

            var resultingPoint = new StructuralPoint(basePoint, extendedCoordinates, structuralInstanceRef.AlternateDimensionNumber);
            return resultingPoint;
        }

        public StructuralPoint GetStructuralPoint(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef, bool useExtendedStructure)
        {
            var structuralInstanceRefs = new ModelObjectReference[] { structuralInstanceRef };
            var results = GetStructuralPoints(modelInstanceRef, ModelObjectReference.EmptyReference, structuralInstanceRefs, useExtendedStructure);

            if (!results.ContainsKey(structuralInstanceRef))
            { throw new InvalidOperationException("The specified StructuralInstanceRef does not exist."); }

            return results[structuralInstanceRef];
        }

        public IDictionary<ModelObjectReference, StructuralPoint> GetStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs, bool useExtendedStructure)
        {
            return GetStructuralPoints(modelInstanceRef, null, structuralTypeRef, structuralInstanceRefs, useExtendedStructure);
        }

        public StructuralPoint GetStructuralPoint(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralInstanceRef, bool useExtendedStructure)
        {
            var structuralInstanceRefs = new ModelObjectReference[] { structuralInstanceRef };
            var results = GetStructuralPoints(modelInstanceRef, desiredPeriod, ModelObjectReference.EmptyReference, structuralInstanceRefs, useExtendedStructure);

            if (!results.ContainsKey(structuralInstanceRef))
            { throw new InvalidOperationException("The specified StructuralInstanceRef does not exist."); }

            return results[structuralInstanceRef];
        }

        public IDictionary<ModelObjectReference, StructuralPoint> GetStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs, bool useExtendedStructure)
        {
            if (!useExtendedStructure)
            { return GetBaseStructuralPoints(modelInstanceRef, structuralTypeRef, structuralInstanceRefs); }
            else
            { return GetExtendedStructuralPoints(modelInstanceRef, desiredPeriod, structuralTypeRef, structuralInstanceRefs); }
        }

        public IDictionary<StructuralPoint, IDictionary<ModelObjectReference, StructuralPoint>> GetRelatedStructuralInstances(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef, IEnumerable<StructuralPoint> relatedStructuralPoints, bool useExtendedStructure)
        {
            return GetRelatedStructuralInstances(modelInstanceRef, null, structuralTypeRef, relatedStructuralPoints, useExtendedStructure);
        }

        public IDictionary<StructuralPoint, IDictionary<ModelObjectReference, StructuralPoint>> GetRelatedStructuralInstances(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralTypeRef, IEnumerable<StructuralPoint> relatedStructuralPoints, bool useExtendedStructure)
        {
            DimensionalModelObjectReferenceComparer dimesionalComparer = new DimensionalModelObjectReferenceComparer();

            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }

            if (relatedStructuralPoints.Count() <= 0)
            { return new Dictionary<StructuralPoint, IDictionary<ModelObjectReference, StructuralPoint>>(); }

            StructuralSpace firstRelatedSpace = relatedStructuralPoints.First().Space;
            int nonMatchingSpaces = relatedStructuralPoints.Where(sp => !firstRelatedSpace.Equals(sp.Space)).ToList().Count;
            if (nonMatchingSpaces > 1)
            { throw new InvalidOperationException("All StructuralSpaces for related StructuralPoints must be the same."); }

            int nonGlobalCount = relatedStructuralPoints.Where(sp => !StructuralPoint.GlobalStructuralPoint.Equals(sp)).ToList().Count;
            if (nonGlobalCount <= 0)
            {
                var globalOutputStructuralPoints = new Dictionary<StructuralPoint, IDictionary<ModelObjectReference, StructuralPoint>>();
                var globalStructuralPointsByInstance = GetStructuralInstancesForType(modelInstanceRef, structuralTypeRef).ToDictionary(rf => rf, rf => GetStructuralPoint(modelInstanceRef, desiredPeriod.Value, rf, useExtendedStructure), dimesionalComparer);
                globalOutputStructuralPoints.Add(StructuralPoint.GlobalStructuralPoint, globalStructuralPointsByInstance);
                return globalOutputStructuralPoints;
            }

            StructuralSpace spaceForType = GetStructuralSpace(structuralTypeRef, useExtendedStructure);
            ICollection<ModelObjectReference> structuralInstancesForType = GetStructuralInstancesForType(modelInstanceRef, structuralTypeRef);

            var structuralPointsByInstance = GetStructuralPoints(modelInstanceRef, desiredPeriod.Value, structuralTypeRef, structuralInstancesForType, useExtendedStructure);
            var outputStructuralPoints = relatedStructuralPoints.ToDictionary(sp => sp, sp => ((IDictionary<ModelObjectReference, StructuralPoint>)(new Dictionary<ModelObjectReference, StructuralPoint>(dimesionalComparer))));

            foreach (ModelObjectReference potentialStructuralRef in structuralPointsByInstance.Keys)
            {
                StructuralPoint potentialStructuralPoint = structuralPointsByInstance[potentialStructuralRef];
                StructuralPoint potentialStructuralPoint_Maximal = potentialStructuralPoint.GetMaximal(this, modelInstanceRef, useExtendedStructure);

                foreach (StructuralPoint outputStructuralPoint in outputStructuralPoints.Keys)
                {
                    if (outputStructuralPoint.Overlaps(this, modelInstanceRef, desiredPeriod, potentialStructuralPoint_Maximal, useExtendedStructure))
                    { outputStructuralPoints[outputStructuralPoint].Add(potentialStructuralRef, potentialStructuralPoint_Maximal); }
                }
            }

            return outputStructuralPoints;
        }

        #endregion

        #region Structural Conversion Methods

        public bool ContainsTypeRef(ModelObjectReference structuralTypeRef)
        {
            if (structuralTypeRef.ModelObjectType == ModelObjectType.GlobalType)
            {
                return (m_GlobalTypeSpaces.Key == structuralTypeRef);
            }
            else if (structuralTypeRef.ModelObjectType == ModelObjectType.EntityType)
            {
                return m_EntityTypeSpaces.ContainsKey(structuralTypeRef);
            }
            else if (structuralTypeRef.ModelObjectType == ModelObjectType.RelationType)
            {
                return m_RelationTypeSpaces.ContainsKey(structuralTypeRef);
            }
            else
            { throw new ApplicationException("The specified Structural Type does not exist in the current Structural Map."); }
        }

        public bool ContainsInstanceRef(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef)
        {
            if (structuralInstanceRef.ModelObjectType == ModelObjectType.GlobalInstance)
            {
                return (m_GlobalInstancePointsByModel[modelInstanceRef].Key == structuralInstanceRef);
            }
            else if (structuralInstanceRef.ModelObjectType == ModelObjectType.EntityInstance)
            {
                return m_EntityInstancePointsByModel[modelInstanceRef].ContainsKey(structuralInstanceRef);
            }
            else if (structuralInstanceRef.ModelObjectType == ModelObjectType.RelationInstance)
            {
                return m_RelationInstancePointsByModel[modelInstanceRef].ContainsKey(structuralInstanceRef);
            }
            else
            { throw new ApplicationException("The specified Structural Instance does not exist in the current Model Instance."); }
        }

        protected ModelObjectReference GetStructuralTypeForInstance(ModelObjectReference structuralInstanceRef)
        {
            Nullable<ModelObjectReference> desiredModelInstanceRef = null;
            foreach (ModelObjectReference modelInstanceRef in ModelInstanceRefs)
            {
                if (ContainsInstanceRef(modelInstanceRef, structuralInstanceRef))
                { desiredModelInstanceRef = modelInstanceRef; }
            }

            if (!desiredModelInstanceRef.HasValue)
            { throw new InvalidOperationException("The specified StructuralMap does not contain a reference to the specified Structural Instance."); }

            return GetStructuralTypeForInstance(desiredModelInstanceRef.Value, structuralInstanceRef);
        }

        public ModelObjectReference GetStructuralTypeForInstance(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef)
        {
            if (!m_ModelInstanceRefs.Contains(modelInstanceRef))
            { throw new InvalidOperationException("The specified Model Instance has not been loaded into the StructuralMap."); }

            CachedTree<ModelObjectReference> instanceTypeTree = m_TypeInstanceTreesByModel[modelInstanceRef];

            if (!instanceTypeTree.Contains(structuralInstanceRef))
            { throw new ApplicationException("The specified Structural Instance does not exist in the current Model Instance."); }

            Nullable<ModelObjectReference> structuralTypeRef = instanceTypeTree.GetParent(structuralInstanceRef);
            if (!structuralTypeRef.HasValue)
            { throw new ApplicationException("The specified Structural Instance does not have a parent Structural Type in the current Model Instance."); }

            return new ModelObjectReference(structuralTypeRef.Value, structuralInstanceRef.AlternateDimensionNumber);
        }

        protected ICollection<ModelObjectReference> GetStructuralTypesForInstances(IEnumerable<ModelObjectReference> structuralInstanceRefs)
        {
            if (structuralInstanceRefs.Count() <= 0)
            { return new List<ModelObjectReference>(); }
            else
            {
                ModelObjectReference firstRelatedTypeRef = structuralInstanceRefs.First();
                Nullable<ModelObjectReference> desiredModelInstanceRef = null;

                foreach (ModelObjectReference modelInstanceRef in ModelInstanceRefs)
                {
                    if (ContainsInstanceRef(modelInstanceRef, firstRelatedTypeRef))
                    { desiredModelInstanceRef = modelInstanceRef; }
                }

                if (!desiredModelInstanceRef.HasValue)
                { throw new InvalidOperationException("The specified StructuralMap does not contain a reference to the specified Structural Instance."); }

                return GetStructuralTypesForInstances(desiredModelInstanceRef.Value, structuralInstanceRefs);
            }
        }

        public ICollection<ModelObjectReference> GetStructuralTypesForInstances(ModelObjectReference modelInstanceRef, IEnumerable<ModelObjectReference> structuralInstanceRefs)
        {
            List<ModelObjectReference> structuralTypeRefs = new List<ModelObjectReference>();
            for (int index = 0; index < structuralInstanceRefs.Count(); index++)
            {
                ModelObjectReference structuralInstanceRef = structuralInstanceRefs.ElementAt(index);
                bool isStructuralInstanceReference = ModelObjectTypeUtils.StructuralInstanceValues.Contains(structuralInstanceRef.ModelObjectType);

                if (!isStructuralInstanceReference)
                {
                    throw new InvalidOperationException("The specified reference is not a Structural Instance Reference.");
                }

                ModelObjectReference structuralTypeRef = GetStructuralTypeForInstance(modelInstanceRef, structuralInstanceRef);
                structuralTypeRef = new ModelObjectReference(structuralTypeRef, structuralInstanceRef.AlternateDimensionNumber);

                structuralTypeRefs.Add(structuralTypeRef);
            }
            return structuralTypeRefs;
        }

        public ICollection<ModelObjectReference> GetStructuralInstancesForType(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef)
        {
            if (!m_ModelInstanceRefs.Contains(modelInstanceRef))
            { throw new InvalidOperationException("The specified Model Instance has not been loaded into the StructuralMap."); }

            CachedTree<ModelObjectReference> instanceTypeTree = m_TypeInstanceTreesByModel[modelInstanceRef];

            if (!instanceTypeTree.Contains(structuralTypeRef))
            { throw new ApplicationException("The specified Structural Type does not exist in the current Model Instance."); }

            ICollection<ModelObjectReference> instanceRefs = instanceTypeTree.GetChildren(structuralTypeRef);
            instanceRefs = instanceRefs.Select(rf => new ModelObjectReference(rf, structuralTypeRef.AlternateDimensionNumber)).ToList();

            return instanceRefs;
        }

        #endregion

        #region Structural Type Navigation Methods

        public bool IsAccessible(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure)
        {
            ModelObjectReference? bridgingStructuralTypeRef = mainTypeRef;
            return IsAccessible(mainTypeRef, relatedTypeRef, useExtendedStructure, out bridgingStructuralTypeRef);
        }

        public bool IsAccessible(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure, out ModelObjectReference? bridgingStructuralTypeRef)
        {
            if (ModelObjectTypeUtils.StructuralInstanceValues.Contains(mainTypeRef.ModelObjectType))
            { mainTypeRef = GetStructuralTypeForInstance(mainTypeRef); }
            if (ModelObjectTypeUtils.StructuralInstanceValues.Contains(relatedTypeRef.ModelObjectType))
            { relatedTypeRef = GetStructuralTypeForInstance(relatedTypeRef); }

            if (!ModelObjectTypeUtils.StructuralTypeValues.Contains(mainTypeRef.ModelObjectType)
                || !ModelObjectTypeUtils.StructuralTypeValues.Contains(relatedTypeRef.ModelObjectType))
            { throw new InvalidOperationException("Both ModelObjectReferences must reference Structural Types."); }

            StructuralSpace mainSpace = GetStructuralSpace(mainTypeRef, useExtendedStructure);
            StructuralSpace relatedSpace = GetStructuralSpace(relatedTypeRef, useExtendedStructure);

            bool mainOverlaps = mainSpace.Overlaps(this, relatedSpace, useExtendedStructure);
            bool relatedOverlaps = relatedSpace.Overlaps(this, mainSpace, useExtendedStructure);

            if (mainOverlaps != relatedOverlaps)
            { throw new InvalidOperationException("Unexpected Overlap result encountered."); }

            if (mainOverlaps)
            {
                if (mainSpace.IsRelatedAndMoreGeneral(this, relatedSpace, useExtendedStructure))
                { bridgingStructuralTypeRef = relatedTypeRef; }
                else
                { bridgingStructuralTypeRef = mainTypeRef; }
                return true;
            }

            var structuralTypeRefsToEvaluate = EntityTypeRefs.Where(x => !ModelObjectReference.DimensionalComparer.Equals(x, mainTypeRef) && !ModelObjectReference.DimensionalComparer.Equals(x, relatedTypeRef)).ToList();

            foreach (var currentTypeRef in structuralTypeRefsToEvaluate)
            {
                var currentSpace = GetStructuralSpace(currentTypeRef, useExtendedStructure);

                bool currentOverlapsMain = currentSpace.Overlaps(this, mainSpace, useExtendedStructure);
                bool currentOverlapsRelated = currentSpace.Overlaps(this, relatedSpace, useExtendedStructure);

                if (currentOverlapsMain && currentOverlapsRelated)
                {
                    bridgingStructuralTypeRef = currentTypeRef;
                    return true;
                }
            }

            bridgingStructuralTypeRef = null;
            return false;
        }

        public bool IsUnique(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure)
        {
            if (ModelObjectTypeUtils.StructuralInstanceValues.Contains(mainTypeRef.ModelObjectType))
            { mainTypeRef = GetStructuralTypeForInstance(mainTypeRef); }
            if (ModelObjectTypeUtils.StructuralInstanceValues.Contains(relatedTypeRef.ModelObjectType))
            { relatedTypeRef = GetStructuralTypeForInstance(relatedTypeRef); }

            if (!ModelObjectTypeUtils.StructuralTypeValues.Contains(mainTypeRef.ModelObjectType)
                || !ModelObjectTypeUtils.StructuralTypeValues.Contains(relatedTypeRef.ModelObjectType))
            { throw new InvalidOperationException("Both ModelObjectReferences must reference Structural Types."); }

            StructuralSpace mainSpace = GetStructuralSpace(mainTypeRef, useExtendedStructure);
            StructuralSpace relatedSpace = GetStructuralSpace(relatedTypeRef, useExtendedStructure);

            return relatedSpace.IsRelatedAndMoreGeneral(this, mainSpace, useExtendedStructure);
        }

        public bool IsDirectlyAccessibleUsingSpace(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, StructuralSpace bridgingSpace, bool useExtendedStructure)
        {
            var relatedTypeRefs = new ModelObjectReference[] { relatedTypeRef };
            return IsDirectlyAccessibleUsingSpace(mainTypeRef, relatedTypeRefs, bridgingSpace, useExtendedStructure);
        }

        public bool IsDirectlyAccessibleUsingSpace(ModelObjectReference mainTypeRef, IEnumerable<ModelObjectReference> relatedTypeRefs, StructuralSpace bridgingSpace, bool useExtendedStructure)
        {
            StructuralSpace mainSpace = GetBaseStructuralSpace(mainTypeRef);
            List<StructuralSpace> relatedSpaces = relatedTypeRefs.Select(rtr => GetBaseStructuralSpace(rtr)).ToList();

            foreach (var mainDimension in mainSpace.Dimensions)
            {
                if (!bridgingSpace.Dimensions.Contains(mainDimension))
                { return false; }
            }

            foreach (var relatedSpace in relatedSpaces)
            {
                foreach (var relatedDimension in relatedSpace.Dimensions)
                {
                    if (!bridgingSpace.Dimensions.Contains(relatedDimension))
                    { return false; }
                }
            }

            return true;
        }

        public Nullable<StructuralSpace> GetRelativeStructuralSpace(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure)
        {
            IEnumerable<ModelObjectReference> relatedRefs = new ModelObjectReference[] { relatedTypeRef };
            return GetRelativeStructuralSpace(mainTypeRef, relatedRefs, useExtendedStructure);
        }

        public Nullable<StructuralSpace> GetRelativeStructuralSpace(ModelObjectReference mainTypeRef, IEnumerable<ModelObjectReference> relatedTypeRefs, bool useExtendedStructure)
        {
            Nullable<KeyValuePair<ReferenceListKey, StructuralSpace>> tempResult = GetRelativeStructuralRefsAndSpace(mainTypeRef, relatedTypeRefs, useExtendedStructure);

            if (!tempResult.HasValue)
            { return null; }

            return tempResult.Value.Value;
        }

        public Nullable<KeyValuePair<ReferenceListKey, StructuralSpace>> GetRelativeStructuralRefsAndSpace(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure)
        {
            IEnumerable<ModelObjectReference> relatedRefs = new ModelObjectReference[] { relatedTypeRef };
            return GetRelativeStructuralRefsAndSpace(mainTypeRef, relatedRefs, useExtendedStructure);
        }

        public Nullable<KeyValuePair<ReferenceListKey, StructuralSpace>> GetRelativeStructuralRefsAndSpace(ModelObjectReference mainTypeRef, IEnumerable<ModelObjectReference> relatedTypeRefs, bool useExtendedStructure)
        {
            DimensionalModelObjectReferenceComparer dimesionalComparer = new DimensionalModelObjectReferenceComparer();
            ReferenceListKey typeRefs;
            SortedDictionary<int, ModelObjectReference> orderedRelatedTypeRefs = new SortedDictionary<int, ModelObjectReference>();

            if (ModelObjectTypeUtils.StructuralInstanceValues.Contains(mainTypeRef.ModelObjectType))
            { mainTypeRef = GetStructuralTypeForInstance(mainTypeRef); }
            if (relatedTypeRefs.Count() > 0)
            {
                if (ModelObjectTypeUtils.StructuralInstanceValues.Contains(relatedTypeRefs.First().ModelObjectType))
                { relatedTypeRefs = GetStructuralTypesForInstances(relatedTypeRefs); }
            }

            var nonTypeRelatedRefs = relatedTypeRefs.Where(rf => !ModelObjectTypeUtils.StructuralTypeValues.Contains(rf.ModelObjectType)).ToList();
            if (!ModelObjectTypeUtils.StructuralTypeValues.Contains(mainTypeRef.ModelObjectType)
                || (nonTypeRelatedRefs.Count > 0))
            { throw new InvalidOperationException("Both ModelObjectReferences must reference Structural Types."); }

            StructuralSpace mainReducedSpace = GetStructuralSpace(mainTypeRef, useExtendedStructure);
            StructuralSpace mainMaximalSpace = mainReducedSpace.GetMaximal(this, useExtendedStructure);

            relatedTypeRefs = new HashSet<ModelObjectReference>(relatedTypeRefs, dimesionalComparer);
            Dictionary<ModelObjectReference, StructuralSpace> relatedReducedSpaces = new Dictionary<ModelObjectReference, StructuralSpace>(dimesionalComparer);
            Dictionary<ModelObjectReference, StructuralSpace> relatedMaximalSpaces = new Dictionary<ModelObjectReference, StructuralSpace>(dimesionalComparer);

            var allRelatedTypeRefs = new HashSet<ModelObjectReference>(dimesionalComparer);
            foreach (var relatedTypeRef in relatedTypeRefs)
            {
                ModelObjectReference? bridgingStructuralTypeRef;
                var isValid = IsAccessible(mainTypeRef, relatedTypeRef, useExtendedStructure, out bridgingStructuralTypeRef);
                var hasBridge = (bridgingStructuralTypeRef.HasValue && (!dimesionalComparer.Equals(mainTypeRef, bridgingStructuralTypeRef.Value) && !dimesionalComparer.Equals(relatedTypeRef, bridgingStructuralTypeRef.Value)));

                if (isValid && hasBridge)
                {
                    var isAccessibleThroughRelation = false;

                    foreach (var relationTypeRef in relatedTypeRefs.Where(x => x.ModelObjectType == ModelObjectType.RelationType).ToList())
                    {
                        ModelObjectReference? junkStructuralTypeRef;
                        var isValidForRelation = IsAccessible(mainTypeRef, relationTypeRef, useExtendedStructure, out junkStructuralTypeRef);
                        var hasBridgeForRelation = (junkStructuralTypeRef.HasValue && (!dimesionalComparer.Equals(mainTypeRef, junkStructuralTypeRef.Value) && !dimesionalComparer.Equals(relationTypeRef, junkStructuralTypeRef.Value)));

                        if (isValidForRelation && !hasBridgeForRelation)
                        {
                            isAccessibleThroughRelation = true;
                            break;
                        }
                    }

                    if (!isAccessibleThroughRelation)
                    { allRelatedTypeRefs.Add(bridgingStructuralTypeRef.Value); }
                }

                allRelatedTypeRefs.Add(relatedTypeRef);
            }
            if (allRelatedTypeRefs.Contains(mainTypeRef))
            { allRelatedTypeRefs.Remove(mainTypeRef); }
            relatedTypeRefs = allRelatedTypeRefs;

            bool initialOverlapFound = false;
            bool hasGlobalsOnly = true;
            ModelObjectReference currentRef;
            StructuralSpace combinedMaximalSpace = StructuralSpace.GlobalStructuralSpace;
            List<ModelObjectReference> remainingRelatedRefsToProcess = new List<ModelObjectReference>(relatedTypeRefs);

            foreach (ModelObjectReference relatedTypeRef in relatedTypeRefs)
            {
                StructuralSpace relatedReducedSpace = GetStructuralSpace(relatedTypeRef, useExtendedStructure);
                StructuralSpace relatedMaximalSpace = relatedReducedSpace.GetMaximal(this, useExtendedStructure);

                relatedReducedSpaces.Add(relatedTypeRef, relatedReducedSpace);
                relatedMaximalSpaces.Add(relatedTypeRef, relatedMaximalSpace);

                if (!relatedMaximalSpace.Equals(StructuralSpace.GlobalStructuralSpace))
                { hasGlobalsOnly = false; }

                if (!initialOverlapFound)
                {
                    if (relatedMaximalSpace.Overlaps(this, mainMaximalSpace, useExtendedStructure))
                    {
                        initialOverlapFound = true;
                        currentRef = relatedTypeRef;
                        combinedMaximalSpace = mainMaximalSpace.Merge(relatedMaximalSpace);

                        remainingRelatedRefsToProcess.Remove(relatedTypeRef, dimesionalComparer);
                        orderedRelatedTypeRefs.Add(orderedRelatedTypeRefs.Count, relatedTypeRef);
                    }
                }
            }

            if (hasGlobalsOnly)
            {
                typeRefs = new ReferenceListKey(orderedRelatedTypeRefs.Values, ModelObjectReference.DimensionalComparer);
                return new KeyValuePair<ReferenceListKey, StructuralSpace>(typeRefs, mainMaximalSpace);
            }

            if (!initialOverlapFound)
            { return null; }

            int iterationsSinceLastChange = 0;
            while (remainingRelatedRefsToProcess.Count > 0)
            {
                int countBeforeUpdate = remainingRelatedRefsToProcess.Count;

                currentRef = remainingRelatedRefsToProcess.First();
                remainingRelatedRefsToProcess.Remove(currentRef, dimesionalComparer);
                StructuralSpace relatedMaximalSpace = relatedMaximalSpaces[currentRef];

                if (combinedMaximalSpace.Overlaps(this, relatedMaximalSpace, useExtendedStructure))
                {
                    combinedMaximalSpace = combinedMaximalSpace.Merge(relatedMaximalSpace);
                    orderedRelatedTypeRefs.Add(orderedRelatedTypeRefs.Count, currentRef);
                }
                else
                {
                    remainingRelatedRefsToProcess.Add(currentRef);
                }

                int countAfterUpdate = remainingRelatedRefsToProcess.Count;

                if (countAfterUpdate >= countBeforeUpdate)
                { iterationsSinceLastChange++; }
                else
                { iterationsSinceLastChange = 0; }

                if (iterationsSinceLastChange > remainingRelatedRefsToProcess.Count)
                { return null; }
            }

            typeRefs = new ReferenceListKey(orderedRelatedTypeRefs.Values, ModelObjectReference.DimensionalComparer);
            return new KeyValuePair<ReferenceListKey, StructuralSpace>(typeRefs, combinedMaximalSpace);
        }

        #endregion

        #region Structural Instance Navigation Methods

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, bool useExtendedStructure)
        {
            return GetRelativeStructuralPoints(modelInstanceRef, desiredPeriod, mainInstanceRef, new Dictionary<ModelObjectReference, bool>(), useExtendedStructure);
        }

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference mainInstanceRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure)
        {
            return GetRelativeStructuralPoints(modelInstanceRef, null, mainInstanceRef, relatedTypeRef, useExtendedStructure);
        }

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure)
        {
            DimensionalModelObjectReferenceComparer dimesionalComparer = new DimensionalModelObjectReferenceComparer();
            Dictionary<ModelObjectReference, bool> relatedTypeRefsWithIsBound = new Dictionary<ModelObjectReference, bool>(dimesionalComparer);
            relatedTypeRefsWithIsBound.Add(relatedTypeRef, DefaultTypeRefIsBound);
            return GetRelativeStructuralPoints(modelInstanceRef, desiredPeriod, mainInstanceRef, relatedTypeRefsWithIsBound, useExtendedStructure);
        }

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference mainInstanceRef, ModelObjectReference relatedTypeRef, bool relatedTypeRefIsBound, bool useExtendedStructure)
        {
            return GetRelativeStructuralPoints(modelInstanceRef, null, mainInstanceRef, relatedTypeRef, relatedTypeRefIsBound, useExtendedStructure);
        }

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, ModelObjectReference relatedTypeRef, bool relatedTypeRefIsBound, bool useExtendedStructure)
        {
            DimensionalModelObjectReferenceComparer dimesionalComparer = new DimensionalModelObjectReferenceComparer();
            Dictionary<ModelObjectReference, bool> relatedTypeRefsWithIsBound = new Dictionary<ModelObjectReference, bool>(dimesionalComparer);
            relatedTypeRefsWithIsBound.Add(relatedTypeRef, relatedTypeRefIsBound);
            return GetRelativeStructuralPoints(modelInstanceRef, desiredPeriod, mainInstanceRef, relatedTypeRefsWithIsBound, useExtendedStructure);
        }

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference mainInstanceRef, IEnumerable<ModelObjectReference> relatedTypeRefs, bool useExtendedStructure)
        {
            return GetRelativeStructuralPoints(modelInstanceRef, null, mainInstanceRef, relatedTypeRefs, useExtendedStructure);
        }

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, IEnumerable<ModelObjectReference> relatedTypeRefs, bool useExtendedStructure)
        {
            DimensionalModelObjectReferenceComparer dimesionalComparer = new DimensionalModelObjectReferenceComparer();
            Dictionary<ModelObjectReference, bool> relatedTypeRefsWithIsBound = new Dictionary<ModelObjectReference, bool>(dimesionalComparer);
            foreach (ModelObjectReference relatedTypeRef in relatedTypeRefs)
            {
                if (!relatedTypeRefsWithIsBound.ContainsKey(relatedTypeRef))
                { relatedTypeRefsWithIsBound.Add(relatedTypeRef, DefaultTypeRefIsBound); }
            }
            return GetRelativeStructuralPoints(modelInstanceRef, desiredPeriod, mainInstanceRef, relatedTypeRefsWithIsBound, useExtendedStructure);
        }

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference mainInstanceRef, IDictionary<ModelObjectReference, bool> relatedTypeRefsWithIsBound, bool useExtendedStructure)
        {
            return GetRelativeStructuralPoints(modelInstanceRef, null, mainInstanceRef, relatedTypeRefsWithIsBound, useExtendedStructure);
        }

        public IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, IDictionary<ModelObjectReference, bool> relatedTypeRefsWithIsBound, bool useExtendedStructure)
        {
            var dimesionalComparer = new DimensionalModelObjectReferenceComparer();
            var hasAnyUnbound = relatedTypeRefsWithIsBound.Values.Contains(false);

            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }

            ModelObjectReference mainTypeRef = GetStructuralTypeForInstance(modelInstanceRef, mainInstanceRef);
            IEnumerable<ModelObjectReference> relatedTypeRefs = relatedTypeRefsWithIsBound.Keys;

            Nullable<KeyValuePair<ReferenceListKey, StructuralSpace>> tempResult = GetRelativeStructuralRefsAndSpace(mainTypeRef, relatedTypeRefs, useExtendedStructure);
            if (!tempResult.HasValue)
            { return new Dictionary<ReferenceListKey, StructuralPoint>(); }

            IList<ModelObjectReference> orderedRelatedTypeRefs = tempResult.Value.Key.KeyPartItems;
            StructuralSpace resultingStructuralSpace = tempResult.Value.Value;

            StructuralPoint mainReducedPoint = GetStructuralPoint(modelInstanceRef, desiredPeriod, mainInstanceRef, useExtendedStructure);
            StructuralPoint mainMaximalPoint = mainReducedPoint.GetMaximal(this, modelInstanceRef, desiredPeriod, useExtendedStructure);

            Dictionary<ReferenceListKey, StructuralPoint> resultingJoins = new Dictionary<ReferenceListKey, StructuralPoint>();
            resultingJoins.Add(new ReferenceListKey(mainInstanceRef, dimesionalComparer), mainMaximalPoint);

            for (int index = 0; index < orderedRelatedTypeRefs.Count; index++)
            {
                var relatedTypeRef = orderedRelatedTypeRefs[index];
                bool isBound = (relatedTypeRefsWithIsBound.ContainsKey(relatedTypeRef)) ? relatedTypeRefsWithIsBound[relatedTypeRef] : (hasAnyUnbound ? this.IsUnique(mainTypeRef, relatedTypeRef, useExtendedStructure) : true);

                IDictionary<StructuralPoint, IDictionary<ModelObjectReference, StructuralPoint>> furtherPoints = null;
                if (isBound)
                {
                    furtherPoints = GetRelatedStructuralInstances(modelInstanceRef, desiredPeriod, relatedTypeRef, resultingJoins.Values, useExtendedStructure);
                }
                else
                {
                    furtherPoints = new Dictionary<StructuralPoint, IDictionary<ModelObjectReference, StructuralPoint>>();
                    foreach (StructuralPoint resultingJoinPoint in resultingJoins.Values)
                    {
                        var structuralInstances = GetStructuralInstancesForType(modelInstanceRef, relatedTypeRef);
                        var structuralInstancesDict = structuralInstances.ToDictionary(si => si, si => GetStructuralPoint(modelInstanceRef, desiredPeriod, si, useExtendedStructure), dimesionalComparer);
                        furtherPoints.Add(resultingJoinPoint, structuralInstancesDict);
                    }
                }

                var oldJoinKeys = resultingJoins.Keys.ToList();

                foreach (var oldJoinKey in oldJoinKeys)
                {
                    var oldJoinPoint = resultingJoins[oldJoinKey];
                    var newPointsDict = furtherPoints[oldJoinPoint];
                    var hasNewPoints = (newPointsDict.Count > 0);

                    foreach (var newPointBucket in newPointsDict)
                    {
                        var newRelatedInstanceRef = newPointBucket.Key;
                        var newPointToJoin = newPointBucket.Value;

                        var newJoinKey = new ReferenceListKey(oldJoinKey, newRelatedInstanceRef);
                        var newJoinPoint = oldJoinPoint.Merge(newPointToJoin);

                        resultingJoins.Add(newJoinKey, newJoinPoint);
                    }

                    resultingJoins.Remove(oldJoinKey);
                }
            }

            return resultingJoins;
        }

        #endregion
    }
}