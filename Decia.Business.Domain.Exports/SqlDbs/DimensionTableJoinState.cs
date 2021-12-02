using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Exports.SqlDbs
{
    public class DimensionTableJoinState
    {
        private Nullable<ModelObjectReference> m_TraversalVariableTemplateRef;
        private string m_TraversalValue;
        private Nullable<ModelObjectReference> m_RootVariableTemplateRef;
        private string m_RootValue;

        public DimensionTableJoinState(ModelObjectReference dimensionRef)
        {
            if (dimensionRef.ModelObjectType != ModelObjectType.EntityType)
            { throw new InvalidOperationException("Only Entity Types can act as dimension references."); }

            DimensionRef = dimensionRef;
        }

        public ModelObjectReference DimensionRef { get; protected set; }
        public bool HasTraversalValue { get { return !string.IsNullOrWhiteSpace(m_TraversalValue); } }
        public bool HasRootValue { get { return !string.IsNullOrWhiteSpace(m_RootValue); } }
        public bool IsDimensionNullable { get; set; }

        public ModelObjectReference TraversalVariableTemplateRef
        {
            get
            {
                if (!HasTraversalValue)
                { throw new InvalidOperationException("No TraversalValue exists."); }
                return m_TraversalVariableTemplateRef.Value;
            }
        }

        public string TraversalValue
        {
            get
            {
                if (!HasTraversalValue)
                { throw new InvalidOperationException("No TraversalValue exists."); }
                return m_TraversalValue;
            }
        }

        public ModelObjectReference RootVariableTemplateRef
        {
            get
            {
                if (!HasRootValue)
                { throw new InvalidOperationException("No RootValue exists."); }
                return m_RootVariableTemplateRef.Value;
            }
        }

        public string RootValue
        {
            get
            {
                if (!HasRootValue)
                { throw new InvalidOperationException("No RootValue exists."); }
                return m_RootValue;
            }
        }

        public void SetTraversalValue(ModelObjectReference variableTemplateRef, string columnValue)
        {
            m_TraversalVariableTemplateRef = variableTemplateRef;
            m_TraversalValue = columnValue;
        }

        public void SetRootValue(ModelObjectReference variableTemplateRef, string columnValue)
        {
            m_RootVariableTemplateRef = variableTemplateRef;
            m_RootValue = columnValue;
        }
    }
}