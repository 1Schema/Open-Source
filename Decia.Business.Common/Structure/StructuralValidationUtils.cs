using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Structure
{
    public static class StructuralValidationUtils
    {
        #region Methods - Get Valid Parent Entity Types

        public static ICollection<ModelObjectReference> GetEntityTypeRefs_ValidParents(this IStructuralMap structuralMap, ModelObjectReference currentEntityTypeRef)
        {
            return GetEntityTypeRefs_ValidParents(structuralMap, currentEntityTypeRef, null, false, false);
        }

        public static ICollection<ModelObjectReference> GetEntityTypeRefs_ValidParents(this IStructuralMap structuralMap, ModelObjectReference currentEntityTypeRef, ModelObjectReference? navigationEntityTypeRef, bool isAlreadySet, bool isForExtendedDimensionality)
        {
            if (currentEntityTypeRef.ModelObjectType != ModelObjectType.EntityType)
            { throw new InvalidOperationException("The provided value must be an EntityType."); }
            if (!structuralMap.EntityTypeRefs.Contains(currentEntityTypeRef))
            { throw new InvalidOperationException("The provided value must be an EntityType."); }
            if (navigationEntityTypeRef.HasValue)
            {
                if (navigationEntityTypeRef.Value.ModelObjectType != ModelObjectType.EntityType)
                { throw new InvalidOperationException("The provided value must be an EntityType."); }
                if (!structuralMap.EntityTypeRefs.Contains(navigationEntityTypeRef.Value))
                { throw new InvalidOperationException("The provided value must be an EntityType."); }
            }

            var pathValidator = structuralMap.CreatePathValidator();
            var currentParent = structuralMap.EntityTypeTree.GetParent(currentEntityTypeRef);
            var possibleParents = new List<ModelObjectReference>();

            foreach (var possibleParent in structuralMap.EntityTypeExtendedNetwork.Nodes)
            {
                if (possibleParent == currentEntityTypeRef)
                { continue; }

                if (isForExtendedDimensionality)
                {
                    var isValidParent = pathValidator.IsRelatedEdgeValid(currentEntityTypeRef, navigationEntityTypeRef, possibleParent);

                    if (isValidParent)
                    { possibleParents.Add(possibleParent); }
                }
                else
                {
                    var isValidParent = pathValidator.IsExistenceEdgeValid(currentEntityTypeRef, currentParent, possibleParent);

                    if (isValidParent)
                    { possibleParents.Add(possibleParent); }
                }
            }
            return possibleParents;
        }

        #endregion

        #region Methods - Get Valid Entity Types for Navigation

        public static HashSet<ModelObjectReference> GetEntityTypeRefs_ValidForNavigationVariable(this IStructuralMap structuralMap, ModelObjectReference currentStructuralTypeRef, ModelObjectReference? navigationEntityTypeRef, bool isAlreadySet)
        {
            if (currentStructuralTypeRef.ModelObjectType.GetStructuralType() == StructuralTypeOption.GlobalType)
            { return new HashSet<ModelObjectReference>(); }
            else if (currentStructuralTypeRef.ModelObjectType.GetStructuralType() == StructuralTypeOption.EntityType)
            {
                var validEntityTypeRefs = StructuralValidationUtils.GetEntityTypeRefs_ValidParents(structuralMap, currentStructuralTypeRef, navigationEntityTypeRef, isAlreadySet, true).ToHashSet();
                return validEntityTypeRefs;
            }
            else if (currentStructuralTypeRef.ModelObjectType.GetStructuralType() == StructuralTypeOption.RelationType)
            {
                var validEntityTypeRefs = structuralMap.EntityTypeRefs.ToHashSet();
                return validEntityTypeRefs;
            }
            else
            { throw new InvalidOperationException("Invalid StructuralTypeOption encountered."); }
        }

        #endregion
    }
}