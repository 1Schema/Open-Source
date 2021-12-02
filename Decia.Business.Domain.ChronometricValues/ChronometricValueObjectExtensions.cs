using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Domain.DynamicValues;

namespace Decia.Business.Domain.ChronometricValues
{
    public static class ChronometricValueObjectExtensions
    {
        public static object GetAsObject(this ChronometricValue chronometricValue)
        {
            return chronometricValue;
        }

        public static object GetAsObject(this DynamicValue dynamicValue)
        {
            return dynamicValue.GetAsObject();
        }
    }
}