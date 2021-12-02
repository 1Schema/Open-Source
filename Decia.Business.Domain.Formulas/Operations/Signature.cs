using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Formulas.Operations
{
    public class Signature
    {
        #region Static Members

        public static readonly string TypesIn_PropName = ClassReflector.GetPropertyName((Signature x) => x.TypesIn);
        public static readonly string AllowsDynamicIn_PropName = ClassReflector.GetPropertyName((Signature x) => x.AllowsDynamicIn);
        public static readonly string TypeOut_PropName = ClassReflector.GetPropertyName((Signature x) => x.TypeOut);

        public static readonly string TypesIn_Prefix = KeyProcessingModeUtils.GetModalDebugText(TypesIn_PropName, "TI");
        public static readonly string AllowsDynamicIn_Prefix = KeyProcessingModeUtils.GetModalDebugText(AllowsDynamicIn_PropName, "DI");
        public static readonly string TypeOut_Prefix = KeyProcessingModeUtils.GetModalDebugText(TypeOut_PropName, "TO");

        public const bool DefaultAllowsDynamic = true;
        public static readonly DeciaDataType InvalidDataType = DeciaDataTypeUtils.InvalidDataType;

        #endregion

        #region Members

        protected SortedDictionary<int, SignatureMember> m_MembersIn;
        protected DeciaDataType m_TypeOut;

        #endregion

        #region Constructors

        public Signature(IEnumerable<Parameter> parameters, IEnumerable<DeciaDataType> typesIn, DeciaDataType typeOut)
            : this(typesIn, parameters.Select(p => p.AllowDynamicValues), typeOut)
        { }

        public Signature(IEnumerable<DeciaDataType> typesIn, IEnumerable<bool> allowsDynamicIn, DeciaDataType typeOut)
        {
            if (typesIn.Count() > allowsDynamicIn.Count())
            { throw new InvalidOperationException("Each Type In must a corresponding value for AllowsDynamic."); }

            m_MembersIn = new SortedDictionary<int, SignatureMember>();
            for (int index = 0; index < typesIn.Count(); index++)
            {
                SignatureMember member = new SignatureMember(typesIn.ElementAt(index), allowsDynamicIn.ElementAt(index));
                m_MembersIn.Add(index, member);
            }
            m_TypeOut = typeOut;
        }

        #endregion

        #region Properties

        public int Key
        {
            get { return this.GetHashCode(); }
        }

        public IList<DeciaDataType> TypesIn
        {
            get { return m_MembersIn.Select(ti => ti.Value.DataType).ToList(); }
        }

        public SortedDictionary<int, DeciaDataType> TypesInByIndex
        {
            get { return new SortedDictionary<int, DeciaDataType>(m_MembersIn.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.DataType)); }
        }

        public IList<bool> AllowsDynamicIn
        {
            get { return m_MembersIn.Select(ti => ti.Value.AllowsDynamic).ToList(); }
        }

        public SortedDictionary<int, bool> AllowsDynamicInByIndex
        {
            get { return new SortedDictionary<int, bool>(m_MembersIn.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AllowsDynamic)); }
        }

        public DeciaDataType TypeOut
        {
            get { return m_TypeOut; }
        }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            Signature otherKey = (Signature)obj;
            bool areEqual = (TypesInByIndex.AreUnorderedDictionariesEqual(otherKey.TypesInByIndex)
                && AllowsDynamicInByIndex.AreUnorderedDictionariesEqual(otherKey.AllowsDynamicInByIndex)
                && (m_TypeOut == otherKey.m_TypeOut));
            return areEqual;
        }

        public override int GetHashCode()
        {
            return GetKeyForInputTypes(TypesIn);
        }

        public override string ToString()
        {
            string item1 = TypedIdUtils.ConvertToString(TypesIn_Prefix, TypesIn);
            string item2 = TypedIdUtils.ConvertToString(AllowsDynamicIn_Prefix, AllowsDynamicIn);
            string item3 = TypedIdUtils.ConvertToString(TypeOut_Prefix, TypeOut);
            string value = string.Format(ConversionUtils.ThreeItemListFormat, item1, item2, item3);
            return value;
        }

        #endregion

        #region Static Methods

        public static int GetKeyForInputTypes(IEnumerable<DeciaDataType> inputTypes)
        {
            return GetKeyForInputTypes(inputTypes, inputTypes.Count());
        }

        public static int GetKeyForInputTypes(IEnumerable<DeciaDataType> inputTypes, int numberOfRelevantIndices)
        {
            List<DeciaDataType> relevantInputTypes = new List<DeciaDataType>();
            int stoppingPoint = System.Math.Min(numberOfRelevantIndices, inputTypes.Count());

            for (int i = 0; i < stoppingPoint; i++)
            {
                relevantInputTypes.Add(inputTypes.ElementAt(i));
            }

            string typesIn = TypedIdUtils.ConvertToString("Types In", relevantInputTypes);
            return typesIn.GetHashCode();
        }

        public static int GetKeyForInputTypes(IDictionary<int, DeciaDataType> inputTypes)
        {
            return GetKeyForInputTypes(inputTypes.Values.ToList());
        }

        public static int GetKeyForInputTypes(IDictionary<int, DeciaDataType> inputTypes, int numberOfRelevantIndices)
        {
            return GetKeyForInputTypes(inputTypes.Values.ToList(), numberOfRelevantIndices);
        }

        #endregion
    }
}