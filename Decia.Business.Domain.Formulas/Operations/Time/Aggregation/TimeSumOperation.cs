using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Time.Aggregation
{
    public class TimeSumOperation : OperationBase
    {
        public const bool DefaultParameter1Value = true;
        public const bool DefaultParameter2Value = true;

        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "number", "Numerical value");
        public static readonly Parameter Parameter1 = new Parameter(1, false, false, false, "useTimeDim1", "Whether to use Primary Time Dimension");
        public static readonly Parameter Parameter2 = new Parameter(2, false, false, false, "useTimeDim2", "Whether to use Secondary Time Dimension");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2 };
        public static readonly Signature Input_Int_Bool_Bool__Return_Int = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Boolean, DeciaDataType.Boolean }, DeciaDataType.Integer);
        public static readonly Signature Input_Double_Bool_Bool__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Boolean, DeciaDataType.Boolean }, DeciaDataType.Decimal);

        public TimeSumOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "T_SUM";
            m_LongName = "Time Sum";
            m_Description = "Aggregates on a Time Dimension(s) using the SUM operation.";
            m_Category = TimeUtils.TimeCategoryName;
            m_QueryType_ForSqlExport = SqlQueryType.SelectAggregation;
            m_TimeOperationType = OperationType.Aggregation;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Int_Bool_Bool__Return_Int);
            validSignatures.Add(Input_Double_Bool_Bool__Return_Double);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            ITimeDimensionSet baseTimeDimensionality = inputs.First().Value.TimeDimesionality;
            bool aggregatePrimaryTimeDimension = (inputs.Count > 1) ? (bool)inputs[1].ChronometricValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetValue() : DefaultParameter1Value;
            bool aggregateSecondaryTimeDimension = (inputs.Count > 2) ? (bool)inputs[2].ChronometricValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetValue() : DefaultParameter2Value;

            ITimeDimension primaryTimeDimension = TimeDimension.EmptyPrimaryTimeDimension;
            if (!aggregatePrimaryTimeDimension)
            { primaryTimeDimension = baseTimeDimensionality.PrimaryTimeDimension; }
            else if (currentState.PrimaryPeriodType.HasValue)
            {
                var isValid = baseTimeDimensionality.PrimaryTimeDimension.NullableTimePeriodType.HasValue;
                if (isValid)
                {
                    var timeComparison = currentState.PrimaryPeriodType.Value.CompareTimeTo(baseTimeDimensionality.PrimaryTimeDimension.NullableTimePeriodType.Value);
                    isValid = ((timeComparison != TimeComparisonResult.NotComparable) && (timeComparison != TimeComparisonResult.ThisIsMoreGranular));
                }

                if (!isValid)
                {
                    resultingTimeDimensionality = new TimeDimensionSet(baseTimeDimensionality.PrimaryTimeDimension, baseTimeDimensionality.SecondaryTimeDimension);
                    return false;
                }

                primaryTimeDimension = new TimeDimension(baseTimeDimensionality.PrimaryTimeDimension, currentState.PrimaryPeriodType.Value);
            }

            ITimeDimension secondaryTimeDimension = TimeDimension.EmptySecondaryTimeDimension;
            if (!aggregateSecondaryTimeDimension)
            { secondaryTimeDimension = baseTimeDimensionality.SecondaryTimeDimension; }
            else if (currentState.SecondaryPeriodType.HasValue)
            {
                var isValid = baseTimeDimensionality.SecondaryTimeDimension.NullableTimePeriodType.HasValue;
                if (isValid)
                {
                    var timeComparison = currentState.SecondaryPeriodType.Value.CompareTimeTo(baseTimeDimensionality.SecondaryTimeDimension.NullableTimePeriodType.Value);
                    isValid = ((timeComparison != TimeComparisonResult.NotComparable) && (timeComparison != TimeComparisonResult.ThisIsMoreGranular));
                }

                if (!isValid)
                {
                    resultingTimeDimensionality = new TimeDimensionSet(baseTimeDimensionality.PrimaryTimeDimension, baseTimeDimensionality.SecondaryTimeDimension);
                    return false;
                }

                secondaryTimeDimension = new TimeDimension(baseTimeDimensionality.SecondaryTimeDimension, currentState.SecondaryPeriodType.Value);
            }

            if (secondaryTimeDimension.HasTimeValue && !primaryTimeDimension.HasTimeValue)
            {
                primaryTimeDimension = new TimeDimension(TimeDimensionType.Primary, secondaryTimeDimension.NullableTimeValueType, secondaryTimeDimension.NullableTimePeriodType, secondaryTimeDimension.NullableFirstPeriodStartDate, secondaryTimeDimension.NullableLastPeriodEndDate);
                secondaryTimeDimension = new TimeDimension(TimeDimensionType.Secondary);
            }

            resultingTimeDimensionality = new TimeDimensionSet(primaryTimeDimension, secondaryTimeDimension);
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

            ITimeDimensionSet timeDimensionSet = null;
            IsTimeDimensionalityValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out timeDimensionSet);

            CompoundUnit unit = null;
            IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out unit);

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Int_Bool_Bool__Return_Int, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Bool_Bool__Return_Double, signature))
            {
                var inputValue = inputs[0].ChronometricValue;
                var aggregatePrimaryTimeDimension = (inputs.Count > 1) ? (bool)inputs[1].ChronometricValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetValue() : DefaultParameter1Value;
                var aggregateSecondaryTimeDimension = (inputs.Count > 2) ? (bool)inputs[2].ChronometricValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetValue() : DefaultParameter2Value;
                var result = (ChronometricValue)null;

                var primaryTimeDimension = new TimeDimension(timeDimensionSet.PrimaryTimeDimension, inputValue.PrimaryTimeDimension.NullableFirstPeriodStartDate, inputValue.PrimaryTimeDimension.NullableLastPeriodEndDate);
                var secondaryTimeDimension = new TimeDimension(timeDimensionSet.SecondaryTimeDimension, inputValue.SecondaryTimeDimension.NullableFirstPeriodStartDate, inputValue.SecondaryTimeDimension.NullableLastPeriodEndDate);

                var isOverEntirePrimaryDimension = (!currentState.PrimaryPeriodType.HasValue && inputValue.PrimaryTimeDimension.HasTimeValue);
                var isOverEntireSecondaryDimension = (!currentState.SecondaryPeriodType.HasValue && inputValue.SecondaryTimeDimension.HasTimeValue);


                result = new ChronometricValue(dataProvider.ProjectGuid, dataProvider.RevisionNumber_NonNull);
                result.DataType = inputValue.DataType;
                result.ReDimension(primaryTimeDimension, secondaryTimeDimension);

                if (!isOverEntirePrimaryDimension && !isOverEntireSecondaryDimension)
                {
                    var incomingValue = inputValue.GetValue(currentState.TimeKey);

                    MultiTimePeriodKey timeKeyOfResult;
                    var existingValue = result.GetValue(currentState.TimeKey, out timeKeyOfResult);

                    result.SetValue(timeKeyOfResult, incomingValue);
                }
                else if (isOverEntirePrimaryDimension && !isOverEntireSecondaryDimension && inputValue.SecondaryTimeDimension.HasTimeValue)
                {
                    foreach (MultiTimePeriodKey timeKey in inputValue.TimeKeys)
                    {
                        var incomingValue = inputValue.GetValue(timeKey);
                        var timeKey_Flipped = new MultiTimePeriodKey(timeKey.NullableSecondaryTimePeriod, timeKey.NullablePrimaryTimePeriod);

                        MultiTimePeriodKey timeKeyOfResult;
                        var existingValue = result.GetValue(timeKey_Flipped, out timeKeyOfResult);

                        if (existingValue.IsNull)
                        { result.SetValue(timeKeyOfResult, incomingValue); }
                        else
                        {
                            existingValue += incomingValue;
                            result.SetValue(timeKeyOfResult, existingValue);
                        }
                    }
                }
                else
                {
                    foreach (MultiTimePeriodKey timeKey in inputValue.TimeKeys)
                    {
                        var incomingValue = inputValue.GetValue(timeKey);

                        MultiTimePeriodKey timeKeyOfResult;
                        var existingValue = result.GetValue(timeKey, out timeKeyOfResult);

                        if (existingValue.IsNull)
                        { result.SetValue(timeKeyOfResult, incomingValue); }
                        else
                        {
                            existingValue += incomingValue;
                            result.SetValue(timeKeyOfResult, existingValue);
                        }
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
            var useTD1 = false;
            var useTD2 = false;

            var argsByIndex = callingExpression.ArgumentsByIndex;
            if (argsByIndex.ContainsKey(Parameter1.Index))
            { useTD1 = callingExpression.GetArgument(Parameter1.Index).DirectValue.GetTypedValue<bool>(); }
            if (argsByIndex.ContainsKey(Parameter2.Index))
            { useTD2 = callingExpression.GetArgument(Parameter2.Index).DirectValue.GetTypedValue<bool>(); }

            result += arg0;

            result = string.Format("{0}({1})", DeciaBaseUtils.SumOperator, result);
            return result;
        }
    }
}