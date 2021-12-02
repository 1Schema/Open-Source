using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;

namespace Decia.Business.Common.Sql.Triggers
{
    public class ChangeTracking_TriggerSet
    {
        public const string NamePartSpacer = GenericDatabaseUtils.NamePartSpacer;
        public const string NamePrefix = "CHANGE_TRACKING";
        public const string NameFormat = (NamePrefix + "_{0}");

        #region Members

        private GenericTable m_ParentTable;
        private Guid m_Id;
        private string m_Name;
        private bool m_IsDimensionalSource;

        #endregion

        #region Constructor

        public ChangeTracking_TriggerSet(GenericTable parentTable, bool isDimensionalSource)
        {
            m_ParentTable = parentTable;
            m_Id = Guid.NewGuid();
            m_Name = string.Format(NameFormat, ParentTable.Name);
            m_IsDimensionalSource = isDimensionalSource;
        }

        #endregion

        #region Properties

        public GenericTable ParentTable { get { return m_ParentTable; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public bool IsDimensionalSource { get { return m_IsDimensionalSource; } }

        #endregion

        #region Export Methods

        public string ExportToScript(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var triggerSetDef = string.Empty;
                if (IsDimensionalSource)
                { triggerSetDef += ExportPreActionTrigger(); }
                triggerSetDef += ExportPostActionTrigger();
                return triggerSetDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private string ExportPreActionTrigger()
        {
            var triggerName = string.Format("[dbo].[{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "AllChanges_Pre]", NamePrefix, ParentTable.Name);
            var triggerTableName = string.Format("[dbo].[{0}]", ParentTable.Name);

            var triggerDef = string.Format("CREATE TRIGGER {0} ON {1}", triggerName, triggerTableName) + Environment.NewLine;
            triggerDef += "INSTEAD OF DELETE" + Environment.NewLine;
            triggerDef += "AS" + Environment.NewLine;
            triggerDef += "BEGIN" + Environment.NewLine;

            triggerDef += GenericDatabaseUtils.Indent_L1 + "IF (object_id('tempdb..#DeciaResults_DeleteLock') IS NULL)" + Environment.NewLine;
            triggerDef += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
            triggerDef += GenericDatabaseUtils.Indent_L2 + "CREATE TABLE #DeciaResults_DeleteLock (IsLocked BIT);" + Environment.NewLine;
            triggerDef += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;
            triggerDef += Environment.NewLine;

            triggerDef += GenericDatabaseUtils.Indent_L1 + "EXEC [dbo].[spDecia_ResultsLock_UnlockDeletion];" + Environment.NewLine;
            triggerDef += Environment.NewLine;

            triggerDef += GenericDatabaseUtils.Indent_L1 + string.Format("DELETE s FROM [dbo].[{0}] s WHERE s.[{1}] IN (SELECT d.[{1}] FROM deleted d);", ParentTable.Name, ParentTable.Column_ForId.Name) + Environment.NewLine;

            triggerDef += "END;" + Environment.NewLine;
            triggerDef += "GO" + Environment.NewLine;

            return triggerDef;
        }

        private string ExportPostActionTrigger()
        {
            var triggerName = string.Format("[dbo].[{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "AllChanges_Post]", NamePrefix, ParentTable.Name);
            var triggerTableName = string.Format("[dbo].[{0}]", ParentTable.Name);

            var triggerDef = string.Format("CREATE TRIGGER {0} ON {1}", triggerName, triggerTableName) + Environment.NewLine;
            triggerDef += "AFTER INSERT, UPDATE, DELETE" + Environment.NewLine;
            triggerDef += "AS" + Environment.NewLine;
            triggerDef += "BEGIN" + Environment.NewLine;
            triggerDef += GenericDatabaseUtils.Indent_L1 + "EXEC [dbo].[spDecia_ChangeState_IncrementLatest] NULL, NULL;" + Environment.NewLine;
            triggerDef += "END;" + Environment.NewLine;
            triggerDef += "GO" + Environment.NewLine;

            return triggerDef;
        }

        #endregion
    }
}