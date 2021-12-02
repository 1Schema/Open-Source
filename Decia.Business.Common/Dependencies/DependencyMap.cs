using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Edge = Decia.Business.Common.DataStructures.Edge<Decia.Business.Common.Modeling.ModelObjectReference>;

namespace Decia.Business.Common.Dependencies
{
    public partial class DependencyMap : IDependencyMap
    {
        public const bool DefaultUseAllEdgesInNetwork = false;
        public const bool DefaultUseExtendedStructure = true;

        public static readonly Nullable<ModelObjectReference> NullModelObjectReference = null;

        #region Members

        private ModelObjectReference m_ModelTemplateRef;
        private Dictionary<ModelObjectReference, ModelObjectReference> m_StructuralTypeIdTemplates;
        private Dictionary<ModelObjectReference, ModelObjectReference> m_StructuralTypeNameTemplates;
        private Dictionary<ModelObjectReference, ModelObjectReference> m_StructuralTypeOrderTemplates;
        private Dictionary<ModelObjectReference, ModelObjectReference> m_VariableTemplateStructuralTypes;
        private Dictionary<ModelObjectReference, HashSet<ModelObjectReference>> m_StructuralTypeVariableTemplates;
        private Dictionary<ModelObjectReference, NavigationSpecification> m_StructuralTypeNavigationSpecifications;
        private CachedNetwork<ModelObjectReference> m_VariableTemplateNetwork;

        #endregion

        #region Constructors

        public DependencyMap(ModelObjectReference modelTemplateRef, IDictionary<ModelObjectReference, ModelObjectReference> structuralTypeIdTemplates, IDictionary<ModelObjectReference, ModelObjectReference> structuralTypeNameTemplates, IDictionary<ModelObjectReference, ModelObjectReference> variableTemplateStructuralTypes, IEnumerable<Edge> variableTemplateEdges, IEnumerable<NavigationSpecification> navigationSpecifications)
            : this(modelTemplateRef, structuralTypeIdTemplates, structuralTypeNameTemplates, new Dictionary<ModelObjectReference, ModelObjectReference>(), variableTemplateStructuralTypes, variableTemplateEdges, navigationSpecifications)
        { }

        public void SetStructuralTypeOrderTemplates(IDictionary<ModelObjectReference, ModelObjectReference> structuralTypeOrderTemplates)
        {
            m_StructuralTypeOrderTemplates = new Dictionary<ModelObjectReference, ModelObjectReference>(structuralTypeOrderTemplates);
        }

        public DependencyMap(ModelObjectReference modelTemplateRef, IDictionary<ModelObjectReference, ModelObjectReference> structuralTypeIdTemplates, IDictionary<ModelObjectReference, ModelObjectReference> structuralTypeNameTemplates, IDictionary<ModelObjectReference, ModelObjectReference> structuralTypeOrderTemplates, IDictionary<ModelObjectReference, ModelObjectReference> variableTemplateStructuralTypes, IEnumerable<Edge> variableTemplateEdges, IEnumerable<NavigationSpecification> navigationSpecifications)
        {
            foreach (var structuralType in structuralTypeIdTemplates.Keys)
            {
                var idTemplate = structuralTypeIdTemplates[structuralType];

                if (!variableTemplateStructuralTypes.ContainsKey(idTemplate))
                { throw new InvalidOperationException("The Id Variable Template does not exist."); }

                if (variableTemplateStructuralTypes[idTemplate] != structuralType)
                { throw new InvalidOperationException("The Id Variable Template belongs to a different Structural Type."); }
            }

            HashSet<ModelObjectReference> structuralTypes = new HashSet<ModelObjectReference>(variableTemplateStructuralTypes.Values);
            foreach (var structuralType in structuralTypes)
            {
                if (!structuralTypeIdTemplates.ContainsKey(structuralType))
                { throw new InvalidOperationException("The Structural Type does not have an Id Variable Template."); }
            }

            HashSet<ModelObjectReference> nodes = new HashSet<ModelObjectReference>(variableTemplateStructuralTypes.Keys);
            HashSet<ModelObjectReference> structures = new HashSet<ModelObjectReference>(variableTemplateStructuralTypes.Values);
            HashSet<Edge> edges = new HashSet<Edge>(variableTemplateEdges);

            var invalidNodes = nodes.Where(n => (!n.IsVariableType())).ToList();

            if (invalidNodes.Count > 0)
            { throw new InvalidOperationException("Only VariableTemplate or AnonymousVariableTemplate can be passed as Nodes."); }

            var invalidStructures = structures.Where(n => (!n.IsStructuralType())).ToList();

            if (invalidStructures.Count > 0)
            { throw new InvalidOperationException("Only StructuralTypes can be passed as Node Structural Locations."); }

            var anonymousNodes = nodes.Where(n => n.ModelObjectType == ModelObjectType.AnonymousVariableTemplate).ToList();
            var invalidEdges = edges.Where(e => anonymousNodes.Contains(e.OutgoingNode)).ToList();

            if (invalidEdges.Count > 0)
            { throw new InvalidOperationException("Anonymous Variables cannot be re-used in Formulas."); }

            m_VariableTemplateNetwork = new CachedNetwork<ModelObjectReference>(nodes, edges);
            m_ModelTemplateRef = modelTemplateRef;
            m_StructuralTypeIdTemplates = new Dictionary<ModelObjectReference, ModelObjectReference>(structuralTypeIdTemplates);
            m_StructuralTypeNameTemplates = new Dictionary<ModelObjectReference, ModelObjectReference>(structuralTypeNameTemplates);
            m_StructuralTypeOrderTemplates = new Dictionary<ModelObjectReference, ModelObjectReference>(structuralTypeOrderTemplates);
            m_VariableTemplateStructuralTypes = new Dictionary<ModelObjectReference, ModelObjectReference>(variableTemplateStructuralTypes);
            m_StructuralTypeVariableTemplates = new Dictionary<ModelObjectReference, HashSet<ModelObjectReference>>();
            m_StructuralTypeNavigationSpecifications = navigationSpecifications.ToDictionary(ns => ns.MainStructuralTypeRef, ns => ns);

            foreach (ModelObjectReference variableTemplateRef in m_VariableTemplateStructuralTypes.Keys)
            {
                ModelObjectReference structuralTypeRef = m_VariableTemplateStructuralTypes[variableTemplateRef];

                if (!m_StructuralTypeVariableTemplates.ContainsKey(structuralTypeRef))
                { m_StructuralTypeVariableTemplates.Add(structuralTypeRef, new HashSet<ModelObjectReference>()); }

                m_StructuralTypeVariableTemplates[structuralTypeRef].Add(variableTemplateRef);
            }
        }

        #endregion

        #region Properties

        public bool IsValid
        {
            get { return true; }
        }

        public ModelObjectReference ModelTemplateRef
        {
            get { return m_ModelTemplateRef; }
        }

        public IEnumerable<ModelObjectReference> VariableTemplateRefs
        {
            get { return new HashSet<ModelObjectReference>(m_VariableTemplateStructuralTypes.Keys); }
        }

        public IEnumerable<ModelObjectReference> StructuralTypeRefs
        {
            get { return new HashSet<ModelObjectReference>(m_StructuralTypeVariableTemplates.Keys); }
        }

        public INetwork<ModelObjectReference> VariableTypeNetwork
        {
            get { return m_VariableTemplateNetwork; }
        }

        #endregion

        #region General Methods

        public ModelObjectReference GetStructuralType(ModelObjectReference variableTemplateRef)
        {
            if (!m_VariableTemplateStructuralTypes.ContainsKey(variableTemplateRef))
            { throw new InvalidOperationException("The specified VariableType does not exist."); }

            return new ModelObjectReference(m_VariableTemplateStructuralTypes[variableTemplateRef], variableTemplateRef.AlternateDimensionNumber);
        }

        public ModelObjectReference GetIdVariableTemplate(ModelObjectReference structuralTypeRef)
        {
            if (!m_StructuralTypeIdTemplates.ContainsKey(structuralTypeRef))
            { throw new InvalidOperationException("Every Structural Type should have an Id Variable Template."); }

            return m_StructuralTypeIdTemplates[structuralTypeRef];
        }

        public ModelObjectReference GetNameVariableTemplate(ModelObjectReference structuralTypeRef)
        {
            if (!m_StructuralTypeNameTemplates.ContainsKey(structuralTypeRef))
            { throw new InvalidOperationException("Every Structural Type should have an Name Variable Template."); }

            return m_StructuralTypeNameTemplates[structuralTypeRef];
        }

        public ModelObjectReference GetOrderVariableTemplate(ModelObjectReference structuralTypeRef)
        {
            if (!m_StructuralTypeOrderTemplates.ContainsKey(structuralTypeRef))
            { throw new InvalidOperationException("Every Structural Type should have an Order Variable Template."); }

            return m_StructuralTypeOrderTemplates[structuralTypeRef];
        }

        public IEnumerable<ModelObjectReference> GetVariableTemplates(ModelObjectReference structuralTypeRef)
        {
            if (!m_StructuralTypeVariableTemplates.ContainsKey(structuralTypeRef))
            { return new List<ModelObjectReference>(); }

            return m_StructuralTypeVariableTemplates[structuralTypeRef].Select(rf => new ModelObjectReference(rf, structuralTypeRef.AlternateDimensionNumber)).ToList();
        }

        public IEnumerable<IEdge<ModelObjectReference>> GetReducedEdges(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnore)
        {
            return GetReducedEdges_Protected(edgesToIgnore);
        }

        protected HashSet<IEdge<ModelObjectReference>> GetReducedEdges_Protected(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnore)
        {
            HashSet<IEdge<ModelObjectReference>> reducedEdges = new HashSet<IEdge<ModelObjectReference>>(m_VariableTemplateNetwork.Edges);

            foreach (IEdge<ModelObjectReference> edgeToIgnore in edgesToIgnore)
            {
                if (reducedEdges.Contains(edgeToIgnore))
                { reducedEdges.Remove(edgeToIgnore); }
            }

            return reducedEdges;
        }

        #endregion

        #region Navigation Methods

        public IList<ModelObjectReference> GetImpliedNavigationVariableReferences(IStructuralMap structuralMap, ModelObjectReference variableTemplateRef)
        {
            return GetImpliedNavigationVariableReferences(structuralMap, variableTemplateRef, DefaultUseExtendedStructure);
        }

        public IList<ModelObjectReference> GetImpliedNavigationVariableReferences(IStructuralMap structuralMap, ModelObjectReference variableTemplateRef, bool useExtendedStructure)
        {
            var structuralTypeRef = this.GetStructuralType(variableTemplateRef);
            var explicitVariableDependencyRefs = this.VariableTypeNetwork.GetParents(variableTemplateRef);

            if (explicitVariableDependencyRefs.Count < 1)
            { return new List<ModelObjectReference>(); }

            var relatedVariablesWithStructure = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);

            foreach (var variableDependencyRef in explicitVariableDependencyRefs)
            {
                if (relatedVariablesWithStructure.ContainsKey(variableDependencyRef))
                { continue; }

                var relatedStructuralTypeRef = this.GetStructuralType(variableDependencyRef);

                if (relatedStructuralTypeRef == ModelObjectReference.GlobalTypeReference)
                { continue; }
                if (relatedStructuralTypeRef == structuralTypeRef)
                { continue; }

                relatedVariablesWithStructure.Add(variableDependencyRef, relatedStructuralTypeRef);
            }

            List<StructuralDimension> impliedDimensions = new List<StructuralDimension>();

            var relatedStructuralTypeRefs = new HashSet<ModelObjectReference>(relatedVariablesWithStructure.Values, ModelObjectReference.DimensionalComparer);
            var spaceOfRelation = structuralMap.GetRelativeStructuralSpace(structuralTypeRef, relatedStructuralTypeRefs, useExtendedStructure);

            if (!spaceOfRelation.HasValue)
            { throw new InvalidOperationException("A referenced VariableTemplate is not accessible."); }

            var bridgingSpace = spaceOfRelation.Value.GetBridge(structuralMap, structuralTypeRef, relatedStructuralTypeRefs, useExtendedStructure);

            if (!bridgingSpace.HasValue)
            { throw new InvalidOperationException("A referenced VariableTemplate is not accessible."); }

            impliedDimensions = new List<StructuralDimension>();

            foreach (var bridgingDimension in bridgingSpace.Value.Dimensions)
            {
                if (!bridgingDimension.IsReferenceMember())
                { continue; }
                if (bridgingDimension.CorrespondingVariableRef == variableTemplateRef)
                { continue; }

                impliedDimensions.Add(bridgingDimension);
            }

            var impliedVariableTemplateRefs = impliedDimensions.Select(dim => dim.CorrespondingVariableRef.Value).ToList();

            if (impliedVariableTemplateRefs.Contains(variableTemplateRef))
            { impliedVariableTemplateRefs.Remove(variableTemplateRef); }

            var uniqueImpliedVariableTemplateRefs = new HashSet<ModelObjectReference>(impliedVariableTemplateRefs, ModelObjectReference.DimensionalComparer);
            return uniqueImpliedVariableTemplateRefs.ToList();
        }

        public void AddImpliedNavigationDependencies(IStructuralMap structuralMap)
        {
            AddImpliedNavigationDependencies(structuralMap, DefaultUseExtendedStructure);
        }

        public void AddImpliedNavigationDependencies(IStructuralMap structuralMap, bool useExtendedStructure)
        {
            var variableTemplateImpliedDependecies = new Dictionary<ModelObjectReference, IEnumerable<ModelObjectReference>>();

            foreach (var variableTemplateRef in m_VariableTemplateStructuralTypes.Keys)
            {
                var impliedNAvigationVariableDependecies = GetImpliedNavigationVariableReferences(structuralMap, variableTemplateRef, useExtendedStructure);
                variableTemplateImpliedDependecies.Add(variableTemplateRef, impliedNAvigationVariableDependecies);
            }

            foreach (var variableTemplateRef in variableTemplateImpliedDependecies.Keys)
            {
                var impliedVariableTemplateDependecies = variableTemplateImpliedDependecies[variableTemplateRef];

                if (impliedVariableTemplateDependecies.Count() < 1)
                { continue; }

                List<ModelObjectReference> nodesToAdd = new List<ModelObjectReference>();
                List<Edge> edgesToAdd = new List<Edge>();
                List<ModelObjectReference> nodesToRemove = new List<ModelObjectReference>();
                List<Edge> edgesToRemove = new List<Edge>();

                foreach (var impliedVariableTemplateRef in impliedVariableTemplateDependecies)
                {
                    Edge edge = new Edge(impliedVariableTemplateRef, variableTemplateRef);
                    edgesToAdd.Add(edge);
                }

                m_VariableTemplateNetwork.UpdateValues(nodesToAdd, edgesToAdd, nodesToRemove, edgesToRemove);
            }
        }

        #endregion

        #region Computation Methods

        public IList<ICycleGroup<ModelObjectReference>> GetCycleGroups()
        {
            List<IEdge<ModelObjectReference>> edgesToIgnore = new List<IEdge<ModelObjectReference>>();
            return GetCycleGroups(edgesToIgnore);
        }

        public IList<ICycleGroup<ModelObjectReference>> GetCycleGroups(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnore)
        {
            HashSet<ModelObjectReference> allNodes = new HashSet<ModelObjectReference>(m_VariableTemplateNetwork.Nodes);
            IEnumerable<IEdge<ModelObjectReference>> remainingEdges = GetReducedEdges(edgesToIgnore);
            bool ignoreSelfCyles = false;

            var originalCycleGroups = AlgortihmUtils.GetCycles<ModelObjectReference>(allNodes, remainingEdges, ignoreSelfCyles);
            var updatedCycleGroups = new List<ICycleGroup<ModelObjectReference>>();

            HashSet<IEdge<ModelObjectReference>> allEdges = new HashSet<IEdge<ModelObjectReference>>(m_VariableTemplateNetwork.Edges);

            foreach (var originalCycleGroup in originalCycleGroups)
            {
                IEnumerable<ModelObjectReference> nodesIncluded = originalCycleGroup.NodesIncluded;
                List<IEdge<ModelObjectReference>> edgesIncluded = new List<IEdge<ModelObjectReference>>();

                foreach (IEdge<ModelObjectReference> edge in allEdges)
                {
                    if (!nodesIncluded.Contains(edge.IncomingNode))
                    { continue; }
                    if (!nodesIncluded.Contains(edge.OutgoingNode))
                    { continue; }

                    edgesIncluded.Add(edge);
                }

                CycleGroup<ModelObjectReference> updatedCycleGroup = new CycleGroup<ModelObjectReference>(nodesIncluded, edgesIncluded);
                updatedCycleGroups.Add(updatedCycleGroup);
            }

            return updatedCycleGroups;
        }

        public IDictionary<Guid, IComputationGroup> GetComputationGroups()
        {
            List<IEdge<ModelObjectReference>> edgesToIgnore = new List<IEdge<ModelObjectReference>>();
            return GetComputationGroups(edgesToIgnore);
        }

        public IDictionary<Guid, IComputationGroup> GetComputationGroups(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnore)
        {
            HashSet<ModelObjectReference> allNodes = new HashSet<ModelObjectReference>(m_VariableTemplateNetwork.Nodes);
            var handledNodes = new HashSet<ModelObjectReference>();

            var cycleGroups = GetCycleGroups(edgesToIgnore);

            var computeGroups = new Dictionary<Guid, IComputationGroup>();
            foreach (var cycleGroup in cycleGroups)
            {
                var computeGroup = new ComputationGroup(cycleGroup);
                computeGroups.Add(computeGroup.Id, computeGroup);

                foreach (var handledNode in cycleGroup.NodesIncluded)
                { handledNodes.Add(handledNode); }
            }

            foreach (ModelObjectReference node in allNodes)
            {
                if (handledNodes.Contains(node))
                { continue; }

                var computeGroup = new ComputationGroup(new ModelObjectReference[] { node });
                computeGroups.Add(computeGroup.Id, computeGroup);
            }

            return computeGroups;
        }

        public INetwork<Guid> GetComputationNetwork()
        {
            List<IEdge<ModelObjectReference>> edgesToIgnoreForGrouping = new List<IEdge<ModelObjectReference>>();
            return GetComputationNetwork(edgesToIgnoreForGrouping, DefaultUseAllEdgesInNetwork);
        }

        public INetwork<Guid> GetComputationNetwork(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnoreForGrouping)
        {
            return GetComputationNetwork(edgesToIgnoreForGrouping, DefaultUseAllEdgesInNetwork);
        }

        public INetwork<Guid> GetComputationNetwork(IEnumerable<IEdge<ModelObjectReference>> edgesToIgnoreForGrouping, bool useAllEdgesInNetwork)
        {
            var computeGroups = GetComputationGroups(edgesToIgnoreForGrouping);

            var refNodes = new HashSet<ModelObjectReference>(m_VariableTemplateNetwork.Nodes);
            var guidNodesByRefNode = new Dictionary<ModelObjectReference, Guid>();

            HashSet<IEdge<ModelObjectReference>> refEdges = null;
            if (useAllEdgesInNetwork)
            { refEdges = new HashSet<IEdge<ModelObjectReference>>(m_VariableTemplateNetwork.Edges); }
            else
            { refEdges = GetReducedEdges_Protected(edgesToIgnoreForGrouping); }

            foreach (var computeGroup in computeGroups.Values)
            {
                foreach (var node in computeGroup.NodesIncluded)
                {
                    guidNodesByRefNode.Add(node, computeGroup.Id);
                }
                foreach (var edge in computeGroup.EdgesIncluded)
                {
                    refEdges.Remove(edge);
                }
            }

            var idNodes = new HashSet<Guid>(computeGroups.Keys);
            var idEdges = new HashSet<IEdge<Guid>>();

            foreach (var unhandledEdge in refEdges)
            {
                Guid outGuid = guidNodesByRefNode[unhandledEdge.OutgoingNode];
                Guid inGuid = guidNodesByRefNode[unhandledEdge.IncomingNode];

                Edge<Guid> idEdge = new Edge<Guid>(outGuid, inGuid);
                idEdges.Add(idEdge);
            }

            CachedNetwork<Guid> computeNetwork = new CachedNetwork<Guid>(idNodes, idEdges);

            foreach (var idNode in idNodes)
            {
                computeNetwork.SetCachedValue<IComputationGroup>(idNode, computeGroups[idNode]);
            }

            return computeNetwork;
        }

        #endregion
    }
}