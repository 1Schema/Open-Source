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
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Time.Introspection
{
    public class CurrentPeriodIndexOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, false, "timeDimensionNumber", "Time Dimension Number");
        public static readonly Parameter Parameter1 = new Parameter(1, false, false, "valueToUse", "Value to introspect");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1 };
        public static readonly Signature Input_Intg_Bool__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Boolean }, DeciaDataType.Integer);
        public static readonly Signature Input_Intg_Intg__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Intg_Dubl__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Decimal }, DeciaDataType.Integer);
        public static readonly Signature Input_Intg_Guid__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.UniqueID }, DeciaDataType.Integer);
        public static readonly Signature Input_Intg_Date__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.DateTime }, DeciaDataType.Integer);
        public static readonly Signature Input_Intg_TSpn__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.TimeSpan }, DeciaDataType.Integer);
        public static readonly Signature Input_Intg_Text__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Text }, DeciaDataType.Integer);
        public static readonly Signature Input_Dubl_Bool__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Boolean }, DeciaDataType.Integer);
        public static readonly Signature Input_Dubl_Intg__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Integer }, DeciaDataType.Integer);
        public static readonly Signature Input_Dubl_Dubl__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Decimal }, DeciaDataType.Integer);
        public static readonly Signature Input_Dubl_Guid__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.UniqueID }, DeciaDataType.Integer);
        public static readonly Signature Input_Dubl_Date__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.DateTime }, DeciaDataType.Integer);
        public static readonly Signature Input_Dubl_TSpn__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.TimeSpan }, DeciaDataType.Integer);
        public static readonly Signature Input_Dubl_Text__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Text }, DeciaDataType.Integer);

        public CurrentPeriodIndexOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "TPER_POSITION";
            m_LongName = "Current Period Position Index";
            m_Description = "Gets the Period Index of the current computation Period for a given Time Dimension number.";
            m_Category = TimeUtils.TimeCategoryName;
            m_TimeOperationType = OperationType.Introspection;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Intg_Bool__Return_Intg);
            validSignatures.Add(Input_Intg_Intg__Return_Intg);
            validSignatures.Add(Input_Intg_Dubl__Return_Intg);
            validSignatures.Add(Input_Intg_Guid__Return_Intg);
            validSignatures.Add(Input_Intg_Date__Return_Intg);
            validSignatures.Add(Input_Intg_TSpn__Return_Intg);
            validSignatures.Add(Input_Intg_Text__Return_Intg);
            validSignatures.Add(Input_Dubl_Bool__Return_Intg);
            validSignatures.Add(Input_Dubl_Intg__Return_Intg);
            validSignatures.Add(Input_Dubl_Dubl__Return_Intg);
            validSignatures.Add(Input_Dubl_Guid__Return_Intg);
            validSignatures.Add(Input_Dubl_Date__Return_Intg);
            validSignatures.Add(Input_Dubl_TSpn__Return_Intg);
            validSignatures.Add(Input_Dubl_Text__Return_Intg);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            if (!inputs.ContainsKey(Parameter1.Index))
            {
                resultingTimeDimensionality = dataProvider.GetSelfTimeDimensionality(currentState);
                return true;
            }
            else
            {
                var valueToUse = inputs[Parameter1.Index];
                resultingTimeDimensionality = valueToUse.TimeDimesionality;
                return true;
            }
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

            ITimeDimensionSet timeDimensionSet = null;
            IsTimeDimensionalityValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out timeDimensionSet);

            CompoundUnit unit = null;
            IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out unit);

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Bool__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Intg__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Dubl__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Guid__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Date__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_TSpn__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Text__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Bool__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Intg__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Dubl__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Guid__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Date__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_TSpn__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Text__Return_Intg, signature))
            {
                var timeDimensionNumberValue = inputs[Parameter0.Index].ChronometricValue;
                var valueToUseValue = (inputs.ContainsKey(Parameter1.Index)) ? inputs[Parameter1.Index].ChronometricValue : null;

                var timeDimensionNumber = timeDimensionNumberValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<int>();
                var timeDimensionType_ToShow = TimeDimensionTypeUtils.GetTimeDimensionTypeForNumber(timeDimensionNumber);

                var primaryTimePeriods_In = timeDimensionSet.PrimaryTimeDimension.GenerateRelevantTimePeriods(currentState.PrimaryPeriod);
                var secondaryTimePeriods_In = timeDimensionSet.SecondaryTimeDimension.GenerateRelevantTimePeriods(currentState.SecondaryPeriod);
                var timePeriods_ToSearch = (timeDimensionType_ToShow == TimeDimensionType.Primary) ? timeDimensionSet.PrimaryTimeDimension.GenerateRelevantTimePeriods(null) : timeDimensionSet.SecondaryTimeDimension.GenerateRelevantTimePeriods(null);
                var timeKeys_Out = new List<MultiTimePeriodKey>();

                var result = new ChronometricValue(dataProvider.ProjectGuid, dataProvider.RevisionNumber_NonNull);
                result.DataType = DeciaDataType.Integer;
                result.ReDimension(timeDimensionSet);

                foreach (var primaryTimePeriod_In in primaryTimePeriods_In)
                {
                    foreach (var secondaryTimePeriod_In in secondaryTimePeriods_In)
                    {
                        var timeKey_In = new MultiTimePeriodKey(primaryTimePeriod_In, secondaryTimePeriod_In);

                        var timePeriod_ToShow = (timeDimensionType_ToShow == TimeDimensionType.Primary) ? primaryTimePeriod_In : secondaryTimePeriod_In;
                        var timePeriodIndex_Out = 0;

                        if (!timePeriod_ToShow.IsForever)
                        { timePeriodIndex_Out = timePeriods_ToSearch.IndexOf(timePeriod_ToShow); }

                        var timeKey_Out = new MultiTimePeriodKey(primaryTimePeriod_In, secondaryTimePeriod_In);
                        timeKeys_Out.Add(timeKey_Out);

                        result.SetValue(timeKey_Out, timePeriodIndex_Out);
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

            var timeDimensionNumberArg = callingExpression.GetArgument(Parameter0.Index);
            var timeDimensionNumber = timeDimensionNumberArg.DirectValue.GetTypedValue<int>();
            var timeDimensionType = TimeDimensionTypeUtils.GetTimeDimensionTypeForNumber(timeDimensionNumber);

            var mostGranularRef = (ModelObjectReference?)currentState.VariableTemplateRef;
            var isForever = true;
            var timePeriod_TableAlias = string.Empty;

            if (callingExpression.HasArgument(Parameter1.Index))
            {
                var mostGranularTimeDimensionality = OperationUtils.GetNestedArguments_MostGranularTimeDimensionality(dataProvider, currentState, parentFormula, callingExpression);
                mostGranularRef = mostGranularTimeDimensionality[timeDimensionType].Key;
            }

            if (mostGranularRef.HasValue)
            {
                var argInfo = exportInfo.ArgumentInfos[mostGranularRef.Value];
                timePeriod_TableAlias = argInfo.GetTimePeriod_TableAlias(timeDimensionNumber, true);
                isForever = string.IsNullOrWhiteSpace(timePeriod_TableAlias);
            }

            if (isForever)
            {
                result = DeciaDataType.DateTime.ToStringValue(TimePeriod.DefaultIndex);
            }
            else
            {
                result = string.Format("{0}.[{1}]", timePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_OrderIndex_ColumnName);
            }
            return result;
        }
    }
}