using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Presentation
{
    public enum FormulaEditorMode
    {
        Normal,
        StructuralAggregation,
        TimeAggregation,
        Anonymous
    }

    public static class FormulaEditorModeUtils
    {
        public static FormulaEditorMode GetEditorMode(this Nullable<VariableType> variableType)
        {
            if (!variableType.HasValue)
            { return FormulaEditorMode.Anonymous; }
            else if (variableType == VariableType.BasicFormula)
            { return FormulaEditorMode.Normal; }
            else if (variableType == VariableType.StructuralAggregationFormula)
            { return FormulaEditorMode.StructuralAggregation; }
            else if (variableType == VariableType.TimeAggregationFormula)
            { return FormulaEditorMode.TimeAggregation; }
            else
            { throw new InvalidOperationException("The specified VariableType does not support Formulas."); }
        }
    }
}