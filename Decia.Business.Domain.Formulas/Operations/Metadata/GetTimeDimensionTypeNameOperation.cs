using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Metadata
{
    public class GetTimeDimensionTypeNameOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "objectRef", "Model Object reference");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, false, "dimensionNum", "Time Dimension Number");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1 };
        public static readonly Signature Input_Bool_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Integer }, DeciaDataType.Text);
        public static readonly Signature Input_Int_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer, DeciaDataType.Integer }, DeciaDataType.Text);
        public static readonly Signature Input_Double_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal, DeciaDataType.Integer }, DeciaDataType.Text);
        public static readonly Signature Input_UniqueID_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.UniqueID, DeciaDataType.Integer }, DeciaDataType.Text);
        public static readonly Signature Input_DateTime_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.DateTime, DeciaDataType.Integer }, DeciaDataType.Text);
        public static readonly Signature Input_TimeSpan_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.TimeSpan, DeciaDataType.Integer }, DeciaDataType.Text);
        public static readonly Signature Input_Text_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text, DeciaDataType.Integer }, DeciaDataType.Text);

        public GetTimeDimensionTypeNameOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "TDIM_NAME";
            m_LongName = "Get Time Dimension Name";
            m_Description = "Gets the Name of the specified Time Dimension";
            m_Category = MetadataUtils.MetadataCategoryName;

            m_EvaluationType = EvaluationType.PreProcessing;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool_Int__Return_Text);
            validSignatures.Add(Input_Int_Int__Return_Text);
            validSignatures.Add(Input_Double_Int__Return_Text);
            validSignatures.Add(Input_UniqueID_Int__Return_Text);
            validSignatures.Add(Input_DateTime_Int__Return_Text);
            validSignatures.Add(Input_TimeSpan_Int__Return_Text);
            validSignatures.Add(Input_Text_Int__Return_Text);

            m_SignatureSpecification = new SignatureValiditySpecification(Parameters, validSignatures);
        }

        protected override bool IsTimeDimensionalityValid(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression, IDictionary<int, OperationMember> inputs, out ITimeDimensionSet resultingTimeDimensionality)
        {
            resultingTimeDimensionality = TimeDimensionSet.EmptyTimeDimensionSet;
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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Int__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int_Int__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double_Int__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_UniqueID_Int__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_DateTime_Int__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_TimeSpan_Int__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Text_Int__Return_Text, signature))
            {
                var refValue = inputs.Values.ElementAt(0);
                var numValue = inputs.Values.ElementAt(1);
                var timeDimNum = numValue.ChronometricValue.GetValue(MultiTimePeriodKey.DimensionlessTimeKey).GetTypedValue<int>();

                var result = new ChronometricValue(dataProvider.ProjectGuid, dataProvider.RevisionNumber_NonNull);
                result.DataType = DeciaDataType.Text;
                result.ReDimension(timeDimensionSet);

                var timeDimName = GetTimeDimensionName(dataProvider, refValue.ObjectRef.Value, timeDimNum);
                result.SetValue(MultiTimePeriodKey.DimensionlessTimeKey, timeDimName);

                return new OperationMember(result, unit);
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;

            var exprGuid = callingExpression.Key.ExpressionGuid;
            var modelObjArg = callingExpression.GetArgument(Parameter0.Index);
            var timeDimArg = callingExpression.GetArgument(Parameter1.Index);
            var modelObjRef = (modelObjArg != null) ? modelObjArg.ReferencedModelObject : (ModelObjectReference?)null;
            var timeDimNum = (timeDimArg != null) ? timeDimArg.DirectValue.GetTypedValue<int>() : (int?)null;

            if (!modelObjRef.HasValue)
            { return string.Empty; }
            if (!timeDimNum.HasValue)
            { return string.Empty; }
            if (modelObjRef.Value.ModelObjectType != ModelObjectType.VariableTemplate)
            { return string.Empty; }

            var argInfo = MetadataUtils.GetSqlArgInfo(modelObjRef.Value, parentFormula, callingExpression, exportInfo);

            result = string.Format("'{0}'", GetTimeDimensionName(dataProvider, modelObjRef.Value, timeDimNum.Value));
            return result;
        }

        private static string GetTimeDimensionName(IFormulaDataProvider dataProvider, ModelObjectReference variableTemplateRef, int timeDimNum)
        {
            var timeDimSet = dataProvider.GetAssessedTimeDimensions(variableTemplateRef);
            var timeDim = timeDimSet.GetTimeDimension(timeDimNum, false);

            if (timeDim.HasTimeValue)
            { return timeDim.TimeDimensionType.ToString(); }
            else
            { return "None"; }
        }
    }
}