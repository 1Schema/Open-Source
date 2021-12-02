using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Structure
{
    public class JoinStep
    {
        #region Constants

        public const bool Default_RequireUsage = false;
        public const int Global_JoinOrder = -1;
        public const int Start_JoinOrder = 0;

        #endregion

        #region Constructors

        public JoinStep(int joinOrder, ModelObjectReference structuralTypeRef, Dictionary<StructuralDimension, JoinedDimensionValue> joinedDimensionValues)
            : this(joinOrder, structuralTypeRef, joinedDimensionValues, null)
        { }

        public JoinStep(int joinOrder, ModelObjectReference structuralTypeRef, Dictionary<StructuralDimension, JoinedDimensionValue> joinedDimensionValues, JoinStep previousJoinStep)
        {
            JoinOrder = joinOrder;
            StructuralTypeRef = structuralTypeRef;
            JoinedDimensionValues = joinedDimensionValues;

            PreviousJoinStep = previousJoinStep;
            DependentJoinSteps = new List<JoinStep>();
            IsUsed = false;

            if (PreviousJoinStep != null)
            { PreviousJoinStep.DependentJoinSteps.Add(this); }
        }

        #endregion

        #region Properties

        public int JoinOrder { get; protected set; }
        public ModelObjectReference StructuralTypeRef { get; protected set; }
        public Dictionary<StructuralDimension, JoinedDimensionValue> JoinedDimensionValues { get; protected set; }

        public JoinStep PreviousJoinStep { get; protected set; }
        public List<JoinStep> DependentJoinSteps { get; protected set; }
        public bool IsUsed { get; set; }
        public bool IsUsed_Or_DependentIsUsed { get { return (IsUsed || DependentJoinSteps.Any(x => x.IsUsed_Or_DependentIsUsed)); } }

        public Dictionary<ModelObjectReference, JoinedDimensionValue> JoinedRefValues { get { return JoinedDimensionValues.ToDictionary(x => x.Key.EntityTypeRefWithDimNum, x => x.Value, ModelObjectReference.DimensionalComparer); } }
        public Dictionary<ModelObjectReference, JoinedDimensionValue> JoinedRefValues_ToCreate { get { return JoinedRefValues.Where(x => CreatesJoinSetting(x.Key)).ToDictionary(x => x.Key, x => x.Value, ModelObjectReference.DimensionalComparer); } }
        public Dictionary<ModelObjectReference, JoinedDimensionValue> JoinedRefValues_FromBase { get { return JoinedRefValues.Where(x => UsesBase_JoinSetting(x.Key)).ToDictionary(x => x.Key, x => x.Value, ModelObjectReference.DimensionalComparer); } }
        public Dictionary<ModelObjectReference, JoinedDimensionValue> JoinedRefValues_FromExpanded { get { return JoinedRefValues.Where(x => UsesExpanded_JoinSetting(x.Key)).ToDictionary(x => x.Key, x => x.Value, ModelObjectReference.DimensionalComparer); } }

        #endregion

        #region Methods

        public bool HasJoinSettingForRef(ModelObjectReference entityTypeRef)
        {
            if (entityTypeRef.ModelObjectType != ModelObjectType.EntityType)
            { return false; }

            return JoinedDimensionValues.ContainsKey(entityTypeRef.ToStructuralDimension());
        }

        public bool OnlyCreatesJoinSettings()
        {
            if (JoinedDimensionValues.Count < 1)
            { return false; }

            foreach (var dimension in JoinedDimensionValues.Keys)
            {
                var structuralTypeRef = dimension.EntityTypeRefWithDimNum;

                if (!CreatesJoinSetting(structuralTypeRef))
                { return false; }
            }
            return true;
        }

        public bool CreatesJoinSetting(ModelObjectReference entityTypeRef)
        {
            return MatchesJoinSettingForRef(entityTypeRef, false, false);
        }

        public bool UsesBase_JoinSetting(ModelObjectReference entityTypeRef)
        {
            return MatchesJoinSettingForRef(entityTypeRef, true, true);
        }

        public bool UsesExpanded_JoinSetting(ModelObjectReference entityTypeRef)
        {
            return MatchesJoinSettingForRef(entityTypeRef, true, false);
        }

        protected bool MatchesJoinSettingForRef(ModelObjectReference entityTypeRef, bool requiresJoin, bool joinsToMainTable)
        {
            if (!HasJoinSettingForRef(entityTypeRef))
            { return false; }

            var joinedValue = JoinedDimensionValues[entityTypeRef.ToStructuralDimension()];
            if (joinedValue.RequiresDimensionalJoin != requiresJoin)
            { return false; }

            if (!requiresJoin)
            { return true; }

            if ((StructuralTypeRef.ModelObjectType != ModelObjectType.RelationType) && (joinedValue.UseExistingJoinSetting != joinsToMainTable))
            { return false; }
            if ((StructuralTypeRef.ModelObjectType == ModelObjectType.RelationType) && (!joinsToMainTable))
            { return true; }

            if (joinsToMainTable)
            { return true; }

            if (!joinedValue.ExtendedJoinPath.Contains(joinedValue.KeySourceRef, ModelObjectReference.DimensionalComparer))
            { return false; }
            if (joinedValue.KeySourceRef.ModelObjectType != ModelObjectType.RelationType)
            {
                if (!joinedValue.ExtendedJoinPath.Contains(joinedValue.LocalRef, ModelObjectReference.DimensionalComparer))
                { return false; }
            }
            return true;
        }

        #endregion

        #region Static Methods

        public static bool IsRefAlreadyHandled(ModelObjectReference desiredStructuralTypeRef, JoinStep rootStep)
        {
            if (rootStep == null)
            { return false; }

            if (ModelObjectReference.DimensionalComparer.Equals(desiredStructuralTypeRef, rootStep.StructuralTypeRef))
            { return true; }

            return IsRefAlreadyHandled(desiredStructuralTypeRef, rootStep.PreviousJoinStep);
        }

        public static List<JoinStep> GetAsList_DepthFirst(JoinStep rootStep)
        {
            return GetAsList_DepthFirst(rootStep, Default_RequireUsage);
        }

        public static List<JoinStep> GetAsList_DepthFirst(JoinStep rootStep, bool requireUsage)
        {
            var steps = new List<JoinStep>();
            if (rootStep == null)
            { return steps; }

            if (!requireUsage || (requireUsage && rootStep.IsUsed_Or_DependentIsUsed))
            { steps.Add(rootStep); }

            foreach (var nestedStep in rootStep.DependentJoinSteps.OrderBy(x => x.JoinOrder).ToList())
            {
                var nestedSteps = GetAsList_DepthFirst(nestedStep, requireUsage);
                steps.AddRange(nestedSteps);
            }
            return steps;
        }

        public static List<JoinStep> GetAsList_BreadthFirst(JoinStep rootStep)
        {
            return GetAsList_BreadthFirst(rootStep, Default_RequireUsage);
        }

        public static List<JoinStep> GetAsList_BreadthFirst(JoinStep rootStep, bool requireUsage)
        {
            var joinStepsByLevel = new SortedDictionary<int, List<JoinStep>>();
            joinStepsByLevel.Add(0, new List<JoinStep>());
            joinStepsByLevel[0].Add(rootStep);
            GetAsList_BreadthFirst_Helper(joinStepsByLevel, 1, rootStep, requireUsage);

            var steps = new List<JoinStep>();
            foreach (var bucket in joinStepsByLevel)
            {
                steps.AddRange(bucket.Value);
            }
            return steps;
        }

        private static void GetAsList_BreadthFirst_Helper(IDictionary<int, List<JoinStep>> joinStepsByLevel, int currentLevel, JoinStep currentStep, bool requireUsage)
        {
            if (currentStep == null)
            { return; }

            if (!joinStepsByLevel.ContainsKey(currentLevel))
            { joinStepsByLevel.Add(currentLevel, new List<JoinStep>()); }

            var steps = currentStep.DependentJoinSteps.Where(x => (!requireUsage || (requireUsage && x.IsUsed_Or_DependentIsUsed))).OrderBy(x => x.JoinOrder).ToList();
            joinStepsByLevel[currentLevel].AddRange(steps);

            foreach (var nestedStep in steps)
            {
                GetAsList_BreadthFirst_Helper(joinStepsByLevel, currentLevel + 1, nestedStep, requireUsage);
            }
        }

        #endregion
    }
}