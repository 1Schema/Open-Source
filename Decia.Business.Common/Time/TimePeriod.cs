using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Time
{
    public struct TimePeriod : IConvertible, IComparable
    {
        public const int DefaultIndex = 0;
        public const DateTimePrecisionType EqualityPrecisionType = DateTimePrecisionType.Second;
        public const bool DefaultPeriodStartDateIsInclusive = true;
        public const bool DefaultPeriodEndDateIsInclusive = false;
        public const bool DefaultSpotStartDateIsInclusive = true;
        public const bool DefaultSpotEndDateIsInclusive = true;

        public static readonly TimePeriod ForeverPeriod = new TimePeriod(true);

        private bool m_IsForever;
        private DateTime m_StartDate;
        private DateTime m_EndDate;
        private Nullable<int> m_Index;
        private bool m_HasBeenConverted;

        public TimePeriod(bool isForever)
        {
            if (!isForever)
            { throw new InvalidOperationException("Only call this constructor if the TimePeriod is forever."); }

            m_IsForever = true;
            m_StartDate = DeciaDataTypeUtils.MinDateTime;
            m_EndDate = DeciaDataTypeUtils.MaxDateTime;
            m_Index = null;
            m_HasBeenConverted = false;
        }

        public TimePeriod(DateTime startDate, DateTime endDate)
            : this(startDate, endDate, null)
        { }

        public TimePeriod(DateTime startDate, DateTime endDate, Nullable<int> index)
        {
            m_IsForever = false;
            m_StartDate = startDate;
            m_EndDate = endDate;
            m_Index = index;
            m_HasBeenConverted = false;
        }

        public TimePeriod(TimePeriod timePeriod)
            : this(timePeriod, false)
        { }
        public TimePeriod(TimePeriod timePeriod, bool hasBeenConverted)
        {
            m_IsForever = timePeriod.m_IsForever;
            m_StartDate = timePeriod.m_StartDate;
            m_EndDate = timePeriod.m_EndDate;
            m_Index = timePeriod.m_Index;
            m_HasBeenConverted = hasBeenConverted;
        }

        public bool IsForever
        { get { return m_IsForever; } }

        public DateTime StartDate
        { get { return m_StartDate; } }

        public DateTime EndDate
        { get { return m_EndDate; } }

        public Guid Id
        {
            get
            {
                var id = m_StartDate.ConvertDatesToGuid(m_EndDate);
                return id;
            }
        }

        public bool HasIndex
        { get { return m_Index.HasValue; } }

        public Nullable<int> NullableIndex
        { get { return m_Index; } }

        public int Index
        { get { return m_Index.Value; } }

        public bool HasBeenConverted
        { get { return m_HasBeenConverted; } }

        public TimeValueType TimeValueType
        {
            get
            {
                if (m_StartDate == m_EndDate)
                { return TimeValueType.SpotValue; }
                return TimeValueType.PeriodValue;
            }
        }

        public Nullable<TimePeriodType> InferredPeriodType
        {
            get
            {
                if (IsForever)
                { return null; }

                DateTime endDateForType = TimePeriodType.Years.GetCurrentEndDate(m_StartDate);
                if (ConversionUtils.AreEqualForPrecision(m_EndDate, endDateForType, EqualityPrecisionType))
                { return TimePeriodType.Years; }

                endDateForType = TimePeriodType.HalfYears.GetCurrentEndDate(m_StartDate);
                if (ConversionUtils.AreEqualForPrecision(m_EndDate, endDateForType, EqualityPrecisionType))
                { return TimePeriodType.HalfYears; }

                endDateForType = TimePeriodType.QuarterYears.GetCurrentEndDate(m_StartDate);
                if (ConversionUtils.AreEqualForPrecision(m_EndDate, endDateForType, EqualityPrecisionType))
                { return TimePeriodType.QuarterYears; }

                endDateForType = TimePeriodType.Months.GetCurrentEndDate(m_StartDate);
                if (ConversionUtils.AreEqualForPrecision(m_EndDate, endDateForType, EqualityPrecisionType))
                { return TimePeriodType.Months; }

                endDateForType = TimePeriodType.HalfMonths.GetCurrentEndDate(m_StartDate);
                if (ConversionUtils.AreEqualForPrecision(m_EndDate, endDateForType, EqualityPrecisionType))
                { return TimePeriodType.HalfMonths; }

                endDateForType = TimePeriodType.QuarterMonths.GetCurrentEndDate(m_StartDate);
                if (ConversionUtils.AreEqualForPrecision(m_EndDate, endDateForType, EqualityPrecisionType))
                { return TimePeriodType.QuarterMonths; }

                endDateForType = TimePeriodType.Days.GetCurrentEndDate(m_StartDate);
                if (ConversionUtils.AreEqualForPrecision(m_EndDate, endDateForType, EqualityPrecisionType))
                { return TimePeriodType.Days; }

                return null;
            }
        }

        public string TimePeriodId { get { return this.GetTimePeriodId(); } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            TimePeriod otherPeriod = (TimePeriod)obj;
            if (otherPeriod.m_IsForever != m_IsForever)
            { return false; }
            if (!ConversionUtils.AreEqualForPrecision(otherPeriod.StartDate, StartDate, EqualityPrecisionType))
            { return false; }
            if (!ConversionUtils.AreEqualForPrecision(otherPeriod.EndDate, EndDate, EqualityPrecisionType))
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public static readonly string IsForever_Prefix = KeyProcessingModeUtils.GetModalDebugText("IsForever");
        public static readonly string StartDate_Prefix = KeyProcessingModeUtils.GetModalDebugText("StartDate");
        public static readonly string EndDate_Prefix = KeyProcessingModeUtils.GetModalDebugText("EndDate");

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(IsForever_Prefix, IsForever);
            string item2 = TypedIdUtils.StructToString(StartDate_Prefix, StartDate);
            string item3 = TypedIdUtils.StructToString(EndDate_Prefix, EndDate);

            string value = string.Format(ConversionUtils.ThreeItemListFormat, item1, item2, item3);
            return value;
        }

        public string ToFriendlyName()
        {
            return TimePeriodUtils.ToFriendlyName(this, InferredPeriodType);
        }

        public static bool operator ==(TimePeriod a, TimePeriod b)
        { return a.Equals(b); }

        public static bool operator !=(TimePeriod a, TimePeriod b)
        { return !(a == b); }

        #region IConvertible Implementation

        public TypeCode GetTypeCode()
        { return TypeCode.Object; }

        public bool ToBoolean(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public byte ToByte(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public char ToChar(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public DateTime ToDateTime(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public decimal ToDecimal(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public double ToDouble(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public short ToInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public int ToInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public long ToInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public sbyte ToSByte(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public float ToSingle(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public string ToString(IFormatProvider provider)
        { return this.ToString(); }

        public object ToType(Type conversionType, IFormatProvider provider)
        { return ToString(provider); }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion

        #region IComparable Implementation

        public int CompareTo(object obj)
        {
            if (obj == null)
            { return -1; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return -1; }

            TimePeriod otherPeriod = (TimePeriod)obj;

            if (IsForever.CompareTo(otherPeriod.IsForever) != 0)
            { return IsForever.CompareTo(otherPeriod.IsForever); }

            if (StartDate < otherPeriod.StartDate)
            { return -1; }
            if (StartDate == otherPeriod.StartDate)
            {
                if (EndDate < otherPeriod.EndDate)
                { return -1; }
                if (EndDate == otherPeriod.EndDate)
                { return 0; }
            }
            return 1;
        }

        #endregion

        #region Range-Related Methods

        public bool ContainsOrEquals(TimePeriod otherPeriod)
        {
            return ContainsOrEquals(otherPeriod, DefaultPeriodStartDateIsInclusive, DefaultPeriodEndDateIsInclusive);
        }

        public bool ContainsOrEquals(TimePeriod otherPeriod, bool startDateIsInclusive, bool endDateIsInclusive)
        {
            if (this.Equals(otherPeriod))
            { return true; }

            return this.Contains(otherPeriod, startDateIsInclusive, endDateIsInclusive);
        }

        public bool Contains(DateTime otherDate)
        {
            if (this.TimeValueType == TimeValueType.PeriodValue)
            { return Contains(otherDate, DefaultPeriodStartDateIsInclusive, DefaultPeriodEndDateIsInclusive); }
            else
            { return Contains(otherDate, DefaultSpotStartDateIsInclusive, DefaultSpotEndDateIsInclusive); }
        }

        public bool Contains(DateTime otherDate, bool startDateIsInclusive, bool endDateIsInclusive)
        {
            return Contains(new TimePeriod(otherDate, otherDate), startDateIsInclusive, endDateIsInclusive);
        }

        public bool Contains(TimePeriod otherPeriod)
        {
            if (this.TimeValueType == TimeValueType.PeriodValue)
            { return Contains(otherPeriod, DefaultPeriodStartDateIsInclusive, DefaultPeriodEndDateIsInclusive); }
            else
            { return Contains(otherPeriod, DefaultSpotStartDateIsInclusive, DefaultSpotEndDateIsInclusive); }
        }

        public bool Contains(TimePeriod otherPeriod, bool startDateIsInclusive, bool endDateIsInclusive)
        {
            bool startDateIsValid = (this.StartDate < otherPeriod.StartDate);
            if (startDateIsInclusive)
            { startDateIsValid = (this.StartDate <= otherPeriod.StartDate); }

            bool endDateIsValid = (this.EndDate > otherPeriod.EndDate);
            if (endDateIsInclusive)
            { endDateIsValid = (this.EndDate >= otherPeriod.EndDate); }

            return (startDateIsValid && endDateIsValid);
        }

        public bool OverlapsOrEquals(TimePeriod otherPeriod)
        {
            if (this.Equals(otherPeriod))
            { return true; }

            return this.Overlaps(otherPeriod);
        }

        public bool Overlaps(DateTime otherDate)
        {
            if (this.TimeValueType == TimeValueType.PeriodValue)
            { return Overlaps(otherDate, DefaultPeriodStartDateIsInclusive, DefaultPeriodEndDateIsInclusive); }
            else
            { return Overlaps(otherDate, DefaultSpotStartDateIsInclusive, DefaultSpotEndDateIsInclusive); }
        }

        public bool Overlaps(DateTime otherDate, bool startDateIsInclusive, bool endDateIsInclusive)
        {
            return Overlaps(new TimePeriod(otherDate, otherDate), startDateIsInclusive, endDateIsInclusive);
        }

        public bool Overlaps(TimePeriod otherPeriod)
        {
            if (this.TimeValueType == TimeValueType.PeriodValue)
            { return Overlaps(otherPeriod, DefaultPeriodStartDateIsInclusive, DefaultPeriodEndDateIsInclusive); }
            else
            { return Overlaps(otherPeriod, DefaultSpotStartDateIsInclusive, DefaultSpotEndDateIsInclusive); }
        }

        public bool Overlaps(TimePeriod otherPeriod, bool startDateIsInclusive, bool endDateIsInclusive)
        {
            if (this.Contains(otherPeriod, startDateIsInclusive, endDateIsInclusive))
            { return true; }
            if (otherPeriod.Contains(this, startDateIsInclusive, endDateIsInclusive))
            { return true; }

            if (this.HasDisjointOverlap(otherPeriod, startDateIsInclusive, endDateIsInclusive))
            { return true; }
            if (otherPeriod.HasDisjointOverlap(this, startDateIsInclusive, endDateIsInclusive))
            { return true; }

            return false;
        }

        private bool HasDisjointOverlap(TimePeriod otherPeriod, bool startDateIsInclusive, bool endDateIsInclusive)
        {
            bool startDateIsAfterStartDate = (this.StartDate > otherPeriod.StartDate);
            if (startDateIsInclusive)
            { startDateIsAfterStartDate = (this.StartDate >= otherPeriod.StartDate); }

            bool startDateIsBeforeEndDate = (this.StartDate < otherPeriod.EndDate);
            if (startDateIsInclusive)
            { startDateIsBeforeEndDate = (this.StartDate <= otherPeriod.EndDate); }

            bool endDateIsAfterStartDate = (this.EndDate > otherPeriod.StartDate);
            if (endDateIsInclusive)
            { endDateIsAfterStartDate = (this.EndDate >= otherPeriod.StartDate); }

            bool endDateIsBeforeEndDate = (this.EndDate < otherPeriod.EndDate);
            if (endDateIsInclusive)
            { endDateIsBeforeEndDate = (this.EndDate <= otherPeriod.EndDate); }

            return ((startDateIsAfterStartDate && startDateIsBeforeEndDate) || (endDateIsAfterStartDate && endDateIsBeforeEndDate));
        }

        public int GetDistance(DateTime otherDate, TimePeriodType periodType)
        {
            return GetDistance(new TimePeriod(otherDate, otherDate), periodType);
        }

        public int GetDistance(TimePeriod otherPeriod, TimePeriodType periodType)
        {
            bool inclusiveStartDate = true;
            bool inclusiveEndDate = (otherPeriod.TimeValueType == TimeValueType.PeriodValue);

            if (Contains(otherPeriod, inclusiveStartDate, inclusiveEndDate))
            { return 0; }
            else if (EndDate <= otherPeriod.StartDate)
            {
                DateTime nextStartDate = TimePeriodTypeUtils.GetNextStartDate(periodType, StartDate);
                DateTime nextEndDate = TimePeriodTypeUtils.GetCurrentEndDate(periodType, nextStartDate);
                int count = 1;

                while (true)
                {
                    if (nextEndDate >= otherPeriod.StartDate)
                    { return (count); }
                    else
                    {
                        nextStartDate = TimePeriodTypeUtils.GetNextStartDate(periodType, nextStartDate);
                        nextEndDate = TimePeriodTypeUtils.GetCurrentEndDate(periodType, nextStartDate);
                        count++;
                    }
                }
            }
            else if (otherPeriod.EndDate <= StartDate)
            {
                DateTime previousStartDate = TimePeriodTypeUtils.GetPreviousStartDate(periodType, StartDate);
                DateTime previousEndDate = TimePeriodTypeUtils.GetCurrentEndDate(periodType, previousStartDate);
                int count = 1;

                while (true)
                {
                    if (otherPeriod.EndDate >= previousStartDate)
                    { return (-1 * count); }
                    else
                    {
                        previousStartDate = TimePeriodTypeUtils.GetPreviousStartDate(periodType, previousStartDate);
                        previousEndDate = TimePeriodTypeUtils.GetCurrentEndDate(periodType, previousStartDate);
                        count++;
                    }
                }
            }
            else
            { throw new InvalidOperationException("An unexpected Date or TimePeriodType was encountered."); }
        }

        #endregion

        #region Mathematical Operators

        public static TimePeriod operator +(TimePeriod firstArg, TimeSpan secondArg)
        {
            DateTime newStartDate = firstArg.m_StartDate.Add(secondArg);
            DateTime newEndDate = firstArg.m_EndDate.Add(secondArg);
            TimePeriod newTimePeriod = new TimePeriod(newStartDate, newEndDate);
            return newTimePeriod;
        }

        public static TimePeriod operator -(TimePeriod firstArg, TimeSpan secondArg)
        {
            DateTime newStartDate = firstArg.m_StartDate.Subtract(secondArg);
            DateTime newEndDate = firstArg.m_EndDate.Subtract(secondArg);
            TimePeriod newTimePeriod = new TimePeriod(newStartDate, newEndDate);
            return newTimePeriod;
        }

        public static bool operator <(TimePeriod firstArg, TimePeriod secondArg)
        {
            int result = firstArg.CompareTo(secondArg);
            return (result == -1);
        }

        public static bool operator <=(TimePeriod firstArg, TimePeriod secondArg)
        {
            int result = firstArg.CompareTo(secondArg);
            return ((result == -1) || (result == 0));
        }

        public static bool operator >(TimePeriod firstArg, TimePeriod secondArg)
        {
            int result = firstArg.CompareTo(secondArg);
            return (result == 1);
        }

        public static bool operator >=(TimePeriod firstArg, TimePeriod secondArg)
        {
            int result = firstArg.CompareTo(secondArg);
            return ((result == 0) || (result == 1));
        }

        #endregion
    }
}