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
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.MessageHandling {

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
        /// <summary>the key used to access the uri-property in messages</summary>
        public const string URI_KEY = "__Uri";
        /// <summary>the key used to access the typename-property in messages</summary>
        public const string TYPENAME_KEY = "__TypeName";
        /// <summary>the key used to access the methodname-property in messages</summary>
        public const string METHODNAME_KEY = "__MethodName";
        /// <summary>the key used to access the argument-property in messages</summary>
        public const string ARGS_KEY = "__Args";

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

        #endregion IFields
        #region IConstructors
        
        /// <param name="reason">the reason for deserialization error</param>
        /// <param name="requestMessage">the message decoded so far</param>
        public RequestDeserializationException(Exception reason, IMessage requestMessage) {
            m_reason = reason;
            m_requestMessage = requestMessage;
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

        #endregion IProperties

    }

    
    /// <summary>
    /// This class is reponsible for serialising/deserialising message bodys of Giop Messages for
    /// the different message types
    /// </summary>
    public class GiopMessageBodySerialiser {

        #region SFields

        private static GiopMessageBodySerialiser s_singleton = new GiopMessageBodySerialiser();

        #endregion SFields
        #region SMethods

        public static GiopMessageBodySerialiser GetSingleton() {
            return s_singleton;
        }

        #endregion SMethods
        #region IMethods
        
        #region Common

        protected void SerialiseContext(CdrOutputStream targetStream, ServiceContextCollection cntxColl) {
            IEnumerator enumerator = cntxColl.GetEnumerator();
            targetStream.WriteULong((uint)cntxColl.Count); // nr of service contexts
            while (enumerator.MoveNext()) {
                ServiceContext cntx = (ServiceContext) enumerator.Current;
                cntx.Serialize(targetStream);    
            }
        }

        protected ServiceContextCollection DeserialiseContext(CdrInputStream sourceStream) {
            ServiceContextCollection cntxColl = new ServiceContextCollection();
            CosServices services = CosServices.GetSingleton();
            uint nrOfContexts = sourceStream.ReadULong();
            for (uint i = 0; i < nrOfContexts; i++) {
                uint serviceId = sourceStream.ReadULong();
                CorbaService service = services.GetForServiceId(serviceId);
                CdrEncapsulationInputStream serviceData = sourceStream.ReadEncapsulation();
                ServiceContext cntx = service.DeserialiseContext(serviceData);
                cntxColl.AddServiceContext(cntx);
            }
            return cntxColl;
        }

        protected void AlignBodyIfNeeded(CdrInputStream cdrStream, GiopVersion version) {
            if ((version.Major == 1) && (version.Minor >= 2)) {
                cdrStream.ForceReadAlign(Aligns.Align8);
            } // force an align on 8 for GIOP-version >= 1.2
        }

        protected void AlignBodyIfNeeded(CdrOutputStream cdrStream, GiopVersion version) {
            if ((version.Major == 1) && (version.Minor >= 2)) { 
                cdrStream.ForceWriteAlign(Aligns.Align8); 
            } // force an align on 8 for GIOP-version >= 1.2
        }

        /// <summary>checks if this it's a one way message</summary>
        protected bool IsOneWayCall(IMethodCallMessage msg) {
            Util.AttributeExtCollection attrs = Util.AttributeExtCollection.ConvertToAttributeCollection(msg.MethodBase.GetCustomAttributes(true));
            if (attrs.IsInCollection(typeof(OneWayAttribute))) { 
                return true; 
            } else { 
                return false; 
            }
        }


        /// <summary>
        /// set the codesets for the stream after codeset service descision
        /// </summary>
        /// <param name="cdrStream"></param>
        private void SetCodeSet(CdrInputStream cdrStream) {
            GiopConnectionContext context = IiopConnectionManager.GetCurrentConnectionContext();
            cdrStream.CharSet = context.CharSet;
            cdrStream.WCharSet = context.WCharSet;
        }

        /// <summary>
        /// set the codesets for the stream after codeset service descision
        /// </summary>
        /// <param name="cdrStream"></param>
        private void SetCodeSet(CdrOutputStream cdrStream) {
            GiopConnectionContext context = IiopConnectionManager.GetCurrentConnectionContext();
            cdrStream.CharSet = context.CharSet;
            cdrStream.WCharSet = context.WCharSet;
        }

        #endregion Common
        #region Requests

        private void WriteTarget(CdrOutputStream cdrStream, 
                                 byte[] objectKey, GiopVersion version) {
            if (!((version.Major == 1) && (version.Minor <= 1))) {
                // for GIOP >= 1.2
                uint targetAdrType = 0;
                cdrStream.WriteULong(targetAdrType); // object key adressing
            }
            WriteTargetKey(cdrStream, objectKey);
        }

        private void WriteTargetKey(CdrOutputStream cdrStream, byte[] objectKey) {
            Debug.WriteLine("writing object key with length: " + objectKey.Length);
            cdrStream.WriteULong((uint)objectKey.Length); // object-key length
            cdrStream.WriteOpaque(objectKey);
        }

        /// <summary>read the target for the request</summary>
        /// <returns>the objectURI extracted from this msg</returns>
        private string ReadTarget(CdrInputStream cdrStream, GiopVersion version) {
            if ((version.Major == 1) && (version.Minor <= 1)) { 
                // for GIOP 1.0 / 1.1 only object key is possible
                return ReadTargetKey(cdrStream, version);
            }
            
            // for GIOP >= 1.2, a union is used for target information
            ulong targetAdrType = cdrStream.ReadULong();
            switch (targetAdrType) {
                case 0:
                    return ReadTargetKey(cdrStream, version);
                default:
                    throw new NotSupportedException("target address type not supported: " + targetAdrType);
            }
        }

        private string ReadTargetKey(CdrInputStream cdrStream, GiopVersion version) {
            uint length = cdrStream.ReadULong();
            Debug.WriteLine("object key follows:");
            byte[] objectKey = cdrStream.ReadOpaque((int)length);
                    
            // get the object-URI of the responsible object
            return IiopUrlUtil.GetObjUriForObjectInfo(objectKey, version);
        }

        /// <summary>aquire the information for a specific object method call</summary>
        /// <param name="serverType">the type of the object called</param>
        /// <param name="calledMethodInfo">the MethodInfo of the method, which is called</param>
        /// <returns>returns the mapped methodName of the operation to call of this object specific method</returns>
        private string DecodeObjectOperation(string objectUri, string methodName, out Type serverType,
                                             out MethodInfo calledMethodInfo) {
            serverType = RemotingServices.GetServerTypeForUri(objectUri);
            if (serverType == null) { 
                throw new OBJECT_NOT_EXIST(0, CompletionStatus.Completed_No); 
            }
            string resultMethodName = IdlNaming.MapIdlMethodNameToClsName(methodName, serverType);
            calledMethodInfo = serverType.GetMethod(resultMethodName);
            if (calledMethodInfo == null) { 
                throw new BAD_OPERATION(0, CompletionStatus.Completed_No); 
            }
            return resultMethodName;
        }

        /// <summary>
        /// aquire the information needed to call a standard corba operation, which is possible for every object
        /// </summary>
        /// <param name="serverType">the type of the object called</param>
        /// <param name="calledMethodInfo">the MethodInfo of the method, which is called</param>
        /// <returns>the method-name of the method implementing the operation</returns>
        private string DecodeStandardOperation(string objectUri, string methodName, out Type serverType,
                                               out MethodInfo calledMethodInfo) {
            serverType = typeof(StandardCorbaOps); // generic handler
            string resultMethodName = StandardCorbaOps.MapMethodName(methodName);
            calledMethodInfo = serverType.GetMethod(methodName); // for parameter unmarshalling, use info of the signature method
            if (calledMethodInfo == null) { 
                // unexpected exception: can't load method of type StandardCorbaOps
                throw new INTERNAL(2801, CompletionStatus.Completed_MayBe);
            }
            return resultMethodName;
        }

        private object[] AdaptArgsForStandardOp(object[] args, string objectUri) {
            object[] result = new object[args.Length+1];
            result[0] = objectUri; // this argument is passed to all standard operations
            Array.Copy((Array)args, 0, result, 1, args.Length);
            return result;
        }

        /// <summary>
        /// serialises the message body for a GIOP request
        /// </summary>
        /// <param name="methodCall">the .NET remoting request Msg</param>
        /// <param name="targetStream"></param>
        /// <param name="version">the Giop version to use</param>
        /// <param name="reqId">the request-id to use</param>
        public void SerialiseRequest(IMethodCallMessage methodCall,
                                     CdrOutputStream targetStream, 
                                     GiopVersion version, uint reqId) {

            string uri = methodCall.Uri;
            // method-call uri is normally a full url, but can also be only the object-uri part
            if (IiopUrlUtil.IsUrl(uri)) {
                IiopUrlUtil.ParseUrl(uri, out uri);                
            } 
            byte[] objectKey = IiopUrlUtil.GetObjectKeyForObjUri(uri);
            Debug.WriteLine("serializing request for id: " + reqId);
            Debug.WriteLine("uri: " + uri);

            ServiceContextCollection cntxColl = CosServices.GetSingleton().
                                                    InformInterceptorsRequestToSend();
            if ((version.Major == 1) && (version.Minor <= 1)) { // for GIOP 1.0 / 1.1
                SerialiseContext(targetStream, cntxColl); // service context                
            }

            targetStream.WriteULong(reqId);
            byte responseFlags = 0;
            if ((version.Major == 1) && (version.Minor <= 1)) { // GIOP 1.0 / 1.1
                responseFlags = 1;
            } else {
                // reply-expected, no DII-call --> must be 0x03, no reply --> must be 0x00
                responseFlags = 3;
            }
            if (IsOneWayCall(methodCall)) { 
                responseFlags = 0; 
            } // check if one-way
            // write response-flags
            targetStream.WriteOctet(responseFlags); 
                        
            targetStream.WritePadding(3); // reserved bytes
            WriteTarget(targetStream, objectKey, version); // write the target-info
            targetStream.WriteString(IdlNaming.MapClsMethodNameToIdlName(methodCall.MethodName,
                                                                         Type.GetType(methodCall.TypeName))); // write the method name
            
            if ((version.Major == 1) && (version.Minor <= 1)) { // GIOP 1.0 / 1.1
                targetStream.WriteULong(0); // no principal
            } else { // GIOP 1.2
                SerialiseContext(targetStream, cntxColl); // service context
            }
            SerialiseRequestBody(targetStream, methodCall.Args, (MethodInfo)methodCall.MethodBase, version);
        }

        /// <summary>serializes the request body</summary>
        /// <param name="targetStream"></param>
        /// <param name="callArgs">the arguments for this methodcall</param>
        /// <param name="methodToCall">the MethodInfo reflection-info for the method which should be called</param>
        /// <param name="version">the GIOP-version</param>
        private void SerialiseRequestBody(CdrOutputStream targetStream, object[] callArgs,
                                          MethodInfo methodToCall, GiopVersion version) {
            // body of request msg: serialize arguments
            AlignBodyIfNeeded(targetStream, version);
            ParameterMarshaller marshaller = ParameterMarshaller.GetSingleton();
            marshaller.SerialiseRequestArgs(methodToCall, callArgs, targetStream);
        }

        /// <summary>
        /// Deserialises the Giop Message body for a request
        /// </summary>
        /// <param name="cdrStream"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public IMessage DeserialiseRequest(CdrInputStream cdrStream, GiopVersion version) {
            SimpleGiopMsg msg = new SimpleGiopMsg();
            msg.Properties.Add(SimpleGiopMsg.GIOP_VERSION_KEY, version);
            try {
                if ((version.Major == 1) && (version.Minor <= 1)) { // GIOP 1.0 / 1.1
                    ServiceContextCollection coll = DeserialiseContext(cdrStream); // Service context deser
                    CosServices.GetSingleton().InformInterceptorsReceivedRequest(coll);
                }
                
                // read the request-ID and set it as a message property
                uint requestId = cdrStream.ReadULong(); 
                msg.Properties.Add(SimpleGiopMsg.REQUEST_ID_KEY, requestId);
                Trace.WriteLine("received a message with reqId: " + requestId);
                // read response-flags:
                byte respFlags = cdrStream.ReadOctet(); Debug.WriteLine("response-flags: " + respFlags);
                cdrStream.ReadPadding(3); // read reserved bytes
                msg.Properties.Add(SimpleGiopMsg.RESPONSE_FLAGS_KEY, respFlags);
                
                // decode the target of this request
                string objectUri = ReadTarget(cdrStream, version);
                string methodName = cdrStream.ReadString();
                Trace.WriteLine("call for .NET object: " + objectUri + ", methodName: " + methodName);

                if ((version.Major == 1) && (version.Minor <= 1)) { // GIOP 1.0 / 1.1
                    uint principalLength = cdrStream.ReadULong();
                    cdrStream.ReadOpaque((int)principalLength);
                } else {
                    ServiceContextCollection coll = DeserialiseContext(cdrStream); // Service context deser
                    CosServices.GetSingleton().InformInterceptorsReceivedRequest(coll);
                }
                // set codeset for stream
                SetCodeSet(cdrStream);
                // request header deserialised

                string calledUri = objectUri; // store received object-uri
                Type serverType;
                MethodInfo calledMethodInfo;
                bool standardOp = false;
                if (!StandardCorbaOps.CheckIfStandardOp(methodName)) {
                    // handle object specific-ops
                    methodName = DecodeObjectOperation(objectUri, methodName, 
                                                       out serverType, out calledMethodInfo);
                } else {
                    // handle standard corba-ops like _is_a
                    methodName = DecodeStandardOperation(objectUri, methodName, 
                                                         out serverType, out calledMethodInfo);
                    objectUri = StandardCorbaOps.WELLKNOWN_URI; // change object-uri
                    standardOp = true;
                }
                msg.Properties.Add(SimpleGiopMsg.URI_KEY, objectUri);
                msg.Properties.Add(SimpleGiopMsg.TYPENAME_KEY, serverType.FullName);
                msg.Properties.Add(SimpleGiopMsg.METHODNAME_KEY, methodName);
                
                // deserialise the body of this request
                object[] args = DeserialiseRequestBody(cdrStream, calledMethodInfo,     
                                                       standardOp, calledUri, version);
                msg.Properties.Add(SimpleGiopMsg.ARGS_KEY, args);
                MethodCall methodCallInfo = new MethodCall(msg);
                return methodCallInfo;
            } catch (Exception e) {
                // an Exception encountered during deserialisation
                cdrStream.SkipRest(); // skip rest of the message, to not corrupt the stream
                throw new RequestDeserializationException(e, msg);
            }
        }

        /// <summary>deserialise the request body</summary>
        /// <returns>the deserialized arguments</returns>
        private object[] DeserialiseRequestBody(CdrInputStream cdrStream, MethodInfo calledMethodInfo,
                                                bool isStandardOp, string calledUri, GiopVersion version) {
            // unmarshall parameters
            AlignBodyIfNeeded(cdrStream, version);
            ParameterMarshaller paramMarshaller = ParameterMarshaller.GetSingleton();
            object[] args = paramMarshaller.DeserialiseRequestArgs(calledMethodInfo, cdrStream);

            // for standard corba ops, adapt args:
            if (isStandardOp) {
                args = AdaptArgsForStandardOp(args, calledUri);
            }                        
            return args;
        }

        #endregion Requests
        #region Replys

        /// <summary>serialize the GIOP message body of a repsonse message</summary>
        /// <param name="requestId">the requestId of the request, this response belongs to</param>
        public void SerialiseReply(ReturnMessage msg, CdrOutputStream targetStream, 
                                   GiopVersion version, uint requestId) {
            Trace.WriteLine("serializing response for method: " + msg.MethodName);
            
            ServiceContextCollection cntxColl = CosServices.GetSingleton().
                                                    InformInterceptorsReplyToSend();
            if ((version.Major == 1) && (version.Minor <= 1)) { // for GIOP 1.0 / 1.1
                SerialiseContext(targetStream, cntxColl); // serialize the context
            }
            
            targetStream.WriteULong(requestId);

            if (msg.Exception == null) { 
                targetStream.WriteULong(0); // reply status ok
                
                if (!((version.Major == 1) && (version.Minor <= 1))) { // for GIOP 1.2 and later, service context is here
                    SerialiseContext(targetStream, cntxColl); // serialize the context                
                }
                // serialize a response to a successful request
                SerialiseResponseOk(targetStream, msg, version);
            } else {
                Trace.WriteLine("sending exceptin to client: " + msg.Exception.GetType());
                if (SerialiseAsSystemException(msg.Exception)) {
                    targetStream.WriteULong(2); // system exception
                } else {
                    targetStream.WriteULong(1); // user exception
                }

                if (!((version.Major == 1) && (version.Minor <= 1))) { // for GIOP 1.2 and later, service context is here
                    SerialiseContext(targetStream, cntxColl); // serialize the context                
                }
                SerialiseResponseException(targetStream, msg, version);
            }
        }

        private void SerialiseResponseOk(CdrOutputStream targetStream, ReturnMessage msg,
                                         GiopVersion version) {
            // reply body
            AlignBodyIfNeeded(targetStream, version);
            // marshal the parameters
            ParameterMarshaller marshaller = ParameterMarshaller.GetSingleton();
            marshaller.SerialiseResponseArgs((MethodInfo)msg.MethodBase, 
                                             msg.ReturnValue, msg.OutArgs, targetStream);            
        }

        /// <summary>serialize the exception as a CORBA System exception</summary>
        private bool SerialiseAsSystemException(Exception e) {
            return (e is omg.org.CORBA.AbstractCORBASystemException);
        }

        /// <summary>serialize an exception</summary>
        /// <param name="targetStream"></param>
        /// <param name="msg"></param>
        /// <param name="version"></param>
        private void SerialiseResponseException(CdrOutputStream targetStream, ReturnMessage msg,
                                                GiopVersion version) {
            // reply body
            AlignBodyIfNeeded(targetStream, version);
            // marshal the exception, TBD distinguish some cases here
            if (SerialiseAsSystemException(msg.Exception)) {
                SerialiseSystemException(targetStream, msg.Exception);
            } else {
                SerialiseUserException(targetStream, msg.Exception);
            }
        }

        private void SerialiseSystemException(CdrOutputStream targetStream, Exception corbaEx) {
            // serialize a system exception
            if (!(corbaEx is AbstractCORBASystemException)) {
                corbaEx = new UNKNOWN(0, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            
            Marshaller marshaller = Marshaller.GetSingleton();
            marshaller.Marshal(corbaEx.GetType(), new Util.AttributeExtCollection(new Attribute[0]),
                               corbaEx, targetStream);
        }

        private void SerialiseUserException(CdrOutputStream targetStream, Exception userEx) {
            // map to a generic User-Exception, if not an Exception created by the IDL to CLS mapping
            Type exceptionType;
            AbstractUserException toSerialise;
            if (userEx is AbstractUserException) {
                exceptionType = userEx.GetType();
                toSerialise = (AbstractUserException) userEx;
            } else {
                toSerialise = new GenericUserException(userEx);
                exceptionType = toSerialise.GetType();
            }
            // marshal the exception
            Marshaller marshaller = Marshaller.GetSingleton();
            marshaller.Marshal(exceptionType, new Util.AttributeExtCollection(new Attribute[0]),
                               toSerialise, targetStream);
        }


        public IMessage DeserialiseReply(CdrInputStream cdrStream, 
                                         GiopVersion version, IMethodCallMessage methodCall) {
            if ((version.Major == 1) && (version.Minor <= 1)) { // for GIOP 1.0 / 1.1, the service context is placed here
                ServiceContextCollection coll = DeserialiseContext(cdrStream); // deserialize the service contexts
                CosServices.GetSingleton().InformInterceptorsReceivedReply(coll);
            }
            
            uint forRequestId = cdrStream.ReadULong();
            uint responseStatus = cdrStream.ReadULong();
            if (!((version.Major == 1) && (version.Minor <= 1))) { // for GIOP 1.2 and later, service context is here
                ServiceContextCollection coll = DeserialiseContext(cdrStream); // deserialize the service contexts
                CosServices.GetSingleton().InformInterceptorsReceivedReply(coll);
            }

            IMessage response = null;
            try {
                switch (responseStatus) {
                    case 0 : 
                        response = DeserialiseNormal(cdrStream, version, methodCall); break;
                    case 1 : 
                        throw DeserialiseUserException(cdrStream, version); // the error .NET message for this exception is created in the formatter
                    case 2 : 
                        throw DeserialiseSystemError(cdrStream, version); // the error .NET message for this exception is created in the formatter
                    default : 
                        // deseralization of reply error, unknown reply status: responseStatus
                        // the error .NET message for this exception is created in the formatter
                        throw new MARSHAL(2401, CompletionStatus.Completed_MayBe);
                }
            } catch (Exception e) {
                // do not corrupt stream --> skip
                cdrStream.SkipRest();
                throw e;
            }

            return response;
        }

        /// <summary>deserialize response with ok-status.</summary>
        private IMessage DeserialiseNormal(CdrInputStream cdrStream, GiopVersion version, 
                                           IMethodCallMessage methodCall) {
            MethodInfo targetMethod = (MethodInfo)methodCall.MethodBase;
            
            // body
            AlignBodyIfNeeded(cdrStream, version);
            // read the parameters
            ParameterMarshaller marshaller = ParameterMarshaller.GetSingleton();
            object[] outArgs;
            object retVal = marshaller.DeserialiseResponseArgs(targetMethod, cdrStream, out outArgs);
            ReturnMessage response = new ReturnMessage(retVal, outArgs, outArgs.Length, null, methodCall);
            LogicalCallContext dnCntx = response.LogicalCallContext;
            // TODO: fill in .NET context ...
            return response;
        }

        /// <summary>deserialises a CORBA system exception </summary>
        private Exception DeserialiseSystemError(CdrInputStream cdrStream, GiopVersion version) {
            // exception body
            AlignBodyIfNeeded(cdrStream, version);

            Marshaller marshaller = Marshaller.GetSingleton();
            Exception result = (Exception) marshaller.Unmarshal(typeof(omg.org.CORBA.AbstractCORBASystemException),
                                                                new Util.AttributeExtCollection(new Attribute[0]),
                                                                cdrStream);
            
            if (result == null) { 
                throw new Exception("received system error from peer orb, but error was not deserializable");
            } else {
                throw result;
            }
        }

        private Exception DeserialiseUserException(CdrInputStream cdrStream, GiopVersion version) {
            // exception body
            AlignBodyIfNeeded(cdrStream, version);

            Marshaller marshaller = Marshaller.GetSingleton();
            Exception result = (Exception) marshaller.Unmarshal(typeof(AbstractUserException),
                                                                new Util.AttributeExtCollection(new Attribute[0]),
                                                                cdrStream);
            if (result == null) {
                throw new Exception("user exception received from peer orb, but was not deserializable");
            } else {
                throw result;
            }
        }

        #endregion Replys
        
        #endregion IMethods

    }

}
