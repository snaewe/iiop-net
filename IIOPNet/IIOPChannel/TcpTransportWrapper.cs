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
using System.Net;
using System.Threading;
using System.Diagnostics;
using Ch.Elca.Iiop.CorbaObjRef;

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
    /// creates TCP transports
    /// </summary>
    internal class TcpTransportFactory : ITransportFactory {
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTransportFactory.CreateTransport(Ior)"/></summary>
        public IClientTransport CreateTransport(Ior target) {
            return new TcpClientTransport(target.HostName, target.Port);
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTransportFactory.GetEndpointKey(Ior)"/></summary>
        public string GetEndpointKey(Ior target) {
            return "iiop://"+target.HostName+":"+target.Port;
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerTransportFactory.CreateConnectionListener"/></summary>
        public IServerConnectionListener CreateConnectionListener(ClientAccepted clientAcceptCallBack) {
            IServerConnectionListener result = new TcpConnectionListener();
            result.Setup(clientAcceptCallBack);
            return result;
        }
                
    }
    
    /// <summary>implementers wait for and accept client connections on their supported transport mechanism.
    /// </summary>
    internal class TcpConnectionListener : IServerConnectionListener {
        
        #region IFields
        
        private ClientAccepted m_clientAcceptCallback;
        
        private Thread m_listenerThread;        
        private TcpListener m_listener;
        
        private bool m_listenerActive = false;
        private bool m_isInitalized = false;
        
        #endregion IFields
        #region IMethods        
        
        private void SetupListenerThread() {
            ThreadStart listenerStart = new ThreadStart(ListenForMessages);
            m_listenerThread = new Thread(listenerStart);
            m_listenerThread.IsBackground = true;
        }
        
        private void ListenForMessages() {
            while (m_listenerActive) {
                // receive messages
                TcpClient client = null;
                try {
                    client = m_listener.AcceptTcpClient();
                    if (client != null) { // everything ok
                        // disable Nagle algorithm, to reduce delay
                        client.NoDelay = true;
                        // now process the message of this client
                        TcpServerTransport transport = new TcpServerTransport(client);
                        m_clientAcceptCallback(transport);
                    } else {
                        Trace.WriteLine("acceptTcpClient hasn't worked");
                    }
                } catch (Exception e) {
                    Debug.WriteLine("Exception in server listener thread: " + e);
                    if (client != null)  { 
                        client.Close(); 
                    }
                }
            }
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerConnectionListener.Setup"</summary>
        public void Setup(ClientAccepted clientAcceptCallback) {
            if (m_isInitalized) {
                throw CreateAlreadyListeningException();
            }
            m_isInitalized = true;
            m_clientAcceptCallback = clientAcceptCallback;
            SetupListenerThread();            
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerConnectionListener.IsInitalized"</summary>
        public bool IsInitalized() {
            return m_isInitalized;
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerConnectionListener.StartListening"</summary>
        public int StartListening(int listeningPortSuggestion, out ITaggedComponent[] additionalTaggedComponents) {
            if (!m_isInitalized) {
                throw CreateNotInitalizedException();
            }
            if (m_listenerActive) {
                throw CreateAlreadyListeningException();
            }
            additionalTaggedComponents = new ITaggedComponent[0];
            int resultPort = listeningPortSuggestion;
            
            // use IPAddress.Any and not m_myAddress, to allow connections to loopback and normal ip
            m_listener = new TcpListener(IPAddress.Any, listeningPortSuggestion);            
            // start TCP-Listening
            m_listener.Start();
            if (listeningPortSuggestion == 0) { 
                // auto-assign port selected
                resultPort = ((IPEndPoint)m_listener.LocalEndpoint).Port; 
            }
            m_listenerActive = true;
            // start the handler thread
            m_listenerThread.Start();
            return resultPort;
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerConnectionListener.IsListening"</summary>
        public bool IsListening() {
            return m_listenerActive;    
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerConnectionListener.StopListening"</summary>
        public void StopListening() {
            if (!m_listenerActive) {
                throw CreateNotListeningException();
            }
            m_listenerActive = false;
            if (m_listenerThread != null) { 
                try {
                    m_listenerThread.Interrupt(); m_listenerThread.Abort(); 
                } catch (Exception) { }
            }
            if (m_listener != null) { 
                m_listener.Stop();
            }
        }
        
        
        #region Exceptions
        
        private Exception CreateNotListeningException() {
            return new InvalidOperationException("Listener is not listening");    
        }
        
        private Exception CreateAlreadyListeningException() {
            return new InvalidOperationException("Listener is already listening");    
        }
        
        private Exception CreateNotInitalizedException() {
            return new InvalidOperationException("Listener not initalized; call setup first");
        }
        
        private Exception CreateAlreadyInitalizedException() {
            return new InvalidOperationException("Listener already initalized");
        }
        
        #endregion Exceptions        
        #endregion IMethods
        
    }

    



}
