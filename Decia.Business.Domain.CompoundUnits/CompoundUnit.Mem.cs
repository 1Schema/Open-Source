using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.CompoundUnits.BaseUnitValues;

namespace Decia.Business.Domain.CompoundUnits
{
    public partial class CompoundUnit : IValueObject<CompoundUnit>
    {
        public static IEqualityComparer<CompoundUnit> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<CompoundUnit>(); }
        }

        IEqualityComparer<CompoundUnit> IValueObject<CompoundUnit>.ValueWiseComparer
        {
            get { return CompoundUnit.ValueWiseComparer; }
        }

        public override CompoundUnit Copy()
        {
            CompoundUnit otherObject = new CompoundUnit(Key.ProjectGuid, Key.RevisionNumber_NonNull);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual CompoundUnit CopyNew()
        {
            CompoundUnit otherObject = new CompoundUnit(Key.ProjectGuid, Key.RevisionNumber_NonNull);
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(CompoundUnit otherObject)
        {
            var exponentiationComparer = EqualityComparer<BaseUnitExponentiationValue>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            if (otherObject.m_ParentModelObjectType != m_ParentModelObjectType)
            { otherObject.m_ParentModelObjectType = m_ParentModelObjectType; }
            if (!otherObject.m_ParentModelObjectRefs.AreUnorderedDictionariesEqual(m_ParentModelObjectRefs))
            { otherObject.m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>(m_ParentModelObjectRefs); }

            this.CopyValuesTo(otherObject, exponentiationComparer);
        }

        public virtual void CopyValuesTo(CompoundUnit otherObject)
        {
            var exponentiationComparer = BaseUnitExponentiationValue.ValueWiseComparer;

            this.CopyValuesTo(otherObject, exponentiationComparer);
        }

        protected virtual void CopyValuesTo(CompoundUnit otherObject, IEqualityComparer<BaseUnitExponentiationValue> exponentiationComparer)
        {
            this.CopyBaseValuesTo(otherObject);

            if (!otherObject.m_BaseUnitExponentiationValues.AreUnorderedDictionariesEqual(m_BaseUnitExponentiationValues, exponentiationComparer.GetUntypedEqualityComparer()))
            {
                otherObject.m_BaseUnitExponentiationValues = new SortedDictionary<int, BaseUnitExponentiationValue>();
                foreach (int thisBaseUnitTypeNumber in m_BaseUnitExponentiationValues.Keys)
                {
                    BaseUnitExponentiationValue thisBaseUnitValue = m_BaseUnitExponentiationValues[thisBaseUnitTypeNumber];
                    BaseUnitExponentiationValue otherBaseUnitValue = new BaseUnitExponentiationValue(otherObject.Key, thisBaseUnitValue.BaseUnitTypeNumber, thisBaseUnitValue.IsBaseUnitTypeScalar, thisBaseUnitValue.NumeratorExponentiation, thisBaseUnitValue.DenominatorExponention);
                    otherObject.m_BaseUnitExponentiationValues.Add(thisBaseUnitTypeNumber, otherBaseUnitValue);
                }
            }
        }

        public override bool Equals(CompoundUnit otherObject)
        {
            var exponentiationComparer = EqualityComparer<BaseUnitExponentiationValue>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            if (otherObject.m_ParentModelObjectType != m_ParentModelObjectType)
            { return false; }
            if (!otherObject.m_ParentModelObjectRefs.AreUnorderedDictionariesEqual(m_ParentModelObjectRefs))
            { return false; }

            return this.EqualsValues(otherObject, exponentiationComparer);
        }

        public virtual bool EqualsValues(CompoundUnit otherObject)
        {
            var exponentiationComparer = BaseUnitExponentiationValue.ValueWiseComparer;

            return this.EqualsValues(otherObject, exponentiationComparer);
        }

        protected virtual bool EqualsValues(CompoundUnit otherObject, IEqualityComparer<BaseUnitExponentiationValue> exponentiationComparer)
        {
            if (otherObject == null)
            { return false; }

            if (!this.EqualsBaseValues(otherObject))
            { return false; }

            if (!otherObject.m_BaseUnitExponentiationValues.AreUnorderedDictionariesEqual(m_BaseUnitExponentiationValues, exponentiationComparer.GetUntypedEqualityComparer()))
            { return false; }
            return true;
        }
    }
}