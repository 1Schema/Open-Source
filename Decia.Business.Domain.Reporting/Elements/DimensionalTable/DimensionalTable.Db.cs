using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;

namespace Decia.Business.Domain.Reporting
{
    public partial class DimensionalTable : IEfAggregate_Detachable<DimensionalTable>
    {
        #region Entity Framework Mapper

        public class DimensionalTable_Mapper : EntityTypeConfiguration<DimensionalTable>
        {
            public DimensionalTable_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_IsTransposed);
                Property(p => p.EF_TableHeaderNumber);
                Property(p => p.EF_RowHeaderNumber);
                Property(p => p.EF_ColumnHeaderNumber);
                Property(p => p.EF_DataAreaNumber);
                Property(p => p.EF_CommonTitleContainerNumber);
                Property(p => p.EF_VariableTitleContainerNumber);
                Property(p => p.EF_VariableDataContainerNumber);
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
        internal bool EF_IsTransposed
        {
            get { return m_IsTransposed; }
            set { m_IsTransposed = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_TableHeaderNumber
        {
            get { return m_TableHeaderNumber; }
            set { m_TableHeaderNumber = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_RowHeaderNumber
        {
            get { return m_RowHeaderNumber; }
            set { m_RowHeaderNumber = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_ColumnHeaderNumber
        {
            get { return m_ColumnHeaderNumber; }
            set { m_ColumnHeaderNumber = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_DataAreaNumber
        {
            get { return m_DataAreaNumber; }
            set { m_DataAreaNumber = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_CommonTitleContainerNumber
        {
            get { return m_CommonTitleContainerNumber; }
            set { m_CommonTitleContainerNumber = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_VariableTitleContainerNumber
        {
            get { return m_VariableTitleContainerNumber; }
            set { m_VariableTitleContainerNumber = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_VariableDataContainerNumber
        {
            get { return m_VariableDataContainerNumber; }
            set { m_VariableDataContainerNumber = value; }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<DimensionalTable>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<DimensionalTable>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<DimensionalTable> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<DimensionalTable>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<DimensionalTable>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<DimensionalTable>.UpdateNestedAggregateValues(DbContext context, DimensionalTable originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<DimensionalTable>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<DimensionalTable>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}