﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Logic
{
    public class OrOperation : OperationBase
    {
        public static readonly Parameter Parameter0 = new Parameter(0, true, false, "value1", "First value");
        public static readonly Parameter Parameter1 = new Parameter(1, true, false, "value2", "Second value");
        public static readonly Parameter Parameter2 = new Parameter(2, false, true, "value3", "Additional value");
        public static readonly Parameter[] Parameters = new Parameter[] { Parameter0, Parameter1, Parameter2 };
        public static readonly Signature Input_Bool_Bool_Bool__Return_Bool = new Signature(Parameters, new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.Boolean, DeciaDataType.Boolean }, DeciaDataType.Boolean);

        public OrOperation()
        {
            m_Id = OperationCatalog.GetOperationId(this.GetType());
            m_ShortName = "OR";
            m_LongName = "Or";
            m_Description = "Ors 2 or more boolean values";
            m_Category = LogicUtils.LogicCategoryName;

            List<Signature> validSignatures = new List<Signature>();
            validSignatures.Add(Input_Bool_Bool_Bool__Return_Bool);

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

            CompoundUnit unit = null;
            IsUnitValid(dataProvider, currentState, parentFormula, callingExpression, inputs, out unit);

            if (SignatureSpecification.IsSpecificInstanceOf(Input_Bool_Bool_Bool__Return_Bool, signature))
            {
                List<ChronometricValue> inputValuesList = inputs.Values.Select(val => val.ChronometricValue).ToList();
                ChronometricValue result = null;

                for (int i = 0; i < inputValuesList.Count; i++)
                {
                    ChronometricValue inputValue = inputValuesList[i];
                    this.AssertInputValueIsNotNull(inputValue, i);

                    if (i == 0)
                    { result = inputValue; }
                    else
                    { result = result | inputValue; }
                }

                return new OperationMember(result, unit);
            }
            else
            { throw new InvalidOperationException("Unrecognized Signature encountered."); }
        }

        protected override string DoRenderAsSql(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression callingExpression, IDictionary<int, string> argumentTextByIndex)
        {
            var result = string.Empty;
            var isFirst = true;

            foreach (var argBucket in argumentTextByIndex)
            {
                if (isFirst)
                { isFirst = false; }
                else
                { result += " OR "; }

                result += argBucket.Value;
            }

            result = string.Format("({0})", result);
            return result;
        }
    }
}