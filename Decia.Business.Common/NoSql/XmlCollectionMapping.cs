using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.NoSql
{
    public class XmlCollectionMapping
    {
        #region Members

        private GenericCollection m_Collection;
        private XmlDatabaseMapping m_DbMapping;

        private XmlSchemaComplexType m_DocumentType;
        private XmlSchemaSequence m_DocumentType_FieldSequence;

        private XmlSchemaComplexType m_CollectionType;
        private XmlSchemaSequence m_CollectionType_ItemSequence;

        private XmlSchemaElement m_DocumentElement;
        private XmlSchemaElement m_CollectionElement;

        #endregion

        #region Constructors

        public XmlCollectionMapping(GenericCollection collection, XmlDatabaseMapping dbMapping)
        {
            if (collection == null)
            { throw new InvalidOperationException("The Collection is null."); }
            if (dbMapping == null)
            { throw new InvalidOperationException("The Database Mapping is null."); }
            if (collection.ParentDatabase != dbMapping.Database)
            { throw new InvalidOperationException("The Database does not match."); }

            m_Collection = collection;
            m_DbMapping = dbMapping;

            var isRootCollection = (m_Collection.ParentCollection == null);

            m_DbMapping.AddCollectionMapping(this);

            m_DocumentType_FieldSequence = new XmlSchemaSequence();
            m_DocumentType = new XmlSchemaComplexType();
            m_DocumentType.Name = m_Collection.TypeName_Escaped;
            m_DocumentType.Particle = m_DocumentType_FieldSequence;

            m_CollectionType_ItemSequence = new XmlSchemaSequence();
            m_CollectionType = new XmlSchemaComplexType();
            if (isRootCollection)
            { m_CollectionType.Name = m_Collection.CollectionTypeName_Escaped; }
            m_CollectionType.Particle = m_CollectionType_ItemSequence;

            m_DocumentElement = new XmlSchemaElement();
            m_DocumentElement.Name = m_Collection.Name_Escaped;
            m_DocumentElement.SchemaTypeName = new XmlQualifiedName(m_DocumentType.Name);
            m_DocumentElement.AddSchemaComments(GetXmlSchemaComments_ForItem());

            m_CollectionType_ItemSequence.Items.Add(m_DocumentElement);

            m_CollectionElement = new XmlSchemaElement();
            m_CollectionElement.Name = m_Collection.Name_Pluralized;
            if (isRootCollection)
            { m_CollectionElement.SchemaTypeName = new XmlQualifiedName(m_CollectionType.Name); }
            else
            { m_CollectionElement.SchemaType = m_CollectionType; }
            m_CollectionElement.MinOccurs = 1;
            m_CollectionElement.MaxOccurs = 1;
            m_CollectionElement.AddSchemaComments(GetXmlSchemaComments_ForCollection());

            if (m_Collection.StructuralTypeRef == ModelObjectReference.GlobalTypeReference)
            {
                m_DocumentElement.MinOccurs = 1;
                m_DocumentElement.MaxOccurs = 1;
            }
            else
            {
                m_DocumentElement.MinOccurs = 0;
                m_DocumentElement.MaxOccursString = XmlMappingUtils.MaxOccurs_Unbounded;
            }

            m_DbMapping.Schema.Items.Add(DocumentType);
            if (isRootCollection)
            { m_DbMapping.Schema.Items.Add(CollectionType); }
        }

        #endregion

        #region Properties

        public GenericCollection Collection { get { return m_Collection; } }
        public XmlDatabaseMapping DbMapping { get { return m_DbMapping; } }
        public MemberType Collection_MemberType { get { return (IsRoot) ? MemberType.Collection : MemberType.Array; } }
        public MemberType Document_MemberType { get { return (IsRoot) ? MemberType.Document : MemberType.ArrayItem; } }

        public GenericField ParentField { get { return Collection.ParentField; } }
        public XmlFieldMapping ParentFieldMapping { get { return DbMapping.GetFieldMapping(ParentField); } }
        public bool IsRoot { get { return (ParentField == null); } }
        public bool IsNested { get { return (!IsRoot); } }

        public XmlSchemaComplexType DocumentType { get { return m_DocumentType; } }
        public XmlSchemaSequence DocumentType_FieldSequence { get { return m_DocumentType_FieldSequence; } }

        public XmlSchemaComplexType CollectionType { get { return m_CollectionType; } }
        public XmlSchemaSequence CollectionType_ItemSequence { get { return m_CollectionType_ItemSequence; } }

        public XmlSchemaElement DocumentElement { get { return m_DocumentElement; } }
        public MemberType DocumentMemberType { get { return IsRoot ? MemberType.Document : MemberType.ArrayItem; } }

        public XmlSchemaElement CollectionElement { get { return m_CollectionElement; } }
        public MemberType CollectionMemberType { get { return IsRoot ? MemberType.Collection : MemberType.Array; } }

        #endregion

        #region Methods

        public string[] GetXmlSchemaComments_ForCollection()
        {
            var comment1 = string.Format("Member Type: {0}", CollectionMemberType.ToString());
            var comment2 = string.Format("Structural Type: {0}", Collection.StructuralTypeRef.ModelObjectType);
            return new string[] { comment1, comment2 };
        }

        public string[] GetXmlSchemaComments_ForItem()
        {
            var comment1 = string.Format("Member Type: {0}", DocumentMemberType.ToString());
            var comment2 = string.Format("Structural Type: {0}", Collection.StructuralTypeRef.ModelObjectType);
            return new string[] { comment1, comment2 };
        }

        #endregion
    }
}