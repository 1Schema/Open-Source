using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.TypedIds
{
    public interface IParentIdProvider
    {
        bool HasParent { get; }
        ModelObjectType ParentModelObjectType { get; }
        IDictionary<ModelObjectType, ModelObjectReference> ParentModelObjectRefs { get; }

        void SetParent(IParentId parentId);
        void SetParent(ModelObjectType parentModelObjectType, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs);
        void ClearParent();

        T GetParentId<T>(Func<IParentIdProvider, T> idConversionMethod);
    }

    public static class IParentIdProviderUtils
    {
        public static void AssertHasParent(this IParentIdProvider parentProvider)
        {
            if (!parentProvider.HasParent)
            { throw new InvalidOperationException("The current object does not have a Parent."); }
        }

        public static void AssertHasParentTypeValue(this IParentIdProvider parentProvider, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs)
        {
            AssertHasParentTypeValue(parentProvider.ParentModelObjectType, parentModelObjectRefs);
        }

        public static void AssertHasParentTypeValue(this ModelObjectType parentModelObjectType, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs)
        {
            if (!parentModelObjectRefs.ContainsKey(parentModelObjectType))
            { throw new InvalidOperationException("The dictionary does contain a reference for the current object's Parent."); }
        }
    }
}