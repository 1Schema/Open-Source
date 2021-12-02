using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.DynamicValues;

namespace Decia.Business.Domain.Reporting
{
    public abstract partial class ValueRangeBase<KDO>
    {
        [ForceMapped]
        public int EF_OutputValueType
        {
            get { return (int)m_OutputValueType; }
            internal set { m_OutputValueType = (OutputValueType)value; }
        }

        [ForceMapped]
        public Nullable<int> EF_DirectValue_DataType
        {
            get
            {
                if (m_DirectValue == DynamicValue.NullInstanceAsObject)
                { return null; }
                return (int)m_DirectValue.DataType;
            }
            internal set
            {
                if (value == null)
                { new DynamicValue(Default_DataType); }
                else
                { m_DirectValue = new DynamicValue((DeciaDataType)value.Value); }
            }
        }

        private Nullable<double> m_EF_DirectValue_NumericalValue = null;
        [ForceMapped]
        public Nullable<double> EF_DirectValue_NumericalValue
        {
            get
            {
                if (m_DirectValue == DynamicValue.NullInstanceAsObject)
                { return null; }
                return m_DirectValue.ValueAsNumber;
            }
            internal set
            {
                m_EF_DirectValue_NumericalValue = value;
                Set_DynamicValue();
            }
        }

        private string m_EF_DirectValue_TextValue = null;
        [ForceMapped]
        public string EF_DirectValue_TextValue
        {
            get
            {
                if (m_DirectValue == DynamicValue.NullInstanceAsObject)
                { return null; }
                return m_DirectValue.ValueAsString;
            }
            internal set
            {
                m_EF_DirectValue_TextValue = value;
                Set_DynamicValue();
            }
        }

        private void Set_DynamicValue()
        {
            if (m_DirectValue == DynamicValue.NullInstanceAsObject)
            { m_DirectValue = new DynamicValue(Default_DataType); }
            else
            { m_DirectValue.LoadFromStorage((DeciaDataType)EF_DirectValue_DataType, m_EF_DirectValue_TextValue, m_EF_DirectValue_NumericalValue); }
        }

        private Nullable<int> m_EF_RefValue_ObjectType = null;
        [ForceMapped]
        public Nullable<int> EF_RefValue_ObjectType
        {
            get { return HasRefValue ? (Nullable<int>)RefValue.ModelObjectType : (Nullable<int>)null; }
            internal set
            {
                m_EF_RefValue_ObjectType = value;
            }
        }

        private Nullable<int> m_EF_RefValue_ObjectId = null;
        [ForceMapped]
        public Nullable<int> EF_RefValue_ObjectId
        {
            get { return HasRefValue ? RefValue.ModelObjectIdAsInt : (Nullable<int>)null; }
            internal set
            {
                m_EF_RefValue_ObjectId = value;
                Set_RefValue();
            }
        }

        private Nullable<int> m_EF_RefValue_AltDimensionNumber = null;
        [ForceMapped]
        public Nullable<int> EF_RefValue_AltDimensionNumber
        {
            get { return HasRefValue ? RefValue.AlternateDimensionNumber : (Nullable<int>)null; }
            internal set
            {
                m_EF_RefValue_AltDimensionNumber = value;
                Set_RefValue();
            }
        }

        private void Set_RefValue()
        {
            if (!m_EF_RefValue_ObjectType.HasValue || !m_EF_RefValue_ObjectId.HasValue)
            { m_RefValue = null; }
            else
            { m_RefValue = new ModelObjectReference((ModelObjectType)m_EF_RefValue_ObjectType.Value, m_EF_RefValue_ObjectId.Value, m_EF_RefValue_AltDimensionNumber); }
        }

        [ForceMapped]
        public Nullable<Guid> EF_ValueFormulaGuid
        {
            get { return m_ValueFormulaGuid; }
            internal set { m_ValueFormulaGuid = value; }
        }
    }
}