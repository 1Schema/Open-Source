using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common
{
    public struct NotificationId : IGuidId, IConvertible
    {
        #region Static Members

        public static readonly string NotificationGuid_PropName = ClassReflector.GetPropertyName((NotificationId x) => x.NotificationGuid);
        public static readonly string NotificationGuid_Prefix = KeyProcessingModeUtils.GetModalDebugText(NotificationGuid_PropName);

        #endregion

        #region Members

        private Guid m_NotificationGuid;

        #endregion

        #region Constructors

        public NotificationId(Guid notificationGuid)
        {
            m_NotificationGuid = notificationGuid;
        }

        #endregion

        #region Properties

        public Guid NotificationGuid
        { get { return m_NotificationGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        { return this.AreEqual(obj); }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        { return TypedIdUtils.ConvertToString(NotificationGuid_Prefix, m_NotificationGuid); }

        public static NotificationId NewId()
        { return new NotificationId(Guid.NewGuid()); }

        public static bool operator ==(NotificationId a, NotificationId b)
        { return a.Equals(b); }

        public static bool operator !=(NotificationId a, NotificationId b)
        { return !(a == b); }

        #endregion

        #region IGuidId Implementation

        Guid IGuidId.InnerGuid
        {
            get { return NotificationGuid; }
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
        { return this.NotificationGuid; }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion
    }
}