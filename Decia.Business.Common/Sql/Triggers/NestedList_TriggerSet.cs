using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Sql.Constraints;

namespace Decia.Business.Common.Sql.Triggers
{
    public class NestedList_TriggerSet
    {
        public const string NamePartSpacer = GenericDatabaseUtils.NamePartSpacer;
        public const string NamePrefix = "NESTED_LIST";
        public const string NameFormat = (NamePrefix + "_{0}");

        #region Members

        private GenericTable m_ParentTable;
        private Guid m_Id;
        private string m_Name;
        private Guid m_ConstrainedColumnId;

        #endregion

        #region Constructor

        public NestedList_TriggerSet(GenericTable parentTable, Guid constrainedColumnId)
        {
            m_ParentTable = parentTable;
            m_Id = Guid.NewGuid();
            m_Name = string.Format(NameFormat, ParentTable.Name);
            m_ConstrainedColumnId = constrainedColumnId;

            var foreignKey = ParentTable.GetForeignKeyForConstrainedColumn(ConstrainedColumnId);
            foreignKey.ConstraintSource = ForeignKey_Source.NestedList;
        }

        #endregion

        #region Properties

        public GenericTable ParentTable { get { return m_ParentTable; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public Guid ConstrainedColumnId { get { return m_ConstrainedColumnId; } }
        public GenericColumn ConstrainedColumn { get { return ParentTable.GetColumn(ConstrainedColumnId); } }

        #endregion

        #region Export Methods

        public string ExportToScript(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var foreignKey = ParentTable.GetForeignKeyForConstrainedColumn(ConstrainedColumnId);

                if (foreignKey.ConstraintSource != ForeignKey_Source.NestedList)
                { throw new InvalidOperationException("The Foreign Key delete action has been changed to invalid setting."); }

                return string.Empty;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}