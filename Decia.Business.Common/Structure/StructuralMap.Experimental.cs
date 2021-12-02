using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Node = Decia.Business.Common.Modeling.ModelObjectReference;
using IEdge = Decia.Business.Common.DataStructures.IEdge<Decia.Business.Common.Modeling.ModelObjectReference>;
using Edge = Decia.Business.Common.DataStructures.Edge<Decia.Business.Common.Modeling.ModelObjectReference>;
using ReferenceListKey = Decia.Business.Common.TypedIds.ListBasedKey<Decia.Business.Common.Modeling.ModelObjectReference>;

namespace Decia.Business.Common.Structure
{
    public partial class StructuralMap
    {
        #region Structural Type Navigation Methods

        public JoinPath GetJoinPath(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef)
        {
            IEnumerable<ModelObjectReference> orderedRelatedTypeRefs = new ModelObjectReference[] { relatedTypeRef };
            return GetJoinPath(mainTypeRef, orderedRelatedTypeRefs);
        }

        public JoinPath GetJoinPath(ModelObjectReference mainTypeRef, IEnumerable<ModelObjectReference> relatedTypeRefs)
        {
            var joinPath = new JoinPath(this, mainTypeRef, relatedTypeRefs);
            return joinPath;
        }

        #endregion
    }
}