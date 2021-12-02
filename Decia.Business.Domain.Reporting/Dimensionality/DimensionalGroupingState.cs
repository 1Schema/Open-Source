using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using Decia.Business.Common.Computation;
using Decia.Business.Domain.Structure;

namespace Decia.Business.Domain.Reporting.Dimensionality
{
    public class DimensionalGroupingState
    {
        protected Dictionary<Guid, DimensionalRepeatGroup> m_RepeatGroups;
        protected Nullable<Guid> m_RootRepeatGroupId;
        protected Dictionary<ReportElementId, Guid> m_ElementIdRepeatGroupIds;

        public DimensionalGroupingState()
        {
            m_RepeatGroups = new Dictionary<Guid, DimensionalRepeatGroup>();
            m_RootRepeatGroupId = null;
            m_ElementIdRepeatGroupIds = new Dictionary<ReportElementId, Guid>(ReportRenderingEngine.EqualityComparer_ReportElementId);
        }

        #region Properties

        public Dictionary<Guid, DimensionalRepeatGroup> RepeatGroups
        {
            get { return m_RepeatGroups.ToDictionary(x => x.Key, x => x.Value); }
        }

        public Guid RootRepeatGroupId
        {
            get { return m_RootRepeatGroupId.Value; }
            protected set
            {
                if (m_RootRepeatGroupId.HasValue)
                { throw new InvalidOperationException("The Root Repeat Group has already been set."); }

                RootRepeatGroupId = value;
            }
        }

        public DimensionalRepeatGroup RootRepeatGroup
        {
            get { return RepeatGroups[RootRepeatGroupId]; }
        }

        public Dictionary<ReportElementId, DimensionalRepeatGroup> ElementRepeatGroups
        {
            get { return m_ElementIdRepeatGroupIds.ToDictionary(x => x.Key, x => m_RepeatGroups[x.Value], ReportRenderingEngine.EqualityComparer_ReportElementId); }
        }

        public List<ReportElementId> ProcessedElementIds
        {
            get { return ElementRepeatGroups.Keys.ToList(); }
        }

        #endregion

        #region Assessment Methods

        public void AddRepeatGroup(DimensionalRepeatGroup repeatGroup)
        {
            if (!m_RootRepeatGroupId.HasValue && repeatGroup.ParentRepeatGroupId.HasValue)
            { throw new InvalidOperationException("The Root Structural Group has not yet been set."); }
            if (m_RootRepeatGroupId.HasValue && !repeatGroup.ParentRepeatGroupId.HasValue)
            { throw new InvalidOperationException("The Root Structural Group has already been set."); }

            m_RepeatGroups.Add(repeatGroup.Id, repeatGroup);
            if (!m_RootRepeatGroupId.HasValue)
            { m_RootRepeatGroupId = repeatGroup.Id; }

            foreach (var nestedRepeatGroup in repeatGroup.NestedRepeatGroups.Values)
            { AddRepeatGroup(nestedRepeatGroup); }

            foreach (var element in repeatGroup.GroupedElements)
            { m_ElementIdRepeatGroupIds.Add(element.Key, repeatGroup.Id); }

            repeatGroup.ReportElementAdded += new EventHandler<AddingNewEventArgs>(RepeatGroup_ReportElementAdded);
            repeatGroup.NestedGroupAdded += new EventHandler<AddingNewEventArgs>(RepeatGroup_NestedGroupAdded);
        }

        private void RepeatGroup_ReportElementAdded(object sender, AddingNewEventArgs e)
        {
            var repeatGroup = sender as DimensionalRepeatGroup;
            var elementId = (e.NewObject as IReportElement).Key;

            m_ElementIdRepeatGroupIds.Add(elementId, repeatGroup.Id);
        }

        private void RepeatGroup_NestedGroupAdded(object sender, AddingNewEventArgs e)
        {
            var parentRepeatGroup = sender as DimensionalRepeatGroup;
            var nestedRepeatGroup = (DimensionalRepeatGroup)e.NewObject;

            if (nestedRepeatGroup.ParentRepeatGroupId != parentRepeatGroup.Id)
            { throw new InvalidOperationException("Nested Repeat Group must reference Parent."); }
            if (!parentRepeatGroup.NestedRepeatGroupIds.Contains(nestedRepeatGroup.Id))
            { throw new InvalidOperationException("Parent Repeat Group must reference Child."); }

            AddRepeatGroup(nestedRepeatGroup);
        }

        #endregion

        #region State Methods

        public void SetToProductionState(IReportingDataProvider dataProvider, ICurrentState productionState)
        {
            foreach (var repeatGroup in RepeatGroups.Values)
            {
                var productionContexts = DimensioningUtils.BuildStructuralContexts(dataProvider, productionState, repeatGroup.AdditionalStructuralTypeRefs);
                repeatGroup.SetProductionState(productionState, productionContexts);
            }
        }

        #endregion
    }
}