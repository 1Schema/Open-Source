using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Structure
{
    public partial interface IStructuralMap
    {
        #region Structural Type Navigation Methods

        JoinPath GetJoinPath(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef);
        JoinPath GetJoinPath(ModelObjectReference mainTypeRef, IEnumerable<ModelObjectReference> relatedTypeRefs);

        #endregion
    }
}