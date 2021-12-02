using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.Formulas.Operations;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula
    {
        public ComputationResult Validate(IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            bool requiresComputationByPeriod;
            return Validate(dataProvider, currentState, out requiresComputationByPeriod);
        }

        public ComputationResult Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, out bool requiresComputationByPeriod)
        {
            if (currentState.ProcessingType == ProcessingType.Normal)
            { return Validate_Normal(dataProvider, currentState, out requiresComputationByPeriod); }
            else if (currentState.ProcessingType == ProcessingType.Anonymous)
            { return Validate_Anonymous((dataProvider as IFormulaDataProvider_Anonymous), currentState, out requiresComputationByPeriod); }
            else
            { throw new InvalidOperationException("Invalid ProcessingType encountered."); }
        }

        public ComputationResult Validate_Normal(IFormulaDataProvider dataProvider, ICurrentState currentState, out bool requiresComputationByPeriod)
        {
            ComputationUtils.AssertValidationConditions(dataProvider, currentState.VariableTemplateRef, this);
            ComputationResult result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key);
            requiresComputationByPeriod = true;

            if (this.HasDeletedRefs)
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidId, "Arguments reference deleted Variables.");
                return result;
            }
            if ((RootExpression == null) || (RootExpression.OperationId == OperationCatalog.GetOperationId<NoOpOperation>()))
            {
                var assessedStructuralSpace = dataProvider.StructuralMap.GetStructuralSpace(currentState.StructuralTypeRef, true);
                var assessedDataType = dataProvider.GetAssessedDataType(currentState.VariableTemplateRef);
                var assessedTimeDimensionality = dataProvider.GetAssessedTimeDimensions(currentState.VariableTemplateRef);
                var assessedUnit = dataProvider.GetAssessedUnit(currentState.VariableTemplateRef);

                result.SetValidatedState(assessedStructuralSpace, new List<ModelObjectReference>(), assessedDataType, assessedTimeDimensionality);

                dataProvider.SetIsValidated(currentState.VariableTemplateRef, true);
                dataProvider.SetValidatedDataType(currentState.VariableTemplateRef, assessedDataType);
                dataProvider.SetValidatedTimeDimensions(currentState.VariableTemplateRef, assessedTimeDimensionality);
                dataProvider.SetValidatedUnit(currentState.VariableTemplateRef, assessedUnit);

                return result;
            }

            if (!this.MeetsMinimalExpressionConditions(result))
            { return result; }
            if (!this.MeetsStructuralAggregationConditions(result))
            { return result; }
            if (!this.MeetsTimeAggregationConditions(result))
            { return result; }

            var hasNonRootTimeAggregation = false;

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
                else if (expression.Operation.TimeOperationType.IsAggregation())
                {
                    hasNonRootTimeAggregation = true;
                }
            }

            var structuralContext = this.BuildStructuralContext(dataProvider, currentState);
            var structuralSpace = structuralContext.ResultingSpace;

            bool isRequirementFromStructuralContext;
            requiresComputationByPeriod = this.RequiresComputationByPeriod(dataProvider, currentState, structuralSpace, out isRequirementFromStructuralContext);

            if (requiresComputationByPeriod && hasNonRootTimeAggregation)
            {
                result.SetErrorState(ComputationResultType.NullFormula, "Only the RootExpression of a TimeAggregation is allowed to contain Aggregation operators that convert between TimePeriods.");
                return result;
            }

            bool atLeastOneArgRequiresTimeNavigationVariable = CheckIfArgsRequireTimeNavigation(dataProvider, currentState);

            if (atLeastOneArgRequiresTimeNavigationVariable)
            {
                if (!requiresComputationByPeriod)
                {
                    result.SetErrorState(ComputationResultType.NavigationFormulaError, "If any Navigation Variable used in Structural Context has a Primary Time Dimension, computation must be performed by Period.");
                    return result;
                }
            }
            if (atLeastOneArgRequiresTimeNavigationVariable && this.IsTimeAggregation)
            {
                result.SetErrorState(ComputationResultType.NavigationFormulaError, "If any Navigation Variable used in Structural Context has a Primary Time Dimension, Time Aggregation is disabled in the current Variable.");
                return result;
            }

            ComputationResult nestedResult = RootExpression.Validate(dataProvider, currentState, this, new List<IExpression>(), structuralContext);

            if (atLeastOneArgRequiresTimeNavigationVariable && !nestedResult.ValidatedTimeDimensionality.PrimaryTimeDimension.HasTimeValue)
            {
                result.SetErrorState(ComputationResultType.NavigationFormulaError, "If any Navigation Variable used in Structural Context has a Primary Time Dimension, the current Variable must also have a Primary Time Dimension.");
                return result;
            }

            if (this.IsNavigationVariable && result.ValidatedTimeDimensionality.SecondaryTimeDimension.HasTimeValue)
            {
                result.SetValidatedState(nestedResult);
                result.SetErrorState(ComputationResultType.NavigationFormulaError, "Navigation Variables must not have Secondary Time Values.");
                return result;
            }


            if (!nestedResult.IsValid)
            {
                result.SetErrorState(nestedResult);

                dataProvider.SetIsValidated(currentState.VariableTemplateRef, false);
                dataProvider.SetValidatedDataType(currentState.VariableTemplateRef, null);
                dataProvider.SetValidatedTimeDimensions(currentState.VariableTemplateRef, null);
                dataProvider.SetValidatedUnit(currentState.VariableTemplateRef, null);

                return result;
            }
            else
            {
                var assessedTimeDimensionality = dataProvider.GetAssessedTimeDimensions(currentState.VariableTemplateRef);
                var validatedTimDimensionality = result.ValidatedTimeDimensionality;
                var timeComparison = validatedTimDimensionality.CompareTimeTo(assessedTimeDimensionality);

                if ((timeComparison == TimeComparisonResult.NotComparable) || (timeComparison == TimeComparisonResult.ThisIsMoreGranular))
                {
                    result.SetErrorState(ComputationResultType.FormulaIsInvalid, "The Validated Time Dimensionality is not valid.");
                    return result;
                }

                result.SetValidatedState(nestedResult);

                dataProvider.SetIsValidated(currentState.VariableTemplateRef, true);
                dataProvider.SetValidatedDataType(currentState.VariableTemplateRef, nestedResult.ValidatedDataType);
                dataProvider.SetValidatedTimeDimensions(currentState.VariableTemplateRef, nestedResult.ValidatedTimeDimensionality);
                dataProvider.SetValidatedUnit(currentState.VariableTemplateRef, nestedResult.ValidatedUnit);

                return result;
            }
        }

        public ComputationResult Validate_Anonymous(IFormulaDataProvider_Anonymous dataProvider, ICurrentState currentState, out bool requiresComputationByPeriod)
        {
            ComputationUtils.AssertValidationConditions(dataProvider, currentState.VariableTemplateRef, this);
            ComputationResult result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key);
            requiresComputationByPeriod = true;

            if (this.HasDeletedRefs)
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidId, "Arguments reference deleted Variables.");
                return result;
            }
            if ((RootExpression == null) || (RootExpression.OperationId == OperationCatalog.GetOperationId<NoOpOperation>()))
            {
                var assessedStructuralSpace = currentState.StructuralSpace;
                var assessedDataType = dataProvider.GetAnonymousVariableDataType(currentState.VariableTemplateRef);
                var assessedTimeDimensionality = dataProvider.GetAnonymousVariableTimeDimensions(currentState.VariableTemplateRef);

                result.SetValidatedState(assessedStructuralSpace, new List<ModelObjectReference>(), assessedDataType, assessedTimeDimensionality);
                dataProvider.SetAnonymousVariableIsValidated(currentState.VariableTemplateRef, true);

                return result;
            }

            if (!this.MeetsAnonymousFormulaConditions(result))
            { return result; }

            foreach (IExpression expression in m_Expressions.Values)
            {
                if (expression == RootExpression)
                { continue; }

                if (expression.Operation == null)
                {
                    result.SetErrorState(ComputationResultType.NullFormula, "The Expression's Operation is not set.");
                    return result;
                }
            }

            var structuralContext = this.BuildAnonymousStructuralContext(dataProvider, currentState);
            var structuralSpace = structuralContext.ResultingSpace;

            bool isRequirementFromStructuralContext;

            ComputationResult nestedResult = RootExpression.Validate(dataProvider, currentState, this, new List<IExpression>(), structuralContext);


            if (!nestedResult.IsValid)
            {
                result.SetErrorState(nestedResult);
                dataProvider.SetAnonymousVariableIsValidated(currentState.VariableTemplateRef, false);

                return result;
            }
            else
            {
                result.SetValidatedState(nestedResult);
                dataProvider.SetAnonymousVariableIsValidated(currentState.VariableTemplateRef, true);

                return result;
            }
        }

        private bool CheckIfArgsRequireTimeNavigation(IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            if (dataProvider.GetVariableType(currentState.VariableTemplateRef) == VariableType.Input)
            { return false; }

            foreach (IArgument argument in this.GetArguments_ContainingReferences(EvaluationType.Processing))
            {
                ModelObjectReference relatedVariableTemplateRef = argument.ReferencedModelObject;
                ModelObjectReference relatedStructuralTypeRef = dataProvider.GetStructuralType(relatedVariableTemplateRef);

                if (relatedStructuralTypeRef == ModelObjectReference.GlobalTypeReference)
                { continue; }
                if (relatedStructuralTypeRef == currentState.StructuralTypeRef)
                { continue; }

                Nullable<StructuralSpace> spaceOfRelation = dataProvider.StructuralMap.GetRelativeStructuralSpace(currentState.StructuralTypeRef, relatedStructuralTypeRef, currentState.UseExtendedStructure);

                if (!spaceOfRelation.HasValue)
                { throw new InvalidOperationException("A referenced VariableTemplate is not accessible."); }

                StructuralSpace originalSpaceOfRelation = spaceOfRelation.Value;
                StructuralSpace reducedSpaceOfRelation = spaceOfRelation.Value.GetReduced(dataProvider.StructuralMap, currentState.UseExtendedStructure);

                foreach (var removedDimension in originalSpaceOfRelation.Dimensions)
                {
                    if (reducedSpaceOfRelation.DimensionsById.ContainsKey(removedDimension.DimensionId))
                    { continue; }

                    bool isReachableUsingReducedSpace = dataProvider.StructuralMap.IsDirectlyAccessibleUsingSpace(currentState.StructuralTypeRef, relatedStructuralTypeRef, reducedSpaceOfRelation, currentState.UseExtendedStructure);
                    if (isReachableUsingReducedSpace)
                    { continue; }

                    if (removedDimension.DimensionType.IsReferenceMember() && removedDimension.UsesTimeDimension)
                    { return true; }
                }

                if (reducedSpaceOfRelation.Dimensions.Where(d => d.UsesTimeDimension).ToList().Count > 0)
                { return true; }
            }
            return false;
        }
    }
}