using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Formulas.Expressions;
using Decia.Business.Domain.Formulas.Operations;

namespace Decia.Business.Domain.Formulas.Parsing
{
    public class FormulaUpdater
    {
        #region Members

        protected IFormulaDataProvider m_DataProvider;
        protected Formula m_CurrentFormula;
        protected bool m_IsAnonymous;
        protected FormulaNode m_FormulaDefinition;

        #endregion

        #region Constructors

        public FormulaUpdater(IFormulaDataProvider dataProvider, Formula currentFormula, bool isAnonymous, FormulaNode formulaDefinition)
        {
            m_DataProvider = dataProvider;
            m_CurrentFormula = currentFormula;
            m_IsAnonymous = isAnonymous;
            m_FormulaDefinition = formulaDefinition;
        }

        #endregion

        #region Properties

        public IFormulaDataProvider DataProvider { get { return m_DataProvider; } }
        public Formula CurrentFormula { get { return m_CurrentFormula; } }
        public bool IsAnonymous { get { return m_IsAnonymous; } }
        public FormulaNode FormulaDefinition { get { return m_FormulaDefinition; } }

        #endregion

        public void UpdateFormula()
        {
            if (m_CurrentFormula.RootExpressionId.HasValue)
            { m_CurrentFormula.DeleteExpression(m_CurrentFormula.RootExpressionId.Value); }
            CreateExpression(null, m_FormulaDefinition.RootExpression, -1);
        }

        private void CreateExpression(IExpression previousExpression, ExpressionNode expressionNode, int argIndex)
        {
            var operation = OperationCatalog.GetOperationByShortName(expressionNode.FunctionName_Trimmed);
            var expression = (IExpression)null;

            if (previousExpression == null)
            {
                var expressionId = m_CurrentFormula.CreateRootExpression(operation.Id);
                expression = m_CurrentFormula.GetExpression(expressionId);
            }
            else
            {
                var argumentId = previousExpression.CreateArgument(argIndex);
                var expressionId = m_CurrentFormula.CreateNestedExpression(argumentId, operation.Id);
                expression = m_CurrentFormula.GetExpression(expressionId);
            }

            expression.ShowAsOperator = expressionNode.UsesOperator;
            expression.OuterParenthesesCount = expressionNode.OuterParenthesesCount;

            foreach (var nestedArgIndex in expressionNode.ArgumentsByIndex.Keys)
            {
                var nestedArgNode = expressionNode.ArgumentsByIndex[nestedArgIndex];
                var nodeType = nestedArgNode.ArgumentType;

                if (nodeType == ArgumentType.DirectValue)
                {
                    Type inferredType;
                    object parsedValue;
                    bool success;
                    DeciaDataType dataType;

                    if (nestedArgNode.UseTextValue)
                    {
                        var textValue = nestedArgNode.TextValue_Trimmed;
                        success = ParseManager.TryParse(textValue, false, out inferredType, out parsedValue);
                    }
                    else
                    {
                        var textValue = m_FormulaDefinition.SymbolTable.ProcessedValues[nestedArgNode.SymbolTableId_NonNull].TextValue;
                        success = ParseManager.TryParse(textValue, true, out inferredType, out parsedValue);
                    }
                    dataType = inferredType.GetDataTypeForSystemType();

                    var argumentId = expression.CreateArgument(nestedArgIndex);
                    var argument = expression.GetArgument(argumentId);
                    argument.SetToDirectValue(dataType, parsedValue);
                }
                else if (nodeType == ArgumentType.ReferencedId)
                {
                    var argumentId = expression.CreateArgument(nestedArgIndex);
                    var argument = expression.GetArgument(argumentId);
                    argument.SetToReferencedId(ModelObjectReference.EmptyReference);

                    var refValue = m_FormulaDefinition.SymbolTable.ProcessedRefs[nestedArgNode.SymbolTableId_NonNull];
                    var matchingVariableTemplates = m_DataProvider.GetObjectDescriptorsForName(refValue.VariableTemplateName).ToList();

                    foreach (var variableTemplate in matchingVariableTemplates)
                    {
                        var variableTemplateRef = variableTemplate.Key;
                        var structuralTypeRef = m_DataProvider.GetStructuralType(variableTemplateRef);

                        var structuralType = m_DataProvider.GetObjectDescriptor(structuralTypeRef);
                        var isValid = (refValue.HasStructuralTypeName) ? (refValue.StructuralTypeName == structuralType.Name) : true;

                        if (isValid)
                        {
                            argument.SetToReferencedId(new ModelObjectReference(variableTemplateRef, refValue.AlternateDimensionNumber));
                            break;
                        }
                    }
                }
                else if (nodeType == ArgumentType.NestedExpression)
                {
                    if (nestedArgNode.NestedNode == null)
                    { throw new InvalidOperationException("The Nested Expression Node should not be null."); }

                    CreateExpression(expression, nestedArgNode.NestedNode, nestedArgIndex);
                }
                else
                { throw new InvalidOperationException("Unrecognized ArgumentType encountered."); }
            }

            if (expressionNode.ArgumentsByIndex.Count < expression.Operation.SignatureSpecification.RequiredParameterCount)
            {
                for (int i = expressionNode.ArgumentsByIndex.Count; i < expression.Operation.SignatureSpecification.RequiredParameterCount; i++)
                {
                    var missingArgumentError = new ErrorResult(expressionNode.Args_EndIndex, expressionNode.Args_EndIndex, "Missing required argument for index " + i + ".");
                    expressionNode.Errors.Add(missingArgumentError);
                }
            }
        }
    }
}