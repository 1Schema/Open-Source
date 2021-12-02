using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.DataStructures
{
    public interface ITimeNetwork<T>
         where T : struct, IComparable
    {
        void AddValues(TimePeriod period, IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd);
        void AddValues(IEnumerable<ITimeNode<T>> nodesToAdd, IEnumerable<ITimeEdge<T>> edgesToAdd);
        void UpdateValues(TimePeriod period, IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd, IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove);
        void UpdateValues(IEnumerable<ITimeNode<T>> nodesToAdd, IEnumerable<ITimeEdge<T>> edgesToAdd, IEnumerable<ITimeNode<T>> nodesToRemove, IEnumerable<ITimeEdge<T>> edgesToRemove);
        void RemoveValues(TimePeriod period, IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove);
        void RemoveValues(IEnumerable<ITimeNode<T>> nodesToRemove, IEnumerable<ITimeEdge<T>> edgesToRemove);

        INetwork<T> ForeverNetwork { get; }

        ICollection<TimePeriod> TimePeriods { get; }
        ICollection<TimePeriod> GetRelevantPeriods(TimePeriod desiredPeriod);
        TimePeriod ConvertDesiredPeriodToAssessedPeriod(TimePeriod desiredPeriod);

        IDictionary<TimePeriod, ICollection<T>> AllNodes { get; }
        IDictionary<TimePeriod, ICollection<IEdge<T>>> AllEdges { get; }
        IDictionary<TimePeriod, ICollection<T>> AllStartNodes { get; }
        IDictionary<TimePeriod, ICollection<T>> AllEndNodes { get; }

        ICollection<T> GetPeriodSpecificNodesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetForeverNodesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetAllNodesForPeriod(TimePeriod desiredPeriod);
        ICollection<IEdge<T>> GetPeriodSpecificEdgesForPeriod(TimePeriod desiredPeriod);
        ICollection<IEdge<T>> GetForeverEdgesForPeriod(TimePeriod desiredPeriod);
        ICollection<IEdge<T>> GetAllEdgesForPeriod(TimePeriod desiredPeriod);
        IDictionary<T, ICollection<T>> GetPeriodSpecificOutgoingEdgesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetPeriodSpecificOutgoingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode);
        IDictionary<T, ICollection<T>> GetPeriodSpecificOutgoingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes);
        IDictionary<T, ICollection<T>> GetForeverOutgoingEdgesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetForeverOutgoingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode);
        IDictionary<T, ICollection<T>> GetForeverOutgoingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes);
        IDictionary<T, ICollection<T>> GetAllOutgoingEdgesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetAllOutgoingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode);
        IDictionary<T, ICollection<T>> GetAllOutgoingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes);
        IDictionary<T, ICollection<T>> GetPeriodSpecificIncomingEdgesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetPeriodSpecificIncomingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode);
        IDictionary<T, ICollection<T>> GetPeriodSpecificIncomingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes);
        IDictionary<T, ICollection<T>> GetForeverIncomingEdgesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetForeverIncomingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode);
        IDictionary<T, ICollection<T>> GetForeverIncomingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes);
        IDictionary<T, ICollection<T>> GetAllIncomingEdgesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetAllIncomingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode);
        IDictionary<T, ICollection<T>> GetAllIncomingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes);
        ICollection<T> GetAllStartNodesForPeriod(TimePeriod desiredPeriod);
        ICollection<T> GetAllEndNodesForPeriod(TimePeriod desiredPeriod);

        bool Contains(T node);
        bool Contains(TimePeriod desiredPeriod, T node);

        bool HasCycles(TimePeriod desiredPeriod);
        bool HasCycles(TimePeriod desiredPeriod, bool ignoreSelfCyles);
        bool HasCycles(TimePeriod desiredPeriod, out IEnumerable<T> nodesInCycles);
        bool HasCycles(TimePeriod desiredPeriod, bool ignoreSelfCyles, out IEnumerable<T> nodesInCycles);

        ICollection<T> GetParents(TimePeriod desiredPeriod, T node);
        ICollection<T> GetAncestors(TimePeriod desiredPeriod, T node);
        IDictionary<T, int> GetAncestorsWithDistances(TimePeriod desiredPeriod, T node);

        ICollection<T> GetChildren(TimePeriod desiredPeriod, T node);
        ICollection<T> GetDecendants(TimePeriod desiredPeriod, T node);
        IDictionary<T, int> GetDecendantsWithDistances(TimePeriod desiredPeriod, T node);

        ICollection<T> GetSiblings(TimePeriod desiredPeriod, T node);

        ICollection<T> GetRelatives(TimePeriod desiredPeriod, T node);
        ICollection<T> GetRelatives(TimePeriod desiredPeriod, T node, bool treatSiblingsAsRelatives);
        ICollection<T> GetNonRelatives(TimePeriod desiredPeriod, T node);
        ICollection<T> GetNonRelatives(TimePeriod desiredPeriod, T node, bool treatSiblingsAsRelatives);

        ICollection<T> GetDepthFirstTraversalFromRoot(TimePeriod desiredPeriod);
        ICollection<T> GetBreadthFirstTraversalFromRoot(TimePeriod desiredPeriod);
        ICollection<T> GetDependencyOrderedTraversalFromRoot(TimePeriod desiredPeriod);

        object GetCachedValue(TimePeriod desiredPeriod, T node);
        CV GetCachedValue<CV>(TimePeriod desiredPeriod, T node);
        void SetCachedValue(TimePeriod desiredPeriod, T node, object cachedValue);
        void SetCachedValue<CV>(TimePeriod desiredPeriod, T node, CV cachedValue);
    }
}