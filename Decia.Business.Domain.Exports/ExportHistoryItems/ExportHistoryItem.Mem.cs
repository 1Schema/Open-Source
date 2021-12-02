using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Exports
{
    public partial class ExportHistoryItem
    {
        public override ExportHistoryItem Copy()
        {
            ExportHistoryItem otherObject = new ExportHistoryItem();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(ExportHistoryItem otherObject)
        {
            this.CopyBaseValuesTo(otherObject);

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }
            if (otherObject.m_TargetTypeCategory != m_TargetTypeCategory)
            { otherObject.m_TargetTypeCategory = m_TargetTypeCategory; }
            if (otherObject.m_TargetTypeValue != m_TargetTypeValue)
            { otherObject.m_TargetTypeValue = m_TargetTypeValue; }
            if (otherObject.m_WasDataIncluded != m_WasDataIncluded)
            { otherObject.m_WasDataIncluded = m_WasDataIncluded; }
            if (otherObject.m_ExtendedData != m_ExtendedData)
            { otherObject.m_ExtendedData = m_ExtendedData; }
        }

        public override bool Equals(ExportHistoryItem otherObject)
        {
            if (!this.EqualsBaseValues(otherObject))
            { return false; }

            if (otherObject.m_Key != m_Key)
            { return false; }
            if (otherObject.m_TargetTypeCategory != m_TargetTypeCategory)
            { return false; }
            if (otherObject.m_TargetTypeValue != m_TargetTypeValue)
            { return false; }
            if (otherObject.m_WasDataIncluded != m_WasDataIncluded)
            { return false; }
            if (otherObject.m_ExtendedData != m_ExtendedData)
            { return false; }

            return true;
        }
    }
}