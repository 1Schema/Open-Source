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
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Operations;
using Decia.Business.Domain.Formulas.Operations.Time.Aggregation;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Expression
    {
        public ComputationResult Compute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext)
        {
            ComputationUtils.AssertComputationConditions(dataProvider, currentState.VariableInstanceRef, parentFormula, this.Key);
            ComputationResult result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key.FormulaId, this.Key.ExpressionGuid);

            if (!OperationCatalog.HasOperation(this.OperationId))
            {
                result.SetErrorState(ComputationResultType.OperationNotDefined, "The requested Operation is not defined.");
                return result;
            }

            IOperation operation = OperationCatalog.GetOperation(this.OperationId);
            bool isStructuralAggregation = operation.StructuralOperationType.IsAggregation();
            bool isTimeAggregation = operation.TimeOperationType.IsAggregation();

            currentState = GetCurrentStateToUse(dataProvider, parentFormula, currentState, isTimeAggregation);

            StructuralSpace resultingSpace = StructuralSpace.GlobalStructuralSpace;
            List<ModelObjectReference> processedTemplates = new List<ModelObjectReference>();

            bool hasError = false;
            var nestedParentExpressions = new List<IExpression>(parentExpressions);
            nestedParentExpressions.Add(this);
            var argumentResults = new SortedDictionary<int, ComputationResult>();

            foreach (int argumentIndex in this.ArgumentsByIndex.Keys)
            {
                IArgument argument = this.GetArgument(argumentIndex);
                ArgumentId argumentId = argument.Key;
                ComputationResult nestedResult = argument.Compute(dataProvider, currentState, parentFormula, nestedParentExpressions, structuralContext);

                argumentResults.Add(argumentIndex, nestedResult);

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
            }

            if (hasError)
            { return result; }

            if (!isStructuralAggregation)
            {
                ComputeValues(dataProvider, parentFormula, currentState, structuralContext, operation, resultingSpace, processedTemplates, argumentResults, result);
            }
            else
            {
                AggregateValues(dataProvider, parentFormula, currentState, structuralContext, operation, resultingSpace, processedTemplates, argumentResults, result);
            }

            return result;
        }

        private void ComputeValues(IFormulaDataProvider dataProvider, IFormula parentFormula, ICurrentState currentState, IStructuralContext structuralContext, IOperation operation, StructuralSpace resultingSpace, List<ModelObjectReference> processedTemplates, SortedDictionary<int, ComputationResult> argumentResults, ComputationResult expressionResult)
        {
            bool hasError = false;
            Nullable<DeciaDataType> dataType = null;
            ChronometricValue defaultOutputValue = null;
            Dictionary<StructuralPoint, ChronometricValue> outputValues = new Dictionary<StructuralPoint, ChronometricValue>();
            CompoundUnit outputUnit = CompoundUnit.GetGlobalScalarUnit(currentState.ProjectGuid, currentState.RevisionNumber);

            if (currentState.ProcessingType == ProcessingType.Normal)
            { defaultOutputValue = dataProvider.GetDefaultTimeMatrix(currentState.ModelInstanceRef, currentState.VariableInstanceRef); }
            else if (currentState.ProcessingType == ProcessingType.Anonymous)
            { defaultOutputValue = (dataProvider as IFormulaDataProvider_Anonymous).GetDefaultAnonymousVariableTimeMatrix(currentState.ModelInstanceRef, currentState.VariableTemplateRef); }
            else
            { throw new InvalidOperationException("Invalid ProcessingType encountered."); }

            dataType = defaultOutputValue.DataType;

            foreach (StructuralPoint structuralPoint in structuralContext.ResultingPoints)
            {
                var hasValidResultsForAllMembers = true;
                var operationMembers = new SortedDictionary<int, OperationMember>();

                foreach (int argumentIndex in argumentResults.Keys)
                {
                    IArgument argument = this.GetArgument(argumentIndex);
                    ArgumentId argumentId = argument.Key;
                    ComputationResult nestedResult = argumentResults[argumentIndex];

                    OperationMember operationMember = null;
                    if (argument.ArgumentType == ArgumentType.DirectValue)
                    { operationMember = new OperationMember(argument.DirectChronometricValue, nestedResult.ValidatedUnit); }
                    else if (argument.ArgumentType == ArgumentType.ReferencedId)
                    {
                        if (!nestedResult.ComputedValuesByPoint.ContainsKey(structuralPoint))
                        { hasValidResultsForAllMembers = false; }
                        else
                        { operationMember = new OperationMember(nestedResult.ComputedValuesByPoint[structuralPoint], nestedResult.ValidatedUnit, argument.ReferencedModelObject); }
                    }
                    else
                    {
                        if (!nestedResult.ComputedValuesByPoint.ContainsKey(structuralPoint))
                        { hasValidResultsForAllMembers = false; }
                        else
                        { operationMember = new OperationMember(nestedResult.ComputedValuesByPoint[structuralPoint], nestedResult.ValidatedUnit); }
                    }
                    operationMembers.Add(argumentId.ArgumentIndex, operationMember);
                }


                CompoundUnit defaultOutputUnit = defaultOutputValue.Unit;
                OperationMember defaultOutputMember = new OperationMember(defaultOutputValue.CopyNew(), defaultOutputUnit);
                OperationMember outputMember = null;

                if (!hasValidResultsForAllMembers)
                { outputMember = defaultOutputMember; }
                else
                { outputMember = operation.Compute(dataProvider, currentState, parentFormula, this, operationMembers, defaultOutputMember); }

                if (!outputMember.IsValid)
                {
                    expressionResult.SetErrorState(ComputationResultType.OperationArgumentsNotValid, "The arguments supplied to this Operation do not match any valid Signature.");
                    hasError = true;
                }
                else if (!outputMember.IncludeInResults)
                { continue; }
                else
                {
                    dataType = outputMember.DataType;
                    outputValues.Add(structuralPoint, outputMember.ChronometricValue);
                    outputUnit = outputMember.Unit;
                }
            }

            if (hasError)
            { return; }

            if (!dataType.HasValue)
            {
                expressionResult.SetErrorState(ComputationResultType.ExpressionContainsZeroInstances, "The current Expression contains no instances of values.");
                return;
            }

            expressionResult.SetComputedState(resultingSpace, processedTemplates.Distinct(), dataType.Value, outputValues, outputUnit);
            return;
        }

        private void AggregateValues(IFormulaDataProvider dataProvider, IFormula parentFormula, ICurrentState currentState, IStructuralContext structuralContext, IOperation operation, StructuralSpace resultingSpace, List<ModelObjectReference> processedTemplates, SortedDictionary<int, ComputationResult> argumentResults, ComputationResult expressionResult)
        {
            bool hasError = false;
            Nullable<DeciaDataType> dataType = null;
            Dictionary<StructuralPoint, ChronometricValue> outputValues = new Dictionary<StructuralPoint, ChronometricValue>();
            CompoundUnit outputUnit = CompoundUnit.GetGlobalScalarUnit(currentState.ProjectGuid, currentState.RevisionNumber);

            SortedDictionary<int, OperationMember> operationMembers = new SortedDictionary<int, OperationMember>();

            foreach (int argumentIndex in argumentResults.Keys)
            {
                IArgument argument = this.GetArgument(argumentIndex);
                ArgumentId argumentId = argument.Key;
                ComputationResult nestedResult = argumentResults[argumentIndex];

                OperationMember operationMember = null;
                if (argument.ArgumentType == ArgumentType.DirectValue)
                { operationMember = new OperationMember(argument.DirectChronometricValue, nestedResult.ValidatedUnit); }
                else
                { operationMember = new OperationMember(nestedResult.ValidatedDataType, nestedResult.ComputedValuesByPoint, nestedResult.ValidatedUnit); }
                operationMembers.Add(argumentId.ArgumentIndex, operationMember);
            }

            ChronometricValue defaultOutputValue = dataProvider.GetDefaultTimeMatrix(currentState.ModelInstanceRef, currentState.VariableInstanceRef);
            CompoundUnit defaultOutputUnit = defaultOutputValue.Unit;
            OperationMember defaultOutputMember = new OperationMember(defaultOutputValue, defaultOutputUnit);
            OperationMember outputMember = operation.Compute(dataProvider, currentState, parentFormula, this, operationMembers, defaultOutputMember);

            if (!outputMember.IsValid)
            {
                expressionResult.SetErrorState(ComputationResultType.OperationArgumentsNotValid, "The arguments supplied to this Operation do not match any valid Signature.");
                hasError = true;
            }
            else
            {
                StructuralPoint outputStructuralPoint = dataProvider.StructuralMap.GetStructuralPoint(currentState.ModelInstanceRef, currentState.NavigationPeriod, currentState.StructuralInstanceRef, currentState.UseExtendedStructure);

                dataType = outputMember.DataType;
                outputValues.Add(outputStructuralPoint, outputMember.ChronometricValue);
                outputUnit = outputMember.Unit;
            }

            if (hasError)
            { return; }

            if (!dataType.HasValue)
            {
                expressionResult.SetErrorState(ComputationResultType.ExpressionContainsZeroInstances, "The current Expression contains no instances of values.");
                return;
            }

            expressionResult.SetComputedState(resultingSpace, processedTemplates.Distinct(), dataType.Value, outputValues, outputUnit);
            return;
        }

        private ICurrentState GetCurrentStateToUse(IFormulaDataProvider dataProvider, IFormula parentFormula, ICurrentState currentState, bool isTimeAggregation)
        {
            if (!isTimeAggregation)
            { return currentState; }

            CurrentState currentStateToPass = new CurrentState(currentState);
            var isRootExpression = (parentFormula.RootExpressionId == this.Key);
            var isValidated = (dataProvider.GetIsValidated(currentState.VariableTemplateRef) == true);

            var timeDimensions = dataProvider.GetAssessedTimeDimensions(currentState.VariableTemplateRef);
            if (isValidated)
            { timeDimensions = dataProvider.GetValidatedTimeDimensions(currentState.VariableTemplateRef); }

            if (isRootExpression)
            { currentStateToPass.SetToTransformTimeframe(timeDimensions.PrimaryTimeDimension.NullableTimePeriodType, timeDimensions.SecondaryTimeDimension.NullableTimePeriodType); }
            else
            { currentStateToPass.SetToComputeEntireTimeframe(); }

            return currentStateToPass;
        }
    }
}