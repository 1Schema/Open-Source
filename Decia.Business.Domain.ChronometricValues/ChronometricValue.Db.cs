using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.Testing;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues.TimeAssessments;
using Decia.Business.Domain.ChronometricValues.TimeDimensions;

namespace Decia.Business.Domain.ChronometricValues
{
    public partial class ChronometricValue : IEfEntity_DbQueryable<ChronometricValueId, string>, IEfAggregate_Detachable<ChronometricValue>, IPartialReadable
    {
        #region Entity Framework Mapper

        public class ChronometricValue_Mapper : EntityTypeConfiguration<ChronometricValue>
        {
            public ChronometricValue_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_ChronometricValueGuid);
                Property(p => p.EF_DataType);
                Property(p => p.EF_DefaultNumberValue);
                Property(p => p.EF_DefaultTextValue);
                Property(p => p.EF_ParentModelObjectType);
                Property(p => p.EF_ParentModelObjectRefs);
                Property(p => p.EF_CreatorGuid);
                Property(p => p.EF_CreationDate);
                Property(p => p.EF_OwnerType);
                Property(p => p.EF_OwnerGuid);
                Property(p => p.EF_IsDeleted);
                Property(p => p.EF_DeleterGuid);
                Property(p => p.EF_DeletionDate);
            }
        }

        #endregion

        #region IEfEntity_DbQueryable<ChronometricValueId, string> Implementation

        public static string KeyMatchingSql { get; protected set; }

        [NotMapped]
        string IEfEntity<string>.EF_Id
        {
            get { return m_Key.ToString(); }
            set { /* do nothing */ }
        }

        string IEfEntity_DbQueryable<ChronometricValueId>.ConvertKey_ToMatchableText(ChronometricValueId key)
        {
            return ConvertKey_ToMatchableText(key);
        }

        protected static string ConvertKey_ToMatchableText(ChronometricValueId key)
        {
            var values = new object[] { key.ProjectGuid, key.RevisionNumber_NonNull, key.ChronometricValueGuid };
            var valuesAsText = AdoNetUtils.CovertToKeyMatchingText(values, false);
            return valuesAsText;
        }

        string IEfEntity_DbQueryable<ChronometricValueId>.GetSqlCode_ForKeyMatching()
        {
            return GetSqlCode_ForKeyMatching();
        }

        protected static string GetSqlCode_ForKeyMatching()
        {
            if (!string.IsNullOrWhiteSpace(KeyMatchingSql))
            { return KeyMatchingSql; }

            var projectGuidName = ClassReflector.GetPropertyName((ChronometricValue cv) => cv.EF_ProjectGuid);
            var revisionNumberName = ClassReflector.GetPropertyName((ChronometricValue cv) => cv.EF_RevisionNumber);
            var chronometricValueGuidName = ClassReflector.GetPropertyName((ChronometricValue cv) => cv.EF_ChronometricValueGuid);

            var thisType = typeof(ChronometricValue);
            var projectGuidType = ClassReflector.GetPropertyByName(thisType, projectGuidName).PropertyType;
            var revisionNumberType = ClassReflector.GetPropertyByName(thisType, revisionNumberName).PropertyType;
            var chronometricValueGuidType = ClassReflector.GetPropertyByName(thisType, chronometricValueGuidName).PropertyType;

            var projectGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(projectGuidType, projectGuidName);
            var revisionNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(revisionNumberType, revisionNumberName);
            var chronometricValueGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(chronometricValueGuidType, chronometricValueGuidName);

            var values = new object[] { projectGuidText, revisionNumberText, chronometricValueGuidText };
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
            set { m_Key = new ChronometricValueId(value, m_Key.RevisionNumber_NonNull, m_Key.ChronometricValueGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_Key.RevisionNumber_NonNull; }
            set { m_Key = new ChronometricValueId(m_Key.ProjectGuid, value, m_Key.ChronometricValueGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public Guid EF_ChronometricValueGuid
        {
            get { return m_Key.ChronometricValueGuid; }
            set { m_Key = new ChronometricValueId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, value); }
        }

        [ForceMapped]
        internal int EF_DataType
        {
            get { return (int)m_DataType; }
            set { m_DataType = (DeciaDataType)value; }
        }

        private Nullable<double> m_EF_DefaultNumberValue = null;
        [ForceMapped]
        internal Nullable<double> EF_DefaultNumberValue
        {
            get { return m_DefaultValue.ValueAsNumber; }
            set
            {
                m_EF_DefaultNumberValue = value;
                InitializeDynamicValue();
            }
        }

        private string m_EF_DefaultTextValue = null;
        [ForceMapped]
        internal string EF_DefaultTextValue
        {
            get { return m_DefaultValue.ValueAsString; }
            set
            {
                m_EF_DefaultTextValue = value;
                InitializeDynamicValue();
            }
        }

        private void InitializeDynamicValue()
        {
            m_DefaultValue.LoadFromStorage(InternalDataTypeGetter, m_EF_DefaultTextValue, m_EF_DefaultNumberValue);
        }

        #region IParentIdProvider Database Persistence Properties

        [ForceMapped]
        internal Nullable<int> EF_ParentModelObjectType
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_ParentModelObjectType);
                return m_ParentModelObjectType.GetAsInt();
            }
            set
            {
                m_ParentModelObjectType = value.GetAsEnum<ModelObjectType>();
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_ParentModelObjectType);
            }
        }

        [ForceMapped]
        internal string EF_ParentModelObjectRefs
        {
            get
            {
                if (!HasParent)
                { return string.Empty; }

                var refsAsDict = m_ParentModelObjectRefs.ToDictionary(x => x.Value.ModelObjectType, x => x.Value.ModelObjectId);
                var refsAsString = refsAsDict.ConvertToDictionaryAsString();
                return refsAsString;
            }
            set
            {
                if (!HasParent)
                { m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>(); }
                else
                {
                    var refsAsDict = value.ConvertToTypedDictionary<ModelObjectType, Guid>();
                    var refsAsTypedDict = refsAsDict.ToDictionary(x => x.Key, x => new ModelObjectReference(x.Key, x.Value));
                    m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>(refsAsTypedDict);
                }
            }
        }

        #endregion

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

        #region IPartialReadable State Properties

        [ThreadStatic]
        private static MultiTimePeriodKey? m_TimeBindings_ForPartialRead;

        public static MultiTimePeriodKey? TimeBindings_ForPartialRead
        {
            get { return m_TimeBindings_ForPartialRead; }
            set { m_TimeBindings_ForPartialRead = value; }
        }

        private bool m_IsPartialObject = false;

        [NotMapped]
        public bool IsPartialObject
        {
            get { return m_IsPartialObject; }
            protected internal set { m_IsPartialObject = value; }
        }

        #endregion

        #region ChronometricValue Aggregate Persistence Properties and Methods

        [NotMapped]
        internal ICollection<TimeAssessment> EF_TimeAssessments
        {
            get
            {
                List<TimeAssessment> timeAssessments = new List<TimeAssessment>();
                foreach (MultiTimePeriodKey timeAssessmentKey in m_TimeAssessments.Keys)
                {
                    timeAssessments.Add(m_TimeAssessments[timeAssessmentKey]);
                }
                return timeAssessments;
            }
            set
            {
                m_TimeAssessments.Clear();
                foreach (TimeAssessment timeAssessment in value)
                {
                    timeAssessment.DataTypeGetter = this.InternalDataTypeGetter;
                    m_TimeAssessments.Add(timeAssessment.Key, timeAssessment);
                }
            }
        }

        [NotMapped]
        internal ICollection<SaveableTimeDimension> EF_TimeDimensions
        {
            get
            {
                List<SaveableTimeDimension> timeDimensions = new List<SaveableTimeDimension>();

                if (m_PrimaryTimeDimension.HasTimeValue)
                { timeDimensions.Add(m_PrimaryTimeDimension); }
                if (m_SecondaryTimeDimension.HasTimeValue)
                { timeDimensions.Add(m_SecondaryTimeDimension); }

                return timeDimensions;
            }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<ChronometricValue>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<ChronometricValue>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<ChronometricValue> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        internal static readonly string TypeName_ChronometricValue = typeof(ChronometricValue).Name;
        internal static readonly string TypeName_TimeAssessment = typeof(TimeAssessment).Name;
        internal static readonly string TypeName_SaveableTimeDimension = typeof(SaveableTimeDimension).Name;

        void IEfAggregate<ChronometricValue>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            var timeAssessmentsDict = new Dictionary<ChronometricValueId, List<TimeAssessment>>();
            var timeDimensionsDict = new Dictionary<ChronometricValueId, List<SaveableTimeDimension>>();

            PerformBatchRead(context, batchReadState, ref timeAssessmentsDict, ref timeDimensionsDict);

            var timeAssessmentsList = (timeAssessmentsDict.ContainsKey(this.Key)) ? timeAssessmentsDict[this.Key] : new List<TimeAssessment>();
            this.EF_TimeAssessments = timeAssessmentsList;

            var timeDimensionsList = (timeDimensionsDict.ContainsKey(this.Key)) ? timeDimensionsDict[this.Key] : new List<SaveableTimeDimension>();
            var timeDimensionsByType = timeDimensionsList.ToDictionary(td => td.Key, td => td);

            InitializeTimeDimension(timeDimensionsByType, ref m_PrimaryTimeDimension);
            InitializeTimeDimension(timeDimensionsByType, ref m_SecondaryTimeDimension);

            this.IsPartialObject = false;
            if (TimeBindings_ForPartialRead.HasValue)
            {
                var isPartial_Primary = (TimeBindings_ForPartialRead.Value.HasPrimaryTimePeriod && this.PrimaryTimeDimension.HasTimeValue);
                var isPartial_Secondary = (TimeBindings_ForPartialRead.Value.HasSecondaryTimePeriod && this.SecondaryTimeDimension.HasTimeValue);
                this.IsPartialObject = (isPartial_Primary || isPartial_Secondary);
            }

            if (this.IsPartialObject && (this.EF_TimeAssessments.Count > 0))
            {
                if (timeAssessmentsList.Where(x => x.IsPartialObject).Count() <= 0)
                { throw new InvalidOperationException("The object should have read partial TimeAssessments."); }
            }

            m_ImpliedTimeKeys = ChronometricValueUtils.GetImpliedTimeKeys(this);
        }

        private void PerformBatchRead(DbContext context, Dictionary<string, object> batchReadState, ref Dictionary<ChronometricValueId, List<TimeAssessment>> timeAssessmentsDict, ref Dictionary<ChronometricValueId, List<SaveableTimeDimension>> timeDimensionsDict)
        {
            var rootObjects = (batchReadState[TypeName_ChronometricValue] as IEnumerable<ChronometricValue>);

            if (batchReadState.ContainsKey(TypeName_SaveableTimeDimension))
            {
                timeDimensionsDict = (batchReadState[TypeName_SaveableTimeDimension] as Dictionary<ChronometricValueId, List<SaveableTimeDimension>>);
            }
            else
            {
                var rootKeys = rootObjects.Select(x => x.Key).ToList();
                var keysAsSqlMatchableText = IEfEntityUtils.GetKeysAsSqlMatchableText(this, rootKeys);
                var keyMatchingSql = GetSqlCode_ForKeyMatching();

                var timeDimensionsSet = context.Set<SaveableTimeDimension>();
                var timeDimensionsQuery = timeDimensionsSet.CreateKeyMatchingDbSqlQuery(keysAsSqlMatchableText, keyMatchingSql);
                var timeDimensions = timeDimensionsQuery.ToList();

                foreach (var td in timeDimensions)
                {
                    var key = new ChronometricValueId(td.EF_ProjectGuid, td.EF_RevisionNumber, td.EF_ChronometricValueGuid);
                    if (!timeDimensionsDict.ContainsKey(key))
                    { timeDimensionsDict.Add(key, new List<SaveableTimeDimension>()); }
                    timeDimensionsDict[key].Add(td);
                }

                batchReadState.Add(TypeName_SaveableTimeDimension, timeDimensionsDict);
            }

            if (batchReadState.ContainsKey(TypeName_TimeAssessment))
            {
                timeAssessmentsDict = (batchReadState[TypeName_TimeAssessment] as Dictionary<ChronometricValueId, List<TimeAssessment>>);
            }
            else
            {
                var timeBinding_HasValue = ChronometricValue.TimeBindings_ForPartialRead.HasValue;
                var timeBinding_PrimaryPeriod = timeBinding_HasValue ? TimeBindings_ForPartialRead.Value.NullablePrimaryTimePeriod : (TimePeriod?)null;
                var timeBinding_SecondaryPeriod = timeBinding_HasValue ? TimeBindings_ForPartialRead.Value.NullableSecondaryTimePeriod : (TimePeriod?)null;

                var timeBinding_HasPrimaryDates = timeBinding_PrimaryPeriod.HasValue;
                var timeBinding_PrimaryStartDate = timeBinding_PrimaryPeriod.HasValue ? timeBinding_PrimaryPeriod.Value.StartDate : (DateTime?)null;
                var timeBinding_PrimaryEndDate = timeBinding_PrimaryPeriod.HasValue ? timeBinding_PrimaryPeriod.Value.EndDate : (DateTime?)null;
                var timeBinding_HasSecondaryDates = timeBinding_SecondaryPeriod.HasValue;
                var timeBinding_SecondaryStartDate = timeBinding_SecondaryPeriod.HasValue ? timeBinding_SecondaryPeriod.Value.StartDate : (DateTime?)null;
                var timeBinding_SecondaryEndDate = timeBinding_SecondaryPeriod.HasValue ? timeBinding_SecondaryPeriod.Value.EndDate : (DateTime?)null;

                var rootKeys = rootObjects.Select(x => x.Key).ToList();
                var keysAsSqlMatchableText = IEfEntityUtils.GetKeysAsSqlMatchableText(this, rootKeys);
                var valueSeparator = AdoNetUtils.KeyMatching_Separator_Text;

                var timeAssessmentSet = context.Set<TimeAssessment>();
                var timeAssessments = (from ta in timeAssessmentSet
                                       where keysAsSqlMatchableText.Contains(ta.EF_ProjectGuid.ToString().ToLower() + valueSeparator + ta.EF_RevisionNumber.ToString().ToLower() + valueSeparator + ta.EF_ChronometricValueGuid.ToString().ToLower()) &&
                                             (!timeBinding_HasPrimaryDates ||
                                                (timeBinding_HasPrimaryDates && !ta.EF_HasPrimaryTimeDimension) ||
                                                (timeBinding_HasPrimaryDates && ta.EF_HasPrimaryTimeDimension && !((ta.EF_PrimaryEndDate < timeBinding_PrimaryStartDate) || (ta.EF_PrimaryStartDate > timeBinding_PrimaryEndDate)))) &&
                                             (!timeBinding_HasSecondaryDates ||
                                                (timeBinding_HasSecondaryDates && !ta.EF_HasSecondaryTimeDimension) ||
                                                (timeBinding_HasSecondaryDates && ta.EF_HasSecondaryTimeDimension && !((ta.EF_SecondaryEndDate < timeBinding_SecondaryStartDate) || (ta.EF_SecondaryStartDate > timeBinding_SecondaryEndDate))))
                                       select ta).ToList();
                timeAssessments.ForEach(x => x.IsPartialObject = ChronometricValueRepositoryExtensions.GetIsPartialRead(timeBinding_HasValue, timeBinding_HasPrimaryDates, timeBinding_HasSecondaryDates, x));

                foreach (var ta in timeAssessments)
                {
                    var key = new ChronometricValueId(ta.EF_ProjectGuid, ta.EF_RevisionNumber, ta.EF_ChronometricValueGuid);
                    if (!timeAssessmentsDict.ContainsKey(key))
                    { timeAssessmentsDict.Add(key, new List<TimeAssessment>()); }
                    timeAssessmentsDict[key].Add(ta);
                }

                batchReadState.Add(TypeName_TimeAssessment, timeAssessmentsDict);
            }
        }

        private void InitializeTimeDimension(IDictionary<TimeDimensionType, SaveableTimeDimension> timeDimensionsDict, ref SaveableTimeDimension timeDimension)
        {
            if (timeDimensionsDict.ContainsKey(timeDimension.TimeDimensionType))
            { timeDimension = timeDimensionsDict[timeDimension.TimeDimensionType]; }
            else
            { timeDimension = new SaveableTimeDimension(m_Key, timeDimension.TimeDimensionType.CreateEmptyTimeDimension()); }
        }

        void IEfAggregate<ChronometricValue>.AddNestedAggregateValues(DbContext context)
        {
            Expression<Func<ICollection<TimeAssessment>>> timeAssessmentsGetterExpression = () => this.EF_TimeAssessments;
            EfAggregateUtilities.AddNestedValues<TimeAssessment>(context, timeAssessmentsGetterExpression);

            Expression<Func<ICollection<SaveableTimeDimension>>> timeDimensionsGetterExpression = () => this.EF_TimeDimensions;
            EfAggregateUtilities.AddNestedValues<SaveableTimeDimension>(context, timeDimensionsGetterExpression);
        }

        void IEfAggregate<ChronometricValue>.UpdateNestedAggregateValues(DbContext context, ChronometricValue originalAggregate)
        {
            Expression<Func<TimeAssessment, MultiTimePeriodKey>> timeAssessmentsKeyGetterExpression = ((TimeAssessment nestedObj) => nestedObj.Key);
            Expression<Func<TimeAssessment, TimeAssessment, bool>> timeAssessmentsMatchExpression = ((TimeAssessment obj1, TimeAssessment obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_ChronometricValueGuid == obj2.EF_ChronometricValueGuid) && (obj1.EF_PrimaryStartDate == obj2.EF_PrimaryStartDate) && (obj1.EF_PrimaryEndDate == obj2.EF_PrimaryEndDate) && (obj1.EF_SecondaryStartDate == obj2.EF_SecondaryStartDate) && (obj1.EF_SecondaryEndDate == obj2.EF_SecondaryEndDate)));
            Expression<Func<ChronometricValue, IDictionary<MultiTimePeriodKey, TimeAssessment>>> timeAssessmentsDictionaryGetterExpression = (ChronometricValue agg) => agg.EF_TimeAssessments.ToDictionary(val => val.Key, val => val);
            EfAggregateUtilities.UpdateNestedValues<ChronometricValue, MultiTimePeriodKey, TimeAssessment>(context, this, originalAggregate, timeAssessmentsKeyGetterExpression, timeAssessmentsMatchExpression, timeAssessmentsDictionaryGetterExpression);

            Expression<Func<SaveableTimeDimension, TimeDimensionType>> timeDimensionsKeyGetterExpression = ((SaveableTimeDimension nestedObj) => nestedObj.Key);
            Expression<Func<SaveableTimeDimension, SaveableTimeDimension, bool>> timeDimensionsMatchExpression = ((SaveableTimeDimension obj1, SaveableTimeDimension obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_ChronometricValueGuid == obj2.EF_ChronometricValueGuid) && (obj1.EF_TimeDimensionType == obj2.EF_TimeDimensionType)));
            Expression<Func<ChronometricValue, IDictionary<TimeDimensionType, SaveableTimeDimension>>> timeDimensionsDictionaryGetterExpression = (ChronometricValue agg) => agg.EF_TimeDimensions.ToDictionary(val => val.Key, val => val);
            EfAggregateUtilities.UpdateNestedValues<ChronometricValue, TimeDimensionType, SaveableTimeDimension>(context, this, originalAggregate, timeDimensionsKeyGetterExpression, timeDimensionsMatchExpression, timeDimensionsDictionaryGetterExpression);
        }

        void IEfAggregate<ChronometricValue>.DeleteNestedAggregateValues(DbContext context)
        {
            Expression<Func<TimeAssessment, bool>> timeAssessmentsDeleteQueryExpression = ((TimeAssessment ta) => ((ta.EF_ProjectGuid == Key.ProjectGuid) && (ta.EF_RevisionNumber == Key.RevisionNumber) && (ta.EF_ChronometricValueGuid == Key.ChronometricValueGuid)));
            EfAggregateUtilities.DeleteNestedValues(context, timeAssessmentsDeleteQueryExpression);

            Expression<Func<SaveableTimeDimension, bool>> timeDimensionsDeleteQueryExpression = ((SaveableTimeDimension td) => ((td.EF_ProjectGuid == Key.ProjectGuid) && (td.EF_RevisionNumber == Key.RevisionNumber) && (td.EF_ChronometricValueGuid == Key.ChronometricValueGuid)));
            EfAggregateUtilities.DeleteNestedValues(context, timeDimensionsDeleteQueryExpression);
        }

        void IEfAggregate_Detachable<ChronometricValue>.DetachNestedAggregateValues(DbContext context)
        {
            foreach (var timeDimension in EF_TimeDimensions)
            {
                context.Entry(timeDimension).State = EntityState.Detached;
            }

            foreach (var timeAssessment in EF_TimeAssessments)
            {
                context.Entry(timeAssessment).State = EntityState.Detached;
            }
        }

        #endregion

        #region DataSet Deep Copy

        public static readonly ChronometricValueDataSet DataSetSchema = new ChronometricValueDataSet();

        public static void ChronometricValue_DeepCopy(ChronometricValue chronometricValue, DataTable dataTable, RevisionChain revisionChain)
        {
            var timeDimensions = new SaveableTimeDimension[] { chronometricValue.m_PrimaryTimeDimension, chronometricValue.m_SecondaryTimeDimension };

            var timeDimensionsDataTable = dataTable.DataSet.Tables[DataSetSchema.SaveableTimeDimensions.TableName];
            var timeAssessmentsDataTable = dataTable.DataSet.Tables[DataSetSchema.TimeAssessments.TableName];

            foreach (var timeDimension in timeDimensions)
            {
                var dataRow = timeDimensionsDataTable.NewRow();
                timeDimension.CopyObjectToDataRow(dataRow, revisionChain.DesiredRevisionNumber);
                timeDimensionsDataTable.Rows.Add(dataRow);
                dataRow.AcceptChanges();
                dataRow.SetAdded();
            }

            foreach (var timeAssessment in chronometricValue.m_TimeAssessments.Values)
            {
                var dataRow = timeAssessmentsDataTable.NewRow();
                timeAssessment.CopyObjectToDataRow(dataRow, revisionChain.DesiredRevisionNumber);
                timeAssessmentsDataTable.Rows.Add(dataRow);
                dataRow.AcceptChanges();
                dataRow.SetAdded();
            }
        }

        #endregion
    }
}