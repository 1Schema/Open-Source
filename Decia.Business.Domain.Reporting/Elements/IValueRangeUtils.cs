using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting
{
    public static class IValueRangeUtils
    {
        #region Methods to Get IsValid

        public static bool GetValueRange_IsValid(this IReportingDataProvider dataProvider, IRenderingState renderingState, ModelObjectReference variableTemplateRef, StructuralSpace structuralSpace)
        {
            var reportState = renderingState.ReportState;

            var structuralTypeRef = dataProvider.DependencyMap.GetStructuralType(variableTemplateRef);
            if (structuralTypeRef.NonNullAlternateDimensionNumber != variableTemplateRef.NonNullAlternateDimensionNumber)
            { throw new InvalidOperationException("The AlternateDimensionNumber was not properly maintained."); }

            var structuralTypeBaseSpace = dataProvider.StructuralMap.GetBaseStructuralSpace(structuralTypeRef);

            var isValid = structuralTypeBaseSpace.IsRelatedAndMoreGeneral(dataProvider.StructuralMap, structuralSpace, false);
            return isValid;
        }

        public static bool GetValueRange_IsValid(this IReportingDataProvider dataProvider, IRenderingState renderingState, FormulaId formulaId, ModelObjectReference rootStructuralTypeRef, StructuralSpace structuralSpace)
        {
            var anonymousFormula = dataProvider.GetAnonymousFormula(formulaId);
            var anonVariableTemplateRef = anonymousFormula.VariableOriginRef.Value;

            var isValid = dataProvider.GetAnonymousVariableIsValidated(anonVariableTemplateRef);
            if (!isValid.HasValue)
            { throw new InvalidOperationException("Anonymous Variable Validation has not yet occurred."); }

            return isValid.Value;
        }

        #endregion

        #region Methods to Get Value

        public static DynamicValue GetValueRange_Value(this IReportingDataProvider dataProvider, IRenderingState renderingState, ModelObjectReference variableTemplateRef, ModelObjectReference modelInstanceRef, StructuralPoint structuralPoint, MultiTimePeriodKey timeKey)
        {
            var reportState = renderingState.ReportState;

            var structuralTypeRef = dataProvider.DependencyMap.GetStructuralType(variableTemplateRef);
            var structuralInstanceRefs = dataProvider.StructuralMap.GetStructuralInstancesForType(modelInstanceRef, structuralTypeRef);

            if (structuralInstanceRefs.Where(x => x.NonNullAlternateDimensionNumber != variableTemplateRef.NonNullAlternateDimensionNumber).Count() > 0)
            { throw new InvalidOperationException("The AlternateDimensionNumber was not properly maintained."); }

            var structuralInstanceRefsByBasePoint = structuralInstanceRefs.ToDictionary(x => dataProvider.StructuralMap.GetBaseStructuralPoint(modelInstanceRef, x), x => x);
            var relevantStructuralInstances = structuralInstanceRefsByBasePoint.Where(x => x.Key.IsRelatedAndMoreGeneral(dataProvider.StructuralMap, modelInstanceRef, structuralPoint, false)).ToDictionary(x => x.Key, x => x.Value);

            if (relevantStructuralInstances.Count != 1)
            { throw new InvalidOperationException("No matching Variable value could be found."); }

            var structuralInstanceRef = relevantStructuralInstances.First().Value;
            var variableInstanceRef = dataProvider.GetChildVariableInstanceReference(modelInstanceRef, variableTemplateRef, structuralInstanceRef);

            var timeMatrix = dataProvider.GetComputedTimeMatrix(reportState.ModelInstanceRef, variableInstanceRef);
            var outputValue = timeMatrix.GetValue(renderingState.CurrentTimeKey);
            return outputValue;
        }

        public static DynamicValue GetValueRange_Value(this IReportingDataProvider dataProvider, IRenderingState renderingState, FormulaId formulaId, ModelObjectReference modelInstanceRef, ModelObjectReference rootStructuralInstanceRef, StructuralPoint structuralPoint, MultiTimePeriodKey timeKey)
        {
            var reportState = renderingState.ReportState;

            var formulaProcessingEngine = renderingState.FormulaProcessingEngine;
            var anonymousFormula = dataProvider.GetAnonymousFormula(formulaId);
            var anonVariableTemplateRef = anonymousFormula.VariableOriginRef.Value;

            var computationResult = formulaProcessingEngine.ComputeAnonymousValue(modelInstanceRef, anonVariableTemplateRef, rootStructuralInstanceRef, structuralPoint, timeKey);
            if (!computationResult.IsValid)
            { return null; }

            var computedValue = dataProvider.GetComputedAnonymousVariableValue(modelInstanceRef, anonVariableTemplateRef, structuralPoint, timeKey);
            return computedValue;
        }

        #endregion
    }
}