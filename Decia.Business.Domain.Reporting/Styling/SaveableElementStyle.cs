using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Styling;

namespace Decia.Business.Domain.Reporting.Styling
{
    public partial class SaveableElementStyle : ElementStyle, IKeyedDomainObject<ReportElementId, SaveableElementStyle>
    {
        public static readonly IEditabilitySpecification DefaultEditabilitySpec = new NoOpEditabilitySpecification();

        private ReportElementId m_ParentReportElementId;

        #region Constructors

        public SaveableElementStyle()
            : this(ReportElementId.DefaultId)
        { }

        public SaveableElementStyle(ReportElementId reportElementId)
            : this(reportElementId, DefaultEditabilitySpec)
        { }

        public SaveableElementStyle(ReportElementId reportElementId, IEditabilitySpecification editabilitySpec)
            : this(reportElementId.ProjectGuid, reportElementId.RevisionNumber_NonNull, reportElementId.ModelTemplateNumber, reportElementId.ReportGuid, reportElementId.ReportElementNumber, editabilitySpec)
        { }

        public SaveableElementStyle(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, int elementNumber)
            : this(projectGuid, revisionNumber, modelTemplateNumber, reportGuid, elementNumber, DefaultEditabilitySpec)
        { }

        public SaveableElementStyle(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, int elementNumber, IEditabilitySpecification editabilitySpec)
            : base(editabilitySpec)
        {
            m_ParentReportElementId = new ReportElementId(projectGuid, revisionNumber, modelTemplateNumber, reportGuid, elementNumber, false);
        }

        public SaveableElementStyle(SaveableElementStyle styleToCopy)
            : this(styleToCopy.m_ParentReportElementId, styleToCopy)
        { }

        public SaveableElementStyle(ReportElementId reportElementId, ElementStyle styleToCopy)
            : this(reportElementId.ProjectGuid, reportElementId.RevisionNumber_NonNull, reportElementId.ModelTemplateNumber, reportElementId.ReportGuid, reportElementId.ReportElementNumber, styleToCopy)
        { }

        public SaveableElementStyle(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, int elementNumber, ElementStyle styleToCopy)
            : base(styleToCopy)
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