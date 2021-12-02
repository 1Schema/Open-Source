using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.Modeling
{
    public enum DeciaDataType
    {
        Boolean = 1,
        Integer,
        Decimal,
        UniqueID,
        DateTime,
        TimeSpan,
        Text
    }

    public static class DeciaDataTypeUtils
    {
        public const bool EnforceStrictTypeMatching = false;

        public const DeciaDataType InvalidDataType = ((DeciaDataType)(-1));
        public const string NullValueString_Lowered = "null";
        public const string UndefinedValueString_Lowered = "undefined";
        public const string MaxValueString_Lowered = "max";
        public const string MinValueString_Lowered = "min";
        public static readonly string NullValueString_Uppered = NullValueString_Lowered.ToUpper();

        public const bool DefaultBoolValue = false;
        public const int DefaultIntegerValue = 0;
        public const double DefaultDecimalValue = 0.0;
        public static readonly Nullable<Guid> DefaultUniqueID = null;
        public static readonly DateTime DefaultDateTime = new DateTime(2015, 1, 1);
        public static readonly TimeSpan DefaultTimeSpan = new TimeSpan(1, 0, 0, 0, 0);
        public const string DefaultString = null;

        public static readonly DateTime MinDateTime = TimePeriodTypeUtils.SqlServerMinDate_SmallDateTime;
        public static readonly DateTime MaxDateTime = TimePeriodTypeUtils.SqlServerMaxDate_SmallDateTime;
        public static readonly TimeSpan MaxTimeSpan = TimePeriodTypeUtils.SqlServerMaxDuration_SmallDateTime;
        public static readonly TimeSpan MinTimeSpan = (new TimeSpan(0) - MaxTimeSpan);

        #region Methods - Type Matching Assertions

        public static void AssertTypesAreCompatible(this DeciaDataType firstType, DeciaDataType? secondType)
        {
            string errorMessage;
            bool areTypeCompatible = AreTypesCompatible(firstType, secondType, out errorMessage);

            if (!areTypeCompatible)
            { throw new InvalidOperationException(errorMessage); }
        }

        public static bool AreTypesCompatible(this DeciaDataType firstType, DeciaDataType? secondType)
        {
            string errorMessage;
            return AreTypesCompatible(firstType, secondType, out errorMessage);
        }

        public static bool AreTypesCompatible(this DeciaDataType firstType, DeciaDataType? secondType, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!EnforceStrictTypeMatching)
            {
                return true;
            }

            if (firstType != secondType)
            {
                errorMessage = "The DefaultValue must be of the same type as the ChronometricValue itself.";
                return false;
            }
            return true;
        }

        #endregion

        #region Methods - Conversion between Decia and System Types

        public static Type GetSystemTypeForDataType(this DeciaDataType dataType)
        {
            if (dataType == DeciaDataType.Boolean)
            { return typeof(bool); }
            else if (dataType == DeciaDataType.Integer)
            { return typeof(int); }
            else if (dataType == DeciaDataType.Decimal)
            { return typeof(double); }
            else if (dataType == DeciaDataType.UniqueID)
            { return typeof(Guid); }
            else if (dataType == DeciaDataType.DateTime)
            { return typeof(DateTime); }
            else if (dataType == DeciaDataType.TimeSpan)
            { return typeof(TimeSpan); }
            else if (dataType == DeciaDataType.Text)
            { return typeof(string); }
            else
            { throw new InvalidOperationException("The specified Type is not supported."); }
        }

        public static DeciaDataType GetDataTypeForSystemType(this Type type)
        {
            DeciaDataType dataType;
            var success = TryGetDataTypeForSystemType(type, out dataType);

            if (!success)
            { throw new InvalidOperationException("The specified Type is not supported."); }

            return dataType;
        }

        public static bool TryGetDataTypeForSystemType(this Type type, out DeciaDataType dataType)
        {
            dataType = DeciaDataType.Text;

            if (type == typeof(bool))
            {
                dataType = DeciaDataType.Boolean;
                return true;
            }
            else if (type == typeof(int))
            {
                dataType = DeciaDataType.Integer;
                return true;
            }
            else if (type == typeof(double))
            {
                dataType = DeciaDataType.Decimal;
                return true;
            }
            else if (type == typeof(Guid))
            {
                dataType = DeciaDataType.UniqueID;
                return true;
            }
            else if (type == typeof(DateTime))
            {
                dataType = DeciaDataType.DateTime;
                return true;
            }
            else if (type == typeof(TimeSpan))
            {
                dataType = DeciaDataType.TimeSpan;
                return true;
            }
            else if (type == typeof(string))
            {
                dataType = DeciaDataType.Text;
                return true;
            }
            return false;
        }

        #endregion

        #region Methods - Get Special Values (Default, Max, Min)

        public static object GetDefaultForDataType(this DeciaDataType dataType)
        {
            if (dataType == DeciaDataType.Boolean)
            { return DefaultBoolValue; }
            else if (dataType == DeciaDataType.Integer)
            { return DefaultIntegerValue; }
            else if (dataType == DeciaDataType.Decimal)
            { return DefaultDecimalValue; }
            else if (dataType == DeciaDataType.UniqueID)
            { return DefaultUniqueID; }
            else if (dataType == DeciaDataType.DateTime)
            { return DefaultDateTime; }
            else if (dataType == DeciaDataType.TimeSpan)
            { return DefaultTimeSpan; }
            else if (dataType == DeciaDataType.Text)
            { return DefaultString; }
            else
            { throw new InvalidOperationException("The specified Type is not supported."); }
        }

        public static object GetMaxValueForDataType(this DeciaDataType dataType)
        {
            object maxValue;
            bool success = TryGetMaxValueForDataType(dataType, out maxValue);

            if (!success)
            { throw new InvalidOperationException("The specfied Data Type does not have a Maximum Value."); }
            return maxValue;
        }

        public static bool TryGetMaxValueForDataType(this DeciaDataType dataType, out object maxValue)
        {
            if (dataType == DeciaDataType.Boolean)
            {
                maxValue = null;
                return false;
            }
            else if (dataType == DeciaDataType.Integer)
            {
                maxValue = int.MaxValue;
                return true;
            }
            else if (dataType == DeciaDataType.Decimal)
            {
                maxValue = double.MaxValue;
                return true;
            }
            else if (dataType == DeciaDataType.UniqueID)
            {
                maxValue = null;
                return false;
            }
            else if (dataType == DeciaDataType.DateTime)
            {
                maxValue = MaxDateTime;
                return true;
            }
            else if (dataType == DeciaDataType.TimeSpan)
            {
                maxValue = MaxTimeSpan;
                return true;
            }
            else if (dataType == DeciaDataType.Text)
            {
                maxValue = null;
                return false;
            }
            else
            { throw new InvalidOperationException("The specified Type is not supported."); }
        }

        public static object GetMinValueForDataType(this DeciaDataType dataType)
        {
            object minValue;
            bool success = TryGetMinValueForDataType(dataType, out minValue);

            if (!success)
            { throw new InvalidOperationException("The specfied Data Type does not have a Minimum Value."); }
            return minValue;
        }

        public static bool TryGetMinValueForDataType(this DeciaDataType dataType, out object minValue)
        {
            if (dataType == DeciaDataType.Boolean)
            {
                minValue = null;
                return false;
            }
            else if (dataType == DeciaDataType.Integer)
            {
                minValue = int.MinValue;
                return true;
            }
            else if (dataType == DeciaDataType.Decimal)
            {
                minValue = double.MinValue;
                return true;
            }
            else if (dataType == DeciaDataType.UniqueID)
            {
                minValue = null;
                return false;
            }
            else if (dataType == DeciaDataType.DateTime)
            {
                minValue = MinDateTime;
                return true;
            }
            else if (dataType == DeciaDataType.TimeSpan)
            {
                minValue = MinTimeSpan;
                return true;
            }
            else if (dataType == DeciaDataType.Text)
            {
                minValue = null;
                return false;
            }
            else
            { throw new InvalidOperationException("The specified Type is not supported."); }
        }

        #endregion

        #region Methods - Conversion between Object and String Values

        public const bool Default_UseTextForSpecialValues = false;
        public const string Default_DateTimeFormat = "o";
        public const string DateOnly_DateTimeFormat = "yyyy-MM-dd";
        public const string Default_TimeSpanFormat = @"dd\:hh\:mm\:ss";

        public static string GetDefaultFormatForDataType(this DeciaDataType dataType)
        {
            if (dataType == DeciaDataType.DateTime)
            { return Default_DateTimeFormat; }
            if (dataType == DeciaDataType.TimeSpan)
            { return Default_TimeSpanFormat; }
            return null;
        }

        public static string ToStringValue(this DeciaDataType dataType, object objectValue)
        {
            return ToStringValue(dataType, objectValue, Default_UseTextForSpecialValues);
        }

        public static string ToStringValue(this DeciaDataType dataType, object objectValue, bool useTextForSpecialValues)
        {
            var formatString = GetDefaultFormatForDataType(dataType);
            return ToStringValue(dataType, objectValue, useTextForSpecialValues, formatString);
        }

        public static string ToStringValue(this DeciaDataType dataType, object objectValue, bool useTextForSpecialValues, string formatString)
        {
            if (objectValue == null)
            {
                if (dataType == DeciaDataType.Text)
                { return string.Empty; }
                else if (useTextForSpecialValues)
                { return NullValueString_Lowered.ToUpper(); }
                else
                { return string.Empty; }
            }

            if (useTextForSpecialValues)
            {
                object maxValue, minValue;
                if (TryGetMaxValueForDataType(dataType, out maxValue))
                {
                    if (objectValue.Equals(maxValue))
                    { return MaxValueString_Lowered.ToUpper(); }
                }
                if (TryGetMinValueForDataType(dataType, out minValue))
                {
                    if (objectValue.Equals(minValue))
                    { return MinValueString_Lowered.ToUpper(); }
                }
            }

            if (dataType == DeciaDataType.Boolean)
            { return objectValue.ToString(); }
            else if (dataType == DeciaDataType.Integer)
            { return objectValue.ToString(); }
            else if (dataType == DeciaDataType.Decimal)
            { return objectValue.ToString(); }
            else if (dataType == DeciaDataType.UniqueID)
            { return objectValue.ToString(); }
            else if (dataType == DeciaDataType.DateTime)
            { return ((DateTime)objectValue).ToString(formatString); }
            else if (dataType == DeciaDataType.TimeSpan)
            { return ((TimeSpan)objectValue).ToString(formatString); }
            else if (dataType == DeciaDataType.Text)
            { return objectValue.ToString(); }
            else
            { throw new InvalidOperationException("The specified Type is not supported."); }
        }

        public static object ParseValue(this DeciaDataType dataType, string stringValue)
        {
            var exceptionMessage = "Parsing failed for the given DataType.";
            return ParseValue(dataType, stringValue, exceptionMessage);
        }

        public static object ParseValue(this DeciaDataType dataType, string stringValue, string exceptionMessage)
        {
            object objectValue;
            bool parseSuccess = TryParseValue(dataType, stringValue, out objectValue);

            if (!parseSuccess)
            { throw new InvalidOperationException(exceptionMessage); }

            return objectValue;
        }

        public static bool TryParseValue(this DeciaDataType dataType, string stringValue, out object objectValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                objectValue = null;
                return true;
            }

            var stringValue_Lowered = stringValue.ToLower();
            object limitValue;

            if (stringValue_Lowered.Contains(NullValueString_Lowered))
            {
                if (dataType != DeciaDataType.Text)
                {
                    objectValue = null;
                    return true;
                }
            }
            else if (stringValue_Lowered.Contains(UndefinedValueString_Lowered))
            {
                if (dataType != DeciaDataType.Text)
                {
                    objectValue = null;
                    return true;
                }
            }
            else if (stringValue_Lowered.Contains(MaxValueString_Lowered))
            {
                if (TryGetMaxValueForDataType(dataType, out limitValue))
                {
                    objectValue = limitValue;
                    return true;
                }
            }
            else if (stringValue_Lowered.Contains(MinValueString_Lowered))
            {
                if (TryGetMinValueForDataType(dataType, out limitValue))
                {
                    objectValue = limitValue;
                    return true;
                }
            }

            if (dataType == DeciaDataType.Boolean)
            {
                bool boolValue;
                var parseSuccess = bool.TryParse(stringValue, out boolValue);

                if (!parseSuccess)
                {
                    double doubleValue;
                    parseSuccess = double.TryParse(stringValue, out doubleValue);

                    objectValue = (parseSuccess) ? (object)(doubleValue > 0) : null;
                    return parseSuccess;
                }
                else
                {
                    objectValue = boolValue;
                    return true;
                }
            }
            else if (dataType == DeciaDataType.Integer)
            {
                int intValue;
                var parseSuccess = int.TryParse(stringValue, out intValue);

                if (!parseSuccess)
                {
                    objectValue = null;
                    return false;
                }
                else
                {
                    objectValue = intValue;
                    return true;
                }
            }
            else if (dataType == DeciaDataType.Decimal)
            {
                double doubleValue;
                var parseSuccess = double.TryParse(stringValue, out doubleValue);

                if (!parseSuccess)
                {
                    objectValue = null;
                    return false;
                }
                else
                {
                    objectValue = doubleValue;
                    return true;
                }
            }
            else if (dataType == DeciaDataType.UniqueID)
            {
                Guid guidValue;
                var parseSuccess = Guid.TryParse(stringValue, out guidValue);

                if (!parseSuccess)
                {
                    objectValue = null;
                    return false;
                }
                else
                {
                    objectValue = guidValue;
                    return true;
                }
            }
            else if (dataType == DeciaDataType.DateTime)
            {
                DateTime dateTimeValue;
                var parseSuccess = DateTime.TryParse(stringValue, out dateTimeValue);

                if (!parseSuccess)
                {
                    objectValue = null;
                    return false;
                }
                else
                {
                    objectValue = dateTimeValue;
                    return true;
                }
            }
            else if (dataType == DeciaDataType.TimeSpan)
            {
                TimeSpan timeSpanValue;
                var parseSuccess = TimeSpan.TryParse(stringValue, out timeSpanValue);

                if (!parseSuccess)
                {
                    objectValue = null;
                    return false;
                }
                else
                {
                    objectValue = timeSpanValue;
                    return true;
                }
            }
            else if (dataType == DeciaDataType.Text)
            {
                objectValue = stringValue;
                return true;
            }
            else
            { throw new InvalidOperationException("The specified Type is not supported."); }
        }

        #endregion

        #region Methods - Conversion of values between System Types

        public static bool TryToChangeValue<T>(this object incomingValue, out object resultingValue, out string errorMessage)
        {
            var typeForResult = typeof(T);
            return TryToChangeValue(incomingValue, typeForResult, out resultingValue, out errorMessage);
        }

        public static bool TryToChangeValue(this object incomingValue, Type typeForResult, out object resultingValue, out string errorMessage)
        {
            resultingValue = null;
            errorMessage = string.Empty;

            if (incomingValue == null)
            {
                var defaultResult = Activator.CreateInstance(typeForResult);

                if (defaultResult == null)
                {
                    resultingValue = null;
                    return true;
                }
                else
                {
                    errorMessage = "Invalid null value encountered.";
                    return false;
                }
            }

            var typeForResult_NonNullable = typeForResult;

            if (typeForResult.IsGenericType)
            {
                var genericArgs = typeForResult.GetGenericArguments();

                if (genericArgs.Count() != 1)
                {
                    errorMessage = "Complex Generic Types are not supported for Updates.";
                    return false;
                }
                typeForResult_NonNullable = genericArgs.First();
            }

            resultingValue = Convert.ChangeType(incomingValue, typeForResult_NonNullable);
            return true;
        }

        #endregion

        #region Methods - Exports

        public static bool DoesExportRequiresQuotes(this DeciaDataType dataType)
        {
            if (dataType == DeciaDataType.Boolean)
            { return false; }
            else if (dataType == DeciaDataType.Integer)
            { return false; }
            else if (dataType == DeciaDataType.Decimal)
            { return false; }
            else if (dataType == DeciaDataType.UniqueID)
            { return true; }
            else if (dataType == DeciaDataType.DateTime)
            { return true; }
            else if (dataType == DeciaDataType.TimeSpan)
            { return true; }
            else if (dataType == DeciaDataType.Text)
            { return true; }
            else
            { throw new InvalidOperationException("Unsupported DataType encountered."); }
        }

        public static string GetExportValue(this object value, DeciaDataType dataType)
        {
            if (value == null)
            { return "NULL"; }

            var requiresQuotes = dataType.DoesExportRequiresQuotes();
            if (!requiresQuotes)
            { return value.ToString(); }
            return string.Format("'{0}'", value);
        }

        #endregion
    }
}