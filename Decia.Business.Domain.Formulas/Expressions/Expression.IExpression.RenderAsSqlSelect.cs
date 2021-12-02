using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Formulas.Exports;
using Decia.Business.Domain.Formulas.Operations;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Expression
    {
        public string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, ICollection<IExpression> parentExpressions, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings)
        {
            var nestedParentExpressions = new List<IExpression>(parentExpressions);
            nestedParentExpressions.Add(this);

            if ((this.Operation.StructuralOperationType == OperationType.Shift) || (this.Operation.TimeOperationType == OperationType.Shift))
            {
                var argsWithRefs = parentFormula.GetNestedArguments_ContainingReferences(this, EvaluationType.Processing);

                foreach (var arg in argsWithRefs)
                {
                    if (arg.ArgumentType != ArgumentType.ReferencedId)
                    { throw new InvalidOperationException("Invalid Argument gathered."); }

                    var argRef = arg.ReferencedModelObject;
                    var exprGuid = this.Key.ExpressionGuid;
                    var baseArgInfo = exportInfo.ArgumentInfos[argRef];

                    if (!(baseArgInfo.TD1_IsRelevant || baseArgInfo.TD2_IsRelevant))
                    { continue; }

                    var newArgInfo = new SqlArgumentOverrideInfo(baseArgInfo, this.Key);
                    exportInfo.ArgumentOverrideInfos.Add(new MultiPartKey<ModelObjectReference, Guid>(argRef, exprGuid), newArgInfo);
                }
            }

            expressionsAsStrings = new Dictionary<ExpressionId, string>();
            argumentsAsStrings = new Dictionary<ArgumentId, string>();
            string result = string.Empty;

            var localArgsAsStrings = new SortedDictionary<int, string>();

            foreach (int argumentIndex in this.ArgumentsByIndex.Keys)
            {
                IDictionary<ExpressionId, string> nestedExpressionsAsStrings;
                IDictionary<ArgumentId, string> nestedArgumentsAsStrings;

                IArgument argument = this.GetArgument(argumentIndex);
                string argumentAsString = argument.RenderAsSqlSelect(dataProvider, currentState, exportInfo, parentFormula, nestedParentExpressions, out nestedExpressionsAsStrings, out nestedArgumentsAsStrings);
                localArgsAsStrings.Add(argumentIndex, argumentAsString);

                foreach (var item in nestedExpressionsAsStrings)
                { expressionsAsStrings.Add(item.Key, item.Value); }
                foreach (var item in nestedArgumentsAsStrings)
                { argumentsAsStrings.Add(item.Key, item.Value); }
            }

            IOperation operation = OperationCatalog.GetOperation(this.OperationId);

            result = operation.RenderAsSqlSelect(dataProvider, currentState, exportInfo, parentFormula, this, localArgsAsStrings);

            expressionsAsStrings.Add(this.Key, result);
            return result;
        }
    }
}