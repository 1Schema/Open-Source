using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;

namespace Decia.Business.Common.DataStructures
{
    public interface ITree<T>
         where T : struct, IComparable
    {
        IOrderManager<T> OrderManager { get; set; }

        ICollection<T> Nodes { get; }
        IDictionary<T, T?> ChildParents { get; }
        ICollection<T> RootNodes { get; }
        ICollection<T> LeafNodes { get; }

        bool Contains(T node);

        bool HasCycles();
        bool HasCycles(bool ignoreSelfCyles);
        bool HasCycles(out IEnumerable<T> nodesInCycles);
        bool HasCycles(bool ignoreSelfCyles, out IEnumerable<T> nodesInCycles);

        Nullable<T> GetParent(T node);
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

        IList<ICollection<T>> GetSubTrees();
        ICollection<T> GetSubTreeForMember(T member);

        object GetCachedValue(T node);
        CV GetCachedValue<CV>(T node);
        void SetCachedValue(T node, object cachedValue);
        void SetCachedValue<CV>(T node, CV cachedValue);
    }
}