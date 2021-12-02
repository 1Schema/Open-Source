using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.Testing;

namespace Decia.Business.Domain.Exports
{
    public partial class ExportHistoryItem : IEfEntity_DbQueryable<ExportHistoryItemId, string>
    {
        #region Entity Framework Mapper

        public class ExportHistoryItem_Mapper : EntityTypeConfiguration<ExportHistoryItem>
        {
            public ExportHistoryItem_Mapper()
            {
                Property(p => p.EF_ExporterGuid);
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_DateOfExport);
                Property(p => p.EF_TargetTypeCategory);
                Property(p => p.EF_TargetTypeValue);
                Property(p => p.EF_WasDataIncluded);
                Property(p => p.EF_ExtendedData);
                Property(p => p.EF_CreatorGuid);
                Property(p => p.EF_CreationDate);
                Property(p => p.EF_OwnerType);
                Property(p => p.EF_OwnerGuid);
            }
        }

        #endregion

        #region IEfEntity_DbQueryable<ExportHistoryItemId, string> Implementation

        public static string KeyMatchingSql { get; protected set; }

        [NotMapped]
        string IEfEntity<string>.EF_Id
        {
            get { return m_Key.ToString(); }
            set { /* do nothing */ }
        }

        string IEfEntity_DbQueryable<ExportHistoryItemId>.ConvertKey_ToMatchableText(ExportHistoryItemId key)
        {
            var values = new object[] { key.ExporterGuid, key.ProjectGuid, key.RevisionNumber_NonNull, key.DateOfExport.ToString(AdoNetUtils.Decia_DbFormatString_ForDateTime) };
            var valuesAsText = AdoNetUtils.CovertToKeyMatchingText(values, false);
            return valuesAsText;
        }

        string IEfEntity_DbQueryable<ExportHistoryItemId>.GetSqlCode_ForKeyMatching()
        {
            if (!string.IsNullOrWhiteSpace(KeyMatchingSql))
            { return KeyMatchingSql; }

            Func<ExportHistoryItem, Type> exporterGuidType = ((ExportHistoryItem ehi) => ehi.EF_ExporterGuid.GetType());
            Func<ExportHistoryItem, Type> projectGuidType = ((ExportHistoryItem ehi) => ehi.EF_ProjectGuid.GetType());
            Func<ExportHistoryItem, Type> revisionNumberType = ((ExportHistoryItem ehi) => ehi.EF_RevisionNumber.GetType());
            Func<ExportHistoryItem, Type> exportDateType = ((ExportHistoryItem ehi) => ehi.EF_DateOfExport.GetType());

            var exporterGuidName = ClassReflector.GetPropertyName((ExportHistoryItem ehi) => ehi.EF_ExporterGuid);
            var projectGuidName = ClassReflector.GetPropertyName((ExportHistoryItem ehi) => ehi.EF_ProjectGuid);
            var revisionNumberName = ClassReflector.GetPropertyName((ExportHistoryItem ehi) => ehi.EF_RevisionNumber);
            var exportDateName = ClassReflector.GetPropertyName((ExportHistoryItem ehi) => ehi.EF_DateOfExport);

            var exporterGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(exporterGuidType(this), exporterGuidName);
            var projectGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(projectGuidType(this), projectGuidName);
            var revisionNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(revisionNumberType(this), revisionNumberName);
            var exportDateText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(exportDateType(this), exportDateName);

            var values = new object[] { exporterGuidText, projectGuidText, revisionNumberText, exportDateText };
            KeyMatchingSql = AdoNetUtils.CovertToKeyMatchingText(values, true);
            return KeyMatchingSql;
        }

        #endregion

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public Guid EF_ExporterGuid
        {
            get { return m_Key.ExporterGuid; }
            set { m_Key = new ExportHistoryItemId(value, m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, m_Key.DateOfExport); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public Guid EF_ProjectGuid
        {
            get { return m_Key.ProjectGuid; }
            set { m_Key = new ExportHistoryItemId(m_Key.ExporterGuid, value, m_Key.RevisionNumber_NonNull, m_Key.DateOfExport); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public long EF_RevisionNumber
        {
            get { return m_Key.RevisionNumber_NonNull; }
            set { m_Key = new ExportHistoryItemId(m_Key.ExporterGuid, m_Key.ProjectGuid, value, m_Key.DateOfExport); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3)]
        public DateTime EF_DateOfExport
        {
            get { return m_Key.DateOfExport; }
            set { m_Key = new ExportHistoryItemId(m_Key.ExporterGuid, m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, value); }
        }

        [ForceMapped]
        internal int EF_TargetTypeCategory
        {
            get { return (int)m_TargetTypeCategory; }
            set { m_TargetTypeCategory = (TargetTypeCategory)value; }
        }

        [ForceMapped]
        internal int EF_TargetTypeValue
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_TargetTypeValue);
                return m_TargetTypeValue;
            }
            set
            {
                m_TargetTypeValue = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_TargetTypeValue);
            }
        }

        [ForceMapped]
        internal bool EF_WasDataIncluded
        {
            get { return m_WasDataIncluded; }
            set { m_WasDataIncluded = value; }
        }

        [ForceMapped]
        internal string EF_ExtendedData
        {
            get { return m_ExtendedData; }
            set { m_ExtendedData = value; }
        }

        #region IPermissionable Database Persistence Properties

        [ForceMapped]
        internal Guid EF_CreatorGuid
        {
            get
            {
                this.AssertCreatorIsNotAnonymous();
                return m_CreatorGuid;
            }
            set { m_CreatorGuid = value; }
        }

        [ForceMapped]
        internal DateTime EF_CreationDate
        {
            get { return m_CreationDate; }
            set { m_CreationDate = value; }
        }

        [ForceMapped]
        internal int EF_OwnerType
        {
            get { return (int)m_OwnerType; }
            set { m_OwnerType = (SiteActorType)value; }
        }

        [ForceMapped]
        internal Guid EF_OwnerGuid
        {
            get { return m_OwnerGuid; }
            set { m_OwnerGuid = value; }
        }

        #endregion
    }
}