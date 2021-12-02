using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public class UserState : IUserState
    {
        #region Static Members

        [ThreadStatic]
        protected static UserState s_CurrentThreadState = null;

        public static UserState CurrentThreadState
        {
            get { return s_CurrentThreadState; }
            set { s_CurrentThreadState = value; }
        }

        public static UserId CurrentThreadUserId
        {
            get
            {
                if (CurrentThreadState == null)
                { return PredefinedUserType.Anonymous.GetUserId_ForUserType(); }
                return CurrentThreadState.CurrentUserId;
            }
        }

        #endregion

        #region Constants and Static Members

        public const bool ApplyEscapeChars_Default = false;

        #endregion

        #region Members

        private Guid m_UserGuid;
        private string m_Username;
        private bool m_ApplyEscapeChars;

        #endregion

        #region Constructors

        public UserState(UserId userId)
            : this(userId.UserGuid)
        { }

        public UserState(UserId userId, string username)
            : this(userId.UserGuid, username)
        { }

        public UserState(Guid userGuid)
            : this(userGuid, userGuid.ToString())
        { }

        public UserState(Guid userGuid, string username)
        {
            m_UserGuid = userGuid;
            m_Username = username;
            m_ApplyEscapeChars = ApplyEscapeChars_Default;
        }

        #endregion

        #region IUserState Implementation

        public UserId CurrentUserId
        {
            get { return new UserId(m_UserGuid); }
        }

        public Guid CurrentUserGuid
        {
            get { return m_UserGuid; }
        }

        public string CurrentUsername
        {
            get { return m_Username; }
        }

        public bool ApplyEscapeChars
        {
            get { return m_ApplyEscapeChars; }
            set { m_ApplyEscapeChars = value; }
        }

        #endregion
    }
}