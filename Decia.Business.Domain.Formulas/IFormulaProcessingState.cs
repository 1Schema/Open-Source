using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;

namespace Decia.Business.Domain.Formulas
{
    public interface IFormulaProcessingState
    {
        IProcessingState InitializationState { get; }
        IProcessingState ValidationState { get; }
        IProcessingState ComputationState { get; }
    }
}