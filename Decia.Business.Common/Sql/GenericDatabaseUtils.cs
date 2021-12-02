using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Sql.Constraints;

namespace Decia.Business.Common.Sql
{
    public static class GenericDatabaseUtils
    {
        #region Members

        public static DeciaBase_DataSet System_Schema { get { return DeciaBaseUtils.DeciaBase_Schema; } }
        public static List<string> System_TableNames { get { return DeciaBaseUtils.DeciaBase_TableNames; } }
        public static int System_TableCount { get { return DeciaBaseUtils.DeciaBase_TableCount; } }

        public static readonly string Exported_TimePeriod_Id_ColumnNameFormat = "TD{0}_TimePeriodId";
        public static readonly string Exported_ResultSet_Id_ColumnName = "ResultSetId";

        public static readonly string ScriptSpacer_Minor = Environment.NewLine + Environment.NewLine + Environment.NewLine;
        public static readonly string ScriptSpacer_Major = ScriptSpacer_Minor + ScriptSpacer_Minor;
        public const string NamePartSpacer = "___";
        public const string Indent_L0 = "";
        public const string Indent_L1 = "    ";
        public const string Indent_L2 = Indent_L1 + Indent_L1;
        public const string Indent_L3 = Indent_L2 + Indent_L1;
        public const string Indent_L4 = Indent_L3 + Indent_L1;
        public const string Indent_L5 = Indent_L4 + Indent_L1;
        public const string Indent_L6 = Indent_L5 + Indent_L1;
        public const string Indent_L7 = Indent_L6 + Indent_L1;
        public const string Indent_L8 = Indent_L7 + Indent_L1;
        public const string Indent_L9 = Indent_L8 + Indent_L1;

        #endregion

        #region Methods

        public static string ExportScriptToServer(string dbName, string script, string connectionString_ToMasterDb)
        {
            using (var connection = AdoNetUtils.GetConnection(connectionString_ToMasterDb, AdoNetUtils.MasterCatalog))
            {
                connection.Open();
                var serverConnection = new ServerConnection(connection);
                var server = new Server(serverConnection);
                server.ConnectionContext.ExecuteNonQuery(script);
                connection.Close();
            }

            var connectionString_ToResultingDatabase = AdoNetUtils.GetConnectionString(connectionString_ToMasterDb, dbName);
            return connectionString_ToResultingDatabase;
        }

        public static IForeignKey_Constraint GetForeignKeyForConstrainedColumn(this GenericTable parentTable, Guid constrainedColumnId)
        {
            var options = parentTable.ForeignKeys.Where(x => x.LocalColumnIds.Contains(constrainedColumnId)).ToList();

            if (options.Count < 1)
            { throw new InvalidOperationException("There are no Foreign Keys for the specified Column Id."); }
            if (options.Count > 1)
            { throw new InvalidOperationException("There are multiple Foreign Keys for the specified Column Id."); }

            return options.First();
        }

        public static string ToNativeDataType(this DeciaDataType dataType, SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                if (dataType == DeciaDataType.Boolean)
                { return "bit"; }
                else if (dataType == DeciaDataType.DateTime)
                { return "datetime"; }
                else if (dataType == DeciaDataType.Decimal)
                { return "float"; }
                else if (dataType == DeciaDataType.Integer)
                { return "bigint"; }
                else if (dataType == DeciaDataType.Text)
                { return "nvarchar(max)"; }
                else if (dataType == DeciaDataType.TimeSpan)
                { return "bigint"; }
                else if (dataType == DeciaDataType.UniqueID)
                { return "uniqueidentifier"; }
                else
                { throw new NotImplementedException("The specified Data Type is not supported yet."); }
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        public static string ToCoding_VarName(this string name)
        {
            return ("@" + name.ToEscaped_VarName());
        }

        public static string ToCoding_ArgString(this IEnumerable<string> names, string oprtrText)
        {
            var result = string.Empty;

            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(result))
                { result = name; }
                else
                { result += oprtrText + name; }
            }
            return result;
        }

        #endregion

        #region Export Database Methods

        public static string InsertPlaceholderValues(this string functionDef, IDictionary<string, string> placeholderValues)
        {
            foreach (var placeholderName in DeciaBaseUtils.Fnctn_Decia_Format_Placeholders)
            {
                if (!functionDef.Contains(placeholderName))
                { continue; }

                var placeholderValue = placeholderValues[placeholderName];
                functionDef = functionDef.Replace(placeholderName, placeholderValue);
            }
            return functionDef;
        }

        #endregion
    }
}