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
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Time.Introspection
{
    public class GetTimePeriodInstanceTextOperation : OperationBase
    {
        public const string DefaultTimePeriodFormat_Full = "{0} - {1}";
        public const string DefaultTimePeriodFormat_Partial = "{0}";
        public const string DefaultDateFormat = "d";
        public const string DefaultYearFormat = "yyyy";
        public const string DefaultForeverText = TimePeriodUtils.DefaultForeverText;

        public static readonly Parameter Parameter0 = new Parameter(0, true, false, false, "timeDimensionNumber", "Time Dimension Number");
        public static readonly Parameter Parameter1 = new Parameter(1, false, false, false, "startDateFormat", "Format for Start Date");
        public static readonly Parameter Parameter2 = new Parameter(2, false, false, false, "endDateFormat", "Format for End Date");
        public static readonly Parameter Parameter3 = new Parameter(3, false, false, false, "timePeriodFormat", "Format for complete Time Period");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2, Parameter3 };
        public static readonly Signature Input_Intg_Bool_Bool_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Boolean, DeciaDataType.Boolean, DeciaDataType.Text }, DeciaDataType.Text);
        public static readonly Signature Input_Intg_Bool_Text_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Boolean, DeciaDataType.Text, DeciaDataType.Text }, DeciaDataType.Text);
        public static readonly Signature Input_Intg_Text_Bool_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Text, DeciaDataType.Boolean, DeciaDataType.Text }, DeciaDataType.Text);
        public static readonly Signature Input_Intg_Text_Text_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Text, DeciaDataType.Text, DeciaDataType.Text }, DeciaDataType.Text);
        public static readonly Signature Input_Dubl_Bool_Bool_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Boolean, DeciaDataType.Boolean, DeciaDataType.Text }, DeciaDataType.Text);
        public static readonly Signature Input_Dubl_Bool_Text_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Boolean, DeciaDataType.Text, DeciaDataType.Text }, DeciaDataType.Text);
        public static readonly Signature Input_Dubl_Text_Bool_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Text, DeciaDataType.Boolean, DeciaDataType.Text }, DeciaDataType.Text);
        public static readonly Signature Input_Dubl_Text_Text_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Text, DeciaDataType.Text, DeciaDataType.Text }, DeciaDataType.Text);

        public GetTimePeriodInstanceTextOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "TPER_TXT";
            m_LongName = "Period as Text Value";
            m_Description = "Gets the Period as text for a given Time Dimension number.";
            m_Category = TimeUtils.TimeCategoryName;
            m_TimeOperationType = OperationType.Introspection;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Intg_Bool_Bool_Text__Return_Text);
            validSignatures.Add(Input_Intg_Bool_Text_Text__Return_Text);
            validSignatures.Add(Input_Intg_Text_Bool_Text__Return_Text);
            validSignatures.Add(Input_Intg_Text_Text_Text__Return_Text);
            validSignatures.Add(Input_Dubl_Bool_Bool_Text__Return_Text);
            validSignatures.Add(Input_Dubl_Bool_Text_Text__Return_Text);
            validSignatures.Add(Input_Dubl_Text_Bool_Text__Return_Text);
            validSignatures.Add(Input_Dubl_Text_Text_Text__Return_Text);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            resultingTimeDimensionality = dataProvider.GetSelfTimeDimensionality(currentState);
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

            ITimeDimensionSet timeDimensionSet = null;
            IsTimeDimensionalityValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out timeDimensionSet);

            CompoundUnit unit = null;
            IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out unit);

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Bool_Bool_Text__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Bool_Text_Text__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Text_Bool_Text__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Text_Text_Text__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Bool_Bool_Text__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Bool_Text_Text__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Text_Bool_Text__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Text_Text_Text__Return_Text, signature))
            {
                var timeDimensionNumberValue = inputs[Parameter0.Index].ChronometricValue;
                var startDateFormatValue = (inputs.ContainsKey(Parameter1.Index)) ? inputs[Parameter1.Index].ChronometricValue : null;
                var endDateFormatValue = (inputs.ContainsKey(Parameter2.Index)) ? inputs[Parameter2.Index].ChronometricValue : null;
                var timePeriodFormatValue = (inputs.ContainsKey(Parameter3.Index)) ? inputs[Parameter3.Index].ChronometricValue : null;

                var timeDimensionNumber = timeDimensionNumberValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<int>();
                var startDateFormat = (startDateFormatValue != ((object)null)) ? startDateFormatValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<string>() : string.Empty;
                var endDateFormat = (endDateFormatValue != ((object)null)) ? endDateFormatValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<string>() : string.Empty;
                var timePeriodFormat = (timePeriodFormatValue != ((object)null)) ? timePeriodFormatValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<string>() : string.Empty;

                var timeDimensionType_ToShow = TimeDimensionTypeUtils.GetTimeDimensionTypeForNumber(timeDimensionNumber);
                var showStartDate = ShowValue(ref startDateFormat, DefaultDateFormat);
                var showEndDate = ShowValue(ref endDateFormat, DefaultDateFormat);
                var showAtLeastOneDate = (showStartDate || showEndDate);

                var defaultTimePeriodFormat = (showStartDate && showEndDate) ? DefaultTimePeriodFormat_Full : DefaultTimePeriodFormat_Partial;
                var showTimePeriod = ShowValue(ref timePeriodFormat, defaultTimePeriodFormat);

                var primaryTimePeriods_In = timeDimensionSet.PrimaryTimeDimension.GenerateRelevantTimePeriods(currentState.PrimaryPeriod);
                var secondaryTimePeriods_In = timeDimensionSet.SecondaryTimeDimension.GenerateRelevantTimePeriods(currentState.SecondaryPeriod);
                var timeKeys_Out = new List<MultiTimePeriodKey>();

                var result = new ChronometricValue(dataProvider.ProjectGuid, dataProvider.RevisionNumber_NonNull);
                result.DataType = DeciaDataType.Text;
                result.ReDimension(timeDimensionSet);

                foreach (var primaryTimePeriod_In in primaryTimePeriods_In)
                {
                    foreach (var secondaryTimePeriod_In in secondaryTimePeriods_In)
                    {
                        var timeKey_In = new MultiTimePeriodKey(primaryTimePeriod_In, secondaryTimePeriod_In);

                        var timePeriod_ToShow = (timeDimensionType_ToShow == TimeDimensionType.Primary) ? primaryTimePeriod_In : secondaryTimePeriod_In;
                        var timePeriodText_Out = string.Empty;

                        if (timePeriod_ToShow.IsForever)
                        {
                            timePeriodText_Out = DefaultForeverText;
                        }
                        else if (showTimePeriod && showAtLeastOneDate)
                        {
                            var startDateText = (showStartDate) ? timePeriod_ToShow.StartDate.ToString(startDateFormat) : string.Empty;
                            var endDateText = (showEndDate) ? timePeriod_ToShow.EndDate.ToString(endDateFormat) : string.Empty;
                            var partialPeriodText = (showStartDate) ? startDateText : endDateText;

                            timePeriodText_Out = (showStartDate && showEndDate) ? string.Format(timePeriodFormat, startDateText, endDateText) : string.Format(timePeriodFormat, partialPeriodText);
                        }

                        var timeKey_Out = new MultiTimePeriodKey(primaryTimePeriod_In, secondaryTimePeriod_In);
                        timeKeys_Out.Add(timeKey_Out);

                        result.SetValue(timeKey_Out, timePeriodText_Out);
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

            var argInfo = exportInfo.ArgumentInfos[currentState.VariableTemplateRef];

            var timeDimensionNumberArg = callingExpression.GetArgument(Parameter0.Index);
            var startDateFormatArg = (callingExpression.HasArgument(Parameter1.Index)) ? callingExpression.GetArgument(Parameter1.Index) : null;
            var endDateFormatArg = (callingExpression.HasArgument(Parameter2.Index)) ? callingExpression.GetArgument(Parameter2.Index) : null;
            var timePeriodFormatArg = (callingExpression.HasArgument(Parameter3.Index)) ? callingExpression.GetArgument(Parameter3.Index) : null;

            var timeDimensionNumber = timeDimensionNumberArg.DirectValue.GetTypedValue<int>();
            var startDateFormatAsObj = (startDateFormatArg != null) ? startDateFormatArg.DirectValue.GetValue() : null;
            var startDateFormat = (startDateFormatAsObj != null) ? startDateFormatAsObj.ToString() : string.Empty;
            var endDateFormatAsObj = (endDateFormatArg != null) ? endDateFormatArg.DirectValue.GetValue() : null;
            var endDateFormat = (endDateFormatAsObj != null) ? endDateFormatAsObj.ToString() : string.Empty;
            var timePeriodFormat = (timePeriodFormatArg != null) ? timePeriodFormatArg.DirectValue.GetTypedValue<string>() : string.Empty;

            var showStartDate = ShowValue(ref startDateFormat, DefaultDateFormat);
            var showEndDate = ShowValue(ref endDateFormat, DefaultDateFormat);

            var defaultTimePeriodFormat = (showStartDate && showEndDate) ? DefaultTimePeriodFormat_Full : DefaultTimePeriodFormat_Partial;
            var showTimePeriod = ShowValue(ref timePeriodFormat, defaultTimePeriodFormat);

            var relevantTimePeriod_TableAlias = argInfo.GetTimePeriod_TableAlias(timeDimensionNumber, true);

            var showAtLeastOneDate = (showStartDate || showEndDate);
            var hasResult = (!string.IsNullOrWhiteSpace(relevantTimePeriod_TableAlias) && showTimePeriod && showAtLeastOneDate);

            if (hasResult)
            {
                var startDateText = (showStartDate) ? string.Format("FORMAT({0}.[{1}], '{2}')", relevantTimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_StartDate_ColumnName, startDateFormat) : string.Empty;
                var endDateText = (showEndDate) ? string.Format("FORMAT({0}.[{1}], '{2}')", relevantTimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_EndDate_ColumnName, endDateFormat) : string.Empty;
                var partialPeriodText = (showStartDate) ? startDateText : endDateText;

                result = (showStartDate && showEndDate) ? ApplyFormat(timePeriodFormat, startDateText, endDateText) : ApplyFormat(timePeriodFormat, partialPeriodText, string.Empty);
            }
            return result;
        }

        private static bool ShowValue(ref string format, string defaultFormat)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = defaultFormat;
                return true;
            }
            if (format.ToLower() == "false")
            {
                return false;
            }
            if (format.ToLower() == "true")
            {
                format = defaultFormat;
                return true;
            }
            return true;
        }

        private string ApplyFormat(string timePeriodFormat, string startDateText, string endDateText)
        {
            var tokens_0 = timePeriodFormat.Split(new string[] { "{0}" }, StringSplitOptions.None);
            var result = string.Empty;

            for (int i = 0; i < tokens_0.Length; i++)
            {
                var token = tokens_0[i];
                var isEmpty = string.IsNullOrWhiteSpace(token);
                var isLast = (i == (tokens_0.Length - 1));

                if (isLast)
                { result += string.Format("'{0}'", token); }
                else
                { result += string.Format("'{0}' + {1} + ", token, startDateText); }
            }

            if (string.IsNullOrWhiteSpace(endDateText))
            { return string.Format("({0})", result); }

            var tokens_1 = result.Split(new string[] { "{1}" }, StringSplitOptions.None);
            result = string.Empty;

            for (int i = 0; i < tokens_1.Length; i++)
            {
                var token = tokens_1[i];
                var isEmpty = string.IsNullOrWhiteSpace(token);
                var isLast = (i == (tokens_1.Length - 1));

                if (isLast)
                { result += string.Format("'{0}", token); }
                else
                { result += string.Format("{0}' + {1} + ", token, endDateText); }
            }

            return string.Format("({0})", result);
        }
    }
}