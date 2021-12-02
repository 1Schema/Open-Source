using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class ColumnHeader : ReportElementBase<ColumnHeader>, IColumnHeader
    {
        protected Nullable<bool> m_HoldsVariableContainer;
        protected Nullable<int> m_NestedTitleContainerNumber;

        public ColumnHeader()
            : this(ReportId.DefaultId)
        { }

        public ColumnHeader(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public ColumnHeader(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_HoldsVariableContainer = null;
            m_NestedTitleContainerNumber = null;
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_ColumnHeader;
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

        #region IColumnHeader Implementation

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var children = new List<ReportElementId>();

                if (m_NestedTitleContainerNumber.HasValue)
                { children.Add(NestedContainerId); }

                return children;
            }
        }

        [NotMapped]
        public ICollection<int> ChildNumbers
        {
            get { return ChildIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        [NotMapped]
        public bool HoldsVariableContainer
        {
            get { return (m_HoldsVariableContainer == true); }
        }

        [NotMapped]
        public bool HoldsCommonContainer
        {
            get { return (m_HoldsVariableContainer == false); }
        }

        [NotMapped]
        public ReportElementId NestedContainerId
        {
            get { return new ReportElementId(m_Key.ReportId, m_NestedTitleContainerNumber.Value, false); }
        }

        [NotMapped]
        public int NestedContainerNumber
        {
            get { return NestedContainerId.ReportElementNumber; }
        }

        internal void SetToVariableTitleContainer(IVariableTitleContainer variableTitleContainer, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> transposableElements)
        {
            var transpose = true;
            var stackingDimension = transpose.GetStackingDimension();

            var typedVariableTitleContainer = (variableTitleContainer as VariableTitleContainer);
            var typedVariableDataContainer = (variableDataContainer as VariableDataContainer);

            if (typedVariableTitleContainer == null)
            { throw new InvalidOperationException("Unrecognized type of VariableTitleContainer encountered."); }
            if (typedVariableDataContainer == null)
            { throw new InvalidOperationException("Unrecognized type of VariableDataContainer encountered."); }

            if (!typedVariableTitleContainer.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            if (!typedVariableDataContainer.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            foreach (var element in transposableElements)
            {
                if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
                { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            }

            m_HoldsVariableContainer = transpose;
            m_NestedTitleContainerNumber = typedVariableTitleContainer.Key.ReportElementNumber;

            typedVariableTitleContainer.ParentElementId = this.Key;
            typedVariableTitleContainer.StackingDimension = stackingDimension;
            typedVariableDataContainer.StackingDimension = stackingDimension;

            foreach (var element in transposableElements)
            {
                element.SetStackingDimension(stackingDimension);
            }
        }

        internal void SetToCommonTitleContainer(ICommonTitleContainer commonTitleContainer, IEnumerable<ITransposableElement> transposableElements)
        {
            var transpose = false;
            var stackingDimension = transpose.GetStackingDimension();

            var typedCommonTitleContainer = (commonTitleContainer as CommonTitleContainer);

            if (typedCommonTitleContainer == null)
            { throw new InvalidOperationException("Unrecognized type of CommonTitleContainer encountered."); }

            if (!typedCommonTitleContainer.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            foreach (var element in transposableElements)
            {
                if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
                { throw new InvalidOperationException("Cannot mix elements from different reports."); }
            }

            m_HoldsVariableContainer = transpose;
            m_NestedTitleContainerNumber = typedCommonTitleContainer.Key.ReportElementNumber;

            typedCommonTitleContainer.ParentElementId = this.Key;
            typedCommonTitleContainer.StackingDimension = stackingDimension;

            foreach (var element in transposableElements)
            {
                element.SetStackingDimension(stackingDimension);
            }
        }

        #endregion
    }
}