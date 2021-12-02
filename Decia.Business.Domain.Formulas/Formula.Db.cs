using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
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
using Decia.Business.Domain.Formulas.Expressions;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula : IEfEntity_DbQueryable<FormulaId, string>, IEfAggregate_Detachable<Formula>
    {
        #region Entity Framework Mapper

        public class Formula_Mapper : EntityTypeConfiguration<Formula>
        {
            public Formula_Mapper()
            {
                Property(p => p.EF_ProjectGuid);
                Property(p => p.EF_RevisionNumber);
                Property(p => p.EF_FormulaGuid);
                Property(p => p.EF_ModelObjectType);
                Property(p => p.EF_ModelObjectIdAsInt);
                Property(p => p.EF_ModelObjectIdAsGuid);
                Property(p => p.EF_IsNavigationVariable);
                Property(p => p.EF_IsStructuralAggregation);
                Property(p => p.EF_IsStructuralFilter);
                Property(p => p.EF_IsTimeAggregation);
                Property(p => p.EF_IsTimeFilter);
                Property(p => p.EF_IsTimeShift);
                Property(p => p.EF_IsTimeIntrospection);
                Property(p => p.EF_RootExpressionGuid);
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

        #region IEfEntity_DbQueryable<FormulaId, string> Implementation

        public static string KeyMatchingSql { get; protected set; }

        [NotMapped]
        string IEfEntity<string>.EF_Id
        {
            get { return m_Key.ToString(); }
            set { /* do nothing */ }
        }

        string IEfEntity_DbQueryable<FormulaId>.ConvertKey_ToMatchableText(FormulaId key)
        {
            return ConvertKey_ToMatchableText(key);
        }

        protected static string ConvertKey_ToMatchableText(FormulaId key)
        {
            var values = new object[] { key.ProjectGuid, key.RevisionNumber_NonNull, key.FormulaGuid };
            var valuesAsText = AdoNetUtils.CovertToKeyMatchingText(values, false);
            return valuesAsText;
        }

        string IEfEntity_DbQueryable<FormulaId>.GetSqlCode_ForKeyMatching()
        {
            return GetSqlCode_ForKeyMatching();
        }

        protected static string GetSqlCode_ForKeyMatching()
        {
            if (!string.IsNullOrWhiteSpace(KeyMatchingSql))
            { return KeyMatchingSql; }

            var projectGuidName = ClassReflector.GetPropertyName((Formula f) => f.EF_ProjectGuid);
            var revisionNumberName = ClassReflector.GetPropertyName((Formula f) => f.EF_RevisionNumber);
            var formulaGuidName = ClassReflector.GetPropertyName((Formula f) => f.EF_FormulaGuid);

            var thisType = typeof(Formula);
            var projectGuidType = ClassReflector.GetPropertyByName(thisType, projectGuidName).PropertyType;
            var revisionNumberType = ClassReflector.GetPropertyByName(thisType, revisionNumberName).PropertyType;
            var formulaGuidType = ClassReflector.GetPropertyByName(thisType, formulaGuidName).PropertyType;

            var projectGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(projectGuidType, projectGuidName);
            var revisionNumberText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(revisionNumberType, revisionNumberName);
            var formulaGuidText = AdoNetUtils.Decia_DbType.ConvertToSqlTextForColumn(formulaGuidType, formulaGuidName);

            var values = new object[] { projectGuidText, revisionNumberText, formulaGuidText };
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
            set { m_Key = new FormulaId(value, m_Key.RevisionNumber_NonNull, m_Key.FormulaGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public long EF_RevisionNumber
        {
            get { return m_Key.RevisionNumber_NonNull; }
            set { m_Key = new FormulaId(m_Key.ProjectGuid, value, m_Key.FormulaGuid); }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        public Guid EF_FormulaGuid
        {
            get { return m_Key.FormulaGuid; }
            set { m_Key = new FormulaId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, value); }
        }

        private Nullable<int> m_EF_ModelObjectType = null;
        [ForceMapped]
        internal Nullable<int> EF_ModelObjectType
        {
            get { return m_ModelObjectRef.HasValue ? (Nullable<int>)m_ModelObjectRef.Value.ModelObjectType : (Nullable<int>)null; }
            set
            {
                m_EF_ModelObjectType = value;
            }
        }

        private Nullable<int> m_EF_ModelObjectIdAsInt = null;
        [ForceMapped]
        internal Nullable<int> EF_ModelObjectIdAsInt
        {
            get { return (m_ModelObjectRef.HasValue && m_ModelObjectRef.Value.IdIsInt) ? (Nullable<int>)m_ModelObjectRef.Value.ModelObjectIdAsInt : (Nullable<int>)null; }
            set
            {
                m_EF_ModelObjectIdAsInt = value;
                SetReferencedModelObject();
            }
        }

        private Nullable<Guid> m_EF_ModelObjectIdAsGuid = null;
        [ForceMapped]
        internal Nullable<Guid> EF_ModelObjectIdAsGuid
        {
            get { return (m_ModelObjectRef.HasValue) ? (Nullable<Guid>)m_ModelObjectRef.Value.ModelObjectId : (Nullable<Guid>)null; }
            set
            {
                m_EF_ModelObjectIdAsGuid = value;
                SetReferencedModelObject();
            }
        }

        private void SetReferencedModelObject()
        {
            if (!m_EF_ModelObjectType.HasValue || (!m_EF_ModelObjectIdAsInt.HasValue && !m_EF_ModelObjectIdAsGuid.HasValue))
            { m_ModelObjectRef = null; }
            else if (m_EF_ModelObjectIdAsInt.HasValue)
            { m_ModelObjectRef = new ModelObjectReference((ModelObjectType)m_EF_ModelObjectType.Value, m_EF_ModelObjectIdAsInt.Value); }
            else
            { m_ModelObjectRef = new ModelObjectReference((ModelObjectType)m_EF_ModelObjectType.Value, m_EF_ModelObjectIdAsGuid.Value); }
        }

        [ForceMapped]
        internal bool EF_IsNavigationVariable
        {
            get { return m_IsNavigationVariable; }
            set { m_IsNavigationVariable = value; }
        }

        [ForceMapped]
        internal bool EF_IsStructuralAggregation
        {
            get { return m_IsStructuralAggregation; }
            set { m_IsStructuralAggregation = value; }
        }

        [ForceMapped]
        internal bool EF_IsStructuralFilter
        {
            get { return m_IsStructuralFilter; }
            set { m_IsStructuralFilter = value; }
        }

        [ForceMapped]
        internal bool EF_IsTimeAggregation
        {
            get { return IsTimeAggregation; }
            set { /* do nothing */ }
        }

        [ForceMapped]
        internal bool EF_IsTimeFilter
        {
            get { return IsTimeFilter; }
            set { /* do nothing */ }
        }

        [ForceMapped]
        internal bool EF_IsTimeShift
        {
            get { return IsTimeShift; }
            set { /* do nothing */ }
        }

        [ForceMapped]
        internal bool EF_IsTimeIntrospection
        {
            get { return IsTimeIntrospection; }
            set { /* do nothing */ }
        }

        [ForceMapped]
        internal Nullable<Guid> EF_RootExpressionGuid
        {
            get
            {
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_RootExpressionGuid);
                return m_RootExpressionGuid;
            }
            set
            {
                m_RootExpressionGuid = value;
                ExecutionInfoRecorder.CurrentRecorder.RecordExecution(m_RootExpressionGuid);
            }
        }

        #region IParentIdProvider Database Persistence Properties

        [ForceMapped]
        internal Nullable<int> EF_ParentModelObjectType
        {
            get { return m_ParentModelObjectType.GetAsInt(); }
            set { m_ParentModelObjectType = value.GetAsEnum<ModelObjectType>(); }
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

        #region Formula Aggregate Persistence Properties and Methods

        [NotMapped]
        internal ICollection<Expression> EF_Expressions
        {
            get { return m_Expressions.Values; }
            set
            {
                m_Expressions.Clear();
                foreach (Expression expression in value)
                {
                    m_Expressions.Add(expression.Key.ExpressionGuid, expression);
                }
            }
        }

        [NotMapped]
        internal ICollection<Argument> EF_Arguments
        {
            get
            {
                IDictionary<ArgumentId, Argument> arguments = new Dictionary<ArgumentId, Argument>();
                foreach (Expression expression in m_Expressions.Values)
                {
                    Dictionary<int, Argument> expressionArguments = expression.EF_Arguments;
                    foreach (Argument argument in expressionArguments.Values)
                    { arguments.Add(argument.Key, argument); }
                }
                return arguments.Values;
            }
            set
            {
                foreach (Expression expression in m_Expressions.Values)
                { expression.EF_Arguments = new Dictionary<int, Argument>(); }

                foreach (Argument argument in value)
                {
                    Expression parentExpression = m_Expressions[argument.Key.ExpressionGuid];
                    parentExpression.EF_Arguments.Add(argument.Key.ArgumentIndex, argument);
                }
            }
        }

        [NotMapped]
        ICollection<QuerySpecification> IEfAggregate<Formula>.OverriddenQueries
        {
            get { return new List<QuerySpecification>(); }
        }

        bool IEfAggregate<Formula>.SearchNestedAggregateValues(DbContext context, IParameterizedQuery<Formula> query)
        {
            throw new InvalidOperationException("The specified query is not properly overridden.");
        }

        internal static readonly string TypeName_Formula = typeof(Formula).Name;
        internal static readonly string TypeName_Expression = typeof(Expression).Name;
        internal static readonly string TypeName_Argument = typeof(Argument).Name;

        void IEfAggregate<Formula>.ReadNestedAggregateValues(DbContext context, object rootQueryPredicate, Dictionary<string, object> batchReadState)
        {
            var expressionsDict = new Dictionary<FormulaId, List<Expression>>();
            var argumentsDict = new Dictionary<FormulaId, List<Argument>>();

            PerformBatchRead(context, batchReadState, ref expressionsDict, ref argumentsDict);

            var expressionsList = (expressionsDict.ContainsKey(this.Key)) ? expressionsDict[this.Key] : new List<Expression>();
            this.EF_Expressions = expressionsList;

            var argumentsList = (argumentsDict.ContainsKey(this.Key)) ? argumentsDict[this.Key] : new List<Argument>();
            this.EF_Arguments = argumentsList;
        }

        private void PerformBatchRead(DbContext context, Dictionary<string, object> batchReadState, ref Dictionary<FormulaId, List<Expression>> expressionsDict, ref Dictionary<FormulaId, List<Argument>> argumentsDict)
        {
            var rootObjects = (batchReadState[TypeName_Formula] as IEnumerable<Formula>);

            if (batchReadState.ContainsKey(TypeName_Expression))
            {
                expressionsDict = (batchReadState[TypeName_Expression] as Dictionary<FormulaId, List<Expression>>);
            }
            else
            {
                var rootKeys = rootObjects.Select(x => x.Key).ToList();
                var keysAsSqlMatchableText = IEfEntityUtils.GetKeysAsSqlMatchableText(this, rootKeys);
                var keyMatchingSql = GetSqlCode_ForKeyMatching();

                var expressionsSet = context.Set<Expression>();
                var expressionsQuery = expressionsSet.CreateKeyMatchingDbSqlQuery(keysAsSqlMatchableText, keyMatchingSql);
                var expressions = expressionsQuery.ToList();

                foreach (var exp in expressions)
                {
                    var key = new FormulaId(exp.EF_ProjectGuid, exp.EF_RevisionNumber, exp.EF_FormulaGuid);
                    if (!expressionsDict.ContainsKey(key))
                    { expressionsDict.Add(key, new List<Expression>()); }
                    expressionsDict[key].Add(exp);
                }

                batchReadState.Add(TypeName_Expression, expressionsDict);
            }

            if (batchReadState.ContainsKey(TypeName_Argument))
            {
                argumentsDict = (batchReadState[TypeName_Argument] as Dictionary<FormulaId, List<Argument>>);
            }
            else
            {
                var rootKeys = rootObjects.Select(x => x.Key).ToList();
                var keysAsSqlMatchableText = IEfEntityUtils.GetKeysAsSqlMatchableText(this, rootKeys);
                var keyMatchingSql = GetSqlCode_ForKeyMatching();

                var argumentsSet = context.Set<Argument>();
                var argumentsQuery = argumentsSet.CreateKeyMatchingDbSqlQuery(keysAsSqlMatchableText, keyMatchingSql);
                var arguments = argumentsQuery.ToList();

                foreach (var arg in arguments)
                {
                    var key = new FormulaId(arg.EF_ProjectGuid, arg.EF_RevisionNumber, arg.EF_FormulaGuid);
                    if (!argumentsDict.ContainsKey(key))
                    { argumentsDict.Add(key, new List<Argument>()); }
                    argumentsDict[key].Add(arg);
                }

                batchReadState.Add(TypeName_Argument, argumentsDict);
            }
        }

        void IEfAggregate<Formula>.AddNestedAggregateValues(DbContext context)
        {
            System.Linq.Expressions.Expression<Func<ICollection<Expression>>> expressionsGetterExpression = () => this.EF_Expressions;
            EfAggregateUtilities.AddNestedValues<Expression>(context, expressionsGetterExpression);

            System.Linq.Expressions.Expression<Func<ICollection<Argument>>> argumentsGetterExpression = () => this.EF_Arguments;
            EfAggregateUtilities.AddNestedValues<Argument>(context, argumentsGetterExpression);
        }

        void IEfAggregate<Formula>.UpdateNestedAggregateValues(DbContext context, Formula originalAggregate)
        {
            System.Linq.Expressions.Expression<Func<Expression, ExpressionId>> expressionsKeyGetterExpression = ((Expression nestedObj) => nestedObj.Key);
            System.Linq.Expressions.Expression<Func<Expression, Expression, bool>> expressionsMatchExpression = ((Expression obj1, Expression obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_FormulaGuid == obj2.EF_FormulaGuid) && (obj1.EF_ExpressionGuid == obj2.EF_ExpressionGuid)));
            System.Linq.Expressions.Expression<Func<Formula, IDictionary<ExpressionId, Expression>>> expressionsDictionaryGetterExpression = (Formula agg) => agg.EF_Expressions.ToDictionary(val => val.Key, val => val);
            EfAggregateUtilities.UpdateNestedValues<Formula, ExpressionId, Expression>(context, this, originalAggregate, expressionsKeyGetterExpression, expressionsMatchExpression, expressionsDictionaryGetterExpression);

            System.Linq.Expressions.Expression<Func<Argument, ArgumentId>> argumentsKeyGetterExpression = ((Argument nestedObj) => nestedObj.Key);
            System.Linq.Expressions.Expression<Func<Argument, Argument, bool>> argumentsMatchExpression = ((Argument obj1, Argument obj2) => ((obj1.EF_ProjectGuid == obj2.EF_ProjectGuid) && (obj1.EF_RevisionNumber == obj2.EF_RevisionNumber) && (obj1.EF_FormulaGuid == obj2.EF_FormulaGuid) && (obj1.EF_ExpressionGuid == obj2.EF_ExpressionGuid) && (obj1.EF_ArgumentIndex == obj2.EF_ArgumentIndex)));
            System.Linq.Expressions.Expression<Func<Formula, IDictionary<ArgumentId, Argument>>> argumentsDictionaryGetterExpression = (Formula agg) => agg.EF_Arguments.ToDictionary(val => val.Key, val => val);
            EfAggregateUtilities.UpdateNestedValues<Formula, ArgumentId, Argument>(context, this, originalAggregate, argumentsKeyGetterExpression, argumentsMatchExpression, argumentsDictionaryGetterExpression);
        }

        void IEfAggregate<Formula>.DeleteNestedAggregateValues(DbContext context)
        {
            System.Linq.Expressions.Expression<Func<Expression, bool>> expressionsDeleteQueryExpression = ((Expression expr) => ((expr.EF_ProjectGuid == Key.ProjectGuid) && (expr.EF_RevisionNumber == Key.RevisionNumber) && (expr.EF_FormulaGuid == Key.FormulaGuid)));
            EfAggregateUtilities.DeleteNestedValues(context, expressionsDeleteQueryExpression);

            System.Linq.Expressions.Expression<Func<Argument, bool>> argumentsDeleteQueryExpression = ((Argument arg) => ((arg.EF_ProjectGuid == Key.ProjectGuid) && (arg.EF_RevisionNumber == Key.RevisionNumber) && (arg.EF_FormulaGuid == Key.FormulaGuid)));
            EfAggregateUtilities.DeleteNestedValues(context, argumentsDeleteQueryExpression);
        }

        void IEfAggregate_Detachable<Formula>.DetachNestedAggregateValues(DbContext context)
        {
            foreach (var expression in EF_Expressions)
            {
                context.Entry(expression).State = EntityState.Detached;
            }

            foreach (var argument in EF_Arguments)
            {
                context.Entry(argument).State = EntityState.Detached;
            }
        }

        #endregion

        #region DataSet Deep Copy

        public static readonly FormulaDataSet DataSetSchema = new FormulaDataSet();

        public static void Formula_DeepCopy(Formula formula, DataTable dataTable, RevisionChain revisionChain)
        {
            var expressionsDataTable = dataTable.DataSet.Tables[DataSetSchema.Expressions.TableName];
            var argumentsDataTable = dataTable.DataSet.Tables[DataSetSchema.Arguments.TableName];

            foreach (var expression in formula.Expressions.Values)
            {
                var dataRow = expressionsDataTable.NewRow();
                expression.CopyObjectToDataRow(dataRow, revisionChain.DesiredRevisionNumber);
                expressionsDataTable.Rows.Add(dataRow);
                dataRow.AcceptChanges();
                dataRow.SetAdded();
            }

            foreach (var argument in formula.Arguments.Values)
            {
                var dataRow = argumentsDataTable.NewRow();
                argument.CopyObjectToDataRow(dataRow, revisionChain.DesiredRevisionNumber);
                argumentsDataTable.Rows.Add(dataRow);
                dataRow.AcceptChanges();
                dataRow.SetAdded();
            }
        }

        #endregion
    }
}