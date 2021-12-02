using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.ChronometricValues.TimeAssessments
{
    public partial class TimeAssessment : DomainObjectBase<TimeAssessment>, IKeyedDomainObject<MultiTimePeriodKey, TimeAssessment>
    {
        public static readonly Guid DefaultTimeAssessmentGuid = Guid.Empty;
        public static readonly Nullable<DateTime> NullDateTime = null;
        public static readonly DateTime NullDateTimeStorageValue_StartDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
        public static readonly DateTime NullDateTimeStorageValue_EndDate = new DateTime(9999, 12, 31, 23, 59, 59, 997);
        public const bool DefaultIsExplicitValue = false;

        protected ChronometricValueId m_ParentChronometricValueId;
        protected Guid m_TimeAssessmentGuid;
        protected bool m_HasPrimaryDimension;
        protected DateTime m_PrimaryStartDate;
        protected DateTime m_PrimaryEndDate;
        protected bool m_HasSecondaryDimension;
        protected DateTime m_SecondaryStartDate;
        protected DateTime m_SecondaryEndDate;
        protected Func<Nullable<DeciaDataType>> m_DataTypeGetter;
        protected DynamicValue m_Value;

        public TimeAssessment()
            : this(ChronometricValueId.DefaultId, DefaultTimeAssessmentGuid, null, null, () => null)
        { }

        public TimeAssessment(ChronometricValueId parentChronometricValueId, Guid timeAssessmentId, MultiTimePeriodKey timeKey, Func<Nullable<DeciaDataType>> dataTypeGetter)
            : this(parentChronometricValueId, timeAssessmentId, timeKey.NullablePrimaryTimePeriod, timeKey.NullableSecondaryTimePeriod, dataTypeGetter)
        { }

        public TimeAssessment(ChronometricValueId parentChronometricValueId, Guid timeAssessmentId, Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod, Func<Nullable<DeciaDataType>> dataTypeGetter)
        {
            m_ParentChronometricValueId = parentChronometricValueId;
            m_TimeAssessmentGuid = timeAssessmentId;
            SetTimeKeyValues(primaryTimePeriod, secondaryTimePeriod);
            m_DataTypeGetter = dataTypeGetter;
            m_Value = new DynamicValue(DataTypeGetter, null);
        }

        protected void SetTimeKeyValues(MultiTimePeriodKey timeKey)
        {
            SetTimeKeyValues(timeKey.NullablePrimaryTimePeriod, timeKey.NullableSecondaryTimePeriod);
        }

        protected void SetTimeKeyValues(Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod)
        {
            m_HasPrimaryDimension = primaryTimePeriod.HasValue;
            m_PrimaryStartDate = (primaryTimePeriod.HasValue) ? primaryTimePeriod.Value.StartDate : NullDateTimeStorageValue_StartDate;
            m_PrimaryEndDate = (primaryTimePeriod.HasValue) ? primaryTimePeriod.Value.EndDate : NullDateTimeStorageValue_EndDate;
            m_HasSecondaryDimension = secondaryTimePeriod.HasValue;
            m_SecondaryStartDate = (secondaryTimePeriod.HasValue) ? secondaryTimePeriod.Value.StartDate : NullDateTimeStorageValue_StartDate;
            m_SecondaryEndDate = (secondaryTimePeriod.HasValue) ? secondaryTimePeriod.Value.EndDate : NullDateTimeStorageValue_EndDate;
        }

        [NotMapped]
        public ChronometricValueId ParentChronometricValueId
        {
            get { return m_ParentChronometricValueId; }
            set { m_ParentChronometricValueId = value; }
        }

        [NotMapped]
        public Guid TimeAssessmentGuid
        {
            get { return m_TimeAssessmentGuid; }
            set { m_TimeAssessmentGuid = value; }
        }

        [NotMapped]
        public MultiTimePeriodKey Key
        {
            get
            {
                Nullable<TimePeriod> primaryPeriod = null;
                Nullable<TimePeriod> secondaryPeriod = null;

                if (m_HasPrimaryDimension)
                { primaryPeriod = new TimePeriod(m_PrimaryStartDate, m_PrimaryEndDate); }
                if (m_HasSecondaryDimension)
                { secondaryPeriod = new TimePeriod(m_SecondaryStartDate, m_SecondaryEndDate); }

                MultiTimePeriodKey timeKey = new MultiTimePeriodKey(primaryPeriod, secondaryPeriod);
                return timeKey;
            }
        }

        [NotMapped]
        public Func<Nullable<DeciaDataType>> DataTypeGetter
        {
            get { return m_DataTypeGetter; }
            internal set
            {
                m_DataTypeGetter = value;
                m_Value.LoadFromStorage(DataTypeGetter, m_Value.ValueAsString, m_Value.ValueAsNumber);
            }
        }

        [NotMapped]
        public DynamicValue Value
        {
            get { return m_Value; }
        }
    }
}