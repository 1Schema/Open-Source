using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.Projects;

namespace Decia.Business.Domain.Exports
{
    public struct ExportHistoryItemId : IProjectMember, IConvertible, IComparable
    {
        #region Static Members

        public static readonly string DateOfExport_PropName = ClassReflector.GetPropertyName((ExportHistoryItemId x) => x.DateOfExport);
        public static readonly string DateOfExport_Prefix = KeyProcessingModeUtils.GetModalDebugText(DateOfExport_PropName);

        #endregion

        #region Members

        private UserId m_ExporterId;
        private RevisionId m_ExportedRevisionId;
        private DateTime m_DateOfExport;

        #endregion

        #region Constructors

        public ExportHistoryItemId(UserId exporterId, RevisionId revisionId, DateTime dateOfExport)
            : this(exporterId, revisionId.ProjectId, revisionId.RevisionNumber_NonNull, dateOfExport)
        { }

        public ExportHistoryItemId(UserId exporterId, ProjectId projectId, long revisionNumber, DateTime dateOfExport)
            : this(exporterId.UserGuid, projectId.ProjectGuid, revisionNumber, dateOfExport)
        { }

        public ExportHistoryItemId(Guid exporterGuid, Guid projectGuid, long revisionNumber, DateTime dateOfExport)
        {
            m_ExporterId = new UserId(exporterGuid);
            m_ExportedRevisionId = new RevisionId(projectGuid, revisionNumber);
            m_DateOfExport = dateOfExport;
        }

        #endregion

        #region Properties

        public UserId ExporterId
        { get { return m_ExporterId; } }

        public SiteActorId ExporterAsSiteActorId
        { get { return m_ExporterId.GetAsSiteActorId(); } }

        public Guid ExporterGuid
        { get { return m_ExporterId.UserGuid; } }

        public ProjectMemberId ProjectMemberId
        { get { return m_ExportedRevisionId.ProjectMemberId; } }

        public ProjectId ProjectId
        { get { return m_ExportedRevisionId.ProjectId; } }

        public Guid ProjectGuid
        { get { return m_ExportedRevisionId.ProjectGuid; } }

        public RevisionId RevisionId
        { get { return m_ExportedRevisionId; } }

        public bool IsRevisionSpecific
        { get { return true; } }

        public Nullable<long> RevisionNumber
        { get { return m_ExportedRevisionId.RevisionNumber_NonNull; } }

        public long RevisionNumber_NonNull
        { get { return m_ExportedRevisionId.RevisionNumber_NonNull; } }

        public DateTime DateOfExport
        { get { return m_DateOfExport; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ExportHistoryItemId))
            { return false; }

            var objTyped = (ExportHistoryItemId)obj;

            if (objTyped.m_ExporterId != this.m_ExporterId)
            { return false; }
            if (objTyped.m_ExportedRevisionId != this.m_ExportedRevisionId)
            { return false; }
            if (objTyped.m_DateOfExport != this.m_DateOfExport)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = m_ExporterId.ToString();
            string item2 = m_ExportedRevisionId.ToString();
            string item3 = TypedIdUtils.StructToString(DateOfExport_Prefix, m_DateOfExport);

            string value = string.Format(ConversionUtils.ThreeItemListFormat, item1, item2, item3);
            return value;
        }

        public static bool operator ==(ExportHistoryItemId a, ExportHistoryItemId b)
        { return a.Equals(b); }

        public static bool operator !=(ExportHistoryItemId a, ExportHistoryItemId b)
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
            if (obj == null)
            { return -1; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return -1; }

            ExportHistoryItemId otherExportHistoryItemId = (ExportHistoryItemId)obj;

            ulong thisULong0, thisULong1, otherULong0, otherULong1;
            ConversionUtils.ConvertGuidToULongs(this.ExporterGuid, out thisULong0, out thisULong1);
            ConversionUtils.ConvertGuidToULongs(otherExportHistoryItemId.ExporterGuid, out otherULong0, out otherULong1);

            var ulong0Result = thisULong0.CompareTo(otherULong0);
            if (ulong0Result != 0)
            { return ulong0Result; }
            var ulong1Result = thisULong1.CompareTo(otherULong1);
            if (ulong1Result != 0)
            { return ulong1Result; }

            ConversionUtils.ConvertGuidToULongs(this.ProjectGuid, out thisULong0, out thisULong1);
            ConversionUtils.ConvertGuidToULongs(otherExportHistoryItemId.ProjectGuid, out otherULong0, out otherULong1);

            ulong0Result = thisULong0.CompareTo(otherULong0);
            if (ulong0Result != 0)
            { return ulong0Result; }
            ulong1Result = thisULong1.CompareTo(otherULong1);
            if (ulong1Result != 0)
            { return ulong1Result; }

            var revisionNumberResult = this.RevisionNumber_NonNull.CompareTo(otherExportHistoryItemId.RevisionNumber_NonNull);
            if (revisionNumberResult != 0)
            { return revisionNumberResult; }

            var dateOfVoteResult = this.DateOfExport.CompareTo(otherExportHistoryItemId.DateOfExport);
            return dateOfVoteResult;
        }

        #endregion
    }
}