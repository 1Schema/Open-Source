using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Dimensionality;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class CommonTitleBox : ReportElementBase<CommonTitleBox>, ICommonTitleBox
    {
        protected Dimension m_StackingDimension;

        protected List<int> m_ContainedStructuralTitleRangeNumbers;
        protected List<int> m_ContainedTimeTitleRangeNumbers;

        protected string m_StyleGroup;

        public CommonTitleBox()
            : this(ReportId.DefaultId)
        { }

        public CommonTitleBox(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public CommonTitleBox(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_StackingDimension = ITransposableElementUtils.Default_StackingDimension;

            m_ContainedStructuralTitleRangeNumbers = new List<int>();
            m_ContainedTimeTitleRangeNumbers = new List<int>();

            m_StyleGroup = null;
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_CommonTitleBox;
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
            return null;
        }

        #endregion

        #region ICommonTitleBox Implementation

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var siblings = new List<ReportElementId>();

                siblings.AddRange(ContainedStructuralTitleRangeIds);
                siblings.AddRange(ContainedTimeTitleRangeIds);

                return siblings;
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

                if (!isChanging)
                { return; }

                this.TransposeElementLayoutAndStyle();
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
        public int StackingOrder
        {
            get { return ElementLayout.GetDimensionLayout(StackingDimension).ContentGroup_Value; }
        }

        [NotMapped]
        public int CommonOrder
        {
            get { return ElementLayout.GetDimensionLayout(CommonDimension).ContentGroup_Value; }
            set
            {
                (EditabilitySpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.System;
                var dimensionLayout = ElementLayout.GetDimensionLayout(CommonDimension);
                dimensionLayout.ContentGroup_Value = value;
                (EditabilitySpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.User;
            }
        }

        public int GetOrder(Dimension dimension)
        {
            if (dimension == StackingDimension)
            { return StackingOrder; }
            else if (dimension == CommonDimension)
            { return CommonOrder; }
            else
            { throw new InvalidOperationException("Invalid Dimension specified for 2-D object."); }
        }

        public bool IsOrderEditable(Dimension dimension)
        {
            if (dimension == StackingDimension)
            { return false; }
            else if (dimension == CommonDimension)
            { return true; }
            else
            { throw new InvalidOperationException("Invalid Dimension specified for 2-D object."); }
        }

        public void SetOrder(Dimension dimension, int order)
        {
            if (!IsOrderEditable(dimension))
            { throw new InvalidOperationException("The specified Dimension is not Orderable withing the current object."); }

            if (dimension == CommonDimension)
            { CommonOrder = order; }
            else
            { throw new InvalidOperationException("Invalid Dimension specified for editing Order."); }
        }

        [NotMapped]
        public ICollection<ReportElementId> ContainedStructuralTitleRangeIds
        {
            get
            {
                var siblings = m_ContainedStructuralTitleRangeNumbers.Select(x => new ReportElementId(m_Key.ReportId, x, false)).ToList();
                return siblings;
            }
        }

        [NotMapped]
        public ICollection<int> ContainedStructuralTitleRangeNumbers
        {
            get { return ContainedStructuralTitleRangeIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        [NotMapped]
        public ICollection<ReportElementId> ContainedTimeTitleRangeIds
        {
            get
            {
                var siblings = m_ContainedTimeTitleRangeNumbers.Select(x => new ReportElementId(m_Key.ReportId, x, false)).ToList();
                return siblings;
            }
        }

        [NotMapped]
        public ICollection<int> ContainedTimeTitleRangeNumbers
        {
            get { return ContainedTimeTitleRangeIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        public void AddContainedStructuralTitleRange(IStructuralTitleRange element)
        {
            var typedElement = (element as StructuralTitleRange);

            if (typedElement == null)
            { throw new InvalidOperationException("Unrecognized type of StructuralTitleRange encountered."); }
            if (!typedElement.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (typedElement.ParentElementNumber.HasValue && (typedElement.ParentElementNumber != this.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object has already been added to a different CommonTitleBox."); }
            if (m_ContainedStructuralTitleRangeNumbers.Contains(typedElement.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object already exists in the current CommonTitleBox."); }

            m_ContainedStructuralTitleRangeNumbers.Add(typedElement.Key.ReportElementNumber);
            typedElement.ParentElementId = this.Key;
            typedElement.IsVariableTitleRelated = false;
            typedElement.StackingDimension = this.StackingDimension;
        }

        public void RemoveContainedStructuralTitleRange(IStructuralTitleRange element)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (!m_ContainedStructuralTitleRangeNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object does not exist in the CommonTitleBox."); }

            m_ContainedStructuralTitleRangeNumbers.Remove(element.Key.ReportElementNumber);
            element.ParentElementId = null;
        }

        public void AddContainedTimeTitleRange(ITimeTitleRange element)
        {
            var typedElement = (element as TimeTitleRange);

            if (typedElement == null)
            { throw new InvalidOperationException("Unrecognized type of TimeTitleRange encountered."); }
            if (!typedElement.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (typedElement.ParentElementNumber.HasValue && (typedElement.ParentElementNumber != this.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object has already been added to a different CommonTitleBox."); }
            if (m_ContainedTimeTitleRangeNumbers.Contains(typedElement.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object already exists in the current CommonTitleBox."); }

            m_ContainedTimeTitleRangeNumbers.Add(typedElement.Key.ReportElementNumber);
            typedElement.ParentElementId = this.Key;
            typedElement.IsVariableTitleRelated = false;
            typedElement.StackingDimension = this.StackingDimension;
        }

        public void RemoveContainedTimeTitleRange(ITimeTitleRange element)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (!m_ContainedTimeTitleRangeNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object does not exist in the CommonTitleBox."); }

            m_ContainedTimeTitleRangeNumbers.Remove(element.Key.ReportElementNumber);
            element.ParentElementId = null;
        }

        [NotMapped]
        public string StyleGroup
        {
            get { return m_StyleGroup; }
            set { m_StyleGroup = value; }
        }

        #endregion
    }
}