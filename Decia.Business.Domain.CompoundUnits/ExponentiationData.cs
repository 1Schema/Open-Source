using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.CompoundUnits
{
    public struct ExponentiationData
    {
        private int m_BaseUnitTypeNumber;
        private int m_NumeratorExponentiation;
        private int m_DenominatorExponentiation;

        public ExponentiationData(int baseUnitTypeNumber, int numeratorExponentiation, int denominatorExponentiation)
        {
            m_BaseUnitTypeNumber = baseUnitTypeNumber;
            m_NumeratorExponentiation = numeratorExponentiation;
            m_DenominatorExponentiation = denominatorExponentiation;
        }

        public int BaseUnitTypeNumber
        { get { return m_BaseUnitTypeNumber; } }

        public int NumeratorExponentiation
        { get { return m_NumeratorExponentiation; } }

        public int DenominatorExponentiation
        { get { return m_DenominatorExponentiation; } }

        public bool IsEmpty
        {
            get { return (NumeratorExponentiation == DenominatorExponentiation); }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            ExponentiationData otherData = (ExponentiationData)obj;
            bool areEqual = (BaseUnitTypeNumber.Equals(otherData.BaseUnitTypeNumber) && NumeratorExponentiation.Equals(otherData.NumeratorExponentiation) && DenominatorExponentiation.Equals(otherData.DenominatorExponentiation));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = TypedIdUtils.ConvertToString("BaseUnitTypeNumber", BaseUnitTypeNumber);
            string item2 = TypedIdUtils.ConvertToString("NumeratorExponentiation", NumeratorExponentiation);
            string item3 = TypedIdUtils.ConvertToString("DenominatorExponentiation", DenominatorExponentiation);
            string value = string.Format(ConversionUtils.ThreeItemListFormat, item1, item2, item3);
            return value;
        }
    }
}