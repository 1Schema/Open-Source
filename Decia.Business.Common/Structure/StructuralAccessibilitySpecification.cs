using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.Structure
{
    public class StructuralAccessibilitySpecification
    {
        private ModelObjectReference m_MainStructuralTypeRef;
        private ModelObjectReference m_MainStructuralInstanceRef;
        private StructuralMap m_StructuralMap;

        public StructuralAccessibilitySpecification(ModelObjectReference mainStructuralTypeRef,
            ModelObjectReference mainStructuralInstanceRef,
            StructuralMap structuralMap)
        {
            m_MainStructuralTypeRef = mainStructuralTypeRef;
            m_MainStructuralInstanceRef = mainStructuralInstanceRef;
            m_StructuralMap = structuralMap;
        }

        public ModelObjectReference MainStructuralTypeRef { get { return m_MainStructuralTypeRef; } }
        public ModelObjectReference MainStructuralInstanceRef { get { return m_MainStructuralInstanceRef; } }
        public StructuralMap StructuralMap { get { return m_StructuralMap; } }

        public HashSet<StructuralPoint> GetStructuralPointsForSpace(ModelObjectReference modelInstanceRef, Nullable<TimePeriod> desiredPeriod, StructuralSpace desiredSpace)
        {
            if (!desiredPeriod.HasValue)
            { desiredPeriod = TimePeriod.ForeverPeriod; }

            StructuralSpace mainSpace = StructuralMap.GetStructuralSpace(MainStructuralTypeRef, true);
            StructuralPoint mainPoint = StructuralMap.GetStructuralPoint(modelInstanceRef, desiredPeriod, MainStructuralInstanceRef, true);
            Dictionary<StructuralDimension, HashSet<StructuralCoordinate>> desiredCoordinatesByDimension = new Dictionary<StructuralDimension, HashSet<StructuralCoordinate>>();

            foreach (StructuralDimension desiredDimension in desiredSpace.Dimensions)
            {
                bool mainContains = mainSpace.Dimensions.Contains(desiredDimension);

                if (mainContains)
                {
                    HashSet<StructuralCoordinate> relevantCooridnates = new HashSet<StructuralCoordinate>();
                    relevantCooridnates.Add(mainPoint.CoordinatesByDimensionId[desiredDimension.DimensionId]);
                    desiredCoordinatesByDimension.Add(desiredDimension, relevantCooridnates);
                }
            }

            throw new NotImplementedException();
        }
    }
}