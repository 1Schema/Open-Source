using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public abstract partial class ValueRangeBase<KDO>
    {
        protected virtual void CopyValuesToBase_VRB(ValueRangeBase<KDO> otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_OutputValueType != m_OutputValueType)
            { otherObject.m_OutputValueType = m_OutputValueType; }
            if (otherObject.m_DirectValue != ((object)m_DirectValue))
            { otherObject.m_DirectValue.SetValue(m_DirectValue.DataType, m_DirectValue.GetValue()); }
            if (otherObject.m_RefValue != m_RefValue)
            { otherObject.m_RefValue = m_RefValue; }
            if (otherObject.m_ValueFormulaGuid != m_ValueFormulaGuid)
            { otherObject.m_ValueFormulaGuid = m_ValueFormulaGuid; }
        }

        protected virtual bool EqualsValuesBase_VRB(ValueRangeBase<KDO> otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_OutputValueType != m_OutputValueType)
            { return false; }
            if (otherObject.m_DirectValue != ((object)m_DirectValue))
            { return false; }
            if (otherObject.m_RefValue != m_RefValue)
            { return false; }
            if (otherObject.m_ValueFormulaGuid != m_ValueFormulaGuid)
            { return false; }

            return true;
        }
    }
}