using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;

namespace Decia.Business.Domain.Styling
{
    public class ElementStyle : IElementStyle
    {
        protected IEditabilitySpecification m_EditabilitySpec;
        protected IElementStyle m_DefaultStyle;

        protected Nullable<ColorSpecification> m_BackColor;
        protected Nullable<ColorSpecification> m_ForeColor;
        protected string m_FontName;
        protected Nullable<double> m_FontSize;
        protected Nullable<DeciaFontStyle> m_FontStyle;
        protected Nullable<HAlignment> m_FontHAlign;
        protected Nullable<VAlignment> m_FontVAlign;
        protected Nullable<int> m_Indent;
        protected BoxStyleValue<Nullable<ColorSpecification>> m_BorderColor;
        protected BoxStyleValue<Nullable<BorderLineStyle>> m_BorderStyle;

        public ElementStyle()
            : this(new NoOpEditabilitySpecification())
        { }

        public ElementStyle(IEditabilitySpecification editabilitySpec)
        {
            m_EditabilitySpec = (editabilitySpec != null) ? editabilitySpec : new NoOpEditabilitySpecification();
            m_DefaultStyle = null;

            ResetTo_DefaultStyle();
        }

        public ElementStyle(ElementStyle styleToCopy)
            : this(styleToCopy, styleToCopy.m_EditabilitySpec)
        { }

        public ElementStyle(ElementStyle styleToCopy, IEditabilitySpecification editabilitySpec)
        {
            m_EditabilitySpec = (editabilitySpec != null) ? editabilitySpec : new NoOpEditabilitySpecification();
            m_DefaultStyle = styleToCopy.m_DefaultStyle;

            m_BackColor = styleToCopy.m_BackColor;
            m_ForeColor = styleToCopy.m_ForeColor;
            m_FontName = styleToCopy.m_FontName;
            m_FontSize = styleToCopy.m_FontSize;
            m_FontStyle = styleToCopy.m_FontStyle;
            m_FontHAlign = styleToCopy.m_FontHAlign;
            m_FontVAlign = styleToCopy.m_FontVAlign;
            m_Indent = styleToCopy.m_Indent;
            m_BorderColor = styleToCopy.m_BorderColor;
            m_BorderStyle = styleToCopy.m_BorderStyle;
        }

        [NotMapped]
        public IEditabilitySpecification EditabilitySpec
        {
            get { return m_EditabilitySpec; }
            set
            {
                if (value == null)
                { value = new NoOpEditabilitySpecification(); }

                m_EditabilitySpec = value;
            }
        }

        [NotMapped]
        public bool DefaultStyle_HasValue
        {
            get { return (m_DefaultStyle != null); }
        }

        [NotMapped]
        public IElementStyle DefaultStyle_Value
        {
            get { return m_DefaultStyle; }
            set { m_DefaultStyle = value; }
        }

        public void ResetTo_DefaultStyle()
        {
            m_BackColor = null;
            m_ForeColor = null;
            m_FontName = null;
            m_FontSize = null;
            m_FontStyle = null;
            m_FontHAlign = null;
            m_FontVAlign = null;
            m_Indent = null;
            m_BorderColor = new BoxStyleValue<Nullable<ColorSpecification>>(null, null, null, null);
            m_BorderStyle = new BoxStyleValue<Nullable<BorderLineStyle>>(null, null, null, null);
        }

        [NotMapped]
        public bool BackColor_HasValue
        {
            get { return m_BackColor.HasValue; }
        }

        [NotMapped]
        public ColorSpecification BackColor_Value
        {
            get
            {
                if (BackColor_HasValue)
                { return m_BackColor.Value; }

                if (DefaultStyle_HasValue)
                { return DefaultStyle_Value.BackColor_Value; }

                return IElementStyleUtils.BackColor_DefaultSpec;
            }
            set
            {
                if (!value.IsValid)
                { throw new InvalidOperationException("The specified ColorSpecification is not valid."); }

                m_BackColor = value;
            }
        }

        public void BackColor_ResetValue()
        { m_BackColor = null; }

        [NotMapped]
        public bool ForeColor_HasValue
        {
            get { return m_ForeColor.HasValue; }
        }

        [NotMapped]
        public ColorSpecification ForeColor_Value
        {
            get
            {
                if (ForeColor_HasValue)
                { return m_ForeColor.Value; }

                if (DefaultStyle_HasValue)
                { return DefaultStyle_Value.ForeColor_Value; }

                return IElementStyleUtils.ForeColor_DefaultSpec;
            }
            set
            {
                if (!value.IsValid)
                { throw new InvalidOperationException("The specified ColorSpecification is not valid."); }

                m_ForeColor = value;
            }
        }

        public void ForeColor_ResetValue()
        { m_ForeColor = null; }

        [NotMapped]
        public bool FontName_HasValue
        {
            get { return !string.IsNullOrWhiteSpace(m_FontName); }
        }

        [NotMapped]
        public string FontName_Value
        {
            get
            {
                if (FontName_HasValue)
                { return m_FontName; }

                if (DefaultStyle_HasValue)
                { return DefaultStyle_Value.FontName_Value; }

                return IElementStyleUtils.FontName_Default;
            }
            set
            {
                m_FontName = value;
            }
        }

        public void FontName_ResetValue()
        { m_FontName = null; }

        [NotMapped]
        public bool FontSize_HasValue
        {
            get { return m_FontSize.HasValue; }
        }

        [NotMapped]
        public double FontSize_Value
        {
            get
            {
                if (FontSize_HasValue)
                { return m_FontSize.Value; }

                if (DefaultStyle_HasValue)
                { return DefaultStyle_Value.FontSize_Value; }

                return IElementStyleUtils.FontSize_Default;
            }
            set
            {
                value.FontSize_AssertIsValid();

                m_FontSize = value;
            }
        }

        public void FontSize_ResetValue()
        { m_FontSize = null; }

        [NotMapped]
        public bool FontStyle_HasValue
        {
            get { return m_FontStyle.HasValue; }
        }

        [NotMapped]
        public DeciaFontStyle FontStyle_Value
        {
            get
            {
                if (FontStyle_HasValue)
                { return m_FontStyle.Value; }

                if (DefaultStyle_HasValue)
                { return DefaultStyle_Value.FontStyle_Value; }

                return IElementStyleUtils.FontStyle_Default;
            }
            set
            {
                m_FontStyle = value;
            }
        }

        public void FontStyle_ResetValue()
        { m_FontStyle = null; }

        [NotMapped]
        public bool FontHAlign_HasValue
        {
            get { return m_FontHAlign.HasValue; }
        }

        [NotMapped]
        public HAlignment FontHAlign_Value
        {
            get
            {
                if (FontHAlign_HasValue)
                { return m_FontHAlign.Value; }

                if (DefaultStyle_HasValue)
                { return DefaultStyle_Value.FontHAlign_Value; }

                return IElementStyleUtils.FontHAlign_Default;
            }
            set
            {
                m_FontHAlign = value;
            }
        }

        public void FontHAlign_ResetValue()
        { m_FontHAlign = null; }

        [NotMapped]
        public bool FontVAlign_HasValue
        {
            get { return m_FontVAlign.HasValue; }
        }

        [NotMapped]
        public VAlignment FontVAlign_Value
        {
            get
            {
                if (FontVAlign_HasValue)
                { return m_FontVAlign.Value; }

                if (DefaultStyle_HasValue)
                { return DefaultStyle_Value.FontVAlign_Value; }

                return IElementStyleUtils.FontVAlign_Default;
            }
            set
            {
                m_FontVAlign = value;
            }
        }

        public void FontVAlign_ResetValue()
        { m_FontVAlign = null; }

        [NotMapped]
        public bool Indent_HasValue
        {
            get { return m_Indent.HasValue; }
        }

        [NotMapped]
        public int Indent_Value
        {
            get
            {
                if (Indent_HasValue)
                { return m_Indent.Value; }

                if (DefaultStyle_HasValue)
                { return DefaultStyle_Value.Indent_Value; }

                return IElementStyleUtils.Indent_Default;
            }
            set
            {
                if (value < 0)
                { throw new InvalidOperationException("The specified Indent is not valid."); }

                m_Indent = value;
            }
        }

        public void Indent_ResetValue()
        { m_Indent = null; }

        [NotMapped]
        public BoxStyleValue<bool> BorderColor_HasValue
        {
            get
            {
                bool hasLeft = m_BorderColor.Left.HasValue;
                bool hasTop = m_BorderColor.Top.HasValue;
                bool hasRight = m_BorderColor.Right.HasValue;
                bool hasBottom = m_BorderColor.Bottom.HasValue;

                return new BoxStyleValue<bool>(hasLeft, hasTop, hasRight, hasBottom);
            }
        }

        [NotMapped]
        public BoxStyleValue<ColorSpecification> BorderColor_Value
        {
            get
            {
                Func<ColorSpecification?> baseDefaultLeftGetter = (() => (DefaultStyle_Value != null) ? (ColorSpecification?)DefaultStyle_Value.BorderColor_Value.Left : (ColorSpecification?)null);
                Func<ColorSpecification?> baseDefaultTopGetter = (() => (DefaultStyle_Value != null) ? (ColorSpecification?)DefaultStyle_Value.BorderColor_Value.Top : (ColorSpecification?)null);
                Func<ColorSpecification?> baseDefaultRightGetter = (() => (DefaultStyle_Value != null) ? (ColorSpecification?)DefaultStyle_Value.BorderColor_Value.Right : (ColorSpecification?)null);
                Func<ColorSpecification?> baseDefaultBottomGetter = (() => (DefaultStyle_Value != null) ? (ColorSpecification?)DefaultStyle_Value.BorderColor_Value.Bottom : (ColorSpecification?)null);

                ColorSpecification left = StylingUtils.GetValueToUse(m_BorderColor.Left, baseDefaultLeftGetter, IElementStyleUtils.BorderColor_DefaultSpec);
                ColorSpecification top = StylingUtils.GetValueToUse(m_BorderColor.Top, baseDefaultTopGetter, IElementStyleUtils.BorderColor_DefaultSpec);
                ColorSpecification right = StylingUtils.GetValueToUse(m_BorderColor.Right, baseDefaultRightGetter, IElementStyleUtils.BorderColor_DefaultSpec);
                ColorSpecification bottom = StylingUtils.GetValueToUse(m_BorderColor.Bottom, baseDefaultBottomGetter, IElementStyleUtils.BorderColor_DefaultSpec);

                return new BoxStyleValue<ColorSpecification>(left, top, right, bottom);
            }
            set
            {
                m_BorderColor = new BoxStyleValue<ColorSpecification?>(value.Left, value.Top, value.Right, value.Bottom);
            }
        }

        public void BorderColor_ResetValue()
        { m_BorderColor = new BoxStyleValue<ColorSpecification?>(null, null, null, null); }

        [NotMapped]
        public BoxStyleValue<bool> BorderStyle_HasValue
        {
            get
            {
                bool hasLeft = m_BorderStyle.Left.HasValue;
                bool hasTop = m_BorderStyle.Top.HasValue;
                bool hasRight = m_BorderStyle.Right.HasValue;
                bool hasBottom = m_BorderStyle.Bottom.HasValue;

                return new BoxStyleValue<bool>(hasLeft, hasTop, hasRight, hasBottom);
            }
        }

        [NotMapped]
        public BoxStyleValue<BorderLineStyle> BorderStyle_Value
        {
            get
            {
                Func<BorderLineStyle?> baseDefaultLeftGetter = (() => (DefaultStyle_Value != null) ? (BorderLineStyle?)DefaultStyle_Value.BorderStyle_Value.Left : (BorderLineStyle?)null);
                Func<BorderLineStyle?> baseDefaultTopGetter = (() => (DefaultStyle_Value != null) ? (BorderLineStyle?)DefaultStyle_Value.BorderStyle_Value.Top : (BorderLineStyle?)null);
                Func<BorderLineStyle?> baseDefaultRightGetter = (() => (DefaultStyle_Value != null) ? (BorderLineStyle?)DefaultStyle_Value.BorderStyle_Value.Right : (BorderLineStyle?)null);
                Func<BorderLineStyle?> baseDefaultBottomGetter = (() => (DefaultStyle_Value != null) ? (BorderLineStyle?)DefaultStyle_Value.BorderStyle_Value.Bottom : (BorderLineStyle?)null);

                BorderLineStyle left = StylingUtils.GetValueToUse(m_BorderStyle.Left, baseDefaultLeftGetter, IElementStyleUtils.BorderStyle_Default);
                BorderLineStyle top = StylingUtils.GetValueToUse(m_BorderStyle.Top, baseDefaultTopGetter, IElementStyleUtils.BorderStyle_Default);
                BorderLineStyle right = StylingUtils.GetValueToUse(m_BorderStyle.Right, baseDefaultRightGetter, IElementStyleUtils.BorderStyle_Default);
                BorderLineStyle bottom = StylingUtils.GetValueToUse(m_BorderStyle.Bottom, baseDefaultBottomGetter, IElementStyleUtils.BorderStyle_Default);

                return new BoxStyleValue<BorderLineStyle>(left, top, right, bottom);
            }
            set
            {
                m_BorderStyle = new BoxStyleValue<BorderLineStyle?>(value.Left, value.Top, value.Right, value.Bottom);
            }
        }

        public void BorderStyle_ResetValue()
        { m_BorderStyle = new BoxStyleValue<BorderLineStyle?>(null, null, null, null); }
    }
}