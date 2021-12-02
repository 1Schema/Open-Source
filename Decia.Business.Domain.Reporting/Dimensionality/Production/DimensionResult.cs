using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class DimensionResult
    {
        public DimensionResult(DimensionEnumerator dimensionEnumerator, ModelObjectReference structuralInstanceRef, ModelObjectReference variableInstanceRef, DynamicValue valueToDisplay)
        {
            if (dimensionEnumerator.DimensionType != DimensionType.Structure)
            { throw new InvalidOperationException("The specified constructor is only for Structural Dimensions."); }

            ParentEnumerator = dimensionEnumerator;
            StructuralInstanceRef = structuralInstanceRef;
            VariableInstanceRef = variableInstanceRef;
            ValueToDisplay = valueToDisplay;
        }

        public DimensionResult(DimensionEnumerator dimensionEnumerator, TimePeriod periodToDisplay)
        {
            if (dimensionEnumerator.DimensionType != DimensionType.Time)
            { throw new InvalidOperationException("The specified constructor is only for Time Dimensions."); }

            ParentEnumerator = dimensionEnumerator;
            PeriodToDisplay = periodToDisplay;
        }

        public DimensionEnumerator ParentEnumerator { get; protected set; }
        public int SortOrder { get { return ParentEnumerator.SortOrder; } }
        public DimensionType DimensionType { get { return ParentEnumerator.DimensionType; } }
        public ModelObjectReference DimensionRef { get { return ParentEnumerator.DimensionRef; } }

        public ModelObjectReference? StructuralInstanceRef { get; protected set; }
        public ModelObjectReference? VariableInstanceRef { get; protected set; }
        public DynamicValue ValueToDisplay { get; protected set; }

        public TimeDimensionType? TimeDimensionType { get { return ParentEnumerator.TimeDimensionType; } }
        public TimePeriod? PeriodToDisplay { get; protected set; }

        public object ObjectToOrderBy
        {
            get
            {
                if ((object)ValueToDisplay == null)
                { return PeriodToDisplay; }
                return ValueToDisplay.GetValue();
            }
        }
    }
}