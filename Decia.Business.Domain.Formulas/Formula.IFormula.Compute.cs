using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Operations;
using Decia.Business.Domain.Formulas.Operations.Time.Aggregation;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula
    {
        public IDictionary<MultiTimePeriodKey, ComputationResult> ComputeForTimeKeys(IFormulaDataProvider dataProvider, ICurrentState currentState, IEnumerable<MultiTimePeriodKey> orderedTimeKeys)
        {
            Dictionary<MultiTimePeriodKey, ComputationResult> results = new Dictionary<MultiTimePeriodKey, ComputationResult>();

            foreach (MultiTimePeriodKey timeKey in orderedTimeKeys)
            {
                ICurrentState timeKeyCurrentState = new CurrentState(currentState, timeKey);
                ComputationResult result = Compute(dataProvider, timeKeyCurrentState);
                results.Add(timeKey, result);
            }
            return results;
        }

        public ComputationResult Compute(IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            if (currentState.ProcessingType == ProcessingType.Normal)
            { return Compute_Normal(dataProvider, currentState); }
            else if (currentState.ProcessingType == ProcessingType.Anonymous)
            { return Compute_Anonymous((dataProvider as IFormulaDataProvider_Anonymous), currentState); }
            else
            { throw new InvalidOperationException("Invalid ProcessingType encountered."); }
        }

        public ComputationResult Compute_Normal(IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            ComputationUtils.AssertComputationConditions(dataProvider, currentState.VariableInstanceRef, this);
            ComputationResult result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key);

            if (!this.MeetsMinimalExpressionConditions(result))
            { return result; }
            if (!this.MeetsStructuralAggregationConditions(result))
            { return result; }
            if (!this.MeetsStructuralFilterConditions(dataProvider, currentState, result))
            { return result; }
            if (!this.MeetsTimeAggregationConditions(result))
            { return result; }

            var timeDimensionality = dataProvider.GetComputationTimeDimensions(currentState.ModelInstanceRef, currentState.VariableTemplateRef);
            if (timeDimensionality == null)
            {
                result.SetErrorState(ComputationResultType.FormulaValidationPending, "The current Formula has not been Validated yet.");
                return result;
            }

            if (timeDimensionality.PrimaryTimeDimension.HasTimeValue)
            { currentState.NavigationPeriod = currentState.PrimaryPeriod; }
            else
            { currentState.NavigationPeriod = null; }


            foreach (IExpression expression in m_Expressions.Values)
            {
                if (expression == RootExpression)
                { continue; }

                if (expression.Operation == null)
                {
                    result.SetErrorState(ComputationResultType.NullFormula, "The Expression's Operation is not set.");
                    return result;
                }
                else if (expression.Operation.StructuralOperationType.IsAggregation())
                {
                    result.SetErrorState(ComputationResultType.NullFormula, "Only the RootExpression of a StructuralAggregation is allowed to contain Aggregation operators.");
                    return result;
                }
            }

            StructuralContext structuralContext = this.BuildStructuralContext(dataProvider, currentState);
            ComputationResult nestedResult = RootExpression.Compute(dataProvider, currentState, this, new List<IExpression>(), structuralContext);

            if (!nestedResult.IsValid)
            {
                result.SetErrorState(nestedResult);

                dataProvider.SetIsValidated(currentState.VariableInstanceRef, false);
                dataProvider.SetComputedTimeValues(currentState.ModelInstanceRef, currentState.VariableInstanceRef, null, currentState);

                return result;
            }
            else if (this.IsNavigationVariable && result.ValidatedTimeDimensionality.SecondaryTimeDimension.HasTimeValue)
            {
                result.SetComputedState(nestedResult);
                result.SetErrorState(ComputationResultType.NavigationFormulaError, "Navigation Variables must not have Secondary Time Values.");

                dataProvider.SetIsValidated(currentState.VariableInstanceRef, false);
                dataProvider.SetComputedTimeValues(currentState.ModelInstanceRef, currentState.VariableInstanceRef, null, currentState);

                return result;
            }
            else
            {
                result.SetComputedState(nestedResult);

                dataProvider.SetIsValidated(currentState.VariableInstanceRef, true);
                dataProvider.SetComputedTimeValues(currentState.ModelInstanceRef, currentState.VariableInstanceRef, nestedResult.FirstComputedValue, currentState);

                return result;
            }
        }

        public ComputationResult Compute_Anonymous(IFormulaDataProvider_Anonymous dataProvider, ICurrentState currentState)
        {
            ComputationUtils.AssertComputationConditions(dataProvider, currentState.VariableInstanceRef, this);
            ComputationResult result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key);

            if (!this.MeetsAnonymousFormulaConditions(result))
            { return result; }

            var timeDimensionality = dataProvider.GetAnonymousVariableTimeDimensions(currentState.VariableTemplateRef);
            if (timeDimensionality == null)
            {
                result.SetErrorState(ComputationResultType.FormulaValidationPending, "The current Formula has not been Validated yet.");
                return result;
            }

            if (timeDimensionality.PrimaryTimeDimension.HasTimeValue)
            { currentState.NavigationPeriod = currentState.PrimaryPeriod; }
            else
            { currentState.NavigationPeriod = null; }


            foreach (IExpression expression in m_Expressions.Values)
            {
                if (expression == RootExpression)
                { continue; }

                if (expression.Operation == null)
                {
                    result.SetErrorState(ComputationResultType.NullFormula, "The Expression's Operation is not set.");
                    return result;
                }
                else if (expression.Operation.StructuralOperationType.IsAggregation())
                {
                    result.SetErrorState(ComputationResultType.NullFormula, "Only the RootExpression of a StructuralAggregation is allowed to contain Aggregation operators.");
                    return result;
                }
            }

            StructuralContext structuralContext = this.BuildAnonymousStructuralContext(dataProvider, currentState);
            ComputationResult nestedResult = RootExpression.Compute(dataProvider, currentState, this, new List<IExpression>(), structuralContext);

            if (!nestedResult.IsValid)
            {
                result.SetErrorState(nestedResult);

                dataProvider.SetComputedAnonymousVariableValue(currentState.ModelInstanceRef, currentState.VariableTemplateRef, structuralContext.FirstPoint.Value, currentState.TimeKey, null);

                return result;
            }
            else
            {
                result.SetComputedState(nestedResult);

                dataProvider.SetComputedAnonymousVariableValue(currentState.ModelInstanceRef, currentState.VariableTemplateRef, structuralContext.FirstPoint.Value, currentState.TimeKey, nestedResult.FirstComputedValue.GetValue(currentState.TimeKey));

                return result;
            }
        }
    }
}