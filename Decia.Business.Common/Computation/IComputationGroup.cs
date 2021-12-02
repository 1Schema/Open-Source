using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using IEdge = Decia.Business.Common.DataStructures.IEdge<Decia.Business.Common.Modeling.ModelObjectReference>;
using ICycleGroup = Decia.Business.Common.DataStructures.ICycleGroup<Decia.Business.Common.Modeling.ModelObjectReference>;

namespace Decia.Business.Common.Computation
{
    public interface IComputationGroup
    {
        Guid Id { get; }
        ushort ComputeCount { get; }
        bool HasCycle { get; }
        ICycleGroup CycleGroup { get; }
        ICollection<ModelObjectReference> NodesIncluded { get; }
        ICollection<IEdge> EdgesIncluded { get; }

        bool IsProcessingInfoSet { get; }
        int ProcessingIndex { get; }
        bool RequiresComputingTimeDimensionByPeriod(int timeDimensionNumber);
        void SetProcessingInfo(int processingIndex, IEnumerable<int> timeDimensionsToComputeByPeriod);

        bool HasStrictTimeOrdering { get; }
        IList<ModelObjectReference> TimeOrderedNodes { get; }
        IDictionary<int, ModelObjectReference> TimeOrderedNodesByIndex { get; }
        IDictionary<ModelObjectReference, TimeLagSet> NodeLags { get; }

        void SetTimeOrderedNodes(bool hasStrictTimeOrdering, IEnumerable<ModelObjectReference> timeOrderedNodes, IDictionary<ModelObjectReference, TimeLagSet> nodeLags);
        void SetTimeOrderedNodes(bool hasStrictTimeOrdering, IDictionary<int, ModelObjectReference> timeOrderedNodes, IDictionary<ModelObjectReference, TimeLagSet> nodeLags);

        void IncrementComputeCount();
        bool Contains(ModelObjectReference reference);
        bool Contains(IEnumerable<ModelObjectReference> references);
    }
}