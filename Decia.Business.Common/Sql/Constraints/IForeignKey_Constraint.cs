using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Sql.Constraints
{
    public class ForeignTableData
    {
        public ForeignTableData(string tableName, string columnName_ForName)
        {
            TableName = tableName;
            ColumnName_ForName = columnName_ForName;
        }

        public string TableName { get; protected set; }
        public string ColumnName_ForName { get; protected set; }
        public DeciaDataType DataType_ForName { get { return DeciaDataType.Text; } }
    }

    public class ForeignColumnData
    {
        public ForeignColumnData(string columnName, DeciaDataType dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }

        public string ColumnName { get; protected set; }
        public DeciaDataType DataType { get; protected set; }
    }

    public interface IForeignKey_Constraint : IDatabaseMember
    {
        #region Properties

        GenericTable ParentTable { get; }
        Guid Id { get; }
        int? DimensionNumber { get; }
        int DimensionNumber_NonNull { get; }
        bool IsAlternateDimension { get; }
        string Name { get; }
        ForeignTableData ForeignTableData { get; }

        ICollection<Guid> LocalColumnIds { get; }
        ICollection<ForeignColumnData> ForeignColumnDatas { get; }

        ForeignKey_Source ConstraintSource { get; set; }
        bool HasMultipleActionPaths { get; }

        bool IncludeCheck { get; set; }
        ForeignKey_Action DeleteRule { get; set; }
        ForeignKey_Action UpdateRule { get; set; }

        #endregion

        #region Methods

        bool HasLocalColumnId(Guid localColumnId);

        ForeignColumnData GetCorrespondingForeignColumnData(Guid localColumnId);

        #endregion
    }
}