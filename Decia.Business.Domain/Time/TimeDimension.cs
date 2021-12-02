using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public class TimeDimension : ITimeDimension
    {
        #region Static Members

        public static readonly TimeDimension EmptyPrimaryTimeDimension = new TimeDimension(TimeDimensionType.Primary);
        public static readonly TimeDimension EmptySecondaryTimeDimension = new TimeDimension(TimeDimensionType.Secondary);

        #endregion

        #region Members

        protected TimeDimensionType m_TimeDimensionType;
        protected Nullable<TimeValueType> m_TimeValueType;
        protected Nullable<TimePeriodType> m_TimePeriodType;
        protected Nullable<DateTime> m_FirstPeriodStartDate;
        protected Nullable<DateTime> m_LastPeriodEndDate;

        #endregion

        #region Constructors

        public TimeDimension(TimeDimensionType timeDimensionType)
            : this(timeDimensionType, null, null, null, null)
        { }

        public TimeDimension(ITimeDimension baseTimeDimension)
            : this(baseTimeDimension, baseTimeDimension.NullableFirstPeriodStartDate, baseTimeDimension.NullableLastPeriodEndDate)
        { }

        public TimeDimension(ITimeDimension baseTimeDimension, TimePeriodType timePeriodType)
            : this(baseTimeDimension.TimeDimensionType, TimeValueType.PeriodValue, timePeriodType, baseTimeDimension.FirstPeriodStartDate, baseTimeDimension.LastPeriodEndDate)
        { }

        public TimeDimension(ITimeDimension baseTimeDimension, Nullable<DateTime> startDate, Nullable<DateTime> endDate)
            : this(baseTimeDimension.TimeDimensionType, baseTimeDimension.NullableTimeValueType, baseTimeDimension.NullableTimePeriodType, startDate, endDate)
        { }

        public TimeDimension(TimeDimensionType timeDimensionType, ITimeDimension baseTimeDimension)
            : this(timeDimensionType, baseTimeDimension.NullableTimeValueType, baseTimeDimension.NullableTimePeriodType, baseTimeDimension.NullableFirstPeriodStartDate, baseTimeDimension.NullableLastPeriodEndDate)
        { }

        public TimeDimension(TimeDimensionType timeDimensionType, Nullable<TimeValueType> timeValueType, Nullable<TimePeriodType> timePeriodType, Nullable<DateTime> firstPeriodStartDate, Nullable<DateTime> lastPeriodEndDate)
        {
            ITimeDimensionUtils.AssertTimeDimensionTypeIsValid(timeDimensionType);
            ITimeDimensionUtils.AssertSecondaryTimeDimensionIsValid(timeDimensionType, timeValueType);
            ITimeDimensionUtils.AssertTimeValueTypeIsValid(timeValueType, timePeriodType);

            m_TimeDimensionType = timeDimensionType;
            m_TimeValueType = timeValueType;
            m_TimePeriodType = timePeriodType;
            m_FirstPeriodStartDate = firstPeriodStartDate;
            m_LastPeriodEndDate = lastPeriodEndDate;
        }

        #endregion

        #region Properties

        [NotMapped]
        public TimeDimensionType TimeDimensionType
        {
            get { return m_TimeDimensionType; }
        }

        [NotMapped]
        public bool HasTimeValue
        {
            get { return m_TimeValueType.HasValue; }
        }

        [NotMapped]
        public TimeValueType TimeValueType
        {
            get { return m_TimeValueType.Value; }
        }

        [NotMapped]
        public bool UsesTimePeriods
        {
            get { return m_TimePeriodType.HasValue; }
        }

        [NotMapped]
        public TimePeriodType TimePeriodType
        {
            get { return m_TimePeriodType.Value; }
        }

        [NotMapped]
        public DateTime FirstPeriodStartDate
        {
            get { return m_FirstPeriodStartDate.Value; }
        }

        [NotMapped]
        public DateTime LastPeriodEndDate
        {
            get { return m_LastPeriodEndDate.Value; }
        }

        [NotMapped]
        public Nullable<TimeValueType> NullableTimeValueType
        {
            get { return m_TimeValueType; }
        }

        [NotMapped]
        public Nullable<TimePeriodType> NullableTimePeriodType
        {
            get { return m_TimePeriodType; }
        }

        [NotMapped]
        public Nullable<DateTime> NullableFirstPeriodStartDate
        {
            get { return m_FirstPeriodStartDate; }
        }

        [NotMapped]
        public Nullable<DateTime> NullableLastPeriodEndDate
        {
            get { return m_LastPeriodEndDate; }
        }

        #endregion

        #region Methods

        public TimeComparisonResult CompareTimeTo(ITimeDimension other)
        {
            return ITimeDimensionUtils.CompareTimeTo(this, other);
        }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ITimeDimension))
            { return false; }

            var objAsTimeDimension = (ITimeDimension)obj;

            return ITimeDimensionUtils.Equals(this, objAsTimeDimension);
        }

        public static bool operator ==(TimeDimension a, TimeDimension b)
        { return a.Equals(b); }

        public static bool operator !=(TimeDimension a, TimeDimension b)
        { return !(a == b); }

        #endregion
    }
}