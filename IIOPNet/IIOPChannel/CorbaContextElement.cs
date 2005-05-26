

using System;
using System.Runtime.Remoting.Messaging;


namespace Ch.Elca.Iiop.Idl {

    /// <summary>used for passing corba method context.</summary>
    /// <remarks>
    /// Put an instance into call context for every element, you need to pass.
    /// </remarks>
    [Serializable]
    public class CorbaContextElement : ILogicalThreadAffinative {

        private string m_elementValue;

        public CorbaContextElement(string elementValue) {
            m_elementValue = elementValue;
        }

        public override string ToString() {
            return m_elementValue;
        }

        /// <summary>
        /// the value of this element
        /// </summary>
        public string ElementValue {
            get {
                return m_elementValue;
            }
        }

    }
}
