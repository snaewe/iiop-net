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
using Ch.Elca.Iiop.CorbaObjRef;
using omg.org.CORBA;

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
               
        private static Type s_iIdlEntityType = typeof(IIdlEntity);
        
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
                // add the service context if not already present. 
                // Important: Don't throw an exception if already present,
                // because WAS4.0.4 includes more than one with same id.
                if (!cntxColl.ContainsContextForService(cntx.ServiceID)) {
                    cntxColl.AddServiceContext(cntx);
                }
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

        /// <summary>read the target for the request</summary>
        /// <returns>the objectURI extracted from this msg</returns>
        private string ReadTarget(CdrInputStream cdrStream, GiopVersion version) {
            if ((version.Major == 1) && (version.Minor <= 1)) { 
                // for GIOP 1.0 / 1.1 only object key is possible
                return ReadTargetKey(cdrStream);
            }
            
            // for GIOP >= 1.2, a union is used for target information
            uint targetAdrType = cdrStream.ReadULong();
            switch (targetAdrType) {
                case 0:
                    return ReadTargetKey(cdrStream);
                default:
                    throw new NotSupportedException("target address type not supported: " + targetAdrType);
            }
        }

        private string ReadTargetKey(CdrInputStream cdrStream) {
            uint length = cdrStream.ReadULong();
            Debug.WriteLine("object key follows:");
            byte[] objectKey = cdrStream.ReadOpaque((int)length);
                    
            // get the object-URI of the responsible object
            return IiopUrlUtil.GetObjUriForObjectKey(objectKey);
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
       
        /// <summary>aquire the information for a specific object method call</summary>
        /// <param name="serverType">the type of the object called</param>
        /// <param name="calledMethodInfo">the MethodInfo of the method, which is called</param>
        /// <returns>returns the mapped methodName of the operation to call of this object specific method</returns>
        private MethodInfo DecodeObjectOperation(string methodName, Type serverType) {
            // method name mapping
            string resultMethodName;
            if (s_iIdlEntityType.IsAssignableFrom(serverType)) {
                resultMethodName = methodName;
                // an interface mapped to from Idl is implemented by server ->
                // compensate 3.2.3.1: removal of _ for names, which clashes with CLS id's
                if (IdlNaming.NameClashesWithClsKeyWord(methodName)) {
                    resultMethodName = "_" + methodName;
                } else if (methodName.StartsWith("_get_")) {
                    // handle properties correctly
                    PropertyInfo prop = serverType.GetProperty(methodName.Substring(5), BindingFlags.Instance | BindingFlags.Public);
                    if (prop != null) {
                        resultMethodName = prop.GetGetMethod().Name;
                    }
                } else if (methodName.StartsWith("_set_")) {
                    // handle properties correctly
                    PropertyInfo prop = serverType.GetProperty(methodName.Substring(5), BindingFlags.Instance | BindingFlags.Public);
                    if (prop != null) {
                        resultMethodName = prop.GetSetMethod().Name;
                    }
                }
            } else {                
                resultMethodName = IdlNaming.ReverseClsToIdlNameMapping(methodName);
                if (resultMethodName.StartsWith("get_") || resultMethodName.StartsWith("set_")) {
                    // special handling for properties, because properties with a name, which is transformed on mapping,
                    // need to be specially identified, because porperty name is included in method name.
                    string propName = IdlNaming.ReverseClsToIdlNameMapping(resultMethodName.Substring(4));
                    PropertyInfo prop = serverType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null) {
                        if (resultMethodName.StartsWith("get_")) {
                            resultMethodName = prop.GetGetMethod().Name;
                        } else {
                            resultMethodName = prop.GetSetMethod().Name;
                        }
                    }
                }

            }
            
            MethodInfo calledMethodInfo = serverType.GetMethod(resultMethodName);
            if (calledMethodInfo == null) { 
                // possibly an overloaded method!
                calledMethodInfo = IdlNaming.FindClsMethodForOverloadedMethodIdlName(methodName, serverType);
                if (calledMethodInfo == null) { // not found -> BAD_OPERATION
                    throw new BAD_OPERATION(0, CompletionStatus.Completed_No); 
                }
            }
            return calledMethodInfo;
        }

        /// <summary>
        /// aquire the information needed to call a standard corba operation, which is possible for every object
        /// </summary>
        /// <param name="methodName">the name of the method called</param>
        /// <returns>the method-info of the method which describes signature for deserialisation</returns>
        private MethodInfo DecodeStandardOperation(string methodName) {
            Type serverType = StandardCorbaOps.s_type; // generic handler
            MethodInfo calledMethodInfo = serverType.GetMethod(methodName); // for parameter unmarshalling, use info of the signature method
            if (calledMethodInfo == null) { 
                // unexpected exception: can't load method of type StandardCorbaOps
                throw new INTERNAL(2801, CompletionStatus.Completed_MayBe);
            }
            return calledMethodInfo;
        }

        private object[] AdaptArgsForStandardOp(object[] args, string objectUri) {
            object[] result = new object[args.Length+1];
            result[0] = objectUri; // this argument is passed to all standard operations
            Array.Copy((Array)args, 0, result, 1, args.Length);
            return result;
        }
        
        /// <summary>generate the signature info for the method</summary>
        private Type[] GenerateSigForMethod(MethodInfo method) {
        	ParameterInfo[] parameters = method.GetParameters();
        	Type[] result = new Type[parameters.Length];
        	for (int i = 0; i < parameters.Length; i++) {        		        	  
                result[i] = parameters[i].ParameterType;
        	}
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
                                     Ior targetIor, uint reqId) {                         
            Debug.WriteLine("serializing request for id: " + reqId);
            Debug.WriteLine("uri: " + methodCall.Uri);
            GiopVersion version = targetIor.Version;

            ServiceContextCollection cntxColl = CosServices.GetSingleton().
                                                    InformInterceptorsRequestToSend(methodCall, targetIor, 
                                                                                    IiopConnectionManager.GetCurrentConnectionContext());

            // set code-set for the stream
            SetCodeSet(targetStream);
                        
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
            WriteTarget(targetStream, targetIor.ObjectKey, version); // write the target-info

            Type targetType = Type.GetType(methodCall.TypeName);
            string methodName = IdlNaming.ReverseIdlToClsNameMapping(methodCall.MethodName);
            // check for IIdlEntity, if not -> map first CLS  method name to IDL method name            
            if (!s_iIdlEntityType.IsAssignableFrom(targetType)) {
                // do a CLS to IDL mapping, because .NET server expect this for every client, also for a
                // native .NET client, which uses not CLS -> IDL -> CLS mapping
                bool isOverloaded = RemotingServices.IsMethodOverloaded(methodCall);
                methodName = IdlNaming.MapClsMethodNameToIdlName((MethodInfo)methodCall.MethodBase, isOverloaded);
            }
            targetStream.WriteString(methodName); // write the method name
            
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
            // clarification from CORBA 2.6, chapter 15.4.1: no padding, when no arguments are serialised  -->
            // for backward compatibility, do it nevertheless
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
                Type serverType = RemotingServices.GetServerTypeForUri(objectUri);
                if (serverType == null) { 
                    throw new OBJECT_NOT_EXIST(0, CompletionStatus.Completed_No); 
                }
                MethodInfo calledMethodInfo;
                bool standardOp = false;
                if (!StandardCorbaOps.CheckIfStandardOp(methodName)) {
                    // handle object specific-ops
                    calledMethodInfo = DecodeObjectOperation(methodName, serverType);
                    methodName = calledMethodInfo.Name;
                    // to handle overloads correctly, add signature info:
                    Type[] sig = GenerateSigForMethod(calledMethodInfo);
            	    msg.Properties.Add(SimpleGiopMsg.METHOD_SIG_KEY, sig);
                } else {
                    // handle standard corba-ops like _is_a
                    calledMethodInfo = DecodeStandardOperation(methodName);
                	MethodInfo methodToCall = StandardCorbaOps.GetMethodToCallForStandardMethod(calledMethodInfo.Name);
                    methodName = methodToCall.Name;
                    objectUri = StandardCorbaOps.WELLKNOWN_URI; // change object-uri
                    standardOp = true;
                    serverType = StandardCorbaOps.s_type;
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
            ParameterMarshaller paramMarshaller = ParameterMarshaller.GetSingleton();
            object[] args;
            // clarification from CORBA 2.6, chapter 15.4.1: no padding, when no arguments are serialised            
            if (paramMarshaller.HasRequestArgs(calledMethodInfo)) {
                AlignBodyIfNeeded(cdrStream, version);
                args = paramMarshaller.DeserialiseRequestArgs(calledMethodInfo, cdrStream);
            } else {
                args = new object[0];
                cdrStream.SkipRest(); // ignore paddings, if included
            }            

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
            // set codeset for stream
            SetCodeSet(targetStream);

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
            // clarification form CORBA 2.6, chapter 15.4.2: no padding, when no arguments are serialised  -->
            // for backward compatibility, do it nevertheless
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
            
            // set codeset for stream
            SetCodeSet(cdrStream);
            
            IMessage response = null;
            try {
                switch (responseStatus) {
                    case 0 : 
                        response = DeserialiseNormal(cdrStream, version, methodCall); break;
                    case 1 : 
                        throw DeserialiseUserException(cdrStream, version); // the error .NET message for this exception is created in the formatter
                    case 2 : 
                        throw DeserialiseSystemError(cdrStream, version); // the error .NET message for this exception is created in the formatter
                    case 3 :
                        // LOCATION_FORWARD:
                        // --> reissue request transparentely to another object
                        response = ReissueLocationFwd(cdrStream, methodCall); break;
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
            ParameterMarshaller paramMarshaller = ParameterMarshaller.GetSingleton();
            object[] outArgs;
            object retVal = null;
            // body
            // clarification from CORBA 2.6, chapter 15.4.2: no padding, when no arguments are serialised
            if (paramMarshaller.HasResponseArgs(targetMethod)) {
                AlignBodyIfNeeded(cdrStream, version);
                // read the parameters                            
                retVal = paramMarshaller.DeserialiseResponseArgs(targetMethod, cdrStream, out outArgs);
            } else {
                outArgs = new object[0];
                cdrStream.SkipRest(); // skip padding, if present
            }
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
        
        /// <summary>
        /// reissue the request on location fwd status
        /// </summary>
        private IMessage ReissueLocationFwd(CdrInputStream cdrStream, IMethodCallMessage request) {
            // read the Location fwd IOR
            Marshaller marshaller = Marshaller.GetSingleton();
            MarshalByRefObject newProxy = marshaller.Unmarshal(request.MethodBase.DeclaringType, new AttributeExtCollection(), cdrStream)
                                              as MarshalByRefObject;
            if (newProxy == null) {
                throw new OBJECT_NOT_EXIST(2402, CompletionStatus.Completed_No);
            }
            object[] reqArgs = new object[request.Args.Length];             
            request.Args.CopyTo(reqArgs, 0);
            object retVal = request.MethodBase.Invoke(newProxy, reqArgs);
            return CreateReturnMsgForValues(retVal, reqArgs, request);
        }
            
        /// <summary>
        /// creates a return message for a return value and possible out/ref args among the sent arguments
        /// </summary>
        private ReturnMessage CreateReturnMsgForValues(object retVal, object[] reqArgs,
                                                       IMethodCallMessage request) {
            // find out args
            MethodInfo targetMethod = (MethodInfo)request.MethodBase;
            ParameterInfo[] parameters = targetMethod.GetParameters();
            ArrayList outArgsList = new ArrayList();
            for (int i = 0; i < parameters.Length; i++) {
                if (ParameterMarshaller.IsOutParam(parameters[i]) || 
                    ParameterMarshaller.IsRefParam(parameters[i])) {
                    outArgsList.Add(reqArgs[i]); // i-th argument is an out/ref param
                }
            }
            
            object[] outArgs = outArgsList.ToArray();
            if (outArgs == null) { 
                outArgs = new object[0]; 
            }
            // create the return message
            return new ReturnMessage(retVal, outArgs, outArgs.Length, null, request); 
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
            return ReadTarget(cdrStream, version);
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
    public class ServiceContextSerTest : TestCase {
        
        public void TestSameServiceIdMultiple() {
            // checks if service contexts with the same id, doesn't throw an exception
            // checks, that the first service context is considered, others are thrown away
            GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();    
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
            // call deser method via reflection, because of protection level
            Type msgBodySerType = ser.GetType();
            MethodInfo method = msgBodySerType.GetMethod("DeserialiseContext", BindingFlags.NonPublic | BindingFlags.Instance);
            Assertion.Assert(method != null);
            ServiceContextCollection result = (ServiceContextCollection) method.Invoke(ser, new object[] { cdrIn });
            // check if context is present
            Assertion.Assert("expected context not in collection", result.ContainsContextForService(1234567) == true);
        }
        
    }
    
}

#endif
