using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Styling
{
    public enum DeciaFontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8,
        Underline_Double = (16 | Underline),
    }

    public enum Editor_FontDisplayStyle
    {
        [Description("Normal")]
        Normal,
        [Description("Bold")]
        Bold,
        [Description("Italic")]
        Italic,
        [Description("Bold Italic")]
        Bold_Italic,
    }

    public enum Editor_FontLineStyle
    {
        [Description("None")]
        None,
        [Description("Underline")]
        Underline,
        [Description("Strikethrough")]
        Strikethrough,
    }

    public static class DeciaFontStyleUtils
    {
        public static FontStyle GetNetFontStyle(this DeciaFontStyle fontStyle)
        {
            var netFontStyle = FontStyle.Regular;

            if (fontStyle.IsBold())
            { netFontStyle = (netFontStyle | FontStyle.Bold); }
            else if (fontStyle.IsItalic())
            { netFontStyle = (netFontStyle | FontStyle.Italic); }
            else if (fontStyle.IsUnderline())
            { netFontStyle = (netFontStyle | FontStyle.Underline); }
            else if (fontStyle.IsStrikeout())
            { netFontStyle = (netFontStyle | FontStyle.Strikeout); }
            else if (fontStyle.IsDoubleUnderline())
            { netFontStyle = (netFontStyle | FontStyle.Underline); }
            else
            { throw new InvalidOperationException("Unrecognized DeciaFontStyle encountered."); }

            return netFontStyle;
        }

        public static Editor_FontDisplayStyle GetFontDisplayStyle(this DeciaFontStyle fontStyle)
        {
            if (fontStyle.IsBold() && fontStyle.IsItalic())
            { return Editor_FontDisplayStyle.Bold_Italic; }
            else if (fontStyle.IsBold())
            { return Editor_FontDisplayStyle.Bold; }
            else if (fontStyle.IsItalic())
            { return Editor_FontDisplayStyle.Italic; }
            else
            { return Editor_FontDisplayStyle.Normal; }
        }

        public static Editor_FontLineStyle GetFontLineStyle(this DeciaFontStyle fontStyle)
        {
            if (fontStyle.IsUnderline())
            { return Editor_FontLineStyle.Underline; }
            else if (fontStyle.IsStrikeout())
            { return Editor_FontLineStyle.Strikethrough; }
            else
            { return Editor_FontLineStyle.None; }
        }

        public static DeciaFontStyle MergeFontStyles(this DeciaFontStyle baseStyle, Editor_FontDisplayStyle? fontDisplayStyle, Editor_FontLineStyle? fontLineStyle)
        {
            var updatedStyle = DeciaFontStyle.Regular;
            fontDisplayStyle = (fontDisplayStyle.HasValue) ? fontDisplayStyle.Value : Editor_FontDisplayStyle.Normal;
            fontLineStyle = (fontLineStyle.HasValue) ? fontLineStyle.Value : Editor_FontLineStyle.None;

            if ((fontDisplayStyle == Editor_FontDisplayStyle.Bold) || (fontDisplayStyle == Editor_FontDisplayStyle.Bold_Italic))
            { updatedStyle |= DeciaFontStyle.Bold; }

            if ((fontDisplayStyle == Editor_FontDisplayStyle.Italic) || (fontDisplayStyle == Editor_FontDisplayStyle.Bold_Italic))
            { updatedStyle |= DeciaFontStyle.Italic; }

            if (fontLineStyle == Editor_FontLineStyle.Underline)
            { updatedStyle |= DeciaFontStyle.Underline; }

            if (fontLineStyle == Editor_FontLineStyle.Strikethrough)
            { updatedStyle |= DeciaFontStyle.Strikeout; }

            return updatedStyle;
        }

        #region Bool Getters

        public static bool IsBold(this DeciaFontStyle fontStyle)
        {
            return (fontStyle == (fontStyle | DeciaFontStyle.Bold));
        }

        public static bool IsItalic(this DeciaFontStyle fontStyle)
        {
            return (fontStyle == (fontStyle | DeciaFontStyle.Italic));
        }

        public static bool IsUnderline(this DeciaFontStyle fontStyle)
        {
            return (fontStyle == (fontStyle | DeciaFontStyle.Underline));
        }

        public static bool IsStrikeout(this DeciaFontStyle fontStyle)
        {
            return (fontStyle == (fontStyle | DeciaFontStyle.Strikeout));
        }

        public static bool IsDoubleUnderline(this DeciaFontStyle fontStyle)
        {
            return (fontStyle == (fontStyle | DeciaFontStyle.Underline_Double));
        }

        #endregion
    }
}