/* IIOPChannel.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dul@elca.ch
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Net;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop {

    /// <summary>
    /// This class represents a .NET Remoting channel for IIOP.
    /// </summary>
    /// <remarks>
    /// It delegates most of the work to the IiopChannelSender, 
    /// IiopChannelReceiver classes
    /// </remarks>
    public class IiopChannel : IChannelSender, IChannelReceiver {

        #region Constants

        internal const string DEFAULT_CHANNEL_NAME = "IIOPChannel";
        internal const int DEFAULT_CHANNEL_PRIORITY = 0;
        
        #endregion Constants
        #region IFields

        private string m_channelName = DEFAULT_CHANNEL_NAME;
        private int m_channelPriority = DEFAULT_CHANNEL_PRIORITY;

        private IiopClientChannel m_clientChannel;
        private IiopServerChannel m_serverChannel;

        #endregion IFields
        #region IConstructors
        
        public IiopChannel() {
            m_clientChannel = new IiopClientChannel();
            // because no port is specified, server part is not used
        }

        public IiopChannel(int port) : this() {
            m_serverChannel = new IiopServerChannel(port);
        }

        /// <summary>this constructor is used by configuration</summary>
        public IiopChannel(IDictionary properties, 
                           IClientChannelSinkProvider clientSinkProvider, 
                           IServerChannelSinkProvider serverSinkProvider) {
            IDictionary clientProp = new Hashtable();
            IDictionary serverProp = new Hashtable();
            bool isServer = false;
            // prepare properties for client channel and server channel
            if (properties != null) {
                foreach (DictionaryEntry entry in properties) {
                    switch ((string)entry.Key) {
                        case "name": 
                            m_channelName = (string)entry.Value; 
                            clientProp["name"] = m_channelName;
                            serverProp["name"] = m_channelName;
                            break;
                        case "priority": 
                            m_channelPriority = Convert.ToInt32(entry.Value); 
                            clientProp["priority"] = m_channelPriority;
                            serverProp["priority"] = m_channelPriority;
                            break;
                        case "port": 
                            serverProp["port"] = Convert.ToInt32(entry.Value); 
                            isServer = true;
                            break;
                        case "useIpAddress": 
                            serverProp["useIpAddress"] = Convert.ToBoolean(entry.Value); 
                            break;
                        default: 
                            Trace.WriteLine("unknown property found for IIOP channel: " +
                                            entry.Key);
                            break; // ignore, because unknown
                    }
                }
            }
            m_clientChannel = new IiopClientChannel(clientProp, clientSinkProvider);
            if (isServer) { 
                // only create server if port is specified
                m_serverChannel = new IiopServerChannel(serverProp, serverSinkProvider);
            }
        }

        #endregion IConstructors
        #region IProperties

        public string ChannelName {
            get { 
                return m_channelName;
            }
        }

        public int ChannelPriority {
            get { 
                return m_channelPriority; 
            }
        }

        public object ChannelData {
            get {
                if (m_serverChannel != null) {
                    return m_serverChannel.ChannelData;
                } else {
                    return null;
                }
            }
        }

        #endregion IProperties
        #region IMethods
        #region Implementation of IChannelSender
        public IMessageSink CreateMessageSink(string url, 
                                              object remoteChannelData, 
                                              out string objectURI) {
            Debug.WriteLine("create message sink for client channel");
            return m_clientChannel.CreateMessageSink(url, remoteChannelData,
                                                     out objectURI);
        }

        #endregion Implementation of IChannelSender
        #region Implementation of IChannel
        public string Parse(string url, out string objectURI) {
            Debug.WriteLine("called parse with url: " + url);
            return m_clientChannel.Parse(url, out objectURI);
        }

        #endregion Implementation of IChannel
        #region Implementation of IChannelReceiver

        public void StartListening(object data) {
            if (m_serverChannel != null) {
                m_serverChannel.StartListening(data);
            }
        }

        public void StopListening(object data) {
            if (m_serverChannel != null) {
                m_serverChannel.StopListening(data);
            }
        }

        public string[] GetUrlsForUri(string objectURI) {
            if (m_serverChannel != null) {
                return m_serverChannel.GetUrlsForUri(objectURI);
            } else {
                return null;
            }
        }
        
        #endregion Implementation of IChannelReceiver
        #endregion IMethods

    }


    /// <summary>
    /// this is the client side part of the IiopChannel
    /// </summary>
    public class IiopClientChannel : IChannelSender {
    
        #region IFields

        private string m_channelName = IiopChannel.DEFAULT_CHANNEL_NAME;
        private int m_channelPriority = IiopChannel.DEFAULT_CHANNEL_PRIORITY;

        private IClientChannelSinkProvider m_providerChain;

        #endregion IFields
        #region IConstructors
        
        public IiopClientChannel() {
            InitChannel();
        }

        /// <summary>the constructor used by the config file</summary>
        public IiopClientChannel(IDictionary properties, IClientChannelSinkProvider sinkProvider) {
            if (!CheckSinkProviderChain(sinkProvider)) { 
                throw new ArgumentException(
                     "IIOPClientSideFormatter provider not found in chain, this channel is only usable with the IIOPFormatters"); 
            }
            m_providerChain = sinkProvider;

            if (properties != null) {
                foreach (DictionaryEntry entry in properties) {
                    switch ((string)entry.Key) {
                        case "name": 
                            m_channelName = (string)entry.Value; 
                            break;
                        case "priority": 
                            m_channelPriority = Convert.ToInt32(entry.Value);
                            break;
                        default: 
                            Trace.WriteLine("unknown property found for IIOPClient channel: " + entry.Key);
                            break; // ignore, because unknown
                    }
                }
            }

            InitChannel();
        }

        #endregion IConstructors
        #region IProperties

        public string ChannelName {
            get { 
                return m_channelName; 
            }
        }

        public int ChannelPriority {
            get {
                return m_channelPriority; 
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>
        /// check if the custom provided provider chain contains the IIOPClientsideFormatter. This channel is not
        /// usable with another formatter ...
        /// </summary>
        private bool CheckSinkProviderChain(IClientChannelSinkProvider prov) {
            if (prov == null) { 
                return true; 
            }
            while (prov != null) {
                if (prov is IiopClientFormatterSinkProver) { 
                    return true; 
                }
                prov = prov.Next;
            }
            return false;
        }
        
        /// <summary>initalize this channel</summary>
        private void InitChannel() {
            if (m_providerChain != null) {
                // append transport provider to the chain
                IClientChannelSinkProvider prov = m_providerChain;
                while (prov.Next != null) { prov = prov.Next; }
                prov.Next = new IiopClientTransportSinkProvider(); // append the transport provider at the end
            } else {
                // create the default provider chain
                IClientFormatterSinkProvider formatterProv = new IiopClientFormatterSinkProver();
                formatterProv.Next = new IiopClientTransportSinkProvider();
                m_providerChain = formatterProv;
            }
        }

        #region Implementation of IChannelSender

        /// <summary>
        /// create the sink chain for the url and return a reference to the first sink in the chain
        /// </summary>
        public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI) {
            objectURI = null;
            if (url != null) {
                string chanURI = IiopUrlUtil.ParseUrl(url, out objectURI);
                if (chanURI == null) { return null; }
            } else {
                // check remoteChannelData
                Console.WriteLine("url null, remote channel data: " + remoteChannelData);
                // TODO
            }
            
            IClientChannelSink sink = m_providerChain.CreateSink(this, url, remoteChannelData);
            if (!(sink is IMessageSink)) { 
                throw new Exception("first sink in the client side channel must be a message-sink"); 
            }
            return (IMessageSink) sink;
        }
    
        #endregion Implementation of IChannelSender
        #region Implementation of IChannel
        public string Parse(string url, out string objectURI) {
            objectURI = null;
            return IiopUrlUtil.ParseUrl(url, out objectURI);
        }

        #endregion Implementation of IChannel
        #endregion IMethods
    }
    

    /// <summary>
    /// this is the server side of the IiopChannel
    /// </summary>
    public class IiopServerChannel : IChannelReceiver {

        #region IFields

        private string m_channelName = IiopChannel.DEFAULT_CHANNEL_NAME;
        private int m_channelPriority = IiopChannel.DEFAULT_CHANNEL_PRIORITY;

        private int m_port = 8085;
        private IiopChannelData m_channelData;

        private bool m_useIpAddr = true;
        private IPAddress m_myAddress;

        private TcpListener m_listener;
        private Thread m_listenerThread;

        private IServerChannelSinkProvider m_providerChain;
        /// <summary>the standard transport sink for this channel</summary>
        private IiopServerTransportSink m_transportSink;

        private bool m_listenActive = false;

        #endregion IFields
        #region IConstructors

        public IiopServerChannel() {
            InitChannel();
        }

        public IiopServerChannel(int port) {
            m_port = port;
            InitChannel();
        }

        /// <summary>Constructor used by configuration</summary>
        public IiopServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider) {
            if (!CheckSinkProviderChain(sinkProvider)) {
                throw new ArgumentException(
                    "IIOPServerSideFormatter provider not found in chain, this channel is only usable with the IIOPFormatters"); 
            }
            // parse properties
            if (properties != null) {
                foreach (DictionaryEntry entry in properties) {
                    switch ((string)entry.Key) {
                        case "name": 
                            m_channelName = (string)entry.Value; 
                            break;
                        case "priority": 
                            m_channelPriority = Convert.ToInt32(entry.Value); 
                            break;
                        case "port": 
                            m_port = Convert.ToInt32(entry.Value); 
                            break;
                        case "useIpAddress": 
                            m_useIpAddr = Convert.ToBoolean(entry.Value); 
                            break;
                        default: 
                            Trace.WriteLine("unknown property found for IIOPClient channel: " + entry.Key);
                            break; // ignore, because unknown
                    }
                }
            }
            InitChannel();
        }
        
        #endregion IConstructors
        #region IProperties
        
        public string ChannelName {
            get { 
                return m_channelName; 
            }
        }

        public int ChannelPriority {
            get {
                return m_channelPriority; 
            }
        }

        public object ChannelData {
            get { 
                return m_channelData;
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>
        /// check if the custom provided provider chain contains the IIOPServersideFormatter. This channel is not
        /// usable with another formatter!
        /// </summary>
        private bool CheckSinkProviderChain(IServerChannelSinkProvider prov) {
            if (prov == null) { 
                return true; 
            }
            while (prov != null) {
                if (prov is IiopServerFormatterSinkProvider) { 
                    return true; 
                }
                prov = prov.Next;
            }
            return false;
        }
        
        /// <summary>initalize the channel</summary>
        private void InitChannel() {
            if (m_port < 0) { 
                throw new ArgumentException("illegal port to listen on: " + m_port); 
            }
            SetupChannelData();
            // create the default provider chain, if no chain specified
            if (m_providerChain == null) {
                m_providerChain = new IiopServerFormatterSinkProvider();
            }
            
            IServerChannelSink sinkChain = ChannelServices.CreateServerChannelSinkChain(m_providerChain, this);
            m_transportSink = new IiopServerTransportSink(sinkChain);

            // ready to wait for messages
            StartListening(null);
            // publish init-service
            Services.CORBAInitServiceImpl.Publish();
            // public the handler for generic corba operations
            StandardCorbaOps.SetUpHandler();
        }

        private void SetupChannelData() {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostByName(hostName);
            IPAddress[] ipAddrs = ipEntry.AddressList;
            if ((ipAddrs == null) || (ipAddrs.Length == 0)) { 
                throw new Exception("can't determine ip-addr of local machine, abort channel creation"); 
            }
            m_myAddress = ipAddrs[0];
            if (m_useIpAddr) {
                m_channelData = new IiopChannelData(m_myAddress.ToString(), m_port);
            } else {
                m_channelData = new IiopChannelData(hostName, m_port);
            }
        }

        #region Implementation of IChannelReceiver
        public void StartListening(object data) {
            SetupListenerThread();
            // start TCP-Listening
            m_listener.Start();
            if (m_port == 0) { 
                // auto-assign port selected -> update data for this port
                m_port = ((IPEndPoint)m_listener.LocalEndpoint).Port; 
                SetupChannelData();
            }
            m_listenActive = true;
            // start the handler thread
            m_listenerThread.Start();
        }

        private void SetupListenerThread() {
            // use IPAddress.Any and not m_myAddress, to allow connections to loopback and normal ip
            m_listener = new TcpListener(IPAddress.Any, m_port);
            ThreadStart listenerStart = new ThreadStart(ListenForMessages);
            m_listenerThread = new Thread(listenerStart);
            m_listenerThread.IsBackground = true;
        }

        /// <summary>
        /// this method handles the incoming messages
        /// </summary>
        private void ListenForMessages() {
            while (m_listenActive) {
                // receive messages
                TcpClient client = null;
                try {
                    client = m_listener.AcceptTcpClient();
                    if (client != null) { // everything ok
                        // disable Nagle algorithm, to reduce delay
                        client.NoDelay = true;
                        // now process the message of this client
                        ServerRequestHandler handler = new ServerRequestHandler(client, m_transportSink);
                        handler.StartMsgHandling();
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
            
        public void StopListening(object data) {
            if (m_listenerThread != null) { 
                try {
                m_listenerThread.Interrupt(); m_listenerThread.Abort(); 
                } catch (Exception) { }
            }
            if (m_listener != null) { m_listener.Stop(); }
        }

        public string[] GetUrlsForUri(string objectURI) {
            return new string[] {
                IiopUrlUtil.GetUrl(m_channelData.HostName, m_channelData.Port, objectURI) };
        }

        #endregion Implementation of IChannelReceiver
        #region Implementation of IChannel
            
        public string Parse(string url, out string objectURI) {
            objectURI = null;
            return IiopUrlUtil.ParseUrl(url, out objectURI);
        }

        #endregion Implementation of IChannel
        #endregion IMethods
        
    }


    /// <summary>
    /// This class is used to hold the IiopChannel specific data
    /// </summary>
    /// <remarks>
    /// RemotingServices.Marshal queries all remoting channels registered for their channel-data.
    /// The IIOPChannel returns an instance of this class.
    /// </remarks>
    [Serializable] // must be serializable for the .NET framework
    public class IiopChannelData : ChannelDataStore {
        
        #region IFields
        private string m_hostName;
        private int m_port;
        #endregion IFields
        #region IConstructors
        public IiopChannelData(string hostName, int port) : base(new String[] { "iiop://"+hostName+":"+port } ) {
            m_hostName = hostName;
            m_port = port;
        }
        #endregion IConstructors
        #region IProperties

        public string HostName {
            get { return m_hostName; }
        }
        
        public int Port {
            get { return m_port; }
        }

        #endregion
        #region IMethods

        public override String ToString() {
            return "IIOP-channel data, hostname: " + m_hostName + 
                   ", port: " + m_port;
        }

        #endregion IMethods

    }
    
}
