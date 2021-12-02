using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Math
{
    public class ExponentiationOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "number", "The base number value");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, "power", "The exponent number value");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1 };
        public static readonly Signature Input_Int_Int__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Double_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Decimal }, DeciaDataType.Decimal);

        public ExponentiationOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "POWER";
            m_LongName = "Exponentiation";
            m_Description = "Exponentiates 2 numbers";
            m_Category = MathUtils.MathCategoryName;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Int_Int__Return_Int);
            validSignatures.Add(Input_Double_Double__Return_Double);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            ITimeDimensionSet firstTimeDimensionality = inputs[0].TimeDimesionality;
            ITimeDimensionSet secondTimeDimensionality = inputs[1].TimeDimesionality;

            if (secondTimeDimensionality.PrimaryTimeDimension.HasTimeValue
                || secondTimeDimensionality.SecondaryTimeDimension.HasTimeValue)
            {
                resultingTimeDimensionality = null;
                return false;
            }

            if (firstTimeDimensionality == null)
            {
                resultingTimeDimensionality = null;
                return false;
            }

            resultingTimeDimensionality = firstTimeDimensionality;
            return true;
        }

        protected override bool IsUnitValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out CompoundUnit resultingUnit)
        {
            CompoundUnit firstInputUnit = inputs[0].Unit;
            ChronometricValue secondInputValue = inputs[1].ChronometricValue;
            CompoundUnit resultUnit = null;

            double power = (double)secondInputValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetValue();

            int powerNumerator = (int)power;
            double powerDenominator = power - powerNumerator;

            for (int i = 1; i < powerNumerator; i++)
            {
                if (firstInputUnit == null)
                { continue; }
                else if (resultUnit == null)
                { resultUnit = firstInputUnit; }
                else
                { resultUnit = resultUnit * firstInputUnit; }
            }

            resultingUnit = resultUnit;
            return true;
        }

        static uint GreatestCommonDivisor(uint valA, uint valB)
        {
            if (valB == 0)
                return valA;
            else
                return GreatestCommonDivisor(valB, valA % valB);
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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Int_Int__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Double__Return_Double, signature))
            {
                List<ChronometricValue> inputValuesList = inputs.Values.Select(val => val.ChronometricValue).ToList();
                ChronometricValue result = null;

                ChronometricValue firstInputValue = inputValuesList[0];
                ChronometricValue secondInputValue = inputValuesList[1];

                this.AssertInputValueIsNotNull(firstInputValue, 0);
                this.AssertInputValueIsNotNull(firstInputValue, 1);

                result = firstInputValue.CreateDimensionedResult(secondInputValue, signature.TypeOut);
                foreach (MultiTimePeriodKey timeKey in result.TimeKeys)
                {
                    DynamicValue firstTimeValue = firstInputValue.GetValue(timeKey);
                    DynamicValue secondTimeValue = secondInputValue.GetValue(timeKey);

                    double resultingDouble = System.Math.Pow((double)firstTimeValue.GetValue(), (double)secondTimeValue.GetValue());
                    result.SetValue(timeKey, resultingDouble);
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

            result += string.Format("POWER({0}, {1})", arg0, arg1);

            return result;
        }
    }
}