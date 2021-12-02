using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Common.Modeling
{
    public class ModelObjectDescriptor : KeyedOrderable<ModelObjectReference>
    {
        public ModelObjectDescriptor(ModelObjectReference key, long? orderNumber, string name, object dataTag)
            : base(key, orderNumber, name)
        {
            DataTag = dataTag;
        }

        public object DataTag { get; protected set; }
    }
}