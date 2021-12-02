using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using ITimeDimensionality = System.Collections.Generic.IDictionary<Decia.Business.Common.Time.TimeDimensionType, System.Collections.Generic.KeyValuePair<Decia.Business.Common.Time.TimeValueType, Decia.Business.Common.Time.TimePeriodType>>;
using TimeDimensionality = System.Collections.Generic.SortedDictionary<Decia.Business.Common.Time.TimeDimensionType, System.Collections.Generic.KeyValuePair<Decia.Business.Common.Time.TimeValueType, Decia.Business.Common.Time.TimePeriodType>>;

namespace Decia.Business.Common.NoSql
{
    public class GenericField : IDatabaseMember
    {
        public static readonly FieldMode[] FieldModes_RequireIndexed = new FieldMode[] { FieldMode.CachedDocument, FieldMode.ReplicatedList };
        public static readonly DeciaDataType[] DataTypes_DefaultToIndexed = new DeciaDataType[] { DeciaDataType.Boolean, DeciaDataType.DateTime, DeciaDataType.Integer, DeciaDataType.UniqueID };

        public const DeciaDataType Default_DataTypeForId = DeciaDataType.UniqueID;
        public const string Required_IdName = "_id";
        public const string SingleIndexFormat = "{0}.createIndex({{ \"{1}\": {2} }});";
        public const string MultiIndexItemFormat = "\"{0}\": {1}";
        public const string MultiIndexRootFormat = "{0}.createIndex({{ {1} }});";

        #region Members

        private FieldMode m_FieldMode;
        private GenericCollection m_ParentCollection;
        private GenericCollection m_NestedCollection;
        private GenericCollection m_CachedDocumentSource;
        private GenericField m_SourceField;

        private Guid m_Id;
        private string m_Name;
        private PredefinedVariableTemplateOption? m_PredefinedType;
        private int? m_AlternateDimensionNumber;
        private DeciaDataType? m_DataType;
        private TimeDimensionality m_TimeDimensionality;
        private bool? m_IncludeIndex = null;

        private List<Guid> m_FieldIdsToCombine;
        private string m_DefaultValue;
        private ModelObjectReference? m_VariableTemplateRef;
        private ModelObjectReference? m_RelatedStructuralTypeRef;
        private bool m_IsForNavigation;

        private bool m_IsCachingSet = false;
        private LocalCacheMode m_LocalCacheMode = LocalCacheMode.IdOnly;
        private ForeignCacheMode m_ForeignCacheMode = ForeignCacheMode.None;
        private int? m_MaxCachingDepth = null;
        private CacheInvalidationMode m_ForeignInvalidationMode = CacheInvalidationMode.None;
        private object m_ForeignInvalidationValue = null;

        private XmlFieldMapping m_XmlMapping = null;

        #endregion

        #region Constructors

        internal GenericField(GenericCollection parentCollection, string fieldName, DeciaDataType dataType, ITimeDimensionality timeDimensionality)
            : this(parentCollection, fieldName, dataType, timeDimensionality, null)
        { }

        internal GenericField(GenericCollection parentCollection, string fieldName, DeciaDataType dataType, ITimeDimensionality timeDimensionality, PredefinedVariableTemplateOption? predefinedType)
        {
            timeDimensionality = (timeDimensionality != null) ? timeDimensionality : new TimeDimensionality();

            m_FieldMode = FieldMode.Data;
            m_ParentCollection = parentCollection;
            m_NestedCollection = null;
            m_CachedDocumentSource = null;
            m_SourceField = null;

            m_Id = Guid.NewGuid();
            m_Name = fieldName;
            m_PredefinedType = predefinedType;
            m_AlternateDimensionNumber = null;
            m_DataType = dataType;
            m_TimeDimensionality = new TimeDimensionality(timeDimensionality);

            m_FieldIdsToCombine = null;
            m_DefaultValue = null;
            m_VariableTemplateRef = null;
            m_RelatedStructuralTypeRef = null;
            m_IsForNavigation = false;
        }

        internal GenericField(GenericCollection parentCollection, string fieldName, ITimeDimensionality timeDimensionality, GenericCollection cachedDocumentSource)
            : this(parentCollection, fieldName, timeDimensionality, cachedDocumentSource, null, null)
        { }

        internal GenericField(GenericCollection parentCollection, string fieldName, ITimeDimensionality timeDimensionality, GenericCollection cachedDocumentSource, PredefinedVariableTemplateOption? predefinedType, int? alternateDimensionNumber)
            : this(parentCollection, fieldName, Default_DataTypeForId, timeDimensionality, cachedDocumentSource, predefinedType, alternateDimensionNumber)
        { }

        internal GenericField(GenericCollection parentCollection, string fieldName, DeciaDataType dataType, ITimeDimensionality timeDimensionality, GenericCollection cachedDocumentSource, PredefinedVariableTemplateOption? predefinedType, int? alternateDimensionNumber)
        {
            timeDimensionality = (timeDimensionality != null) ? timeDimensionality : new TimeDimensionality();

            m_FieldMode = FieldMode.CachedDocument;
            m_ParentCollection = parentCollection;
            m_NestedCollection = null;
            m_CachedDocumentSource = cachedDocumentSource;
            m_SourceField = null;

            m_Id = Guid.NewGuid();
            m_Name = fieldName;
            m_PredefinedType = predefinedType;
            m_AlternateDimensionNumber = alternateDimensionNumber;
            m_DataType = dataType;
            m_TimeDimensionality = new TimeDimensionality(timeDimensionality);

            m_FieldIdsToCombine = null;
            m_DefaultValue = null;
            m_VariableTemplateRef = null;
            m_RelatedStructuralTypeRef = null;
            m_IsForNavigation = false;
        }

        internal GenericField(GenericCollection parentCollection, GenericField sourceField)
        {
            if (sourceField.FieldMode != FieldMode.CachedDocument)
            { throw new InvalidOperationException("Only cached Documents can be replicated."); }

            var timeDimensionality = new TimeDimensionality();

            m_FieldMode = FieldMode.ReplicatedList;
            m_ParentCollection = parentCollection;
            m_NestedCollection = null;
            m_CachedDocumentSource = sourceField.ParentCollection;
            m_SourceField = sourceField;

            m_Id = Guid.NewGuid();
            m_Name = sourceField.ParentCollection.Name_Pluralized + "_from_" + ((sourceField.MemberType == MemberType.Array) ? sourceField.Name_Pluralized : sourceField.Name_Escaped);
            m_PredefinedType = SourceField.PredefinedType;
            m_AlternateDimensionNumber = SourceField.AlternateDimensionNumber;
            m_DataType = SourceField.DataType;
            m_TimeDimensionality = timeDimensionality;

            m_FieldIdsToCombine = null;
            m_DefaultValue = null;
            m_VariableTemplateRef = null;
            m_RelatedStructuralTypeRef = null;
            m_IsForNavigation = false;

            if (!sourceField.IsCachingSet)
            { sourceField.SetCaching(sourceField.LocalCacheMode, sourceField.ForeignCacheMode, sourceField.MaxCachingDepth, sourceField.ForeignInvalidationMode, sourceField.ForeignInvalidationValue); }

            m_IsCachingSet = sourceField.IsCachingSet;
            m_LocalCacheMode = sourceField.LocalCacheMode;
            m_ForeignCacheMode = sourceField.ForeignCacheMode;
            m_MaxCachingDepth = sourceField.MaxCachingDepth;
            m_ForeignInvalidationMode = sourceField.ForeignInvalidationMode;
            m_ForeignInvalidationValue = sourceField.ForeignInvalidationValue;
        }

        internal GenericField(GenericCollection parentCollection, GenericCollection nestedCollection)
        {
            m_FieldMode = FieldMode.NestedCollection;
            m_ParentCollection = parentCollection;
            m_NestedCollection = nestedCollection;
            m_CachedDocumentSource = null;
            m_SourceField = null;

            m_Id = Guid.NewGuid();
            m_Name = nestedCollection.Name;
            m_PredefinedType = null;
            m_AlternateDimensionNumber = null;
            m_DataType = null;
            m_TimeDimensionality = new TimeDimensionality();

            m_FieldIdsToCombine = null;
            m_DefaultValue = null;
            m_VariableTemplateRef = null;
            m_RelatedStructuralTypeRef = null;
            m_IsForNavigation = false;
        }

        #endregion

        #region Properties

        public FieldMode FieldMode { get { return m_FieldMode; } }
        public GenericCollection ParentCollection { get { return m_ParentCollection; } }
        public GenericCollection NestedCollection { get { return m_NestedCollection; } }
        public GenericCollection CachedDocumentSource { get { return m_CachedDocumentSource; } }
        public GenericField SourceField { get { return m_SourceField; } }

        public bool IsDocumentId { get { return (PredefinedType == PredefinedVariableTemplateOption.Id); } }

        public Guid Id { get { return m_Id; } }
        public string Name { get { return (!IsDocumentId) ? m_Name : Required_IdName; } }
        public MemberType MemberType { get { return ((FieldMode == FieldMode.ReplicatedList) || (TimeDimensionality.Count > 0)) ? MemberType.Array : MemberType.Field; } }
        public string Name_Escaped { get { return Name.ToEscaped_VarName(); } }
        public string Name_Pluralized { get { return Name.ToPluralized(false).ToEscaped_VarName(); } }
        public PredefinedVariableTemplateOption? PredefinedType { get { return m_PredefinedType; } }
        public int? AlternateDimensionNumber { get { return m_AlternateDimensionNumber; } }
        public DeciaDataType DataType { get { return m_DataType.HasValue ? m_DataType.Value : DeciaDataTypeUtils.InvalidDataType; } }
        public XmlQualifiedName XmlSimpleTypeName { get { return DataType.GetXmlSimpleTypeQualdName(); } }
        public XmlSchemaType XmlSimpleType { get { return DataType.ToXmlSimpleType(); } }
        public ITimeDimensionality TimeDimensionality { get { return m_TimeDimensionality.ToDictionary(x => x.Key, x => x.Value); } }

        #region Include Index

        public bool? IncludeIndex
        {
            get
            {
                if (PredefinedType.HasValue)
                { return true; }
                if (FieldModes_RequireIndexed.Contains(FieldMode))
                { return true; }

                if (m_IncludeIndex.HasValue)
                { return m_IncludeIndex.Value; }

                return DataTypes_DefaultToIndexed.Contains(DataType);
            }
            set
            {
                if (PredefinedType.HasValue)
                { throw new InvalidOperationException("The Field requires Indexing."); }
                if (FieldModes_RequireIndexed.Contains(FieldMode))
                { throw new InvalidOperationException("The Field requires Indexing."); }

                m_IncludeIndex = value;
            }
        }

        #endregion

        #region Fields To Combine

        public ICollection<Guid> FieldIdsToCombine
        {
            get
            {
                var fieldIds = new ReadOnlyList<Guid>(m_FieldIdsToCombine);
                fieldIds.IsReadOnly = true;
                return fieldIds;
            }
        }

        public ICollection<GenericField> FieldsToCombine
        {
            get
            {
                var fields = new ReadOnlyList<GenericField>(m_FieldIdsToCombine.Select(x => ParentCollection.GetLocalField(x)));
                fields.IsReadOnly = true;
                return fields;
            }
        }

        public void SetFieldIdsToCombine(IEnumerable<Guid> fieldIds)
        {
            if (fieldIds.Count() < 1)
            { throw new InvalidOperationException("Cannot set fields to combine to empty set."); }
            if (fieldIds.Count() != fieldIds.ToHashSet().Count)
            { throw new InvalidOperationException("Cannot set fields to combine to duplicated fields."); }

            foreach (var fieldId in fieldIds)
            {
                if (!ParentCollection.HasLocalField(fieldId))
                { throw new InvalidOperationException("Cannot set fields to combine to non-local fields."); }
            }

            m_FieldIdsToCombine = new List<Guid>(fieldIds);
            ParentCollection.AddIndex(m_FieldIdsToCombine.ToList());
        }

        #endregion

        #region Default Value

        public string DefaultValue
        {
            get { return m_DefaultValue; }
            set
            {
                if (IsDocumentId)
                { throw new InvalidOperationException("Cannot set default for auto-ID."); }

                m_DefaultValue = value;
            }
        }

        #endregion

        #region Variable Info

        public ModelObjectReference LocalStructuralTypeRef { get { return ParentCollection.StructuralTypeRef; } }
        public bool IsVariableInfoSet { get { return m_VariableTemplateRef.HasValue; } }

        public ModelObjectReference? VariableTemplateRef { get { return m_VariableTemplateRef; } }
        public ModelObjectReference? RelatedStructuralTypeRef { get { return m_RelatedStructuralTypeRef; } }
        public bool IsForNavigation { get { return m_IsForNavigation; } }

        public GenericField SetVariableInfo(ModelObjectReference variableTemplateRef)
        {
            return SetVariableInfo(variableTemplateRef, null);
        }

        public GenericField SetVariableInfo(ModelObjectReference variableTemplateRef, ModelObjectReference? relatedStructuralTypeRef)
        {
            return SetVariableInfo(variableTemplateRef, relatedStructuralTypeRef, false);
        }

        public GenericField SetVariableInfo(ModelObjectReference variableTemplateRef, ModelObjectReference? relatedStructuralTypeRef, bool isForNavigation)
        {
            if (m_VariableTemplateRef.HasValue)
            { throw new InvalidOperationException("The VariableTemplateRef has already been set."); }

            m_VariableTemplateRef = variableTemplateRef;
            m_RelatedStructuralTypeRef = relatedStructuralTypeRef;
            m_IsForNavigation = isForNavigation;

            return this;
        }

        #endregion

        #region Cache Mode

        public void SetCaching(LocalCacheMode localCacheMode, ForeignCacheMode foreignCacheMode, int? maxCachingDepth, CacheInvalidationMode foreignInvalidationMode, object foreignInvalidationValue)
        {
            if (IsCachingSet)
            { throw new InvalidOperationException("The Caching behavior can only be set once."); }
            if (FieldMode != FieldMode.CachedDocument)
            { throw new InvalidOperationException("The cache mode can only be set for Cached Documents."); }

            m_IsCachingSet = true;
            m_LocalCacheMode = localCacheMode;
            m_ForeignCacheMode = foreignCacheMode;
            m_MaxCachingDepth = maxCachingDepth;
            m_ForeignInvalidationMode = foreignInvalidationMode;
            m_ForeignInvalidationValue = foreignInvalidationValue;

            if (ForeignCacheMode != ForeignCacheMode.None)
            {
                CachedDocumentSource.CreateLocalField(this);
            }
        }

        public bool IsCachingSet { get { return m_IsCachingSet; } }
        public LocalCacheMode LocalCacheMode { get { return m_LocalCacheMode; } }
        public ForeignCacheMode ForeignCacheMode { get { return m_ForeignCacheMode; } }
        public int? MaxCachingDepth { get { return m_MaxCachingDepth; } }
        public CacheInvalidationMode ForeignInvalidationMode { get { return m_ForeignInvalidationMode; } }
        public object ForeignInvalidationValue { get { return m_ForeignInvalidationValue; } }

        #endregion

        #endregion

        #region Export XML Schema Methods

        public XmlFieldMapping XmlMapping { get { return m_XmlMapping; } }

        public XmlSchemaElement ExportXmlSchema(XmlDatabaseMapping dbMapping, XmlCollectionMapping parentCollectionMapping)
        {
            m_XmlMapping = new XmlFieldMapping(this, dbMapping, parentCollectionMapping);
            return m_XmlMapping.RootFieldElement;
        }

        #endregion

        #region Export Database Methods

        public IEnumerable<string> ExportCollectionIndexes(NoSqlDb_TargetType dbType, string collectionVarName)
        {
            var indexDefs = new List<string>();

            string collectionVar, collectionPath;
            var collectionPathToRoot = ParentCollection.GetCollectionPathToRoot(true, out collectionVar, out collectionPath);

            if (dbType == NoSqlDb_TargetType.MongoDb)
            {
                if ((IncludeIndex == true) && (FieldMode != FieldMode.ReplicatedList))
                {
                    var valueName = (XmlMapping.ValueElement != null) ? (XmlMapping.RootFieldElement.Name + "." + XmlMapping.ValueElement.Name) : XmlMapping.RootFieldElement.Name;
                    var valuePath = (collectionPath + valueName);

                    var indexDef = string.Format(SingleIndexFormat, collectionVarName, valuePath, "1");
                    indexDefs.Add(indexDef);
                }

                if (XmlMapping.KeyElements.Count > 0)
                {
                    var keyPathsForMultiIndex = new List<string>();
                    foreach (var keyElement in XmlMapping.KeyElements.Values)
                    {
                        var keyName = (XmlMapping.RootFieldElement.Name + "." + keyElement.Name);
                        var keyPath = (collectionPath + keyName);

                        var indexDef = string.Format(SingleIndexFormat, collectionVarName, keyPath, "1");
                        indexDefs.Add(indexDef);

                        if (keyElement.Name != Required_IdName)
                        { keyPathsForMultiIndex.Add(keyPath); }
                    }

                    if (XmlMapping.KeyElements.Count > 1)
                    {
                        var fullIndexDef = string.Empty;
                        foreach (var keyPath in keyPathsForMultiIndex)
                        {
                            var fullIndexPart = string.Format(MultiIndexItemFormat, keyPath, "1");

                            if (string.IsNullOrWhiteSpace(fullIndexDef))
                            { fullIndexDef = fullIndexPart; }
                            else
                            { fullIndexDef += ", " + fullIndexPart; }
                        }
                        fullIndexDef = string.Format(MultiIndexRootFormat, collectionVarName, fullIndexDef);
                        indexDefs.Add(fullIndexDef);
                    }
                }

                return indexDefs;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}