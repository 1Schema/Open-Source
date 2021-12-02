using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class VariableDataContainer : IEfAggregate_Detachable<VariableDataContainer>
    {
        #region Entity Framework Mapper

        public class VariableDataContainer_Mapper : EntityTypeConfiguration<VariableDataContainer>
        {
            public VariableDataContainer_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_VariableTitleContainerNumber);
                Property(p => p.EF_CommonTitleContainerNumber);
                Property(p => p.EF_StackingDimension);
                Property(p => p.EF_ChildNumbers);
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
        internal Nullable<int> EF_VariableTitleContainerNumber
        {
            get { return m_VariableTitleContainerNumber; }
            set { m_VariableTitleContainerNumber = value; }
        }

        [ForceMapped]
        internal Nullable<int> EF_CommonTitleContainerNumber
        {
            get { return m_CommonTitleContainerNumber; }
            set { m_CommonTitleContainerNumber = value; }
        }

        [ForceMapped]
        internal int EF_StackingDimension
        {
            get { return (int)m_StackingDimension; }
            set { StackingDimension = (Dimension)value; }
        }

        [ForceMapped]
        internal string EF_ChildNumbers
        {
            get { return m_ChildNumbers.ConvertToCollectionAsString(); }
            set { m_ChildNumbers = value.ConvertToTypedCollection<int>().ToList(); }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<VariableDataContainer>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<VariableDataContainer>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<VariableDataContainer> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<VariableDataContainer>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<VariableDataContainer>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<VariableDataContainer>.UpdateNestedAggregateValues(DbContext context, VariableDataContainer originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<VariableDataContainer>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<VariableDataContainer>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}