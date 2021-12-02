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
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class StructuralTitleRange : IEfAggregate_Detachable<StructuralTitleRange>
    {
        #region Entity Framework Mapper

        public class StructuralTitleRange_Mapper : EntityTypeConfiguration<StructuralTitleRange>
        {
            public StructuralTitleRange_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_ReportElementNumber);
                Property(p => p.EF_IsVariableTitleRelated);
                Property(p => p.EF_StackingDimension);
                Property(p => p.EF_IsHidden);
                Property(p => p.EF_StyleGroup);
                Property(p => p.EF_OnlyRepeatOnChange);
                Property(p => p.EF_MergeRepeatedValues);
                Property(p => p.EF_EntityTypeRefType);
                Property(p => p.EF_EntityTypeRefId);
                Property(p => p.EF_EntityTypeRefAltDimensionNumber);
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
        internal bool EF_IsVariableTitleRelated
        {
            get { return m_IsVariableTitleRelated; }
            set { m_IsVariableTitleRelated = value; }
        }

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

        [ForceMapped]
        internal bool EF_OnlyRepeatOnChange
        {
            get { return m_OnlyRepeatOnChange; }
            set { m_OnlyRepeatOnChange = value; }
        }

        [ForceMapped]
        internal bool EF_MergeRepeatedValues
        {
            get { return m_MergeRepeatedValues; }
            set { m_MergeRepeatedValues = value; }
        }

        private Nullable<int> m_EF_EntityTypeRefType = null;
        [ForceMapped]
        internal int EF_EntityTypeRefType
        {
            get { return (int)m_EntityTypeRef.ModelObjectType; }
            set
            {
                m_EF_EntityTypeRefType = value;
            }
        }

        private Nullable<int> m_EF_EntityTypeRefId = null;
        [ForceMapped]
        internal int EF_EntityTypeRefId
        {
            get { return m_EntityTypeRef.ModelObjectIdAsInt; }
            set
            {
                m_EF_EntityTypeRefId = value;
                SetEntityTypeRef();
            }
        }

        private Nullable<int> m_EF_EntityTypeRefAltDimensionNumber = null;
        [ForceMapped]
        internal Nullable<int> EF_EntityTypeRefAltDimensionNumber
        {
            get { return m_EntityTypeRef.AlternateDimensionNumber; }
            set
            {
                m_EF_EntityTypeRefAltDimensionNumber = value;
                SetEntityTypeRef();
            }
        }

        private void SetEntityTypeRef()
        {
            if (!m_EF_EntityTypeRefType.HasValue || !m_EF_EntityTypeRefId.HasValue)
            { throw new InvalidOperationException("Null Entity Type references are not allowed."); }
            else
            { m_EntityTypeRef = new ModelObjectReference((ModelObjectType)m_EF_EntityTypeRefType.Value, m_EF_EntityTypeRefId.Value, m_EF_EntityTypeRefAltDimensionNumber); }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<StructuralTitleRange>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<StructuralTitleRange>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<StructuralTitleRange> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        void IEfAggregate<StructuralTitleRange>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            ReadNestedAggregateValuesBase(context, rootQueryPredicate, batchReadState);
        }

        void IEfAggregate<StructuralTitleRange>.AddNestedAggregateValues(DbContext context)
        {
            AddNestedAggregateValuesBase(context);
        }

        void IEfAggregate<StructuralTitleRange>.UpdateNestedAggregateValues(DbContext context, StructuralTitleRange originalAggregate)
        {
            UpdateNestedAggregateValuesBase(context, originalAggregate);
        }

        void IEfAggregate<StructuralTitleRange>.DeleteNestedAggregateValues(DbContext context)
        {
            DeleteNestedAggregateValuesBase(context);
        }

        void IEfAggregate_Detachable<StructuralTitleRange>.DetachNestedAggregateValues(DbContext context)
        {
            DetachNestedAggregateValuesBase(context);
        }
    }
}