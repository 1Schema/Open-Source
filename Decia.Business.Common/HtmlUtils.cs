using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Common
{
    public static class HtmlUtils
    {
        public const string TrueText = "true";
        public const string FalseText = "false";
        public const double MaxSystemAlpha = 255.0;
        public const double MaxCssAlpha = 100.0;
        public const double IndentMultiplier = 8.0;

        #region Boolean Rendering Methods

        public static string ToHtmlText(this bool value)
        {
            return (value ? TrueText : FalseText);
        }

        public static string ToInverseHtmlText(this bool value)
        {
            var inverseValue = (!value);
            return ToHtmlText(inverseValue);
        }

        #endregion

        public static int ConstrainToValidColorValue(int colorValue)
        {
            if (colorValue < byte.MinValue)
            { return byte.MinValue; }
            if (colorValue > byte.MaxValue)
            { return byte.MaxValue; }
            return colorValue;
        }

        public static Color GetShadeOfColor(this Color color, double factor)
        {
            var red = (int)(color.R * factor);
            var green = (int)(color.G * factor);
            var blue = (int)(color.B * factor);

            red = ConstrainToValidColorValue(red);
            green = ConstrainToValidColorValue(green);
            blue = ConstrainToValidColorValue(blue);

            return Color.FromArgb(color.A, red, green, blue);
        }

        public static string GetShadeOfColorToHtml(this Color color, double factor)
        {
            var shadeOfColor = color.GetShadeOfColor(factor);
            return shadeOfColor.ColorToHtml();
        }

        public static Color ColorToColorArgb(this Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static string ColorToHtml(this Color color)
        {
            if (color == Color.Transparent)
            { return ColorTranslator.ToHtml(Color.Transparent); }
            if (color.A == 0)
            { return ColorTranslator.ToHtml(Color.Transparent); }
            return ColorTranslator.ToHtml(color);
        }

        public static double GetCssAlpha(this Color color)
        {
            return (color.A * (MaxCssAlpha / MaxSystemAlpha));
        }

        public static string BorderStyleToHtml(this BorderLineStyle borderStyle)
        {
            if (borderStyle == BorderLineStyle.None)
            { return "none"; }
            else if (borderStyle == BorderLineStyle.Thin)
            { return "solid " + BorderStyleToPx(borderStyle) + "px "; }
            else if (borderStyle == BorderLineStyle.Medium)
            { return "solid " + BorderStyleToPx(borderStyle) + "px "; }
            else if (borderStyle == BorderLineStyle.Thick)
            { return "solid " + BorderStyleToPx(borderStyle) + "px "; }
            else if (borderStyle == BorderLineStyle.Double)
            { return "double " + BorderStyleToPx(borderStyle) + "px "; }
            else
            { throw new InvalidOperationException("Unsupported BorderLineStyle encountered."); }
        }

        public static int BorderStyleToPx(this BorderLineStyle borderStyle)
        {
            if (borderStyle == BorderLineStyle.None)
            { return 0; }
            else if (borderStyle == BorderLineStyle.Thin)
            { return 1; }
            else if (borderStyle == BorderLineStyle.Medium)
            { return 2; }
            else if (borderStyle == BorderLineStyle.Thick)
            { return 3; }
            else if (borderStyle == BorderLineStyle.Double)
            { return 3; }
            else
            { throw new InvalidOperationException("Unsupported BorderLineStyle encountered."); }
        }

        public static string TextDecorationToHtml(bool hasOverline, bool hasLineThrough, bool hasUnderline)
        {
            string textDecoration = string.Empty;

            if (hasOverline)
            { textDecoration += " overline"; }
            if (hasLineThrough)
            { textDecoration += " line-through"; }
            if (hasUnderline)
            { textDecoration += " underline"; }

            if (string.IsNullOrWhiteSpace(textDecoration))
            { textDecoration = " normal"; }

            return textDecoration;
        }
    }
}