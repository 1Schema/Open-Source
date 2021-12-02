using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;

namespace Decia.Business.Common.Dependencies
{
    public partial interface IDependencyMap
    {
        #region Navigation Methods

        NavigationSpecification GetNavigationSpecification(ModelObjectReference structuralTypeRef);

        #endregion
    }
}