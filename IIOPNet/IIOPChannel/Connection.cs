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
    public class GiopConnectionContext {

        #region IFields

        private uint m_charSetChosen = CodeSetService.DEFAULT_CHAR_SET;
        private uint m_wcharSetChosen = CodeSetService.DEFAULT_WCHAR_SET;
    	
    	private Hashtable m_items = new Hashtable();

        #endregion IFields
        #region IConstructors

        internal GiopConnectionContext() {
        }

        #endregion IConstructors
        #region IProperties

        public uint CharSet {
            get {
                return m_charSetChosen;
            }
            set {
                m_charSetChosen = value;
            }
        }

        public uint WCharSet {
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

        #endregion IProperties

    }
    
    /// <summary>the connection context for the client side</summary>
    internal class GiopClientConnectionContext : GiopConnectionContext {
		
		#region IFields
		
		private GiopRequestNumberGenerator m_reqNumGen = 
		            new GiopRequestNumberGenerator();
    	
    	#endregion IFields
    	#region IProperties 
    	
    	public GiopRequestNumberGenerator ReqNumberGen {
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
    public class IiopClientConnection {

        #region IFields

        private GiopClientConnectionContext m_assocContext;

        private Uri m_chanUri;

        private TcpClient m_socket;
        private string m_host;
        private int m_port;

        #endregion IFields
        #region IConstructors

        public IiopClientConnection(Uri chanUri) {
            m_chanUri = chanUri;
        	m_assocContext = new GiopClientConnectionContext();
        }

        #endregion IConstructors
        #region IProperties

        internal GiopClientConnectionContext Context {
            get {
                return m_assocContext;
            }
        }

        internal string ChanUri {
            get {
                return m_chanUri.ToString();
            }
        }

        internal TcpClient ClientSocket {
            get {
                return m_socket;
            }
        }

        #endregion IProperties
        #region IMethods


        internal void ConnectTo(string host, int port) {
            TcpClient tcpSocket = new TcpClient(host, port);
            tcpSocket.NoDelay = false; // what is better here ?
            m_socket = tcpSocket;
            m_host = host;
            m_port = port;
        }

        internal bool CheckConnected() {
            try {
                if (m_socket == null) {
                    return false;
                }
                m_socket.GetStream();
                return true;
            } catch (Exception) {
                // not connected any more
                return false;
            }
        }

        internal void CloseConnection() {
            try {
                m_socket.Close();
            } catch (Exception) { }
        }

        #endregion IMethods

    
    }


}
