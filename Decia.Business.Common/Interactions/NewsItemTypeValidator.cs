using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Common.Interactions
{
    public static class NewsItemTypeValidator
    {
        private static Dictionary<SiteActorType, List<NewsItemType>> s_ValidItemTypes;

        static NewsItemTypeValidator()
        {
            s_ValidItemTypes = new Dictionary<SiteActorType, List<NewsItemType>>();

            NewsItemType[] validOrganizationItems = new NewsItemType[] { NewsItemType.AddMember, NewsItemType.AddProject };
            s_ValidItemTypes.Add(SiteActorType.Organization, new List<NewsItemType>(validOrganizationItems));
            NewsItemType[] validWorkgroupItems = new NewsItemType[] { NewsItemType.AddMember, NewsItemType.AddProject };
            s_ValidItemTypes.Add(SiteActorType.Workgroup, new List<NewsItemType>(validWorkgroupItems));
            NewsItemType[] validUserItems = new NewsItemType[] { NewsItemType.CreateAccount, NewsItemType.CreateConnection, NewsItemType.CreateGroup, NewsItemType.CreateModel, NewsItemType.CreateProject, NewsItemType.CreateReport, NewsItemType.ExportProject, NewsItemType.JoinGroup, NewsItemType.JoinProject, NewsItemType.UpdateProfile, NewsItemType.UpdateModel, NewsItemType.UpdateProject, NewsItemType.UpdateReport, NewsItemType.VersionProject };
            s_ValidItemTypes.Add(SiteActorType.User, new List<NewsItemType>(validUserItems));
        }

        public static bool IsItemTypeValidForOwnerType(SiteActorType ownerType, NewsItemType itemType)
        {
            if (!s_ValidItemTypes.ContainsKey(ownerType))
            { return false; }

            List<NewsItemType> validItemTypes = s_ValidItemTypes[ownerType];

            if (!validItemTypes.Contains(itemType))
            { return false; }
            return true;
        }
    }
}