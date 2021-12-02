using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Sql.Triggers;

namespace Decia.Business.Common.Sql.Constraints
{
    public enum ForeignKey_Source
    {
        Self,
        NestedList,
        Matrix,
        SubStructure,
        SubTime,
        SubResult,
    }

    public static class ForeignKey_SourceUtils
    {
        public static string GetConstraint_NamePrefix(this ForeignKey_Source constraintSource)
        {
            if (constraintSource == ForeignKey_Source.Self)
            { return ForeignKey_ConstraintBase.NamePrefix; }
            else if (constraintSource == ForeignKey_Source.Matrix)
            { return Matrix_TriggerSet.NamePrefix; }
            else if (constraintSource == ForeignKey_Source.NestedList)
            { return NestedList_TriggerSet.NamePrefix; }
            else if (constraintSource == ForeignKey_Source.SubResult)
            { return SubTable_Result_TriggerSet.NamePrefix; }
            else if (constraintSource == ForeignKey_Source.SubStructure)
            { return SubTable_Structure_TriggerSet.NamePrefix; }
            else if (constraintSource == ForeignKey_Source.SubTime)
            { return SubTable_Time_TriggerSet.NamePrefix; }
            else
            { throw new InvalidOperationException("Unsupported constraint source encountered."); }
        }
    }
}