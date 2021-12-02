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
    public partial class DataArea : ReportElementBase<DataArea>, IDataArea
    {
        protected Nullable<int> m_VariableDataContainerNumber;

        public DataArea()
            : this(ReportId.DefaultId)
        { }

        public DataArea(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public DataArea(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_VariableDataContainerNumber = null;
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_DataArea;
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

        #region IDataArea Implementation

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var children = new List<ReportElementId>();

                if (m_VariableDataContainerNumber.HasValue)
                { children.Add(VariableDataContainerId); }

                return children;
            }
        }

        [NotMapped]
        public ICollection<int> ChildNumbers
        {
            get { return ChildIds.Select(x => x.ReportElementNumber).ToList(); }
        }

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
            if (!variableDataContainer.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The VariableDataContainer specified belongs to a different Report."); }

            m_VariableDataContainerNumber = variableDataContainer.Key.ReportElementNumber;
            variableDataContainer.ParentElementId = this.Key;
        }

        #endregion
    }
}