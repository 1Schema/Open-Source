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
    public partial class VariableTitleBox : IEfAggregate_Detachable<VariableTitleBox>
    {
        #region Entity Framework Mapper

        public class VariableTitleBox_Mapper : EntityTypeConfiguration<VariableTitleBox>
        {
            public VariableTitleBox_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_StackingDimension);
                Property(p => p.EF_StackingOrder);
                Property(p => p.EF_CommonOrder);
                Property(p => p.EF_ContainedVariableTitleRangeNumber);
                Property(p => p.EF_ContainedStructuralTitleRangeNumbers);
                Property(p => p.EF_ContainedTimeTitleRangeNumbers);
                Property(p => p.EF_RepeatGroup);
                Property(p => p.EF_RepeatMode);
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
        internal Nullable<int> EF_ContainedVariableTitleRangeNumber
        {
            get { return m_ContainedVariableTitleRangeNumber; }
            set { m_ContainedVariableTitleRangeNumber = value; }
        }

        [ForceMapped]
        internal string EF_ContainedStructuralTitleRangeNumbers
        {
            get { return m_ContainedStructuralTitleRangeNumbers.ConvertToCollectionAsString(); }
            set { m_ContainedStructuralTitleRangeNumbers = value.ConvertToTypedCollection<int>().ToList(); }
        }

        [ForceMapped]
        internal string EF_ContainedTimeTitleRangeNumbers
        {
            get { return m_ContainedTimeTitleRangeNumbers.ConvertToCollectionAsString(); }
            set { m_ContainedTimeTitleRangeNumbers = value.ConvertToTypedCollection<int>().ToList(); }
        }

        [ForceMapped]
        internal string EF_RepeatGroup
        {
            get { return m_RepeatGroup; }
            set { m_RepeatGroup = value; }
        }

        [ForceMapped]
        internal int EF_RepeatMode
        {
            get { return (int)m_RepeatMode; }
            set { m_RepeatMode = (RepeatMode)value; }
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
        ICollection<QuerySpecification> IEfAggregate<VariableTitleBox>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<VariableTitleBox>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<VariableTitleBox> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<VariableTitleBox>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<VariableTitleBox>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<VariableTitleBox>.UpdateNestedAggregateValues(DbContext context, VariableTitleBox originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<VariableTitleBox>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<VariableTitleBox>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}