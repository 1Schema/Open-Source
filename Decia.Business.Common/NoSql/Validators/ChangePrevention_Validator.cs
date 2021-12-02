using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.NoSql.Validators
{
    public class ChangePrevention_Validator
    {
        private GenericCollection genericCollection;

        public ChangePrevention_Validator(GenericCollection genericCollection)
        {
            this.genericCollection = genericCollection;
        }
    }
}