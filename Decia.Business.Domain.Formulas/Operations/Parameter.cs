using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Formulas.Operations
{
    public struct Parameter : IOrderable
    {
        #region Static Members

        public static readonly string Index_PropName = ClassReflector.GetPropertyName((Parameter x) => x.Index);
        public static readonly string IsRequired_PropName = ClassReflector.GetPropertyName((Parameter x) => x.IsRequired);
        public static readonly string AllowMultiple_PropName = ClassReflector.GetPropertyName((Parameter x) => x.AllowMultiple);
        public static readonly string AllowDynamicValues_PropName = ClassReflector.GetPropertyName((Parameter x) => x.AllowDynamicValues);
        public static readonly string Name_PropName = ClassReflector.GetPropertyName((Parameter x) => x.Name);
        public static readonly string Description_PropName = ClassReflector.GetPropertyName((Parameter x) => x.Description);

        public static readonly string Index_Prefix = KeyProcessingModeUtils.GetModalDebugText(Index_PropName, "I");
        public static readonly string IsRequired_Prefix = KeyProcessingModeUtils.GetModalDebugText(IsRequired_PropName, "IR");
        public static readonly string AllowMultiple_Prefix = KeyProcessingModeUtils.GetModalDebugText(AllowMultiple_PropName, "AM");
        public static readonly string AllowDynamicValues_Prefix = KeyProcessingModeUtils.GetModalDebugText(AllowDynamicValues_PropName, "ADV");
        public static readonly string Name_Prefix = KeyProcessingModeUtils.GetModalDebugText(Name_PropName, "N");
        public static readonly string Description_Prefix = KeyProcessingModeUtils.GetModalDebugText(Description_PropName, "D");

        public const bool DefaultIsRequired = true;
        public const bool DefaultAllowMultiple = false;
        public const bool DefaultAllowDynamicValues = true;

        #endregion

        #region Members

        private int m_Index;
        private bool m_IsRequired;
        private bool m_AllowMultiple;
        private bool m_AllowDynamicValues;
        private string m_Name;
        private string m_Description;

        #endregion

        #region Constructors

        public Parameter(int index, string name, string description)
            : this(index, DefaultIsRequired, DefaultAllowMultiple, DefaultAllowDynamicValues, name, description)
        { }

        public Parameter(int index, bool isRequired, bool allowMultiple)
            : this(index, isRequired, allowMultiple, string.Empty, string.Empty)
        { }

        public Parameter(int index, bool isRequired, bool allowMultiple, string name, string description)
            : this(index, isRequired, allowMultiple, DefaultAllowDynamicValues, name, description)
        { }

        public Parameter(int index, bool isRequired, bool allowMultiple, bool allowDynamicValues)
            : this(index, isRequired, allowMultiple, allowDynamicValues, string.Empty, string.Empty)
        { }

        public Parameter(int index, bool isRequired, bool allowMultiple, bool allowDynamicValues, string name, string description)
        {
            if (isRequired && allowMultiple)
            { throw new InvalidOperationException("The system can only allow multiple instances of optional parameters."); }

            m_Index = index;
            m_IsRequired = isRequired;
            m_AllowMultiple = allowMultiple;
            m_AllowDynamicValues = allowDynamicValues;
            m_Name = name;
            m_Description = description;
        }

        #endregion

        #region Properties

        public int Index
        { get { return m_Index; } }

        public bool IsRequired
        { get { return m_IsRequired; } }

        public bool AllowMultiple
        { get { return m_AllowMultiple; } }

        public bool AllowDynamicValues
        { get { return m_AllowDynamicValues; } }

        public string Name
        { get { return m_Name; } }

        public string Description
        { get { return m_Description; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            Parameter otherKey = (Parameter)obj;
            bool areEqual = ((Index.Equals(otherKey.Index))
                && (IsRequired.Equals(otherKey.IsRequired))
                && (AllowMultiple.Equals(otherKey.AllowMultiple))
                && (AllowDynamicValues.Equals(otherKey.AllowDynamicValues))
                && (Name.Equals(otherKey.Name))
                && (Description.Equals(otherKey.Description)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = TypedIdUtils.ConvertToString(Index_Prefix, Index);
            string item2 = TypedIdUtils.ConvertToString(IsRequired_Prefix, IsRequired);
            string item3 = TypedIdUtils.ConvertToString(AllowMultiple_Prefix, AllowMultiple);
            string item4 = TypedIdUtils.ConvertToString(AllowDynamicValues_Prefix, AllowDynamicValues);
            string item5 = TypedIdUtils.ConvertToString(Name_Prefix, Name);
            string item6 = TypedIdUtils.ConvertToString(Description_Prefix, Description);
            string value = string.Format(ConversionUtils.SixItemListFormat, item1, item2, item3, item4, item5, item6);
            return value;
        }

        public static bool operator ==(Parameter a, Parameter b)
        { return a.Equals(b); }

        public static bool operator !=(Parameter a, Parameter b)
        { return !(a == b); }

        #endregion

        #region IOrderable Implementation

        public long? OrderNumber
        {
            get { return Index; }
        }

        public string OrderValue
        {
            get { return Name; }
        }

        #endregion
    }
}