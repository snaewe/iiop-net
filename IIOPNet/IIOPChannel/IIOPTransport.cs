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


    /// <summary>this delegate is used to call back the ClientTransportSink, when response message is available</summary>
    /// <param name="resultStream">the response message, if everything went ok, otherwise null</param>
    /// <param name="resultException">the exception, if something was not ok, othersise null</param>
    internal delegate void AsyncResponseAvailableCallBack(IClientChannelSinkStack sinkStack, GiopClientConnection con,
                                                          Stream resultStream, Exception resultException);

    
    /// <summary>
    /// this class is the client side transport sink of the IIOPChannel
    /// </summary>
    public class IiopClientTransportSink : IClientChannelSink {

        #region IFields
    
        // empty property table
        private IDictionary m_properties = new Hashtable();       
        private GiopClientConnectionManager m_conManager;

        #endregion IFields
        #region IConstructors

        internal IiopClientTransportSink(string url, object channelData, 
                                         GiopClientConnectionManager conManager) {
            Trace.WriteLine("create transport sink for URL: " + url);
            m_conManager = conManager;    
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
                
        private GiopClientConnection GetClientConnection(IMessage msg) {
            GiopClientConnection con = m_conManager.GetConnectionFor(msg);            
            if (con ==  null) {
                // should not occur, because AllocateConnectionFor has been previouse called
                throw new omg.org.CORBA.INTERNAL(998, omg.org.CORBA.CompletionStatus.Completed_No);
            }
            if (!con.CheckConnected()) {
                // a new connection must not be opened, because this would require a remarshal 
                // of the message (service-contexts) -> Therefore connection must already be open
                throw new omg.org.CORBA.TRANSIENT(CorbaSystemExceptionCodes.TRANSIENT_CONNECTION_DROPPED, 
                                                  omg.org.CORBA.CompletionStatus.Completed_No, "Connection to target lost");
            }
            return con;
        }

        #region Implementation of IClientChannelSink
        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, 
                                        ITransportHeaders headers, Stream requestStream) {
            #if DEBUG
            OutputHelper.LogStream(requestStream);
            #endif            
            // this is the last sink in the chain, therefore the call is not forwarded, instead the request is sent
            GiopClientConnection clientCon = GetClientConnection(msg);
            GiopTransportMessageHandler handler = clientCon.TransportHandler;
            uint reqNr = (uint)msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY];
            if (!GiopMessageHandler.IsOneWayCall((IMethodCallMessage)msg)) {
                handler.SendRequestMessageAsync(requestStream, reqNr, 
                                                new AsyncResponseAvailableCallBack(this.AsyncResponseArrived),
                                                sinkStack, clientCon);
            } else {
                handler.SendRequestMessageOneWay(requestStream, reqNr, clientCon);
            }                        
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
                                   out ITransportHeaders responseHeaders, out Stream responseStream) {
            #if DEBUG
            OutputHelper.LogStream(requestStream);
            #endif
            // called by the chain, chain expect response-stream and headers back
            GiopClientConnection clientCon = GetClientConnection(msg);
            GiopTransportMessageHandler handler = clientCon.TransportHandler;
            // send request and wait for response
            responseHeaders = new TransportHeaders();
            responseHeaders[GiopClientConnectionDesc.CLIENT_TR_HEADER_KEY]= clientCon.Desc; // add to response headers            
            uint reqNr = (uint)msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY];
            responseStream = handler.SendRequestSynchronous(requestStream, reqNr, clientCon);
            #if DEBUG
            OutputHelper.LogStream(responseStream);
            #endif            
            responseStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
            // the previous sink in the chain does further process this response ...
        }

        /// <summary>call back to call, when the async response has arrived.</summary>
        private void AsyncResponseArrived(IClientChannelSinkStack sinkStack, GiopClientConnection con,
                                          Stream responseStream, Exception resultException) {
            ITransportHeaders responseHeaders = new TransportHeaders();
            responseHeaders[GiopClientConnectionDesc.CLIENT_TR_HEADER_KEY]= con.Desc; // add to response headers            

            // forward the response
            if ((resultException == null) && (responseStream != null)) {
                #if DEBUG
                OutputHelper.LogStream(responseStream);
                #endif                            
                responseStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
                sinkStack.AsyncProcessResponse(responseHeaders, responseStream);
            } else {
                Exception toThrow = resultException;
                if (toThrow == null) {
                    toThrow = new omg.org.CORBA.INTERNAL(79, omg.org.CORBA.CompletionStatus.Completed_MayBe);
                }
                sinkStack.DispatchException(toThrow);
            }                        
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
    public class IiopServerTransportSink : IServerChannelSink, IGiopRequestMessageReceiver {

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
                
        private void ProcessRequestInternal(Stream requestStream, GiopServerConnection serverCon) {
#if DEBUG
            OutputHelper.LogStream(requestStream);
#endif
            requestStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
            // the out params returned form later sinks
            IMessage responseMsg;
            ITransportHeaders responseHeaders;
            Stream responseStream;
            
            // create the sink stack for async processing of message
            ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
            sinkStack.Push(this, serverCon);
            // empty transport headers for this protocol
            ITransportHeaders requestHeaders = new TransportHeaders();
            requestHeaders[GiopServerConnection.SERVER_TR_HEADER_KEY] = serverCon;
            requestHeaders[CommonTransportKeys.IPAddress] = serverCon.TransportHandler.GetPeerAddress();
            
            // next sink will process the request-message
            ServerProcessing result = 
                NextChannelSink.ProcessMessage(sinkStack, null, /* no RequestMessage in transport handler */
                                               requestHeaders, requestStream, 
                                               out responseMsg, out responseHeaders,
                                               out responseStream);
            switch (result) {
                case ServerProcessing.Complete :
                    try { 
                        sinkStack.Pop(this); 
                    } catch (Exception) { }                    
#if DEBUG
                    Debug.WriteLine("Send response sync");
                    OutputHelper.LogStream(responseStream);
#endif                    
                    serverCon.TransportHandler.SendResponse(responseStream);
                    break;                    
                case ServerProcessing.Async : 
                    sinkStack.StoreAndDispatch(this, serverCon); // this sink wants to handle response
                    // no reply, async
                    break;
                case ServerProcessing.OneWay :
                    try { 
                        sinkStack.Pop(this); 
                    } catch (Exception) { }
                    // no message to send
                    break;
                default:
                    // should not arrive here
                    Trace.WriteLine("internal problem, invalid processing state: " + result);
                    throw new omg.org.CORBA.INTERNAL(568, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }            
        }
        
        /// <summary>
        /// process a giop request
        /// </summary>
        /// <param name="requestStream">the request stream</param>
        /// <remarks>is called by GiopTransportMessageHandler</remarks>
        public void ProcessRequest(Stream requestStream, GiopServerConnection serverCon) {
            Trace.WriteLine("Process request");
            ProcessRequestInternal(requestStream, serverCon);
            Trace.WriteLine("Request processed");
        }
        
        /// <summary>
        /// process a giop locate request
        /// </summary>
        /// <param name="requestStream">the request stream</param>
        /// <remarks>is called by GiopTransportMessageHandler</remarks>
        public void ProcessLocateRequest(Stream requestStream, GiopServerConnection serverCon) {
            Trace.WriteLine("Process Locate request");
            ProcessRequestInternal(requestStream, serverCon);
            Trace.WriteLine("Locate request processed");
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
#if DEBUG
            Debug.WriteLine("Send response async");
            OutputHelper.LogStream(stream);
#endif
            GiopTransportMessageHandler giopTransportMsgHandler =
                ((GiopServerConnection) state).TransportHandler;
            giopTransportMsgHandler.SendResponse(stream); // send the response
        }

        #endregion
               
        #endregion IMethods
    }

    
    /// <summary>
    /// this class is a provider for the IIOPClientTransportSink
    /// </summary>
    internal class IiopClientTransportSinkProvider : IClientChannelSinkProvider {
    
        #region IFields
        
        private GiopClientConnectionManager m_conManager;
        
        #endregion IFiels
        #region IConstructors
        
        /// <param name="conManager">the connection manager for this channel, must be != null</param>
        internal IiopClientTransportSinkProvider(GiopClientConnectionManager conManager) {
            if (conManager == null) {
                // should not occur
                throw new omg.org.CORBA.INTERNAL(997, omg.org.CORBA.CompletionStatus.Completed_No);
            }            
            m_conManager = conManager;
        }
        
        #endregion IConstructors
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
            
            return new IiopClientTransportSink(url, remoteChannelData,
                                               m_conManager);
        }

        #endregion

        #endregion IMethods

    }
    
}
