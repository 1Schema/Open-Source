using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.DataStructures
{
    public class CachedTimeNetwork<T> : ITimeNetwork<T>
        where T : struct, IComparable
    {
        public const bool DefaultIgnoreSelfCyles = CachedNetwork<T>.DefaultIgnoreSelfCyles;
        public const bool DefaultTreatSiblingsAsRelatives = CachedNetwork<T>.DefaultTreatSiblingsAsRelatives;

        public static ITimeNode<T> ConvertNodeToTimeNode(TimePeriod period, T node)
        { return new TimeNode<T>(period, node); }

        #region Private Members

        private TimeNodeEqualityComparer<T> m_NodeComparer;
        private TimeEdgeEqualityComparer<T> m_EdgeComparer;
        private IOrderManager<T> m_OrderManager;
        private ReadOnlyDictionary<TimePeriod, IOrderManager<T>> m_PeriodSpecificOrderManagers;
        private bool m_IsEditLocked;

        private CachedNetwork<T> m_ForeverNetwork;
        private ReadOnlyHashSet<TimePeriod> m_TimePeriods;
        private ReadOnlyHashSet<TimePeriod> m_TimePeriodsPlusForever;

        private ReadOnlyDictionary<TimePeriod, ReadOnlyHashSet<T>> m_PeriodSpecificNodes;
        private ReadOnlyDictionary<TimePeriod, ReadOnlyHashSet<IEdge<T>>> m_PeriodSpecificEdges;

        private ReadOnlyDictionary<TimePeriod, ReadOnlyDictionary<T, ReadOnlyHashSet<T>>> m_PeriodSpecificOutgoingEdges;
        private ReadOnlyDictionary<TimePeriod, ReadOnlyDictionary<T, ReadOnlyHashSet<T>>> m_PeriodSpecificIncomingEdges;
        private ReadOnlyDictionary<TimePeriod, ReadOnlyHashSet<T>> m_AllStartNodes;
        private ReadOnlyDictionary<TimePeriod, ReadOnlyHashSet<T>> m_AllEndNodes;

        private Dictionary<TimePeriod, Dictionary<T, object>> m_PeriodSpecificCachedValues;

        #endregion

        #region Constructors

        public CachedTimeNetwork()
            : this(null)
        { }

        public CachedTimeNetwork(IEqualityComparer<T> basicNodeComparer)
            : this(new List<T>(), new List<Edge<T>>(), basicNodeComparer)
        { }

        public CachedTimeNetwork(IEnumerable<T> foreverNodes, IEnumerable<Edge<T>> foreverEdges)
            : this(foreverNodes, foreverEdges, foreverNodes.GetComparer())
        { }

        public CachedTimeNetwork(IEnumerable<T> foreverNodes, IEnumerable<Edge<T>> foreverEdges, IEqualityComparer<T> basicNodeComparer)
            : this(foreverNodes, foreverEdges.Select(e => (IEdge<T>)e).ToList(), basicNodeComparer)
        { }

        public CachedTimeNetwork(IEnumerable<T> foreverNodes, IEnumerable<IEdge<T>> foreverEdges)
            : this(foreverNodes, foreverEdges, foreverNodes.GetComparer())
        { }

        public CachedTimeNetwork(IEnumerable<T> foreverNodes, IEnumerable<IEdge<T>> foreverEdges, IEqualityComparer<T> basicNodeComparer)
        {
            try
            {
                m_NodeComparer = new TimeNodeEqualityComparer<T>(basicNodeComparer);
                m_EdgeComparer = new TimeEdgeEqualityComparer<T>(basicNodeComparer);
                m_OrderManager = new NoOpOrderManager<T>();
                m_PeriodSpecificOrderManagers = new ReadOnlyDictionary<TimePeriod, IOrderManager<T>>();
                m_IsEditLocked = false;

                m_ForeverNetwork = new CachedNetwork<T>(foreverNodes, foreverEdges);
                m_TimePeriods = new ReadOnlyHashSet<TimePeriod>();
                m_TimePeriodsPlusForever = new ReadOnlyHashSet<TimePeriod>();
                m_TimePeriodsPlusForever.Add(TimePeriod.ForeverPeriod);

                m_PeriodSpecificNodes = new ReadOnlyDictionary<TimePeriod, ReadOnlyHashSet<T>>();
                m_PeriodSpecificEdges = new ReadOnlyDictionary<TimePeriod, ReadOnlyHashSet<IEdge<T>>>();

                m_PeriodSpecificOutgoingEdges = new ReadOnlyDictionary<TimePeriod, ReadOnlyDictionary<T, ReadOnlyHashSet<T>>>();
                m_PeriodSpecificIncomingEdges = new ReadOnlyDictionary<TimePeriod, ReadOnlyDictionary<T, ReadOnlyHashSet<T>>>();
                m_AllStartNodes = new ReadOnlyDictionary<TimePeriod, ReadOnlyHashSet<T>>();
                m_AllEndNodes = new ReadOnlyDictionary<TimePeriod, ReadOnlyHashSet<T>>();

                m_PeriodSpecificCachedValues = new Dictionary<TimePeriod, Dictionary<T, object>>();
            }
            catch (Exception e)
            { throw e; }
            finally
            { SetReadOnly(true); }
        }

        #endregion

        #region Methods for Adding State

        public void AddValues(TimePeriod period, IEnumerable<T> nodesToAdd, IEnumerable<Edge<T>> edgesToAdd)
        {
            AddValues(period, nodesToAdd, edgesToAdd.Select(e => (IEdge<T>)e).ToList());
        }

        public void AddValues(TimePeriod period, IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd)
        {
            UpdateValues(period, nodesToAdd, edgesToAdd, new List<T>(), new List<IEdge<T>>());
        }

        public void AddValues(IEnumerable<TimeNode<T>> nodesToAdd, IEnumerable<TimeEdge<T>> edgesToAdd)
        {
            IEnumerable<ITimeNode<T>> iNodesToAdd = nodesToAdd.Select(n => (ITimeNode<T>)n).ToList();
            IEnumerable<ITimeEdge<T>> iEdgesToAdd = edgesToAdd.Select(e => (ITimeEdge<T>)e).ToList();
            AddValues(iNodesToAdd, iEdgesToAdd);
        }

        public void AddValues(IEnumerable<ITimeNode<T>> nodesToAdd, IEnumerable<ITimeEdge<T>> edgesToAdd)
        {
            UpdateValues(nodesToAdd, edgesToAdd, new List<ITimeNode<T>>(), new List<ITimeEdge<T>>());
        }

        public void UpdateValues(TimePeriod period, IEnumerable<T> nodesToAdd, IEnumerable<Edge<T>> edgesToAdd, IEnumerable<T> nodesToRemove, IEnumerable<Edge<T>> edgesToRemove)
        {
            IEnumerable<IEdge<T>> iEdgesToAdd = edgesToAdd.Select(e => (IEdge<T>)e).ToList();
            IEnumerable<IEdge<T>> iEdgesToRemove = edgesToRemove.Select(e => (IEdge<T>)e).ToList();
            UpdateValues(period, nodesToAdd, iEdgesToAdd, nodesToRemove, iEdgesToRemove);
        }

        public void UpdateValues(TimePeriod period, IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd, IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove)
        {
            UpdateValuesForSinglePeriod(period, nodesToAdd, edgesToAdd, nodesToRemove, edgesToRemove);
        }

        public void UpdateValues(IEnumerable<TimeNode<T>> nodesToAdd, IEnumerable<TimeEdge<T>> edgesToAdd, IEnumerable<TimeNode<T>> nodesToRemove, IEnumerable<TimeEdge<T>> edgesToRemove)
        {
            IEnumerable<ITimeNode<T>> iNodesToAdd = nodesToAdd.Select(n => (ITimeNode<T>)n).ToList();
            IEnumerable<ITimeEdge<T>> iEdgesToAdd = edgesToAdd.Select(e => (ITimeEdge<T>)e).ToList();
            IEnumerable<ITimeNode<T>> iNodesToRemove = nodesToRemove.Select(n => (ITimeNode<T>)n).ToList();
            IEnumerable<ITimeEdge<T>> iEdgesToRemove = edgesToRemove.Select(e => (ITimeEdge<T>)e).ToList();
            UpdateValues(iNodesToAdd, iEdgesToAdd, iNodesToRemove, iEdgesToRemove);
        }

        public void UpdateValues(IEnumerable<ITimeNode<T>> nodesToAdd, IEnumerable<ITimeEdge<T>> edgesToAdd, IEnumerable<ITimeNode<T>> nodesToRemove, IEnumerable<ITimeEdge<T>> edgesToRemove)
        {
            List<TimePeriod> nodePeriodsToAdd = nodesToAdd.Select(n => n.Period).Distinct().ToList();
            List<TimePeriod> edgePeriodsToAdd = edgesToAdd.Select(e => e.Period).Distinct().ToList();
            List<TimePeriod> nodePeriodsToRemove = nodesToRemove.Select(n => n.Period).Distinct().ToList();
            List<TimePeriod> edgePeriodsToRemove = edgesToRemove.Select(e => e.Period).Distinct().ToList();

            List<TimePeriod> allPeriods = new List<TimePeriod>();
            allPeriods.AddRange(nodePeriodsToAdd);
            allPeriods.AddRange(edgePeriodsToAdd);
            allPeriods.AddRange(nodePeriodsToRemove);
            allPeriods.AddRange(edgePeriodsToRemove);
            HashSet<TimePeriod> uniquePeriods = new HashSet<TimePeriod>(allPeriods);

            foreach (TimePeriod timePeriod in uniquePeriods)
            {
                IEnumerable<T> periodNodesToAdd = nodesToAdd.Where(n => n.Period == timePeriod).Select(n => n.Node).ToList();
                IEnumerable<IEdge<T>> periodEdgesToAdd = edgesToAdd.Where(e => e.Period == timePeriod).Select(e => e.Edge).ToList();
                IEnumerable<T> periodNodesToRemove = nodesToRemove.Where(n => n.Period == timePeriod).Select(n => n.Node).ToList();
                IEnumerable<IEdge<T>> periodEdgesToRemove = edgesToRemove.Where(e => e.Period == timePeriod).Select(e => e.Edge).ToList();

                UpdateValuesForSinglePeriod(timePeriod, periodNodesToAdd, periodEdgesToAdd, periodNodesToRemove, periodEdgesToRemove);
            }
        }

        protected void UpdateValuesForSinglePeriod(TimePeriod period, IEnumerable<T> nodesToAdd, IEnumerable<IEdge<T>> edgesToAdd, IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove)
        {
            try
            {
                SetReadOnly(false);

                if (period.IsForever)
                {
                    m_ForeverNetwork.UpdateValues(nodesToAdd, edgesToAdd, nodesToRemove, edgesToRemove);
                    return;
                }

                nodesToAdd = new HashSet<T>(nodesToAdd, BasicNodeComparer);
                edgesToAdd = new HashSet<IEdge<T>>(edgesToAdd, BasicEdgeComparer);
                nodesToRemove = new HashSet<T>(nodesToRemove, BasicNodeComparer);
                edgesToRemove = new HashSet<IEdge<T>>(edgesToRemove, BasicEdgeComparer);

                List<T> foreverNodes = ForeverNetwork.Nodes.ToList();
                List<IEdge<T>> foreverEdges = ForeverNetwork.Edges.ToList();
                List<T> periodNodes = m_TimePeriods.Contains(period) ? m_PeriodSpecificNodes[period].ToList() : new List<T>();
                List<IEdge<T>> periodEdges = m_TimePeriods.Contains(period) ? m_PeriodSpecificEdges[period].ToList() : new List<IEdge<T>>();

                List<T> allNodes = periodNodes.Union(nodesToAdd).ToList();
                allNodes = allNodes.Where(n => !nodesToRemove.Contains(n)).ToList();
                allNodes = allNodes.Union(foreverNodes).ToList();
                List<IEdge<T>> allEdges = periodEdges.Union(edgesToAdd).ToList();
                allEdges = allEdges.Where(e => (!edgesToRemove.Contains(e)) && (allNodes.Contains(e.IncomingNode) && (allNodes.Contains(e.OutgoingNode)))).ToList();

                foreach (IEdge<T> edge in allEdges)
                {
                    T outgoingNode = edge.OutgoingNode;
                    T incomingNode = edge.IncomingNode;

                    if (!allNodes.Contains(outgoingNode))
                    { throw new InvalidOperationException("The outgoing Node does not exist in the list of Nodes."); }
                    if (!allNodes.Contains(incomingNode))
                    { throw new InvalidOperationException("The incoming Node does not exist in the list of Nodes."); }
                }

                if (!m_TimePeriods.Contains(period))
                {
                    m_TimePeriods.Add(period);
                    m_TimePeriodsPlusForever.Add(period);

                    m_PeriodSpecificNodes.Add(period, new ReadOnlyHashSet<T>(BasicNodeComparer));
                    m_PeriodSpecificEdges.Add(period, new ReadOnlyHashSet<IEdge<T>>(BasicEdgeComparer));

                    m_PeriodSpecificOutgoingEdges.Add(period, new ReadOnlyDictionary<T, ReadOnlyHashSet<T>>(BasicNodeComparer));
                    m_PeriodSpecificIncomingEdges.Add(period, new ReadOnlyDictionary<T, ReadOnlyHashSet<T>>(BasicNodeComparer));
                    m_AllStartNodes.Add(period, new ReadOnlyHashSet<T>(BasicNodeComparer));
                    m_AllEndNodes.Add(period, new ReadOnlyHashSet<T>(BasicNodeComparer));

                    m_PeriodSpecificCachedValues.Add(period, new Dictionary<T, object>(BasicNodeComparer));
                }
                else
                {
                    m_AllStartNodes[period].Clear();
                    m_AllEndNodes[period].Clear();
                }

                foreach (T node in nodesToAdd)
                {
                    if (m_PeriodSpecificNodes[period].Contains(node))
                    { continue; }

                    m_PeriodSpecificNodes[period].Add(node);

                    m_PeriodSpecificOutgoingEdges[period].Add(node, new ReadOnlyHashSet<T>(BasicNodeComparer));
                    m_PeriodSpecificIncomingEdges[period].Add(node, new ReadOnlyHashSet<T>(BasicNodeComparer));
                }
                foreach (T node in nodesToRemove)
                {
                    if (!m_PeriodSpecificNodes[period].Contains(node))
                    { continue; }

                    m_PeriodSpecificNodes[period].Remove(node);

                    m_PeriodSpecificOutgoingEdges[period].Remove(node);
                    m_PeriodSpecificIncomingEdges[period].Remove(node);

                    if (m_PeriodSpecificCachedValues[period].ContainsKey(node))
                    { m_PeriodSpecificCachedValues[period].Remove(node); }
                }

                foreach (IEdge<T> edge in edgesToAdd)
                {
                    if (m_PeriodSpecificEdges[period].Contains(edge))
                    { continue; }

                    m_PeriodSpecificEdges[period].Add(edge);

                    T outgoingNode = edge.OutgoingNode;
                    T incomingNode = edge.IncomingNode;

                    if (!m_PeriodSpecificOutgoingEdges[period].ContainsKey(outgoingNode))
                    { throw new InvalidOperationException("Currently, all Outgoing Nodes used in TimeEdges must also be added for the Period."); }
                    if (!m_PeriodSpecificIncomingEdges[period].ContainsKey(incomingNode))
                    { throw new InvalidOperationException("Currently, all Incoming Nodes used in TimeEdges must also be added for the Period."); }

                    m_PeriodSpecificOutgoingEdges[period][outgoingNode].Add(incomingNode);
                    m_PeriodSpecificIncomingEdges[period][incomingNode].Add(outgoingNode);
                }
                foreach (IEdge<T> edge in edgesToRemove)
                {
                    if (!m_PeriodSpecificEdges[period].Contains(edge))
                    { continue; }

                    m_PeriodSpecificEdges[period].Remove(edge);

                    T outgoingNode = edge.OutgoingNode;
                    T incomingNode = edge.IncomingNode;

                    m_PeriodSpecificOutgoingEdges[period][outgoingNode].Remove(incomingNode);
                    m_PeriodSpecificIncomingEdges[period][incomingNode].Remove(outgoingNode);
                }

                IDictionary<T, ICollection<T>> allOutgoingEdgesForPeriod = GetAllOutgoingEdgesForPeriod(period);
                IDictionary<T, ICollection<T>> allIncomingEdgesForPeriod = GetAllIncomingEdgesForPeriod(period);

                foreach (var outgoingNodeBucket in allOutgoingEdgesForPeriod)
                {
                    if (outgoingNodeBucket.Value.Count <= 0)
                    {
                        m_AllEndNodes[period].Add(outgoingNodeBucket.Key);
                    }
                }

                foreach (var incomingNodeBucket in allIncomingEdgesForPeriod)
                {
                    if (incomingNodeBucket.Value.Count <= 0)
                    {
                        m_AllStartNodes[period].Add(incomingNodeBucket.Key);
                    }
                }
            }
            catch (Exception e)
            { throw e; }
            finally
            { SetReadOnly(true); }
        }

        public void RemoveValues(TimePeriod period, IEnumerable<T> nodesToRemove, IEnumerable<Edge<T>> edgesToRemove)
        {
            RemoveValues(period, nodesToRemove, edgesToRemove.Select(e => (IEdge<T>)e).ToList());
        }

        public void RemoveValues(TimePeriod period, IEnumerable<T> nodesToRemove, IEnumerable<IEdge<T>> edgesToRemove)
        {
            UpdateValues(period, new List<T>(), new List<IEdge<T>>(), nodesToRemove, edgesToRemove);
        }

        public void RemoveValues(IEnumerable<TimeNode<T>> nodesToRemove, IEnumerable<TimeEdge<T>> edgesToRemove)
        {
            IEnumerable<ITimeNode<T>> iNodesToRemove = nodesToRemove.Select(n => (ITimeNode<T>)n).ToList();
            IEnumerable<ITimeEdge<T>> iEdgesToRemove = edgesToRemove.Select(e => (ITimeEdge<T>)e).ToList();
            RemoveValues(iNodesToRemove, iEdgesToRemove);
        }

        public void RemoveValues(IEnumerable<ITimeNode<T>> nodesToRemove, IEnumerable<ITimeEdge<T>> edgesToRemove)
        {
            UpdateValues(new List<ITimeNode<T>>(), new List<ITimeEdge<T>>(), nodesToRemove, edgesToRemove);
        }

        #endregion

        protected void SetReadOnly(bool readOnly)
        {
            m_IsEditLocked = readOnly;

            m_TimePeriods.IsReadOnly = readOnly;
            m_TimePeriodsPlusForever.IsReadOnly = readOnly;
            m_PeriodSpecificNodes.IsReadOnly = readOnly;
            m_PeriodSpecificEdges.IsReadOnly = readOnly;
            m_PeriodSpecificOutgoingEdges.IsReadOnly = readOnly;
            m_PeriodSpecificIncomingEdges.IsReadOnly = readOnly;
            m_AllStartNodes.IsReadOnly = readOnly;
            m_AllEndNodes.IsReadOnly = readOnly;

            foreach (var value in m_PeriodSpecificNodes.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;
            }
            foreach (var value in m_PeriodSpecificEdges.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;
            }
            foreach (var value in m_PeriodSpecificOutgoingEdges.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;

                foreach (var nestedValue in value.Values)
                {
                    if (nestedValue == null)
                    { continue; }
                    nestedValue.IsReadOnly = readOnly;
                }
            }
            foreach (var value in m_PeriodSpecificIncomingEdges.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;

                foreach (var nestedValue in value.Values)
                {
                    if (nestedValue == null)
                    { continue; }
                    nestedValue.IsReadOnly = readOnly;
                }
            }
            foreach (var value in m_AllStartNodes.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;
            }
            foreach (var value in m_AllEndNodes.Values)
            {
                if (value == null)
                { continue; }
                value.IsReadOnly = readOnly;
            }
        }

        #region Properties and Basic Getter Methods

        public IEqualityComparer<ITimeNode<T>> NodeComparer
        {
            get { return m_NodeComparer; }
        }

        public IEqualityComparer<ITimeEdge<T>> EdgeComparer
        {
            get { return m_EdgeComparer; }
        }

        public IEqualityComparer<T> BasicNodeComparer
        {
            get { return m_ForeverNetwork.NodeComparer; }
        }

        public IEqualityComparer<IEdge<T>> BasicEdgeComparer
        {
            get { return m_ForeverNetwork.EdgeComparer; }
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

        public IOrderManager<T> GetPeriodSpecificOrderManager(TimePeriod desiredPeriod)
        {
            IOrderManager<T> periodOrderManager = null;
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);

            if (!m_PeriodSpecificOrderManagers.TryGetValue(desiredPeriod, out periodOrderManager))
            { return m_OrderManager; }
            return periodOrderManager;
        }

        public void SetPeriodSpecificOrderManager(TimePeriod desiredPeriod, IOrderManager<T> orderManager)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            if (m_PeriodSpecificOrderManagers.ContainsKey(desiredPeriod) && (orderManager == null))
            {
                m_PeriodSpecificOrderManagers.Remove(desiredPeriod);
            }
            else
            {
                m_PeriodSpecificOrderManagers[desiredPeriod] = orderManager;
            }
        }

        public bool IsEditLocked
        {
            get { return m_IsEditLocked; }
        }

        public INetwork<T> ForeverNetwork
        {
            get { return m_ForeverNetwork; }
        }

        public ICollection<TimePeriod> TimePeriods
        {
            get { return m_TimePeriods; }
        }

        public ICollection<TimePeriod> TimePeriodsPlusForever
        {
            get { return m_TimePeriodsPlusForever; }
        }

        public ICollection<TimePeriod> GetRelevantPeriods(TimePeriod desiredPeriod)
        {
            if (desiredPeriod.IsForever)
            { return new TimePeriod[] { TimePeriod.ForeverPeriod }; }

            ICollection<TimePeriod> relevantPeriods = m_TimePeriods.GetRelevantOrContainedPeriods(desiredPeriod);
            return relevantPeriods;
        }

        public TimePeriod ConvertDesiredPeriodToAssessedPeriod(TimePeriod desiredPeriod)
        {
            if (desiredPeriod.HasBeenConverted)
            { return desiredPeriod; }

            ICollection<TimePeriod> timePeriods = GetRelevantPeriods(desiredPeriod);

            if (timePeriods.Count < 1)
            { throw new InvalidOperationException("TimeNetwork could not match the requested TimePeriod."); }
            if (timePeriods.Count > 1)
            { throw new InvalidOperationException("TimeNetwork currently does not support results for multiple TimePeriods."); }

            var currentPeriod = new TimePeriod(timePeriods.First(), true);
            return currentPeriod;
        }

        public IDictionary<TimePeriod, ICollection<T>> AllNodes
        {
            get { return TimePeriodsPlusForever.ToDictionary(tp => tp, tp => GetAllNodesForPeriod(tp)); }
        }

        public IDictionary<TimePeriod, ICollection<IEdge<T>>> AllEdges
        {
            get { return TimePeriodsPlusForever.ToDictionary(tp => tp, tp => GetAllEdgesForPeriod(tp)); }
        }

        public IDictionary<TimePeriod, ICollection<T>> AllStartNodes
        {
            get { return TimePeriodsPlusForever.ToDictionary(tp => tp, tp => GetAllStartNodesForPeriod(tp)); }
        }

        public IDictionary<TimePeriod, ICollection<T>> AllEndNodes
        {
            get { return TimePeriodsPlusForever.ToDictionary(tp => tp, tp => GetAllEndNodesForPeriod(tp)); }
        }

        public ICollection<T> GetPeriodSpecificNodesForPeriod(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);
            return orderManager.ToOrderedSet(m_PeriodSpecificNodes[desiredPeriod], BasicNodeComparer);
        }

        public ICollection<T> GetForeverNodesForPeriod(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);
            return orderManager.ToOrderedSet(m_ForeverNetwork.Nodes_INTERNAL, BasicNodeComparer);
        }

        public ICollection<T> GetAllNodesForPeriod(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);
            ICollection<T> foreverNodes = GetForeverNodesForPeriod(desiredPeriod);

            if (desiredPeriod.IsForever)
            { return orderManager.ToOrderedSet(foreverNodes, BasicNodeComparer); }

            return orderManager.ToOrderedSet(m_PeriodSpecificNodes[desiredPeriod].Union(foreverNodes), BasicNodeComparer);
        }

        public ICollection<IEdge<T>> GetPeriodSpecificEdgesForPeriod(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            return m_PeriodSpecificEdges[desiredPeriod].ToHashSet(BasicEdgeComparer);
        }

        public ICollection<IEdge<T>> GetForeverEdgesForPeriod(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            return m_ForeverNetwork.Edges_INTERNAL.ToHashSet(BasicEdgeComparer);
        }

        public ICollection<IEdge<T>> GetAllEdgesForPeriod(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            ICollection<IEdge<T>> foreverEdges = GetForeverEdgesForPeriod(desiredPeriod);

            if (desiredPeriod.IsForever)
            { return foreverEdges; }

            return m_PeriodSpecificEdges[desiredPeriod].Union(foreverEdges).ToHashSet(BasicEdgeComparer);
        }

        public IDictionary<T, ICollection<T>> GetPeriodSpecificOutgoingEdgesForPeriod(TimePeriod desiredPeriod)
        {
            return GetPeriodSpecificOutgoingEdgesForPeriod(desiredPeriod, null);
        }

        public ICollection<T> GetPeriodSpecificOutgoingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode)
        {
            return GetPeriodSpecificOutgoingEdgesForPeriod(desiredPeriod, new T[] { relevantNode })[relevantNode];
        }

        public IDictionary<T, ICollection<T>> GetPeriodSpecificOutgoingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            if (desiredPeriod.IsForever)
            { return new Dictionary<T, ICollection<T>>(BasicNodeComparer); }

            var relevantEdges = m_PeriodSpecificOutgoingEdges[desiredPeriod];
            relevantNodes = (relevantNodes != null) ? relevantNodes : (ICollection<T>)relevantEdges.Keys;
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            var unorderedResults = new Dictionary<T, ICollection<T>>();
            foreach (var relevantNode in relevantNodes)
            {
                ReadOnlyHashSet<T> relevantValue;
                if (!relevantEdges.TryGetValue(relevantNode, out relevantValue))
                { continue; }

                unorderedResults.Add(relevantNode, (ICollection<T>)orderManager.ToOrderedSet(relevantValue, BasicNodeComparer));
            }
            var orderedResults = orderManager.ToOrderedDictionary(unorderedResults, BasicNodeComparer);
            return orderedResults;
        }

        public IDictionary<T, ICollection<T>> GetForeverOutgoingEdgesForPeriod(TimePeriod desiredPeriod)
        {
            return GetForeverOutgoingEdgesForPeriod(desiredPeriod, null);
        }

        public ICollection<T> GetForeverOutgoingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode)
        {
            return GetForeverOutgoingEdgesForPeriod(desiredPeriod, new T[] { relevantNode })[relevantNode];
        }

        public IDictionary<T, ICollection<T>> GetForeverOutgoingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var relevantEdges = m_ForeverNetwork.OutgoingEdges_INTERNAL;
            relevantNodes = (relevantNodes != null) ? relevantNodes : (ICollection<T>)relevantEdges.Keys;
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            var unorderedResults = new Dictionary<T, ICollection<T>>();
            foreach (var relevantNode in relevantNodes)
            {
                ReadOnlyHashSet<T> relevantValue;
                if (!relevantEdges.TryGetValue(relevantNode, out relevantValue))
                { continue; }

                unorderedResults.Add(relevantNode, (ICollection<T>)orderManager.ToOrderedSet(relevantValue, BasicNodeComparer));
            }
            var orderedResults = orderManager.ToOrderedDictionary(unorderedResults, BasicNodeComparer);
            return orderedResults;
        }

        public IDictionary<T, ICollection<T>> GetAllOutgoingEdgesForPeriod(TimePeriod desiredPeriod)
        {
            return GetAllOutgoingEdgesForPeriod(desiredPeriod, null);
        }

        public ICollection<T> GetAllOutgoingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode)
        {
            return GetAllOutgoingEdgesForPeriod(desiredPeriod, new T[] { relevantNode })[relevantNode];
        }

        public IDictionary<T, ICollection<T>> GetAllOutgoingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            IDictionary<T, ICollection<T>> periodSpecificOutgoingEdges = GetPeriodSpecificOutgoingEdgesForPeriod(desiredPeriod, relevantNodes);
            IDictionary<T, ICollection<T>> foreverOutgoingEdges = GetForeverOutgoingEdgesForPeriod(desiredPeriod, relevantNodes);

            Dictionary<T, ICollection<T>> mergedOutgoingEdges = new Dictionary<T, ICollection<T>>(periodSpecificOutgoingEdges, BasicNodeComparer);
            foreach (var nodeBucket in foreverOutgoingEdges)
            {
                ICollection<T> outgoingNodes;

                if (!mergedOutgoingEdges.TryGetValue(nodeBucket.Key, out outgoingNodes))
                {
                    mergedOutgoingEdges.Add(nodeBucket.Key, nodeBucket.Value);
                }
                else
                {
                    var values = outgoingNodes.ToHashSet(BasicNodeComparer);
                    values = values.Union(nodeBucket.Value).ToHashSet(BasicNodeComparer);
                    mergedOutgoingEdges[nodeBucket.Key] = orderManager.ToOrderedSet(values, BasicNodeComparer);
                }
            }
            return mergedOutgoingEdges;
        }

        public IDictionary<T, ICollection<T>> GetPeriodSpecificIncomingEdgesForPeriod(TimePeriod desiredPeriod)
        {
            return GetPeriodSpecificIncomingEdgesForPeriod(desiredPeriod, null);
        }

        public ICollection<T> GetPeriodSpecificIncomingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode)
        {
            var edges = GetPeriodSpecificIncomingEdgesForPeriod(desiredPeriod, new T[] { relevantNode });

            if ((edges == null) || (edges.Count < 1))
            { return new List<T>(); }

            return edges[relevantNode];
        }

        public IDictionary<T, ICollection<T>> GetPeriodSpecificIncomingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            if (desiredPeriod.IsForever)
            { return new Dictionary<T, ICollection<T>>(BasicNodeComparer); }

            var relevantEdges = m_PeriodSpecificIncomingEdges[desiredPeriod];
            relevantNodes = (relevantNodes != null) ? relevantNodes : (ICollection<T>)relevantEdges.Keys;
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            var unorderedResults = new Dictionary<T, ICollection<T>>();
            foreach (var relevantNode in relevantNodes)
            {
                ReadOnlyHashSet<T> relevantValue;
                if (!relevantEdges.TryGetValue(relevantNode, out relevantValue))
                { continue; }

                unorderedResults.Add(relevantNode, (ICollection<T>)orderManager.ToOrderedSet(relevantValue, BasicNodeComparer));
            }
            var orderedResults = orderManager.ToOrderedDictionary(unorderedResults, BasicNodeComparer);
            return orderedResults;
        }

        public IDictionary<T, ICollection<T>> GetForeverIncomingEdgesForPeriod(TimePeriod desiredPeriod)
        {
            return GetForeverIncomingEdgesForPeriod(desiredPeriod, null);
        }

        public ICollection<T> GetForeverIncomingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode)
        {
            return GetForeverIncomingEdgesForPeriod(desiredPeriod, new T[] { relevantNode })[relevantNode];
        }

        public IDictionary<T, ICollection<T>> GetForeverIncomingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var relevantEdges = m_ForeverNetwork.IncomingEdges_INTERNAL;
            relevantNodes = (relevantNodes != null) ? relevantNodes : (ICollection<T>)relevantEdges.Keys;
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            var unorderedResults = new Dictionary<T, ICollection<T>>();
            foreach (var relevantNode in relevantNodes)
            {
                ReadOnlyHashSet<T> relevantValue;
                if (!relevantEdges.TryGetValue(relevantNode, out relevantValue))
                { continue; }

                unorderedResults.Add(relevantNode, (ICollection<T>)orderManager.ToOrderedSet(relevantValue, BasicNodeComparer));
            }
            var orderedResults = orderManager.ToOrderedDictionary(unorderedResults, BasicNodeComparer);
            return orderedResults;
        }

        public IDictionary<T, ICollection<T>> GetAllIncomingEdgesForPeriod(TimePeriod desiredPeriod)
        {
            return GetAllIncomingEdgesForPeriod(desiredPeriod, null);
        }

        public ICollection<T> GetAllIncomingEdgesForPeriod(TimePeriod desiredPeriod, T relevantNode)
        {
            return GetAllIncomingEdgesForPeriod(desiredPeriod, new T[] { relevantNode })[relevantNode];
        }

        public IDictionary<T, ICollection<T>> GetAllIncomingEdgesForPeriod(TimePeriod desiredPeriod, ICollection<T> relevantNodes)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            IDictionary<T, ICollection<T>> periodSpecificIncomingEdges = GetPeriodSpecificIncomingEdgesForPeriod(desiredPeriod, relevantNodes);
            IDictionary<T, ICollection<T>> foreverIncomingEdges = GetForeverIncomingEdgesForPeriod(desiredPeriod, relevantNodes);

            Dictionary<T, ICollection<T>> mergedIncomingEdges = new Dictionary<T, ICollection<T>>(periodSpecificIncomingEdges, BasicNodeComparer);
            foreach (var nodeBucket in foreverIncomingEdges)
            {
                ICollection<T> incomingNodes;

                if (!mergedIncomingEdges.TryGetValue(nodeBucket.Key, out incomingNodes))
                {
                    mergedIncomingEdges.Add(nodeBucket.Key, nodeBucket.Value);
                }
                else
                {
                    var values = incomingNodes.ToHashSet(BasicNodeComparer);
                    values = values.Union(nodeBucket.Value).ToHashSet(BasicNodeComparer);
                    mergedIncomingEdges[nodeBucket.Key] = orderManager.ToOrderedSet(values, BasicNodeComparer);
                }
            }
            return mergedIncomingEdges;
        }

        public ICollection<T> GetAllStartNodesForPeriod(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            if (desiredPeriod.IsForever)
            { return orderManager.ToOrderedSet(m_ForeverNetwork.StartNodes_INTERNAL, BasicNodeComparer); }

            return orderManager.ToOrderedSet(m_AllStartNodes[desiredPeriod], BasicNodeComparer);
        }

        public ICollection<T> GetAllEndNodesForPeriod(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            if (desiredPeriod.IsForever)
            { return orderManager.ToOrderedSet(m_ForeverNetwork.EndNodes_INTERNAL, BasicNodeComparer); }

            return orderManager.ToOrderedSet(m_AllEndNodes[desiredPeriod], BasicNodeComparer);
        }

        public bool Contains(T node)
        {
            return m_ForeverNetwork.Contains(node);
        }

        public bool Contains(TimePeriod desiredPeriod, T node)
        {
            return GetAllNodesForPeriod(desiredPeriod).Contains(node);
        }

        #endregion

        #region HasCycles Methods

        public bool HasCycles(TimePeriod desiredPeriod)
        {
            IEnumerable<T> nodesInCycles;
            return HasCycles(desiredPeriod, DefaultIgnoreSelfCyles, out nodesInCycles);
        }

        public bool HasCycles(TimePeriod desiredPeriod, bool ignoreSelfCyles)
        {
            IEnumerable<T> nodesInCycles;
            return HasCycles(desiredPeriod, ignoreSelfCyles, out nodesInCycles);
        }

        public bool HasCycles(TimePeriod desiredPeriod, out IEnumerable<T> nodesInCycles)
        {
            return HasCycles(desiredPeriod, DefaultIgnoreSelfCyles, out nodesInCycles);
        }

        public bool HasCycles(TimePeriod desiredPeriod, bool ignoreSelfCyles, out IEnumerable<T> nodesInCycles)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var remainingNodes = GetAllNodesForPeriod(desiredPeriod).ToHashSet(BasicNodeComparer);
            var remainingEdges = GetAllEdgesForPeriod(desiredPeriod).ToHashSet(BasicEdgeComparer);

            return AlgortihmUtils.HasCycles<T>(remainingNodes, remainingEdges, ignoreSelfCyles, out nodesInCycles);
        }

        #endregion

        public ICollection<T> GetParents(TimePeriod desiredPeriod, T node)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            ICollection<T> allNodesForPeriod = GetAllNodesForPeriod(desiredPeriod);

            if (!allNodesForPeriod.Contains(node))
            { return new HashSet<T>(BasicNodeComparer); }

            var incomingEdges = GetAllIncomingEdgesForPeriod(desiredPeriod, node).ToHashSet(BasicNodeComparer);
            return incomingEdges;
        }

        public ICollection<T> GetAncestors(TimePeriod desiredPeriod, T node)
        {
            return GetAncestorsWithDistances(desiredPeriod, node).Keys.ToHashSet(BasicNodeComparer);
        }

        public IDictionary<T, int> GetAncestorsWithDistances(TimePeriod desiredPeriod, T node)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            ICollection<T> allNodesForPeriod = GetAllNodesForPeriod(desiredPeriod);

            if (!allNodesForPeriod.Contains(node))
            { return new Dictionary<T, int>(BasicNodeComparer); }

            Dictionary<T, int> result = new Dictionary<T, int>(BasicNodeComparer);
            ICollection<T> parents = GetParents(desiredPeriod, node);
            int distance = 1;

            while (parents.Count > 0)
            {
                var nextParents = new HashSet<T>(BasicNodeComparer);

                foreach (T parentNode in parents)
                {
                    if (result.ContainsKey(parentNode))
                    { continue; }

                    result.Add(parentNode, distance);
                    nextParents.AddRange(GetParents(desiredPeriod, parentNode));
                }

                parents = nextParents;
                distance++;
            }
            return result;
        }

        public ICollection<T> GetChildren(TimePeriod desiredPeriod, T node)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            ICollection<T> allNodesForPeriod = GetAllNodesForPeriod(desiredPeriod);

            if (!allNodesForPeriod.Contains(node))
            { return new HashSet<T>(BasicNodeComparer); }

            var outgoingEdges = GetAllOutgoingEdgesForPeriod(desiredPeriod, node).ToHashSet(BasicNodeComparer);
            return outgoingEdges;
        }

        public ICollection<T> GetDecendants(TimePeriod desiredPeriod, T node)
        {
            return GetDecendantsWithDistances(desiredPeriod, node).Keys.ToHashSet(BasicNodeComparer);
        }

        public IDictionary<T, int> GetDecendantsWithDistances(TimePeriod desiredPeriod, T node)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            ICollection<T> allNodesForPeriod = GetAllNodesForPeriod(desiredPeriod);

            if (!allNodesForPeriod.Contains(node))
            { return new Dictionary<T, int>(BasicNodeComparer); }

            Dictionary<T, int> result = new Dictionary<T, int>(BasicNodeComparer);
            ICollection<T> children = GetChildren(desiredPeriod, node);
            int distance = 1;

            while (children.Count > 0)
            {
                var nextChildren = new HashSet<T>(BasicNodeComparer);

                foreach (T childNode in children)
                {
                    if (result.ContainsKey(childNode))
                    { continue; }

                    result.Add(childNode, distance);
                    nextChildren.AddRange(GetChildren(desiredPeriod, childNode));
                }

                children = nextChildren;
                distance++;
            }
            return result;
        }

        public ICollection<T> GetSiblings(TimePeriod desiredPeriod, T node)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            var orderManager = GetPeriodSpecificOrderManager(desiredPeriod);

            ICollection<T> allNodesForPeriod = GetAllNodesForPeriod(desiredPeriod);

            if (!allNodesForPeriod.Contains(node))
            { return new HashSet<T>(BasicNodeComparer); }

            ICollection<T> parents = GetParents(desiredPeriod, node);
            ICollection<T> children = GetChildren(desiredPeriod, node);
            HashSet<T> siblings = new HashSet<T>(BasicNodeComparer);

            foreach (T loopNode in allNodesForPeriod)
            {
                if (loopNode.Equals(node))
                { continue; }

                ICollection<T> currentParents = GetParents(desiredPeriod, loopNode);
                ICollection<T> currentChildren = GetChildren(desiredPeriod, loopNode);

                IEnumerable<T> parentOverlap = currentParents.Intersect(parents);
                IEnumerable<T> childOverlap = currentChildren.Intersect(children);

                bool parentsPass = (parents.Count != 0) ? (parentOverlap.Count() > 0) : (currentParents.Count == 0);
                bool childrenPass = (children.Count != 0) ? (childOverlap.Count() > 0) : (currentChildren.Count == 0);

                if (parentsPass && childrenPass)
                { siblings.Add(loopNode); }
            }
            return orderManager.ToOrderedSet(siblings, BasicNodeComparer);
        }

        public ICollection<T> GetRelatives(TimePeriod desiredPeriod, T node)
        {
            return GetRelatives(desiredPeriod, node, DefaultTreatSiblingsAsRelatives);
        }

        public ICollection<T> GetRelatives(TimePeriod desiredPeriod, T node, bool treatSiblingsAsRelatives)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            ICollection<T> allNodesForPeriod = GetAllNodesForPeriod(desiredPeriod);

            ICollection<T> nonRelatives = GetNonRelatives(desiredPeriod, node, treatSiblingsAsRelatives);
            return allNodesForPeriod.Where(n => !nonRelatives.Contains(n)).ToHashSet(BasicNodeComparer);
        }

        public ICollection<T> GetNonRelatives(TimePeriod desiredPeriod, T node)
        {
            return GetNonRelatives(desiredPeriod, node, DefaultTreatSiblingsAsRelatives);
        }

        public ICollection<T> GetNonRelatives(TimePeriod desiredPeriod, T node, bool treatSiblingsAsRelatives)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            ICollection<T> allNodesForPeriod = GetAllNodesForPeriod(desiredPeriod);

            if (!allNodesForPeriod.Contains(node))
            { return new HashSet<T>(BasicNodeComparer); }

            IDictionary<T, int> ancestors = GetAncestorsWithDistances(desiredPeriod, node);
            IDictionary<T, int> descendants = GetDecendantsWithDistances(desiredPeriod, node);
            ICollection<T> siblings = new HashSet<T>(BasicNodeComparer);
            if (treatSiblingsAsRelatives)
            { siblings = GetSiblings(desiredPeriod, node); }

            HashSet<T> unrelated = new HashSet<T>(BasicNodeComparer);
            foreach (T otherNode in allNodesForPeriod)
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
            return unrelated;
        }

        public ICollection<T> GetDepthFirstTraversalFromRoot(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);

            ICollection<T> allStartNodesForPeriod = GetAllStartNodesForPeriod(desiredPeriod);
            Func<T, ICollection<T>> getChildrenMethod = (node => GetChildren(desiredPeriod, node));

            var result = AlgortihmUtils.GetDepthFirstTraversalFromRoot(allStartNodesForPeriod, getChildrenMethod);
            return result;
        }

        public ICollection<T> GetBreadthFirstTraversalFromRoot(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);

            ICollection<T> allStartNodesForPeriod = GetAllStartNodesForPeriod(desiredPeriod);
            Func<T, ICollection<T>> getChildrenMethod = (node => GetChildren(desiredPeriod, node));

            var result = AlgortihmUtils.GetBreadthFirstTraversalFromRoot(allStartNodesForPeriod, getChildrenMethod);
            return result;
        }

        public ICollection<T> GetDependencyOrderedTraversalFromRoot(TimePeriod desiredPeriod)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);

            ICollection<T> allStartNodesForPeriod = GetAllStartNodesForPeriod(desiredPeriod);
            Func<T, ICollection<T>> getChildrenMethod = (node => GetChildren(desiredPeriod, node));
            Func<T, ICollection<T>> getParentsMethod = (node => GetParents(desiredPeriod, node));

            var result = AlgortihmUtils.GetDependencyOrderedTraversalFromRoot(allStartNodesForPeriod, getChildrenMethod, getParentsMethod);
            return result;
        }

        public object GetCachedValue(TimePeriod desiredPeriod, T node)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            return m_PeriodSpecificCachedValues[desiredPeriod][node];
        }

        public CV GetCachedValue<CV>(TimePeriod desiredPeriod, T node)
        {
            object cachedValue = GetCachedValue(desiredPeriod, node);
            return (CV)cachedValue;
        }

        public void SetCachedValue(TimePeriod desiredPeriod, T node, object cachedValue)
        {
            desiredPeriod = ConvertDesiredPeriodToAssessedPeriod(desiredPeriod);
            m_PeriodSpecificCachedValues[desiredPeriod][node] = cachedValue;
        }

        public void SetCachedValue<CV>(TimePeriod desiredPeriod, T node, CV cachedValue)
        {
            object valueToCache = (object)cachedValue;
            SetCachedValue(desiredPeriod, node, valueToCache);
        }
    }
}