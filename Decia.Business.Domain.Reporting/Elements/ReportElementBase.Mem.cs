using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public abstract partial class ReportElementBase<KDO>
    {
        protected virtual void CopyValuesToBase(ReportElementBase<KDO> otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            m_DimensionLayout_X.Default_CheckDefaultLayout_Setting = checkDefaultValues;
            m_DimensionLayout_Y.Default_CheckDefaultLayout_Setting = checkDefaultValues;
            m_ElementStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;

            this.CopyBaseValuesTo(otherObject as KDO);

            if (otherObject.m_Name != m_Name)
            { otherObject.m_Name = m_Name; }

            if (otherObject.m_ParentElementNumber != m_ParentElementNumber)
            { otherObject.m_ParentElementNumber = m_ParentElementNumber; }
            if (otherObject.m_IsLocked != m_IsLocked)
            { otherObject.m_IsLocked = m_IsLocked; }
            if (otherObject.m_ZOrder != m_ZOrder)
            { otherObject.m_ZOrder = m_ZOrder; }

            if (otherObject.m_IsParentEditable != m_IsParentEditable)
            { otherObject.m_IsParentEditable = m_IsParentEditable; }
            if (otherObject.m_IsDirectlyDeletable != m_IsDirectlyDeletable)
            { otherObject.m_IsDirectlyDeletable = m_IsDirectlyDeletable; }

            if (!dimensionLayoutComparer.Equals(otherObject.m_DimensionLayout_X, m_DimensionLayout_X))
            {
                otherObject.m_DimensionLayout_X = m_DimensionLayout_X.Copy();
                otherObject.m_DimensionLayout_X.ParentReportElementId = otherObject.Key;
            }
            if (!dimensionLayoutComparer.Equals(otherObject.m_DimensionLayout_Y, m_DimensionLayout_Y))
            {
                otherObject.m_DimensionLayout_Y = m_DimensionLayout_Y.Copy();
                otherObject.m_DimensionLayout_Y.ParentReportElementId = otherObject.Key;
            }

            if (otherObject.m_DefaultStyleType != m_DefaultStyleType)
            { otherObject.m_DefaultStyleType = m_DefaultStyleType; }
            if (otherObject.m_StyleInheritanceElementNumber != m_StyleInheritanceElementNumber)
            { otherObject.m_StyleInheritanceElementNumber = m_StyleInheritanceElementNumber; }
            if (!elementStyleComparer.Equals(otherObject.m_ElementStyle, m_ElementStyle))
            {
                otherObject.m_ElementStyle = m_ElementStyle.Copy();
                otherObject.m_ElementStyle.ParentReportElementId = otherObject.Key;
            }
        }

        protected virtual bool EqualsValuesBase(ReportElementBase<KDO> otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            m_DimensionLayout_X.Default_CheckDefaultLayout_Setting = checkDefaultValues;
            m_DimensionLayout_Y.Default_CheckDefaultLayout_Setting = checkDefaultValues;
            m_ElementStyle.Default_CheckDefaultStyle_Setting = checkDefaultValues;

            if (!this.EqualsBaseValues(otherObject as KDO))
            { return false; }

            if (otherObject.m_Name != m_Name)
            { return false; }

            if (otherObject.m_ParentElementNumber != m_ParentElementNumber)
            { return false; }
            if (otherObject.m_IsLocked != m_IsLocked)
            { return false; }
            if (otherObject.m_ZOrder != m_ZOrder)
            { return false; }

            if (otherObject.m_IsParentEditable != m_IsParentEditable)
            { return false; }
            if (otherObject.m_IsDirectlyDeletable != m_IsDirectlyDeletable)
            { return false; }

            if (!dimensionLayoutComparer.Equals(otherObject.m_DimensionLayout_X, m_DimensionLayout_X))
            { return false; }
            if (!dimensionLayoutComparer.Equals(otherObject.m_DimensionLayout_Y, m_DimensionLayout_Y))
            { return false; }

            if (otherObject.m_DefaultStyleType != m_DefaultStyleType)
            { return false; }
            if (otherObject.m_StyleInheritanceElementNumber != m_StyleInheritanceElementNumber)
            { return false; }
            if (!elementStyleComparer.Equals(otherObject.m_ElementStyle, m_ElementStyle))
            { return false; }

            return true;
        }
    }
}