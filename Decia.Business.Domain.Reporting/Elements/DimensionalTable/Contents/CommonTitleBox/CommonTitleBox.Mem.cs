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
    public partial class CommonTitleBox : IValueObject<CommonTitleBox>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<CommonTitleBox> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<CommonTitleBox>(); }
        }

        IEqualityComparer<CommonTitleBox> IValueObject<CommonTitleBox>.ValueWiseComparer
        {
            get { return CommonTitleBox.ValueWiseComparer; }
        }

        public override CommonTitleBox Copy()
        {
            CommonTitleBox otherObject = new CommonTitleBox();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual CommonTitleBox CopyNew()
        {
            CommonTitleBox otherObject = new CommonTitleBox();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(CommonTitleBox otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(CommonTitleBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(CommonTitleBox otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(CommonTitleBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(CommonTitleBox otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_StackingDimension != m_StackingDimension)
            { otherObject.m_StackingDimension = m_StackingDimension; }

            if (!otherObject.m_ContainedStructuralTitleRangeNumbers.AreUnorderedCollectionsEqual(m_ContainedStructuralTitleRangeNumbers))
            { otherObject.m_ContainedStructuralTitleRangeNumbers = new List<int>(m_ContainedStructuralTitleRangeNumbers); }
            if (!otherObject.m_ContainedTimeTitleRangeNumbers.AreUnorderedCollectionsEqual(m_ContainedTimeTitleRangeNumbers))
            { otherObject.m_ContainedTimeTitleRangeNumbers = new List<int>(m_ContainedTimeTitleRangeNumbers); }

            if (otherObject.m_StyleGroup != m_StyleGroup)
            { otherObject.m_StyleGroup = m_StyleGroup; }
        }

        public override bool Equals(CommonTitleBox otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(CommonTitleBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(CommonTitleBox otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(CommonTitleBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(CommonTitleBox otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_StackingDimension != m_StackingDimension)
            { return false; }

            if (!otherObject.m_ContainedStructuralTitleRangeNumbers.AreUnorderedCollectionsEqual(m_ContainedStructuralTitleRangeNumbers))
            { return false; }
            if (!otherObject.m_ContainedTimeTitleRangeNumbers.AreUnorderedCollectionsEqual(m_ContainedTimeTitleRangeNumbers))
            { return false; }

            if (otherObject.m_StyleGroup != m_StyleGroup)
            { return false; }

            return true;
        }
    }
}