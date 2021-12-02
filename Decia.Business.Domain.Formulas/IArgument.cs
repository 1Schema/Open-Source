using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas
{
    public interface IArgument
    {
        #region Properties

        ArgumentId Key { get; }
        int ArgumentIndex { get; }
        ArgumentType ArgumentType { get; }

        ExpressionId ParentExpressionId { get; }
        OperationId ParentOperationId { get; }
        IOperation ParentOperation { get; }
        int AutoJoinOrder { get; set; }

        ExpressionId NestedExpressionId { get; }
        ModelObjectReference ReferencedModelObject { get; }
        DynamicValue DirectValue { get; }
        ChronometricValue DirectChronometricValue { get; }

        bool IsRefDeleted { get; }
        string DeletedRef_StructuralTypeText { get; }
        string DeletedRef_VariableTemplateText { get; }

        #endregion

        #region Assessment Methods

        void SetToNestedExpression(ExpressionId expressionId);

        void SetToReferencedId(ModelObjectReference reference);
        void SetToReferencedId(ModelObjectReference reference, Nullable<int> alternateDimensionNumber);
        void SetToReferencedId(ModelObjectType referencedType, Guid referencedGuid);
        void SetToReferencedId(ModelObjectType referencedType, Guid referencedGuid, Nullable<int> alternateDimensionNumber);

        void SetToDirectValue(DeciaDataType dataType);
        void SetToDirectValue(DeciaDataType dataType, object value);

        void DeleteRef(string structuralTypeText, string variableTemplateText);

        #endregion

        #region Usage Methods

        ComputationResult Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext);

        ComputationResult Compute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, ICollection<IExpression> parentExpressions, IStructuralContext structuralContext);

        string RenderAsString(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings);

        string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, ICollection<IExpression> parentExpressions, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings);

        #endregion
    }
}