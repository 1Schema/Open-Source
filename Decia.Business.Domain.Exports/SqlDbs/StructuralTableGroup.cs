using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Exports.SqlDbs
{
    public class StructuralTableGroup
    {
        public string DefaultJoinText = "(1 = 1)";

        private string m_JoinOnText;

        public StructuralTableGroup(bool isMainGroup, ModelObjectReference structuralTypeRef, GenericTable rootTable)
        {
            if (rootTable.PrimaryKey.Columns.Count != 1)
            { throw new InvalidOperationException("The Root Table has a complex Primary Key."); }

            IsMainGroup = isMainGroup;
            StructuralTypeRef = structuralTypeRef;
            RootTable = rootTable;
            SubTableValuesByAlias = new Dictionary<string, StructuralTableValue>();
            UseOuterJoin = false;
        }

        public bool IsMainGroup { get; protected set; }
        public ModelObjectReference StructuralTypeRef { get; protected set; }
        public GenericTable RootTable { get; protected set; }
        public GenericColumn RootIdColumn { get { return ((RootTable != null) && (RootTable.HasPrimaryKey)) ? RootTable.PrimaryKey.Columns.First() : null; } }
        public Dictionary<string, StructuralTableValue> SubTableValuesByAlias { get; protected set; }
        public bool UseOuterJoin { get; set; }

        public bool HasJoinOnText { get { return !string.IsNullOrWhiteSpace(m_JoinOnText); } }
        public string JoinOnText
        {
            get
            {
                if (!HasJoinOnText)
                { return DefaultJoinText; }
                return m_JoinOnText;
            }
            set { m_JoinOnText = value; }
        }
    }
}