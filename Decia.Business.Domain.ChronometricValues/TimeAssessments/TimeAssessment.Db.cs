using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Testing;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.ChronometricValues.TimeAssessments
{
    public partial class TimeAssessment : IEfStorableObject
    {
        #region Entity Framework Mapper

        public class TimeAssessment_Mapper : EntityTypeConfiguration<TimeAssessment>
        {
            public TimeAssessment_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ChronometricValueGuid);
                Property(p => p.EF_TimeAssessmentGuid);
                Property(p => p.EF_HasPrimaryTimeDimension);
                Property(p => p.EF_PrimaryStartDate);
                Property(p => p.EF_PrimaryEndDate);
                Property(p => p.EF_HasSecondaryTimeDimension);
                Property(p => p.EF_SecondaryStartDate);
                Property(p => p.EF_SecondaryEndDate);
                Property(p => p.EF_DataType);
                Property(p => p.EF_NumberValue);
                Property(p => p.EF_TextValue);
            }
        }

        #endregion

        public object[] EF_CombinedId
        {
            get
            {
                object[] combined = new object[] { EF_ProjectGuid, EF_RevisionNumber, EF_ChronometricValueGuid, EF_TimeAssessmentGuid };
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
        public Guid EF_TimeAssessmentGuid
        {
            get { return m_TimeAssessmentGuid; }
            set { m_TimeAssessmentGuid = value; }
        }

        [ForceMapped]
        internal bool EF_HasPrimaryTimeDimension
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_HasPrimaryDimension);
                return m_HasPrimaryDimension;
            }
            set
            {
                m_HasPrimaryDimension = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_HasPrimaryDimension);
            }
        }

        [ForceMapped]
        internal DateTime EF_PrimaryStartDate
        {
            get { return m_PrimaryStartDate; }
            set { m_PrimaryStartDate = value; }
        }

        [ForceMapped]
        internal DateTime EF_PrimaryEndDate
        {
            get { return m_PrimaryEndDate; }
            set { m_PrimaryEndDate = value; }
        }

        [ForceMapped]
        internal bool EF_HasSecondaryTimeDimension
        {
            get { return m_HasSecondaryDimension; }
            set { m_HasSecondaryDimension = value; }
        }

        [ForceMapped]
        internal DateTime EF_SecondaryStartDate
        {
            get { return m_SecondaryStartDate; }
            set { m_SecondaryStartDate = value; }
        }

        [ForceMapped]
        internal DateTime EF_SecondaryEndDate
        {
            get { return m_SecondaryEndDate; }
            set { m_SecondaryEndDate = value; }
        }

        private Nullable<int> m_EF_DataType = null;
        [ForceMapped]
        internal int EF_DataType
        {
            get { return (int)m_Value.DataType; }
            set { m_EF_DataType = value; }
        }

        private Nullable<double> m_EF_NumberValue = null;
        [ForceMapped]
        internal Nullable<double> EF_NumberValue
        {
            get { return m_Value.ValueAsNumber; }
            set
            {
                m_EF_NumberValue = value;
                InitializeDynamicValue();
            }
        }

        private string m_EF_TextValue = null;
        [ForceMapped]
        internal string EF_TextValue
        {
            get { return m_Value.ValueAsString; }
            set
            {
                m_EF_TextValue = value;
                InitializeDynamicValue();
            }
        }

        private void InitializeDynamicValue()
        {
            if (m_DataTypeGetter != null)
            { m_Value.LoadFromStorage(m_DataTypeGetter, m_EF_TextValue, m_EF_NumberValue); }
            else
            { m_Value.LoadFromStorage((DeciaDataType)m_EF_DataType.Value, m_EF_TextValue, m_EF_NumberValue); }
        }

        #region IPartialReadable State Properties

        private bool m_IsPartialObject = false;

        [NotMapped]
        internal bool IsPartialObject
        {
            get { return m_IsPartialObject; }
            set { m_IsPartialObject = value; }
        }

        #endregion
    }
}