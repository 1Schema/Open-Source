using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    [Flags]
    public enum ProjectPermissionType
    {
        [Description("View All Project Info")]
        ReadProjectInfo = 1,
        [Description("Edit Project Info")]
        EditProjectInfo = 2,
        [Description("Erase Project")]
        EraseProject = 4,
        [Description("Publish Project")]
        PublishProject = 8,
        [Description("Manage Members")]
        ManageMembers = 16,
        [Description("Create Template Data")]
        CreateTemplateData = 32,
        [Description("Edit Template Data")]
        EditTemplateData = 64,
        [Description("Delete Template Data")]
        DeleteTemplateData = 128,
        [Description("Create Instance Data")]
        CreateInstanceData = 256,
        [Description("Edit Instance Data")]
        EditInstanceData = 512,
        [Description("Delete Instance Data")]
        DeleteInstanceData = 1024,
        [Description("Export Data")]
        ExportData = 2048,
    }

    public static class ProjectPermissionTypeUtils
    {
        public static readonly ProjectPermissionType FullControl = EnumUtils.GetEnumValues<ProjectPermissionType>().Bitwise_Combine();
    }
}