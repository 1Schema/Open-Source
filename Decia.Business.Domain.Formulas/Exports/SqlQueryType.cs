using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Formulas.Exports
{
    public enum SqlQueryType
    {
        SimpleSelect,
        SelectOneFromMany,
        SelectAggregation
    }
}