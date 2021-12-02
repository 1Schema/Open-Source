using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Formulas.Exports;
using Decia.Business.Domain.Formulas.Operations;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Argument
    {
        public string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, ICollection<IExpression> parentExpressions, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings)
        {
            expressionsAsStrings = new Dictionary<ExpressionId, string>();
            argumentsAsStrings = new Dictionary<ArgumentId, string>();
            var result = string.Empty;

            if (this.ArgumentType == ArgumentType.DirectValue)
            {
                var dataType = this.DirectValue.DataType;
                object value = this.DirectValue.GetValue();
                result = exportInfo.DbType.ConvertToSqlValue(dataType, value);
            }
            else if (this.ArgumentType == ArgumentType.NestedExpression)
            {
                IDictionary<ExpressionId, string> nestedExpressionsAsStrings;
                IDictionary<ArgumentId, string> nestedArgumentsAsStrings;

                IExpression nestedExpression = parentFormula.GetExpression(this.NestedExpressionId);
                result = nestedExpression.RenderAsSqlSelect(dataProvider, currentState, exportInfo, parentFormula, parentExpressions, out nestedExpressionsAsStrings, out  nestedArgumentsAsStrings);

                expressionsAsStrings = nestedExpressionsAsStrings;
                argumentsAsStrings = nestedArgumentsAsStrings;
            }
            else if (this.ArgumentType == ArgumentType.ReferencedId)
            {
                if (exportInfo.DbType == SqlDb_TargetType.MsSqlServer)
                {
                    var argRef = this.ReferencedModelObject;
                    var argInfo = (SqlArgumentInfo)null;

                    var overrrideParentExpr = parentExpressions.Where(x => exportInfo.ArgumentOverrideInfos.ContainsKey(new MultiPartKey<ModelObjectReference, Guid>(argRef, x.Key.ExpressionGuid))).FirstOrDefault();
                    var hasOverride = (overrrideParentExpr != null);

                    if (!hasOverride)
                    {
                        argInfo = exportInfo.ArgumentInfos[argRef];
                        argInfo.VariableRef_IsUsed = true;
                    }
                    else
                    {
                        argInfo = exportInfo.ArgumentOverrideInfos[new MultiPartKey<ModelObjectReference, Guid>(argRef, overrrideParentExpr.Key.ExpressionGuid)];
                        argInfo.VariableRef_IsUsed = true;
                    }

                    result = string.Format("{0}.[{1}]", argInfo.VariableRef_TableAlias, argInfo.VariableRef_ColumnName);
                }
                else
                { throw new InvalidOperationException("Unsupported DbType encountered."); }
            }
            else
            { throw new InvalidOperationException("Invalid ArgumentType encountered."); }

            argumentsAsStrings.Add(this.Key, result);
            return result;
        }
    }
}