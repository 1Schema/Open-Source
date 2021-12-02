using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Structure;

namespace Decia.Business.Domain.Reporting.Dimensionality
{
    public static class DimensioningUtils
    {
        public static SortedDictionary<TimePeriod, IStructuralContext> BuildStructuralContexts(IReportingDataProvider dataProvider, ICurrentState reportState)
        {
            var additionalStructuralTypeRefs = new List<ModelObjectReference>();
            return BuildStructuralContexts(dataProvider, reportState, additionalStructuralTypeRefs);
        }

        public static SortedDictionary<TimePeriod, IStructuralContext> BuildStructuralContexts(IReportingDataProvider dataProvider, ICurrentState reportState, IEnumerable<ModelObjectReference> additionalStructuralTypeRefs)
        {
            var mainTypeRef = reportState.StructuralTypeRef;
            var requiresPoints = reportState.NullableModelInstanceRef.HasValue;
            bool useExtendedStructure = reportState.UseExtendedStructure;
            bool isSpaceUnique = false;

            var result = new SortedDictionary<TimePeriod, IStructuralContext>();
            var startDate = reportState.ModelStartDate;
            var endDate = reportState.ModelEndDate;
            var timeframe = new TimePeriod(startDate, endDate);

            StructuralSpace expectedSpace = dataProvider.StructuralMap.GetStructuralSpace(reportState.StructuralTypeRef, useExtendedStructure);

            List<TimePeriod> navigationPeriods = null;
            SortedDictionary<TimePeriod, StructuralPoint> expectedPointsByPeriod = null;
            if (requiresPoints)
            {
                navigationPeriods = dataProvider.StructuralMap.GetDefinedPeriodsForExtendedStructure(reportState.ModelInstanceRef).ToList();
                navigationPeriods = navigationPeriods.Where(np => np.Overlaps(timeframe)).ToList();

                expectedPointsByPeriod = new SortedDictionary<TimePeriod, StructuralPoint>();

                foreach (var navigationPeriod in navigationPeriods)
                {
                    var expectedPoint = dataProvider.StructuralMap.GetStructuralPoint(reportState.ModelInstanceRef, navigationPeriod, reportState.StructuralInstanceRef, useExtendedStructure);
                    expectedPointsByPeriod.Add(navigationPeriod, expectedPoint);
                }
            }


            HashSet<ModelObjectReference> structuralTypeRefs = new HashSet<ModelObjectReference>(ModelObjectReference.DimensionalComparer);
            StructuralSpace? tempResultingSpace = null;

            foreach (ModelObjectReference structuralTypeRef in additionalStructuralTypeRefs)
            { structuralTypeRefs.Add(structuralTypeRef); }

            tempResultingSpace = dataProvider.StructuralMap.GetRelativeStructuralSpace(reportState.StructuralTypeRef, structuralTypeRefs, useExtendedStructure);

            if (!tempResultingSpace.HasValue)
            {
                var structuralDimensions = structuralTypeRefs.Select(x => new StructuralDimension(x.ModelObjectIdAsInt, x.NonNullAlternateDimensionNumber, StructuralDimensionType.NotSet, dataProvider.DependencyMap.GetIdVariableTemplate(x))).ToList();
                tempResultingSpace = new StructuralSpace(structuralDimensions);
            }

            StructuralSpace resultingSpace = tempResultingSpace.Value;


            if (!requiresPoints)
            {
                StructuralContext structuralContext = new StructuralContext(dataProvider.StructuralMap, reportState.ModelTemplateRef, reportState.NullableModelInstanceRef);
                structuralContext.SetResultingSpace(resultingSpace, isSpaceUnique, structuralTypeRefs);

                result.Add(timeframe, structuralContext);
                return result;
            }

            foreach (var navigationPeriod in expectedPointsByPeriod.Keys)
            {
                IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> resultingPoints = new Dictionary<ListBasedKey<ModelObjectReference>, StructuralPoint>();
                int maxDimension = 1;
                if (structuralTypeRefs.Count > 0)
                {
                    maxDimension = structuralTypeRefs.Select(r => r.AlternateDimensionNumber.HasValue ? r.AlternateDimensionNumber.Value : 1).Max();
                }

                resultingPoints = dataProvider.StructuralMap.GetRelativeStructuralPoints(reportState.ModelInstanceRef, navigationPeriod, reportState.StructuralInstanceRef, structuralTypeRefs, useExtendedStructure);

                StructuralContext structuralContext = new StructuralContext(dataProvider.StructuralMap, reportState.ModelTemplateRef, reportState.NullableModelInstanceRef);
                structuralContext.SetResultingSpace(resultingSpace, isSpaceUnique, structuralTypeRefs);

                structuralContext.SetResultingPoints(resultingPoints);

                result.Add(navigationPeriod, structuralContext);
            }
            return result;
        }
    }
}