using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.NoSql
{
    public class XmlDatabaseMapping
    {
        #region Members

        private GenericDatabase m_Database;

        private XmlSchema m_Schema;

        private XmlSchemaSequence m_DatabaseType_CollectionSequence;
        private XmlSchemaComplexType m_DatabaseType;
        private XmlSchemaElement m_DatabaseElement;

        private Dictionary<GenericCollection, XmlCollectionMapping> m_CollectionMappings;
        private Dictionary<GenericField, XmlFieldMapping> m_FieldMappings;

        #endregion

        #region Constructors

        public XmlDatabaseMapping(GenericDatabase database)
        {
            if (database == null)
            { throw new InvalidOperationException("The Database is null."); }

            m_Database = database;

            m_Schema = new XmlSchema();
            m_Schema.Id = m_Database.Name_Escaped;

            m_DatabaseElement = new XmlSchemaElement();

            m_DatabaseType_CollectionSequence = new XmlSchemaSequence();
            m_DatabaseType = new XmlSchemaComplexType();
            m_DatabaseType.Particle = m_DatabaseType_CollectionSequence;

            m_DatabaseElement = new XmlSchemaElement();
            m_DatabaseElement.Name = m_Database.Name_Escaped;
            m_DatabaseElement.SchemaType = m_DatabaseType;
            m_DatabaseElement.AddSchemaComments(GetXmlSchemaComments_ForDatabase());

            m_CollectionMappings = new Dictionary<GenericCollection, XmlCollectionMapping>();
            m_FieldMappings = new Dictionary<GenericField, XmlFieldMapping>();

            foreach (var dataType in EnumUtils.GetEnumValues<DeciaDataType>())
            {
                var simpleType = dataType.ToXmlSimpleType();
                m_Schema.Items.Add(simpleType);
            }
        }

        #endregion

        #region Properties

        public GenericDatabase Database { get { return m_Database; } }
        public XmlSchema Schema { get { return m_Schema; } }
        public MemberType Database_MemberType { get { return MemberType.Database; } }

        public XmlSchemaSequence DatabaseType_CollectionSequence { get { return m_DatabaseType_CollectionSequence; } }
        public XmlSchemaComplexType DatabaseType { get { return m_DatabaseType; } }
        public XmlSchemaElement DatabaseElement { get { return m_DatabaseElement; } }

        public Dictionary<GenericCollection, XmlCollectionMapping> CollectionMappings { get { return new Dictionary<GenericCollection, XmlCollectionMapping>(m_CollectionMappings); } }
        public Dictionary<GenericField, XmlFieldMapping> FieldMappings { get { return new Dictionary<GenericField, XmlFieldMapping>(m_FieldMappings); } }

        #endregion

        #region Methods

        public bool HasCollectionMapping(GenericCollection collection)
        {
            return m_CollectionMappings.ContainsKey(collection);
        }

        public XmlCollectionMapping GetCollectionMapping(GenericCollection collection)
        {
            return m_CollectionMappings[collection];
        }

        public bool HasFieldMapping(GenericField field)
        {
            return m_FieldMappings.ContainsKey(field);
        }

        public XmlFieldMapping GetFieldMapping(GenericField field)
        {
            return m_FieldMappings[field];
        }

        internal void AddCollectionMapping(XmlCollectionMapping collectionMapping)
        {
            if (collectionMapping.DbMapping != this)
            { throw new InvalidOperationException("The Database does not match."); }
            if (m_CollectionMappings.ContainsKey(collectionMapping.Collection))
            { throw new InvalidOperationException("The mapping has already been added."); }

            m_CollectionMappings.Add(collectionMapping.Collection, collectionMapping);
        }

        internal void AddFieldMapping(XmlFieldMapping fieldMapping)
        {
            if (fieldMapping.DbMapping != this)
            { throw new InvalidOperationException("The Database does not match."); }
            if (m_FieldMappings.ContainsKey(fieldMapping.Field))
            { throw new InvalidOperationException("The mapping has already been added."); }

            m_FieldMappings.Add(fieldMapping.Field, fieldMapping);
        }

        public string[] GetXmlSchemaComments_ForDatabase()
        {
            var comment1 = string.Format("Source: {0}", "www.1schema.com");
            var comment2 = string.Format("Notice: {0}", "Copyright © 2016 Decia LLC. All rights reserved.");
            return new string[] { comment1, comment2 };
        }

        #endregion
    }
}