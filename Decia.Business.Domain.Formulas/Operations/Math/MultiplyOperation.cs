using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Math
{
    public class MultiplyOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "number1", "First number value");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, "number2", "Second number value");
        public static readonly Parameter Parameter2 = new Parameter(2, false, true, "number3", "Additional number value");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2 };
        public static readonly Signature Input_Int_Int_Int__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Integer, DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Double_Double_Int__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Decimal, DeciaDataType.Integer }, DeciaDataType.Decimal);
        public static readonly Signature Input_Double_Int_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Integer, DeciaDataType.Decimal }, DeciaDataType.Decimal);
        public static readonly Signature Input_Double_Int_Int__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Integer, DeciaDataType.Integer }, DeciaDataType.Decimal);
        public static readonly Signature Input_Int_Double_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Decimal, DeciaDataType.Decimal }, DeciaDataType.Decimal);
        public static readonly Signature Input_Int_Double_Int__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Decimal, DeciaDataType.Integer }, DeciaDataType.Decimal);
        public static readonly Signature Input_Int_Int_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Integer, DeciaDataType.Decimal }, DeciaDataType.Decimal);
        public static readonly Signature Input_Double_Double_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Decimal, DeciaDataType.Decimal }, DeciaDataType.Decimal);

        public MultiplyOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "MULTIPLY";
            m_LongName = "Multiply";
            m_Description = "Multiplies 2 or more numbers";
            m_Category = MathUtils.MathCategoryName;

            m_DisplayAsFunction = false;
            m_OperatorNotation = OperatorNotationType.Infix;
            m_OperatorText = "*";

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Int_Int_Int__Return_Int);
            validSignatures.Add(Input_Double_Double_Int__Return_Double);
            validSignatures.Add(Input_Double_Int_Double__Return_Double);
            validSignatures.Add(Input_Double_Int_Int__Return_Double);
            validSignatures.Add(Input_Int_Double_Double__Return_Double);
            validSignatures.Add(Input_Int_Double_Int__Return_Double);
            validSignatures.Add(Input_Int_Int_Double__Return_Double);
            validSignatures.Add(Input_Double_Double_Double__Return_Double);

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
            List<CompoundUnit> inputUnitsList = inputs.Values.Select(val => val.Unit).ToList();
            CompoundUnit resultUnit = null;

            for (int i = 0; i < inputUnitsList.Count; i++)
            {
                CompoundUnit inputUnit = inputUnitsList[i];

                if (inputUnit == null)
                { continue; }
                else if (resultUnit == null)
                { resultUnit = inputUnit; }
                else
                { resultUnit = resultUnit * inputUnit; }
            }

            resultingUnit = resultUnit;
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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Int_Int_Int__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Double_Int__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Int_Double__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Int_Int__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int_Double_Double__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int_Double_Int__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int_Int_Double__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Double_Double__Return_Double, signature))
            {
                List<ChronometricValue> inputValuesList = inputs.Values.Select(val => val.ChronometricValue).ToList();
                ChronometricValue result = null;

                for (int i = 0; i < inputValuesList.Count; i++)
                {
                    ChronometricValue inputValue = inputValuesList[i];
                    this.AssertInputValueIsNotNull(inputValue, i);

                    if (i == 0)
                    { result = inputValue; }
                    else
                    { result = result * inputValue; }
                }

                return new OperationMember(result, unit);
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;
            var isFirst = true;

            foreach (var argBucket in argumentTextByIndex)
            {
                if (isFirst)
                { isFirst = false; }
                else
                { result += " * "; }

                result += argBucket.Value;
            }

            result = string.Format("({0})", result);
            return result;
        }
    }
}