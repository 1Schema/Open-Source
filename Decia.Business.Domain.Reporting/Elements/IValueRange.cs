using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public interface IValueRange : IFormulaHost, IReportElement
    {
        bool TryGetDesignValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout);
        bool TryGetProductionValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout);
    }

    public interface IValueRange_Configurable : IValueRange, IReportElement
    {
        OutputValueType OutputValueType { get; }

        DynamicValue DirectValue { get; }
        bool HasRefValue { get; }
        ModelObjectReference RefValue { get; }
        bool HasValueFormula { get; }
        FormulaId ValueFormulaId { get; }
        Guid ValueFormulaGuid { get; }

        void SetToDirectValue();
        void SetToDirectValue(object value);
        void SetToRefValue();
        void SetToRefValue(ModelObjectReference reference);
        void SetToRefValue(ModelObjectReference reference, Nullable<int> alternateDimensionNumber);
        void SetToRefValue(ModelObjectType referencedType, int referencedNumber);
        void SetToRefValue(ModelObjectType referencedType, int referencedNumber, Nullable<int> alternateDimensionNumber);
        void SetToValueFormula();
        void SetToValueFormula(FormulaId formulaId);
        void ClearValueFormula();
    }
}