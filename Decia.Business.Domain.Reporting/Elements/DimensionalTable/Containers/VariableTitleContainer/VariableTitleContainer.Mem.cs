using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class VariableTitleContainer : IValueObject<VariableTitleContainer>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<VariableTitleContainer> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<VariableTitleContainer>(); }
        }

        IEqualityComparer<VariableTitleContainer> IValueObject<VariableTitleContainer>.ValueWiseComparer
        {
            get { return VariableTitleContainer.ValueWiseComparer; }
        }

        public override VariableTitleContainer Copy()
        {
            VariableTitleContainer otherObject = new VariableTitleContainer();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual VariableTitleContainer CopyNew()
        {
            VariableTitleContainer otherObject = new VariableTitleContainer();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(VariableTitleContainer otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(VariableTitleContainer otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(VariableTitleContainer otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(VariableTitleContainer otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(VariableTitleContainer otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_VariableDataContainerNumber != m_VariableDataContainerNumber)
            { otherObject.m_VariableDataContainerNumber = m_VariableDataContainerNumber; }
            if (otherObject.m_StackingDimension != m_StackingDimension)
            { otherObject.m_StackingDimension = m_StackingDimension; }

            if (!otherObject.m_VariableTitleBoxNumbers.AreUnorderedCollectionsEqual(m_VariableTitleBoxNumbers))
            { otherObject.m_VariableTitleBoxNumbers = new List<int>(m_VariableTitleBoxNumbers); }

            if (!otherObject.m_DimensionsBySortIndex.AreUnorderedCollectionsEqual(m_DimensionsBySortIndex))
            { otherObject.m_DimensionsBySortIndex = new SortedDictionary<int, ModelObjectReference>(m_DimensionsBySortIndex); }
        }

        public override bool Equals(VariableTitleContainer otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(VariableTitleContainer otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(VariableTitleContainer otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(VariableTitleContainer otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(VariableTitleContainer otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_VariableDataContainerNumber != m_VariableDataContainerNumber)
            { return false; }
            if (otherObject.m_StackingDimension != m_StackingDimension)
            { return false; }

            if (!otherObject.m_VariableTitleBoxNumbers.AreUnorderedCollectionsEqual(m_VariableTitleBoxNumbers))
            { return false; }

            if (!otherObject.m_DimensionsBySortIndex.AreUnorderedCollectionsEqual(m_DimensionsBySortIndex))
            { return false; }

            return true;
        }
    }
}