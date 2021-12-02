using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula
    {
        public string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo)
        {
            IDictionary<ExpressionId, string> expressionsAsStrings;
            IDictionary<ArgumentId, string> argumentsAsStrings;

            return this.RenderAsSqlSelect(dataProvider, currentState, exportInfo, out expressionsAsStrings, out argumentsAsStrings);
        }

        public string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings)
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
                result = RootExpression.RenderAsSqlSelect(dataProvider, currentState, exportInfo, this, new List<IExpression>(), out expressionsAsStrings, out argumentsAsStrings);
            }

            return result;
        }
    }
}