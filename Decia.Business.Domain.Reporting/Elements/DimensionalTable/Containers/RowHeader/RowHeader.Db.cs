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

namespace Decia.Business.Domain.Reporting
{
    public partial class RowHeader : IEfAggregate_Detachable<RowHeader>
    {
        #region Entity Framework Mapper

        public class RowHeader_Mapper : EntityTypeConfiguration<RowHeader>
        {
            public RowHeader_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_HoldsVariableContainer);
                Property(p => p.EF_NestedTitleContainerNumber);
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
        internal Nullable<bool> EF_HoldsVariableContainer
        {
            get { return m_HoldsVariableContainer; }
            set { m_HoldsVariableContainer = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_NestedTitleContainerNumber
        {
            get { return m_NestedTitleContainerNumber; }
            set { m_NestedTitleContainerNumber = value; }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<RowHeader>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<RowHeader>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<RowHeader> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<RowHeader>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<RowHeader>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<RowHeader>.UpdateNestedAggregateValues(DbContext context, RowHeader originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<RowHeader>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<RowHeader>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}