using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Reporting
{
    public interface IVariableTitleBox : IDimensionalBox, ITransposableOrderableElement
    {
        ReportElementId ContainedVariableTitleRangeId { get; }
        int ContainedVariableTitleRangeNumber { get; }
        void SetContainedVariableTitleRange(IVariableTitleRange element);

        string RepeatGroup { get; set; }
        RepeatMode RepeatMode { get; set; }
        string StyleGroup { get; set; }

        ModelObjectReference VariableTemplateRef { get; }
    }
}