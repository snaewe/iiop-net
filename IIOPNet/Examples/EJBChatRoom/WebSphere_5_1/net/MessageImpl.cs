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

        public override string originator {
            get {
                return m_originator;
            }
        }

        public override string msg {
            get {
                return m_msg;
            }
        }

	}
}
