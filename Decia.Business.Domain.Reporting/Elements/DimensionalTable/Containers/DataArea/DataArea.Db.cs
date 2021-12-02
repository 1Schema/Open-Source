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
    public partial class DataArea : IEfAggregate_Detachable<DataArea>
    {
        #region Entity Framework Mapper

        public class DataArea_Mapper : EntityTypeConfiguration<DataArea>
        {
            public DataArea_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
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
        internal Nullable<int> EF_VariableDataContainerNumber
        {
            get { return m_VariableDataContainerNumber; }
            set { m_VariableDataContainerNumber = value; }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<DataArea>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<DataArea>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<DataArea> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<DataArea>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<DataArea>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<DataArea>.UpdateNestedAggregateValues(DbContext context, DataArea originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<DataArea>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<DataArea>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}