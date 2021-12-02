using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Schema;

namespace Decia.Business.Common.JsonSets
{
    public class JsonSet
    {
        public const bool Default_IncludeIds = false;
        public const bool Default_IncludeDocsAsArray = false;

        public const string Default_Name = "JsonSet";
        public const string UriFormat = "http://{0}.json";

        #region Members

        private string m_Name;
        private Uri m_UriBaseForSet;
        private Dictionary<Guid, JsonDocument> m_DocumentsById;
        private Dictionary<string, JsonDocument> m_DocumentsByName;

        #endregion

        #region Constructors

        public JsonSet()
            : this(Default_Name)
        { }

        public JsonSet(string name)
        {
            var name_Escaped = name.ToEscaped_VarName();
            var uri = string.Format(UriFormat, name_Escaped);

            m_Name = name;
            m_UriBaseForSet = new Uri(uri);

            m_DocumentsById = new Dictionary<Guid, JsonDocument>();
            m_DocumentsByName = new Dictionary<string, JsonDocument>();
        }

        #endregion

        #region Properties

        public string Name { get { return m_Name; } }
        public Uri UriBaseForSet { get { return m_UriBaseForSet; } }

        public Dictionary<Guid, JsonDocument> DocumentsById { get { return m_DocumentsById.ToDictionary(x => x.Key, x => x.Value); } }
        public Dictionary<string, JsonDocument> DocumentsByName { get { return m_DocumentsByName.ToDictionary(x => x.Key, x => x.Value); } }

        #endregion

        #region Methods

        public JsonDocument CreateDocument(string name)
        {
            var jsonDoc = new JsonDocument(this, name);
            AddDocument(jsonDoc);
            return jsonDoc;
        }

        protected T AddDocument<T>(T jsonDoc)
            where T : JsonDocument
        {
            m_DocumentsById.Add(jsonDoc.DocId, jsonDoc);
            m_DocumentsByName.Add(jsonDoc.DocName, jsonDoc);
            return jsonDoc;
        }

        public JSchema GenerateSchema()
        {
            var documentSchemas = new Dictionary<JsonDocument, JSchema>();

            var rootSchema = new JSchema();
            rootSchema.Title = this.Name;
            rootSchema.Type = JSchemaType.Object;

            if (JsonSet.Default_IncludeIds)
            { rootSchema.Id = this.UriBaseForSet; }

            var docsSchema = rootSchema;
            if (Default_IncludeDocsAsArray)
            {
                docsSchema = new JSchema();
                rootSchema.Properties.Add("Documents", docsSchema);
            }

            foreach (var documentBucket in DocumentsById)
            {
                var document = documentBucket.Value;

                var documentSchema = new JSchema();
                documentSchema.Title = document.DocName;
                documentSchema.Type = JSchemaType.Object;

                if (JsonSet.Default_IncludeIds)
                { documentSchema.Id = document.DocUri; }

                documentSchemas.Add(document, documentSchema);

                if (Default_IncludeDocsAsArray)
                { docsSchema.Items.Add(documentSchema); }
                else
                { docsSchema.Properties.Add(document.DocName, documentSchema); }
            }

            foreach (var documentBucket in DocumentsById)
            {
                var document = documentBucket.Value;
                var documentSchema = documentSchemas[document];

                foreach (var nestedPropertyBucket in document.PropertiesById)
                {
                    var nestedProperty = nestedPropertyBucket.Value;
                    var nestedJSchema = nestedProperty.GenerateSchema(documentSchemas);

                    documentSchema.Properties.Add(nestedProperty.Name, nestedJSchema);
                }
            }

            return rootSchema;
        }

        #endregion
    }
}