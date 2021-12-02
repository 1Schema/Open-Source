using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public struct ProjectMemberId : IProjectMember, IProjectMember_Revisionless, IConvertible, IComparable
    {
        #region Static Members

        public static readonly long MinRevisionNumber = IProjectMemberUtils.RevisionNumber_Min;
        public static readonly long MaxRevisionNumber = IProjectMemberUtils.RevisionNumber_Max;

        public static readonly Guid EmptyProjectGuid = IProjectMemberUtils.ProjectGuid_Empty;
        public static readonly long EmptyRevisionNumber = IProjectMemberUtils.RevisionNumber_Empty;

        public static readonly ProjectMemberId DefaultId = new ProjectMemberId(EmptyProjectGuid, EmptyRevisionNumber);

        #endregion

        #region Members

        private Guid m_ProjectGuid;
        private Nullable<long> m_RevisionNumber;

        #endregion

        #region Constructors

        public ProjectMemberId(Guid projectGuid, Nullable<long> revisionNumber)
        {
            if (revisionNumber.HasValue)
            {
                if ((revisionNumber.Value < MinRevisionNumber) || (revisionNumber.Value > MaxRevisionNumber))
                { throw new InvalidOperationException("The RevisionNumber specified is not within the valid bounds."); }
            }

            m_ProjectGuid = projectGuid;
            m_RevisionNumber = revisionNumber;
        }

        #endregion

        #region Properties

        public Guid ProjectGuid
        { get { return m_ProjectGuid; } }

        public bool IsRevisionSpecific
        { get { return m_RevisionNumber.HasValue; } }

        public Nullable<long> RevisionNumber
        { get { return m_RevisionNumber; } }

        public long RevisionNumber_NonNull
        { get { return m_RevisionNumber.HasValue ? m_RevisionNumber.Value : MaxRevisionNumber; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IProjectMember))
            { return false; }

            var objAsMember = (IProjectMember)obj;

            if (objAsMember.ProjectGuid != m_ProjectGuid)
            { return false; }
            if (objAsMember.RevisionNumber != m_RevisionNumber)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(IProjectMemberUtils.ProjectGuid_Prefix, ProjectGuid);
            string item2 = TypedIdUtils.NStructToString(IProjectMemberUtils.RevisionNumber_Prefix, RevisionNumber);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(ProjectMemberId a, ProjectMemberId b)
        { return a.Equals(b); }

        public static bool operator !=(ProjectMemberId a, ProjectMemberId b)
        { return !(a == b); }

        #endregion

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }

            var objAsMember = (obj as IProjectMember);

            if (objAsMember == null)
            { return false; }

            if (objAsMember.ProjectGuid != m_ProjectGuid)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = TypedIdUtils.StructToString(IProjectMemberUtils.ProjectGuid_Prefix, ProjectGuid);

            string value = item1;
            return value;
        }

        #endregion

        #region IConvertible Implementation

        public TypeCode GetTypeCode()
        { return TypeCode.Object; }

        public bool ToBoolean(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public byte ToByte(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public char ToChar(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public DateTime ToDateTime(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public decimal ToDecimal(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public double ToDouble(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public short ToInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public int ToInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public long ToInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public sbyte ToSByte(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public float ToSingle(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public string ToString(IFormatProvider provider)
        { return this.ToString(); }

        public object ToType(Type conversionType, IFormatProvider provider)
        { return ToString(provider); }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion

        #region IComparable Implementation

        public int CompareTo(object obj)
        {
            return ProjectMemberId.CompareTo(this, obj);
        }

        public static int CompareTo(object obj1, object obj2)
        {
            if (obj1 == null)
            { return -1; }
            if (!(obj1 is IProjectMember))
            { return -1; }

            if (obj2 == null)
            { return 1; }
            if (!(obj2 is IProjectMember))
            { return 1; }

            var projectMember1 = (IProjectMember)obj1;
            var projectMember2 = (IProjectMember)obj2;

            int projectGuidResult = projectMember1.ProjectGuid.CompareTo(projectMember2.ProjectGuid);
            if (projectGuidResult != 0)
            { return projectGuidResult; }

            int isRevisionSpecificResult = projectMember1.IsRevisionSpecific.CompareTo(projectMember2.IsRevisionSpecific);
            if (isRevisionSpecificResult != 0)
            { return isRevisionSpecificResult; }

            if (!projectMember1.IsRevisionSpecific)
            { return 0; }

            int revisionNumberResult = projectMember1.RevisionNumber_NonNull.CompareTo(projectMember2.RevisionNumber_NonNull);
            if (revisionNumberResult != 0)
            { return revisionNumberResult; }

            return 0;
        }

        #endregion
    }
}