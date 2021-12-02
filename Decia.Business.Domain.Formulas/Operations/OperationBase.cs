using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations
{
    public abstract class OperationBase : IOperation
    {
        protected OperationId m_Id;
        protected string m_ShortName;
        protected string m_LongName;
        protected string m_Description;
        protected string m_Category;
        protected Nullable<long> m_OrderNumber;

        protected OperationValidityType m_ValidityType = OperationValidityType.All;
        protected EvaluationType m_EvaluationType = EvaluationType.Processing;
        protected ProcessingType m_ValidProcessingType = ProcessingType.NormalAndAnonymous;
        protected SqlQueryType m_QueryType_ForSqlExport = SqlQueryType.SimpleSelect;

        protected bool m_IsVisible = true;
        protected bool m_DisplayAsFunction = true;
        protected OperatorNotationType m_OperatorNotation = OperatorNotationType.Prefix;
        protected string m_OperatorText = string.Empty;
        protected string m_SubOperatorText = string.Empty;
        protected bool m_AllowAutoConversionToText = false;

        protected OperationType m_StructuralOperationType = OperationType.SimpleComputation;
        protected OperationType m_TimeOperationType = OperationType.SimpleComputation;
        protected ISignatureValiditySpecification m_SignatureSpecification;

        #region Public Properties

        public OperationId Id
        {
            get { return m_Id; }
        }

        public string ShortName
        {
            get { return m_ShortName; }
        }

        public string LongName
        {
            get { return m_LongName; }
        }

        public string Description
        {
            get { return m_Description; }
        }

        public string Category
        {
            get { return m_Category; }
        }

        public OperationValidityType ValidityType
        {
            get { return m_ValidityType; }
        }

        public EvaluationType EvaluationType
        {
            get { return m_EvaluationType; }
        }

        public ProcessingType ValidProcessingType
        {
            get { return m_ValidProcessingType; }
        }

        public SqlQueryType QueryType_ForSqlExport
        {
            get { return m_QueryType_ForSqlExport; }
        }

        public bool IsVisible
        {
            get { return m_IsVisible; }
        }

        public bool DisplayAsFunction
        {
            get { return m_DisplayAsFunction; }
        }

        public OperatorNotationType OperatorNotation
        {
            get { return m_OperatorNotation; }
        }

        public string OperatorText
        {
            get { return m_OperatorText; }
        }

        public string SubOperatorText
        {
            get { return (!string.IsNullOrWhiteSpace(m_SubOperatorText)) ? m_SubOperatorText : m_OperatorText; }
        }

        public bool AllowAutoConversionToText
        {
            get { return m_AllowAutoConversionToText; }
        }

        public OperationType StructuralOperationType
        {
            get { return m_StructuralOperationType; }
        }

        public OperationType TimeOperationType
        {
            get { return m_TimeOperationType; }
        }

        public ISignatureValiditySpecification SignatureSpecification
        {
            get { return m_SignatureSpecification; }
        }

        public int RequiredArgCount
        {
            get { return SignatureSpecification.Parameters.Where(x => x.IsRequired).ToList().Count; }
        }

        #endregion

        #region Public Methods

        public void AssertProcessingTypeIsValid(ICurrentState currentState)
        {
            var currentProcessingType = currentState.ProcessingType;

            if (currentProcessingType != (currentProcessingType & this.ValidProcessingType))
            { throw new InvalidOperationException("The Current ProcessingType is not valid for the Operation."); }
        }

        public OperationMember Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IEnumerable<OperationMember> inputs)
        {
            SortedDictionary<int, OperationMember> inputsByIndex = inputs.GetAsSortedDictionary();
            return Validate(dataProvider, currentState, parentFormula, callingExpression, inputsByIndex);
        }

        public OperationMember Validate(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs)
        {
            AssertProcessingTypeIsValid(currentState);

            DeciaDataType resultingDataType;
            bool isValid = IsSignatureValid(inputs, out resultingDataType);

            if (!isValid)
            { return new OperationMember(); }

            ITimeDimensionSet resultingTimeDimensionality = null;
            isValid = IsTimeDimensionalityValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out resultingTimeDimensionality);

            if (!isValid)
            { return new OperationMember(); }

            CompoundUnit resultingUnit = null;
            isValid = IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out resultingUnit);

            if (!isValid)
            { return new OperationMember(); }

            return new OperationMember(resultingDataType, resultingTimeDimensionality, resultingUnit);
        }

        public OperationMember Compute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IEnumerable<OperationMember> inputs, OperationMember defaultReturnValue)
        {
            Dictionary<int, OperationMember> inputValuesByIndex = inputs.GetAsDictionary();
            return Compute(dataProvider, currentState, parentFormula, callingExpression, inputValuesByIndex, defaultReturnValue);
        }

        public OperationMember Compute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, OperationMember defaultReturnValue)
        {
            return DoCompute(dataProvider, currentState, parentFormula, callingExpression, inputs, defaultReturnValue);
        }

        public string RenderAsSqlSelect(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            return DoRenderAsSql(dataProvider, currentState, exportInfo, parentFormula, callingExpression, argumentTextByIndex);
        }

        #endregion

        #region Protected Abstract Methods

        protected abstract bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality);
        protected abstract bool IsUnitValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out CompoundUnit resultingUnit);
        protected abstract OperationMember DoCompute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, OperationMember defaultReturnValue);
        protected abstract string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex);

        #endregion

        #region Protected Virtual Methods

        protected virtual bool IsSignatureValid(IDictionary<int, OperationMember> inputs, out DeciaDataType resultingDataType)
        {
            Signature matchingSignature;
            Dictionary<int, DeciaDataType> inputDataTypes = inputs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.DataType);
            resultingDataType = DeciaDataTypeUtils.InvalidDataType;

            if (!SignatureSpecification.TryGetValidSignature(inputDataTypes, out matchingSignature))
            {
                if (AllowAutoConversionToText)
                {
                    inputDataTypes = inputDataTypes.ToDictionary(kvp => kvp.Key, kvp => DeciaDataType.Text);

                    if (!SignatureSpecification.TryGetValidSignature(inputDataTypes, out matchingSignature))
                    { return false; }
                }
                else
                { return false; }
            }

            foreach (int argumentIndex in inputs.Keys)
            {
                if (!matchingSignature.AllowsDynamicInByIndex[argumentIndex])
                {
                    if (inputs[argumentIndex].ChronometricValue == ChronometricValue.NullInstanceAsObject)
                    { return false; }
                }
            }
            resultingDataType = matchingSignature.TypeOut;
            return true;
        }

        #endregion

        #region IOrderable Implementation

        public long? OrderNumber
        {
            get { return m_OrderNumber; }
        }

        public string OrderValue
        {
            get { return ShortName; }
        }

        #endregion
    }
}