using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum ReportElementType_New
    {
        [Description("Report")]
        Report,

        [Description("Header")]
        Header,
        [Description("Footer")]
        Footer,

        [Description("Cell")]
        Cell,
        [Description("Container")]
        Container,

        [Description("Dimensional Table")]
        DimensionalTable,

        [Description("Table Header")]
        DimensionalTable_TableHeader,
        [Description("Row Header")]
        DimensionalTable_RowHeader,
        [Description("Column Header")]
        DimensionalTable_ColumnHeader,
        [Description("Data Area")]
        DimensionalTable_DataArea,

        [Description("Common Title Container")]
        DimensionalTable_CommonTitleContainer,
        [Description("Variable Title Container")]
        DimensionalTable_VariableTitleContainer,
        [Description("Variable Data Container")]
        DimensionalTable_VariableDataContainer,

        [Description("Common Title Box")]
        DimensionalTable_CommonTitleBox,
        [Description("Variable Title Box")]
        DimensionalTable_VariableTitleBox,
        [Description("Variable Data Box")]
        DimensionalTable_VariableDataBox,

        [Description("Structural Title Range")]
        DimensionalTable_StructuralTitleRange,
        [Description("Time Title Range")]
        DimensionalTable_TimeTitleRange,
        [Description("Variable Title Range")]
        DimensionalTable_VariableTitleRange,
        [Description("Variable Data Range")]
        DimensionalTable_VariableDataRange
    }
}