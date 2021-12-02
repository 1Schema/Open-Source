using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Structure
{
    public class StructuralFirstOperation : OperationBase
    {
        public const bool Default_UseOrdering = false;
        public const int Default_SkipCount = 0;

        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "value", "Value to aggregate");
        public static readonly Parameter Parameter1 = new Parameter(1, false, false, false, "useOrdering", "Order the values before choosing a specific value");
        public static readonly Parameter Parameter2 = new Parameter(2, false, false, false, "skipCount", "Number of values to skip before choosing a specific value");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2 };
        public static readonly Signature Input_Bool_Bool_Int__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.Boolean);
        public static readonly Signature Input_Int_Bool_Int__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Double_Bool_Int__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.Decimal);
        public static readonly Signature Input_Guid_Bool_Int__Return_Guid = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.UniqueID, DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.UniqueID);
        public static readonly Signature Input_DateTime_Bool_Int__Return_DateTime = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.DateTime, DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.DateTime);
        public static readonly Signature Input_TimeSpan_Bool_Int__Return_TimeSpan = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.TimeSpan, DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.TimeSpan);
        public static readonly Signature Input_Text_Bool_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.Text);

        public StructuralFirstOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "FIRST";
            m_LongName = "First";
            m_Description = "Aggregates across Structural Dimension(s) by selecting the first available value.";
            m_Category = StructureUtils.StructureCategoryName;
            m_ValidProcessingType = ProcessingType.Normal;
            m_QueryType_ForSqlExport = SqlQueryType.SelectOneFromMany;
            m_StructuralOperationType = OperationType.Aggregation;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool_Bool_Int__Return_Bool);
            validSignatures.Add(Input_Int_Bool_Int__Return_Int);
            validSignatures.Add(Input_Double_Bool_Int__Return_Double);
            validSignatures.Add(Input_Guid_Bool_Int__Return_Guid);
            validSignatures.Add(Input_DateTime_Bool_Int__Return_DateTime);
            validSignatures.Add(Input_TimeSpan_Bool_Int__Return_TimeSpan);
            validSignatures.Add(Input_Text_Bool_Int__Return_Text);

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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Bool_Int__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int_Bool_Int__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Bool_Int__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Guid_Bool_Int__Return_Guid, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_DateTime_Bool_Int__Return_DateTime, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_TimeSpan_Bool_Int__Return_TimeSpan, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Text_Bool_Int__Return_Text, signature))
            {
                var aggregationValuesList = inputs.Values.ElementAt(Parameter0.Index).AggregationValues.Values.ToList();
                var useOrdering = (inputs.Values.Count > Parameter1.Index) ? inputs.Values.ElementAt(Parameter1.Index).ChronometricValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<bool>() : Default_UseOrdering;
                var skipCount = (inputs.Values.Count > Parameter2.Index) ? inputs.Values.ElementAt(Parameter2.Index).ChronometricValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<int>() : Default_SkipCount;
                ChronometricValue result = null;

                if (aggregationValuesList.Count <= 0)
                { return defaultReturnValue; }
                if (aggregationValuesList.Count <= skipCount)
                { skipCount = aggregationValuesList.Count - 1; }

                if (!useOrdering && (skipCount <= 0))
                { result = aggregationValuesList.FirstOrDefault(); }
                else if (!useOrdering && (skipCount > 0))
                { result = aggregationValuesList.Skip(skipCount).FirstOrDefault(); }
                else if (useOrdering && (skipCount <= 0))
                { result = aggregationValuesList.OrderBy(x => x.GetValue(currentState.TimeKey).GetValue().ToString()).FirstOrDefault(); }
                else
                { result = aggregationValuesList.OrderBy(x => x.GetValue(currentState.TimeKey).GetValue().ToString()).Skip(skipCount).FirstOrDefault(); }

                return new OperationMember(result, unit);
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;
            var arg0 = argumentTextByIndex[Parameter0.Index];
            var arg1 = argumentTextByIndex.ContainsKey(Parameter1.Index) ? argumentTextByIndex[Parameter1.Index] : Convert.ToInt32(Default_UseOrdering).ToString();
            var arg2 = argumentTextByIndex.ContainsKey(Parameter2.Index) ? argumentTextByIndex[Parameter2.Index] : Default_SkipCount.ToString();

            var rowCount = System.Math.Max(1 + int.Parse(arg2), 1);

            exportInfo.RowLimiting_OrderByText = string.Format("ORDER BY {0} ASC", exportInfo.Filtering_ResultColumnText);
            exportInfo.RowLimiting_WhereText = string.Format("WHERE [{0}] BETWEEN {1} AND {1};", SqlFormulaInfo.RowNumber_ColumnName, rowCount);

            result += arg0;

            result = string.Format("({0})", result);
            return result;
        }
    }
}