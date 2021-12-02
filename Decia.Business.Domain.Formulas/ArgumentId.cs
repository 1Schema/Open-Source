using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Formulas
{
    public struct ArgumentId : IProjectMember, IConvertible
    {
        #region Static Members

        public static readonly string ArgumentIndex_PropName = ClassReflector.GetPropertyName((ArgumentId x) => x.ArgumentIndex);
        public static readonly string ArgumentIndex_Prefix = KeyProcessingModeUtils.GetModalDebugText(ArgumentIndex_PropName);

        public static readonly int EmptyArgumentIndex = 0;
        public static readonly ArgumentId DefaultId = new ArgumentId(ExpressionId.DefaultId.ProjectGuid, ExpressionId.DefaultId.RevisionNumber_NonNull, ExpressionId.DefaultId.FormulaGuid, ExpressionId.DefaultId.ExpressionGuid, EmptyArgumentIndex);

        #endregion

        #region Members

        private ExpressionId m_ExpressionId;
        private int m_ArgumentIndex;

        #endregion

        #region Constructors

        public ArgumentId(FormulaId formulaId, Guid expressionGuid, int argumentIndex)
            : this(formulaId.ProjectGuid, formulaId.RevisionNumber_NonNull, formulaId.FormulaGuid, expressionGuid, argumentIndex)
        { }

        public ArgumentId(ExpressionId expressionId, int argumentIndex)
            : this(expressionId.ProjectGuid, expressionId.RevisionNumber_NonNull, expressionId.FormulaGuid, expressionId.ExpressionGuid, argumentIndex)
        { }

        public ArgumentId(Guid projectGuid, long revisionNumber, Guid formulaGuid, Guid expressionGuid, int argumentIndex)
        {
            m_ExpressionId = new ExpressionId(projectGuid, revisionNumber, formulaGuid, expressionGuid);
            m_ArgumentIndex = argumentIndex;
        }

        #endregion

        #region Properties

        public ProjectMemberId ProjectMemberId
        { get { return m_ExpressionId.ProjectMemberId; } }

        public FormulaId FormulaId
        { get { return m_ExpressionId.FormulaId; } }

        public ExpressionId ExpressionId
        { get { return m_ExpressionId; } }

        public Guid ProjectGuid
        { get { return m_ExpressionId.ProjectGuid; } }

        public bool IsRevisionSpecific
        { get { return m_ExpressionId.IsRevisionSpecific; } }

        public Nullable<long> RevisionNumber
        { get { return m_ExpressionId.RevisionNumber; } }

        public long RevisionNumber_NonNull
        { get { return m_ExpressionId.RevisionNumber_NonNull; } }

        public Guid FormulaGuid
        { get { return m_ExpressionId.FormulaGuid; } }

        public Guid ExpressionGuid
        { get { return m_ExpressionId.ExpressionGuid; } }

        public int ArgumentIndex
        { get { return m_ArgumentIndex; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ArgumentId))
            { return false; }

            var objTyped = (ArgumentId)obj;
            var objExpressionId = objTyped.ExpressionId;

            if (objExpressionId != m_ExpressionId)
            { return false; }
            if (objTyped.ArgumentIndex != m_ArgumentIndex)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ExpressionId.ToString();
            string item2 = TypedIdUtils.ConvertToString(ArgumentIndex_Prefix, ArgumentIndex);
            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(ArgumentId a, ArgumentId b)
        { return a.Equals(b); }

        public static bool operator !=(ArgumentId a, ArgumentId b)
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