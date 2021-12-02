using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Exports.SqlDbs
{
    public class FormulaExporter
    {
        public Dictionary<ModelObjectReference, DimensionSetting> DimensionSettings { get; protected set; }
    }
}