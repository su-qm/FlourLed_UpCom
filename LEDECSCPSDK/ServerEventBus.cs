using System;
using System.Collections.Generic;
using System.Linq;

namespace GATEECSCPSDK
{
    public class ServerEventBus
    {
        private static ServerEventBus instance;
        public static ServerEventBus GetInstance()
        {
            if (instance == null)
            {
                instance = new ServerEventBus();
            }
            return instance;
        }

        public delegate void MessageReceivedEvent(object sender, uint session, uint msgtype, uint numOfParameters, string parameters);
        public delegate void MessageSentEvent(object sender, uint session, object message);

        public static event MessageReceivedEvent MessageReceived;
        public static event MessageSentEvent MessageSent;

        public delegate void SessionCreatedEvent(object sender, uint session);
        public static event SessionCreatedEvent SessionCreated;


        public delegate void SessionOpenedEvent(object sender, uint session);
        public static event SessionOpenedEvent SessionOpened;

        public delegate void SessionClosedEvent(object sender, uint session);
        public static event SessionClosedEvent SessionClosed;

        public delegate void ExceptionCaughtEvent(object sender, uint session, Exception ex);
        public static event ExceptionCaughtEvent ExceptionCaught;

        public void OnMessageReceive(uint session, uint msgtype, uint numOfParameters, string parameters)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, session, msgtype, numOfParameters, parameters);
            }
        }

        public void OnMessageSent(uint session, object message)
        {
            if (MessageSent != null)
            {
                MessageSent(this, session, message);
            }
        }

        public void OnSessionCreated(uint session)
        {
            if (SessionCreated != null)
            {
                SessionCreated(this, session);
            }
        }

        public void OnSessionOpened(uint session)
        {
            if (SessionOpened != null)
            {
                SessionOpened(this, session);
            }
        }

        public void OnSessionClosed(uint session)
        {
            if (SessionClosed != null)
            {
                SessionClosed(this, session);
            }
        }

        public void OnExceptionCaught(uint session, Exception ex)
        {
            if (ExceptionCaught != null)
            {
                ExceptionCaught(this, session, ex);
            }
        }


    }
}
