using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Drawing;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Testing;

namespace Decia.Business.Domain.Reporting.Styling
{
    public partial class SaveableElementStyle : IEfStorableObject
    {
        #region Entity Framework Mapper

        public class SaveableElementStyle_Mapper : EntityTypeConfiguration<SaveableElementStyle>
        {
            public SaveableElementStyle_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_BackColor);
                Property(p => p.EF_ForeColor);
                Property(p => p.EF_FontName);
                Property(p => p.EF_FontSize);
                Property(p => p.EF_FontStyle);
                Property(p => p.EF_FontHAlign);
                Property(p => p.EF_FontVAlign);
                Property(p => p.EF_Indent);
                Property(p => p.EF_BorderColor_Left);
                Property(p => p.EF_BorderColor_Top);
                Property(p => p.EF_BorderColor_Right);
                Property(p => p.EF_BorderColor_Bottom);
                Property(p => p.EF_BorderStyle_Left);
                Property(p => p.EF_BorderStyle_Top);
                Property(p => p.EF_BorderStyle_Right);
                Property(p => p.EF_BorderStyle_Bottom);
            }
        }

        #endregion

        public object[] EF_CombinedId
        {
            get
            {
                object[] combined = new object[] { EF_ProjectGuid, EF_RevisionNumber, EF_ModelTemplateNumber, EF_ReportGuid, EF_ReportElementNumber };
                return combined;
            }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public Guid EF_ProjectGuid
        {
            get { return m_ParentReportElementId.ProjectGuid; }
            set { m_ParentReportElementId = new ReportElementId(value, m_ParentReportElementId.RevisionNumber_NonNull, m_ParentReportElementId.ModelTemplateNumber, m_ParentReportElementId.ReportGuid, m_ParentReportElementId.ReportElementNumber, false); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_ParentReportElementId.RevisionNumber_NonNull; }
            set { m_ParentReportElementId = new ReportElementId(m_ParentReportElementId.ProjectGuid, value, m_ParentReportElementId.ModelTemplateNumber, m_ParentReportElementId.ReportGuid, m_ParentReportElementId.ReportElementNumber, false); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public int EF_ModelTemplateNumber
        {
            get { return m_ParentReportElementId.ModelTemplateNumber; }
            set { m_ParentReportElementId = new ReportElementId(m_ParentReportElementId.ProjectGuid, m_ParentReportElementId.RevisionNumber_NonNull, value, m_ParentReportElementId.ReportGuid, m_ParentReportElementId.ReportElementNumber, false); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3)]
        public Guid EF_ReportGuid
        {
            get { return m_ParentReportElementId.ReportGuid; }
            set { m_ParentReportElementId = new ReportElementId(m_ParentReportElementId.ProjectGuid, m_ParentReportElementId.RevisionNumber_NonNull, m_ParentReportElementId.ModelTemplateNumber, value, m_ParentReportElementId.ReportElementNumber, false); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 4)]
        public int EF_ReportElementNumber
        {
            get { return m_ParentReportElementId.ReportElementNumber; }
            set { m_ParentReportElementId = new ReportElementId(m_ParentReportElementId.ProjectGuid, m_ParentReportElementId.RevisionNumber_NonNull, m_ParentReportElementId.ModelTemplateNumber, m_ParentReportElementId.ReportGuid, value, false); }
        }

        [ForceMapped]
        internal string EF_BackColor
        {
            get { return ColorSpecification.SaveAsString(m_BackColor); }
            set { ColorSpecification.LoadFromString(value, out m_BackColor); }
        }

        [ForceMapped]
        internal string EF_ForeColor
        {
            get { return ColorSpecification.SaveAsString(m_ForeColor); }
            set { ColorSpecification.LoadFromString(value, out m_ForeColor); }
        }

        [ForceMapped]
        internal string EF_FontName
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_FontName);
                return m_FontName;
            }
            set
            {
                m_FontName = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_FontName);
            }
        }

        [ForceMapped]
        internal Nullable<double> EF_FontSize
        {
            get { return m_FontSize; }
            set { m_FontSize = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_FontStyle
        {
            get { return StylingUtils.SaveEnumAsInt(m_FontStyle); }
            set { StylingUtils.LoadEnumFromInt(value, out m_FontStyle); }
        }

        [ForceMapped]
        internal Nullable<int> EF_FontHAlign
        {
            get { return StylingUtils.SaveEnumAsInt(m_FontHAlign); }
            set { StylingUtils.LoadEnumFromInt(value, out m_FontHAlign); }
        }

        [ForceMapped]
        internal Nullable<int> EF_FontVAlign
        {
            get { return StylingUtils.SaveEnumAsInt(m_FontVAlign); }
            set { StylingUtils.LoadEnumFromInt(value, out m_FontVAlign); }
        }

        [ForceMapped]
        internal Nullable<int> EF_Indent
        {
            get { return m_Indent; }
            set { m_Indent = value; }
        }

        private Nullable<ColorSpecification> m_EF_BorderColor_Left = null;
        [ForceMapped]
        internal string EF_BorderColor_Left
        {
            get { return ColorSpecification.SaveAsString(m_BorderColor.Left); }
            set
            {
                ColorSpecification.LoadFromString(value, out m_EF_BorderColor_Left);
            }
        }

        private Nullable<ColorSpecification> m_EF_BorderColor_Top = null;
        [ForceMapped]
        internal string EF_BorderColor_Top
        {
            get { return ColorSpecification.SaveAsString(m_BorderColor.Top); }
            set
            {
                ColorSpecification.LoadFromString(value, out m_EF_BorderColor_Top);
            }
        }

        private Nullable<ColorSpecification> m_EF_BorderColor_Right = null;
        [ForceMapped]
        internal string EF_BorderColor_Right
        {
            get { return ColorSpecification.SaveAsString(m_BorderColor.Right); }
            set
            {
                ColorSpecification.LoadFromString(value, out m_EF_BorderColor_Right);
            }
        }

        private Nullable<ColorSpecification> m_EF_BorderColor_Bottom = null;
        [ForceMapped]
        internal string EF_BorderColor_Bottom
        {
            get { return ColorSpecification.SaveAsString(m_BorderColor.Bottom); }
            set
            {
                ColorSpecification.LoadFromString(value, out m_EF_BorderColor_Bottom);
                SetBorderColor();
            }
        }

        private void SetBorderColor()
        {
            m_BorderColor = new BoxStyleValue<ColorSpecification?>(m_EF_BorderColor_Left, m_EF_BorderColor_Top, m_EF_BorderColor_Right, m_EF_BorderColor_Bottom);

            m_EF_BorderColor_Left = null;
            m_EF_BorderColor_Top = null;
            m_EF_BorderColor_Right = null;
            m_EF_BorderColor_Bottom = null;
        }

        private Nullable<BorderLineStyle> m_EF_BorderStyle_Left = null;
        [ForceMapped]
        internal Nullable<int> EF_BorderStyle_Left
        {
            get { return StylingUtils.SaveEnumAsInt(m_BorderStyle.Left); }
            set
            {
                StylingUtils.LoadEnumFromInt(value, out m_EF_BorderStyle_Left);
            }
        }

        private Nullable<BorderLineStyle> m_EF_BorderStyle_Top = null;
        [ForceMapped]
        internal Nullable<int> EF_BorderStyle_Top
        {
            get { return StylingUtils.SaveEnumAsInt(m_BorderStyle.Top); }
            set
            {
                StylingUtils.LoadEnumFromInt(value, out m_EF_BorderStyle_Top);
            }
        }

        private Nullable<BorderLineStyle> m_EF_BorderStyle_Right = null;
        [ForceMapped]
        internal Nullable<int> EF_BorderStyle_Right
        {
            get { return StylingUtils.SaveEnumAsInt(m_BorderStyle.Right); }
            set
            {
                StylingUtils.LoadEnumFromInt(value, out m_EF_BorderStyle_Right);
            }
        }

        private Nullable<BorderLineStyle> m_EF_BorderStyle_Bottom = null;
        [ForceMapped]
        internal Nullable<int> EF_BorderStyle_Bottom
        {
            get { return StylingUtils.SaveEnumAsInt(m_BorderStyle.Bottom); }
            set
            {
                StylingUtils.LoadEnumFromInt(value, out m_EF_BorderStyle_Bottom);
                SetBorderStyle();
            }
        }

        private void SetBorderStyle()
        {
            m_BorderStyle = new BoxStyleValue<BorderLineStyle?>(m_EF_BorderStyle_Left, m_EF_BorderStyle_Top, m_EF_BorderStyle_Right, m_EF_BorderStyle_Bottom);

            m_EF_BorderStyle_Left = null;
            m_EF_BorderStyle_Top = null;
            m_EF_BorderStyle_Right = null;
            m_EF_BorderStyle_Bottom = null;
        }
    }
}