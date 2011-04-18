/* GiopMessageBodySerializer.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 17.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Interception;
using omg.org.CORBA;
using omg.org.IOP;


namespace Ch.Elca.Iiop.MessageHandling {

    
    /// <summary>
    /// this exception is thrown, when something does not work during request deserialization.
    /// This exception stores
    /// all the information needed to construct an exception reply.
    /// </summary>
    [Serializable]
    internal class RequestDeserializationException : Exception {
        
        #region IFields
        
        private Exception m_reason;
        private IMessage m_requestMessage;
        private IMessage m_responseMessage;

        #endregion IFields
        #region IConstructors
        
        /// <param name="reason">the reason for deserialization error</param>
        /// <param name="requestMessage">the message decoded so far</param>
        /// <param name="responseMessage">the response message to this problem</param>
        public RequestDeserializationException(Exception reason, IMessage requestMessage,
                                               IMessage responseMessage) {
            m_reason = reason;
            m_requestMessage = requestMessage;
            m_responseMessage = responseMessage;
        }

        #endregion IConstructors
        #region IProperties

        public Exception Reason {
            get { 
                return m_reason; 
            }
        }

        public IMessage RequestMessage {
            get { 
                return m_requestMessage; 
            }
        }
        
        public IMessage ResponseMessage {
            get {
                return m_responseMessage;
            }
        }

        #endregion IProperties

    }

    
    /// <summary>
    /// This class is reponsible for serialising/deserialising message bodys of Giop Messages for
    /// the different message types
    /// </summary>
    internal class GiopMessageBodySerialiser {

        #region IFields

        private ArgumentsSerializerFactory m_argSerFactory; 
        private SerializerFactory m_serFactory;
        private CodecFactory m_codecFactory;

        #endregion IFields
        #region IConstructors

        internal GiopMessageBodySerialiser(ArgumentsSerializerFactory argSerFactory) {
            m_serFactory = argSerFactory.SerializerFactory;
            m_argSerFactory = argSerFactory;
            m_codecFactory = new CodecFactoryImpl(m_serFactory);
        }

        #endregion IConstructors
        #region IMethods
        
        #region Common
        
        /// <summary>
        /// perform code set establishment on the client side
        /// </summary>
        protected void PerformCodeSetEstablishmentClient(IIorProfile targetProfile,
                                                         GiopConnectionDesc conDesc,
                                                         ServiceContextList cntxColl) {
            
            if (targetProfile.Version.IsAfterGiop1_0()) {

                if (!conDesc.IsCodeSetNegotiated()) {
                    Codec codec =
                        m_codecFactory.create_codec(new Encoding(ENCODING_CDR_ENCAPS.ConstVal,
                                                                 targetProfile.Version.Major,
                                                                 targetProfile.Version.Minor));
                    object codeSetComponent = CodeSetService.FindCodeSetComponent(targetProfile, codec);
                    if (codeSetComponent != null) {
                        int charSet = CodeSetService.ChooseCharSet((CodeSetComponentData)codeSetComponent);
                        int wcharSet = CodeSetService.ChooseWCharSet((CodeSetComponentData)codeSetComponent);
                        conDesc.SetNegotiatedCodeSets(charSet, wcharSet);
                    } else {
                        conDesc.SetCodeSetNegotiated();
                    }
                    if (conDesc.IsCodeSetDefined()) {
                        // only insert a codeset service context, if a codeset is selected
                        CodeSetService.InsertCodeSetServiceContext(cntxColl,
                            conDesc.CharSet, conDesc.WCharSet);
                    }
                }
            } else {
                // giop 1.0; don't send code set service context; don't check again
                conDesc.SetCodeSetNegotiated();
            }
            
        }
        
        /// <summary>
        /// perform code set establishment on the server side
        /// </summary>
        protected void PerformCodeSetEstablishmentServer(GiopVersion version,
                                                         GiopConnectionDesc conDesc, 
                                                         ServiceContextList cntxColl) {
            if (version.IsAfterGiop1_0()) {
                if (!conDesc.IsCodeSetNegotiated()) {
                    // check for code set establishment
                    CodeSetServiceContext context = 
                        CodeSetService.FindCodeSetServiceContext(cntxColl);
                    if (context != null) {
                        CodeSetService.CheckCodeSetCompatible(context.CharSet,
                                                              context.WCharSet);
                        conDesc.SetNegotiatedCodeSets(context.CharSet,
                                                      context.WCharSet);
                    }
                }
            } else {
                conDesc.SetCodeSetNegotiated();
            }
        }

        protected void SerialiseContext(CdrOutputStream targetStream, ServiceContextList cntxList) {
            cntxList.WriteSvcContextList(targetStream);
        }

        protected ServiceContextList DeserialiseContext(CdrInputStream sourceStream) {
            return new ServiceContextList(sourceStream);
        }

        protected void AlignBodyIfNeeded(CdrInputStream cdrStream, GiopVersion version) {
            if (!version.IsBeforeGiop1_2()) {
                cdrStream.ForceReadAlign(Aligns.Align8);
            } // force an align on 8 for GIOP-version >= 1.2
        }

        protected void AlignBodyIfNeeded(CdrOutputStream cdrStream, GiopVersion version) {
            if (!version.IsBeforeGiop1_2()) {
                cdrStream.ForceWriteAlign(Aligns.Align8); 
            } // force an align on 8 for GIOP-version >= 1.2
        }

        /// <summary>
        /// the same as AlignBodyIfNeeded, but without throwing exception, when not enough bytes.
        /// </summary>
        protected void TryAlignBodyIfNeeded(CdrInputStream cdrStream, GiopVersion version) {
            if (!version.IsBeforeGiop1_2()) {
                cdrStream.TryForceReadAlign(Aligns.Align8);
            } // force an align on 8 for GIOP-version >= 1.2
        }

        /// <summary>
        /// set the codesets for the stream after codeset service descision
        /// </summary>
        /// <param name="cdrStream"></param>
        private void SetCodeSet(CdrInputStream cdrStream, GiopConnectionDesc conDesc) {
            if (conDesc.IsCodeSetDefined()) {
                // set the codeset, if one is chosen
                cdrStream.CharSet = conDesc.CharSet;
                cdrStream.WCharSet = conDesc.WCharSet;
            } // otherwise: use cdrStream default.
        }

        /// <summary>
        /// set the codesets for the stream after codeset service descision
        /// </summary>
        /// <param name="cdrStream"></param>
        private void SetCodeSet(CdrOutputStream cdrStream, GiopConnectionDesc conDesc) {
            if (conDesc.IsCodeSetDefined()) {
                // set the codeset, if one is chosen
                cdrStream.CharSet = conDesc.CharSet;
                cdrStream.WCharSet = conDesc.WCharSet;
            } // otherwise: use cdrStream default.
        }

        /// <summary>read the target for the request</summary>
        /// <returns>the objectURI extracted from this msg</returns>
        private string ReadTarget(CdrInputStream cdrStream, GiopVersion version,
                                  out byte[] objectKey) {
            if (version.IsBeforeGiop1_2()) {
                // for GIOP 1.0 / 1.1 only object key is possible
                objectKey = ReadTargetKey(cdrStream);
            } else {
                // for GIOP >= 1.2, a union is used for target information
                ushort targetAdrType = cdrStream.ReadUShort();
                switch (targetAdrType) {
                    case 0:
                        objectKey = ReadTargetKey(cdrStream);
                        break;
                    default:
                        Trace.WriteLine("received not yet supported target address type: " + targetAdrType);
                        throw new BAD_PARAM(650, CompletionStatus.Completed_No);
                }
            }
            // get the object-URI of the responsible object
            return IorUtil.GetObjectUriForObjectKey(objectKey);
        }

        private byte[] ReadTargetKey(CdrInputStream cdrStream) {
            uint length = cdrStream.ReadULong();
            Debug.WriteLine("object key follows:");
            byte[] objectKey = cdrStream.ReadOpaque((int)length);
            return objectKey;
        }

        #endregion Common
        #region Requests

        private void WriteTarget(CdrOutputStream cdrStream, 
                                 byte[] objectKey, GiopVersion version) {
            if (!version.IsBeforeGiop1_2()) {
                // for GIOP >= 1.2
                ushort targetAdrType = 0;
                cdrStream.WriteUShort(targetAdrType); // object key adressing
            }
            WriteTargetKey(cdrStream, objectKey);
        }

        private void WriteTargetKey(CdrOutputStream cdrStream, byte[] objectKey) {
            Debug.WriteLine("writing object key with length: " + objectKey.Length);
            cdrStream.WriteULong((uint)objectKey.Length); // object-key length
            cdrStream.WriteOpaque(objectKey);
        }

        /// <summary>
        /// serialises the message body for a GIOP request
        /// </summary>
        /// <param name="clientRequest">the giop request Msg</param>
        /// <param name="targetStream"></param>
        /// <param name="version">the Giop version to use</param>
        /// <param name="conDesc">the connection used for this request</param>
        internal void SerialiseRequest(GiopClientRequest clientRequest,
                                       CdrOutputStream targetStream, 
                                       IIorProfile targetProfile, GiopConnectionDesc conDesc) {
            Trace.WriteLine(String.Format("serializing request for method {0}; uri {1}; id {2}", 
                                          clientRequest.MethodToCall, clientRequest.CalledUri, 
                                          clientRequest.RequestId));
            try {
                clientRequest.SetRequestPICurrentFromThreadScopeCurrent(); // copy from thread scope picurrent before processing request
                
                ArgumentsSerializer ser =
                    m_argSerFactory.Create(clientRequest.MethodToCall.DeclaringType);
                // determine the request method to send
                string idlRequestName = ser.GetRequestNameFor(clientRequest.MethodToCall);
                clientRequest.RequestMethodName = idlRequestName;
                
                clientRequest.InterceptSendRequest();
                GiopVersion version = targetProfile.Version;
                ServiceContextList cntxColl = clientRequest.RequestServiceContext;
                // set code-set for the stream
                PerformCodeSetEstablishmentClient(targetProfile, conDesc, cntxColl);
                SetCodeSet(targetStream, conDesc);
                
                if (version.IsBeforeGiop1_2()) { // for GIOP 1.0 / 1.1
                    SerialiseContext(targetStream, cntxColl); // service context
                }
                
                targetStream.WriteULong(clientRequest.RequestId);
                byte responseFlags = 0;
                if (version.IsBeforeGiop1_2()) { // GIOP 1.0 / 1.1
                    responseFlags = 1;
                } else {
                    // reply-expected, no DII-call --> must be 0x03, no reply --> must be 0x00
                    responseFlags = 3;
                }
                if (clientRequest.IsOneWayCall) {
                    responseFlags = 0;
                } // check if one-way
                // write response-flags
                targetStream.WriteOctet(responseFlags);
                
                targetStream.WritePadding(3); // reserved bytes
                WriteTarget(targetStream, targetProfile.ObjectKey, version); // write the target-info
                targetStream.WriteString(clientRequest.RequestMethodName); // write the method name
                
                if (version.IsBeforeGiop1_2()) { // GIOP 1.0 / 1.1
                    targetStream.WriteULong(0); // no principal
                } else { // GIOP 1.2
                    SerialiseContext(targetStream, cntxColl); // service context
                }
                SerialiseRequestBody(targetStream, clientRequest, version, ser);
            } catch (Exception ex) {
                Debug.WriteLine("exception while serialising request: " + ex);
                Exception newException = clientRequest.InterceptReceiveException(ex); // interception point may change exception
                if (newException == ex) {
                    throw;
                } else {
                    throw newException; // exception has been changed by interception point
                }
            }
        }

        /// <summary>serializes the request body</summary>
        /// <param name="targetStream"></param>
        /// <param name="clientRequest">the request to serialise</param>
        /// <param name="version">the GIOP-version</param>
        private void SerialiseRequestBody(CdrOutputStream targetStream, GiopClientRequest clientRequest,
                                          GiopVersion version, ArgumentsSerializer ser) {
            // body of request msg: serialize arguments
            // clarification from CORBA 2.6, chapter 15.4.1: no padding, when no arguments are serialised  -->
            // for backward compatibility, do it nevertheless
            AlignBodyIfNeeded(targetStream, version);
            ser.SerializeRequestArgs(clientRequest.RequestMethodName, 
                                     clientRequest.RequestArguments,
                                     targetStream, clientRequest.RequestCallContext);
        }

        /// <summary>
        /// Deserialises the Giop Message body for a request
        /// </summary>
        /// <param name="cdrStream"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        internal IMessage DeserialiseRequest(CdrInputStream cdrStream, GiopVersion version,
                                             GiopConnectionDesc conDesc, IInterceptionOption[] interceptionOptions) {
            MethodCall methodCallInfo = null;
            GiopServerRequest serverRequest = new GiopServerRequest(conDesc, interceptionOptions);
            serverRequest.Version = version;
            try {
                ServiceContextList cntxColl = null;
                if (version.IsBeforeGiop1_2()) { // GIOP 1.0 / 1.1
                    cntxColl = DeserialiseContext(cdrStream); // Service context deser
                }
                
                // read the request-ID and set it as a message property
                uint requestId = cdrStream.ReadULong(); 
                serverRequest.RequestId = requestId;
                Trace.WriteLine("received a message with reqId: " + requestId);
                // read response-flags:
                byte respFlags = cdrStream.ReadOctet(); Debug.WriteLine("response-flags: " + respFlags);
                cdrStream.ReadPadding(3); // read reserved bytes
                serverRequest.ResponseFlags = respFlags;
                
                // decode the target of this request
                byte[] objectKey;
                serverRequest.RequestUri = ReadTarget(cdrStream, version, out objectKey);
                serverRequest.ObjectKey = objectKey;
                serverRequest.RequestMethodName = cdrStream.ReadString();
                Trace.WriteLine("call for .NET object: " + serverRequest.RequestUri + 
                                ", methodName: " + serverRequest.RequestMethodName);

                if (version.IsBeforeGiop1_2()) { // GIOP 1.0 / 1.1
                    uint principalLength = cdrStream.ReadULong();
                    cdrStream.ReadOpaque((int)principalLength);
                } else {
                    cntxColl = DeserialiseContext(cdrStream); // Service context deser
                }
                PerformCodeSetEstablishmentServer(version, conDesc, cntxColl);
                // set codeset for stream
                SetCodeSet(cdrStream, conDesc);
                // request header deserialised

                serverRequest.RequestServiceContext = cntxColl;
                serverRequest.InterceptReceiveRequestServiceContexts();
                serverRequest.SetThreadScopeCurrentFromPICurrent(); // copy request scope picurrent to thread scope pi-current
                
                serverRequest.ResolveTargetType(); // determine the .net target object type and check if target object is available
                ArgumentsSerializer argSer =
                    m_argSerFactory.Create(serverRequest.ServerTypeType);
                MethodInfo called =
                    argSer.GetMethodInfoFor(serverRequest.RequestMethodName);
                serverRequest.ResolveCalledMethod(called); // set target method and handle special cases
                IDictionary contextElements;
                DeserialiseRequestBody(cdrStream, version, serverRequest, argSer, out contextElements);
                methodCallInfo = new MethodCall(serverRequest.Request);
                if (contextElements != null) {
                    AddContextElementsToCallContext(methodCallInfo.LogicalCallContext, contextElements);
                }
                serverRequest.UpdateWithFinalRequest(methodCallInfo);
                serverRequest.InterceptReceiveRequest(); // all information now available
                return methodCallInfo;
            } catch (Exception e) {
                // an Exception encountered during deserialisation
                try {
                    cdrStream.SkipRest(); // skip rest of the message, to not corrupt the stream
                } catch (Exception) {
                    // ignore exception here, already an other exception leading to problems
                }
                ReturnMessage exceptionResponse;
                exceptionResponse = new ReturnMessage(e, methodCallInfo);
                throw new RequestDeserializationException(e, serverRequest.Request, exceptionResponse);
                // send exception interception point will be called when serialising exception response
            }
        }

        private void AddContextElementsToCallContext(LogicalCallContext callContext, IDictionary elements) {
            foreach (DictionaryEntry entry in elements) {
                callContext.SetData((string)entry.Key, new CorbaContextElement((string)entry.Value));
            }
        }

        private object[] AdaptArgsForStandardOp(object[] args, string objectUri) {
            object[] result = new object[args.Length+1];
            result[0] = objectUri; // this argument is passed to all standard operations
            Array.Copy((Array)args, 0, result, 1, args.Length);
            return result;
        }

        /// <summary>deserialise the request body</summary>
        /// <param name="contextElements">the deserialised context elements, if any or null</param>
        private void DeserialiseRequestBody(CdrInputStream cdrStream, GiopVersion version,
                                            GiopServerRequest request,
                                            ArgumentsSerializer ser,
                                            out IDictionary contextElements) {
            // clarification from CORBA 2.6, chapter 15.4.1: no padding, when no arguments/no context elements
            // are serialised, i.e. body empty
            // ignores, if not enough bytes because no args/context; because in this case, no more bytes follow
            TryAlignBodyIfNeeded(cdrStream, version);

            // unmarshall parameters
            object[] args = ser.DeserializeRequestArgs(request.RequestMethodName, cdrStream,
                                                       out contextElements);

            // for standard corba ops, adapt args:
            if (request.IsStandardCorbaOperation) {
                args = AdaptArgsForStandardOp(args, request.RequestUri);
            }
            request.RequestArgs = args;
        }

        #endregion Requests
        #region Replys

        /// <summary>serialize the GIOP message body of a repsonse message</summary>
        /// <param name="requestId">the requestId of the request, this response belongs to</param>
        internal void SerialiseReply(GiopServerRequest request, CdrOutputStream targetStream, 
                                   GiopVersion version,
                                   GiopConnectionDesc conDesc) {
            Trace.WriteLine("serializing response for method: " + request.GetRequestedMethodNameInternal());
            try {
                bool isExceptionReply = request.IsExceptionReply;
                Exception exceptionToSend = null;
                try {
                    request.SetRequestPICurrentFromThreadScopeCurrent(); // copy from thread scope picurrent after processing request by servant
                    // reply interception point
                    if (!request.IsExceptionReply) {
                        request.InterceptSendReply();
                    } else {
                        exceptionToSend = request.InterceptSendException(request.IdlException);
                    }
                } catch (Exception ex) {
                    // update the reply with the exception from interception layer
                    isExceptionReply = true;
                    if (SerialiseAsSystemException(ex)) {
                        exceptionToSend = ex;
                    } else {
                        exceptionToSend = new UNKNOWN(300, CompletionStatus.Completed_MayBe);
                    }
                }
                ServiceContextList cntxColl = request.ResponseServiceContext;
                SetCodeSet(targetStream, conDesc);
                
                if (version.IsBeforeGiop1_2()) { // for GIOP 1.0 / 1.1
                    SerialiseContext(targetStream, cntxColl); // serialize the context
                }
                
                targetStream.WriteULong(request.RequestId);
                
                if (!isExceptionReply) {
                    Trace.WriteLine("sending normal response to client");
                    targetStream.WriteULong(0); // reply status ok
                    
                    if (!version.IsBeforeGiop1_2()) { // for GIOP 1.2 and later, service context is here
                        SerialiseContext(targetStream, cntxColl); // serialize the context
                    }
                    // serialize a response to a successful request
                    SerialiseResponseOk(targetStream, request, version);
                    Trace.WriteLine("reply body serialised");
                } else {
                    Trace.WriteLine("excpetion to send to client: " + exceptionToSend.GetType());
                    
                    if (SerialiseAsSystemException(exceptionToSend)) {
                        targetStream.WriteULong(2); // system exception
                    } else if (SerialiseAsUserException(exceptionToSend)) {
                        targetStream.WriteULong(1); // user exception
                    } else {
                        // should not occur
                        targetStream.WriteULong(2);
                        exceptionToSend = new INTERNAL(204, CompletionStatus.Completed_Yes);
                    }
                    
                    if (!version.IsBeforeGiop1_2()) { // for GIOP 1.2 and later, service context is here
                        SerialiseContext(targetStream, cntxColl); // serialize the context
                    }
                    AlignBodyIfNeeded(targetStream, version);
                    if (SerialiseAsSystemException(exceptionToSend)) {
                        SerialiseSystemException(targetStream, exceptionToSend);
                    } else {
                        SerialiseUserException(targetStream, (AbstractUserException)exceptionToSend);
                    }
                    Trace.WriteLine("exception reply serialised");
                }
            } finally {
                request.ClearThreadScopePICurrent(); // no longer needed, clear afterwards to prevent access to stale data during next requests
            }
        }

        private void SerialiseResponseOk(CdrOutputStream targetStream, GiopServerRequest request,
                                         GiopVersion version) {
            // reply body
            // clarification form CORBA 2.6, chapter 15.4.2: no padding, when no arguments are serialised  -->
            // for backward compatibility, do it nevertheless
            AlignBodyIfNeeded(targetStream, version);
            // marshal the parameters
            
            ArgumentsSerializer ser =
                m_argSerFactory.Create(request.CalledMethod.DeclaringType);
            ser.SerializeResponseArgs(request.RequestMethodName, request.ReturnValue, request.OutArgs,
                                      targetStream);
        }

        /// <summary>serialize the exception as a CORBA System exception</summary>
        private bool SerialiseAsSystemException(Exception e) {
            return (e is omg.org.CORBA.AbstractCORBASystemException);
        }
        
        /// <summary>serialize the exception as a CORBA user exception</summary>
        private bool SerialiseAsUserException(Exception e) {
            return (e is AbstractUserException);
        }

        private void SerialiseSystemException(CdrOutputStream targetStream, Exception corbaEx) {
            // serialize a system exception
            if (!(corbaEx is AbstractCORBASystemException)) {
                corbaEx = new UNKNOWN(202, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            Serializer ser =
                m_serFactory.Create(corbaEx.GetType(), Util.AttributeExtCollection.EmptyCollection);
            ser.Serialize(corbaEx, targetStream);
        }

        private void SerialiseUserException(CdrOutputStream targetStream, AbstractUserException userEx) {
            Type exceptionType = userEx.GetType();
            // serialize a user exception
            Serializer ser =
                m_serFactory.Create(exceptionType, Util.AttributeExtCollection.EmptyCollection);
            ser.Serialize(userEx, targetStream);
        }


        /// <summary>
        /// helper method to update the GiopClientRequest with the new data from the reply
        /// </summary>
        private void UpdateClientRequestWithReplyData(GiopClientRequest request,
                                                      IMessage response,
                                                      ServiceContextList cntxColl) {
            request.Reply = response;
            request.ResponseServiceContext = cntxColl; // store the deserialised service context for handling in interceptors
        }
        
        internal IMessage DeserialiseReply(CdrInputStream cdrStream, 
                                         GiopVersion version, GiopClientRequest request,
                                         GiopConnectionDesc conDesc) {

            ServiceContextList cntxColl = null;
            IMessage response = null;
            try {
                if (version.IsBeforeGiop1_2()) { // for GIOP 1.0 / 1.1, the service context is placed here
                    cntxColl = DeserialiseContext(cdrStream); // deserialize the service contexts
                }
                
                cdrStream.ReadULong(); // skip request id, already handled by transport
                uint responseStatus = cdrStream.ReadULong();
                if (!version.IsBeforeGiop1_2()) { // for GIOP 1.2 and later, service context is here
                    cntxColl = DeserialiseContext(cdrStream); // deserialize the service contexts
                }
                // set codeset for stream
                SetCodeSet(cdrStream, conDesc);
                switch (responseStatus) {
                    case 0 :
                        Trace.WriteLine("deserializing normal reply for methodCall: " + request.MethodToCall);
                        response = DeserialiseNormalReply(cdrStream, version, request);
                        UpdateClientRequestWithReplyData(request, response, cntxColl);
                        request.InterceptReceiveReply();
                        break;
                    case 1 :
                        Exception userEx = DeserialiseUserException(cdrStream, version); // the error .NET message for this exception is created in the formatter
                        UpdateClientRequestWithReplyData(request, new ReturnMessage(userEx, request.Request), cntxColl);
                        userEx = request.InterceptReceiveException(userEx);
                        response = new ReturnMessage(userEx, request.Request); // definitive exception only available here, because interception chain may change exception
                        break;
                    case 2 :
                        Exception systemEx = DeserialiseSystemError(cdrStream, version); // the error .NET message for this exception is created in the formatter
                        UpdateClientRequestWithReplyData(request, new ReturnMessage(systemEx, request.Request), cntxColl);
                        systemEx = request.InterceptReceiveException(systemEx);
                        response = new ReturnMessage(systemEx, request.Request); // definitive exception only available here, because interception chain may change exception
                        break;
                    case 3 :
                    case 4 :
                        // LOCATION_FORWARD / LOCATION_FORWARD_PERM:
                        // --> deserialise it and return location fwd message
                        response = DeserialiseLocationFwdReply(cdrStream, version, request);
                        UpdateClientRequestWithReplyData(request, response, cntxColl);
                        request.InterceptReceiveOther();
                        break;
                        default :
                            // deseralization of reply error, unknown reply status: responseStatus
                            // the error .NET message for this exception is created in the formatter
                            throw new MARSHAL(2401, CompletionStatus.Completed_MayBe);
                }
            } catch (Exception ex) {
                Trace.WriteLine("exception while deserialising reply: " + ex);
                try {
                    cdrStream.SkipRest();
                } catch (Exception) {
                    // ignore this one, already problems.
                }
                if (!request.IsReplyInterceptionChainCompleted()) { // reply interception chain not yet called for this reply
                    // deserialisation not ok: interception not called;
                    // call interceptors with this exception.
                    request.Reply = new ReturnMessage(ex, request.Request as IMethodCallMessage);
                    Exception newException = request.InterceptReceiveException(ex); // exception may be changed by interception point
                    if (ex != newException) {
                        throw newException; // exception have been changed by interception point
                    }
                }
                throw;
            }
            return response;
        }

        /// <summary>deserialize response with ok-status.</summary>
        private IMessage DeserialiseNormalReply(CdrInputStream cdrStream, GiopVersion version, 
                                                GiopClientRequest request) {
            // body
            // clarification from CORBA 2.6, chapter 15.4.2: no padding, when no arguments are serialised
            TryAlignBodyIfNeeded(cdrStream, version); // read alignement, if present
            
            // read the parameters
            object[] outArgs;
            object retVal = null;

            ArgumentsSerializer ser =
                m_argSerFactory.Create(request.MethodToCall.DeclaringType);
            retVal = ser.DeserializeResponseArgs(request.RequestMethodName, out outArgs,
                                                 cdrStream);
            ReturnMessage response = new ReturnMessage(retVal, outArgs, outArgs.Length, null, 
                                                       request.Request);
            return response;
        }

        /// <summary>deserialises a CORBA system exception </summary>
        private Exception DeserialiseSystemError(CdrInputStream cdrStream, GiopVersion version) {
            // exception body
            AlignBodyIfNeeded(cdrStream, version);

            Serializer ser =
                m_serFactory.Create(typeof(omg.org.CORBA.AbstractCORBASystemException), 
                                    Util.AttributeExtCollection.EmptyCollection);
            Exception result = (Exception) ser.Deserialize(cdrStream);
            if (result == null) { 
                return new Exception("received system error from peer orb, but error was not deserializable");
            } else {
                return result;
            }
        }

        private Exception DeserialiseUserException(CdrInputStream cdrStream, GiopVersion version) {
            // exception body
            AlignBodyIfNeeded(cdrStream, version);

            Serializer ser =
                m_serFactory.Create(typeof(AbstractUserException), 
                                    Util.AttributeExtCollection.EmptyCollection);
            Exception result = (Exception) ser.Deserialize(cdrStream);
            if (result == null) {
                return new Exception("user exception received from peer orb, but was not deserializable");
            } else {
                return result;
            }
        }
        
        /// <summary>
        /// deserialise the location fwd
        /// </summary>
        private LocationForwardMessage DeserialiseLocationFwdReply(CdrInputStream cdrStream, 
                                                                   GiopVersion version,
                                                                   GiopClientRequest request) {
            AlignBodyIfNeeded(cdrStream, version);
            // read the Location fwd IOR
            Serializer ser =
                m_serFactory.Create(request.MethodToCall.DeclaringType, 
                                    Util.AttributeExtCollection.EmptyCollection);
            MarshalByRefObject newProxy = ser.Deserialize(cdrStream)
                                              as MarshalByRefObject;
            if (newProxy == null) {
                throw new OBJECT_NOT_EXIST(2402, CompletionStatus.Completed_No);
            }
            return new LocationForwardMessage(newProxy);
        }
                    
        #endregion Replys
        #region Locate

        /// <summary>
        /// deserialise a locate request msg.
        /// </summary>
        /// <param name="cdrStream"></param>
        /// <param name="version"></param>
        /// <param name="forRequestId">returns the request id as out param</param>
        /// <returns>the uri of the object requested to find</returns>
        public LocateRequestMessage DeserialiseLocateRequest(CdrInputStream cdrStream, GiopVersion version) {
            uint forRequestId = cdrStream.ReadULong();
            byte[] objectKey;
            string uri = ReadTarget(cdrStream, version, out objectKey);
            return new LocateRequestMessage(forRequestId, objectKey, uri);
        }

        /// <summary>
        /// serialises a locate reply message.
        /// </summary>
        /// <param name="forwardAddr">
        /// specifies the IOR of the object to forward the call to. This parameter must be != null,
        /// if LocateStatus is OBJECT_FORWARD.
        ///  </param>
        public void SerialiseLocateReply(CdrOutputStream targetStream, GiopVersion version, uint forRequestId, 
                                         LocateReplyMessage msg) {
            targetStream.WriteULong(forRequestId);
            switch (msg.Status) {
                case LocateStatus.OBJECT_HERE:
                    targetStream.WriteULong((uint)msg.Status);
                    break;
                default:
                    Debug.WriteLine("Locate reply status not supported");
                    throw new NotSupportedException("not supported");
            }
        }

        #endregion Locate
        
        #endregion IMethods

    }

}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Idl;
    using Ch.Elca.Iiop.Util;
    using Ch.Elca.Iiop.MessageHandling;
    using Ch.Elca.Iiop.Cdr;
    using omg.org.CORBA;

    
    interface TestStringInterface {
        
        [return: StringValue()]
        [return: WideChar(false)]
        string EchoString([StringValue] [WideChar(false)] string arg);
        
        [return: StringValue()]
        [return: WideChar(true)]
        string EchoWString([StringValue] [WideChar(true)] string arg);
        
    }
    
    public class TestStringInterfaceImpl : MarshalByRefObject, TestStringInterface {

        [return: StringValue()]
        [return: WideChar(false)]
        public string EchoString([StringValue] [WideChar(true)] string arg) {
            return arg;
        }

        [return: StringValue()]
        [return: WideChar(false)]
        public string EchoWString([StringValue] [WideChar(true)] string arg) {
            return arg;
        }
    }

    /// <summary>
    /// Unit-tests for testing request/reply serialisation/deserialisation
    /// </summary>
    [TestFixture]
    public class MessageBodySerialiserTest {
        
        private IiopUrlUtil m_iiopUrlUtil;
        private SerializerFactory m_serFactory;
        
        [SetUp]
        public void SetUp() {
            m_serFactory =
                new SerializerFactory();
            CodecFactory codecFactory =
                new CodecFactoryImpl(m_serFactory);
            Codec codec = 
                codecFactory.create_codec(
                    new Encoding(ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_iiopUrlUtil = 
                IiopUrlUtil.Create(codec, new object[] { 
                    Services.CodeSetService.CreateDefaultCodesetComponent(codec)});
            m_serFactory.Initalize(new SerializerFactoryConfig(), m_iiopUrlUtil);
        }
        
        [Test]
        public void TestSameServiceIdMultiple() {
            // checks if service contexts with the same id, doesn't throw an exception
            // checks, that the first service context is considered, others are thrown away
            GiopMessageBodySerialiser ser = new GiopMessageBodySerialiser(
                                                new ArgumentsSerializerFactory(m_serFactory));
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, 0, new GiopVersion(1,2));
            cdrOut.WriteULong(2); // nr of contexts
            cdrOut.WriteULong(1234567); // id of context 1
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0);
            cdrOut.WriteEncapsulation(encap);
            cdrOut.WriteULong(1234567); // id of context 2
            encap = new CdrEncapsulationOutputStream(0);
            cdrOut.WriteEncapsulation(encap);
            // reset stream
            stream.Seek(0, SeekOrigin.Begin);
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(0, new GiopVersion(1,2));
            omg.org.IOP.ServiceContextList result = new ServiceContextList(cdrIn);
            // check if context is present
            Assert.IsTrue(result.ContainsServiceContext(1234567), "expected context not in collection");
        }
        
        [Test]
        [ExpectedException(typeof(BAD_PARAM))]
        public void TestWCharSetNotDefinedClient() {
            MethodInfo methodToCall =
                typeof(TestStringInterface).GetMethod("EchoWString");
            object[] args = new object[] { "test" };
            string uri = 
                "IOR:000000000000000100000000000000010000000000000020000102000000000A6C6F63616C686F73740004D2000000047465737400000000";
            Ior target = m_iiopUrlUtil.CreateIorForUrl(uri, "");
            IIorProfile targetProfile = target.Profiles[0];
            TestMessage msg = new TestMessage(methodToCall, args, uri);
            msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY] = (uint)5; // set request-id
            msg.Properties[SimpleGiopMsg.TARGET_PROFILE_KEY] = targetProfile;
            
            // prepare connection context
            GiopClientConnectionDesc conDesc = new GiopClientConnectionDesc(null, null, 
                                                                            new GiopRequestNumberGenerator(), null);
            
            GiopMessageBodySerialiser ser = new GiopMessageBodySerialiser(
                                                new ArgumentsSerializerFactory(m_serFactory));
            GiopClientRequest request = 
                new GiopClientRequest(msg, conDesc,
                                      new IInterceptionOption[0]);
            CdrOutputStreamImpl targetStream = 
                new CdrOutputStreamImpl(new MemoryStream(), 0, new GiopVersion(1,2));
            ser.SerialiseRequest(request, targetStream, targetProfile,
                                 conDesc);
        }
        
        [Test]
        public void TestWCharSetDefinedClient() {
            MethodInfo methodToCall =
                typeof(TestStringInterface).GetMethod("EchoWString");
            object[] args = new object[] { "test" };
            string uri = "iiop://localhost:8087/testuri"; // Giop 1.2 will be used because no version spec in uri
            Ior target = m_iiopUrlUtil.CreateIorForUrl(uri, "");
            IIorProfile targetProfile = target.Profiles[0];
            TestMessage msg = new TestMessage(methodToCall, args, uri);
            msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY] = (uint)5; // set request-id
            msg.Properties[SimpleGiopMsg.TARGET_PROFILE_KEY] = targetProfile;
            
            // prepare connection context
            GiopClientConnectionDesc conDesc = new GiopClientConnectionDesc(null, null, 
                                                                            new GiopRequestNumberGenerator(), null);
                        
            GiopMessageBodySerialiser ser = new GiopMessageBodySerialiser(
                                                new ArgumentsSerializerFactory(m_serFactory));
            GiopClientRequest request = 
                new GiopClientRequest(msg, conDesc,
                                      new IInterceptionOption[0]);
            MemoryStream baseStream = new MemoryStream();
            CdrOutputStreamImpl targetStream =
                new CdrOutputStreamImpl(baseStream, 0, new GiopVersion(1,2));
            ser.SerialiseRequest(request, targetStream, targetProfile,
                                 conDesc);
            
            Assert.AreEqual(
                new byte[] { 0, 0, 0, 5, 3, 0, 0, 0,
                             0, 0, 0, 0, 
                             0, 0, 0, 7, 116, 101, 115, 116,
                             117, 114, 105, 0,
                             0, 0, 0, 12, 69, 99, 104, 111, 
                             87, 83, 116, 114, 105, 110, 103, 0,
                             0, 0, 0, 1, 0, 0, 0, 1,
                             0, 0, 0, 12, 1, 0, 0, 0,
                             0, 1, 0, 1, 0, 1, 1, 9,
                             0, 0, 0, 8, 0, 116, 0, 101,
                             0, 115, 0, 116},
                baseStream.ToArray(),"serialised message");
        }
        
        
        [Test]
        public void TestWCharSetDefinedServer() {
            byte[] sourceContent = 
                new byte[] {
                             0, 0, 0, 5, 3, 0, 0, 0,
                             0, 0, 0, 0, 
                             0, 0, 0, 7, 116, 101, 115, 116,
                             117, 114, 105, 0,
                             0, 0, 0, 12, 69, 99, 104, 111, 
                             87, 83, 116, 114, 105, 110, 103, 0,
                             0, 0, 0, 1, 0, 0, 0, 1,
                             0, 0, 0, 12, 0, 0, 0, 0,
                             0, 1, 0, 1, 0, 1, 1, 9,
                             0, 0, 0, 8, 0, 116, 0, 101,
                             0, 115, 0, 116};
            MemoryStream sourceStream =
                new MemoryStream(sourceContent);
            
            // create a connection context: this is needed for request deserialisation
            GiopConnectionDesc conDesc = new GiopConnectionDesc(null, null);

            // go to stream begin
            sourceStream.Seek(0, SeekOrigin.Begin);
            
            GiopMessageBodySerialiser ser = new GiopMessageBodySerialiser(
                                                new ArgumentsSerializerFactory(m_serFactory));
            
            CdrInputStreamImpl cdrSourceStream = 
                new CdrInputStreamImpl(sourceStream);
            cdrSourceStream.ConfigStream(0, new GiopVersion(1, 2));
            cdrSourceStream.SetMaxLength((uint)sourceContent.Length);
 
            IMessage result = null;
            TestStringInterfaceImpl service = new TestStringInterfaceImpl();
            try {
                // object which should be called
                string uri = "testuri";
                RemotingServices.Marshal(service, uri);

                // deserialise request message
                result = ser.DeserialiseRequest(cdrSourceStream, new GiopVersion(1,2),
                                                conDesc, InterceptorManager.EmptyInterceptorOptions);
            } finally {
                RemotingServices.Disconnect(service);
            }

            // now check if values are correct
            Assert.IsTrue(result != null, "deserialised message is null");
            object[] args = (object[])result.Properties[SimpleGiopMsg.ARGS_KEY];
            Assert.IsTrue(args != null, "args is null");
            Assert.AreEqual(1, args.Length);
            Assert.AreEqual("test", args[0]);
        }
        
        [Test]
        public void TestWCharSetNotDefinedServer() {
            byte[] sourceContent = 
                new byte[] {
                             0, 0, 0, 5, 3, 0, 0, 0,
                             0, 0, 0, 0, 
                             0, 0, 0, 7, 116, 101, 115, 116,
                             117, 114, 105, 0,
                             0, 0, 0, 12, 69, 99, 104, 111, 
                             87, 83, 116, 114, 105, 110, 103, 0,
                             0, 0, 0, 0, 0, 0, 0, 0, 
                             0, 0, 0, 8, 0, 116, 0, 101,
                             0, 115, 0, 116};
            MemoryStream sourceStream =
                new MemoryStream(sourceContent);
            
            // create a connection context: this is needed for request deserialisation
            GiopConnectionDesc conDesc = new GiopConnectionDesc(null, null);

            // go to stream begin
            sourceStream.Seek(0, SeekOrigin.Begin);
            
            GiopMessageBodySerialiser ser = new GiopMessageBodySerialiser(
                                                new ArgumentsSerializerFactory(m_serFactory));
            
            CdrInputStreamImpl cdrSourceStream = 
                new CdrInputStreamImpl(sourceStream);
            cdrSourceStream.ConfigStream(0, new GiopVersion(1, 2));
            cdrSourceStream.SetMaxLength((uint)sourceContent.Length);
 
            IMessage result = null;
            TestStringInterfaceImpl service = new TestStringInterfaceImpl();
            try {
                // object which should be called
                string uri = "testuri";
                RemotingServices.Marshal(service, uri);

                // deserialise request message
                result = ser.DeserialiseRequest(cdrSourceStream, new GiopVersion(1,2),
                                                conDesc, InterceptorManager.EmptyInterceptorOptions);
                Assert.Fail("no exception, although code set not set");
            } catch (RequestDeserializationException rde) {
                Assert.NotNull(rde.Reason, "rde inner exception");
                Assert.AreEqual(typeof(BAD_PARAM), rde.Reason.GetType(), "rde type");
            } finally {
                RemotingServices.Disconnect(service);
            }
        }

    }
    
}

#endif
