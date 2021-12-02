using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DomainObjects;

namespace Decia.Business.Domain.Reporting.Styling
{
    public partial class SaveableElementStyle : IValueObject<SaveableElementStyle>
    {
        internal bool Default_CheckDefaultStyle_Setting = true;

        public static IEqualityComparer<SaveableElementStyle> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<SaveableElementStyle>(); }
        }

        IEqualityComparer<SaveableElementStyle> IValueObject<SaveableElementStyle>.ValueWiseComparer
        {
            get { return SaveableElementStyle.ValueWiseComparer; }
        }

        public virtual SaveableElementStyle Copy()
        {
            SaveableElementStyle otherObject = new SaveableElementStyle();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual SaveableElementStyle CopyNew()
        {
            SaveableElementStyle otherObject = new SaveableElementStyle();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public virtual void CopyTo(SaveableElementStyle otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultStyle_Setting);
        }

        public virtual void CopyTo(SaveableElementStyle otherObject, bool checkDefaultStyle)
        {
            if (otherObject.m_ParentReportElementId != m_ParentReportElementId)
            { otherObject.m_ParentReportElementId = m_ParentReportElementId; }

            this.CopyValuesTo(otherObject, checkDefaultStyle);
        }

        public virtual void CopyValuesTo(SaveableElementStyle otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultStyle_Setting);
        }

        public virtual void CopyValuesTo(SaveableElementStyle otherObject, bool checkDefaultStyle)
        {
            if (otherObject.m_BackColor != m_BackColor)
            { otherObject.m_BackColor = m_BackColor; }
            if (otherObject.m_ForeColor != m_ForeColor)
            { otherObject.m_ForeColor = m_ForeColor; }
            if (otherObject.m_FontName != m_FontName)
            { otherObject.m_FontName = m_FontName; }
            if (otherObject.m_FontSize != m_FontSize)
            { otherObject.m_FontSize = m_FontSize; }
            if (otherObject.m_FontStyle != m_FontStyle)
            { otherObject.m_FontStyle = m_FontStyle; }
            if (otherObject.m_FontHAlign != m_FontHAlign)
            { otherObject.m_FontHAlign = m_FontHAlign; }
            if (otherObject.m_FontVAlign != m_FontVAlign)
            { otherObject.m_FontVAlign = m_FontVAlign; }
            if (otherObject.m_Indent != m_Indent)
            { otherObject.m_Indent = m_Indent; }
            if (otherObject.m_BorderColor != m_BorderColor)
            { otherObject.m_BorderColor = m_BorderColor; }
            if (otherObject.m_BorderStyle != m_BorderStyle)
            { otherObject.m_BorderStyle = m_BorderStyle; }

            if (!checkDefaultStyle)
            { return; }

            if (otherObject.BackColor_Value != BackColor_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.ForeColor_Value != ForeColor_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.FontName_Value != FontName_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.FontSize_Value != FontSize_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.FontStyle_Value != FontStyle_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.FontHAlign_Value != FontHAlign_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.FontVAlign_Value != FontVAlign_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.Indent_Value != Indent_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.BorderColor_Value != BorderColor_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
            if (otherObject.BorderStyle_Value != BorderStyle_Value)
            { otherObject.DefaultStyle_Value = DefaultStyle_Value; }
        }

        public virtual bool Equals(SaveableElementStyle otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultStyle_Setting);
        }

        public virtual bool Equals(SaveableElementStyle otherObject, bool checkDefaultStyle)
        {
            if (otherObject.m_ParentReportElementId != m_ParentReportElementId)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultStyle);
        }

        public virtual bool EqualsValues(SaveableElementStyle otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultStyle_Setting);
        }

        public virtual bool EqualsValues(SaveableElementStyle otherObject, bool checkDefaultStyle)
        {
            if (otherObject.m_BackColor != m_BackColor)
            { return false; }
            if (otherObject.m_ForeColor != m_ForeColor)
            { return false; }
            if (otherObject.m_FontName != m_FontName)
            { return false; }
            if (otherObject.m_FontSize != m_FontSize)
            { return false; }
            if (otherObject.m_FontStyle != m_FontStyle)
            { return false; }
            if (otherObject.m_FontHAlign != m_FontHAlign)
            { return false; }
            if (otherObject.m_FontVAlign != m_FontVAlign)
            { return false; }
            if (otherObject.m_Indent != m_Indent)
            { return false; }
            if (otherObject.m_BorderColor != m_BorderColor)
            { return false; }
            if (otherObject.m_BorderStyle != m_BorderStyle)
            { return false; }

            if (!checkDefaultStyle)
            { return true; }

            if (otherObject.BackColor_Value != BackColor_Value)
            { return false; }
            if (otherObject.ForeColor_Value != ForeColor_Value)
            { return false; }
            if (otherObject.FontName_Value != FontName_Value)
            { return false; }
            if (otherObject.FontSize_Value != FontSize_Value)
            { return false; }
            if (otherObject.FontStyle_Value != FontStyle_Value)
            { return false; }
            if (otherObject.FontHAlign_Value != FontHAlign_Value)
            { return false; }
            if (otherObject.FontVAlign_Value != FontVAlign_Value)
            { return false; }
            if (otherObject.Indent_Value != Indent_Value)
            { return false; }
            if (otherObject.BorderColor_Value != BorderColor_Value)
            { return false; }
            if (otherObject.BorderStyle_Value != BorderStyle_Value)
            { return false; }
            return true;
        }

        public string GetPropertyName(Expression<Func<SaveableElementStyle, object>> propertyGetter)
        {
            return ClassReflector.GetPropertyName<SaveableElementStyle, object>(propertyGetter);
        }

        #region Object Overrides

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type objType = obj.GetType();
            Type thisType = typeof(SaveableElementStyle);

            if (!thisType.Equals(objType))
            { return false; }

            SaveableElementStyle typedObject = (SaveableElementStyle)obj;
            return Equals(typedObject);
        }

        public override string ToString()
        {
            return Key.ToString();
        }

        #endregion
    }
}