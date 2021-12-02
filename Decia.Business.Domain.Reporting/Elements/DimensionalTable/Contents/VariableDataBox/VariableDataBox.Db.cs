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
    public partial class VariableDataBox : IEfAggregate_Detachable<VariableDataBox>
    {
        #region Entity Framework Mapper

        public class VariableDataBox_Mapper : EntityTypeConfiguration<VariableDataBox>
        {
            public VariableDataBox_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_RelatedVariableTitleBoxNumber);
                Property(p => p.EF_RelatedCommonTitleBoxNumber);
                Property(p => p.EF_StackingDimension);
                Property(p => p.EF_StackingOrder);
                Property(p => p.EF_CommonOrder);
                Property(p => p.EF_ContainedVariableDataRangeNumber);
                Property(p => p.EF_StyleGroup);
                Property(p => p.EF_VariableTemplateRefType);
                Property(p => p.EF_VariableTemplateRefId);
                Property(p => p.EF_VariableTemplateRefAltDimensionNumber);
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
        internal Nullable<int> EF_RelatedVariableTitleBoxNumber
        {
            get { return m_RelatedVariableTitleBoxNumber; }
            set { m_RelatedVariableTitleBoxNumber = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_RelatedCommonTitleBoxNumber
        {
            get { return m_RelatedCommonTitleBoxNumber; }
            set { m_RelatedCommonTitleBoxNumber = value; }
        }

        [ForceMapped]
        internal int EF_StackingDimension
        {
            get { return (int)m_StackingDimension; }
            set { StackingDimension = (Dimension)value; }
        }

        [ForceMapped]
        internal int EF_StackingOrder
        {
            get { return StackingOrder; }
            set { /* do nothing - this is a cached value */ }
        }

        [ForceMapped]
        internal int EF_CommonOrder
        {
            get { return CommonOrder; }
            set { /* do nothing - this is a cached value */ }
        }

        [ForceMapped]
        internal Nullable<int> EF_ContainedVariableDataRangeNumber
        {
            get { return m_ContainedVariableDataRangeNumber; }
            set { m_ContainedVariableDataRangeNumber = value; }
        }

        [ForceMapped]
        internal string EF_StyleGroup
        {
            get { return m_StyleGroup; }
            set { m_StyleGroup = value; }
        }

        private Nullable<int> m_EF_VariableTemplateRefType = null;
        [ForceMapped]
        internal int EF_VariableTemplateRefType
        {
            get { return (int)m_VariableTemplateRef.ModelObjectType; }
            set
            {
                m_EF_VariableTemplateRefType = value;
            }
        }

        private Nullable<int> m_EF_VariableTemplateRefId = null;
        [ForceMapped]
        internal int EF_VariableTemplateRefId
        {
            get { return m_VariableTemplateRef.ModelObjectIdAsInt; }
            set
            {
                m_EF_VariableTemplateRefId = value;
                SetVariableTemplateRef();
            }
        }

        private Nullable<int> m_EF_VariableTemplateRefAltDimensionNumber = null;
        [ForceMapped]
        internal Nullable<int> EF_VariableTemplateRefAltDimensionNumber
        {
            get { return m_VariableTemplateRef.AlternateDimensionNumber; }
            set
            {
                m_EF_VariableTemplateRefAltDimensionNumber = value;
                SetVariableTemplateRef();
            }
        }

        private void SetVariableTemplateRef()
        {
            if (!m_EF_VariableTemplateRefType.HasValue || !m_EF_VariableTemplateRefId.HasValue)
            { throw new InvalidOperationException("Null Variable Template references are not allowed."); }
            else
            { m_VariableTemplateRef = new ModelObjectReference((ModelObjectType)m_EF_VariableTemplateRefType.Value, m_EF_VariableTemplateRefId.Value, m_EF_VariableTemplateRefAltDimensionNumber); }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<VariableDataBox>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<VariableDataBox>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<VariableDataBox> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<VariableDataBox>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<VariableDataBox>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<VariableDataBox>.UpdateNestedAggregateValues(DbContext context, VariableDataBox originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<VariableDataBox>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<VariableDataBox>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}