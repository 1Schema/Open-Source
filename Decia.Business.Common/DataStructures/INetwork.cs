using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;

namespace Decia.Business.Common.DataStructures
{
    public interface INetwork<T>
         where T : struct, IComparable
    {
        void AddValues(IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd);
        void UpdateValues(IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd, IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove);
        void RemoveValues(IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove);

        IOrderManager<T> OrderManager { get; set; }

        ICollection<T> Nodes { get; }
        ICollection<IEdge<T>> Edges { get; }
        IDictionary<T, ICollection<T>> OutgoingEdges { get; }
        IDictionary<T, ICollection<T>> IncomingEdges { get; }
        ICollection<T> StartNodes { get; }
        ICollection<T> EndNodes { get; }

        bool Contains(T node);

        bool HasCycles();
        bool HasCycles(bool ignoreSelfCyles);
        bool HasCycles(out IEnumerable<T> nodesInCycles);
        bool HasCycles(bool ignoreSelfCyles, out IEnumerable<T> nodesInCycles);

        ICollection<T> GetParents(T node);
        ICollection<T> GetAncestors(T node);
        IDictionary<T, int> GetAncestorsWithDistances(T node);

        ICollection<T> GetChildren(T node);
        ICollection<T> GetDecendants(T node);
        IDictionary<T, int> GetDecendantsWithDistances(T node);

        ICollection<T> GetSiblings(T node);

        ICollection<T> GetRelatives(T node);
        ICollection<T> GetRelatives(T node, bool treatSiblingsAsRelatives);
        ICollection<T> GetNonRelatives(T node);
        ICollection<T> GetNonRelatives(T node, bool treatSiblingsAsRelatives);

        ICollection<T> GetDepthFirstTraversalFromRoot();
        ICollection<T> GetBreadthFirstTraversalFromRoot();
        ICollection<T> GetDependencyOrderedTraversalFromRoot();

        IList<ICollection<T>> GetSubNetworks();
        ICollection<T> GetSubNetworkForMember(T member);
        IList<ICollection<T>> GetSubNetworksAboveMembers(IEnumerable<T> members);
        IList<T> GetPath(T ancestor, T descendant);

        object GetCachedValue(T node);
        CV GetCachedValue<CV>(T node);
        void SetCachedValue(T node, object cachedValue);
        void SetCachedValue<CV>(T node, CV cachedValue);
    }
}