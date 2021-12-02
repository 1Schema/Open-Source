using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Reporting
{
    public interface IVariableDataBox : IReadOnlyContainer, ITransposableOrderableElement
    {
        ReportElementId RelatedVariableTitleBoxId { get; }
        int RelatedVariableTitleBoxNumber { get; }
        ReportElementId RelatedCommonTitleBoxId { get; }
        int RelatedCommonTitleBoxNumber { get; }

        ReportElementId ContainedVariableDataRangeId { get; }
        int ContainedVariableDataRangeNumber { get; }
        void SetContainedVariableDataRange(IVariableDataRange element);

        string StyleGroup { get; set; }

        ModelObjectReference VariableTemplateRef { get; set; }
    }
}