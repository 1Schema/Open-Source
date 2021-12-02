using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Formulas.Operations;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas
{
    public interface IExpression
    {
        #region Properties

        ExpressionId Key { get; }
        OperationId OperationId { get; }
        IOperation Operation { get; }
        bool ShowAsOperator { get; set; }
        int OuterParenthesesCount { get; set; }

        ICollection<ArgumentId> ArgumentIds { get; }
        IDictionary<ArgumentId, IArgument> ArgumentsById { get; }
        IDictionary<int, IArgument> ArgumentsByIndex { get; }

        #endregion

        #region Assessment Methods

        bool HasArgument(int argumentIndex);
        ArgumentId CreateArgument(int argumentIndex);
        IArgument GetArgument(int argumentIndex);
        IArgument GetArgument(ArgumentId argumentId);
        void DeleteArgument(int argumentIndex);
        void DeleteArgument(ArgumentId argumentId);

        #endregion

        #region Usage Methods

        ComputationResult Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext);

        ComputationResult Compute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext);

        string RenderAsString(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings);

        string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, ICollection<IExpression> parentExpressions, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings);

        #endregion
    }
}