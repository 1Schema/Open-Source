using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Logic
{
    public class NotOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "value", "The value to check");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0 };
        public static readonly Signature Input_Bool__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean }, DeciaDataType.Boolean);
        public static readonly Signature Input_Int__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer }, DeciaDataType.Boolean);
        public static readonly Signature Input_Double__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal }, DeciaDataType.Boolean);
        public static readonly Signature Input_Guid__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.UniqueID }, DeciaDataType.Boolean);
        public static readonly Signature Input_DateTime__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.DateTime }, DeciaDataType.Boolean);
        public static readonly Signature Input_TimeSpan__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.TimeSpan }, DeciaDataType.Boolean);
        public static readonly Signature Input_Text__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text }, DeciaDataType.Boolean);

        public NotOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "NOT";
            m_LongName = "Not";
            m_Description = "Nots 1 boolean values";
            m_Category = LogicUtils.LogicCategoryName;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool__Return_Bool);
            validSignatures.Add(Input_Int__Return_Bool);
            validSignatures.Add(Input_Double__Return_Bool);
            validSignatures.Add(Input_Guid__Return_Bool);
            validSignatures.Add(Input_DateTime__Return_Bool);
            validSignatures.Add(Input_TimeSpan__Return_Bool);
            validSignatures.Add(Input_Text__Return_Bool);

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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Guid__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_DateTime__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_TimeSpan__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Text__Return_Bool, signature))
            {
                List<ChronometricValue> inputValuesList = inputs.Values.Select(val => val.ChronometricValue).ToList();
                ChronometricValue result = null;

                for (int i = 0; i < inputValuesList.Count; i++)
                {
                    ChronometricValue inputValue = inputValuesList[i];
                    this.AssertInputValueIsNotNull(inputValue, i);

                    if (i == 0)
                    { result = !inputValue; }
                    else
                    { throw new InvalidOperationException("Not only takes 1 argument."); }
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

            result += string.Format("NOT {0}", arg0);

            result = string.Format("({0})", result);
            return result;
        }
    }
}