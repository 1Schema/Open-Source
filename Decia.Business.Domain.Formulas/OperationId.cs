using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Formulas
{
    public struct OperationId : IGuidId, IConvertible
    {
        #region Static Members

        public static readonly string OperationGuid_PropName = ClassReflector.GetPropertyName((OperationId x) => x.OperationGuid);
        public static readonly string OperationGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(OperationGuid_PropName);

        #endregion

        #region Members

        private Guid m_OperationGuid;

        #endregion

        #region Constructors

        public OperationId(Guid innerGuid)
        { m_OperationGuid = innerGuid; }

        #endregion

        #region Properties

        public Guid OperationGuid
        { get { return m_OperationGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        { return this.AreEqual(obj); }

        public override int GetHashCode()
        { return m_OperationGuid.GetHashCode(); }

        public override string ToString()
        { return TypedIdUtils.ConvertToString(OperationGuid_Prefix, m_OperationGuid); }

        public static bool operator ==(OperationId a, OperationId b)
        { return a.Equals(b); }

        public static bool operator !=(OperationId a, OperationId b)
        { return !(a == b); }

        public static OperationId NewId()
        { return new OperationId(Guid.NewGuid()); }

        #endregion

        #region IGuidId Implementation

        Guid IGuidId.InnerGuid
        {
            get { return OperationGuid; }
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
        { return OperationGuid; }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion
    }
}