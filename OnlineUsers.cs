using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Web;
using System.Threading;

namespace ParsElecom.OnlineUsers
{
    public class OnlineUsersInstance
    {
        public static OnlineUsers OnlineUsers;
        static OnlineUsersInstance()
        {
            OnlineUsers = new OnlineUsers();
        }
    }

    /// <summary>
    /// Checks users online state
    /// </summary>
    public class OnlineUsers
    {
        private string _UnknownUser = "_'?Unknown\nUser?'_";
        private Hashtable fOnlineUsers = new Hashtable();
        private int _MembersCount = 0;
        private int _GuestsCount = 0;
        private Timer thTimer;

        /// <summary>
        /// 240000 = 4 minutes
        /// </summary>
        private const int _TimerPeriod = 240000;
        //int imtoi


        public OnlineUsers()
        {
            _UnknownUser += DateTime.Now.Ticks.ToString();
            // Timer will start after _TimerPeriod
            thTimer = new Timer(_ThreadTimerCallback, this, _TimerPeriod, _TimerPeriod);
        }

        #region Properties
        public int RegistredUsersCount
        {
            get { return _MembersCount; }
        }
        public int GuestUsersCount
        {
            get { return _GuestsCount; }
        }

        public int UsersCount
        {
            get
            {
                Monitor.Enter(fOnlineUsers);
                try
                {
                    return fOnlineUsers.Count;
                }
                finally
                {
                    Monitor.Exit(fOnlineUsers);
                }
            }
        }

        #endregion


        #region public

        public void UpdateForUserLeave()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                return;
            }

            if (context.User != null && context.User.Identity.IsAuthenticated)
            {
                SetOfflineMemberInternal(context.User.Identity.Name);
            }
            else if (context.Session != null)
            {
                SetOfflineGuestInternal(_UnknownUser + context.Session.SessionID);
            }
        }


        /// <summary>
        /// Check user online state
        /// </summary>
        public bool IsOnline(string userName)
        {
            Monitor.Enter(fOnlineUsers);
            try
            {
                return fOnlineUsers[userName] != null;
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }
        }

        public DateTime GetLastActivity(string userName)
        {
            object state;
            Monitor.Enter(fOnlineUsers);
            try
            {
                state = fOnlineUsers[userName];
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }
            return (DateTime)state;//new DateTime((long)state);
        }

        /// <summary>
        /// Set user state to online
        /// </summary>
        public void SetUserOnline(string userName)
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                SetOnlineMemberInternal(userName);
            }
            else
            {
                if (context.Session != null)
                {
                    // This user may are already in site
                    SetOfflineGuestInternal(_UnknownUser + context.Session.SessionID);
                }

                SetOnlineMemberInternal(userName);
            }
        }

        /// <summary>
        /// Make user offline
        /// </summary>
        public void SetUserOffline(string userName)
        {
            SetOfflineInternal(userName);
        }

        /// <summary>
        /// Adds user to the list
        /// </summary>
        internal void UpdateForNewUser()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                AddUnknownUser();
                return;
            }

            if (context.User != null && context.User.Identity.IsAuthenticated)
            {
                SetOnlineMemberInternal(context.User.Identity.Name);
            }
            else if (context.Session != null)
            {
                SetOnlineGuestInternal(_UnknownUser + context.Session.SessionID);
            }
            else
            {
                AddUnknownUser();
            }
        }

        internal void UpldateUserActivity()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
                return;

            if (context.User != null && context.User.Identity.IsAuthenticated)
            {
                string userName = context.User.Identity.Name;
                SetOnlineMemberInternal(userName);
            }
            else if (context.Session != null)
            {
                string sessionID = _UnknownUser + context.Session.SessionID;
                SetOnlineGuestInternal(sessionID);
            }
        }
        #endregion

        #region private

        /// <summary>
        /// This callback should recheck all the uesrs list
        /// </summary>
        /// <param name="state"></param>
        private void _ThreadTimerCallback(Object state)
        {
            DateTime now = DateTime.Now;
            ArrayList expired = new ArrayList(fOnlineUsers.Count);

            Monitor.Enter(fOnlineUsers);
            try
            {
                // Searching for expired users
                foreach (DictionaryEntry entry in fOnlineUsers)
                {
                    if (((TimeSpan)(now - (DateTime)entry.Value)).TotalMilliseconds > _TimerPeriod)
                    {
                        expired.Add(entry.Key);
                    }
                }
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }

            // Remove expired items
            for (int i = 0; i < expired.Count; i++)
            {
                string user = expired[i].ToString();
                SetOfflineInternal(user);
            }
        }

        private void AddUnknownUser()
        {
            Monitor.Enter(fOnlineUsers);
            try
            {
                fOnlineUsers[_UnknownUser + Guid.NewGuid().ToString()] = DateTime.Now;
                IncreaseGuestsCount();
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }
        }

        private void SetOnlineMemberInternal(string userName)
        {
            Monitor.Enter(fOnlineUsers);
            try
            {
                if (fOnlineUsers.ContainsKey(userName) == false)
                    IncreaseMembersCount();
                fOnlineUsers[userName] = DateTime.Now;
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }
        }

        private void SetOnlineGuestInternal(string identity)
        {
            Monitor.Enter(fOnlineUsers);
            try
            {
                if (fOnlineUsers.ContainsKey(identity) == false)
                    IncreaseGuestsCount();
                fOnlineUsers[identity] = DateTime.Now;
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }
        }

        private void SetOfflineInternal(string identity)
        {
            Monitor.Enter(fOnlineUsers);
            try
            {
                if (fOnlineUsers.ContainsKey(identity))
                {
                    fOnlineUsers.Remove(identity);

                    if (identity.StartsWith(_UnknownUser))
                        DecreaseGuestsCount();
                    else
                        DecreaseMembersCount();
                }
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }
        }

        private void SetOfflineMemberInternal(string userName)
        {
            Monitor.Enter(fOnlineUsers);
            try
            {
                if (fOnlineUsers.ContainsKey(userName))
                {
                    fOnlineUsers.Remove(userName);
                    DecreaseMembersCount();
                }
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }
        }

        private void SetOfflineGuestInternal(string identity)
        {
            Monitor.Enter(fOnlineUsers);
            try
            {
                if (fOnlineUsers.ContainsKey(identity))
                {
                    fOnlineUsers.Remove(identity);
                    DecreaseGuestsCount();
                }
            }
            finally
            {
                Monitor.Exit(fOnlineUsers);
            }
        }

        private void DecreaseMembersCount()
        {
            _MembersCount--;
            if (_MembersCount < 0)
                _MembersCount = 0;
        }

        private void DecreaseGuestsCount()
        {
            _GuestsCount--;
            if (_GuestsCount < 0)
                _GuestsCount = 0;
        }
        private void IncreaseMembersCount()
        {
            _MembersCount++;
        }

        private void IncreaseGuestsCount()
        {
            _GuestsCount++;
        }
        #endregion

    }
}
