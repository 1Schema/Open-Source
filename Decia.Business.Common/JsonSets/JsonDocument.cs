using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Decia.Business.Common.JsonSets
{
    public class JsonDocument
    {
        public const bool AssertInstanceIsUnique = false;

        #region Members

        protected JsonSet m_ParentSet;
        protected JsonProperty m_RootObjectProperty;

        protected string m_CollectionName = null;
        protected List<JObject> m_Instances;

        #endregion

        #region Constructors

        public JsonDocument(JsonSet parentSet, string name)
            : this(parentSet, name, JsonProperty.Default_Description)
        { }

        public JsonDocument(JsonSet parentSet, string name, string description)
            : this(parentSet, name, description, JsonProperty.Default_IsRequired)
        { }

        public JsonDocument(JsonSet parentSet, string name, string description, bool isRequired)
        {
            if (parentSet == null)
            { throw new InvalidOperationException("The Parent Set must not be null."); }

            m_RootObjectProperty = new JsonProperty(this, name, description, isRequired);
            m_Instances = new List<JObject>();

            m_ParentSet = parentSet;
        }

        #endregion

        #region Properties

        public JsonSet ParentSet { get { return m_ParentSet; } }
        public JsonProperty Root { get { return m_RootObjectProperty; } }

        public Guid DocId { get { return m_RootObjectProperty.Id; } }
        public string DocName { get { return m_RootObjectProperty.Name; } }
        public string DocDescription { get { return m_RootObjectProperty.Description; } }
        public bool DocIsRequired { get { return m_RootObjectProperty.IsRequired; } }
        public Uri DocUri { get { return new Uri(ParentSet.UriBaseForSet, DocName); } }

        public string CollectionName
        {
            get { return (!string.IsNullOrWhiteSpace(m_CollectionName)) ? m_CollectionName : DocName; }
            set { m_CollectionName = value; }
        }
        public IList<JObject> Instances { get { return m_Instances.AsReadOnly(); } }

        public Dictionary<Guid, JsonProperty> PropertiesById { get { return m_RootObjectProperty.PropertiesById; } }
        public Dictionary<string, JsonProperty> PropertiesByName { get { return m_RootObjectProperty.PropertiesByName; } }

        #endregion

        #region Methods

        public JSchema GenerateSchema()
        {
            var documentSchemas = new Dictionary<JsonDocument, JSchema>();
            return GenerateSchema(documentSchemas);
        }

        public JSchema GenerateSchema(Dictionary<JsonDocument, JSchema> documentSchemas)
        {
            var schema = m_RootObjectProperty.GenerateSchema(documentSchemas);
            schema.Title = this.DocName;

            if (JsonSet.Default_IncludeIds)
            { schema.Id = DocUri; }

            return schema;
        }

        public void AddInstance(JObject instance)
        {
            if (AssertInstanceIsUnique && Instances.Contains(instance))
            { throw new InvalidOperationException("The Instance has already been added."); }

            m_Instances.Add(instance);
        }

        public bool DoesInstanceMatchSchema(JObject instance)
        {
            var errorMessages = (IList<string>)null;
            return DoesInstanceMatchSchema(instance, out errorMessages);
        }

        public bool DoesInstanceMatchSchema(JObject instance, out IList<string> errorMessages)
        {
            var schema = GenerateSchema();
            return DoesInstanceMatchSchema(instance, schema, out errorMessages);
        }

        protected bool DoesInstanceMatchSchema(JObject instance, JSchema schema, out IList<string> errorMessages)
        {
            return instance.IsValid(schema, out errorMessages);
        }

        public bool DoAllInstancesMatchSchema()
        {
            var errorMessages = (IDictionary<JObject, IList<string>>)null;
            return DoAllInstancesMatchSchema(out errorMessages);
        }

        public bool DoAllInstancesMatchSchema(out IDictionary<JObject, IList<string>> errorMessages)
        {
            var schema = GenerateSchema();
            var isValid = true;

            errorMessages = new Dictionary<JObject, IList<string>>();

            foreach (var instance in m_Instances)
            {
                var instanceErrorMessages = (IList<string>)null;
                var instanceIsValid = false;

                instanceIsValid = DoesInstanceMatchSchema(instance, schema, out instanceErrorMessages);

                if (!instanceIsValid)
                {
                    isValid = false;
                    errorMessages.Add(instance, instanceErrorMessages);
                }
            }
            return isValid;
        }

        public void AssertInstanceMatchesSchema(JObject instance)
        {
            var errorMessages = (IList<string>)null;
            var isValid = DoesInstanceMatchSchema(instance, out errorMessages);

            if (!isValid)
            { throw new InvalidOperationException("The instance does not match the Document's schema."); }
        }

        public void AssertAllInstancesMatchSchema()
        {
            var errorMessages = (IDictionary<JObject, IList<string>>)null;
            var areValid = DoAllInstancesMatchSchema(out errorMessages);

            if (!areValid)
            { throw new InvalidOperationException("There are instances that do not match the Document's schema."); }
        }

        #endregion
    }
}