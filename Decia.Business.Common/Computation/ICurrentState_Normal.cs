using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Computation
{
    public interface ICurrentState_Normal : ICurrentState_Base
    {
        ModelObjectReference StructuralTypeRef { get; }
        ModelObjectReference StructuralInstanceRef { get; }
        Nullable<ModelObjectReference> NullableStructuralInstanceRef { get; }

        string DebugHelperText { get; set; }
    }
}