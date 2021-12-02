using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;

namespace Decia.Business.Common.DataStructures
{
    public static class AlgortihmUtils
    {
        #region Depth-First Traversal Methods

        public static ICollection<T> GetDepthFirstTraversalFromRoot<T>(IEnumerable<T> rootNodes, Func<T, ICollection<T>> getChildrenMethod)
             where T : struct, IComparable
        {
            var nodeComparer = rootNodes.GetComparer();

            var depthFirstTraversal = new Dictionary<T, int>(nodeComparer);
            var loopElements = rootNodes.ToList();

            while (loopElements.Count > 0)
            {
                T currentNode = loopElements[0];
                loopElements.RemoveAt(0);

                if (depthFirstTraversal.ContainsKey(currentNode))
                { continue; }

                depthFirstTraversal.Add(currentNode, depthFirstTraversal.Count);
                var children = getChildrenMethod(currentNode);

                for (int i = children.Count - 1; i >= 0; i--)
                { loopElements.Insert(0, children.ElementAt(i)); }
            }
            return depthFirstTraversal.Keys.ToHashSet(nodeComparer);
        }

        #endregion

        #region Breadth-First Traversal Methods

        public static ICollection<T> GetBreadthFirstTraversalFromRoot<T>(IEnumerable<T> rootNodes, Func<T, ICollection<T>> getChildrenMethod)
             where T : struct, IComparable
        {
            var nodeComparer = rootNodes.GetComparer();

            var breadthFirstTraversal = new Dictionary<T, int>(nodeComparer);
            var loopElements = rootNodes.ToList();

            while (loopElements.Count > 0)
            {
                T currentNode = loopElements[0];
                loopElements.RemoveAt(0);

                if (breadthFirstTraversal.ContainsKey(currentNode))
                { continue; }

                breadthFirstTraversal.Add(currentNode, breadthFirstTraversal.Count);
                var children = getChildrenMethod(currentNode);

                for (int i = 0; i < children.Count; i++)
                { loopElements.Add(children.ElementAt(i)); }
            }
            return breadthFirstTraversal.Keys.ToHashSet(nodeComparer);
        }

        #endregion

        #region Dependency-Ordered Traversal Methods

        public static ICollection<T> GetDependencyOrderedTraversalFromRoot<T>(IEnumerable<T> rootNodes, Func<T, ICollection<T>> getChildrenMethod, Func<T, ICollection<T>> getParentsMethod)
             where T : struct, IComparable
        {
            var nodeComparer = rootNodes.GetComparer();

            var dependencyOrderedTraversal = new Dictionary<T, int>(nodeComparer);
            var loopElements = rootNodes.ToList();

            while (loopElements.Count > 0)
            {
                T currentNode = loopElements[0];
                loopElements.RemoveAt(0);

                if (dependencyOrderedTraversal.ContainsKey(currentNode))
                { continue; }

                ICollection<T> parents = getParentsMethod(currentNode);
                bool waitToProcessCurrent = false;
                foreach (T parent in parents)
                {
                    if (!dependencyOrderedTraversal.ContainsKey(parent))
                    { waitToProcessCurrent = true; }
                }
                if (waitToProcessCurrent)
                {
                    loopElements.Add(currentNode);
                    continue;
                }

                dependencyOrderedTraversal.Add(currentNode, dependencyOrderedTraversal.Count);
                var children = getChildrenMethod(currentNode);

                for (int i = 0; i < children.Count; i++)
                { loopElements.Add(children.ElementAt(i)); }
            }
            return dependencyOrderedTraversal.Keys.ToList();
        }

        #endregion

        #region HasCycles Methods

        public static bool HasCycles<T>(IEnumerable<T> nodes, IEnumerable<IEdge<T>> edges, bool ignoreSelfCyles, out IEnumerable<T> nodesInCycles)
            where T : struct, IComparable
        {
            var nodeComparer = nodes.GetComparer();

            var remainingNodes = nodes.ToList();
            var remainingEdges = edges.ToList();

            if (ignoreSelfCyles)
            { remainingEdges = remainingEdges.Where(kvp => (!kvp.OutgoingNode.Equals(kvp.IncomingNode))).ToList(); }

            int iterationsSinceLastChange = 0;
            while (remainingEdges.Count > 0)
            {
                int countBeforeUpdate = remainingNodes.Count;
                HasCycles_ProcessNode<T>(remainingNodes, remainingEdges, nodeComparer);
                int countAfterUpdate = remainingNodes.Count;

                if (countAfterUpdate >= countBeforeUpdate)
                { iterationsSinceLastChange++; }
                else
                { iterationsSinceLastChange = 0; }

                if (iterationsSinceLastChange > remainingNodes.Count)
                {
                    nodesInCycles = remainingNodes.ToHashSet(nodeComparer);
                    return true;
                }
            }

            nodesInCycles = new HashSet<T>(nodeComparer);
            return false;
        }

        private static void HasCycles_ProcessNode<T>(List<T> remainingNodes, List<IEdge<T>> remainingEdges, IEqualityComparer<T> nodeComparer)
            where T : struct, IComparable
        {
            T currentNode = remainingNodes.First();
            remainingNodes.Remove(currentNode);

            var incomingEdges = remainingEdges.Where(e => nodeComparer.Equals(e.IncomingNode, currentNode)).ToHashSet(new EdgeEqualityComparer<T>(nodeComparer));
            bool hasIncomingEdges = (incomingEdges.Count() > 0);

            if (hasIncomingEdges)
            {
                remainingNodes.Add(currentNode);
            }
            else
            {
                var outgoingEdges = remainingEdges.Where(e => nodeComparer.Equals(e.OutgoingNode, currentNode)).ToHashSet(new EdgeEqualityComparer<T>(nodeComparer));

                foreach (IEdge<T> outgoingEdge in outgoingEdges)
                { remainingEdges.Remove(outgoingEdge); }
            }
        }

        #endregion

        #region GetCycles Methods

        public static IList<ICycleGroup<T>> GetCycles<T>(IEnumerable<T> nodes, IEnumerable<IEdge<T>> edges, bool ignoreSelfCyles)
           where T : struct, IComparable
        {
            var nodeComparer = nodes.GetComparer();

            int tarjanIndex = 0;
            List<T> tarjanStack = new List<T>();
            Dictionary<T, int> tarjanIndices = new Dictionary<T, int>(nodeComparer);
            Dictionary<T, int> tarjanLowLinks = new Dictionary<T, int>(nodeComparer);
            List<ICycleGroup<T>> outputCycleGroups = new List<ICycleGroup<T>>();

            foreach (T node in nodes)
            {
                if (tarjanIndices.ContainsKey(node))
                { continue; }

                GetCycles_StrongConnect(node, ref tarjanIndex, nodes, edges, ignoreSelfCyles, nodeComparer, tarjanStack, tarjanIndices, tarjanLowLinks, outputCycleGroups);
            }

            return outputCycleGroups;
        }

        private static void GetCycles_StrongConnect<T>(T currentNode, ref int currentTarjanIndex, IEnumerable<T> nodes, IEnumerable<IEdge<T>> edges, bool ignoreSelfCyles, IEqualityComparer<T> nodeComparer, List<T> tarjanStack, Dictionary<T, int> tarjanIndices, Dictionary<T, int> tarjanLowLinks, List<ICycleGroup<T>> outputCycleGroups)
            where T : struct, IComparable
        {
            tarjanIndices[currentNode] = currentTarjanIndex;
            tarjanLowLinks[currentNode] = currentTarjanIndex;
            currentTarjanIndex++;
            tarjanStack.Push(currentNode);


            bool hasAtLeastOneEdge = false;
            foreach (IEdge<T> edge in edges)
            {
                if (!edge.OutgoingNode.Equals(currentNode))
                { continue; }

                T nextNode = edge.IncomingNode;

                if (!tarjanIndices.ContainsKey(nextNode))
                {
                    GetCycles_StrongConnect<T>(nextNode, ref currentTarjanIndex, nodes, edges, ignoreSelfCyles, nodeComparer, tarjanStack, tarjanIndices, tarjanLowLinks, outputCycleGroups);
                    tarjanLowLinks[currentNode] = Math.Min(tarjanLowLinks[currentNode], tarjanLowLinks[nextNode]);
                    hasAtLeastOneEdge = true;
                }
                else if (tarjanStack.Contains(nextNode))
                {
                    tarjanLowLinks[currentNode] = Math.Min(tarjanLowLinks[currentNode], tarjanIndices[nextNode]);
                    hasAtLeastOneEdge = true;
                }
            }


            if (tarjanLowLinks[currentNode] != tarjanIndices[currentNode])
            { return; }

            List<T> nodesInCycleGroup = new List<T>();
            Nullable<T> stackNode = null;
            bool isStackNodeCurrentNode = false;

            do
            {
                stackNode = tarjanStack.Pop();

                if (!stackNode.HasValue)
                { continue; }

                nodesInCycleGroup.Add(stackNode.Value);
                isStackNodeCurrentNode = currentNode.Equals(stackNode.Value);
            }
            while ((tarjanStack.Count > 0) && (!isStackNodeCurrentNode));


            if (nodesInCycleGroup.Count < 1)
            {
                return;
            }

            if (nodesInCycleGroup.Count == 1)
            {
                var loneNode = nodesInCycleGroup.ElementAt(0);
                var selfEdges = edges.Where(e => (e.OutgoingNode.Equals(loneNode)) && (e.IncomingNode.Equals(loneNode))).ToList();

                if (selfEdges.Count < 1)
                {
                    return;
                }
            }

            CycleGroup<T> cycleGroup = new CycleGroup<T>(nodesInCycleGroup, nodeComparer);
            outputCycleGroups.Add(cycleGroup);
        }

        #endregion

        #region Get Independent Sub-Graphs Methods

        public static IList<ICollection<T>> GetIndependentSubGraphs<T>(IEnumerable<T> nodes, IEnumerable<IEdge<T>> edges)
            where T : struct, IComparable
        {
            Nullable<T> memberOfSubGraph = null;
            var subGraphs = GetIndependentSubGraphs(nodes, edges, memberOfSubGraph);
            return subGraphs;
        }

        public static ICollection<T> GetIndependentSubGraph<T>(IEnumerable<T> nodes, IEnumerable<IEdge<T>> edges, T memberOfSubGraph)
           where T : struct, IComparable
        {
            var subGraphs = GetIndependentSubGraphs(nodes, edges, memberOfSubGraph);
            var subGraph = subGraphs.First();
            return subGraph;
        }

        private static IList<ICollection<T>> GetIndependentSubGraphs<T>(IEnumerable<T> nodes, IEnumerable<IEdge<T>> edges, Nullable<T> memberOfSubGraph)
            where T : struct, IComparable
        {
            if (memberOfSubGraph.HasValue && !nodes.Contains(memberOfSubGraph.Value))
            { throw new InvalidOperationException("The specified StartNode is not in the Graph."); }

            var groups = new List<ICollection<T>>();
            var unprocessedNodes = new HashSet<T>(nodes);
            var unprocessedEdges = new Dictionary<int, IEdge<T>>();

            foreach (var edge in edges)
            { unprocessedEdges.Add(unprocessedEdges.Count, edge); }

            while (unprocessedNodes.Count > 0)
            {
                var node = memberOfSubGraph.HasValue ? memberOfSubGraph.Value : unprocessedNodes.First();
                unprocessedNodes.Remove(node);

                var groupMembers = new List<T>();
                groupMembers.Add(node);

                var edgeIndicesToProcess = new List<int>();

                edgeIndicesToProcess.AddRange(unprocessedEdges.Where(x => (x.Value.IncomingNode.Equals(node)) && (!x.Value.OutgoingNode.Equals(node))).Select(x => x.Key).ToList());
                edgeIndicesToProcess.AddRange(unprocessedEdges.Where(x => (x.Value.OutgoingNode.Equals(node)) && (!x.Value.IncomingNode.Equals(node))).Select(x => x.Key).ToList());

                while ((edgeIndicesToProcess.Count > 0) && (unprocessedEdges.Count > 0))
                {
                    var nextEdgeIndex = edgeIndicesToProcess.First();
                    var nextEdge = unprocessedEdges[nextEdgeIndex];
                    var nextNode = unprocessedNodes.Contains(nextEdge.OutgoingNode) ? nextEdge.OutgoingNode : nextEdge.IncomingNode;

                    edgeIndicesToProcess.RemoveAt(0);
                    unprocessedEdges.Remove(nextEdgeIndex);

                    if (!unprocessedNodes.Contains(nextNode))
                    { continue; }

                    unprocessedNodes.Remove(nextNode);
                    groupMembers.Add(nextNode);

                    edgeIndicesToProcess.AddRange(unprocessedEdges.Where(x => (x.Value.IncomingNode.Equals(nextNode)) && (!x.Value.OutgoingNode.Equals(nextNode))).Select(x => x.Key).ToList());
                    edgeIndicesToProcess.AddRange(unprocessedEdges.Where(x => (x.Value.OutgoingNode.Equals(nextNode)) && (!x.Value.IncomingNode.Equals(nextNode))).Select(x => x.Key).ToList());
                }

                groups.Add(groupMembers.ToHashSet());

                if (memberOfSubGraph.HasValue)
                { return groups; }
            }

            return groups;
        }

        #endregion

        #region Common Utility Methods

        public static void Push<T>(this IList<T> stack, T element)
             where T : struct, IComparable
        {
            stack.Insert(0, element);
        }

        public static Nullable<T> Pop<T>(this IList<T> stack)
            where T : struct, IComparable
        {
            if (stack.Count < 1)
            { return null; }

            T result = stack[0];
            stack.RemoveAt(0);

            return result;
        }

        #endregion
    }
}