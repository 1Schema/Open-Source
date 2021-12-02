using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common
{
    public struct EmailAddressId : IConvertible
    {
        #region Static Members

        public static readonly string EmailAddress_PropName = ClassReflector.GetPropertyName((EmailAddressId x) => x.EmailAddress);
        public static readonly string EmailAddress_Prefix = KeyProcessingModeUtils.GetModalDebugText(EmailAddress_PropName);

        #endregion

        #region Members

        private string m_EmailAddress;

        #endregion

        #region Constructors

        public EmailAddressId(string emailAddress)
        {
            emailAddress.AssertEmailAddressIsValid();
            m_EmailAddress = emailAddress.ToNormalized_EmailPart();
        }

        #endregion

        #region Properties

        public string EmailAddress
        { get { return m_EmailAddress; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is EmailAddressId))
            { return false; }

            var objAsEmailAddressId = (EmailAddressId)obj;

            if (objAsEmailAddressId.EmailAddress != EmailAddress)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            return m_EmailAddress;
        }

        public static bool operator ==(EmailAddressId a, EmailAddressId b)
        { return a.Equals(b); }

        public static bool operator !=(EmailAddressId a, EmailAddressId b)
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
        { throw new NotImplementedException(); }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion
    }
}