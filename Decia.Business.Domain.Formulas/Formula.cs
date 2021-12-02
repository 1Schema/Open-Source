using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Formulas.Expressions;
using Decia.Business.Domain.Formulas.Operations;
using Decia.Business.Domain.Formulas.Operations.Queries;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula : ProjectDomainObjectBase_Deleteable<FormulaId, Formula>, IFormula<Formula>, IParentIdProvider
    {
        #region Static Members

        public const bool DefaultIsNavigationVariable = false;
        public const bool DefaultIsStructuralAggregation = false;
        public const bool DefaultIsStructuralFilter = false;
        public static readonly DeciaDataType InvalidDataType = DeciaDataTypeUtils.InvalidDataType;

        #endregion

        #region Members

        protected Nullable<ModelObjectType> m_ParentModelObjectType;
        protected SortedDictionary<ModelObjectType, ModelObjectReference> m_ParentModelObjectRefs;

        protected Nullable<ModelObjectReference> m_ModelObjectRef;
        protected bool m_IsNavigationVariable;
        protected bool m_IsStructuralAggregation;
        protected bool m_IsStructuralFilter;
        protected Nullable<Guid> m_RootExpressionGuid;
        protected Dictionary<Guid, Expression> m_Expressions;

        #endregion

        #region Constructors

        public Formula()
            : this(FormulaId.DefaultId.ProjectGuid, FormulaId.DefaultId.RevisionNumber_NonNull)
        { }

        public Formula(Guid projectGuid, long revisionNumber)
            : this(projectGuid, revisionNumber, Guid.NewGuid())
        { }

        public Formula(Guid projectGuid, long revisionNumber, Guid formulaGuid)
            : this(projectGuid, revisionNumber, formulaGuid, null)
        { }

        public Formula(Guid projectGuid, long revisionNumber, Nullable<ModelObjectReference> modelObjectRef)
            : this(projectGuid, revisionNumber, Guid.NewGuid(), modelObjectRef)
        { }

        public Formula(Guid projectGuid, long revisionNumber, Guid formulaGuid, Nullable<ModelObjectReference> modelObjectRef)
            : this(projectGuid, revisionNumber, formulaGuid, modelObjectRef, null, null)
        { }

        public Formula(Guid projectGuid, long revisionNumber, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, Guid.NewGuid(), parentModelObjectType, parentModelObjectRefs)
        { }

        public Formula(Guid projectGuid, long revisionNumber, Guid formulaGuid, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, formulaGuid, null, parentModelObjectType, parentModelObjectRefs)
        { }

        public Formula(Guid projectGuid, long revisionNumber, Nullable<ModelObjectReference> modelObjectRef, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, Guid.NewGuid(), modelObjectRef, parentModelObjectType, parentModelObjectRefs)
        { }

        public Formula(Guid projectGuid, long revisionNumber, Guid formulaGuid, Nullable<ModelObjectReference> modelObjectRef, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, formulaGuid, modelObjectRef, parentModelObjectType, parentModelObjectRefs.ToDictionary(x => x.ModelObjectType, x => x))
        { }

        protected Formula(Guid projectGuid, long revisionNumber, Guid formulaGuid, Nullable<ModelObjectReference> modelObjectRef, Nullable<ModelObjectType> parentModelObjectType, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs)
            : base(projectGuid, revisionNumber)
        {
            m_Key = new FormulaId(projectGuid, revisionNumber, formulaGuid);

            if (!parentModelObjectType.HasValue)
            { ClearParent(); }
            else
            { SetParent(parentModelObjectType.Value, parentModelObjectRefs); }

            m_ModelObjectRef = modelObjectRef;
            m_IsNavigationVariable = DefaultIsNavigationVariable;
            m_IsStructuralAggregation = DefaultIsStructuralAggregation;
            m_IsStructuralFilter = DefaultIsStructuralFilter;
            m_RootExpressionGuid = null;
            m_Expressions = new Dictionary<Guid, Expression>();
        }

        #endregion

        #region Base-Class Method Overrides

        protected override Guid GetProjectGuid()
        {
            return EF_ProjectGuid;
        }

        protected override long? GetRevisionNumber()
        {
            return EF_RevisionNumber;
        }

        protected override void SetProjectGuid(Guid projectGuid)
        {
            EF_ProjectGuid = projectGuid;
        }

        protected override void SetRevisionNumber(long revisionNumber)
        {
            EF_RevisionNumber = revisionNumber;
        }

        #endregion

        [NotMapped]
        public Nullable<ModelObjectReference> VariableOriginRef
        {
            get { return m_ModelObjectRef; }
            set { m_ModelObjectRef = value; }
        }

        [NotMapped]
        public bool IsNavigationVariable
        {
            get { return m_IsNavigationVariable; }
            set { m_IsNavigationVariable = value; }
        }

        [NotMapped]
        public bool IsStructuralAggregation
        {
            get { return m_IsStructuralAggregation; }
            set { m_IsStructuralAggregation = value; }
        }

        [NotMapped]
        public bool IsStructuralFilter
        {
            get
            {
                int structuralFilterCount = m_Expressions.Values.Where(expr => expr.Operation.StructuralOperationType.IsFilter()).ToList().Count;
                return (structuralFilterCount > 0);
            }
        }

        [NotMapped]
        public bool IsTimeAggregation
        {
            get
            {
                int timeAggrCount = m_Expressions.Values.Where(expr => expr.Operation.TimeOperationType.IsAggregation()).ToList().Count;
                return (timeAggrCount > 0);
            }
        }

        [NotMapped]
        public bool IsTimeFilter
        {
            get
            {
                int timeFilterCount = m_Expressions.Values.Where(expr => expr.Operation.TimeOperationType.IsFilter()).ToList().Count;
                return (timeFilterCount > 0);
            }
        }

        [NotMapped]
        public bool IsTimeShift
        {
            get
            {
                int timeShiftCount = m_Expressions.Values.Where(expr => expr.Operation.TimeOperationType.IsShift()).ToList().Count;
                return (timeShiftCount > 0);
            }
        }

        [NotMapped]
        public bool IsTimeIntrospection
        {
            get
            {
                int timeIntrospectionCount = m_Expressions.Values.Where(expr => expr.Operation.TimeOperationType.IsIntrospection()).ToList().Count;
                return (timeIntrospectionCount > 0);
            }
        }

        [NotMapped]
        public bool HasQuery
        {
            get { return (m_Expressions.Values.Where(x => (x.Operation is QueryOperation)).Count() > 0); }
        }

        [NotMapped]
        public Nullable<Guid> RootExpressionGuid
        {
            get { return m_RootExpressionGuid; }
            set
            {
                if (!value.HasValue)
                {
                    m_RootExpressionGuid = null;
                    return;
                }

                if (!m_Expressions.ContainsKey(value.Value))
                { throw new InvalidOperationException("The specified ExpressionId does not exist in the Formula."); }

                m_RootExpressionGuid = value;
            }
        }

        [NotMapped]
        public Nullable<ExpressionId> RootExpressionId
        {
            get
            {
                if (!m_RootExpressionGuid.HasValue)
                { return null; }
                return new ExpressionId(m_Key, m_RootExpressionGuid.Value);
            }
            set
            {
                if (!value.HasValue)
                {
                    m_RootExpressionGuid = null;
                    return;
                }

                if (!m_Expressions.ContainsKey(value.Value.ExpressionGuid))
                { throw new InvalidOperationException("The specified ExpressionId does not exist in the Formula."); }

                m_RootExpressionGuid = value.Value.ExpressionGuid;
            }
        }

        [NotMapped]
        public IExpression RootExpression
        {
            get
            {
                if (!m_RootExpressionGuid.HasValue)
                { return null; }
                return m_Expressions[m_RootExpressionGuid.Value];
            }
        }

        [NotMapped]
        public ICollection<ExpressionId> ExpressionIds
        {
            get { return m_Expressions.Keys.Select(expr => new ExpressionId(m_Key, expr)).ToList(); }
        }

        [NotMapped]
        public IDictionary<ExpressionId, IExpression> Expressions
        {
            get { return m_Expressions.ToDictionary(expr => new ExpressionId(m_Key, expr.Key), expr => (IExpression)expr.Value); }
        }

        [NotMapped]
        public ICollection<ArgumentId> ArgumentIds
        {
            get
            {
                List<ArgumentId> argumentIds = new List<ArgumentId>();
                foreach (Guid expressionGuid in m_Expressions.Keys)
                {
                    Expression expression = m_Expressions[expressionGuid];
                    argumentIds.AddRange(expression.ArgumentIds);
                }
                return argumentIds;
            }
        }

        [NotMapped]
        public IDictionary<ArgumentId, IArgument> Arguments
        {
            get
            {
                Dictionary<ArgumentId, IArgument> argumentsById = new Dictionary<ArgumentId, IArgument>();
                foreach (Guid expressionGuid in m_Expressions.Keys)
                {
                    Expression expression = m_Expressions[expressionGuid];

                    foreach (ArgumentId argumentId in expression.ArgumentIds)
                    {
                        IArgument argument = expression.GetArgument(argumentId);
                        argumentsById.Add(argumentId, argument);
                    }
                }
                return argumentsById;
            }
        }

        public bool HasDeletedRefs
        {
            get { return (Arguments.Where(x => x.Value.IsRefDeleted).Count() > 0); }
        }

        public ExpressionId CreateRootExpression<T>()
            where T : IOperation
        {
            var operationId = OperationCatalog.GetOperationId<T>();
            return CreateExpression(null, operationId);
        }

        public ExpressionId CreateNestedExpression<T>(ArgumentId argumentIdToFill)
            where T : IOperation
        {
            var operationId = OperationCatalog.GetOperationId<T>();
            return CreateExpression(argumentIdToFill, operationId);
        }

        public ExpressionId CreateRootExpression(Type operationType)
        {
            var operationId = OperationCatalog.GetOperationId(operationType);
            return CreateExpression(null, operationId);
        }

        public ExpressionId CreateNestedExpression(ArgumentId argumentIdToFill, Type operationType)
        {
            var operationId = OperationCatalog.GetOperationId(operationType);
            return CreateExpression(argumentIdToFill, operationId);
        }

        public ExpressionId CreateRootExpression(OperationId operationId)
        {
            return CreateExpression(null, operationId);
        }

        public ExpressionId CreateNestedExpression(ArgumentId argumentIdToFill, OperationId operationId)
        {
            return CreateExpression(argumentIdToFill, operationId);
        }

        protected ExpressionId CreateExpression(Nullable<ArgumentId> argumentIdToFill, OperationId operationId)
        {
            return CreateExpression(argumentIdToFill, Guid.NewGuid(), operationId);
        }

        internal ExpressionId CreateExpression(Nullable<ArgumentId> argumentIdToFill, Guid expressionGuid, OperationId operationId)
        {
            Expression expression = new Expression(this.Key, expressionGuid, operationId);
            m_Expressions.Add(expression.Key.ExpressionGuid, expression);

            if (!argumentIdToFill.HasValue)
            { m_RootExpressionGuid = expression.Key.ExpressionGuid; }
            else
            {
                ArgumentId parentArgumentId = argumentIdToFill.Value;
                IExpression parentExpression = GetExpression(parentArgumentId.ExpressionId);
                IArgument parentArgument = parentExpression.GetArgument(parentArgumentId);
                parentArgument.SetToNestedExpression(expression.Key);
            }

            return expression.Key;
        }

        public IExpression GetExpression(ExpressionId expressionId)
        {
            if (!m_Expressions.ContainsKey(expressionId.ExpressionGuid))
            { throw new InvalidOperationException("The specified ExpressionId does not exist in the Formula."); }

            return m_Expressions[expressionId.ExpressionGuid];
        }

        public void DeleteExpression(ExpressionId expressionId)
        {
            if (!m_Expressions.ContainsKey(expressionId.ExpressionGuid))
            { throw new InvalidOperationException("The specified ExpressionId does not exist in the Formula."); }

            Expression startExpression = m_Expressions[expressionId.ExpressionGuid];
            foreach (ExpressionId nestedExpressionId in GetNestedExpressionIds(startExpression))
            { m_Expressions.Remove(nestedExpressionId.ExpressionGuid); }

            if (m_RootExpressionGuid == expressionId.ExpressionGuid)
            { m_RootExpressionGuid = null; }
        }

        public void PruneExpressions()
        {
            ICollection<ExpressionId> connectedExpressionIds = GetNestedExpressionIds(RootExpression);
            var connectedExpressionGuids = connectedExpressionIds.Select(expr => expr.ExpressionGuid).ToList();

            foreach (Guid expressionGuid in m_Expressions.Keys)
            {
                if (connectedExpressionGuids.Contains(expressionGuid))
                { continue; }

                m_Expressions.Remove(expressionGuid);
            }
        }

        public ICollection<ExpressionId> GetNestedExpressionIds()
        {
            IExpression startExpression = RootExpression;
            return GetNestedExpressionIds(startExpression);
        }

        public ICollection<ExpressionId> GetNestedExpressionIds(IExpression startExpression)
        {
            List<ExpressionId> nestedExpressionIds = new List<ExpressionId>();
            GetNestedExpressionIds(startExpression, nestedExpressionIds);
            return nestedExpressionIds;
        }

        private void GetNestedExpressionIds(IExpression startExpression, ICollection<ExpressionId> nestedExpressionIds)
        {
            if (startExpression == null)
            { return; }

            nestedExpressionIds.Add(startExpression.Key);

            foreach (IArgument argument in startExpression.GetArguments_ContainingNestedExpressions())
            {
                Expression nestedExpression = m_Expressions[argument.NestedExpressionId.ExpressionGuid];
                GetNestedExpressionIds(nestedExpression, nestedExpressionIds);
            }
        }

        public ICollection<ArgumentId> GetNestedArgumentIds()
        {
            IExpression startExpression = RootExpression;
            return GetNestedArgumentIds(startExpression);
        }

        public ICollection<ArgumentId> GetNestedArgumentIds(IExpression startExpression)
        {
            List<ArgumentId> nestedArgumentIds = new List<ArgumentId>();
            GetNestedArgumentIds(startExpression, nestedArgumentIds);
            return nestedArgumentIds;
        }

        private void GetNestedArgumentIds(IExpression startExpression, ICollection<ArgumentId> nestedArgumentIds)
        {
            if (startExpression == null)
            { return; }

            foreach (ArgumentId argumentId in startExpression.ArgumentIds)
            { nestedArgumentIds.Add(argumentId); }

            foreach (IArgument argument in startExpression.GetArguments_ContainingNestedExpressions())
            {
                Expression nestedExpression = m_Expressions[argument.NestedExpressionId.ExpressionGuid];
                GetNestedArgumentIds(nestedExpression, nestedArgumentIds);
            }
        }

        #region IProjectMember<Formula> Implementation

        public Formula CopyForRevision(long newRevisionNumber)
        {
            if (this.Key.RevisionNumber >= newRevisionNumber)
            { throw new InvalidOperationException("A Formula created for a new revision must have a greater revision number."); }

            return (this as IProjectMember<Formula>).CopyForProject(this.Key.ProjectGuid, newRevisionNumber);
        }

        Formula IProjectMember<Formula>.CopyForProject(Guid projectGuid, long revisionNumber)
        {
            var newValue = this.Copy();

            newValue.EF_ProjectGuid = projectGuid;
            newValue.EF_RevisionNumber = revisionNumber;

            foreach (var newExpression in newValue.m_Expressions.Values)
            {
                newExpression.EF_ProjectGuid = projectGuid;
                newExpression.EF_RevisionNumber = revisionNumber;

                foreach (Argument newArgument in newExpression.ArgumentsById.Values)
                {
                    newArgument.EF_ProjectGuid = projectGuid;
                    newArgument.EF_RevisionNumber = revisionNumber;
                }
            }
            return newValue;
        }

        #endregion

        #region IProjectMember_Cloneable<T> Implementation

        public virtual Formula CopyAsNew(long revisionNumber)
        {
            var newValue = this.Copy();
            var formulaGuid = Guid.NewGuid();

            newValue.EF_FormulaGuid = formulaGuid;
            newValue.EF_RevisionNumber = revisionNumber;

            foreach (var newExpression in newValue.m_Expressions.Values)
            {
                newExpression.EF_FormulaGuid = formulaGuid;
                newExpression.EF_RevisionNumber = revisionNumber;

                foreach (Argument newArgument in newExpression.ArgumentsById.Values)
                {
                    newArgument.EF_FormulaGuid = formulaGuid;
                    newArgument.EF_RevisionNumber = revisionNumber;
                }
            }
            return newValue;
        }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return Key.ModelObjectType; }
        }

        public Guid ModelObjectId
        {
            get { return Key.ModelObjectId; }
        }

        public bool IdIsInt
        {
            get { return Key.IdIsInt; }
        }

        public int ModelObjectIdAsInt
        {
            get { return Key.ModelObjectIdAsInt; }
        }

        public string ComplexId
        {
            get { return Key.ComplexId; }
        }

        public ModelObjectReference ModelObjectRef
        {
            get { return Key.ModelObjectRef; }
        }

        #endregion

        #region IParentIdProvider Implementation

        [NotMapped]
        public bool HasParent
        {
            get { return m_ParentModelObjectType.HasValue; }
        }

        [NotMapped]
        public ModelObjectType ParentModelObjectType
        {
            get
            {
                this.AssertHasParent();
                return m_ParentModelObjectType.Value;
            }
        }

        [NotMapped]
        public IDictionary<ModelObjectType, ModelObjectReference> ParentModelObjectRefs
        {
            get
            {
                this.AssertHasParent();
                return m_ParentModelObjectRefs;
            }
        }

        public void SetParent(IParentId parentId)
        {
            SetParent(parentId.ModelObjectType, parentId.IdReferences.ToDictionary(x => x.ModelObjectType, x => x));
        }

        public void SetParent(ModelObjectType parentModelObjectType, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs)
        {
            parentModelObjectType.AssertHasParentTypeValue(parentModelObjectRefs);

            m_ParentModelObjectType = parentModelObjectType;
            m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>(parentModelObjectRefs);
        }

        public void ClearParent()
        {
            m_ParentModelObjectType = null;
            m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>();
        }

        public T GetParentId<T>(Func<IParentIdProvider, T> idConversionMethod)
        {
            this.AssertHasParent();
            return idConversionMethod(this);
        }

        #endregion
    }
}