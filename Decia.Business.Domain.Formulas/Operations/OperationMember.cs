using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues;

namespace Decia.Business.Domain.Formulas.Operations
{
    public class OperationMember
    {
        private bool m_IsValid;
        private bool m_IncludeInResults;
        private DeciaDataType m_DataType;
        private ITimeDimensionSet m_TimeDimesionality;
        private ChronometricValue m_ChronometricValue;
        private ReadOnlyDictionary<StructuralPoint, ChronometricValue> m_AggregationValues;
        private CompoundUnit m_Unit;
        private ModelObjectReference? m_ObjectRef = null;

        public OperationMember()
            : this(true)
        { }

        public OperationMember(bool includeInResults)
        {
            m_IsValid = !includeInResults;
            m_IncludeInResults = includeInResults;
        }

        public OperationMember(DeciaDataType dataType, ITimeDimensionSet timeDimensionality, CompoundUnit unit)
        {
            m_IsValid = true;
            m_IncludeInResults = true;
            m_DataType = dataType;
            m_TimeDimesionality = timeDimensionality;
            m_ChronometricValue = null;
            m_AggregationValues = null;
            m_Unit = unit;
        }

        public OperationMember(ChronometricValue chronometricValue, CompoundUnit unit)
            : this(chronometricValue, unit, null)
        { }

        public OperationMember(ChronometricValue chronometricValue, CompoundUnit unit, ModelObjectReference? objectRef)
        {
            m_IsValid = true;
            m_IncludeInResults = true;
            m_DataType = chronometricValue.DataType;
            m_TimeDimesionality = null;
            m_ChronometricValue = chronometricValue;
            m_AggregationValues = null;
            m_Unit = unit;
            m_ObjectRef = objectRef;
        }

        public OperationMember(DeciaDataType dataType, IDictionary<StructuralPoint, ChronometricValue> aggregationValues, CompoundUnit unit)
        {
            if (aggregationValues.Count() > 0)
            {
                ChronometricValue firstValue = aggregationValues.Values.ElementAt(0);

                DeciaDataTypeUtils.AssertTypesAreCompatible(firstValue.DataType, dataType);

                foreach (ChronometricValue aggregationValue in aggregationValues.Values)
                {
                    if (firstValue == (object)aggregationValue)
                    { continue; }

                    DeciaDataTypeUtils.AssertTypesAreCompatible(firstValue.DataType, dataType);

                    if (!aggregationValue.TimeDimensionSet.Equals(firstValue.TimeDimensionSet))
                    { throw new InvalidOperationException("The TimeDimensionality of an Aggregation Value does not match."); }
                }
            }

            m_IsValid = true;
            m_IncludeInResults = true;
            m_DataType = dataType;
            m_TimeDimesionality = null;
            m_ChronometricValue = null;
            m_AggregationValues = new ReadOnlyDictionary<StructuralPoint, ChronometricValue>(aggregationValues);
            m_Unit = unit;

            m_AggregationValues.IsReadOnly = true;
        }

        public bool IsValid
        {
            get { return m_IsValid; }
        }

        public bool IncludeInResults
        {
            get { return m_IncludeInResults; }
        }

        public bool IsComputable
        {
            get { return m_ChronometricValue != ChronometricValue.NullInstanceAsObject; }
        }

        public DeciaDataType DataType
        {
            get { return m_DataType; }
        }

        public ITimeDimensionSet TimeDimesionality
        {
            get
            {
                if (m_ChronometricValue == ChronometricValue.NullInstanceAsObject)
                { return m_TimeDimesionality; }
                return m_ChronometricValue.TimeDimensionSet;
            }
        }

        public bool IsForAggregation
        {
            get { return (m_AggregationValues != null); }
        }

        public ChronometricValue ChronometricValue
        {
            get { return m_ChronometricValue; }
        }

        public IDictionary<StructuralPoint, ChronometricValue> AggregationValues
        {
            get
            {
                if (m_AggregationValues == null)
                {
                    var aggrValues = new ReadOnlyDictionary<StructuralPoint, ChronometricValue>();
                    aggrValues.Add(StructuralPoint.GlobalStructuralPoint, ChronometricValue);
                    aggrValues.IsReadOnly = true;
                    return aggrValues;
                }

                return m_AggregationValues;
            }
        }

        public ChronometricValue GetAggregationValue(IFormulaDataProvider dataProvider, ICurrentState currentState, StructuralPoint structuralPoint)
        {
            if (m_AggregationValues == null)
            { return ChronometricValue; }

            foreach (var actualPoint in m_AggregationValues.Keys)
            {
                if (actualPoint.IsRelatedAndMoreGeneral(dataProvider.StructuralMap, currentState.ModelInstanceRef, structuralPoint, true, true))
                { return m_AggregationValues[actualPoint]; }
            }

            return null;
        }

        public CompoundUnit Unit
        {
            get
            {
                if (m_Unit != null)
                { return m_Unit; }
                if (m_ChronometricValue != ChronometricValue.NullInstanceAsObject)
                { return m_ChronometricValue.Unit; }
                return null;
            }
        }

        public ModelObjectReference? ObjectRef
        {
            get { return m_ObjectRef; }
        }
    }
}