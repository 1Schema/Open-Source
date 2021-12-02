using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public enum StructuralInstanceOption
    {
        [Description("Global Instance")]
        GlobalInstance,
        [Description("Entity Instance")]
        EntityInstance,
        [Description("Relation Instance")]
        RelationInstance
    }
}