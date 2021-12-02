using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting.Layouts
{
    public partial class SaveableDimensionLayout : DimensionLayout, IKeyedDomainObject<ReportElementId, SaveableDimensionLayout>
    {
        public static readonly Dimension DefaultDimension = IDimensionLayoutUtils.Dimension_Default;
        public static readonly IEditabilitySpecification DefaultEditabilitySpec = new NoOpEditabilitySpecification();

        private ReportElementId m_ParentReportElementId;

        #region Constructors

        public SaveableDimensionLayout()
            : this(DefaultDimension)
        { }

        public SaveableDimensionLayout(Dimension dimension)
            : this(dimension, DefaultEditabilitySpec)
        { }

        public SaveableDimensionLayout(Dimension dimension, IEditabilitySpecification editabilitySpec)
            : this(ReportElementId.DefaultId, dimension, editabilitySpec)
        { }

        public SaveableDimensionLayout(ReportElementId reportElementId, Dimension dimension)
            : this(reportElementId, dimension, DefaultEditabilitySpec)
        { }

        public SaveableDimensionLayout(ReportElementId reportElementId, Dimension dimension, IEditabilitySpecification editabilitySpec)
            : this(reportElementId.ProjectGuid, reportElementId.RevisionNumber_NonNull, reportElementId.ModelTemplateNumber, reportElementId.ReportGuid, reportElementId.ReportElementNumber, dimension, editabilitySpec)
        { }

        public SaveableDimensionLayout(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, int elementNumber, Dimension dimension)
            : this(projectGuid, revisionNumber, modelTemplateNumber, reportGuid, elementNumber, dimension, DefaultEditabilitySpec)
        { }

        public SaveableDimensionLayout(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, int elementNumber, Dimension dimension, IEditabilitySpecification editabilitySpec)
            : base(dimension, editabilitySpec)
        {
            m_ParentReportElementId = new ReportElementId(projectGuid, revisionNumber, modelTemplateNumber, reportGuid, elementNumber, false);
        }

        public SaveableDimensionLayout(SaveableDimensionLayout layoutToCopy)
            : this(layoutToCopy.m_ParentReportElementId, layoutToCopy)
        { }

        public SaveableDimensionLayout(ReportElementId reportElementId, DimensionLayout layoutToCopy)
            : this(reportElementId.ProjectGuid, reportElementId.RevisionNumber_NonNull, reportElementId.ModelTemplateNumber, reportElementId.ReportGuid, reportElementId.ReportElementNumber, layoutToCopy)
        { }

        public SaveableDimensionLayout(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, int elementNumber, DimensionLayout layoutToCopy)
            : base(layoutToCopy)
        {
            m_ParentReportElementId = new ReportElementId(projectGuid, revisionNumber, modelTemplateNumber, reportGuid, elementNumber, false);
        }

        #endregion

        #region Properties

        [NotMapped]
        public ReportElementId Key
        {
            get { return m_ParentReportElementId; }
        }

        [NotMapped]
        public ReportElementId ParentReportElementId
        {
            get { return m_ParentReportElementId; }
            internal set { m_ParentReportElementId = value; }
        }

        #endregion
    }
}