/* Formatter.cs
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
using System.Collections;
using System.IO;
using System.Diagnostics;
using Ch.Elca.Iiop.MessageHandling;
using Ch.Elca.Iiop.Services;

namespace Ch.Elca.Iiop {
    
    /// <summary>
    /// the message types in the IIOP Protocol
    /// </summary>
    public enum IiopMsgTypes {
        Request = 0,
        Reply = 1,
        CancelRequest = 2,
        LocateRequest = 3,
        LocateReply = 4,
        CloseConnection = 5,
        MessageError = 6,
        Fragment = 7
    }

    
    /// <summary>
    /// this class is a client side formatter for IIOP-messages in the IIOP-channel
    /// </summary>
    public class IiopClientFormatterSink : IClientFormatterSink {
    
        #region IFields

        private IDictionary m_properties = new Hashtable();
        
        private IClientChannelSink m_nextSink;

        #endregion IFields
        #region IConstructors

        public IiopClientFormatterSink(IClientChannelSink nextSink) {
            m_nextSink = nextSink;
        }

        #endregion IConstructors
        #region IProperties

        public System.Runtime.Remoting.Messaging.IMessageSink NextSink {
            get { 
                throw new NotSupportedException(); 
            } // this sink serialises the message, therefore no other message sinks possible
        }

        public IClientChannelSink NextChannelSink {
            get    {
                return m_nextSink; // the next sink, processing the serialised msg
            }
        }

        public System.Collections.IDictionary Properties {
            get    {
                return m_properties;
            }
        }

        #endregion IProperties
        #region IMethods
        
        /// <summary>serialises the .NET msg to a GIOP-message</summary>
        private void SerialiseRequest(IMessage msg, GiopRequestNumberGenerator reqNumGen, 
                                      out ITransportHeaders headers, out Stream stream) {
            headers = new TransportHeaders();
            // get the stream into which the message should be serialied from the first stream handling
            // sink in the stream handling chain
            stream = m_nextSink.GetRequestStream(msg, headers);
            if (stream == null) { // the next sink delegated the decision to which stream the message should be serialised to this sink
                stream = new MemoryStream(); // create a new stream
            }
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            handler.SerialiseOutgoingClientMessage(msg, stream, reqNumGen);
        }

        /// <summary>deserialises an IIOP-msg from the response stream</summary>
        /// <param name="result">the .NET message created from the IIOP-msg</param>
        /// <returns>status</returns>
        internal IncomingHandlingStatus DeserialiseResponse(Stream responseStream, 
                                                            ITransportHeaders headers, IMessage requestMsg,
                                                            out IMessage result) {
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            IncomingHandlingStatus status = handler.ParseIncomingClientMessage(responseStream, 
                                                        (IMethodCallMessage) requestMsg, out result);
            responseStream.Close(); // stream not needed any more
            IiopClientConnectionManager conManager = IiopClientConnectionManager.GetManager();
            conManager.ReleaseClientConnection(); // release the current connection, because this interaction is complete
            
            return status;
        }
    
        #region Implementation of IMessageSink
        public IMessage SyncProcessMessage(IMessage msg) {
            // prepare connection
            IiopClientConnectionManager manager = IiopClientConnectionManager.GetManager();
            IiopClientConnection con = manager.CreateOrGetClientConnection(this, 
                                                                           (string)msg.Properties["__Uri"]);
            GiopRequestNumberGenerator reqNumGen = con.ReqNumberGen;
            // serialise
            IMessage result;
            try {
                ITransportHeaders requestHeaders;
                Stream requestStream;
                SerialiseRequest(msg, reqNumGen, out requestHeaders, out requestStream);

                // pass the serialised GIOP-request to the first stream handling sink
                // when the call returns, the response message has been received
                ITransportHeaders responseHeaders;
                Stream responseStream;
                m_nextSink.ProcessMessage(msg, requestHeaders, requestStream, 
                                          out responseHeaders, out responseStream);

                // now deserialise the response
                IncomingHandlingStatus status = DeserialiseResponse(responseStream, 
                                                                    responseHeaders, msg, out result);
                if (status != IncomingHandlingStatus.normal) { 
                    return null; 
                } // no reply-msg, abort further processing
            } catch (Exception e) {
                result = new ReturnMessage(e, (IMethodCallMessage) msg);
            }
            return result;
        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink) {
            // prepare connection
            IiopClientConnectionManager manager = IiopClientConnectionManager.GetManager();
            IiopClientConnection con = manager.CreateOrGetClientConnection(this, 
                                                                           (string)msg.Properties["__Uri"]);
            GiopRequestNumberGenerator reqNumGen = con.ReqNumberGen;
            
            try {
                ITransportHeaders requestHeaders;
                Stream requestStream;
                SerialiseRequest(msg, reqNumGen, out requestHeaders, out requestStream);
                // pass the serialised GIOP-request to the first stream handling sink
                // this sink is the last sink in the message handling sink chain, therefore the reply sink chain of all the previous message handling
                // sink is passed to the ClientChannelSinkStack, which will inform this chain of the received reply
                ClientChannelSinkStack clientSinkStack = new ClientChannelSinkStack(replySink);
                clientSinkStack.Push(this, msg); // push the formatter onto the sink stack, to get the chance to handle the incoming reply stream
                // forward the message to the next sink
                m_nextSink.AsyncProcessRequest(clientSinkStack, msg, requestHeaders, requestStream);
            } catch (Exception e) {
                IMessage retMsg = new ReturnMessage(e, (IMethodCallMessage) msg);
                if (replySink != null) {
                    replySink.SyncProcessMessage(msg); // process the return message in the reply sink chain
                }
            }
            return null;  // TODO, it would be possible to return a possiblity to cancel a message ...
        }
        
        #endregion

        #region Implementation of IClientChannelSink
        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, 
                                        IMessage msg, ITransportHeaders headers, 
                                        System.IO.Stream stream) {
            throw new NotSupportedException(); // not supported, because client side formatter is the first sink in the chain, using the serialized msg.
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
                                   out ITransportHeaders responseHeaders, out Stream responseStream) {
            throw new NotSupportedException(); // not supported, because client side formatter is the first sink in the chain, using the serialized msg.
        }

        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, 
                                         ITransportHeaders headers, Stream stream) {
            // client side formatter is the last sink in the chain accessing the serialised message, therefore this method is called on the return path
            IMessage requestMsg = (IMessage) state; // retrieve the request msg stored on the channelSinkStack
            try {
                IMessage responseMsg;
                IncomingHandlingStatus status = DeserialiseResponse(stream, headers, requestMsg, out responseMsg);
                if (status != IncomingHandlingStatus.normal) { 
                    return; 
                }
                sinkStack.DispatchReplyMessage(responseMsg); // dispatch the result message to the message handling reply sink chain
            } catch (Exception e) {
                sinkStack.DispatchException(e); // dispatch the exception to the message handling reply sink chain        
            }
        }

        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers) {
            throw new NotSupportedException(); // this operation is not useful on client side formatter sink, because previous message sinks, can't produce a serialise msg

        }

        #endregion
    
        #endregion IMethods

    }

    /// <summary>
    /// this class is a server side formater for IIOP-messages in the IIOP-Channel
    /// </summary>
    public class IiopServerFormatterSink : IServerChannelSink {
    
        #region IFields

        private IServerChannelSink m_nextSink;

        private IDictionary m_properties = new Hashtable();

        #endregion IFields
        #region IConstructors

        public IiopServerFormatterSink(IServerChannelSink nextSink) {
            m_nextSink = nextSink;
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

        /// <summary>deserialises an IIOP-msg from the request stream</summary>
        /// <returns>the .NET message created from the IIOP-msg</returns>
        private IncomingHandlingStatus DeserialiseRequest(Stream requestStream, ITransportHeaders headers,
                                                          out IMessage result) {
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            IncomingHandlingStatus status = handler.ParseIncomingServerMessage(requestStream, out result);
            requestStream.Close(); // not needed any more
            return status;
        }

        /// <summary>serialises the .NET msg to a GIOP-message</summary>
        private void SerialiseResponse(IServerResponseChannelSinkStack sinkStack, IMessage requestMsg,
                                       IMessage msg, out ITransportHeaders headers, out Stream stream) {
            uint requestId = Convert.ToUInt32(requestMsg.Properties[SimpleGiopMsg.REQUEST_ID_KEY]);
            GiopVersion version = (GiopVersion)requestMsg.Properties[SimpleGiopMsg.GIOP_VERSION_KEY];
            headers = new TransportHeaders();
            // get the stream into which the message should be serialied from a stream handling
            // sink in the stream handling chain
            stream = sinkStack.GetResponseStream(msg, headers);
            if (stream == null) { 
                // the previous stream-handling sinks delegated the decision to which stream the message should be serialised to this sink
                stream = new MemoryStream(); // create a new stream
            }
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            handler.SerialiseOutgoingServerMessage(msg, version, requestId, stream);
        }

        /// <summary>serialises an Exception</summary>
        private void SerialiseExceptionResponse(Exception e, IMessage requestMsg, out IMessage responseMsg,
                                                out ITransportHeaders headers, out Stream stream) {
            uint requestId = Convert.ToUInt32(requestMsg.Properties[SimpleGiopMsg.REQUEST_ID_KEY]);
            GiopVersion version = (GiopVersion)requestMsg.Properties[SimpleGiopMsg.GIOP_VERSION_KEY];
            // serialise an exception response
            if (requestMsg is IMethodCallMessage) {
                responseMsg = new ReturnMessage(e, (IMethodCallMessage) requestMsg);
            } else {
                responseMsg = new ReturnMessage(e, null); // no useful information present for requestMsg
            }
            headers = new TransportHeaders();
            stream = new MemoryStream();
            // serialise a server result
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            handler.SerialiseOutgoingServerMessage(responseMsg, version, requestId, stream);
        }
    
        #region Implementation of IServerChannelSink
        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state,
                                        IMessage msg, ITransportHeaders headers) {
            throw new NotSupportedException(); // this is not supported on this sink, because later sinks in the chain can't serialise a response, therefore a response stream is not available for them
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg,
                                               ITransportHeaders requestHeaders, Stream requestStream, 
                                               out IMessage responseMsg, out ITransportHeaders responseHeaders,
                                               out Stream responseStream) {
            IMessage deserReqMsg = null;
            try {
                // processing may be done asynchronous, therefore push this sink on the stack to process a response async
                sinkStack.Push(this, deserReqMsg);
                // deserialise the request
                IncomingHandlingStatus status = DeserialiseRequest(requestStream, requestHeaders,
                                                                   out deserReqMsg);
                if (status != IncomingHandlingStatus.normal) { // check for non-request-msgs
                    sinkStack.Pop(this); 
                    responseHeaders = null; 
                    responseMsg = null;
                    responseStream = null;
                    return ServerProcessing.OneWay;
                } // no response is sent
                
                // forward the call to the next message handling sink
                ServerProcessing processingResult = m_nextSink.ProcessMessage(sinkStack, deserReqMsg,
                                                                              requestHeaders, null, out responseMsg,
                                                                              out responseHeaders, out responseStream);
                switch (processingResult) {
                    case ServerProcessing.Complete:
                        sinkStack.Pop(this); // not async
                        // send the response
                        SerialiseResponse(sinkStack, deserReqMsg, responseMsg, 
                                          out responseHeaders, out responseStream);                        
                        break;
                    case ServerProcessing.Async:
                        sinkStack.Store(this, deserReqMsg); // this sink want's to process async response
                        break;
                    case ServerProcessing.OneWay:
                        // nothing to do, because no response expected
                        sinkStack.Pop(this);
                        break;
                }
                return processingResult;

            } catch (IOException ioEx) {
                throw ioEx;
            } catch (MessageHandling.RequestDeserializationException deserEx) {
                try { 
                    sinkStack.Pop(this); // prevent an async response handling
                } catch (Exception) {}
                // an exception was thrown during deserialization
                SerialiseExceptionResponse(deserEx.Reason, deserEx.RequestMessage, out responseMsg,
                                           out responseHeaders, out responseStream);
                return ServerProcessing.Complete;
            } catch (Exception e) {
                try { 
                    sinkStack.Pop(this); // prevent an async response handling
                } catch (Exception) {}
                // serialise an exception response
                if (deserReqMsg != null) {
                    SerialiseExceptionResponse(e, deserReqMsg, out responseMsg, out responseHeaders,
                                               out responseStream);
                } else {
                    throw e;
                }
                return ServerProcessing.Complete; // send back an error msg
            }
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state,
                                         IMessage msg, ITransportHeaders headers, Stream stream) {
            try {
                IMessage requestMsg = (IMessage)state;
                SerialiseResponse(sinkStack, requestMsg, msg, out headers, out stream);
            } 
            catch (Exception e) {
                // serialise the exception
                SerialiseExceptionResponse(e, (IMessage)state, out msg, out headers, out stream); 
            }
            sinkStack.AsyncProcessResponse(msg, headers, stream); // pass further on to the stream handling sinks
        }
        
        #endregion
        
        #endregion IMethods
        
    }

    /// <summary>
    /// this class is a provider for the IIOPClientFormatterSink.
    /// </summary>
    public class IiopClientFormatterSinkProver : IClientFormatterSinkProvider {
    
        #region IFields

        /// <summary>the next provider in the provider chain</summary>
        private IClientChannelSinkProvider m_nextProvider; // next provider is set during channel creating with the set method on the property      

        #endregion IFields
        #region IConstructors
        
        public IiopClientFormatterSinkProver() {
        }

        #endregion IConstructors
        #region IProperties

        public IClientChannelSinkProvider Next {
            get { 
                return m_nextProvider; 
            }
            set { 
                m_nextProvider = value; 
            }
        }

        #endregion IProperties
        #region IMethods

        #region Implementation of IClientChannelSinkProvider
        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData) {
            if ((!(channel is IiopChannel)) && (!(channel is IiopClientChannel))) {
                throw new ArgumentException("this provider is only usable with the IIOPChannel, but not with : " +
                                            channel);
            }
            Debug.WriteLine("create an IIOP client formatter sink");
            // create the client formatter, therefore create first the rest of the chain, than the formatter itself
            IClientChannelSink nextSink = null;
            if (m_nextProvider != null) {
                nextSink = m_nextProvider.CreateSink(channel, url, remoteChannelData);
            }

            return new IiopClientFormatterSink(nextSink);
        }

        #endregion

        #endregion IMethods

    }

    /// <summary>
    /// this class is a provider for the IIOPServerFormatterSink.
    /// </summary>
    public class IiopServerFormatterSinkProvider : IServerFormatterSinkProvider {
    
        #region IFields
        
        private IServerChannelSinkProvider m_nextProvider; // is set during channel creation with the set accessor of the property

        #endregion IFields
        #region IConstructors

        public IiopServerFormatterSinkProvider() {
        }

        #endregion IConstructors
        #region IProperties

        public System.Runtime.Remoting.Channels.IServerChannelSinkProvider Next {
            get {
                return m_nextProvider; 
            }
            set {
                m_nextProvider = value; 
            }
        }

        #endregion IProperties
        #region IMethods
        
        #region Implementation of IServerChannelSinkProvider

        public IServerChannelSink CreateSink(IChannelReceiver channel) {
            if ((!(channel is IiopChannel)) && (!(channel is IiopServerChannel))) {
                throw new ArgumentException("this provider is only usable with the IIOPChannel, but not with : " +
                                            channel);
            }
            Debug.WriteLine("create an IIOP server formatter sink");
            
            IServerChannelSink next = null;
            if (m_nextProvider != null) {
                next = m_nextProvider.CreateSink(channel); // create the rest of the sink chain
            }
            return new IiopServerFormatterSink(next); // create the formatter
        }
        
        public void GetChannelData(IChannelDataStore channelData) {
            // not useful for this provider
        }

        #endregion

        #endregion IMethods

    }




}
