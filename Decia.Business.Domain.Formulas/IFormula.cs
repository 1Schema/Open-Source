using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Formulas.Exports;
using Decia.Business.Domain.Formulas.Expressions;

namespace Decia.Business.Domain.Formulas
{
    public interface IFormula<T> : IFormula, IProjectMember_Deleteable<T>, IProjectMember_Cloneable<T>, IModelObjectWithRef
        where T : IFormula<T>
    { }

    public interface IFormula : IProjectMember
    {
        #region Properties

        FormulaId Key { get; }
        Nullable<ExpressionId> RootExpressionId { get; set; }
        IExpression RootExpression { get; }
        ICollection<ExpressionId> ExpressionIds { get; }
        IDictionary<ExpressionId, IExpression> Expressions { get; }
        ICollection<ArgumentId> ArgumentIds { get; }
        IDictionary<ArgumentId, IArgument> Arguments { get; }
        bool HasDeletedRefs { get; }

        Nullable<ModelObjectReference> VariableOriginRef { get; set; }
        bool IsNavigationVariable { get; set; }

        bool IsStructuralAggregation { get; set; }
        bool IsStructuralFilter { get; }

        bool IsTimeAggregation { get; }
        bool IsTimeFilter { get; }
        bool IsTimeShift { get; }
        bool IsTimeIntrospection { get; }

        bool HasQuery { get; }

        #endregion

        #region Assessment Methods

        ExpressionId CreateRootExpression<T>() where T : IOperation;
        ExpressionId CreateNestedExpression<T>(ArgumentId argumentIdToFill) where T : IOperation;
        ExpressionId CreateRootExpression(Type operationType);
        ExpressionId CreateNestedExpression(ArgumentId argumentIdToFill, Type operationType);
        ExpressionId CreateRootExpression(OperationId operationId);
        ExpressionId CreateNestedExpression(ArgumentId argumentIdToFill, OperationId operationId);

        IExpression GetExpression(ExpressionId expressionId);
        void DeleteExpression(ExpressionId expressionId);
        void PruneExpressions();
        ICollection<ExpressionId> GetNestedExpressionIds();
        ICollection<ExpressionId> GetNestedExpressionIds(IExpression startExpression);
        ICollection<ArgumentId> GetNestedArgumentIds();
        ICollection<ArgumentId> GetNestedArgumentIds(IExpression startExpression);

        #endregion

        #region Usage Methods

        ComputationResult Initialize(IFormulaDataProvider dataProvider, ICurrentState currentState);

        ComputationResult Validate(IFormulaDataProvider dataProvider, ICurrentState currentState);
        ComputationResult Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, out bool requiresComputationByPeriod);

        ComputationResult Compute(IFormulaDataProvider dataProvider, ICurrentState currentState);
        IDictionary<MultiTimePeriodKey, ComputationResult> ComputeForTimeKeys(IFormulaDataProvider dataProvider, ICurrentState currentState, IEnumerable<MultiTimePeriodKey> orderedTimeKeys);

        string RenderAsString(IFormulaDataProvider dataProvider, ICurrentState currentState);
        string RenderAsString(IFormulaDataProvider dataProvider, ICurrentState currentState, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings);

        string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo);
        string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings);

        #endregion
    }
}