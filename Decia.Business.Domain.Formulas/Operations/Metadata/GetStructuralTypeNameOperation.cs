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
    public class GetStructuralTypeNameOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "objectRef", "Model Object reference");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0 };
        public static readonly Signature Input_Bool__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean }, DeciaDataType.Text);
        public static readonly Signature Input_Int__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Integer }, DeciaDataType.Text);
        public static readonly Signature Input_Double__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Decimal }, DeciaDataType.Text);
        public static readonly Signature Input_UniqueID__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.UniqueID }, DeciaDataType.Text);
        public static readonly Signature Input_DateTime__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.DateTime }, DeciaDataType.Text);
        public static readonly Signature Input_TimeSpan__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.TimeSpan }, DeciaDataType.Text);
        public static readonly Signature Input_Text__Return_Text = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Text }, DeciaDataType.Text);

        public GetStructuralTypeNameOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "TYPE_NAME";
            m_LongName = "Get Structural Type Name";
            m_Description = "Gets the Name of the specified Structural Type";
            m_Category = MetadataUtils.MetadataCategoryName;

            m_EvaluationType = EvaluationType.PreProcessing;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool__Return_Text);
            validSignatures.Add(Input_Int__Return_Text);
            validSignatures.Add(Input_Double__Return_Text);
            validSignatures.Add(Input_UniqueID__Return_Text);
            validSignatures.Add(Input_DateTime__Return_Text);
            validSignatures.Add(Input_TimeSpan__Return_Text);
            validSignatures.Add(Input_Text__Return_Text);

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

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Int__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Double__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_UniqueID__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_DateTime__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_TimeSpan__Return_Text, signature)
                || SignatureSpecification.IsSpecificInstanceOf(Input_Text__Return_Text, signature))
            {
                var inputValue = inputs.Values.First();

                var result = new ChronometricValue(dataProvider.ProjectGuid, dataProvider.RevisionNumber_NonNull);
                result.DataType = DeciaDataType.Text;
                result.ReDimension(timeDimensionSet);

                var structuralTypeRef = dataProvider.GetStructuralType(inputValue.ObjectRef.Value);
                var structuralTypeName = dataProvider.GetObjectName(structuralTypeRef);
                result.SetValue(MultiTimePeriodKey.DimensionlessTimeKey, structuralTypeName);

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
            var modelObjRef = (modelObjArg != null) ? modelObjArg.ReferencedModelObject : (ModelObjectReference?)null;

            if (!modelObjRef.HasValue)
            { return string.Empty; }
            if (modelObjRef.Value.ModelObjectType != ModelObjectType.VariableTemplate)
            { return string.Empty; }

            var argInfo = MetadataUtils.GetSqlArgInfo(modelObjRef.Value, parentFormula, callingExpression, exportInfo);
            argInfo.StructuralType_IsUsed = true;

            result = string.Format("{0}.[{1}]", argInfo.StructuralType_TableAlias, "Name");
            return result;
        }
    }
}