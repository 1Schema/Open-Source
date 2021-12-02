using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Logic
{
    public class IfOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "condition", "Condition value");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, "trueValue", "The value to return if the condition is met");
        public static readonly Parameter Parameter2 = new Parameter(2, true, false, "falseValue", "The value to return if the condition is not met");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2 };
        public static readonly Signature Input_Bool_Bool_Bool__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Boolean, DeciaDataType.Boolean }, DeciaDataType.Boolean);
        public static readonly Signature Input_Bool_Int_Int__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Integer, DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Bool_Double_Int__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Decimal, DeciaDataType.Integer }, DeciaDataType.Decimal);
        public static readonly Signature Input_Bool_Int_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Integer, DeciaDataType.Decimal }, DeciaDataType.Decimal);
        public static readonly Signature Input_Bool_Double_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Decimal, DeciaDataType.Decimal }, DeciaDataType.Decimal);
        public static readonly Signature Input_Bool_Guid_Guid__Return_Guid = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.UniqueID, DeciaDataType.UniqueID }, DeciaDataType.UniqueID);
        public static readonly Signature Input_Bool_DateTime_DateTime__Return_DateTime = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.DateTime);
        public static readonly Signature Input_Bool_TimeSpan_TimeSpan__Return_TimeSpan = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.TimeSpan, DeciaDataType.TimeSpan }, DeciaDataType.TimeSpan);
        public static readonly Signature Input_Bool_Text_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Text, DeciaDataType.Text }, DeciaDataType.Text);

        public IfOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "IF";
            m_LongName = "If";
            m_Description = "If first operand is true, returns second operand, otherwise, returns third operand";
            m_Category = LogicUtils.LogicCategoryName;

            m_DisplayAsFunction = true;
            m_OperatorNotation = OperatorNotationType.Infix;
            m_OperatorText = "?";
            m_SubOperatorText = ":";

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool_Bool_Bool__Return_Bool);
            validSignatures.Add(Input_Bool_Int_Int__Return_Int);
            validSignatures.Add(Input_Bool_Double_Int__Return_Double);
            validSignatures.Add(Input_Bool_Int_Double__Return_Double);
            validSignatures.Add(Input_Bool_Double_Double__Return_Double);
            validSignatures.Add(Input_Bool_Guid_Guid__Return_Guid);
            validSignatures.Add(Input_Bool_DateTime_DateTime__Return_DateTime);
            validSignatures.Add(Input_Bool_TimeSpan_TimeSpan__Return_TimeSpan);
            validSignatures.Add(Input_Bool_Text_Text__Return_Text);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            ITimeDimensionSet inputTimeDimensionality = null;
            for (int i = 0; i < inputs.Count; i++)
            {
                if (i == 0)
                { inputTimeDimensionality = inputs[i].TimeDimesionality; }
                else
                { inputTimeDimensionality = inputTimeDimensionality.GetDimensionsForResult(inputs[i].TimeDimesionality); }
            }

            if (inputTimeDimensionality == null)
            {
                resultingTimeDimensionality = null;
                return false;
            }

            resultingTimeDimensionality = inputTimeDimensionality;
            return true;
        }

        protected override bool IsUnitValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out CompoundUnit resultingUnit)
        {
            resultingUnit = CompoundUnit.GetGlobalScalarUnit(currentState.ProjectGuid, currentState.RevisionNumber);
            return true;
        }

        protected override OperationMember DoCompute(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, OperationMember defaultReturnValue)
        {
            AssertProcessingTypeIsValid(currentState);

            Signature signature;
            IDictionary<int, DeciaDataType> inputTypes = inputs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.DataType);
            bool isValid = SignatureSpecification.TryGetValidSignature(inputTypes, out signature);

            if (!isValid)
            { return new OperationMember(); }

            CompoundUnit unit = null;
            IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out unit);

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Bool_Bool__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Int_Int__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Double_Int__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Int_Double__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Double_Double__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Guid_Guid__Return_Guid, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_DateTime_DateTime__Return_DateTime, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_TimeSpan_TimeSpan__Return_TimeSpan, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Text_Text__Return_Text, signature))
            {
                List<ChronometricValue> inputValuesList = inputs.Values.Select(val => val.ChronometricValue).ToList();
                ChronometricValue ifValues = inputValuesList[0];
                ChronometricValue trueValues = inputValuesList[1];
                ChronometricValue falseValues = inputValuesList[2];

                IList<MultiTimePeriodKey> timeKeys = trueValues.GetTimeKeysForIteration(falseValues);
                Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
                DeciaDataType resultingDataType = DeciaDataType.Text;

                foreach (MultiTimePeriodKey timeKey in timeKeys)
                {
                    DynamicValue ifValue = ifValues.GetValue(timeKey);
                    DynamicValue trueValue = trueValues.GetValue(timeKey);
                    DynamicValue falseValue = falseValues.GetValue(timeKey);
                    DynamicValue resultValue = falseValue;

                    if (!ifValue.IsNull)
                    {
                        if (ifValue.GetTypedValue<bool>() == true)
                        { resultValue = trueValue; }
                    }

                    resultingDataType = resultValue.DataType;
                    resultingValues.Add(timeKey, resultValue);
                }

                ChronometricValue result = trueValues.CreateDimensionedResult(falseValues, resultingDataType);
                foreach (MultiTimePeriodKey timeKey in timeKeys)
                {
                    result.SetValue(timeKey, resultingValues[timeKey]);
                }
                return new OperationMember(result, unit);
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;
            var arg0 = argumentTextByIndex[Parameter0.Index];
            var arg1 = argumentTextByIndex[Parameter1.Index];
            var arg2 = argumentTextByIndex[Parameter2.Index];

            result += string.Format("CASE WHEN {0} THEN {1} ELSE {2} END", arg0, arg1, arg2);

            result = string.Format("({0})", result);
            return result;
        }
    }
}