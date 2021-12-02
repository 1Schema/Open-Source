using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.JsonSets;

namespace Decia.Business.Common.NoSql
{
    public class GenericDatabase_JsonState
    {
        public GenericDatabase_JsonState(JsonSet schema, Dictionary<GenericCollection, KeyValuePair<JsonDocument, JsonProperty>> collectionMappings, Dictionary<GenericField, JsonProperty> fieldMappings)
        {
            Schema = schema;
            CollectionMappings = collectionMappings;
            FieldMappings = fieldMappings;
        }

        public JsonSet Schema { get; protected set; }
        public Dictionary<GenericCollection, KeyValuePair<JsonDocument, JsonProperty>> CollectionMappings { get; protected set; }
        public Dictionary<GenericField, JsonProperty> FieldMappings { get; protected set; }
    }
}