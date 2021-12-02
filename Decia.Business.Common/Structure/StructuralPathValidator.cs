using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Structure
{
    public class StructuralPathValidator
    {
        private CachedTree<ModelObjectReference> m_EntityTypeTree;
        private CachedNetwork<ModelObjectReference> m_EntityTypeExtendedNetwork;

        public StructuralPathValidator(CachedTree<ModelObjectReference> entityTypeTree, CachedNetwork<ModelObjectReference> entityTypeExtendedNetwork)
        {
            m_EntityTypeTree = entityTypeTree;
            m_EntityTypeExtendedNetwork = entityTypeExtendedNetwork;
        }

        public bool IsExistenceEdgeValid(ModelObjectReference child, ModelObjectReference proposedParent)
        {
            var currentParent = (ModelObjectReference?)null;
            return IsExistenceEdgeValid(child, currentParent, proposedParent);
        }

        public bool IsExistenceEdgeValid(ModelObjectReference child, ModelObjectReference? currentParent, ModelObjectReference proposedParent)
        {
            bool isParentAlreadySet = currentParent.HasValue;
            var existingParent = m_EntityTypeTree.GetParent(child);

            if (isParentAlreadySet)
            {
                if (existingParent == proposedParent)
                { return true; }
            }
            else
            {
                if (existingParent.HasValue)
                { return false; }
            }

            var childParents = m_EntityTypeTree.ChildParents;
            var comparer = m_EntityTypeTree.Comparer;

            if (isParentAlreadySet)
            { childParents.Remove(child); }
            childParents[child] = proposedParent;
            var testTree = new CachedTree<ModelObjectReference>(childParents);

            var hasCycle = testTree.HasCycles();
            if (hasCycle)
            { return false; }

            return IsRelatedEdgeValid(child, currentParent, proposedParent, true);
        }

        public bool IsRelatedEdgeValid(ModelObjectReference child, ModelObjectReference proposedParent)
        {
            var currentParent = (ModelObjectReference?)null;
            return IsRelatedEdgeValid(child, currentParent, proposedParent);
        }

        public bool IsRelatedEdgeValid(ModelObjectReference child, ModelObjectReference? currentParent, ModelObjectReference proposedParent)
        {
            var isComingFromParent = (bool)false;
            return IsRelatedEdgeValid(child, currentParent, proposedParent, isComingFromParent);
        }

        private bool IsRelatedEdgeValid(ModelObjectReference child, ModelObjectReference? currentParent, ModelObjectReference proposedParent, bool isComingFromParent)
        {
            if (!isComingFromParent)
            {
                if (m_EntityTypeTree.GetParent(child) == proposedParent)
                { return false; }
            }

            bool isParentAlreadySet = currentParent.HasValue;
            var existingParents = m_EntityTypeExtendedNetwork.GetParents(child);

            if (isParentAlreadySet)
            {
                if (existingParents.Contains(proposedParent))
                { return (currentParent == proposedParent); }
            }
            else
            {
                if (existingParents.Contains(proposedParent))
                { return false; }
            }

            var nodes = m_EntityTypeExtendedNetwork.Nodes.ToList();
            var edges = m_EntityTypeExtendedNetwork.Edges.ToList();
            var nodeComparer = m_EntityTypeExtendedNetwork.NodeComparer;

            if (isParentAlreadySet)
            {
                var parentEdges = edges.Where(x => (x.OutgoingNode == currentParent) && (x.IncomingNode == child)).ToList();
                foreach (var parentEdge in parentEdges)
                { edges.Remove(parentEdge); }
            }
            edges.Add(new Edge<ModelObjectReference>(proposedParent, child));
            var testNetwork = new CachedNetwork<ModelObjectReference>(nodes, edges, nodeComparer);

            var hasCycle = testNetwork.HasCycles();
            if (hasCycle)
            { return false; }

            foreach (var node1 in testNetwork.Nodes)
            {
                foreach (var node2 in testNetwork.Nodes)
                {
                    var allPaths = testNetwork.GetAllPaths(node1, node2);
                    if (allPaths.Count > 1)
                    { return false; }
                }
            }

            return true;
        }
    }
}