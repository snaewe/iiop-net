/* Formatter.cs
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
using System.IO;
using System.Diagnostics;
using Ch.Elca.Iiop.MessageHandling;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Interception;
using omg.org.CORBA;

namespace Ch.Elca.Iiop {


    /// <summary>used to store relevant data for async response processing</summary>
    internal class AsyncProcessingData {
            
        private GiopConnectionDesc m_conDesc;
        private IMessage m_reqMsg;
            
        internal AsyncProcessingData(IMessage reqMsg,
                                     GiopConnectionDesc conDesc) {
            m_reqMsg = reqMsg;                
            m_conDesc = conDesc;
        }
            
        internal IMessage RequestMsg {
            get {
                return m_reqMsg;
            }
        }
            
        internal GiopConnectionDesc ConDesc {
            get {
                return m_conDesc;
            }
        }
                        
    }
    
        
    /// <summary>
    /// this class is a client side formatter for IIOP-messages in the IIOP-channel
    /// </summary>
    internal class IiopClientFormatterSink : IClientFormatterSink {
    
        #region IFields

        private IDictionary m_properties = new Hashtable();
        
        private IClientChannelSink m_nextSink;
        
        private GiopClientConnectionManager m_conManager;
        
        private IInterceptionOption[] m_interceptionOptions;

        #endregion IFields
        #region IConstructors

        /// <param name="nextSink">the next sink in the channel. In this sink chain, a
        /// IiopClientTransportSink must be present.</param>
        internal IiopClientFormatterSink(IClientChannelSink nextSink, GiopClientConnectionManager conManager,
                                         IInterceptionOption[] interceptionOptions) {
            m_nextSink = nextSink;            
            m_conManager = conManager;
            m_interceptionOptions = interceptionOptions;            
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
        private void SerialiseRequest(IMessage msg, IIorProfile target, GiopClientConnectionDesc conDesc,
                                      uint reqId,
                                      out ITransportHeaders headers, out Stream stream) {
            headers = new TransportHeaders();
            headers[GiopConnectionDesc.CLIENT_TR_HEADER_KEY] = conDesc;
            // get the stream into which the message should be serialied from the first stream handling
            // sink in the stream handling chain
            stream = m_nextSink.GetRequestStream(msg, headers);
            if (stream == null) { // the next sink delegated the decision to which stream the message should be serialised to this sink
                stream = new MemoryStream(); // create a new stream
            }
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            handler.SerialiseOutgoingRequestMessage(msg, target, conDesc, stream, reqId, m_interceptionOptions);
        }

        /// <summary>deserialises an IIOP-msg from the response stream</summary>
        /// <returns> the .NET message created from the IIOP-msg</returns>
        internal IMessage DeserialiseResponse(Stream responseStream, 
                                              ITransportHeaders headers,
                                              IMessage requestMsg,
                                              GiopClientConnectionDesc conDesc) {
            
            IMessage result;
            try {
                GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
                result = handler.ParseIncomingReplyMessage(responseStream, 
                                                           (IMethodCallMessage) requestMsg,
                                                           conDesc, m_interceptionOptions);
            } finally {
                responseStream.Close(); // stream not needed any more
                m_conManager.ReleaseConnectionFor(requestMsg); // release the connection, because this interaction is complete            
            }            
            return result;
        }
        
        /// <summary>allocates a connection and adds 
        /// the connectionDesc to the message</summary>
        private GiopClientConnectionDesc AllocateConnection(IMessage msg, Ior target, out IIorProfile selectedProfile,
                                                            out uint reqId) {
            for (int i = 0; i < target.Profiles.Length; i++) {
                if (m_conManager.CanConnectWithProfile(target.Profiles[i])) {
                    selectedProfile = target.Profiles[i];
                    try {
                        return m_conManager.AllocateConnectionFor(msg, selectedProfile, out reqId);
                    } catch (Exception ex) {
                        Trace.WriteLine("exception while trying to connect to target: " + ex);
                        continue; // try next profile
                    }
                }
            }
            throw new COMM_FAILURE(4000, CompletionStatus.Completed_No); // can't connect to ior.            
        }
        
        private Ior DetermineTarget(IMessage msg) {
            IMethodMessage methodMsg = msg as IMethodMessage;
            if ((methodMsg == null) || (methodMsg.Uri == null)){
                throw new INTERNAL(319, CompletionStatus.Completed_No);
            }
            // for urls, which are not stringified iors, no very accurate type information,
            // because pass as repository id information base type of all corba interfaces: Object;
            // for urls, which are stringified iors, the type information is extracted from the ior directly
            Ior target = IiopUrlUtil.CreateIorForUrl(methodMsg.Uri, "");
            if (target == null) {
                throw new INTERNAL(319, CompletionStatus.Completed_No);
            }
            return target;
        }
            
        #region Implementation of IMessageSink
        public IMessage SyncProcessMessage(IMessage msg) {
            // allocate (reserve) connection
            Ior target = DetermineTarget(msg);
            IIorProfile selectedProfile;
            uint reqId;
            GiopClientConnectionDesc conDesc = AllocateConnection(msg, target, out selectedProfile, out reqId);
            
            // serialise
            IMessage result;
            try {
                ITransportHeaders requestHeaders;
                Stream requestStream;
                SerialiseRequest(msg, selectedProfile, conDesc, reqId,
                                 out requestHeaders, out requestStream);

                // pass the serialised GIOP-request to the first stream handling sink
                // when the call returns, the response message has been received
                ITransportHeaders responseHeaders;                                
                Stream responseStream;
                m_nextSink.ProcessMessage(msg, requestHeaders, requestStream, 
                                          out responseHeaders, out responseStream);

                // now deserialise the response
                result = DeserialiseResponse(responseStream, 
                                             responseHeaders, msg, conDesc);
            } catch (Exception e) {
                result = new ReturnMessage(e, (IMethodCallMessage) msg);
            }
            return result;
        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink) {
            // allocate (reserve) connection
            Ior target = DetermineTarget(msg);
            IIorProfile selectedProfile;
            uint reqId;
            GiopClientConnectionDesc conDesc = AllocateConnection(msg, target, out selectedProfile, out reqId);
            
            try {
                SimpleGiopMsg.SetMessageAsyncRequest(msg); // mark message as async, needed for portable interceptors
                ITransportHeaders requestHeaders;
                Stream requestStream;
                SerialiseRequest(msg, selectedProfile, conDesc, reqId,
                                 out requestHeaders, out requestStream);
                // pass the serialised GIOP-request to the first stream handling sink
                // this sink is the last sink in the message handling sink chain, therefore the reply sink chain of all the previous message handling
                // sink is passed to the ClientChannelSinkStack, which will inform this chain of the received reply
                ClientChannelSinkStack clientSinkStack = new ClientChannelSinkStack(replySink);
                AsyncProcessingData asyncData = new AsyncProcessingData(msg, conDesc);
                clientSinkStack.Push(this, asyncData); // push the formatter onto the sink stack, to get the chance to handle the incoming reply stream
                // forward the message to the next sink
                m_nextSink.AsyncProcessRequest(clientSinkStack, msg, requestHeaders, requestStream);

                // for oneway messages, release the connections for future use
                if ((msg is IMethodCallMessage) && GiopMessageHandler.IsOneWayCall((IMethodCallMessage)msg)) {
                    m_conManager.ReleaseConnectionFor(msg); // release the connection, because this interaction is complete
                }
            } catch (Exception e) {
                // formulate an exception reply for an non-oneway call
                if ( ((msg is IMethodCallMessage) && (!GiopMessageHandler.IsOneWayCall((IMethodCallMessage)msg))) ||
                     (!(msg is IMethodCallMessage))) {
                
                    IMessage retMsg = new ReturnMessage(e, (IMethodCallMessage) msg);
                    if (replySink != null) {
                        replySink.SyncProcessMessage(retMsg); // process the return message in the reply sink chain
                    }
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
            AsyncProcessingData asyncData = (AsyncProcessingData) state; // retrieve the request msg stored on the channelSinkStack
            IMessage requestMsg = asyncData.RequestMsg; 
            GiopClientConnectionDesc conDesc = (GiopClientConnectionDesc)asyncData.ConDesc;
            try {
                IMessage responseMsg = DeserialiseResponse(stream, headers,
                                                           requestMsg, conDesc);
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
        
        private IInterceptionOption[] m_interceptionOptions;

        #endregion IFields
        #region IConstructors

        internal IiopServerFormatterSink(IServerChannelSink nextSink, IInterceptionOption[] interceptionOptions) {
            m_nextSink = nextSink;
            m_interceptionOptions = interceptionOptions;
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

        /// <summary>deserialises an Giop-msg from the request stream</summary>
        /// <returns>the .NET message created from the Giop-msg</returns>
        private IMessage DeserialiseRequest(Stream requestStream, 
                                            ITransportHeaders headers,
                                            GiopConnectionDesc conDesc) {
            try {
                GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
                IMessage result = handler.ParseIncomingRequestMessage(requestStream, 
                                                                      conDesc, m_interceptionOptions);
                return result;
            } finally {
                requestStream.Close(); // not needed any more
            }            
        }

        /// <summary>serialises the .NET msg to a GIOP-message</summary>
        private void SerialiseResponse(IServerResponseChannelSinkStack sinkStack, IMessage requestMsg,
                                       GiopConnectionDesc conDesc, IMessage responseMsg, 
                                       ref ITransportHeaders headers, out Stream stream) {            
            GiopVersion version = (GiopVersion)requestMsg.Properties[SimpleGiopMsg.GIOP_VERSION_KEY];
            if (headers == null) {
                headers = new TransportHeaders();
            }
            headers[GiopConnectionDesc.SERVER_TR_HEADER_KEY] = conDesc;
            // get the stream into which the message should be serialied from a stream handling
            // sink in the stream handling chain
            stream = sinkStack.GetResponseStream(responseMsg, headers);
            if (stream == null) { 
                // the previous stream-handling sinks delegated the decision to which stream the message should be serialised to this sink
                stream = new MemoryStream(); // create a new stream
            }
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            handler.SerialiseOutgoingReplyMessage(responseMsg, requestMsg, version, stream, conDesc,
                                                  m_interceptionOptions);
        }

        /// <summary>serialises an Exception</summary>
        private void SerialiseExceptionResponse(IServerResponseChannelSinkStack sinkStack,
                                                IMessage requestMsg,
                                                GiopConnectionDesc conDesc,
                                                IMessage responseMsg,                                                                                                
                                                ref ITransportHeaders headers, out Stream stream) {
            // serialise an exception response
            headers = new TransportHeaders();
            SerialiseResponse(sinkStack, requestMsg, conDesc, responseMsg, ref headers, out stream);
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
            responseMsg = null;
            responseHeaders = null;
            responseStream = null;
            GiopConnectionDesc conDesc = (GiopConnectionDesc)
                requestHeaders[GiopConnectionDesc.SERVER_TR_HEADER_KEY];
                                                   
            try {
                // deserialise the request
                deserReqMsg = DeserialiseRequest(requestStream, requestHeaders, conDesc);
                // processing may be done asynchronous, therefore push this sink on the stack to process a response async
                AsyncProcessingData asyncData = 
                    new AsyncProcessingData(deserReqMsg, conDesc);
                sinkStack.Push(this, asyncData);
                
                ServerProcessing processingResult;
                try {
                    // forward the call to the next message handling sink
                    processingResult = m_nextSink.ProcessMessage(sinkStack, deserReqMsg,
                                                                 requestHeaders, null, out responseMsg,
                                                                 out responseHeaders, out responseStream);
                } catch (Exception) {
                    sinkStack.Pop(this);
                    throw;
                }
                switch (processingResult) {
                    case ServerProcessing.Complete:
                        sinkStack.Pop(this); // not async
                        // send the response
                        SerialiseResponse(sinkStack, deserReqMsg, conDesc, responseMsg,
                                          ref responseHeaders, out responseStream);                        
                        break;
                    case ServerProcessing.Async:
                        sinkStack.Store(this, asyncData); // this sink want's to process async response
                        break;
                    case ServerProcessing.OneWay:
                        // nothing to do, because no response expected
                        sinkStack.Pop(this);
                        break;
                }
                return processingResult;
            
            } catch (MessageHandling.RequestDeserializationException deserEx) {
                // exception from DeserialisRequest
                responseMsg = deserEx.ResponseMessage;
                // an exception was thrown during deserialization
                SerialiseExceptionResponse(sinkStack, 
                                           deserEx.RequestMessage, conDesc, responseMsg,
                                           ref responseHeaders, out responseStream);
                return ServerProcessing.Complete;
            } catch (Exception e) {
                // serialise an exception response
                if (deserReqMsg != null) {
                    if (deserReqMsg is IMethodCallMessage) {
                        responseMsg = new ReturnMessage(e, (IMethodCallMessage) deserReqMsg);
                    } else {
                        responseMsg = new ReturnMessage(e, null); // no usable information present
                    }
                    SerialiseExceptionResponse(sinkStack, 
                                               deserReqMsg, conDesc, responseMsg,
                                               ref responseHeaders, out responseStream);
                } else {
                    throw e;
                }
                return ServerProcessing.Complete; // send back an error msg
            }
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state,
                                         IMessage msg, ITransportHeaders headers, Stream stream) {
            // headers, stream are null, because formatting is first sink to create these two
            AsyncProcessingData asyncData = (AsyncProcessingData) state;
            try {                
                IMessage requestMsg = asyncData.RequestMsg;
                SerialiseResponse(sinkStack, requestMsg, asyncData.ConDesc, msg, 
                                  ref headers, out stream);
            } 
            catch (Exception e) {
                if (asyncData.RequestMsg is IMethodCallMessage) {
                    msg = new ReturnMessage(e, (IMethodCallMessage) asyncData.RequestMsg);
                } else {
                    msg = new ReturnMessage(e, null); // no useful information present for requestMsg
                }
                // serialise the exception
                SerialiseExceptionResponse(sinkStack, (IMessage)state, asyncData.ConDesc, msg,
                                           ref headers, out stream);
            }
            sinkStack.AsyncProcessResponse(msg, headers, stream); // pass further on to the stream handling sinks
        }
        
        #endregion
        
        #endregion IMethods
        
    }

    /// <summary>
    /// this class is a provider for the IIOPClientFormatterSink.
    /// </summary>
    public class IiopClientFormatterSinkProvider : IClientFormatterSinkProvider {
    
        #region IFields

        /// <summary>the next provider in the provider chain</summary>
        private IClientChannelSinkProvider m_nextProvider; // next provider is set during channel creating with the set method on the property      

        /// <summary>
        /// the client side connection Manager
        /// </summary>
        private GiopClientConnectionManager m_conManager;
        
        private IInterceptionOption[] m_interceptionOptions = InterceptorManager.EmptyInterceptorOptions;
        
        #endregion IFields
        #region IConstructors
        
        public IiopClientFormatterSinkProvider() {
        }

        public IiopClientFormatterSinkProvider(IDictionary properties, ICollection providerData) {
            if ((providerData != null) && (providerData.Count > 0)) {
                throw new ArgumentException(String.Format("Provider {0} is not expection providerData", GetType().Name));
            }
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
            
            return new IiopClientFormatterSink(nextSink, m_conManager, m_interceptionOptions);
        }

        #endregion

        internal void Configure(GiopClientConnectionManager conManager, IInterceptionOption[] interceptionOptions) {
            m_conManager = conManager;
            m_interceptionOptions = interceptionOptions;
        }        
        
        #endregion IMethods

    }

    /// <summary>
    /// this class is a provider for the IIOPServerFormatterSink.
    /// </summary>
    public class IiopServerFormatterSinkProvider : IServerFormatterSinkProvider {
    
        #region IFields
        
        private IServerChannelSinkProvider m_nextProvider; // is set during channel creation with the set accessor of the property
        
        private IInterceptionOption[] m_interceptionOptions = InterceptorManager.EmptyInterceptorOptions;

        #endregion IFields
        #region IConstructors

        public IiopServerFormatterSinkProvider() {
        }

        public IiopServerFormatterSinkProvider(IDictionary properties, ICollection providerData) {
            if ((providerData != null) && (providerData.Count > 0)) {
                throw new ArgumentException(String.Format("Provider {0} is not expection providerData", GetType().Name));
            }
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
            return new IiopServerFormatterSink(next, m_interceptionOptions); // create the formatter
        }
        
        public void GetChannelData(IChannelDataStore channelData) {
            // not useful for this provider
        }

        #endregion
        
        internal void Configure(IInterceptionOption[] interceptionOptions) {
            m_interceptionOptions = interceptionOptions;
        }

        #endregion IMethods

    }




}
