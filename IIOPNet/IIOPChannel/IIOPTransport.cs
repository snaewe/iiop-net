/* IIOPTranport.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Text;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.MessageHandling;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop {   

    
    /// <summary>
    /// this class is the client side transport sink of the IIOPChannel
    /// </summary>
    public class IiopClientTransportSink : IClientChannelSink {

        #region IFields
    
        // empty property table
        private IDictionary m_properties = new Hashtable();

        private Uri         m_target;

        #endregion IFields
        #region IConstructors

        public IiopClientTransportSink(string url, object channelData) {
            Trace.WriteLine("create transport sink for URL: " + url);
            
            string objectURI;
            m_target = IiopUrlUtil.ParseUrl(url, out objectURI);

            Trace.WriteLine("client transport, host: " + m_target.Host + ", port :" + m_target.Port);
        }

        #endregion IConstructors
        #region IProperties

        public IClientChannelSink NextChannelSink {
        	get {
                return null; // no more sinks
            }
        }

        public System.Collections.IDictionary Properties {
            get {
                return m_properties;
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>
        /// gets a socket to the target to send the message
        /// </summary>
        /// <param name="msg">The message to send</param>
        private TcpClient GetSocket(IMessage msg) {
            IiopClientConnectionManager conManager = IiopClientConnectionManager.GetManager();
            IiopClientConnection con = conManager.GetConnectionFor(msg);
            if (!con.CheckConnected()) {
                con.ConnectTo(m_target.Host, m_target.Port);
                return con.ClientSocket;
            } else {
                return con.ClientSocket;
            }
        }

        /// <summary>
        /// send the formatted request to the target
        /// </summary>
        private void ProcessRequest(Stream networkStream, Stream requestStream) {                      
            GiopTransportClientMsgHandler transportHandler = 
                new GiopTransportClientMsgHandler();
            
            requestStream.Seek(0, SeekOrigin.Begin); // go to the beginning of the stream
            
            Debug.WriteLine("sending an IIOP message in the Client side Transport sink");
            transportHandler.SendRequestMessage(requestStream, networkStream);
            Debug.WriteLine("message sent");
        }

        /// <summary>
        /// process the response stream, before forwarding it to the formatter
        /// </summary>
        /// <remarks>
        /// Precondition: in the networkStream, the message following is not a response
        /// to another request, than the request sent by ProcessMessage
        /// </remarks>
        private void ProcessResponse(NetworkStream networkStream, uint forReqId,
                                     out Stream responseStream,
                                     out ITransportHeaders responseHeaders) {
            responseHeaders = new TransportHeaders();
            responseStream = null;
			          
            GiopTransportClientMsgHandler transportHandler = 
                new GiopTransportClientMsgHandler();
            Debug.WriteLine("receiving an IIOP message in the Client side Transport sink");
            responseStream = transportHandler.ReceiveResponseMessage(networkStream,
                                                                     forReqId);            
            Debug.WriteLine("message received");
            responseStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
        }

        #region Implementation of IClientChannelSink
        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, 
                                        ITransportHeaders headers, Stream requestStream) {
            // this is the last sink in the chain, therefore the call is not forwarded, instead the request is sent
            TcpClient client = GetSocket(msg);
            NetworkStream networkStream = client.GetStream();
            ProcessRequest(networkStream, requestStream); // send the request
            
            // wait only for response, if not oneway!
            if (!GiopMessageHandler.IsOneWayCall((IMethodCallMessage)msg)) {
            
                uint reqNr = (uint)msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY];
                // now initate asynchronous response listener
                AsyncResponseWaiter waiter = new AsyncResponseWaiter(sinkStack, networkStream, reqNr, 
                                                                     new DataAvailableCallBack(this.AsyncResponseArrived));
                waiter.StartWait(); // this call is non-blocking
            }
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
                                   out ITransportHeaders responseHeaders, out Stream responseStream) {
            // called by the chain, chain expect response-stream and headers back
            TcpClient client = GetSocket(msg);
            NetworkStream networkStream = client.GetStream();
            ProcessRequest(networkStream, requestStream);            
            ProcessResponse(networkStream, (uint)msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY],
                            out responseStream, out responseHeaders);
            // the previous sink in the chain does further process this response ...
        }

        /// <summary>call back to call, when the async response has arrived.</summary>
        private void AsyncResponseArrived(NetworkStream networkStream, IClientChannelSinkStack sinkStack,
                                          uint reqNr) {
            ITransportHeaders responseHeaders;
            Stream responseStream;
            ProcessResponse(networkStream, reqNr, out responseStream, out responseHeaders);
            // forward the response
            sinkStack.AsyncProcessResponse(responseHeaders, responseStream);
        }

        public void AsyncProcessResponse(System.Runtime.Remoting.Channels.IClientResponseChannelSinkStack sinkStack, object state, System.Runtime.Remoting.Channels.ITransportHeaders headers, System.IO.Stream stream) {
            throw new NotSupportedException(); // this should not be called, because this sink is the first in the chain, receiving the response
        }
        
        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers) {
            // let the previous sink create the stream into which the message is serialised.
            // To open the connection already here and give the network-stream to the previous sink is too risky.
            return null;
        }

        #endregion
        
        #endregion IMethods

    }


    /// <summary>
    /// this class is the server side transport sink for the IIOPChannel
    /// </summary>
    public class IiopServerTransportSink : IServerChannelSink {

        #region IFields
    
        // empty property table
        private Hashtable m_properties = new Hashtable();
        
        /// <summary>the next sink after the transport sink on the server side</summary>
        private IServerChannelSink m_nextSink;
        
        private AutoResetEvent m_waitForAsyncComplete = new AutoResetEvent(false);

        #endregion IFields
        #region IConstructors

        public IiopServerTransportSink(IServerChannelSink nextSink) {
            m_nextSink = nextSink; // this is the first sink in the server chain, set next sink
        }

        #endregion IConstructors
        #region IProperties

        public IServerChannelSink NextChannelSink {
            get {
                return m_nextSink;
            }
        }

        public System.Collections.IDictionary Properties {
            get {
                return m_properties;
            }
        }

        #endregion IProperties
        #region IMethods
        
        /// <summary>
        /// processes an incoming request
        /// </summary>
        /// <param name="giopTransportMsgHandler">
        /// Knows, how to process Giop-Messages.
        /// </param>
        /// <returns>true, if more messages can be read, otherwise false</returns>
        internal bool Process(NetworkStream networkStream, 
                              GiopTransportServerMsgHandler giopTransportMsgHandler) {
                                                          
            // read in the message
            GiopTransportServerMsgHandler.HandlingResult result = giopTransportMsgHandler.ProcessIncomingMsg();
            if (result == GiopTransportServerMsgHandler.HandlingResult.AsyncReply) {
               // wait for async response               
               Debug.WriteLine("AsyncReply: wait for async response");
               m_waitForAsyncComplete.WaitOne();
               Debug.WriteLine("AsyncReply: response sent");
               // now, can read next messages
               return true;
            } else if (result == GiopTransportServerMsgHandler.HandlingResult.ConnectionClose) {
                return false;    
            } else {
                return true;
            }                        
        }
        
        #region Implementation of IServerChannelSink
        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state,
                                        IMessage msg, ITransportHeaders headers) {
            return null; // do not force a use of a response-stream, delegate descision to other sinks
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg,
                                               ITransportHeaders requestHeaders, Stream requestStream,
                                               out IMessage responseMsg, out ITransportHeaders responseHeaders, 
                                               out Stream responseStream) {
            throw new NotSupportedException(); // is always first sink, ProcessMessage is therefor not useful
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, 
                                         IMessage msg, ITransportHeaders headers, Stream stream) {
            GiopTransportServerMsgHandler giopTransportMsgHandler =
                 (GiopTransportServerMsgHandler) state;
            giopTransportMsgHandler.SendResponseMessage(stream); // send the response
            // stream is now available for processing next messages
            m_waitForAsyncComplete.Set();
        }

        #endregion
               
        #endregion IMethods
    }

    
    /// <summary>
    /// this class is a provider for the IIOPClientTransportSink
    /// </summary>
    public class IiopClientTransportSinkProvider : IClientChannelSinkProvider {
    
        #region IProperties

        public IClientChannelSinkProvider Next {
            get {
                return null; // no next provider, because transport provider is the last provider on client side
            }
            set {
                throw new NotSupportedException("a provider after client side transport is not allowed");
            }
        }

        #endregion IProperties
        #region IMethods

        #region Implementation of IClientChannelSinkProvider

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData) {
            if ((!(channel is IiopChannel)) && (!(channel is IiopClientChannel))) {
                throw new ArgumentException("this provider is only usable with the IIOPChannel, but not with : " + channel);
            }
            return new IiopClientTransportSink(url, remoteChannelData);
        }

        #endregion

        #endregion IMethods

    }

    /// <summary>this delegate is used to call back the ClientTransportSink, when response data is available</summary>
    internal delegate void DataAvailableCallBack(NetworkStream networkStream, IClientChannelSinkStack sinkStack,
                                                 uint forReqId);
    
    /// <summary>
    /// this class provide functionaly for waiting for an aync response
    /// </summary>
    internal class AsyncResponseWaiter {

        #region IFields
        
        private NetworkStream m_stream;
        private IClientChannelSinkStack m_sinkStack;
        private DataAvailableCallBack m_callBack;
        private GiopConnectionContext m_connectionDesc;
        private uint m_reqNr;

        #endregion IFields
        #region IConstructors
        
        public AsyncResponseWaiter(IClientChannelSinkStack sinkStack, NetworkStream networkStream,
                                   uint reqNr, DataAvailableCallBack callBack) {
            m_sinkStack = sinkStack;
            m_stream = networkStream;
            m_callBack = callBack;
            m_connectionDesc = IiopConnectionManager.GetCurrentConnectionContext();
            m_reqNr = reqNr;
        }

        #endregion IConstructors
        #region IMethods

        /// <summary> waits asynchronously for data</summary>
        internal void StartWait() {
            ThreadStart start = new ThreadStart(this.WaitForData);
            Thread waitForResult = new Thread(start);
            waitForResult.Start();
        }

        /// <summary>wait for data on the stream</summary>
        private void WaitForData() {
            while (!(m_stream.DataAvailable)) {
                // let the other threads continue ...
                Thread.Sleep(0);
            }
            // call back the handler
            IiopConnectionManager.SetCurrentConnectionContext(m_connectionDesc); // make the current connection context available in this thread            
            m_callBack(m_stream, m_sinkStack, m_reqNr);
        }

        #endregion IMethods

    }


    /// <summary>
    /// this class is responsible for reading requests out of a network-stream on the server side
    /// </summary>
    internal class ServerRequestHandler {

        #region IFields
        
        private TcpClient m_client;
        private IiopServerTransportSink m_transportSink;

        #endregion IFields
        #region IConstructors
        
        public ServerRequestHandler(TcpClient client, IiopServerTransportSink transportSink) {
            m_client = client;
            m_transportSink = transportSink;
        }

        #endregion IConstructors
        #region IMethods

        public void StartMsgHandling() {
            ThreadStart start = new ThreadStart(this.HandleRequests);
            Thread handleThread = new Thread(start);
            // do not prevent main thread from exiting on app end:
            handleThread.IsBackground = true;
            handleThread.Start();
        }

        private void HandleRequests() {
            bool connected = true;
            NetworkStream inStream = m_client.GetStream();
            // create a connection context for the server connection
            IiopServerConnectionManager.GetManager().RegisterActiveConnection();
            
            GiopTransportServerMsgHandler serverMsgHandler = 
                new GiopTransportServerMsgHandler(inStream, m_transportSink);
            while (connected) {
                try {
                    bool okToReceiveMore = 
                        m_transportSink.Process(inStream, serverMsgHandler);
                    if (!okToReceiveMore) {
                        // close connection
                        m_client.Close();
                        connected = false;
                    }                    
                } catch (IOException ie) {
                    if (!(typeof(SocketException).IsInstanceOfType(ie.InnerException))) {
                        Debug.WriteLine("unexpected Exception in handle-requests: " + ie);
                    }
                    connected = false;
                } catch (Exception e) {
                    Debug.WriteLine("Exception in handle-requests: " + e);
                    Debug.WriteLine("inner-exception: " + e.InnerException);
                    connected = false;
                    try { 
                        inStream.Close();    
                    } catch (Exception) { }
                    try { 
                        m_client.Close();
                    } catch (Exception) { }
                }
            }
            IiopServerConnectionManager.GetManager().UnregisterActiveConnection(); // discard connection
        }

        #endregion IMethods

    }
    
}
