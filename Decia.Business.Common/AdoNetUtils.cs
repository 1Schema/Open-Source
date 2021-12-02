using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DataProviders;
using DomainDriver.DomainModeling.DomainModels;
using Decia.Business.Common;
using Decia.Business.Common.Exports;

namespace Decia.Business.Common
{
    public static class AdoNetUtils
    {
        public const SqlDb_TargetType Decia_DbType = SqlDb_TargetType.MsSqlServer;
        public const string Decia_DbFormatString_ForDateTime = "yyyy-MM-dd HH:mm:ss.fff";
        public const int Decia_DbFormatNumber_ForDateTime = 121;

        public const bool Decia_AutoCreate_SqlObjects = false;
        public const int Decia_SuccessFlag = 0;
        public const int Decia_FailureFlag = -1;
        public const string Default_InitialCatalog = null;
        public const string MasterCatalog = "master";
        public const bool Default_Prefer_StoredProcs = true;

        public static readonly string SP_SplitList_Name = ClassReflector.GetPropertyName<string>(() => Resources.Decia_Utils_SplitList);
        public const string SP_SplitList_Parameter0_Name = "projectGuid";
        public const string SP_SplitList_Parameter1_Name = "modelTemplateNumber";
        public const string SP_SplitList_Parameter2_Name = "tableName";
        public const string SP_SplitList_Parameter3_Name = "idColumnName";
        public const string SP_SplitList_Parameter4_Name = "list";
        public const string SP_SplitList_Parameter5_Name = "delimiter";

        public static readonly string SP_ConvertList_Name = ClassReflector.GetPropertyName<string>(() => Resources.Decia_Utils_SplitStrings);
        public const string SP_ConvertList_Parameter0_Name = "List";
        public const string SP_ConvertList_Parameter1_Name = "Delimiter";

        #region Methods - Get Connection

        public static SqlConnection GetConnection(this IDomainModel domainModel)
        {
            return GetConnection(domainModel, Default_InitialCatalog);
        }

        public static SqlConnection GetConnection(this IDomainModel domainModel, string initialCatalog)
        {
            var dataProvider = domainModel.DataProvider;

            if (dataProvider.PersistenceType != DataSourcePersistenceType.Database)
            { return null; }

            var dbContext = (dataProvider.DataSource as DbContext);
            return GetConnection(dbContext, initialCatalog);
        }

        public static SqlConnection GetConnection<T>()
            where T : DbContext, new()
        {
            return GetConnection<T>(Default_InitialCatalog);
        }

        public static SqlConnection GetConnection<T>(string initialCatalog)
            where T : DbContext, new()
        {
            var dbContext = new T();
            return GetConnection(dbContext, initialCatalog);
        }

        public static SqlConnection GetConnection(this DbContext dbContext)
        {
            return GetConnection(dbContext, Default_InitialCatalog);
        }

        public static SqlConnection GetConnection(this DbContext dbContext, string initialCatalog)
        {
            var dbConnection = dbContext.Database.Connection;
            var connectionString = dbConnection.ConnectionString;
            return GetConnection(connectionString, initialCatalog);
        }

        public static SqlConnection GetConnection(this string connectionString)
        {
            return GetConnection(connectionString, Default_InitialCatalog);
        }

        public static SqlConnection GetConnection(this string connectionString, string initialCatalog)
        {
            var resultingConnectionString = GetConnectionString(connectionString, initialCatalog);
            var sqlConnection = new SqlConnection(resultingConnectionString);
            return sqlConnection;
        }

        internal static string Preferred_ConnectionString = string.Empty;

        public static string GetConnectionString(this string baseConnectionString)
        {
            return GetConnectionString(baseConnectionString, null);
        }

        public static string GetConnectionString(this string baseConnectionString, string initialCatalog)
        {
            var connectionStringsToCheck = new List<string>();
            var wasFound = false;
            var foundConnectionString = string.Empty;

            if (!string.IsNullOrWhiteSpace(Preferred_ConnectionString))
            { connectionStringsToCheck.Add(Preferred_ConnectionString); }
            foreach (ConnectionStringSettings setting in ConfigurationManager.ConnectionStrings)
            { connectionStringsToCheck.Add(setting.ConnectionString); }

            foreach (var connectionString in connectionStringsToCheck)
            {
                if (wasFound)
                { continue; }

                if (DoesPartialConnectionStringsMatch(baseConnectionString, connectionString))
                {
                    foundConnectionString = connectionString;
                    Preferred_ConnectionString = foundConnectionString;
                    wasFound = true;
                }
            }

            if (wasFound)
            { baseConnectionString = foundConnectionString; }

            if (string.IsNullOrWhiteSpace(initialCatalog))
            { return baseConnectionString; }

            var pieces = baseConnectionString.Split(new char[] { ';' });
            var newConnectionString = string.Empty;

            foreach (var piece in pieces)
            {
                var isCatalog = (piece.Contains("Initial Catalog"));
                newConnectionString += (isCatalog ? string.Format("Initial Catalog={0};", initialCatalog) : string.Format("{0};", piece));
            }
            return newConnectionString;
        }

        public static readonly string[] SettingNamesToRemove = new string[] { "Password", "Initial Catalog" };
        public static bool DoesPartialConnectionStringsMatch(string partial, string full)
        {
            partial = partial.ToLower();
            full = full.ToLower();

            foreach (var settingNameToRemove in SettingNamesToRemove)
            {
                var settingNameToRemoveLowered = settingNameToRemove.ToLower();
                partial = RemoveSettingPart(partial, settingNameToRemoveLowered);
                full = RemoveSettingPart(full, settingNameToRemoveLowered);
            }
            return (partial == full);
        }

        private static string RemoveSettingPart(string connectionString, string settingToRemove)
        {
            if (connectionString == null)
            { return null; }

            var parts = connectionString.Split(new string[] { settingToRemove }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 0)
            { return string.Empty; }
            if (parts.Length <= 1)
            { return parts[0]; }

            var part0 = parts[0];
            var part1 = parts[1];
            bool foundSemiColon = false;

            for (int i = 0; i < part1.Length; i++)
            {
                var currChar = part1[i];

                if (foundSemiColon)
                { part0 += currChar; }
                else if (currChar == ';')
                { foundSemiColon = true; }
            }
            return part0;
        }

        #endregion

        #region Methods - Reset Connection

        public static void ResetConnection(this IDomainModel domainModel)
        {
            var dataProvider = domainModel.DataProvider;

            if (dataProvider.PersistenceType != DataSourcePersistenceType.Database)
            { return; }

            var dbContext = (dataProvider.DataSource as DbContext);
            ResetConnection(dbContext);
        }

        public static void ResetConnection<T>()
            where T : DbContext, new()
        {
            var dbContext = new T();
            ResetConnection(dbContext);
        }

        public static void ResetConnection(this DbContext dbContext)
        {
            var dbConnection = dbContext.Database.Connection;
            dbConnection.Close();
            dbConnection.Open();
        }

        #endregion

        #region Methods - Perform Bulk Copy

        public static void PerformBulkCopy(this IDomainModel domainModel, DataSet modelDataSet)
        {
            using (var connection = AdoNetUtils.GetConnection(domainModel))
            {
                connection.Open();

                var bulkCopy = new SqlBulkCopy(connection);
                foreach (DataTable dataTable in modelDataSet.Tables)
                {
                    bulkCopy.DestinationTableName = dataTable.TableName;
                    bulkCopy.WriteToServer(dataTable);
                }

                bulkCopy.Close();
                connection.Close();
            }
        }

        #endregion

        #region Methods - Stored Procedures

        public static void CreateOrUpdateStoredProcedure(this SqlConnection sqlConnection, string spName, string spText)
        {
            CreateOrUpdateStoredProcedure(sqlConnection, spName, spText, false);
        }

        public static void CreateOrUpdateStoredProcedure(this SqlConnection sqlConnection, string spName, string spText, bool isFunction)
        {
            if (!spText.Contains(spName))
            { throw new InvalidOperationException("The spName must exist in the spText."); }

            var createCommandText = "IF (OBJECT_ID('dbo." + spName + "') IS NULL) BEGIN EXEC('CREATE PROCEDURE dbo." + spName + " AS SET NOCOUNT ON;') END;";
            if (isFunction)
            { createCommandText = "IF (OBJECT_ID('dbo." + spName + "') IS NULL) BEGIN EXEC('CREATE FUNCTION dbo." + spName + " () RETURNS TABLE AS RETURN (SELECT Item = 0);') END;"; }

            var createSpCommand = sqlConnection.CreateCommand();
            createSpCommand.CommandText = createCommandText;

            sqlConnection.Open();
            createSpCommand.ExecuteNonQuery();
            sqlConnection.Close();

            var alterSpCommand = sqlConnection.CreateCommand();
            alterSpCommand.CommandText = spText;

            sqlConnection.Open();
            alterSpCommand.ExecuteNonQuery();
            sqlConnection.Close();
        }

        public static int ExecuteStoredProcedure(this SqlConnection connection, string procedureName, IDictionary<string, object> parameters)
        {
            var resultName = "ReturnValue";
            int result = 0;

            var command = CreateCommand(connection, procedureName, parameters, resultName, SqlDbType.Int);

            using (command)
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
                result = (int)command.Parameters[resultName].Value;
                command.Connection.Close();
            }
            return result;
        }

        public static DataSet ExecuteQueryWithResult(this SqlConnection connection, string queryText)
        {
            using (var dataAdapter = new SqlDataAdapter())
            {
                dataAdapter.SelectCommand = new SqlCommand(queryText, connection);
                dataAdapter.SelectCommand.CommandType = CommandType.Text;

                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                return dataSet;
            }
        }

        public static DataSet ExecuteStoredProcedureWithResult(this SqlConnection connection, string procedureName, IDictionary<string, object> parameters)
        {
            using (var dataAdapter = new SqlDataAdapter())
            {
                dataAdapter.SelectCommand = new SqlCommand(procedureName, connection);
                dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                foreach (var parameterName in parameters.Keys)
                {
                    var parameterValue = parameters[parameterName];
                    var adjustedParameterName = (parameterName.ElementAt(0) == '@') ? parameterName : ("@" + parameterName);

                    var parameter = new SqlParameter(adjustedParameterName, parameterValue);
                    dataAdapter.SelectCommand.Parameters.Add(parameter);
                }

                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                return dataSet;
            }
        }

        public static DataSet ExecuteFunctionWithResult(this SqlConnection connection, string functionName, IDictionary<string, object> parameters)
        {
            using (var dataAdapter = new SqlDataAdapter())
            {
                var args = string.Empty;

                foreach (var parameterName in parameters.Keys)
                {
                    var adjustedParameterName = (parameterName.ElementAt(0) == '@') ? parameterName : ("@" + parameterName);
                    args += (args.Length < 1) ? adjustedParameterName : ("," + adjustedParameterName);
                }

                var query = string.Format("SELECT * FROM {0}({1})", functionName, args);

                dataAdapter.SelectCommand = new SqlCommand(query, connection);
                dataAdapter.SelectCommand.CommandType = CommandType.Text;

                foreach (var parameterName in parameters.Keys)
                {
                    var parameterValue = parameters[parameterName];
                    var adjustedParameterName = (parameterName.ElementAt(0) == '@') ? parameterName : ("@" + parameterName);

                    args += (args.Length < 1) ? adjustedParameterName : ("," + adjustedParameterName);

                    var parameter = new SqlParameter(adjustedParameterName, parameterValue);
                    dataAdapter.SelectCommand.Parameters.Add(parameter);
                }

                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                return dataSet;
            }
        }

        private static SqlCommand CreateCommand(SqlConnection connection, string commandText, IDictionary<string, object> parameters)
        {
            return CreateCommand(connection, commandText, parameters, null, SqlDbType.Int);
        }

        private static SqlCommand CreateCommand(SqlConnection connection, string commandText, IDictionary<string, object> parameters, string resultParameterName, SqlDbType resultParameterType)
        {
            var command = new SqlCommand(commandText, connection);
            command.CommandType = CommandType.StoredProcedure;

            foreach (var parameterName in parameters.Keys)
            {
                var parameterValue = parameters[parameterName];
                var adjustedParameterName = (parameterName.ElementAt(0) == '@') ? parameterName : ("@" + parameterName);

                var parameter = new SqlParameter(adjustedParameterName, parameterValue);
                command.Parameters.Add(parameter);
            }

            if (string.IsNullOrWhiteSpace(resultParameterName))
            { return command; }

            var returnValue = new SqlParameter(resultParameterName, resultParameterType);
            returnValue.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(returnValue);
            return command;
        }

        #endregion

        #region Methods - DataSets

        public const string RevisionNumber_ColumnName = "EF_RevisionNumber";
        public static readonly BindingFlags RowCopyFlags = (System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public static void CopyObjectToDataRow<T>(this T value, DataRow dataRow, long revisionNumber)
        {
            var valueType = typeof(T);
            var valueIsBatchPersistable = (value is IBatchPersistable);

            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                var columnName = dataColumn.ColumnName;
                var propertyInfo = valueType.GetProperty(columnName, RowCopyFlags);
                var columnValue = propertyInfo.GetValue(value, null);

                if (columnName.ToLower() == RevisionNumber_ColumnName.ToLower())
                { dataRow[columnName] = revisionNumber; }
                else
                {
                    if (columnValue == ((object)null))
                    { dataRow[columnName] = DBNull.Value; }
                    else
                    { dataRow[columnName] = columnValue; }
                }
            }
        }

        public static void CopyDataRowToObject<T>(this T value, DataRow dataRow)
        {
            CopyDataRowToObject<T>(value, dataRow, null);
        }

        public static void CopyDataRowToObject<T>(this T value, DataRow dataRow, long? revisionNumber)
        {
            var valueType = typeof(T);
            var valueIsBatchPersistable = (value is IBatchPersistable);

            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                var columnName = dataColumn.ColumnName;
                var propertyInfo = valueType.GetProperty(columnName, RowCopyFlags);

                if (revisionNumber.HasValue && (columnName.ToLower() == RevisionNumber_ColumnName.ToLower()))
                { propertyInfo.SetValue(value, revisionNumber, null); }
                else
                {
                    if (dataRow.IsNull(columnName))
                    { propertyInfo.SetValue(value, null, null); }
                    else
                    { propertyInfo.SetValue(value, dataRow[columnName], null); }
                }
            }
        }

        #endregion

        #region Methods - Key Matching

        public const string KeyMatching_Opener_Text = "{";
        public const string KeyMatching_Closer_Text = "}";
        public const string KeyMatching_Separator_Text = ",";

        public static string Get_KeyMatching_Opener_SqlCode(string opener) { return "'" + opener + "' + "; }
        public static string Get_KeyMatching_Closer_SqlCode(string closer) { return " + '" + closer + "'"; }
        public static string Get_KeyMatching_Separator_SqlCode(string separator) { return " + '" + separator + "' + "; }

        public static string CovertToKeyMatchingText(this IEnumerable<object> values, bool returnSqlCode)
        {
            return CovertToKeyMatchingText(values, KeyMatching_Opener_Text, KeyMatching_Closer_Text, returnSqlCode);
        }

        public static string CovertToKeyMatchingText(this IEnumerable<object> values, string openingText, string closingText, bool returnSqlCode)
        {
            var opener = (returnSqlCode) ? Get_KeyMatching_Opener_SqlCode(openingText) : openingText;
            var closer = (returnSqlCode) ? Get_KeyMatching_Closer_SqlCode(closingText) : closingText;
            var separator = (returnSqlCode) ? Get_KeyMatching_Separator_SqlCode(KeyMatching_Separator_Text) : KeyMatching_Separator_Text;
            var text = string.Empty;

            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(text))
                { text += value.ToString(); }
                else
                { text += separator + value.ToString(); }
            }

            text = opener + text + closer;
            return text;
        }

        #endregion
    }
}