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
    public partial class CommonTitleBox : IEfAggregate_Detachable<CommonTitleBox>
    {
        #region Entity Framework Mapper

        public class CommonTitleBox_Mapper : EntityTypeConfiguration<CommonTitleBox>
        {
            public CommonTitleBox_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_StackingDimension);
                Property(p => p.EF_StackingOrder);
                Property(p => p.EF_CommonOrder);
                Property(p => p.EF_ContainedStructuralTitleRangeNumbers);
                Property(p => p.EF_ContainedTimeTitleRangeNumbers);
                Property(p => p.EF_StyleGroup);
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
        internal string EF_StyleGroup
        {
            get { return m_StyleGroup; }
            set { m_StyleGroup = value; }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<CommonTitleBox>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<CommonTitleBox>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<CommonTitleBox> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<CommonTitleBox>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<CommonTitleBox>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<CommonTitleBox>.UpdateNestedAggregateValues(DbContext context, CommonTitleBox originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<CommonTitleBox>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<CommonTitleBox>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}