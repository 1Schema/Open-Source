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
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.CompoundUnits.BaseUnitValues;

namespace Decia.Business.Domain.CompoundUnits
{
    public partial class CompoundUnit : IEfEntity_DbQueryable<CompoundUnitId, string>, IEfAggregate_Detachable<CompoundUnit>
    {
        #region Entity Framework Mapper

        public class CompoundUnit_Mapper : EntityTypeConfiguration<CompoundUnit>
        {
            public CompoundUnit_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_CompoundUnitGuid);
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

        #region IEfEntity_DbQueryable<CompoundUnitId, string> Implementation

        public static string KeyMatchingSql { get; protected set; }

        [NotMapped]
        string IEfEntity<string>.EF_Id
        {
            get { return m_Key.ToString(); }
            set { /* do nothing */ }
        }

        string IEfEntity_DbQueryable<CompoundUnitId>.ConvertKey_ToMatchableText(CompoundUnitId key)
        {
            return ConvertKey_ToMatchableText(key);
        }

        protected static string ConvertKey_ToMatchableText(CompoundUnitId key)
        {
            var values = new object[] { key.ProjectGuid, key.RevisionNumber_NonNull, key.CompoundUnitGuid };
            var valuesAsText = AdoNetUtils.CovertToKeyMatchingText(values, false);
            return valuesAsText;
        }

        string IEfEntity_DbQueryable<CompoundUnitId>.GetSqlCode_ForKeyMatching()
        {
            return GetSqlCode_ForKeyMatching();
        }

        protected static string GetSqlCode_ForKeyMatching()
        {
            if (!string.IsNullOrWhiteSpace(KeyMatchingSql))
            { return KeyMatchingSql; }

            var projectGuidName = ClassReflector.GetPropertyName((CompoundUnit cu) => cu.EF_ProjectGuid);
            var revisionNumberName = ClassReflector.GetPropertyName((CompoundUnit cu) => cu.EF_RevisionNumber);
            var compoundUnitGuidName = ClassReflector.GetPropertyName((CompoundUnit cu) => cu.EF_CompoundUnitGuid);

            var thisType = typeof(CompoundUnit);
            var projectGuidType = ClassReflector.GetPropertyByName(thisType, projectGuidName).PropertyType;
            var revisionNumberType = ClassReflector.GetPropertyByName(thisType, revisionNumberName).PropertyType;
            var compoundUnitGuidType = ClassReflector.GetPropertyByName(thisType, compoundUnitGuidName).PropertyType;

            var projectGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(projectGuidType, projectGuidName);
            var revisionNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(revisionNumberType, revisionNumberName);
            var compoundUnitGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(compoundUnitGuidType, compoundUnitGuidName);

            var values = new object[] { projectGuidText, revisionNumberText, compoundUnitGuidText };
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
            set { m_Key = new CompoundUnitId(value, m_Key.RevisionNumber_NonNull, m_Key.CompoundUnitGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_Key.RevisionNumber_NonNull; }
            set { m_Key = new CompoundUnitId(m_Key.ProjectGuid, value, m_Key.CompoundUnitGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public Guid EF_CompoundUnitGuid
        {
            get { return m_Key.CompoundUnitGuid; }
            set { m_Key = new CompoundUnitId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, value); }
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

        #region CompoundUnit Aggregate Persistence Properties and Methods

        [NotMapped]
        internal ICollection<BaseUnitExponentiationValue> EF_BaseUnitExponentiationValues
        {
            get
            {
                List<BaseUnitExponentiationValue> exponentiationValues = new List<BaseUnitExponentiationValue>();
                foreach (int baseUnitTypeNumber in m_BaseUnitExponentiationValues.Keys)
                {
                    exponentiationValues.Add(m_BaseUnitExponentiationValues[baseUnitTypeNumber]);
                }
                return exponentiationValues;
            }
            set
            {
                m_BaseUnitExponentiationValues.Clear();
                foreach (BaseUnitExponentiationValue exponentiationValue in value)
                {
                    m_BaseUnitExponentiationValues.Add(exponentiationValue.BaseUnitTypeNumber, exponentiationValue);
                }
            }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<CompoundUnit>.OverriddenQueries
        {
            get
            {
                List<QuerySpecification> overriddenQueries = new List<QuerySpecification>();
                return overriddenQueries;
            }
        }

        bool IEfAggregate<CompoundUnit>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<CompoundUnit> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        internal static readonly string TypeName_CompoundUnit = typeof(CompoundUnit).Name;
        internal static readonly string TypeName_BaseUnitExponentiationValue = typeof(BaseUnitExponentiationValue).Name;

        void IEfAggregate<CompoundUnit>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            var exponentiationValuesDict = new Dictionary<CompoundUnitId, List<BaseUnitExponentiationValue>>();

            PerformBatchRead(context, batchReadState, ref exponentiationValuesDict);

            var exponentiationValuesList = (exponentiationValuesDict.ContainsKey(this.Key)) ? exponentiationValuesDict[this.Key] : new List<BaseUnitExponentiationValue>();
            this.EF_BaseUnitExponentiationValues = exponentiationValuesList;
        }

        private void PerformBatchRead(DbContext context, Dictionary<string, object> batchReadState, ref Dictionary<CompoundUnitId, List<BaseUnitExponentiationValue>> exponentiationValuesDict)
        {
            var rootObjects = (batchReadState[TypeName_CompoundUnit] as IEnumerable<CompoundUnit>);

            if (batchReadState.ContainsKey(TypeName_BaseUnitExponentiationValue))
            {
                exponentiationValuesDict = (batchReadState[TypeName_BaseUnitExponentiationValue] as Dictionary<CompoundUnitId, List<BaseUnitExponentiationValue>>);
            }
            else
            {
                var rootKeys = rootObjects.Select(x => x.Key).ToList();
                var keysAsSqlMatchableText = IEfEntityUtils.GetKeysAsSqlMatchableText(this, rootKeys);
                var keyMatchingSql = GetSqlCode_ForKeyMatching();

                var exponentiationValuesSet = context.Set<BaseUnitExponentiationValue>();
                var exponentiationValuesQuery = exponentiationValuesSet.CreateKeyMatchingDbSqlQuery(keysAsSqlMatchableText, keyMatchingSql);
                var exponentiationValues = exponentiationValuesQuery.ToList();

                foreach (var ev in exponentiationValues)
                {
                    var key = new CompoundUnitId(ev.EF_ProjectGuid, ev.EF_RevisionNumber, ev.EF_CompoundUnitGuid);
                    if (!exponentiationValuesDict.ContainsKey(key))
                    { exponentiationValuesDict.Add(key, new List<BaseUnitExponentiationValue>()); }
                    exponentiationValuesDict[key].Add(ev);
                }

                batchReadState.Add(TypeName_BaseUnitExponentiationValue, exponentiationValuesDict);
            }
        }

        void IEfAggregate<CompoundUnit>.AddNestedAggregateValues(DbContext context)
        {
            Expression<Func<ICollection<BaseUnitExponentiationValue>>> exponentiationValuesGetterExpression = () => this.EF_BaseUnitExponentiationValues;
            EfAggregateUtilities.AddNestedValues<BaseUnitExponentiationValue>(context, exponentiationValuesGetterExpression);
        }

        void IEfAggregate<CompoundUnit>.UpdateNestedAggregateValues(DbContext context, CompoundUnit originalAggregateRoot)
        {
            Expression<Func<BaseUnitExponentiationValue, int>> exponentiationValuesKeyGetterExpression = ((BaseUnitExponentiationValue nestedObj) => nestedObj.BaseUnitTypeNumber);
            Expression<Func<BaseUnitExponentiationValue, BaseUnitExponentiationValue, bool>> exponentiationValuesMatchExpression = ((BaseUnitExponentiationValue obj1, BaseUnitExponentiationValue obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_CompoundUnitGuid == obj2.EF_CompoundUnitGuid) && (obj1.EF_BaseUnitTypeNumber == obj2.EF_BaseUnitTypeNumber)));
            Expression<Func<CompoundUnit, IDictionary<int, BaseUnitExponentiationValue>>> exponentiationValuesDictionaryGetterExpression = (CompoundUnit agg) => agg.EF_BaseUnitExponentiationValues.ToDictionary(val => val.BaseUnitTypeNumber, val => val);
            EfAggregateUtilities.UpdateNestedValues<CompoundUnit, int, BaseUnitExponentiationValue>(context, this, originalAggregateRoot, exponentiationValuesKeyGetterExpression, exponentiationValuesMatchExpression, exponentiationValuesDictionaryGetterExpression);
        }

        void IEfAggregate<CompoundUnit>.DeleteNestedAggregateValues(DbContext context)
        {
            Expression<Func<BaseUnitExponentiationValue, bool>> exponentiationValuesDeleteQueryExpression = ((BaseUnitExponentiationValue buev) => ((buev.EF_ProjectGuid == Key.ProjectGuid) && (buev.EF_RevisionNumber == Key.RevisionNumber) && (buev.EF_CompoundUnitGuid == Key.CompoundUnitGuid)));
            EfAggregateUtilities.DeleteNestedValues(context, exponentiationValuesDeleteQueryExpression);
        }

        void IEfAggregate_Detachable<CompoundUnit>.DetachNestedAggregateValues(DbContext context)
        {
            foreach (var baseUnitExponentiationValue in EF_BaseUnitExponentiationValues)
            {
                context.Entry(baseUnitExponentiationValue).State = EntityState.Detached;
            }
        }

        #endregion

        #region DataSet Deep Copy

        public static readonly CompoundUnitDataSet DataSetSchema = new CompoundUnitDataSet();

        public static void Formula_DeepCopy(CompoundUnit compoundUnit, DataTable dataTable, RevisionChain revisionChain)
        {
            var exponentiationsDataTable = dataTable.DataSet.Tables[DataSetSchema.BaseUnitExponentiationValues.TableName];

            foreach (var exponentiation in compoundUnit.BaseUnitActualExponentiations.Values)
            {
                var dataRow = exponentiationsDataTable.NewRow();
                exponentiation.CopyObjectToDataRow(dataRow, revisionChain.DesiredRevisionNumber);
                exponentiationsDataTable.Rows.Add(dataRow);
                dataRow.AcceptChanges();
                dataRow.SetAdded();
            }
        }

        #endregion
    }
}