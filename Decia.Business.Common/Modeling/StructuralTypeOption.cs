using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public enum StructuralTypeOption
    {
        [Description("Global Type")]
        GlobalType,
        [Description("Entity Type")]
        EntityType,
        [Description("Relation Type")]
        RelationType
    }
}