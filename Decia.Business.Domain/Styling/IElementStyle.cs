using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;

namespace Decia.Business.Domain.Styling
{
    public interface IElementStyle
    {
        IEditabilitySpecification EditabilitySpec { get; set; }

        bool DefaultStyle_HasValue { get; }
        IElementStyle DefaultStyle_Value { get; set; }
        void ResetTo_DefaultStyle();

        bool BackColor_HasValue { get; }
        ColorSpecification BackColor_Value { get; set; }
        void BackColor_ResetValue();

        bool ForeColor_HasValue { get; }
        ColorSpecification ForeColor_Value { get; set; }
        void ForeColor_ResetValue();

        bool FontName_HasValue { get; }
        string FontName_Value { get; set; }
        void FontName_ResetValue();

        bool FontSize_HasValue { get; }
        double FontSize_Value { get; set; }
        void FontSize_ResetValue();

        bool FontStyle_HasValue { get; }
        DeciaFontStyle FontStyle_Value { get; set; }
        void FontStyle_ResetValue();

        bool FontHAlign_HasValue { get; }
        HAlignment FontHAlign_Value { get; set; }
        void FontHAlign_ResetValue();

        bool FontVAlign_HasValue { get; }
        VAlignment FontVAlign_Value { get; set; }
        void FontVAlign_ResetValue();

        bool Indent_HasValue { get; }
        int Indent_Value { get; set; }
        void Indent_ResetValue();

        BoxStyleValue<bool> BorderColor_HasValue { get; }
        BoxStyleValue<ColorSpecification> BorderColor_Value { get; set; }
        void BorderColor_ResetValue();

        BoxStyleValue<bool> BorderStyle_HasValue { get; }
        BoxStyleValue<BorderLineStyle> BorderStyle_Value { get; set; }
        void BorderStyle_ResetValue();
    }
}