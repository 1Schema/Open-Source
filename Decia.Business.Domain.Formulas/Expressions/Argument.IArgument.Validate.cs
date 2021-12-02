using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Argument
    {
        public ComputationResult Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext)
        {
            ComputationUtils.AssertValidationConditions(dataProvider, currentState.VariableTemplateRef, parentFormula, this.Key);
            ComputationResult result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key.FormulaId, this.Key.ExpressionGuid, this.Key.ArgumentIndex);

            if (this.ArgumentType == ArgumentType.DirectValue)
            { ValidateDirectValue(currentState, result); }
            else if (this.ArgumentType == ArgumentType.ReferencedId)
            { ValidateReferencedId(dataProvider, currentState, result); }
            else if (this.ArgumentType == ArgumentType.NestedExpression)
            { ValidateNestedExpression(dataProvider, currentState, parentFormula, parentExpressions, structuralContext, result); }
            else
            { throw new InvalidOperationException("Invalid ArgumentType encountered."); }

            return result;
        }

        private void ValidateDirectValue(ICurrentState currentState, ComputationResult result)
        {
            if (this.DirectValue == DynamicValue.NullInstanceAsObject)
            {
                result.SetErrorState(ComputationResultType.ArgumentValueIsException, "DirectValue instance must not be null.");
            }
            else if (!this.DirectValue.IsValid)
            {
                result.SetErrorState(ComputationResultType.ArgumentValueIsException, "DirectValue must be in a valid state.");
            }
            else if (this.DirectValue.IsNull)
            {
                result.SetErrorState(ComputationResultType.ArgumentValueIsNull, "DirectValue stored value must not be null.");
            }
            else
            {
                StructuralSpace resultingSpace = StructuralSpace.GlobalStructuralSpace;
                IEnumerable<ModelObjectReference> referencedTemplates = new ModelObjectReference[] { };
                var defaultUnit = CompoundUnit.GetGlobalScalarUnit(currentState.ProjectGuid, currentState.RevisionNumber);

                result.SetValidatedState(resultingSpace, referencedTemplates, this.DirectValue.DataType, TimeDimensionSet.EmptyTimeDimensionSet, defaultUnit);
            }
        }

        private void ValidateReferencedId(IFormulaDataProvider dataProvider, ICurrentState currentState, ComputationResult result)
        {
            ModelObjectReference variableTemplateRef = this.ReferencedModelObject;

            if (!variableTemplateRef.IsVariableType())
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidId, "Currently, Arguments can only reference VariableTemplates.");
                return;
            }
            else if (variableTemplateRef.IsAnonymous())
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidId, "Currently, Anonymous Arguments are not supported.");
                return;
            }
            else if (!dataProvider.IsValid(variableTemplateRef))
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidId, "The Argument references an invalid Model object.");
                return;
            }


            bool useExtendedStructure = currentState.UseExtendedStructure;
            ModelObjectReference structuralTypeRef = dataProvider.GetStructuralType(variableTemplateRef);
            StructuralSpace resultingSpace = dataProvider.StructuralMap.GetStructuralSpace(structuralTypeRef, useExtendedStructure);

            bool useValidatedValues = (dataProvider.GetIsValidated(variableTemplateRef) == true);
            bool useAssessedValues = false;
            if (!useValidatedValues)
            {
                if (ParentOperation.EvaluationType == EvaluationType.PreProcessing)
                {
                    useAssessedValues = true;
                }
                else if (currentState.HasParentComputationGroup)
                {
                    if (currentState.ParentComputationGroup.HasCycle)
                    { useAssessedValues = currentState.ParentComputationGroup.TimeOrderedNodes.Contains(variableTemplateRef); }
                }
            }

            if (useValidatedValues)
            {
                Nullable<DeciaDataType> dataType = dataProvider.GetValidatedDataType(variableTemplateRef);
                ITimeDimensionSet timeDimensionality = dataProvider.GetValidatedTimeDimensions(variableTemplateRef);
                CompoundUnit unit = dataProvider.GetValidatedUnit(variableTemplateRef);

                if (!dataType.HasValue)
                { result.SetErrorState(ComputationResultType.ArgumentValidationError, "The Validated Data Type is not set."); }
                else if (timeDimensionality == null)
                { result.SetErrorState(ComputationResultType.ArgumentValidationError, "The Validated Time Dimensionality is not set."); }
                else
                { result.SetValidatedState(resultingSpace, variableTemplateRef.ToEnumerable(), dataType.Value, timeDimensionality, unit); }
            }
            else if (useAssessedValues)
            {
                Nullable<DeciaDataType> dataType = dataProvider.GetAssessedDataType(variableTemplateRef);
                ITimeDimensionSet timeDimensionality = dataProvider.GetAssessedTimeDimensions(variableTemplateRef);
                CompoundUnit unit = dataProvider.GetAssessedUnit(variableTemplateRef);

                if (!dataType.HasValue)
                { result.SetErrorState(ComputationResultType.ArgumentValidationError, "The Assessed Data Type is not set."); }
                else if (timeDimensionality == null)
                { result.SetErrorState(ComputationResultType.ArgumentValidationError, "The Assessed Time Dimensionality is not set."); }
                else
                { result.SetValidatedState(resultingSpace, variableTemplateRef.ToEnumerable(), dataType.Value, timeDimensionality, unit); }
            }
            else
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidValue, "The Argument references a Model object that failed Validation.");
            }
        }

        private void ValidateNestedExpression(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext, ComputationResult result)
        {
            IExpression nestedExpression = parentFormula.GetExpression(this.NestedExpressionId);
            ComputationResult nestedResult = nestedExpression.Validate(dataProvider, currentState, parentFormula, parentExpressions, structuralContext);

            if (!nestedResult.IsValid)
            {
                result.SetErrorState(nestedResult);
            }
            else
            {
                result.SetValidatedState(nestedResult);
            }
        }
    }
}