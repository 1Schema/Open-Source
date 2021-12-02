using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class VariableDataContainer : ReportElementBase<VariableDataContainer>, IVariableDataContainer
    {
        protected Nullable<int> m_VariableTitleContainerNumber;
        protected Nullable<int> m_CommonTitleContainerNumber;
        protected Dimension m_StackingDimension;

        protected List<int> m_ChildNumbers;

        public VariableDataContainer()
            : this(ReportId.DefaultId)
        { }

        public VariableDataContainer(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public VariableDataContainer(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_VariableTitleContainerNumber = null;
            m_CommonTitleContainerNumber = null;
            m_StackingDimension = ITransposableElementUtils.Default_StackingDimension;

            m_ChildNumbers = new List<int>();
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_VariableDataContainer;
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

        #region IVariableDataContainer Implementation

        [NotMapped]
        public ReportElementId VariableTitleContainerId
        {
            get { return new ReportElementId(m_Key.ReportId, m_VariableTitleContainerNumber.Value); }
            internal set
            {
                if (!value.ReportId.Equals_Revisionless(this.ParentReportId))
                { throw new InvalidOperationException("Cannot mix elements from different reports."); }

                m_VariableTitleContainerNumber = value.ReportElementNumber;
            }
        }

        [NotMapped]
        public int VariableTitleContainerNumber
        {
            get { return VariableTitleContainerId.ReportElementNumber; }
        }

        [NotMapped]
        public ReportElementId CommonTitleContainerId
        {
            get { return new ReportElementId(m_Key.ReportId, m_CommonTitleContainerNumber.Value); }
            internal set
            {
                if (!value.ReportId.Equals_Revisionless(this.ParentReportId))
                { throw new InvalidOperationException("Cannot mix elements from different reports."); }

                m_CommonTitleContainerNumber = value.ReportElementNumber;
            }
        }

        [NotMapped]
        public int CommonTitleContainerNumber
        {
            get { return CommonTitleContainerId.ReportElementNumber; }
        }

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var children = new List<ReportElementId>();

                children.AddRange(VariableDataBoxIds);

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

                var isStackingModeCorrect = (ElementLayout.GetDimensionLayout(StackingDimension).ContainerMode_Value == ContainerMode.Grid);
                var isCommonModeCorrect = (ElementLayout.GetDimensionLayout(CommonDimension).ContainerMode_Value == ContainerMode.Grid);

                if (!isStackingModeCorrect || !isCommonModeCorrect)
                { throw new InvalidOperationException("The VariableDataContainer's ContainerMode values are not set properly."); }
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
        public ICollection<ReportElementId> VariableDataBoxIds
        {
            get
            {
                var children = m_ChildNumbers.Select(x => new ReportElementId(m_Key.ReportId, x, false)).ToList();
                return children;
            }
        }

        [NotMapped]
        public ICollection<int> VariableDataBoxNumbers
        {
            get { return VariableDataBoxIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        public void AddVariableDataBox(IVariableDataBox element, IVariableDataRange containedVariableRange, ICommonTitleBox relatedCommonTitleBox, IVariableTitleBox relatedVariableTitleBox)
        {
            var reportIdComparer = new ProjectMemberComparer_Revisionless<ReportId>();

            var typedElement = (element as VariableDataBox);
            if (typedElement == null)
            { throw new InvalidOperationException("Unrecognized type of VariableDataBox encountered."); }

            if (!typedElement.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            if (!containedVariableRange.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            if (!relatedCommonTitleBox.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            if (!relatedVariableTitleBox.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (m_ChildNumbers.Contains(typedElement.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object already exists in the VariableDataContainer."); }

            if (!(typedElement.ChildNumbers.Contains(containedVariableRange.Key.ReportElementNumber) && (containedVariableRange.ParentElementNumber == typedElement.Key.ReportElementNumber)))
            { typedElement.SetContainedVariableDataRange(containedVariableRange); }

            if (typedElement.ContainedVariableDataRangeId != containedVariableRange.Key)
            { throw new InvalidOperationException("Invalid Data Range encountered."); }

            m_ChildNumbers.Add(typedElement.Key.ReportElementNumber);
            typedElement.ParentElementId = this.Key;

            typedElement.RelatedCommonTitleBoxId = relatedCommonTitleBox.Key;
            typedElement.RelatedVariableTitleBoxId = relatedVariableTitleBox.Key;

            typedElement.SetStackingDimension(this.StackingDimension);
            containedVariableRange.SetStackingDimension(this.StackingDimension);
        }

        public void RemoveVariableDataBox(IVariableDataBox element)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (!m_ChildNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object does not exist in the VariableDataContainer."); }

            m_ChildNumbers.Remove(element.Key.ReportElementNumber);
            element.ParentElementId = null;
        }

        #endregion
    }
}