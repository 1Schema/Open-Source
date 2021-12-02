using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Operations;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Expression
    {
        public ComputationResult Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext)
        {
            ComputationUtils.AssertValidationConditions(dataProvider, currentState.VariableTemplateRef, parentFormula, this.Key);
            ComputationResult result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key.FormulaId, this.Key.ExpressionGuid);

            if (!OperationCatalog.HasOperation(this.OperationId))
            {
                result.SetErrorState(ComputationResultType.OperationNotDefined, "The requested Operation is not defined.");
                return result;
            }

            IOperation operation = OperationCatalog.GetOperation(this.OperationId);
            bool isStructuralAggregation = operation.StructuralOperationType.IsAggregation();
            bool isTimeAggregation = operation.TimeOperationType.IsAggregation();

            if (!operation.ValidityType.IsValid(currentState.ValidityArea))
            {
                result.SetErrorState(ComputationResultType.OperationNotValidForArea, "The requested Operation is not valid for the current Validity Area.");
                return result;
            }

            currentState = GetCurrentStateToUse(dataProvider, parentFormula, currentState, isTimeAggregation);

            StructuralSpace resultingSpace = StructuralSpace.GlobalStructuralSpace;
            List<ModelObjectReference> processedTemplates = new List<ModelObjectReference>();
            SortedDictionary<int, OperationMember> operationMembers = new SortedDictionary<int, OperationMember>();

            bool hasError = false;
            var nestedParentExpressions = new List<IExpression>(parentExpressions);
            nestedParentExpressions.Add(this);

            foreach (int argumentIndex in this.ArgumentsByIndex.Keys)
            {
                IArgument argument = this.GetArgument(argumentIndex);
                ArgumentId argumentId = argument.Key;
                ComputationResult nestedResult = argument.Validate(dataProvider, currentState, parentFormula, nestedParentExpressions, structuralContext);

                if (!nestedResult.IsValid)
                {
                    result.SetErrorState(nestedResult);
                    hasError = true;
                }
                else
                {
                    result.AddNestedResult(nestedResult);

                    resultingSpace = resultingSpace.Merge(nestedResult.ResultingSpace);
                    processedTemplates.AddRange(nestedResult.ProcessedVariableTemplates);
                }

                if (hasError)
                { return result; }

                OperationMember operationMember = null;
                if (argument.ArgumentType == ArgumentType.DirectValue)
                { operationMember = new OperationMember(argument.DirectChronometricValue, nestedResult.ValidatedUnit); }
                else
                { operationMember = new OperationMember(nestedResult.ValidatedDataType, nestedResult.ValidatedTimeDimensionality, nestedResult.ValidatedUnit); }
                operationMembers.Add(argumentId.ArgumentIndex, operationMember);
            }

            OperationMember outputMember = operation.Validate(dataProvider, currentState, parentFormula, this, operationMembers);

            if (!outputMember.IsValid)
            {
                result.SetErrorState(ComputationResultType.OperationArgumentsNotValid, "The arguments supplied to this Operation do not match any valid Signature.");
                return result;
            }
            else
            {
                result.SetValidatedState(resultingSpace, processedTemplates.Distinct(), outputMember.DataType, outputMember.TimeDimesionality, outputMember.Unit);
                return result;
            }
        }
    }
}