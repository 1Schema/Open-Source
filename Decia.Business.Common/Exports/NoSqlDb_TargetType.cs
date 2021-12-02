using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Exports
{
    public enum NoSqlDb_TargetType
    {
        [Description("MongoDB®")]
        [IsImplemented(true)]
        MongoDb,
    }
}