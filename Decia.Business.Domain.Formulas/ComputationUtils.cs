using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Formulas
{
    public static class ComputationUtils
    {
        private static void AssertValidationConditions(IFormulaDataProvider dataProvider, ModelObjectReference variableTemplate)
        {
            if (dataProvider == null)
            { throw new InvalidOperationException("The Data Provider must not be null."); }
            if (!variableTemplate.IsVariableType())
            { throw new InvalidOperationException("Currently, only Variable Templates can be validated."); }
        }

        public static void AssertValidationConditions(IFormulaDataProvider dataProvider, ModelObjectReference variableTemplate, IFormula parentFormula)
        {
            AssertValidationConditions(dataProvider, variableTemplate);
            if (parentFormula == null)
            { throw new InvalidOperationException("The Parent Formula must not be null."); }
        }

        public static void AssertValidationConditions(IFormulaDataProvider dataProvider, ModelObjectReference variableTemplate, IFormula parentFormula, ArgumentId argumentId)
        {
            AssertValidationConditions(dataProvider, variableTemplate, parentFormula);
            if (!parentFormula.GetNestedArgumentIds().Contains(argumentId))
            { throw new InvalidOperationException("The requested Argument is not defined in the Parent Formula."); }
        }

        public static void AssertValidationConditions(IFormulaDataProvider dataProvider, ModelObjectReference variableTemplate, IFormula parentFormula, ExpressionId expressionId)
        {
            AssertValidationConditions(dataProvider, variableTemplate, parentFormula);
            if (!parentFormula.GetNestedExpressionIds().Contains(expressionId))
            { throw new InvalidOperationException("The requested Expression is not defined in the Parent Formula."); }
        }

        private static void AssertComputationConditions(IFormulaDataProvider dataProvider, ModelObjectReference variableInstance)
        {
            if (dataProvider == null)
            { throw new InvalidOperationException("The Data Provider must not be null."); }
            if (!variableInstance.IsVariableInstance())
            { throw new InvalidOperationException("Currently, only Variable Instances can be computed."); }
        }

        public static void AssertComputationConditions(IFormulaDataProvider dataProvider, ModelObjectReference variableInstance, IFormula parentFormula)
        {
            AssertComputationConditions(dataProvider, variableInstance);
            if (parentFormula == null)
            { throw new InvalidOperationException("The Parent Formula must not be null."); }
        }

        public static void AssertComputationConditions(IFormulaDataProvider dataProvider, ModelObjectReference variableTemplate, IFormula parentFormula, ArgumentId argumentId)
        {
            AssertComputationConditions(dataProvider, variableTemplate, parentFormula);
            if (!parentFormula.GetNestedArgumentIds().Contains(argumentId))
            { throw new InvalidOperationException("The requested Argument is not defined in the Parent Formula."); }
        }

        public static void AssertComputationConditions(IFormulaDataProvider dataProvider, ModelObjectReference variableTemplate, IFormula parentFormula, ExpressionId expressionId)
        {
            AssertComputationConditions(dataProvider, variableTemplate, parentFormula);
            if (!parentFormula.GetNestedExpressionIds().Contains(expressionId))
            { throw new InvalidOperationException("The requested Expression is not defined in the Parent Formula."); }
        }
    }
}