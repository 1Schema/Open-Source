using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.ValueObjects
{
    public enum RenderingResultType
    {
        Ok,
        NestedErrorOccurred,
        ModelLevelErrorOccurred,
        NullReport,
        ReportInitializationFailed,
        ReportValidationPending,
        ReportIsInvalid,
        ParentReportDoesNotMatch,
        ReportElementNotFound,
        ParentReportElementNotFound,
        InvalidChildElementsFound,
        LayoutCycleExists,
        StyleCycleExists,
        StructuralReferenceIsInvalid,
        VariableReferenceIsInvalid,
        AnonymousFormulaMissing,
        AnonymousFormulaIsInvalid,
    }
}