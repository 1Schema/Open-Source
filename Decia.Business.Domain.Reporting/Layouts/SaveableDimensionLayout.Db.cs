using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Testing;

namespace Decia.Business.Domain.Reporting.Layouts
{
    public partial class SaveableDimensionLayout : IEfStorableObject
    {
        #region Entity Framework Mapper

        public class SaveableDimensionLayout_Mapper : EntityTypeConfiguration<SaveableDimensionLayout>
        {
            public SaveableDimensionLayout_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_Dimension);
                Property(p => p.EF_RenderingMode);
                Property(p => p.EF_AlignmentMode);
                Property(p => p.EF_SizeMode);
                Property(p => p.EF_RangeSize_CombineInputValues);
                Property(p => p.EF_RangeSize_Design);
                Property(p => p.EF_RangeSize_Production);
                Property(p => p.EF_ContainerMode);
                Property(p => p.EF_ContentGroup);
                Property(p => p.EF_MinRangeSizeInCells);
                Property(p => p.EF_MaxRangeSizeInCells);
                Property(p => p.EF_Margin_LesserSide);
                Property(p => p.EF_Margin_GreaterSide);
                Property(p => p.EF_Padding_LesserSide);
                Property(p => p.EF_Padding_GreaterSide);
                Property(p => p.EF_CellSize_CombineInputValues);
                Property(p => p.EF_CellSize_Design);
                Property(p => p.EF_CellSize_Production);
                Property(p => p.EF_OverrideCellSizeInPadding);
                Property(p => p.EF_MergeInteriorAreaCells);
            }
        }

        #endregion

        public object[] EF_CombinedId
        {
            get
            {
                object[] combined = new object[] { EF_ProjectGuid, EF_RevisionNumber, EF_ModelTemplateNumber, EF_ReportGuid, EF_ReportElementNumber, EF_Dimension };
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

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 5)]
        public int EF_Dimension
        {
            get { return (int)m_Dimension; }
            set { m_Dimension = (Dimension)value; }
        }

        [ForceMapped]
        internal int EF_RenderingMode
        {
            get { return (int)m_RenderingMode; }
            set { m_RenderingMode = (RenderingMode)value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_AlignmentMode
        {
            get { return StylingUtils.SaveEnumAsInt(m_AlignmentMode); }
            set { StylingUtils.LoadEnumFromInt(value, out m_AlignmentMode); }
        }

        [ForceMapped]
        internal Nullable<int> EF_SizeMode
        {
            get { return StylingUtils.SaveEnumAsInt(m_SizeMode); }
            set { StylingUtils.LoadEnumFromInt(value, out m_SizeMode); }
        }

        [ForceMapped]
        internal bool EF_RangeSize_CombineInputValues
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_RangeSize_CombineInputValues);
                return m_RangeSize_CombineInputValues;
            }
            set
            {
                m_RangeSize_CombineInputValues = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_RangeSize_CombineInputValues);
            }
        }

        [ForceMapped]
        internal Nullable<int> EF_RangeSize_Design
        {
            get { return m_RangeSize_Design; }
            set { m_RangeSize_Design = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_RangeSize_Production
        {
            get { return m_RangeSize_Production; }
            set { m_RangeSize_Production = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_ContainerMode
        {
            get { return StylingUtils.SaveEnumAsInt(m_ContainerMode); }
            set { StylingUtils.LoadEnumFromInt(value, out m_ContainerMode); }
        }

        [ForceMapped]
        internal Nullable<int> EF_ContentGroup
        {
            get { return m_ContentGroup; }
            set { m_ContentGroup = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_MinRangeSizeInCells
        {
            get { return m_MinRangeSizeInCells; }
            set { m_MinRangeSizeInCells = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_MaxRangeSizeInCells
        {
            get { return m_MaxRangeSizeInCells; }
            set { m_MaxRangeSizeInCells = value; }
        }

        private Nullable<int> m_EF_Margin_LesserSide = null;
        [ForceMapped]
        internal Nullable<int> EF_Margin_LesserSide
        {
            get { return m_Margin.LesserSide; }
            set
            {
                m_EF_Margin_LesserSide = value;
            }
        }

        private Nullable<int> m_EF_Margin_GreaterSide = null;
        [ForceMapped]
        internal Nullable<int> EF_Margin_GreaterSide
        {
            get { return m_Margin.GreaterSide; }
            set
            {
                m_EF_Margin_GreaterSide = value;
                SetMargin();
            }
        }

        private void SetMargin()
        {
            m_Margin = new DimensionStyleValue<int?>(m_EF_Margin_LesserSide, m_EF_Margin_GreaterSide);

            m_EF_Margin_LesserSide = null;
            m_EF_Margin_GreaterSide = null;
        }

        private Nullable<int> m_EF_Padding_LesserSide = null;
        [ForceMapped]
        internal Nullable<int> EF_Padding_LesserSide
        {
            get { return m_Padding.LesserSide; }
            set
            {
                m_EF_Padding_LesserSide = value;
            }
        }

        private Nullable<int> m_EF_Padding_GreaterSide = null;
        [ForceMapped]
        internal Nullable<int> EF_Padding_GreaterSide
        {
            get { return m_Padding.GreaterSide; }
            set
            {
                m_EF_Padding_GreaterSide = value;
                SetPadding();
            }
        }

        private void SetPadding()
        {
            m_Padding = new DimensionStyleValue<int?>(m_EF_Padding_LesserSide, m_EF_Padding_GreaterSide);

            m_EF_Padding_LesserSide = null;
            m_EF_Padding_GreaterSide = null;
        }

        [ForceMapped]
        internal bool EF_CellSize_CombineInputValues
        {
            get { return m_CellSize_CombineInputValues; }
            set { m_CellSize_CombineInputValues = value; }
        }

        [ForceMapped]
        internal Nullable<double> EF_CellSize_Design
        {
            get { return m_CellSize_Design; }
            set { m_CellSize_Design = value; }
        }

        [ForceMapped]
        internal Nullable<double> EF_CellSize_Production
        {
            get { return m_CellSize_Production; }
            set { m_CellSize_Production = value; }
        }

        [ForceMapped]
        internal Nullable<bool> EF_OverrideCellSizeInPadding
        {
            get { return m_OverridePaddingCellSize; }
            set { m_OverridePaddingCellSize = value; }
        }

        [ForceMapped]
        internal Nullable<bool> EF_MergeInteriorAreaCells
        {
            get { return m_MergeInteriorAreaCells; }
            set { m_MergeInteriorAreaCells = value; }
        }
    }
}