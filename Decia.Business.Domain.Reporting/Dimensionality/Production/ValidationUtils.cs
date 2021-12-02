using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public static class ValidationUtils
    {
        public static bool ValidateSpaceResultForGroup(IReportingDataProvider dataProvider, IRenderingState renderingState, IEnumerable<ModelObjectReference> structuralTypeRefs, SpaceResult spaceResult)
        {
            var dimensionResults = spaceResult.DimensionResults;

            var structuralValues = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);
            var variableValues = new Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, ModelObjectReference>>(ModelObjectReference.DimensionalComparer);
            var timeKey = MultiTimePeriodKey.DimensionlessTimeKey;

            foreach (var dimensionIndex in dimensionResults.Keys)
            {
                var dimensionResult = dimensionResults[dimensionIndex];

                if (dimensionResult.DimensionType == DimensionType.Structure)
                {
                    var structuralTypeRef = dimensionResult.DimensionRef;
                    var structuralInstanceRef = dimensionResult.StructuralInstanceRef.Value;

                    structuralValues[structuralTypeRef] = structuralInstanceRef;

                    var variableTemplateRef = dimensionResult.ParentEnumerator.NameVariableRef.Value;
                    var variableInstanceRef = dimensionResult.VariableInstanceRef.Value;

                    variableValues[structuralTypeRef] = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);
                    variableValues[structuralTypeRef][variableTemplateRef] = variableInstanceRef;
                }
                else if (dimensionResult.DimensionType == DimensionType.Time)
                {
                    var timeDimensionType = dimensionResult.TimeDimensionType.Value;
                    var timePeriod = dimensionResult.PeriodToDisplay.Value;

                    if (timeDimensionType == TimeDimensionType.Primary)
                    { timeKey = new MultiTimePeriodKey(timePeriod, timeKey.NullableSecondaryTimePeriod); }
                    else if (timeDimensionType == TimeDimensionType.Secondary)
                    { timeKey = new MultiTimePeriodKey(timeKey.NullablePrimaryTimePeriod, timePeriod); }
                    else
                    { throw new InvalidOperationException(); }
                }
                else
                { throw new InvalidOperationException(); }
            }

            return ValidateSpaceResultForGroup(dataProvider, renderingState, structuralTypeRefs, structuralValues, timeKey);
        }

        public static bool ValidateSpaceResultForGroup(IReportingDataProvider dataProvider, IRenderingState renderingState, IEnumerable<ModelObjectReference> structuralTypeRefs, Dictionary<ModelObjectReference, ModelObjectReference> structuralBindings, MultiTimePeriodKey timeBindings)
        {
            var reportState = renderingState.ReportState;

            foreach (var structuralTypeRef in structuralTypeRefs)
            {
                var structuralSpace = dataProvider.StructuralMap.GetStructuralSpace(structuralTypeRef, reportState.UseExtendedStructure);

                var structuralCoordinates = new List<StructuralCoordinate>();
                foreach (var structuralDimension in structuralSpace.Dimensions)
                {
                    var relevantTypeRef = structuralDimension.EntityTypeRefWithDimNum;

                    if (!structuralBindings.ContainsKey(relevantTypeRef))
                    { continue; }

                    var relevantInstanceRef = structuralBindings[relevantTypeRef];
                    var structuralCoordinate = new StructuralCoordinate(relevantTypeRef.ModelObjectIdAsInt, relevantTypeRef.NonNullAlternateDimensionNumber, relevantInstanceRef.ModelObjectId, StructuralDimensionType.NotSet);
                    structuralCoordinates.Add(structuralCoordinate);
                }

                var structuralPoint = new StructuralPoint(structuralCoordinates);
                var relatedStructuralInstances = dataProvider.StructuralMap.GetRelatedStructuralInstances(reportState.ModelInstanceRef, timeBindings.NullablePrimaryTimePeriod, structuralTypeRef, new StructuralPoint[] { structuralPoint }, reportState.UseExtendedStructure);

                if (relatedStructuralInstances[structuralPoint].Count < 1)
                { return false; }
            }
            return true;
        }
    }
}