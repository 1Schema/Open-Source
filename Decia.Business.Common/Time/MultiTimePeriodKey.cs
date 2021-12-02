using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Time
{
    public struct MultiTimePeriodKey : IConvertible, IComparable
    {
        public const bool Default_ThrowExceptionOnError = TimeDimensionTypeUtils.Default_ThrowExceptionOnError;
        public static readonly MultiTimePeriodKey DimensionlessTimeKey = new MultiTimePeriodKey(null, null);

        private Nullable<TimePeriod> m_PrimaryTimePeriod;
        private Nullable<TimePeriod> m_SecondaryTimePeriod;

        public MultiTimePeriodKey(Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod)
        {
            if (primaryTimePeriod.HasValue && primaryTimePeriod.Value.IsForever)
            { primaryTimePeriod = null; }
            if (secondaryTimePeriod.HasValue && secondaryTimePeriod.Value.IsForever)
            { secondaryTimePeriod = null; }

            m_PrimaryTimePeriod = primaryTimePeriod;
            m_SecondaryTimePeriod = secondaryTimePeriod;
        }

        public MultiTimePeriodKey(MultiTimePeriodKey timeKey)
        {
            m_PrimaryTimePeriod = timeKey.m_PrimaryTimePeriod;
            m_SecondaryTimePeriod = timeKey.m_SecondaryTimePeriod;
        }

        public bool HasPrimaryTimePeriod
        {
            get { return m_PrimaryTimePeriod.HasValue; }
        }

        public bool HasSecondaryTimePeriod
        {
            get { return m_SecondaryTimePeriod.HasValue; }
        }

        public TimePeriod PrimaryTimePeriod
        {
            get { return m_PrimaryTimePeriod.Value; }
        }

        public TimePeriod SecondaryTimePeriod
        {
            get { return m_SecondaryTimePeriod.Value; }
        }

        public Nullable<TimePeriod> NullablePrimaryTimePeriod
        {
            get { return m_PrimaryTimePeriod; }
        }

        public Nullable<TimePeriod> NullableSecondaryTimePeriod
        {
            get { return m_SecondaryTimePeriod; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            MultiTimePeriodKey otherKey = (MultiTimePeriodKey)obj;
            bool areEqual = ((m_PrimaryTimePeriod == otherKey.m_PrimaryTimePeriod) && (m_SecondaryTimePeriod == otherKey.m_SecondaryTimePeriod));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public static readonly string HasPrimaryTimePeriod_Prefix = KeyProcessingModeUtils.GetModalDebugText("HasPrimaryTimePeriod");
        public static readonly string HasSecondaryTimePeriod_Prefix = KeyProcessingModeUtils.GetModalDebugText("HasSecondaryTimePeriod");
        public static readonly string PrimaryTimePeriod_Prefix = KeyProcessingModeUtils.GetModalDebugText("PrimaryTimePeriod");
        public static readonly string SecondaryTimePeriod_Prefix = KeyProcessingModeUtils.GetModalDebugText("SecondaryTimePeriod");

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(HasPrimaryTimePeriod_Prefix, HasPrimaryTimePeriod);
            string item2 = TypedIdUtils.StructToString(HasSecondaryTimePeriod_Prefix, HasSecondaryTimePeriod);
            string item3 = TypedIdUtils.NStructToString(PrimaryTimePeriod_Prefix, NullablePrimaryTimePeriod);
            string item4 = TypedIdUtils.NStructToString(SecondaryTimePeriod_Prefix, NullableSecondaryTimePeriod);

            string value = string.Format(ConversionUtils.FourItemListFormat, item1, item2, item3, item4);
            return value;
        }

        public static bool operator ==(MultiTimePeriodKey a, MultiTimePeriodKey b)
        { return a.Equals(b); }

        public static bool operator !=(MultiTimePeriodKey a, MultiTimePeriodKey b)
        { return !(a == b); }

        #region Methods

        public TimePeriod? GetTimePeriod(int dimensionNumber)
        {
            return GetTimePeriod(dimensionNumber, Default_ThrowExceptionOnError);
        }

        public TimePeriod? GetTimePeriod(int dimensionNumber, bool throwExceptionOnError)
        {
            var timeDimensionType = dimensionNumber.GetTimeDimensionTypeForNumber(throwExceptionOnError);
            return GetTimePeriod(timeDimensionType, throwExceptionOnError);
        }

        public TimePeriod? GetTimePeriod(TimeDimensionType dimensionType)
        {
            return GetTimePeriod(dimensionType, Default_ThrowExceptionOnError);
        }

        public TimePeriod? GetTimePeriod(TimeDimensionType dimensionType, bool throwExceptionOnError)
        {
            if (dimensionType == TimeDimensionType.Primary)
            { return m_PrimaryTimePeriod; }
            if (dimensionType == TimeDimensionType.Secondary)
            { return m_SecondaryTimePeriod; }

            if (throwExceptionOnError)
            { throw new InvalidOperationException("The requested Time Dimension Type is invalid."); }
            else
            { return null; }
        }

        #endregion

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

            MultiTimePeriodKey otherKey = (MultiTimePeriodKey)obj;
            int result = 0;

            if (!HasPrimaryTimePeriod && otherKey.HasPrimaryTimePeriod)
            { result = -1; }
            else if (!HasPrimaryTimePeriod && !otherKey.HasPrimaryTimePeriod)
            { result = 0; }
            else if (HasPrimaryTimePeriod && !otherKey.HasPrimaryTimePeriod)
            { result = 1; }
            else
            { result = PrimaryTimePeriod.CompareTo(otherKey.PrimaryTimePeriod); }

            if (result != 0)
            { return result; }

            if (!HasSecondaryTimePeriod && otherKey.HasSecondaryTimePeriod)
            { result = -1; }
            else if (!HasSecondaryTimePeriod && !otherKey.HasSecondaryTimePeriod)
            { result = 0; }
            else if (HasSecondaryTimePeriod && !otherKey.HasSecondaryTimePeriod)
            { result = 1; }
            else
            { result = SecondaryTimePeriod.CompareTo(otherKey.SecondaryTimePeriod); }

            return result;
        }

        #endregion

        #region Mathematical Operators

        public static bool operator <(MultiTimePeriodKey firstArg, MultiTimePeriodKey secondArg)
        {
            int result = firstArg.CompareTo(secondArg);
            return (result == -1);
        }

        public static bool operator <=(MultiTimePeriodKey firstArg, MultiTimePeriodKey secondArg)
        {
            int result = firstArg.CompareTo(secondArg);
            return ((result == -1) || (result == 0));
        }

        public static bool operator >(MultiTimePeriodKey firstArg, MultiTimePeriodKey secondArg)
        {
            int result = firstArg.CompareTo(secondArg);
            return (result == 1);
        }

        public static bool operator >=(MultiTimePeriodKey firstArg, MultiTimePeriodKey secondArg)
        {
            int result = firstArg.CompareTo(secondArg);
            return ((result == 0) || (result == 1));
        }

        #endregion

        public static IList<MultiTimePeriodKey> GetKeyCombinationsForPeriods(IEnumerable<TimePeriod> primaryPeriods, IEnumerable<TimePeriod> secondaryPeriods)
        {
            List<MultiTimePeriodKey> timeKeys = new List<MultiTimePeriodKey>();

            if (primaryPeriods.Count() < 1)
            {
                foreach (TimePeriod secondaryPeriod in secondaryPeriods)
                {
                    timeKeys.Add(new MultiTimePeriodKey(null, secondaryPeriod));
                }
            }
            else if (secondaryPeriods.Count() < 1)
            {
                foreach (TimePeriod primaryPeriod in primaryPeriods)
                {
                    timeKeys.Add(new MultiTimePeriodKey(primaryPeriod, null));
                }
            }
            else
            {
                foreach (TimePeriod primaryPeriod in primaryPeriods)
                {
                    foreach (TimePeriod secondaryPeriod in secondaryPeriods)
                    {
                        timeKeys.Add(new MultiTimePeriodKey(primaryPeriod, secondaryPeriod));
                    }
                }
            }
            return timeKeys;
        }
    }
}