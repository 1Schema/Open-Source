using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Edge = Decia.Business.Common.DataStructures.Edge<Decia.Business.Common.Modeling.ModelObjectReference>;

namespace Decia.Business.Common.Dependencies
{
    public partial class DependencyMap
    {
        #region Navigation Methods

        public NavigationSpecification GetNavigationSpecification(ModelObjectReference structuralTypeRef)
        {
            if (!m_StructuralTypeNavigationSpecifications.ContainsKey(structuralTypeRef))
            { return null; }
            return m_StructuralTypeNavigationSpecifications[structuralTypeRef];
        }

        #endregion
    }
}