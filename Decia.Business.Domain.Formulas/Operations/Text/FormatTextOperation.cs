using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Text
{
    public class FormatTextOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "formatText", "The text value with parameters specified as \"{N}\"");
        public static readonly Parameter Parameter1 = new Parameter(1, false, true, "value1", "First value");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1 };
        public static readonly Signature Input_Text_Bool__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.Boolean }, DeciaDataType.Text);
        public static readonly Signature Input_Text_Intg__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.Integer }, DeciaDataType.Text);
        public static readonly Signature Input_Text_Dubl__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.Decimal }, DeciaDataType.Text);
        public static readonly Signature Input_Text_Guid__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.UniqueID }, DeciaDataType.Text);
        public static readonly Signature Input_Text_Date__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.DateTime }, DeciaDataType.Text);
        public static readonly Signature Input_Text_Tmsp__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.TimeSpan }, DeciaDataType.Text);
        public static readonly Signature Input_Text_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.Text }, DeciaDataType.Text);

        public FormatTextOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "FORMAT";
            m_LongName = "Format Text";
            m_Description = "Applies a Format String to combine a series of values.";
            m_Category = TextUtils.TextCategoryName;

            m_AllowAutoConversionToText = true;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Text_Bool__Return_Text);
            validSignatures.Add(Input_Text_Intg__Return_Text);
            validSignatures.Add(Input_Text_Dubl__Return_Text);
            validSignatures.Add(Input_Text_Guid__Return_Text);
            validSignatures.Add(Input_Text_Date__Return_Text);
            validSignatures.Add(Input_Text_Tmsp__Return_Text);
            validSignatures.Add(Input_Text_Text__Return_Text);

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

            DeciaDataType returnType;
            bool isValid = this.IsSignatureValid(inputs, out returnType);

            if (!isValid)
            { return new OperationMember(); }

            ITimeDimensionSet timeDimensionSet = null;
            IsTimeDimensionalityValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out timeDimensionSet);
            CompoundUnit unit = null;
            IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out unit);

            List<ChronometricValue> inputValuesList = inputs.Values.Select(val => val.ChronometricValue).ToList();
            ChronometricValue formatStringValue = inputValuesList[0];

            ChronometricValue result = result = new ChronometricValue(dataProvider.ProjectGuid, dataProvider.RevisionNumber_NonNull);
            result.DataType = DeciaDataType.Text;
            result.ReDimension(timeDimensionSet);

            IList<MultiTimePeriodKey> timeKeys = result.TimeKeys;

            foreach (var timeKey in timeKeys)
            {
                string formatString = formatStringValue.GetValue(timeKey).GetTypedValue<string>();
                List<object> formatValues = new List<object>();

                for (int i = 1; i < inputValuesList.Count; i++)
                {
                    ChronometricValue inputValue = inputValuesList[i];

                    object formatValue = inputValue.GetValue(timeKey).GetValue();
                    formatValues.Add(formatValue);
                }

                string formattedText = string.Format(formatString, formatValues.ToArray<object>());
                result.SetValue(timeKey, formattedText);
            }

            return new OperationMember(result, unit);
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;
            var isFirst = true;

            foreach (var argBucket in argumentTextByIndex)
            {
                var value = argBucket.Value;

                if (isFirst)
                {
                    isFirst = false;
                    value = string.Format("'{0}'", value);
                }
                else
                { result += ", "; }

                result += value;
            }

            result = string.Format("FORMATMESSAGE({0})", result);
            return result;
        }
    }
}