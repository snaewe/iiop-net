using System;

namespace Ch.Elca.Iiop.Demo.Chatroom {
	
    /// <summary>
    /// Implementation of the CORBA value type Message
    /// </summary>
    [Serializable]
    public class MessageImpl : Message {

        public MessageImpl() {
        }

        public MessageImpl(string originator, string msg) {
            m_originator = originator;
            m_msg = msg;
        }

        public override string Originator {
            get {
                return m_originator;
            }
        }

        public override string Msg {
            get {
                return m_msg;
            }
        }

	}
}
