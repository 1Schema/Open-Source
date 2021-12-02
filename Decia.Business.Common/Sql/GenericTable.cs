using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Constraints;
using Decia.Business.Common.Sql.Triggers;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.Sql
{
    public class GenericTable : IDatabaseMember
    {
        public const bool Default_IsForInputs = true;
        public const bool Default_IsDimensionalSource = false;
        public static readonly int? Default_DimensionNumber = ModelObjectReference.DefaultDimensionNumber;

        #region Members

        private GenericDatabase m_ParentDatabase;
        private Guid m_Id;
        private string m_Name;
        private bool m_IsForInputs;

        private ModelObjectReference? m_StructuralTypeRef;
        private TimePeriodType? m_TD1_TimePeriodType;
        private TimePeriodType? m_TD2_TimePeriodType;

        private Guid? m_ColumnId_ForId;
        private Guid? m_ColumnId_ForName;
        private Guid? m_ColumnId_ForSorting;

        private Dictionary<Guid, GenericColumn> m_Columns;
        private SortedDictionary<string, GenericColumn> m_ColumnsByName;
        private SortedDictionary<int, GenericColumn> m_ColumnsByOrder;

        private PrimaryKey_Constraint m_PrimaryKey;
        private List<UniqueKey_Constraint> m_UniqueKeys;
        private List<Foreign_StaticKey_Constraint> m_StaticForeignKeys;
        private List<Foreign_DynamicKey_Constraint> m_DynamicForeignKeys;

        private ChangeTracking_TriggerSet m_ChangeTrackingTrigger;
        private ChangePrevention_TriggerSet m_ChangePreventionTrigger;
        private Singleton_TriggerSet m_SingletonTrigger;
        private NestedList_TriggerSet m_NestedListTrigger;
        private Matrix_TriggerSet m_MatrixTrigger;
        private SubTable_Structure_TriggerSet m_SubTable_StructureTrigger;
        private SubTable_Time_TriggerSet m_SubTable_TimeD1Trigger;
        private SubTable_Time_TriggerSet m_SubTable_TimeD2Trigger;
        private SubTable_Result_TriggerSet m_SubTable_ResultTrigger;

        #endregion

        #region Constructors

        public GenericTable(GenericDatabase parentDatabase, string tableName)
            : this(parentDatabase, tableName, Default_IsForInputs)
        { }

        public GenericTable(GenericDatabase parentDatabase, string tableName, bool isForInputs)
            : this(parentDatabase, tableName, isForInputs, Default_IsDimensionalSource)
        { }

        public GenericTable(GenericDatabase parentDatabase, string tableName, bool isForInputs, bool isDimensionalSource)
        {
            m_ParentDatabase = parentDatabase;
            m_Id = Guid.NewGuid();
            m_Name = tableName;
            m_IsForInputs = isForInputs;

            m_StructuralTypeRef = null;
            m_TD1_TimePeriodType = null;
            m_TD2_TimePeriodType = null;

            m_ColumnId_ForId = null;
            m_ColumnId_ForName = null;
            m_ColumnId_ForSorting = null;

            m_Columns = new Dictionary<Guid, GenericColumn>();
            m_ColumnsByName = new SortedDictionary<string, GenericColumn>();
            m_ColumnsByOrder = new SortedDictionary<int, GenericColumn>();

            m_PrimaryKey = null;
            m_UniqueKeys = new List<UniqueKey_Constraint>();
            m_StaticForeignKeys = new List<Foreign_StaticKey_Constraint>();
            m_DynamicForeignKeys = new List<Foreign_DynamicKey_Constraint>();

            if (IsForInputs)
            { m_ChangeTrackingTrigger = new ChangeTracking_TriggerSet(this, isDimensionalSource); }
            else
            { m_ChangePreventionTrigger = new ChangePrevention_TriggerSet(this); }

            m_SingletonTrigger = null;
            m_NestedListTrigger = null;
            m_MatrixTrigger = null;
            m_SubTable_StructureTrigger = null;
            m_SubTable_TimeD1Trigger = null;
            m_SubTable_TimeD2Trigger = null;
            m_SubTable_ResultTrigger = null;
        }

        #endregion

        #region Properties

        public GenericDatabase ParentDatabase { get { return m_ParentDatabase; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public bool IsForInputs { get { return m_IsForInputs; } }
        public string Alias { get; set; }

        public ModelObjectReference? StructuralTypeRef { get { return m_StructuralTypeRef; } }
        public void SetStructuralTypeRef(ModelObjectReference structuralTypeRef)
        {
            if (m_StructuralTypeRef.HasValue)
            { throw new InvalidOperationException("The StructuralTypeRef has already been set."); }
            m_StructuralTypeRef = structuralTypeRef;
        }
        public TimePeriodType? TD1_TimePeriodType { get { return m_TD1_TimePeriodType; } }
        public void SetTD1_TimePeriodType(TimePeriodType timePeriodType)
        {
            if (m_TD1_TimePeriodType.HasValue)
            { throw new InvalidOperationException("The TD1 TimePeriodType has already been set."); }
            m_TD1_TimePeriodType = timePeriodType;
        }
        public TimePeriodType? TD2_TimePeriodType { get { return m_TD2_TimePeriodType; } }
        public void SetTD2_TimePeriodType(TimePeriodType timePeriodType)
        {
            if (m_TD2_TimePeriodType.HasValue)
            { throw new InvalidOperationException("The TD2 TimePeriodType has already been set."); }
            m_TD2_TimePeriodType = timePeriodType;
        }
        public string OrderingValue
        {
            get
            {
                var orderingValue = string.Empty;
                if (StructuralTypeRef.HasValue)
                { orderingValue += string.Format("ST: {0}, ", StructuralTypeRef.Value.ModelObjectIdAsInt); }
                orderingValue += string.Format("RS: {0}, ", HasSubTable_ResultTrigger ? 1 : 0);
                if (TD1_TimePeriodType.HasValue)
                { orderingValue += string.Format("TD1: {0}, ", (int)TD1_TimePeriodType.Value); }
                if (TD2_TimePeriodType.HasValue)
                { orderingValue += string.Format("TD2: {0}, ", (int)TD2_TimePeriodType.Value); }
                return orderingValue;
            }
        }

        public Guid? ColumnId_ForId { get { return m_ColumnId_ForId; } }
        public Guid? ColumnId_ForName { get { return m_ColumnId_ForName; } }
        public Guid? ColumnId_ForSorting { get { return m_ColumnId_ForSorting; } }
        public Guid? ColumnId_ForStructure { get { return (this.HasSubTable_StructureTrigger) ? this.SubTable_StructureTrigger.ConstrainedColumnId : (Guid?)null; } }
        public Guid? ColumnId_ForTimeD1 { get { return (this.HasSubTable_TimeD1Trigger) ? this.SubTable_TimeD1Trigger.ConstrainedColumnId : (Guid?)null; } }
        public Guid? ColumnId_ForTimeD2 { get { return (this.HasSubTable_TimeD2Trigger) ? this.SubTable_TimeD2Trigger.ConstrainedColumnId : (Guid?)null; } }
        public Guid? ColumnId_ForResultSet { get { return (this.HasSubTable_ResultTrigger) ? this.SubTable_ResultTrigger.ConstrainedColumnId : (Guid?)null; } }

        public GenericColumn Column_ForId { get { return (m_ColumnId_ForId.HasValue) ? m_Columns[m_ColumnId_ForId.Value] : null; } }
        public GenericColumn Column_ForName { get { return (m_ColumnId_ForName.HasValue) ? m_Columns[m_ColumnId_ForName.Value] : null; } }
        public GenericColumn Column_ForSorting { get { return (m_ColumnId_ForSorting.HasValue) ? m_Columns[m_ColumnId_ForSorting.Value] : null; } }
        public GenericColumn Column_ForStructure { get { return (ColumnId_ForStructure.HasValue) ? m_Columns[ColumnId_ForStructure.Value] : null; } }
        public GenericColumn Column_ForTimeD1 { get { return (ColumnId_ForTimeD1.HasValue) ? m_Columns[ColumnId_ForTimeD1.Value] : null; } }
        public GenericColumn Column_ForTimeD2 { get { return (ColumnId_ForTimeD2.HasValue) ? m_Columns[ColumnId_ForTimeD2.Value] : null; } }
        public GenericColumn Column_ForResultSet { get { return (ColumnId_ForResultSet.HasValue) ? m_Columns[ColumnId_ForResultSet.Value] : null; } }

        public ICollection<GenericColumn> Columns
        {
            get
            {
                var columns = new ReadOnlyList<GenericColumn>(m_Columns.Values);
                columns.IsReadOnly = true;
                return columns;
            }
        }

        public bool HasPrimaryKey { get { return (m_PrimaryKey != null); } }
        public PrimaryKey_Constraint PrimaryKey { get { return m_PrimaryKey; } }
        public bool HasUniqueKeys { get { return (m_UniqueKeys.Count > 0); } }
        public ICollection<UniqueKey_Constraint> UniqueKeys
        {
            get
            {
                var uniqueKeys = new ReadOnlyList<UniqueKey_Constraint>(m_UniqueKeys);
                uniqueKeys.IsReadOnly = true;
                return uniqueKeys;
            }
        }

        public bool HasForeignKeys { get { return (ForeignKeys.Count > 0); } }
        public ICollection<IForeignKey_Constraint> ForeignKeys { get { return m_StaticForeignKeys.Select(x => (x as IForeignKey_Constraint)).Union(m_DynamicForeignKeys.Select(x => (x as IForeignKey_Constraint))).ToList(); } }
        public bool HasStaticForeignKeys { get { return (m_StaticForeignKeys.Count > 0); } }
        public ICollection<Foreign_StaticKey_Constraint> StaticForeignKeys
        {
            get
            {
                var foreignKeys = new ReadOnlyList<Foreign_StaticKey_Constraint>(m_StaticForeignKeys);
                foreignKeys.IsReadOnly = true;
                return foreignKeys;
            }
        }
        public bool HasDynamicForeignKeys { get { return (m_DynamicForeignKeys.Count > 0); } }
        public ICollection<Foreign_DynamicKey_Constraint> DynamicForeignKeys
        {
            get
            {
                var foreignKeys = new ReadOnlyList<Foreign_DynamicKey_Constraint>(m_DynamicForeignKeys);
                foreignKeys.IsReadOnly = true;
                return foreignKeys;
            }
        }

        public bool HasChangeTrackingTrigger { get { return (m_ChangeTrackingTrigger != null); } }
        public ChangeTracking_TriggerSet ChangeTrackingTrigger { get { return m_ChangeTrackingTrigger; } }

        public bool HasChangePreventionTrigger { get { return (m_ChangePreventionTrigger != null); } }
        public ChangePrevention_TriggerSet ChangePreventionTrigger { get { return m_ChangePreventionTrigger; } }

        public bool HasSingletonTrigger { get { return (m_SingletonTrigger != null); } }
        public Singleton_TriggerSet SingletonTrigger { get { return m_SingletonTrigger; } }

        public bool HasNestedListTrigger { get { return (m_NestedListTrigger != null); } }
        public NestedList_TriggerSet NestedListTrigger { get { return m_NestedListTrigger; } }

        public bool HasMatrixTrigger { get { return (m_MatrixTrigger != null); } }
        public Matrix_TriggerSet MatrixTrigger { get { return m_MatrixTrigger; } }

        public bool HasSubTable_StructureTrigger { get { return (m_SubTable_StructureTrigger != null); } }
        public SubTable_Structure_TriggerSet SubTable_StructureTrigger { get { return m_SubTable_StructureTrigger; } }

        public bool HasSubTable_TimeD1Trigger { get { return (m_SubTable_TimeD1Trigger != null); } }
        public SubTable_Time_TriggerSet SubTable_TimeD1Trigger { get { return m_SubTable_TimeD1Trigger; } }

        public bool HasSubTable_TimeD2Trigger { get { return (m_SubTable_TimeD2Trigger != null); } }
        public SubTable_Time_TriggerSet SubTable_TimeD2Trigger { get { return m_SubTable_TimeD2Trigger; } }

        public bool HasSubTable_ResultTrigger { get { return (m_SubTable_ResultTrigger != null); } }
        public SubTable_Result_TriggerSet SubTable_ResultTrigger { get { return m_SubTable_ResultTrigger; } }

        #endregion

        #region Column Methods

        public GenericColumn CreateColumn(string columnName, DeciaDataType dataType, bool allowNulls, bool setToAutomaticId)
        {
            var column = new GenericColumn(this, columnName, dataType, allowNulls, setToAutomaticId);
            m_Columns.Add(column.Id, column);
            m_ColumnsByName.Add(column.Name, column);
            m_ColumnsByOrder.Add(m_ColumnsByOrder.Count, column);
            return column;
        }

        public bool HasColumn(Guid columnId)
        {
            return (m_Columns.ContainsKey(columnId));
        }

        public bool HasColumn(ModelObjectReference variableTemplateRef)
        {
            return (m_Columns.Values.Where(x => x.VariableTemplateRef == variableTemplateRef).Count() > 0);
        }

        public GenericColumn GetColumn(Guid columnId)
        {
            if (!m_Columns.ContainsKey(columnId))
            { return null; }
            return m_Columns[columnId];
        }

        public GenericColumn GetColumn(ModelObjectReference variableTemplateRef)
        {
            var columns = m_Columns.Values.Where(x => x.VariableTemplateRef == variableTemplateRef).ToList();
            return columns.FirstOrDefault();
        }

        public void SetColumn_ForId(Guid columnId)
        {
            if (!HasColumn(columnId))
            { throw new InvalidOperationException("The specified Column does not exist in the current Table."); }

            m_ColumnId_ForId = columnId;
        }

        public void SetColumn_ForName(Guid columnId)
        {
            if (!HasColumn(columnId))
            { throw new InvalidOperationException("The specified Column does not exist in the current Table."); }

            m_ColumnId_ForName = columnId;
        }

        public void SetColumn_ForSorting(Guid columnId)
        {
            if (!HasColumn(columnId))
            { throw new InvalidOperationException("The specified Column does not exist in the current Table."); }

            m_ColumnId_ForSorting = columnId;
        }

        #endregion

        #region Constraint Methods

        public void SetPrimaryKey(Guid columnId)
        {
            SetPrimaryKey(new Guid[] { columnId });
        }

        public void SetPrimaryKey(IEnumerable<Guid> columnIds)
        {
            if (HasPrimaryKey)
            { throw new InvalidOperationException("The current Table already has a Primary Key Constraint."); }

            m_PrimaryKey = new PrimaryKey_Constraint(this, columnIds);
        }

        public UniqueKey_Constraint AddUniqueKey(Guid columnId)
        {
            return AddUniqueKey(new Guid[] { columnId });
        }

        public UniqueKey_Constraint AddUniqueKey(IEnumerable<Guid> columnIds)
        {
            var uniqueKey = new UniqueKey_Constraint(this, columnIds);
            m_UniqueKeys.Add(uniqueKey);
            return uniqueKey;
        }

        public Foreign_StaticKey_Constraint AddStaticForeignKey(ForeignTableData foreignTableData, Guid localColumnId, ForeignColumnData foreignColumnData)
        {
            return AddStaticForeignKey(foreignTableData, localColumnId, foreignColumnData, Default_DimensionNumber);
        }

        public Foreign_StaticKey_Constraint AddStaticForeignKey(ForeignTableData foreignTableData, Guid localColumnId, ForeignColumnData foreignColumnData, int? dimensionNumber)
        {
            var localToForeignColumnIdMappings = new Dictionary<Guid, ForeignColumnData>();
            localToForeignColumnIdMappings.Add(localColumnId, foreignColumnData);

            return AddStaticForeignKey(foreignTableData, localToForeignColumnIdMappings, dimensionNumber);
        }

        public Foreign_StaticKey_Constraint AddStaticForeignKey(ForeignTableData foreignTableData, IDictionary<Guid, ForeignColumnData> localToForeignColumnIdMappings)
        {
            return AddStaticForeignKey(foreignTableData, localToForeignColumnIdMappings, Default_DimensionNumber);
        }

        public Foreign_StaticKey_Constraint AddStaticForeignKey(ForeignTableData foreignTableData, IDictionary<Guid, ForeignColumnData> localToForeignColumnIdMappings, int? dimensionNumber)
        {
            var foreignKey = new Foreign_StaticKey_Constraint(this, foreignTableData, localToForeignColumnIdMappings, dimensionNumber);
            m_StaticForeignKeys.Add(foreignKey);
            return foreignKey;
        }

        public Foreign_DynamicKey_Constraint AddDynamicForeignKey(Guid foreignTableId, Guid localColumnId, Guid foreignColumnId)
        {
            return AddDynamicForeignKey(foreignTableId, localColumnId, foreignColumnId, Default_DimensionNumber);
        }

        public Foreign_DynamicKey_Constraint AddDynamicForeignKey(Guid foreignTableId, Guid localColumnId, Guid foreignColumnId, int? dimensionNumber)
        {
            var localToForeignColumnIdMappings = new Dictionary<Guid, Guid>();
            localToForeignColumnIdMappings.Add(localColumnId, foreignColumnId);

            return AddDynamicForeignKey(foreignTableId, localToForeignColumnIdMappings, dimensionNumber);
        }

        public Foreign_DynamicKey_Constraint AddDynamicForeignKey(Guid foreignTableId, IDictionary<Guid, Guid> localToForeignColumnIdMappings)
        {
            return AddDynamicForeignKey(foreignTableId, localToForeignColumnIdMappings, Default_DimensionNumber);
        }

        public Foreign_DynamicKey_Constraint AddDynamicForeignKey(Guid foreignTableId, IDictionary<Guid, Guid> localToForeignColumnIdMappings, int? dimensionNumber)
        {
            var foreignKey = new Foreign_DynamicKey_Constraint(this, foreignTableId, localToForeignColumnIdMappings, dimensionNumber);
            m_DynamicForeignKeys.Add(foreignKey);
            return foreignKey;
        }

        #endregion

        #region Trigger Methods

        public void SetSingletonTrigger()
        {
            if (HasSingletonTrigger)
            { throw new InvalidOperationException("The current Table already has a Singleton Trigger."); }

            m_SingletonTrigger = new Singleton_TriggerSet(this);
        }

        public void SetNestedListTrigger(Guid constrainedColumnId)
        {
            if (HasNestedListTrigger)
            { throw new InvalidOperationException("The current Table already has a Nested List Trigger."); }

            m_NestedListTrigger = new NestedList_TriggerSet(this, constrainedColumnId);
        }

        public void SetMatrixTrigger(IEnumerable<Guid> constrainedColumnIds)
        {
            if (HasMatrixTrigger)
            { throw new InvalidOperationException("The current Table already has a Matrix Trigger."); }

            m_MatrixTrigger = new Matrix_TriggerSet(this, constrainedColumnIds);
        }

        public void SetSubTable_StructureTrigger(Guid constrainedColumnId)
        {
            if (HasSubTable_StructureTrigger)
            { throw new InvalidOperationException("The current Table already has a Structure Sub-Table Trigger."); }

            m_SubTable_StructureTrigger = new SubTable_Structure_TriggerSet(this, constrainedColumnId);
        }

        public void SetSubTable_TimeD1Trigger(Guid constrainedColumnId)
        {
            SetSubTable_TimeD1Trigger(constrainedColumnId, SubTable_Time_TriggerSet.Default_IsComputed);
        }

        public void SetSubTable_TimeD1Trigger(Guid constrainedColumnId, bool isComputed)
        {
            if (HasSubTable_TimeD1Trigger)
            { throw new InvalidOperationException("The current Table already has a Primary Time Sub-Table Trigger."); }

            m_SubTable_TimeD1Trigger = new SubTable_Time_TriggerSet(this, 1, constrainedColumnId, isComputed);
        }

        public void SetSubTable_TimeD2Trigger(Guid constrainedColumnId)
        {
            SetSubTable_TimeD2Trigger(constrainedColumnId, SubTable_Time_TriggerSet.Default_IsComputed);
        }

        public void SetSubTable_TimeD2Trigger(Guid constrainedColumnId, bool isComputed)
        {
            if (HasSubTable_TimeD2Trigger)
            { throw new InvalidOperationException("The current Table already has a Secondary Time Sub-Table Trigger."); }

            m_SubTable_TimeD2Trigger = new SubTable_Time_TriggerSet(this, 2, constrainedColumnId, isComputed);
        }

        public void SetSubTable_ResultTrigger(Guid constrainedColumnId)
        {
            if (HasSubTable_ResultTrigger)
            { throw new InvalidOperationException("The current Table already has a Result Sub-Table Trigger."); }

            m_SubTable_ResultTrigger = new SubTable_Result_TriggerSet(this, constrainedColumnId);
        }

        #endregion

        #region Export Methods

        public string ExportSchema(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var headerText = "SET ANSI_NULLS ON" + Environment.NewLine + "GO" + Environment.NewLine + "SET QUOTED_IDENTIFIER ON" + Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine;

                var columnDefsText = string.Empty;
                var maxColumnOrder = (m_ColumnsByOrder.Count > 0) ? m_ColumnsByOrder.Keys.Max() : 0;
                foreach (var columnOrder in m_ColumnsByOrder.Keys)
                {
                    var postFix = (((columnOrder != maxColumnOrder) || HasPrimaryKey) ? "," : "") + Environment.NewLine;
                    var columnText = m_ColumnsByOrder[columnOrder].ExportSchema(dbType);
                    columnDefsText += columnText + postFix;
                }

                var primaryKeyDefText = (HasPrimaryKey) ? m_PrimaryKey.ExportSchema(dbType) : string.Empty;

                var tableDefFormat = "CREATE TABLE [dbo].[{0}](" + Environment.NewLine + "{1}" + Environment.NewLine + ") ON [PRIMARY]" + Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine;
                var tableDefText = string.Format(tableDefFormat, Name, (columnDefsText + primaryKeyDefText));

                var footerText = string.Empty;
                foreach (var foreignKey in ForeignKeys)
                {
                    var foreignKeyDefText = foreignKey.ExportSchema(dbType);
                    footerText += foreignKeyDefText + GenericDatabaseUtils.ScriptSpacer_Minor;
                }

                var tableDef = headerText + tableDefText + footerText;
                return tableDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        public string ExportProgrammatics(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var tableDef = string.Empty;
                foreach (var foreignKey in ForeignKeys)
                {
                    var foreignKeyDefText = foreignKey.ExportProgrammatics(dbType);
                    tableDef += foreignKeyDefText + GenericDatabaseUtils.ScriptSpacer_Minor;
                }

                if (HasChangeTrackingTrigger)
                {
                    var changeTrackingTriggerDefText = ChangeTrackingTrigger.ExportToScript(dbType);
                    tableDef += changeTrackingTriggerDefText + GenericDatabaseUtils.ScriptSpacer_Minor;
                }
                if (HasChangePreventionTrigger)
                {
                    var changePreventionTriggerDefText = ChangePreventionTrigger.ExportToScript(dbType);
                    tableDef += changePreventionTriggerDefText + GenericDatabaseUtils.ScriptSpacer_Minor;
                }
                if (HasSingletonTrigger)
                {
                    var singletonTriggerDefText = SingletonTrigger.ExportToScript(dbType);
                    tableDef += singletonTriggerDefText + GenericDatabaseUtils.ScriptSpacer_Minor;
                }
                if (HasMatrixTrigger)
                {
                    var matrixTriggerDefText = MatrixTrigger.ExportToScript(dbType);
                    tableDef += matrixTriggerDefText + GenericDatabaseUtils.ScriptSpacer_Minor;
                }

                return tableDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}