using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;

namespace Decia.Business.Common.Computation
{
    public class CurrentState : ICurrentState
    {
        public const bool DefaultUseExtendedStructure = true;
        public static DateTime DefaultModelStartDate = new DateTime(2010, 1, 1);
        public static DateTime DefaultModelEndDate = new DateTime(2030, 12, 31);

        private ProcessingType m_ProcessingType;
        private OperationValidityType m_ValidityArea;

        private Guid m_ProjectGuid;
        private long m_RevisionNumber;
        private ModelObjectReference m_ModelTemplateRef;
        private Nullable<ModelObjectReference> m_StructuralTypeRef;
        private Nullable<StructuralSpace> m_StructuralSpace;
        private Nullable<ModelObjectReference> m_VariableTemplateRef;
        private Nullable<ModelObjectReference> m_ModelInstanceRef;
        private Nullable<ModelObjectReference> m_StructuralInstanceRef;
        private Nullable<StructuralPoint> m_StructuralPoint;
        private Nullable<ModelObjectReference> m_VariableInstanceRef;

        private Nullable<TimePeriodType> m_PrimaryPeriodType;
        private Nullable<TimePeriodType> m_SecondaryPeriodType;

        private bool m_ComputeByPeriod;
        private Nullable<TimePeriod> m_PrimaryPeriod;
        private Nullable<TimePeriod> m_SecondaryPeriod;

        private Nullable<TimePeriod> m_NavigationPeriod;
        private DateTime m_ModelStartDate;
        private DateTime m_ModelEndDate;

        private IComputationGroup m_ParentComputationGroup;
        private HashSet<StructuralDimension> m_StructuralAggregationDimensions = new HashSet<StructuralDimension>();
        private HashSet<TimeDimensionType> m_TimeAggregationDimensions = new HashSet<TimeDimensionType>();

        public CurrentState(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef,
            DateTime modelStartDate, DateTime modelEndDate)
            : this(projectGuid, revisionNumber, modelTemplateRef, null, null, null, null, null, null, null, modelStartDate, modelEndDate)
        { }

        public CurrentState(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef,
            ModelObjectReference modelInstanceRef, DateTime modelStartDate, DateTime modelEndDate)
            : this(projectGuid, revisionNumber, modelTemplateRef, null, null, null, modelInstanceRef, null, null, null, modelStartDate, modelEndDate)
        { }

        public CurrentState(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef,
            ModelObjectReference structuralTypeRef,
            ModelObjectReference variableTemplateRef)
            : this(projectGuid, revisionNumber, modelTemplateRef, structuralTypeRef, null, variableTemplateRef, null, null, null, null, null, null)
        { }

        public CurrentState(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef,
            ModelObjectReference structuralTypeRef, StructuralSpace structuralSpace,
            ModelObjectReference variableTemplateRef)
            : this(projectGuid, revisionNumber, modelTemplateRef, structuralTypeRef, structuralSpace, variableTemplateRef, null, null, null, null, null, null)
        { }

        public CurrentState(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef,
            ModelObjectReference structuralTypeRef,
            ModelObjectReference variableTemplateRef, ModelObjectReference modelInstanceRef,
            ModelObjectReference structuralInstanceRef,
            ModelObjectReference variableInstanceRef)
            : this(projectGuid, revisionNumber, modelTemplateRef, structuralTypeRef, null, variableTemplateRef, modelInstanceRef, structuralInstanceRef, null, variableInstanceRef, null, null)
        { }

        public CurrentState(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef,
            ModelObjectReference structuralTypeRef, StructuralSpace structuralSpace,
            ModelObjectReference variableTemplateRef, ModelObjectReference modelInstanceRef,
            ModelObjectReference structuralInstanceRef, StructuralPoint structuralPoint,
            ModelObjectReference variableInstanceRef)
            : this(projectGuid, revisionNumber, modelTemplateRef, structuralTypeRef, structuralSpace, variableTemplateRef, modelInstanceRef, structuralInstanceRef, structuralPoint, variableInstanceRef, null, null)
        { }

        protected CurrentState(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef,
            Nullable<ModelObjectReference> structuralTypeRef, Nullable<StructuralSpace> structuralSpace,
            Nullable<ModelObjectReference> variableTemplateRef, Nullable<ModelObjectReference> modelInstanceRef,
            Nullable<ModelObjectReference> structuralInstanceRef, Nullable<StructuralPoint> structuralPoint,
            Nullable<ModelObjectReference> variableInstanceRef, Nullable<DateTime> modelStartDate, Nullable<DateTime> modelEndDate)
        {
            m_ValidityArea = variableTemplateRef.HasValue ? OperationValidityType.Variable : OperationValidityType.Model;

            m_ProjectGuid = projectGuid;
            m_RevisionNumber = revisionNumber;
            m_ModelTemplateRef = modelTemplateRef;
            m_StructuralTypeRef = structuralTypeRef;
            m_StructuralSpace = structuralSpace;
            m_VariableTemplateRef = variableTemplateRef;
            m_ModelInstanceRef = modelInstanceRef;
            m_StructuralInstanceRef = structuralInstanceRef;
            m_StructuralPoint = structuralPoint;
            m_VariableInstanceRef = variableInstanceRef;

            m_PrimaryPeriodType = null;
            m_SecondaryPeriodType = null;

            m_ComputeByPeriod = false;
            m_PrimaryPeriod = null;
            m_SecondaryPeriod = null;

            m_NavigationPeriod = null;
            m_ModelStartDate = modelStartDate.HasValue ? modelStartDate.Value : DefaultModelStartDate;
            m_ModelEndDate = modelEndDate.HasValue ? modelEndDate.Value : DefaultModelEndDate;

            m_ParentComputationGroup = null;
        }

        public CurrentState(ICurrentState otherCurrentState)
        {
            m_ValidityArea = otherCurrentState.ValidityArea;

            m_ProjectGuid = otherCurrentState.ProjectGuid;
            m_RevisionNumber = otherCurrentState.RevisionNumber;
            m_ModelTemplateRef = otherCurrentState.ModelTemplateRef;
            m_StructuralTypeRef = otherCurrentState.StructuralTypeRef;
            m_StructuralSpace = otherCurrentState.NullableStructuralSpace;
            m_VariableTemplateRef = otherCurrentState.VariableTemplateRef;
            m_ModelInstanceRef = otherCurrentState.NullableModelInstanceRef;
            m_StructuralInstanceRef = otherCurrentState.NullableStructuralInstanceRef;
            m_StructuralPoint = otherCurrentState.NullableStructuralPoint;
            m_VariableInstanceRef = otherCurrentState.NullableVariableInstanceRef;

            m_PrimaryPeriodType = otherCurrentState.PrimaryPeriodType;
            m_SecondaryPeriodType = otherCurrentState.SecondaryPeriodType;

            m_ComputeByPeriod = otherCurrentState.ComputeByPeriod;
            m_PrimaryPeriod = otherCurrentState.PrimaryPeriod;
            m_SecondaryPeriod = otherCurrentState.SecondaryPeriod;

            m_NavigationPeriod = otherCurrentState.NavigationPeriod;
            m_ModelStartDate = otherCurrentState.ModelStartDate;
            m_ModelEndDate = otherCurrentState.ModelEndDate;

            m_ParentComputationGroup = otherCurrentState.ParentComputationGroup;
            DebugHelperText = otherCurrentState.DebugHelperText;
        }

        public CurrentState(ICurrentState otherCurrentState, ModelObjectReference structuralTypeRef, ModelObjectReference variableTemplateRef)
        {
            m_ValidityArea = otherCurrentState.ValidityArea;

            m_ProjectGuid = otherCurrentState.ProjectGuid;
            m_RevisionNumber = otherCurrentState.RevisionNumber;
            m_ModelTemplateRef = otherCurrentState.ModelTemplateRef;
            m_StructuralTypeRef = structuralTypeRef;
            m_StructuralSpace = null;
            m_VariableTemplateRef = variableTemplateRef;
            m_ModelInstanceRef = null;
            m_StructuralInstanceRef = null;
            m_StructuralPoint = null;
            m_VariableInstanceRef = null;

            m_PrimaryPeriodType = otherCurrentState.PrimaryPeriodType;
            m_SecondaryPeriodType = otherCurrentState.SecondaryPeriodType;

            m_ComputeByPeriod = otherCurrentState.ComputeByPeriod;
            m_PrimaryPeriod = otherCurrentState.PrimaryPeriod;
            m_SecondaryPeriod = otherCurrentState.SecondaryPeriod;

            m_NavigationPeriod = otherCurrentState.NavigationPeriod;
            m_ModelStartDate = otherCurrentState.ModelStartDate;
            m_ModelEndDate = otherCurrentState.ModelEndDate;

            m_ParentComputationGroup = otherCurrentState.ParentComputationGroup;
            DebugHelperText = otherCurrentState.DebugHelperText;
        }

        public CurrentState(ICurrentState otherCurrentState, ModelObjectReference structuralTypeRef, ModelObjectReference variableTemplateRef, ModelObjectReference structuralInstanceRef, ModelObjectReference variableInstanceRef)
            : this(otherCurrentState, structuralTypeRef, null, variableTemplateRef, structuralInstanceRef, null, variableInstanceRef)
        { }

        public CurrentState(ICurrentState otherCurrentState, ModelObjectReference structuralTypeRef, Nullable<StructuralSpace> structuralSpace, ModelObjectReference variableTemplateRef, ModelObjectReference structuralInstanceRef, Nullable<StructuralPoint> structuralPoint, ModelObjectReference variableInstanceRef)
        {
            m_ValidityArea = otherCurrentState.ValidityArea;

            m_ProjectGuid = otherCurrentState.ProjectGuid;
            m_RevisionNumber = otherCurrentState.RevisionNumber;
            m_ModelTemplateRef = otherCurrentState.ModelTemplateRef;
            m_StructuralTypeRef = structuralTypeRef;
            m_StructuralSpace = structuralSpace;
            m_VariableTemplateRef = variableTemplateRef;
            m_ModelInstanceRef = otherCurrentState.ModelInstanceRef;
            m_StructuralInstanceRef = structuralInstanceRef;
            m_StructuralPoint = structuralPoint;
            m_VariableInstanceRef = variableInstanceRef;

            m_PrimaryPeriodType = otherCurrentState.PrimaryPeriodType;
            m_SecondaryPeriodType = otherCurrentState.SecondaryPeriodType;

            m_ComputeByPeriod = otherCurrentState.ComputeByPeriod;
            m_PrimaryPeriod = otherCurrentState.PrimaryPeriod;
            m_SecondaryPeriod = otherCurrentState.SecondaryPeriod;

            m_NavigationPeriod = otherCurrentState.NavigationPeriod;
            m_ModelStartDate = otherCurrentState.ModelStartDate;
            m_ModelEndDate = otherCurrentState.ModelEndDate;

            m_ParentComputationGroup = otherCurrentState.ParentComputationGroup;
            DebugHelperText = otherCurrentState.DebugHelperText;
        }

        public CurrentState(ICurrentState otherCurrentState, Nullable<TimePeriod> primaryPeriod, Nullable<TimePeriod> secondaryPeriod)
            : this(otherCurrentState, new MultiTimePeriodKey(primaryPeriod, secondaryPeriod))
        { }

        public CurrentState(ICurrentState otherCurrentState, MultiTimePeriodKey timeKey)
        {
            m_ValidityArea = otherCurrentState.ValidityArea;

            m_ProjectGuid = otherCurrentState.ProjectGuid;
            m_RevisionNumber = otherCurrentState.RevisionNumber;
            m_ModelTemplateRef = otherCurrentState.ModelTemplateRef;
            m_StructuralTypeRef = otherCurrentState.StructuralTypeRef;
            m_StructuralSpace = otherCurrentState.NullableStructuralSpace;
            m_VariableTemplateRef = otherCurrentState.VariableTemplateRef;
            m_ModelInstanceRef = otherCurrentState.NullableModelInstanceRef;
            m_StructuralInstanceRef = otherCurrentState.NullableStructuralInstanceRef;
            m_StructuralPoint = otherCurrentState.NullableStructuralPoint;
            m_VariableInstanceRef = otherCurrentState.NullableVariableInstanceRef;

            m_PrimaryPeriodType = otherCurrentState.PrimaryPeriodType;
            m_SecondaryPeriodType = otherCurrentState.SecondaryPeriodType;

            m_ComputeByPeriod = true;
            m_PrimaryPeriod = timeKey.NullablePrimaryTimePeriod;
            m_SecondaryPeriod = timeKey.NullableSecondaryTimePeriod;

            m_NavigationPeriod = null;
            m_ModelStartDate = otherCurrentState.ModelStartDate;
            m_ModelEndDate = otherCurrentState.ModelEndDate;

            m_ParentComputationGroup = otherCurrentState.ParentComputationGroup;
            DebugHelperText = otherCurrentState.DebugHelperText;
        }

        public ProcessingType ProcessingType
        {
            get { return (m_StructuralSpace.HasValue) ? ProcessingType.Anonymous : ProcessingType.Normal; }
        }

        public OperationValidityType ValidityArea
        {
            get { return m_ValidityArea; }
            set { m_ValidityArea = value; }
        }

        public bool UseExtendedStructure
        {
            get { return DefaultUseExtendedStructure; }
        }

        public ProcessingAcivityType AcivityType
        {
            get
            {
                if (!m_ModelInstanceRef.HasValue)
                { return ProcessingAcivityType.Validation; }
                return ProcessingAcivityType.Computation;
            }
        }

        public Guid ProjectGuid
        {
            get { return m_ProjectGuid; }
        }

        public long RevisionNumber
        {
            get { return m_RevisionNumber; }
        }

        public bool IsModelLevel
        {
            get { return !m_StructuralTypeRef.HasValue; }
        }

        public ModelObjectReference ModelTemplateRef
        {
            get { return m_ModelTemplateRef; }
        }

        public ModelObjectReference StructuralTypeRef
        {
            get { return m_StructuralTypeRef.Value; }
        }

        public StructuralSpace StructuralSpace
        {
            get { return m_StructuralSpace.Value; }
        }

        public ModelObjectReference VariableTemplateRef
        {
            get { return m_VariableTemplateRef.Value; }
        }

        public ModelObjectReference ModelInstanceRef
        {
            get { return m_ModelInstanceRef.Value; }
        }

        public ModelObjectReference StructuralInstanceRef
        {
            get { return m_StructuralInstanceRef.Value; }
        }

        public StructuralPoint StructuralPoint
        {
            get { return m_StructuralPoint.Value; }
        }

        public ModelObjectReference VariableInstanceRef
        {
            get { return m_VariableInstanceRef.Value; }
        }

        public Nullable<StructuralSpace> NullableStructuralSpace
        {
            get { return m_StructuralSpace; }
        }

        public Nullable<ModelObjectReference> NullableModelInstanceRef
        {
            get { return m_ModelInstanceRef; }
        }

        public Nullable<ModelObjectReference> NullableStructuralInstanceRef
        {
            get { return m_StructuralInstanceRef; }
        }

        public Nullable<StructuralPoint> NullableStructuralPoint
        {
            get { return m_StructuralPoint; }
        }

        public Nullable<ModelObjectReference> NullableVariableInstanceRef
        {
            get { return m_VariableInstanceRef; }
        }

        public Nullable<TimePeriodType> PrimaryPeriodType
        {
            get { return m_PrimaryPeriodType; }
        }

        public Nullable<TimePeriodType> SecondaryPeriodType
        {
            get { return m_SecondaryPeriodType; }
        }

        public bool ComputeByPeriod
        {
            get { return m_ComputeByPeriod; }
        }

        public Nullable<TimePeriod> PrimaryPeriod
        {
            get { return m_PrimaryPeriod; }
        }

        public Nullable<TimePeriod> SecondaryPeriod
        {
            get { return m_SecondaryPeriod; }
        }

        public MultiTimePeriodKey TimeKey
        {
            get { return new MultiTimePeriodKey(m_PrimaryPeriod, m_SecondaryPeriod); }
        }

        public Nullable<TimePeriod> NavigationPeriod
        {
            get { return m_NavigationPeriod; }
            set { m_NavigationPeriod = value; }
        }

        public DateTime ModelStartDate
        {
            get { return m_ModelStartDate; }
            set { m_ModelStartDate = value; }
        }

        public DateTime ModelEndDate
        {
            get { return m_ModelEndDate; }
            set { m_ModelEndDate = value; }
        }

        public void SetToComputeSpecificTimePeriods(Nullable<TimePeriod> primaryPeriod, Nullable<TimePeriod> secondaryPeriod)
        {
            m_ComputeByPeriod = true;
            m_PrimaryPeriod = primaryPeriod;
            m_SecondaryPeriod = secondaryPeriod;
        }

        public void SetToTransformTimeframe(TimePeriodType? primaryTimePeriodType, TimePeriodType? secondaryTimePeriodType)
        {
            m_PrimaryPeriodType = primaryTimePeriodType;
            m_SecondaryPeriodType = secondaryTimePeriodType;
        }

        public void SetToComputeEntireTimeframe()
        {
            m_PrimaryPeriodType = null;
            m_SecondaryPeriodType = null;

            m_ComputeByPeriod = false;
            m_PrimaryPeriod = null;
            m_SecondaryPeriod = null;
        }

        public bool HasParentComputationGroup
        {
            get { return (m_ParentComputationGroup != null); }
        }

        public IComputationGroup ParentComputationGroup
        {
            get { return m_ParentComputationGroup; }
            set { m_ParentComputationGroup = value; }
        }

        public ICollection<StructuralDimension> StructuralAggregationDimensions
        {
            get
            {
                ProcessingType.AssertIsAnonymous();
                return m_StructuralAggregationDimensions.ToHashSet();
            }
        }

        public ICollection<TimeDimensionType> TimeAggregationDimensions
        {
            get
            {
                ProcessingType.AssertIsAnonymous();
                return m_TimeAggregationDimensions.ToHashSet();
            }
        }

        public void AddStructuralAggregationDimension(StructuralDimension structuralDimension)
        {
            ProcessingType.AssertIsAnonymous();
            if (m_StructuralAggregationDimensions.Contains(structuralDimension))
            { throw new InvalidOperationException("The StructuralDimension was already added."); }

            m_StructuralAggregationDimensions.Add(structuralDimension);
        }

        public void AddTimeAggregationDimension(TimeDimensionType timeDimensionType)
        {
            ProcessingType.AssertIsAnonymous();
            if (m_TimeAggregationDimensions.Contains(timeDimensionType))
            { throw new InvalidOperationException("The TimeDimensionType was already added."); }

            m_TimeAggregationDimensions.Add(timeDimensionType);
        }

        public string DebugHelperText { get; set; }
    }
}