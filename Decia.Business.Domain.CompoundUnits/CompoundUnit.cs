using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.CommonUtilities.Enums;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain;
using Decia.Business.Domain.CompoundUnits.BaseUnitValues;

namespace Decia.Business.Domain.CompoundUnits
{
    public partial class CompoundUnit : ProjectDomainObjectBase_Deleteable<CompoundUnitId, CompoundUnit>, ICompoundUnit<CompoundUnit>, IParentIdProvider
    {
        #region Static Members

        public const CompoundUnit NullInstance = null;
        public const object NullInstanceAsObject = null;

        public static CompoundUnit GetGlobalScalarUnit(Guid projectGuid, long revisionNumber)
        { return new CompoundUnit(projectGuid, revisionNumber, Guid.Empty); }

        #endregion

        #region Members

        protected Nullable<ModelObjectType> m_ParentModelObjectType;
        protected SortedDictionary<ModelObjectType, ModelObjectReference> m_ParentModelObjectRefs;

        protected SortedDictionary<int, BaseUnitExponentiationValue> m_BaseUnitExponentiationValues;

        #endregion

        #region Constructors

        public CompoundUnit()
            : this(CompoundUnitId.DefaultId.ProjectGuid, CompoundUnitId.DefaultId.RevisionNumber_NonNull)
        { }

        public CompoundUnit(Guid projectGuid, long revisionNumber)
            : this(projectGuid, revisionNumber, Guid.NewGuid())
        { }

        public CompoundUnit(Guid projectGuid, long revisionNumber, Guid compoundUnitGuid)
            : this(projectGuid, revisionNumber, compoundUnitGuid, null, null)
        { }

        public CompoundUnit(Guid projectGuid, long revisionNumber, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, Guid.NewGuid(), parentModelObjectType, parentModelObjectRefs)
        { }

        public CompoundUnit(Guid projectGuid, long revisionNumber, Guid compoundUnitGuid, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, compoundUnitGuid, parentModelObjectType, parentModelObjectRefs.ToDictionary(x => x.ModelObjectType, x => x))
        { }

        protected CompoundUnit(Guid projectGuid, long revisionNumber, Guid compoundUnitGuid, Nullable<ModelObjectType> parentModelObjectType, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs)
            : base(projectGuid, revisionNumber)
        {
            m_Key = new CompoundUnitId(projectGuid, revisionNumber, compoundUnitGuid);

            if (!parentModelObjectType.HasValue)
            { ClearParent(); }
            else
            { SetParent(parentModelObjectType.Value, parentModelObjectRefs); }

            m_BaseUnitExponentiationValues = new SortedDictionary<int, BaseUnitExponentiationValue>();
        }

        #endregion

        #region Base-Class Method Overrides

        protected override Guid GetProjectGuid()
        {
            return EF_ProjectGuid;
        }

        protected override long? GetRevisionNumber()
        {
            return EF_RevisionNumber;
        }

        protected override void SetProjectGuid(Guid projectGuid)
        {
            EF_ProjectGuid = projectGuid;
        }

        protected override void SetRevisionNumber(long revisionNumber)
        {
            EF_RevisionNumber = revisionNumber;
        }

        #endregion

        public bool IsScalar
        {
            get
            {
                if (BaseUnitScalarities.Values.Contains(false))
                { return false; }
                return true;
            }
        }

        public bool IsGlobalScalar
        {
            get
            {
                if (!IsScalar)
                { return false; }
                return (m_Key.ProjectMemberId.Equals(ProjectMemberId.DefaultId));
            }
        }

        public ICollection<int> BaseUnitTypeNumbers
        {
            get { return m_BaseUnitExponentiationValues.Keys; }
        }

        public ICollection<T> GetBaseUnitTypeIds<T>(Func<int, T> baseUnitTypeIdConverter)
        {
            if (baseUnitTypeIdConverter == null)
            { throw new InvalidOperationException("Cannot convert Ids with null converter delegate."); }

            List<T> baseUnitTypeIds = m_BaseUnitExponentiationValues.Keys.Select(k => baseUnitTypeIdConverter(k)).ToList();
            return baseUnitTypeIds;
        }

        public IDictionary<int, bool> BaseUnitScalarities
        {
            get
            {
                SortedDictionary<int, bool> isScalar = new SortedDictionary<int, bool>();
                foreach (BaseUnitExponentiationValue exponentiationValue in m_BaseUnitExponentiationValues.Values)
                {
                    isScalar.Add(exponentiationValue.BaseUnitTypeNumber, exponentiationValue.IsBaseUnitTypeScalar);
                }
                return isScalar;
            }
        }

        public IDictionary<int, ExponentiationData> BaseUnitActualExponentiations
        {
            get
            {
                SortedDictionary<int, ExponentiationData> actualExponentiations = new SortedDictionary<int, ExponentiationData>();
                foreach (BaseUnitExponentiationValue exponentiationValue in m_BaseUnitExponentiationValues.Values)
                {
                    actualExponentiations.Add(exponentiationValue.BaseUnitTypeNumber, exponentiationValue.ActualExponentiationData);
                }
                return actualExponentiations;
            }
        }

        public IDictionary<int, ExponentiationData> BaseUnitReducedExponentiations
        {
            get
            {
                SortedDictionary<int, ExponentiationData> reducedExponentiations = new SortedDictionary<int, ExponentiationData>();
                foreach (BaseUnitExponentiationValue exponentiationValue in m_BaseUnitExponentiationValues.Values)
                {
                    reducedExponentiations.Add(exponentiationValue.BaseUnitTypeNumber, exponentiationValue.ReducedExponentiationData);
                }
                return reducedExponentiations;
            }
        }

        public void AddBaseUnitType(int baseUnitTypeNumber, bool isScalar)
        {
            AddBaseUnitType(baseUnitTypeNumber, isScalar, 0, 0);
        }

        public void AddBaseUnitType(int baseUnitTypeNumber, bool isScalar, int numeratorExp, int denominatorExp)
        {
            if (m_BaseUnitExponentiationValues.ContainsKey(baseUnitTypeNumber))
            { throw new ApplicationException("The specified BaseUnitType was already added to the CompoundUnit."); }

            BaseUnitExponentiationValue exponentiationValue = new BaseUnitExponentiationValue(this.Key, baseUnitTypeNumber, isScalar, numeratorExp, denominatorExp);
            m_BaseUnitExponentiationValues.Add(exponentiationValue.BaseUnitTypeNumber, exponentiationValue);
        }

        public void UpdateBaseUnitTypeScalarity(int baseUnitTypeNumber, bool scalarity)
        {
            if (!m_BaseUnitExponentiationValues.ContainsKey(baseUnitTypeNumber))
            { throw new InvalidOperationException("The specified BaseUnitType does not exist within the CompoundUnit."); }
            BaseUnitExponentiationValue exponentiationValue = m_BaseUnitExponentiationValues[baseUnitTypeNumber];
            exponentiationValue.IsBaseUnitTypeScalar = scalarity;
        }

        public void UpdateBaseUnitTypeNumeratorExponentiation(int baseUnitTypeNumber, int numeratorExponentiation)
        {
            if (!m_BaseUnitExponentiationValues.ContainsKey(baseUnitTypeNumber))
            { throw new InvalidOperationException("The specified BaseUnitType does not exist within the CompoundUnit."); }
            BaseUnitExponentiationValue exponentiationValue = m_BaseUnitExponentiationValues[baseUnitTypeNumber];
            exponentiationValue.NumeratorExponentiation = numeratorExponentiation;
        }

        public void UpdateBaseUnitTypeDenominatorExponentiation(int baseUnitTypeNumber, int denominatorExponentiation)
        {
            if (!m_BaseUnitExponentiationValues.ContainsKey(baseUnitTypeNumber))
            { throw new InvalidOperationException("The specified BaseUnitType does not exist within the CompoundUnit."); }
            BaseUnitExponentiationValue exponentiationValue = m_BaseUnitExponentiationValues[baseUnitTypeNumber];
            exponentiationValue.DenominatorExponention = denominatorExponentiation;
        }

        public void RemoveBaseUnitType(int baseUnitTypeNumber)
        {
            if (!m_BaseUnitExponentiationValues.ContainsKey(baseUnitTypeNumber))
            { throw new InvalidOperationException("The specified BaseUnitType does not exist within the CompoundUnit."); }
            m_BaseUnitExponentiationValues.Remove(baseUnitTypeNumber);
        }

        public bool IsSameValue(CompoundUnit otherObject)
        {
            CompoundUnit otherObjectReduced = otherObject.Reduce();
            CompoundUnit thisObjectReduced = this.Reduce();

            bool equivalent = otherObjectReduced.EqualsValues(thisObjectReduced);
            return equivalent;
        }

        public bool IsNotSameValue(CompoundUnit otherObject)
        {
            return !IsSameValue(otherObject);
        }

        public void AssignValuesTo(CompoundUnit otherObject)
        {
            this.CopyValuesTo(otherObject);
        }

        public CompoundUnit Reduce()
        {
            CompoundUnit reducedUnit = new CompoundUnit(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull);

            foreach (int baseUnitTypeNumber in m_BaseUnitExponentiationValues.Keys)
            {
                BaseUnitExponentiationValue thisExponentiationValue = m_BaseUnitExponentiationValues[baseUnitTypeNumber];
                if (thisExponentiationValue.IsBaseUnitTypeScalar)
                { continue; }

                ExponentiationData reducedData = thisExponentiationValue.ReducedExponentiationData;
                if (reducedData.IsEmpty)
                { continue; }

                BaseUnitExponentiationValue reducedExponentiationValue = new BaseUnitExponentiationValue(thisExponentiationValue.ParentCompoundUnitId, thisExponentiationValue.BaseUnitTypeNumber, thisExponentiationValue.IsBaseUnitTypeScalar, reducedData.NumeratorExponentiation, reducedData.DenominatorExponentiation);
                reducedUnit.m_BaseUnitExponentiationValues.Add(reducedExponentiationValue.BaseUnitTypeNumber, reducedExponentiationValue);
            }
            return reducedUnit;
        }

        public CompoundUnit Invert()
        {
            CompoundUnit otherObject = new CompoundUnit(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull);

            foreach (int baseUnitTypeNumber in m_BaseUnitExponentiationValues.Keys)
            {
                BaseUnitExponentiationValue thisExponentiationValue = m_BaseUnitExponentiationValues[baseUnitTypeNumber];
                otherObject.AddBaseUnitType(baseUnitTypeNumber, thisExponentiationValue.IsBaseUnitTypeScalar, thisExponentiationValue.DenominatorExponention, thisExponentiationValue.NumeratorExponentiation);
            }
            return otherObject;
        }

        public CompoundUnit Add(CompoundUnit otherObject)
        {
            if (IsNotSameValue(otherObject))
            { throw new ApplicationException("Cannot add values if units don't match."); }

            var projectMemberId = this.GetProjectMemberId_MostSpecific(otherObject);

            CompoundUnit returnObject = new CompoundUnit(projectMemberId.ProjectGuid, projectMemberId.RevisionNumber_NonNull);
            this.AssignValuesTo(returnObject);
            return returnObject;
        }

        public CompoundUnit Subtract(CompoundUnit otherObject)
        {
            if (IsNotSameValue(otherObject))
            { throw new ApplicationException("Cannot add values if units don't match."); }

            var projectMemberId = this.GetProjectMemberId_MostSpecific(otherObject);

            CompoundUnit returnObject = new CompoundUnit(projectMemberId.ProjectGuid, projectMemberId.RevisionNumber_NonNull);
            this.AssignValuesTo(returnObject);
            return returnObject;
        }

        public CompoundUnit Multiply(CompoundUnit otherObject)
        {
            var projectMemberId = this.GetProjectMemberId_MostSpecific(otherObject);

            CompoundUnit returnObject = new CompoundUnit(projectMemberId.ProjectGuid, projectMemberId.RevisionNumber_NonNull);
            IEnumerable<int> keys = BaseUnitTypeNumbers;
            keys.Union<int>(otherObject.BaseUnitTypeNumbers);
            keys = keys.Distinct<int>();

            foreach (int key in keys)
            {
                bool thisIsScalar = true;
                bool otherIsScalar = true;
                int combinedNumerator = 0;
                int combinedDenominator = 0;

                if (BaseUnitTypeNumbers.Contains(key))
                {
                    BaseUnitExponentiationValue exponentiationValue = m_BaseUnitExponentiationValues[key];
                    thisIsScalar = exponentiationValue.IsBaseUnitTypeScalar;
                    combinedNumerator += exponentiationValue.NumeratorExponentiation;
                    combinedDenominator += exponentiationValue.DenominatorExponention;
                }
                if (otherObject.BaseUnitTypeNumbers.Contains(key))
                {
                    BaseUnitExponentiationValue exponentiationValue = otherObject.m_BaseUnitExponentiationValues[key];
                    otherIsScalar = exponentiationValue.IsBaseUnitTypeScalar;
                    combinedNumerator += exponentiationValue.NumeratorExponentiation;
                    combinedDenominator += exponentiationValue.DenominatorExponention;
                }

                if (thisIsScalar != otherIsScalar)
                { throw new ApplicationException("IsScalar values do not match in specified ComputationUnits."); }

                if ((combinedNumerator == 0) && (combinedDenominator == 0))
                { continue; }

                returnObject.AddBaseUnitType(key, thisIsScalar, combinedNumerator, combinedDenominator);
            }

            return returnObject;
        }

        public CompoundUnit Divide(CompoundUnit otherObject)
        {
            CompoundUnit invertedObject = otherObject.Invert();
            return Multiply(invertedObject);
        }

        public CompoundUnit Modulo(CompoundUnit otherObject)
        {
            return this.CopyNew();
        }

        #region Logic Operators

        public static bool operator ==(CompoundUnit firstArg, CompoundUnit secondArg)
        {
            if ((((object)firstArg) == null) && (((object)secondArg) == null))
            { return true; }
            if ((((object)firstArg) == null) ^ (((object)secondArg) == null))
            { return false; }

            return (firstArg.Equals(secondArg));
        }

        public static bool operator !=(CompoundUnit firstArg, CompoundUnit secondArg)
        {
            return (!(firstArg == secondArg));
        }

        #endregion

        #region Mathematical Operators

        public static CompoundUnit operator +(CompoundUnit firstArg, CompoundUnit secondArg)
        {
            return firstArg.Add(secondArg);
        }

        public static CompoundUnit operator -(CompoundUnit firstArg, CompoundUnit secondArg)
        {
            return firstArg.Subtract(secondArg);
        }

        public static CompoundUnit operator *(CompoundUnit firstArg, CompoundUnit secondArg)
        {
            return firstArg.Multiply(secondArg);
        }

        public static CompoundUnit operator /(CompoundUnit firstArg, CompoundUnit secondArg)
        {
            return firstArg.Divide(secondArg);
        }

        public static CompoundUnit operator %(CompoundUnit firstArg, CompoundUnit secondArg)
        {
            return firstArg.Modulo(secondArg);
        }

        #endregion

        #region IProjectMember<CompoundUnit> Implementation

        public CompoundUnit CopyForRevision(long newRevisionNumber)
        {
            if (this.Key.RevisionNumber >= newRevisionNumber)
            { throw new InvalidOperationException("A CompoundUnit created for a new revision must have a greater revision number."); }

            return (this as IProjectMember<CompoundUnit>).CopyForProject(this.Key.ProjectGuid, newRevisionNumber);
        }

        CompoundUnit IProjectMember<CompoundUnit>.CopyForProject(Guid projectGuid, long revisionNumber)
        {
            var newValue = this.Copy();

            newValue.EF_ProjectGuid = projectGuid;
            newValue.EF_RevisionNumber = revisionNumber;

            foreach (var newBaseUnitExponentiationValue in newValue.m_BaseUnitExponentiationValues.Values)
            {
                newBaseUnitExponentiationValue.EF_ProjectGuid = projectGuid;
                newBaseUnitExponentiationValue.EF_RevisionNumber = revisionNumber;
            }
            return newValue;
        }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return Key.ModelObjectType; }
        }

        public Guid ModelObjectId
        {
            get { return Key.ModelObjectId; }
        }

        public bool IdIsInt
        {
            get { return Key.IdIsInt; }
        }

        public int ModelObjectIdAsInt
        {
            get { return Key.ModelObjectIdAsInt; }
        }

        public string ComplexId
        {
            get { return Key.ComplexId; }
        }

        public ModelObjectReference ModelObjectRef
        {
            get { return Key.ModelObjectRef; }
        }

        #endregion

        #region IParentIdProvider Implementation

        [NotMapped]
        public bool HasParent
        {
            get { return m_ParentModelObjectType.HasValue; }
        }

        [NotMapped]
        public ModelObjectType ParentModelObjectType
        {
            get
            {
                this.AssertHasParent();
                return m_ParentModelObjectType.Value;
            }
        }

        [NotMapped]
        public IDictionary<ModelObjectType, ModelObjectReference> ParentModelObjectRefs
        {
            get
            {
                this.AssertHasParent();
                return m_ParentModelObjectRefs;
            }
        }

        public void SetParent(IParentId parentId)
        {
            SetParent(parentId.ModelObjectType, parentId.IdReferences.ToDictionary(x => x.ModelObjectType, x => x));
        }

        public void SetParent(ModelObjectType parentModelObjectType, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs)
        {
            parentModelObjectType.AssertHasParentTypeValue(parentModelObjectRefs);

            m_ParentModelObjectType = parentModelObjectType;
            m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>(parentModelObjectRefs);
        }

        public void ClearParent()
        {
            m_ParentModelObjectType = null;
            m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>();
        }

        public T GetParentId<T>(Func<IParentIdProvider, T> idConversionMethod)
        {
            this.AssertHasParent();
            return idConversionMethod(this);
        }

        #endregion
    }
}