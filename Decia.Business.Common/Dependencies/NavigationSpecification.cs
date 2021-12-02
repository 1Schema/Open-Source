using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Dependencies
{
    public class NavigationSpecification
    {
        private ModelObjectReference m_MainStructuralTypeRef;
        private Dictionary<ModelObjectReference, ModelObjectReference> m_StructuralTypeNavigationVariableTemplates;
        private Dictionary<ModelObjectReference, ModelObjectReference> m_NavigationVariableTemplateStructuralTypes;

        public NavigationSpecification(ModelObjectReference mainStructuralTypeRef)
            : this(mainStructuralTypeRef, new Dictionary<ModelObjectReference, ModelObjectReference>())
        { }

        public NavigationSpecification(ModelObjectReference mainStructuralTypeRef, IDictionary<ModelObjectReference, ModelObjectReference> structuralTypeNavigationVariableTemplates)
        {
            m_MainStructuralTypeRef = mainStructuralTypeRef;
            m_StructuralTypeNavigationVariableTemplates = new Dictionary<ModelObjectReference, ModelObjectReference>(structuralTypeNavigationVariableTemplates, ModelObjectReference.DimensionalComparer);
            m_NavigationVariableTemplateStructuralTypes = new Dictionary<ModelObjectReference, ModelObjectReference>(ModelObjectReference.DimensionalComparer);

            foreach (var structuralTypeRef in m_StructuralTypeNavigationVariableTemplates.Keys)
            {
                var navigationVariableTemplateRef = m_StructuralTypeNavigationVariableTemplates[structuralTypeRef];
                m_NavigationVariableTemplateStructuralTypes.Add(navigationVariableTemplateRef, structuralTypeRef);
            }
        }

        public ModelObjectReference MainStructuralTypeRef
        {
            get { return m_MainStructuralTypeRef; }
        }

        public Nullable<ModelObjectReference> GetNavigationVariableForStructuralType(ModelObjectReference desiredStructuralTypeRef)
        {
            bool exactDimensionalMatch;
            return GetNavigationVariableForStructuralType(desiredStructuralTypeRef, out exactDimensionalMatch);
        }

        public Nullable<ModelObjectReference> GetNavigationVariableForStructuralType(ModelObjectReference desiredStructuralTypeRef, out bool exactDimensionalMatch)
        {
            if (m_StructuralTypeNavigationVariableTemplates.ContainsKey(desiredStructuralTypeRef))
            {
                exactDimensionalMatch = true;
                return m_StructuralTypeNavigationVariableTemplates[desiredStructuralTypeRef];
            }
            else
            {
                exactDimensionalMatch = false;

                foreach (ModelObjectReference structuralTypeRef in m_StructuralTypeNavigationVariableTemplates.Keys.OrderBy(st => string.Format("{0}, {1}", st.ModelObjectId, -1 * st.NonNullAlternateDimensionNumber)))
                {
                    if (structuralTypeRef.ModelObjectId == desiredStructuralTypeRef.ModelObjectId)
                    {
                        return structuralTypeRef;
                    }
                }
                return null;
            }
        }
    }
}