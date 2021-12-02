using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class DimensionEnumerator
    {
        #region Constructors

        public DimensionEnumerator(IReportElement sourceElement, ModelObjectReference dimensionRef, int sortOrder)
        {
            DimensionType dimensionType;

            if (dimensionRef.ModelObjectType.IsStructuralType())
            {
                dimensionType = DimensionType.Structure;
            }
            else if (dimensionRef.ModelObjectType.IsTimeType())
            {
                dimensionType = DimensionType.Time;
            }
            else
            { throw new InvalidOperationException("The specified DimensionType is not currently supported."); }

            SourceElement = sourceElement;
            DimensionType = dimensionType;
            DimensionRef = dimensionRef;
            SortOrder = sortOrder;
            NameVariableRef = null;
        }

        #endregion

        public IReportElement SourceElement { get; protected set; }
        public DimensionType DimensionType { get; protected set; }
        public ModelObjectReference DimensionRef { get; protected set; }
        public int SortOrder { get; protected set; }
        public ModelObjectReference? NameVariableRef { get; protected set; }

        public Func<object, object, int> OrderByCompareMethod { get; set; }

        public ReportId ReportId { get { return ElementId.ReportId; } }
        public ReportElementId ElementId { get { return SourceElement.Key; } }
        public TimeDimensionType? TimeDimensionType { get { return (DimensionType == DimensionType.Time) ? DimensionRef.NonNullTimeDimensionType : (TimeDimensionType?)null; } }

        public IComparer GetComparer()
        { return new MethodBasedComparer(OrderByCompareMethod); }

        public IComparer<T> GetTypedComparer<T>()
        { return new MethodBasedComparer<T>(OrderByCompareMethod); }

        public List<DimensionResult> EnumerateResults(IReportingDataProvider dataProvider, ICurrentState reportState)
        {
            var enumeratedResults = new List<DimensionResult>();

            if (DimensionType == DimensionType.Structure)
            {
                if (DimensionRef.ModelObjectType != ModelObjectType.EntityType)
                { throw new InvalidOperationException("Only Entity Types can be used as Structural Dimensions in Reports."); }

                var modelInstanceRef = reportState.ModelInstanceRef;
                var structuralTypeRef_ForReport = reportState.StructuralTypeRef;
                var structuralInstanceRef_ForReport = reportState.StructuralInstanceRef;
                var timeKey_ForReport = reportState.TimeKey;
                var isReportGlobal = (structuralTypeRef_ForReport.ModelObjectType == ModelObjectType.GlobalType);

                var timeKey = timeKey_ForReport;
                var structuralPoint_ForReport = dataProvider.StructuralMap.GetStructuralPoint(modelInstanceRef, timeKey.NullablePrimaryTimePeriod, structuralInstanceRef_ForReport, reportState.UseExtendedStructure);
                var structuralPoints_ForReport = new List<StructuralPoint>();
                structuralPoints_ForReport.Add(structuralPoint_ForReport);

                var structuralTypeRef_ToEnumerate = DimensionRef;
                var isEnumerationTypeAccessibleFromReportType = dataProvider.StructuralMap.IsAccessible(structuralTypeRef_ToEnumerate, structuralTypeRef_ForReport, reportState.UseExtendedStructure);

                var structuralInstanceRefs_ToEnumerate = dataProvider.StructuralMap.GetStructuralInstancesForType(modelInstanceRef, structuralTypeRef_ToEnumerate);
                var validStructuralInstanceRefs_ToEnumerate = structuralInstanceRefs_ToEnumerate.ToList();

                if (!isReportGlobal && isEnumerationTypeAccessibleFromReportType)
                {
                    var relatedPoints = dataProvider.StructuralMap.GetRelatedStructuralInstances(modelInstanceRef, timeKey.NullablePrimaryTimePeriod, structuralTypeRef_ToEnumerate, structuralPoints_ForReport, reportState.UseExtendedStructure);
                    var reportRelatedPoints = relatedPoints[structuralPoint_ForReport];

                    var relatedInstanceRefs = new List<ModelObjectReference>();
                    foreach (var point in reportRelatedPoints.Values)
                    {
                        foreach (var coordinate in point.Coordinates)
                        {
                            if (coordinate.EntityTypeRef == structuralTypeRef_ToEnumerate)
                            { relatedInstanceRefs.Add(coordinate.EntityInstanceRef); }
                        }
                    }

                    foreach (var validStructuralInstanceRef in validStructuralInstanceRefs_ToEnumerate.ToList())
                    {
                        if (!relatedInstanceRefs.Contains(validStructuralInstanceRef))
                        { validStructuralInstanceRefs_ToEnumerate.Remove(validStructuralInstanceRef); }
                    }
                }

                foreach (var structuralInstanceRef in validStructuralInstanceRefs_ToEnumerate)
                {
                    if (!NameVariableRef.HasValue)
                    { NameVariableRef = dataProvider.DependencyMap.GetNameVariableTemplate(DimensionRef); }

                    var variableInstanceRef = dataProvider.GetChildVariableInstanceReference(reportState.ModelInstanceRef, NameVariableRef.Value, structuralInstanceRef);
                    var timeMatrix = dataProvider.GetComputedTimeMatrix(reportState.ModelInstanceRef, variableInstanceRef);

                    if (timeMatrix.HasTimeSpecificValue)
                    { throw new InvalidOperationException("The Name Variable must not have Time-specific value."); }

                    var valueToDisplay = timeMatrix.GetValue(MultiTimePeriodKey.DimensionlessTimeKey);

                    var enumeratedResult = new DimensionResult(this, structuralInstanceRef, variableInstanceRef, valueToDisplay);
                    enumeratedResults.Add(enumeratedResult);
                }
            }
            else if (DimensionType == DimensionType.Time)
            {
                var timeTitleRange = (SourceElement as ITimeTitleRange);
                var timeDimension_ToEnumerate = new TimeDimension(timeTitleRange.TimeDimensionType, timeTitleRange.TimeValueType, timeTitleRange.TimePeriodType, reportState.ModelStartDate, reportState.ModelEndDate);

                var timePeriods = ITimeDimensionUtils.GeneratePeriodsForTimeDimension(timeDimension_ToEnumerate);

                foreach (var timePeriod in timePeriods)
                {
                    var enumeratedResult = new DimensionResult(this, timePeriod);
                    enumeratedResults.Add(enumeratedResult);
                }
            }
            else
            { throw new InvalidOperationException("The specified DimensionType is not currently supported."); }

            return enumeratedResults;
        }
    }
}