using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using DomainDriver.DomainModeling.DataProviders;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Conversion
{
    public static class ConversionUtils
    {
        public const int MilliSecondsInSecond = 1000;
        public const int SecondsInMinute = 60;
        public const char NestedItemStart = '<';
        public const char NestedItemEnd = '>';
        public static readonly string NestedItemFormat = NestedItemStart + "{0}" + NestedItemEnd;
        public const char ItemPartSeparator = ':';
        public const char ItemPartSeparator_Override = '|';
        public static readonly string TwoPartItemFormat = "{0}" + ItemPartSeparator + "{1}";
        public static readonly string ThreePartItemFormat = "{0}" + ItemPartSeparator + "{1}" + ItemPartSeparator + "{2}";
        public static readonly string FourPartItemFormat = "{0}" + ItemPartSeparator + "{1}" + ItemPartSeparator + "{2}" + ItemPartSeparator + "{3}";
        public const char ListItemSeparator = ',';
        public static readonly string TwoItemListFormat = "{0}" + ListItemSeparator + "{1}";
        public static readonly string ThreeItemListFormat = "{0}" + ListItemSeparator + "{1}" + ListItemSeparator + "{2}";
        public static readonly string FourItemListFormat = "{0}" + ListItemSeparator + "{1}" + ListItemSeparator + "{2}" + ListItemSeparator + "{3}";
        public static readonly string FiveItemListFormat = "{0}" + ListItemSeparator + "{1}" + ListItemSeparator + "{2}" + ListItemSeparator + "{3}" + ListItemSeparator + "{4}";
        public static readonly string SixItemListFormat = "{0}" + ListItemSeparator + "{1}" + ListItemSeparator + "{2}" + ListItemSeparator + "{3}" + ListItemSeparator + "{4}" + ListItemSeparator + "{5}";
        public static readonly string SevenItemListFormat = "{0}" + ListItemSeparator + "{1}" + ListItemSeparator + "{2}" + ListItemSeparator + "{3}" + ListItemSeparator + "{4}" + ListItemSeparator + "{5}" + ListItemSeparator + "{6}";
        public static readonly string EightItemListFormat = "{0}" + ListItemSeparator + "{1}" + ListItemSeparator + "{2}" + ListItemSeparator + "{3}" + ListItemSeparator + "{4}" + ListItemSeparator + "{5}" + ListItemSeparator + "{6}" + ListItemSeparator + "{7}";

        public const string PrefixString_UIntToGuid_Standard = "00000000-0000-0000-0000-0000";
        public const string PrefixString_UIntToGuid_NonStandard = "12345678-1234-5678-1234-5678";

        public const string PrefixString_ULongToGuid_Standard = "00000000-0000-0000-";
        public const string PrefixString_ULongToGuid_NonStandard = "12345678-1234-5678-";

        public const string FormatString_ULongToGuid_Standard = "00000000-0000-0000-{0}-{1}";
        public const string FormatString_ULongToGuid_NonStandard = "12345678-1234-5678-{0}-{1}";


        #region Integer to Guid Conversion Methods

        public static Guid ConvertToGuid<T>(this T value)
            where T : struct
        {
            var uinteger = (int)((object)value);
            return ConvertIntToGuid(uinteger);
        }

        public static Guid ConvertToGuid<T>(this T value, bool forceNonStandardPrefix)
            where T : struct
        {
            var uinteger = (int)((object)value);
            return ConvertIntToGuid(uinteger, forceNonStandardPrefix);
        }

        public static Guid ConvertIntToGuid(this int integer)
        {
            var uinteger = (uint)integer;
            return ConvertUIntToGuid(uinteger);
        }

        public static Guid ConvertIntToGuid(this int integer, bool forceNonStandardPrefix)
        {
            var uinteger = (uint)integer;
            return ConvertUIntToGuid(uinteger, forceNonStandardPrefix);
        }

        public static Guid ConvertIntToGuid(this int integer, string prefix)
        {
            var uinteger = (uint)integer;
            return ConvertUIntToGuid(uinteger, prefix);
        }

        public static Guid ConvertUIntToGuid(this uint uinteger)
        {
            return uinteger.ConvertUIntToGuid(false);
        }

        public static Guid ConvertUIntToGuid(this uint uinteger, bool forceNonStandardPrefix)
        {
            var prefix = forceNonStandardPrefix ? PrefixString_UIntToGuid_NonStandard : PrefixString_UIntToGuid_Standard;
            return ConvertUIntToGuid(uinteger, prefix);
        }

        public static Guid ConvertUIntToGuid(this uint uinteger, string prefix)
        {
            var uintAsStr = prefix + uinteger.ToString("X8");
            var uintAsGuid = new Guid(uintAsStr);
            return uintAsGuid;
        }

        public static bool IsGuidStandardInt(this Guid guid)
        {
            var guidAsStr = guid.ToString();
            return guidAsStr.Contains(PrefixString_UIntToGuid_Standard);
        }

        public static int ConvertGuidToInt(this Guid guid)
        {
            var uinteger = guid.ConvertGuidToUInt();
            return (int)uinteger;
        }

        public static int ConvertGuidToInt(this Guid guid, bool allowNonStandardPrefix)
        {
            var uinteger = guid.ConvertGuidToUInt(allowNonStandardPrefix);
            return (int)uinteger;
        }

        public static uint ConvertGuidToUInt(this Guid guid)
        {
            var guidAsStr = guid.ToString();
            if (!guidAsStr.Contains(PrefixString_UIntToGuid_Standard))
            { throw new InvalidOperationException("The specified Guid cannot be converted to Int32 since it contains extra information."); }

            var intAsHexStr = guidAsStr.Replace(PrefixString_UIntToGuid_Standard, "");
            if (intAsHexStr.Length != 8)
            { throw new InvalidOperationException("The specified Guid cannot be converted to Int32 since it contains extra information."); }

            var uInt = uint.Parse(intAsHexStr, NumberStyles.HexNumber);
            return uInt;
        }

        public static uint ConvertGuidToUInt(this Guid guid, bool allowNonStandardPrefix)
        {
            if (!allowNonStandardPrefix)
            { return guid.ConvertGuidToUInt(); }

            var guidAsStr = guid.ToString();
            var intAsHexStr = string.Empty;

            for (int i = 8; i > 0; i--)
            {
                var strIndex = (guidAsStr.Length - i);
                intAsHexStr = intAsHexStr + guidAsStr[strIndex];
            }
            var uInt = uint.Parse(intAsHexStr, NumberStyles.HexNumber);
            return uInt;
        }

        public static void ConvertGuidToUInts(this Guid guid, out uint uInt0, out uint uInt1, out uint uInt2, out uint uInt3)
        {
            int int0, int1, int2, int3;
            ConvertGuidToInts(guid, out int0, out int1, out int2, out int3);

            uInt0 = (uint)int0;
            uInt1 = (uint)int1;
            uInt2 = (uint)int2;
            uInt3 = (uint)int3;
        }

        public static void ConvertGuidToInts(this Guid guid, out int int0, out int int1, out int int2, out int int3)
        {
            var guidAsBytes = guid.ToByteArray();

            var int0AsBytes = new byte[] { guidAsBytes[0], guidAsBytes[1], guidAsBytes[2], guidAsBytes[3] };
            var int1AsBytes = new byte[] { guidAsBytes[4], guidAsBytes[5], guidAsBytes[6], guidAsBytes[7] };
            var int2AsBytes = new byte[] { guidAsBytes[8], guidAsBytes[9], guidAsBytes[10], guidAsBytes[11], };
            var int3AsBytes = new byte[] { guidAsBytes[12], guidAsBytes[13], guidAsBytes[14], guidAsBytes[15] };

            int0 = BitConverter.ToInt32(int0AsBytes, 0);
            int1 = BitConverter.ToInt32(int1AsBytes, 0);
            int2 = BitConverter.ToInt32(int2AsBytes, 0);
            int3 = BitConverter.ToInt32(int3AsBytes, 0);
        }

        #endregion

        #region Long to Guid Conversion Methods

        public static Guid ConvertLongAsIntsToGuid(this int longPart0, int longPart1)
        {
            return ConvertLongAsIntsToGuid(longPart0, longPart1, false);
        }

        public static Guid ConvertLongAsIntsToGuid(this int longPart0, int longPart1, bool forceNonStandardPrefix)
        {
            var uLongPart0 = (uint)longPart0;
            var uLongPart1 = (uint)longPart1;

            ulong ulongValue = (((ulong)uLongPart0) << 32) | uLongPart1;
            var longValue = (long)ulongValue;

            var guidValue = ConvertLongToGuid(longValue, forceNonStandardPrefix);
            return guidValue;
        }

        public static Guid ConvertLongToGuid(this long longInt)
        {
            var uLongInt = (ulong)longInt;
            return ConvertULongToGuid(uLongInt);
        }

        public static Guid ConvertLongToGuid(this long longInt, bool forceNonStandardPrefix)
        {
            var uLongInt = (ulong)longInt;
            return ConvertULongToGuid(uLongInt, forceNonStandardPrefix);
        }

        public static Guid ConvertULongToGuid(this ulong uLongInt)
        {
            return uLongInt.ConvertULongToGuid(false);
        }

        public static Guid ConvertULongToGuid(this ulong uLongInt, bool forceNonStandardPrefix)
        {
            var format = forceNonStandardPrefix ? FormatString_ULongToGuid_NonStandard : FormatString_ULongToGuid_Standard;
            var uLongAsStr = uLongInt.ToString("X16");
            var uLongAsStr_Part0 = uLongAsStr.Substring(0, 4);
            var uLongAsStr_Part1 = uLongAsStr.Substring(4, 12);
            var uLongAsGuid = new Guid(string.Format(format, uLongAsStr_Part0, uLongAsStr_Part1));
            return uLongAsGuid;
        }

        public static Guid ConvertDatesToGuid(this IEnumerable<DateTime> dates)
        {
            if (dates.Count() != 2)
            { throw new InvalidOperationException("The dates collection must contain exactly 2 dates."); }

            var date0 = dates.ElementAt(0);
            var date1 = dates.ElementAt(1);
            return ConvertDatesToGuid(date0, date1);
        }

        public static Guid ConvertDatesToGuid(this DateTime date0, DateTime date1)
        {
            var long0 = date0.Ticks;
            var long1 = date1.Ticks;
            return ConvertLongsToGuid(long0, long1);
        }

        public static Guid ConvertLongsToGuid(this long long0, long long1)
        {
            var uLong0 = (ulong)long0;
            var uLong1 = (ulong)long1;
            return ConvertULongsToGuid(uLong0, uLong1);
        }

        public static Guid ConvertULongsToGuid(this ulong ulong0, ulong ulong1)
        {
            BigInteger bigInt0 = ulong0;
            BigInteger bigInt1 = ulong1;
            BigInteger combinedValue = (bigInt1 << 64) | bigInt0;
            var guidValue = new Guid(combinedValue.ToByteArray());
            return guidValue;
        }

        public static bool IsGuidStandardLong(this Guid guid)
        {
            var guidAsStr = guid.ToString();
            return guidAsStr.Contains(PrefixString_ULongToGuid_Standard);
        }

        public static long ConvertGuidToLong(this Guid guid)
        {
            var uLongInt = guid.ConvertGuidToULong();
            return (long)uLongInt;
        }

        public static long ConvertGuidToLong(this Guid guid, bool allowNonStandardPrefix)
        {
            var uLongInt = guid.ConvertGuidToULong(allowNonStandardPrefix);
            return (long)uLongInt;
        }

        public static ulong ConvertGuidToULong(this Guid guid)
        {
            var guidAsStr = guid.ToString();
            if (!guidAsStr.Contains(PrefixString_ULongToGuid_Standard))
            { throw new InvalidOperationException("The specified Guid cannot be converted to Int64 since it contains extra information."); }

            var longAsHexStr = guidAsStr.Replace(PrefixString_ULongToGuid_Standard, "").Replace("-", "");
            if (longAsHexStr.Length != 16)
            { throw new InvalidOperationException("The specified Guid cannot be converted to Int64 since it contains extra information."); }

            var uLongInt = ulong.Parse(longAsHexStr, NumberStyles.HexNumber);
            return uLongInt;
        }

        public static ulong ConvertGuidToULong(this Guid guid, bool allowNonStandardPrefix)
        {
            if (!allowNonStandardPrefix)
            { return guid.ConvertGuidToULong(); }

            var guidAsStr = guid.ToString().Replace("-", "");
            var longAsHexStr = string.Empty;

            for (int i = 16; i > 0; i--)
            {
                var strIndex = (guidAsStr.Length - i);
                longAsHexStr = longAsHexStr + guidAsStr[strIndex];
            }
            var uLongInt = ulong.Parse(longAsHexStr, NumberStyles.HexNumber);
            return uLongInt;
        }

        public static IEnumerable<DateTime> ConvertGuidToDates(this Guid guid)
        {
            DateTime date0, date1;
            ConvertGuidToDates(guid, out date0, out date1);

            var dates = new DateTime[2];
            dates[0] = date0;
            dates[1] = date1;
            return dates;
        }

        public static void ConvertGuidToDates(this Guid guid, out DateTime date0, out DateTime date1)
        {
            long long0, long1;
            ConvertGuidToLongs(guid, out long0, out long1);

            date0 = new DateTime(long0);
            date1 = new DateTime(long1);
        }

        public static void ConvertGuidToULongs(this Guid guid, out ulong uLong0, out ulong uLong1)
        {
            long long0, long1;
            ConvertGuidToLongs(guid, out long0, out long1);

            uLong0 = (ulong)long0;
            uLong1 = (ulong)long1;
        }

        public static void ConvertGuidToLongs(this Guid guid, out long long0, out long long1)
        {
            var guidAsBytes = guid.ToByteArray();

            var long0AsBytes = new byte[] { guidAsBytes[0], guidAsBytes[1], guidAsBytes[2], guidAsBytes[3], guidAsBytes[4], guidAsBytes[5], guidAsBytes[6], guidAsBytes[7] };
            var long1AsBytes = new byte[] { guidAsBytes[8], guidAsBytes[9], guidAsBytes[10], guidAsBytes[11], guidAsBytes[12], guidAsBytes[13], guidAsBytes[14], guidAsBytes[15] };

            long0 = (long)BitConverter.ToInt64(long0AsBytes, 0);
            long1 = (long)BitConverter.ToInt64(long1AsBytes, 0);
        }

        #endregion

        #region String to ComplexId (ActorType) Conversion Methods

        public static bool ConvertStringToComplexId(this string complexId, out SiteActorType actorType, out Guid id)
        {
            if (string.IsNullOrWhiteSpace(complexId))
            {
                actorType = (SiteActorType)0;
                id = Guid.Empty;
                return false;
            }

            string[] parts = complexId.Split(new char[] { ItemPartSeparator });
            if (parts.Length != 2)
            {
                actorType = (SiteActorType)0;
                id = Guid.Empty;
                return false;
            }

            actorType = (SiteActorType)int.Parse(parts[0]);
            id = new Guid(parts[1]);
            return true;
        }

        public static string ConvertComplexIdToString(SiteActorType actorType, Guid id)
        {
            return ((int)actorType).ToString() + ItemPartSeparator + id.ToString();
        }

        #endregion

        #region String to ComplexId (ModelObjectType) Conversion Methods (LATER: Add "ConvertString" methods with alternate dimension numbers)

        public static bool ConvertStringToComplexId(this string complexId, out ModelObjectType objectType, out Guid id)
        {
            return ConvertStringToComplexId(complexId, ItemPartSeparator, out objectType, out id);
        }

        public static bool ConvertStringToComplexId(this string complexId, char separator, out ModelObjectType objectType, out Guid id)
        {
            if (string.IsNullOrWhiteSpace(complexId))
            {
                objectType = ModelObjectType.None;
                id = Guid.Empty;
                return false;
            }

            string[] parts = complexId.Split(new char[] { separator });
            if (parts.Length != 2)
            {
                objectType = (ModelObjectType)0;
                id = Guid.Empty;
                return false;
            }

            int objectTypeInt;
            Guid idGuid;
            var intSuccess = int.TryParse(parts[0], out objectTypeInt);
            var guidSuccess = Guid.TryParse(parts[1], out idGuid);

            if (!intSuccess || !guidSuccess)
            {
                objectType = (ModelObjectType)0;
                id = Guid.Empty;
                return false;
            }

            objectType = (ModelObjectType)objectTypeInt;
            id = idGuid;
            return true;
        }

        public static bool ConvertStringToComplexId(this string complexId, out ModelObjectType objectType, out int id)
        {
            return ConvertStringToComplexId(complexId, ItemPartSeparator, out objectType, out id);
        }

        public static bool ConvertStringToComplexId(this string complexId, char separator, out ModelObjectType objectType, out int id)
        {
            if (string.IsNullOrWhiteSpace(complexId))
            {
                objectType = ModelObjectType.None;
                id = 0;
                return false;
            }

            string[] parts = complexId.Split(new char[] { separator });
            if (parts.Length != 2)
            {
                objectType = (ModelObjectType)0;
                id = 0;
                return false;
            }

            int objectTypeInt;
            int idInt;
            var intSuccess0 = int.TryParse(parts[0], out objectTypeInt);
            var intSuccess1 = int.TryParse(parts[1], out idInt);

            if (!intSuccess0 || !intSuccess1)
            {
                objectType = (ModelObjectType)0;
                id = 0;
                return false;
            }

            objectType = (ModelObjectType)objectTypeInt;
            id = idInt;
            return true;
        }

        public static bool ConvertStringToModelObjectRef(this string complexId, out ModelObjectReference objRef)
        {
            return ConvertStringToModelObjectRef(complexId, ItemPartSeparator, out objRef);
        }

        public static bool ConvertStringToModelObjectRef(this string complexId, char separator, out ModelObjectReference objRef)
        {
            ModelObjectType objectType;
            Guid id;
            var result = ConvertStringToComplexId(complexId, separator, out objectType, out id);

            objRef = new ModelObjectReference(objectType, id);
            return result;
        }

        public static ModelObjectReference ConvertStringToModelObjectRef(this string complexId)
        {
            return ConvertStringToModelObjectRef(complexId, ItemPartSeparator);
        }

        public static ModelObjectReference ConvertStringToModelObjectRef(this string complexId, char separator)
        {
            ModelObjectReference objRef;
            var result = ConvertStringToModelObjectRef(complexId, separator, out objRef);
            return objRef;
        }

        public static string ConvertComplexIdToString(ModelObjectType objectType, Guid id)
        {
            return ConvertComplexIdToString(objectType, id, ItemPartSeparator);
        }

        public static string ConvertComplexIdToString(ModelObjectType objectType, Guid id, char separator)
        {
            return ((int)objectType).ToString() + separator + id.ToString();
        }

        public static string ConvertDimensionalComplexIdToString(ModelObjectType objectType, Guid id, int dimensionNumber)
        {
            return ConvertDimensionalComplexIdToString(objectType, id, dimensionNumber, ItemPartSeparator);
        }

        public static string ConvertDimensionalComplexIdToString(ModelObjectType objectType, Guid id, int dimensionNumber, char separator)
        {
            return ((int)objectType).ToString() + separator + id.ToString() + separator + dimensionNumber.ToString();
        }

        public static string ConvertComplexIdToString(ModelObjectType objectType, int id)
        {
            return ConvertComplexIdToString(objectType, id, ItemPartSeparator);
        }

        public static string ConvertComplexIdToString(ModelObjectType objectType, int id, char separator)
        {
            return ((int)objectType).ToString() + separator + id.ToString();
        }

        public static string ConvertDimensionalComplexIdToString(ModelObjectType objectType, int id, int dimensionNumber)
        {
            return ConvertDimensionalComplexIdToString(objectType, id, dimensionNumber, ItemPartSeparator);
        }

        public static string ConvertDimensionalComplexIdToString(ModelObjectType objectType, int id, int dimensionNumber, char separator)
        {
            return ((int)objectType).ToString() + separator + id.ToString() + separator + dimensionNumber.ToString();
        }

        #endregion

        #region String to ComplexId (StructuralTypeOption) Conversion Methods (LATER: Add "ConvertString" methods with alternate dimension numbers)

        public static bool ConvertStringToComplexId(this string complexId, out StructuralTypeOption structuralType, out Guid id)
        {
            ModelObjectType objectType;
            Guid objectId;
            bool success = complexId.ConvertStringToComplexId(out objectType, out objectId);

            if (!success)
            {
                structuralType = StructuralTypeOption.GlobalType;
                id = Guid.Empty;
                return false;
            }

            structuralType = objectType.GetStructuralType();
            id = objectId;
            return true;
        }

        public static bool ConvertStringToComplexId(this string complexId, out StructuralTypeOption structuralType, out int id)
        {
            ModelObjectType objectType;
            int objectId;
            bool success = complexId.ConvertStringToComplexId(out objectType, out objectId);

            if (!success)
            {
                structuralType = StructuralTypeOption.GlobalType;
                id = 0;
                return false;
            }

            structuralType = objectType.GetStructuralType();
            id = objectId;
            return true;
        }

        public static string ConvertComplexIdToString(StructuralTypeOption objectType, Guid id)
        {
            ModelObjectType modelObjectType = objectType.GetModelObjectType();
            return ConvertComplexIdToString(modelObjectType, id);
        }

        public static string ConvertComplexIdToString(StructuralTypeOption objectType, int id)
        {
            ModelObjectType modelObjectType = objectType.GetModelObjectType();
            return ConvertComplexIdToString(modelObjectType, id);
        }

        #endregion

        #region DateTime Precision Methods

        public static DateTime ConvertToPrecision(this DateTime original, DateTimePrecisionType precisionType)
        {
            if (precisionType == DateTimePrecisionType.Minute)
            {
                DateTime converted = new DateTime(original.Year, original.Month, original.Day, original.Hour, original.Minute, 0);
                return converted;
            }
            else if (precisionType == DateTimePrecisionType.Second)
            {
                DateTime converted = new DateTime(original.Year, original.Month, original.Day, original.Hour, original.Minute, original.Second);
                return converted;
            }
            else if (precisionType == DateTimePrecisionType.Millisecond)
            {
                DateTime converted = original;
                return converted;
            }
            else
            { throw new InvalidOperationException("Unsupported DateTimePrecisionType encountered."); }
        }

        public static bool AreEqualForPrecision(this Nullable<DateTime> firstNullable, Nullable<DateTime> secondNullable, DateTimePrecisionType precisionType)
        {
            if (firstNullable.HasValue ^ secondNullable.HasValue)
            { return false; }

            if (!firstNullable.HasValue && !secondNullable.HasValue)
            { return true; }

            DateTime first = firstNullable.Value;
            DateTime second = secondNullable.Value;

            if (first == second)
            { return true; }

            int validRange = 1;

            if (precisionType == DateTimePrecisionType.Second)
            { validRange = MilliSecondsInSecond; }
            else if (precisionType == DateTimePrecisionType.Minute)
            { validRange = SecondsInMinute * MilliSecondsInSecond; }

            if (first > second)
            { return (first <= second.AddMilliseconds(validRange)); }
            else
            { return (second <= first.AddMilliseconds(validRange)); }
        }

        #endregion

        #region Collection Conversion Methods

        public static string ConvertToCollectionAsString<T>(this IEnumerable<T> collection)
        {
            return ConvertToCollectionAsString(collection, ListItemSeparator.ToString());
        }

        public static string ConvertToCollectionAsString<T>(this IEnumerable<T> collection, string separator)
        {
            string listAsString = string.Empty;
            foreach (T item in collection)
            {
                string itemAsString = ParseManager.GetAsString<T>(item);

                if (string.IsNullOrWhiteSpace(listAsString))
                { listAsString = item.ToString(); }
                else
                { listAsString += separator + itemAsString; }
            }
            return listAsString;
        }

        public static List<T> ConvertToTypedCollection<T>(this string listAsString)
        {
            ParseManager.AssertParsingIsSupported<T>();

            List<T> list = new List<T>();
            if (string.IsNullOrWhiteSpace(listAsString))
            { return list; }

            string[] valuesAsString = listAsString.Split(new char[] { ListItemSeparator });

            foreach (string valueAsString in valuesAsString)
            {
                if (string.IsNullOrWhiteSpace(valueAsString))
                { continue; }

                T typedValue = ParseManager.DoParse<T>(valueAsString);
                list.Add(typedValue);
            }
            return list;
        }

        public static string ConvertToDictionaryAsString<K, V>(this IDictionary<K, V> dictionary)
        {
            string dictionaryAsString = string.Empty;
            foreach (K key in dictionary.Keys)
            {
                V value = dictionary[key];

                string keyAsString = ParseManager.GetAsString<K>(key);
                string valueAsString = ParseManager.GetAsString<V>(value);

                string itemAsString = string.Format(TwoPartItemFormat, keyAsString, valueAsString);

                if (!string.IsNullOrWhiteSpace(valueAsString))
                {
                    if (valueAsString.Contains(ItemPartSeparator) || valueAsString.Contains(ListItemSeparator))
                    { itemAsString = string.Format(TwoPartItemFormat, keyAsString, string.Format(NestedItemFormat, valueAsString)); }
                }

                if (string.IsNullOrWhiteSpace(dictionaryAsString))
                { dictionaryAsString = itemAsString; }
                else
                { dictionaryAsString += ListItemSeparator + itemAsString; }
            }
            return dictionaryAsString;
        }

        public static Dictionary<K, V> ConvertToTypedDictionary<K, V>(this string dictionaryAsString)
        {
            ParseManager.AssertParsingIsSupported<K>();
            ParseManager.AssertParsingIsSupported<V>();

            Dictionary<K, V> dictionary = new Dictionary<K, V>();
            if (string.IsNullOrWhiteSpace(dictionaryAsString))
            { return dictionary; }

            if (dictionaryAsString[0] == NestedItemStart)
            { dictionaryAsString = dictionaryAsString.Substring(1, dictionaryAsString.Length - 2); }

            List<string> nestedItemsAsString = new List<string>();
            foreach (string nestedItemAsString in dictionaryAsString.Split(new char[] { NestedItemStart }))
            {
                if (string.IsNullOrWhiteSpace(nestedItemAsString))
                { continue; }

                if (nestedItemAsString.Contains(NestedItemEnd))
                {
                    bool first = true;
                    foreach (string nestedNestedItemAsString in nestedItemAsString.Split(new char[] { NestedItemEnd }))
                    {
                        if (string.IsNullOrWhiteSpace(nestedNestedItemAsString))
                        { continue; }

                        if (first)
                        {
                            nestedItemsAsString.Add(nestedNestedItemAsString + NestedItemEnd);
                            first = false;
                        }
                        else
                        { nestedItemsAsString.Add(nestedNestedItemAsString); }
                    }
                }
                else
                {
                    nestedItemsAsString.Add(nestedItemAsString);
                }
            }


            List<string> itemsAsString = new List<string>();
            foreach (string nestedItemAsString in nestedItemsAsString)
            {
                if (nestedItemAsString.Contains(NestedItemEnd))
                {
                    string[] primaryItemsAsString = nestedItemAsString.Split(new char[] { NestedItemEnd });
                    itemsAsString[itemsAsString.Count - 1] = itemsAsString[itemsAsString.Count - 1] + NestedItemStart + primaryItemsAsString[0] + NestedItemEnd;

                    primaryItemsAsString = primaryItemsAsString[1].Split(new char[] { ListItemSeparator });
                    if (primaryItemsAsString.Length > 1)
                    { itemsAsString.Add(primaryItemsAsString[1]); }
                }
                else
                {
                    string[] primaryItemsAsString = nestedItemAsString.Split(new char[] { ListItemSeparator });
                    itemsAsString.AddRange(primaryItemsAsString);
                }
            }


            foreach (string itemAsString in itemsAsString)
            {
                if (string.IsNullOrWhiteSpace(itemAsString))
                { continue; }

                string[] itemParts = itemAsString.Split(new char[] { ItemPartSeparator }, 2);

                string keyPart = itemParts[0];
                string valuePart = (itemParts.Length > 1) ? itemParts[1] : string.Empty;

                K key = default(K);
                key = ParseManager.DoParse<K>(keyPart);

                V value = default(V);
                if (!string.IsNullOrWhiteSpace(valuePart))
                {
                    if ((valuePart[0] == NestedItemStart) || (valuePart[valuePart.Length - 1] == NestedItemEnd))
                    {
                        valuePart = valuePart.Remove(0, 1);
                        valuePart = valuePart.Remove(valuePart.Length - 1, 1);
                    }
                    if (!string.IsNullOrWhiteSpace(valuePart))
                    {
                        value = ParseManager.DoParse<V>(valuePart);
                    }
                }

                dictionary.Add(key, value);
            }
            return dictionary;
        }

        #endregion

        #region Nullable Conversion Methods

        public static Nullable<Guid> ConvertToNullableGuid<T>(this Nullable<T> nullableId)
            where T : struct, IGuidId
        {
            Func<T, Guid> conversionMethod = ((T id) => id.InnerGuid);
            return ConvertToNullableGuid<T>(nullableId, conversionMethod);
        }

        public static Nullable<Guid> ConvertToNullableGuid<T>(this Nullable<T> nullableId, Func<T, Guid> conversionMethod)
            where T : struct
        {
            if (!nullableId.HasValue)
            { return null; }

            Guid guid = conversionMethod(nullableId.Value);
            return guid;
        }

        public static Nullable<T> ConvertToNullableId<T>(this Nullable<Guid> nullableGuid, Func<Guid, T> conversionMethod)
            where T : struct
        {
            if (!nullableGuid.HasValue)
            { return null; }

            T id = conversionMethod(nullableGuid.Value);
            return id;
        }

        #endregion

        #region Rectangle to String Conversion Methods

        public static string ConvertToRectangleAsString(this Rectangle rectangle)
        {
            string item0 = string.Format(TwoPartItemFormat, "Left", rectangle.Left.ToString());
            string item1 = string.Format(TwoPartItemFormat, "Top", rectangle.Top.ToString());
            string item2 = string.Format(TwoPartItemFormat, "Width", rectangle.Width.ToString());
            string item3 = string.Format(TwoPartItemFormat, "Height", rectangle.Height.ToString());
            string rectangleAsString = string.Format(FourItemListFormat, item0, item1, item2, item3);
            return rectangleAsString;
        }

        public static Rectangle ConvertToRectangleStruct(this string rectangleAsString)
        {
            string[] itemsAsStrings = rectangleAsString.Split(new char[] { ListItemSeparator });
            string item0AsString = itemsAsStrings[0];
            string item1AsString = itemsAsStrings[1];
            string item2AsString = itemsAsStrings[2];
            string item3AsString = itemsAsStrings[3];

            int left = int.Parse(item0AsString.Split(new char[] { ItemPartSeparator })[1]);
            int top = int.Parse(item1AsString.Split(new char[] { ItemPartSeparator })[1]);
            int width = int.Parse(item2AsString.Split(new char[] { ItemPartSeparator })[1]);
            int height = int.Parse(item3AsString.Split(new char[] { ItemPartSeparator })[1]);

            Rectangle rectangle = new Rectangle(left, top, width, height);
            return rectangle;
        }

        #endregion

        #region Enum Handling Methods

        public static string ConvertEnumValueToDescription(this Type enumType, string enumValue)
        {
            return ConvertEnumValueToDescription(enumType, enumValue, new object[] { });
        }

        public static string ConvertEnumValueToDescription(this Type enumType, string enumValue, object formatArg)
        {
            return ConvertEnumValueToDescription(enumType, enumValue, new object[] { formatArg });
        }

        public static string ConvertEnumValueToDescription(this Type enumType, string enumValue, object[] formatArgs)
        {
            MemberInfo[] memInfos = enumType.GetMember(enumValue);
            object[] attributes = memInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            string format = ((DescriptionAttribute)attributes[0]).Description;
            string value = string.Format(format, formatArgs);
            return value;
        }

        public static SortedDictionary<string, string> GetEnumOptions_StringString(this Type enumType)
        {
            IDictionary<int, string> enumOptions = GetEnumOptions_IntString(enumType);
            return new SortedDictionary<string, string>(enumOptions.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value));
        }

        public static SortedDictionary<int, string> GetEnumOptions_IntString(this Type enumType)
        {
            return GetEnumOptions_IntString(enumType, string.Empty);
        }

        public static SortedDictionary<string, string> GetEnumOptions_StringString(this Type enumType, object nameFormatArg)
        {
            IDictionary<int, string> enumOptions = GetEnumOptions_IntString(enumType, nameFormatArg);
            return new SortedDictionary<string, string>(enumOptions.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value));
        }

        public static SortedDictionary<int, string> GetEnumOptions_IntString(this Type enumType, object nameFormatArg)
        {
            return GetEnumOptions_IntString(enumType, new object[] { nameFormatArg });
        }

        public static SortedDictionary<string, string> GetEnumOptions_StringString(this Type enumType, object[] nameFormatArgs)
        {
            IDictionary<int, string> enumOptions = GetEnumOptions_IntString(enumType, nameFormatArgs);
            return new SortedDictionary<string, string>(enumOptions.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value));
        }

        public static SortedDictionary<int, string> GetEnumOptions_IntString(this Type enumType, object[] nameFormatArgs)
        {
            SortedDictionary<int, string> enumOptions = new SortedDictionary<int, string>();

            foreach (object enumValue in Enum.GetValues(enumType))
            {
                enumOptions.Add((int)enumValue, enumType.ConvertEnumValueToDescription(enumValue.ToString(), nameFormatArgs));
            }
            return enumOptions;
        }

        #endregion

        #region Repository to IQueryable Conversion Method

        public static IQueryable<KEYED_DOMAIN_OBJECT> GetActualDataStore<KEY, KEYED_DOMAIN_OBJECT>(this IReadOnlyRepository<KEY, KEYED_DOMAIN_OBJECT> repository)
            where KEYED_DOMAIN_OBJECT : class, DomainDriver.DomainModeling.DomainObjects.IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>
        {
            IQueryable<KEYED_DOMAIN_OBJECT> storedObjects = null;

            if (repository.CurrentPersistenceType == DataSourcePersistenceType.None)
            {
                storedObjects = new EnumerableQuery<KEYED_DOMAIN_OBJECT>(repository.Read());
            }
            else if (repository.CurrentPersistenceType == DataSourcePersistenceType.Database)
            {
                var dbContext = (DbContext)repository.CurrentDataSource;
                storedObjects = dbContext.Set<KEYED_DOMAIN_OBJECT>();
            }
            else
            { throw new InvalidOperationException("Unrecognized DataSourcePersistenceType encountered."); }

            return storedObjects;
        }

        #endregion

        #region CodeName to Text Method

        public static string GetTextForType(this Type type)
        {
            var typeName = type.Name;
            typeName = typeName.Contains("Report") ? typeName.Replace("Report", "ReportTemplate") : typeName;
            typeName = typeName.GetTextForCodeName();
            return typeName;
        }

        public static string GetTextForCodeName(this string codeName)
        {
            if (string.IsNullOrWhiteSpace(codeName))
            { return string.Empty; }

            var text = string.Empty;
            var lastChar = (char?)null;

            for (int i = 0; i < codeName.Length; i++)
            {
                var currChar = (char)codeName[i];

                if (!lastChar.HasValue)
                { text += currChar; }
                else
                { text += (char.IsLower(lastChar.Value) && char.IsUpper(currChar)) ? (" " + currChar) : ("" + currChar); }

                lastChar = currChar;
            }
            return text;
        }

        #endregion
    }
}