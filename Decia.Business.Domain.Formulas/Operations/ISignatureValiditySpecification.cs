using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Formulas.Operations
{
    public interface ISignatureValiditySpecification
    {
        IList<Parameter> Parameters { get; }
        IList<Parameter> RequiredParameters { get; }
        IList<Parameter> OptionalParameters { get; }
        Nullable<Parameter> ParameterAllowingMultipleInstances { get; }

        int ParameterCount { get; }
        int RequiredParameterCount { get; }
        int OptionalParameterCount { get; }
        bool HasParameterAllowingMultipleInstances { get; }

        bool TryGetValidReturnType(IEnumerable<DeciaDataType> desiredInputTypes, out DeciaDataType returnType);
        bool TryGetValidReturnType(IDictionary<int, DeciaDataType> desiredInputTypes, out DeciaDataType returnType);

        bool TryGetValidSignature(IEnumerable<DeciaDataType> desiredInputTypes, out Signature signature);
        bool TryGetValidSignature(IDictionary<int, DeciaDataType> desiredInputTypes, out Signature signature);

        bool IsSpecificInstanceOf(Signature baseSignature, Signature inferredSignature);
    }
}