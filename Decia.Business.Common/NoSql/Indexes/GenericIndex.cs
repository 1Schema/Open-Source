using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;

namespace Decia.Business.Common.NoSql.Indexes
{
    public class GenericIndex
    {
        public const string NameFormat = "INDEX__{0}__{1}";
        public static readonly FieldMode[] AllowedFieldModes = new FieldMode[] { FieldMode.Data, FieldMode.CachedDocument };

        #region Members

        private GenericCollection m_ParentCollection;
        private Guid m_Id;
        private string m_Name;
        private Dictionary<Guid, GenericField> m_Fields;

        #endregion

        #region Constructors

        internal GenericIndex(GenericCollection parentCollection, GenericField field)
            : this(parentCollection, new GenericField[] { field })
        { }

        internal GenericIndex(GenericCollection parentCollection, IEnumerable<GenericField> fields)
        {
            if (fields.Count() < 1)
            { throw new InvalidOperationException("Cannot create Index with zero Fields."); }

            var uniqueFieldIds = fields.Select(x => x.Id).ToHashSet();

            if (fields.Count() != uniqueFieldIds.Count)
            { throw new InvalidOperationException("Cannot create Index with Fields specified multiple times."); }

            var fieldsById = fields.ToDictionary(x => x.Id, x => x);
            var allFieldsWithDepth = parentCollection.AllFieldsWithDepth;
            var allFieldsById = allFieldsWithDepth.Keys.ToDictionary(x => x.Id, x => x);

            var existInCollection = fieldsById.Keys.Select(x => allFieldsById.ContainsKey(x)).ToHashSet();
            if (existInCollection.Contains(false))
            { throw new InvalidOperationException("Cannot create Index with Fields that do not exist."); }

            var fieldModes = fieldsById.Keys.Select(x => allFieldsById[x].FieldMode).ToHashSet();
            var allowedFieldModes = fieldModes.Select(x => AllowedFieldModes.Contains(x)).ToHashSet();
            if (allowedFieldModes.Contains(false))
            { throw new InvalidOperationException("Cannot create Index with Fields not of FieldMode \"Data\"."); }

            m_ParentCollection = parentCollection;
            m_Id = Guid.NewGuid();
            m_Name = string.Format(NameFormat, ParentCollection.Name, fields.Select(x => ParentCollection.GetLocalField(x.Id).Name).Aggregate((x, y) => (x + y)));
            m_Fields = fields.ToDictionary(x => x.Id, x => x);
        }

        #endregion

        #region Properties

        public GenericCollection ParentCollection { get { return m_ParentCollection; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }

        public ICollection<Guid> FieldIds
        {
            get
            {
                var fieldIds = new ReadOnlyList<Guid>(m_Fields.Keys);
                fieldIds.IsReadOnly = true;
                return fieldIds;
            }
        }

        public ICollection<GenericField> Fields
        {
            get
            {
                var fieldIds = new ReadOnlyList<GenericField>(m_Fields.Values);
                fieldIds.IsReadOnly = true;
                return fieldIds;
            }
        }

        #endregion
    }
}