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
            requestStream.Seek(0, SeekOrigin.Begin); // go to the beginning of the stream
            Debug.WriteLine("sending an IIOP message in the Client side Transport sink");
            // for (int i = 0; i < requestStream.Length; i++) {
            //    byte nextByte = (byte)requestStream.ReadByte();
            //    if (i % 16 == 0) { Debug.WriteLine(""); }
            //    Debug.Write(nextByte + " ");
            //    networkStream.WriteByte(nextByte);
            // }
            // requestStream.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[requestStream.Length];
            requestStream.Read(data, 0, (int)requestStream.Length);
            for (int i = 0; i < data.Length; i++) {
                if (i % 16 == 0) { 
                    Debug.WriteLine(""); 
                }
                Debug.Write(data[i] + " ");
            }
            networkStream.Write(data, 0, (int)requestStream.Length);
            Debug.WriteLine("message sent");
        }

        /// <summary>
        /// process the response stream, before forwarding it to the formatter
        /// </summary>
        /// <remarks>
        /// Precondition: in the networkStream, the message following is not a response
        /// to another request, than the request sent by ProcessMessage
        /// </remarks>
        private void ProcessResponse(NetworkStream networkStream, out Stream responseStream, 
                                     out ITransportHeaders responseHeaders) {  
            responseHeaders = new TransportHeaders();
            responseStream = null;
			          
            FragmentedMsgAssembler fragmentAssembler = new FragmentedMsgAssembler();
            bool fullyRead = false;
            
            while (!fullyRead) {
                // create a stream for reading a new message
                CdrInputStreamImpl reader = new CdrInputStreamImpl(networkStream);
                GiopHeader msgHeader = new GiopHeader(reader);
             
            	switch(msgHeader.GiopType) {
            		case GiopMsgTypes.Reply:
            		    if ((msgHeader.GiopFlags & GiopHeader.FRAGMENT_MASK) > 0) {
                            fragmentAssembler.StartFragment(reader, msgHeader);
                        } else {
                            // no fragmentation
                            responseStream = new MemoryStream();
                            msgHeader.WriteToStream(responseStream,
                                                    msgHeader.ContentMsgLength);
                            byte[] body = reader.ReadOpaque((int)msgHeader.ContentMsgLength);
                            responseStream.Write(body, 0, body.Length);
                            fullyRead = true; // no more fragments
                        }                    
                        break;
            		case GiopMsgTypes.Fragment:
            			if (!(fragmentAssembler.IsLastFragment(msgHeader))) {
                            fragmentAssembler.AddFragment(reader, msgHeader);
                        } else {
                            responseStream = fragmentAssembler.FinishFragmentedMsg(reader, 
                                                                                   msgHeader);
                        	fullyRead = true; // no more fragments
                        }
                        break;            			
            		default:
            			throw new IOException("unsupported GIOP-msg received: " +
            			                      msgHeader.GiopType);
            	}
            	
            } // end while (!fullyRead)

            responseStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
                                         
        }


        #region Implementation of IClientChannelSink
        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, 
                                        ITransportHeaders headers, Stream requestStream) {
            // this is the last sink in the chain, therefore the call is not forwarded, instead the request is sent
            TcpClient client = GetSocket(msg);
            NetworkStream networkStream = client.GetStream();
            ProcessRequest(networkStream, requestStream); // send the request
            // now initate asynchronous response listener
            AsyncResponseWaiter waiter = new AsyncResponseWaiter(sinkStack, networkStream, 
                                                                 new DataAvailableCallBack(this.AsyncResponseArrived));
            waiter.StartWait(); // this call is non-blocking
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
                                   out ITransportHeaders responseHeaders, out Stream responseStream) {
            // called by the chain, chain expect response-stream and headers back
            TcpClient client = GetSocket(msg);
            NetworkStream networkStream = client.GetStream();
            ProcessRequest(networkStream, requestStream);
            ProcessResponse(networkStream, out responseStream, out responseHeaders);
            // the previous sink in the chain does further process this response ...
        }

        /// <summary>call back to call, when the async response has arrived.</summary>
        private void AsyncResponseArrived(NetworkStream networkStream, IClientChannelSinkStack sinkStack) {
            ITransportHeaders responseHeaders;
            Stream responseStream;
            ProcessResponse(networkStream, out responseStream, out responseHeaders);
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
        /// handles a Locate request msg: sends a LocateReply message as a result.
        /// </summary>
        private void HandleLocateRequest(Stream msgStream, Stream networkStream) {
            msgStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in GiopMessageHandler

            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            Stream resultMsgStream = handler.HandleIncomingLocateRequestMessage(msgStream);
            SendResponseMessage(networkStream, resultMsgStream);
        }

        /// <summary>
        /// handles a request msg: sends a Reply message as a result.
        /// </summary>
        /// <param name="msgStream"></param>
        /// <param name="networkStream"></param>
        private void HandleRequest(Stream msgStream, Stream networkStream) {
            msgStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter

            
            // the out params returned form later sinks
            IMessage responseMsg;
            ITransportHeaders responseHeaders;
            Stream responseStream;
            
            // create the sink stack for async processing of message
            ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
            sinkStack.Push(this, networkStream);
            // empty transport headers for this protocol
            ITransportHeaders requestHeaders = new TransportHeaders();
            
            // next sink will process the request-message
            ServerProcessing result = m_nextSink.ProcessMessage(sinkStack, null, /* no RequestMessage in transport handler */
                                                                requestHeaders, msgStream, 
                                                                out responseMsg, out responseHeaders,
                                                                out responseStream);
            switch (result) {
                case ServerProcessing.Complete :
                    try { 
                        sinkStack.Pop(this); 
                    } catch (Exception) { }
                    SendResponseMessage(networkStream, responseStream);
                    break;
                case ServerProcessing.Async : 
                    sinkStack.StoreAndDispatch(this, networkStream); // this sink wants to handle response
                    break;
                case ServerProcessing.OneWay :
                    try { 
                        sinkStack.Pop(this); 
                    } catch (Exception) { }
                    // no message to send
                    break;
            }

        }

        /// <summary>
        /// creates the stream for an unfragmented giop message, which can be used for deserialising the
        /// message.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="header">Giop header of the message</param>
        /// <returns></returns>
        private Stream CreateStreamForGiopMessage(CdrInputStreamImpl reader, GiopHeader msgHeader) {
            Stream msgStream = new MemoryStream();
            msgHeader.WriteToStream(msgStream, msgHeader.ContentMsgLength);
            byte[] body = reader.ReadOpaque((int)msgHeader.ContentMsgLength);
            msgStream.Write(body, 0, body.Length);
            return msgStream;
        }

        
        /// <summary>
        /// processes an incoming request
        /// </summary>
        /// <param name="fragmentAssembler">
        /// The fragmented message assembler and manager for this connection
        /// </param>
        internal void Process(NetworkStream networkStream, 
                              FragmentedMsgAssembler fragmentAssembler) {
                                                          
            // read in the message
            CdrInputStreamImpl reader = new CdrInputStreamImpl(networkStream);
            GiopHeader msgHeader = new GiopHeader(reader);
            Stream msgStream;
            switch(msgHeader.GiopType) {
                case GiopMsgTypes.Request:
                    if ((msgHeader.GiopFlags & GiopHeader.FRAGMENT_MASK) > 0) {
                        fragmentAssembler.StartFragment(reader, msgHeader);
                        // more fragments
                    } else {
                        // no fragmentation
                        msgStream = CreateStreamForGiopMessage(reader, msgHeader);
                        HandleRequest(msgStream, networkStream);
                    }                    
                    break;
                case GiopMsgTypes.Fragment:
                    if (!(fragmentAssembler.IsLastFragment(msgHeader))) {
                        fragmentAssembler.AddFragment(reader, msgHeader);
                        // more fragments to follow
                    } else {
                        msgStream = fragmentAssembler.FinishFragmentedMsg(reader, 
                                                                          ref msgHeader);
                        if (msgHeader.GiopType.Equals(GiopMsgTypes.Request)) {
                            HandleRequest(msgStream, networkStream);
                        } else if (msgHeader.GiopType.Equals(GiopMsgTypes.LocateRequest)) {
                            HandleLocateRequest(msgStream, networkStream);
                        }
                    }
                    break;
                case GiopMsgTypes.CloseConnection:
                    networkStream.Close();
                    break;
                case GiopMsgTypes.LocateRequest:
                    // read the message, may be fragmented in GIOP 1.2
                    if ((msgHeader.GiopFlags & GiopHeader.FRAGMENT_MASK) > 0) {
                        fragmentAssembler.StartFragment(reader, msgHeader);
                        // more fragments
                    } else {
                        msgStream = CreateStreamForGiopMessage(reader, msgHeader);
                        HandleLocateRequest(msgStream, networkStream);
                    }
                    break;
                default:
                    throw new IOException("unsupported Giop message : " +
                                          msgHeader.GiopType);
            }
            
        }

        /// <summary>sends a response message</summary>
        private void SendResponseMessage(Stream networkStream, Stream responseMsgStream) {
            responseMsgStream.Seek(0, SeekOrigin.Begin); // go to the beginning of the stream
            Debug.WriteLine("sending an IIOP message in the server side Transport sink");
            // for (int i = 0; i < responseMsgStream.Length; i++) {
            //    byte nextByte = (byte)responseMsgStream.ReadByte();
            //    if (i % 16 == 0) { Debug.WriteLine(""); }
            //    Debug.Write(nextByte + " ");
            //    networkStream.WriteByte(nextByte);
            //}
            byte[] data = new byte[responseMsgStream.Length];
            responseMsgStream.Read(data, 0, (int)responseMsgStream.Length);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                if (i % 16 == 0) {
                    Debug.WriteLine("  |   " +sb.ToString()); 
                    sb = new StringBuilder();
                }
                String msg = String.Format("{0, 3} ", data[i]);
                char ch = (char)data[i];
                sb = sb.Append(((ch > 0x20) ? ch.ToString() : " "));
                Debug.Write(msg);
            }
            for (int i = data.Length % 16; i < 16; i++) {
                Debug.Write("    ");
            }
            Debug.WriteLine("  |   " +sb.ToString());
            networkStream.Write(data, 0, data.Length);
            responseMsgStream.Close();
            Debug.WriteLine("message sent");
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

        public void AsyncProcessResponse(System.Runtime.Remoting.Channels.IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream) {
            SendResponseMessage((Stream) state, stream); // send the response stream
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
    internal delegate void DataAvailableCallBack(NetworkStream networkStream, IClientChannelSinkStack sinkStack);
    
    /// <summary>
    /// this class provide functionaly for waiting for an aync response
    /// </summary>
    internal class AsyncResponseWaiter {

        #region IFields
        
        private NetworkStream m_stream;
        private IClientChannelSinkStack m_sinkStack;
        private DataAvailableCallBack m_callBack;
        private GiopConnectionContext m_connectionDesc;

        #endregion IFields
        #region IConstructors
        
        public AsyncResponseWaiter(IClientChannelSinkStack sinkStack, NetworkStream networkStream,
                                   DataAvailableCallBack callBack) {
            m_sinkStack = sinkStack;
            m_stream = networkStream;
            m_callBack = callBack;
            m_connectionDesc = IiopConnectionManager.GetCurrentConnectionContext();
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
            m_callBack(m_stream, m_sinkStack);
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
            handleThread.Start();
        }

        private void HandleRequests() {
            bool connected = true;
            NetworkStream inStream = m_client.GetStream();
            // create a connection context for the server connection
            IiopServerConnectionManager.GetManager().RegisterActiveConnection();
            
            FragmentedMsgAssembler fragmentAssembler = new FragmentedMsgAssembler();            
            while (connected) {
                try {
                    m_transportSink.Process(inStream, fragmentAssembler);
                    
                    // check connected:
                    // TODO
                    
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
