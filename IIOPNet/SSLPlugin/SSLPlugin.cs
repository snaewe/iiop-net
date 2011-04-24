/* SSLPlugin.cs
 * 
 * Project: IIOP.NET
 * SslPlugin for IIOPChannel using mentalis security library
 * 
 * WHEN      RESPONSIBLE
 * 29.04.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2004 Dominic Ullmann
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
using Ch.Elca.Iiop.CorbaObjRef;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Certificates;
using omg.org.CORBA;
using omg.org.IOP;

namespace Ch.Elca.Iiop.Security.Ssl {


    /// <summary>Base class for ssl transports</summary>
    public class SslTransportBase : ITransport {
        
        #region SFields

        private static System.Reflection.PropertyInfo s_secureTcpClientClientPropertyInfo =
            typeof(SecureTcpClient).GetProperty("Client",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
        
        #endregion SFields
        #region IFields
        
        protected SecureNetworkStream m_stream;
        protected SecureTcpClient m_socket;
                
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
            SecureSocket secureSocket = (SecureSocket)s_secureTcpClientClientPropertyInfo.GetValue(m_socket, null);
            return ((IPEndPoint)secureSocket.RemoteEndPoint).Address;
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.ITranport.IsConnectionOpen/></summary>
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
    
    /// <summary>represnets a ssl/iiop connection to a server</summary>
    public class SslClientTransport : SslTransportBase, IClientTransport {
                        
        #region IFields
               
        private string m_targetHost;
        private IPAddress m_targetHostIp;
        private int m_port;
        private SecurityOptions m_options;
        private int m_receiveTimeOut = 0;
        private int m_sendTimeOut = 0;
        
        #endregion IFields
        #region IConstructors
        
        public SslClientTransport(string host, int port, SecurityOptions options) {
            m_targetHost = host;
            m_port = port;
            m_options = options;
        }
        
        /// <summary>
        /// create a transport wrapper to host with ip-address hostAddr and port port.
        /// </summary>
        public SslClientTransport(IPAddress hostIp, int port, SecurityOptions options) {
            m_targetHostIp = hostIp;
            m_port = port;
            m_options = options;
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
            m_socket = new SecureTcpClient(m_options);
            if (m_targetHostIp != null) {
                m_socket.Connect(m_targetHostIp, m_port);
            } else if (m_targetHost != null) {
                m_socket.Connect(m_targetHost, m_port);
            } else {
                throw new INTERNAL(547, CompletionStatus.Completed_No);
            }
            m_socket.NoDelay = true; // send immediately; (TODO: what is better here?)
            m_socket.ReceiveTimeout = m_receiveTimeOut;
            m_socket.SendTimeout = m_sendTimeOut;
            m_stream = m_socket.GetStream();
        }
                        
        #endregion IMethods
        
    }
    
    /// <summary>represnets a ssl/iiop connection to a client</summary>
    public class SslServerTransport : SslTransportBase, IServerTransport {
                        
        #region SFields
        
        private static Type s_socketExType = typeof(SocketException);
                
        #endregion SFields
        #region IConstructors
        
        public SslServerTransport(SecureTcpClient theClient) {
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
    /// creates Ssl transports
    /// </summary>
    public class SslTransportFactory : ITransportFactory {
        
        #region Constants
        
        public const string SERVER_REQUIRED_OPTS = "ServerRequiredSecurityAssoc";
        public const string SERVER_SUPPORTED_OPTS = "ServerSupportedSecurityAssoc";
        
        public const string CLIENT_AUTHENTICATION = "ClientAuthentication";
        public const string SERVER_AUTHENTICATION = "ServerAuthentication";
        
        #endregion Constants
        #region SFields
        
        private Type SEC_ASSOC_TYPE = typeof(SecurityAssociationOptions);
        
        #endregion SFields
        #region IFields
        
        private SecurityAssociationOptions m_server_required_opts;
        private SecurityAssociationOptions m_server_supported_opts;
        
        private IClientSideAuthentication m_clientAuth = new DefaultClientAuthenticationImpl();
        private IServerSideAuthentication m_serverAuth = new DefaultServerAuthenticationImpl();
        
        private int m_receiveTimeOut = 0;
        private int m_sendTimeOut = 0;
        
        private omg.org.IOP.Codec m_codec;
        
        #endregion IFields
        #region IConstructors
        
        public SslTransportFactory() {
        }
        
        #endregion IConstructors
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
        public IClientTransport CreateTransport(IIorProfile profile) {
            if (profile.ProfileId != TAG_INTERNET_IOP.ConstVal) {
                throw new INTERNAL(734, CompletionStatus.Completed_No);
            }
            object sslComponentDataObject = GetSSLComponent(profile, m_codec);
            if (sslComponentDataObject == null) {
                throw new INTERNAL(734, CompletionStatus.Completed_No);
            }
            SSLComponentData sslComponent = (SSLComponentData)sslComponentDataObject;
            IInternetIiopProfile targetProfile = (IInternetIiopProfile)profile;
            int port = sslComponent.GetPort();
            SecurityOptions options = CreateClientSecurityOptions(sslComponent);
            IPAddress asIpAddress;
            IClientTransport result =
                IPAddress.TryParse(targetProfile.HostName, out asIpAddress)
                    ? new SslClientTransport(asIpAddress, port, options)
                    : new SslClientTransport(targetProfile.HostName, port, options);
            result.ReceiveTimeOut = m_receiveTimeOut;
            result.SendTimeOut = m_sendTimeOut;
            return result;
        }
        
        private SecurityOptions CreateClientSecurityOptions(SSLComponentData sslData) {
            CertVerifyEventHandler serverCertificateCheckHandler = null;
            CertRequestEventHandler clientCertificateRequestHandler = null;
            CredentialVerification credentialVerification = CredentialVerification.Auto;
            SecureProtocol protocol = SecureProtocol.None;
            SslAlgorithms sslAlgs = SslAlgorithms.ALL;
            

            if (((sslData.TargetRequiredOptions & SecurityAssociationOptions.EstablishTrustInTarget) > 0) ||
                ((sslData.TargetRequiredOptions & SecurityAssociationOptions.EstablishTrustInClient) > 0)) {
                protocol = SecureProtocol.Tls1 | SecureProtocol.Ssl3;
                sslAlgs = SslAlgorithms.SECURE_CIPHERS;
                
                credentialVerification = CredentialVerification.Manual;
                serverCertificateCheckHandler = new CertVerifyEventHandler(this.CheckServerCertAtClient);
                clientCertificateRequestHandler = new CertRequestEventHandler(this.GetClientCertAtClient);
            }
            
            SecurityOptions result =
                new SecurityOptions(protocol,
                                    null, ConnectionEnd.Client,
                                    credentialVerification, serverCertificateCheckHandler,
                                    null, SecurityFlags.Default, sslAlgs,
                                    clientCertificateRequestHandler);
            return result;
        }
        
        private void CheckServerCertAtClient(SecureSocket socket, Certificate cert, CertificateChain chain, VerifyEventArgs args) {
            Debug.WriteLine("check the server certificate event");
            args.Valid = m_clientAuth.IsValidServerCertificate(cert, chain, ((IPEndPoint)socket.RemoteEndPoint).Address);
        }
        
        private void GetClientCertAtClient(SecureSocket socket, DistinguishedNameList acceptable, RequestEventArgs e) {
            Debug.WriteLine("server requested client certificate");
            e.Certificate = m_clientAuth.GetClientCertificate(acceptable);
        }
        
        /// <summary>
        /// <see cref="Ch.Elca.Iiop.IClientTransportFactory.CanCreateTransportForIor"/>
        /// </summary>
        public bool CanCreateTranporForIor(Ior target) {
            // check for SSL component
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
                return ((iiopProf.HostName != null) && HasSSLComponent(profile));
            } else {
                return false;
            }
        }
        
        private bool HasSSLComponent(IIorProfile profile) {
            if (profile.ProfileId == TAG_INTERNET_IOP.ConstVal) {
                if (profile.TaggedComponents.ContainsTaggedComponent(TAG_SSL_SEC_TRANS.ConstVal)) {
                    return true;
                }
            }
            return false;
        }
        
        private SSLComponentData GetSSLComponent(Ior ior, Codec codec) {
            object result = null;
            for (int i = 0; i < ior.Profiles.Length; i++) {
                result = GetSSLComponent(ior.Profiles[i], codec);
                if (result != null) {
                    break;
                }
            }
            if (result != null) {
                return (SSLComponentData)result;
            } else {
                throw new INTERNAL(734, CompletionStatus.Completed_No);
            }
        }
        
        private object GetSSLComponent(IIorProfile profile, Codec codec) {
            if (profile.ProfileId == TAG_INTERNET_IOP.ConstVal) {
                return profile.TaggedComponents.GetComponentData(TAG_SSL_SEC_TRANS.ConstVal, codec,
                                                                 SSLComponentData.TypeCode);
            } else {
                return null;
            }
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTransportFactory.GetEndpointKey(IIorProfile)"/></summary>
        public string GetEndpointKey(IIorProfile target) {
            if (target.ProfileId ==  TAG_INTERNET_IOP.ConstVal) {
                object sslComponent = GetSSLComponent(target, m_codec);
                IInternetIiopProfile prof = (IInternetIiopProfile)target;
                return "iiop-ssl" + prof.Version.Major + "." +
                       prof.Version.Minor + "://"+prof.HostName+":"+((SSLComponentData)sslComponent).Port;
            } else {
                return String.Empty;
            }
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IClientTransportFactory.GetEndPointKeyForBidirEndpoint(object)"/></summary>
        public string GetEndPointKeyForBidirEndpoint(object endPoint) {
            if (endPoint is omg.org.IIOP.ListenPoint) {
                return "iiop-ssl://"+((omg.org.IIOP.ListenPoint)endPoint).ListenHost + ":" +
                                 ((omg.org.IIOP.ListenPoint)endPoint).ListenPort;
            } else {
                return null;
            }
        }

        /// <summary><see cref="Ch.Elca.Iiop.IServerTransportFactory.GetListenPoints(object)"/></summary>
        public object[] GetListenPoints(Ch.Elca.Iiop.IiopChannelData chanData) {
            ArrayList listenpoints = new ArrayList();
            for (int i = 0; i < chanData.AdditionalTaggedComponents.Length; i++) {
                if (chanData.AdditionalTaggedComponents[i].tag == TAG_SSL_SEC_TRANS.ConstVal) {
                    SSLComponentData sslComp =
                        (SSLComponentData)m_codec.decode_value(chanData.AdditionalTaggedComponents[i].component_data,
                                                               SSLComponentData.TypeCode);
                    listenpoints.Add(new omg.org.IIOP.ListenPoint(chanData.HostName, sslComp.Port));
                }
            }
            return listenpoints.ToArray();
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerTransportFactory.CreateConnectionListener"/></summary>
        public IServerConnectionListener CreateConnectionListener(ClientAccepted clientAcceptCallBack) {
            IServerConnectionListener result = new SslConnectionListener(m_server_required_opts, m_server_supported_opts,
                                                                         m_serverAuth, m_codec);
            result.Setup(clientAcceptCallBack);
            return result;
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerTransportFactory.SetupServerOptions"/></summary>
        public void SetupServerOptions(IDictionary properties) {
            foreach (DictionaryEntry entry in properties) {
                switch ((string)entry.Key) {
                    case SERVER_REQUIRED_OPTS:
                        m_server_required_opts = (SecurityAssociationOptions)
                            Enum.Parse(SEC_ASSOC_TYPE, (string)entry.Value);
                        break;
                    case SERVER_SUPPORTED_OPTS:
                        m_server_supported_opts = (SecurityAssociationOptions)
                            Enum.Parse(SEC_ASSOC_TYPE, (string)entry.Value);
                        break;
                    case SERVER_AUTHENTICATION:
                        // instantiate server side authentication instance
                        string type = (string)entry.Value;
                        m_serverAuth = (IServerSideAuthentication)Activator.CreateInstance(Type.GetType(type, true));
                        m_serverAuth.SetupServerOptions(properties);
                        break;
                    default:
                        // ignore
                        break;
                }
            }
        }
        
        /// <summary><see cref="Ch.Elca.Iiop.IServerTransportFactory.SetupClientOptions"/></summary>
        public void SetupClientOptions(IDictionary properties) {
            foreach (DictionaryEntry entry in properties) {
                switch ((string)entry.Key) {
                    case CLIENT_AUTHENTICATION:
                        // instantiate client side authentication instance
                        string type = (string)entry.Value;
                        m_clientAuth = (IClientSideAuthentication)Activator.CreateInstance(Type.GetType(type, true));
                        m_clientAuth.SetupClientOptions(properties);
                        break;
                    default:
                        // ignore
                        break;
                }
            }
        }
        
        public void SetClientTimeOut(int receiveTimeOut, int sendTimeOut) {
            m_receiveTimeOut = receiveTimeOut;
            m_sendTimeOut = sendTimeOut;
        }
        
        #endregion IMethods
                
    }
    
    /// <summary>implementers wait for and accept client connections on their supported transport mechanism.
    /// </summary>
    public class SslConnectionListener : IServerConnectionListener {
        
        #region IFields
        
        private ClientAccepted m_clientAcceptCallback;
        
        private Thread m_listenerThread;
        private SecureTcpListener m_listener;
        private SecurityOptions m_sslOpts;
        
        private bool m_listenerActive = false;
        private bool m_isInitalized = false;
                
        private bool m_isSecured = false;
        private IServerSideAuthentication m_serverAuth;
        private SecurityAssociationOptions m_supportedOptions;
        private SecurityAssociationOptions m_requiredOptions;
        
        private omg.org.IOP.Codec m_codec;
        
        #endregion IFields
        #region IConstructors
        
        internal SslConnectionListener(SecurityAssociationOptions requiredOptions,
                                       SecurityAssociationOptions supportedOptions,
                                       IServerSideAuthentication serverAuth,
                                       omg.org.IOP.Codec codec) {
            m_codec = codec;
            
            if (((requiredOptions & SecurityAssociationOptions.NoProtection) > 0) &&
                (((supportedOptions & SecurityAssociationOptions.EstablishTrustInTarget) > 0) ||
                 ((supportedOptions & SecurityAssociationOptions.EstablishTrustInClient) > 0))) {
                throw new ArgumentException("unsupported options combination: required no protection and supported EstablishTrustInTarget/Client");
            }
            
            SecureProtocol protocol = SecureProtocol.None;
            SslAlgorithms allowedCiphers = SslAlgorithms.ALL;
            if (((supportedOptions & SecurityAssociationOptions.EstablishTrustInTarget) > 0) ||
                ((supportedOptions & SecurityAssociationOptions.EstablishTrustInClient) > 0)) {
                protocol = SecureProtocol.Tls1 | SecureProtocol.Ssl3;
                allowedCiphers = SslAlgorithms.SECURE_CIPHERS;
                m_isSecured = true;
            }
            
            CredentialVerification clientVerification = CredentialVerification.None;
            CertVerifyEventHandler verifyClient = null;
            SecurityFlags authFlags = SecurityFlags.Default;
            if (((supportedOptions & SecurityAssociationOptions.EstablishTrustInClient) > 0) ||
                ((requiredOptions & SecurityAssociationOptions.EstablishTrustInClient) > 0)) {
                clientVerification = CredentialVerification.Manual;
                verifyClient = new CertVerifyEventHandler(this.CheckClientCertAtServer);
            }
            if ((requiredOptions & SecurityAssociationOptions.EstablishTrustInClient) > 0) {
                authFlags = SecurityFlags.MutualAuthentication;
            }
                                                                                                  
            m_sslOpts = new SecurityOptions(protocol, serverAuth.GetServerCertificate(), ConnectionEnd.Server,
                                            clientVerification, verifyClient,
                                            null, authFlags, allowedCiphers, null);
            m_serverAuth = serverAuth;
            m_supportedOptions = supportedOptions;
            m_requiredOptions = requiredOptions;
        }
        
        #endregion IConstructors
        #region IMethods
        
        private void CheckClientCertAtServer(SecureSocket socket, Certificate clientCertificate, CertificateChain allClientCertificates,
                                             VerifyEventArgs args) {
            Debug.WriteLine("check the client certificate event");
            if (allClientCertificates != null) {
                args.Valid = m_serverAuth.IsValidClientCertificate(clientCertificate,
                                                                   allClientCertificates, ((IPEndPoint)socket.RemoteEndPoint).Address);
            } else {
                args.Valid = !((m_requiredOptions & SecurityAssociationOptions.EstablishTrustInClient) > 0);
            }
        }
                        
        private void SetupListenerThread() {
            ThreadStart listenerStart = new ThreadStart(ListenForMessages);
            m_listenerThread = new Thread(listenerStart);
            m_listenerThread.IsBackground = true;
        }
        
        private void ListenForMessages() {
            while (m_listenerActive) {
                // receive messages
                SecureTcpClient client = null;
                try {
                    client = m_listener.AcceptTcpClient();
                    if (client != null) { // everything ok
                        // disable Nagle algorithm, to reduce delay
                        client.NoDelay = true;
                        // now process the message of this client
                        SslServerTransport transport = new SslServerTransport(client);
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
        public int StartListening(IPAddress bindTo, int listeningPortSuggestion, out TaggedComponent[] taggedComponents) {
            if (!m_isInitalized) {
                throw CreateNotInitalizedException();
            }
            if (m_listenerActive) {
                throw CreateAlreadyListeningException();
            }
            int resultPort = listeningPortSuggestion;
                        
            m_listener = new SecureTcpListener(bindTo, listeningPortSuggestion, m_sslOpts);
            // start TCP-Listening
            m_listener.Start();
            if (listeningPortSuggestion == 0) {
                // auto-assign port selected
                resultPort = ((IPEndPoint)m_listener.LocalEndpoint).Port;
            }
            
            if (m_isSecured) {
                // create ssl tagged component
                SSLComponentData sslData = new SSLComponentData(Convert.ToInt16(m_supportedOptions),
                                                                Convert.ToInt16(m_requiredOptions),
                                                                (short)resultPort);
                taggedComponents = new TaggedComponent[] {
                    new TaggedComponent(TAG_SSL_SEC_TRANS.ConstVal,
                                        m_codec.encode_value(sslData)) };
                resultPort = 0; // don't allow unsecured connections -> port is in ssl components
            } else {
                taggedComponents = new TaggedComponent[0];
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

