using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
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
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class Report : IEfEntity_DbQueryable<ReportId, string>, IEfAggregate_Detachable<Report>
    {
        #region Entity Framework Mapper

        public class Report_Mapper : EntityTypeConfiguration<Report>
        {
            public Report_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ModelTemplateNumber);
                Property(p => p.EF_ReportGuid);
                Property(p => p.EF_Name);
                Property(p => p.EF_Description);
                Property(p => p.EF_IsLocked);
                Property(p => p.EF_StructuralTypeType);
                Property(p => p.EF_StructuralTypeId);
                Property(p => p.EF_StructuralTypeDimensionNumber);
                Property(p => p.EF_HasPrimaryTimePeriod);
                Property(p => p.EF_HasSecondaryTimePeriod);
                Property(p => p.EF_ZoomFactor);
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

        #region IEfEntity_DbQueryable<ReportId, string> Implementation

        public static string KeyMatchingSql { get; protected set; }

        [NotMapped]
        string IEfEntity<string>.EF_Id
        {
            get { return m_Key.ToString(); }
            set { /* do nothing */ }
        }

        string IEfEntity_DbQueryable<ReportId>.ConvertKey_ToMatchableText(ReportId key)
        {
            return ConvertKey_ToMatchableText(key);
        }

        protected static string ConvertKey_ToMatchableText(ReportId key)
        {
            var values = new object[] { key.ProjectGuid, key.RevisionNumber_NonNull, key.ModelTemplateNumber, key.ReportGuid };
            var valuesAsText = AdoNetUtils.CovertToKeyMatchingText(values, false);
            return valuesAsText;
        }

        string IEfEntity_DbQueryable<ReportId>.GetSqlCode_ForKeyMatching()
        {
            return GetSqlCode_ForKeyMatching();
        }

        protected static string GetSqlCode_ForKeyMatching()
        {
            if (!string.IsNullOrWhiteSpace(KeyMatchingSql))
            { return KeyMatchingSql; }

            var projectGuidName = ClassReflector.GetPropertyName((Report r) => r.EF_ProjectGuid);
            var revisionNumberName = ClassReflector.GetPropertyName((Report r) => r.EF_RevisionNumber);
            var modelTemplateNumberName = ClassReflector.GetPropertyName((Report r) => r.EF_ModelTemplateNumber);
            var reportGuidName = ClassReflector.GetPropertyName((Report r) => r.EF_ReportGuid);

            var thisType = typeof(Report);
            var projectGuidType = ClassReflector.GetPropertyByName(thisType, projectGuidName).PropertyType;
            var revisionNumberType = ClassReflector.GetPropertyByName(thisType, revisionNumberName).PropertyType;
            var modelTemplateNumberType = ClassReflector.GetPropertyByName(thisType, modelTemplateNumberName).PropertyType;
            var reportGuidType = ClassReflector.GetPropertyByName(thisType, reportGuidName).PropertyType;

            var projectGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(projectGuidType, projectGuidName);
            var revisionNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(revisionNumberType, revisionNumberName);
            var modelTemplateNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(modelTemplateNumberType, modelTemplateNumberName);
            var reportGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(reportGuidType, reportGuidName);

            var values = new object[] { projectGuidText, revisionNumberText, modelTemplateNumberText, reportGuidText };
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
            set { m_Key = new ReportId(value, m_Key.RevisionNumber_NonNull, m_Key.ModelTemplateNumber, m_Key.ReportGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_Key.RevisionNumber_NonNull; }
            set { m_Key = new ReportId(m_Key.ProjectGuid, value, m_Key.ModelTemplateNumber, m_Key.ReportGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public int EF_ModelTemplateNumber
        {
            get { return m_Key.ModelTemplateNumber; }
            set { m_Key = new ReportId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, value, m_Key.ReportGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3)]
        public Guid EF_ReportGuid
        {
            get { return m_Key.ReportGuid; }
            set { m_Key = new ReportId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, m_Key.ModelTemplateNumber, value); }
        }

        [ForceMapped]
        internal string EF_Name
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_Name);
                return m_Name;
            }
            set
            {
                m_Name = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_Name);
            }
        }

        [ForceMapped]
        internal string EF_Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        [ForceMapped]
        internal bool? EF_IsLocked
        {
            get { return m_IsLocked; }
            set { m_IsLocked = value; }
        }

        private Nullable<int> m_EF_StructuralTypeType = null;
        [ForceMapped]
        internal int EF_StructuralTypeType
        {
            get { return (int)m_StructuralTypeRef.ModelObjectType; }
            set
            {
                m_EF_StructuralTypeType = value;
            }
        }

        private Nullable<int> m_EF_StructuralTypeId = null;
        [ForceMapped]
        internal int EF_StructuralTypeId
        {
            get { return m_StructuralTypeRef.ModelObjectIdAsInt; }
            set
            {
                m_EF_StructuralTypeId = value;
                SetStructuralTypeReference();
            }
        }

        private Nullable<int> m_EF_StructuralTypeDimensionNumber = null;
        [ForceMapped]
        internal Nullable<int> EF_StructuralTypeDimensionNumber
        {
            get { return m_StructuralTypeRef.AlternateDimensionNumber.HasValue ? m_StructuralTypeRef.AlternateDimensionNumber : (Nullable<int>)null; }
            set
            {
                m_EF_StructuralTypeDimensionNumber = value;
                SetStructuralTypeReference();
            }
        }

        private void SetStructuralTypeReference()
        {
            if (!m_EF_StructuralTypeType.HasValue || !m_EF_StructuralTypeId.HasValue)
            { m_StructuralTypeRef = Default_StructuralTypeRef; }
            else
            { m_StructuralTypeRef = new ModelObjectReference((ModelObjectType)m_EF_StructuralTypeType.Value, m_EF_StructuralTypeId.Value, m_EF_StructuralTypeDimensionNumber); }
        }

        [ForceMapped]
        internal bool EF_HasPrimaryTimePeriod
        {
            get { return m_HasPrimaryTimePeriod; }
            set { m_HasPrimaryTimePeriod = value; }
        }

        [ForceMapped]
        internal bool EF_HasSecondaryTimePeriod
        {
            get { return m_HasSecondaryTimePeriod; }
            set { m_HasSecondaryTimePeriod = value; }
        }

        [ForceMapped]
        internal double EF_ZoomFactor
        {
            get { return m_ZoomFactor; }
            set { m_ZoomFactor = value; }
        }

        #region IPermissionable Database Persistence Properties

        [ForceMapped]
        internal Guid EF_CreatorGuid
        {
            get
            {
                this.AssertCreatorIsNotAnonymous();
                return m_CreatorGuid;
            }
            set { m_CreatorGuid = value; }
        }

        [ForceMapped]
        internal DateTime EF_CreationDate
        {
            get { return m_CreationDate; }
            set { m_CreationDate = value; }
        }

        [ForceMapped]
        internal int EF_OwnerType
        {
            get { return (int)m_OwnerType; }
            set { m_OwnerType = (SiteActorType)value; }
        }

        [ForceMapped]
        internal Guid EF_OwnerGuid
        {
            get { return m_OwnerGuid; }
            set { m_OwnerGuid = value; }
        }

        #endregion

        #region IDeletable Database Persistence Properties

        [ForceMapped]
        internal bool EF_IsDeleted
        {
            get { return m_IsDeleted; }
            set { m_IsDeleted = value; }
        }

        [ForceMapped]
        internal Nullable<Guid> EF_DeleterGuid
        {
            get
            {
                if (!this.IsDeleted)
                { return null; }

                this.AssertDeleterIsNotAnonymous();
                return m_DeleterGuid.Value;
            }
            set { m_DeleterGuid = value; }
        }

        [ForceMapped]
        internal Nullable<DateTime> EF_DeletionDate
        {
            get
            {
                if (!this.IsDeleted)
                { return null; }

                return m_DeletionDate.Value;
            }
            set { m_DeletionDate = value; }
        }

        #endregion

        #region IOrderable Database Persistence Properties

        [ForceMapped]
        internal Nullable<long> EF_OrderNumber
        {
            get { return m_OrderNumber; }
            set { m_OrderNumber = value; }
        }

        #endregion

        #region Report Aggregate Persistence Properties and Methods

        [NotMapped]
        internal ICollection<SaveableDimensionLayout> EF_DimensionLayouts
        {
            get
            {
                List<SaveableDimensionLayout> dimLayouts = new List<SaveableDimensionLayout>();
                dimLayouts.Add(m_ReportArea_DimensionLayout_X);
                dimLayouts.Add(m_ReportArea_DimensionLayout_Y);
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
                    { m_ReportArea_DimensionLayout_X = sdl; }
                    else if (sdl.Dimension == Dimension.Y)
                    { m_ReportArea_DimensionLayout_Y = sdl; }
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
                styles.Add(m_ReportAreaStyle);
                styles.Add(m_OutsideAreaStyle);
                styles.Add(m_DefaultTitleStyle);
                styles.Add(m_DefaultHeaderStyle);
                styles.Add(m_DefaultDataStyle);
                return styles;
            }
            set
            {
                if (value.Count != 5)
                { throw new InvalidOperationException("Unexpected number of Element Styles encountered."); }

                var styles = value.ToDictionary(ses => ses.Key.ReportElementNumber, ses => ses);

                m_ReportAreaStyle = styles[(int)ReservedElementType.Report];
                m_OutsideAreaStyle = styles[(int)ReservedElementType.AreaOutsideReport];
                m_DefaultTitleStyle = styles[(int)ReservedElementType.TitleTemplate];
                m_DefaultHeaderStyle = styles[(int)ReservedElementType.HeaderTemplate];
                m_DefaultDataStyle = styles[(int)ReservedElementType.DataTemplate];
            }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<Report>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<Report>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<Report> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        internal static readonly string TypeName_Report = typeof(Report).Name;
        internal static readonly string TypeName_ElementStyle = typeof(SaveableElementStyle).Name;
        internal static readonly string TypeName_DimensionLayout = typeof(SaveableDimensionLayout).Name;

        void IEfAggregate<Report>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            var elementStylesDict = new Dictionary<ReportId, List<SaveableElementStyle>>();
            var dimensionLayoutsDict = new Dictionary<ReportId, List<SaveableDimensionLayout>>();

            PerformBatchRead(context, batchReadState, ref elementStylesDict, ref dimensionLayoutsDict);

            var elementStylesList = (elementStylesDict.ContainsKey(this.Key)) ? elementStylesDict[this.Key] : new List<SaveableElementStyle>();
            this.EF_ElementStyles = elementStylesList;

            var dimensionLayoutsList = (dimensionLayoutsDict.ContainsKey(this.Key)) ? dimensionLayoutsDict[this.Key] : new List<SaveableDimensionLayout>();
            this.EF_DimensionLayouts = dimensionLayoutsList;
        }

        private void PerformBatchRead(DbContext context, Dictionary<string, object> batchReadState, ref Dictionary<ReportId, List<SaveableElementStyle>> elementStylesDict, ref Dictionary<ReportId, List<SaveableDimensionLayout>> dimensionLayoutsDict)
        {
            var rootObjects = (batchReadState[TypeName_Report] as IEnumerable<Report>);

            if (batchReadState.ContainsKey(TypeName_ElementStyle))
            {
                elementStylesDict = (batchReadState[TypeName_ElementStyle] as Dictionary<ReportId, List<SaveableElementStyle>>);
            }
            else
            {
                var rootKeys = rootObjects.Select(x => x.Key).ToList();
                var keysAsSqlMatchableText = IEfEntityUtils.GetKeysAsSqlMatchableText(this, rootKeys);
                var keyMatchingSql = GetSqlCode_ForKeyMatching();

                var elementStylesSet = context.Set<SaveableElementStyle>();
                var elementStylesQuery = elementStylesSet.CreateKeyMatchingDbSqlQuery(keysAsSqlMatchableText, keyMatchingSql);
                var elementStylesSelect = (from esq in elementStylesQuery where Report.Report_AllElementNumbers.Contains(esq.EF_ReportElementNumber) select esq);
                var elementStyles = elementStylesSelect.ToList();

                foreach (var es in elementStyles)
                {
                    var key = new ReportId(es.EF_ProjectGuid, es.EF_RevisionNumber, es.EF_ModelTemplateNumber, es.EF_ReportGuid);
                    if (!elementStylesDict.ContainsKey(key))
                    { elementStylesDict.Add(key, new List<SaveableElementStyle>()); }
                    elementStylesDict[key].Add(es);
                }

                batchReadState.Add(TypeName_ElementStyle, elementStylesDict);
            }

            if (batchReadState.ContainsKey(TypeName_DimensionLayout))
            {
                dimensionLayoutsDict = (batchReadState[TypeName_DimensionLayout] as Dictionary<ReportId, List<SaveableDimensionLayout>>);
            }
            else
            {
                var rootKeys = rootObjects.Select(x => x.Key).ToList();
                var keysAsSqlMatchableText = IEfEntityUtils.GetKeysAsSqlMatchableText(this, rootKeys);
                var keyMatchingSql = GetSqlCode_ForKeyMatching();

                var dimensionLayoutsSet = context.Set<SaveableDimensionLayout>();
                var dimensionLayoutsQuery = dimensionLayoutsSet.CreateKeyMatchingDbSqlQuery(keysAsSqlMatchableText, keyMatchingSql);
                var dimensionLayoutsSelect = (from dlq in dimensionLayoutsQuery where (Report.Report_ReportElementNumber == dlq.EF_ReportElementNumber) select dlq);
                var dimensionLayouts = dimensionLayoutsSelect.ToList();

                foreach (var dl in dimensionLayouts)
                {
                    var key = new ReportId(dl.EF_ProjectGuid, dl.EF_RevisionNumber, dl.EF_ModelTemplateNumber, dl.EF_ReportGuid);
                    if (!dimensionLayoutsDict.ContainsKey(key))
                    { dimensionLayoutsDict.Add(key, new List<SaveableDimensionLayout>()); }
                    dimensionLayoutsDict[key].Add(dl);
                }

                batchReadState.Add(TypeName_DimensionLayout, dimensionLayoutsDict);
            }
        }

        void IEfAggregate<Report>.AddNestedAggregateValues(DbContext context)
        {
            Expression<Func<ICollection<SaveableDimensionLayout>>> sdlGetterExpression = () => this.EF_DimensionLayouts;
            EfAggregateUtilities.AddNestedValues<SaveableDimensionLayout>(context, sdlGetterExpression);

            Expression<Func<ICollection<SaveableElementStyle>>> sesGetterExpression = () => this.EF_ElementStyles;
            EfAggregateUtilities.AddNestedValues<SaveableElementStyle>(context, sesGetterExpression);
        }

        void IEfAggregate<Report>.UpdateNestedAggregateValues(DbContext context, Report originalAggregate)
        {
            Expression<Func<SaveableDimensionLayout, Dimension>> sdlKeyGetterExpression = ((SaveableDimensionLayout nestedObj) => nestedObj.Dimension);
            Expression<Func<SaveableDimensionLayout, SaveableDimensionLayout, bool>> sdlMatchExpression = ((SaveableDimensionLayout obj1, SaveableDimensionLayout obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_ModelTemplateNumber == obj2.EF_ModelTemplateNumber) && (obj1.EF_ReportGuid == obj2.EF_ReportGuid) && (obj1.EF_ReportElementNumber == obj2.EF_ReportElementNumber) && (obj1.EF_Dimension == obj2.EF_Dimension)));
            Expression<Func<Report, IDictionary<Dimension, SaveableDimensionLayout>>> sdlDictionaryGetterExpression = (Report agg) => agg.EF_DimensionLayouts.ToDictionary(val => val.Dimension, val => val);
            EfAggregateUtilities.UpdateNestedValues<Report, Dimension, SaveableDimensionLayout>(context, this, originalAggregate, sdlKeyGetterExpression, sdlMatchExpression, sdlDictionaryGetterExpression);

            Expression<Func<SaveableElementStyle, int>> sesKeyGetterExpression = ((SaveableElementStyle nestedObj) => nestedObj.Key.ReportElementNumber);
            Expression<Func<SaveableElementStyle, SaveableElementStyle, bool>> sesMatchExpression = ((SaveableElementStyle obj1, SaveableElementStyle obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_ModelTemplateNumber == obj2.EF_ModelTemplateNumber) && (obj1.EF_ReportGuid == obj2.EF_ReportGuid) && (obj1.EF_ReportElementNumber == obj2.EF_ReportElementNumber)));
            Expression<Func<Report, IDictionary<int, SaveableElementStyle>>> sesDictionaryGetterExpression = (Report agg) => agg.EF_ElementStyles.ToDictionary(val => val.Key.ReportElementNumber, val => val);
            EfAggregateUtilities.UpdateNestedValues<Report, int, SaveableElementStyle>(context, this, originalAggregate, sesKeyGetterExpression, sesMatchExpression, sesDictionaryGetterExpression);
        }

        void IEfAggregate<Report>.DeleteNestedAggregateValues(DbContext context)
        {
            Expression<Func<SaveableDimensionLayout, bool>> sdlDeleteQueryExpression = ((SaveableDimensionLayout sdl) => ((sdl.EF_ProjectGuid == Key.ProjectGuid) && (sdl.EF_RevisionNumber == Key.RevisionNumber) && (sdl.EF_ModelTemplateNumber == Key.ModelTemplateNumber) && (sdl.EF_ReportGuid == Key.ReportGuid) && (sdl.EF_ReportElementNumber == ReportAreaTemplateId.ReportElementNumber)));
            EfAggregateUtilities.DeleteNestedValues(context, sdlDeleteQueryExpression);

            Expression<Func<SaveableElementStyle, bool>> sesDeleteQueryExpression = ((SaveableElementStyle ses) => ((ses.EF_ProjectGuid == Key.ProjectGuid) && (ses.EF_RevisionNumber == Key.RevisionNumber) && (ses.EF_ModelTemplateNumber == Key.ModelTemplateNumber) && (ses.EF_ReportGuid == Key.ReportGuid) && ((ses.EF_ReportElementNumber == ReportAreaTemplateId.ReportElementNumber) || (ses.EF_ReportElementNumber == OutsideAreaTemplateId.ReportElementNumber) || (ses.EF_ReportElementNumber == TitleTemplateId.ReportElementNumber) || (ses.EF_ReportElementNumber == HeaderTemplateId.ReportElementNumber) || (ses.EF_ReportElementNumber == DataTemplateId.ReportElementNumber))));
            EfAggregateUtilities.DeleteNestedValues(context, sesDeleteQueryExpression);
        }

        void IEfAggregate_Detachable<Report>.DetachNestedAggregateValues(DbContext context)
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