using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Exports;

namespace Decia.Business.Common.Sql.Constraints
{
    public class UniqueKey_Constraint : IDatabaseMember
    {
        public const string NameFormat = "UQ_{0}____{1}";

        #region Members

        private GenericTable m_ParentTable;
        private Guid m_Id;
        private string m_Name;
        private List<Guid> m_ColumnIds;

        #endregion

        #region Constructors

        public UniqueKey_Constraint(GenericTable parentTable, Guid columnId)
            : this(parentTable, new Guid[] { columnId })
        { }

        public UniqueKey_Constraint(GenericTable parentTable, IEnumerable<Guid> columnIds)
        {
            if (columnIds.Count() < 1)
            { throw new InvalidOperationException("Cannot create Unique Key with zero Columns."); }

            m_ParentTable = parentTable;
            m_Id = Guid.NewGuid();
            m_Name = string.Format(NameFormat, ParentTable.Name, columnIds.Select(x => ParentTable.GetColumn(x).Name).ToCoding_ArgString("_"));
            m_ColumnIds = new List<Guid>(columnIds);
        }

        #endregion

        #region Properties

        public GenericTable ParentTable { get { return m_ParentTable; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public ICollection<Guid> ColumnIds
        {
            get
            {
                var columnIds = new ReadOnlyList<Guid>(m_ColumnIds);
                columnIds.IsReadOnly = true;
                return columnIds;
            }
        }
        public ICollection<GenericColumn> Columns
        {
            get
            {
                var columns = new ReadOnlyList<GenericColumn>(m_ColumnIds.Select(x => ParentTable.GetColumn(x)));
                columns.IsReadOnly = true;
                return columns;
            }
        }

        #endregion

        #region Export Methods

        public string ExportSchema(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var createUniqueKeyText = string.Empty;
                var lastColumnId = m_ColumnIds.Last();

                createUniqueKeyText += Environment.NewLine;
                createUniqueKeyText += GenericDatabaseUtils.Indent_L1 + string.Format("CONSTRAINT [{0}] UNIQUE NONCLUSTERED (", Name);

                foreach (var columnId in m_ColumnIds)
                {
                    var column = m_ParentTable.GetColumn(columnId);
                    var postFix = ((columnId != lastColumnId) ? "," : "") + Environment.NewLine;
                    createUniqueKeyText += GenericDatabaseUtils.Indent_L2 + string.Format("[{0}] ASC", column.Name) + postFix;
                }

                createUniqueKeyText += GenericDatabaseUtils.Indent_L1 + ")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" + Environment.NewLine;
                return createUniqueKeyText;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        public string ExportProgrammatics(SqlDb_TargetType dbType)
        {
            return string.Empty;
        }

        #endregion
    }
}