using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Reporting
{
    public interface ITransposableElement : IReportElement
    {
        Dimension StackingDimension { get; }
        Dimension CommonDimension { get; }

        bool IsTransposed { get; }
    }

    public interface ITransposableOrderableElement : ITransposableElement
    {
        int StackingOrder { get; }
        int CommonOrder { get; }

        int GetOrder(Dimension dimension);
        bool IsOrderEditable(Dimension dimension);
        void SetOrder(Dimension dimension, int order);
    }
}