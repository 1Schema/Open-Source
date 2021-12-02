using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.ValueObjects
{
    public enum OperationValidityType
    {
        None = 0x0000000,
        Model = 0x0000001,
        Variable = 0x0000010,
        Report = 0x0000100,
        Element = 0x0001000,
        All = 0x1111111
    }

    public static class OperationValidityTypeUtils
    {
        public static bool IsValid(this OperationValidityType requiredValidityType, OperationValidityType actualValidityType)
        {
            return (requiredValidityType == (requiredValidityType | actualValidityType));
        }
    }
}