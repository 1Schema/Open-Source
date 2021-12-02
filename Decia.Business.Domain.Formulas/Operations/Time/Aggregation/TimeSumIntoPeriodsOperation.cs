using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Time.Aggregation
{
    public class TimeSumIntoPeriodsOperation : OperationBase
    {
        public const int DefaultParameter3Value = (int)TimePeriodType.Years;
        public const int DefaultParameter5Value = (int)TimePeriodType.Years;

        public static int RequiredArgCount { get { return Parameter2.Index + 1; } }

        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "condition", "Condition value that must be met for inclusion in aggregation");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, "value", "Value to Aggregate");
        public static readonly Parameter Parameter2 = new Parameter(2, true, false, "date1", "The Primary Date to Aggregate on");
        public static readonly Parameter Parameter3 = new Parameter(3, false, false, "date2", "The Secondary Date to Aggregate on");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2, Parameter3 };
        public static readonly Signature Input_Bool_Bool_DateTime_DateTime__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Boolean, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Boolean);
        public static readonly Signature Input_Bool_Int_DateTime_DateTime__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Integer, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Integer);
        public static readonly Signature Input_Bool_Double_DateTime_DateTime__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Decimal, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Decimal);
        public static readonly Signature Input_Int_Bool_DateTime_DateTime__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Boolean, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Boolean);
        public static readonly Signature Input_Int_Int_DateTime_DateTime__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Integer, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Integer);
        public static readonly Signature Input_Int_Double_DateTime_DateTime__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Decimal, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Decimal);
        public static readonly Signature Input_Double_Bool_DateTime_DateTime__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Boolean, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Boolean);
        public static readonly Signature Input_Double_Int_DateTime_DateTime__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Integer, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Integer);
        public static readonly Signature Input_Double_Double_DateTime_DateTime__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Decimal, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Decimal);

        public TimeSumIntoPeriodsOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "SUM_INTO_TIME_PERIODS";
            m_LongName = "Sum into Time Periods";
            m_Description = "Aggregates records on both Structural and Time Dimension(s) using the SUM operation.";
            m_Category = TimeUtils.TimeCategoryName;
            m_ValidProcessingType = ProcessingType.Normal;
            m_QueryType_ForSqlExport = SqlQueryType.SelectAggregation;
            m_StructuralOperationType = OperationType.Aggregation;
            m_TimeOperationType = OperationType.Aggregation;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool_Bool_DateTime_DateTime__Return_Bool);
            validSignatures.Add(Input_Bool_Int_DateTime_DateTime__Return_Int);
            validSignatures.Add(Input_Bool_Double_DateTime_DateTime__Return_Double);
            validSignatures.Add(Input_Int_Bool_DateTime_DateTime__Return_Bool);
            validSignatures.Add(Input_Int_Int_DateTime_DateTime__Return_Int);
            validSignatures.Add(Input_Int_Double_DateTime_DateTime__Return_Double);
            validSignatures.Add(Input_Double_Bool_DateTime_DateTime__Return_Bool);
            validSignatures.Add(Input_Double_Int_DateTime_DateTime__Return_Int);
            validSignatures.Add(Input_Double_Double_DateTime_DateTime__Return_Double);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected void GetOptionalArgumentIndices(IDictionary<int, OperationMember> inputs, out int? secondary_Date_Index)
        {
            secondary_Date_Index = null;

            if (inputs.Count <= RequiredArgCount)
            { return; }

            var arg3_DataType = inputs.ContainsKey(Parameter3.Index) ? inputs[Parameter3.Index].DataType : (DeciaDataType?)null;

            if (arg3_DataType == DeciaDataType.DateTime)
            { secondary_Date_Index = Parameter3.Index; }
            else
            { throw new InvalidOperationException("Could not locate secondary Date argument."); }
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            if (inputs.Count < RequiredArgCount)
            {
                resultingTimeDimensionality = null;
                return false;
            }

            int? secondary_Date_Index;
            GetOptionalArgumentIndices(inputs, out secondary_Date_Index);
            bool hasSecondaryTimeDimension = (secondary_Date_Index.HasValue);

            var assessedTimeDimensions = dataProvider.GetAssessedTimeDimensions(currentState.VariableTemplateRef);
            var assessedPrimaryTimeDimension = assessedTimeDimensions.PrimaryTimeDimension;
            var assessedSecondaryTimeDimension = assessedTimeDimensions.SecondaryTimeDimension;

            ITimeDimensionSet conditionTimeDimensionality = inputs.ElementAt(Parameter0.Index).Value.TimeDimesionality;
            ITimeDimensionSet valueTimeDimensionality = inputs.ElementAt(Parameter1.Index).Value.TimeDimesionality;
            ITimeDimensionSet primaryDateTimeDimensionality = inputs.ElementAt(Parameter2.Index).Value.TimeDimesionality;
            ITimeDimensionSet secondaryDateTimeDimensionality = (secondary_Date_Index.HasValue) ? inputs.ElementAt(secondary_Date_Index.Value).Value.TimeDimesionality : null;

            var primaryTimePeriodType = assessedPrimaryTimeDimension.HasTimeValue ? assessedPrimaryTimeDimension.TimePeriodType : (TimePeriodType?)null;
            var secondaryTimePeriodType = assessedSecondaryTimeDimension.HasTimeValue ? assessedSecondaryTimeDimension.TimePeriodType : (TimePeriodType?)null;

            var inferredPrimaryTimeDimension = new TimeDimension(TimeDimensionType.Primary, TimeValueType.PeriodValue, primaryTimePeriodType, currentState.ModelStartDate, currentState.ModelEndDate);
            var inferredSecondaryTimeDimension = (hasSecondaryTimeDimension) ? new TimeDimension(TimeDimensionType.Secondary, TimeValueType.PeriodValue, secondaryTimePeriodType, currentState.ModelStartDate, currentState.ModelEndDate) : TimeDimension.EmptySecondaryTimeDimension;

            var resultingPrimaryTimeDimension = (inferredPrimaryTimeDimension.CompareTimeTo(assessedPrimaryTimeDimension) == TimeComparisonResult.ThisIsMoreGranular) ? inferredPrimaryTimeDimension : assessedPrimaryTimeDimension;
            var resultingSecondaryTimeDimension = (inferredSecondaryTimeDimension.CompareTimeTo(assessedSecondaryTimeDimension) == TimeComparisonResult.ThisIsMoreGranular) ? inferredSecondaryTimeDimension : assessedSecondaryTimeDimension;
            resultingTimeDimensionality = new TimeDimensionSet(resultingPrimaryTimeDimension, resultingSecondaryTimeDimension);

            return true;
        }

        protected override bool IsUnitValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out CompoundUnit resultingUnit)
        {
            if (inputs.Count < RequiredArgCount)
            {
                resultingUnit = CompoundUnit.GetGlobalScalarUnit(currentState.ProjectGuid, currentState.RevisionNumber);
                return false;
            }

            resultingUnit = inputs.ElementAt(Parameter1.Index).Value.Unit;

            if (resultingUnit == null)
            { resultingUnit = CompoundUnit.GetGlobalScalarUnit(currentState.ProjectGuid, currentState.RevisionNumber); }

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

            int? secondary_Date_Index;
            GetOptionalArgumentIndices(inputs, out secondary_Date_Index);
            bool hasSecondaryTimeDimension = (secondary_Date_Index.HasValue);

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Bool_DateTime_DateTime__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Int_DateTime_DateTime__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Double_DateTime_DateTime__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int_Bool_DateTime_DateTime__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int_Int_DateTime_DateTime__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int_Double_DateTime_DateTime__Return_Double, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Bool_DateTime_DateTime__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Int_DateTime_DateTime__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Double_DateTime_DateTime__Return_Double, signature))
            {
                OperationMember conditionArg = inputs.ElementAt(Parameter0.Index).Value;
                OperationMember valueArg = inputs.ElementAt(Parameter1.Index).Value;
                OperationMember primaryDateArg = inputs.ElementAt(Parameter2.Index).Value;
                OperationMember secondaryDateArg = (hasSecondaryTimeDimension) ? inputs.ElementAt(secondary_Date_Index.Value).Value : (OperationMember)null;
                ChronometricValue resultMatrix = defaultReturnValue.ChronometricValue;

                var argsToExpand = (secondaryDateArg != null) ? new OperationMember[] { conditionArg, valueArg, primaryDateArg, secondaryDateArg } : new OperationMember[] { conditionArg, valueArg, primaryDateArg };
                var expandedKeys = OperationUtils.GetExpandedKeysForAggregation(dataProvider, currentState, argsToExpand);

                foreach (var aggrKey in expandedKeys)
                {
                    var conditionMatrix = conditionArg.GetAggregationValue(dataProvider, currentState, aggrKey);
                    var valueMatrix = valueArg.GetAggregationValue(dataProvider, currentState, aggrKey);
                    var primaryDateMatrix = primaryDateArg.GetAggregationValue(dataProvider, currentState, aggrKey);
                    var secondaryDateMatrix = (hasSecondaryTimeDimension) ? secondaryDateArg.GetAggregationValue(dataProvider, currentState, aggrKey) : (ChronometricValue)null;

                    if ((conditionMatrix == (object)null) || (valueMatrix == (object)null) || (primaryDateMatrix == (object)null) || (hasSecondaryTimeDimension && (secondaryDateMatrix == (object)null)))
                    { continue; }

                    DynamicValue condition = conditionMatrix.GetValue(currentState.TimeKey);
                    if (!condition.GetTypedValue<bool>())
                    { continue; }

                    DynamicValue value = valueMatrix.GetValue(currentState.TimeKey);
                    DateTime primaryDate = primaryDateMatrix.GetValue(currentState.TimeKey).GetTypedValue<DateTime>();
                    Nullable<DateTime> secondaryDate = (hasSecondaryTimeDimension) ? secondaryDateMatrix.GetValue(currentState.TimeKey).GetTypedValue<DateTime>() : (DateTime?)null;

                    var resultTimeKeys = (hasSecondaryTimeDimension) ? resultMatrix.TimeKeys.GetRelevantTimeKeys(primaryDate.ConvertToPeriod(), secondaryDate.Value.ConvertToPeriod()) : resultMatrix.TimeKeys.GetRelevantTimeKeys(primaryDate.ConvertToPeriod(), null);
                    if (resultTimeKeys.Count < 1)
                    {
                        continue;
                    }

                    var resultTimeKey = resultTimeKeys.First();
                    var resultValue = resultMatrix.GetValue(resultTimeKey);

                    resultValue = resultValue + value;
                    resultMatrix.SetValue(resultTimeKey, resultValue);
                }

                return new OperationMember(resultMatrix, unit);
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;
            var conditionText = argumentTextByIndex[Parameter0.Index];
            var valueText = argumentTextByIndex[Parameter1.Index];
            var useTD1 = argumentTextByIndex.ContainsKey(Parameter2.Index);
            var useTD2 = argumentTextByIndex.ContainsKey(Parameter3.Index);

            var conditionArg = callingExpression.GetArgument(Parameter0.Index);
            var conditionText_Adj = string.Empty;

            if (conditionArg.ArgumentType != ArgumentType.DirectValue)
            { conditionText_Adj = conditionText; }
            else
            {
                var conditionValue = conditionArg.DirectValue.GetValue();
                var isTrue = Convert.ToBoolean(conditionValue);
                conditionText_Adj = (isTrue) ? "1 = 1" : "1 = 0";
            }

            exportInfo.Filtering_WhereText = conditionText_Adj;
            exportInfo.Filtering_ResultColumnText = valueText;

            if (useTD1)
            { exportInfo.Aggregation_PrimaryDateText = argumentTextByIndex[Parameter2.Index]; }
            if (useTD2)
            { exportInfo.Aggregation_SecondaryDateText = argumentTextByIndex[Parameter3.Index]; }

            result += valueText;

            result = string.Format("{0}({1})", DeciaBaseUtils.SumOperator, result);
            return result;
        }
    }
}