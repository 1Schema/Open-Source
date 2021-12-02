using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Formulas.Operations;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Expression
    {
        public string RenderAsString(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings)
        {
            expressionsAsStrings = new Dictionary<ExpressionId, string>();
            argumentsAsStrings = new Dictionary<ArgumentId, string>();
            string result = string.Empty;

            var localArgsAsStrings = new SortedDictionary<int, string>();

            foreach (int argumentIndex in this.ArgumentsByIndex.Keys)
            {
                IDictionary<ExpressionId, string> nestedExpressionsAsStrings;
                IDictionary<ArgumentId, string> nestedArgumentsAsStrings;

                IArgument argument = this.GetArgument(argumentIndex);
                string argumentAsString = argument.RenderAsString(dataProvider, currentState, parentFormula, out nestedExpressionsAsStrings, out nestedArgumentsAsStrings);
                localArgsAsStrings.Add(argumentIndex, argumentAsString);

                foreach (var item in nestedExpressionsAsStrings)
                { expressionsAsStrings.Add(item.Key, item.Value); }
                foreach (var item in nestedArgumentsAsStrings)
                { argumentsAsStrings.Add(item.Key, item.Value); }
            }

            IOperation operation = OperationCatalog.GetOperation(this.OperationId);
            var showUsingOperator = (!string.IsNullOrWhiteSpace(operation.OperatorText) && this.ShowAsOperator);

            if (!operation.IsVisible && !showUsingOperator)
            {
                var prefix = string.Empty;
                var inFix = string.Empty;
                var postFix = string.Empty;

                result = FormulaRenderingUtils.RenderArgumentsAsString(prefix, inFix, postFix, localArgsAsStrings);
            }
            else if (!operation.DisplayAsFunction || showUsingOperator)
            {
                var prefix = (operation.OperatorNotation == OperatorNotationType.Prefix ? operation.OperatorText : string.Empty);
                var firstInFix = " " + (operation.OperatorNotation == OperatorNotationType.Infix ? operation.OperatorText + " " : string.Empty);
                var subInFix = " " + (operation.OperatorNotation == OperatorNotationType.Infix ? operation.SubOperatorText + " " : string.Empty);
                var postFix = (operation.OperatorNotation == OperatorNotationType.Postfix ? operation.OperatorText : string.Empty);

                result = FormulaRenderingUtils.RenderArgumentsAsString(prefix, firstInFix, subInFix, postFix, localArgsAsStrings);
            }
            else
            {
                var prefix = operation.ShortName + "( ";
                var inFix = ", ";
                var postFix = " )";

                result = FormulaRenderingUtils.RenderArgumentsAsString(prefix, inFix, postFix, localArgsAsStrings);
            }

            for (int i = 0; i < OuterParenthesesCount; i++)
            { result = "(" + result + ")"; }

            expressionsAsStrings.Add(this.Key, result);
            return result;
        }
    }
}