using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.CompoundUnits
{
    public struct CompoundUnitId : IProjectMember, IProjectMember_Revisionless, IModelObjectWithRef, IConvertible
    {
        #region Static Members

        public static readonly string CompoundUnitGuid_PropName = ClassReflector.GetPropertyName((CompoundUnitId x) => x.CompoundUnitGuid);
        public static readonly string CompoundUnitGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(CompoundUnitGuid_PropName);

        public static readonly Guid EmptyCompoundUnitGuid = Guid.Empty;
        public static readonly CompoundUnitId DefaultId = new CompoundUnitId(ProjectMemberId.EmptyProjectGuid, ProjectMemberId.EmptyRevisionNumber, EmptyCompoundUnitGuid);

        #endregion

        #region Members

        private ProjectMemberId m_ProjectMemberId;
        private Guid m_CompoundUnitGuid;

        #endregion

        #region Constructors

        public CompoundUnitId(Guid projectGuid, long revisionNumber, Guid compoundUnitGuid)
        {
            m_ProjectMemberId = new ProjectMemberId(projectGuid, revisionNumber);
            m_CompoundUnitGuid = compoundUnitGuid;
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

        public Guid CompoundUnitGuid
        { get { return m_CompoundUnitGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is CompoundUnitId))
            { return false; }

            var objTyped = (CompoundUnitId)obj;
            var objProjectMemberId = objTyped.GetProjectMemberId();

            if (objProjectMemberId != m_ProjectMemberId)
            { return false; }
            if (objTyped.CompoundUnitGuid != m_CompoundUnitGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ProjectMemberId.ToString();
            string item2 = TypedIdUtils.StructToString(CompoundUnitGuid_Prefix, CompoundUnitGuid);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(CompoundUnitId a, CompoundUnitId b)
        { return a.Equals(b); }

        public static bool operator !=(CompoundUnitId a, CompoundUnitId b)
        { return !(a == b); }

        public static CompoundUnitId NewId(Guid projectGuid, long revisionNumber)
        { return new CompoundUnitId(projectGuid, revisionNumber, Guid.NewGuid()); }

        #endregion

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is CompoundUnitId))
            { return false; }

            var objTyped = (CompoundUnitId)obj;
            var objProjectMemberId = objTyped.GetProjectMemberId();

            if (!objProjectMemberId.Equals_Revisionless(m_ProjectMemberId))
            { return false; }
            if (objTyped.CompoundUnitGuid != m_CompoundUnitGuid)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = ProjectMemberId.ToString_Revisionless();
            string item2 = TypedIdUtils.StructToString(CompoundUnitGuid_Prefix, CompoundUnitGuid);

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
        { return CompoundUnitGuid; }

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
            get { return ModelObjectType.Unit; }
        }

        public Guid ModelObjectId
        {
            get { return CompoundUnitGuid; }
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