using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.JsonSets
{
    public enum JsonSchemaType_Extended
    {
        None = 0,
        String = 1,
        Number = 2,
        Integer = 4,
        Boolean = 8,
        Object = 16,
        Array = 32,
        Null = 64,
        Date = 128,
        DateOrString = 128 | 1,
        Buffer = 256,
        BufferOrString = 256 | 1,
        ObjectId = 512,
        ObjectIdOrString = 512 | 1,
    }
}