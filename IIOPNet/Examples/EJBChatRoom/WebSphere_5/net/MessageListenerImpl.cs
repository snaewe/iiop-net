using System;
using System.Threading;
using Ch.Elca.Iiop.Idl;

namespace ch.elca.iiop.demo.ejbChatroom {

    /// <summary>
    /// the callback impl.
    /// </summary>
    [SupportedInterface(typeof(MessageListener))]
    public class MessageListenerImpl : MarshalByRefObject, MessageListener {

        private string m_userName;

        private Chatform m_view;
        
        public MessageListenerImpl(string userName, Chatform view) {
            m_userName = userName;
            m_view = view;
        }

        public string userName {
            get {
                return m_userName;
            }
        }

        #region IMethods

        public void notifyMessage(Message msg) {
            // m_view.AddMessage(msg);
            // works only when the message was sent asynchronously

            // because we are in a different thread from the thread created the form, we need to invoke the update method in
            // in the following way:
            Chatform.UpdateInvoker invoker = new Chatform.UpdateInvoker(m_view.AddMessage);
            // need an async call here, because click on button blocks windows, call to update can only succeed after click handling
            // has finished
            m_view.BeginInvoke(invoker, new object[] { msg });
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

        #endregion IMethods
        
    }
}
