using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Enums;
using DomainDriver.DomainModeling.DomainObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace Decia.Business.Domain.CompoundUnits.BaseUnitValues
{
    public partial class BaseUnitExponentiationValue : DomainObjectBase<BaseUnitExponentiationValue>
    {
        public static readonly int DefaultBaseUnitTypeId = 0;
        public static readonly int MinimumExponention = 0;

        protected CompoundUnitId m_ParentCompoundUnitId;
        protected int m_BaseUnitTypeNumber;
        protected bool m_IsBaseUnitTypeScalar = true;
        protected int m_NumeratorExponentiation = 0;
        protected int m_DenominatorExponention = 0;

        public BaseUnitExponentiationValue()
            : this(CompoundUnitId.DefaultId, DefaultBaseUnitTypeId, true, 0, 0)
        { }

        public BaseUnitExponentiationValue(CompoundUnitId parentCompoundUnitId, int baseUnitTypeNumber, bool isBaseUnitTypeScalar, int numeratorExponentiation, int denominatorExponention)
        {
            if ((numeratorExponentiation < MinimumExponention) || (denominatorExponention < MinimumExponention))
            { throw new ApplicationException("Exponentiation values must be greater than or equal to zero."); }

            m_ParentCompoundUnitId = parentCompoundUnitId;
            m_BaseUnitTypeNumber = baseUnitTypeNumber;
            m_IsBaseUnitTypeScalar = isBaseUnitTypeScalar;
            m_NumeratorExponentiation = numeratorExponentiation;
            m_DenominatorExponention = denominatorExponention;
        }

        [NotMapped]
        public CompoundUnitId ParentCompoundUnitId
        {
            get { return m_ParentCompoundUnitId; }
            set { m_ParentCompoundUnitId = value; }
        }

        [NotMapped]
        public int BaseUnitTypeNumber
        {
            get { return m_BaseUnitTypeNumber; }
            set { m_BaseUnitTypeNumber = value; }
        }

        [NotMapped]
        public bool IsBaseUnitTypeScalar
        {
            get { return m_IsBaseUnitTypeScalar; }
            set { m_IsBaseUnitTypeScalar = value; }
        }

        [NotMapped]
        public int NumeratorExponentiation
        {
            get { return m_NumeratorExponentiation; }
            set
            {
                if (value < MinimumExponention)
                { throw new ApplicationException("Exponentiation values must be positive integers."); }
                m_NumeratorExponentiation = value;
            }
        }

        [NotMapped]
        public int DenominatorExponention
        {
            get { return m_DenominatorExponention; }
            set
            {
                if (value < MinimumExponention)
                { throw new ApplicationException("Exponentiation values must be positive integers."); }
                m_DenominatorExponention = value;
            }
        }

        [NotMapped]
        public ExponentiationData ActualExponentiationData
        {
            get
            {
                ExponentiationData exponentiationData = new ExponentiationData(BaseUnitTypeNumber, NumeratorExponentiation, DenominatorExponention);
                return exponentiationData;
            }
        }

        [NotMapped]
        public ExponentiationData ReducedExponentiationData
        {
            get
            {
                int reducedNumerator = 0;
                if (NumeratorExponentiation > DenominatorExponention)
                { reducedNumerator = (NumeratorExponentiation - DenominatorExponention); }
                int reducedDenominator = 0;
                if (DenominatorExponention > NumeratorExponentiation)
                { reducedDenominator = (DenominatorExponention - NumeratorExponentiation); }

                ExponentiationData exponentiationData = new ExponentiationData(BaseUnitTypeNumber, reducedNumerator, reducedDenominator);
                return exponentiationData;
            }
        }
    }
}