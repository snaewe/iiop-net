/* TcpTransportWrapper.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 18.01.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.IO;
using System.Net.Sockets;

namespace Ch.Elca.Iiop {


    /// <summary>Base class for tcp transports</summary>
    internal class TcpTransportBase : ITransport {
                        
        #region IFields
        
        protected NetworkStream m_stream;
        protected TcpClient m_socket;
                
        #endregion IFields
        #region IProperties
        
        /// <summary><see cref="Ch.Elca.Iiop.ITransport.TransportStream"/></summary>
        public Stream TransportStream {
            get {
                return m_stream;
            }
        }
        
        #endregion IProperties
        #region IMethods
                
        /// <summary><see cref="Ch.Elca.Iiop.ITranport.IsDataAvailable/></summary>
        public bool IsDataAvailable() {
            return m_stream.DataAvailable;
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.ITranport.CloseConnection/></summary>
        public void CloseConnection() {
            try {
                m_socket.Close(); // closes the stream too
            } catch (Exception) {}
            m_socket = null;
        }
        
        #endregion IMethods
        
    }
    
    /// <summary>represnets a tcp/iiop connection to a server</summary>
    internal class TcpClientTransport : TcpTransportBase, IClientTransport {
                        
        #region IFields
               
        private string m_targetHost;
        private int m_port;
        
        #endregion IFields
        #region IConstructors
        
        public TcpClientTransport(string host, int port) {
            m_targetHost = host;
            m_port = port;
        }
        
        #endregion Ionstructors
        #region IMethods
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTranport.OpenConnection/></summary>
        public void OpenConnection() {
            if (IsConnectionOpen()) { 
                return; // already open
            }
            m_socket = new TcpClient(m_targetHost, m_port);
            m_socket.NoDelay = true; // send immediately; (TODO: what is better here?)
            m_stream = m_socket.GetStream();
        }
                
        /// <summary><see cref="Ch.Elca.Iiop.IClientTranport.IsConnectionOpen/></summary>
        public bool IsConnectionOpen() {
            if (m_socket == null) {
                return false;
            } else {
                try {
                    m_socket.GetStream(); // TODO, search a better way to do this
                } catch (Exception) {
                    return false;
                }                                
                return true; 
            }
        }
        
        #endregion IMethods
        
    }
    
    /// <summary>represnets a tcp/iiop connection to a client</summary>
    internal class TcpServerTransport : TcpTransportBase, IServerTransport {
                        
        #region SFields
        
        private static Type s_socketExType = typeof(SocketException);
        
        #endregion SFields
        #region IConstructors
        
        public TcpServerTransport(TcpClient theClient) {
            m_socket = theClient;
            m_stream = m_socket.GetStream();
        }
        
        #endregion Ionstructors
        #region IMethods
                        
        /// <summary><see cref="Ch.Elca.Iiop.IServerTransport.IsConnectionCloseException"/></summary>
        public bool IsConnectionCloseException(Exception e) {
            return s_socketExType.IsInstanceOfType(e.InnerException);            
        }
                
        #endregion IMethods
        
    }        
    
    /// <summary>
    /// creates TCP client transports
    /// </summary>
    internal class TcpClientTransportFactory : IClientTransportFactory {
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTransportFactory.CreateTransport(string, int)"/></summary>
        public IClientTransport CreateTransport(string targetHost, int targetPort) {
            return new TcpClientTransport(targetHost, targetPort);
        }
                
    }
    
    



}
