using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.ValueObjects
{
    public enum ParameterType
    {
        TypeReference = 0x00000001,
        InstanceReference = 0x00000010,
        BaseValue = 0x00000100,
    }
}