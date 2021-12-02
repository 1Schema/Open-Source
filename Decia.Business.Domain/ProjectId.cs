using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public struct ProjectId : IProjectMember, IConvertible, IComparable, IGuidId
    {
        #region Members

        private ProjectMemberId m_ProjectMemberId;

        #endregion

        #region Constructors

        public ProjectId(IProjectMember projectMember)
            : this(projectMember.ProjectGuid)
        { }

        public ProjectId(ProjectId projectId)
            : this(projectId.ProjectGuid)
        { }

        public ProjectId(Guid projectGuid)
        {
            m_ProjectMemberId = new ProjectMemberId(projectGuid, null);
        }

        public static ProjectId NewId()
        {
            return new ProjectId(Guid.NewGuid());
        }

        #endregion

        #region Properties

        public ProjectMemberId ProjectMemberId
        { get { return m_ProjectMemberId; } }

        public Guid ProjectGuid
        { get { return m_ProjectMemberId.ProjectGuid; } }

        public bool IsRevisionSpecific
        { get { return false; } }

        public Nullable<long> RevisionNumber
        { get { return null; } }

        public long RevisionNumber_NonNull
        { get { throw new InvalidOperationException("ProjectIds are not Revision-specific."); } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IProjectMember))
            { return false; }

            var objAsMember = (IProjectMember)obj;
            var objProjectMemberId = objAsMember.GetProjectMemberId();

            if (objProjectMemberId != m_ProjectMemberId)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = m_ProjectMemberId.ToString();
            return item1;
        }

        public static bool operator ==(ProjectId a, ProjectId b)
        { return a.Equals(b); }

        public static bool operator !=(ProjectId a, ProjectId b)
        { return !(a == b); }

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
        { return ProjectGuid; }

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
            return m_ProjectMemberId.CompareTo(obj);
        }

        #endregion

        #region IGuidId Implementation

        Guid IGuidId.InnerGuid
        {
            get { return this.ProjectGuid; }
        }

        #endregion
    }
}