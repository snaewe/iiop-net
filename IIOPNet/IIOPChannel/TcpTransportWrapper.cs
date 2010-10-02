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
using System.Collections;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.CorbaObjRef;
using omg.org.CORBA;
using omg.org.IOP;

namespace Ch.Elca.Iiop {


    /// <summary>Base class for tcp transports</summary>
    internal abstract class TcpTransportBase : ITransport {

        #region SFields
        
        private static System.Reflection.PropertyInfo s_tcpClientClientPropertyInfo =
            typeof(TcpClient).GetProperty("Client",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
        
        #endregion SFields
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
                if (m_socket != null) {
                    m_socket.Close();
                }
            } catch {
                // ignore
            }
            m_socket = null;
            try {
                if(m_stream != null) {
                    m_stream.Close(); // close the stream and the socket.
                }
            } catch {
                // ignore
            }
            m_stream = null;
        }
        
        public IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state) {
            return m_stream.BeginRead(buffer, offset, size, callback, state);
        }

        public IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state) {
            return m_stream.BeginWrite(buffer, offset, size, callback, state);
        }
        
        public int EndRead(IAsyncResult asyncResult) {
            return m_stream.EndRead(asyncResult);
        }
        
        public void EndWrite(IAsyncResult asyncResult) {
            m_stream.EndWrite(asyncResult);
        }

        public int Read(byte[] buffer, int offset, int size) {
            return m_stream.Read(buffer, offset, size);
        }        
        
        public void Write(byte[] buffer, int offset, int size) {
            m_stream.Write(buffer, offset, size);
        }
                
        /// <summary><see cref="Ch.Elca.Iiop.ITransport.GetPeerAddress"/></summary>
        public IPAddress GetPeerAddress() {
            if (m_socket != null) {
                Socket socket = (Socket)s_tcpClientClientPropertyInfo.GetValue(m_socket, null);
                return ((IPEndPoint)socket.RemoteEndPoint).Address;
            } else {
                // not ok to call if no connection available
                throw new omg.org.CORBA.BAD_OPERATION(87, CompletionStatus.Completed_MayBe);
            }
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTranport.IsConnectionOpen/></summary>
        public bool IsConnectionOpen() {
            return m_socket != null && m_socket.Client != null && m_socket.Client.Connected;
        }
        
        #endregion IMethods
        
    }
    
    /// <summary>represnets a tcp/iiop connection to a server</summary>
    internal class TcpClientTransport : TcpTransportBase, IClientTransport {

        #region IFields

        private string[] m_targetHosts;
        private IPAddress m_targetHostIp;
        private int[] m_ports;
        private int m_receiveTimeOut = 0;
        private int m_sendTimeOut = 0;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>
        /// create a transport wrapper to host with name host and port port.
        /// </summary>
        /// <param name="host">a symbolic name, which is resolved using dns</param>
        /// <param name="port">the port to connect to</param>
        public TcpClientTransport(string host, int port) {
            m_targetHosts = new string[] { host };
            m_ports = new int[] { port };
            
        }

        public TcpClientTransport(string[] hosts, int[] ports) {
            m_targetHosts = hosts;
            m_ports = ports;
        }

        /// <summary>
        /// create a transport wrapper to host with ip-address hostAddr and port port.
        /// </summary>        
        public TcpClientTransport(IPAddress hostIp, int port) {
            m_targetHostIp = hostIp;
            m_ports = new int[] { port };
        }
        
        #endregion IConstructors
        #region IProperties
        
        public int ReceiveTimeOut {
            get {
                return m_receiveTimeOut;
            }
            set {
                m_receiveTimeOut = value;
            }
        }
        
        public int SendTimeOut {
            get {
                return m_sendTimeOut;
            }
            set {
                m_sendTimeOut = value;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTranport.OpenConnection/></summary>
        public void OpenConnection() {
            if (IsConnectionOpen()) {
                return; // already open
            }
            m_socket = new TcpClient();
            if (m_targetHostIp != null) {
                m_socket.Connect(m_targetHostIp, m_ports[0]);
            } else if (m_targetHosts != null) {
                OpenConnectionByHostName();
            } else {
                throw new INTERNAL(547, CompletionStatus.Completed_No);
            }
            m_socket.NoDelay = true; // send immediately; (TODO: what is better here?)
            m_socket.ReceiveTimeout = m_receiveTimeOut;
            m_socket.SendTimeout = m_sendTimeOut;
            m_stream = m_socket.GetStream();
        }

        private void OpenConnectionByHostName() {
            Exception lastException = null;
            for (int i = 0; i < m_targetHosts.Length; i++) {
                lastException = null;
                try {
                    Debug.WriteLine(
                        string.Format(
                            "OpenConnection(): Try to connect to {0}:{1}",
                            m_targetHosts[i],
                            m_ports[i]));
                    m_socket.Connect(m_targetHosts[i], m_ports[i]);
                    Debug.WriteLine("OpenConnection(): connection succeeded !");
                    break;
                } catch (Exception e) {
                    Trace.WriteLine("OpenConnection(): connect failed: " +e.ToString());
                    lastException = e;
                }
            }
            if (lastException != null) {
                throw lastException;
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
        
        #region IFields
        
        private int m_receiveTimeOut = 0;
        private int m_sendTimeOut = 0;
        private omg.org.IOP.Codec m_codec;
        
        #endregion IFields
        #region IProperties
        
        /// <summary><see cref="Ch.Elca.Iiop.ITransportFactory.Codec"/></summary>
        public omg.org.IOP.Codec Codec {
            set {
                m_codec = value;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTransportFactory.CreateTransport(IIorProfile)"/></summary>
        public IClientTransport CreateTransport(IIorProfile targetProfile) {
            if (targetProfile.ProfileId == TAG_INTERNET_IOP.ConstVal) {
                IInternetIiopProfile iiopProf = (IInternetIiopProfile)targetProfile;
                IPAddress asIpAddress = ConvertToIpAddress(iiopProf.HostName);
                IClientTransport result;
                if (asIpAddress == null) {
                    ArrayList hostNames = new ArrayList();
                    ArrayList ports = new ArrayList();
                    hostNames.Add(iiopProf.HostName);
                    ports.Add(iiopProf.Port);
                    foreach (TaggedComponent taggedComponent in 
                             iiopProf.TaggedComponents.GetComponents(ALTERNATE_IIOP_ADDRESS.ConstVal)) {
                        string hostName = System.Text.ASCIIEncoding.ASCII.GetString(
                            taggedComponent.component_data,
                            8,
                            BitConverter.ToInt32(taggedComponent.component_data, 4) - 1);
                        ushort port = BitConverter.ToUInt16(taggedComponent.component_data, 
                                                            taggedComponent.component_data.Length - 2);
                        hostNames.Add(hostName);
                        ports.Add(port);
                        Debug.WriteLine(string.Format("CreateTransport(): host {0}:{1} from IOR added to list", hostName, port));
                    }                    
                    result = new TcpClientTransport((string[])hostNames.ToArray(ReflectionHelper.StringType),
                                                    (int[])ports.ToArray(ReflectionHelper.Int32Type));
                } else {
                    result = new TcpClientTransport(asIpAddress, iiopProf.Port);
                }
                result.ReceiveTimeOut = m_receiveTimeOut;
                result.SendTimeOut = m_sendTimeOut;
                return result;
            } else {
                throw new INTERNAL(3001, CompletionStatus.Completed_No);
            }
        }
        
        /// <summary>
        /// <see cref="Ch.Elca.Iiop.IClientTransportFactory.CanCreateTransportForIor"/>
        /// </summary>
        public bool CanCreateTranporForIor(Ior target) {
            for (int i = 0; i < target.Profiles.Length; i++) {
                if (CanUseProfile(target.Profiles[i])) {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// <see cref="Ch.Elca.Iiop.IClientTransportFactory.CanUseProfile"/>
        /// </summary>
        public bool CanUseProfile(IIorProfile profile) {
            if (profile.ProfileId == TAG_INTERNET_IOP.ConstVal) {
                IInternetIiopProfile iiopProf = (IInternetIiopProfile)profile;
                if ((iiopProf.HostName != null) && (iiopProf.Port > 0)) {
                    return true;
                }
            }
            return false;
        }

        
        /// <summary>
        /// returns the IPAddress if hostName is a valid ipAdress, otherwise returns null.
        /// </summary>
        private IPAddress ConvertToIpAddress(string hostName) {
            IPAddress addr;
            if(IPAddress.TryParse(hostName, out addr))
                return addr;

            return null;
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTransportFactory.GetEndpointKey(Ior)"/></summary>
        public string GetEndpointKey(IIorProfile target) {
            if (target.ProfileId ==  TAG_INTERNET_IOP.ConstVal) {
                IInternetIiopProfile prof = (IInternetIiopProfile)target;
                return "iiop" + prof.Version.Major + "." +
                       prof.Version.Minor + "://"+prof.HostName+":"+prof.Port;
            } else {
                return String.Empty;
            }
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTransportFactory.GetEndPointKeyForBidirEndpoint(object)"/></summary>
        public string GetEndPointKeyForBidirEndpoint(object endPoint) {
            if (endPoint is omg.org.IIOP.ListenPoint) {
                return "iiop://"+((omg.org.IIOP.ListenPoint)endPoint).ListenHost + ":" + 
                                 ((omg.org.IIOP.ListenPoint)endPoint).ListenPort;
            } else {
                return null;
            }
        }

        /// <summary><see cref="Ch.Elca.Iiop.IServerTransportFactory.GetListenPoints(object)"/></summary>
        public object[] GetListenPoints(IiopChannelData chanData) {
            object[] result = new object[] { new omg.org.IIOP.ListenPoint(chanData.HostName,
                                                                          (short)((ushort)chanData.Port)) };
            return result;
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerTransportFactory.CreateConnectionListener"/></summary>
        public IServerConnectionListener CreateConnectionListener(ClientAccepted clientAcceptCallBack) {
            IServerConnectionListener result = new TcpConnectionListener();
            result.Setup(clientAcceptCallBack);
            return result;
        }
        
        public void SetupClientOptions(IDictionary options) {
            // no specific options, ignore
        }
        
        public void SetClientTimeOut(int receiveTimeOut, int sendTimeOut) {
            m_receiveTimeOut = receiveTimeOut;
            m_sendTimeOut = sendTimeOut;
        }
        
        public void SetupServerOptions(IDictionary options) {
            // no specific options, ignore
        }
        
        #endregion IMethods

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
            m_listenerThread.Name = "IIOPNet_ServerChannel_TcpPortListener";
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
                } catch (ThreadAbortException) {
                    throw;
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
        public int StartListening(IPAddress bindTo, int listeningPortSuggestion, out TaggedComponent[] additionalTaggedComponents) {
            if (!m_isInitalized) {
                throw CreateNotInitalizedException();
            }
            if (m_listenerActive) {
                throw CreateAlreadyListeningException();
            }
            additionalTaggedComponents = new TaggedComponent[0];
            int resultPort = listeningPortSuggestion;
            
            m_listener = new TcpListener(bindTo, listeningPortSuggestion);
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
                
        #endregion Exceptions
        #endregion IMethods
        
    }

}
