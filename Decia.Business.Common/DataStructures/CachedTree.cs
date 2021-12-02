using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Common.DataStructures
{
    public class CachedTree<T> : ITree<T>
        where T : struct, IComparable
    {
        public const bool DefaultIgnoreSelfCyles = false;
        public const bool DefaultTreatSiblingsAsRelatives = false;
        public static readonly Nullable<T> NullT = null;

        private IEqualityComparer<T> m_Comparer;
        private IOrderManager<T> m_OrderManager;
        private bool m_IsEditLocked;

        private ReadOnlyDictionary<T, Nullable<T>> m_ChildParents;
        private ReadOnlyDictionary<T, ReadOnlyHashSet<T>> m_ParentChildren;
        private ReadOnlyHashSet<T> m_RootNodes;
        private ReadOnlyHashSet<T> m_LeafNodes;
        private Dictionary<T, object> m_CachedValues;

        #region Constructors

        public CachedTree(IDictionary<T, Nullable<T>> childParents)
            : this(childParents, childParents.GetComparer())
        { }

        public CachedTree(IDictionary<T, Nullable<T>> childParents, IEqualityComparer<T> comparer)
        {
            try
            {
                m_Comparer = comparer;
                m_OrderManager = new NoOpOrderManager<T>();
                m_IsEditLocked = false;

                m_ChildParents = new ReadOnlyDictionary<T, Nullable<T>>(Comparer);
                m_ParentChildren = new ReadOnlyDictionary<T, ReadOnlyHashSet<T>>(Comparer);
                m_RootNodes = new ReadOnlyHashSet<T>(Comparer);
                m_LeafNodes = new ReadOnlyHashSet<T>(Comparer);
                m_CachedValues = new Dictionary<T, object>(Comparer);

                childParents = new Dictionary<T, Nullable<T>>(childParents, Comparer);
                var explicitLinks = childParents.ToList();

                foreach (var childParent in explicitLinks)
                {
                    var childNode = childParent.Key;
                    var parentNode = childParent.Value;

                    if (!parentNode.HasValue)
                    { continue; }
                    if (childParents.ContainsKey(parentNode.Value))
                    { continue; }

                    childParents.Add(parentNode.Value, null);
                }

                foreach (T child in childParents.Keys)
                {
                    Nullable<T> parent = childParents[child];
                    m_ChildParents.Add(child, parent);

                    if (!m_ParentChildren.ContainsKey(child))
                    { m_ParentChildren.Add(child, new ReadOnlyHashSet<T>(Comparer)); }

                    if (!parent.HasValue)
                    {
                        m_RootNodes.Add(child);
                        continue;
                    }

                    if (!m_ParentChildren.ContainsKey(parent.Value))
                    { m_ParentChildren.Add(parent.Value, new ReadOnlyHashSet<T>(Comparer)); }

                    m_ParentChildren[parent.Value].Add(child);
                }

                foreach (var node in m_ChildParents.Keys)
                {
                    if (m_ParentChildren[node].Count < 1)
                    { m_LeafNodes.Add(node); }
                }
            }
            catch (Exception e)
            { throw e; }
            finally
            { SetReadOnly(true); }
        }

        #endregion

        protected void SetReadOnly(bool readOnly)
        {
            m_IsEditLocked = readOnly;

            m_ChildParents.IsReadOnly = readOnly;
            m_ParentChildren.IsReadOnly = readOnly;
            m_RootNodes.IsReadOnly = readOnly;
            m_LeafNodes.IsReadOnly = readOnly;

            foreach (var value in m_ParentChildren.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;
            }
        }

        public IEqualityComparer<T> Comparer
        {
            get { return m_Comparer; }
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
            get { return m_OrderManager.ToOrderedSet(m_ChildParents.Keys, Comparer); }
        }

        public IDictionary<T, T?> ChildParents
        {
            get { return m_ChildParents.ToDictionary(x => x.Key, x => x.Value, Comparer); }
        }

        public ICollection<T> RootNodes
        {
            get { return m_OrderManager.ToOrderedSet(m_RootNodes, Comparer); }
        }

        public ICollection<T> LeafNodes
        {
            get { return m_OrderManager.ToOrderedSet(m_LeafNodes, Comparer); }
        }

        public bool Contains(T node)
        {
            return m_ChildParents.ContainsKey(node);
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
            var remainingNodes = m_ChildParents.KeysToHashSet(Comparer);

            List<IEdge<T>> remainingEdges = m_ChildParents.Where(cp => cp.Value.HasValue).Select(cp => (IEdge<T>)new Edge<T>(cp.Value.Value, cp.Key)).ToList();

            return AlgortihmUtils.HasCycles<T>(remainingNodes, remainingEdges, ignoreSelfCyles, out nodesInCycles);
        }

        public Nullable<T> GetParent(T node)
        {
            if (!Contains(node))
            { return null; }

            Nullable<T> parent = m_ChildParents[node];
            return parent;
        }

        public ICollection<T> GetAncestors(T node)
        {
            return GetAncestorsWithDistances(node).KeysToHashSet(Comparer);
        }

        public IDictionary<T, int> GetAncestorsWithDistances(T node)
        {
            if (!Contains(node))
            { return new Dictionary<T, int>(Comparer); }

            Dictionary<T, int> result = new Dictionary<T, int>(Comparer);
            Nullable<T> parent = m_ChildParents[node];
            int distance = 1;

            while (parent.HasValue)
            {
                result.Add(parent.Value, distance);
                parent = m_ChildParents[parent.Value];
                distance++;
            }
            return m_OrderManager.ToOrderedDictionary(result, Comparer);
        }

        public ICollection<T> GetSiblings(T node)
        {
            if (!Contains(node))
            { return new HashSet<T>(Comparer); }

            Nullable<T> parent = m_ChildParents[node];
            ISet<T> siblings = null;

            if (!parent.HasValue)
            { siblings = m_RootNodes.ToHashSet(Comparer); }
            else
            { siblings = m_ParentChildren[parent.Value].ToHashSet(Comparer); }

            if (siblings.Contains(node))
            { siblings.Remove(node); }

            return m_OrderManager.ToOrderedSet(siblings, Comparer);
        }

        public ICollection<T> GetChildren(T node)
        {
            if (!Contains(node))
            { return new HashSet<T>(Comparer); }

            ISet<T> children = m_ParentChildren[node];
            return m_OrderManager.ToOrderedSet(children, Comparer);
        }

        public ICollection<T> GetDecendants(T node)
        {
            return GetDecendantsWithDistances(node).KeysToHashSet(Comparer);
        }

        public IDictionary<T, int> GetDecendantsWithDistances(T node)
        {
            if (!Contains(node))
            { return new Dictionary<T, int>(Comparer); }

            Dictionary<T, int> result = new Dictionary<T, int>(Comparer);
            ICollection<T> children = m_ParentChildren[node];
            int distance = 1;

            while (children.Count > 0)
            {
                var nextChildren = new HashSet<T>(Comparer);

                foreach (T childNode in children)
                {
                    result.Add(childNode, distance);
                    nextChildren.AddRange(m_ParentChildren[childNode]);
                }

                children = nextChildren;
                distance++;
            }
            return m_OrderManager.ToOrderedDictionary(result, Comparer);
        }

        public ICollection<T> GetRelatives(T node)
        {
            return GetRelatives(node, DefaultTreatSiblingsAsRelatives);
        }

        public ICollection<T> GetRelatives(T node, bool treatSiblingsAsRelatives)
        {
            ICollection<T> nonRelatives = GetNonRelatives(node, treatSiblingsAsRelatives);
            var relatives = Nodes.Where(n => !nonRelatives.Contains(n)).ToHashSet(Comparer);

            return m_OrderManager.ToOrderedSet(relatives, Comparer);
        }

        public ICollection<T> GetNonRelatives(T node)
        {
            return GetNonRelatives(node, DefaultTreatSiblingsAsRelatives);
        }

        public ICollection<T> GetNonRelatives(T node, bool treatSiblingsAsRelatives)
        {
            if (!Contains(node))
            { return new HashSet<T>(Comparer); }

            IDictionary<T, int> ancestors = GetAncestorsWithDistances(node);
            IDictionary<T, int> descendants = GetDecendantsWithDistances(node);
            ICollection<T> siblings = new HashSet<T>(Comparer);
            if (treatSiblingsAsRelatives)
            { siblings = GetSiblings(node); }

            HashSet<T> unrelated = new HashSet<T>(Comparer);
            foreach (T otherNode in m_ChildParents.Keys)
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
            return m_OrderManager.ToOrderedSet(unrelated, Comparer);
        }

        public ICollection<T> GetDepthFirstTraversalFromRoot()
        {
            var result = AlgortihmUtils.GetDepthFirstTraversalFromRoot(RootNodes, GetChildren);
            return result;
        }

        public ICollection<T> GetBreadthFirstTraversalFromRoot()
        {
            var result = AlgortihmUtils.GetBreadthFirstTraversalFromRoot(RootNodes, GetChildren);
            return result;
        }

        public ICollection<T> GetDependencyOrderedTraversalFromRoot()
        {
            Func<T, ICollection<T>> getParentsMethod = (node => CollectionUtils.GetNullableAsCollection(GetParent(node)));

            var result = AlgortihmUtils.GetDependencyOrderedTraversalFromRoot(RootNodes, GetChildren, getParentsMethod);
            return result;
        }

        public IList<ICollection<T>> GetSubTrees()
        {
            var nodes = Nodes;
            var edges = m_ChildParents.Where(x => x.Value.HasValue).Select(x => (IEdge<T>)new Edge<T>(x.Value.Value, x.Key)).ToList();
            var results = AlgortihmUtils.GetIndependentSubGraphs(nodes, edges);
            return results;
        }

        public ICollection<T> GetSubTreeForMember(T member)
        {
            var nodes = Nodes;
            var edges = m_ChildParents.Where(x => x.Value.HasValue).Select(x => (IEdge<T>)new Edge<T>(x.Value.Value, x.Key)).ToList();
            var result = AlgortihmUtils.GetIndependentSubGraph(nodes, edges, member);
            return result;
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