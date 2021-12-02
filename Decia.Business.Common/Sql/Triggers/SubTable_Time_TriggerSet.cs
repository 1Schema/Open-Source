using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Sql.Constraints;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.Sql.Triggers
{
    public class SubTable_Time_TriggerSet
    {
        public const string NamePartSpacer = GenericDatabaseUtils.NamePartSpacer;
        public const string NamePrefix = "SUB_TIME";
        public const string NameFormat = (NamePrefix + "_{0}");
        public const bool Default_IsComputed = false;

        #region Members

        private GenericTable m_ParentTable;
        private Guid m_Id;
        private string m_Name;
        private int m_DimensionNumber;
        private Guid m_ConstrainedColumnId;

        #endregion

        #region Constructor

        public SubTable_Time_TriggerSet(GenericTable parentTable, int dimensionNumber, Guid constrainedColumnId)
            : this(parentTable, dimensionNumber, constrainedColumnId, Default_IsComputed)
        { }

        public SubTable_Time_TriggerSet(GenericTable parentTable, int dimensionNumber, Guid constrainedColumnId, bool isComputed)
        {
            if (dimensionNumber < TimeDimensionTypeUtils.MinimumTimeDimensionNumber)
            { throw new InvalidOperationException("The Time Dimension Number specified is not valid."); }
            if (dimensionNumber > TimeDimensionTypeUtils.MaximumTimeDimensionNumber)
            { throw new InvalidOperationException("The Time Dimension Number specified is not valid."); }

            m_ParentTable = parentTable;
            m_Id = Guid.NewGuid();
            m_Name = string.Format(NameFormat, dimensionNumber, ParentTable.Name);
            m_DimensionNumber = dimensionNumber;
            m_ConstrainedColumnId = constrainedColumnId;

            var foreignKey = ParentTable.GetForeignKeyForConstrainedColumn(ConstrainedColumnId);
            foreignKey.ConstraintSource = ForeignKey_Source.SubTime;

            if (isComputed)
            {
                foreignKey.IncludeCheck = false;
                foreignKey.UpdateRule = ForeignKey_Action.Nothing;
                foreignKey.DeleteRule = ForeignKey_Action.Nothing;
            }
        }

        #endregion

        #region Properties

        public GenericTable ParentTable { get { return m_ParentTable; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public int DimensionNumber { get { return m_DimensionNumber; } }
        public Guid ConstrainedColumnId { get { return m_ConstrainedColumnId; } }

        #endregion

        #region Export Methods

        public string ExportToScript(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var foreignKey = ParentTable.GetForeignKeyForConstrainedColumn(ConstrainedColumnId);

                if (foreignKey.ConstraintSource != ForeignKey_Source.SubTime)
                { throw new InvalidOperationException("The Foreign Key delete action has been changed to invalid setting."); }

                return string.Empty;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}