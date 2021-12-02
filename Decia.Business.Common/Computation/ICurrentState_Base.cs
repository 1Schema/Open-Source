using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;

namespace Decia.Business.Common.Computation
{
    public interface ICurrentState_Base
    {
        ProcessingType ProcessingType { get; }
        OperationValidityType ValidityArea { get; }
        bool UseExtendedStructure { get; }
        ProcessingAcivityType AcivityType { get; }
        bool IsModelLevel { get; }
        bool ComputeByPeriod { get; }

        Guid ProjectGuid { get; }
        long RevisionNumber { get; }
        ModelObjectReference ModelTemplateRef { get; }
        ModelObjectReference VariableTemplateRef { get; }
        ModelObjectReference ModelInstanceRef { get; }
        ModelObjectReference VariableInstanceRef { get; }
        Nullable<ModelObjectReference> NullableModelInstanceRef { get; }
        Nullable<ModelObjectReference> NullableVariableInstanceRef { get; }

        Nullable<TimePeriodType> PrimaryPeriodType { get; }
        Nullable<TimePeriodType> SecondaryPeriodType { get; }

        Nullable<TimePeriod> PrimaryPeriod { get; }
        Nullable<TimePeriod> SecondaryPeriod { get; }
        MultiTimePeriodKey TimeKey { get; }

        Nullable<TimePeriod> NavigationPeriod { get; set; }
        DateTime ModelStartDate { get; set; }
        DateTime ModelEndDate { get; set; }

        bool HasParentComputationGroup { get; }
        IComputationGroup ParentComputationGroup { get; set; }
    }
}