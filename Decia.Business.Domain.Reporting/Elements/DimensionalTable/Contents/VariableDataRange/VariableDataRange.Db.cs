using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting
{
    public partial class VariableDataRange : IEfAggregate_Detachable<VariableDataRange>
    {
        #region Entity Framework Mapper

        public class VariableDataRange_Mapper : EntityTypeConfiguration<VariableDataRange>
        {
            public VariableDataRange_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_StackingDimension);
                Property(p => p.EF_IsHidden);
                Property(p => p.EF_StyleGroup);
                Property(p => p.EF_ValueVariableTemplateRefType);
                Property(p => p.EF_ValueVariableTemplateRefId);
                Property(p => p.EF_ValueVariableTemplateRefAltDimensionNumber);
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

        private Nullable<int> m_EF_ValueVariableTemplateRefType = null;
        [ForceMapped]
        internal int EF_ValueVariableTemplateRefType
        {
            get { return (int)m_ValueVariableTemplateRef.ModelObjectType; }
            set
            {
                m_EF_ValueVariableTemplateRefType = value;
            }
        }

        private Nullable<int> m_EF_ValueVariableTemplateRefId = null;
        [ForceMapped]
        internal int EF_ValueVariableTemplateRefId
        {
            get { return m_ValueVariableTemplateRef.ModelObjectIdAsInt; }
            set
            {
                m_EF_ValueVariableTemplateRefId = value;
                SetValueVariableTemplateRef();
            }
        }

        private Nullable<int> m_EF_ValueVariableTemplateRefAltDimensionNumber = null;
        [ForceMapped]
        internal Nullable<int> EF_ValueVariableTemplateRefAltDimensionNumber
        {
            get { return m_ValueVariableTemplateRef.AlternateDimensionNumber; }
            set
            {
                m_EF_ValueVariableTemplateRefAltDimensionNumber = value;
                SetValueVariableTemplateRef();
            }
        }

        private void SetValueVariableTemplateRef()
        {
            if (!m_EF_ValueVariableTemplateRefType.HasValue || !m_EF_ValueVariableTemplateRefId.HasValue)
            { throw new InvalidOperationException("Null Variable Template references are not allowed."); }
            else
            { m_ValueVariableTemplateRef = new ModelObjectReference((ModelObjectType)m_EF_ValueVariableTemplateRefType.Value, m_EF_ValueVariableTemplateRefId.Value, m_EF_ValueVariableTemplateRefAltDimensionNumber); }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<VariableDataRange>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<VariableDataRange>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<VariableDataRange> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<VariableDataRange>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<VariableDataRange>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<VariableDataRange>.UpdateNestedAggregateValues(DbContext context, VariableDataRange originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<VariableDataRange>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<VariableDataRange>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}