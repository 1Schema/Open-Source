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
    public partial class TableHeader : ReportElementBase<TableHeader>, ITableHeader
    {
        protected List<int> m_ChildNumbers;

        public TableHeader()
            : this(ReportId.DefaultId)
        { }

        public TableHeader(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public TableHeader(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_ChildNumbers = new List<int>();
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_TableHeader;
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

        #region ITableHeader Implementation

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var children = m_ChildNumbers.Select(x => new ReportElementId(m_Key.ReportId, x, false)).ToList();
                return children;
            }
        }

        [NotMapped]
        public ICollection<int> ChildNumbers
        {
            get { return ChildIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        public void AddChild(ICell element)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (m_ChildNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object already exists in the VariableTitleContainer."); }

            m_ChildNumbers.Add(element.Key.ReportElementNumber);
            element.ParentElementId = this.Key;
        }

        public void RemoveChild(ICell element)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (!m_ChildNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object does not exist in the VariableTitleContainer."); }

            m_ChildNumbers.Remove(element.Key.ReportElementNumber);
            element.ParentElementId = null;
        }

        #endregion

        #region IContainer Implementation

        void IContainer.AddChild(IReportElement element)
        {
            if (!(element is ICell))
            { throw new InvalidOperationException("The specified child is not a Cell."); }

            AddChild(element as ICell);
        }

        void IContainer.RemoveChild(IReportElement element)
        {
            if (!(element is ICell))
            { throw new InvalidOperationException("The specified child is not a Cell."); }

            RemoveChild(element as ICell);
        }

        #endregion
    }
}