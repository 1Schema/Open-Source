using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Testing;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Argument : IEfStorableObject
    {
        #region Entity Framework Mapper

        public class Argument_Mapper : EntityTypeConfiguration<Argument>
        {
            public Argument_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_FormulaGuid);
                Property(p => p.EF_ExpressionGuid);
                Property(p => p.EF_ArgumentIndex);
                Property(p => p.EF_ArgumentType);
                Property(p => p.EF_ParentOperationGuid);
                Property(p => p.EF_AutoJoinOrder);
                Property(p => p.EF_NestedExpressionGuid);
                Property(p => p.EF_ReferencedType);
                Property(p => p.EF_ReferencedId);
                Property(p => p.EF_ReferencedAlternateDimensionNumber);
                Property(p => p.EF_DirectDataType);
                Property(p => p.EF_DirectNumberValue);
                Property(p => p.EF_DirectTextValue);
                Property(p => p.EF_IsRefDeleted);
                Property(p => p.EF_DeletedRef_StructuralTypeText);
                Property(p => p.EF_DeletedRef_VariableTemplateText);
            }
        }

        #endregion

        public object[] EF_CombinedId
        {
            get
            {
                object[] combined = new object[] { EF_ProjectGuid, EF_RevisionNumber, EF_FormulaGuid, EF_ExpressionGuid, EF_ArgumentIndex };
                return combined;
            }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public Guid EF_ProjectGuid
        {
            get { return m_Key.ProjectGuid; }
            set { m_Key = new ArgumentId(value, m_Key.RevisionNumber_NonNull, m_Key.FormulaGuid, m_Key.ExpressionGuid, m_Key.ArgumentIndex); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_Key.RevisionNumber_NonNull; }
            set { m_Key = new ArgumentId(m_Key.ProjectGuid, value, m_Key.FormulaGuid, m_Key.ExpressionGuid, m_Key.ArgumentIndex); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public Guid EF_FormulaGuid
        {
            get { return m_Key.FormulaId.FormulaGuid; }
            set { m_Key = new ArgumentId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, value, m_Key.ExpressionGuid, m_Key.ArgumentIndex); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3)]
        public Guid EF_ExpressionGuid
        {
            get { return m_Key.ExpressionGuid; }
            set { m_Key = new ArgumentId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, m_Key.FormulaGuid, value, m_Key.ArgumentIndex); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 4)]
        public int EF_ArgumentIndex
        {
            get { return m_Key.ArgumentIndex; }
            set { m_Key = new ArgumentId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, m_Key.FormulaGuid, m_Key.ExpressionGuid, value); }
        }

        [ForceMapped]
        internal int EF_ArgumentType
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_ArgumentType);
                return (int)m_ArgumentType;
            }
            set
            {
                m_ArgumentType = (ArgumentType)value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_ArgumentType);
            }
        }

        [ForceMapped]
        internal Guid EF_ParentOperationGuid
        {
            get { return m_ParentOperationGuid; }
            set { m_ParentOperationGuid = value; }
        }

        [ForceMapped]
        internal int EF_AutoJoinOrder
        {
            get { return m_AutoJoinOrder; }
            set { m_AutoJoinOrder = value; }
        }

        [ForceMapped]
        internal Nullable<Guid> EF_NestedExpressionGuid
        {
            get
            {
                if (!m_NestedExpressionId.HasValue)
                { return null; }
                return m_NestedExpressionId.Value.ExpressionGuid;
            }
            set
            {
                if (!value.HasValue)
                { m_NestedExpressionId = null; }
                else
                { m_NestedExpressionId = new ExpressionId(this.Key.FormulaId, value.Value); }
            }
        }

        private Nullable<int> m_EF_ReferencedType = null;
        [ForceMapped]
        internal Nullable<int> EF_ReferencedType
        {
            get { return m_ReferencedModelObject.HasValue ? (Nullable<int>)m_ReferencedModelObject.Value.ModelObjectType : (Nullable<int>)null; }
            set
            {
                m_EF_ReferencedType = value;
            }
        }

        private Nullable<Guid> m_EF_ReferencedId = null;
        [ForceMapped]
        internal Nullable<Guid> EF_ReferencedId
        {
            get { return m_ReferencedModelObject.HasValue ? m_ReferencedModelObject.Value.ModelObjectId : (Nullable<Guid>)null; }
            set
            {
                m_EF_ReferencedId = value;
                SetReferencedModelObject();
            }
        }

        private Nullable<int> m_EF_ReferencedAlternateDimensionNumber = null;
        [ForceMapped]
        internal Nullable<int> EF_ReferencedAlternateDimensionNumber
        {
            get { return m_ReferencedModelObject.HasValue ? m_ReferencedModelObject.Value.AlternateDimensionNumber : (Nullable<int>)null; }
            set
            {
                m_EF_ReferencedAlternateDimensionNumber = value;
                SetReferencedModelObject();
            }
        }

        private void SetReferencedModelObject()
        {
            if (!m_EF_ReferencedType.HasValue || !m_EF_ReferencedId.HasValue)
            { m_ReferencedModelObject = null; }
            else
            { m_ReferencedModelObject = new ModelObjectReference((ModelObjectType)m_EF_ReferencedType.Value, m_EF_ReferencedId.Value, m_EF_ReferencedAlternateDimensionNumber); }
        }

        [ForceMapped]
        internal Nullable<int> EF_DirectDataType
        {
            get
            {
                if (m_DirectValue == DynamicValue.NullInstanceAsObject)
                { return null; }
                return (int)m_DirectValue.DataType;
            }
            set
            {
                if (value == null)
                { m_DirectValue = null; }
                else
                { m_DirectValue = new DynamicValue((DeciaDataType)value.Value); }
            }
        }

        private Nullable<double> m_EF_DirectNumberValue = null;
        [ForceMapped]
        internal Nullable<double> EF_DirectNumberValue
        {
            get
            {
                if (m_DirectValue == DynamicValue.NullInstanceAsObject)
                { return null; }
                return m_DirectValue.ValueAsNumber;
            }
            set
            {
                m_EF_DirectNumberValue = value;
                InitializeDynamicValue();
            }
        }

        private string m_EF_DirectTextValue = null;
        [ForceMapped]
        internal string EF_DirectTextValue
        {
            get
            {
                if (m_DirectValue == DynamicValue.NullInstanceAsObject)
                { return null; }
                return m_DirectValue.ValueAsString;
            }
            set
            {
                m_EF_DirectTextValue = value;
                InitializeDynamicValue();
            }
        }

        [ForceMapped]
        internal bool? EF_IsRefDeleted
        {
            get { return m_IsRefDeleted; }
            set { m_IsRefDeleted = value; }
        }

        [ForceMapped]
        internal string EF_DeletedRef_StructuralTypeText
        {
            get { return m_DeletedRef_StructuralTypeText; }
            set { m_DeletedRef_StructuralTypeText = value; }
        }

        [ForceMapped]
        internal string EF_DeletedRef_VariableTemplateText
        {
            get { return m_DeletedRef_VariableTemplateText; }
            set { m_DeletedRef_VariableTemplateText = value; }
        }

        private void InitializeDynamicValue()
        {
            if (m_DirectValue == DynamicValue.NullInstanceAsObject)
            { m_DirectValue = null; }
            else
            { m_DirectValue.LoadFromStorage((DeciaDataType)EF_DirectDataType, m_EF_DirectTextValue, m_EF_DirectNumberValue); }
        }
    }
}