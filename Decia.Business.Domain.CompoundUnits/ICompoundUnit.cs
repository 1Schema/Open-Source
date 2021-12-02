using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.CompoundUnits
{
    public interface ICompoundUnit<T> : IProjectMember_Deleteable<T>, IModelObjectWithRef
        where T : ICompoundUnit<T>
    {
        CompoundUnitId Key { get; }
        bool IsScalar { get; }
        bool IsGlobalScalar { get; }
        ICollection<int> BaseUnitTypeNumbers { get; }
        IDictionary<int, bool> BaseUnitScalarities { get; }
        IDictionary<int, ExponentiationData> BaseUnitActualExponentiations { get; }
        IDictionary<int, ExponentiationData> BaseUnitReducedExponentiations { get; }

        void AddBaseUnitType(int baseUnitTypeNumber, bool isScalar);
        void AddBaseUnitType(int baseUnitTypeNumber, bool isScalar, int numeratorExp, int denominatorExp);
        void UpdateBaseUnitTypeScalarity(int baseUnitTypeNumber, bool scalarity);
        void UpdateBaseUnitTypeNumeratorExponentiation(int baseUnitTypeNumber, int numeratorExponentiation);
        void UpdateBaseUnitTypeDenominatorExponentiation(int baseUnitTypeNumber, int denominatorExponentiation);
        void RemoveBaseUnitType(int baseUnitTypeNumber);

        bool IsSameValue(T otherObject);
        bool IsNotSameValue(T otherObject);
        void AssignValuesTo(T otherObject);

        T Reduce();
        T Invert();
        T Add(T otherObject);
        T Subtract(T otherObject);
        T Multiply(T otherObject);
        T Divide(T otherObject);
        T Modulo(T otherObject);
    }
}