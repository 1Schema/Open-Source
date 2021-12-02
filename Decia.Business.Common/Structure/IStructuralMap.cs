using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using ReferenceListKey = Decia.Business.Common.TypedIds.ListBasedKey<Decia.Business.Common.Modeling.ModelObjectReference>;

namespace Decia.Business.Common.Structure
{
    public partial interface IStructuralMap
    {
        void AddExtendedCoordinates(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, IDictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>> entityInstanceExtendedCoordinates, IDictionary<ModelObjectReference, IEnumerable<StructuralCoordinate>> relationInstanceExtendedCoordinates);
        void UpdateExtendedCoordinates(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, IDictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>> entityInstanceExtendedCoordinatesByDimension, IDictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>> relationInstanceExtendedCoordinatesByDimension);
        void RemoveExtendedCoordinates(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, IEnumerable<ModelObjectReference> entityInstanceRefs, IEnumerable<ModelObjectReference> relationInstanceRefs);
        void ClearExtendedCoordinates(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod);

        #region Properties

        bool IsTypeMapValid { get; }
        ModelObjectReference ModelTemplateRef { get; }
        ModelObjectReference GlobalTypeRef { get; }
        IEnumerable<ModelObjectReference> EntityTypeRefs { get; }
        IEnumerable<ModelObjectReference> RelationTypeRefs { get; }
        ITree<ModelObjectReference> EntityTypeTree { get; }
        INetwork<ModelObjectReference> EntityTypeExtendedNetwork { get; }
        IEnumerable<ModelObjectReference> ModelInstanceRefs { get; }

        ICollection<TimePeriod> GetDefinedPeriodsForExtendedStructure(ModelObjectReference modelInstanceRef);

        bool GetIsInstanceMapValid(ModelObjectReference modelInstanceRef);
        ModelObjectReference GetGlobalInstanceRef(ModelObjectReference modelInstanceRef);
        IEnumerable<ModelObjectReference> GetEntityInstanceRefs(ModelObjectReference modelInstanceRef);
        IEnumerable<ModelObjectReference> GetRelationInstanceRefs(ModelObjectReference modelInstanceRef);
        ITree<ModelObjectReference> GetEntityInstanceTree(ModelObjectReference modelInstanceRef);
        ITimeNetwork<ModelObjectReference> GetEntityInstanceExtendedTimeNetwork(ModelObjectReference modelInstanceRef);

        ICollection<ModelObjectReference> StructuralTypeRefs { get; }
        IDictionary<ModelObjectReference, ICollection<ModelObjectReference>> StructuralInstanceRefsByModelInstanceRef { get; }

        #endregion

        #region Structural Path Validator Methods

        StructuralPathValidator CreatePathValidator();

        #endregion

        #region Structural Space Methods

        StructuralSpace GetBaseStructuralSpace(ModelObjectReference structuralTypeRef);
        StructuralSpace GetExtendedStructuralSpace(ModelObjectReference structuralTypeRef);
        StructuralSpace GetStructuralSpace(ModelObjectReference structuralTypeRef, bool useExtendedStructure);

        #endregion

        #region Structural Point Methods

        StructuralPoint GetBaseStructuralPoint(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef);
        IDictionary<ModelObjectReference, StructuralPoint> GetBaseStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs);

        StructuralPoint GetExtendedStructuralPoint(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef);
        IDictionary<ModelObjectReference, StructuralPoint> GetExtendedStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs);
        StructuralPoint GetExtendedStructuralPoint(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralInstanceRef);
        IDictionary<ModelObjectReference, StructuralPoint> GetExtendedStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs);

        StructuralPoint GetStructuralPoint(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef, bool useExtendedStructure);
        IDictionary<ModelObjectReference, StructuralPoint> GetStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs, bool useExtendedStructure);
        StructuralPoint GetStructuralPoint(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralInstanceRef, bool useExtendedStructure);
        IDictionary<ModelObjectReference, StructuralPoint> GetStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralTypeRef, IEnumerable<ModelObjectReference> structuralInstanceRefs, bool useExtendedStructure);

        IDictionary<StructuralPoint, IDictionary<ModelObjectReference, StructuralPoint>> GetRelatedStructuralInstances(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef, IEnumerable<StructuralPoint> relatedStructuralPoints, bool useExtendedStructure);
        IDictionary<StructuralPoint, IDictionary<ModelObjectReference, StructuralPoint>> GetRelatedStructuralInstances(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference structuralTypeRef, IEnumerable<StructuralPoint> relatedStructuralPoints, bool useExtendedStructure);

        #endregion

        #region Structural Conversion Methods

        bool ContainsTypeRef(ModelObjectReference structuralTypeRef);
        bool ContainsInstanceRef(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef);
        ModelObjectReference GetStructuralTypeForInstance(ModelObjectReference modelInstanceRef, ModelObjectReference structuralInstanceRef);
        ICollection<ModelObjectReference> GetStructuralTypesForInstances(ModelObjectReference modelInstanceRef, IEnumerable<ModelObjectReference> relatedTypeRefs);
        ICollection<ModelObjectReference> GetStructuralInstancesForType(ModelObjectReference modelInstanceRef, ModelObjectReference structuralTypeRef);

        #endregion

        #region Structural Type Navigation Methods

        bool IsAccessible(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure);
        bool IsUnique(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure);
        bool IsDirectlyAccessibleUsingSpace(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, StructuralSpace bridgingSpace, bool useExtendedStructure);
        bool IsDirectlyAccessibleUsingSpace(ModelObjectReference mainTypeRef, IEnumerable<ModelObjectReference> relatedTypeRefs, StructuralSpace bridgingSpace, bool useExtendedStructure);
        Nullable<StructuralSpace> GetRelativeStructuralSpace(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure);
        Nullable<StructuralSpace> GetRelativeStructuralSpace(ModelObjectReference mainTypeRef, IEnumerable<ModelObjectReference> relatedTypeRefs, bool useExtendedStructure);
        Nullable<KeyValuePair<ReferenceListKey, StructuralSpace>> GetRelativeStructuralRefsAndSpace(ModelObjectReference mainTypeRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure);
        Nullable<KeyValuePair<ReferenceListKey, StructuralSpace>> GetRelativeStructuralRefsAndSpace(ModelObjectReference mainTypeRef, IEnumerable<ModelObjectReference> relatedTypeRefs, bool useExtendedStructure);

        #endregion

        #region Structural Instance Navigation Methods

        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, bool useExtendedStructure);
        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference mainInstanceRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure);
        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, ModelObjectReference relatedTypeRef, bool useExtendedStructure);
        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference mainInstanceRef, ModelObjectReference relatedTypeRef, bool relatedTypeRefIsBound, bool useExtendedStructure);
        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, ModelObjectReference relatedTypeRef, bool relatedTypeRefIsBound, bool useExtendedStructure);
        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference mainInstanceRef, IEnumerable<ModelObjectReference> relatedTypeRefs, bool useExtendedStructure);
        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, IEnumerable<ModelObjectReference> relatedTypeRefs, bool useExtendedStructure);
        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, ModelObjectReference mainInstanceRef, IDictionary<ModelObjectReference, bool> relatedTypeRefsWithIsBound, bool useExtendedStructure);
        IDictionary<ReferenceListKey, StructuralPoint> GetRelativeStructuralPoints(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, ModelObjectReference mainInstanceRef, IDictionary<ModelObjectReference, bool> relatedTypeRefsWithIsBound, bool useExtendedStructure);

        #endregion
    }
}