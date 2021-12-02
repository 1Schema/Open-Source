using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public struct ModelMemberId : IModelMember, IProjectMember_Revisionless, IModelObjectWithRef, IConvertible, IComparable
    {
        #region Static Members

        public static readonly long MinRevisionNumber = IModelMemberUtils.RevisionNumber_Min;
        public static readonly long MaxRevisionNumber = IModelMemberUtils.RevisionNumber_Max;

        public static readonly Guid EmptyProjectGuid = IModelMemberUtils.ProjectGuid_Empty;
        public static readonly long EmptyRevisionNumber = IModelMemberUtils.RevisionNumber_Empty;
        public static readonly int ModelTemplateNumber_Empty = IModelMemberUtils.ModelTemplateNumber_Empty;
        public static readonly Nullable<Guid> ModelInstanceGuid_Null = IModelMemberUtils.ModelInstanceGuid_Null;
        public static readonly Guid ModelInstanceGuid_Empty = IModelMemberUtils.ModelInstanceGuid_Empty;

        public static readonly ModelMemberId DefaultId = (!ModelInstanceGuid_Null.HasValue) ? new ModelMemberId(EmptyProjectGuid, EmptyRevisionNumber, ModelTemplateNumber_Empty) : new ModelMemberId(EmptyProjectGuid, EmptyRevisionNumber, ModelTemplateNumber_Empty, ModelInstanceGuid_Null.Value);

        #endregion

        #region Members

        private ProjectMemberId m_ProjectMemberId;
        private int m_ModelTemplateNumber;
        private Nullable<Guid> m_ModelInstanceGuid;

        #endregion

        #region Constructors

        public ModelMemberId(Guid projectGuid, Nullable<long> revisionNumber, int modelTemplateNumber)
        {
            m_ProjectMemberId = new ProjectMemberId(projectGuid, revisionNumber);
            m_ModelTemplateNumber = modelTemplateNumber;
            m_ModelInstanceGuid = null;
        }

        public ModelMemberId(Guid projectGuid, Nullable<long> revisionNumber, int modelTemplateNumber, Guid modelInstanceGuid)
        {
            m_ProjectMemberId = new ProjectMemberId(projectGuid, revisionNumber);
            m_ModelTemplateNumber = modelTemplateNumber;
            m_ModelInstanceGuid = modelInstanceGuid;
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

        public bool IsInstance
        { get { return m_ModelInstanceGuid.HasValue; } }

        public int ModelTemplateNumber
        { get { return m_ModelTemplateNumber; } }

        public Nullable<Guid> ModelInstanceGuid
        { get { return m_ModelInstanceGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IModelMember))
            { return false; }

            var objAsMember = (IModelMember)obj;
            var objProjectMemberId = objAsMember.GetProjectMemberId();

            if (objProjectMemberId != m_ProjectMemberId)
            { return false; }
            if (objAsMember.ModelTemplateNumber != m_ModelTemplateNumber)
            { return false; }
            if (objAsMember.ModelInstanceGuid != m_ModelInstanceGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ProjectMemberId.ToString();
            string item2 = TypedIdUtils.StructToString(IModelMemberUtils.ModelTemplateNumber_Prefix, ModelTemplateNumber);
            string item3 = TypedIdUtils.NStructToString(IModelMemberUtils.ModelInstanceGuid_Prefix, ModelInstanceGuid);

            string value = string.Format(ConversionUtils.ThreeItemListFormat, item1, item2, item3);
            return value;
        }

        public static bool operator ==(ModelMemberId a, ModelMemberId b)
        { return a.Equals(b); }

        public static bool operator !=(ModelMemberId a, ModelMemberId b)
        { return !(a == b); }

        #endregion

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }

            var objAsMember = (obj as IModelMember);

            if (objAsMember == null)
            { return false; }

            var objProjectMemberId = objAsMember.GetProjectMemberId();

            if (!objProjectMemberId.Equals_Revisionless(m_ProjectMemberId))
            { return false; }
            if (objAsMember.ModelTemplateNumber != m_ModelTemplateNumber)
            { return false; }
            if (objAsMember.ModelInstanceGuid != m_ModelInstanceGuid)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = ProjectMemberId.ToString_Revisionless();
            string item2 = TypedIdUtils.StructToString(IModelMemberUtils.ModelTemplateNumber_Prefix, ModelTemplateNumber);
            string item3 = TypedIdUtils.NStructToString(IModelMemberUtils.ModelInstanceGuid_Prefix, ModelInstanceGuid);

            string value = string.Format(ConversionUtils.ThreeItemListFormat, item1, item2, item3);
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
            return ModelMemberId.CompareTo(this, obj);
        }

        public static int CompareTo(object obj1, object obj2)
        {
            if (obj1 == null)
            { return -1; }
            if (!(obj1 is IModelMember))
            { return -1; }

            if (obj2 == null)
            { return 1; }
            if (!(obj2 is IModelMember))
            { return 1; }

            var modelMember1 = (IModelMember)obj1;
            var modelMember2 = (IModelMember)obj2;

            int projectMemberResult = ProjectMemberId.CompareTo(modelMember1, modelMember2);
            if (projectMemberResult != 0)
            { return projectMemberResult; }

            int isInstanceResult = modelMember1.IsInstance.CompareTo(modelMember2.IsInstance);
            if (isInstanceResult != 0)
            { return isInstanceResult; }

            int modelTemplateNumberResult = modelMember1.ModelTemplateNumber.CompareTo(modelMember2.ModelTemplateNumber);
            if (modelTemplateNumberResult != 0)
            { return modelTemplateNumberResult; }

            if (!modelMember1.IsInstance)
            { return 0; }

            int modelInstanceGuidResult = modelMember1.ModelInstanceGuid.Value.CompareTo(modelMember2.ModelInstanceGuid.Value);
            if (modelInstanceGuidResult != 0)
            { return modelInstanceGuidResult; }

            return 0;
        }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return !IsInstance ? ModelObjectType.ModelTemplate : ModelObjectType.ModelInstance; }
        }

        public Guid ModelObjectId
        {
            get { return !IsInstance ? ModelTemplateNumber.ConvertIntToGuid() : ModelInstanceGuid.Value; }
        }

        public bool IdIsInt
        {
            get { return !IsInstance; }
        }

        public int ModelObjectIdAsInt
        {
            get
            {
                if (IsInstance)
                { throw new InvalidOperationException("A true Guid value cannot be converted to Int32."); }
                return ModelTemplateNumber;
            }
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