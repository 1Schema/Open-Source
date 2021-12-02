using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Sql.Constraints
{
    public class Foreign_StaticKey_Constraint : ForeignKey_ConstraintBase
    {
        #region Constants

        public static readonly ForeignTableData DataType_TableData = new ForeignTableData("Decia_DataType", "Name");
        public static readonly ForeignColumnData DataType_ColumnData_Id = new ForeignColumnData("Id", DeciaDataType.Integer);
        public static readonly ForeignTableData ResultSet_TableData = new ForeignTableData("Decia_ResultSet", "Name");
        public static readonly ForeignColumnData ResultSet_ColumnData_Id = new ForeignColumnData("Id", DeciaDataType.UniqueID);
        public static readonly ForeignTableData TimePeriod_TableData = new ForeignTableData("Decia_TimePeriod", "Id");
        public static readonly ForeignColumnData TimePeriod_ColumnData_Id = new ForeignColumnData("Id", DeciaDataType.UniqueID);

        #endregion

        #region Members

        private ForeignTableData m_ForeignTableData;
        private Dictionary<Guid, ForeignColumnData> m_LocalToForeignColumnIdMappings;
        private Dictionary<ForeignColumnData, Guid> m_ForeignToLocalColumnIdMappings;

        #endregion

        #region Constructors

        public Foreign_StaticKey_Constraint(GenericTable parentTable, ForeignTableData foreignTableData, IDictionary<Guid, ForeignColumnData> localToForeignColumnIdMappings, int? dimensionNumber)
            : base(parentTable, dimensionNumber, foreignTableData.TableName)
        {
            if (localToForeignColumnIdMappings.Count() < 1)
            { throw new InvalidOperationException("Cannot create Foreign Key with zero Columns."); }

            m_ForeignTableData = foreignTableData;
            m_LocalToForeignColumnIdMappings = new Dictionary<Guid, ForeignColumnData>(localToForeignColumnIdMappings);
            m_ForeignToLocalColumnIdMappings = localToForeignColumnIdMappings.ToDictionary(x => x.Value, x => x.Key);
        }

        #endregion

        #region Properties

        public override ForeignTableData ForeignTableData { get { return m_ForeignTableData; } }

        public override ICollection<Guid> LocalColumnIds
        {
            get
            {
                var columnIds = new ReadOnlyList<Guid>(m_LocalToForeignColumnIdMappings.Keys);
                columnIds.IsReadOnly = true;
                return columnIds;
            }
        }

        public override ICollection<ForeignColumnData> ForeignColumnDatas
        {
            get
            {
                var columnNames = new ReadOnlyList<ForeignColumnData>(m_ForeignToLocalColumnIdMappings.Keys);
                columnNames.IsReadOnly = true;
                return columnNames;
            }
        }

        #endregion

        #region Methods

        public override bool HasLocalColumnId(Guid localColumnId)
        {
            return (m_LocalToForeignColumnIdMappings.ContainsKey(localColumnId));
        }

        public override ForeignColumnData GetCorrespondingForeignColumnData(Guid localColumnId)
        {
            if (!HasLocalColumnId(localColumnId))
            { throw new InvalidOperationException("The given Local Column Id is not a member of the Foreign Key."); }

            return m_LocalToForeignColumnIdMappings[localColumnId];
        }

        #endregion
    }
}