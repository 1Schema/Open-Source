using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting
{
    public interface ITimeTitleRange : IValueRange_Configurable, IDimensionalRange, ITransposableElement, IHidableElement
    {
        bool IsVariableTitleRelated { get; }
        bool IsCommonTitleRelated { get; }

        string StyleGroup { get; set; }

        bool OnlyRepeatOnChange { get; set; }
        bool MergeRepeatedValues { get; set; }

        TimeDimensionType TimeDimensionType { get; }
        TimeValueType TimeValueType { get; }
        TimePeriodType TimePeriodType { get; set; }
    }
}