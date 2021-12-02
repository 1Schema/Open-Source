using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.CompoundUnits
{
    public static class CompoundUnitObjectExtensions
    {
        public static object GetAsObject(this CompoundUnit compoundUnit)
        {
            return compoundUnit;
        }
    }
}