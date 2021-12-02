using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common
{
    public struct UserId : IUser, ISiteActor, IGuidId, IConvertible
    {
        #region Static Members

        public const SiteActorType ActorType = IUserUtils.ActorType;
        public static readonly Guid EmptyUserGuid = IUserUtils.EmptyUserGuid;

        public static readonly UserId DefaultId = new UserId(EmptyUserGuid);
        public static readonly UserId SystemUserId = PredefinedUserTypeUtils.SystemUserId;
        public static readonly UserId AnonymousUserId = PredefinedUserTypeUtils.AnonymousUserId;

        #endregion

        #region Members

        private Guid m_UserGuid;

        #endregion

        #region Constructors

        public UserId(Guid userGuid)
        {
            m_UserGuid = userGuid;
        }

        #endregion

        #region Properties

        public Guid UserGuid
        { get { return m_UserGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IUser))
            { return false; }

            var objAsUser = (IUser)obj;

            if (objAsUser.UserGuid != m_UserGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(IUserUtils.UserGuid_Prefix, UserGuid);
            return item1;
        }

        public static UserId NewId()
        { return new UserId(Guid.NewGuid()); }

        public static bool operator ==(UserId a, UserId b)
        { return a.Equals(b); }

        public static bool operator !=(UserId a, UserId b)
        { return !(a == b); }

        #endregion

        #region ISiteActor Implementation

        SiteActorType ISiteActor.ActorType
        {
            get { return ActorType; }
        }

        Guid ISiteActor.ActorGuid
        {
            get { return UserGuid; }
        }

        #endregion

        #region IGuidId Implementation

        Guid IGuidId.InnerGuid
        {
            get { return UserGuid; }
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
        { return this.UserGuid; }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion
    }
}