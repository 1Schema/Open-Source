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

namespace Decia.Business.Domain.Formulas.Operations
{
    public class IdentityOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false);
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0 };
        public static readonly Signature Input_Bool__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean }, DeciaDataType.Boolean);
        public static readonly Signature Input_Int__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal }, DeciaDataType.Decimal);
        public static readonly Signature Input_DateTime__Return_DateTime = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.DateTime }, DeciaDataType.DateTime);
        public static readonly Signature Input_TimeSpan__Return_TimeSpan = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.TimeSpan }, DeciaDataType.TimeSpan);
        public static readonly Signature Input_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text }, DeciaDataType.Text);

        public IdentityOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "IDENT";
            m_LongName = "Identity";
            m_Description = "Does nothing";
            m_Category = string.Empty;

            m_IsVisible = false;
            m_DisplayAsFunction = true;
            m_OperatorNotation = OperatorNotationType.Prefix;
            m_OperatorText = "+";

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool__Return_Bool);
            validSignatures.Add(Input_Int__Return_Int);
            validSignatures.Add(Input_Double__Return_Double);
            validSignatures.Add(Input_DateTime__Return_DateTime);
            validSignatures.Add(Input_TimeSpan__Return_TimeSpan);
            validSignatures.Add(Input_Text__Return_Text);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            ITimeDimensionSet inputTimeDimensionality = inputs.First().Value.TimeDimesionality;

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
            CompoundUnit inputUnit = inputs.First().Value.Unit;

            resultingUnit = inputUnit;
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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_DateTime__Return_DateTime, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_TimeSpan__Return_TimeSpan, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Text__Return_Text, signature))
            {
                List<ChronometricValue> inputValuesList = inputs.Values.Select(val => val.ChronometricValue).ToList();
                ChronometricValue result = inputValuesList.First();
                return new OperationMember(result, unit);
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;
            var arg0 = argumentTextByIndex[Parameter0.Index];

            result = string.Format("({0})", arg0);
            return result;
        }
    }
}