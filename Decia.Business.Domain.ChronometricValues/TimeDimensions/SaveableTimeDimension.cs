using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.ChronometricValues.TimeDimensions
{
    public partial class SaveableTimeDimension : TimeDimension, IKeyedDomainObject<TimeDimensionType, SaveableTimeDimension>
    {
        public static readonly TimeDimensionType DefaultTimeDimensionType = TimeDimensionType.Primary;

        private ChronometricValueId m_ParentChronometricValueId;

        #region Constructors

        public SaveableTimeDimension()
            : this(ChronometricValueId.DefaultId, DefaultTimeDimensionType)
        { }

        public SaveableTimeDimension(ChronometricValueId parentChronometricValueId, TimeDimensionType timeDimensionType)
            : this(parentChronometricValueId, timeDimensionType, null, null, null, null)
        { }

        public SaveableTimeDimension(ChronometricValueId parentChronometricValueId, ITimeDimension baseTimeDimension)
            : this(parentChronometricValueId, baseTimeDimension.TimeDimensionType, baseTimeDimension.NullableTimeValueType, baseTimeDimension.NullableTimePeriodType, baseTimeDimension.NullableFirstPeriodStartDate, baseTimeDimension.NullableLastPeriodEndDate)
        { }

        public SaveableTimeDimension(ChronometricValueId parentChronometricValueId, TimeDimensionType timeDimensionType, ITimeDimension baseTimeDimension)
            : this(parentChronometricValueId, timeDimensionType, baseTimeDimension.NullableTimeValueType, baseTimeDimension.NullableTimePeriodType, baseTimeDimension.NullableFirstPeriodStartDate, baseTimeDimension.NullableLastPeriodEndDate)
        { }

        public SaveableTimeDimension(ChronometricValueId parentChronometricValueId, TimeDimensionType timeDimensionType, Nullable<TimeValueType> timeValueType, Nullable<TimePeriodType> timePeriodType, Nullable<DateTime> firstPeriodStartDate, Nullable<DateTime> lastPeriodEndDate)
            : base(timeDimensionType, timeValueType, timePeriodType, firstPeriodStartDate, lastPeriodEndDate)
        {
            m_ParentChronometricValueId = parentChronometricValueId;
        }

        #endregion

        #region Properties

        [NotMapped]
        public TimeDimensionType Key
        {
            get { return m_TimeDimensionType; }
        }

        [NotMapped]
        public ChronometricValueId ParentChronometricValueId
        {
            get { return m_ParentChronometricValueId; }
            internal set { m_ParentChronometricValueId = value; }
        }

        #endregion

        #region Methods

        public TimeComparisonResult CompareTimeTo(ITimeDimension other)
        {
            return ITimeDimensionUtils.CompareTimeTo(this, other);
        }

        #endregion
    }
}