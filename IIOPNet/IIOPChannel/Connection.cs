/* Connection.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 30.04.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2003 Dominic Ullmann
 *
 * Copyright 2003 ELCA Informatique SA
 * Av. de la Harpe 22-24, 1000 Lausanne 13, Switzerland
 * www.elca.ch
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Ch.Elca.Iiop.Services;

namespace Ch.Elca.Iiop {


    /// <summary>
    /// Stores information associated with a GIOP connection,
    /// e.g. the Codesets chosen
    /// </summary>
    public class GiopConnectionDesc {

        #region Constants
        
        internal const string SERVER_TR_HEADER_KEY = "_server_giop_con_desc_";
        internal const string CLIENT_TR_HEADER_KEY = "_client_giop_con_desc_";
        
        #endregion Constants
        #region IFields

        private int m_charSetChosen = CodeSetService.DEFAULT_CHAR_SET;
        private int m_wcharSetChosen = CodeSetService.DEFAULT_WCHAR_SET;
        
        private Hashtable m_items = new Hashtable();                
        
        private bool m_messagesExchanged = false;

        #endregion IFields
        #region IConstructors

        internal GiopConnectionDesc() {
        }

        #endregion IConstructors
        #region IProperties

        public int CharSet {
            get {
                return m_charSetChosen;
            }
            set {
                m_charSetChosen = value;
            }
        }

        public int WCharSet {
            get {
                return m_wcharSetChosen;
            }
            set {
                m_wcharSetChosen = value;
            }
        }
        
        public IDictionary Items {
            get {
                return m_items;
            }
        }
        
        /// <summary>
        /// Were any messages (request/reply) already exchanged
        /// on this connection
        /// </summary>
        public bool MessagesAlreadyExchanged {
            get {
                return m_messagesExchanged;    
            }
            set {
                m_messagesExchanged = value;
            }
        }
        
        #endregion IProperties

    }
    
    /// <summary>the connection context for the client side</summary>
    internal class GiopClientConnectionDesc : GiopConnectionDesc {
        
        #region IFields
        
        private GiopRequestNumberGenerator m_reqNumGen = 
                    new GiopRequestNumberGenerator();
        
        #endregion IFields
        #region IProperties 
        
        internal GiopRequestNumberGenerator ReqNumberGen {
            get {
                return m_reqNumGen;
            }
        }
                
        #endregion IProporties
    }

    /// <summary>
    /// stores the relevant information of an IIOP client side
    /// connection
    /// </summary>
    internal class GiopClientConnection {

        #region IFields

        private GiopClientConnectionDesc m_assocDesc;

        private string m_connectionKey;
                
        private IClientTransport m_clientTransport;

        #endregion IFields
        #region IConstructors

        internal GiopClientConnection(string connectionKey, IClientTransport transport) {
            m_connectionKey = connectionKey;
            m_assocDesc = new GiopClientConnectionDesc();
            m_clientTransport = transport;
        }

        #endregion IConstructors
        #region IProperties

        internal GiopClientConnectionDesc Desc {
            get {
                return m_assocDesc;
            }
        }

        internal string ConnectionKey {
            get {
                return m_connectionKey;
            }
        }

        internal IClientTransport ClientTransport {
            get {
                return m_clientTransport;
            }
        }

        #endregion IProperties
        #region IMethods


        internal void Connect() {
            m_clientTransport.OpenConnection();
        }

        internal bool CheckConnected() {
            return m_clientTransport.IsConnectionOpen();
        }

        internal void CloseConnection() {
            m_clientTransport.CloseConnection();
        }

        #endregion IMethods

    
    }


}
