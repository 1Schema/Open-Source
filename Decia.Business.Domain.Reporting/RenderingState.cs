using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Dimensionality;
using Decia.Business.Domain.Reporting.Dimensionality.Production;
using ReferenceListKey = Decia.Business.Common.TypedIds.ListBasedKey<Decia.Business.Common.Modeling.ModelObjectReference>;

namespace Decia.Business.Domain.Reporting
{
    public interface IRenderingState
    {
        IReport Report { get; }
        IDictionary<ReportElementId, IReportElement> ReportElements { get; }
        ICurrentState ReportState { get; }
        bool IsInDesignMode { get; }
        DimensionalGroupingState GroupingState { get; }
        IFormulaProcessingEngine FormulaProcessingEngine { get; }

        SortedDictionary<Dimension, int> OverriddenContentGroups { get; }

        Dictionary<ModelObjectReference, ModelObjectReference> CurrentStructuralBindings { get; }
        Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, ModelObjectReference>> CurrentVariableBindings { get; }
        MultiTimePeriodKey CurrentTimeBindings { get; set; }

        Nullable<KeyValuePair<ReferenceListKey, StructuralPoint>> CurrentNullableStructuralPosition { get; }
        KeyValuePair<ReferenceListKey, StructuralPoint> CurrentStructuralPosition { get; }
        MultiTimePeriodKey CurrentTimeKey { get; }

        List<CommonHeaderResult> DimensionalTable_CommonHeaders { get; set; }
        List<VariableHeaderResult> DimensionalTable_VariableHeaders { get; set; }
    }

    public class RenderingState : IRenderingState
    {
        public static readonly MultiTimePeriodKey DefaultTimeKey = MultiTimePeriodKey.DimensionlessTimeKey;

        public RenderingState(IReport report, IDictionary<ReportElementId, IReportElement> reportElements, ICurrentState reportState, IFormulaProcessingEngine formulaProcessingEngine)
        {
            Report = report;
            ReportElements = reportElements.ToDictionary(x => x.Key, x => x.Value, ReportRenderingEngine.EqualityComparer_ReportElementId);
            ReportState = reportState;
            GroupingState = new DimensionalGroupingState();
            FormulaProcessingEngine = formulaProcessingEngine;

            CurrentNullableStructuralPosition = null;
            CurrentTimeKey = DefaultTimeKey;

            OverriddenContentGroups = new SortedDictionary<Dimension, int>();

            DimensionalTable_CommonHeaders = null;
            DimensionalTable_VariableHeaders = null;
        }

        public RenderingState(IRenderingState otherRenderingState)
        {
            Report = otherRenderingState.Report; ;
            ReportElements = otherRenderingState.ReportElements.ToDictionary(x => x.Key, x => x.Value, ReportRenderingEngine.EqualityComparer_ReportElementId);
            ReportState = otherRenderingState.ReportState;
            GroupingState = otherRenderingState.GroupingState;
            FormulaProcessingEngine = otherRenderingState.FormulaProcessingEngine;

            CurrentNullableStructuralPosition = otherRenderingState.CurrentNullableStructuralPosition;
            CurrentTimeKey = otherRenderingState.CurrentTimeKey;

            OverriddenContentGroups = new SortedDictionary<Dimension, int>(otherRenderingState.OverriddenContentGroups);

            DimensionalTable_CommonHeaders = otherRenderingState.DimensionalTable_CommonHeaders;
            DimensionalTable_VariableHeaders = otherRenderingState.DimensionalTable_VariableHeaders;
        }

        public IReport Report { get; protected set; }
        public IDictionary<ReportElementId, IReportElement> ReportElements { get; protected set; }
        public ICurrentState ReportState { get; protected set; }
        public bool IsInDesignMode { get { return !ReportState.NullableModelInstanceRef.HasValue; } }
        public DimensionalGroupingState GroupingState { get; protected set; }
        public IFormulaProcessingEngine FormulaProcessingEngine { get; protected set; }

        public SortedDictionary<Dimension, int> OverriddenContentGroups { get; protected set; }

        public Dictionary<ModelObjectReference, ModelObjectReference> CurrentStructuralBindings { get; set; }
        public Dictionary<ModelObjectReference, Dictionary<ModelObjectReference, ModelObjectReference>> CurrentVariableBindings { get; set; }
        public MultiTimePeriodKey CurrentTimeBindings { get; set; }

        public Nullable<KeyValuePair<ReferenceListKey, StructuralPoint>> CurrentNullableStructuralPosition { get; protected set; }
        public KeyValuePair<ReferenceListKey, StructuralPoint> CurrentStructuralPosition { get { return CurrentNullableStructuralPosition.Value; } }
        public MultiTimePeriodKey CurrentTimeKey { get; protected set; }

        public List<CommonHeaderResult> DimensionalTable_CommonHeaders { get; set; }
        public List<VariableHeaderResult> DimensionalTable_VariableHeaders { get; set; }

        public void SetToProductionState(IReportingDataProvider dataProvider, ICurrentState productionState, KeyValuePair<ReferenceListKey, StructuralPoint> structuralPosition)
        {
            SetToProductionState(dataProvider, productionState, structuralPosition, DefaultTimeKey);
        }

        public void SetToProductionState(IReportingDataProvider dataProvider, ICurrentState productionState, KeyValuePair<ReferenceListKey, StructuralPoint> structuralPosition, MultiTimePeriodKey timeKey)
        {
            ReportState = productionState;
            GroupingState.SetToProductionState(dataProvider, productionState);
            CurrentNullableStructuralPosition = structuralPosition;
            CurrentTimeKey = timeKey;
        }

        public void UpdateStructuralPoint(KeyValuePair<ReferenceListKey, StructuralPoint> structuralPosition)
        {
            UpdateStructuralPoint(structuralPosition, DefaultTimeKey);
        }

        public void UpdateStructuralPoint(KeyValuePair<ReferenceListKey, StructuralPoint> structuralPosition, MultiTimePeriodKey timeKey)
        {
            CurrentNullableStructuralPosition = structuralPosition;
            CurrentTimeKey = timeKey;
        }

        public void UpdateProductionStateFromBindings()
        {
            var newResultingKey = CurrentStructuralPosition.Key;
            var newResultingPoint = CurrentStructuralPosition.Value;

            foreach (var entityTypeRef in CurrentStructuralBindings.Keys)
            {
                var entityInstanceRef = CurrentStructuralBindings[entityTypeRef];
                var entityInstanceCoord = new StructuralCoordinate(entityTypeRef.ModelObjectIdAsInt, entityTypeRef.NonNullAlternateDimensionNumber, entityInstanceRef.ModelObjectId, StructuralDimensionType.NotSet);

                newResultingKey = new ReferenceListKey(newResultingKey, entityInstanceRef);
                newResultingPoint = newResultingPoint.Merge(new StructuralCoordinate[] { entityInstanceCoord });
            }

            var newTimeKey = CurrentTimeBindings.MergeTimeKeys(CurrentTimeKey);

            CurrentNullableStructuralPosition = new KeyValuePair<ReferenceListKey, StructuralPoint>(newResultingKey, newResultingPoint);
            CurrentTimeKey = newTimeKey;
        }
    }
}