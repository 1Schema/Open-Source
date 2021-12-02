using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Operations;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas
{
    public interface IOperation : IOrderable
    {
        #region Properties

        OperationId Id { get; }
        string ShortName { get; }
        string LongName { get; }
        string Description { get; }
        string Category { get; }

        OperationValidityType ValidityType { get; }
        EvaluationType EvaluationType { get; }
        ProcessingType ValidProcessingType { get; }
        SqlQueryType QueryType_ForSqlExport { get; }

        bool IsVisible { get; }
        bool DisplayAsFunction { get; }
        OperatorNotationType OperatorNotation { get; }
        string OperatorText { get; }
        string SubOperatorText { get; }
        bool AllowAutoConversionToText { get; }

        OperationType StructuralOperationType { get; }
        OperationType TimeOperationType { get; }
        ISignatureValiditySpecification SignatureSpecification { get; }

        #endregion

        #region Methods

        OperationMember Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IEnumerable<OperationMember> inputs);
        OperationMember Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs);

        OperationMember Compute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IEnumerable<OperationMember> inputs, OperationMember defaultReturnValue);
        OperationMember Compute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, OperationMember defaultReturnValue);

        string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex);

        #endregion
    }
}