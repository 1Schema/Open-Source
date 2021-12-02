using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.Formulas.Operations.Logic;
using Decia.Business.Domain.Formulas.Operations.Math;
using Decia.Business.Domain.Formulas.Operations.Metadata;
using Decia.Business.Domain.Formulas.Operations.Queries;
using Decia.Business.Domain.Formulas.Operations.Structure;
using Decia.Business.Domain.Formulas.Operations.Text;
using Decia.Business.Domain.Formulas.Operations.Time.Aggregation;
using Decia.Business.Domain.Formulas.Operations.Time.Introspection;
using Decia.Business.Domain.Formulas.Operations.Time.Search;

namespace Decia.Business.Domain.Formulas.Operations
{
    public static class OperationCatalog
    {
        public const string OperatorArgCount_Format = "{0},{1}";

        private static Dictionary<string, Guid> m_OperationTypeNameGuids;
        private static Dictionary<Guid, Guid> m_BackwardsCompatibilityGuids;
        private static Dictionary<string, Guid> m_OperationShortNamesWithGuids;
        private static Dictionary<string, Guid> m_OperatorArgCountsWithGuids;
        private static Dictionary<OperationId, IOperation> m_Operations;

        static OperationCatalog()
        {
            LoadOperationGuids();
            LoadOperations();
        }

        private static void LoadOperationGuids()
        {
            m_OperationTypeNameGuids = new Dictionary<string, Guid>();

            m_OperationTypeNameGuids.Add(GetNameForType<IdentityOperation>(), new Guid("00000000-0000-0000-0000-000000000000"));
            m_OperationTypeNameGuids.Add(GetNameForType<NoOpOperation>(), new Guid("00000000-0000-0000-0000-000000000001"));
            m_OperationTypeNameGuids.Add(GetNameForType<GetStructuralTypeNameOperation>(), new Guid("00000000-0000-0000-0000-000000000005"));
            m_OperationTypeNameGuids.Add(GetNameForType<GetVariableTemplateNameOperation>(), new Guid("00000000-0000-0000-0000-000000000006"));
            m_OperationTypeNameGuids.Add(GetNameForType<GetTimeDimensionTypeNameOperation>(), new Guid("00000000-0000-0000-0000-000000000007"));
            m_OperationTypeNameGuids.Add(GetNameForType<GetTimePeriodTypeNameOperation>(), new Guid("00000000-0000-0000-0000-000000000008"));

            m_OperationTypeNameGuids.Add(GetNameForType<StructuralSumOperation>(), new Guid("00000001-0000-0000-0000-000000000001"));
            m_OperationTypeNameGuids.Add(GetNameForType<StructuralFirstOperation>(), new Guid("00000001-0000-0000-0000-000000000002"));
            m_OperationTypeNameGuids.Add(GetNameForType<StructuralLastOperation>(), new Guid("00000001-0000-0000-0000-000000000003"));

            m_OperationTypeNameGuids.Add(GetNameForType<TimeSumOperation>(), new Guid("00000002-0000-0000-0000-000000000001"));
            m_OperationTypeNameGuids.Add(GetNameForType<PresentValueOperation>(), new Guid("00000002-0000-0000-0000-000000000002"));
            m_OperationTypeNameGuids.Add(GetNameForType<TimeSumIntoPeriodsOperation>(), new Guid("00000002-0000-0000-0000-000000000003"));
            m_OperationTypeNameGuids.Add(GetNameForType<QueryOperation>(), new Guid("00000003-0000-0000-0000-000000000001"));

            m_OperationTypeNameGuids.Add(GetNameForType<Offset_ByPeriodCount_Operation>(), new Guid("00000004-0000-0000-0000-000000000001"));
            m_OperationTypeNameGuids.Add(GetNameForType<Offset_ByTimeSpan_Operation>(), new Guid("00000004-0000-0000-0000-000000000002"));
            m_OperationTypeNameGuids.Add(GetNameForType<Match_ByIndex_Operation>(), new Guid("00000004-0000-0000-0000-000000000003"));
            m_OperationTypeNameGuids.Add(GetNameForType<Match_ByDate_Operation>(), new Guid("00000004-0000-0000-0000-000000000004"));
            m_OperationTypeNameGuids.Add(GetNameForType<GetTimePeriodInstanceTextOperation>(), new Guid("00000004-0000-0000-0000-000000000005"));
            m_OperationTypeNameGuids.Add(GetNameForType<CurrentPeriodDurationInDaysOperation>(), new Guid("00000004-0000-0000-0000-000000000006"));
            m_OperationTypeNameGuids.Add(GetNameForType<CurrentPeriodDurationOperation>(), new Guid("00000004-0000-0000-0000-000000000007"));
            m_OperationTypeNameGuids.Add(GetNameForType<CurrentPeriodEndDateOperation>(), new Guid("00000004-0000-0000-0000-000000000008"));
            m_OperationTypeNameGuids.Add(GetNameForType<CurrentPeriodIndexOperation>(), new Guid("00000004-0000-0000-0000-000000000009"));
            m_OperationTypeNameGuids.Add(GetNameForType<CurrentPeriodStartDateOperation>(), new Guid("00000004-0000-0000-0000-00000000000a"));

            m_OperationTypeNameGuids.Add(GetNameForType<IfOperation>(), new Guid("00000007-0000-0000-0000-000000000001"));
            m_OperationTypeNameGuids.Add(GetNameForType<AndOperation>(), new Guid("00000007-0000-0000-0000-000000000002"));
            m_OperationTypeNameGuids.Add(GetNameForType<OrOperation>(), new Guid("00000007-0000-0000-0000-000000000003"));
            m_OperationTypeNameGuids.Add(GetNameForType<XorOperation>(), new Guid("00000007-0000-0000-0000-000000000004"));
            m_OperationTypeNameGuids.Add(GetNameForType<NotOperation>(), new Guid("00000007-0000-0000-0000-000000000005"));
            m_OperationTypeNameGuids.Add(GetNameForType<EqualsOperation>(), new Guid("00000007-0000-0000-0000-000000000006"));
            m_OperationTypeNameGuids.Add(GetNameForType<NotEqualsOperation>(), new Guid("00000007-0000-0000-0000-000000000007"));
            m_OperationTypeNameGuids.Add(GetNameForType<GreaterThanOperation>(), new Guid("00000007-0000-0000-0000-000000000008"));
            m_OperationTypeNameGuids.Add(GetNameForType<GreaterThanOrEqualToOperation>(), new Guid("00000007-0000-0000-0000-000000000009"));
            m_OperationTypeNameGuids.Add(GetNameForType<LessThanOperation>(), new Guid("00000007-0000-0000-0000-00000000000a"));
            m_OperationTypeNameGuids.Add(GetNameForType<LessThanOrEqualToOperation>(), new Guid("00000007-0000-0000-0000-00000000000b"));
            m_OperationTypeNameGuids.Add(GetNameForType<IsNullOperation>(), new Guid("00000007-0000-0000-0000-00000000000c"));
            m_OperationTypeNameGuids.Add(GetNameForType<IsNotNullOperation>(), new Guid("00000007-0000-0000-0000-00000000000d"));

            m_OperationTypeNameGuids.Add(GetNameForType<NegateOperation>(), new Guid("00000009-0000-0000-0000-000000000000"));
            m_OperationTypeNameGuids.Add(GetNameForType<AddOperation>(), new Guid("00000009-0000-0000-0000-000000000001"));
            m_OperationTypeNameGuids.Add(GetNameForType<SubtractOperation>(), new Guid("00000009-0000-0000-0000-000000000002"));
            m_OperationTypeNameGuids.Add(GetNameForType<MultiplyOperation>(), new Guid("00000009-0000-0000-0000-000000000003"));
            m_OperationTypeNameGuids.Add(GetNameForType<DivideOperation>(), new Guid("00000009-0000-0000-0000-000000000004"));
            m_OperationTypeNameGuids.Add(GetNameForType<ModuloOperation>(), new Guid("00000009-0000-0000-0000-000000000005"));
            m_OperationTypeNameGuids.Add(GetNameForType<ExponentiationOperation>(), new Guid("00000009-0000-0000-0000-000000000006"));

            m_OperationTypeNameGuids.Add(GetNameForType<FormatTextOperation>(), new Guid("00000010-0000-0000-0000-000000000001"));

            m_BackwardsCompatibilityGuids = new Dictionary<Guid, Guid>();
            m_BackwardsCompatibilityGuids.Add(new Guid("00000002-0000-0000-0000-000000000004"), m_OperationTypeNameGuids[GetNameForType<Match_ByDate_Operation>()]);
        }

        private static void LoadOperations()
        {
            List<IOperation> operations = new List<IOperation>();
            operations.Add(new IdentityOperation());
            operations.Add(new NoOpOperation());
            operations.Add(new GetStructuralTypeNameOperation());
            operations.Add(new GetVariableTemplateNameOperation());
            operations.Add(new GetTimeDimensionTypeNameOperation());
            operations.Add(new GetTimePeriodTypeNameOperation());

            operations.Add(new StructuralSumOperation());
            operations.Add(new StructuralFirstOperation());
            operations.Add(new StructuralLastOperation());

            operations.Add(new TimeSumOperation());
            operations.Add(new PresentValueOperation());
            operations.Add(new TimeSumIntoPeriodsOperation());
            operations.Add(new QueryOperation());

            operations.Add(new Offset_ByPeriodCount_Operation());
            operations.Add(new Offset_ByTimeSpan_Operation());
            operations.Add(new Match_ByIndex_Operation());
            operations.Add(new Match_ByDate_Operation());
            operations.Add(new CurrentPeriodDurationInDaysOperation());
            operations.Add(new CurrentPeriodDurationOperation());
            operations.Add(new CurrentPeriodEndDateOperation());
            operations.Add(new CurrentPeriodIndexOperation());
            operations.Add(new CurrentPeriodStartDateOperation());
            operations.Add(new GetTimePeriodInstanceTextOperation());

            operations.Add(new IfOperation());
            operations.Add(new AndOperation());
            operations.Add(new OrOperation());
            operations.Add(new XorOperation());
            operations.Add(new NotOperation());
            operations.Add(new EqualsOperation());
            operations.Add(new NotEqualsOperation());
            operations.Add(new GreaterThanOperation());
            operations.Add(new GreaterThanOrEqualToOperation());
            operations.Add(new LessThanOperation());
            operations.Add(new LessThanOrEqualToOperation());
            operations.Add(new IsNullOperation());
            operations.Add(new IsNotNullOperation());

            operations.Add(new NegateOperation());
            operations.Add(new AddOperation());
            operations.Add(new SubtractOperation());
            operations.Add(new MultiplyOperation());
            operations.Add(new DivideOperation());
            operations.Add(new ModuloOperation());
            operations.Add(new ExponentiationOperation());

            operations.Add(new FormatTextOperation());

            m_Operations = operations.ToDictionary(op => op.Id, op => op);
            m_OperationShortNamesWithGuids = new Dictionary<string, Guid>();
            m_OperatorArgCountsWithGuids = new Dictionary<string, Guid>();

            foreach (var operation in m_Operations.Values)
            {
                var operationGuid = operation.Id.OperationGuid;
                m_OperationShortNamesWithGuids.Add(operation.ShortName, operationGuid);

                if (string.IsNullOrWhiteSpace(operation.OperatorText))
                { continue; }

                var operatorTextArgCount = string.Format(OperatorArgCount_Format, operation.OperatorText, operation.SignatureSpecification.RequiredParameterCount);
                m_OperatorArgCountsWithGuids.Add(operatorTextArgCount, operationGuid);
            }
        }

        public static ICollection<Guid> OperationGuids
        {
            get { return m_Operations.Keys.Select(k => k.OperationGuid).ToList(); }
        }

        public static ICollection<OperationId> OperationIds
        {
            get { return m_Operations.Keys.ToList(); }
        }

        public static IDictionary<string, Guid> OperationGuidsByTypeName
        {
            get { return new Dictionary<string, Guid>(m_OperationTypeNameGuids); }
        }

        public static IDictionary<string, OperationId> OperationIdsByTypeName
        {
            get { return m_OperationTypeNameGuids.ToDictionary(kvp => kvp.Key, kvp => new OperationId(kvp.Value)); }
        }

        public static IDictionary<Guid, Guid> BackwardsCompatibilityGuids
        {
            get { return new Dictionary<Guid, Guid>(m_BackwardsCompatibilityGuids); }
        }

        public static IDictionary<OperationId, OperationId> BackwardsCompatibilityIds
        {
            get { return m_BackwardsCompatibilityGuids.ToDictionary(x => new OperationId(x.Key), x => new OperationId(x.Value)); }
        }

        public static IDictionary<OperationId, IOperation> Operations
        {
            get { return new Dictionary<OperationId, IOperation>(m_Operations); }
        }

        public static bool HasOperation(Guid typeGuid)
        {
            var typeId = new OperationId(typeGuid);
            return HasOperation(typeId);
        }

        public static bool HasOperation(OperationId typeId)
        {
            if (m_Operations.ContainsKey(typeId))
            { return true; }
            if (m_BackwardsCompatibilityGuids.ContainsKey(typeId.OperationGuid))
            { return true; }
            return false;
        }

        public static string GetNameForType<T>()
        {
            return GetNameForType(typeof(T));
        }

        public static string GetNameForType(Type type)
        {
            return type.FullName;
        }

        public static OperationId GetOperationId<T>()
        {
            return GetOperationId(typeof(T));
        }

        public static OperationId GetOperationId(Type type)
        {
            string typeName = GetNameForType(type);
            return GetOperationId(typeName);
        }

        public static OperationId GetOperationId(string typeName)
        {
            Guid guid = m_OperationTypeNameGuids[typeName];
            OperationId id = new OperationId(guid);
            return id;
        }

        public static IOperation GetOperation<T>()
        {
            return GetOperation(typeof(T));
        }

        public static IOperation GetOperation(Type type)
        {
            string typeName = GetNameForType(type);
            return GetOperation(typeName);
        }

        public static IOperation GetOperation(string typeName)
        {
            OperationId id = GetOperationId(typeName);
            return GetOperation(id);
        }

        public static IOperation GetOperation(Nullable<Guid> typeGuid)
        {
            var typeId = typeGuid.HasValue ? new OperationId(typeGuid.Value) : (OperationId?)null;
            return GetOperation(typeId);
        }

        public static IOperation GetOperation(Nullable<OperationId> typeId)
        {
            if (!typeId.HasValue)
            { return new IdentityOperation(); }

            var operationGuid = typeId.Value.OperationGuid;
            if (m_BackwardsCompatibilityGuids.ContainsKey(operationGuid))
            { operationGuid = m_BackwardsCompatibilityGuids[operationGuid]; }

            var operationId = new OperationId(operationGuid);
            IOperation operation = m_Operations[operationId];
            return operation;
        }

        public static IOperation GetOperationByShortName(string operationShortName)
        {
            if (!m_OperationShortNamesWithGuids.ContainsKey(operationShortName))
            { return null; }

            var operationId = new OperationId(m_OperationShortNamesWithGuids[operationShortName]);
            return m_Operations[operationId];
        }

        public static IOperation GetOperationByOperator(string operatorText, int requiredArgCount)
        {
            var operatorTextArgCount = string.Format(OperatorArgCount_Format, operatorText, requiredArgCount);
            if (!m_OperatorArgCountsWithGuids.ContainsKey(operatorTextArgCount))
            { return null; }

            var operationId = new OperationId(m_OperatorArgCountsWithGuids[operatorTextArgCount]);
            return m_Operations[operationId];
        }

        public static void AddOperationToCatalog(IOperation operation)
        {
            Type type = operation.GetType();
            Guid operationGuid = operation.Id.OperationGuid;
            string typeName = GetNameForType(type);
            var operatorTextArgCount = string.Format(OperatorArgCount_Format, operation.OperatorText, operation.SignatureSpecification.RequiredParameterCount);

            if (m_OperationTypeNameGuids.ContainsKey(typeName))
            { throw new InvalidOperationException("The specified Type already exists in the Operation catalog."); }
            if (m_OperationTypeNameGuids.ContainsValue(operationGuid))
            { throw new InvalidOperationException("The specified Guid already exists in the Operation catalog."); }
            if (m_Operations.ContainsKey(operation.Id))
            { throw new InvalidOperationException("The specified Id already exists in the Operation catalog."); }
            if (m_OperationShortNamesWithGuids.ContainsKey(operatorTextArgCount))
            { throw new InvalidOperationException("The specified Id already exists in the Operation catalog."); }
            if (m_OperatorArgCountsWithGuids.ContainsKey(operatorTextArgCount))
            { throw new InvalidOperationException("The specified Id already exists in the Operation catalog."); }

            m_OperationTypeNameGuids.Add(typeName, operationGuid);
            m_Operations.Add(operation.Id, operation);
            m_OperationShortNamesWithGuids.Add(operation.ShortName, operationGuid);
            m_OperatorArgCountsWithGuids.Add(operatorTextArgCount, operationGuid);
        }
    }
}