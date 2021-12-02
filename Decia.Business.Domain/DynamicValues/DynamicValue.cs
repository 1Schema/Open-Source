using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;

namespace Decia.Business.Domain.DynamicValues
{
    public class DynamicValue : IComparable
    {
        #region Static Members

        public static readonly string DataType_PropName = "Data Type";
        public static readonly string Value_PropName = "Value";

        public static readonly string DataType_Prefix = KeyProcessingModeUtils.GetModalDebugText(DataType_PropName, "T");
        public static readonly string Value_Prefix = KeyProcessingModeUtils.GetModalDebugText(Value_PropName, "V");

        public const DynamicValue NullInstance = null;
        public const object NullInstanceAsObject = null;
        public const DeciaDataType DefaultDataType = DeciaDataType.Decimal;
        internal const string NullInternalValue = null;

        #endregion

        #region Members

        private TypeProvisionMode m_TypeProvisionMode;
        private Func<Nullable<DeciaDataType>> m_DataTypeGetter;
        private DeciaDataType m_DataType = DefaultDataType;
        private string m_StringValue;
        private Nullable<double> m_NumberValue;

        #endregion

        #region Constructors

        public DynamicValue(Func<Nullable<DeciaDataType>> dataTypeGetter)
            : this(dataTypeGetter, DefaultDataType, true, null)
        { }

        public DynamicValue(Func<Nullable<DeciaDataType>> dataTypeGetter, object value)
            : this(dataTypeGetter, DefaultDataType, false, value)
        { }

        public DynamicValue(Func<Nullable<DeciaDataType>> requiredDataTypeGetter, DeciaDataType defaultDataType, object value)
            : this(requiredDataTypeGetter, defaultDataType, false, value)
        { }

        protected DynamicValue(Func<Nullable<DeciaDataType>> requiredDataTypeGetter, DeciaDataType defaultDataType, bool useDefaultForDataType, object value)
        {
            m_TypeProvisionMode = TypeProvisionMode.External;
            m_DataTypeGetter = requiredDataTypeGetter;
            m_DataType = defaultDataType;

            if (DataType != defaultDataType)
            { m_DataType = DataType; }

            if (useDefaultForDataType)
            { SetValue(m_DataType, m_DataType.GetDefaultForDataType()); }
            else
            { SetValue(m_DataType, value); }
        }

        public DynamicValue(DeciaDataType dataType)
            : this(dataType, dataType.GetDefaultForDataType())
        { }

        public DynamicValue(DeciaDataType dataType, object value)
        {
            m_TypeProvisionMode = TypeProvisionMode.Internal;
            m_DataTypeGetter = null;
            m_DataType = dataType;
            SetValue(m_DataType, value);
        }

        #endregion

        #region Properties & Methods

        public TypeProvisionMode TypeProvisionMode
        {
            get { return m_TypeProvisionMode; }
        }

        public Func<Nullable<DeciaDataType>> DataTypeGetter
        {
            get { return m_DataTypeGetter; }
        }

        public DeciaDataType DataType
        {
            get
            {
                if (TypeProvisionMode == TypeProvisionMode.External)
                {
                    if (m_DataTypeGetter == null)
                    { return m_DataType; }
                    if (!m_DataTypeGetter().HasValue)
                    { return m_DataType; }

                    m_DataType = m_DataTypeGetter().Value;
                    return m_DataType;
                }
                else if (TypeProvisionMode == TypeProvisionMode.Internal)
                {
                    return m_DataType;
                }
                else
                { throw new InvalidOperationException("Unrecognized TypeProvisionMode encountered."); }
            }
        }

        public bool IsValid
        {
            get
            {
                object typedValue = TryToGetTypedValue(DataType);
                return (!(typedValue is Exception));
            }
        }

        public bool IsNull
        {
            get { return (m_StringValue == NullInternalValue); }
        }

        public string ValueAsString
        {
            get { return m_StringValue; }
        }

        public Nullable<double> ValueAsNumber
        {
            get { return m_NumberValue; ; }
        }

        public void DataTypeChangeHandler(object sender, EventArgs args)
        {
            if (TypeProvisionMode == TypeProvisionMode.Internal)
            { return; }
            if (DataTypeGetter == null)
            { return; }
            if (!DataTypeGetter().HasValue)
            { return; }

            if (DataType != m_DataType)
            {
                m_DataType = DataType;
                ResetToDefault();
            }
        }

        public void ResetToDefault()
        {
            SetValue(DataType, DataType.GetDefaultForDataType());
        }

        public void SetToNull()
        {
            SetValue(DataType, null);
        }

        public T GetTypedValue<T>()
        {
            var incomingValue = GetValue();
            var resultingValue = (object)null;
            var errorMessage = string.Empty;

            var success = DeciaDataTypeUtils.TryToChangeValue<T>(incomingValue, out resultingValue, out errorMessage);

            if (!success)
            { throw new InvalidOperationException(errorMessage); }
            return (T)resultingValue;
        }

        public object GetValue()
        {
            object value = TryToGetTypedValue(DataType);

            if (value is Exception)
            { throw (value as Exception); }
            return value;
        }

        public object TryToGetTypedValue(DeciaDataType dataType)
        {
            bool success = false;

            if (m_StringValue == NullInternalValue)
            { return null; }
            if (!((m_DataType == DeciaDataType.UniqueID)
                || (m_DataType == DeciaDataType.Text)))
            {
                if (m_NumberValue == null)
                { return null; }
            }

            if (dataType == DeciaDataType.Boolean)
            {
                bool numberValueAsBool = (m_NumberValue != 0);
                return numberValueAsBool;
            }
            else if (dataType == DeciaDataType.Integer)
            {
                int numberValueAsInt = (int)m_NumberValue;
                return numberValueAsInt;
            }
            else if (dataType == DeciaDataType.Decimal)
            {
                return m_NumberValue;
            }
            else if (dataType == DeciaDataType.UniqueID)
            {
                var isNull = string.IsNullOrWhiteSpace(m_StringValue);
                if (isNull)
                { return null; }

                Guid parseResult;
                if (!Guid.TryParse(m_StringValue, out parseResult))
                { return new InvalidOperationException("The current value does not match the requested DataType."); }

                return parseResult;
            }
            else if (dataType == DeciaDataType.DateTime)
            {
                DateTime numberValueAsDate = DateTime.FromOADate(m_NumberValue.Value);
                return numberValueAsDate;
            }
            else if (dataType == DeciaDataType.TimeSpan)
            {
                long numberValueAsLong = (long)m_NumberValue;
                TimeSpan numberValueAsTimeSpan = TimeSpan.FromTicks(numberValueAsLong);
                return numberValueAsTimeSpan;
            }
            else if (dataType == DeciaDataType.Text)
            { return m_StringValue; }

            return new InvalidOperationException("The requested DataType is not supported.");
        }

        public void SetValue(object value)
        {
            SetValue(DataType, value);
        }

        public void SetTypedValue<T>(T value)
        {
            SetValue(typeof(T), value);
        }

        public void SetValue(Type type, object value)
        {
            DeciaDataType dataType = type.GetDataTypeForSystemType();
            SetValue(dataType, value);
        }

        public void SetValue(DeciaDataType dataType, object value)
        {
            if ((TypeProvisionMode == TypeProvisionMode.External) && (m_DataTypeGetter().HasValue))
            {
                if (DataType != dataType)
                { throw new InvalidOperationException("The current BaseValue's DataType is set externally."); }
            }
            if (value == null)
            {
                m_DataType = dataType;
                m_StringValue = NullInternalValue;
                m_NumberValue = null;
                return;
            }

            string valueAsString = null;
            Nullable<double> valueAsNumber = null;

            if (dataType == DeciaDataType.Boolean)
            {
                if (value is bool)
                {
                    bool valueAsBool = (bool)value;
                    valueAsNumber = valueAsBool ? 1 : 0;
                }
                else
                { valueAsNumber = (double)value; }
                valueAsString = valueAsNumber.ToString();
            }
            else if ((dataType == DeciaDataType.Integer)
                || (dataType == DeciaDataType.Decimal))
            {
                if (value is int)
                {
                    int valueAsInt = (int)value;
                    valueAsNumber = (double)valueAsInt;
                }
                else if (value is long)
                {
                    int valueAsInt = (int)((long)value);
                    valueAsNumber = (double)valueAsInt;
                }
                else
                { valueAsNumber = (double)value; }
                valueAsString = valueAsNumber.ToString();
            }
            else if (dataType == DeciaDataType.UniqueID)
            {
                valueAsNumber = null;
                valueAsString = value.ToString();
            }
            else if (dataType == DeciaDataType.DateTime)
            {
                valueAsNumber = ((DateTime)value).ToOADate();
                valueAsString = valueAsNumber.ToString();
            }
            else if (dataType == DeciaDataType.TimeSpan)
            {
                valueAsNumber = ((TimeSpan)value).Ticks;
                valueAsString = valueAsNumber.ToString();
            }
            else if (dataType == DeciaDataType.Text)
            {
                valueAsNumber = null;
                valueAsString = value.ToString();
            }
            else
            { throw new InvalidOperationException("The requested type is not supported."); }

            m_DataType = dataType;
            m_StringValue = valueAsString;
            m_NumberValue = valueAsNumber;
        }

        public void SetToOtherValue(DynamicValue value)
        {
            SetValue(value.DataType, value.GetValue());
        }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            DynamicValue otherValue = (DynamicValue)obj;

            if (otherValue.m_TypeProvisionMode != m_TypeProvisionMode)
            { return false; }
            if (otherValue.m_DataType != m_DataType)
            { return false; }
            if (otherValue.DataType != DataType)
            { return false; }
            if ((m_StringValue == null) ^ (otherValue.m_StringValue == null))
            { return false; }
            if (otherValue.m_StringValue != null)
            {
                if (!otherValue.m_StringValue.Equals(m_StringValue))
                { return false; }
            }
            if ((m_NumberValue == null) ^ (otherValue.m_NumberValue == null))
            { return false; }
            if (otherValue.m_NumberValue != null)
            {
                if (!otherValue.m_NumberValue.Equals(m_NumberValue))
                { return false; }
            }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = TypedIdUtils.ConvertToString(DataType_Prefix, DataType);
            string item2 = TypedIdUtils.ConvertToString(Value_Prefix, (ValueAsString != null) ? ValueAsString : string.Empty);
            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        #endregion

        #region Persistence Methods

        public DynamicValue Copy()
        {
            if (TypeProvisionMode == TypeProvisionMode.External)
            { return new DynamicValue(m_DataTypeGetter, m_DataType, this.GetValue()); }
            else if (TypeProvisionMode == TypeProvisionMode.Internal)
            { return new DynamicValue(m_DataType, this.GetValue()); }
            else
            { throw new InvalidOperationException("Unrecognized TypeProvisionMode encountered."); }
        }

        public string SaveAsString()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add(DataType_PropName, ((int)DataType).ToString());
            values.Add(Value_PropName, ValueAsString);

            string saveValue = values.ConvertToDictionaryAsString();
            return saveValue;
        }

        public void LoadFromStorage(string storedTypeAndValue)
        {
            IDictionary<string, string> values = storedTypeAndValue.ConvertToTypedDictionary<string, string>();

            m_DataType = (DeciaDataType)int.Parse(values[DataType_PropName]);
            m_StringValue = values[Value_PropName];
            DynamicValueUtils.TryToConvertToNumber(m_DataType, m_StringValue, out m_NumberValue);
        }

        public void LoadFromStorage(DeciaDataType dataType, string stringValue, Nullable<double> numberValue)
        {
            m_TypeProvisionMode = TypeProvisionMode.Internal;
            m_DataTypeGetter = null;
            m_DataType = dataType;
            m_StringValue = stringValue;
            m_NumberValue = numberValue;
        }

        public void LoadFromStorage(Func<Nullable<DeciaDataType>> dataTypeGetter, string stringValue, Nullable<double> numberValue)
        {
            m_TypeProvisionMode = (dataTypeGetter != null) ? TypeProvisionMode.External : TypeProvisionMode.Internal;
            m_DataTypeGetter = dataTypeGetter;
            m_DataType = DataType;
            m_StringValue = stringValue;
            m_NumberValue = numberValue;
        }

        public static Nullable<DeciaDataType> ConvertNonNullGetter(Func<DeciaDataType> getter)
        {
            return getter();
        }

        #endregion

        #region IComparable Implementation

        public int CompareTo(object obj)
        {
            if (obj == null)
            { return -1; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return -1; }

            DynamicValue otherValue = (DynamicValue)obj;

            if (this.IsNull || otherValue.IsNull)
            {
                return this.IsNull.CompareTo(otherValue.IsNull);
            }

            if (this.DataType != otherValue.DataType)
            {
                int thisDataTypeAsInt = (int)this.DataType;
                int otherDataTypeAsInt = (int)otherValue.DataType;
                return thisDataTypeAsInt.CompareTo(otherDataTypeAsInt);
            }

            if ((this.DataType == DeciaDataType.Text)
                || (this.DataType == DeciaDataType.UniqueID))
            {
                string thisAsString = this.GetValue().ToString();
                string otherAsString = otherValue.GetValue().ToString();

                if (thisAsString == otherAsString)
                { return 0; }
                else
                {
                    int maxCount = thisAsString.Count();
                    if (otherAsString.Count() > maxCount)
                    { maxCount = otherAsString.Count(); }

                    for (int i = 0; i < maxCount; i++)
                    {
                        if (thisAsString.Count() < (i + 1))
                        { return -1; }
                        if (otherAsString.Count() < (i + 1))
                        { return 1; }

                        char thisChar = thisAsString[i];
                        char otherChar = otherAsString[i];

                        if (thisChar < otherChar)
                        { return -1; }
                        if (thisChar > otherChar)
                        { return 1; }
                    }
                    return 0;
                }
            }
            else if (this.DataType == DeciaDataType.DateTime)
            {
                DateTime thisAsDateTime = this.GetTypedValue<DateTime>();
                DateTime otherAsDateTime = otherValue.GetTypedValue<DateTime>();

                if (thisAsDateTime < otherAsDateTime)
                { return -1; }
                else if (thisAsDateTime == otherAsDateTime)
                { return 0; }
                else
                { return 1; }
            }
            else if (this.DataType == DeciaDataType.TimeSpan)
            {
                TimeSpan thisAsTimeSpan = this.GetTypedValue<TimeSpan>();
                TimeSpan otherAsTimeSpan = otherValue.GetTypedValue<TimeSpan>();

                if (thisAsTimeSpan < otherAsTimeSpan)
                { return -1; }
                else if (thisAsTimeSpan == otherAsTimeSpan)
                { return 0; }
                else
                { return 1; }
            }
            else if (this.DataType == DeciaDataType.Decimal)
            {
                double thisAsDouble = this.GetTypedValue<double>();
                double otherAsDouble = otherValue.GetTypedValue<double>();

                if (thisAsDouble < otherAsDouble)
                { return -1; }
                else if (thisAsDouble == otherAsDouble)
                { return 0; }
                else
                { return 1; }
            }
            else if (this.DataType == DeciaDataType.Integer)
            {
                int thisAsInt = this.GetTypedValue<int>();
                int otherAsInt = otherValue.GetTypedValue<int>();

                if (thisAsInt < otherAsInt)
                { return -1; }
                else if (thisAsInt == otherAsInt)
                { return 0; }
                else
                { return 1; }
            }
            else if (this.DataType == DeciaDataType.Boolean)
            {
                bool thisAsBool = this.GetTypedValue<bool>();
                bool otherAsBool = otherValue.GetTypedValue<bool>();

                if (!thisAsBool && otherAsBool)
                { return -1; }
                else if (thisAsBool == otherAsBool)
                { return 0; }
                else
                { return 1; }
            }
            else
            { throw new InvalidOperationException("The requested type is not supported."); }
        }

        #endregion

        #region Basic Equivalence Operators

        public static bool operator ==(DynamicValue firstArg, object secondArg)
        {
            if (((object)firstArg) == null)
            { return (secondArg == null); }

            return (firstArg.Equals(secondArg));
        }

        public static bool operator !=(DynamicValue firstArg, object secondArg)
        {
            return (!(firstArg == secondArg));
        }

        #endregion

        #region Logical Operators

        public static DynamicValue operator !(DynamicValue firstArg)
        {
            if ((firstArg.DataType == DeciaDataType.Text)
                || (firstArg.DataType == DeciaDataType.DateTime)
                || (firstArg.DataType == DeciaDataType.TimeSpan)
                || (firstArg.DataType == DeciaDataType.UniqueID)
                || (firstArg.DataType == DeciaDataType.Decimal)
                || (firstArg.DataType == DeciaDataType.Integer)
                || (firstArg.DataType == DeciaDataType.Boolean))
            {
                if (firstArg.IsNull)
                { return new DynamicValue(DeciaDataType.Boolean, null); }

                bool firstBool = (bool)firstArg.GetValue();
                return new DynamicValue(DeciaDataType.Boolean, !firstBool);
            }
            else
            { throw new InvalidOperationException("The requested type is not supported."); }
        }

        public static DynamicValue operator ==(DynamicValue firstArg, DynamicValue secondArg)
        {
            bool result = firstArg.Equals(secondArg);
            return new DynamicValue(DeciaDataType.Boolean, result);
        }

        public static DynamicValue operator !=(DynamicValue firstArg, DynamicValue secondArg)
        {
            bool result = !(firstArg.Equals(secondArg));
            return new DynamicValue(DeciaDataType.Boolean, result);
        }

        public static DynamicValue operator <(DynamicValue firstArg, DynamicValue secondArg)
        {
            int resultAsInt = firstArg.CompareTo(secondArg);
            bool resultAsBool = (resultAsInt == -1);
            return new DynamicValue(DeciaDataType.Boolean, resultAsBool);
        }

        public static DynamicValue operator <=(DynamicValue firstArg, DynamicValue secondArg)
        {
            int resultAsInt = firstArg.CompareTo(secondArg);
            bool resultAsBool = ((resultAsInt == -1) || (resultAsInt == 0));
            return new DynamicValue(DeciaDataType.Boolean, resultAsBool);
        }

        public static DynamicValue operator >(DynamicValue firstArg, DynamicValue secondArg)
        {
            int resultAsInt = firstArg.CompareTo(secondArg);
            bool resultAsBool = (resultAsInt == 1);
            return new DynamicValue(DeciaDataType.Boolean, resultAsBool);
        }

        public static DynamicValue operator >=(DynamicValue firstArg, DynamicValue secondArg)
        {
            int resultAsInt = firstArg.CompareTo(secondArg);
            bool resultAsBool = ((resultAsInt == 0) || (resultAsInt == 1));
            return new DynamicValue(DeciaDataType.Boolean, resultAsBool);
        }

        public static DynamicValue operator &(DynamicValue firstArg, DynamicValue secondArg)
        {
            if (firstArg.IsNull || secondArg.IsNull)
            { return new DynamicValue(DeciaDataType.Boolean, null); }

            object firstObject = firstArg.GetValue();
            object secondObject = secondArg.GetValue();

            bool firstBool = (firstObject is bool) ? (bool)firstObject : Convert.ToBoolean(firstObject);
            bool secondBool = (secondObject is bool) ? (bool)secondObject : Convert.ToBoolean(secondObject);
            bool outputBool = firstBool && secondBool;

            return new DynamicValue(DeciaDataType.Boolean, outputBool);
        }

        public static DynamicValue operator |(DynamicValue firstArg, DynamicValue secondArg)
        {
            if (firstArg.IsNull || secondArg.IsNull)
            { return new DynamicValue(DeciaDataType.Boolean, null); }

            object firstObject = firstArg.GetValue();
            object secondObject = secondArg.GetValue();

            bool firstBool = (firstObject is bool) ? (bool)firstObject : Convert.ToBoolean(firstObject);
            bool secondBool = (secondObject is bool) ? (bool)secondObject : Convert.ToBoolean(secondObject);
            bool outputBool = firstBool || secondBool;

            return new DynamicValue(DeciaDataType.Boolean, outputBool);
        }

        public static DynamicValue operator ^(DynamicValue firstArg, DynamicValue secondArg)
        {
            if (firstArg.IsNull || secondArg.IsNull)
            { return new DynamicValue(DeciaDataType.Boolean, null); }

            object firstObject = firstArg.GetValue();
            object secondObject = secondArg.GetValue();

            bool firstBool = (firstObject is bool) ? (bool)firstObject : Convert.ToBoolean(firstObject);
            bool secondBool = (secondObject is bool) ? (bool)secondObject : Convert.ToBoolean(secondObject);
            bool outputBool = firstBool ^ secondBool;

            return new DynamicValue(DeciaDataType.Boolean, outputBool);
        }

        #endregion

        #region Mathematical Operators

        public static DynamicValue operator -(DynamicValue firstArg)
        {
            if (firstArg.DataType == DeciaDataType.Text)
            {
                if (firstArg.IsNull)
                { return new DynamicValue(DeciaDataType.Text, null); }

                string firstString = firstArg.GetValue().ToString();
                return new DynamicValue(DeciaDataType.Text, firstString.Reverse().ToString());
            }
            else if (firstArg.DataType == DeciaDataType.DateTime)
            {
                throw new InvalidOperationException("Cannot use - operator.");
            }
            else if (firstArg.DataType == DeciaDataType.TimeSpan)
            {
                if (firstArg.IsNull)
                { return new DynamicValue(DeciaDataType.TimeSpan, null); }

                TimeSpan firstTimeSpan = firstArg.GetTypedValue<TimeSpan>();
                return new DynamicValue(DeciaDataType.TimeSpan, firstTimeSpan.Negate());
            }
            else if (firstArg.DataType == DeciaDataType.Decimal)
            {
                if (firstArg.IsNull)
                { return new DynamicValue(DeciaDataType.Decimal, null); }

                double firstDouble = firstArg.GetTypedValue<double>();
                return new DynamicValue(DeciaDataType.Decimal, (firstDouble * -1.0));
            }
            else if (firstArg.DataType == DeciaDataType.Integer)
            {
                if (firstArg.IsNull)
                { return new DynamicValue(DeciaDataType.Integer, null); }

                int firstInt = firstArg.GetTypedValue<int>();
                return new DynamicValue(DeciaDataType.Integer, (firstInt * -1));
            }
            else if (firstArg.DataType == DeciaDataType.Boolean)
            {
                if (firstArg.IsNull)
                { return new DynamicValue(DeciaDataType.Boolean, null); }

                bool firstBool = firstArg.GetTypedValue<bool>();
                return new DynamicValue(DeciaDataType.Boolean, !firstBool);
            }
            else
            { throw new InvalidOperationException("The requested type is not supported."); }
        }

        public static DynamicValue operator +(DynamicValue firstArg, DynamicValue secondArg)
        {
            if ((firstArg.DataType == DeciaDataType.Text)
                || (secondArg.DataType == DeciaDataType.Text))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Text, null); }

                string firstString = firstArg.GetValue().ToString();
                string secondString = secondArg.GetValue().ToString();
                string newString = firstString + secondString;
                return new DynamicValue(DeciaDataType.Text, newString);
            }
            else if (firstArg.DataType == DeciaDataType.DateTime)
            {
                if (secondArg.DataType != DeciaDataType.TimeSpan)
                { throw new InvalidOperationException("Cannot use + operator."); }
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.DateTime, null); }

                DateTime firstDateTime = firstArg.GetTypedValue<DateTime>();
                TimeSpan secondTimeSpan = secondArg.GetTypedValue<TimeSpan>();
                DateTime newDateTime = firstDateTime.Add(secondTimeSpan);
                return new DynamicValue(DeciaDataType.DateTime, newDateTime);
            }
            else if (firstArg.DataType == DeciaDataType.TimeSpan)
            {
                if (secondArg.DataType != DeciaDataType.TimeSpan)
                { throw new InvalidOperationException("Cannot use + operator."); }
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.TimeSpan, null); }

                TimeSpan firstTimeSpan = firstArg.GetTypedValue<TimeSpan>();
                TimeSpan secondTimeSpan = secondArg.GetTypedValue<TimeSpan>();
                TimeSpan newTimeSpan = firstTimeSpan.Add(secondTimeSpan);
                return new DynamicValue(DeciaDataType.TimeSpan, newTimeSpan);
            }
            else if ((firstArg.DataType == DeciaDataType.Decimal)
                || (secondArg.DataType == DeciaDataType.Decimal))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Decimal, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstDecimal = (firstObject is double) ? (double)firstObject : Convert.ToDouble(firstObject);
                double secondDecimal = (secondObject is double) ? (double)secondObject : Convert.ToDouble(secondObject);
                double newDouble = firstDecimal + secondDecimal;

                return new DynamicValue(DeciaDataType.Decimal, newDouble);
            }
            else if ((firstArg.DataType == DeciaDataType.Integer)
                || (secondArg.DataType == DeciaDataType.Integer))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Integer, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstInt = (firstObject is int) ? (int)firstObject : Convert.ToInt32(firstObject);
                double secondInt = (secondObject is int) ? (int)secondObject : Convert.ToInt32(secondObject);
                double newInt = firstInt + secondInt;

                return new DynamicValue(DeciaDataType.Integer, newInt);
            }
            else if (firstArg.DataType == DeciaDataType.Boolean)
            {
                if (secondArg.DataType != DeciaDataType.Boolean)
                { throw new InvalidOperationException("Cannot use + operator."); }
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Boolean, null); }

                bool firstBool = firstArg.GetTypedValue<bool>();
                bool secondBool = secondArg.GetTypedValue<bool>();
                bool newBool = (firstBool || secondBool);
                return new DynamicValue(DeciaDataType.Boolean, newBool);
            }
            { throw new InvalidOperationException("The requested type is not supported."); }
        }

        public static DynamicValue operator -(DynamicValue firstArg, DynamicValue secondArg)
        {
            if (firstArg.DataType == DeciaDataType.Text)
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Text, null); }

                string firstString = firstArg.GetValue().ToString();
                string secondString = secondArg.GetValue().ToString();
                string newString = firstString.Replace(secondString, string.Empty);
                return new DynamicValue(DeciaDataType.Text, newString);
            }
            else if (firstArg.DataType == DeciaDataType.DateTime)
            {
                if (secondArg.DataType != DeciaDataType.TimeSpan)
                { throw new InvalidOperationException("Cannot use - operator."); }
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.DateTime, null); }

                DateTime firstDateTime = firstArg.GetTypedValue<DateTime>();
                TimeSpan secondTimeSpan = secondArg.GetTypedValue<TimeSpan>();
                DateTime newDateTime = firstDateTime.Subtract(secondTimeSpan);
                return new DynamicValue(DeciaDataType.DateTime, newDateTime);
            }
            else if (firstArg.DataType == DeciaDataType.TimeSpan)
            {
                if (secondArg.DataType != DeciaDataType.TimeSpan)
                { throw new InvalidOperationException("Cannot use - operator."); }
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.TimeSpan, null); }

                TimeSpan firstTimeSpan = firstArg.GetTypedValue<TimeSpan>();
                TimeSpan secondTimeSpan = secondArg.GetTypedValue<TimeSpan>();
                TimeSpan newTimeSpan = firstTimeSpan.Subtract(secondTimeSpan);
                return new DynamicValue(DeciaDataType.TimeSpan, newTimeSpan);
            }
            else if ((firstArg.DataType == DeciaDataType.Decimal)
                || (secondArg.DataType == DeciaDataType.Decimal))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Decimal, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstDecimal = (firstObject is double) ? (double)firstObject : Convert.ToDouble(firstObject);
                double secondDecimal = (secondObject is double) ? (double)secondObject : Convert.ToDouble(secondObject);
                double newDouble = firstDecimal - secondDecimal;

                return new DynamicValue(DeciaDataType.Decimal, newDouble);

            }
            else if ((firstArg.DataType == DeciaDataType.Integer)
                || (secondArg.DataType == DeciaDataType.Integer))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Integer, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstInt = (firstObject is int) ? (int)firstObject : Convert.ToInt32(firstObject);
                double secondInt = (secondObject is int) ? (int)secondObject : Convert.ToInt32(secondObject);
                double newInt = firstInt - secondInt;

                return new DynamicValue(DeciaDataType.Integer, newInt);
            }
            else if (firstArg.DataType == DeciaDataType.Boolean)
            {
                if (secondArg.DataType != DeciaDataType.Boolean)
                { throw new InvalidOperationException("Cannot use - operator."); }
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Boolean, null); }

                bool firstBool = firstArg.GetTypedValue<bool>();
                bool secondBool = secondArg.GetTypedValue<bool>();
                bool newBool = (firstBool && !secondBool);
                return new DynamicValue(DeciaDataType.Boolean, newBool);
            }
            { throw new InvalidOperationException("The requested type is not supported."); }
        }

        public static DynamicValue operator *(DynamicValue firstArg, DynamicValue secondArg)
        {
            if ((firstArg.DataType == DeciaDataType.Text)
                && ((secondArg.DataType == DeciaDataType.Decimal) || (secondArg.DataType == DeciaDataType.Integer)))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Text, null); }

                string firstString = firstArg.GetValue().ToString();
                int secondInt = (int)secondArg.GetValue();

                string newString = string.Empty;
                for (int i = 0; i < secondInt; i++)
                { newString += firstString; }

                return new DynamicValue(DeciaDataType.Text, newString);
            }
            else if ((secondArg.DataType == DeciaDataType.Text)
                && ((firstArg.DataType == DeciaDataType.Decimal) || (firstArg.DataType == DeciaDataType.Integer)))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Text, null); }

                int firstInt = (int)firstArg.GetValue();
                string secondString = secondArg.GetValue().ToString();

                string newString = string.Empty;
                for (int i = 0; i < firstInt; i++)
                { newString += secondString; }

                return new DynamicValue(DeciaDataType.Text, newString);
            }
            else if (firstArg.DataType == DeciaDataType.DateTime)
            {
                throw new InvalidOperationException("Cannot use * operator.");
            }
            else if (firstArg.DataType == DeciaDataType.TimeSpan)
            {
                throw new InvalidOperationException("Cannot use * operator.");
            }
            else if ((firstArg.DataType == DeciaDataType.Decimal)
                || (secondArg.DataType == DeciaDataType.Decimal))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Decimal, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstDecimal = (firstObject is double) ? (double)firstObject : Convert.ToDouble(firstObject);
                double secondDecimal = (secondObject is double) ? (double)secondObject : Convert.ToDouble(secondObject);
                double newDouble = firstDecimal * secondDecimal;

                return new DynamicValue(DeciaDataType.Decimal, newDouble);
            }
            else if ((firstArg.DataType == DeciaDataType.Integer)
                || (secondArg.DataType == DeciaDataType.Integer))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Integer, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstInt = (firstObject is int) ? (int)firstObject : Convert.ToInt32(firstObject);
                double secondInt = (secondObject is int) ? (int)secondObject : Convert.ToInt32(secondObject);
                double newInt = firstInt * secondInt;

                return new DynamicValue(DeciaDataType.Integer, newInt);
            }
            else if (firstArg.DataType == DeciaDataType.Boolean)
            {
                if (secondArg.DataType != DeciaDataType.Boolean)
                { throw new InvalidOperationException("Cannot use * operator."); }
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Boolean, null); }

                bool firstBool = firstArg.GetTypedValue<bool>();
                bool secondBool = secondArg.GetTypedValue<bool>();
                bool newBool = (firstBool && secondBool);
                return new DynamicValue(DeciaDataType.Boolean, newBool);
            }
            { throw new InvalidOperationException("The requested type is not supported."); }
        }

        public static DynamicValue operator /(DynamicValue firstArg, DynamicValue secondArg)
        {
            if ((firstArg.DataType == DeciaDataType.Text)
                || (secondArg.DataType == DeciaDataType.Text))
            {
                throw new InvalidOperationException("Cannot use / operator.");
            }
            else if (firstArg.DataType == DeciaDataType.DateTime)
            {
                throw new InvalidOperationException("Cannot use / operator.");
            }
            else if (firstArg.DataType == DeciaDataType.TimeSpan)
            {
                throw new InvalidOperationException("Cannot use / operator.");
            }
            else if ((firstArg.DataType == DeciaDataType.Decimal)
                || (firstArg.DataType == DeciaDataType.Decimal))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Decimal, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstDecimal = (firstObject is double) ? (double)firstObject : Convert.ToDouble(firstObject);
                double secondDecimal = (secondObject is double) ? (double)secondObject : Convert.ToDouble(secondObject);
                double newDouble = firstDecimal / secondDecimal;

                return new DynamicValue(DeciaDataType.Decimal, newDouble);
            }
            else if ((firstArg.DataType == DeciaDataType.Integer)
                || (firstArg.DataType == DeciaDataType.Integer))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Integer, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstInt = (firstObject is int) ? (int)firstObject : Convert.ToInt32(firstObject);
                double secondInt = (secondObject is int) ? (int)secondObject : Convert.ToInt32(secondObject);
                double newInt = firstInt / secondInt;

                return new DynamicValue(DeciaDataType.Integer, newInt);
            }
            else if (firstArg.DataType == DeciaDataType.Boolean)
            {
                if (secondArg.DataType != DeciaDataType.Boolean)
                { throw new InvalidOperationException("Cannot use / operator."); }
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Boolean, null); }

                bool firstBool = firstArg.GetTypedValue<bool>();
                bool secondBool = secondArg.GetTypedValue<bool>();
                bool newBool = (firstBool && secondBool);
                return new DynamicValue(firstArg.DataType, newBool);
            }
            { throw new InvalidOperationException("The requested type is not supported."); }
        }

        public static DynamicValue operator %(DynamicValue firstArg, DynamicValue secondArg)
        {
            if ((firstArg.DataType == DeciaDataType.Text)
                || (secondArg.DataType == DeciaDataType.Text))
            {
                throw new InvalidOperationException("Cannot use % operator.");
            }
            else if (firstArg.DataType == DeciaDataType.DateTime)
            {
                throw new InvalidOperationException("Cannot use % operator.");
            }
            else if (firstArg.DataType == DeciaDataType.TimeSpan)
            {
                throw new InvalidOperationException("Cannot use % operator.");
            }
            else if ((firstArg.DataType == DeciaDataType.Decimal)
                || (firstArg.DataType == DeciaDataType.Decimal))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Decimal, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstDecimal = (firstObject is double) ? (double)firstObject : Convert.ToDouble(firstObject);
                double secondDecimal = (secondObject is double) ? (double)secondObject : Convert.ToDouble(secondObject);
                double newDouble = firstDecimal % secondDecimal;

                return new DynamicValue(DeciaDataType.Decimal, newDouble);
            }
            else if ((firstArg.DataType == DeciaDataType.Integer)
                || (firstArg.DataType == DeciaDataType.Integer))
            {
                if (firstArg.IsNull || secondArg.IsNull)
                { return new DynamicValue(DeciaDataType.Integer, null); }

                object firstObject = firstArg.GetValue();
                object secondObject = secondArg.GetValue();

                double firstInt = (firstObject is int) ? (int)firstObject : Convert.ToInt32(firstObject);
                double secondInt = (secondObject is int) ? (int)secondObject : Convert.ToInt32(secondObject);
                double newInt = firstInt % secondInt;

                return new DynamicValue(DeciaDataType.Integer, newInt);
            }
            else if (firstArg.DataType == DeciaDataType.Boolean)
            {
                throw new InvalidOperationException("Cannot use % operator.");
            }
            else
            { throw new InvalidOperationException("The requested type is not supported."); }
        }

        #endregion
    }
}