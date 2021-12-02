using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public class IsImplementedAttribute : Attribute
    {
        public IsImplementedAttribute(bool isImplemented)
        {
            IsImplemented = isImplemented;
        }

        public bool IsImplemented { get; protected set; }
    }
}