using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Exports;
using Decia.Business.Common.JsonSets;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.NoSql.Base;
using Decia.Business.Common.NoSql.Functions;
using GDU = Decia.Business.Common.NoSql.GenericDatabaseUtils;

namespace Decia.Business.Common.NoSql
{
    public class GenericDatabase : IDatabaseMember
    {
        public const string DataSet_Ref_Name = "Ref";

        #region Members

        private Guid m_Id;
        private string m_Name;
        private bool m_UseCachedReferences;
        private Dictionary<Guid, GenericCollection> m_Collections;
        private Dictionary<Guid, GenericCollection> m_RootCollections;
        private Dictionary<Guid, GenericCollection> m_NestedCollections;
        private Dictionary<Guid, GenericFunction> m_Functions;

        private SortedDictionary<string, GenericCollection> m_CollectionsByName;
        private SortedDictionary<string, GenericFunction> m_FunctionsByName;

        private SortedDictionary<int, GenericCollection> m_CollectionsByOrder;
        private SortedDictionary<int, GenericFunction> m_FunctionsByOrder;

        private XmlDatabaseMapping m_XmlMapping = null;

        #endregion

        #region Constructors

        public GenericDatabase(string databaseName, bool useCachedReferences)
        {
            m_Id = Guid.NewGuid();
            m_Name = databaseName;
            m_UseCachedReferences = useCachedReferences;

            m_Collections = new Dictionary<Guid, GenericCollection>();
            m_RootCollections = new Dictionary<Guid, GenericCollection>();
            m_NestedCollections = new Dictionary<Guid, GenericCollection>();
            m_Functions = new Dictionary<Guid, GenericFunction>();

            m_CollectionsByOrder = new SortedDictionary<int, GenericCollection>();
            m_FunctionsByOrder = new SortedDictionary<int, GenericFunction>();

            m_CollectionsByName = new SortedDictionary<string, GenericCollection>();
            m_FunctionsByName = new SortedDictionary<string, GenericFunction>();

            SortCollectionsOnName = false;
        }

        #endregion

        #region Properties

        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public string Name_Escaped { get { return Name.ToEscaped_VarName(); } }
        public bool UseCachedReferences { get { return m_UseCachedReferences; } }

        public ICollection<GenericCollection> Collections
        {
            get
            {
                var collections = new ReadOnlyList<GenericCollection>(m_Collections.Values);
                collections.IsReadOnly = true;
                return collections;
            }
        }

        public ICollection<GenericCollection> RootCollections
        {
            get
            {
                var rootCollections = new ReadOnlyList<GenericCollection>(m_RootCollections.Values);
                rootCollections.IsReadOnly = true;
                return rootCollections;
            }
        }

        public ICollection<GenericCollection> NestedCollections
        {
            get
            {
                var nestedCollections = new ReadOnlyList<GenericCollection>(m_NestedCollections.Values);
                nestedCollections.IsReadOnly = true;
                return nestedCollections;
            }
        }

        public bool SortCollectionsOnName { get; set; }
        public ICollection<GenericCollection> RootCollections_Ordered
        {
            get
            {
                if (!SortCollectionsOnName)
                { return RootCollections; }
                return RootCollections.OrderBy(x => x.Name_Pluralized).ToList();
            }
        }

        #endregion

        #region Methods

        public GenericCollection CreateCollection(string collectionName, ModelObjectReference structuralTypeRef)
        {
            return CreateCollection(collectionName, structuralTypeRef, null);
        }

        public GenericCollection CreateCollection(string collectionName, ModelObjectReference structuralTypeRef, GenericCollection parentCollection)
        {
            var collection = new GenericCollection(this, collectionName, structuralTypeRef, parentCollection);

            m_Collections.Add(collection.Id, collection);
            m_CollectionsByName.Add(collection.Name, collection);
            m_CollectionsByOrder.Add(m_CollectionsByOrder.Count, collection);

            if (parentCollection == null)
            { m_RootCollections.Add(collection.Id, collection); }
            else
            { m_NestedCollections.Add(collection.Id, collection); }

            return collection;
        }

        public GenericCollection GetCollection(Guid collectionId)
        {
            if (!m_Collections.ContainsKey(collectionId))
            { return null; }
            return m_Collections[collectionId];
        }

        public void AddFunction(GenericFunction function)
        {
            if (function.ParentDatabase != this)
            { throw new InvalidOperationException("The Parent Database does not match."); }
            if (m_Functions.Values.Contains(function))
            { throw new InvalidOperationException("The Procedure has already been added."); }

            var index = m_Functions.Count;

            m_Functions.Add(function.Id, function);
            m_FunctionsByName.Add(function.Name, function);
            m_FunctionsByOrder.Add(index, function);
        }

        public GenericDatabase_JsonState GenerateJsonSet()
        {
            Dictionary<GenericCollection, KeyValuePair<JsonDocument, JsonProperty>> collectionMappings;
            Dictionary<GenericField, JsonProperty> fieldMappings;

            var schema = GenerateJsonSet(out collectionMappings, out fieldMappings);

            var jsonState = new GenericDatabase_JsonState(schema, collectionMappings, fieldMappings);
            return jsonState;
        }

        public JsonSet GenerateJsonSet(out Dictionary<GenericCollection, KeyValuePair<JsonDocument, JsonProperty>> collectionMappings, out Dictionary<GenericField, JsonProperty> fieldMappings)
        {
            collectionMappings = new Dictionary<GenericCollection, KeyValuePair<JsonDocument, JsonProperty>>();
            fieldMappings = new Dictionary<GenericField, JsonProperty>();
            var jsonSet = new JsonSet(Name);

            var xmlSchema = ExportXmlSchema();

            foreach (var collection in m_CollectionsByOrder.Values)
            {
                if (collection.ParentCollection != null)
                { continue; }

                var documentType = jsonSet.CreateDocument(collection.Name);
                documentType.CollectionName = collection.Name_Pluralized;

                var documentProperty = documentType.Root;
                var collectionMapping = new KeyValuePair<JsonDocument, JsonProperty>(documentType, documentProperty);
                collectionMappings.Add(collection, collectionMapping);

                AddNestedCollectionsToJsonSet(collection, documentType, documentProperty, collectionMappings, fieldMappings);
            }

            foreach (var collection in RootCollections)
            {
                var documentType = collectionMappings[collection].Key;
                var documentProperty = documentType.Root;

                AddFieldsToJsonSet(collection, documentProperty, collectionMappings, fieldMappings);
            }

            return jsonSet;
        }

        private static void AddNestedCollectionsToJsonSet(GenericCollection collection, JsonDocument rootDocument, JsonProperty parentProperty, Dictionary<GenericCollection, KeyValuePair<JsonDocument, JsonProperty>> collectionMappings, Dictionary<GenericField, JsonProperty> fieldMappings)
        {
            foreach (var field in collection.LocalFields.ToList())
            {
                var xmlMapping = field.XmlMapping;
                var isNestedCollection = (xmlMapping.FieldMappingType == FieldMappingType.PassThrough);

                if (!isNestedCollection)
                { continue; }

                var nestedCollection = field.NestedCollection;

                var arrayProperty = new JsonProperty(parentProperty, xmlMapping.RootFieldElement.Name, DeciaComplexType.Array);
                var objectProperty = new JsonProperty(arrayProperty);
                var collectionMapping = new KeyValuePair<JsonDocument, JsonProperty>(rootDocument, objectProperty);

                collectionMappings.Add(nestedCollection, collectionMapping);
                fieldMappings.Add(field, arrayProperty);

                AddNestedCollectionsToJsonSet(nestedCollection, rootDocument, objectProperty, collectionMappings, fieldMappings);
            }
        }

        private void AddFieldsToJsonSet(GenericCollection collection, JsonProperty parentProperty, Dictionary<GenericCollection, KeyValuePair<JsonDocument, JsonProperty>> collectionMappings, Dictionary<GenericField, JsonProperty> fieldMappings)
        {
            var nestedPropertiesToMoveAfterCompletion = new List<JsonProperty>();

            foreach (var field in collection.LocalFieldsByOrder.ToList())
            {
                var cachesReferences = this.UseCachedReferences;
                var isReference = ((field.FieldMode == FieldMode.CachedDocument) || (field.FieldMode == FieldMode.ReplicatedList));
                var xmlMapping = field.XmlMapping;
                var isNestedCollection = (xmlMapping.FieldMappingType == FieldMappingType.PassThrough);
                var isComplexType = ((!isNestedCollection) && ((xmlMapping.HasKeyElements || xmlMapping.HasValueElement)));
                var arrayProperty = (JsonProperty)null;
                var objectProperty = (JsonProperty)null;
                var valueDataType = field.DataType;

                if (isNestedCollection)
                {
                    arrayProperty = fieldMappings[field];
                    objectProperty = arrayProperty.PropertiesById.Values.First();

                    nestedPropertiesToMoveAfterCompletion.Add(arrayProperty);
                    AddFieldsToJsonSet(field.NestedCollection, objectProperty, collectionMappings, fieldMappings);
                }
                else if (isComplexType)
                {
                    var isSimpleElementInArray = (!cachesReferences && (xmlMapping.NestedElementCount <= 1));

                    if (xmlMapping.HasArrayItemElement)
                    {
                        arrayProperty = new JsonProperty(parentProperty, xmlMapping.RootFieldElement.Name, DeciaComplexType.Array);
                        fieldMappings.Add(field, arrayProperty);

                        if (field.FieldMode == FieldMode.ReplicatedList)
                        { nestedPropertiesToMoveAfterCompletion.Add(arrayProperty); }
                    }
                    else
                    {
                        objectProperty = new JsonProperty(parentProperty, xmlMapping.RootFieldElement.Name, DeciaComplexType.Object);
                        fieldMappings.Add(field, objectProperty);
                    }


                    if (isSimpleElementInArray)
                    {
                        var currentProperty = new JsonProperty(arrayProperty, valueDataType);

                        if (!cachesReferences && isReference)
                        {
                            var refMapping = collectionMappings[field.CachedDocumentSource];
                            currentProperty.SetToReference(refMapping.Key, refMapping.Value);
                        }
                    }
                    else
                    {
                        if (arrayProperty != null)
                        { objectProperty = new JsonProperty(arrayProperty); }

                        var nestedProperties = new Dictionary<string, JsonProperty>();

                        foreach (var keyElementBucket in xmlMapping.KeyElements)
                        {
                            var keyElementType = keyElementBucket.Key;
                            var keyElement = keyElementBucket.Value;
                            var dataType = DeciaDataType.Text;

                            if (keyElementType.ModelObjectType == ModelObjectType.TimeType)
                            { dataType = DeciaDataType.Text; }
                            else
                            { dataType = DeciaDataType.UniqueID; }

                            var keyProperty = new JsonProperty(objectProperty, keyElement.Name, dataType);
                            nestedProperties.Add(keyElement.Name, keyProperty);
                        }
                        if (xmlMapping.HasValueElement)
                        {
                            var valueElement = xmlMapping.ValueElement;
                            var dataType = valueDataType;

                            var valueProperty = new JsonProperty(objectProperty, valueElement.Name, dataType);
                            nestedProperties.Add(valueElement.Name, valueProperty);
                        }
                        if (xmlMapping.HasCacheElement)
                        {
                            var cacheElement = xmlMapping.CacheElement;
                            var referencedCollection = field.CachedDocumentSource;
                            var referencedMapping = collectionMappings[referencedCollection];
                            var referencedType = referencedMapping.Key;
                            var referencedProperty = referencedMapping.Value;

                            var cacheProperty = new JsonProperty(objectProperty, cacheElement.Name, referencedType, referencedProperty);
                            nestedProperties.Add(cacheElement.Name, cacheProperty);
                        }

                        if (!cachesReferences && nestedProperties.ContainsKey(GenericDatabaseUtils.DocId_Name))
                        {
                            var refProperty = nestedProperties[GenericDatabaseUtils.DocId_Name];
                            var refMapping = collectionMappings[field.CachedDocumentSource];
                            refProperty.SetToReference(refMapping.Key, refMapping.Value);
                        }
                    }
                }
                else
                {
                    if (xmlMapping.HasArrayItemElement)
                    { throw new InvalidOperationException("Simple Elements should not have Arrays."); }

                    var currentProperty = new JsonProperty(parentProperty, xmlMapping.RootFieldElement.Name, valueDataType);
                    fieldMappings.Add(field, currentProperty);

                    if (!cachesReferences && isReference)
                    {
                        var refMapping = collectionMappings[field.CachedDocumentSource];
                        currentProperty.SetToReference(refMapping.Key, refMapping.Value);
                    }
                }
            }

            foreach (var nestedProperty in nestedPropertiesToMoveAfterCompletion)
            { parentProperty.MovePropertyToLastPlace(nestedProperty); }
        }

        #endregion

        #region Export XML Schema Methods

        public bool HasXmlMapping { get { return (m_XmlMapping != null); } }
        public XmlDatabaseMapping XmlMapping { get { return m_XmlMapping; } }

        public XmlDatabaseMapping ExportXmlSchema()
        {
            if (HasXmlMapping)
            { return m_XmlMapping; }

            var dbMapping = new XmlDatabaseMapping(this);

            foreach (var collectionItem in m_CollectionsByOrder)
            {
                var collection = collectionItem.Value;
                var collectionMapping = new XmlCollectionMapping(collection, dbMapping);
            }

            var rootCollectionMappings = dbMapping.CollectionMappings.Where(x => x.Value.IsRoot).ToList();

            foreach (var collectionMappingItem in rootCollectionMappings)
            {
                var collection = collectionMappingItem.Key;
                var collectionMapping = collectionMappingItem.Value;

                collection.ExportXmlSchema(dbMapping, collectionMapping);
                dbMapping.DatabaseType_CollectionSequence.Items.Add(collectionMapping.CollectionElement);
            }

            dbMapping.Schema.Items.Add(dbMapping.DatabaseElement);

            m_XmlMapping = dbMapping;
            return dbMapping;
        }

        #endregion

        #region Export Database Methods

        public XmlDatabaseMapping Exported_XmlSchema { get; protected set; }
        public GenericDatabase_JsonState Exported_JsonSchema { get; protected set; }

        public string ExportToScript(NoSqlDb_TargetType dbType)
        {
            return ExportToScript(dbType, false);
        }

        public string ExportToScript(NoSqlDb_TargetType dbType, bool useMinimalMode)
        {
            Exported_XmlSchema = ExportXmlSchema();
            Exported_JsonSchema = GenerateJsonSet();

            if (dbType == NoSqlDb_TargetType.MongoDb)
            {
                var dbDef = string.Empty;
                dbDef += ExportDatabaseSchema(dbType, useMinimalMode);
                dbDef += ExportDatabaseProgrammatics(dbType, useMinimalMode);
                return dbDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private string ExportDatabaseSchema(NoSqlDb_TargetType dbType, bool useMinimalMode)
        {
            var placeholderValues = GetPlaceholderValues();

            if (dbType == NoSqlDb_TargetType.MongoDb)
            {
                var dbDef = string.Empty;

                dbDef += DeciaBase_Resources.Config_Copyright_Format.InsertPlaceholderValues(placeholderValues) + Environment.NewLine + Environment.NewLine + Environment.NewLine;
                if (UseCachedReferences)
                { dbDef += DeciaBase_Resources.Config_OpLogListener_Control + Environment.NewLine + Environment.NewLine; }

                dbDef += DeciaBase_Resources.Config_DropMyDb_Format.InsertPlaceholderValues(placeholderValues) + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Config_LoadSysDb_Format.InsertPlaceholderValues(placeholderValues) + Environment.NewLine;
                dbDef += DeciaBase_Resources.Config_LoadMyDb_Format.InsertPlaceholderValues(placeholderValues) + Environment.NewLine + Environment.NewLine;

                var prereqSet = new DeciaBase_JsonSet();
                var docSchemasByCollectionName = prereqSet.DocumentsById.Values.ToDictionary(x => x.CollectionName, x => x);
                var deciaBaseClctnNames = (UseCachedReferences) ? ((useMinimalMode) ? DeciaBaseUtils.Clctn_Decia_Names_Minimal : DeciaBaseUtils.Clctn_Decia_Names) : DeciaBaseUtils.Clctn_Decia_Names_None;

                foreach (var baseCollectionName in deciaBaseClctnNames)
                {
                    var docSchema = docSchemasByCollectionName.ContainsKey(baseCollectionName) ? docSchemasByCollectionName[baseCollectionName] : null;
                    var baseCollectionDef = DeciaBase_Resources.ResourceManager.GetString(baseCollectionName);

                    var docSchemaHeader = "DOCUMENT SCHEMA:";
                    var docSchemaText = "{ FOR SYSTEM USE ONLY }";

                    if (docSchema != null)
                    {
                        if (UseCachedReferences)
                        {
                            docSchemaHeader = "DOCUMENT SCHEMA:";
                            docSchemaText = docSchema.GenerateSchema().GetSchemaAsCode_ForInternalChangeMgmt();
                        }
                        else
                        {
                            docSchemaHeader = "DOCUMENT SCHEMA FOR MONGOOSE ODM:";
                            docSchemaText = docSchema.GenerateSchema().GetSchemaAsCode_ForMongoose();
                        }
                    }

                    var firstCommentIndex = baseCollectionDef.IndexOf("/*");
                    var lastCommentIndex = baseCollectionDef.IndexOf("*/");
                    if (firstCommentIndex >= 0)
                    {
                        var removeCharCount = (lastCommentIndex - firstCommentIndex) + 2;
                        baseCollectionDef = baseCollectionDef.Remove(firstCommentIndex, removeCharCount);
                    }

                    var commentText = string.Empty;
                    commentText += "/*" + Environment.NewLine;
                    commentText += docSchemaHeader + Environment.NewLine;
                    commentText += docSchemaText + Environment.NewLine;
                    commentText += "*/";

                    baseCollectionDef = (commentText + baseCollectionDef);
                    dbDef += baseCollectionDef + Environment.NewLine + Environment.NewLine;
                }
                dbDef += Environment.NewLine;

                foreach (var rootCollection in RootCollections_Ordered)
                {
                    var rootCollectionDef = rootCollection.ExportDatabaseSchema(dbType);
                    dbDef += rootCollectionDef + Environment.NewLine + Environment.NewLine;
                }
                dbDef += Environment.NewLine;

                return dbDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private string ExportDatabaseProgrammatics(NoSqlDb_TargetType dbType, bool useMinimalMode)
        {
            var placeholderValues = GetPlaceholderValues();

            if (dbType == NoSqlDb_TargetType.MongoDb)
            {
                var fnDef = string.Empty;
                var deciaBaseFnctnNames = (UseCachedReferences) ? ((useMinimalMode) ? DeciaBaseUtils.Fnctn_Decia_All_Names_Minimal : DeciaBaseUtils.Fnctn_Decia_All_Names) : DeciaBaseUtils.Fnctn_Decia_All_Names_None;
                var rootCollectionsForChangeHandling = (UseCachedReferences) ? RootCollections_Ordered : new List<GenericCollection>();

                foreach (var baseFunctionName in deciaBaseFnctnNames)
                {
                    var baseFunctionDef = DeciaBase_Resources.ResourceManager.GetString(baseFunctionName);

                    if (baseFunctionName.EndsWith(DeciaBaseUtils.Fnctn_Decia_Format_Specifier))
                    { baseFunctionDef = baseFunctionDef.InsertPlaceholderValues(placeholderValues); }

                    fnDef += baseFunctionDef + Environment.NewLine + Environment.NewLine;
                }

                foreach (var rootCollection in rootCollectionsForChangeHandling)
                {
                    var changeHandler = new CollectionChangeHandler_Function(rootCollection);
                    var changeHandlerDef = changeHandler.ExportCollectionChangeHandler(dbType);

                    changeHandlerDef = changeHandlerDef.InsertPlaceholderValues(placeholderValues);

                    fnDef += changeHandlerDef + Environment.NewLine + Environment.NewLine;
                }

                fnDef += Environment.NewLine;
                return fnDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private Dictionary<string, string> GetPlaceholderValues()
        {
            var rootCollectionNames = RootCollections_Ordered.Select(x => x.XmlMapping.CollectionElement.Name).ToList().ConvertToCollectionAsString(",");
            var now = DateTime.UtcNow;
            var dateAsText = now.ToShortDateString() + ", " + now.ToShortTimeString() + " (UTC)";

            var placeholderValues = new Dictionary<string, string>();
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_MyDbName, this.Name_Escaped);
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_SysDbName, DeciaBaseUtils.SysDb_Value);
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_DateOfExportName, dateAsText);
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_RelevantCollectionNames, rootCollectionNames);
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_RelevantActionNames, "i,u,d");
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_MaxConnectionUtilization, ".5");
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_MaxQueueSize, "10");
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_MaxActiveSize, "50");
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_MaxCursorCount, "100");
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_MaxHandlingErrorCount, "10");
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_MaxCachingDepth, "2");
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_CollectionChangeHandlerName_Prefix, CollectionChangeHandler_Function.CollectionChangeHandler_Prefix);
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_CollectionChangeHandlerName_Postfix, CollectionChangeHandler_Function.CollectionChangeHandler_Postfix);
            return placeholderValues;
        }

        #endregion
    }
}