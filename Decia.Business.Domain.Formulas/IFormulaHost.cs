using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Formulas
{
    public interface IFormulaHost
    {
        ICollection<FormulaId> ContainedFormulaIds { get; }
        ICollection<Guid> ContainedFormulaGuids { get; }
        ICollection<FormulaId> ActiveFormulaIds { get; }
        ICollection<Guid> ActiveFormulaGuids { get; }
    }
}