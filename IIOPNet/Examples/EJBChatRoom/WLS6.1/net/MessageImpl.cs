using System;

namespace ch.elca.iiop.demo.ejbChatroom {
	
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

        public string originator {
            get {
                return m_originator;
            }
        }

        public string msg {
            get {
                return m_msg;
            }
        }

	}
}
