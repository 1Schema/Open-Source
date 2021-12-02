using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting
{
    public interface IStructuralTitleRange : IValueRange_Configurable, IDimensionalRange, ITransposableElement, IHidableElement
    {
        bool IsVariableTitleRelated { get; }
        bool IsCommonTitleRelated { get; }

        string StyleGroup { get; set; }

        bool OnlyRepeatOnChange { get; set; }
        bool MergeRepeatedValues { get; set; }

        ModelObjectReference EntityTypeRef { get; }
    }
}