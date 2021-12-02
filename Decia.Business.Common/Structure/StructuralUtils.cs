using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Structure
{
    public static class StructuralUtils
    {
        public static IDictionary<ModelObjectReference, StructuralDimension> GetDescendantsWithConnectingDimensions(this IStructuralMap structuralMap, StructuralDimension currentDimension, bool useExtendedStructure)
        {
            List<ModelObjectReference> descendantRefs = null;
            Dictionary<ModelObjectReference, StructuralDimension> descendantRefsWithDimensions = new Dictionary<ModelObjectReference, StructuralDimension>();

            if (!useExtendedStructure || (structuralMap.EntityTypeExtendedNetwork == null))
            { descendantRefs = structuralMap.EntityTypeTree.GetDecendants(currentDimension.EntityTypeRef).ToList(); }
            else
            { descendantRefs = structuralMap.EntityTypeExtendedNetwork.GetDecendants(currentDimension.EntityTypeRef).ToList(); }

            foreach (var descendantRef in descendantRefs.ToList())
            {
                var descendantSpace = structuralMap.GetStructuralSpace(descendantRef, useExtendedStructure);
                var relatingDimensions = descendantSpace.Dimensions.Where(d => d.EntityTypeRef == currentDimension.EntityTypeRef).ToList();

                if (relatingDimensions.Count < 1)
                { continue; }

                var relatingDimension = relatingDimensions.First();
                descendantRefsWithDimensions.Add(descendantRef, relatingDimension);
                descendantRefs.Remove(descendantRef);
            }

            while (descendantRefs.Count > 0)
            {
                var descendantRef = descendantRefs[0];
                bool wasProcessed = false;

                foreach (var processedDescendantRef in descendantRefsWithDimensions.Keys.ToList())
                {
                    if (wasProcessed)
                    { continue; }

                    var currentSpace = structuralMap.GetStructuralSpace(descendantRef, useExtendedStructure);
                    var relatingDimensions = currentSpace.Dimensions.Where(d => d.EntityTypeRef == processedDescendantRef).ToList();

                    if (relatingDimensions.Count < 1)
                    { continue; }

                    var relatingDimension = relatingDimensions.First();
                    descendantRefsWithDimensions.Add(descendantRef, relatingDimension);
                    descendantRefs.Remove(descendantRef);
                    wasProcessed = true;
                }
            }

            return descendantRefsWithDimensions;
        }

        public static IDictionary<ModelObjectReference, StructuralDimension> GetAncestorsWithConnectingDimensions(this IStructuralMap structuralMap, StructuralDimension currentDimension, bool useExtendedStructure)
        {
            List<ModelObjectReference> ancestorRefs = null;
            Dictionary<ModelObjectReference, StructuralDimension> ancestorRefsWithDimensions = new Dictionary<ModelObjectReference, StructuralDimension>();

            if (!useExtendedStructure || (structuralMap.EntityTypeExtendedNetwork == null))
            { ancestorRefs = structuralMap.EntityTypeTree.GetAncestors(currentDimension.EntityTypeRef).ToList(); }
            else
            { ancestorRefs = structuralMap.EntityTypeExtendedNetwork.GetAncestors(currentDimension.EntityTypeRef).ToList(); }

            foreach (var ancestorRef in ancestorRefs.ToList())
            {
                var currentSpace = structuralMap.GetStructuralSpace(currentDimension.EntityTypeRef, useExtendedStructure);
                var relatingDimensions = currentSpace.Dimensions.Where(d => d.EntityTypeRef == ancestorRef).ToList();

                if (relatingDimensions.Count < 1)
                { continue; }

                var relatingDimension = relatingDimensions.First();
                ancestorRefsWithDimensions.Add(ancestorRef, relatingDimension);
                ancestorRefs.Remove(ancestorRef);
            }

            while (ancestorRefs.Count > 0)
            {
                var ancestorRef = ancestorRefs[0];
                bool wasProcessed = false;

                foreach (var processedAncestorRef in ancestorRefsWithDimensions.Keys.ToList())
                {
                    if (wasProcessed)
                    { continue; }

                    var currentSpace = structuralMap.GetStructuralSpace(processedAncestorRef, useExtendedStructure);
                    var relatingDimensions = currentSpace.Dimensions.Where(d => d.EntityTypeRef == ancestorRef).ToList();

                    if (relatingDimensions.Count < 1)
                    { continue; }

                    var relatingDimension = relatingDimensions.First();
                    ancestorRefsWithDimensions.Add(ancestorRef, relatingDimension);
                    ancestorRefs.Remove(ancestorRef);
                    wasProcessed = true;
                }
            }

            return ancestorRefsWithDimensions;
        }
    }
}