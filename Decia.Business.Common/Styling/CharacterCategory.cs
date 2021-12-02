using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Styling
{
    public enum CharacterCategory
    {
        Small,
        Medium,
        Large,
    }

    public static class CharacterCategoryUtils
    {
        public static readonly CharacterCategory Default_CharacterCategory = CharacterCategory.Medium;

        public static readonly string Number_Small = "1";

        public static readonly string LowerCase_Small = "fijlrt";
        public static readonly string LowerCase_Medium = "abcdeghknopqsuvxyz";
        public static readonly string LowerCase_Large = "mw";

        public static readonly string UpperCase_Medium = "IJL";
        public static readonly string UpperCase_Large = "ABCDEFGHKMNOPQRSTUVWXYZ";

        public static CharacterCategory GetCategory(this char value)
        {
            if (Number_Small.Contains(value))
            { return CharacterCategory.Small; }
            if (LowerCase_Small.Contains(value))
            { return CharacterCategory.Small; }
            if (LowerCase_Large.Contains(value))
            { return CharacterCategory.Large; }
            if (UpperCase_Large.Contains(value))
            { return CharacterCategory.Large; }
            return CharacterCategory.Medium;
        }

        public static SortedDictionary<CharacterCategory, int> GetCategoryCounts(this string value)
        {
            var counts = new SortedDictionary<CharacterCategory, int>();

            foreach (var enumValue in EnumUtils.GetEnumValues<CharacterCategory>())
            { counts.Add(enumValue, 0); }

            foreach (var c in value)
            {
                var category = GetCategory(c);
                counts[category]++;
            }
            return counts;
        }

        public static double GetTotalSize(this string value, IDictionary<CharacterCategory, double> categorySizes)
        {
            var counts = value.GetCategoryCounts();
            var totalSize = 0.0;

            foreach (var categoryCount in counts)
            {
                var category = categoryCount.Key;
                var count = categoryCount.Value;
                var size = categorySizes[category];
                totalSize += (count * size);
            }
            return totalSize;
        }
    }
}