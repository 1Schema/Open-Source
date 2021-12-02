using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;

namespace Decia.Business.Common.Sql.Triggers
{
    public class ChangePrevention_TriggerSet
    {
        public const string NamePartSpacer = GenericDatabaseUtils.NamePartSpacer;
        public const string NamePrefix = "CHANGE_PREVENTION";
        public const string NameFormat = (NamePrefix + "_{0}");

        #region Members

        private GenericTable m_ParentTable;
        private Guid m_Id;
        private string m_Name;

        #endregion

        #region Constructor

        public ChangePrevention_TriggerSet(GenericTable parentTable)
        {
            m_ParentTable = parentTable;
            m_Id = Guid.NewGuid();
            m_Name = string.Format(NameFormat, ParentTable.Name);
        }

        #endregion

        #region Properties

        public GenericTable ParentTable { get { return m_ParentTable; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }

        #endregion

        #region Export Methods

        public string ExportToScript(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var triggerName = string.Format("[dbo].[{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "AllChanges]", NamePrefix, ParentTable.Name);
                var triggerTableName = string.Format("[dbo].[{0}]", ParentTable.Name);

                var triggerDef = string.Format("CREATE TRIGGER {0} ON {1}", triggerName, triggerTableName) + Environment.NewLine;
                triggerDef += "AFTER INSERT, UPDATE, DELETE" + Environment.NewLine;
                triggerDef += "AS" + Environment.NewLine;
                triggerDef += "BEGIN" + Environment.NewLine;

                triggerDef += GenericDatabaseUtils.Indent_L1 + "EXEC [dbo].[spDecia_ResultsLock_AssertIsNotReadOnly];" + Environment.NewLine;

                triggerDef += "END;" + Environment.NewLine;
                triggerDef += "GO" + Environment.NewLine;

                return triggerDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}