using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Formulas
{
    public struct ExpressionId : IProjectMember, IConvertible
    {
        #region Static Members

        public static readonly string ExpressionGuid_PropName = ClassReflector.GetPropertyName((ExpressionId x) => x.ExpressionGuid);
        public static readonly string ExpressionGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(ExpressionGuid_PropName);

        public static readonly Guid EmptyExpressionGuid = Guid.Empty;
        public static readonly ExpressionId DefaultId = new ExpressionId(FormulaId.DefaultId.ProjectGuid, FormulaId.DefaultId.RevisionNumber_NonNull, FormulaId.DefaultId.FormulaGuid, EmptyExpressionGuid);

        #endregion

        #region Members

        private FormulaId m_FormulaId;
        private Guid m_ExpressionGuid;

        #endregion

        #region Constructors

        public ExpressionId(FormulaId formulaId, Guid expressionGuid)
            : this(formulaId.ProjectGuid, formulaId.RevisionNumber_NonNull, formulaId.FormulaGuid, expressionGuid)
        { }

        public ExpressionId(Guid projectGuid, long revisionNumber, Guid formulaGuid, Guid expressionGuid)
        {
            m_FormulaId = new FormulaId(projectGuid, revisionNumber, formulaGuid);
            m_ExpressionGuid = expressionGuid;
        }

        #endregion

        #region Properties

        public ProjectMemberId ProjectMemberId
        { get { return m_FormulaId.ProjectMemberId; } }

        public FormulaId FormulaId
        { get { return m_FormulaId; } }

        public Guid ProjectGuid
        { get { return m_FormulaId.ProjectGuid; } }

        public bool IsRevisionSpecific
        { get { return m_FormulaId.IsRevisionSpecific; } }

        public Nullable<long> RevisionNumber
        { get { return m_FormulaId.RevisionNumber; } }

        public long RevisionNumber_NonNull
        { get { return m_FormulaId.RevisionNumber_NonNull; } }

        public Guid FormulaGuid
        { get { return m_FormulaId.FormulaGuid; } }

        public Guid ExpressionGuid
        { get { return m_ExpressionGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ExpressionId))
            { return false; }

            var objTyped = (ExpressionId)obj;
            var objFormulaId = objTyped.FormulaId;

            if (objFormulaId != m_FormulaId)
            { return false; }
            if (objTyped.ExpressionGuid != m_ExpressionGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = FormulaId.ToString();
            string item2 = TypedIdUtils.ConvertToString(ExpressionGuid_Prefix, ExpressionGuid);
            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(ExpressionId a, ExpressionId b)
        { return a.Equals(b); }

        public static bool operator !=(ExpressionId a, ExpressionId b)
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
    }
}