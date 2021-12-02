using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class CommonTitleContainer : ReportElementBase<CommonTitleContainer>, ICommonTitleContainer
    {
        protected Nullable<int> m_VariableDataContainerNumber;
        protected Dimension m_StackingDimension;

        protected List<int> m_CommonTitleBoxNumbers;

        protected SortedDictionary<int, ModelObjectReference> m_DimensionsBySortIndex;

        public CommonTitleContainer()
            : this(ReportId.DefaultId)
        { }

        public CommonTitleContainer(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public CommonTitleContainer(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_VariableDataContainerNumber = null;
            m_StackingDimension = ITransposableElementUtils.Default_StackingDimension;

            m_CommonTitleBoxNumbers = new List<int>();

            m_DimensionsBySortIndex = new SortedDictionary<int, ModelObjectReference>();
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_CommonTitleContainer;
        }

        protected override IEditabilitySpecification GetEditabilitySpecForLayout(Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return EditabilitySpec; }
            else if (dimension == Dimension.Y)
            { return EditabilitySpec; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        protected override IEditabilitySpecification GetEditabilitySpecForStyle()
        {
            return null;
        }

        protected override IDimensionLayout GetDefaultLayout(Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return DefaultLayout_X; }
            else if (dimension == Dimension.Y)
            { return DefaultLayout_Y; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        protected override IElementStyle GetDefaultStyle()
        {
            return DefaultStyle;
        }

        #endregion

        #region ICommonTitleContainer Implementation

        [NotMapped]
        public ReportElementId VariableDataContainerId
        {
            get { return new ReportElementId(this.Key.ReportId, m_VariableDataContainerNumber.Value); }
        }

        [NotMapped]
        public int VariableDataContainerNumber
        {
            get { return VariableDataContainerId.ReportElementNumber; }
        }

        internal void SetVariableDataContainer(IVariableDataContainer variableDataContainer)
        {
            var typedVariableDataContainer = (variableDataContainer as VariableDataContainer);

            if (typedVariableDataContainer == null)
            { throw new InvalidOperationException("Unrecognized type of VariableDataContainer encountered."); }

            if (!typedVariableDataContainer.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The VariableDataContainer specified belongs to a different Report."); }

            m_VariableDataContainerNumber = typedVariableDataContainer.Key.ReportElementNumber;
            typedVariableDataContainer.CommonTitleContainerId = this.Key;
        }

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var children = new List<ReportElementId>();

                children.AddRange(CommonTitleBoxIds);

                return children;
            }
        }

        [NotMapped]
        public ICollection<int> ChildNumbers
        {
            get { return ChildIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        [NotMapped]
        public Dimension StackingDimension
        {
            get { return m_StackingDimension; }
            internal set
            {
                value.AssertIsValidTableDimension();
                var isChanging = (m_StackingDimension != value);
                m_StackingDimension = value;

                if (isChanging)
                {
                    this.TransposeElementLayoutAndStyle();
                }

                var isStackingModeCorrect = (ElementLayout.GetDimensionLayout(StackingDimension).ContainerMode_Value == ContainerMode.Single);
                var isCommonModeCorrect = (ElementLayout.GetDimensionLayout(CommonDimension).ContainerMode_Value == ContainerMode.Grid);

                if (!isStackingModeCorrect || !isCommonModeCorrect)
                { throw new InvalidOperationException("The CommonTitleContainer's ContainerMode values are not set properly."); }
            }
        }

        [NotMapped]
        public Dimension CommonDimension
        {
            get { return StackingDimension.Invert(); }
        }

        [NotMapped]
        public bool IsTransposed
        {
            get
            {
                var stackingResult = StackingDimension.IsStackingDimensionTransposed();
                var commonResult = CommonDimension.IsCommonDimensionTransposed();

                if (stackingResult != commonResult)
                { throw new InvalidOperationException("Unexpected IsTransposed setting encountered."); }

                return stackingResult;
            }
        }

        [NotMapped]
        public ICollection<ReportElementId> CommonTitleBoxIds
        {
            get
            {
                var children = m_CommonTitleBoxNumbers.Select(x => new ReportElementId(m_Key.ReportId, x, false)).ToList();
                return children;
            }
        }

        [NotMapped]
        public ICollection<int> CommonTitleBoxNumbers
        {
            get { return CommonTitleBoxIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        public void AddCommonTitleBox(ICommonTitleBox element, IEnumerable<IDimensionalRange> containedDimensionalRanges)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            if (containedDimensionalRanges.Where(x => !x.ParentReportId.Equals_Revisionless(this.ParentReportId)).ToList().Count() > 0)
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (m_CommonTitleBoxNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object already exists in the current CommonTitleContainer."); }

            foreach (var dimensionalRange in containedDimensionalRanges)
            {
                if (element.ChildNumbers.Contains(dimensionalRange.Key.ReportElementNumber) && (dimensionalRange.ParentElementNumber == element.Key.ReportElementNumber))
                { continue; }

                if (dimensionalRange is IStructuralTitleRange)
                { element.AddContainedStructuralTitleRange(dimensionalRange as IStructuralTitleRange); }
                else if (dimensionalRange is ITimeTitleRange)
                { element.AddContainedTimeTitleRange(dimensionalRange as ITimeTitleRange); }
                else
                { throw new InvalidOperationException("Unrecognized DimensionalRange encountered."); }
            }

            var expectedIds = new List<ReportElementId>();
            expectedIds.AddRange(element.ContainedStructuralTitleRangeIds);
            expectedIds.AddRange(element.ContainedTimeTitleRangeIds);

            var containedRanges = containedDimensionalRanges.ToDictionary(x => x.Key, x => x, ReportRenderingEngine.EqualityComparer_ReportElementId);

            if (!expectedIds.AreUnorderedCollectionsEqual(containedRanges.Keys))
            { throw new InvalidOperationException("Missing Dimensional Ranges expected."); }

            m_CommonTitleBoxNumbers.Add(element.Key.ReportElementNumber);
            element.ParentElementId = this.Key;

            element.SetStackingDimension(this.StackingDimension);

            foreach (var range in containedDimensionalRanges)
            { range.SetStackingDimension(this.StackingDimension); }

            var existingDimensionOrders = DimensionSortOrders;
            var orderingDimension = this.CommonDimension.Invert();
            var orderedElements = containedDimensionalRanges.ProduceOrderedElements(orderingDimension);

            foreach (int orderIndex in orderedElements.Keys)
            {
                var dimensionalRange = orderedElements[orderIndex];

                if (existingDimensionOrders.ContainsKey(dimensionalRange.DimensionalTypeRef))
                { continue; }

                SetToMaxSortOrder(dimensionalRange.DimensionalTypeRef);
            }
        }

        public void RemoveCommonTitleBox(ICommonTitleBox element)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (!m_CommonTitleBoxNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object does not exist in the CommonTitleContainer."); }

            m_CommonTitleBoxNumbers.Remove(element.Key.ReportElementNumber);
            element.ParentElementId = null;
        }

        [NotMapped]
        public bool UseDynamicDimensionSorting
        {
            get { return false; }
        }

        [NotMapped]
        public ICollection<ModelObjectReference> Dimensions
        {
            get
            {
                var dimensions = new HashSet<ModelObjectReference>(m_DimensionsBySortIndex.Values, ModelObjectReference.DimensionalComparer);
                return dimensions;
            }
        }

        [NotMapped]
        public IDictionary<ModelObjectReference, int> DimensionSortOrders
        {
            get
            {
                var dimensionOrders = SortOrderedDimensions.ToDictionary(x => x.Value, x => x.Key, ModelObjectReference.DimensionalComparer);
                return dimensionOrders;
            }
        }

        [NotMapped]
        public IDictionary<int, ModelObjectReference> SortOrderedDimensions
        {
            get
            {
                var orderedDimensions = new SortedDictionary<int, ModelObjectReference>(m_DimensionsBySortIndex);
                return orderedDimensions;
            }
        }

        [NotMapped]
        public int MinSortOrder
        {
            get { return (m_DimensionsBySortIndex.Count > 0) ? m_DimensionsBySortIndex.Keys.Min() : 0; }
        }

        [NotMapped]
        public int MaxSortOrder
        {
            get { return (m_DimensionsBySortIndex.Count > 0) ? m_DimensionsBySortIndex.Keys.Max() : 0; }
        }

        public bool HasSortOrder(ModelObjectReference dimensionTypeRef)
        {
            return Dimensions.Contains(dimensionTypeRef);
        }

        public void SetToMaxSortOrder(ModelObjectReference dimensionTypeRef)
        {
            SetSortOrder(dimensionTypeRef, this.MaxSortOrder + 1);
        }

        public void SetSortOrder(ModelObjectReference dimensionTypeRef, int sortOrder)
        {
            var elementOrders = DimensionSortOrders;
            var elementIsAlreadyOrdered = elementOrders.ContainsKey(dimensionTypeRef);

            if (!elementIsAlreadyOrdered)
            {
                ElementOrderingUtils.MoveOrderedDimensionRefs(ref m_DimensionsBySortIndex, sortOrder, MaxSortOrder, 1);
            }
            else
            {
                int previousSortOrder = elementOrders[dimensionTypeRef];

                m_DimensionsBySortIndex.Remove(previousSortOrder);

                if (sortOrder > previousSortOrder)
                {
                    ElementOrderingUtils.MoveOrderedDimensionRefs(ref m_DimensionsBySortIndex, (previousSortOrder + 1), sortOrder, -1);
                }
                else
                {
                    ElementOrderingUtils.MoveOrderedDimensionRefs(ref m_DimensionsBySortIndex, sortOrder, (previousSortOrder - 1), 1);
                }
            }

            m_DimensionsBySortIndex.Add(sortOrder, dimensionTypeRef);
        }

        public void RemoveSortOrder(ModelObjectReference dimensionTypeRef)
        {
            var dimensionOrders = DimensionSortOrders;

            if (!dimensionOrders.ContainsKey(dimensionTypeRef))
            { throw new InvalidOperationException("The specified Dimension does not exist in the Container."); }

            var dimensionOrder = dimensionOrders[dimensionTypeRef];
            m_DimensionsBySortIndex.Remove(dimensionOrder);
        }

        public void ClearSortOrders()
        {
            m_DimensionsBySortIndex.Clear();
        }

        #endregion
    }
}