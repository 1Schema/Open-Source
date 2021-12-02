using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Reporting
{
    public struct ReportId : IModelMember, IProjectMember_Revisionless, IConvertible
    {
        #region Static Members

        public static readonly string ReportGuid_PropName = ClassReflector.GetPropertyName((ReportId x) => x.ReportGuid);
        public static readonly string ReportGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(ReportGuid_PropName);

        public static readonly ProjectMemberComparer_Revisionless<ReportId> Comparer_Revisionless = new ProjectMemberComparer_Revisionless<ReportId>();

        public static readonly Guid EmptyReportGuid = Guid.Empty;
        public static readonly ReportId DefaultId = new ReportId(ModelMemberId.EmptyProjectGuid, ModelMemberId.EmptyRevisionNumber, ModelMemberId.ModelTemplateNumber_Empty, EmptyReportGuid);

        #endregion

        private ModelMemberId m_ModelMemberId;
        private Guid m_ReportGuid;

        #region Constructors

        public ReportId(IModelMember modelMember, Guid reportGuid)
            : this(modelMember.ProjectGuid, modelMember.RevisionNumber_NonNull, modelMember.ModelTemplateNumber, reportGuid)
        { }

        public ReportId(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
        {
            m_ModelMemberId = new ModelMemberId(projectGuid, revisionNumber, modelTemplateNumber);
            m_ReportGuid = reportGuid;
        }

        #endregion

        public ProjectMemberId ProjectMemberId
        { get { return m_ModelMemberId.GetProjectMemberId(); } }

        public ModelMemberId ModelMemberId
        { get { return m_ModelMemberId; } }

        public Guid ProjectGuid
        { get { return m_ModelMemberId.ProjectGuid; } }

        public bool IsRevisionSpecific
        { get { return m_ModelMemberId.IsRevisionSpecific; } }

        public Nullable<long> RevisionNumber
        { get { return m_ModelMemberId.RevisionNumber; } }

        public long RevisionNumber_NonNull
        { get { return m_ModelMemberId.RevisionNumber_NonNull; } }

        public bool IsInstance
        { get { return m_ModelMemberId.IsInstance; } }

        public int ModelTemplateNumber
        { get { return m_ModelMemberId.ModelTemplateNumber; } }

        public Nullable<Guid> ModelInstanceGuid
        { get { return m_ModelMemberId.ModelInstanceGuid; } }

        public Guid ReportGuid
        { get { return m_ReportGuid; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ReportId))
            { return false; }

            var objTyped = (ReportId)obj;
            var objModelMemberId = objTyped.GetModelMemberId();

            if (objModelMemberId != m_ModelMemberId)
            { return false; }
            if (objTyped.ReportGuid != m_ReportGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ModelMemberId.ToString();
            string item2 = TypedIdUtils.StructToString(ReportGuid_Prefix, ReportGuid);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(ReportId a, ReportId b)
        { return a.Equals(b); }

        public static bool operator !=(ReportId a, ReportId b)
        { return !(a == b); }

        public static ReportId NewId(Guid projectGuid, long revisionNumber, int modelTemplateNumber)
        { return new ReportId(projectGuid, revisionNumber, modelTemplateNumber, Guid.NewGuid()); }

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ReportId))
            { return false; }

            var objTyped = (ReportId)obj;
            var objModelMemberId = objTyped.GetModelMemberId();

            if (!objModelMemberId.Equals_Revisionless(m_ModelMemberId))
            { return false; }
            if (objTyped.ReportGuid != m_ReportGuid)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = ModelMemberId.ToString_Revisionless();
            string item2 = TypedIdUtils.StructToString(ReportGuid_Prefix, ReportGuid);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
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
        { return ReportGuid; }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return ModelObjectType.ReportTemplate; }
        }

        public Guid ModelObjectId
        {
            get { return ReportGuid; }
        }

        public bool IdIsInt
        {
            get { return false; }
        }

        public int ModelObjectIdAsInt
        {
            get { throw new InvalidOperationException("A true Guid value cannot be converted to Int32."); }
        }

        public string ComplexId
        {
            get { return ConversionUtils.ConvertComplexIdToString(ModelObjectType, ModelObjectId); }
        }

        public ModelObjectReference ModelObjectRef
        {
            get { return new ModelObjectReference(ModelObjectType, ModelObjectId); }
        }

        #endregion
    }
}