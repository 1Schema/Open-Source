using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public class Timeframe : DomainObjectBase<Timeframe>, ITimeDimension
    {
        #region Static Members

        public static readonly Nullable<TimeValueType> NullTimeValueType = null;
        public static readonly Nullable<TimePeriodType> NullTimePeriodType = null;
        public static readonly Nullable<DateTime> NullDateTime = null;

        public const bool Default_HasTimeDimension = false;
        public const TimeValueType Default_TimeValueType = TimeframeUtils.Default_TimeValueType;
        public const TimePeriodType Default_TimePeriodType = TimeframeUtils.Default_TimePeriodType;
        public const int Default_YearSpan = TimeframeUtils.Default_YearSpan;
        public static readonly DateTime Default_FirstPeriodStartDate = TimeframeUtils.Default_FirstPeriodStartDate;
        public static readonly DateTime Default_LastPeriodEndDate = TimeframeUtils.Default_LastPeriodEndDate;

        #endregion

        #region Members

        private TimeDimensionType m_TimeDimensionType;
        private Nullable<TimeValueType> m_TimeValueType;
        private Nullable<TimePeriodType> m_TimePeriodType;
        private Nullable<DateTime> m_FirstPeriodStartDate;
        private Nullable<DateTime> m_LastPeriodEndDate;

        #endregion

        #region Constructors

        public Timeframe(TimeDimensionType timeDimensionType)
            : this(timeDimensionType, Default_HasTimeDimension)
        { }

        public Timeframe(TimeDimensionType timeDimensionType, bool hasTimeValue)
        {
            m_TimeDimensionType = timeDimensionType;
            HasTimeValue = hasTimeValue;
        }

        #endregion

        #region Properties

        public TimeDimensionType TimeDimensionType
        {
            get { return m_TimeDimensionType; }
        }

        public bool HasTimeValue
        {
            get { return m_TimeValueType.HasValue; }
            set
            {
                m_TimeValueType = (value) ? Default_TimeValueType : NullTimeValueType;
                m_TimePeriodType = (value) ? Default_TimePeriodType : NullTimePeriodType;
                m_FirstPeriodStartDate = (value) ? Default_FirstPeriodStartDate : NullDateTime;
                m_LastPeriodEndDate = (value) ? Default_LastPeriodEndDate : NullDateTime;
            }
        }

        public TimeValueType TimeValueType
        {
            get
            {
                if (!m_TimeValueType.HasValue)
                { throw new InvalidOperationException("Cannot access the TimeValueType since it does not exist."); }

                return m_TimeValueType.Value;
            }
            set
            {
                m_TimeValueType = value;

                if (m_TimeValueType == TimeValueType.PeriodValue)
                {
                    m_TimePeriodType = Default_TimePeriodType;
                    m_FirstPeriodStartDate = Default_FirstPeriodStartDate;
                    m_LastPeriodEndDate = Default_LastPeriodEndDate;
                }
                else if (m_TimeValueType == TimeValueType.SpotValue)
                {
                    m_TimePeriodType = null;
                    m_FirstPeriodStartDate = Default_FirstPeriodStartDate;
                    m_LastPeriodEndDate = Default_LastPeriodEndDate;
                }
                else
                { throw new InvalidOperationException("Unexpected TimeValueType encountered."); }

            }
        }

        public bool UsesTimePeriods
        {
            get { return (TimeValueType == TimeValueType.PeriodValue); }
        }

        public TimePeriodType TimePeriodType
        {
            get
            {
                if (!m_TimePeriodType.HasValue)
                { throw new InvalidOperationException("Cannot access the TimePeriodType since it does not exist."); }

                return m_TimePeriodType.Value;
            }
            set
            {
                m_TimePeriodType = value;

                m_FirstPeriodStartDate = Default_FirstPeriodStartDate;
                m_LastPeriodEndDate = Default_LastPeriodEndDate;
            }
        }

        public DateTime FirstPeriodStartDate
        {
            get
            {
                if (!m_FirstPeriodStartDate.HasValue)
                { throw new InvalidOperationException("Cannot access the StartDate since it does not exist."); }

                return m_FirstPeriodStartDate.Value;
            }
            set { m_FirstPeriodStartDate = value; }
        }

        public DateTime LastPeriodEndDate
        {
            get
            {
                if (!m_LastPeriodEndDate.HasValue)
                { throw new InvalidOperationException("Cannot access the EndDate since it does not exist."); }

                return m_LastPeriodEndDate.Value;
            }
            set { m_LastPeriodEndDate = value; }
        }

        public Nullable<TimeValueType> NullableTimeValueType
        {
            get { return m_TimeValueType; }
        }

        public Nullable<TimePeriodType> NullableTimePeriodType
        {
            get { return m_TimePeriodType; }
        }

        public Nullable<DateTime> NullableFirstPeriodStartDate
        {
            get { return m_FirstPeriodStartDate; }
        }

        public Nullable<DateTime> NullableLastPeriodEndDate
        {
            get { return m_LastPeriodEndDate; }
        }

        #endregion

        #region Static Methods

        public static Timeframe GetDefaultTimeframe(TimeDimensionType timeDimensionType)
        {
            return new Timeframe(timeDimensionType);
        }

        #endregion

        #region ITimeComparable<ITimeDimension> Implementation

        public TimeComparisonResult CompareTimeTo(ITimeDimension other)
        {
            return ITimeDimensionUtils.CompareTimeTo(this, other);
        }

        #endregion

        #region DomainObjectBase<Timeframe> Overrides

        public override Timeframe Copy()
        {
            Timeframe otherObject = new Timeframe(this.TimeDimensionType);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(Timeframe otherObject)
        {
            if (otherObject.m_TimeDimensionType != m_TimeDimensionType)
            { otherObject.m_TimeDimensionType = m_TimeDimensionType; }
            if (otherObject.m_TimeValueType != m_TimeValueType)
            { otherObject.m_TimeValueType = m_TimeValueType; }
            if (otherObject.m_TimePeriodType != m_TimePeriodType)
            { otherObject.m_TimePeriodType = m_TimePeriodType; }
            if (otherObject.m_FirstPeriodStartDate != m_FirstPeriodStartDate)
            { otherObject.m_FirstPeriodStartDate = m_FirstPeriodStartDate; }
            if (otherObject.m_LastPeriodEndDate != m_LastPeriodEndDate)
            { otherObject.m_LastPeriodEndDate = m_LastPeriodEndDate; }
        }

        public override bool Equals(Timeframe otherObject)
        {
            return ITimeDimensionUtils.Equals(this, (ITimeDimension)otherObject);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ITimeDimension))
            { return false; }

            var objAsTimeDimension = (ITimeDimension)obj;

            return ITimeDimensionUtils.Equals(this, objAsTimeDimension);
        }

        #endregion
    }
}