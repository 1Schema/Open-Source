using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Formulas.Expressions;
using Decia.Business.Domain.Formulas.Operations;

namespace Decia.Business.Domain.Formulas.Parsing
{
    public class FormulaParser
    {
        #region Constants

        public static readonly HashSet<char> Quotes = new HashSet<char>(new char[] { '\'', '"' });
        public static readonly HashSet<string> Separators = new HashSet<string>(new string[] { " ", ",", ";" });
        public static readonly HashSet<string> Separators_NoSpace = new HashSet<string>(new string[] { ",", ";" });
        public static readonly HashSet<string> OpenMarkers = new HashSet<string>(new string[] { "(", "[", "{" });
        public static readonly HashSet<string> CloseMarkers = new HashSet<string>(new string[] { ")", "]", "}" });
        public static HashSet<string> ExpressionIndicators { get { return Operators_Ordered.Union(OpenMarkers).Union(CloseMarkers).ToHashSet(); } }

        public static readonly HashSet<char> RefMarkers = new HashSet<char>(Argument.ReferenceMarkers);
        public static readonly char RefPartSeparator = Argument.ReferencePartSeparator;

        public static readonly HashSet<string> Operators_Prefix = OperationCatalog.Operations.Values.Where(x => !x.DisplayAsFunction && (x.OperatorNotation == OperatorNotationType.Prefix)).Select(x => x.OperatorText).ToHashSet();
        public static readonly HashSet<string> Operators_Infix = OperationCatalog.Operations.Values.Where(x => !x.DisplayAsFunction && (x.OperatorNotation == OperatorNotationType.Infix)).Select(x => x.OperatorText).ToHashSet();
        public static readonly HashSet<string> Operators_Postfix = OperationCatalog.Operations.Values.Where(x => !x.DisplayAsFunction && (x.OperatorNotation == OperatorNotationType.Postfix)).Select(x => x.OperatorText).ToHashSet();

        public static readonly string[] Operators_Unary_Ordered = new[] { "+", "-", "!", "~" };
        public static readonly string[] Operators_Binary_Ordered = new[] { "*", "/", "%", "+", "-", "<<", ">>", "<", ">", "<=", ">=", "=", "!=", "&", "^", "|", "&&", "||" };
        public static readonly string[] Operators_Trinary_Ordered = new[] { "?" };
        public static List<string> Operators_Nary_Ordered
        {
            get
            {
                var operators = new List<string>(Operators_Binary_Ordered);
                operators.AddRange(Operators_Trinary_Ordered);
                return operators;
            }
        }
        public static List<string> Operators_Ordered
        {
            get
            {
                var operators = new List<string>(Operators_Unary_Ordered);
                operators.AddRange(Operators_Binary_Ordered);
                operators.AddRange(Operators_Trinary_Ordered);
                return operators;
            }
        }

        #endregion

        #region Members

        protected string m_FormulaAsText;
        protected FormulaNode m_Result;
        protected List<ErrorResult> m_Errors;

        #endregion

        #region Constructors

        public FormulaParser(string formulaAsText)
        {
            m_FormulaAsText = formulaAsText;
            m_Result = null;
            m_Errors = new List<ErrorResult>();
        }

        #endregion

        #region Properties

        public string FormulaAsText { get { return m_FormulaAsText; } }
        public FormulaNode Result { get { return m_Result; } }
        public List<ErrorResult> Errors { get { return m_Errors; } }

        #endregion

        #region Parse Methods

        public void ParseText()
        {
            var formulaAsText = (!string.IsNullOrWhiteSpace(m_FormulaAsText)) ? m_FormulaAsText : string.Empty;
            var symbolTable = new SymbolTable();

            foreach (var quoteChar in Quotes)
            {
                formulaAsText = ExtractTextValues(formulaAsText, symbolTable, quoteChar, m_Errors);

                if (m_Errors.Count > 0)
                {
                    m_Result = new FormulaNode(formulaAsText, null, symbolTable, m_Errors);
                    return;
                }
            }

            foreach (var refChar in RefMarkers)
            {
                formulaAsText = ExtractReferenceValues(formulaAsText, symbolTable, refChar, m_Errors);

                if (m_Errors.Count > 0)
                {
                    m_Result = new FormulaNode(formulaAsText, null, symbolTable, m_Errors);
                    return;
                }
            }

            formulaAsText = formulaAsText + "    ";

            var openBraceCount = 0;
            var closeBraceCount = 0;
            for (int i = 0; i < formulaAsText.Length; i++)
            {
                var currChar = formulaAsText[i];
                var currStr = currChar.ToString();

                if (OpenMarkers.Contains(currStr))
                { openBraceCount++; }
                if (CloseMarkers.Contains(currStr))
                { closeBraceCount++; }
            }
            for (int i = 0; i < (openBraceCount - closeBraceCount); i++)
            { formulaAsText += CloseMarkers.First(); }

            var rootExpressionNode = ParseText(formulaAsText, null, symbolTable);

            if (m_Errors.Count > 0)
            {
                m_Result = new FormulaNode(formulaAsText, rootExpressionNode, symbolTable, m_Errors);
                return;
            }

            m_Result = new FormulaNode(formulaAsText, rootExpressionNode, symbolTable);
        }

        private ExpressionNode ParseText(string remainingFormulaText, ExpressionNode currNode, SymbolTable symbolTable)
        {
            List<KeyValuePair<int, int>> originalOprtrPositions;
            List<KeyValuePair<int, int>> originalArgPositions;
            var errors = (currNode == null) ? this.Errors : currNode.Errors;
            var outerParenthesesCount = GetOuterParenthesesCount(remainingFormulaText);

            var convertedText = ConvertOperatorsToFunctions(remainingFormulaText, outerParenthesesCount, out originalOprtrPositions, out originalArgPositions, errors);
            if (errors.Count > 0)
            { return null; }

            var usesOperator = (convertedText != remainingFormulaText);

            var nestedNode = (ExpressionNode)null;
            int? openIndex = null;
            int? closeIndex = null;

            GetFunctionPosition(convertedText, ref openIndex, ref closeIndex, errors);
            if (errors.Count > 0)
            { return null; }

            var funcName = openIndex.HasValue ? convertedText.SubStringForIndices(0, openIndex.Value - 1) : string.Empty;

            if (openIndex.HasValue ^ closeIndex.HasValue)
            {
                errors.Add(new ErrorResult(openIndex.HasValue ? openIndex.Value : 0, closeIndex.HasValue ? closeIndex.Value : convertedText.Length - 1, "Both the Open and Close indexes must have values."));
                return null;
            }
            if (!openIndex.HasValue && (currNode != null))
            {
                errors.Add(new ErrorResult(openIndex.HasValue ? openIndex.Value : 0, closeIndex.HasValue ? closeIndex.Value : convertedText.Length - 1, "Invalid function call to ParseText encountered."));
                return null;
            }

            else if (!openIndex.HasValue || (funcName == SymbolTable.Symbol_ForRefs.ToString()) || (funcName == SymbolTable.Symbol_ForValues.ToString()))
            {
                if (string.IsNullOrWhiteSpace(remainingFormulaText))
                {
                    var noOpNode = new ExpressionNode(remainingFormulaText, convertedText, 0, convertedText.Length - 1);
                    noOpNode.FunctionName = OperationCatalog.GetOperation<NoOpOperation>().ShortName;
                    return noOpNode;
                }
                else
                {
                    var identityNode = new ExpressionNode(remainingFormulaText, convertedText, 0, convertedText.Length - 1);
                    identityNode.FunctionName = OperationCatalog.GetOperation<IdentityOperation>().ShortName;

                    ParseNestedArgs(identityNode, symbolTable);
                    return identityNode;
                }
            }

            nestedNode = new ExpressionNode(remainingFormulaText, convertedText, openIndex.Value + 1, closeIndex.Value - 1);
            nestedNode.FunctionName = convertedText.SubStringForIndices(0, openIndex.Value - 1);
            if (usesOperator)
            { nestedNode.SetToUseOperator(outerParenthesesCount, originalOprtrPositions, originalArgPositions); }

            ParseNestedArgs(nestedNode, symbolTable);
            return nestedNode;
        }

        private void ParseNestedArgs(ExpressionNode currNode, SymbolTable symbolTable)
        {
            var separatorIndices = new List<int>();
            for (int i = 0; i < currNode.Args_Text.Length; i++)
            {
                var currChar = currNode.Args_Text[i];
                var currStr = currChar.ToString();

                if (Separators_NoSpace.Contains(currStr))
                { separatorIndices.Add(i); }
                else if (OpenMarkers.Contains(currStr))
                { currNode.Args_Text.SkipNestedText(ref i); }
            }

            var args = new List<string>();
            int startIndex = 0;
            for (int i = 0; i < separatorIndices.Count; i++)
            {
                int separatorIndex = separatorIndices[i];
                var arg = currNode.Args_Text.SubStringForIndices(startIndex, separatorIndex - 1);
                args.Add(arg);
                startIndex = separatorIndex + 1;
            }
            var lastArg = currNode.Args_Text.SubStringForIndices(startIndex, currNode.Args_Text.Length - 1);
            args.Add(lastArg);


            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                if (arg.IsReference())
                {
                    arg = arg.Replace(SymbolTable.Symbol_ForRefs, ' ').TrimOpenAndCloseChars().Trim();
                    var id = int.Parse(arg);

                    var argNode = new ArgumentNode(true, id);
                    currNode.ArgumentsByIndex.Add(i, argNode);
                    continue;
                }
                if (arg.IsValue())
                {
                    arg = arg.Replace(SymbolTable.Symbol_ForValues, ' ').TrimOpenAndCloseChars().Trim();
                    var id = int.Parse(arg);

                    var argNode = new ArgumentNode(false, id);
                    currNode.ArgumentsByIndex.Add(i, argNode);
                    continue;
                }

                var isExpression = false;
                foreach (var expressionIndicator in ExpressionIndicators)
                {
                    if (arg.Contains(expressionIndicator))
                    {
                        var trimmedArg = arg.Trim();
                        double d;
                        bool success = double.TryParse(trimmedArg, out d);

                        isExpression = !success;
                        break;
                    }
                }

                if (isExpression)
                {
                    var nestedNode = ParseText(arg, currNode, symbolTable);
                    var argNode = new ArgumentNode(nestedNode);
                    currNode.ArgumentsByIndex.Add(i, argNode);
                }
                else
                {
                    var argNode = new ArgumentNode(arg);
                    currNode.ArgumentsByIndex.Add(i, argNode);
                }
            }
        }

        #endregion

        #region Internal Evaluation Methods

        internal static string ConvertOperatorsToFunctions(string remainingFormulaText, int outerParenthesesCount, out List<KeyValuePair<int, int>> originalOprtrPositions, out List<KeyValuePair<int, int>> originalArgPositions, List<ErrorResult> errors)
        {
            originalOprtrPositions = new List<KeyValuePair<int, int>>();
            originalArgPositions = new List<KeyValuePair<int, int>>();
            var originalFormulaText = remainingFormulaText;
            var updatedFormulaText = string.Empty;

            var operatorsOrdered_Trinary = Operators_Trinary_Ordered.ToList();
            var operatorPositions_Trinary = GetOperatorPositions(ref remainingFormulaText, outerParenthesesCount, operatorsOrdered_Trinary, false, true, errors);
            if (errors.Count > 0)
            { return null; }
            var hasResults_Trinary = (operatorPositions_Trinary.Count > 0);

            if (hasResults_Trinary)
            {
                var questionMarkIndex = -1;
                var colonIndex = -1;

                foreach (var index in operatorPositions_Trinary.Keys)
                {
                    if (questionMarkIndex >= 0)
                    { continue; }

                    var value = operatorPositions_Trinary[index];

                    if ((questionMarkIndex < 0) && (value == "?"))
                    {
                        questionMarkIndex = index;

                        for (int j = index + 1; j < remainingFormulaText.Length; j++)
                        {
                            var nextChar = remainingFormulaText[j];

                            if ((colonIndex >= 0) && (nextChar == '?'))
                            { break; }
                            else if (nextChar == ':')
                            { colonIndex = j; }
                        }
                    }
                }

                if ((questionMarkIndex < 0) || (colonIndex < 0))
                {
                    errors.Add(new ErrorResult(operatorPositions_Trinary.Keys.First(), operatorPositions_Trinary.Keys.Last(), "The expected Trinary operator could not be found."));
                    return null;
                }

                var relevantOperator = "?";
                var relevantOperator_Part2 = ":";
                var operation = OperationCatalog.GetOperationByOperator(relevantOperator, 3);

                var operatorLoc = new KeyValuePair<int, int>(questionMarkIndex, questionMarkIndex + (relevantOperator.Length - 1));
                var operatorLoc_Part2 = new KeyValuePair<int, int>(colonIndex, colonIndex + (relevantOperator_Part2.Length - 1));
                originalOprtrPositions.Add(operatorLoc);
                originalOprtrPositions.Add(operatorLoc_Part2);

                var conditionLoc = new KeyValuePair<int, int>(0, questionMarkIndex - 1);
                var ifTrueLoc = new KeyValuePair<int, int>(questionMarkIndex + relevantOperator.Length, colonIndex - 1);
                var ifFalseLoc = new KeyValuePair<int, int>(colonIndex + relevantOperator_Part2.Length, remainingFormulaText.Length - 1);
                originalArgPositions.Add(conditionLoc);
                originalArgPositions.Add(ifTrueLoc);
                originalArgPositions.Add(ifFalseLoc);

                var functionPart = operation.ShortName;
                var conditionPart = remainingFormulaText.SubStringForIndices(conditionLoc);
                var ifTruePart = remainingFormulaText.SubStringForIndices(ifTrueLoc);
                var ifFalsePart = remainingFormulaText.SubStringForIndices(ifFalseLoc);
                var argsPart = string.Format("{0},{1},{2}", conditionPart, ifTruePart, ifFalsePart);

                updatedFormulaText = GetAsFunction(functionPart, argsPart);
                return updatedFormulaText;
            }

            remainingFormulaText = originalFormulaText;
            var operatorsOrdered_Binary = Operators_Binary_Ordered.ToList();
            var operatorPositions_Binary = GetOperatorPositions(ref remainingFormulaText, outerParenthesesCount, operatorsOrdered_Binary, false, true, errors);
            if (errors.Count > 0)
            { return null; }
            var hasResults_Binary = (operatorPositions_Binary.Count > 0);

            if (hasResults_Binary)
            {
                var uniqueOperators = operatorPositions_Binary.Values.ToHashSet();
                var relevantOperator = string.Empty;

                foreach (var operator_Binary in operatorsOrdered_Binary.Reverse<string>())
                {
                    if (!uniqueOperators.Contains(operator_Binary))
                    { continue; }

                    relevantOperator = operator_Binary;
                    break;
                }

                if (string.IsNullOrWhiteSpace(relevantOperator))
                {
                    errors.Add(new ErrorResult(operatorPositions_Binary.Keys.First(), operatorPositions_Binary.Keys.Last(), "The expected Binary operator could not be found."));
                    return null;
                }

                var relevantIndices = new List<int>();
                foreach (var index in operatorPositions_Binary.Keys)
                {
                    var operator_Binary = operatorPositions_Binary[index];

                    if (operator_Binary == relevantOperator)
                    { relevantIndices.Add(index); }
                }

                if (relevantIndices.Count <= 0)
                {
                    errors.Add(new ErrorResult(operatorPositions_Binary.Keys.First(), operatorPositions_Binary.Keys.Last(), "The expected Binary operator is not valid."));
                    return null;
                }

                var operation = OperationCatalog.GetOperationByOperator(relevantOperator, 2);
                var canBeNary = (operation.SignatureSpecification.HasParameterAllowingMultipleInstances && (operation.SignatureSpecification.OptionalParameterCount > 0));
                relevantIndices = (canBeNary) ? relevantIndices : new List<int>(new int[] { relevantIndices.Last() });

                var functionPart = operation.ShortName;
                var argsPart = string.Empty;
                int previousIndex = 0;

                foreach (var relevantIndex in relevantIndices)
                {
                    var operatorLoc = new KeyValuePair<int, int>(relevantIndex, relevantIndex + (relevantOperator.Length - 1));
                    originalOprtrPositions.Add(operatorLoc);

                    var argLoc = new KeyValuePair<int, int>(previousIndex, relevantIndex - 1);
                    originalArgPositions.Add(argLoc);

                    var argSubstring = remainingFormulaText.SubStringForIndices(argLoc);
                    previousIndex = relevantIndex + relevantOperator.Length;

                    if (string.IsNullOrWhiteSpace(argsPart))
                    { argsPart = argSubstring; }
                    else
                    { argsPart += "," + argSubstring; }
                }

                var lastArgLoc = new KeyValuePair<int, int>(previousIndex, remainingFormulaText.Length - 1);
                originalArgPositions.Add(lastArgLoc);

                var lastArgSubstring = remainingFormulaText.SubStringForIndices(lastArgLoc);
                argsPart += "," + lastArgSubstring;

                updatedFormulaText = GetAsFunction(functionPart, argsPart);
                return updatedFormulaText;
            }

            remainingFormulaText = originalFormulaText;
            var operatorsOrdered_Unary = Operators_Unary_Ordered.ToList();
            var operatorPositions_Unary = GetOperatorPositions(ref remainingFormulaText, outerParenthesesCount, operatorsOrdered_Unary, true, false, errors);
            if (errors.Count > 0)
            { return null; }
            var hasResults_Unary = (operatorPositions_Unary.Count > 0);

            if (hasResults_Unary)
            {
                var uniqueOperators = operatorPositions_Unary.Values.ToHashSet();
                var relevantOperator = uniqueOperators.First();

                if (string.IsNullOrWhiteSpace(relevantOperator))
                {
                    errors.Add(new ErrorResult(operatorPositions_Unary.Keys.First(), operatorPositions_Unary.Keys.Last(), "The expected Unary operator could not be found."));
                    return null;
                }

                var relevantIndices = new List<int>();
                foreach (var index in operatorPositions_Unary.Keys)
                {
                    var operator_Unary = operatorPositions_Unary[index];

                    if (operator_Unary == relevantOperator)
                    { relevantIndices.Add(index); }
                }

                if (relevantIndices.Count <= 0)
                {
                    errors.Add(new ErrorResult(operatorPositions_Unary.Keys.First(), operatorPositions_Unary.Keys.Last(), "The expected Unary operator is not valid."));
                    return null;
                }

                var operation = OperationCatalog.GetOperationByOperator(relevantOperator, 1);
                var operatorIndex = relevantIndices.First();

                var operatorLoc = new KeyValuePair<int, int>(operatorIndex, operatorIndex + (relevantOperator.Length - 1));
                originalOprtrPositions.Add(operatorLoc);

                var argsLoc = new KeyValuePair<int, int>(operatorIndex + relevantOperator.Length, remainingFormulaText.Length - 1);
                originalArgPositions.Add(argsLoc);

                var functionPart = operation.ShortName;
                var argsPart = remainingFormulaText.SubStringForIndices(argsLoc);

                updatedFormulaText = GetAsFunction(functionPart, argsPart);
                return updatedFormulaText;
            }

            return remainingFormulaText;
        }

        private static string GetAsFunction(string functionPart, string argsPart)
        {
            var usesOuterParentheses = UsesOuterParentheses(argsPart);
            if (usesOuterParentheses)
            { throw new InvalidOperationException("The Outer Parentheses should have already been removed."); }

            var updatedFormulaText = string.Format("{0}({1})", functionPart, argsPart);
            return updatedFormulaText;
        }

        private static bool UsesOuterParentheses(string argsPart)
        {
            var outerParenthesesCount = GetOuterParenthesesCount(argsPart);
            return (outerParenthesesCount > 0);
        }

        private static int GetOuterParenthesesCount(string argsPart)
        {
            if (string.IsNullOrWhiteSpace(argsPart))
            { return 0; }

            var trimmedArgsPart = argsPart.Trim();
            var firstIsOpen = OpenMarkers.Contains(trimmedArgsPart.First().ToString());
            var lastIsClose = CloseMarkers.Contains(trimmedArgsPart.Last().ToString());

            if (!(firstIsOpen && lastIsClose))
            { return 0; }

            var adjustedArgsPart = trimmedArgsPart.SubStringForIndices(1, trimmedArgsPart.Length - 2);
            return (1 + GetOuterParenthesesCount(adjustedArgsPart));
        }

        internal static Dictionary<int, string> GetOperatorPositions(ref string remainingFormulaText, int outerParenthesesCount, IEnumerable<string> operators, bool takeUnary, bool takeNary, List<ErrorResult> errors)
        {
            var operatorPositions = new Dictionary<int, string>();
            var length = remainingFormulaText.Length;
            var lengthMinusOne = length - 1;
            var lastOperatorIndex = -1;

            int handledCount = 0;
            for (int i = 0; i < remainingFormulaText.Length; i++)
            {
                if (handledCount >= outerParenthesesCount)
                { break; }

                if (!OpenMarkers.Contains(remainingFormulaText[i].ToString()))
                { continue; }

                remainingFormulaText = remainingFormulaText.Remove(i, 1).Insert(i, " ");
                handledCount++;
            }
            handledCount = 0;
            for (int i = remainingFormulaText.Length - 1; i >= 0; i--)
            {
                if (handledCount >= outerParenthesesCount)
                { break; }

                if (!CloseMarkers.Contains(remainingFormulaText[i].ToString()))
                { continue; }

                remainingFormulaText = remainingFormulaText.Remove(i, 1).Insert(i, " ");
                handledCount++;
            }

            bool hasHitNonSpace = false;
            bool hasOuterBraces = false;

            for (int i = 0; i < length; i++)
            {
                var str1 = remainingFormulaText.Substring(i, 1);
                var str2 = (i < lengthMinusOne) ? remainingFormulaText.Substring(i, 2) : "JUNK";

                if (operators.Contains(str2) && (str2.Trim() == str2))
                {
                    if (takeNary)
                    { operatorPositions.Add(i, str2); }

                    lastOperatorIndex = i;
                }
                else if (operators.Contains(str1))
                {
                    var startCheckIndex = lastOperatorIndex + 1;
                    var endCheckIndex = i - 1;
                    var sizeToCheck = (endCheckIndex - startCheckIndex) + 1;

                    var spaceBetween = remainingFormulaText.Substring(startCheckIndex, sizeToCheck);
                    var isUnary = string.IsNullOrWhiteSpace(spaceBetween);

                    if (isUnary && takeUnary)
                    { operatorPositions.Add(i, str1); }
                    else if (!isUnary && takeNary)
                    { operatorPositions.Add(i, str1); }

                    lastOperatorIndex = i;
                }
                else if (CloseMarkers.Contains(str1))
                {
                    if (!hasOuterBraces)
                    {
                        errors.Add(new ErrorResult(lastOperatorIndex, i, "Invalid close marker identified."));
                        return null;
                    }
                    else
                    { hasOuterBraces = false; }
                }
                else if (OpenMarkers.Contains(str1))
                {
                    if (!hasHitNonSpace)
                    { hasOuterBraces = true; }
                    else
                    { remainingFormulaText.SkipNestedText(ref i); }
                }

                hasHitNonSpace = (hasHitNonSpace || (str1 != " "));
            }
            return operatorPositions;
        }

        internal static void GetFunctionPosition(string remainingFormulaText, ref int? nextOpenIndex, ref int? nextCloseIndex, List<ErrorResult> errors)
        {
            for (int i = 0; i < remainingFormulaText.Length; i++)
            {
                char currChar = remainingFormulaText[i];

                if (OpenMarkers.Contains(currChar.ToString()))
                {
                    nextOpenIndex = i;
                    break;
                }
            }

            if (nextOpenIndex.HasValue)
            {
                nextCloseIndex = (remainingFormulaText.Length - 1);
                for (int i = (remainingFormulaText.Length - 1); i > 0; i--)
                {
                    char currChar = remainingFormulaText[i];

                    if (CloseMarkers.Contains(currChar.ToString()))
                    {
                        nextCloseIndex = i;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Internal Pre-Processing Methods

        internal static string ExtractTextValues(string formulaAsText, SymbolTable symbolTable, char quoteChar, List<ErrorResult> errors)
        {
            bool isInQuote = false;
            int quoteStartIndex = -1;

            for (int i = 0; i < formulaAsText.Length; i++)
            {
                var currChar = formulaAsText[i];
                var isLastChar = (i == (formulaAsText.Length - 1));

                if (currChar != quoteChar)
                {
                    if (isInQuote && isLastChar)
                    {
                        errors.Add(new ErrorResult(quoteStartIndex, i, "A terminating quote was expected."));
                        return null;
                    }
                    continue;
                }
                if (isInQuote && (formulaAsText[i - 1] == '\\'))
                { continue; }

                if (currChar != quoteChar)
                {
                    errors.Add(new ErrorResult(quoteStartIndex, i, "A quote was expected at the current position."));
                    return null;
                }

                if (!isInQuote)
                {
                    isInQuote = true;
                    quoteStartIndex = i;
                    continue;
                }

                var text = formulaAsText.SubStringForIndices(quoteStartIndex + 1, i - 1);
                var pVal = symbolTable.AddValue(DeciaDataType.Text, text);

                var priorText = formulaAsText.SubStringForIndices(0, quoteStartIndex - 1);
                var postText = (i < (formulaAsText.Length - 1)) ? formulaAsText.SubStringForIndices(i + 1, formulaAsText.Length - 1) : string.Empty;

                var processedPart = priorText + pVal.Marker;
                i = processedPart.Length - 1;

                formulaAsText = processedPart + postText;
                isInQuote = false;
                quoteStartIndex = -1;
            }
            return formulaAsText;
        }

        internal static string ExtractReferenceValues(string formulaAsText, SymbolTable symbolTable, char refChar, List<ErrorResult> errors)
        {
            bool isInReference = false;
            int refStartIndex = -1;

            for (int i = 0; (i < formulaAsText.Length) || isInReference; i++)
            {
                var isLastChar = false;
                var isPartOfRef = false;

                if (i < formulaAsText.Length)
                {
                    var currChar = formulaAsText[i];
                    isLastChar = (i == (formulaAsText.Length - 1));
                    isPartOfRef = ((currChar != ' ')
                                       && !Separators.Contains(currChar.ToString())
                                       && !OpenMarkers.Contains(currChar.ToString())
                                       && !CloseMarkers.Contains(currChar.ToString())
                                       && !Operators_Ordered.Contains(currChar.ToString()));

                    if (currChar == SymbolTable.Symbol_ForValues)
                    {
                        i += 2;
                        ParsingUtils.SkipNestedText(formulaAsText, ref i);
                    }

                    var hasCharsAlready = (isInReference && ((i - refStartIndex) > 0));
                    if (hasCharsAlready && Operators_Ordered.Contains(currChar.ToString()))
                    {
                        if (formulaAsText[i - 1] != RefPartSeparator)
                        { isPartOfRef = false; }
                    }

                    if (isInReference && isPartOfRef && !isLastChar)
                    { continue; }

                    if (currChar == refChar)
                    {
                        isInReference = true;
                        refStartIndex = i;
                        continue;
                    }

                    if (!isInReference)
                    { continue; }
                }

                if (i >= formulaAsText.Length)
                {
                    isLastChar = true;
                    isPartOfRef = true;
                    i = formulaAsText.Length - 1;
                }

                var endIndex = (isLastChar && isPartOfRef) ? i : (i - 1);
                var text = formulaAsText.SubStringForIndices(refStartIndex, endIndex);
                text = text.SubStringForIndices(1, text.Length - 1);
                var refParts = text.Split(new char[] { RefPartSeparator }, StringSplitOptions.RemoveEmptyEntries);

                if ((refChar == Argument.Reference_Full_Operator) && refParts.Length < 2)
                {
                    errors.Add(new ErrorResult(refStartIndex, i, "The minimum number of Reference Parts for a Full Reference is 2."));
                    return null;
                }
                if ((refChar == Argument.Reference_Full_Operator) && refParts.Length > 3)
                {
                    errors.Add(new ErrorResult(refStartIndex, i, "The maximum number of Reference Parts for a Full Reference is 3."));
                    return null;
                }
                if ((refChar == Argument.Reference_Concise_Operator) && refParts.Length < 1)
                {
                    errors.Add(new ErrorResult(refStartIndex, i, "The minimum number of Reference Parts for a Concise Reference is 1."));
                    return null;
                }
                if ((refChar == Argument.Reference_Concise_Operator) && refParts.Length > 2)
                {
                    errors.Add(new ErrorResult(refStartIndex, i, "The maximum number of Reference Parts for a Concise Reference is 2."));
                    return null;
                }

                var values = new List<ProcessedValue>();
                for (int j = 0; j < refParts.Length; j++)
                {
                    var valueText = refParts[j];
                    valueText = valueText.Trim();

                    if (symbolTable.ProcessedValuesByMarker.ContainsKey(valueText))
                    { values.Add(symbolTable.ProcessedValuesByMarker[valueText]); }
                    else if (((refChar == '#') && (j < 1)) || ((refChar == '$') && (j < 2)))
                    {
                        var pVal = symbolTable.AddValue(DeciaDataType.Text, valueText);
                        values.Add(pVal);
                    }
                    else
                    {
                        int val;
                        var success = int.TryParse(valueText, out val);

                        if (!success)
                        {
                            errors.Add(new ErrorResult(refStartIndex, i, "The Alternate Dimension Number should be an integer."));
                            return null;
                        }
                        if (val < 1)
                        {
                            errors.Add(new ErrorResult(refStartIndex, i, "The Alternate Dimension Number should be greater than zero."));
                            return null;
                        }

                        var pVal = symbolTable.AddValue(DeciaDataType.Integer, valueText);
                        values.Add(pVal);
                    }
                }

                var rVal = symbolTable.AddRef(refChar, values);

                var priorText = formulaAsText.SubStringForIndices(0, refStartIndex - 1);
                var postText = (endIndex < (formulaAsText.Length - 1)) ? formulaAsText.SubStringForIndices(endIndex + 1, formulaAsText.Length - 1) : string.Empty;

                var processedPart = priorText + rVal.Marker;
                i = processedPart.Length - 1;

                formulaAsText = processedPart + postText;
                isInReference = false;
                refStartIndex = -1;
            }
            return formulaAsText;
        }

        #endregion
    }
}