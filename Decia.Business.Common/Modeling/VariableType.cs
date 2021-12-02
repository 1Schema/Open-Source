using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public enum VariableType
    {
        [Description("Input")]
        Input,
        [Description("Basic Formula")]
        BasicFormula,
        [Description("Structural Aggregation Formula")]
        StructuralAggregationFormula,
        [Description("Time Aggregation Formula")]
        TimeAggregationFormula,
    }

    public static class VariableTypeUtils
    {
        public static readonly IEnumerable<VariableType> VariableTypes_WithFormulas = new HashSet<VariableType>(new VariableType[] { VariableType.BasicFormula, VariableType.StructuralAggregationFormula, VariableType.TimeAggregationFormula });
        public static readonly IEnumerable<VariableType> VariableTypes_ForUI = new HashSet<VariableType>(new VariableType[] { VariableType.Input, VariableType.BasicFormula, VariableType.StructuralAggregationFormula, VariableType.TimeAggregationFormula });
        public static readonly IEnumerable<VariableType> VariableTypes_ForUI_ForNavVars = new HashSet<VariableType>(new VariableType[] { VariableType.Input, VariableType.StructuralAggregationFormula });

        public static bool IsComputed(this VariableType variableType)
        {
            if (variableType == VariableType.Input)
            { return false; }
            return true;
        }

        public static bool IsAggregationFormula(this VariableType variableType)
        {
            if (variableType == VariableType.StructuralAggregationFormula)
            { return true; }
            if (variableType == VariableType.TimeAggregationFormula)
            { return true; }
            return false;
        }
    }
}