using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Structure
{
    public class StructuralSumOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "value", "Value to aggregate");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0 };
        public static readonly Signature Input_Bool__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean }, DeciaDataType.Boolean);
        public static readonly Signature Input_Int__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Double__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal }, DeciaDataType.Decimal);
        public static readonly Signature Input_TimeSpan__Return_TimeSpan = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.TimeSpan }, DeciaDataType.TimeSpan);
        public static readonly Signature Input_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text }, DeciaDataType.Text);

        public StructuralSumOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "SUM";
            m_LongName = "Sum";
            m_Description = "Aggregates across Structural Dimension(s) by summing values.";
            m_Category = StructureUtils.StructureCategoryName;
            m_ValidProcessingType = ProcessingType.Normal;
            m_QueryType_ForSqlExport = SqlQueryType.SelectAggregation;
            m_StructuralOperationType = OperationType.Aggregation;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool__Return_Bool);
            validSignatures.Add(Input_Int__Return_Int);
            validSignatures.Add(Input_Double__Return_Double);
            validSignatures.Add(Input_TimeSpan__Return_TimeSpan);
            validSignatures.Add(Input_Text__Return_Text);

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
                || SignatureSpecification.IsSpecificInstanceOf(Input_TimeSpan__Return_TimeSpan, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Text__Return_Text, signature))
            {
                IList<ChronometricValue> aggregationValuesList = inputs.Values.First().AggregationValues.Values.ToList();
                ChronometricValue result = null;

                if (aggregationValuesList.Count <= 0)
                { return defaultReturnValue; }

                for (int i = 0; i < aggregationValuesList.Count; i++)
                {
                    ChronometricValue inputValue = aggregationValuesList[i];
                    this.AssertInputValueIsNotNull(inputValue, i);

                    if (i == 0)
                    { result = inputValue; }
                    else
                    {
                        if (inputValue.DataType == DeciaDataType.Text)
                        {
                            inputValue = inputValue.CopyNew();
                            foreach (MultiTimePeriodKey timeKey in inputValue.TimeKeys)
                            {
                                string value = inputValue.GetValue(timeKey).GetValue() as string;
                                inputValue.SetValue(timeKey, ", " + value);
                            }
                        }

                        result = result + inputValue;
                    }
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

            result += arg0;

            result = string.Format("{0}({1})", DeciaBaseUtils.SumOperator, result);
            return result;
        }
    }
}