using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Formulas.Operations
{
    public class SignatureMember
    {
        public SignatureMember(DeciaDataType dataType, bool allowsDynamic)
        {
            DataType = dataType;
            AllowsDynamic = allowsDynamic;
        }

        public DeciaDataType DataType { get; protected set; }
        public bool AllowsDynamic { get; protected set; }
    }
}