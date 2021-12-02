using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Common.TypedIds
{
    public static class TypedIdUtils
    {
        public static int GenerateUniqueInteger()
        {
            Guid guid = Guid.NewGuid();
            int uniqueInt = guid.GetHashCode();
            return uniqueInt;
        }

        public static int GenerateUniqueInteger(int excludedInt)
        {
            return GenerateUniqueInteger(new int[] { excludedInt });
        }

        public static int GenerateUniqueInteger(IEnumerable<int> excludedInts)
        {
            int uniqueInt = GenerateUniqueInteger();
            while (excludedInts.Contains(uniqueInt))
            { uniqueInt = GenerateUniqueInteger(); }
            return uniqueInt;
        }

        public static Nullable<T> GetAndAssertUniqueValue<T>(this IEnumerable<T> ids)
            where T : struct
        { return GetAndAssertUniqueValue<T>(ids, true); }

        public static Nullable<T> GetAndAssertUniqueValue<T>(this IEnumerable<T> ids, bool allowNull)
            where T : struct
        {
            if (ids.Count() > 1)
            { throw new InvalidOperationException("The " + typeof(T).Name + " must be unique for the parameters specified."); }
            if (ids.Count() < 1)
            {
                if (allowNull)
                { return null; }
                else
                { throw new InvalidOperationException("The " + typeof(T).Name + " does not exist for the parameters specified."); }
            }
            return ids.First();
        }

        public static string StructToString<T>(string name, T value)
            where T : struct
        {
            Nullable<T> nValue = value;
            return NStructToString(name, nValue);
        }

        public static string NStructToString<T>(string name, Nullable<T> value)
            where T : struct
        {
            var stringValue = value.HasValue ? value.ToString() : string.Empty;
            return GetNamedString(name, stringValue);
        }

        public static string GetNamedString(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            { return !string.IsNullOrWhiteSpace(value) ? value : string.Empty; }
            else
            { return string.Format(ConversionUtils.TwoPartItemFormat, name, value); }
        }

        public static string ConvertToString(Type idType, object innerIdValue)
        {
            return ConvertToString(idType.Name, innerIdValue);
        }

        public static string ConvertToString(string name, object innerIdValue)
        {
            string valueAsString = string.Empty;

            if (innerIdValue != null)
            {
                if ((innerIdValue is IEnumerable) && (!(innerIdValue is string)))
                {
                    bool firstTime = true;
                    foreach (object element in (innerIdValue as IEnumerable))
                    {
                        if (firstTime)
                        {
                            valueAsString += element.ToString();
                            firstTime = false;
                        }
                        else
                        {
                            valueAsString += ", " + element.ToString();
                        }
                    }
                }
                else
                { valueAsString = innerIdValue.ToString(); }
            }

            string idText = string.Format(ConversionUtils.TwoPartItemFormat, name, valueAsString);
            return idText;
        }

        public static bool AreEqual(this IGuidId thisValue, object otherValue)
        {
            if (otherValue == null)
            { return false; }

            Type thisType = thisValue.GetType();
            if (!otherValue.GetType().Equals(thisType))
            { return false; }

            IGuidId otherIdValue = (IGuidId)otherValue;
            bool areEqual = (thisValue.InnerGuid == otherIdValue.InnerGuid);
            return areEqual;
        }
    }
}