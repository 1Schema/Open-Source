using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;

namespace Decia.Business.Domain.CompoundUnits.BaseUnitValues
{
    public partial class BaseUnitExponentiationValue : IValueObject<BaseUnitExponentiationValue>
    {
        public static IEqualityComparer<BaseUnitExponentiationValue> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<BaseUnitExponentiationValue>(); }
        }

        IEqualityComparer<BaseUnitExponentiationValue> IValueObject<BaseUnitExponentiationValue>.ValueWiseComparer
        {
            get { return BaseUnitExponentiationValue.ValueWiseComparer; }
        }

        public override BaseUnitExponentiationValue Copy()
        {
            BaseUnitExponentiationValue otherObject = new BaseUnitExponentiationValue(CompoundUnitId.DefaultId, BaseUnitTypeNumber, IsBaseUnitTypeScalar, NumeratorExponentiation, DenominatorExponention);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual BaseUnitExponentiationValue CopyNew()
        {
            BaseUnitExponentiationValue otherObject = new BaseUnitExponentiationValue(CompoundUnitId.DefaultId, BaseUnitTypeNumber, IsBaseUnitTypeScalar, NumeratorExponentiation, DenominatorExponention);
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(BaseUnitExponentiationValue otherObject)
        {
            if (otherObject.m_ParentCompoundUnitId != m_ParentCompoundUnitId)
            { otherObject.m_ParentCompoundUnitId = m_ParentCompoundUnitId; }

            this.CopyValuesTo(otherObject);
        }

        public virtual void CopyValuesTo(BaseUnitExponentiationValue otherObject)
        {
            if (!otherObject.m_BaseUnitTypeNumber.Equals(m_BaseUnitTypeNumber))
            { otherObject.m_BaseUnitTypeNumber = m_BaseUnitTypeNumber; }
            if (otherObject.m_IsBaseUnitTypeScalar != m_IsBaseUnitTypeScalar)
            { otherObject.m_IsBaseUnitTypeScalar = m_IsBaseUnitTypeScalar; }
            if (otherObject.m_NumeratorExponentiation != m_NumeratorExponentiation)
            { otherObject.m_NumeratorExponentiation = m_NumeratorExponentiation; }
            if (otherObject.m_DenominatorExponention != m_DenominatorExponention)
            { otherObject.m_DenominatorExponention = m_DenominatorExponention; }
        }

        public override bool Equals(BaseUnitExponentiationValue otherObject)
        {
            if (otherObject.m_ParentCompoundUnitId != m_ParentCompoundUnitId)
            { return false; }

            return this.EqualsValues(otherObject);
        }

        public virtual bool EqualsValues(BaseUnitExponentiationValue otherObject)
        {
            if (!otherObject.m_BaseUnitTypeNumber.Equals(m_BaseUnitTypeNumber))
            { return false; }
            if (otherObject.m_IsBaseUnitTypeScalar != m_IsBaseUnitTypeScalar)
            { return false; }
            if (otherObject.m_NumeratorExponentiation != m_NumeratorExponentiation)
            { return false; }
            if (otherObject.m_DenominatorExponention != m_DenominatorExponention)
            { return false; }
            return true;
        }
    }
}