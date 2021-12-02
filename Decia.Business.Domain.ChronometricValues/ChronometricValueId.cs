using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.ChronometricValues
{
    public struct ChronometricValueId : IProjectMember, IProjectMember_Revisionless, IModelObjectWithRef, IConvertible
    {
        #region Static Members

        public static readonly string ChronometricValueGuid_PropName = ClassReflector.GetPropertyName((ChronometricValueId x) => x.ChronometricValueGuid);
        public static readonly string ChronometricValueGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(ChronometricValueGuid_PropName);

        public static readonly Guid EmptyChronometricValueGuid = Guid.Empty;
        public static readonly ChronometricValueId DefaultId = new ChronometricValueId(ProjectMemberId.EmptyProjectGuid, ProjectMemberId.EmptyRevisionNumber, EmptyChronometricValueGuid);

        #endregion

        #region Members

        private ProjectMemberId m_ProjectMemberId;
        private Guid m_ChronometricValueGuid;

        #endregion

        #region Constructors

        public ChronometricValueId(Guid projectGuid, long revisionNumber, Guid chronometricValueGuid)
        {
            m_ProjectMemberId = new ProjectMemberId(projectGuid, revisionNumber);
            m_ChronometricValueGuid = chronometricValueGuid;
        }

        #endregion

        #region Properties

        public ProjectMemberId ProjectMemberId
        { get { return m_ProjectMemberId; } }

        public Guid ProjectGuid
        { get { return m_ProjectMemberId.ProjectGuid; } }

        public bool IsRevisionSpecific
        { get { return m_ProjectMemberId.IsRevisionSpecific; } }

        public Nullable<long> RevisionNumber
        { get { return m_ProjectMemberId.RevisionNumber; } }

        public long RevisionNumber_NonNull
        { get { return m_ProjectMemberId.RevisionNumber_NonNull; } }

        public Guid ChronometricValueGuid
        { get { return m_ChronometricValueGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ChronometricValueId))
            { return false; }

            var objTyped = (ChronometricValueId)obj;
            var objProjectMemberId = objTyped.GetProjectMemberId();

            if (objProjectMemberId != m_ProjectMemberId)
            { return false; }
            if (objTyped.ChronometricValueGuid != m_ChronometricValueGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ProjectMemberId.ToString();
            string item2 = TypedIdUtils.StructToString(ChronometricValueGuid_Prefix, ChronometricValueGuid);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(ChronometricValueId a, ChronometricValueId b)
        { return a.Equals(b); }

        public static bool operator !=(ChronometricValueId a, ChronometricValueId b)
        { return !(a == b); }

        public static ChronometricValueId NewId(Guid projectGuid, long revisionNumber)
        { return new ChronometricValueId(projectGuid, revisionNumber, Guid.NewGuid()); }

        #endregion

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ChronometricValueId))
            { return false; }

            var objTyped = (ChronometricValueId)obj;
            var objProjectMemberId = objTyped.GetProjectMemberId();

            if (!objProjectMemberId.Equals_Revisionless(m_ProjectMemberId))
            { return false; }
            if (objTyped.ChronometricValueGuid != m_ChronometricValueGuid)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = ProjectMemberId.ToString_Revisionless();
            string item2 = TypedIdUtils.StructToString(ChronometricValueGuid_Prefix, ChronometricValueGuid);

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
        { return ChronometricValueGuid; }

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
            get { return ModelObjectType.TimeMatrix; }
        }

        public Guid ModelObjectId
        {
            get { return ChronometricValueGuid; }
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