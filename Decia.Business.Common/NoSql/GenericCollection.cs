using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Exports;
using Decia.Business.Common.JsonSets;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.NoSql.Functions;
using Decia.Business.Common.NoSql.Indexes;
using Decia.Business.Common.NoSql.Validators;
using ITimeDimensionality = System.Collections.Generic.IDictionary<Decia.Business.Common.Time.TimeDimensionType, System.Collections.Generic.KeyValuePair<Decia.Business.Common.Time.TimeValueType, Decia.Business.Common.Time.TimePeriodType>>;
using TimeDimensionality = System.Collections.Generic.SortedDictionary<Decia.Business.Common.Time.TimeDimensionType, System.Collections.Generic.KeyValuePair<Decia.Business.Common.Time.TimeValueType, Decia.Business.Common.Time.TimePeriodType>>;

namespace Decia.Business.Common.NoSql
{
    public class GenericCollection : IDatabaseMember
    {
        public const bool Default_IsForInputs = true;
        public const bool Default_IsDimensionalSource = false;
        public static readonly int? Default_DimensionNumber = ModelObjectReference.DefaultDimensionNumber;
        public const string CollectionPrefix = "clct_";

        #region Members

        private CollectionMode m_CollectionMode;
        private GenericDatabase m_ParentDatabase;
        private GenericCollection m_ParentCollection;
        private GenericField m_ParentField;

        private Guid m_Id;
        private string m_Name;
        private ModelObjectReference m_StructuralTypeRef;

        private Guid? m_FieldId_ForId;
        private Guid? m_FieldId_ForName;
        private Guid? m_FieldId_ForSorting;

        private Dictionary<Guid, GenericField> m_Fields;
        private SortedDictionary<string, GenericField> m_FieldsByName;
        private SortedDictionary<int, GenericField> m_FieldsByOrder;

        private List<GenericIndex> m_Indexes;

        private ChangeTracking_Function m_ChangeTrackingFunction;
        private ChangePrevention_Validator m_ChangePreventionValidator;
        private Singleton_Validator m_SingletonValidator;

        private XmlCollectionMapping m_XmlMapping = null;

        #endregion

        #region Constructors

        public GenericCollection(GenericDatabase parentDatabase, string collectionName, ModelObjectReference structuralTypeRef)
            : this(parentDatabase, collectionName, structuralTypeRef, null)
        { }

        public GenericCollection(GenericDatabase parentDatabase, string collectionName, ModelObjectReference structuralTypeRef, GenericCollection parentCollection)
        {
            m_CollectionMode = (parentCollection == null) ? CollectionMode.Collection : CollectionMode.Array;
            m_ParentDatabase = parentDatabase;
            m_ParentCollection = parentCollection;

            m_Id = Guid.NewGuid();
            m_Name = collectionName;
            m_StructuralTypeRef = structuralTypeRef;

            m_FieldId_ForId = null;
            m_FieldId_ForName = null;
            m_FieldId_ForSorting = null;

            m_Fields = new Dictionary<Guid, GenericField>();
            m_FieldsByName = new SortedDictionary<string, GenericField>();
            m_FieldsByOrder = new SortedDictionary<int, GenericField>();

            m_Indexes = new List<GenericIndex>();
            m_ChangeTrackingFunction = new ChangeTracking_Function(this, (StructuralTypeRef.ModelObjectType == ModelObjectType.EntityType));
            m_SingletonValidator = null;

            if (ParentCollection != null)
            { m_ParentField = ParentCollection.CreateLocalField(this); }
        }

        #endregion

        #region Properties

        public CollectionMode CollectionMode { get { return m_CollectionMode; } }
        public GenericDatabase ParentDatabase { get { return m_ParentDatabase; } }
        public GenericCollection ParentCollection { get { return m_ParentCollection; } }
        public GenericField ParentField { get { return m_ParentField; } }

        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public string Name_Escaped { get { return Name.ToEscaped_VarName(); } }
        public string Name_Pluralized { get { return Name.ToPluralized(StructuralTypeRef.ModelObjectType == ModelObjectType.RelationType).ToEscaped_VarName(); } }
        public string TypeName_Escaped { get { return Name_Escaped.GetTypeName_FromName(); } }
        public string CollectionTypeName_Escaped { get { return Name_Escaped + "_Collection_Type"; } }
        public string Alias { get; set; }

        public ModelObjectReference StructuralTypeRef { get { return m_StructuralTypeRef; } }

        public Guid? FieldId_ForId { get { return m_FieldId_ForId; } }
        public Guid? FieldId_ForName { get { return m_FieldId_ForName; } }
        public Guid? FieldId_ForSorting { get { return m_FieldId_ForSorting; } }

        public GenericField Field_ForId { get { return (m_FieldId_ForId.HasValue) ? m_Fields[m_FieldId_ForId.Value] : null; } }
        public GenericField Field_ForName { get { return (m_FieldId_ForName.HasValue) ? m_Fields[m_FieldId_ForName.Value] : null; } }
        public GenericField Field_ForSorting { get { return (m_FieldId_ForSorting.HasValue) ? m_Fields[m_FieldId_ForSorting.Value] : null; } }

        public ICollection<GenericField> LocalFields
        {
            get
            {
                var fields = new ReadOnlyList<GenericField>(m_Fields.Values);
                fields.IsReadOnly = true;
                return fields;
            }
        }

        public ICollection<GenericField> LocalFieldsByOrder
        {
            get
            {
                var fields = new ReadOnlyList<GenericField>(m_FieldsByOrder.Values);
                fields.IsReadOnly = true;
                return fields;
            }
        }

        public IDictionary<GenericField, int> AllFieldsWithDepth
        {
            get
            {
                var allFields = new List<KeyValuePair<GenericField, int>>(m_Fields.Values.Select(x => new KeyValuePair<GenericField, int>(x, 0)));

                foreach (var nestedCollectionField in m_Fields.Values.Where(x => x.FieldMode == FieldMode.NestedCollection))
                {
                    var nestedCollection = nestedCollectionField.NestedCollection;
                    var nestedFields = nestedCollection.AllFieldsWithDepth;

                    nestedFields.ToDictionary(x => x.Key, x => (x.Value + 1));
                    allFields.AddRange(nestedFields);
                }

                var fields = new ReadOnlyDictionary<GenericField, int>(allFields.ToDictionary(x => x.Key, x => x.Value));
                fields.IsReadOnly = true;
                return fields;
            }
        }

        public ICollection<GenericField> AllFields
        {
            get
            {
                var fields = new ReadOnlyList<GenericField>(AllFieldsWithDepth.Keys);
                fields.IsReadOnly = true;
                return fields;
            }
        }

        public bool HasIndexes { get { return (m_Indexes.Count > 0); } }
        public ICollection<GenericIndex> Indexes
        {
            get
            {
                var indexes = new ReadOnlyList<GenericIndex>(m_Indexes);
                indexes.IsReadOnly = true;
                return indexes;
            }
        }

        public bool HasChangeTrackingFunction { get { return (m_ChangeTrackingFunction != null); } }
        public ChangeTracking_Function ChangeTrackingFunction { get { return m_ChangeTrackingFunction; } }

        public bool HasSingletonTrigger { get { return (m_SingletonValidator != null); } }
        public Singleton_Validator SingletonTrigger { get { return m_SingletonValidator; } }

        #endregion

        #region Field Methods

        public GenericField CreateLocalField(string fieldName, DeciaDataType dataType, ITimeDimensionality timeDimensionality)
        {
            PredefinedVariableTemplateOption? predefinedType = null;
            return CreateLocalField(fieldName, dataType, timeDimensionality, predefinedType);
        }

        public GenericField CreateLocalField(string fieldName, DeciaDataType dataType, ITimeDimensionality timeDimensionality, PredefinedVariableTemplateOption? predefinedType)
        {
            var field = new GenericField(this, fieldName, dataType, timeDimensionality, predefinedType);
            m_Fields.Add(field.Id, field);
            m_FieldsByName.Add(field.Name, field);
            m_FieldsByOrder.Add(m_FieldsByOrder.Count, field);

            if (field.PredefinedType == PredefinedVariableTemplateOption.Id)
            { this.SetLocalField_ForId(field.Id); }
            if (field.PredefinedType == PredefinedVariableTemplateOption.Name)
            { this.SetLocalField_ForName(field.Id); }
            if (field.PredefinedType == PredefinedVariableTemplateOption.Order)
            { this.SetLocalField_ForSorting(field.Id); }

            if (field.PredefinedType.HasValue)
            { this.AddIndex(field.Id); }

            return field;
        }

        public GenericField CreateLocalField(string fieldName, ITimeDimensionality timeDimensionality, GenericCollection cachedDocumentSource)
        {
            PredefinedVariableTemplateOption? predefinedType = null;
            int? alternateDimensionNumber = null;
            return CreateLocalField(fieldName, timeDimensionality, cachedDocumentSource, predefinedType, alternateDimensionNumber);
        }

        public GenericField CreateLocalField(string fieldName, DeciaDataType dataType, ITimeDimensionality timeDimensionality, GenericCollection cachedDocumentSource)
        {
            PredefinedVariableTemplateOption? predefinedType = null;
            int? alternateDimensionNumber = null;
            return CreateLocalField(fieldName, dataType, timeDimensionality, cachedDocumentSource, predefinedType, alternateDimensionNumber);
        }

        public GenericField CreateLocalField(string fieldName, ITimeDimensionality timeDimensionality, GenericCollection cachedDocumentSource, PredefinedVariableTemplateOption? predefinedType, int? alternateDimensionNumber)
        {
            var dataType = GenericField.Default_DataTypeForId;
            return CreateLocalField(fieldName, dataType, timeDimensionality, cachedDocumentSource, predefinedType, alternateDimensionNumber);
        }

        public GenericField CreateLocalField(string fieldName, DeciaDataType dataType, ITimeDimensionality timeDimensionality, GenericCollection cachedDocumentSource, PredefinedVariableTemplateOption? predefinedType, int? alternateDimensionNumber)
        {
            var field = new GenericField(this, fieldName, dataType, timeDimensionality, cachedDocumentSource, predefinedType, alternateDimensionNumber);
            m_Fields.Add(field.Id, field);
            m_FieldsByName.Add(field.Name, field);
            m_FieldsByOrder.Add(m_FieldsByOrder.Count, field);
            return field;
        }

        internal GenericField CreateLocalField(GenericField sourceField)
        {
            var field = new GenericField(this, sourceField);
            m_Fields.Add(field.Id, field);
            m_FieldsByName.Add(field.Name, field);
            m_FieldsByOrder.Add(m_FieldsByOrder.Count, field);
            return field;
        }

        public GenericField CreateLocalField(GenericCollection nestedCollection)
        {
            var field = new GenericField(this, nestedCollection);
            m_Fields.Add(field.Id, field);
            m_FieldsByName.Add(field.Name, field);
            m_FieldsByOrder.Add(m_FieldsByOrder.Count, field);
            return field;
        }

        public bool HasLocalField(Guid fieldId)
        {
            return (m_Fields.ContainsKey(fieldId));
        }

        public bool HasLocalField(ModelObjectReference variableTemplateRef)
        {
            return (m_Fields.Values.Where(x => x.VariableTemplateRef == variableTemplateRef).Count() > 0);
        }

        public GenericField GetLocalField(Guid fieldId)
        {
            if (!HasLocalField(fieldId))
            { return null; }

            return m_Fields[fieldId];
        }

        public GenericField GetLocalField(ModelObjectReference variableTemplateRef)
        {
            if (!HasLocalField(variableTemplateRef))
            { return null; }

            var fields = m_Fields.Values.Where(x => x.VariableTemplateRef == variableTemplateRef).ToList();
            return fields.FirstOrDefault();
        }

        public bool HasAnyField(Guid fieldId)
        {
            var matchingField = GetAnyField(fieldId);
            return (matchingField != null);
        }

        public bool HasAnyField(ModelObjectReference variableTemplateRef)
        {
            var matchingField = GetAnyField(variableTemplateRef);
            return (matchingField != null);
        }

        public GenericField GetAnyField(Guid fieldId)
        {
            var allFields = AllFields.ToDictionary(x => x.Id);

            if (!allFields.ContainsKey(fieldId))
            { return null; }

            return allFields[fieldId];
        }

        public GenericField GetAnyField(ModelObjectReference variableTemplateRef)
        {
            var allFields = AllFieldsWithDepth;
            var matchingFields = allFields.Where(x => x.Key.VariableTemplateRef == variableTemplateRef).ToDictionary(x => x.Key, x => x.Value);

            if (matchingFields.Count <= 0)
            { return null; }

            var minDepth = matchingFields.Values.Min();
            var fields = matchingFields.Where(x => x.Value == minDepth).ToList();

            var firstPair = fields.FirstOrDefault();
            return firstPair.Key;
        }

        public void SetLocalField_ForId(Guid fieldId)
        {
            if (!HasLocalField(fieldId))
            { throw new InvalidOperationException("The specified Field does not exist in the current Collection."); }

            m_FieldId_ForId = fieldId;
        }

        public void SetLocalField_ForName(Guid fieldId)
        {
            if (!HasLocalField(fieldId))
            { throw new InvalidOperationException("The specified Field does not exist in the current Collection."); }

            m_FieldId_ForName = fieldId;
        }

        public void SetLocalField_ForSorting(Guid fieldId)
        {
            if (!HasLocalField(fieldId))
            { throw new InvalidOperationException("The specified Field does not exist in the current Collection."); }

            m_FieldId_ForSorting = fieldId;
        }

        #endregion

        #region Constraint Methods

        public GenericIndex AddIndex(Guid fieldId)
        {
            return AddIndex(new Guid[] { fieldId });
        }

        public GenericIndex AddIndex(IEnumerable<Guid> fieldIds)
        {
            var allFieldsById = AllFields.ToDictionary(x => x.Id, x => x);
            var fields = fieldIds.Select(x => allFieldsById[x]).ToList();

            return AddIndex(fields);
        }

        public GenericIndex AddIndex(GenericField field)
        {
            return AddIndex(new GenericField[] { field });
        }

        public GenericIndex AddIndex(IEnumerable<GenericField> fields)
        {
            var index = new GenericIndex(this, fields);
            m_Indexes.Add(index);
            return index;
        }

        #endregion

        #region Export XML Schema Methods

        public XmlCollectionMapping XmlMapping { get { return m_XmlMapping; } }

        public void ExportXmlSchema(XmlDatabaseMapping dbMapping, XmlCollectionMapping collectionMapping)
        {
            var fieldsByMode = this.GetFieldsByMode();

            foreach (var fieldModeBucket in fieldsByMode)
            {
                var fieldsForMode = fieldModeBucket.Value;

                foreach (var field in fieldsForMode)
                {
                    var fieldElem = field.ExportXmlSchema(dbMapping, collectionMapping);
                    collectionMapping.DocumentType_FieldSequence.Items.Add(fieldElem);
                }
            }

            m_XmlMapping = collectionMapping;
        }

        #endregion

        #region Export Database Methods

        public string ExportDatabaseSchema(NoSqlDb_TargetType dbType)
        {
            var collectionVarName = XmlMapping.CollectionElement.Name;
            var collectionSubTree = this.GetCollectionSubTree(true);
            var containedDocTypes = collectionSubTree.Select(x => x.XmlMapping.DocumentType).ToList();

            if (Char.IsNumber(collectionVarName.FirstOrDefault()))
            { collectionVarName = (CollectionPrefix + collectionVarName); }

            if (dbType == NoSqlDb_TargetType.MongoDb)
            {
                var collectionMapping = ParentDatabase.Exported_JsonSchema.CollectionMappings[this];
                var documentType = collectionMapping.Key;
                var documentSchema = documentType.GenerateSchema();

                var docSchemaHeader = string.Empty;
                var docSchemaText = string.Empty;

                if (ParentDatabase.UseCachedReferences)
                {
                    docSchemaHeader = "DOCUMENT SCHEMA:";
                    docSchemaText = documentSchema.GetSchemaAsCode_ForInternalChangeMgmt();
                }
                else
                {
                    docSchemaHeader = "DOCUMENT SCHEMA FOR MONGOOSE ODM:";
                    docSchemaText = documentSchema.GetSchemaAsCode_ForMongoose();
                }

                var headerText = string.Empty;
                headerText += "/*" + Environment.NewLine;
                headerText += docSchemaHeader + Environment.NewLine;
                headerText += docSchemaText + Environment.NewLine;
                headerText += "*/" + Environment.NewLine;

                var arrayFields = this.LocalFields.Where(x => x.XmlMapping.HasArrayItemElement).ToList();
                var validatorText = string.Empty;
                if (arrayFields.Count > 0)
                {
                    foreach (var arrayField in arrayFields)
                    {
                        if (!string.IsNullOrWhiteSpace(validatorText))
                        { validatorText += ", "; }

                        validatorText += "{ " + arrayField.XmlMapping.RootFieldElement.Name + ": { $exists: true } }";
                    }
                    validatorText = ", { validator: { $and: [ " + validatorText + "  ] } }";
                }

                var collectionDefText = string.Empty;
                collectionDefText += string.Format("MyDb.createCollection(\"{0}\"{1});", XmlMapping.CollectionElement.Name, validatorText) + Environment.NewLine;
                collectionDefText += string.Format("var {0} = MyDb.getCollection(\"{1}\");", collectionVarName, XmlMapping.CollectionElement.Name) + Environment.NewLine;

                var footerText = string.Empty;
                foreach (var collection in collectionSubTree)
                {
                    var fieldsByMode = collection.GetFieldsByMode();

                    foreach (var fieldModeBucket in fieldsByMode)
                    {
                        var fieldsForMode = fieldModeBucket.Value;

                        foreach (var field in fieldsForMode)
                        {
                            var indexDefinitions = field.ExportCollectionIndexes(dbType, collectionVarName);

                            foreach (var indexDefinition in indexDefinitions)
                            { footerText += indexDefinition + Environment.NewLine; }
                        }
                    }
                }

                var collectionDef = headerText + collectionDefText + footerText;
                return collectionDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}