using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.TypedIds
{
    public interface IParentId
    {
        ModelObjectType ModelObjectType { get; }
        ICollection<ModelObjectReference> IdReferences { get; }
    }
}