using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting
{
    public interface IReportingDataProvider : IFormulaDataProvider_Anonymous
    {
        ICollection<ReportId> AvailableReportIds { get; }
        ICollection<ReportElementId> GetAvailableReportElementIds(ReportId reportId);

        IReport GetReport(ModelObjectReference reportRef);
        T GetReport<T>(ModelObjectReference reportRef)
            where T : IReport;

        IReportElement GetReportElement(ModelObjectReference reportElementRef);
        T GetReportElement<T>(ModelObjectReference reportElementRef)
            where T : IReportElement;

        IEnumerable<ModelObjectReference> GetReportElementRefsInReport(ModelObjectReference reportRef);
        IEnumerable<IReportElement> GetReportElementsInReport(ModelObjectReference reportRef);

        ICollection<FormulaId> GetAvailableReportingFormulaIds(ReportId reportId);
        IFormula GetReportingFormula(FormulaId formulaId);

        void ClearAnonymousVariable_ReportingStates(ReportId reportId);
        void AddAnonymousVariable_ReportingState(ReportElementId reportElementId, Formula formula, ModelObjectReference rootStructuralTypeRef, StructuralSpace structuralSpace, IDictionary<TimeDimensionType, Nullable<TimePeriodType>> timeDimensionTypes, DeciaDataType dataType);
        void AddAnonymousVariable_ReportingState(ReportElementId reportElementId, Formula formula, ModelObjectReference rootStructuralTypeRef, StructuralSpace structuralSpace, IDictionary<TimeDimensionType, Nullable<TimePeriodType>> timeDimensionTypes, DeciaDataType dataType, DynamicValue defaultValue);
    }
}