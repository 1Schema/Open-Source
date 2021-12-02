using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Structure;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Dimensionality;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public partial class Cell
    {
        public override RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            ICurrentState reportState = renderingState.ReportState;
            DimensionalRepeatGroup parentRepeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.ParentElementOrReportId];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.Validation);

            var additionalStructuralTypeRefs = new List<ModelObjectReference>();
            additionalStructuralTypeRefs.AddRange(parentRepeatGroup.AdditionalStructuralTypeRefs);

            if (this.OutputValueType == OutputValueType.DirectValue)
            { /* do nothing */ }
            else if (this.OutputValueType == OutputValueType.ReferencedId)
            {
                ModelObjectReference structuralTypeRef, variableTemplateRef;
                bool areRefsValid;

                var isVarTempRef = IsObjectRef_VariableTemplate(this, dataProvider, out structuralTypeRef, out variableTemplateRef, out areRefsValid);
                if (!isVarTempRef)
                { throw new InvalidOperationException("The specified reference is not supported."); }

                if (!areRefsValid)
                {
                    result.SetErrorState(RenderingResultType.VariableReferenceIsInvalid, "The current Cell's reference is not valid.");
                    return result;
                }

                additionalStructuralTypeRefs.Add(structuralTypeRef);
            }
            else if (this.OutputValueType == OutputValueType.AnonymousFormula)
            {
                var formula = dataProvider.GetAnonymousFormula(this.ValueFormulaId);

                if (formula == null)
                {
                    result.SetErrorState(RenderingResultType.AnonymousFormulaMissing, "The current Cell's Anonymous Formula could not be found.");
                    return result;
                }

                var refArgs = formula.GetArguments_ForArgumentType(ArgumentType.ReferencedId, null);
                foreach (var refArg in refArgs)
                {
                    var variableTemplateRef = refArg.ReferencedModelObject;

                    if (!dataProvider.DependencyMap.VariableTemplateRefs.Contains(variableTemplateRef))
                    {
                        result.SetErrorState(RenderingResultType.AnonymousFormulaIsInvalid, "The current Cell's Formula is not valid.");
                        return result;
                    }

                    if (refArg.ParentOperation.EvaluationType == EvaluationType.PreProcessing)
                    { continue; }

                    var structuralTypeRef = dataProvider.GetStructuralType(variableTemplateRef);
                    additionalStructuralTypeRefs.Add(structuralTypeRef);
                }
            }
            else
            { throw new InvalidOperationException("Invalid OutputValueType setting encountered."); }


            var structuralContexts = DimensioningUtils.BuildStructuralContexts(dataProvider, reportState, additionalStructuralTypeRefs);

            if (structuralContexts.Count != 1)
            {
                result.SetErrorState(RenderingResultType.StructuralReferenceIsInvalid, "The current Report Element does not have a valid Structural Context.");
                return result;
            }

            var structuralContext = structuralContexts.First().Value;

            var parentSpace = parentRepeatGroup.DesignContext.ResultingSpace;
            var cellSpace = structuralContext.ResultingSpace;
            var isValid = cellSpace.IsRelatedAndMoreGeneral(dataProvider.StructuralMap, parentSpace, reportState.UseExtendedStructure, true);

            if (!isValid)
            { throw new InvalidOperationException("The Cell's DataBinding is not valid for the Report."); }

            parentRepeatGroup.AddElementToGroup(this);

            if (this.OutputValueType == OutputValueType.AnonymousFormula)
            {
                var localRepeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.Key];
                var textFormula = dataProvider.GetAnonymousFormula(this.ValueFormulaId);
                var dataType = DeciaDataType.Text;

                dataProvider.AddAnonymousVariable_ReportingState(this.Key, textFormula, reportState.StructuralTypeRef, localRepeatGroup.DesignContext.ResultingSpace, localRepeatGroup.RelevantTimeDimensions, dataType);
            }

            result.SetValidatedState();
            return result;
        }

        public override RenderingResult RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            var result = ReportElementUtils.RenderRangeForDesign(this, dataProvider, renderingState, elementTree, layoutResults, parentRenderingKey);
            return result;
        }

        public override RenderingResult RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            var result = ReportElementUtils.RenderRangeForProduction(this, dataProvider, renderingState, elementTree, layoutResults, parentRenderingKey);
            return result;
        }
    }
}