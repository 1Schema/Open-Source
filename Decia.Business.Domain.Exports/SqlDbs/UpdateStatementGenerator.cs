using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;

namespace Decia.Business.Domain.Exports.SqlDbs
{
    public class UpdateStatementGenerator
    {
        public bool HasNestedStatement { get; protected set; }
        public ModelObjectReference MainVariableTemplateRef { get; protected set; }
        public GenericTable MainResultTable { get; protected set; }
        public Dictionary<ModelObjectReference, SqlUpdate_TableHelper> VariableRefTable { get; protected set; }

    }

    public class SqlUpdate_TableHelper
    {
        public SqlUpdate_TableHelper()
        {

        }

        public GenericTable Table { get; set; }
        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public string IdColumnName { get; set; }

        public bool JoinOnStructure { get; set; }
        public bool JoinOnTimeD1 { get; set; }
        public bool JoinOnTimeD2 { get; set; }
        public bool JoinOnResultSet { get; set; }
    }
}