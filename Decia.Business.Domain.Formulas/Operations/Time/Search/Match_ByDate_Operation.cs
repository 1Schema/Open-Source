using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Time.Search
{
    public class Match_ByDate_Operation : OperationBase
    {
        public const double DefaultPeriodRatio = 0;

        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "value", "The Value to evaluate");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, "date1", "D1 Date");
        public static readonly Parameter Parameter2 = new Parameter(2, false, false, "date2", "D2 Date");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2 };
        public static readonly Signature Input_Bool_Date_Date__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Boolean);
        public static readonly Signature Input_Intg_Date_Date__Return_Intg = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Integer);
        public static readonly Signature Input_Dubl_Date_Date__Return_Dubl = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Decimal);
        public static readonly Signature Input_Date_Date_Date__Return_Date = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.DateTime, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.DateTime);
        public static readonly Signature Input_Tmsp_Date_Date__Return_Tmsp = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.TimeSpan, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.TimeSpan);
        public static readonly Signature Input_Text_Date_Date__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.DateTime, DeciaDataType.DateTime }, DeciaDataType.Text);

        public static ChronometricValue GetDefaultPeriodRatios(Guid projectGuid, int revisionNumber)
        {
            ChronometricValue defaultPeriodRatios = new ChronometricValue(projectGuid, revisionNumber);
            defaultPeriodRatios.ReDimension(TimeDimensionSet.EmptyTimeDimensionSet);
            defaultPeriodRatios.SetValue(MultiTimePeriodKey.DimensionlessTimeKey, DefaultPeriodRatio);
            return defaultPeriodRatios;
        }

        public Match_ByDate_Operation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "T_MATCH_DATE";
            m_LongName = "Match Date to get Value";
            m_Description = "Searches the Time Dimension(s) in a Variable for the specified Date(s) and retrieves the corresponding Value.";
            m_Category = TimeUtils.TimeCategoryName;
            m_TimeOperationType = OperationType.Shift;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool_Date_Date__Return_Bool);
            validSignatures.Add(Input_Intg_Date_Date__Return_Intg);
            validSignatures.Add(Input_Dubl_Date_Date__Return_Dubl);
            validSignatures.Add(Input_Date_Date_Date__Return_Date);
            validSignatures.Add(Input_Tmsp_Date_Date__Return_Tmsp);
            validSignatures.Add(Input_Text_Date_Date__Return_Text);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            var primaryTimeDimension = inputs.ContainsKey(Parameter1.Index) ? inputs[Parameter1.Index].TimeDimesionality.PrimaryTimeDimension : TimeDimension.EmptyPrimaryTimeDimension;
            var secondaryTimeDimension = inputs.ContainsKey(Parameter2.Index) ? inputs[Parameter2.Index].TimeDimesionality.SecondaryTimeDimension : TimeDimension.EmptySecondaryTimeDimension;
            resultingTimeDimensionality = new TimeDimensionSet(primaryTimeDimension, secondaryTimeDimension);
            return true;
        }

        protected override bool IsUnitValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out CompoundUnit resultingUnit)
        {
            CompoundUnit inputUnit = inputs[Parameter0.Index].Unit;

            if (inputUnit == null)
            { inputUnit = CompoundUnit.GetGlobalScalarUnit(currentState.ProjectGuid, currentState.RevisionNumber); }

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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Date_Date__Return_Bool, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Intg_Date_Date__Return_Intg, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Dubl_Date_Date__Return_Dubl, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Date_Date_Date__Return_Date, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Tmsp_Date_Date__Return_Tmsp, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Text_Date_Date__Return_Text, signature))
            {
                ChronometricValue values = inputs[Parameter0.Index].ChronometricValue;
                ChronometricValue d1Dates = inputs.ContainsKey(Parameter1.Index) ? inputs[Parameter1.Index].ChronometricValue : ChronometricValue.NullInstance;
                ChronometricValue d2Dates = inputs.ContainsKey(Parameter2.Index) ? inputs[Parameter2.Index].ChronometricValue : ChronometricValue.NullInstance;
                ChronometricValue result = values.CopyNew();

                if (!values.PrimaryTimeDimension.HasTimeValue && !values.SecondaryTimeDimension.HasTimeValue)
                { return new OperationMember(result, unit); }
                if ((d1Dates == ((object)null)) && (d2Dates == ((object)null)))
                { return new OperationMember(result, unit); }

                result.ReDimension(timeDimensionSet);

                var primaryTimePeriods_In = timeDimensionSet.PrimaryTimeDimension.GenerateRelevantTimePeriods(currentState.PrimaryPeriod);
                var secondaryTimePeriods_In = timeDimensionSet.SecondaryTimeDimension.GenerateRelevantTimePeriods(currentState.SecondaryPeriod);
                var primaryTimePeriods_ToSearch = values.PrimaryTimeDimension.GenerateRelevantTimePeriods(null);
                var secondaryTimePeriods_ToSearch = values.SecondaryTimeDimension.GenerateRelevantTimePeriods(null);
                var timeKeys_Out = new List<MultiTimePeriodKey>();

                foreach (var primaryTimePeriod_In in primaryTimePeriods_In)
                {
                    foreach (var secondaryTimePeriod_In in secondaryTimePeriods_In)
                    {
                        var timeKey_In = new MultiTimePeriodKey(primaryTimePeriod_In, secondaryTimePeriod_In);

                        var primaryTimePeriod_Out = primaryTimePeriod_In;
                        var secondaryTimePeriod_Out = secondaryTimePeriod_In;

                        if (values.PrimaryTimeDimension.HasTimeValue && (d1Dates != ((object)null)))
                        {
                            var primaryDate = d1Dates.GetValue(timeKey_In).GetTypedValue<DateTime>();
                            primaryTimePeriod_Out = primaryTimePeriods_ToSearch.GetDesiredPeriodForRetrievalValue(values.PrimaryTimeDimension.TimePeriodType, primaryDate);
                        }
                        if (values.SecondaryTimeDimension.HasTimeValue && (d2Dates != ((object)null)))
                        {
                            var secondaryDate = d2Dates.GetValue(timeKey_In).GetTypedValue<DateTime>();
                            secondaryTimePeriod_Out = secondaryTimePeriods_ToSearch.GetDesiredPeriodForRetrievalValue(values.SecondaryTimeDimension.TimePeriodType, secondaryDate);
                        }

                        var timeKey_Out = new MultiTimePeriodKey(primaryTimePeriod_Out, secondaryTimePeriod_Out);
                        timeKeys_Out.Add(timeKey_Out);

                        var value = values.GetValue(timeKey_Out);
                        result.SetValue(timeKey_In, value);
                    }
                }
                return new OperationMember(result, unit);
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var arg1 = callingExpression.GetArgument(Parameter1.Index);
            var arg2 = callingExpression.HasArgument(Parameter2.Index) ? callingExpression.GetArgument(Parameter2.Index) : null;
            var usesTD1 = (arg1 != null);
            var usesTD2 = (arg2 != null);

            var date1 = (usesTD1) ? argumentTextByIndex[Parameter1.Index] : string.Empty;
            var date2 = (usesTD2) ? argumentTextByIndex[Parameter2.Index] : string.Empty;

            var argsWithRefs = parentFormula.GetNestedArguments_ContainingReferences(callingExpression, EvaluationType.Processing);
            var refs = argsWithRefs.Select(x => x.ReferencedModelObject).ToList();

            var exprGuid = callingExpression.Key.ExpressionGuid;
            var refExprIds = refs.Select(x => new MultiPartKey<ModelObjectReference, Guid>(x, exprGuid)).Where(x => exportInfo.ArgumentOverrideInfos.ContainsKey(x)).ToList();
            var refsWithInfo = refExprIds.ToDictionary(x => x.KeyPart1, x => exportInfo.ArgumentOverrideInfos[x]);

            foreach (var refBucket in refsWithInfo)
            {
                var argInfo = refBucket.Value;

                var needsTD1 = (argInfo.TD1_IsRelevant && !argInfo.TD1_HasShiftExpression);
                var needsTD2 = (argInfo.TD2_IsRelevant && !argInfo.TD2_HasShiftExpression);

                if (!needsTD1 && !needsTD2)
                { continue; }

                if (needsTD1)
                {
                    argInfo.TD1_ShiftExpression_WhereText = "A";
                    var newStartDate = string.Format("{0}.[{1}]", argInfo.TD1_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_StartDate_ColumnName);
                    var setDate = date1;
                    var newEndDate = string.Format("{0}.[{1}]", argInfo.TD1_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_EndDate_ColumnName);
                    argInfo.TD1_ShiftExpression_WhereText = string.Format("(({0} <= {1}) AND ({2} >= {1}))", newStartDate, setDate, newEndDate);
                    argInfo.TD1_ShiftExpression_WorksAsJoinOnText = false;
                }
                if (needsTD2)
                {
                    argInfo.TD2_ShiftExpression_WhereText = "A";
                    var newStartDate = string.Format("{0}.[{1}]", argInfo.TD2_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_StartDate_ColumnName);
                    var setDate = date2;
                    var newEndDate = string.Format("{0}.[{1}]", argInfo.TD2_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_EndDate_ColumnName);
                    argInfo.TD2_ShiftExpression_WhereText = string.Format("(({0} <= {1}) AND ({2} >= {1}))", newStartDate, setDate, newEndDate);
                    argInfo.TD2_ShiftExpression_WorksAsJoinOnText = false;
                }
            }

            if (argumentTextByIndex.Count < 1)
            { throw new InvalidOperationException("At least one Argument must have a value."); }

            var firstArgText = argumentTextByIndex.Values.First();
            return firstArgText;
        }

        private string GetDateTimeAsText(DateTime dateTime)
        {
            var dateTimeAsText = dateTime.ToString("s");
            dateTimeAsText = dateTimeAsText.Replace('T', ' ');
            return dateTimeAsText;
        }
    }
}