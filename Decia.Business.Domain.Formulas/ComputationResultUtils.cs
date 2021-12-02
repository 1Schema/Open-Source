using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Formulas
{
    public static class ComputationResultUtils
    {
        public static ComputationResult GetFirstSourceOfError(this ComputationResult rootResult)
        {
            if (rootResult.IsValid)
            { return null; }

            IEnumerable<ComputationResult> nestedExressionErrors = rootResult.NestedExpressionResults.Values.Where(r => !r.IsValid).ToList();
            ComputationResult firstNestedExressionError = nestedExressionErrors.FirstOrDefault();
            if (firstNestedExressionError != null)
            { return GetFirstSourceOfError(firstNestedExressionError); }

            IEnumerable<ComputationResult> nestedArgumentErrors = rootResult.NestedArgumentResults.Values.Where(r => !r.IsValid).ToList();
            ComputationResult firstNestedArgumentError = nestedArgumentErrors.FirstOrDefault();
            if (firstNestedArgumentError != null)
            { return GetFirstSourceOfError(firstNestedArgumentError); }

            return rootResult;
        }
    }
}