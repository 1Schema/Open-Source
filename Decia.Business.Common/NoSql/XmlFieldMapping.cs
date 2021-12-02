using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.NoSql
{
    public class XmlFieldMapping
    {
        #region Members

        private GenericField m_Field;
        private XmlDatabaseMapping m_DbMapping;
        private XmlCollectionMapping m_ParentCollectionMapping;

        private FieldMappingType m_FieldMappingType;
        private XmlSchemaElement m_RootFieldElement;
        private XmlSchemaElement m_ArrayItemElement;
        private Dictionary<ModelObjectReference, XmlSchemaElement> m_KeyElements;
        private XmlSchemaElement m_ValueElement;
        private XmlSchemaElement m_CacheElement;

        #endregion

        #region Constructors

        public XmlFieldMapping(GenericField field, XmlDatabaseMapping dbMapping, XmlCollectionMapping parentCollectionMapping)
        {
            if (field == null)
            { throw new InvalidOperationException("The Field is null."); }
            if (dbMapping == null)
            { throw new InvalidOperationException("The Database Mapping is null."); }
            if (field.ParentCollection.ParentDatabase != dbMapping.Database)
            { throw new InvalidOperationException("The Database does not match."); }
            if (parentCollectionMapping == null)
            { throw new InvalidOperationException("The parent Collection Mapping cannot be null."); }
            if (field.ParentCollection.ParentDatabase != parentCollectionMapping.Collection.ParentDatabase)
            { throw new InvalidOperationException("The Database does not match."); }
            if (field.ParentCollection != parentCollectionMapping.Collection)
            { throw new InvalidOperationException("The parent Collection does not match."); }
            if (dbMapping != parentCollectionMapping.DbMapping)
            { throw new InvalidOperationException("The Database does not match."); }

            m_Field = field;
            m_DbMapping = dbMapping;
            m_ParentCollectionMapping = parentCollectionMapping;

            m_DbMapping.AddFieldMapping(this);

            m_FieldMappingType = (FieldMappingType)(-1);
            m_RootFieldElement = null;
            m_ArrayItemElement = null;
            m_KeyElements = new Dictionary<ModelObjectReference, XmlSchemaElement>(ModelObjectReference.DimensionalComparer);
            m_ValueElement = null;
            m_CacheElement = null;

            if (Field.FieldMode == FieldMode.NestedCollection)
            {
                var nestedCollectionMapping = dbMapping.GetCollectionMapping(Field.NestedCollection);
                Field.NestedCollection.ExportXmlSchema(dbMapping, nestedCollectionMapping);

                m_FieldMappingType = FieldMappingType.PassThrough;
                m_RootFieldElement = nestedCollectionMapping.CollectionElement;

                return;
            }
            else if (Field.FieldMode == FieldMode.Data)
            {
                Create_Data_Elements();
                return;
            }
            else if (Field.FieldMode == FieldMode.CachedDocument)
            {
                Create_Referenced_Elements();
                return;
            }
            else if (Field.FieldMode == FieldMode.ReplicatedList)
            {
                Create_Replica_Elements();
                return;
            }
            else
            { throw new InvalidOperationException("The specified FieldMode is not supported."); }
        }

        private void Create_Data_Elements()
        {
            var referencedCollection = Field.CachedDocumentSource;
            var referencedCollectionMapping = (referencedCollection != null) ? DbMapping.GetCollectionMapping(referencedCollection) : null;

            m_RootFieldElement = new XmlSchemaElement();
            m_RootFieldElement.Name = (Field.TimeDimensionality.Count > 0) ? Field.Name_Pluralized : Field.Name_Escaped;
            m_RootFieldElement.MinOccurs = 1;
            m_RootFieldElement.MaxOccurs = 1;

            if (Field.TimeDimensionality.Count < 1)
            {
                m_FieldMappingType = FieldMappingType.Single_Simple;
                m_RootFieldElement.SchemaTypeName = Field.XmlSimpleTypeName;
                if (!ParentDatabase.UseCachedReferences && (referencedCollectionMapping != null))
                { m_RootFieldElement.RefName = new XmlQualifiedName(referencedCollectionMapping.DocumentType.Name); }
                m_RootFieldElement.IsNillable = (!Field.IsDocumentId);
                return;
            }

            var rootFieldItemSequence = new XmlSchemaSequence();
            var rootFieldType = new XmlSchemaComplexType();
            rootFieldType.Particle = rootFieldItemSequence;

            m_FieldMappingType = FieldMappingType.Multiple_Complex;
            m_RootFieldElement.SchemaType = rootFieldType;

            var itemValueSequence = new XmlSchemaSequence();
            var itemType = new XmlSchemaComplexType();
            itemType.Particle = itemValueSequence;

            m_ArrayItemElement = new XmlSchemaElement();
            m_ArrayItemElement.Name = XmlMappingUtils.Item_Name;
            m_ArrayItemElement.SchemaType = itemType;
            m_ArrayItemElement.MinOccurs = 0;
            m_ArrayItemElement.MaxOccursString = XmlMappingUtils.MaxOccurs_Unbounded;
            rootFieldItemSequence.Items.Add(m_ArrayItemElement);

            if (Field.TimeDimensionality.Count >= 1)
            {
                var tp1IdElement = new XmlSchemaElement();
                tp1IdElement.Name = XmlMappingUtils.TpId_Name_D1;
                tp1IdElement.SchemaTypeName = GenericDatabaseUtils.UniqueId_XmlSimpleTypeName;
                tp1IdElement.IsNillable = false;
                tp1IdElement.MinOccurs = 1;
                tp1IdElement.MaxOccurs = 1;
                itemValueSequence.Items.Add(tp1IdElement);

                m_KeyElements.Add(XmlMappingUtils.TpId_Key_D1, tp1IdElement);
            }

            if (Field.TimeDimensionality.Count >= 2)
            {
                var tp2IdElement = new XmlSchemaElement();
                tp2IdElement.Name = XmlMappingUtils.TpId_Name_D2;
                tp2IdElement.SchemaTypeName = GenericDatabaseUtils.UniqueId_XmlSimpleTypeName;
                tp2IdElement.IsNillable = false;
                tp2IdElement.MinOccurs = 1;
                tp2IdElement.MaxOccurs = 1;
                itemValueSequence.Items.Add(tp2IdElement);

                m_KeyElements.Add(XmlMappingUtils.TpId_Key_D2, tp2IdElement);
            }

            m_ValueElement = new XmlSchemaElement();
            m_ValueElement.Name = XmlMappingUtils.Value_Name;
            m_ValueElement.SchemaTypeName = Field.XmlSimpleTypeName;
            if (!ParentDatabase.UseCachedReferences && (referencedCollectionMapping != null))
            { m_ValueElement.RefName = new XmlQualifiedName(referencedCollectionMapping.DocumentType.Name); }
            m_ValueElement.IsNillable = (!Field.IsDocumentId);
            m_ValueElement.MinOccurs = 1;
            m_ValueElement.MaxOccurs = 1;
            itemValueSequence.Items.Add(m_ValueElement);
        }

        private void Create_Referenced_Elements()
        {
            if (!ParentDatabase.UseCachedReferences)
            {
                Create_Data_Elements();
                return;
            }

            var referencedCollection = Field.CachedDocumentSource;
            var referencedStructuralType = referencedCollection.StructuralTypeRef;
            var referencedCollectionMapping = DbMapping.GetCollectionMapping(referencedCollection);

            if (referencedStructuralType.ModelObjectType != ModelObjectType.EntityType)
            { throw new InvalidOperationException("The Structural Type cannot be cached."); }

            var itemValueSequence = (XmlSchemaSequence)null;

            var rootFieldItemSequence = new XmlSchemaSequence();
            var rootFieldType = new XmlSchemaComplexType();
            rootFieldType.Particle = rootFieldItemSequence;

            m_RootFieldElement = new XmlSchemaElement();
            m_RootFieldElement.Name = (Field.TimeDimensionality.Count > 0) ? Field.Name_Pluralized : Field.Name_Escaped;
            m_RootFieldElement.SchemaType = rootFieldType;
            m_RootFieldElement.MinOccurs = 1;
            m_RootFieldElement.MaxOccurs = 1;

            if (Field.TimeDimensionality.Count < 1)
            {
                m_FieldMappingType = FieldMappingType.Single_Complex;

                itemValueSequence = rootFieldItemSequence;
            }
            else
            {
                m_FieldMappingType = FieldMappingType.Multiple_Complex;

                itemValueSequence = new XmlSchemaSequence();
                var itemType = new XmlSchemaComplexType();
                itemType.Particle = itemValueSequence;

                m_ArrayItemElement = new XmlSchemaElement();
                m_ArrayItemElement.Name = XmlMappingUtils.Item_Name;
                m_ArrayItemElement.SchemaType = itemType;
                m_ArrayItemElement.MinOccurs = 0;
                m_ArrayItemElement.MaxOccursString = XmlMappingUtils.MaxOccurs_Unbounded;
                rootFieldItemSequence.Items.Add(m_ArrayItemElement);
            }

            if (Field.TimeDimensionality.Count >= 1)
            {
                var tp1IdElement = new XmlSchemaElement();
                tp1IdElement.Name = XmlMappingUtils.TpId_Name_D1;
                tp1IdElement.SchemaTypeName = GenericDatabaseUtils.UniqueId_XmlSimpleTypeName;
                tp1IdElement.IsNillable = false;
                tp1IdElement.MinOccurs = 1;
                tp1IdElement.MaxOccurs = 1;
                itemValueSequence.Items.Add(tp1IdElement);

                m_KeyElements.Add(XmlMappingUtils.TpId_Key_D1, tp1IdElement);
            }

            if (Field.TimeDimensionality.Count >= 2)
            {
                var tp2IdElement = new XmlSchemaElement();
                tp2IdElement.Name = XmlMappingUtils.TpId_Name_D2;
                tp2IdElement.SchemaTypeName = GenericDatabaseUtils.UniqueId_XmlSimpleTypeName;
                tp2IdElement.IsNillable = false;
                tp2IdElement.MinOccurs = 1;
                tp2IdElement.MaxOccurs = 1;
                itemValueSequence.Items.Add(tp2IdElement);

                m_KeyElements.Add(XmlMappingUtils.TpId_Key_D2, tp2IdElement);
            }

            m_ValueElement = new XmlSchemaElement();
            m_ValueElement.Name = XmlMappingUtils.DocId_Name;
            m_ValueElement.SchemaTypeName = GenericDatabaseUtils.UniqueId_XmlSimpleTypeName;
            m_ValueElement.IsNillable = true;
            m_ValueElement.MinOccurs = 1;
            m_ValueElement.MaxOccurs = 1;
            itemValueSequence.Items.Add(m_ValueElement);

            if (Field.LocalCacheMode != LocalCacheMode.IdOnly)
            {
                m_CacheElement = new XmlSchemaElement();
                m_CacheElement.Name = XmlMappingUtils.CachedDoc_Name;
                m_CacheElement.SchemaTypeName = new XmlQualifiedName(referencedCollectionMapping.DocumentType.Name);
                m_CacheElement.IsNillable = true;
                m_CacheElement.MinOccurs = 1;
                m_CacheElement.MaxOccurs = 1;
                itemValueSequence.Items.Add(m_CacheElement);
            }
        }

        private void Create_Replica_Elements()
        {
            if (Field.TimeDimensionality.Count > 0)
            { throw new InvalidOperationException("Replicated Fields should not have Time Dimensionality."); }

            var referencedCollection = Field.CachedDocumentSource;
            var referencedStructuralType = referencedCollection.StructuralTypeRef;
            var referencedCollectionMapping = DbMapping.GetCollectionMapping(referencedCollection);

            m_FieldMappingType = FieldMappingType.Multiple_Complex;

            var rootFieldItemSequence = new XmlSchemaSequence();
            var rootFieldType = new XmlSchemaComplexType();
            rootFieldType.Particle = rootFieldItemSequence;

            m_RootFieldElement = new XmlSchemaElement();
            m_RootFieldElement.Name = (Field.TimeDimensionality.Count > 0) ? Field.Name_Pluralized : Field.Name_Escaped;
            m_RootFieldElement.SchemaType = rootFieldType;
            m_RootFieldElement.MinOccurs = 1;
            m_RootFieldElement.MaxOccurs = 1;

            var itemValueSequence = new XmlSchemaSequence();
            var itemType = new XmlSchemaComplexType();
            itemType.Particle = itemValueSequence;

            m_ArrayItemElement = new XmlSchemaElement();
            m_ArrayItemElement.Name = XmlMappingUtils.Item_Name;
            m_ArrayItemElement.SchemaType = itemType;
            m_ArrayItemElement.MinOccurs = 0;
            m_ArrayItemElement.MaxOccursString = XmlMappingUtils.MaxOccurs_Unbounded;
            rootFieldItemSequence.Items.Add(m_ArrayItemElement);

            var isRelation = (referencedCollection.StructuralTypeRef.ModelObjectType == ModelObjectType.RelationType);

            var desiredPredefinedTypes = new List<PredefinedVariableTemplateOption>();
            desiredPredefinedTypes.Add(PredefinedVariableTemplateOption.Id);
            if (ParentDatabase.UseCachedReferences && isRelation)
            { desiredPredefinedTypes.Add(PredefinedVariableTemplateOption.Id_Related); }

            var desiredFields = referencedCollection.LocalFields.Where(x => (x.PredefinedType.HasValue && (desiredPredefinedTypes.Contains(x.PredefinedType.Value)))).ToList();

            if (desiredFields.Count > 0)
            {
                foreach (var desiredField in desiredFields)
                {
                    var isId = (desiredField.PredefinedType == PredefinedVariableTemplateOption.Id);

                    var relatedIdElement = new XmlSchemaElement();
                    relatedIdElement.Name = desiredField.Name_Escaped;
                    relatedIdElement.SchemaTypeName = GenericDatabaseUtils.UniqueId_XmlSimpleTypeName;
                    if (!ParentDatabase.UseCachedReferences && isId && (referencedCollectionMapping != null))
                    { relatedIdElement.RefName = new XmlQualifiedName(referencedCollectionMapping.DocumentType.Name); }
                    relatedIdElement.IsNillable = false;
                    relatedIdElement.MinOccurs = 1;
                    relatedIdElement.MaxOccurs = 1;
                    itemValueSequence.Items.Add(relatedIdElement);

                    m_KeyElements.Add(desiredField.RelatedStructuralTypeRef.Value, relatedIdElement);
                }
            }
            else
            {
                var fieldName = GenericDatabaseUtils.DocId_Name;
                var fieldRef = referencedCollection.StructuralTypeRef;

                var relatedIdElement = new XmlSchemaElement();
                relatedIdElement.Name = fieldName;
                relatedIdElement.SchemaTypeName = GenericDatabaseUtils.UniqueId_XmlSimpleTypeName;
                if (!ParentDatabase.UseCachedReferences && (referencedCollectionMapping != null))
                { relatedIdElement.RefName = new XmlQualifiedName(referencedCollectionMapping.DocumentType.Name); }
                relatedIdElement.IsNillable = false;
                relatedIdElement.MinOccurs = 1;
                relatedIdElement.MaxOccurs = 1;
                itemValueSequence.Items.Add(relatedIdElement);

                m_KeyElements.Add(fieldRef, relatedIdElement);
            }

            if (!ParentDatabase.UseCachedReferences)
            { return; }

            m_CacheElement = new XmlSchemaElement();
            m_CacheElement.Name = XmlMappingUtils.CachedDoc_Name;
            m_CacheElement.SchemaTypeName = new XmlQualifiedName(referencedCollectionMapping.DocumentType.Name);
            m_CacheElement.IsNillable = true;
            m_CacheElement.MinOccurs = 1;
            m_CacheElement.MaxOccurs = 1;
            itemValueSequence.Items.Add(m_CacheElement);
        }

        #endregion

        #region Properties

        public GenericField Field { get { return m_Field; } }
        public GenericCollection ParentCollection { get { return Field.ParentCollection; } }
        public GenericDatabase ParentDatabase { get { return ParentCollection.ParentDatabase; } }
        public XmlDatabaseMapping DbMapping { get { return m_DbMapping; } }
        public XmlCollectionMapping ParentCollectionMapping { get { return m_ParentCollectionMapping; } }

        public FieldMappingType FieldMappingType { get { return m_FieldMappingType; } }
        public XmlSchemaElement RootFieldElement { get { return m_RootFieldElement; } }

        public bool HasArrayItemElement { get { return (m_ArrayItemElement != null); } }
        public XmlSchemaElement ArrayItemElement { get { return m_ArrayItemElement; } }

        public bool HasKeyElements { get { return ((m_KeyElements != null) && (m_KeyElements.Count > 0)); } }
        public Dictionary<ModelObjectReference, XmlSchemaElement> KeyElements { get { return new Dictionary<ModelObjectReference, XmlSchemaElement>(m_KeyElements, m_KeyElements.Comparer); } }

        public bool HasValueElement { get { return (m_ValueElement != null); } }
        public XmlSchemaElement ValueElement { get { return m_ValueElement; } }

        public bool HasCacheElement { get { return (m_CacheElement != null); } }
        public XmlSchemaElement CacheElement { get { return m_CacheElement; } }

        public bool HasDocIdElement { get { return (DocIdElement != null); } }
        public XmlSchemaElement DocIdElement
        {
            get
            {
                if (HasValueElement)
                {
                    if (ValueElement.Name == GenericField.Required_IdName)
                    { return ValueElement; }
                }
                if (HasKeyElements)
                {
                    foreach (var keyElement in KeyElements.Values)
                    {
                        if (keyElement.Name == GenericField.Required_IdName)
                        { return keyElement; }
                    }
                }
                return null;
            }
        }

        public int NestedElementCount
        {
            get
            {
                var nestedElementCount = KeyElements.Count;
                nestedElementCount += (HasValueElement ? 1 : 0);
                nestedElementCount += (HasCacheElement ? 1 : 0);
                return nestedElementCount;
            }
        }

        #endregion
    }
}