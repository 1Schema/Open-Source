using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.NoSql.Functions
{
    public abstract class GenericFunction
    {
        #region Members

        private GenericDatabase m_ParentDatabase;
        private Guid m_Id;
        private string m_Name;
        private List<GenericParameter> m_InputParameters;
        private GenericParameter m_OutputParameter;

        #endregion

        #region Constructor

        public GenericFunction(GenericDatabase parentDatabase, string procedureName)
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
    }
}