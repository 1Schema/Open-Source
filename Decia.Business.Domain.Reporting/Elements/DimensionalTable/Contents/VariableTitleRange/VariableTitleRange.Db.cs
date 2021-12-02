using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting
{
    public partial class VariableTitleRange : IEfAggregate_Detachable<VariableTitleRange>
    {
        #region Entity Framework Mapper

        public class VariableTitleRange_Mapper : EntityTypeConfiguration<VariableTitleRange>
        {
            public VariableTitleRange_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_StackingDimension);
                Property(p => p.EF_IsHidden);
                Property(p => p.EF_StyleGroup);
                Property(p => p.EF_NameVariableTemplateRefType);
                Property(p => p.EF_NameVariableTemplateRefId);
                Property(p => p.EF_NameVariableTemplateRefAltDimensionNumber);
                Property(p => p.EF_OutputValueType);
                Property(p => p.EF_DirectValue_DataType);
                Property(p => p.EF_DirectValue_NumericalValue);
                Property(p => p.EF_DirectValue_TextValue);
                Property(p => p.EF_RefValue_ObjectType);
                Property(p => p.EF_RefValue_ObjectId);
                Property(p => p.EF_RefValue_AltDimensionNumber);
                Property(p => p.EF_ValueFormulaGuid);
                Property(p => p.EF_Name);
                Property(p => p.EF_ParentElementNumber);
                Property(p => p.EF_IsLocked);
                Property(p => p.EF_ZOrder);
                Property(p => p.EF_IsParentEditable);
                Property(p => p.EF_IsDirectlyDeletable);
                Property(p => p.EF_DefaultStyleType);
                Property(p => p.EF_StyleInheritanceElementNumber);
                Property(p => p.EF_CreatorGuid);
                Property(p => p.EF_CreationDate);
                Property(p => p.EF_OwnerType);
                Property(p => p.EF_OwnerGuid);
                Property(p => p.EF_IsDeleted);
                Property(p => p.EF_DeleterGuid);
                Property(p => p.EF_DeletionDate);
                Property(p => p.EF_OrderNumber);
            }
        }

        #endregion

        [ForceMapped]
        internal int EF_StackingDimension
        {
            get { return (int)m_StackingDimension; }
            set { StackingDimension = (Dimension)value; }
        }

        [ForceMapped]
        internal bool EF_IsHidden
        {
            get { return m_IsHidden; }
            set { m_IsHidden = value; }
        }

        [ForceMapped]
        internal string EF_StyleGroup
        {
            get { return m_StyleGroup; }
            set { m_StyleGroup = value; }
        }

        private Nullable<int> m_EF_NameVariableTemplateRefType = null;
        [ForceMapped]
        internal int EF_NameVariableTemplateRefType
        {
            get { return (int)m_NameVariableTemplateRef.ModelObjectType; }
            set
            {
                m_EF_NameVariableTemplateRefType = value;
            }
        }

        private Nullable<int> m_EF_NameVariableTemplateRefId = null;
        [ForceMapped]
        internal int EF_NameVariableTemplateRefId
        {
            get { return m_NameVariableTemplateRef.ModelObjectIdAsInt; }
            set
            {
                m_EF_NameVariableTemplateRefId = value;
                SetNameVariableTemplateRef();
            }
        }

        private Nullable<int> m_EF_NameVariableTemplateRefAltDimensionNumber = null;
        [ForceMapped]
        internal Nullable<int> EF_NameVariableTemplateRefAltDimensionNumber
        {
            get { return m_NameVariableTemplateRef.AlternateDimensionNumber; }
            set
            {
                m_EF_NameVariableTemplateRefAltDimensionNumber = value;
                SetNameVariableTemplateRef();
            }
        }

        private void SetNameVariableTemplateRef()
        {
            if (!m_EF_NameVariableTemplateRefType.HasValue || !m_EF_NameVariableTemplateRefId.HasValue)
            { throw new InvalidOperationException("Null Variable Template references are not allowed."); }
            else
            { m_NameVariableTemplateRef = new ModelObjectReference((ModelObjectType)m_EF_NameVariableTemplateRefType.Value, m_EF_NameVariableTemplateRefId.Value, m_EF_NameVariableTemplateRefAltDimensionNumber); }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<VariableTitleRange>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<VariableTitleRange>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<VariableTitleRange> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<VariableTitleRange>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<VariableTitleRange>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<VariableTitleRange>.UpdateNestedAggregateValues(DbContext context, VariableTitleRange originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<VariableTitleRange>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<VariableTitleRange>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}