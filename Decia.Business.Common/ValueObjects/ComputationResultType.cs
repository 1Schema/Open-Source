using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.ValueObjects
{
    public enum ComputationResultType
    {
        Ok,
        NestedErrorOccurred,
        ModelLevelErrorOccurred,
        NullFormula,
        FormulaInitializationFailed,
        FormulaValidationPending,
        FormulaIsInvalid,
        FormulaRequiresPeriodSpecificComputation,
        OperationNotDefined,
        OperationNotValidForArea,
        OperationArgumentsNotValid,
        OperationArgumentsNotComputable,
        ExpressionNotDefinedInFormula,
        ExpressionContainsZeroInstances,
        ArgumentNotDefinedInFormula,
        ArgumentValueIsException,
        ArgumentValueIsNull,
        ArgumentReferencesInvalidId,
        ArgumentReferencesInvalidValue,
        ArgumentReferencesStructurallyInaccessibleId,
        ArgumentValidationError,
        NavigationFormulaError,
        AggregationRequiredError,
        AggregationNotAllowedError,
        FilterRequiredError,
        FilterNotAllowedError,
        ShiftNotAllowedError,
    }
}