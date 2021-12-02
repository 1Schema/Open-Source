using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Testing;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public abstract partial class ReportElementBase<KDO> : IEfEntity_DbQueryable<ReportElementId, string>
    {
        #region IEfEntity_DbQueryable<ReportElementId, string> Implementation

        public static string KeyMatchingSql { get; protected set; }

        [NotMapped]
        string IEfEntity<string>.EF_Id
        {
            get { return m_Key.ToString(); }
            set { /* do nothing */ }
        }

        string IEfEntity_DbQueryable<ReportElementId>.ConvertKey_ToMatchableText(ReportElementId key)
        {
            var values = new object[] { key.ProjectGuid, key.RevisionNumber_NonNull, key.ModelTemplateNumber, key.ReportGuid, key.ReportElementNumber };
            var valuesAsText = AdoNetUtils.CovertToKeyMatchingText(values, false);
            return valuesAsText;
        }

        string IEfEntity_DbQueryable<ReportElementId>.GetSqlCode_ForKeyMatching()
        {
            if (!string.IsNullOrWhiteSpace(KeyMatchingSql))
            { return KeyMatchingSql; }

            Func<IReportElement<KDO>, Type> projectGuidType = ((IReportElement<KDO> re) => this.EF_ProjectGuid.GetType());
            Func<IReportElement<KDO>, Type> revisionNumberType = ((IReportElement<KDO> re) => this.EF_RevisionNumber.GetType());
            Func<IReportElement<KDO>, Type> modelTemplateNumberType = ((IReportElement<KDO> re) => this.EF_ModelTemplateNumber.GetType());
            Func<IReportElement<KDO>, Type> reportGuidType = ((IReportElement<KDO> re) => this.EF_ReportGuid.GetType());
            Func<IReportElement<KDO>, Type> reportElementNumberType = ((IReportElement<KDO> re) => this.EF_ReportElementNumber.GetType());

            var projectGuidName = ClassReflector.GetPropertyName((IReportElement<KDO> re) => this.EF_ProjectGuid);
            var revisionNumberName = ClassReflector.GetPropertyName((IReportElement<KDO> re) => this.EF_RevisionNumber);
            var modelTemplateNumberName = ClassReflector.GetPropertyName((IReportElement<KDO> re) => this.EF_ModelTemplateNumber);
            var reportGuidName = ClassReflector.GetPropertyName((IReportElement<KDO> re) => this.EF_ReportGuid);
            var reportElementNumberName = ClassReflector.GetPropertyName((IReportElement<KDO> re) => this.EF_ReportElementNumber);

            var projectGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(projectGuidType(this), projectGuidName);
            var revisionNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(revisionNumberType(this), revisionNumberName);
            var modelTemplateNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(modelTemplateNumberType(this), modelTemplateNumberName);
            var reportGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(reportGuidType(this), reportGuidName);
            var reportElementNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(reportElementNumberType(this), reportElementNumberName);

            var values = new object[] { projectGuidText, revisionNumberText, modelTemplateNumberText, reportGuidText, reportElementNumberText };
            KeyMatchingSql = AdoNetUtils.CovertToKeyMatchingText(values, true);
            return KeyMatchingSql;
        }

        #endregion

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public Guid EF_ProjectGuid
        {
            get { return m_Key.ProjectGuid; }
            set { m_Key = new ReportElementId(value, m_Key.RevisionNumber_NonNull, m_Key.ModelTemplateNumber, m_Key.ReportGuid, m_Key.ReportElementNumber, false); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_Key.RevisionNumber_NonNull; }
            set { m_Key = new ReportElementId(m_Key.ProjectGuid, value, m_Key.ModelTemplateNumber, m_Key.ReportGuid, m_Key.ReportElementNumber, false); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public int EF_ModelTemplateNumber
        {
            get { return m_Key.ModelTemplateNumber; }
            set { m_Key = new ReportElementId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, value, m_Key.ReportGuid, m_Key.ReportElementNumber, false); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3)]
        public Guid EF_ReportGuid
        {
            get { return m_Key.ReportGuid; }
            set { m_Key = new ReportElementId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, m_Key.ModelTemplateNumber, value, m_Key.ReportElementNumber, false); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 4)]
        public int EF_ReportElementNumber
        {
            get { return m_Key.ReportElementNumber; }
            set { m_Key = new ReportElementId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, m_Key.ModelTemplateNumber, m_Key.ReportGuid, value, false); }
        }

        [ForceMapped]
        public string EF_Name
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_Name);
                return m_Name;
            }
            internal set
            {
                m_Name = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_Name);
            }
        }

        [ForceMapped]
        public Nullable<int> EF_ParentElementNumber
        {
            get { return m_ParentElementNumber; }
            internal set { m_ParentElementNumber = value; }
        }

        [ForceMapped]
        public bool? EF_IsLocked
        {
            get { return m_IsLocked; }
            internal set { m_IsLocked = value; }
        }

        [ForceMapped]
        public int EF_ZOrder
        {
            get { return m_ZOrder; }
            internal set { m_ZOrder = value; }
        }

        [ForceMapped]
        public bool? EF_IsParentEditable
        {
            get { return m_IsParentEditable; }
            internal set { m_IsParentEditable = value; }
        }

        [ForceMapped]
        public bool? EF_IsDirectlyDeletable
        {
            get { return m_IsDirectlyDeletable; }
            internal set { m_IsDirectlyDeletable = value; }
        }

        [ForceMapped]
        public int EF_DefaultStyleType
        {
            get { return (int)m_DefaultStyleType; }
            internal set { m_DefaultStyleType = (KnownStyleType)value; }
        }

        [ForceMapped]
        public Nullable<int> EF_StyleInheritanceElementNumber
        {
            get { return m_StyleInheritanceElementNumber; }
            internal set { m_StyleInheritanceElementNumber = value; }
        }

        #region IPermissionable Database Persistence Properties

        [ForceMapped]
        public Guid EF_CreatorGuid
        {
            get
            {
                this.AssertCreatorIsNotAnonymous();
                return m_CreatorGuid;
            }
            internal set { m_CreatorGuid = value; }
        }

        [ForceMapped]
        public DateTime EF_CreationDate
        {
            get { return m_CreationDate; }
            internal set { m_CreationDate = value; }
        }

        [ForceMapped]
        public int EF_OwnerType
        {
            get { return (int)m_OwnerType; }
            internal set { m_OwnerType = (SiteActorType)value; }
        }

        [ForceMapped]
        public Guid EF_OwnerGuid
        {
            get { return m_OwnerGuid; }
            internal set { m_OwnerGuid = value; }
        }

        #endregion

        #region IDeletable Database Persistence Properties

        [ForceMapped]
        public bool EF_IsDeleted
        {
            get { return m_IsDeleted; }
            internal set { m_IsDeleted = value; }
        }

        [ForceMapped]
        public Nullable<Guid> EF_DeleterGuid
        {
            get
            {
                if (!this.IsDeleted)
                { return null; }

                this.AssertDeleterIsNotAnonymous();
                return m_DeleterGuid.Value;
            }
            internal set { m_DeleterGuid = value; }
        }

        [ForceMapped]
        public Nullable<DateTime> EF_DeletionDate
        {
            get
            {
                if (!this.IsDeleted)
                { return null; }

                return m_DeletionDate.Value;
            }
            internal set { m_DeletionDate = value; }
        }

        #endregion

        #region IOrderable Database Persistence Properties

        [ForceMapped]
        public Nullable<long> EF_OrderNumber
        {
            get { return m_OrderNumber; }
            internal set { m_OrderNumber = value; }
        }

        #endregion

        #region ReportElementBase<KDO> Aggregate Persistence Properties and Methods

        [NotMapped]
        internal ICollection<SaveableDimensionLayout> EF_DimensionLayouts
        {
            get
            {
                List<SaveableDimensionLayout> dimLayouts = new List<SaveableDimensionLayout>();
                dimLayouts.Add(m_DimensionLayout_X);
                dimLayouts.Add(m_DimensionLayout_Y);
                return dimLayouts;
            }
            set
            {
                if (value.Count != 2)
                { throw new InvalidOperationException("Unexpected number of Dimension Types encountered."); }

                foreach (var sdl in value)
                {
                    sdl.EditabilitySpec = GetEditabilitySpecForLayout(sdl.Dimension);
                    sdl.DefaultLayout_Value = GetDefaultLayout(sdl.Dimension);

                    if (sdl.Dimension == Dimension.X)
                    { m_DimensionLayout_X = sdl; }
                    else if (sdl.Dimension == Dimension.Y)
                    { m_DimensionLayout_Y = sdl; }
                    else
                    { throw new InvalidOperationException("Unsupported Dimension Type encountered."); }
                }
            }
        }

        [NotMapped]
        internal ICollection<SaveableElementStyle> EF_ElementStyles
        {
            get
            {
                List<SaveableElementStyle> styles = new List<SaveableElementStyle>();
                styles.Add(m_ElementStyle);
                return styles;
            }
            set
            {
                if (value.Count != 1)
                { throw new InvalidOperationException("Unexpected number of Element Styles encountered."); }

                var ses = value.First();

                ses.EditabilitySpec = GetEditabilitySpecForStyle();
                ses.DefaultStyle_Value = GetDefaultStyle();

                m_ElementStyle = ses;
            }
        }

        protected void ReadNestedAggregateValuesBase(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            IQueryable<SaveableDimensionLayout> sdlQuery = context.Set<SaveableDimensionLayout>().Where((SaveableDimensionLayout sdl) => ((sdl.EF_ProjectGuid == Key.ProjectGuid) && (sdl.EF_RevisionNumber == Key.RevisionNumber) && (sdl.EF_ModelTemplateNumber == Key.ModelTemplateNumber) && (sdl.EF_ReportGuid == Key.ReportGuid) && (sdl.EF_ReportElementNumber == Key.ReportElementNumber)));
            this.EF_DimensionLayouts = sdlQuery.ToList();

            IQueryable<SaveableElementStyle> sesQuery = context.Set<SaveableElementStyle>().Where((SaveableElementStyle ses) => ((ses.EF_ProjectGuid == Key.ProjectGuid) && (ses.EF_RevisionNumber == Key.RevisionNumber) && (ses.EF_ModelTemplateNumber == Key.ModelTemplateNumber) && (ses.EF_ReportGuid == Key.ReportGuid) && (ses.EF_ReportElementNumber == Key.ReportElementNumber)));
            this.EF_ElementStyles = sesQuery.ToList();
        }

        protected void AddNestedAggregateValuesBase(DbContext context)
        {
            Expression<Func<ICollection<SaveableDimensionLayout>>> sdlGetterExpression = () => this.EF_DimensionLayouts;
            EfAggregateUtilities.AddNestedValues<SaveableDimensionLayout>(context, sdlGetterExpression);

            Expression<Func<ICollection<SaveableElementStyle>>> sesGetterExpression = () => this.EF_ElementStyles;
            EfAggregateUtilities.AddNestedValues<SaveableElementStyle>(context, sesGetterExpression);
        }

        protected void UpdateNestedAggregateValuesBase(DbContext context, ReportElementBase<KDO> originalAggregate)
        {
            Expression<Func<SaveableDimensionLayout, Dimension>> sdlKeyGetterExpression = ((SaveableDimensionLayout nestedObj) => nestedObj.Dimension);
            Expression<Func<SaveableDimensionLayout, SaveableDimensionLayout, bool>> sdlMatchExpression = ((SaveableDimensionLayout obj1, SaveableDimensionLayout obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_ModelTemplateNumber == obj2.EF_ModelTemplateNumber) && (obj1.EF_ReportGuid == obj2.EF_ReportGuid) && (obj1.EF_ReportElementNumber == obj2.EF_ReportElementNumber) && (obj1.EF_Dimension == obj2.EF_Dimension)));
            Expression<Func<ReportElementBase<KDO>, IDictionary<Dimension, SaveableDimensionLayout>>> sdlDictionaryGetterExpression = (ReportElementBase<KDO> agg) => agg.EF_DimensionLayouts.ToDictionary(val => val.Dimension, val => val);
            EfAggregateUtilities.UpdateNestedValues<ReportElementBase<KDO>, Dimension, SaveableDimensionLayout>(context, this, originalAggregate, sdlKeyGetterExpression, sdlMatchExpression, sdlDictionaryGetterExpression);

            Expression<Func<SaveableElementStyle, bool>> sesKeyGetterExpression = ((SaveableElementStyle nestedObj) => true);
            Expression<Func<SaveableElementStyle, SaveableElementStyle, bool>> sesMatchExpression = ((SaveableElementStyle obj1, SaveableElementStyle obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_ModelTemplateNumber == obj2.EF_ModelTemplateNumber) && (obj1.EF_ReportGuid == obj2.EF_ReportGuid) && (obj1.EF_ReportElementNumber == obj2.EF_ReportElementNumber)));
            Expression<Func<ReportElementBase<KDO>, IDictionary<bool, SaveableElementStyle>>> sesDictionaryGetterExpression = (ReportElementBase<KDO> agg) => agg.EF_ElementStyles.ToDictionary(val => true, val => val);
            EfAggregateUtilities.UpdateNestedValues<ReportElementBase<KDO>, bool, SaveableElementStyle>(context, this, originalAggregate, sesKeyGetterExpression, sesMatchExpression, sesDictionaryGetterExpression);
        }

        protected void DeleteNestedAggregateValuesBase(DbContext context)
        {
            Expression<Func<SaveableDimensionLayout, bool>> sdlDeleteQueryExpression = ((SaveableDimensionLayout sdl) => ((sdl.EF_ProjectGuid == Key.ProjectGuid) && (sdl.EF_RevisionNumber == Key.RevisionNumber) && (sdl.EF_ModelTemplateNumber == Key.ModelTemplateNumber) && (sdl.EF_ReportGuid == Key.ReportGuid) && (sdl.EF_ReportElementNumber == Key.ReportElementNumber)));
            EfAggregateUtilities.DeleteNestedValues(context, sdlDeleteQueryExpression);

            Expression<Func<SaveableElementStyle, bool>> sesDeleteQueryExpression = ((SaveableElementStyle ses) => ((ses.EF_ProjectGuid == Key.ProjectGuid) && (ses.EF_RevisionNumber == Key.RevisionNumber) && (ses.EF_ModelTemplateNumber == Key.ModelTemplateNumber) && (ses.EF_ReportGuid == Key.ReportGuid) && (ses.EF_ReportElementNumber == Key.ReportElementNumber)));
            EfAggregateUtilities.DeleteNestedValues(context, sesDeleteQueryExpression);
        }

        protected void DetachNestedAggregateValuesBase(DbContext context)
        {
            foreach (var dimensionLayout in EF_DimensionLayouts)
            {
                context.Entry(dimensionLayout).State = EntityState.Detached;
            }

            foreach (var elementStyle in EF_ElementStyles)
            {
                context.Entry(elementStyle).State = EntityState.Detached;
            }
        }

        #endregion
    }
}