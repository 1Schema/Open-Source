using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Schema;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.JsonSets
{
    public class JsonProperty
    {
        public const string PropertyName_ForArrayItem = "items";
        public const string Default_Description = "";
        public const bool Default_IsRequired = false;

        #region Members

        protected JsonDocument m_ParentDocument;
        protected JsonProperty m_ParentProperty;

        protected Guid m_Id;
        protected string m_Name;
        protected string m_Description;
        protected bool m_IsRequired;

        protected DeciaComplexType? m_ComplexType;
        protected DeciaDataType? m_SimpleType;

        protected JsonDocument m_ReferencedType;
        protected JsonProperty m_ReferencedProperty;
        protected Dictionary<Guid, JsonProperty> m_NestedProperties;

        #endregion

        #region Constructors
        #region Constructors for Root Document

        internal JsonProperty(JsonDocument parentDocument, string name, string description, bool isRequired)
        {
            if (parentDocument == null)
            { throw new InvalidOperationException("The Parent Document must not be null."); }

            m_Id = Guid.NewGuid();
            m_Name = name;
            m_Description = description;
            m_IsRequired = isRequired;

            m_ComplexType = DeciaComplexType.Object;
            m_SimpleType = null;

            m_ReferencedType = null;
            m_ReferencedProperty = null;
            m_NestedProperties = new Dictionary<Guid, JsonProperty>();

            m_ParentDocument = parentDocument;
            m_ParentProperty = null;
        }

        #endregion
        #region Constructors for Array Members

        public JsonProperty(JsonProperty parentProperty)
            : this(parentProperty, (JsonDocument)null, (JsonProperty)null)
        { }

        public JsonProperty(JsonProperty parentProperty, JsonDocument referencedType, JsonProperty referencedProperty)
        {
            if (parentProperty == null)
            { throw new InvalidOperationException("The Parent Property must not be null."); }
            if (!parentProperty.IsComplexType)
            { throw new InvalidOperationException("The Parent Property must represent an Array."); }
            if (parentProperty.ComplexType != DeciaComplexType.Array)
            { throw new InvalidOperationException("The Parent Property must represent an Array."); }
            if (parentProperty.PropertiesById.Count > 0)
            { throw new InvalidOperationException("The Parent Property that are Arrays can only contain one Object."); }

            m_Id = Guid.NewGuid();
            m_Name = PropertyName_ForArrayItem;
            m_Description = Default_Description;
            m_IsRequired = Default_IsRequired;

            m_ComplexType = DeciaComplexType.Object;
            m_SimpleType = null;

            m_ReferencedType = referencedType;
            m_ReferencedProperty = referencedProperty;
            m_NestedProperties = (UsesReferencedType) ? null : new Dictionary<Guid, JsonProperty>();

            m_ParentDocument = parentProperty.ParentDocument;
            m_ParentProperty = parentProperty;
            m_ParentProperty.m_NestedProperties.Add(this.Id, this);
        }

        public JsonProperty(JsonProperty parentProperty, DeciaDataType simpleType)
           : this(parentProperty, simpleType, Default_IsRequired)
        { }

        public JsonProperty(JsonProperty parentProperty, DeciaDataType simpleType, bool isRequired)
        {
            if (parentProperty == null)
            { throw new InvalidOperationException("The Parent Property must not be null."); }
            if (!parentProperty.IsComplexType)
            { throw new InvalidOperationException("The Parent Property must represent an Object."); }
            if (parentProperty.ComplexType != DeciaComplexType.Array)
            { throw new InvalidOperationException("The Parent Property must represent an Array."); }
            if (parentProperty.UsesReferencedType)
            { throw new InvalidOperationException("The Parent Property uses a referenced type."); }

            m_Id = Guid.NewGuid();
            m_Name = PropertyName_ForArrayItem;
            m_Description = Default_Description;
            m_IsRequired = isRequired;

            m_ComplexType = null;
            m_SimpleType = simpleType;

            m_ReferencedType = null;
            m_ReferencedProperty = null;
            m_NestedProperties = null;

            m_ParentDocument = parentProperty.ParentDocument;
            m_ParentProperty = parentProperty;
            m_ParentProperty.m_NestedProperties.Add(this.Id, this);
        }

        #endregion
        #region Constructors for Object Members

        public JsonProperty(JsonProperty parentProperty, string name, JsonDocument referencedType, JsonProperty referencedProperty)
        {
            if (parentProperty == null)
            { throw new InvalidOperationException("The Parent Property must not be null."); }
            if (!parentProperty.IsComplexType)
            { throw new InvalidOperationException("The Parent Property must represent an Object."); }
            if (parentProperty.ComplexType != DeciaComplexType.Object)
            { throw new InvalidOperationException("The Parent Property must represent an Object."); }
            if (parentProperty.UsesReferencedType)
            { throw new InvalidOperationException("The Parent Property uses a referenced type."); }

            m_Id = Guid.NewGuid();
            m_Name = name;
            m_Description = Default_Description;
            m_IsRequired = Default_IsRequired;

            m_ComplexType = DeciaComplexType.Object;
            m_SimpleType = null;

            m_ReferencedType = referencedType;
            m_ReferencedProperty = referencedProperty;
            m_NestedProperties = null;

            m_ParentDocument = parentProperty.ParentDocument;
            m_ParentProperty = parentProperty;
            m_ParentProperty.m_NestedProperties.Add(this.Id, this);
        }

        public JsonProperty(JsonProperty parentProperty, string name, DeciaComplexType complexType)
        {
            if (parentProperty == null)
            { throw new InvalidOperationException("The Parent Property must not be null."); }
            if (!parentProperty.IsComplexType)
            { throw new InvalidOperationException("The Parent Property must represent an Object."); }
            if (parentProperty.ComplexType != DeciaComplexType.Object)
            { throw new InvalidOperationException("The Parent Property must represent an Object."); }
            if (parentProperty.UsesReferencedType)
            { throw new InvalidOperationException("The Parent Property uses a referenced type."); }

            m_Id = Guid.NewGuid();
            m_Name = name;
            m_Description = Default_Description;
            m_IsRequired = Default_IsRequired;

            m_ComplexType = complexType;
            m_SimpleType = null;

            m_ReferencedType = null;
            m_ReferencedProperty = null;
            m_NestedProperties = new Dictionary<Guid, JsonProperty>();

            m_ParentDocument = parentProperty.ParentDocument;
            m_ParentProperty = parentProperty;
            m_ParentProperty.m_NestedProperties.Add(this.Id, this);
        }

        public JsonProperty(JsonProperty parentProperty, string name, DeciaDataType simpleType)
            : this(parentProperty, name, simpleType, Default_IsRequired)
        { }

        public JsonProperty(JsonProperty parentProperty, string name, DeciaDataType simpleType, bool isRequired)
        {
            if (parentProperty == null)
            { throw new InvalidOperationException("The Parent Property must not be null."); }
            if (!parentProperty.IsComplexType)
            { throw new InvalidOperationException("The Parent Property must represent an Object."); }
            if (parentProperty.ComplexType != DeciaComplexType.Object)
            { throw new InvalidOperationException("The Parent Property must represent an Object."); }
            if (parentProperty.UsesReferencedType)
            { throw new InvalidOperationException("The Parent Property uses a referenced type."); }

            m_Id = Guid.NewGuid();
            m_Name = name;
            m_Description = Default_Description;
            m_IsRequired = isRequired;

            m_ComplexType = null;
            m_SimpleType = simpleType;

            m_ReferencedType = null;
            m_ReferencedProperty = null;
            m_NestedProperties = null;

            m_ParentDocument = parentProperty.ParentDocument;
            m_ParentProperty = parentProperty;
            m_ParentProperty.m_NestedProperties.Add(this.Id, this);
        }

        #endregion
        #endregion

        #region Properties

        public JsonDocument ParentDocument { get { return m_ParentDocument; } }
        public JsonProperty ParentProperty { get { return m_ParentProperty; } }

        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }
        public bool IsRequired
        {
            get { return m_IsRequired; }
            set { m_IsRequired = value; }
        }

        public bool IsComplexType { get { return (m_ComplexType.HasValue); } }
        public bool IsSimpleType { get { return (!IsComplexType); } }
        public DeciaComplexType ComplexType { get { return m_ComplexType.Value; } }
        public bool IsArray { get { return (IsComplexType && (ComplexType == DeciaComplexType.Array)); } }
        public bool IsObject { get { return (IsComplexType && (ComplexType == DeciaComplexType.Object)); } }
        public DeciaDataType SimpleType { get { return m_SimpleType.Value; } }

        public bool UsesReferencedType { get { return (m_ReferencedType != null); } }
        public JsonDocument ReferencedType { get { return m_ReferencedType; } }
        public JsonProperty ReferencedProperty { get { return m_ReferencedProperty; } }

        public bool UsesProperties { get { return (m_NestedProperties != null); } }
        public Dictionary<Guid, JsonProperty> PropertiesById { get { return m_NestedProperties.ToDictionary(x => x.Key, x => x.Value); } }
        public Dictionary<string, JsonProperty> PropertiesByName { get { return m_NestedProperties.ToDictionary(x => x.Value.Name, x => x.Value); } }

        #endregion

        #region Methods

        public void SetToReference(JsonDocument referencedType, JsonProperty referencedProperty)
        {
            if (UsesReferencedType)
            { throw new InvalidOperationException("The Reference has already been set."); }

            m_ReferencedType = referencedType;
            m_ReferencedProperty = referencedProperty;
        }

        public void MovePropertyToLastPlace(JsonProperty property)
        {
            if (!m_NestedProperties.ContainsKey(property.Id))
            { throw new InvalidOperationException("Only properties that have already been added can be moved."); }

            var matchingPropertyBucket = (KeyValuePair<Guid, JsonProperty>?)null;

            foreach (var nestedPropertyBucket in m_NestedProperties.ToList())
            {
                if (nestedPropertyBucket.Key != property.Id)
                { continue; }

                matchingPropertyBucket = nestedPropertyBucket;
                m_NestedProperties.Remove(property.Id);
            }

            if (matchingPropertyBucket.HasValue)
            {
                m_NestedProperties = new Dictionary<Guid, JsonProperty>(m_NestedProperties);
                m_NestedProperties.Add(matchingPropertyBucket.Value.Key, matchingPropertyBucket.Value.Value);
            }
        }

        public JSchema GenerateSchema(Dictionary<JsonDocument, JSchema> documentSchemas)
        {
            var schemaExists = ((ParentDocument != null) && (ParentProperty == null));
            schemaExists = (schemaExists) ? documentSchemas.ContainsKey(ParentDocument) : schemaExists;

            var jSchema = (schemaExists) ? documentSchemas[ParentDocument] : new JSchema();
            if (!string.IsNullOrWhiteSpace(Description))
            { jSchema.Description = Description; }

            if (IsComplexType)
            { jSchema.Type = ComplexType.GetJsonType(); }
            else if (IsSimpleType)
            { jSchema.Type = SimpleType.GetJsonType(); }
            else
            { throw new InvalidOperationException("The Json Property is not configured correctly."); }

            if (IsObject)
            {
                if (UsesReferencedType)
                {
                    if (documentSchemas.ContainsKey(ReferencedType))
                    { jSchema = documentSchemas[ReferencedType]; }
                    else
                    {
                        jSchema.Title = ReferencedType.DocName;

                        if (JsonSet.Default_IncludeIds)
                        { jSchema.Id = ReferencedType.DocUri; }
                    }
                }
                else if (IsComplexType)
                {
                    foreach (var nestedPropertyBucket in m_NestedProperties)
                    {
                        var nestedProperty = nestedPropertyBucket.Value;
                        var nestedJSchema = nestedProperty.GenerateSchema(documentSchemas);

                        jSchema.Properties.Add(nestedProperty.Name, nestedJSchema);

                        if (nestedProperty.IsRequired)
                        { jSchema.Required.Add(nestedProperty.Name); }
                    }
                }
                return jSchema;
            }
            else if (IsArray)
            {
                if (UsesReferencedType)
                {
                    if (documentSchemas.ContainsKey(ReferencedType))
                    { jSchema = documentSchemas[ReferencedType]; }
                    else
                    {
                        jSchema.Title = ReferencedType.DocName;

                        if (JsonSet.Default_IncludeIds)
                        { jSchema.Id = ReferencedType.DocUri; }
                    }
                }
                else
                {
                    foreach (var nestedPropertyBucket in m_NestedProperties)
                    {
                        var nestedProperty = nestedPropertyBucket.Value;
                        var nestedJSchema = nestedProperty.GenerateSchema(documentSchemas);

                        jSchema.Items.Add(nestedJSchema);
                    }
                }
                return jSchema;
            }
            else
            {
                if (UsesReferencedType)
                {
                    if (documentSchemas.ContainsKey(ReferencedType))
                    { jSchema = documentSchemas[ReferencedType]; }
                    else
                    {
                        jSchema.Title = ReferencedType.DocName;

                        if (JsonSet.Default_IncludeIds)
                        { jSchema.Id = ReferencedType.DocUri; }
                    }
                }
                return jSchema;
            }
        }

        #endregion
    }
}