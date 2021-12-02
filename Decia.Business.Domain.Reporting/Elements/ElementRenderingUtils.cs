using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting
{
    public static class ElementRenderingUtils
    {
        #region Dynamic Values

        public const string DynamicVariableValue = "#Var";
        public const string DynamicUnitValue = "#Unit";
        public const string DynamicDimensionPrefix = "#Dim";

        public static string ApplyDynamicValues(IReportingDataProvider dataProvider, ICurrentState reportState, Dictionary<ModelObjectReference, ModelObjectReference> structuralBindings, MultiTimePeriodKey timeBindings, string formulaValue)
        {
            if (formulaValue.Contains(DynamicVariableValue))
            { throw new InvalidOperationException("Cannot use Variable Name in dynamic processing."); }
            if (formulaValue.Contains(DynamicUnitValue))
            { throw new InvalidOperationException("Cannot use Variable Unit in dynamic processing."); }

            return ApplyDynamicValues(dataProvider, reportState, structuralBindings, timeBindings, ModelObjectReference.GlobalTypeReference, string.Empty, formulaValue);
        }

        public static string ApplyDynamicValues(IReportingDataProvider dataProvider, ICurrentState reportState, Dictionary<ModelObjectReference, ModelObjectReference> structuralBindings, MultiTimePeriodKey timeBindings, ModelObjectReference variableTemplateRef, string variableTemplateName, string formulaValue)
        {
            var result = formulaValue;

            if (result.Contains(DynamicVariableValue))
            {
                result = result.Replace(DynamicVariableValue, variableTemplateName);
            }

            if (result.Contains(DynamicUnitValue))
            {
                var compoundUnit = dataProvider.GetValidatedUnit(variableTemplateRef);
                var unitName = compoundUnit.ToString();

                throw new InvalidOperationException("Currently, text rendering of Compount Units is not implemented.");
            }

            var globalDimFlag = DynamicDimensionPrefix + 0;
            if (result.Contains(globalDimFlag))
            {
                var dimStructuralTypeRef = ModelObjectReference.GlobalTypeReference;
                var dimStructuralInstanceRef = dataProvider.StructuralMap.GetGlobalInstanceRef(reportState.ModelInstanceRef);

                var dimNameVariableTemplateRef = dataProvider.DependencyMap.GetNameVariableTemplate(dimStructuralTypeRef);
                var dimNameVariableInstanceRef = dataProvider.GetChildVariableInstanceReference(reportState.ModelInstanceRef, dimNameVariableTemplateRef, dimStructuralInstanceRef);

                var dimNameTimeMatrix = dataProvider.GetComputedTimeMatrix(reportState.ModelInstanceRef, dimNameVariableInstanceRef);
                var dimName = dimNameTimeMatrix.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<string>();

                result = result.Replace(globalDimFlag, dimName);
            }

            int dimCount = 1;
            foreach (var dimStructuralTypeRef in structuralBindings.Keys)
            {
                if (dimStructuralTypeRef == ModelObjectReference.GlobalTypeReference)
                { continue; }

                var dimFlag = DynamicDimensionPrefix + dimCount;

                if (result.Contains(dimFlag))
                {
                    var dimStructuralInstanceRef = structuralBindings[dimStructuralTypeRef];

                    var dimNameVariableTemplateRef = dataProvider.DependencyMap.GetNameVariableTemplate(dimStructuralTypeRef);
                    var dimNameVariableInstanceRef = dataProvider.GetChildVariableInstanceReference(reportState.ModelInstanceRef, dimNameVariableTemplateRef, dimStructuralInstanceRef);

                    var dimNameTimeMatrix = dataProvider.GetComputedTimeMatrix(reportState.ModelInstanceRef, dimNameVariableInstanceRef);
                    var dimName = dimNameTimeMatrix.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<string>();

                    result = result.Replace(dimFlag, dimName);
                }
                dimCount++;
            }
            return result;
        }

        #endregion

        #region Size Validation

        public static void SetMinSizeToShowValues(IElementLayout elementLayout, Dimension requiredDimension)
        {
            var requiredLayout = elementLayout.GetDimensionLayout(requiredDimension);

            if (!requiredLayout.MinRangeSizeInCells_HasValue)
            { requiredLayout.MinRangeSizeInCells_Value = 1; }
            else if (requiredLayout.MinRangeSizeInCells_Value < 1)
            { requiredLayout.MinRangeSizeInCells_Value = 1; }
        }

        #endregion
    }
}