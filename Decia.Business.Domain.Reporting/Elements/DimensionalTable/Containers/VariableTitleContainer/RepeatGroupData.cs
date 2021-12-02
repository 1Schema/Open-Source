using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Domain.Reporting.Dimensionality;

namespace Decia.Business.Domain.Reporting
{
    internal class RepeatGroupData
    {
        private Guid m_GroupId;
        private string m_GroupName;
        private int m_GroupIndex;

        private Nullable<Guid> m_ParentGroupId;
        private List<IVariableTitleBox> m_GroupedBoxes;

        public RepeatGroupData(string groupName, int groupIndex)
            : this(groupName, groupIndex, (Nullable<Guid>)null)
        { }

        public RepeatGroupData(string groupName, int groupIndex, RepeatGroupData parentGroup)
            : this(groupName, groupIndex, (parentGroup == null) ? (Nullable<Guid>)null : (Nullable<Guid>)parentGroup.GroupId)
        { }

        public RepeatGroupData(string groupName, int groupIndex, Nullable<Guid> parentGroupId)
        {
            m_GroupId = Guid.NewGuid();
            m_GroupName = groupName;
            m_GroupIndex = groupIndex;

            m_ParentGroupId = parentGroupId;
            m_GroupedBoxes = new List<IVariableTitleBox>();

            if (!string.IsNullOrWhiteSpace(m_GroupName))
            {
                if (TokenizedLoweredCommonGroupName.Contains(string.Empty))
                { throw new InvalidOperationException("Invalid Repeat Group name encountered."); }
            }
        }

        #region Properties

        public Guid GroupId
        {
            get { return m_GroupId; }
        }

        public string GroupName
        {
            get { return m_GroupName; }
        }

        public int GroupIndex
        {
            get { return m_GroupIndex; }
        }

        public string CommonGroupName
        {
            get { return GetStandardizedGroupName(GroupName); }
        }

        public string LoweredCommonGroupName
        {
            get { return GroupName.ToLower(); }
        }

        public string[] TokenizedCommonGroupName
        {
            get { return CommonGroupName.Split(DimensionalRepeatGroup.ValidGroupNameSeparators); }
        }

        public string[] TokenizedLoweredCommonGroupName
        {
            get { return LoweredCommonGroupName.Split(DimensionalRepeatGroup.ValidGroupNameSeparators); }
        }

        public Nullable<Guid> ParentGroupId
        {
            get { return m_ParentGroupId; }
        }

        public IDictionary<ReportElementId, IVariableTitleBox> GroupedBoxes
        {
            get { return m_GroupedBoxes.ToDictionary(x => x.Key, x => x, ReportRenderingEngine.EqualityComparer_ReportElementId); }
        }

        #endregion

        #region Methods

        public void AddVariableTitleBox(IVariableTitleBox titleBox)
        {
            if (m_GroupedBoxes.Contains(titleBox))
            { throw new InvalidOperationException("The specified VariableTitleBox has already been added to the Repeat Group data."); }

            m_GroupedBoxes.Add(titleBox);
        }

        public bool DoesGroupNameMatch(string otherGroupName)
        {
            return DoGroupNamesMatch(this.GroupName, otherGroupName);
        }

        public bool IsGroupNameAncestor(string otherGroupName)
        {
            if (string.IsNullOrWhiteSpace(m_GroupName) || string.IsNullOrWhiteSpace(otherGroupName))
            { return false; }

            return IsGroupNameAncestor(this.LoweredCommonGroupName, GetStandardizedGroupName(otherGroupName).ToLower());
        }

        public bool IsGroupNameDescendant(string otherGroupName)
        {
            if (string.IsNullOrWhiteSpace(m_GroupName) || string.IsNullOrWhiteSpace(otherGroupName))
            { return false; }

            return IsGroupNameAncestor(GetStandardizedGroupName(otherGroupName).ToLower(), this.LoweredCommonGroupName);
        }

        public bool IsGroupNameRelated(string otherGroupName)
        {
            return GetGroupNameRelationSeparationIndex(otherGroupName).HasValue;
        }

        public int? GetGroupNameRelationSeparationIndex(string otherGroupName)
        {
            if (string.IsNullOrWhiteSpace(m_GroupName) || string.IsNullOrWhiteSpace(otherGroupName))
            { return null; }

            var thisTokens = TokenizedLoweredCommonGroupName;
            var otherTokens = GetStandardizedGroupName(otherGroupName).ToLower().Split(DimensionalRepeatGroup.ValidGroupNameSeparators);

            if (otherTokens.Contains(string.Empty))
            { throw new InvalidOperationException("Invalid Repeat Group name encountered."); }

            if ((thisTokens.Count() < 2) || (otherTokens.Count() < 2))
            { return null; }

            if (thisTokens.Count() != otherTokens.Count())
            { return null; }

            int i = 0;
            for (i = 0; i < (thisTokens.Count() - 1); i++)
            {
                if (thisTokens[i] != otherTokens[i])
                {
                    if (i < 1)
                    { return null; }
                    else
                    { return i; }
                }
            }
            return i;
        }

        #endregion

        #region Static Methods

        public static string GetStandardizedGroupName(string groupName)
        {
            var standardizedName = groupName;

            foreach (char separator in DimensionalRepeatGroup.ValidGroupNameSeparators)
            { standardizedName = standardizedName.Replace(separator, DimensionalRepeatGroup.DefaultGroupNameSeparator); }
            return standardizedName;
        }

        public static bool DoGroupNamesMatch(string thisGroupName, string otherGroupName)
        {
            if (string.IsNullOrWhiteSpace(thisGroupName) && string.IsNullOrWhiteSpace(otherGroupName))
            { return true; }

            if (string.IsNullOrWhiteSpace(thisGroupName) || string.IsNullOrWhiteSpace(otherGroupName))
            { return false; }

            var adjustedThisGroupName = RepeatGroupData.GetStandardizedGroupName(thisGroupName).ToLower();
            var adjustedOtherGroupName = RepeatGroupData.GetStandardizedGroupName(otherGroupName).ToLower();
            return (adjustedThisGroupName == adjustedOtherGroupName);
        }

        public static bool IsGroupNameAncestor(string thisLoweredGroupName, string otherLoweredGroupName)
        {
            if (thisLoweredGroupName.Length <= otherLoweredGroupName.Length)
            { return false; }

            if (!thisLoweredGroupName.Contains(otherLoweredGroupName))
            { return false; }

            var nextChar = thisLoweredGroupName.ElementAt(otherLoweredGroupName.Length);
            return DimensionalRepeatGroup.ValidGroupNameSeparators.Contains(nextChar);
        }

        public static int GetTotalGroupDepth(RepeatGroupData thisData, IDictionary<Guid, RepeatGroupData> allDatas)
        {
            var currentData = thisData;
            int depth = 0;

            while (currentData.ParentGroupId.HasValue)
            {
                currentData = allDatas[currentData.ParentGroupId.Value];
                depth++;
            }
            return depth;
        }

        public static RepeatGroupData GetAncesorAtDepth(RepeatGroupData thisData, IDictionary<Guid, RepeatGroupData> allDatas, int distanceFromRoot)
        {
            int depth = GetTotalGroupDepth(thisData, allDatas);

            var currentData = thisData;
            int downCount = depth - distanceFromRoot;

            while (downCount > 0)
            {
                currentData = allDatas[currentData.ParentGroupId.Value];
                downCount--;
            }
            return currentData;
        }

        #endregion
    }
}