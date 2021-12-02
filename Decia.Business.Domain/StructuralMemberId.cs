using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public struct StructuralMemberId : IStructuralMember, IProjectMember_Revisionless, IModelObjectWithRef, IConvertible, IComparable
    {
        #region Static Members

        public static readonly long MinRevisionNumber = IStructuralMemberUtils.RevisionNumber_Min;
        public static readonly long MaxRevisionNumber = IStructuralMemberUtils.RevisionNumber_Max;

        public static readonly Guid EmptyProjectGuid = IStructuralMemberUtils.ProjectGuid_Empty;
        public static readonly long EmptyRevisionNumber = IStructuralMemberUtils.RevisionNumber_Empty;
        public static readonly int ModelTemplateNumber_Empty = IStructuralMemberUtils.ModelTemplateNumber_Empty;
        public static readonly Nullable<Guid> ModelInstanceGuid_Null = IStructuralMemberUtils.ModelInstanceGuid_Null;
        public static readonly Guid ModelInstanceGuid_Empty = IStructuralMemberUtils.ModelInstanceGuid_Empty;
        public static readonly StructuralTypeOption StructuralTypeOption_Empty = IStructuralMemberUtils.StructuralTypeOption_Empty;
        public static readonly int StructuralTypeNumber_Empty = IStructuralMemberUtils.StructuralTypeNumber_Empty;
        public static readonly Nullable<Guid> StructuralInstanceGuid_Null = IStructuralMemberUtils.StructuralInstanceGuid_Null;
        public static readonly Guid StructuralInstanceGuid_Empty = IStructuralMemberUtils.StructuralInstanceGuid_Empty;

        public static readonly StructuralMemberId DefaultId = (!ModelInstanceGuid_Null.HasValue) ? new StructuralMemberId(EmptyProjectGuid, EmptyRevisionNumber, ModelTemplateNumber_Empty, StructuralTypeOption_Empty, StructuralTypeNumber_Empty) : new StructuralMemberId(EmptyProjectGuid, EmptyRevisionNumber, ModelTemplateNumber_Empty, ModelInstanceGuid_Null.Value, StructuralTypeOption_Empty, StructuralTypeNumber_Empty, StructuralInstanceGuid_Null.Value);

        #endregion

        #region Members

        private ModelMemberId m_ModelMemberId;
        private StructuralTypeOption m_StructuralTypeOption;
        private int m_StructuralTypeNumber;
        private Nullable<Guid> m_StructuralInstanceGuid;

        #endregion

        #region Constructors

        public StructuralMemberId(Guid projectGuid, Nullable<long> revisionNumber, int modelTemplateNumber, StructuralTypeOption structuralTypeOption, int structuralTypeNumber)
        {
            m_ModelMemberId = new ModelMemberId(projectGuid, revisionNumber, modelTemplateNumber);
            m_StructuralTypeOption = structuralTypeOption;
            m_StructuralTypeNumber = structuralTypeNumber;
            m_StructuralInstanceGuid = null;
        }

        public StructuralMemberId(Guid projectGuid, Nullable<long> revisionNumber, int modelTemplateNumber, Guid modelInstanceGuid, StructuralTypeOption structuralTypeOption, int structuralTypeNumber, Guid structuralInstanceGuid)
        {
            m_ModelMemberId = new ModelMemberId(projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid);
            m_StructuralTypeOption = structuralTypeOption;
            m_StructuralTypeNumber = structuralTypeNumber;
            m_StructuralInstanceGuid = structuralInstanceGuid;
        }

        #endregion

        #region Properties

        public ProjectMemberId ProjectMemberId
        { get { return m_ModelMemberId.ProjectMemberId; } }

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

        public StructuralTypeOption StructuralTypeOption
        { get { return m_StructuralTypeOption; } }

        public int StructuralTypeNumber
        { get { return m_StructuralTypeNumber; } }

        public Nullable<StructuralInstanceOption> StructuralInstanceOption
        { get { return IsInstance ? m_StructuralTypeOption.GetStructuralInstance() : (Nullable<StructuralInstanceOption>)null; } }

        public Nullable<Guid> StructuralInstanceGuid
        { get { return m_StructuralInstanceGuid; } }

        public bool MatchesStructuralType(IStructuralMember structuralType, bool checkRevision)
        {
            if (structuralType.ProjectGuid != this.ProjectGuid)
            { return false; }
            if (checkRevision)
            {
                if (structuralType.RevisionNumber != this.RevisionNumber)
                { return false; }
            }
            if (structuralType.ModelTemplateNumber != this.ModelTemplateNumber)
            { return false; }
            if (structuralType.StructuralTypeOption != this.StructuralTypeOption)
            { return false; }
            if (structuralType.StructuralTypeNumber != this.StructuralTypeNumber)
            { return false; }
            return true;
        }

        public bool MatchesModelInstance(IModelMember modelInstance, bool checkRevision)
        {
            if (!modelInstance.IsInstance)
            { throw new InvalidOperationException("The specified value is not a ModelInstance."); }

            if (modelInstance.ProjectGuid != this.ProjectGuid)
            { return false; }
            if (checkRevision)
            {
                if (modelInstance.RevisionNumber != this.RevisionNumber)
                { return false; }
            }
            if (modelInstance.ModelTemplateNumber != this.ModelTemplateNumber)
            { return false; }
            if (modelInstance.ModelInstanceGuid != this.ModelInstanceGuid)
            { return false; }
            return true;
        }

        public bool MatchesStructuralInstance(IStructuralMember structuralInstance, bool checkRevision)
        {
            if (!this.MatchesStructuralType(structuralInstance, checkRevision))
            { return false; }
            if (!this.MatchesModelInstance(structuralInstance, checkRevision))
            { return false; }

            if (!structuralInstance.StructuralInstanceGuid.HasValue)
            { throw new InvalidOperationException("The specified value is not an Instance."); }
            if (structuralInstance.StructuralInstanceGuid.Value != this.StructuralInstanceGuid)
            { return false; }

            return true;
        }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IStructuralMember))
            { return false; }

            var objAsMember = (IStructuralMember)obj;
            var objModelMemberId = objAsMember.GetModelMemberId();

            if (objModelMemberId != m_ModelMemberId)
            { return false; }
            if (objAsMember.StructuralTypeOption != m_StructuralTypeOption)
            { return false; }
            if (objAsMember.StructuralTypeNumber != m_StructuralTypeNumber)
            { return false; }
            if (objAsMember.StructuralInstanceGuid != m_StructuralInstanceGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ModelMemberId.ToString();
            string item2 = TypedIdUtils.StructToString(IStructuralMemberUtils.StructuralTypeOption_Prefix, StructuralTypeOption);
            string item3 = TypedIdUtils.StructToString(IStructuralMemberUtils.StructuralTypeNumber_Prefix, StructuralTypeNumber);
            string item4 = TypedIdUtils.NStructToString(IStructuralMemberUtils.StructuralInstanceGuid_Prefix, StructuralInstanceGuid);

            string value = string.Format(ConversionUtils.FourItemListFormat, item1, item2, item3, item4);
            return value;
        }

        public static bool operator ==(StructuralMemberId a, StructuralMemberId b)
        { return a.Equals(b); }

        public static bool operator !=(StructuralMemberId a, StructuralMemberId b)
        { return !(a == b); }

        #endregion

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }

            var objAsMember = (obj as IStructuralMember);

            if (objAsMember == null)
            { return false; }

            var objModelMemberId = objAsMember.GetModelMemberId();

            if (!objModelMemberId.Equals_Revisionless(m_ModelMemberId))
            { return false; }
            if (objAsMember.StructuralTypeOption != m_StructuralTypeOption)
            { return false; }
            if (objAsMember.StructuralTypeNumber != m_StructuralTypeNumber)
            { return false; }
            if (objAsMember.StructuralInstanceGuid != m_StructuralInstanceGuid)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = ModelMemberId.ToString_Revisionless();
            string item2 = TypedIdUtils.StructToString(IStructuralMemberUtils.StructuralTypeOption_Prefix, StructuralTypeOption);
            string item3 = TypedIdUtils.StructToString(IStructuralMemberUtils.StructuralTypeNumber_Prefix, StructuralTypeNumber);
            string item4 = TypedIdUtils.NStructToString(IStructuralMemberUtils.StructuralInstanceGuid_Prefix, StructuralInstanceGuid);

            string value = string.Format(ConversionUtils.FourItemListFormat, item1, item2, item3, item4);
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
            return StructuralMemberId.CompareTo(this, obj);
        }

        public static int CompareTo(object obj1, object obj2)
        {
            if (obj1 == null)
            { return -1; }
            if (!(obj1 is IStructuralMember))
            { return -1; }

            if (obj2 == null)
            { return 1; }
            if (!(obj2 is IStructuralMember))
            { return 1; }

            var structuralMember1 = (IStructuralMember)obj1;
            var structuralMember2 = (IStructuralMember)obj2;

            int projectMemberResult = ProjectMemberId.CompareTo(structuralMember1, structuralMember2);
            if (projectMemberResult != 0)
            { return projectMemberResult; }

            int modelMemberResult = ModelMemberId.CompareTo(structuralMember1, structuralMember2);
            if (modelMemberResult != 0)
            { return modelMemberResult; }

            int structuralTypeOptionResult = structuralMember1.StructuralTypeOption.CompareTo(structuralMember2.StructuralTypeOption);
            if (structuralTypeOptionResult != 0)
            { return structuralTypeOptionResult; }

            int structuralTypeNumberResult = structuralMember1.StructuralTypeNumber.CompareTo(structuralMember2.StructuralTypeNumber);
            if (structuralTypeNumberResult != 0)
            { return structuralTypeNumberResult; }

            if (!structuralMember1.IsInstance)
            { return 0; }

            int structuralInstanceGuidResult = structuralMember1.StructuralInstanceGuid.Value.CompareTo(structuralMember2.StructuralInstanceGuid.Value);
            if (structuralInstanceGuidResult != 0)
            { return structuralInstanceGuidResult; }

            return 0;
        }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return !IsInstance ? StructuralTypeOption.GetModelObjectType() : StructuralInstanceOption.Value.GetModelObjectType(); }
        }

        public Guid ModelObjectId
        {
            get { return !IsInstance ? StructuralTypeNumber.ConvertIntToGuid() : StructuralInstanceGuid.Value; }
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
                return StructuralTypeNumber;
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