using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Structure;

namespace Decia.Business.Domain.Reporting.Dimensionality
{
    public class DimensionalRepeatGroup
    {
        #region Static Members

        public const int InitialGroupNumber = 1;
        private static int CurrentGroupNumber = InitialGroupNumber;

        public static readonly string InitialGroupName = string.Empty;
        public static readonly string DefaultGroupNamePrefix = "[Default]";
        public static readonly string GroupNameCountSymbol = "-";
        public static readonly string DefaultGroupNameBase = DefaultGroupNamePrefix + GroupNameCountSymbol;
        public static readonly string DefaultGroupNameFormat = DefaultGroupNameBase + "{0}";
        public static readonly int DefaultGroupNameStartNumber = 1;
        public static readonly char DefaultGroupNameSeparator = '.';
        public static readonly char[] ValidGroupNameSeparators = new char[] { DefaultGroupNameSeparator };

        #endregion

        #region Static Methods

        public static int GetGroupNumber()
        {
            return CurrentGroupNumber++;
        }

        public static string GetDefaultGroupNameForType(Type type)
        {
            return GetDefaultGroupNameForType(type, new List<string>());
        }

        public static string GetDefaultGroupNameForType(Type type, IEnumerable<string> siblingGroupNames)
        {
            var typeName = type.Name;
            return GetDefaultGroupName(typeName, siblingGroupNames);
        }

        public static string GetDefaultGroupName(string parentObjectName)
        {
            return GetDefaultGroupName(parentObjectName, new List<string>());
        }

        public static string GetDefaultGroupName(string parentObjectName, IEnumerable<string> siblingGroupNames)
        {
            return GetGroupName(parentObjectName, DefaultGroupNamePrefix, siblingGroupNames);
        }

        public static string GetGroupName(string parentObjectName, string specifiedGroupName)
        {
            return GetGroupName(parentObjectName, specifiedGroupName, new List<string>());
        }

        public static string GetGroupName(string parentObjectName, string specifiedGroupName, IEnumerable<string> siblingGroupNames)
        {
            var groupNameBase = parentObjectName + DefaultGroupNameSeparator + specifiedGroupName + GroupNameCountSymbol;
            var groupNameFormat = groupNameBase + "{0}";

            var likeOccurrenceCount = siblingGroupNames.Where(x => x.Contains(groupNameBase)).Count();

            var nextNumber = (likeOccurrenceCount <= 0) ? DefaultGroupNameStartNumber : likeOccurrenceCount + 1;
            var nextName = string.Format(groupNameFormat, nextNumber);

            while (siblingGroupNames.Contains(nextName))
            {
                nextNumber++;
                nextName = string.Format(groupNameFormat, nextNumber);
            }
            return nextName;
        }

        #endregion

        #region Members

        protected Guid m_Id;
        protected string m_Name;
        protected int m_Number;
        protected Dictionary<ReportElementId, IReportElement> m_GroupedElements;

        protected Nullable<Guid> m_ParentRepeatGroupId;
        protected Dictionary<Guid, DimensionalRepeatGroup> m_NestedRepeatGroups;

        protected List<ModelObjectReference> m_AdditionalStructuralTypeRefs;
        protected Dictionary<TimeDimensionType, Nullable<TimePeriodType>> m_RelevantTimeDimensions;
        protected List<ModelObjectReference> m_GroupedStructuralTypeRefs;

        protected ICurrentState m_DesignState;
        protected IStructuralContext m_DesignContext;

        protected ICurrentState m_ProductionState;
        protected Dictionary<ModelObjectReference, Nullable<ModelObjectReference>> m_AdditionalStructuralTypeBindings;
        protected Dictionary<TimePeriod, IStructuralContext> m_ProductionContexts;

        #endregion

        #region Events

        public event EventHandler<AddingNewEventArgs> ReportElementAdded;
        public event EventHandler<AddingNewEventArgs> NestedGroupAdded;

        #endregion

        #region Constructors

        public DimensionalRepeatGroup(string name, ICurrentState designState, IStructuralContext designContext)
            : this(name, designState, designContext, new List<ModelObjectReference>(), new Dictionary<TimeDimensionType, Nullable<TimePeriodType>>())
        { }

        public DimensionalRepeatGroup(string name, ICurrentState designState, IStructuralContext designContext, IEnumerable<ModelObjectReference> additionalStructuralTypeRefs)
            : this(name, designState, designContext, additionalStructuralTypeRefs, new Dictionary<TimeDimensionType, Nullable<TimePeriodType>>())
        { }

        public DimensionalRepeatGroup(string name, ICurrentState designState, IStructuralContext designContext, IDictionary<TimeDimensionType, Nullable<TimePeriodType>> relevantTimeDimensions)
            : this(name, designState, designContext, new List<ModelObjectReference>(), relevantTimeDimensions)
        { }

        public DimensionalRepeatGroup(string name, ICurrentState designState, IStructuralContext designContext, IEnumerable<ModelObjectReference> additionalStructuralTypeRefs, IDictionary<TimeDimensionType, Nullable<TimePeriodType>> relevantTimeDimensions)
            : this(name, designState, designContext, additionalStructuralTypeRefs, relevantTimeDimensions, new List<ModelObjectReference>())
        { }

        public DimensionalRepeatGroup(string name, ICurrentState designState, IStructuralContext designContext, IEnumerable<ModelObjectReference> additionalStructuralTypeRefs, IDictionary<TimeDimensionType, Nullable<TimePeriodType>> relevantTimeDimensions, IEnumerable<ModelObjectReference> groupedStructuralTypeRefs)
        {
            if ((designState == null) || (designContext == null) || (additionalStructuralTypeRefs == null) || (relevantTimeDimensions == null))
            { throw new InvalidOperationException("Null construction parameters are not allowed for StructuralGroups."); }

            m_Id = Guid.NewGuid();
            m_Name = name;
            m_Number = GetGroupNumber();
            m_GroupedElements = new Dictionary<ReportElementId, IReportElement>(ReportRenderingEngine.EqualityComparer_ReportElementId);

            m_ParentRepeatGroupId = null;
            m_NestedRepeatGroups = new Dictionary<Guid, DimensionalRepeatGroup>();

            m_AdditionalStructuralTypeRefs = new List<ModelObjectReference>(additionalStructuralTypeRefs);
            m_RelevantTimeDimensions = new Dictionary<TimeDimensionType, TimePeriodType?>(relevantTimeDimensions);

            var uniqueGroupedStructuralTypeRefs = new HashSet<ModelObjectReference>(groupedStructuralTypeRefs, ModelObjectReference.DimensionalComparer);
            m_GroupedStructuralTypeRefs = new List<ModelObjectReference>(uniqueGroupedStructuralTypeRefs);

            m_DesignState = designState;
            m_DesignContext = designContext;

            m_ProductionState = null;
            m_AdditionalStructuralTypeBindings = null;
            m_ProductionContexts = null;
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_Id; }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public int Number
        {
            get { return m_Number; }
        }

        public ICollection<ReportElementId> GroupedElementIds
        {
            get { return m_GroupedElements.Keys.ToList(); }
        }

        public IDictionary<ReportElementId, IReportElement> GroupedElements
        {
            get { return m_GroupedElements.ToDictionary(x => x.Key, x => x.Value, ReportRenderingEngine.EqualityComparer_ReportElementId); }
        }

        public Nullable<Guid> ParentRepeatGroupId
        {
            get { return m_ParentRepeatGroupId; }
            set
            {
                if (m_ParentRepeatGroupId.HasValue)
                { throw new InvalidOperationException("The Parent Repeat Group has already been set."); }

                m_ParentRepeatGroupId = value;
            }
        }

        public ICollection<Guid> NestedRepeatGroupIds
        {
            get { return m_NestedRepeatGroups.Keys.ToList(); }
        }

        public IDictionary<Guid, DimensionalRepeatGroup> NestedRepeatGroups
        {
            get { return m_NestedRepeatGroups.ToDictionary(x => x.Key, x => x.Value); }
        }

        public RenderingMode RenderingMode
        {
            get { return ((m_ProductionState == null) || (m_ProductionContexts == null)) ? RenderingMode.Design : RenderingMode.Production; }
        }

        public ICollection<ModelObjectReference> AdditionalStructuralTypeRefs
        {
            get { return m_AdditionalStructuralTypeRefs.ToList(); }
        }

        public IDictionary<TimeDimensionType, Nullable<TimePeriodType>> RelevantTimeDimensions
        {
            get { return m_RelevantTimeDimensions.ToDictionary(x => x.Key, x => x.Value); }
        }

        public ICollection<ModelObjectReference> GroupedStructuralTypeRefs
        {
            get { return m_GroupedStructuralTypeRefs.ToList(); }
        }

        public ICurrentState DesignState
        {
            get { return m_DesignState; }
        }

        public IStructuralContext DesignContext
        {
            get { return m_DesignContext; }
        }

        public ICurrentState ProductionState
        {
            get
            {
                if (RenderingMode == RenderingMode.Design)
                { throw new InvalidOperationException("Cannot access ProductionState when in Design Mode."); }

                return m_ProductionState;
            }
        }

        public IDictionary<TimePeriod, IStructuralContext> ProductionContexts
        {
            get
            {
                if (RenderingMode == RenderingMode.Design)
                { throw new InvalidOperationException("Cannot access ProductionContext when in Design Mode."); }

                return m_ProductionContexts.ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public IDictionary<ModelObjectReference, bool> AdditionalStructuralTypeHasBinding
        {
            get { return m_AdditionalStructuralTypeBindings.ToDictionary(x => x.Key, x => x.Value.HasValue); }
        }

        public IDictionary<ModelObjectReference, Nullable<ModelObjectReference>> AdditionalStructuralTypeBindings
        {
            get { return m_AdditionalStructuralTypeBindings.ToDictionary(x => x.Key, x => x.Value); }
        }

        public MultiTimePeriodKey RelevantTimeBindings
        {
            get { return m_ProductionState.TimeKey; }
        }

        #endregion

        #region Methods

        public bool Get_IsGlobalGroup(IReportingDataProvider dataProvider)
        {
            foreach (var element in m_GroupedElements.Values)
            {
                if (!(element is VariableTitleRange))
                { continue; }

                var titleRange = (element as VariableTitleRange);
                var variableTemplateRef = titleRange.NameVariableTemplateRef;
                var structuralTypeRef = dataProvider.DependencyMap.GetStructuralType(variableTemplateRef);

                if (structuralTypeRef.ModelObjectType != ModelObjectType.GlobalType)
                { return false; }
            }
            return true;
        }

        public void AddElementToGroup(IReportElement element)
        {
            if (m_GroupedElements.ContainsKey(element.Key))
            { throw new InvalidOperationException("The specified Report Element has already been added."); }

            m_GroupedElements.Add(element.Key, element);

            if (ReportElementAdded != null)
            {
                var args = new AddingNewEventArgs(element);
                ReportElementAdded(this, args);
            }
        }

        public void AddNestedRepeatGroup(DimensionalRepeatGroup repeatGroup)
        {
            if (m_NestedRepeatGroups.ContainsKey(repeatGroup.Id))
            { throw new InvalidOperationException("The specified Repeat Group has already been added."); }

            repeatGroup.ParentRepeatGroupId = this.Id;
            m_NestedRepeatGroups.Add(repeatGroup.Id, repeatGroup);

            if (NestedGroupAdded != null)
            {
                var args = new AddingNewEventArgs(repeatGroup);
                NestedGroupAdded(this, args);
            }
        }

        public void SetProductionState(ICurrentState productionState)
        {
            var productionContexts = new Dictionary<TimePeriod, IStructuralContext>();
            SetProductionState(productionState, productionContexts);
        }

        public void SetProductionState(ICurrentState productionState, IDictionary<TimePeriod, IStructuralContext> productionContexts)
        {
            var additionalStructuralTypeBindings = new Dictionary<ModelObjectReference, Nullable<ModelObjectReference>>();
            SetProductionState(productionState, additionalStructuralTypeBindings, productionContexts);
        }

        public void SetProductionState(ICurrentState productionState, IDictionary<ModelObjectReference, Nullable<ModelObjectReference>> additionalStructuralTypeBindings, IDictionary<TimePeriod, IStructuralContext> productionContexts)
        {
            if ((productionState == null) || (additionalStructuralTypeBindings == null) || (productionContexts == null))
            { throw new InvalidOperationException("Null construction parameters are not allowed for StructuralGroups."); }

            if (!productionState.NullableModelInstanceRef.HasValue)
            { throw new InvalidOperationException("The Production State must pertain to a Model Instance."); }

            m_ProductionState = productionState;
            m_AdditionalStructuralTypeBindings = additionalStructuralTypeBindings.ToDictionary(x => x.Key, x => x.Value);
            m_ProductionContexts = productionContexts.ToDictionary(x => x.Key, x => x.Value);
        }

        #endregion

        #region Base Class Overrides

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}