using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Structure
{
    public interface IStructuralContext
    {
        IStructuralMap StructuralMap { get; }
        bool UsableForComputation { get; }
        ModelObjectReference ModelTemplateRef { get; }
        ModelObjectReference ModelInstanceRef { get; }

        bool IsResultingSpaceSet { get; }
        StructuralSpace ResultingSpace { get; }
        StructuralMultiplicityType ResultingMultiplicity { get; }
        bool KnowsIncomingStructuralTypeRefs { get; }
        ICollection<ModelObjectReference> IncomingStructuralTypeRefs { get; }

        bool IsResultingPointsSet { get; }
        StructuralPoint? FirstPoint { get; }
        IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> ResultingPointsByKey { get; }
        ICollection<StructuralPoint> ResultingPoints { get; }
        bool KnowsIncomingStructuralInstanceRefs { get; }
        ICollection<ModelObjectReference> IncomingStructuralInstanceRefs { get; }

        void SetResultingSpace(StructuralSpace resultingSpace, bool isUnique);
        void SetResultingSpace(StructuralSpace resultingSpace, bool isUnique, IEnumerable<ModelObjectReference> incomingTypeRefs);

        void SetResultingPoints(IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> resultingPointsByKey);
        void SetResultingPoints(IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> resultingPointsByKey, IEnumerable<ModelObjectReference> incomingInstanceRefs);
    }
}