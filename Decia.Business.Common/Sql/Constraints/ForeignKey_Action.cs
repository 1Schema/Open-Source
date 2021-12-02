using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Sql.Constraints
{
    public enum ForeignKey_Action
    {
        Nothing,
        Cascade,
        Set_Null,
        Set_Default,
    }
}