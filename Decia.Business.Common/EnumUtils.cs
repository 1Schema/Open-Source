using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Common
{
    public static class EnumUtils
    {
        public static readonly string MissingDescriptionValue = string.Empty;

        public static ICollection<T> GetEnumValues<T>()
            where T : struct
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
            { throw new InvalidOperationException("The specified type is not an Enum."); }

            var result = new HashSet<T>();

            foreach (T value in Enum.GetValues(enumType))
            { result.Add(value); }
            return result;
        }

        public static Dictionary<string, string> GetEnumValuesAsSelectOptions<T>()
            where T : struct
        {
            return GetEnumValuesAsSelectOptions(typeof(T));
        }

        public static Dictionary<string, string> GetEnumValuesAsSelectOptions(this Type enumType)
        {
            if (!enumType.IsEnum)
            { throw new InvalidOperationException("The specified type is not an Enum."); }

            var result = new Dictionary<string, string>();

            foreach (var value in Enum.GetValues(enumType))
            {
                var id = value.ToString();
                var name = id;

                var fieldInfo = enumType.GetField(id);
                var descriptionAttribute = (Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute);

                if (descriptionAttribute != null)
                { name = descriptionAttribute.Description; }

                result.Add(id, name);
            }
            return result;
        }

        public static object GetAsEnum(Type enumType, object enumValue)
        {
            if (enumType == null)
            { return null; }
            if (enumValue == null)
            { return null; }

            object enumValueAsEnum;

            if (enumValue is string)
            { enumValueAsEnum = Enum.Parse(enumType, (string)enumValue); }
            else if (enumValue is int)
            { enumValueAsEnum = Enum.GetValues(enumType).GetValue((int)enumValue); }
            else
            { enumValueAsEnum = enumValue; }

            return enumValueAsEnum;
        }

        public static Nullable<T> GetAsEnum<T>(this Nullable<int> intValue)
            where T : struct
        {
            if (!intValue.HasValue)
            { return null; }
            return (T)((object)intValue.Value);
        }

        public static Nullable<T> GetAsEnum<T>(this string textValue)
            where T : struct
        {
            T enumValue;
            var success = Enum.TryParse<T>(textValue, out enumValue);

            if (!success)
            { return null; }
            return enumValue;
        }

        public static Nullable<int> GetAsInt<T>(this Nullable<T> enumValue)
            where T : struct
        {
            if (!enumValue.HasValue)
            { return null; }
            return (int)((object)enumValue.Value);
        }

        public static string GetName(Type enumType, object enumValue)
        {
            if (enumType == null)
            { return string.Empty; }
            if (enumValue == null)
            { return string.Empty; }

            var enumValueAsEnum = GetAsEnum(enumType, enumValue);
            return enumValueAsEnum.ToString();
        }

        public static string GetName<T>(this Nullable<T> enumValue)
            where T : struct
        {
            var name = GetName(typeof(T), enumValue);
            return name;
        }

        public static T GetAttributeValue<T>(this object enumValue)
            where T : Attribute
        {
            if (enumValue == null)
            { return null; }

            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attributeValue = (Attribute.GetCustomAttribute(fieldInfo, typeof(T)) as T);
            return attributeValue;
        }

        public static string GetDescription(Type enumType, object enumValue)
        {
            return GetDescription(enumType, enumValue, true);
        }

        public static string GetDescription(Type enumType, object enumValue, bool useNameIfNoneExists)
        {
            if (enumType == null)
            { return MissingDescriptionValue; }
            if (enumValue == null)
            { return MissingDescriptionValue; }

            var enumValueAsEnum = GetAsEnum(enumType, enumValue);
            var descriptionAttribute = GetAttributeValue<DescriptionAttribute>(enumValueAsEnum);

            if (descriptionAttribute == null)
            {
                if (useNameIfNoneExists)
                { return GetName(enumType, enumValue); }
                else
                { return MissingDescriptionValue; }
            }
            return descriptionAttribute.Description;
        }

        public static string GetDescription<T>(this Nullable<T> enumValue)
            where T : struct
        {
            return GetDescription<T>(enumValue, true);
        }

        public static string GetDescription<T>(this Nullable<T> enumValue, bool useNameIfNoneExists)
            where T : struct
        {
            var description = GetDescription(typeof(T), enumValue, useNameIfNoneExists);
            return description;
        }

        public static bool Bitwise_IsSet<T>(this T currentValue, T checkValue)
            where T : struct
        {
            int currentInt = (int)((object)currentValue);
            int checkInt = (int)((object)checkValue);

            var result = ((currentInt & checkInt) != 0);
            return result;
        }

        public static T Bitwise_Set<T>(this T currentValue, T addValue)
            where T : struct
        {
            int currentInt = (int)((object)currentValue);
            int addInt = (int)((object)addValue);

            var result = (currentInt | addInt);
            return (T)((object)result);
        }

        public static T Bitwise_Unset<T>(this T currentValue, T removeValue)
            where T : struct
        {
            int currentInt = (int)((object)currentValue);
            int removeInt = (int)((object)removeValue);

            var result = (currentInt & (~removeInt));
            return (T)((object)result);
        }

        public static T Bitwise_Combine<T>(this IEnumerable<T> currentValues)
            where T : struct
        {
            T result = (T)((object)0);

            foreach (T value in currentValues)
            { result = result.Bitwise_Set(value); }
            return result;
        }

        public static ICollection<T> Bitwise_Uncombine<T>(this T currentValue)
            where T : struct
        {
            var result = new HashSet<T>();

            foreach (T checkValue in GetEnumValues<T>())
            {
                if (currentValue.Bitwise_IsSet(checkValue))
                { result.Add(checkValue); }
            }
            return result;
        }

        public static void Bitwise_AssertValuesAreSet<T>(this T currentValues, T checkValues)
           where T : struct
        {
            string failureMessage;
            var areAllValuesSet = Bitwise_AreValuesSet<T>(currentValues, checkValues, out failureMessage);

            if (!areAllValuesSet)
            { throw new InvalidOperationException(failureMessage); }
        }

        public static bool Bitwise_AreValuesSet<T>(this T currentValues, T checkValues)
            where T : struct
        {
            string failureMessage;
            return Bitwise_AreValuesSet<T>(currentValues, checkValues, out failureMessage);
        }

        public static bool Bitwise_AreValuesSet<T>(this T currentValues, T checkValues, out string failureMessage)
            where T : struct
        {
            var areAllValuesSet = true;
            var missingValues = new List<T>();

            foreach (var definedValue in EnumUtils.GetEnumValues<T>())
            {
                var isValueRelevant = checkValues.Bitwise_IsSet(definedValue);
                if (!isValueRelevant)
                { continue; }

                var isValueSet = currentValues.Bitwise_IsSet(definedValue);
                if (isValueSet)
                { continue; }

                areAllValuesSet = false;
                missingValues.Add(definedValue);
            }

            if (areAllValuesSet)
            { failureMessage = string.Empty; }
            else
            { failureMessage = "Missing " + typeof(T).Name + "s encountered: " + missingValues.ConvertToCollectionAsString(", "); }

            return areAllValuesSet;
        }
    }
}