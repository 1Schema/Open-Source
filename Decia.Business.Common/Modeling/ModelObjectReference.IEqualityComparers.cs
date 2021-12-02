using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public class DefaultModelObjectReferenceComparer : IEqualityComparer<ModelObjectReference>, IStringGenerator<ModelObjectReference>
    {
        public bool Equals(ModelObjectReference x, ModelObjectReference y)
        {
            IModelObject xCasted = (IModelObject)x;
            IModelObject yCasted = (IModelObject)y;
            bool areEqual = ((xCasted.ModelObjectType == yCasted.ModelObjectType)
                && (xCasted.ModelObjectId == yCasted.ModelObjectId));
            return areEqual;
        }

        public int GetHashCode(ModelObjectReference obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(ModelObjectReference obj)
        {
            return IModelElementUtils.GetComplexId(obj as IModelObject);
        }
    }

    public class DimensionalModelObjectReferenceComparer : IEqualityComparer<ModelObjectReference>, IStringGenerator<ModelObjectReference>
    {
        public bool Equals(ModelObjectReference x, ModelObjectReference y)
        {
            IDimensionalModelObject xCasted = (IDimensionalModelObject)x;
            IDimensionalModelObject yCasted = (IDimensionalModelObject)y;
            bool areEqual = ((xCasted.ModelObjectType == yCasted.ModelObjectType)
                && (xCasted.ModelObjectId == yCasted.ModelObjectId)
                && (xCasted.NonNullAlternateDimensionNumber == yCasted.NonNullAlternateDimensionNumber));
            return areEqual;
        }

        public int GetHashCode(ModelObjectReference obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(ModelObjectReference obj)
        {
            return IModelElementUtils.GetDimensionalComplexId(obj as IDimensionalModelObject);
        }
    }
}