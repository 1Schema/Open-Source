using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.DynamicValues
{
    public static class DynamicValueExtensions
    {
        public static object GetAsObject(this DynamicValue dynamicValue)
        {
            return dynamicValue;
        }
    }
}