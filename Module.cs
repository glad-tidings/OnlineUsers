using System;
using System.Web;
using System.Collections.Generic;
using System.Text;
using System.Web.SessionState;

namespace ParsElecom.OnlineUsers
{
    public class OnlineUsersModule : IHttpModule
    {
        #region IHttpModule Members
        private SessionStateModule _SessionModule;

        public void Dispose()
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);

            for (int i = 0; i < context.Modules.Count; i++)
            {
                if (context.Modules[i] is SessionStateModule)
                {
                    _SessionModule = (SessionStateModule)context.Modules[i];
                    break;
                }
            }

            if (_SessionModule != null)
            {
                _SessionModule.Start += new EventHandler(SessionModule_Start);
                //_SessionModule.End += new EventHandler(SessionModule_End); // doesn't work
            }
        }

        void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            OnlineUsersInstance.OnlineUsers.UpldateUserActivity();
        }

        void SessionModule_End(object sender, EventArgs e)
        {
            //OnlineUsersInstance.OnlineUsers.UpdateForUserLeave();
        }

        void SessionModule_Start(object sender, EventArgs e)
        {
            OnlineUsersInstance.OnlineUsers.UpdateForNewUser();
        }

        #endregion
    }
}
