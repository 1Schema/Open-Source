using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Reporting
{
    public struct ReportElementId : IModelMember, IProjectMember_Revisionless, IConvertible, IComparable
    {
        #region Static Members

        public static readonly string ReportElementNumber_PropName = ClassReflector.GetPropertyName((ReportElementId x) => x.ReportElementNumber);
        public static readonly string ReportElementNumber_Prefix = KeyProcessingModeUtils.GetModalDebugText(ReportElementNumber_PropName);

        public static readonly ProjectMemberComparer_Revisionless<ReportElementId> Comparer_Revisionless = new ProjectMemberComparer_Revisionless<ReportElementId>();

        public static readonly int EmptyReportElementNumber = int.MinValue;
        public static readonly ReportElementId DefaultId = new ReportElementId(ReportId.DefaultId, EmptyReportElementNumber);

        #endregion

        #region Static Methods

        public static Func<Nullable<ReportElementId>> GetElementId_Null = (() => (Nullable<ReportElementId>)null);
        public static Func<ReportId, ReportElementId> GetElementId_ReportLevel = ((ReportId reportId) => new ReportElementId(reportId, Report.Report_ReportElementType));

        #endregion

        private ReportId m_ReportId;
        private int m_ReportElementNumber;

        #region Constructors

        public ReportElementId(ReportId reportId, ReservedElementType elementType)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid, elementType)
        { }

        public ReportElementId(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, ReservedElementType elementType)
        {
            int elementNumber = (int)elementType;

            ReservedElementTypeUtils.AssertIsReservedElement(elementNumber);

            m_ReportId = new ReportId(projectGuid, revisionNumber, modelTemplateNumber, reportGuid);
            m_ReportElementNumber = elementNumber;
        }

        public ReportElementId(ReportId reportId, int elementNumber)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid, elementNumber)
        { }

        public ReportElementId(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, int elementNumber)
            : this(projectGuid, revisionNumber, modelTemplateNumber, reportGuid, elementNumber, true)
        { }

        public ReportElementId(ReportId reportId, int elementNumber, bool assertNotReserved)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid, elementNumber, assertNotReserved)
        { }

        public ReportElementId(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, int elementNumber, bool assertNotReserved)
        {
            if (assertNotReserved)
            { ReservedElementTypeUtils.AssertIsNotReservedElement(elementNumber); }

            m_ReportId = new ReportId(projectGuid, revisionNumber, modelTemplateNumber, reportGuid);
            m_ReportElementNumber = elementNumber;
        }

        #endregion

        #region Properties

        public ProjectMemberId ProjectMemberId
        { get { return m_ReportId.GetProjectMemberId(); } }

        public ModelMemberId ModelMemberId
        { get { return m_ReportId.GetModelMemberId(); } }

        public ReportId ReportId
        { get { return m_ReportId; } }

        public Guid ProjectGuid
        { get { return m_ReportId.ProjectGuid; } }

        public bool IsRevisionSpecific
        { get { return m_ReportId.IsRevisionSpecific; } }

        public Nullable<long> RevisionNumber
        { get { return m_ReportId.RevisionNumber; } }

        public long RevisionNumber_NonNull
        { get { return m_ReportId.RevisionNumber_NonNull; } }

        public bool IsInstance
        { get { return m_ReportId.IsInstance; } }

        public int ModelTemplateNumber
        { get { return m_ReportId.ModelTemplateNumber; } }

        public Nullable<Guid> ModelInstanceGuid
        { get { return m_ReportId.ModelInstanceGuid; } }

        public Guid ReportGuid
        { get { return m_ReportId.ReportGuid; } }

        public int ReportElementNumber
        { get { return m_ReportElementNumber; } }

        #endregion

        #region Method Overrides

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ReportElementId))
            { return false; }

            var objTyped = (ReportElementId)obj;
            var objReportId = objTyped.ReportId;

            if (objReportId != m_ReportId)
            { return false; }
            if (objTyped.ReportElementNumber != m_ReportElementNumber)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ReportId.ToString();
            string item2 = TypedIdUtils.StructToString(ReportElementNumber_Prefix, ReportElementNumber);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(ReportElementId a, ReportElementId b)
        { return a.Equals(b); }

        public static bool operator !=(ReportElementId a, ReportElementId b)
        { return !(a == b); }

        #endregion

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ReportElementId))
            { return false; }

            var objTyped = (ReportElementId)obj;
            var objReportId = objTyped.ReportId;

            if (!objReportId.Equals_Revisionless(m_ReportId))
            { return false; }
            if (objTyped.ReportElementNumber != m_ReportElementNumber)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = ReportId.ToString_Revisionless();
            string item2 = TypedIdUtils.StructToString(ReportElementNumber_Prefix, ReportElementNumber);

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
            ReportElementId otherAsElementId = (ReportElementId)obj;
            return this.m_ReportElementNumber.CompareTo(otherAsElementId.m_ReportElementNumber);
        }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return ModelObjectType.ReportElementTemplate; }
        }

        public Guid ModelObjectId
        {
            get { return ReportElementNumber.ConvertIntToGuid(); }
        }

        public bool IdIsInt
        {
            get { return true; }
        }

        public int ModelObjectIdAsInt
        {
            get { return ReportElementNumber; }
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