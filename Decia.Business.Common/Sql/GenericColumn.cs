using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.Sql
{
    public class GenericColumn : IDatabaseMember
    {
        public const string ConstraintName_Default_Format = "DF_{0}_{1}";

        #region Members

        private GenericTable m_ParentTable;
        private Guid m_Id;
        private string m_Name;
        private DeciaDataType m_DataType;
        private bool m_AllowNulls;
        private bool m_SetToAutomaticId;
        private string m_DefaultValue;
        private string m_SqlFormula;

        private ModelObjectReference? m_VariableTemplateRef;
        private TimeDimensionType? m_TimeDimensionType;

        #endregion

        #region Constructors

        public GenericColumn(GenericTable parentTable, string columnName, DeciaDataType dataType, bool allowNulls, bool setToAutomaticId)
        {
            m_ParentTable = parentTable;
            m_Id = Guid.NewGuid();
            m_Name = columnName;
            m_DataType = dataType;
            m_AllowNulls = allowNulls;
            m_SetToAutomaticId = setToAutomaticId;
            m_DefaultValue = null;
            m_SqlFormula = null;

            m_VariableTemplateRef = null;
        }

        #endregion

        #region Properties

        public GenericTable ParentTable { get { return m_ParentTable; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public string ConstraintName_Default { get { return string.Format(ConstraintName_Default_Format, ParentTable.Name, Name); } }
        public DeciaDataType DataType { get { return m_DataType; } }
        public bool AllowNulls { get { return m_AllowNulls; } }
        public bool SetToAutomaticId { get { return m_SetToAutomaticId; } }

        public string DefaultValue
        {
            get { return m_DefaultValue; }
            set
            {
                if (SetToAutomaticId)
                { throw new InvalidOperationException("Cannot set default for auto-ID."); }

                m_DefaultValue = value;
            }
        }

        public bool UsesSqlFormula
        {
            get { return !string.IsNullOrWhiteSpace(SqlFormula); }
        }

        public string SqlFormula
        {
            get { return m_SqlFormula; }
            set { m_SqlFormula = value; }
        }

        public ModelObjectReference? StructuralTypeRef { get { return ParentTable.StructuralTypeRef; } }
        public ModelObjectReference? VariableTemplateRef { get { return m_VariableTemplateRef; } }
        public TimeDimensionType? TimeDimensionType { get { return m_TimeDimensionType; } }
        public void SetVariableTemplateRef(ModelObjectReference variableTemplateRef)
        {
            if (m_VariableTemplateRef.HasValue)
            { throw new InvalidOperationException("The VariableTemplateRef has already been set."); }
            m_VariableTemplateRef = variableTemplateRef;
        }
        public void SetTimeDimensionType(TimeDimensionType timeDimensionType)
        {
            if (m_TimeDimensionType.HasValue)
            { throw new InvalidOperationException("The TimeDimensionType has already been set."); }
            m_TimeDimensionType = timeDimensionType;
        }

        #endregion

        #region Export Methods

        public string ExportSchema(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var msSqlFormat = string.Empty;
                var columnDef = string.Empty;

                if (!string.IsNullOrWhiteSpace(SqlFormula))
                {
                    msSqlFormat = GenericDatabaseUtils.Indent_L1 + "[{0}] AS ({1})";
                    columnDef = string.Format(msSqlFormat, Name, SqlFormula);
                    return columnDef;
                }

                msSqlFormat = GenericDatabaseUtils.Indent_L1 + "[{0}] {1} {2}" + (AllowNulls ? "NULL" : "NOT NULL") + "{3}";
                var prefix = string.Empty;
                var postfix = string.Empty;

                if (SetToAutomaticId)
                {
                    if (DataType == DeciaDataType.Integer)
                    { prefix = "IDENTITY(1,1) "; }
                    else if (DataType == DeciaDataType.UniqueID)
                    { msSqlFormat += " CONSTRAINT [" + ConstraintName_Default + "] DEFAULT (NEWID())"; }
                    else
                    { throw new NotImplementedException("Defaults for Ids that are not Guid or Ints are not implemented yet."); }
                }
                else if (!string.IsNullOrWhiteSpace(DefaultValue))
                {
                    msSqlFormat += " CONSTRAINT [" + ConstraintName_Default + "] DEFAULT " + DefaultValue.GetExportValue(DataType);
                }

                columnDef = string.Format(msSqlFormat, Name, DataType.ToNativeDataType(dbType), prefix, postfix);
                return columnDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        public string ExportProgrammatics(SqlDb_TargetType dbType)
        {
            return string.Empty;
        }

        #endregion
    }
}