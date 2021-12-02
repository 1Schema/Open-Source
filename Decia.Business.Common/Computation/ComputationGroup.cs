using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using IEdge = Decia.Business.Common.DataStructures.IEdge<Decia.Business.Common.Modeling.ModelObjectReference>;
using Edge = Decia.Business.Common.DataStructures.Edge<Decia.Business.Common.Modeling.ModelObjectReference>;
using ICycleGroup = Decia.Business.Common.DataStructures.ICycleGroup<Decia.Business.Common.Modeling.ModelObjectReference>;
using CycleGroup = Decia.Business.Common.DataStructures.CycleGroup<Decia.Business.Common.Modeling.ModelObjectReference>;

namespace Decia.Business.Common.Computation
{
    public class ComputationGroup : IComputationGroup
    {
        private Guid m_Id;
        private ushort m_ComputeCount;
        private HashSet<ModelObjectReference> m_NodesIncluded;
        private HashSet<IEdge> m_EdgesIncluded;
        private Nullable<CycleGroup> m_CycleGroup;

        private Nullable<int> m_ProcessingIndex;
        private HashSet<int> m_TimeDimensionsToComputeByPeriod;

        private Nullable<bool> m_HasStrictTimeOrdering;
        private SortedDictionary<int, ModelObjectReference> m_TimeOrderedNodes;
        private Dictionary<ModelObjectReference, TimeLagSet> m_NodeLags;

        public ComputationGroup(IEnumerable<ModelObjectReference> nodesIncluded)
            : this(nodesIncluded, new List<IEdge>())
        { }

        public ComputationGroup(IEnumerable<ModelObjectReference> nodesIncluded, IEnumerable<IEdge> edgesIncluded)
        {
            if ((nodesIncluded == null) || (edgesIncluded == null))
            { throw new InvalidOperationException("The \"nodesIncluded\" and \"edgesIncluded\" must not be null."); }

            m_Id = Guid.NewGuid();
            m_ComputeCount = 0;
            m_NodesIncluded = new HashSet<ModelObjectReference>(nodesIncluded);
            m_EdgesIncluded = new HashSet<IEdge>(edgesIncluded);
            m_CycleGroup = null;

            m_ProcessingIndex = null;
            m_TimeDimensionsToComputeByPeriod = null;

            m_HasStrictTimeOrdering = null;
            m_TimeOrderedNodes = new SortedDictionary<int, ModelObjectReference>();
            m_NodeLags = new Dictionary<ModelObjectReference, TimeLagSet>();

            foreach (var node in nodesIncluded)
            {
                m_TimeOrderedNodes.Add(m_TimeOrderedNodes.Count, node);
                m_NodeLags.Add(node, new TimeLagSet());
            }
        }

        public ComputationGroup(ICycleGroup cycleGroup)
        {
            if (cycleGroup == null)
            { throw new InvalidOperationException("The \"cycleGroup\" must not be null."); }

            m_Id = Guid.NewGuid();
            m_ComputeCount = 0;
            m_NodesIncluded = null;
            m_EdgesIncluded = null;
            m_CycleGroup = new CycleGroup(cycleGroup.NodesIncluded, cycleGroup.EdgesIncluded);

            m_ProcessingIndex = null;
            m_TimeDimensionsToComputeByPeriod = null;
        }

        public Guid Id
        {
            get { return m_Id; }
        }

        public ushort ComputeCount
        {
            get { return m_ComputeCount; }
        }

        public bool HasCycle
        {
            get { return m_CycleGroup.HasValue; }
        }

        public ICycleGroup CycleGroup
        {
            get { return m_CycleGroup; }
        }

        public ICollection<ModelObjectReference> NodesIncluded
        {
            get
            {
                if (HasCycle)
                { return m_CycleGroup.Value.NodesIncluded; }
                return new HashSet<ModelObjectReference>(m_NodesIncluded);
            }
        }

        public ICollection<IEdge> EdgesIncluded
        {
            get
            {
                if (HasCycle)
                { return m_CycleGroup.Value.EdgesIncluded; }
                return new HashSet<IEdge>(m_EdgesIncluded);
            }
        }

        public bool IsProcessingInfoSet
        {
            get { return m_ProcessingIndex.HasValue; }
        }

        public int ProcessingIndex
        {
            get
            {
                if (!IsProcessingInfoSet)
                { throw new InvalidOperationException("The current ComputeGroup's processing info has not yet been set."); }

                return m_ProcessingIndex.Value;
            }
        }

        public bool RequiresComputingTimeDimensionByPeriod(int timeDimensionNumber)
        {
            if (!IsProcessingInfoSet)
            { throw new InvalidOperationException("The current ComputeGroup's processing info has not yet been set."); }

            return m_TimeDimensionsToComputeByPeriod.Contains(timeDimensionNumber);
        }

        public void SetProcessingInfo(int processingIndex, IEnumerable<int> timeDimensionsToComputeByPeriod)
        {
            if (IsProcessingInfoSet)
            { throw new InvalidOperationException("The current ComputeGroup's processing info has already been set."); }

            m_ProcessingIndex = processingIndex;
            m_TimeDimensionsToComputeByPeriod = new HashSet<int>(timeDimensionsToComputeByPeriod);
        }

        public bool HasStrictTimeOrdering
        {
            get { return (m_HasStrictTimeOrdering == true); }
        }

        public IList<ModelObjectReference> TimeOrderedNodes
        {
            get
            {
                List<ModelObjectReference> orderedNodes = new List<ModelObjectReference>();
                foreach (int orderKey in m_TimeOrderedNodes.Keys)
                {
                    orderedNodes.Add(m_TimeOrderedNodes[orderKey]);
                }
                return orderedNodes;
            }
        }

        public IDictionary<int, ModelObjectReference> TimeOrderedNodesByIndex
        {
            get { return new SortedDictionary<int, ModelObjectReference>(m_TimeOrderedNodes); }
        }

        public IDictionary<ModelObjectReference, TimeLagSet> NodeLags
        {
            get { return new Dictionary<ModelObjectReference, TimeLagSet>(m_NodeLags); }
        }

        public void SetTimeOrderedNodes(bool hasStrictTimeOrdering, IEnumerable<ModelObjectReference> timeOrderedNodes, IDictionary<ModelObjectReference, TimeLagSet> nodeLags)
        {
            if (timeOrderedNodes == null)
            { throw new InvalidOperationException("Cannot set Time-Ordering of Nodes to null."); }

            Dictionary<int, ModelObjectReference> timeOrderedNodesDict = new Dictionary<int, ModelObjectReference>();
            for (int i = 0; i < timeOrderedNodes.Count(); i++)
            {
                timeOrderedNodesDict.Add(i, timeOrderedNodes.ElementAt(i));
            }

            SetTimeOrderedNodes(hasStrictTimeOrdering, timeOrderedNodesDict, nodeLags);
        }

        public void SetTimeOrderedNodes(bool hasStrictTimeOrdering, IDictionary<int, ModelObjectReference> timeOrderedNodes, IDictionary<ModelObjectReference, TimeLagSet> nodeLags)
        {
            if (timeOrderedNodes == null)
            { throw new InvalidOperationException("Cannot set Time-Ordering of Nodes to null."); }
            if (m_HasStrictTimeOrdering.HasValue)
            { throw new InvalidOperationException("The Time-Ordering of Nodes can only be set once."); }

            m_HasStrictTimeOrdering = hasStrictTimeOrdering;
            m_TimeOrderedNodes = new SortedDictionary<int, ModelObjectReference>(timeOrderedNodes);
            m_NodeLags = new Dictionary<ModelObjectReference, TimeLagSet>(nodeLags);

            foreach (var node in NodesIncluded)
            {
                if (m_NodeLags.ContainsKey(node))
                { continue; }

                m_NodeLags.Add(node, new TimeLagSet());
            }
        }

        public void IncrementComputeCount()
        {
            if (!IsProcessingInfoSet)
            { throw new InvalidOperationException("The current ComputeGroup's processing info has not yet been set."); }
            if (m_ComputeCount == ushort.MaxValue)
            { throw new InvalidOperationException("The maximum number of Computations has been exceeded."); }

            m_ComputeCount++;
        }

        public bool Contains(ModelObjectReference reference)
        {
            return Contains(new ModelObjectReference[] { reference });
        }

        public bool Contains(IEnumerable<ModelObjectReference> references)
        {
            var nodesIncluded = NodesIncluded;

            foreach (var reference in references)
            {
                if (!nodesIncluded.Contains(reference))
                { return false; }
            }
            return true;
        }
    }
}