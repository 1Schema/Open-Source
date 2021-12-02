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
    public class PresentValueOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "number", "Numerical value");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, "presentDate", "Date to discount to");
        public static readonly Parameter Parameter2 = new Parameter(2, true, false, "discountRate", "Discount Rate");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2 };
        public static readonly Signature Input_Double_Double_DateTime__Return_Double = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Decimal, DeciaDataType.DateTime }, DeciaDataType.Decimal);

        public static readonly DynamicValue ZeroValue = new DynamicValue(DeciaDataType.Decimal, 0.0);
        public static readonly DynamicValue OneValue = new DynamicValue(DeciaDataType.Decimal, 1.0);
        public static readonly DynamicValue Time0MultiplierValue = OneValue;

        public PresentValueOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "PV";
            m_LongName = "Present Value";
            m_Description = "Aggregates on the Primary Time Dimension using the specified discount rate and start date.";
            m_Category = TimeUtils.TimeCategoryName;
            m_QueryType_ForSqlExport = SqlQueryType.SelectAggregation;
            m_TimeOperationType = OperationType.Aggregation;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Double_Double_DateTime__Return_Double);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            ITimeDimensionSet firstTimeDimensionality = inputs[0].TimeDimesionality;
            ITimeDimensionSet secondTimeDimensionality = inputs[1].TimeDimesionality;
            ITimeDimensionSet thirdTimeDimensionality = inputs[2].TimeDimesionality;

            ITimeDimension primaryTimeDimension = TimeDimension.EmptyPrimaryTimeDimension;
            ITimeDimension secondaryTimeDimension = firstTimeDimensionality.SecondaryTimeDimension;

            if (!firstTimeDimensionality.PrimaryTimeDimension.HasTimeValue
                || (secondTimeDimensionality == null)
                || (thirdTimeDimensionality == null))
            {
                resultingTimeDimensionality = null;
                return false;
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

            CompoundUnit unit = null;
            IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out unit);

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Double_Double_DateTime__Return_Double, signature))
            {
                ChronometricValue input = inputs[0].ChronometricValue;
                ChronometricValue discountRate = inputs[1].ChronometricValue;
                DateTime presentDate = inputs[2].ChronometricValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<DateTime>();
                ChronometricValue result = null;
                ITimeDimensionSet resultingTimeDimensionality = null;

                IsTimeDimensionalityValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out resultingTimeDimensionality);

                if (!input.PrimaryTimeDimension.HasTimeValue)
                { return new OperationMember(input.Copy(), unit); }

                IList<MultiTimePeriodKey> timeKeys = input.GetTimeKeysForIteration(discountRate);
                SortedDictionary<MultiTimePeriodKey, int> timeKeyDistances = new SortedDictionary<MultiTimePeriodKey, int>();
                SortedDictionary<int, MultiTimePeriodKey> distanceTimeKeys = new SortedDictionary<int, MultiTimePeriodKey>();

                foreach (MultiTimePeriodKey timeKey in timeKeys)
                {
                    TimePeriodType periodType = timeKey.PrimaryTimePeriod.InferredPeriodType.Value;
                    int distance = timeKey.PrimaryTimePeriod.GetDistance(presentDate, periodType);
                    timeKeyDistances.Add(timeKey, distance);
                    distanceTimeKeys.Add(distance, timeKey);
                }

                SortedDictionary<int, DynamicValue> periodTimeMultipliers = new SortedDictionary<int, DynamicValue>();
                foreach (MultiTimePeriodKey timeKey in timeKeys)
                {
                    int distance = timeKeyDistances[timeKey];
                    DynamicValue periodMultiplierValue = null;

                    if (distance == 0)
                    {
                        periodMultiplierValue = Time0MultiplierValue;
                    }
                    else
                    {
                        DynamicValue discountRateValue = discountRate.GetValue(timeKey);
                        periodMultiplierValue = (OneValue / (OneValue + discountRateValue));
                    }

                    periodTimeMultipliers.Add(distance, periodMultiplierValue);
                }

                SortedDictionary<int, DynamicValue> discountingTimeMultipliers = new SortedDictionary<int, DynamicValue>();
                foreach (int distance in periodTimeMultipliers.Keys)
                {
                    int current = distance;
                    int step = -1;
                    DynamicValue discountingMultiplierValue = Time0MultiplierValue;

                    if (current < 0)
                    { step = 1; }

                    while (current != 0)
                    {
                        discountingMultiplierValue = discountingMultiplierValue * periodTimeMultipliers[current];
                        current += step;
                    }
                    discountingTimeMultipliers.Add(distance, discountingMultiplierValue);
                }

                SortedDictionary<MultiTimePeriodKey, DynamicValue> timeKeyResults = new SortedDictionary<MultiTimePeriodKey, DynamicValue>();
                foreach (MultiTimePeriodKey timeKey in timeKeys)
                {
                    DynamicValue inputValue = input.GetValue(timeKey);
                    int distance = timeKeyDistances[timeKey];
                    DynamicValue discountingMultiplierValue = discountingTimeMultipliers[distance];

                    DynamicValue resultValue = inputValue * discountingMultiplierValue;
                    timeKeyResults.Add(timeKey, resultValue);
                }

                HashSet<Nullable<TimePeriod>> secondaryPeriods = new HashSet<Nullable<TimePeriod>>();
                foreach (MultiTimePeriodKey timeKey in timeKeys)
                {
                    secondaryPeriods.Add(timeKey.NullableSecondaryTimePeriod);
                }

                result = new ChronometricValue(dataProvider.ProjectGuid, dataProvider.RevisionNumber_NonNull);
                result.DataType = input.DataType;
                result.ReDimension(resultingTimeDimensionality);
                result.Unit = input.Unit;

                foreach (Nullable<TimePeriod> secondaryPeriod in secondaryPeriods)
                {
                    MultiTimePeriodKey resultingTimeKey = new MultiTimePeriodKey(null, secondaryPeriod);
                    DynamicValue resultingValue = ZeroValue;

                    foreach (MultiTimePeriodKey timeKey in timeKeyResults.Keys)
                    {
                        if (timeKey.NullableSecondaryTimePeriod != secondaryPeriod)
                        { continue; }

                        resultingValue = resultingValue + timeKeyResults[timeKey];
                    }

                    result.SetValue(resultingTimeKey, resultingValue);
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

            var refsUsed = parentFormula.Arguments.Values.Where(x => x.ArgumentType == ArgumentType.ReferencedId).Select(x => x.ReferencedModelObject).ToList();
            var refTimeDimSets = refsUsed.ToDictionary(x => x, x => dataProvider.GetAssessedTimeDimensions(x));
            var mainRef = (ModelObjectReference?)null;
            var mainTimeDimSet = (ITimeDimensionSet)null;

            foreach (var refTimeDimSet in refTimeDimSets)
            {
                if (!mainRef.HasValue)
                {
                    mainRef = refTimeDimSet.Key;
                    mainTimeDimSet = refTimeDimSet.Value;
                    continue;
                }

                if (mainTimeDimSet.PrimaryTimeDimension.CompareTimeTo(refTimeDimSet.Value.PrimaryTimeDimension) == TimeComparisonResult.ThisIsLessGranular)
                {
                    mainRef = refTimeDimSet.Key;
                    mainTimeDimSet = refTimeDimSet.Value;
                }
            }

            var argInfo = exportInfo.ArgumentInfos[mainRef.Value];
            argInfo.TD1_TimePeriod_IsUsed = true;
            argInfo.TD1_TimePeriodType_IsUsed = true;

            var startDate = string.Format("{0}.[{1}]", argInfo.TD1_TimePeriod_TableAlias, SqlFormulaInfo.StartDate_ColumnName);
            var datePartValue = string.Format("{0}.[{1}]", argInfo.TD1_TimePeriodType_TableAlias, SqlFormulaInfo.DatePartValue_ColumnName);
            var datePartMultiplier = string.Format("{0}.[{1}]", argInfo.TD1_TimePeriodType_TableAlias, SqlFormulaInfo.DatePartMultiplier_ColumnName);

            var primaryTimeDimensions = refTimeDimSets.Select(x => x.Value.PrimaryTimeDimension).Where(x => x.HasTimeValue && x.NullableTimePeriodType.HasValue).ToList();
            var hasPrimaryTimeValue = (primaryTimeDimensions.Count > 0);

            if (!hasPrimaryTimeValue)
            {
                result = string.Format("{0}", arg0);
            }
            else
            {
                var maxTimePeriodType = (TimePeriodType)primaryTimeDimensions.Select(x => (int)x.TimePeriodType).Max();
                var formatString = string.Empty;

                if (maxTimePeriodType == TimePeriodType.Years)
                { formatString = "{0} * POWER(1.0 / (1.0 + {1}), ({5} * DATEDIFF(year, {2}, {3})))"; }
                else if (maxTimePeriodType == TimePeriodType.HalfYears)
                { formatString = "{0} * POWER(1.0 / (1.0 + {1}), ({5} * DATEDIFF(year, {2}, {3})))"; }
                else if (maxTimePeriodType == TimePeriodType.QuarterYears)
                { formatString = "{0} * POWER(1.0 / (1.0 + {1}), ({5} * DATEDIFF(quarter, {2}, {3})))"; }
                else if (maxTimePeriodType == TimePeriodType.Months)
                { formatString = "{0} * POWER(1.0 / (1.0 + {1}), ({5} * DATEDIFF(month, {2}, {3})))"; }
                else if (maxTimePeriodType == TimePeriodType.HalfMonths)
                { formatString = "{0} * POWER(1.0 / (1.0 + {1}), ({5} * DATEDIFF(month, {2}, {3})))"; }
                else if (maxTimePeriodType == TimePeriodType.QuarterMonths)
                { formatString = "{0} * POWER(1.0 / (1.0 + {1}), ({5} * DATEDIFF(week, {2}, {3})))"; }
                else if (maxTimePeriodType == TimePeriodType.Days)
                { formatString = "{0} * POWER(1.0 / (1.0 + {1}), ({5} * DATEDIFF(day, {2}, {3})))"; }
                else
                { throw new InvalidOperationException("Unsupported TimePeriodType encountered."); }

                result = string.Format(formatString, arg0, arg1, arg2, startDate, datePartValue, datePartMultiplier, DeciaBaseUtils.NullValue);
                result = string.Format("{0}({1})", DeciaBaseUtils.SumOperator, result);
            }
            return result;
        }
    }
}