using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Testing;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.ChronometricValues.TimeDimensions
{
    public partial class SaveableTimeDimension : IEfStorableObject
    {
        #region Entity Framework Mapper

        public class SaveableTimeDimension_Mapper : EntityTypeConfiguration<SaveableTimeDimension>
        {
            public SaveableTimeDimension_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ChronometricValueGuid);
                Property(p => p.EF_TimeDimensionType);
                Property(p => p.EF_TimeValueType);
                Property(p => p.EF_TimePeriodType);
                Property(p => p.EF_FirstPeriodStartDate);
                Property(p => p.EF_LastPeriodEndDate);
            }
        }

        #endregion

        public object[] EF_CombinedId
        {
            get
            {
                object[] combined = new object[] { EF_ProjectGuid, EF_RevisionNumber, EF_ChronometricValueGuid, EF_TimeDimensionType };
                return combined;
            }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public Guid EF_ProjectGuid
        {
            get { return m_ParentChronometricValueId.ProjectGuid; }
            set { m_ParentChronometricValueId = new ChronometricValueId(value, m_ParentChronometricValueId.RevisionNumber_NonNull, m_ParentChronometricValueId.ChronometricValueGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_ParentChronometricValueId.RevisionNumber_NonNull; }
            set { m_ParentChronometricValueId = new ChronometricValueId(m_ParentChronometricValueId.ProjectGuid, value, m_ParentChronometricValueId.ChronometricValueGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public Guid EF_ChronometricValueGuid
        {
            get { return m_ParentChronometricValueId.ChronometricValueGuid; }
            set { m_ParentChronometricValueId = new ChronometricValueId(m_ParentChronometricValueId.ProjectGuid, m_ParentChronometricValueId.RevisionNumber_NonNull, value); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3)]
        public int EF_TimeDimensionType
        {
            get { return (int)m_TimeDimensionType; }
            set { m_TimeDimensionType = (TimeDimensionType)value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_TimeValueType
        {
            get
            {
                if (!m_TimeValueType.HasValue)
                { return null; }
                return (int)m_TimeValueType.Value;
            }
            set
            {
                if (!value.HasValue)
                { m_TimeValueType = null; }
                else
                { m_TimeValueType = (TimeValueType)value.Value; }
            }
        }

        [ForceMapped]
        internal Nullable<int> EF_TimePeriodType
        {
            get
            {
                if (!m_TimePeriodType.HasValue)
                { return null; }
                return (int)m_TimePeriodType.Value;
            }
            set
            {
                if (!value.HasValue)
                { m_TimePeriodType = null; }
                else
                { m_TimePeriodType = (TimePeriodType)value.Value; }
            }
        }

        [ForceMapped]
        internal Nullable<DateTime> EF_FirstPeriodStartDate
        {
            get { return m_FirstPeriodStartDate; }
            set { m_FirstPeriodStartDate = value; }
        }

        [ForceMapped]
        internal Nullable<DateTime> EF_LastPeriodEndDate
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_LastPeriodEndDate);
                return m_LastPeriodEndDate;
            }
            set
            {
                m_LastPeriodEndDate = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_LastPeriodEndDate);
            }
        }
    }
}