using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Sql.Programmatics
{
    public abstract class GenericProcedure
    {
        #region Members

        private GenericDatabase m_ParentDatabase;
        private Guid m_Id;
        private string m_Name;
        private List<GenericParameter> m_InputParameters;
        private GenericParameter m_OutputParameter;

        #endregion

        #region Constructor

        public GenericProcedure(GenericDatabase parentDatabase, string procedureName)
        {
            m_ParentDatabase = parentDatabase;
            m_Id = Guid.NewGuid();
            m_Name = procedureName;
            m_InputParameters = new List<GenericParameter>();
            m_OutputParameter = null;
        }

        #endregion

        #region Properties

        public GenericDatabase ParentDatabase { get { return m_ParentDatabase; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }

        public bool HasInputParameters { get { return (m_InputParameters.Count > 1); } }
        public SortedDictionary<int, GenericParameter> InputParameters { get { return new SortedDictionary<int, GenericParameter>(m_InputParameters.ToDictionary(x => x.Index, x => x)); } }
        public bool HasOutputParameter { get { return (m_OutputParameter != null); } }
        public GenericParameter OutputParameter { get { return m_OutputParameter; } }

        #endregion

        #region Methods

        public GenericParameter AddInputParameter(string name, DeciaDataType dataType)
        {
            if (GetInputParameter(name) != null)
            { throw new InvalidOperationException("The Input Parameter has already been set."); }

            var parameter = new GenericParameter(m_InputParameters.Count, name, dataType);
            m_InputParameters.Add(parameter);
            return parameter;
        }

        public GenericParameter GetInputParameter(int index)
        {
            return m_InputParameters[index];
        }

        public GenericParameter GetInputParameter(string name)
        {
            var matchingParams = m_InputParameters.Where(x => (x.Name.ToLower() == name.ToLower())).ToList();

            if (matchingParams.Count > 1)
            { throw new InvalidOperationException("The Input Parameter has duplicates."); }

            return matchingParams.FirstOrDefault();
        }

        public GenericParameter SetOutputParameter(string name, DeciaDataType dataType)
        {
            if (m_OutputParameter != null)
            { throw new InvalidOperationException("The Output Parameter has already been set."); }

            m_OutputParameter = new GenericParameter(0, name, dataType);
            return m_OutputParameter;
        }

        #endregion

        #region Export Methods

        public abstract string ExportProcedure(SqlDb_TargetType dbType);

        protected string GetProcedureHeader(SqlDb_TargetType dbType, string name, ICollection<GenericParameter> includedParameters)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var paramsText = GetParameterText(dbType, includedParameters, true);
                var headerText = "SET ANSI_NULLS ON" + Environment.NewLine + "GO" + Environment.NewLine + "SET QUOTED_IDENTIFIER ON" + Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine;

                headerText += string.Format("CREATE PROCEDURE [dbo].[{0}] ({1})", name, paramsText) + Environment.NewLine;
                headerText += "AS" + Environment.NewLine;
                headerText += "BEGIN" + Environment.NewLine;
                return headerText;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        protected string GetParameterText(SqlDb_TargetType dbType, IEnumerable<GenericParameter> includedParameters, bool isDefinition)
        {
            var defaultParameters = new List<GenericParameter>();
            return GetParameterText(dbType, includedParameters, isDefinition, defaultParameters);
        }

        protected string GetParameterText(SqlDb_TargetType dbType, IEnumerable<GenericParameter> includedParameters, bool isDefinition, IEnumerable<GenericParameter> defaultParameters)
        {
            var paramsText = string.Empty;

            var parameters = includedParameters.Where(x => m_InputParameters.Contains(x)).OrderBy(x => x.Index).ToList();
            if (OutputParameter != null)
            { parameters.Add(OutputParameter); }

            foreach (var parameter in parameters)
            {
                var paramName = parameter.Name;
                var paramType = (isDefinition) ? parameter.DataType.ToNativeDataType(dbType) : string.Empty;
                var paramDefault = (isDefinition && (parameter.HasDefaultValue)) ? string.Format(" = {0}", parameter.DefaultValue) : string.Empty;
                var paramOutput = (parameter == m_OutputParameter) ? " OUTPUT" : string.Empty;
                var paramText = (paramName + " " + paramType + paramDefault + paramOutput);

                paramText = (isDefinition || !defaultParameters.Contains(parameter)) ? paramText : parameter.DefaultValue;

                if (string.IsNullOrWhiteSpace(paramsText))
                { paramsText += paramText; }
                else
                { paramsText += ", " + paramText; }
            }
            return paramsText;
        }

        protected string GetProcedureFooter(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var footerText = "END;" + Environment.NewLine;
                footerText += "GO" + Environment.NewLine;
                return footerText;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}