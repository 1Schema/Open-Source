using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.ValueObjects;
using Newtonsoft.Json;

namespace Decia.Business.Domain.Formulas.Parsing
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FormulaNode
    {
        protected List<ErrorResult> m_Errors;

        public FormulaNode(string incomingText, ExpressionNode rootExpression, SymbolTable symbolTable)
            : this(incomingText, rootExpression, symbolTable, new List<ErrorResult>())
        { }

        public FormulaNode(string incomingText, ExpressionNode rootExpression, SymbolTable symbolTable, List<ErrorResult> errors)
        {
            IncomingText = incomingText;
            RootExpression = rootExpression;
            SymbolTable = symbolTable;
            m_Errors = errors.ToList();
        }

        [JsonProperty]
        public string IncomingText { get; protected set; }
        [JsonProperty]
        public ExpressionNode RootExpression { get; protected set; }
        [JsonProperty]
        public SymbolTable SymbolTable { get; protected set; }
        [JsonProperty]
        public List<ErrorResult> Errors { get { return m_Errors; } }
        [JsonProperty]
        public bool HasRootLevelErrors { get { return (m_Errors.Count > 0); } }

        [JsonProperty]
        public bool PassesParsing
        {
            get
            {
                if (HasRootLevelErrors)
                { return false; }
                if (RootExpression == null)
                { return false; }
                if (!RootExpression.PassesParsing)
                { return false; }
                return true;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ExpressionNode
    {
        protected List<ErrorResult> m_Errors;

        public ExpressionNode(string originalText, string convertedText, int argsStartIndex, int argsEndIndex)
        {
            OriginalText = originalText;
            ConvertedText = convertedText;

            Args_StartIndex = argsStartIndex;
            Args_EndIndex = argsEndIndex;

            FunctionName = string.Empty;
            UsesOperator = false;
            ArgumentsByIndex = new SortedDictionary<int, ArgumentNode>();

            m_Errors = new List<ErrorResult>();
        }

        [JsonProperty]
        public string OriginalText { get; protected set; }
        [JsonProperty]
        public string ConvertedText { get; protected set; }

        [JsonProperty]
        public int Args_StartIndex { get; protected set; }
        [JsonProperty]
        public int Args_EndIndex { get; protected set; }
        [JsonProperty]
        public string Args_Text { get { return ConvertedText.SubStringForIndices(Args_StartIndex, Args_EndIndex); } }

        [JsonProperty]
        public string FunctionName { get; set; }
        [JsonProperty]
        public string FunctionName_Trimmed { get { return (FunctionName != null) ? FunctionName.Trim() : string.Empty; } }
        [JsonProperty]
        public SortedDictionary<int, ArgumentNode> ArgumentsByIndex { get; protected set; }

        [JsonProperty]
        public bool UsesOperator { get; protected set; }
        [JsonProperty]
        public int OuterParenthesesCount { get; protected set; }
        [JsonProperty]
        public List<KeyValuePair<int, int>> OriginalOprtrPositions { get; protected set; }
        [JsonProperty]
        public List<KeyValuePair<int, int>> OriginalArgPositions { get; protected set; }

        internal void SetToUseOperator(int outerParenthesesCount, List<KeyValuePair<int, int>> originalOprtrPositions, List<KeyValuePair<int, int>> originalArgPositions)
        {
            UsesOperator = true;
            OuterParenthesesCount = outerParenthesesCount;
            OriginalOprtrPositions = originalOprtrPositions.ToList();
            OriginalArgPositions = originalArgPositions.ToList();
        }

        [JsonProperty]
        public List<ErrorResult> Errors { get { return m_Errors; } }
        [JsonProperty]
        public bool HasErrors { get { return (m_Errors.Count > 0); } }

        [JsonProperty]
        public bool PassesParsing
        {
            get
            {
                if (HasErrors)
                { return false; }
                foreach (var argumentNode in ArgumentsByIndex.Values)
                {
                    if (argumentNode == null)
                    { return false; }
                    if (!argumentNode.PassesParsing)
                    { return false; }
                }
                return true;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ArgumentNode
    {
        protected int? m_SymbolTableId;

        public ArgumentNode(ExpressionNode expressionNode)
        {
            NestedNode = expressionNode;
            IsReference = false;
            m_SymbolTableId = null;
            TextValue = string.Empty;
            HasErrors = false;
        }

        public ArgumentNode(bool isRef, int id)
        {
            NestedNode = null;
            IsReference = isRef;
            m_SymbolTableId = id;
            TextValue = string.Empty;
            HasErrors = false;
        }

        public ArgumentNode(string textValue)
        {
            NestedNode = null;
            IsReference = false;
            m_SymbolTableId = null;
            TextValue = textValue;
            HasErrors = false;
        }

        [JsonProperty]
        public ExpressionNode NestedNode { get; protected set; }

        [JsonProperty]
        public bool IsReference { get; protected set; }
        [JsonProperty]
        public int? SymbolTableId { get { return m_SymbolTableId; } }
        public int SymbolTableId_NonNull { get { return m_SymbolTableId.Value; } }

        [JsonProperty]
        public bool UseTextValue { get { return ((ArgumentType == ArgumentType.DirectValue) && (!m_SymbolTableId.HasValue)); } }
        [JsonProperty]
        public string TextValue { get; protected set; }
        [JsonProperty]
        public string TextValue_Trimmed { get { return (TextValue != null) ? TextValue.Trim() : string.Empty; } }

        [JsonProperty]
        public ArgumentType ArgumentType
        {
            get
            {
                if (NestedNode != null)
                { return ArgumentType.NestedExpression; }
                if (IsReference)
                { return ArgumentType.ReferencedId; }
                return ArgumentType.DirectValue;
            }
        }

        [JsonProperty]
        public bool HasErrors { get; set; }
        [JsonProperty]
        public bool PassesParsing
        {
            get
            {
                if (HasErrors)
                { return false; }
                if (ArgumentType == ArgumentType.NestedExpression)
                {
                    if (NestedNode == null)
                    { return false; }
                    if (!NestedNode.PassesParsing)
                    { return false; }
                }
                return true;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ErrorResult
    {
        public ErrorResult(int startIndex, int endIndex, string message)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            Message = message;
        }

        [JsonProperty]
        public int StartIndex { get; protected set; }
        [JsonProperty]
        public int EndIndex { get; protected set; }
        [JsonProperty]
        public string Message { get; protected set; }
    }
}