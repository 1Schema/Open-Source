using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula
    {
        public string RenderAsString(IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            IDictionary<ExpressionId, string> expressionsAsStrings;
            IDictionary<ArgumentId, string> argumentsAsStrings;

            return this.RenderAsString(dataProvider, currentState, out expressionsAsStrings, out argumentsAsStrings);
        }

        public string RenderAsString(IFormulaDataProvider dataProvider, ICurrentState currentState, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings)
        {
            expressionsAsStrings = new Dictionary<ExpressionId, string>();
            argumentsAsStrings = new Dictionary<ArgumentId, string>();
            string result = string.Empty;

            if (!RootExpressionId.HasValue)
            {
                result = FormulaRenderingUtils.NullValueAsString;
            }
            else
            {
                result = RootExpression.RenderAsString(dataProvider, currentState, this, out expressionsAsStrings, out argumentsAsStrings);
            }

            return result;
        }
    }
}