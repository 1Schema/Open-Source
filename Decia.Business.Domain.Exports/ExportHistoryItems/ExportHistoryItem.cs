using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Permissions;
using Decia.Business.Domain.Projects;

namespace Decia.Business.Domain.Exports
{
    public partial class ExportHistoryItem : ProjectDomainObjectBase<ExportHistoryItemId, ExportHistoryItem>, IExportHistoryItem<ExportHistoryItem>
    {
        public static readonly DateTime Default_DateOfExport = new DateTime(2016, 1, 1);
        public const TargetTypeCategory Default_TargetTypeCategory = Decia.Business.Common.Exports.TargetTypeCategory.SqlDb;
        public static object Default_TargetTypeValue { get { return Enum.GetValues(TargetTypeCategoryUtils.TargetTypesByCategory[Default_TargetTypeCategory]).GetValue(0); } }
        public static string Default_TargetTypeValue_AsString { get { return Default_TargetTypeValue.ToString(); } }
        public static int Default_TargetTypeValue_AsInt { get { return (int)Default_TargetTypeValue; } }
        public const bool Default_WasDataIncluded = false;
        public const string Default_ExtendedData = "";

        #region Members

        private TargetTypeCategory m_TargetTypeCategory;
        private int m_TargetTypeValue;
        private bool m_WasDataIncluded;
        private string m_ExtendedData;

        #endregion

        #region Constructors

        public ExportHistoryItem()
            : this(Guid.Empty, Guid.Empty, Revision.EarliestRevisionNumber, Default_DateOfExport)
        { }

        public ExportHistoryItem(UserId exporterId, RevisionId revisionId, DateTime dateOfExport)
            : this(exporterId, revisionId.ProjectId, revisionId.RevisionNumber_NonNull, dateOfExport)
        { }

        public ExportHistoryItem(UserId exporterId, ProjectId projectId, long revisionNumber, DateTime dateOfExport)
            : this(exporterId.UserGuid, projectId.ProjectGuid, revisionNumber, dateOfExport)
        { }

        public ExportHistoryItem(Guid exporterGuid, Guid projectGuid, long revisionNumber, DateTime dateOfExport)
            : base(PredefinedUserTypeUtils.SystemUserId.UserGuid, projectGuid, revisionNumber)
        {
            m_Key = new ExportHistoryItemId(exporterGuid, projectGuid, revisionNumber, dateOfExport);
            m_TargetTypeCategory = Default_TargetTypeCategory;
            m_TargetTypeValue = Default_TargetTypeValue_AsInt;
            m_WasDataIncluded = Default_WasDataIncluded;
            m_ExtendedData = Default_ExtendedData;
        }

        #endregion

        #region Base-Class Method Overrides

        protected override Guid GetProjectGuid()
        {
            return EF_ProjectGuid;
        }

        protected override long? GetRevisionNumber()
        {
            return EF_RevisionNumber;
        }

        protected override void SetProjectGuid(Guid projectGuid)
        {
            EF_ProjectGuid = projectGuid;
        }

        protected override void SetRevisionNumber(long revisionNumber)
        {
            EF_RevisionNumber = revisionNumber;
        }

        #endregion

        #region IExportHistoryItem<ProjectMetadata> Implementation

        [NotMapped]
        public UserId ExporterId
        { get { return m_Key.ExporterId; } }

        [NotMapped]
        public SiteActorId ExporterAsSiteActorId
        { get { return m_Key.ExporterAsSiteActorId; } }

        [NotMapped]
        public Guid ExporterGuid
        { get { return m_Key.ExporterGuid; } }

        [NotMapped]
        public ProjectMemberId ProjectMemberId
        { get { return m_Key.ProjectMemberId; } }

        [NotMapped]
        public ProjectId ProjectId
        { get { return m_Key.ProjectId; } }

        [NotMapped]
        public Guid ProjectGuid
        { get { return m_Key.ProjectGuid; } }

        [NotMapped]
        public RevisionId RevisionId
        { get { return m_Key.RevisionId; } }

        [NotMapped]
        public bool IsRevisionSpecific
        { get { return m_Key.IsRevisionSpecific; } }

        [NotMapped]
        public Nullable<long> RevisionNumber
        { get { return m_Key.RevisionNumber; } }

        [NotMapped]
        public long RevisionNumber_NonNull
        { get { return m_Key.RevisionNumber_NonNull; } }

        [NotMapped]
        public DateTime DateOfExport
        { get { return m_Key.DateOfExport; } }

        [NotMapped]
        public TargetTypeCategory TargetTypeCategory
        {
            get { return m_TargetTypeCategory; }
        }

        [NotMapped]
        public string TargetTypeCategory_Name
        {
            get { return EnumUtils.GetName<TargetTypeCategory>(TargetTypeCategory); }
        }

        [NotMapped]
        public string TargetTypeCategory_Description
        {
            get { return EnumUtils.GetDescription<TargetTypeCategory>(TargetTypeCategory); }
        }

        [NotMapped]
        public int TargetTypeValue
        {
            get { return m_TargetTypeValue; }
        }

        [NotMapped]
        public string TargetTypeValue_Name
        {
            get { return EnumUtils.GetName(TargetTypeCategoryUtils.TargetTypesByCategory[TargetTypeCategory], TargetTypeValue); }
        }

        [NotMapped]
        public string TargetTypeValue_Description
        {
            get { return EnumUtils.GetDescription(TargetTypeCategoryUtils.TargetTypesByCategory[TargetTypeCategory], TargetTypeValue); }
        }

        public void Set_TargetType(TargetTypeCategory targetTypeCategory, int targetTypeValue)
        {
            m_TargetTypeCategory = targetTypeCategory;
            m_TargetTypeValue = targetTypeValue;
        }

        public T Get_TargetTypeValue_Typed<T>()
            where T : struct
        { return EnumUtils.GetAsEnum<T>(m_TargetTypeValue).Value; }

        [NotMapped]
        public bool WasDataIncluded
        {
            get { return m_WasDataIncluded; }
            set { m_WasDataIncluded = value; }
        }

        [NotMapped]
        public string ExtendedData
        {
            get { return m_ExtendedData; }
            set { m_ExtendedData = value; }
        }

        public ExportHistoryItem CopyForRevision(long newRevisionNumber)
        {
            if (this.Key.RevisionNumber >= newRevisionNumber)
            { throw new InvalidOperationException("A ProjectMetadata created for a new revision must have a greater revision number."); }

            return (this as IProjectMember<ExportHistoryItem>).CopyForProject(this.Key.ProjectGuid, newRevisionNumber);
        }

        ExportHistoryItem IProjectMember<ExportHistoryItem>.CopyForProject(Guid projectGuid, long revisionNumber)
        {
            ExportHistoryItem newInstance = new ExportHistoryItem(m_Key.ExporterGuid, projectGuid, revisionNumber, m_Key.DateOfExport);

            newInstance.m_TargetTypeCategory = this.m_TargetTypeCategory;
            newInstance.m_TargetTypeValue = this.m_TargetTypeValue;
            newInstance.m_ExtendedData = this.m_ExtendedData;

            return newInstance;
        }

        #endregion

        #region ManagedDomainObjectBase Overrides

        public override bool IsOwnerEditable()
        {
            return false;
        }

        #endregion
    }
}