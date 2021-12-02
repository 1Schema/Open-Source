using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain
{
    public struct VariableMemberId : IVariableMember, IProjectMember_Revisionless, IModelObjectWithRef, IConvertible, IComparable
    {
        #region Static Members

        public static readonly long MinRevisionNumber = IVariableMemberUtils.RevisionNumber_Min;
        public static readonly long MaxRevisionNumber = IVariableMemberUtils.RevisionNumber_Max;

        public static readonly Guid EmptyProjectGuid = IVariableMemberUtils.ProjectGuid_Empty;
        public static readonly long EmptyRevisionNumber = IVariableMemberUtils.RevisionNumber_Empty;
        public static readonly int ModelTemplateNumber_Empty = IVariableMemberUtils.ModelTemplateNumber_Empty;
        public static readonly Nullable<Guid> ModelInstanceGuid_Null = IVariableMemberUtils.ModelInstanceGuid_Null;
        public static readonly Guid ModelInstanceGuid_Empty = IVariableMemberUtils.ModelInstanceGuid_Empty;
        public static readonly int VariableTemplateNumber_Empty = IVariableMemberUtils.VariableTemplateNumber_Empty;
        public static readonly Nullable<Guid> VariableInstanceGuid_Null = IVariableMemberUtils.VariableInstanceGuid_Null;
        public static readonly Nullable<Guid> VariableInstanceGuid_Empty = IVariableMemberUtils.VariableInstanceGuid_Empty;

        public static readonly VariableMemberId DefaultId = (!ModelInstanceGuid_Null.HasValue) ? new VariableMemberId(EmptyProjectGuid, EmptyRevisionNumber, ModelTemplateNumber_Empty, VariableTemplateNumber_Empty) : new VariableMemberId(EmptyProjectGuid, EmptyRevisionNumber, ModelTemplateNumber_Empty, ModelInstanceGuid_Null.Value, VariableTemplateNumber_Empty, VariableInstanceGuid_Null.Value);

        #endregion

        #region Members

        private ModelMemberId m_ModelMemberId;
        private int m_VariableTemplateNumber;
        private Nullable<Guid> m_VariableInstanceGuid;

        #endregion

        #region Constructors

        public VariableMemberId(Guid projectGuid, Nullable<long> revisionNumber, int modelTemplateNumber, int variableTemplateNumber)
        {
            m_ModelMemberId = new ModelMemberId(projectGuid, revisionNumber, modelTemplateNumber);
            m_VariableTemplateNumber = variableTemplateNumber;
            m_VariableInstanceGuid = null;
        }

        public VariableMemberId(Guid projectGuid, Nullable<long> revisionNumber, int modelTemplateNumber, Guid modelInstanceGuid, int variableTemplateNumber, Guid variableInstanceGuid)
        {
            m_ModelMemberId = new ModelMemberId(projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid);
            m_VariableTemplateNumber = variableTemplateNumber;
            m_VariableInstanceGuid = variableInstanceGuid;
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

        public int VariableTemplateNumber
        { get { return m_VariableTemplateNumber; } }

        public Nullable<Guid> VariableInstanceGuid
        { get { return m_VariableInstanceGuid; } }

        public bool MatchesVariableTemplate(IVariableMember variableTemplate, bool checkRevision)
        {
            if (variableTemplate.ProjectGuid != this.ProjectGuid)
            { return false; }
            if (checkRevision)
            {
                if (variableTemplate.RevisionNumber != this.RevisionNumber)
                { return false; }
            }
            if (variableTemplate.ModelTemplateNumber != this.ModelTemplateNumber)
            { return false; }
            if (variableTemplate.VariableTemplateNumber != this.VariableTemplateNumber)
            { return false; }
            return true;
        }

        public bool MatchesVariableInstance(IVariableMember variableInstance, bool checkRevision)
        {
            if (!this.MatchesVariableTemplate(variableInstance, checkRevision))
            { return false; }
            if (variableInstance.VariableInstanceGuid != this.VariableInstanceGuid)
            { return false; }
            return true;
        }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IVariableMember))
            { return false; }

            var objAsMember = (IVariableMember)obj;
            var objModelMemberId = objAsMember.GetModelMemberId();

            if (objModelMemberId != m_ModelMemberId)
            { return false; }
            if (objAsMember.VariableTemplateNumber != m_VariableTemplateNumber)
            { return false; }
            if (objAsMember.VariableInstanceGuid != m_VariableInstanceGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ModelMemberId.ToString();
            string item2 = TypedIdUtils.StructToString(IVariableMemberUtils.VariableTemplateNumber_Prefix, VariableTemplateNumber);
            string item3 = TypedIdUtils.NStructToString(IVariableMemberUtils.VariableInstanceGuid_Prefix, VariableInstanceGuid);

            string value = string.Format(ConversionUtils.ThreeItemListFormat, item1, item2, item3);
            return value;
        }

        public static bool operator ==(VariableMemberId a, VariableMemberId b)
        { return a.Equals(b); }

        public static bool operator !=(VariableMemberId a, VariableMemberId b)
        { return !(a == b); }

        #endregion

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }

            var objAsMember = (obj as IVariableMember);

            if (objAsMember == null)
            { return false; }

            var objModelMemberId = objAsMember.GetModelMemberId();

            if (!objModelMemberId.Equals_Revisionless(m_ModelMemberId))
            { return false; }
            if (objAsMember.VariableTemplateNumber != m_VariableTemplateNumber)
            { return false; }
            if (objAsMember.VariableInstanceGuid != m_VariableInstanceGuid)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = ModelMemberId.ToString_Revisionless();
            string item2 = TypedIdUtils.StructToString(IVariableMemberUtils.VariableTemplateNumber_Prefix, VariableTemplateNumber);
            string item3 = TypedIdUtils.NStructToString(IVariableMemberUtils.VariableInstanceGuid_Prefix, VariableInstanceGuid);

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
            return VariableMemberId.CompareTo(this, obj);
        }

        public static int CompareTo(object obj1, object obj2)
        {
            if (obj1 == null)
            { return -1; }
            if (!(obj1 is IVariableMember))
            { return -1; }

            if (obj2 == null)
            { return 1; }
            if (!(obj2 is IVariableMember))
            { return 1; }

            var variableMember1 = (IVariableMember)obj1;
            var variableMember2 = (IVariableMember)obj2;

            int projectMemberResult = ProjectMemberId.CompareTo(variableMember1, variableMember2);
            if (projectMemberResult != 0)
            { return projectMemberResult; }

            int modelMemberResult = ModelMemberId.CompareTo(variableMember1, variableMember2);
            if (modelMemberResult != 0)
            { return modelMemberResult; }

            int structuralTypeNumberResult = variableMember1.VariableTemplateNumber.CompareTo(variableMember2.VariableTemplateNumber);
            if (structuralTypeNumberResult != 0)
            { return structuralTypeNumberResult; }

            if (!variableMember1.IsInstance)
            { return 0; }

            int structuralInstanceGuidResult = variableMember1.VariableInstanceGuid.Value.CompareTo(variableMember2.VariableInstanceGuid.Value);
            if (structuralInstanceGuidResult != 0)
            { return structuralInstanceGuidResult; }

            return 0;
        }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return !IsInstance ? ModelObjectType.VariableTemplate : ModelObjectType.VariableInstance; }
        }

        public Guid ModelObjectId
        {
            get { return !IsInstance ? VariableTemplateNumber.ConvertIntToGuid() : VariableInstanceGuid.Value; }
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
                return VariableTemplateNumber;
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