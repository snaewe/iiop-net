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
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Cdr;
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
    /// async data for the server side.
    /// </summary>
    internal class AsyncServerProcessingData : AsyncProcessingData {

        private GiopServerConnection m_serverCon;

        internal AsyncServerProcessingData(IMessage reqMsg, GiopServerConnection serverCon) :
            base(reqMsg, serverCon.ConDesc) {
            m_serverCon = serverCon;
        }

        internal GiopServerConnection ServerCon {
            get {
                return m_serverCon;
            }
        }

    }

    /// <summary>
    /// this class is a client side formatter for IIOP-messages in the IIOP-channel
    /// </summary>
    internal class IiopClientFormatterSink : IClientFormatterSink {

        #region IFields

        private readonly IDictionary m_properties = new Hashtable();

        private readonly IClientChannelSink m_nextSink;

        private readonly GiopClientConnectionManager m_conManager;

        private readonly GiopMessageHandler m_messageHandler;

        private readonly IiopUrlUtil m_iiopUrlUtil;

        private readonly RetryConfig m_retries;

        private readonly Hashtable m_typesVerified = new Hashtable(); // contains the verified types for this proxy

        #endregion IFields
        #region IConstructors

        /// <param name="nextSink">the next sink in the channel. In this sink chain, a
        /// IiopClientTransportSink must be present.</param>
        internal IiopClientFormatterSink(IClientChannelSink nextSink, GiopClientConnectionManager conManager,
                                         GiopMessageHandler messageHandler,
                                         IiopUrlUtil iiopUrlUtil,
                                         RetryConfig retries) {
            m_nextSink = nextSink;
            m_conManager = conManager;
            m_messageHandler = messageHandler;
            m_iiopUrlUtil = iiopUrlUtil;
            m_retries = retries;
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
            headers[GiopClientConnectionDesc.CLIENT_TR_HEADER_KEY] = conDesc;
            // get the stream into which the message should be serialied from the first stream handling
            // sink in the stream handling chain
            stream = m_nextSink.GetRequestStream(msg, headers);
            if (stream == null) { // the next sink delegated the decision to which stream the message should be serialised to this sink
                stream = new MemoryStream(); // create a new stream
            }
            m_messageHandler.SerialiseOutgoingRequestMessage(msg, target, conDesc, stream, reqId);
        }

        /// <summary>deserialises an IIOP-msg from the response stream</summary>
        /// <returns> the .NET message created from the IIOP-msg</returns>
        internal IMessage DeserialiseResponse(Stream responseStream,
                                              ITransportHeaders headers,
                                              IMessage requestMsg,
                                              GiopClientConnectionDesc conDesc) {
            // stream won't be needed any more
            using (responseStream) {
                IMessage result = m_messageHandler.ParseIncomingReplyMessage(responseStream,
                                              (IMethodCallMessage) requestMsg,
                                              conDesc);
                MarshalByRefObject fwdToTarget;
                if (GiopMessageHandler.IsLocationForward(result, out fwdToTarget)) {
                    // location-fwd
                    // reissue request to new target
                    result = m_messageHandler.ForwardRequest((IMethodCallMessage) requestMsg,
                                                             fwdToTarget);
                }
                return result;
            }
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
            throw new TRANSIENT(CorbaSystemExceptionCodes.TRANSIENT_CANTCONNECT,
                                CompletionStatus.Completed_No, "Unable to connect to target."); // can't connect to ior at the moment.
        }

        private Ior DetermineTarget(IMessage msg) {
            IMethodMessage methodMsg = msg as IMethodMessage;
            if ((methodMsg == null) || (methodMsg.Uri == null)){
                throw new INTERNAL(319, CompletionStatus.Completed_No);
            }
            // for urls, which are not stringified iors, no very accurate type information,
            // because pass as repository id information base type of all corba interfaces: Object;
            // for urls, which are stringified iors, the type information is extracted from the ior directly
            Ior target = m_iiopUrlUtil.CreateIorForUrl(methodMsg.Uri, "");
            if (target == null) {
                throw new INTERNAL(319, CompletionStatus.Completed_No);
            }
            return target;
        }

        /// <summary>if compatibility is not checkable with type information included in
        /// IOR, call _is_a method to check.</summary>
        private bool CheckAssignableRemote(Type formal, string url) {
            IObject proxy = (IObject)RemotingServices.Connect(ReflectionHelper.IObjectType, url);
            return proxy._is_a(Repository.GetRepositoryID(formal));
        }

        private bool IsInterfaceCompatible(Ior target, Type neededTargetType, string targetUrl) {
            Type interfaceType;
            // local check first otherwise remote
            return Repository.IsInterfaceCompatible(neededTargetType, target.TypID, out interfaceType) ||
                   CheckAssignableRemote(neededTargetType, targetUrl);
        }

        private void VerifyInterfaceCompatible(Ior target, IMessage msg) {
            if (msg is IMethodMessage) {
                IMethodMessage methodCall = (IMethodMessage)msg;
                Type targetType = methodCall.MethodBase.DeclaringType;
                lock(m_typesVerified.SyncRoot) {
                    if (m_typesVerified.ContainsKey(targetType)) {
                        return;
                    } else {
                        if (IsInterfaceCompatible(target, targetType, methodCall.Uri)) {
                            // this sink chain is assigned to a remote proxy for the
                            // methodCall.Uri; for a distinct target url, a different
                            // formatter instance is used -> therefore, don't need to
                            // distinguish for different uris.
                            m_typesVerified[targetType] = true;
                        } else {
                            throw new BAD_PARAM(20010, CompletionStatus.Completed_No,
                                                "The target object with the uri: " + methodCall.Uri +
                                                " doesn't support the interface: " +
                                                targetType.AssemblyQualifiedName);
                        }
                    }
                }
            } else {
                // can't verify for this message
                throw new INTERNAL(319, CompletionStatus.Completed_No);
            }
        }

        #region Implementation of IMessageSink

        private bool CanRetryOnException(Exception ex, int numberOfRetriesDone) {
            TRANSIENT tEx = ex as TRANSIENT;
            if (tEx == null || tEx.Status != CompletionStatus.Completed_No) {
                return false;
            }
            if (numberOfRetriesDone >= m_retries.MaxNumberOfRetries) {
                // check, that number of retries not yet exceeded, i.e
                // is a retry allowed or not;
                // >=, because of case no retries.
                return false;
            }
            return true;
        }

        private IMessage SyncProcessMessageOnce(IMessage msg,
                                                Ior target) {

            IIorProfile selectedProfile;
            uint reqId;
            try {
                // allocate (reserve) connection
                GiopClientConnectionDesc conDesc =
                    AllocateConnection(msg, target, out selectedProfile, out reqId);
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
                return DeserialiseResponse(responseStream,
                                           responseHeaders, msg, conDesc);
            } finally {
                m_conManager.RequestOnConnectionCompleted(msg); // release the connection, because this interaction is complete
            }
        }

        public IMessage SyncProcessMessage(IMessage msg) {
            Ior target = DetermineTarget(msg);
            VerifyInterfaceCompatible(target, msg);
            // serialise
            IMessage result = null;
            int numberOfRetries = 0;
            while (true) {
                try {
                    result = SyncProcessMessageOnce(msg, target);
                    break;
                } catch (Exception e) {
                    if (!CanRetryOnException(e, numberOfRetries)) {
                        result = new ReturnMessage(e, (IMethodCallMessage) msg);
                        break;
                    }
                    numberOfRetries++;
                    m_retries.DelayNextRetryIfNeeded();
                }
            }
            return result;
        }

        private void AsyncProcessMessageOnce(IMessage msg,
                                                 Ior target, IMessageSink replySink) {
            IIorProfile selectedProfile;
            uint reqId;
            try {
                // allocate (reserve) connection
                GiopClientConnectionDesc conDesc = AllocateConnection(msg, target, out selectedProfile, out reqId);
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
                     m_conManager.RequestOnConnectionCompleted(msg); // release the connection, because this interaction is complete
                }
            } catch {
                // release the connection, if something went wrong during connection allocation and send
                m_conManager.RequestOnConnectionCompleted(msg); // release the connection, because this interaction is complete
                throw;
            }
        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink) {
            Ior target = DetermineTarget(msg);
            VerifyInterfaceCompatible(target, msg);
            int numberOfRetries = 0;
            while (true) {
                try {
                    AsyncProcessMessageOnce(msg, target, replySink);
                    break;
                } catch (Exception e) {
                    if (!CanRetryOnException(e, numberOfRetries)) {
                        // formulate an exception reply for an non-oneway call
                        IMethodCallMessage methodCallMsg = msg as IMethodCallMessage;
                        if (replySink != null &&
                            methodCallMsg != null && !GiopMessageHandler.IsOneWayCall(methodCallMsg)) {

                            IMessage retMsg = new ReturnMessage(e, methodCallMsg);
                            replySink.SyncProcessMessage(retMsg); // process the return message in the reply sink chain
                        }
                        break;
                    }
                    m_retries.DelayNextRetryIfNeeded();
                    numberOfRetries++;
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
            try {
                IMessage responseMsg;
                try {
                    GiopClientConnectionDesc conDesc = (GiopClientConnectionDesc)asyncData.ConDesc;
                    responseMsg = DeserialiseResponse(stream, headers,
                                                      requestMsg, conDesc);
                } finally {
                    m_conManager.RequestOnConnectionCompleted(requestMsg); // release the connection, because this interaction is complete
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

        #region Types

        /// <summary>
        /// exception thrown by NotifyDeserialiseRequestComplete handler, if a problem occurs and processing
        /// should be stopped.
        /// </summary>
        internal class NotifyReadRequestException : Exception {

            public NotifyReadRequestException() : base() {
            }

            public NotifyReadRequestException(string msg) : base(msg) {
            }

            public NotifyReadRequestException(string msg, Exception inner) : base(msg, inner) {
            }

            protected NotifyReadRequestException(System.Runtime.Serialization.SerializationInfo info,
                                                 System.Runtime.Serialization.StreamingContext context) : base(info, context) {
            }

        }

        #endregion Types
        #region IFields

        private IServerChannelSink m_nextSink;

        private IDictionary m_properties = new Hashtable();

        private GiopMessageHandler m_messageHandler;

        #endregion IFields
        #region IConstructors

        internal IiopServerFormatterSink(IServerChannelSink nextSink,
                                         GiopMessageHandler messageHandler) {
            m_nextSink = nextSink;
            m_messageHandler = messageHandler;
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

        private void PrepareResponseHeaders(ref ITransportHeaders headers,
                                            GiopServerConnection con) {
            if (headers == null) {
                headers = new TransportHeaders();
            }
            headers[GiopServerConnection.SERVER_TR_HEADER_KEY] = con;
        }

        private Stream GetResponseStreamFor(IServerResponseChannelSinkStack sinkStack,
                                            IMessage responseMsg, ITransportHeaders headers) {
            Stream stream = sinkStack.GetResponseStream(responseMsg, headers);
            if (stream == null) {
                // the previous stream-handling sinks delegated the decision to which stream the message should be serialised to this sink
                stream = new MemoryStream(); // create a new stream
            }
            return stream;
        }

        /// <summary>serialises the .NET msg to a GIOP reply message</summary>
        private void SerialiseResponse(IServerResponseChannelSinkStack sinkStack, IMessage requestMsg,
                                       GiopServerConnection con, IMessage responseMsg,
                                       ref ITransportHeaders headers, out Stream stream) {
            GiopVersion version = (GiopVersion)requestMsg.Properties[SimpleGiopMsg.GIOP_VERSION_KEY];
            PrepareResponseHeaders(ref headers, con);
            // get the stream into which the message should be serialied from a stream handling
            // sink in the stream handling chain
            stream = GetResponseStreamFor(sinkStack, responseMsg, headers);
            m_messageHandler.SerialiseOutgoingReplyMessage(responseMsg, requestMsg, version, stream, con.ConDesc);
        }

        /// <summary>serialises an Exception as GIOP reply message</summary>
        private void SerialiseExceptionResponse(IServerResponseChannelSinkStack sinkStack,
                                                IMessage requestMsg,
                                                GiopServerConnection con,
                                                IMessage responseMsg,
                                                ref ITransportHeaders headers, out Stream stream) {
            // serialise an exception response
            headers = new TransportHeaders();
            SerialiseResponse(sinkStack, requestMsg, con, responseMsg, ref headers, out stream);
        }

        #region Implementation of IServerChannelSink
        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state,
                                        IMessage msg, ITransportHeaders headers) {
            throw new NotSupportedException(); // this is not supported on this sink, because later sinks in the chain can't serialise a response, therefore a response stream is not available for them
        }

        /// <summary>
        /// process a giop request message.
        /// </summary>
        private ServerProcessing ProcessRequestMessage(IServerChannelSinkStack sinkStack,
                                                       ITransportHeaders requestHeaders,
                                                       CdrMessageInputStream msgInput,
                                                       GiopServerConnection serverCon,
                                                       out IMessage responseMsg, out ITransportHeaders responseHeaders,
                                                       out Stream responseStream) {
            IMessage deserReqMsg = null;
            responseHeaders = null;
            try {
                try {
                    // deserialise the request
                    deserReqMsg = m_messageHandler.ParseIncomingRequestMessage(msgInput,
                                                       serverCon.ConDesc);
                } finally {
                    //request deserialised -> safe to read next request while processing request in servant
                    // (or sending request deserialisation exception)
                    try {
                        serverCon.NotifyDeserialiseRequestComplete();
                    } catch (Exception ne) {
                        // unexpected exception. Abort message processing, problem with transport.
                        throw new NotifyReadRequestException("Problem while trying to inform transport about request deserialistion.",
                                                             ne);
                    }
                }

                // processing may be done asynchronous, therefore push this sink on the stack to process a response async
                AsyncServerProcessingData asyncData =
                    new AsyncServerProcessingData(deserReqMsg, serverCon);
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
                        SerialiseResponse(sinkStack, deserReqMsg, serverCon, responseMsg,
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
                                           deserEx.RequestMessage, serverCon, responseMsg,
                                           ref responseHeaders, out responseStream);
                return ServerProcessing.Complete;
            } catch (NotifyReadRequestException nrre) {
                Trace.WriteLine("Failed to inform transport about request deserialisation. Processing problem on server connection after unexpected exception: " + nrre.InnerException);
                throw nrre;
            } catch (Exception e) {
                // serialise an exception response
                if (deserReqMsg != null) {
                    if (deserReqMsg is IMethodCallMessage) {
                        responseMsg = new ReturnMessage(e, (IMethodCallMessage) deserReqMsg);
                    } else {
                        responseMsg = new ReturnMessage(e, null); // no usable information present
                    }
                    SerialiseExceptionResponse(sinkStack,
                                               deserReqMsg, serverCon, responseMsg,
                                               ref responseHeaders, out responseStream);
                } else {
                    throw e;
                }
                return ServerProcessing.Complete; // send back an error msg
            }
        }

        /// <summary>
        /// process a giop locate request message.
        /// </summary>
        private ServerProcessing ProcessLocateRequestMessage(IServerChannelSinkStack sinkStack,
                                                             ITransportHeaders requestHeaders,
                                                             CdrMessageInputStream msgInput,
                                                             GiopServerConnection serverCon,
                                                             out IMessage responseMsg, out ITransportHeaders responseHeaders,
                                                             out Stream responseStream) {
            responseHeaders = null;
            LocateRequestMessage deserReqMsg =
                m_messageHandler.ParseIncomingLocateRequestMessage(msgInput);

            // TODO: dummy implementation, don't check yet
            LocateReplyMessage response = new LocateReplyMessage(LocateStatus.OBJECT_HERE);

            responseMsg = response;
            PrepareResponseHeaders(ref responseHeaders, serverCon);
            // get the stream into which the message should be serialied from a stream handling
            // sink in the stream handling chain
            responseStream = GetResponseStreamFor(sinkStack, responseMsg, responseHeaders);

            m_messageHandler.SerialiseOutgoingLocateReplyMessage(response, deserReqMsg,
                                                                 msgInput.Header.Version,
                                                                 responseStream, serverCon.ConDesc);
            return ServerProcessing.Complete;
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg,
                                               ITransportHeaders requestHeaders, Stream requestStream,
                                               out IMessage responseMsg, out ITransportHeaders responseHeaders,
                                               out Stream responseStream) {
            responseMsg = null;
            responseHeaders = null;
            responseStream = null;
            CdrMessageInputStream msgInput = new CdrMessageInputStream(requestStream);
            GiopServerConnection serverCon = (GiopServerConnection)
                requestHeaders[GiopServerConnection.SERVER_TR_HEADER_KEY];
            try {
                if (msgInput.Header.GiopType == GiopMsgTypes.Request) {
                    return ProcessRequestMessage(sinkStack, requestHeaders, msgInput, serverCon,
                                                 out responseMsg, out responseHeaders, out responseStream);
                } else if (msgInput.Header.GiopType == GiopMsgTypes.LocateRequest) {
                    return ProcessLocateRequestMessage(sinkStack, requestHeaders,
                                                       msgInput, serverCon,
                                                       out responseMsg, out responseHeaders, out responseStream);
                } else {
                    Trace.WriteLine("Processing problem on server connection after unexpected message of type " +
                                    msgInput.Header.GiopType);
                    throw new NotSupportedException("wrong message type in server side formatter: " +
                                                    msgInput.Header.GiopType);
                }
            } finally {
                try {
                    requestStream.Close(); // not needed any more
                } catch {
                    // ignore
                }
            }
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state,
                                         IMessage msg, ITransportHeaders headers, Stream stream) {
            // headers, stream are null, because formatting is first sink to create these two
            AsyncServerProcessingData asyncData = (AsyncServerProcessingData) state;
            try {
                IMessage requestMsg = asyncData.RequestMsg;
                SerialiseResponse(sinkStack, requestMsg, asyncData.ServerCon, msg,
                                  ref headers, out stream);
            }
            catch (Exception e) {
                if (asyncData.RequestMsg is IMethodCallMessage) {
                    msg = new ReturnMessage(e, (IMethodCallMessage) asyncData.RequestMsg);
                } else {
                    msg = new ReturnMessage(e, null); // no useful information present for requestMsg
                }
                // serialise the exception
                SerialiseExceptionResponse(sinkStack, (IMessage)state, asyncData.ServerCon, msg,
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

        /// <summary>
        /// the giop message handler responsible for serializing/deserializing Giop messages.
        /// </summary>
        private GiopMessageHandler m_messageHandler;

        /// <summary>
        /// helper class to convert from url to ior.
        /// </summary>
        private IiopUrlUtil m_iiopUrlUtil;

        private RetryConfig m_retries;

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

            return new IiopClientFormatterSink(nextSink, m_conManager, m_messageHandler,
                                               m_iiopUrlUtil,
                                               m_retries);
        }

        #endregion

        internal void Configure(GiopClientConnectionManager conManager, GiopMessageHandler messageHandler,
                                IiopUrlUtil iiopUrlUtil,
                                RetryConfig retries) {
            m_conManager = conManager;
            m_messageHandler = messageHandler;
            m_iiopUrlUtil = iiopUrlUtil;
            m_retries = retries;
        }

        #endregion IMethods

    }

    /// <summary>
    /// this class is a provider for the IIOPServerFormatterSink.
    /// </summary>
    public class IiopServerFormatterSinkProvider : IServerFormatterSinkProvider {

        #region IFields

        private IServerChannelSinkProvider m_nextProvider; // is set during channel creation with the set accessor of the property

        /// <summary>
        /// the giop message handler responsible for serializing/deserializing Giop messages.
        /// </summary>
        private GiopMessageHandler m_messageHandler;

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
            return new IiopServerFormatterSink(next, m_messageHandler);
        }

        public void GetChannelData(IChannelDataStore channelData) {
            // not useful for this provider
        }

        #endregion

        internal void Configure(GiopMessageHandler messageHandler) {
            m_messageHandler = messageHandler;
        }

        #endregion IMethods

    }




}
