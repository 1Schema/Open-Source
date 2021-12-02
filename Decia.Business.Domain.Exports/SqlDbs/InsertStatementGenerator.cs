using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Sql;

namespace Decia.Business.Domain.Exports.SqlDbs
{
    public class InsertStatementGenerator
    {
        public InsertStatementGenerator()
        { SelectTables = new List<SqlInsert_TableHelper>(); }

        public string TableName_ForInsert { get; set; }
        public List<SqlInsert_TableHelper> SelectTables { get; protected set; }

        public string ExportStatement(SqlDb_TargetType dbType, bool includeResultSet)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                string insertList = string.Empty, selectList = string.Empty, fromList = string.Empty, whereList = string.Empty;
                var isFirstSelect = true;

                foreach (var select in SelectTables)
                {
                    var fromFormat = (select.IsTemporary) ? "{0} {1}" : "[dbo].[{0}] {1}";

                    insertList += (isFirstSelect ? "" : ", ") + string.Format("[{0}]", select.ColumnName_ForInsert);
                    selectList += (isFirstSelect ? "" : ", ") + string.Format("{0}.[{1}]", select.TableAlias, select.IdColumnName);
                    fromList += (isFirstSelect ? "" : ", ") + string.Format(fromFormat, select.TableName, select.TableAlias);
                    isFirstSelect = false;

                    foreach (var equals in select.EqualsClauses)
                    {
                        var isFirstWhere = string.IsNullOrWhiteSpace(whereList);
                        whereList += (isFirstWhere ? "" : " AND ") + string.Format("({0}.[{1}] = {2})", select.TableAlias, equals.ColumnName_ForWhere, equals.Equals_ValueText);
                    }
                    foreach (var exists in select.ExistsClauses)
                    {
                        bool isFirstWhere = string.IsNullOrWhiteSpace(whereList);
                        whereList += (isFirstWhere ? "" : " AND ") + string.Format("({0}.[{1}] IN ({2}))", select.TableAlias, exists.ColumnName_ForWhere, exists.ExistsIn_ValueText);
                    }
                }

                if (includeResultSet)
                {
                    insertList = ("ResultSetId, " + insertList);
                    selectList = ("@resultSetId, " + selectList);
                }

                var stmtDef = string.Empty;
                stmtDef += GenericDatabaseUtils.Indent_L1 + string.Format("INSERT INTO [dbo].[{0}] ({1})", TableName_ForInsert, insertList) + Environment.NewLine;
                stmtDef += GenericDatabaseUtils.Indent_L2 + string.Format("SELECT {0}", selectList) + Environment.NewLine;
                stmtDef += GenericDatabaseUtils.Indent_L2 + string.Format("FROM {0}", fromList) + Environment.NewLine;
                stmtDef += GenericDatabaseUtils.Indent_L2 + string.Format("WHERE {0};", (!string.IsNullOrWhiteSpace(whereList) ? whereList : "(1 = 1)")) + Environment.NewLine;
                return stmtDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }
    }

    public class SqlInsert_TableHelper
    {
        public SqlInsert_TableHelper()
        {
            EqualsClauses = new List<SqlEquals_ClauseHelper>();
            ExistsClauses = new List<SqlExists_ClauseHelper>();
        }

        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public bool IsTemporary { get { return ((TableName[0] == '@') || (TableName[0] == '#')); } }
        public string IdColumnName { get; set; }

        public List<SqlEquals_ClauseHelper> EqualsClauses { get; protected set; }
        public List<SqlExists_ClauseHelper> ExistsClauses { get; protected set; }

        public string ColumnName_ForInsert { get; set; }
    }

    public class SqlEquals_ClauseHelper
    {
        public SqlEquals_ClauseHelper()
        { }

        public string ColumnName_ForWhere { get; set; }
        public string Equals_ValueText { get; set; }
    }

    public class SqlExists_ClauseHelper
    {
        public SqlExists_ClauseHelper()
        { }

        public string ColumnName_ForWhere { get; set; }
        public string ExistsIn_ValueText { get; set; }
    }
}