using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;

namespace Decia.Business.Common.DataStructures
{
    public class CachedNetwork<T> : INetwork<T>
        where T : struct, IComparable
    {
        public const bool DefaultIgnoreSelfCyles = CachedTree<T>.DefaultIgnoreSelfCyles;
        public const bool DefaultTreatSiblingsAsRelatives = CachedTree<T>.DefaultTreatSiblingsAsRelatives;

        private IEqualityComparer<T> m_NodeComparer;
        private EdgeEqualityComparer<T> m_EdgeComparer;
        private IOrderManager<T> m_OrderManager;
        private bool m_IsEditLocked;

        private ReadOnlyHashSet<T> m_Nodes;
        private ReadOnlyHashSet<IEdge<T>> m_Edges;
        private ReadOnlyDictionary<T, ReadOnlyHashSet<T>> m_OutgoingEdges;
        private ReadOnlyDictionary<T, ReadOnlyHashSet<T>> m_IncomingEdges;
        private ReadOnlyHashSet<T> m_StartNodes;
        private ReadOnlyHashSet<T> m_EndNodes;
        private Dictionary<T, object> m_CachedValues;

        #region Constructors

        public CachedNetwork()
            : this(null)
        { }

        public CachedNetwork(IEqualityComparer<T> nodeComparer)
            : this(new T[] { }, new IEdge<T>[] { }, nodeComparer)
        { }

        public CachedNetwork(IEnumerable<T> nodes, IEnumerable<Edge<T>> edges)
            : this(nodes, edges, nodes.GetComparer())
        { }

        public CachedNetwork(IEnumerable<T> nodes, IEnumerable<Edge<T>> edges, IEqualityComparer<T> nodeComparer)
            : this(nodes, edges.Select(e => (IEdge<T>)e).ToList(), nodeComparer)
        { }

        public CachedNetwork(IEnumerable<T> nodes, IEnumerable<IEdge<T>> edges)
            : this(nodes, edges, nodes.GetComparer())
        { }

        public CachedNetwork(IEnumerable<T> nodes, IEnumerable<IEdge<T>> edges, IEqualityComparer<T> nodeComparer)
        {
            try
            {
                m_NodeComparer = nodeComparer;
                m_EdgeComparer = new EdgeEqualityComparer<T>(nodeComparer);
                m_OrderManager = new NoOpOrderManager<T>();
                m_IsEditLocked = false;

                m_Nodes = new ReadOnlyHashSet<T>(NodeComparer);
                m_Edges = new ReadOnlyHashSet<IEdge<T>>(EdgeComparer);

                m_OutgoingEdges = new ReadOnlyDictionary<T, ReadOnlyHashSet<T>>(NodeComparer);
                m_IncomingEdges = new ReadOnlyDictionary<T, ReadOnlyHashSet<T>>(NodeComparer);
                m_StartNodes = new ReadOnlyHashSet<T>(NodeComparer);
                m_EndNodes = new ReadOnlyHashSet<T>(NodeComparer);

                m_CachedValues = new Dictionary<T, object>(NodeComparer);

                AddValues(nodes, edges);
            }
            catch (Exception e)
            { throw e; }
            finally
            { SetReadOnly(true); }
        }

        #endregion

        #region Internal Getters - To Prevent Compying

        internal ReadOnlyHashSet<T> Nodes_INTERNAL { get { return m_Nodes; } }
        internal ReadOnlyHashSet<IEdge<T>> Edges_INTERNAL { get { return m_Edges; } }
        internal ReadOnlyDictionary<T, ReadOnlyHashSet<T>> OutgoingEdges_INTERNAL { get { return m_OutgoingEdges; } }
        internal ReadOnlyDictionary<T, ReadOnlyHashSet<T>> IncomingEdges_INTERNAL { get { return m_IncomingEdges; } }
        internal ReadOnlyHashSet<T> StartNodes_INTERNAL { get { return m_StartNodes; } }
        internal ReadOnlyHashSet<T> EndNodes_INTERNAL { get { return m_EndNodes; } }

        #endregion

        #region Modification Methods

        public void AddValues(IEnumerable<T> nodesToAdd, IEnumerable<Edge<T>> edgesToAdd)
        {
            AddValues(nodesToAdd, edgesToAdd.Select(e => (IEdge<T>)e).ToHashSet(EdgeComparer));
        }

        public void AddValues(IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd)
        {
            UpdateValues(nodesToAdd, edgesToAdd, new HashSet<T>(NodeComparer), new HashSet<IEdge<T>>(EdgeComparer));
        }

        public void UpdateValues(IEnumerable<T> nodesToAdd, IEnumerable<Edge<T>> edgesToAdd, IEnumerable<T> nodesToRemove, IEnumerable<Edge<T>> edgesToRemove)
        {
            UpdateValues(nodesToAdd, edgesToAdd.Select(e => (IEdge<T>)e).ToHashSet(EdgeComparer), nodesToRemove, edgesToRemove.Select(e => (IEdge<T>)e).ToHashSet(EdgeComparer));
        }

        public void UpdateValues(IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd, IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove)
        {
            try
            {
                SetReadOnly(false);

                nodesToAdd = new ReadOnlyHashSet<T>(nodesToAdd, NodeComparer);
                edgesToAdd = new ReadOnlyHashSet<IEdge<T>>(edgesToAdd, EdgeComparer);
                nodesToRemove = new ReadOnlyHashSet<T>(nodesToRemove, NodeComparer);
                edgesToRemove = new ReadOnlyHashSet<IEdge<T>>(edgesToRemove, EdgeComparer);

                var allNodes = m_Nodes.Union(nodesToAdd, NodeComparer).ToHashSet(NodeComparer);
                allNodes = allNodes.Where(n => !nodesToRemove.Contains(n)).ToHashSet(NodeComparer);
                var allEdges = m_Edges.Union(edgesToAdd, EdgeComparer).ToHashSet(EdgeComparer);
                allEdges = allEdges.Where(e => (!edgesToRemove.Contains(e)) && (allNodes.Contains(e.IncomingNode) && (allNodes.Contains(e.OutgoingNode)))).ToHashSet(EdgeComparer);

                foreach (IEdge<T> edge in allEdges)
                {
                    T outgoingNode = edge.OutgoingNode;
                    T incomingNode = edge.IncomingNode;

                    if (!allNodes.Contains(outgoingNode))
                    { throw new InvalidOperationException("The outgoing Node does not exist in the list of Nodes."); }
                    if (!allNodes.Contains(incomingNode))
                    { throw new InvalidOperationException("The incoming Node does not exist in the list of Nodes."); }
                }

                m_StartNodes.Clear();
                m_EndNodes.Clear();

                foreach (T node in nodesToAdd)
                {
                    if (m_Nodes.Contains(node))
                    { continue; }

                    m_Nodes.Add(node);

                    m_OutgoingEdges.Add(node, new ReadOnlyHashSet<T>(NodeComparer));
                    m_IncomingEdges.Add(node, new ReadOnlyHashSet<T>(NodeComparer));
                }
                foreach (T node in nodesToRemove)
                {
                    if (!m_Nodes.Contains(node))
                    { continue; }

                    m_Nodes.Remove(node);

                    m_OutgoingEdges.Remove(node);
                    m_IncomingEdges.Remove(node);

                    if (m_CachedValues.ContainsKey(node))
                    { m_CachedValues.Remove(node); }
                }

                foreach (IEdge<T> edge in edgesToAdd)
                {
                    if (m_Edges.Contains(edge))
                    { continue; }

                    m_Edges.Add(edge);

                    T outgoingNode = edge.OutgoingNode;
                    T incomingNode = edge.IncomingNode;

                    m_OutgoingEdges[outgoingNode].Add(incomingNode);
                    m_IncomingEdges[incomingNode].Add(outgoingNode);
                }
                foreach (IEdge<T> edge in edgesToRemove)
                {
                    if (!m_Edges.Contains(edge))
                    { continue; }

                    m_Edges.Remove(edge);

                    T outgoingNode = edge.OutgoingNode;
                    T incomingNode = edge.IncomingNode;

                    m_OutgoingEdges[outgoingNode].Remove(incomingNode);
                    m_IncomingEdges[incomingNode].Remove(outgoingNode);
                }

                foreach (T outgoingNode in m_OutgoingEdges.Where(oe => (oe.Value.Count <= 0)).Select(oe => oe.Key))
                {
                    m_EndNodes.Add(outgoingNode);
                }

                foreach (T incomingNode in m_IncomingEdges.Where(ie => (ie.Value.Count <= 0)).Select(ie => ie.Key))
                {
                    m_StartNodes.Add(incomingNode);
                }
            }
            catch (Exception e)
            { throw e; }
            finally
            { SetReadOnly(true); }
        }

        public void RemoveValues(IEnumerable<T> nodesToRemove, IEnumerable<Edge<T>> edgesToRemove)
        {
            RemoveValues(nodesToRemove, edgesToRemove.Select(e => (IEdge<T>)e).ToHashSet(EdgeComparer));
        }

        public void RemoveValues(IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove)
        {
            UpdateValues(new HashSet<T>(NodeComparer), new HashSet<IEdge<T>>(EdgeComparer), nodesToRemove, edgesToRemove);
        }

        #endregion

        protected void SetReadOnly(bool readOnly)
        {
            m_IsEditLocked = readOnly;

            m_Nodes.IsReadOnly = readOnly;
            m_Edges.IsReadOnly = readOnly;
            m_OutgoingEdges.IsReadOnly = readOnly;
            m_IncomingEdges.IsReadOnly = readOnly;
            m_StartNodes.IsReadOnly = readOnly;
            m_EndNodes.IsReadOnly = readOnly;

            foreach (var value in m_OutgoingEdges.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;
            }
            foreach (var value in m_IncomingEdges.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;
            }
        }

        public IEqualityComparer<T> NodeComparer
        {
            get { return m_NodeComparer; }
        }

        public IEqualityComparer<IEdge<T>> EdgeComparer
        {
            get { return m_EdgeComparer; }
        }

        public IOrderManager<T> OrderManager
        {
            get { return m_OrderManager; }
            set
            {
                if (value == null)
                { m_OrderManager = new NoOpOrderManager<T>(); }
                else
                { m_OrderManager = value; }
            }
        }

        public bool IsEditLocked
        {
            get { return m_IsEditLocked; }
        }

        public ICollection<T> Nodes
        {
            get { return m_OrderManager.ToOrderedSet(m_Nodes, NodeComparer); }
        }

        public ICollection<IEdge<T>> Edges
        {
            get { return m_Edges.ToHashSet(EdgeComparer); }
        }

        public IDictionary<T, ICollection<T>> OutgoingEdges
        {
            get
            {
                var outEdges = m_OutgoingEdges.ToDictionary(kvp => kvp.Key, kvp => (ICollection<T>)m_OrderManager.ToOrderedSet(kvp.Value, NodeComparer), NodeComparer);
                return m_OrderManager.ToOrderedDictionary(outEdges, NodeComparer);
            }
        }

        public IDictionary<T, ICollection<T>> IncomingEdges
        {
            get
            {
                var outEdges = m_IncomingEdges.ToDictionary(kvp => kvp.Key, kvp => (ICollection<T>)m_OrderManager.ToOrderedSet(kvp.Value, NodeComparer), NodeComparer);
                return m_OrderManager.ToOrderedDictionary(outEdges, NodeComparer);
            }
        }

        public ICollection<T> StartNodes
        {
            get { return m_OrderManager.ToOrderedSet(m_StartNodes, NodeComparer); }
        }

        public ICollection<T> EndNodes
        {
            get { return m_OrderManager.ToOrderedSet(m_EndNodes, NodeComparer); }
        }

        public bool Contains(T node)
        {
            return m_Nodes.Contains(node);
        }

        public bool HasCycles()
        {
            IEnumerable<T> nodesInCycles;
            return HasCycles(DefaultIgnoreSelfCyles, out nodesInCycles);
        }

        public bool HasCycles(bool ignoreSelfCyles)
        {
            IEnumerable<T> nodesInCycles;
            return HasCycles(ignoreSelfCyles, out nodesInCycles);
        }

        public bool HasCycles(out IEnumerable<T> nodesInCycles)
        {
            return HasCycles(DefaultIgnoreSelfCyles, out nodesInCycles);
        }

        public bool HasCycles(bool ignoreSelfCyles, out IEnumerable<T> nodesInCycles)
        {
            var remainingNodes = m_Nodes.ToHashSet(NodeComparer);
            List<IEdge<T>> remainingEdges = new List<IEdge<T>>(m_Edges);

            return AlgortihmUtils.HasCycles<T>(remainingNodes, remainingEdges, ignoreSelfCyles, out nodesInCycles);
        }

        public ICollection<T> GetParents(T node)
        {
            if (!Contains(node))
            { return new HashSet<T>(NodeComparer); }

            ISet<T> parents = m_IncomingEdges[node];
            return m_OrderManager.ToOrderedSet(parents, NodeComparer);
        }

        public ICollection<T> GetAncestors(T node)
        {
            return GetAncestorsWithDistances(node).KeysToHashSet(NodeComparer);
        }

        public IDictionary<T, int> GetAncestorsWithDistances(T node)
        {
            if (!Contains(node))
            { return new Dictionary<T, int>(NodeComparer); }

            Dictionary<T, int> result = new Dictionary<T, int>(NodeComparer);
            ICollection<T> parents = m_IncomingEdges[node];
            int distance = 1;

            while (parents.Count > 0)
            {
                var nextParents = new HashSet<T>(NodeComparer);

                foreach (T parentNode in parents)
                {
                    if (result.ContainsKey(parentNode))
                    { continue; }

                    result.Add(parentNode, distance);
                    nextParents.AddRange(m_IncomingEdges[parentNode]);
                }

                parents = nextParents;
                distance++;
            }
            return m_OrderManager.ToOrderedDictionary(result, NodeComparer);
        }

        public ICollection<T> GetChildren(T node)
        {
            if (!Contains(node))
            { return new HashSet<T>(NodeComparer); }

            ISet<T> children = m_OutgoingEdges[node];
            return m_OrderManager.ToOrderedSet(children, NodeComparer);
        }

        public ICollection<T> GetDecendants(T node)
        {
            return GetDecendantsWithDistances(node).KeysToHashSet(NodeComparer);
        }

        public IDictionary<T, int> GetDecendantsWithDistances(T node)
        {
            if (!Contains(node))
            { return new Dictionary<T, int>(NodeComparer); }

            Dictionary<T, int> result = new Dictionary<T, int>(NodeComparer);
            ICollection<T> children = m_OutgoingEdges[node];
            int distance = 1;

            while (children.Count > 0)
            {
                var nextChildren = new HashSet<T>(NodeComparer);

                foreach (T childNode in children)
                {
                    if (result.ContainsKey(childNode))
                    { continue; }

                    result.Add(childNode, distance);
                    nextChildren.AddRange(m_OutgoingEdges[childNode]);
                }

                children = nextChildren;
                distance++;
            }
            return m_OrderManager.ToOrderedDictionary(result, NodeComparer);
        }

        public ICollection<T> GetSiblings(T node)
        {
            if (!Contains(node))
            { return new HashSet<T>(NodeComparer); }

            ICollection<T> parents = m_IncomingEdges[node];
            ICollection<T> children = m_OutgoingEdges[node];
            HashSet<T> siblings = new HashSet<T>();

            foreach (T currentNode in m_Nodes)
            {
                if (currentNode.Equals(node))
                { continue; }

                ICollection<T> currentParents = m_IncomingEdges[currentNode];
                ICollection<T> currentChildren = m_OutgoingEdges[currentNode];

                IEnumerable<T> parentOverlap = currentParents.Intersect(parents);
                IEnumerable<T> childOverlap = currentChildren.Intersect(children);

                bool parentsPass = (parents.Count != 0) ? (parentOverlap.Count() > 0) : (currentParents.Count == 0);
                bool childrenPass = (children.Count != 0) ? (childOverlap.Count() > 0) : (currentChildren.Count == 0);

                if (parentsPass && childrenPass)
                { siblings.Add(currentNode); }
            }

            return m_OrderManager.ToOrderedSet(siblings, NodeComparer);
        }

        public ICollection<T> GetRelatives(T node)
        {
            return GetRelatives(node, DefaultTreatSiblingsAsRelatives);
        }

        public ICollection<T> GetRelatives(T node, bool treatSiblingsAsRelatives)
        {
            ICollection<T> nonRelatives = GetNonRelatives(node, treatSiblingsAsRelatives);
            var relatives = Nodes.Where(n => !nonRelatives.Contains(n)).ToHashSet(NodeComparer);

            return m_OrderManager.ToOrderedSet(relatives, NodeComparer);
        }

        public ICollection<T> GetNonRelatives(T node)
        {
            return GetNonRelatives(node, DefaultTreatSiblingsAsRelatives);
        }

        public ICollection<T> GetNonRelatives(T node, bool treatSiblingsAsRelatives)
        {
            if (!Contains(node))
            { return new HashSet<T>(NodeComparer); }

            IDictionary<T, int> ancestors = GetAncestorsWithDistances(node);
            IDictionary<T, int> descendants = GetDecendantsWithDistances(node);
            ICollection<T> siblings = new HashSet<T>(NodeComparer);
            if (treatSiblingsAsRelatives)
            { siblings = GetSiblings(node); }

            HashSet<T> unrelated = new HashSet<T>(NodeComparer);
            foreach (T otherNode in m_Nodes)
            {
                if (otherNode.Equals(node))
                { continue; }
                if (ancestors.ContainsKey(otherNode))
                { continue; }
                if (descendants.ContainsKey(otherNode))
                { continue; }
                if (siblings.Contains(otherNode))
                { continue; }
                unrelated.Add(otherNode);
            }
            return m_OrderManager.ToOrderedSet(unrelated, NodeComparer);
        }

        public ICollection<T> GetDepthFirstTraversalFromRoot()
        {
            var result = AlgortihmUtils.GetDepthFirstTraversalFromRoot(StartNodes, GetChildren);
            return result;
        }

        public ICollection<T> GetBreadthFirstTraversalFromRoot()
        {
            var result = AlgortihmUtils.GetBreadthFirstTraversalFromRoot(StartNodes, GetChildren);
            return result;
        }

        public ICollection<T> GetDependencyOrderedTraversalFromRoot()
        {
            var result = AlgortihmUtils.GetDependencyOrderedTraversalFromRoot(StartNodes, GetChildren, GetParents);
            return result;
        }

        public IList<ICollection<T>> GetSubNetworks()
        {
            var nodes = Nodes;
            var edges = Edges;
            var results = AlgortihmUtils.GetIndependentSubGraphs(nodes, edges);
            return results;
        }

        public ICollection<T> GetSubNetworkForMember(T member)
        {
            var nodes = Nodes;
            var edges = Edges;
            var result = AlgortihmUtils.GetIndependentSubGraph(nodes, edges, member);
            return result;
        }

        public IList<ICollection<T>> GetSubNetworksAboveMembers(IEnumerable<T> members)
        {
            var nodes = new List<T>();
            nodes.AddRange(members);
            foreach (var member in members)
            {
                var ancestors = this.GetAncestors(member);
                nodes.AddRange(ancestors);
            }

            var uniqueNodes = nodes.ToHashSet();
            var edges = Edges.Where(x => (uniqueNodes.Contains(x.IncomingNode) && (uniqueNodes.Contains(x.OutgoingNode)))).ToList();

            var results = AlgortihmUtils.GetIndependentSubGraphs(uniqueNodes, edges);
            return results;
        }

        public IList<T> GetPath(T ancestor, T descendant)
        {
            var currentPath = new List<T>();
            var result = ExplorePath(ancestor, descendant, currentPath);
            return result;
        }

        private List<T> ExplorePath(T ancestor, T descendant, List<T> currentPath)
        {
            var newPath = currentPath.ToList();
            newPath.Add(ancestor);

            if (NodeComparer == null)
            {
                if (ancestor.Equals(descendant))
                { return newPath; }
            }
            else
            {
                if (NodeComparer.Equals(ancestor, descendant))
                { return newPath; }
            }

            foreach (var child in GetChildren(ancestor))
            {
                var result = ExplorePath(child, descendant, newPath);

                if (result != null)
                { return result; }
            }
            return null;
        }

        public IList<List<T>> GetAllPaths(T ancestor, T descendant)
        {
            var currentPath = new List<T>();
            var result = ExploreAllPaths(ancestor, descendant, currentPath);
            return result;
        }

        private List<List<T>> ExploreAllPaths(T ancestor, T descendant, List<T> currentPath)
        {
            var newPaths = new List<List<T>>();

            var newPath = currentPath.ToList();
            newPath.Add(ancestor);

            if (NodeComparer == null)
            {
                if (ancestor.Equals(descendant))
                {
                    newPaths.Add(newPath);
                    return newPaths;
                }
            }
            else
            {
                if (NodeComparer.Equals(ancestor, descendant))
                {
                    newPaths.Add(newPath);
                    return newPaths;
                }
            }

            foreach (var child in GetChildren(ancestor))
            {
                var results = ExploreAllPaths(child, descendant, newPath);

                foreach (var result in results)
                { newPaths.Add(result); }
            }
            return newPaths;
        }

        public object GetCachedValue(T node)
        {
            return m_CachedValues[node];
        }

        public CV GetCachedValue<CV>(T node)
        {
            object cachedValue = GetCachedValue(node);
            return (CV)cachedValue;
        }

        public void SetCachedValue(T node, object cachedValue)
        {
            m_CachedValues[node] = cachedValue;
        }

        public void SetCachedValue<CV>(T node, CV cachedValue)
        {
            object valueToCache = (object)cachedValue;
            SetCachedValue(node, valueToCache);
        }
    }
}