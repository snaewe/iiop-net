/* IIOPMessageBodySerializer.cs
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
    /// used to specify the LocateStatus for a locate reply
    /// </summary>
    public enum LocateStatus {
        UNKNOWN_OBJECT,
        OBJECT_HERE,
        OBJECT_FORWARD
    }
    
    
    /// <summary>
    /// simple implementation of the IMessage interface
    /// </summary>
    public class SimpleGiopMsg : IMessage {
        
        #region Constants

        /// <summary>the key to access the requestId in the message properties</summary>
        public const string REQUEST_ID_KEY = "_request_ID";
        /// <summary>the key to access the giop-version in the message properties</summary>
        public const string GIOP_VERSION_KEY = "_giop_version";
        /// <summary>the key to access the response-flags in the message properties</summary>
        public const string RESPONSE_FLAGS_KEY = "_response_flags";
        /// <summary>the key to access the idl method name in the message properties</summary>
        public const string IDL_METHOD_NAME_KEY = "_idl_method_name";        
        /// <summary>the key to access the flag, specifying if one of the corba standard ops, like is_a is called or a regular object operation, in the message properties</summary>
        public const string IS_STANDARD_CORBA_OP_KEY = "_is_standard_corba_op";           
        /// <summary>the key to access the client side requested uri in the message properties</summary>
        public const string REQUESTED_URI_KEY = "_requested_uri_op";                   
        /// <summary>the key to access the called method info</summary>
        public const string CALLED_METHOD_KEY = "_called_method";
        /// <summary>the key to access the service context for this message (either request or reply context, depending on the message</summary>
        public const string SERVICE_CONTEXT = "_service_context";
        /// <summary>the key to access the piCurrent request scoped slots for this message</summary>
        public const string PI_CURRENT_SLOTS = "_piCurrent_slots_";
        /// <summary>the key to access the interception flow instance in this message</summary>
        public const string INTERCEPTION_FLOW = "_interception_flow";
        /// <summary>the key to access the isAsyncMessage property in the message properties.</summary>
        public const string IS_ASYNC_REQUEST = "_is_async_request";
        /// <summary>the key to access the selected profile for connection on client side.</summary>
        public const string TARGET_PROFILE_KEY = "_target_profile_key";
        /// <summary>the key used to access the object key in the message properties (only server side)</summary>
        public const string REQUESTED_OBJECT_KEY = "__ObjectKey";        
        /// <summary>the key used to access the uri-property in messages</summary>         
        public const string URI_KEY = "__Uri";
        /// <summary>the key used to access the typename-property in messages</summary>
        public const string TYPENAME_KEY = "__TypeName";
        /// <summary>the key used to access the methodname-property in messages</summary>
        public const string METHODNAME_KEY = "__MethodName";
        /// <summary>the key used to access the argument-property in messages</summary>
        public const string ARGS_KEY = "__Args";
        /// <summary>the key used to access the method-signature property in messages</summary>
        public const string METHOD_SIG_KEY = "__MethodSignature";

        #endregion Constants
        #region IFields
        
        private Hashtable m_properties = new Hashtable();

        #endregion IFields
        #region IProperties
    
        #region Implementation of IMessage
        
        public IDictionary Properties {
            get {
                return m_properties;
            }
        }
        
        #endregion

        #endregion IProperties
        #region SMethods
        
        /// <summary>
        /// helper method, which sets the service context inside the message
        /// </summary>
        internal static void SetServiceContextInMessage(IMessage msg, ServiceContextList svcContext) {
            msg.Properties[SimpleGiopMsg.SERVICE_CONTEXT] = svcContext;                        
        }

        /// <summary>
        /// helper method, which gets the service context from the message
        /// </summary>        
        internal static ServiceContextList GetServiceContextFromMessage(IMessage msg) {
            return (ServiceContextList)msg.Properties[SimpleGiopMsg.SERVICE_CONTEXT];
        }
        
        /// <summary>
        /// helper method to the the interception flow  inside a message
        /// </summary>
        internal static void SetInterceptionFlow(IMessage msg, RequestInterceptionFlow flow) {
            msg.Properties[SimpleGiopMsg.INTERCEPTION_FLOW] = flow;
        }
        
        /// <summary>
        /// helper method to extract the interception flow from inside a message
        /// </summary>
        internal static RequestInterceptionFlow GetInterceptionFlow(IMessage msg) {
            return (RequestInterceptionFlow)msg.Properties[SimpleGiopMsg.INTERCEPTION_FLOW];
        }
        
        /// <summary>
        /// helper method to extract the pi current from inside a message
        /// </summary>
        internal static PICurrentImpl GetPICurrent(IMessage msg) {
            return (PICurrentImpl)msg.Properties[SimpleGiopMsg.PI_CURRENT_SLOTS];
        }
        
        /// <summary>
        /// helper method to set the picurrent inside a message.
        /// </summary>
        internal static void SetPICurrent(IMessage msg, PICurrentImpl current) {
            msg.Properties[SimpleGiopMsg.PI_CURRENT_SLOTS] = current;
        }                

        /// <summary>
        /// helper method, which sets the is_async to true for message
        /// </summary>
        internal static void SetMessageAsyncRequest(IMessage msg) {
            msg.Properties[SimpleGiopMsg.IS_ASYNC_REQUEST] = true;
        }                        
        
        /// <summary>
        /// helper method, which gets the is_async for the message
        /// </summary>
        internal static bool IsMessageAsyncRequest(IMessage msg) {
            if (msg.Properties.Contains(SimpleGiopMsg.IS_ASYNC_REQUEST)) {
                return (bool)msg.Properties[SimpleGiopMsg.IS_ASYNC_REQUEST];
            } else {
                return false;
            }
        }
        
        #endregion SMethods        

    }
    
    /// <summary>contains the information for a location forward reply</summary>
    internal class LocationForwardMessage : IMessage {
        
        #region Constants
        
        internal const string FWD_PROXY_KEY = "__FwdToProxy";
        
        #endregion Constants
        #region IFields        
        
        private IDictionary m_properties = new Hashtable();
        
        #endregion IFields
        #region IConstructors
        
        internal LocationForwardMessage(MarshalByRefObject toProxy) {
            m_properties[FWD_PROXY_KEY] = toProxy;            
        }
        
        #endregion IConstructors
        #region IProperties
        
        public IDictionary Properties {
            get {
                return m_properties;
            }
        }
        
        internal MarshalByRefObject FwdToProxy {
            get {
                return (MarshalByRefObject)m_properties[FWD_PROXY_KEY];
            }
        }
        
        #endregion IProperties
        
    }


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

        private MarshallerForType m_contextSeqMarshaller;

        #endregion IFields
        #region IConstructors

        internal GiopMessageBodySerialiser() {            
            m_contextSeqMarshaller = new MarshallerForType(typeof(string[]), 
                                        new AttributeExtCollection(new Attribute[] { new IdlSequenceAttribute(0L),
                                                                                     new StringValueAttribute(),
                                                                                     new WideCharAttribute(false) }));
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
                    object codeSetComponent = CodeSetService.FindCodeSetComponent(targetProfile);
                    if (codeSetComponent != null) {
                        int charSet = CodeSetService.ChooseCharSet((CodeSetComponentData)codeSetComponent);
                        int wcharSet = CodeSetService.ChooseWCharSet((CodeSetComponentData)codeSetComponent);
                        conDesc.SetNegotiatedCodeSets(charSet, wcharSet);
                    } else {
                        conDesc.SetCodeSetNegotiated();
                    }                    
                    Ch.Elca.Iiop.Services.CodeSetService.InsertCodeSetServiceContext(cntxColl,
                                                                                     conDesc.CharSet, 
                                                                                     conDesc.WCharSet);
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
            cdrStream.CharSet = conDesc.CharSet;
            cdrStream.WCharSet = conDesc.WCharSet;
        }

        /// <summary>
        /// set the codesets for the stream after codeset service descision
        /// </summary>
        /// <param name="cdrStream"></param>
        private void SetCodeSet(CdrOutputStream cdrStream, GiopConnectionDesc conDesc) {            
            cdrStream.CharSet = conDesc.CharSet;
            cdrStream.WCharSet = conDesc.WCharSet;
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
            return IiopUrlUtil.GetObjectUriForObjectKey(objectKey);
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
                SerialiseRequestBody(targetStream, clientRequest, version);                
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

        private void SerialiseContextElements(CdrOutputStream targetStream, MethodInfo methodToCall,
                                              LogicalCallContext callContext) {
            AttributeExtCollection methodAttrs =
                ReflectionHelper.GetCustomAttriutesForMethod(methodToCall, true,
                                                             ReflectionHelper.ContextElementAttributeType);
            if (methodAttrs.Count > 0) {
                string[] contextSeq = new string[methodAttrs.Count * 2];
                for (int i = 0; i < methodAttrs.Count; i++) {
                    string contextKey =
                        ((ContextElementAttribute)methodAttrs.GetAttributeAt(i)).ContextElementKey;
                    contextSeq[i * 2] = contextKey;
                    if (callContext.GetData(contextKey) != null) {
                        contextSeq[i * 2 + 1] = callContext.GetData(contextKey).ToString();
                    } else {
                        contextSeq[i * 2 + 1] = "";
                    }
                }
                m_contextSeqMarshaller.Marshal(contextSeq, targetStream);
            }
        }

        /// <summary>serializes the request body</summary>
        /// <param name="targetStream"></param>
        /// <param name="clientRequest">the request to serialise</param>
        /// <param name="version">the GIOP-version</param>
        private void SerialiseRequestBody(CdrOutputStream targetStream, GiopClientRequest clientRequest,
                                          GiopVersion version) {
            // body of request msg: serialize arguments
            // clarification from CORBA 2.6, chapter 15.4.1: no padding, when no arguments are serialised  -->
            // for backward compatibility, do it nevertheless
            AlignBodyIfNeeded(targetStream, version);
            ParameterMarshaller marshaller = ParameterMarshaller.GetSingleton();
            marshaller.SerialiseRequestArgs(clientRequest.MethodToCall, clientRequest.RequestArguments, 
                                            targetStream);
            // check for context elements
            SerialiseContextElements(targetStream, clientRequest.MethodToCall,
                                     clientRequest.RequestCallContext);
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
                
                IDictionary contextElements;
                serverRequest.ResolveCall(); // determine the .net target method
                DeserialiseRequestBody(cdrStream, version, serverRequest, out contextElements);                
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

        private IDictionary DeserialseContextElements(CdrInputStream cdrStream, AttributeExtCollection contextElemAttrs) {
            IDictionary result = new HybridDictionary();
            string[] contextElems = (string[])m_contextSeqMarshaller.Unmarshal(cdrStream);
            if (contextElems.Length % 2 != 0) {
                throw new MARSHAL(67, CompletionStatus.Completed_No);
            }
            for (int i = 0; i < contextElems.Length; i += 2) {
                string contextElemKey = contextElems[i];
                // insert into call context, if part of signature
                foreach (ContextElementAttribute attr in contextElemAttrs) {
                    if (attr.ContextElementKey == contextElemKey) {
                        result[contextElemKey] = contextElems[i + 1];
                        break;
                    }
                }
            }
            return result;
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
                                            out IDictionary contextElements) {
            // clarification from CORBA 2.6, chapter 15.4.1: no padding, when no arguments/no context elements
            // are serialised, i.e. body empty            
            // ignores, if not enough bytes because no args/context; because in this case, no more bytes follow
            TryAlignBodyIfNeeded(cdrStream, version);

            // unmarshall parameters
            ParameterMarshaller paramMarshaller = ParameterMarshaller.GetSingleton();            
            bool hasRequestArgs = paramMarshaller.HasRequestArgs(request.CalledMethod);
            AttributeExtCollection methodAttrs =
                ReflectionHelper.GetCustomAttriutesForMethod(request.CalledMethod, true,
                                                             ReflectionHelper.ContextElementAttributeType);
            object[] args;
            contextElements = null;
            if (hasRequestArgs) {
                args = paramMarshaller.DeserialiseRequestArgs(request.CalledMethod, cdrStream);
            } else {
                args = new object[request.CalledMethod.GetParameters().Length];
            }
            if (methodAttrs.Count > 0) {
                contextElements = DeserialseContextElements(cdrStream, methodAttrs);
            }            
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
            ParameterMarshaller marshaller = ParameterMarshaller.GetSingleton();
            marshaller.SerialiseResponseArgs(request.CalledMethod, 
                                             request.ReturnValue, request.OutArgs, targetStream);            
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
            
            Marshaller marshaller = Marshaller.GetSingleton();
            marshaller.Marshal(corbaEx.GetType(), Util.AttributeExtCollection.EmptyCollection,
                               corbaEx, targetStream);
        }

        private void SerialiseUserException(CdrOutputStream targetStream, AbstractUserException userEx) {            
            Type exceptionType = userEx.GetType();            
            // marshal the exception
            Marshaller marshaller = Marshaller.GetSingleton();
            marshaller.Marshal(exceptionType, Util.AttributeExtCollection.EmptyCollection,
                               userEx, targetStream);
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
                        // LOCATION_FORWARD:
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
            MethodInfo targetMethod = request.MethodToCall;
            ParameterMarshaller paramMarshaller = ParameterMarshaller.GetSingleton();
            // body
            // clarification from CORBA 2.6, chapter 15.4.2: no padding, when no arguments are serialised
            TryAlignBodyIfNeeded(cdrStream, version); // read alignement, if present
            
            // read the parameters                            
            object[] outArgs;
            object retVal = null;

            retVal = paramMarshaller.DeserialiseResponseArgs(targetMethod, cdrStream, out outArgs);
            ReturnMessage response = new ReturnMessage(retVal, outArgs, outArgs.Length, null, request.Request);
            return response;
        }

        /// <summary>deserialises a CORBA system exception </summary>
        private Exception DeserialiseSystemError(CdrInputStream cdrStream, GiopVersion version) {
            // exception body
            AlignBodyIfNeeded(cdrStream, version);

            Marshaller marshaller = Marshaller.GetSingleton();
            Exception result = (Exception) marshaller.Unmarshal(typeof(omg.org.CORBA.AbstractCORBASystemException),
                                                                Util.AttributeExtCollection.EmptyCollection,
                                                                cdrStream);
            
            if (result == null) { 
                return new Exception("received system error from peer orb, but error was not deserializable");
            } else {
                return result;
            }
        }

        private Exception DeserialiseUserException(CdrInputStream cdrStream, GiopVersion version) {
            // exception body
            AlignBodyIfNeeded(cdrStream, version);

            Marshaller marshaller = Marshaller.GetSingleton();
            Exception result = (Exception) marshaller.Unmarshal(typeof(AbstractUserException),
                                                                Util.AttributeExtCollection.EmptyCollection,
                                                                cdrStream);
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
            Marshaller marshaller = Marshaller.GetSingleton();
            MarshalByRefObject newProxy = marshaller.Unmarshal(request.MethodToCall.DeclaringType, 
                                                               AttributeExtCollection.EmptyCollection, cdrStream)
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
        public string DeserialiseLocateRequest(CdrInputStream cdrStream, GiopVersion version, out uint forRequestId) {
            forRequestId = cdrStream.ReadULong();
            byte[] objectKey;
            return ReadTarget(cdrStream, version, out objectKey);
        }

        /// <summary>
        /// serialises a locate reply message.
        /// </summary>
        /// <param name="forwardAddr">
        /// specifies the IOR of the object to forward the call to. This parameter must be != null,
        /// if LocateStatus is OBJECT_FORWARD.
        ///  </param>
        public void SerialiseLocateReply(CdrOutputStream targetStream, GiopVersion version, uint forRequestId, 
                                         LocateStatus status, Ior forward) {
            targetStream.WriteULong(forRequestId);
            switch (status) {
                case LocateStatus.OBJECT_HERE:
                    targetStream.WriteULong(1);
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
    

    /// <summary>
    /// Unit-tests for testing request/reply serialisation/deserialisation
    /// </summary>
    public class MessageBodySerialiserTest : TestCase {
        
        public void TestSameServiceIdMultiple() {
            // checks if service contexts with the same id, doesn't throw an exception
            // checks, that the first service context is considered, others are thrown away
            GiopMessageBodySerialiser ser = new GiopMessageBodySerialiser();
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
            Assertion.Assert("expected context not in collection", result.ContainsServiceContext(1234567) == true);
        }        
                
    }
    
}

#endif
