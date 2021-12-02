using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;

namespace Decia.Business.Domain.Styling
{
    public static class IElementStyleUtils
    {
        public static readonly Color BackColor_Default = Color.Transparent;
        public static readonly ColorSpecification BackColor_DefaultSpec = new ColorSpecification(BackColor_Default);
        public static readonly Color ForeColor_Default = Color.Black;
        public static readonly ColorSpecification ForeColor_DefaultSpec = new ColorSpecification(ForeColor_Default);
        public static readonly DeciaFontFamily FontFamily_Default = ((DeciaFontFamily)0);
        public static readonly string FontName_Default = FontFamily_Default.GetFontName();
        public static readonly double FontSize_Min = 0.1;
        public static readonly double FontSize_Default = 11.0;
        public static readonly DeciaFontStyle FontStyle_Default = DeciaFontStyle.Regular;
        public static readonly HAlignment FontHAlign_Default = HAlignment.Left;
        public static readonly VAlignment FontVAlign_Default = VAlignment.Middle;
        public static readonly int Indent_Default = 0;
        public static readonly Color BorderColor_Default = Color.Black;
        public static readonly ColorSpecification BorderColor_DefaultSpec = new ColorSpecification(BorderColor_Default);
        public static readonly BorderLineStyle BorderStyle_Default = BorderLineStyle.None;

        public static bool FontSize_IsValid(this double fontSize)
        {
            return (fontSize >= FontSize_Min);
        }

        public static void FontSize_AssertIsValid(this double fontSize)
        {
            if (!fontSize.FontSize_IsValid())
            { throw new InvalidOperationException("The specified Font Size is not valid."); }
        }
    }
}