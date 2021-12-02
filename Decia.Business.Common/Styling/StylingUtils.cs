using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Styling
{
    public static class StylingUtils
    {
        public static T GetValueToUse<T>(Nullable<T> actualValue, Nullable<T> baseDefaultValue, T absoluteDefaultValue)
            where T : struct
        {
            return GetValueToUse(actualValue, (() => baseDefaultValue), absoluteDefaultValue);
        }

        public static T GetValueToUse<T>(Nullable<T> actualValue, Func<Nullable<T>> baseDefaultValueGetter, T absoluteDefaultValue)
            where T : struct
        {
            if (actualValue.HasValue)
            { return actualValue.Value; }

            T? baseDefaultValue = baseDefaultValueGetter();

            if (baseDefaultValue.HasValue)
            { return baseDefaultValue.Value; }

            return absoluteDefaultValue;
        }

        public static Nullable<int> SaveEnumAsInt<T>(Nullable<T> enumValue)
            where T : struct
        {
            if (!enumValue.HasValue)
            { return null; }

            return (int)((object)enumValue.Value);
        }

        public static void LoadEnumFromInt<T>(Nullable<int> savedInt, out Nullable<T> enumValue)
            where T : struct
        {
            if (!savedInt.HasValue)
            { enumValue = null; }
            else
            { enumValue = (T)((object)savedInt.Value); }
        }

        #region Color Utils

        public static Color GetColor(this ColorSpecification colorSpec)
        {
            return Color.FromArgb(colorSpec.Alpha, colorSpec.Red, colorSpec.Green, colorSpec.Blue);
        }

        public static ColorSpecification GetColorSpec(this Color color)
        {
            return new ColorSpecification(color);
        }

        public static string GetColorAsString(this Color color)
        {
            return GetColorAsString(new ColorSpecification(color));
        }

        public static string GetColorAsString(this ColorSpecification color)
        {
            return GetColorAsString(color.Alpha, color.Red, color.Green, color.Blue);
        }

        public static string GetColorAsString(this int alpha, int r, int g, int b)
        {
            return (alpha.ToString("X2") + r.ToString("X2") + g.ToString("X2") + b.ToString("X2"));
        }

        #endregion

        #region Position Utils

        public static readonly BoxStyleValue<int> ZeroBoxStyleValue = new BoxStyleValue<int>(0, 0, 0, 0);

        public static BoxPosition GetUsablePosition(this BoxPosition fullPosition, BoxStyleValue<int> marginOrPadding)
        { return GetUsablePosition(fullPosition, marginOrPadding, ZeroBoxStyleValue); }

        public static BoxPosition GetUsablePosition(this BoxPosition fullPosition, BoxStyleValue<int> margin, BoxStyleValue<int> padding)
        {
            int leftReducer = margin.Left + padding.Left;
            int topReducer = margin.Top + padding.Top;
            int rightReducer = margin.Right + padding.Right;
            int bottomReducer = margin.Bottom + padding.Bottom;

            int left = fullPosition.Left + leftReducer;
            int top = fullPosition.Top + topReducer;
            int width = fullPosition.Width - (leftReducer + rightReducer);
            int height = fullPosition.Height - (topReducer + bottomReducer);

            return new BoxPosition(left, top, width, height);
        }

        #endregion
    }
}