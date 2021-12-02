using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.ValueObjects;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Argument
    {
        public const char Reference_Full_Operator = '$';
        public const char Reference_Concise_Operator = '#';

        public static readonly IEnumerable<char> ReferenceMarkers = new char[] { Reference_Full_Operator, Reference_Concise_Operator };
        public static readonly char ReferencePartSeparator = ':';

        public static readonly string RenderFormat_Text = "\"{0}\"";
        public static readonly string RenderFormat_Reference_Full = "${0}:{1}:{2}";
        public static readonly string RenderFormat_Reference_Full_NAD = "${0}:{1}";
        public static readonly string RenderFormat_Reference_Concise = "#{0}:{1}";
        public static readonly string RenderFormat_Reference_Concise_NAD = "#{0}";

        public const string RefDeleted_Prefix = "[DELETED:";
        public const string RefDeleted_Postfix = "]";

        public string RenderAsString(IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, out IDictionary<ExpressionId, string> expressionsAsStrings, out IDictionary<ArgumentId, string> argumentsAsStrings)
        {
            expressionsAsStrings = new Dictionary<ExpressionId, string>();
            argumentsAsStrings = new Dictionary<ArgumentId, string>();
            string result = string.Empty;

            if (this.ArgumentType == ArgumentType.DirectValue)
            {
                var dataType = this.DirectValue.DataType;
                object value = this.DirectValue.GetValue();
                string valueAsString = (value != null) ? dataType.ToStringValue(value, true) : FormulaRenderingUtils.NullValueAsString;

                if ((dataType == DeciaDataType.UniqueID) || (dataType == DeciaDataType.DateTime)
                    || (dataType == DeciaDataType.TimeSpan) || (dataType == DeciaDataType.Text))
                { result = string.Format(RenderFormat_Text, valueAsString); }
                else
                { result = valueAsString; }
            }
            else if (this.ArgumentType == ArgumentType.NestedExpression)
            {
                IDictionary<ExpressionId, string> nestedExpressionsAsStrings;
                IDictionary<ArgumentId, string> nestedArgumentsAsStrings;

                IExpression nestedExpression = parentFormula.GetExpression(this.NestedExpressionId);
                result = nestedExpression.RenderAsString(dataProvider, currentState, parentFormula, out nestedExpressionsAsStrings, out  nestedArgumentsAsStrings);

                expressionsAsStrings = nestedExpressionsAsStrings;
                argumentsAsStrings = nestedArgumentsAsStrings;
            }
            else if (this.ArgumentType == ArgumentType.ReferencedId)
            {
                var structuralTypeName = string.Empty;
                var variableTemplateName = string.Empty;
                var alternateDimensionNumber = this.ReferencedModelObject.NonNullAlternateDimensionNumber;
                var refAsText = string.Empty;

                if (!this.IsRefDeleted)
                {
                    var variableTemplateRef = this.ReferencedModelObject;
                    var structuralTypeRef = dataProvider.GetStructuralType(variableTemplateRef);

                    variableTemplateName = dataProvider.GetObjectName(variableTemplateRef);
                    structuralTypeName = dataProvider.GetObjectName(structuralTypeRef);

                    variableTemplateName = variableTemplateName.Contains(" ") ? string.Format(RenderFormat_Text, variableTemplateName) : variableTemplateName;
                    structuralTypeName = structuralTypeName.Contains(" ") ? string.Format(RenderFormat_Text, structuralTypeName) : structuralTypeName;
                }
                else
                {
                    structuralTypeName = this.DeletedRef_StructuralTypeText;
                    variableTemplateName = this.DeletedRef_VariableTemplateText;
                }

                if (!this.ReferencedModelObject.AlternateDimensionNumber.HasValue)
                { refAsText = string.Format(RenderFormat_Reference_Full_NAD, structuralTypeName, variableTemplateName); }
                else
                { refAsText = string.Format(RenderFormat_Reference_Full, structuralTypeName, variableTemplateName, alternateDimensionNumber); }

                if (!this.IsRefDeleted)
                { result = refAsText; }
                else
                { result = RefDeleted_Prefix + refAsText + RefDeleted_Postfix; }
            }
            else
            { throw new InvalidOperationException("Invalid ArgumentType encountered."); }

            argumentsAsStrings.Add(this.Key, result);
            return result;
        }
    }
}