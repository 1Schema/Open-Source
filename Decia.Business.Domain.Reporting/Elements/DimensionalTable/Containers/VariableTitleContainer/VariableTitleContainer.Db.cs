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
    public partial class VariableTitleContainer : IEfAggregate_Detachable<VariableTitleContainer>
    {
        #region Entity Framework Mapper

        public class VariableTitleContainer_Mapper : EntityTypeConfiguration<VariableTitleContainer>
        {
            public VariableTitleContainer_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_VariableDataContainerNumber);
                Property(p => p.EF_StackingDimension);
                Property(p => p.EF_VariableTitleBoxNumbers);
                Property(p => p.EF_DimensionsBySortIndex);
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

        [ForceMapped]
        internal int EF_StackingDimension
        {
            get { return (int)m_StackingDimension; }
            set { StackingDimension = (Dimension)value; }
        }

        [ForceMapped]
        internal string EF_VariableTitleBoxNumbers
        {
            get { return m_VariableTitleBoxNumbers.ConvertToCollectionAsString(); }
            set { m_VariableTitleBoxNumbers = value.ConvertToTypedCollection<int>().ToList(); }
        }

        [ForceMapped]
        internal string EF_DimensionsBySortIndex
        {
            get
            {
                var strValue = m_DimensionsBySortIndex.ConvertToDictionaryAsString();
                return strValue;
            }
            set
            {
                var dictValue = value.ConvertToTypedDictionary<int, ModelObjectReference>();
                m_DimensionsBySortIndex = new SortedDictionary<int, ModelObjectReference>(dictValue);
            }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<VariableTitleContainer>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<VariableTitleContainer>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<VariableTitleContainer> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<VariableTitleContainer>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<VariableTitleContainer>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<VariableTitleContainer>.UpdateNestedAggregateValues(DbContext context, VariableTitleContainer originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<VariableTitleContainer>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<VariableTitleContainer>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}