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

namespace Decia.Business.Domain.Formulas.Operations.Queries
{
    public class QueryOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "where", "The condition that results must meet.");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, "select", "The value to return.");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1 };
        public static readonly Signature Input_Bool_Bool__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Boolean }, DeciaDataType.Boolean);
        public static readonly Signature Input_Bool_Int__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Bool_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Decimal }, DeciaDataType.Decimal);
        public static readonly Signature Input_Bool_Guid__Return_Guid = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.UniqueID }, DeciaDataType.UniqueID);
        public static readonly Signature Input_Bool_DateTime__Return_DateTime = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.DateTime }, DeciaDataType.DateTime);
        public static readonly Signature Input_Bool_TimeSpan__Return_TimeSpan = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.TimeSpan }, DeciaDataType.TimeSpan);
        public static readonly Signature Input_Bool_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Text }, DeciaDataType.Text);

        public QueryOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "QUERY";
            m_LongName = "Query";
            m_Description = "Filters results across Structural Dimension(s) based on the first boolean parameter, and returns the second parameter.";
            m_Category = QueryUtils.QueriesCategoryName;
            m_ValidProcessingType = ProcessingType.Normal;
            m_StructuralOperationType = OperationType.Filter;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool_Bool__Return_Bool);
            validSignatures.Add(Input_Bool_Int__Return_Int);
            validSignatures.Add(Input_Bool_Double__Return_Double);
            validSignatures.Add(Input_Bool_Guid__Return_Guid);
            validSignatures.Add(Input_Bool_DateTime__Return_DateTime);
            validSignatures.Add(Input_Bool_TimeSpan__Return_TimeSpan);
            validSignatures.Add(Input_Bool_Text__Return_Text);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            ITimeDimensionSet whereTimeDimensionality = inputs[0].TimeDimesionality;
            ITimeDimensionSet returnTimeDimensionality = inputs[1].TimeDimesionality;

            if ((whereTimeDimensionality == null) || (returnTimeDimensionality == null))
            {
                resultingTimeDimensionality = null;
                return false;
            }

            if ((whereTimeDimensionality.SecondaryTimeDimension.HasTimeValue) || (returnTimeDimensionality.SecondaryTimeDimension.HasTimeValue))
            {
                resultingTimeDimensionality = null;
                return false;
            }

            resultingTimeDimensionality = whereTimeDimensionality.GetDimensionsForResult(returnTimeDimensionality);
            return true;
        }

        protected override bool IsUnitValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out CompoundUnit resultingUnit)
        {
            CompoundUnit inputUnit = inputs[1].Unit;

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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Bool__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Int__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Double__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Guid__Return_Guid, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_DateTime__Return_DateTime, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_TimeSpan__Return_TimeSpan, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Text__Return_Text, signature))
            {
                List<ChronometricValue> inputValuesList = inputs.Values.Select(val => val.ChronometricValue).ToList();
                ChronometricValue whereValues = inputValuesList[0];
                ChronometricValue returnValues = inputValuesList[1];

                bool includeInResults = false;
                if (!currentState.PrimaryPeriod.HasValue)
                {
                    includeInResults = whereValues.GetValue(currentState.TimeKey).GetTypedValue<bool>();
                }
                else if (currentState.PrimaryPeriod.HasValue && !currentState.SecondaryPeriod.HasValue)
                {
                    includeInResults = whereValues.GetValue(currentState.TimeKey).GetTypedValue<bool>();
                }
                else
                { throw new InvalidOperationException("Currently Queries only support a PrimaryTimeDimension"); }

                if (!includeInResults)
                {
                    return new OperationMember(false);
                }
                else
                {
                    return new OperationMember(returnValues, unit);
                }
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;
            var arg0 = argumentTextByIndex[Parameter0.Index];
            var arg1 = argumentTextByIndex[Parameter1.Index];

            exportInfo.Filtering_WhereText = arg0;
            exportInfo.Filtering_ResultColumnText = arg1;

            result += arg1;

            result = string.Format("({0})", result);
            return result;
        }
    }
}