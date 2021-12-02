using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Formulas.Operations
{
    public static class ISignatureValiditySpecificationUtils
    {
        public static bool IsSpecificInstanceOf(this ISignatureValiditySpecification spec, Signature baseSignature, Signature inferredSignature)
        {
            if (baseSignature.TypeOut != inferredSignature.TypeOut)
            { return false; }

            if (baseSignature.TypesIn.Count != inferredSignature.TypesIn.Count)
            {
                if (!spec.HasParameterAllowingMultipleInstances)
                { return false; }
            }

            int index = 0;
            for (index = 0; index < spec.ParameterCount; index++)
            {
                if (index >= baseSignature.TypesIn.Count)
                { break; }

                if (baseSignature.TypesInByIndex[index] != inferredSignature.TypesInByIndex[index])
                { return false; }
            }

            if (!spec.HasParameterAllowingMultipleInstances)
            { return true; }

            int baseIndex = (index - 1);
            for (int inferredIndex = index; inferredIndex < inferredSignature.TypesIn.Count; inferredIndex++)
            {
                if (baseSignature.TypesInByIndex[baseIndex] != inferredSignature.TypesInByIndex[inferredIndex])
                { return false; }
            }
            return true;
        }
    }
}