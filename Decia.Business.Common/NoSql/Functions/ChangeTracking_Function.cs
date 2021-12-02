using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.NoSql.Functions
{
    public class ChangeTracking_Function
    {
        private GenericCollection genericCollection;
        private bool isDimensionalSource;

        public ChangeTracking_Function(GenericCollection genericCollection, bool isDimensionalSource)
        {
            this.genericCollection = genericCollection;
            this.isDimensionalSource = isDimensionalSource;
        }
    }
}