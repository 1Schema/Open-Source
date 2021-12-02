using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Sql.Constraints
{
    public class Foreign_DynamicKey_Constraint : ForeignKey_ConstraintBase
    {
        #region Members

        private Guid m_ForeignTableId;
        private Dictionary<Guid, Guid> m_LocalToForeignColumnIdMappings;
        private Dictionary<Guid, Guid> m_ForeignToLocalColumnIdMappings;

        #endregion

        #region Constructors

        public Foreign_DynamicKey_Constraint(GenericTable parentTable, Guid foreignTableId, IDictionary<Guid, Guid> localToForeignColumnIdMappings, int? dimensionNumber)
            : base(parentTable, dimensionNumber, GetTableName(parentTable, foreignTableId))
        {
            if (localToForeignColumnIdMappings.Count() < 1)
            { throw new InvalidOperationException("Cannot create Foreign Key with zero Columns."); }

            m_ForeignTableId = foreignTableId;
            m_LocalToForeignColumnIdMappings = new Dictionary<Guid, Guid>(localToForeignColumnIdMappings);
            m_ForeignToLocalColumnIdMappings = localToForeignColumnIdMappings.ToDictionary(x => x.Value, x => x.Key);
        }

        private static string GetTableName(GenericTable parentTable, Guid foreignTableId)
        {
            var foreignTable = parentTable.ParentDatabase.GetTable(foreignTableId);

            if (foreignTable == null)
            { throw new InvalidOperationException("The specified Foreign Table does not exist."); }

            return foreignTable.Name;
        }

        #endregion

        #region Properties

        public Guid ForeignTableId { get { return m_ForeignTableId; } }
        public GenericTable ForeignTable { get { return ParentTable.ParentDatabase.GetTable(ForeignTableId); } }
        public override ForeignTableData ForeignTableData
        {
            get
            {
                var foreignTable = ForeignTable;
                var foreignNameColumn = foreignTable.Column_ForName;
                return new ForeignTableData(foreignTable.Name, foreignNameColumn.Name);
            }
        }

        public override ICollection<Guid> LocalColumnIds
        {
            get
            {
                var columnIds = new ReadOnlyList<Guid>(m_LocalToForeignColumnIdMappings.Keys);
                columnIds.IsReadOnly = true;
                return columnIds;
            }
        }

        public ICollection<Guid> ForeignColumnIds
        {
            get
            {
                var columnIds = new ReadOnlyList<Guid>(m_ForeignToLocalColumnIdMappings.Keys);
                columnIds.IsReadOnly = true;
                return columnIds;
            }
        }

        public ICollection<GenericColumn> ForeignColumns
        {
            get
            {
                var foreignTable = ParentTable.ParentDatabase.GetTable(ForeignTableId);
                var foreignColumns = m_ForeignToLocalColumnIdMappings.Keys.Select(x => foreignTable.GetColumn(x)).ToList();

                var columns = new ReadOnlyList<GenericColumn>(foreignColumns);
                columns.IsReadOnly = true;
                return columns;
            }
        }

        public override ICollection<ForeignColumnData> ForeignColumnDatas
        {
            get
            {
                var foreignColumns = ForeignColumns;
                var foreignColumnDatas = foreignColumns.Select(x => new ForeignColumnData(x.Name, x.DataType)).ToList();

                var columnDatas = new ReadOnlyList<ForeignColumnData>(foreignColumnDatas);
                columnDatas.IsReadOnly = true;
                return columnDatas;
            }
        }

        #endregion

        #region Methods

        public override bool HasLocalColumnId(Guid localColumnId)
        {
            return (m_LocalToForeignColumnIdMappings.ContainsKey(localColumnId));
        }

        public bool HasForeignColumnId(Guid foreignColumnId)
        {
            return (m_ForeignToLocalColumnIdMappings.ContainsKey(foreignColumnId));
        }

        public Guid GetCorrespondingForeignColumnId(Guid localColumnId)
        {
            if (!HasLocalColumnId(localColumnId))
            { throw new InvalidOperationException("The given Local Column Id is not a member of the Foreign Key."); }

            return m_LocalToForeignColumnIdMappings[localColumnId];
        }

        public GenericColumn GetCorrespondingForeignColumn(Guid localColumnId)
        {
            var foreignColumnId = GetCorrespondingForeignColumnId(localColumnId);
            var foreignColumn = ForeignTable.GetColumn(foreignColumnId);
            return foreignColumn;
        }

        public override ForeignColumnData GetCorrespondingForeignColumnData(Guid localColumnId)
        {
            var foreignColumn = GetCorrespondingForeignColumn(localColumnId);
            var foreignColumnData = new ForeignColumnData(foreignColumn.Name, foreignColumn.DataType);
            return foreignColumnData;
        }

        public Guid GetCorrespondingLocalColumnId(Guid foreignColumnId)
        {
            if (!HasForeignColumnId(foreignColumnId))
            { throw new InvalidOperationException("The given Foreign Column Id is not a member of the Foreign Key."); }

            return m_ForeignToLocalColumnIdMappings[foreignColumnId];
        }

        #endregion
    }
}