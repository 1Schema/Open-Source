using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Structure
{
    public class StructuralContext : IStructuralContext
    {
        private IStructuralMap m_StructuralMap;
        private ModelObjectReference m_ModelTemplateRef;
        private Nullable<ModelObjectReference> m_ModelInstanceRef;
        private Nullable<StructuralSpace> m_ResultingSpace;
        private Nullable<StructuralMultiplicityType> m_ResultingMultiplicity;
        private ReadOnlyList<ModelObjectReference> m_IncomingStructuralTypeRefs;
        private ReadOnlyDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> m_ResultingPointsByKey;
        private ReadOnlyList<ModelObjectReference> m_IncomingStructuralInstanceRefs;

        public StructuralContext(IStructuralMap structuralMap, ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef)
        {
            if (structuralMap == null)
            { throw new InvalidOperationException("The StructuralMap must not be null."); }

            m_StructuralMap = structuralMap;
            m_ModelTemplateRef = modelTemplateRef;
            m_ModelInstanceRef = modelInstanceRef;

            m_ResultingSpace = null;
            m_ResultingMultiplicity = null;
            m_IncomingStructuralTypeRefs = null;

            m_ResultingPointsByKey = null;
            m_IncomingStructuralInstanceRefs = null;
        }

        public IStructuralMap StructuralMap
        {
            get { return m_StructuralMap; }
        }

        public bool UsableForComputation
        {
            get { return m_ModelInstanceRef.HasValue; }
        }

        public ModelObjectReference ModelTemplateRef
        {
            get { return m_ModelTemplateRef; }
        }

        public ModelObjectReference ModelInstanceRef
        {
            get
            {
                if (!UsableForComputation)
                { throw new InvalidOperationException("The current Structural Context is not instantiated for Computation."); }

                return m_ModelInstanceRef.Value;
            }
        }

        public bool IsResultingSpaceSet
        {
            get { return m_ResultingSpace.HasValue; }
        }

        public StructuralSpace ResultingSpace
        {
            get
            {
                if (!IsResultingSpaceSet)
                { throw new InvalidOperationException("The current Structural Context's resulting StructuralSpace is not set."); }

                return m_ResultingSpace.Value;
            }
        }

        public StructuralMultiplicityType ResultingMultiplicity
        {
            get
            {
                if (!IsResultingSpaceSet)
                { throw new InvalidOperationException("The current Structural Context's resulting StructuralSpace is not set."); }

                return m_ResultingMultiplicity.Value;
            }
        }

        public bool KnowsIncomingStructuralTypeRefs
        {
            get { return (m_IncomingStructuralTypeRefs != null); }
        }

        public ICollection<ModelObjectReference> IncomingStructuralTypeRefs
        {
            get
            {
                if (!IsResultingSpaceSet)
                { throw new InvalidOperationException("The current Structural Context's resulting StructuralSpace is not set."); }

                if (!KnowsIncomingStructuralTypeRefs)
                { return null; }

                return m_IncomingStructuralTypeRefs.ToList();
            }
        }

        public bool IsResultingPointsSet
        {
            get { return (m_ResultingPointsByKey != null); }
        }

        public StructuralPoint? FirstPoint
        {
            get
            {
                if (!IsResultingPointsSet)
                { throw new InvalidOperationException("The current Structural Context's resulting StructuralPoints is not set."); }

                if (m_ResultingPointsByKey.Count < 1)
                { return null; }
                return m_ResultingPointsByKey.Values.First();
            }
        }

        public IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> ResultingPointsByKey
        {
            get
            {
                if (!IsResultingPointsSet)
                { throw new InvalidOperationException("The current Structural Context's resulting StructuralPoints is not set."); }

                return m_ResultingPointsByKey;
            }
        }

        public ICollection<StructuralPoint> ResultingPoints
        {
            get
            {
                if (!IsResultingPointsSet)
                { throw new InvalidOperationException("The current Structural Context's resulting StructuralPoints is not set."); }

                return m_ResultingPointsByKey.Values;
            }
        }

        public bool KnowsIncomingStructuralInstanceRefs
        {
            get { return (m_IncomingStructuralInstanceRefs != null); }
        }

        public ICollection<ModelObjectReference> IncomingStructuralInstanceRefs
        {
            get
            {
                if (!IsResultingPointsSet)
                { throw new InvalidOperationException("The current Structural Context's resulting StructuralPoints is not set."); }

                if (!KnowsIncomingStructuralInstanceRefs)
                { return null; }

                return m_IncomingStructuralInstanceRefs;
            }
        }

        public void SetResultingSpace(StructuralSpace resultingSpace, bool isUnique)
        {
            SetResultingSpace(resultingSpace, isUnique, null);
        }

        public void SetResultingSpace(StructuralSpace resultingSpace, bool isUnique, IEnumerable<ModelObjectReference> incomingTypeRefs)
        {
            if (IsResultingSpaceSet)
            { throw new InvalidOperationException("The current Structural Context's resulting StructuralSpace is already set."); }

            m_ResultingSpace = resultingSpace;
            m_ResultingMultiplicity = (isUnique) ? StructuralMultiplicityType.Single : StructuralMultiplicityType.Multiple;

            if (incomingTypeRefs != null)
            {
                m_IncomingStructuralTypeRefs = new ReadOnlyList<ModelObjectReference>(incomingTypeRefs);
                m_IncomingStructuralTypeRefs.IsReadOnly = true;
            }
        }

        public void SetResultingPoints(IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> resultingPointsByKey)
        {
            SetResultingPoints(resultingPointsByKey, null);
        }

        public void SetResultingPoints(IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> resultingPointsByKey, IEnumerable<ModelObjectReference> incomingInstanceRefs)
        {
            if (!UsableForComputation)
            { throw new InvalidOperationException("The current Structural Context's is not usable in computation because it's ModelInstance is not set."); }
            if (!IsResultingSpaceSet)
            { throw new InvalidOperationException("The current Structural Context's resulting StructuralSpace is not set."); }
            if (IsResultingPointsSet)
            { throw new InvalidOperationException("The current Structural Context's resulting StructuralPoints is already set."); }

            if (resultingPointsByKey != null)
            {
                if ((resultingPointsByKey.Count > 1) && (ResultingMultiplicity == StructuralMultiplicityType.Single))
                { throw new InvalidOperationException("The number of resulting StructuralPoints should be at most 1."); }
            }

            if (resultingPointsByKey == null)
            {
                m_ResultingPointsByKey = new ReadOnlyDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint>();
                m_ResultingPointsByKey.IsReadOnly = true;
            }
            else
            {
                m_ResultingPointsByKey = new ReadOnlyDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint>(resultingPointsByKey);
                m_ResultingPointsByKey.IsReadOnly = true;
            }

            if (incomingInstanceRefs != null)
            {
                m_IncomingStructuralInstanceRefs = new ReadOnlyList<ModelObjectReference>(incomingInstanceRefs);
                m_IncomingStructuralInstanceRefs.IsReadOnly = true;
            }
        }
    }
}