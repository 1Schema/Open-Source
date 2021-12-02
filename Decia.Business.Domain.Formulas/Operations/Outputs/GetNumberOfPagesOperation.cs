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

namespace Decia.Business.Domain.Formulas.Operations.Outputs
{
    public class GetNumberOfPagesOperation : OperationBase
    {
        public static readonly Parameter[] Parameters = new Parameter[] { };
        public static readonly Signature Input__Return_Text = new Signature(Parameters, new DeciaDataType[] { }, DeciaDataType.Text);

        public GetNumberOfPagesOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "NumPgs";
            m_LongName = "Get Number of Pages";
            m_Description = "When exporting output, loads the number of pages into the Header or Footer.";
            m_Category = OutputUtils.OutputCategoryName;

            m_ValidityType = OperationValidityType.Report;
            m_ValidProcessingType = ProcessingType.Anonymous;
            m_AllowAutoConversionToText = true;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input__Return_Text);

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

            DeciaDataType returnType;
            bool isValid = this.IsSignatureValid(inputs, out returnType);

            if (!isValid)
            { return new OperationMember(); }

            ITimeDimensionSet timeDimensionSet = null;
            IsTimeDimensionalityValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out timeDimensionSet);
            CompoundUnit unit = null;
            IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out unit);

            ChronometricValue result = result = new ChronometricValue(dataProvider.ProjectGuid, dataProvider.RevisionNumber_NonNull);
            result.DataType = DeciaDataType.Text;
            result.ReDimension(timeDimensionSet);
            result.SetValue(MultiTimePeriodKey.DimensionlessTimeKey, "&[Pages]");

            return new OperationMember(result, unit);
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            throw new NotImplementedException("LATER: Implement this");
        }
    }
}