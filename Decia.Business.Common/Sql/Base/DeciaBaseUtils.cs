using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Decia.Business.Common.Sql.Base.DeciaBase_DataSetTableAdapters;

namespace Decia.Business.Common.Sql.Base
{
    public static class DeciaBaseUtils
    {
        public static string Fnctn_Decia_Format_Specifier = "_Format";
        public static string[] Fnctn_Decia_Format_Placeholders { get { return new string[] { Fnctn_Decia_Format_Placeholder_MyDbName, Fnctn_Decia_Format_Placeholder_DateOfExportName }; } }
        public static string Fnctn_Decia_Format_Placeholder_MyDbName = "<MY_DB_NAME>";
        public static string Fnctn_Decia_Format_Placeholder_DateOfExportName = "<DATE_OF_EXPORT>";

        public static DeciaBase_DataSet DeciaBase_Schema { get { return new DeciaBase_DataSet(); } }
        public static List<string> DeciaBase_TableNames
        {
            get
            {
                var schemaProvider = DeciaBase_Schema;
                var tableNames = new List<string>();

                foreach (DataTable dataTable in schemaProvider.Tables)
                {
                    tableNames.Add(dataTable.TableName);
                }
                return tableNames;
            }
        }
        public static int DeciaBase_TableCount { get { return DeciaBase_TableNames.Count; } }

        public const int Insertion_RowLimit = 1000;
        public const string NullValue = "NULL";
        public const string SumOperator = "SUM";
        public static readonly string Required_Prefix = "Decia_";
        public static readonly string Included_Prefix = "#Included_";
        public static readonly string Included_StructuralInstance_TableName = Included_Prefix + "StructuralInstance";
        public static readonly string Included_StructuralInstance_Id_ColumnName = "Id";
        public static readonly string Included_StructuralInstance_TypeId_ColumnName = "StructuralTypeId";
        public static readonly string Included_TimePeriod_TableName = Included_Prefix + DeciaBase_Schema.Decia_TimePeriod.TableName.Replace(Required_Prefix, string.Empty);
        public static readonly string Included_TimePeriod_Id_ColumnName = "TimePeriodId";
        public static readonly string Included_TimePeriod_TypeId_ColumnName = "TimePeriodTypeId";
        public static readonly string Included_TimePeriod_StartDate_ColumnName = "StartDate";
        public static readonly string Included_TimePeriod_EndDate_ColumnName = "EndDate";
        public static readonly string Included_TimePeriod_OrderIndex_ColumnName = "OrderIndex";
        public static readonly string Included_VariableTemplate_TableName = Included_Prefix + DeciaBase_Schema.Decia_VariableTemplate.TableName.Replace(Required_Prefix, string.Empty);
        public static readonly string Included_VariableTemplateGroup_TableName = Included_Prefix + DeciaBase_Schema.Decia_VariableTemplateGroup.TableName.Replace(Required_Prefix, string.Empty);

        public static TableAdapterManager CreateTableAdapterManager(this string connectionString)
        {
            var tableAdapterManager = new TableAdapterManager();
            tableAdapterManager.Connection = AdoNetUtils.GetConnection(connectionString);

            tableAdapterManager.Decia_MetadataTableAdapter = new Decia_MetadataTableAdapter();
            tableAdapterManager.Decia_TimeDimensionSettingTableAdapter = new Decia_TimeDimensionSettingTableAdapter();
            tableAdapterManager.Decia_TimePeriodTypeTableAdapter = new Decia_TimePeriodTypeTableAdapter();
            tableAdapterManager.Decia_DataTypeTableAdapter = new Decia_DataTypeTableAdapter();
            tableAdapterManager.Decia_ObjectTypeTableAdapter = new Decia_ObjectTypeTableAdapter();
            tableAdapterManager.Decia_TimePeriodTableAdapter = new Decia_TimePeriodTableAdapter();
            tableAdapterManager.Decia_StructuralTypeTableAdapter = new Decia_StructuralTypeTableAdapter();
            tableAdapterManager.Decia_VariableTemplateTableAdapter = new Decia_VariableTemplateTableAdapter();
            tableAdapterManager.Decia_VariableTemplateDependencyTableAdapter = new Decia_VariableTemplateDependencyTableAdapter();
            tableAdapterManager.Decia_VariableTemplateGroupTableAdapter = new Decia_VariableTemplateGroupTableAdapter();
            tableAdapterManager.Decia_VariableTemplateGroupMemberTableAdapter = new Decia_VariableTemplateGroupMemberTableAdapter();
            tableAdapterManager.Decia_ResultSetTableAdapter = new Decia_ResultSetTableAdapter();
            tableAdapterManager.Decia_ResultSetTimeDimensionSettingTableAdapter = new Decia_ResultSetTimeDimensionSettingTableAdapter();
            tableAdapterManager.Decia_ResultSetProcessingMemberTableAdapter = new Decia_ResultSetProcessingMemberTableAdapter();

            tableAdapterManager.Connection = AdoNetUtils.GetConnection(connectionString);
            return tableAdapterManager;
        }

        public static string GetAllInserts_AsSqlText(this DataTable dataTable)
        {
            return GetAllInserts_AsSqlText(dataTable, null);
        }

        public static string GetAllInserts_AsSqlText(this DataTable dataTable, GenericTable table)
        {
            var rows = dataTable.Select();

            if (rows.Length < 1)
            { return string.Empty; }

            var insertText = ExportRows(dataTable, rows);

            if (table == null)
            { return insertText; }
            if (!(table.HasSingletonTrigger || table.HasMatrixTrigger))
            { return insertText; }

            var tryCatchDef = string.Empty;
            if (table.HasSingletonTrigger)
            {
                tryCatchDef += "BEGIN TRY" + Environment.NewLine;
                tryCatchDef += string.Format("DISABLE TRIGGER [dbo].[{0}] ON [dbo].[{1}];", table.SingletonTrigger.GetInsertOrDelete_TriggerName, table.Name) + Environment.NewLine;
                tryCatchDef += string.Format("DELETE FROM [dbo].[{0}];", table.Name) + Environment.NewLine;
                tryCatchDef += insertText;
                tryCatchDef += "END TRY" + Environment.NewLine;
                tryCatchDef += "BEGIN CATCH" + Environment.NewLine;
                tryCatchDef += string.Format("ENABLE TRIGGER [dbo].[{0}] ON [dbo].[{1}];", table.SingletonTrigger.GetInsertOrDelete_TriggerName, table.Name) + Environment.NewLine;
                tryCatchDef += "END CATCH;" + Environment.NewLine;
            }
            if (table.HasMatrixTrigger)
            {
                tryCatchDef += "BEGIN TRY" + Environment.NewLine;
                tryCatchDef += string.Format("DISABLE TRIGGER [dbo].[{0}] ON [dbo].[{1}];", table.MatrixTrigger.GetInsert_TriggerName, table.Name) + Environment.NewLine;
                tryCatchDef += string.Format("DISABLE TRIGGER [dbo].[{0}] ON [dbo].[{1}];", table.MatrixTrigger.GetUpdate_TriggerName, table.Name) + Environment.NewLine;
                tryCatchDef += string.Format("DISABLE TRIGGER [dbo].[{0}] ON [dbo].[{1}];", table.MatrixTrigger.GetDelete_TriggerName, table.Name) + Environment.NewLine;
                tryCatchDef += string.Format("DELETE FROM [dbo].[{0}];", table.Name) + Environment.NewLine;
                tryCatchDef += insertText;
                tryCatchDef += "END TRY" + Environment.NewLine;
                tryCatchDef += "BEGIN CATCH" + Environment.NewLine;
                tryCatchDef += string.Format("ENABLE TRIGGER [dbo].[{0}] ON [dbo].[{1}];", table.MatrixTrigger.GetInsert_TriggerName, table.Name) + Environment.NewLine;
                tryCatchDef += string.Format("ENABLE TRIGGER [dbo].[{0}] ON [dbo].[{1}];", table.MatrixTrigger.GetUpdate_TriggerName, table.Name) + Environment.NewLine;
                tryCatchDef += string.Format("ENABLE TRIGGER [dbo].[{0}] ON [dbo].[{1}];", table.MatrixTrigger.GetDelete_TriggerName, table.Name) + Environment.NewLine;
                tryCatchDef += "END CATCH;" + Environment.NewLine;
            }

            return tryCatchDef;
        }

        private static string ExportRows(DataTable dataTable, DataRow[] rows)
        {
            var numberOfBatches = (rows.Length / Insertion_RowLimit) + (((rows.Length % Insertion_RowLimit) > 0) ? 1 : 0);
            var insertText = string.Empty;

            for (int i = 0; i < numberOfBatches; i++)
            {
                var relevantRows = rows.Skip(i * Insertion_RowLimit).Take(Insertion_RowLimit).ToList();
                var lastRow = relevantRows.Last();
                var insertText_Part = string.Format("INSERT INTO [dbo].[{0}] VALUES", dataTable.TableName) + Environment.NewLine;

                foreach (DataRow row in relevantRows)
                {
                    var rowText = string.Empty;
                    bool isFirst = true;

                    foreach (DataColumn column in dataTable.Columns)
                    {
                        var cellText = GetValue_AsSqlText(row, column);

                        if (!isFirst)
                        { cellText = ", " + cellText; }
                        else
                        { isFirst = false; }

                        rowText += cellText;
                    }
                    rowText = string.Format("({0})", rowText);

                    if (row != lastRow)
                    { rowText += ","; }
                    else
                    { rowText += ";"; }

                    insertText_Part += rowText + Environment.NewLine;
                }
                insertText += insertText_Part;
            }
            return insertText;
        }

        public static string GetValue_AsSqlText(this DataRow row, DataColumn column)
        {
            if (row.IsNull(column))
            { return NullValue; }

            var value = row[column];
            var stringWrapper = "'{0}'";

            if (column.DataType == typeof(bool))
            { return ((bool)value == true) ? "1" : "0"; }
            if (column.DataType == typeof(int))
            { return ((int)value).ToString(); }
            if (column.DataType == typeof(long))
            { return ((long)value).ToString(); }
            if (column.DataType == typeof(double))
            { return ((double)value).ToString("R"); }
            if (column.DataType == typeof(Guid))
            { return string.Format(stringWrapper, ((Guid)value).ToString()); }
            if (column.DataType == typeof(DateTime))
            { return string.Format(stringWrapper, ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff")); }
            if (column.DataType == typeof(TimeSpan))
            { return ((TimeSpan)value).Ticks.ToString(); }
            if (column.DataType == typeof(string))
            {
                var baseValue = (value != null) ? (string)value : string.Empty;
                var escapedValue = baseValue.Replace("'", "''");
                return string.Format(stringWrapper, escapedValue);
            }

            throw new InvalidOperationException("The specified DataType cannot be converted to SQL text.");
        }
    }
}