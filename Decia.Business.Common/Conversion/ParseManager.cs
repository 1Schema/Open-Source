using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Conversion
{
    public static class ParseManager
    {
        public const string UnparseableMessage = "Parsing is not supported for the requested Type.";

        public static readonly Nullable<bool> NullBool = null;
        public static readonly Nullable<byte> NullByte = null;
        public static readonly Nullable<int> NullInt = null;
        public static readonly Nullable<short> NullShort = null;
        public static readonly Nullable<long> NullLong = null;
        public static readonly Nullable<uint> NullUInt = null;
        public static readonly Nullable<ushort> NullUShort = null;
        public static readonly Nullable<ulong> NullULong = null;
        public static readonly Nullable<float> NullFloat = null;
        public static readonly Nullable<double> NullDouble = null;
        public static readonly Nullable<Guid> NullGuid = null;


        public static bool IsParsingSupported<T>()
        {
            Type typeToParse = typeof(T);
            bool isParseable = IsParsingSupported(typeToParse);
            return isParseable;
        }

        public static bool IsParsingSupported(Type typeToParse)
        {
            if (typeToParse == typeof(bool))
            { return true; }
            if (typeToParse == typeof(byte))
            { return true; }
            if (typeToParse == typeof(int))
            { return true; }
            if (typeToParse == typeof(short))
            { return true; }
            if (typeToParse == typeof(long))
            { return true; }
            if (typeToParse == typeof(uint))
            { return true; }
            if (typeToParse == typeof(ushort))
            { return true; }
            if (typeToParse == typeof(ulong))
            { return true; }
            if (typeToParse == typeof(float))
            { return true; }
            if (typeToParse == typeof(double))
            { return true; }
            if (typeToParse == typeof(Guid))
            { return true; }

            if (typeToParse == typeof(Nullable<bool>))
            { return true; }
            if (typeToParse == typeof(Nullable<byte>))
            { return true; }
            if (typeToParse == typeof(Nullable<int>))
            { return true; }
            if (typeToParse == typeof(Nullable<short>))
            { return true; }
            if (typeToParse == typeof(Nullable<long>))
            { return true; }
            if (typeToParse == typeof(Nullable<uint>))
            { return true; }
            if (typeToParse == typeof(Nullable<ushort>))
            { return true; }
            if (typeToParse == typeof(Nullable<ulong>))
            { return true; }
            if (typeToParse == typeof(Nullable<float>))
            { return true; }
            if (typeToParse == typeof(Nullable<double>))
            { return true; }
            if (typeToParse == typeof(Nullable<Guid>))
            { return true; }

            if (typeToParse.IsEnum)
            { return true; }
            if (typeToParse == typeof(ModelObjectReference))
            { return true; }

            if (typeToParse == typeof(string))
            { return true; }

            return false;
        }

        public static void AssertParsingIsSupported<T>()
        {
            Type typeToParse = typeof(T);
            AssertParsingIsSupported(typeToParse);
        }

        public static void AssertParsingIsSupported(Type typeToParse)
        {
            if (!IsParsingSupported(typeToParse))
            { throw new InvalidOperationException(UnparseableMessage); }
        }

        public static string GetAsString<T>(T value)
        {
            Type typeToParse = typeof(T);
            string textValue = GetAsString(typeToParse, value);
            return textValue;
        }

        public static string GetAsString(Type typeOfValue, object value)
        {
            AssertParsingIsSupported(typeOfValue);

            if (value == null)
            { return null; }

            if (typeOfValue == typeof(bool))
            { return value.ToString(); }
            if (typeOfValue == typeof(byte))
            { return value.ToString(); }
            if (typeOfValue == typeof(int))
            { return value.ToString(); }
            if (typeOfValue == typeof(short))
            { return value.ToString(); }
            if (typeOfValue == typeof(long))
            { return value.ToString(); }
            if (typeOfValue == typeof(uint))
            { return value.ToString(); }
            if (typeOfValue == typeof(ushort))
            { return value.ToString(); }
            if (typeOfValue == typeof(ulong))
            { return value.ToString(); }
            if (typeOfValue == typeof(float))
            { return value.ToString(); }
            if (typeOfValue == typeof(double))
            { return value.ToString(); }
            if (typeOfValue == typeof(Guid))
            { return value.ToString(); }

            if (typeOfValue == typeof(Nullable<bool>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<byte>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<int>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<short>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<long>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<uint>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<ushort>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<ulong>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<float>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<double>))
            { return value.ToString(); }
            if (typeOfValue == typeof(Nullable<Guid>))
            { return value.ToString(); }

            if (typeOfValue.IsEnum)
            { return value.ToString(); }
            if (typeOfValue == typeof(ModelObjectReference))
            { return ((ModelObjectReference)value).SaveAsText(); }

            if (typeOfValue == typeof(string))
            { return (string)value; }

            return null;
        }

        public static T DoParse<T>(string valueToParse)
        {
            Type typeToParse = typeof(T);
            object parsedValue = DoParse(typeToParse, valueToParse);
            T typedValue = (T)parsedValue;
            return typedValue;
        }

        public static object DoParse(Type typeToParse, string valueToParse)
        {
            AssertParsingIsSupported(typeToParse);

            if (typeToParse == typeof(bool))
            { return bool.Parse(valueToParse); }
            if (typeToParse == typeof(byte))
            { return byte.Parse(valueToParse); }
            if (typeToParse == typeof(int))
            { return int.Parse(valueToParse); }
            if (typeToParse == typeof(short))
            { return short.Parse(valueToParse); }
            if (typeToParse == typeof(long))
            { return long.Parse(valueToParse); }
            if (typeToParse == typeof(uint))
            { return uint.Parse(valueToParse); }
            if (typeToParse == typeof(ushort))
            { return ushort.Parse(valueToParse); }
            if (typeToParse == typeof(ulong))
            { return ulong.Parse(valueToParse); }
            if (typeToParse == typeof(float))
            { return float.Parse(valueToParse); }
            if (typeToParse == typeof(double))
            { return double.Parse(valueToParse); }
            if (typeToParse == typeof(Guid))
            { return new Guid(valueToParse); }

            if (typeToParse == typeof(Nullable<bool>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? bool.Parse(valueToParse) : NullBool; }
            if (typeToParse == typeof(Nullable<byte>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? byte.Parse(valueToParse) : NullByte; }
            if (typeToParse == typeof(Nullable<int>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? int.Parse(valueToParse) : NullInt; }
            if (typeToParse == typeof(Nullable<short>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? short.Parse(valueToParse) : NullShort; }
            if (typeToParse == typeof(Nullable<long>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? long.Parse(valueToParse) : NullLong; }
            if (typeToParse == typeof(Nullable<uint>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? uint.Parse(valueToParse) : NullUInt; }
            if (typeToParse == typeof(Nullable<ushort>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? ushort.Parse(valueToParse) : NullUShort; }
            if (typeToParse == typeof(Nullable<ulong>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? ulong.Parse(valueToParse) : NullULong; }
            if (typeToParse == typeof(Nullable<float>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? float.Parse(valueToParse) : NullFloat; }
            if (typeToParse == typeof(Nullable<double>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? new Guid(valueToParse) : NullGuid; }
            if (typeToParse == typeof(Nullable<Guid>))
            { return (!string.IsNullOrWhiteSpace(valueToParse)) ? double.Parse(valueToParse) : NullDouble; }

            if (typeToParse.IsEnum)
            { return Enum.Parse(typeToParse, valueToParse); }
            if (typeToParse == typeof(ModelObjectReference))
            { return ModelObjectReference.GlobalTypeReference.LoadFromText(valueToParse); }

            if (typeToParse == typeof(string))
            { return valueToParse; }

            return null;
        }

        public static bool TryParse(string valueToParse, bool wasSpecifiedAsText, out Type inferredType, out object parsedValue)
        {
            var isInteger = valueToParse.All(x => char.IsDigit(x) || (x == '-'));
            bool success = false;

            if (isInteger)
            {
                int parsedInt;
                success = int.TryParse(valueToParse, out parsedInt);
                if (success)
                {
                    parsedValue = parsedInt;
                    inferredType = parsedValue.GetType();
                    return true;
                }
            }

            TimeSpan parsedTimeSpan;
            success = TimeSpan.TryParse(valueToParse, out parsedTimeSpan);
            if (success)
            {
                parsedValue = parsedTimeSpan;
                inferredType = parsedValue.GetType();
                return true;
            }

            DateTime parsedDateTime;
            success = DateTime.TryParse(valueToParse, out parsedDateTime);
            if (success)
            {
                parsedValue = parsedDateTime;
                inferredType = parsedValue.GetType();
                return true;
            }

            Guid parsedGuid;
            success = Guid.TryParse(valueToParse, out parsedGuid);
            if (success)
            {
                parsedValue = parsedGuid;
                inferredType = parsedValue.GetType();
                return true;
            }

            if (wasSpecifiedAsText)
            {
                parsedValue = valueToParse.ToString();
                inferredType = parsedValue.GetType();
                return true;
            }

            double parsedDouble;
            success = double.TryParse(valueToParse, out parsedDouble);
            if (success)
            {
                parsedValue = parsedDouble;
                inferredType = parsedValue.GetType();
                return true;
            }

            bool parsedBool;
            success = bool.TryParse(valueToParse, out parsedBool);
            if (success)
            {
                parsedValue = parsedBool;
                inferredType = parsedValue.GetType();
                return true;
            }

            parsedValue = null;
            inferredType = typeof(object);
            return true;
        }
    }
}