using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.NoSql
{
    public enum FieldMode
    {
        Data,
        NestedCollection,
        CachedDocument,
        ReplicatedList
    }
}