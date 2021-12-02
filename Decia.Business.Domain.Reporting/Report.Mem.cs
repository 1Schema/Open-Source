using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class Report : IValueObject<Report>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<Report> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<Report>(); }
        }

        IEqualityComparer<Report> IValueObject<Report>.ValueWiseComparer
        {
            get { return Report.ValueWiseComparer; }
        }

        public override Report Copy()
        {
            Report otherObject = new Report();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual Report CopyNew()
        {
            Report otherObject = new Report();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(Report otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(Report otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(Report otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(Report otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(Report otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            m_ReportArea_DimensionLayout_X.Default_CheckDefaultLayout_Setting = checkDefaultValues;
            m_ReportArea_DimensionLayout_Y.Default_CheckDefaultLayout_Setting = checkDefaultValues;
            m_ReportAreaStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;
            m_OutsideAreaStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;
            m_DefaultTitleStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;
            m_DefaultHeaderStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;
            m_DefaultDataStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;

            this.CopyBaseValuesTo(otherObject);

            if (otherObject.m_Name != m_Name)
            { otherObject.m_Name = m_Name; }
            if (otherObject.m_Description != m_Description)
            { otherObject.m_Description = m_Description; }
            if (otherObject.m_IsLocked != m_IsLocked)
            { otherObject.m_IsLocked = m_IsLocked; }

            if (otherObject.m_StructuralTypeRef != m_StructuralTypeRef)
            { otherObject.m_StructuralTypeRef = m_StructuralTypeRef; }
            if (otherObject.m_HasPrimaryTimePeriod != m_HasPrimaryTimePeriod)
            { otherObject.m_HasPrimaryTimePeriod = m_HasPrimaryTimePeriod; }
            if (otherObject.m_HasSecondaryTimePeriod != m_HasSecondaryTimePeriod)
            { otherObject.m_HasSecondaryTimePeriod = m_HasSecondaryTimePeriod; }

            if (otherObject.m_ZoomFactor != m_ZoomFactor)
            { otherObject.m_ZoomFactor = m_ZoomFactor; }
            if (!dimensionLayoutComparer.Equals(otherObject.m_ReportArea_DimensionLayout_X, m_ReportArea_DimensionLayout_X))
            {
                otherObject.m_ReportArea_DimensionLayout_X = m_ReportArea_DimensionLayout_X.Copy();
                otherObject.m_ReportArea_DimensionLayout_X.ParentReportElementId = otherObject.ReportAreaTemplateId;
            }
            if (!dimensionLayoutComparer.Equals(otherObject.m_ReportArea_DimensionLayout_Y, m_ReportArea_DimensionLayout_Y))
            {
                otherObject.m_ReportArea_DimensionLayout_Y = m_ReportArea_DimensionLayout_Y.Copy();
                otherObject.m_ReportArea_DimensionLayout_Y.ParentReportElementId = otherObject.ReportAreaTemplateId;
            }

            if (!elementStyleComparer.Equals(otherObject.m_ReportAreaStyle, m_ReportAreaStyle))
            {
                otherObject.m_ReportAreaStyle = m_ReportAreaStyle.Copy();
                otherObject.m_ReportAreaStyle.ParentReportElementId = otherObject.ReportAreaTemplateId;
            }
            if (!elementStyleComparer.Equals(otherObject.m_OutsideAreaStyle, m_OutsideAreaStyle))
            {
                otherObject.m_OutsideAreaStyle = m_OutsideAreaStyle.Copy();
                otherObject.m_OutsideAreaStyle.ParentReportElementId = otherObject.OutsideAreaTemplateId;
            }
            if (!elementStyleComparer.Equals(otherObject.m_DefaultTitleStyle, m_DefaultTitleStyle))
            {
                otherObject.m_DefaultTitleStyle = m_DefaultTitleStyle.Copy();
                otherObject.m_DefaultTitleStyle.ParentReportElementId = otherObject.TitleTemplateId;
            }
            if (!elementStyleComparer.Equals(otherObject.m_DefaultHeaderStyle, m_DefaultHeaderStyle))
            {
                otherObject.m_DefaultHeaderStyle = m_DefaultHeaderStyle.Copy();
                otherObject.m_DefaultHeaderStyle.ParentReportElementId = otherObject.HeaderTemplateId;
            }
            if (!elementStyleComparer.Equals(otherObject.m_DefaultDataStyle, m_DefaultDataStyle))
            {
                otherObject.m_DefaultDataStyle = m_DefaultDataStyle.Copy();
                otherObject.m_DefaultDataStyle.ParentReportElementId = otherObject.DataTemplateId;
            }
        }

        public override bool Equals(Report otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(Report otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(Report otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(Report otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(Report otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            m_ReportArea_DimensionLayout_X.Default_CheckDefaultLayout_Setting = checkDefaultValues;
            m_ReportArea_DimensionLayout_Y.Default_CheckDefaultLayout_Setting = checkDefaultValues;
            m_ReportAreaStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;
            m_OutsideAreaStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;
            m_DefaultTitleStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;
            m_DefaultHeaderStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;
            m_DefaultDataStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;

            if (!this.EqualsBaseValues(otherObject))
            { return false; }

            if (otherObject.m_Name != m_Name)
            { return false; }
            if (otherObject.m_Description != m_Description)
            { return false; }
            if (otherObject.m_IsLocked != m_IsLocked)
            { return false; }

            if (otherObject.m_StructuralTypeRef != m_StructuralTypeRef)
            { return false; }
            if (otherObject.m_HasPrimaryTimePeriod != m_HasPrimaryTimePeriod)
            { return false; }
            if (otherObject.m_HasSecondaryTimePeriod != m_HasSecondaryTimePeriod)
            { return false; }

            if (otherObject.m_ZoomFactor != m_ZoomFactor)
            { return false; }
            if (!dimensionLayoutComparer.Equals(otherObject.m_ReportArea_DimensionLayout_X, m_ReportArea_DimensionLayout_X))
            { return false; }
            if (!dimensionLayoutComparer.Equals(otherObject.m_ReportArea_DimensionLayout_Y, m_ReportArea_DimensionLayout_Y))
            { return false; }

            if (!elementStyleComparer.Equals(otherObject.m_ReportAreaStyle, m_ReportAreaStyle))
            { return false; }
            if (!elementStyleComparer.Equals(otherObject.m_OutsideAreaStyle, m_OutsideAreaStyle))
            { return false; }
            if (!elementStyleComparer.Equals(otherObject.m_DefaultTitleStyle, m_DefaultTitleStyle))
            { return false; }
            if (!elementStyleComparer.Equals(otherObject.m_DefaultHeaderStyle, m_DefaultHeaderStyle))
            { return false; }
            if (!elementStyleComparer.Equals(otherObject.m_DefaultDataStyle, m_DefaultDataStyle))
            { return false; }

            return true;
        }
    }
}