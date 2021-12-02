using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DomainObjects;
using DomainDriver.DomainModeling.Repositories;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public static class IProjectMemberUtils
    {
        #region Static Members

        public static readonly string ProjectGuid_PropName = ClassReflector.GetPropertyName((IProjectMember x) => x.ProjectGuid);
        public static readonly string IsRevisionSpecific_PropName = ClassReflector.GetPropertyName((IProjectMember x) => x.IsRevisionSpecific);
        public static readonly string RevisionNumber_PropName = ClassReflector.GetPropertyName((IProjectMember x) => x.RevisionNumber);
        public static readonly string RevisionNumber_NonNull_PropName = ClassReflector.GetPropertyName((IProjectMember x) => x.RevisionNumber_NonNull);

        public static readonly string ProjectGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(ProjectGuid_PropName);
        public static readonly string IsRevisionSpecific_Prefix = KeyProcessingModeUtils.GetModalDebugText(IsRevisionSpecific_PropName);
        public static readonly string RevisionNumber_Prefix = KeyProcessingModeUtils.GetModalDebugText(RevisionNumber_PropName);
        public static readonly string RevisionNumber_NonNull_Prefix = KeyProcessingModeUtils.GetModalDebugText(RevisionNumber_NonNull_PropName);

        public static readonly long RevisionNumber_Min = RevisionChain.EarliestRevisionNumber;
        public static readonly long RevisionNumber_Max = long.MaxValue;

        public static readonly Guid ProjectGuid_Empty = Guid.Empty;
        public static readonly long RevisionNumber_Empty = RevisionNumber_Min;

        #endregion

        #region Static Methods - Gets

        public static ProjectMemberId GetProjectMemberId(this IProjectMember projectMember)
        {
            return new ProjectMemberId(projectMember.ProjectGuid, projectMember.RevisionNumber);
        }

        public static ProjectMemberId GetProjectMemberId_MostSpecific(this IProjectMember first, IProjectMember second)
        {
            return GetProjectMemberId_MostSpecific(first, new IProjectMember[] { second });
        }

        public static ProjectMemberId GetProjectMemberId_MostSpecific(this IProjectMember first, IProjectMember second, IProjectMember third)
        {
            return GetProjectMemberId_MostSpecific(first, new IProjectMember[] { second, third });
        }

        public static ProjectMemberId GetProjectMemberId_MostSpecific(this IProjectMember main, IEnumerable<IProjectMember> others)
        {
            var all = new List<IProjectMember>();
            all.Add(main);
            all.AddRange(others);

            var unique = all.Where(x => x.ProjectGuid != ProjectMemberId.EmptyProjectGuid).Distinct().ToList();

            var uniqueProjectGuids = unique.Select(x => x.ProjectGuid).Distinct().ToList();
            var uniqueRevisionNumbers = unique.Where(x => x.IsRevisionSpecific).Select(x => x.RevisionNumber_NonNull).Distinct().ToList();

            if (uniqueProjectGuids.Count > 1)
            { throw new InvalidOperationException("Cannot mix values from different Projects."); }

            var projectGuid = (uniqueProjectGuids.Count < 1) ? ProjectMemberId.EmptyProjectGuid : uniqueProjectGuids.First();
            var revisionNumber = (uniqueRevisionNumbers.Count < 1) ? ProjectMemberId.EmptyRevisionNumber : uniqueRevisionNumbers.Max();

            return new ProjectMemberId(projectGuid, revisionNumber);
        }

        #endregion

        #region Static Methods - Checks

        public static bool AreProjectMemberIdsEqual(this IProjectMember first, IProjectMember second)
        {
            return AreProjectMemberIdsEqual(first, new IProjectMember[] { second });
        }

        public static bool AreProjectMemberIdsEqual(this IProjectMember first, IProjectMember second, IProjectMember third)
        {
            return AreProjectMemberIdsEqual(first, new IProjectMember[] { second, third });
        }

        public static bool AreProjectMemberIdsEqual(this IProjectMember main, IEnumerable<IProjectMember> others)
        {
            foreach (var other in others)
            {
                var mainId = main.GetProjectMemberId();
                var otherId = other.GetProjectMemberId();

                if (!mainId.Equals(otherId))
                { return false; }
            }
            return true;
        }

        #endregion

        #region Static Methods - Asserts

        public static void AssertProjectMemberIdsAreEqual(this IProjectMember first, IProjectMember second)
        {
            AssertProjectMemberIdsAreEqual(first, new IProjectMember[] { second });
        }

        public static void AssertProjectMemberIdsAreEqual(this IProjectMember first, IProjectMember second, IProjectMember third)
        {
            AssertProjectMemberIdsAreEqual(first, new IProjectMember[] { second, third });
        }

        public static void AssertProjectMemberIdsAreEqual(this IProjectMember main, IEnumerable<IProjectMember> others)
        {
            if (!AreProjectMemberIdsEqual(main, others))
            { throw new InvalidOperationException("The objects provided do not have the same ProjectMemberId values."); }
        }

        public static void AssertIsNotDeleted<T>(this T projectMember)
            where T : IProjectMember_Deleteable<T>
        {
            if (projectMember.IsDeleted)
            {
                var message = string.Format("The specified {0} has been deleted.", projectMember.GetType().Name);
                throw new InvalidOperationException(message);
            }
        }

        public static void AssertIsDeleted<T>(this T projectMember)
            where T : IProjectMember_Deleteable<T>
        {
            if (!projectMember.IsDeleted)
            {
                var message = string.Format("The specified {0} has not been deleted.", projectMember.GetType().Name);
                throw new InvalidOperationException(message);
            }
        }

        #endregion

        #region Static Methods - Comparers

        public static ProjectMemberComparer_Default<T> GetDefaultComparer<T>()
            where T : IProjectMember
        { return new ProjectMemberComparer_Default<T>(); }

        public static ProjectMemberComparer_Default<T> GetDefaultComparer<T>(T obj)
            where T : IProjectMember
        { return GetDefaultComparer<T>(); }

        public static ProjectMemberComparer_Revisionless<T> GetRevisionlessComparer<T>()
            where T : IProjectMember_Revisionless
        { return new ProjectMemberComparer_Revisionless<T>(); }

        public static ProjectMemberComparer_Revisionless<T> GetRevisionlessComparer<T>(T obj)
            where T : IProjectMember_Revisionless
        { return GetRevisionlessComparer<T>(); }

        #endregion

        #region Static Methods - Read If Exists

        public static void ReadIfExists<K, V, R>(this IRepository<K, V> repository, ref HashSet<K> desiredIds, List<R> results)
            where K : struct, IProjectMember
            where V : IProjectMember<V>, IBatchPersistable, IKeyedDomainObject<K, V>
        {
            var existingIds = desiredIds.Where(x => repository.Exists(x)).ToList();
            var existingValues = repository.Read(existingIds);

            foreach (var existingValue in existingValues)
            {
                var existingValueAsResult = (R)((object)existingValue);
                results.Add(existingValueAsResult);
            }

            desiredIds = desiredIds.Where(x => !existingIds.Contains(x)).ToHashSet();
        }

        #endregion

        #region Static Methods - Update for Revision

        public static K UpdateForRevision<K, V>(this IRepository<K, V> repository, V domainObject, RevisionChain revisionChain)
            where K : struct, IProjectMember
            where V : class, IProjectMember<V>, IBatchPersistable, IKeyedDomainObject<K, V>
        {
            var autoGeneratedRevisionType = revisionChain.DesiredRevisionData.AutoGenerated_RevisionType;

            if (!revisionChain.HasDesiredRevisionData)
            { throw new InvalidOperationException("Only the latest Revision can be updated."); }
            if (autoGeneratedRevisionType == AutoGeneratedRevisionType.ComputationRevision)
            { throw new InvalidOperationException("The standard \"UpdateForRevision\" method cannot be used with \"ComputationRevisions\"."); }

            var desiredRevisionNumber = revisionChain.DesiredRevisionNumber;
            var currentKey = domainObject.Key;
            var modelObjectType = (domainObject is IModelObject) ? (domainObject as IModelObject).ModelObjectType : (ModelObjectType?)null;

            ModelObjectTypeUtils.AssertUpdateIsAllowed_ForRevisionType(modelObjectType, autoGeneratedRevisionType);

            if (domainObject.Key.RevisionNumber_NonNull == desiredRevisionNumber)
            {
                domainObject.BatchState = BatchState.Updated;
                repository.Update(domainObject);
            }
            else if (domainObject.Key.RevisionNumber_NonNull < desiredRevisionNumber)
            {
                V copyForRevision = domainObject.CopyForRevision(desiredRevisionNumber);
                currentKey = copyForRevision.Key;

                repository.DetachDomainObjectIfNecessary(domainObject);

                copyForRevision.BatchState = BatchState.Updated;
                repository.Add(copyForRevision);
            }
            else
            { throw new InvalidOperationException("Cannot commit data for past Revision."); }

            return currentKey;
        }

        public static K UpdateForComputationRevision<K, V>(this IRepository<K, V> repository, V domainObject, RevisionChain revisionChain)
            where K : struct, IProjectMember
            where V : class, IProjectMember<V>, IBatchPersistable, IKeyedDomainObject<K, V>
        {
            var autoGeneratedRevisionType = revisionChain.DesiredRevisionData.AutoGenerated_RevisionType;

            if (!revisionChain.HasDesiredRevisionData)
            { throw new InvalidOperationException("Only the latest Revision can be updated."); }
            if (autoGeneratedRevisionType != AutoGeneratedRevisionType.ComputationRevision)
            { throw new InvalidOperationException("The \"UpdateForComputationRevision\" method can only be used with \"ComputationRevisions\"."); }

            var desiredRevisionNumber = revisionChain.DesiredRevisionNumber;
            var currentKey = domainObject.Key;
            var modelObjectType = (domainObject is IModelObject) ? (domainObject as IModelObject).ModelObjectType : (ModelObjectType?)null;

            ModelObjectTypeUtils.AssertUpdateIsAllowed_ForRevisionType(modelObjectType, autoGeneratedRevisionType);

            if (domainObject.Key.RevisionNumber_NonNull == desiredRevisionNumber)
            {
                domainObject.BatchState = BatchState.Updated;
                repository.Update(domainObject);
            }
            else if (domainObject.Key.RevisionNumber_NonNull < desiredRevisionNumber)
            {
                V copyForRevision = domainObject.CopyForRevision(desiredRevisionNumber);
                currentKey = copyForRevision.Key;

                repository.DetachDomainObjectIfNecessary(domainObject);

                copyForRevision.BatchState = BatchState.Updated;
                repository.Add(copyForRevision);
            }
            else
            { throw new InvalidOperationException("Cannot commit data for past Revision."); }

            return currentKey;
        }

        #endregion
    }
}