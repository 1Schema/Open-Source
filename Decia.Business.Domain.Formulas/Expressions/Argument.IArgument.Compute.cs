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
        public ComputationResult Compute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext)
        {
            ComputationUtils.AssertComputationConditions(dataProvider, currentState.VariableInstanceRef, parentFormula, this.Key);
            ComputationResult result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key.FormulaId, this.Key.ExpressionGuid, this.Key.ArgumentIndex);

            if (this.ArgumentType == ArgumentType.DirectValue)
            { ComputeDirectValue(currentState, result); }
            else if (this.ArgumentType == ArgumentType.NestedExpression)
            { ComputeNestedExpression(dataProvider, currentState, parentFormula, parentExpressions, structuralContext, result); }
            else if (this.ArgumentType == ArgumentType.ReferencedId)
            { ComputeReferencedId(dataProvider, currentState, parentFormula, structuralContext, result); }
            else
            { throw new InvalidOperationException("Invalid ArgumentType encountered."); }

            return result;
        }

        private void ComputeDirectValue(ICurrentState currentState, ComputationResult result)
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
                StructuralPoint resultingPoint = StructuralPoint.GlobalStructuralPoint;
                IEnumerable<ModelObjectReference> referencedTemplates = new ModelObjectReference[] { };
                var defaultUnit = CompoundUnit.GetGlobalScalarUnit(currentState.ProjectGuid, currentState.RevisionNumber);

                result.SetComputedState(resultingSpace, referencedTemplates, resultingPoint, this.DirectChronometricValue, defaultUnit);
            }
        }

        private void ComputeReferencedId(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IStructuralContext structuralContext, ComputationResult result)
        {
            ModelObjectReference variableTemplateRef = this.ReferencedModelObject;

            if (!variableTemplateRef.IsVariableType())
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidId, "Currently, Arguments can only reference VariableTemplates.");
            }
            else if (variableTemplateRef.IsAnonymous())
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidId, "Currently, Anonymous Arguments are not supported.");
                return;
            }
            else if (!dataProvider.IsValid(variableTemplateRef))
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidId, "The Argument references an invalid Model object.");
            }
            else if (dataProvider.GetIsValidated(variableTemplateRef) != true)
            {
                result.SetErrorState(ComputationResultType.ArgumentReferencesInvalidValue, "The Argument references a Model object that failed Validation.");
            }
            else
            {
                bool useExtendedStructure = currentState.UseExtendedStructure;
                ModelObjectReference structuralTypeRef = dataProvider.GetStructuralType(variableTemplateRef);

                StructuralSpace resultingSpace = dataProvider.StructuralMap.GetStructuralSpace(structuralTypeRef, useExtendedStructure);
                Nullable<DeciaDataType> dataType = dataProvider.GetValidatedDataType(variableTemplateRef);
                ITimeDimensionSet timeDimensionality = dataProvider.GetComputationTimeDimensions(currentState.ModelInstanceRef, variableTemplateRef);
                CompoundUnit unit = dataProvider.GetValidatedUnit(variableTemplateRef);

                var childVarInstanceRefs = dataProvider.GetRelatedChildVariableInstanceReferences(currentState.ModelInstanceRef, currentState.NavigationPeriod, variableTemplateRef, structuralContext);
                var isMainRef = ModelObjectReference.DimensionalComparer.Equals(currentState.StructuralTypeRef, structuralTypeRef);
                var isUniquelyAccessibleForBasicStructure = (dataProvider.StructuralMap.IsAccessible(currentState.StructuralTypeRef, structuralTypeRef, false) && dataProvider.StructuralMap.IsUnique(currentState.StructuralTypeRef, structuralTypeRef, false));
                if (isMainRef)
                {
                    var structuralInstanceRef = currentState.StructuralInstanceRef;
                    var childVarInstanceRef = dataProvider.GetChildVariableInstanceReference(currentState.ModelInstanceRef, variableTemplateRef, structuralInstanceRef);
                    childVarInstanceRefs = childVarInstanceRefs.ToDictionary(x => x.Key, x => childVarInstanceRef);
                }
                else if (isUniquelyAccessibleForBasicStructure)
                {
                    var mainPoint = dataProvider.StructuralMap.GetStructuralPoint(currentState.ModelInstanceRef, currentState.StructuralInstanceRef, useExtendedStructure);
                    var structuralInstanceRef = dataProvider.StructuralMap.GetRelatedStructuralInstances(currentState.ModelInstanceRef, currentState.NavigationPeriod, structuralTypeRef, new StructuralPoint[] { mainPoint }, useExtendedStructure).First().Value.First().Key;
                    var childVarInstanceRef = dataProvider.GetChildVariableInstanceReference(currentState.ModelInstanceRef, variableTemplateRef, structuralInstanceRef);
                    childVarInstanceRefs = childVarInstanceRefs.ToDictionary(x => x.Key, x => childVarInstanceRef);
                }

                Dictionary<StructuralPoint, ChronometricValue> childVarInstanceValues = childVarInstanceRefs.ToDictionary(kvp => kvp.Key, kvp => dataProvider.GetComputedTimeMatrix(currentState.ModelInstanceRef, kvp.Value));

                if (!dataType.HasValue)
                {
                    result.SetErrorState(ComputationResultType.ArgumentValidationError, "The Validated Data Type is not set.");
                }
                else if (timeDimensionality == null)
                {
                    result.SetErrorState(ComputationResultType.ArgumentValidationError, "The Validated Time Dimensionality is not set.");
                }
                else if (childVarInstanceValues.ContainsValue(null))
                {
                    result.SetErrorState(ComputationResultType.ArgumentValueIsNull, "The Referenced Time Matrix is null.");
                }
                else
                {
                    result.SetComputedState(resultingSpace, variableTemplateRef.ToEnumerable(), dataType.Value, childVarInstanceValues, unit);
                }
            }
        }

        private void ComputeNestedExpression(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext, ComputationResult result)
        {
            IExpression nestedExpression = parentFormula.GetExpression(this.NestedExpressionId);
            ComputationResult nestedResult = nestedExpression.Compute(dataProvider, currentState, parentFormula, parentExpressions, structuralContext);

            if (!nestedResult.IsValid)
            {
                result.SetErrorState(nestedResult);
            }
            else
            {
                result.SetComputedState(nestedResult);
            }
        }
    }
}