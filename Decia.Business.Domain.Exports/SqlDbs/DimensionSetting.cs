using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Exports.SqlDbs
{
    public class DimensionSetting
    {
        public ModelObjectReference Dimension { get; protected set; }
        public bool ProcessValuesIndividually { get; protected set; }
        public bool CurrentValue_ParameterName { get; protected set; }
    }
}