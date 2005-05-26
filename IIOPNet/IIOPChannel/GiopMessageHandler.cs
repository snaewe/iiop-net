/* GiopMessageHandler.cs
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Interception;

namespace Ch.Elca.Iiop.MessageHandling {


    /// <summary>
    /// This class handles Giop-Messages
    /// </summary>
    /// <remarks>
    /// This class is a helper class for the formatter
    /// </remarks>
    internal class GiopMessageHandler {
        
        #region SFields

        private static GiopMessageHandler s_handler = new GiopMessageHandler();

        #endregion SFields
        #region IFields
        
        private GiopMessageBodySerialiser m_ser;
        
        #endregion IFields
        #region IConstructors

        private GiopMessageHandler() {
            m_ser = new GiopMessageBodySerialiser();
        }

        #endregion IConstructors
        #region SMethods

        internal static GiopMessageHandler GetSingleton() {
            return s_handler;
        }

        /// <summary>checks if this it's a one way message</summary>
        internal static bool IsOneWayCall(IMethodCallMessage msg) {
            return RemotingServices.IsOneWay(msg.MethodBase);
        }

        #endregion SMethods
        #region IMethods

        /// <summary>reads an incoming Giop reply message from the Stream sourceStream</summary>
        /// <remarks>Precondition: sourceStream contains a Giop reply Msg</remarks>
        /// <returns>the .NET reply Msg created from the Giop Reply</returns>
        internal IMessage ParseIncomingReplyMessage(Stream sourceStream, 
                                                  IMethodCallMessage requestMessage,
                                                  GiopClientConnectionDesc conDesc, IInterceptionOption[] interceptionOptions) {
            Debug.WriteLine("receive reply message at client side");            
            CdrMessageInputStream msgInput = new CdrMessageInputStream(sourceStream);
            CdrInputStream msgBody = msgInput.GetMessageContentReadingStream();
            GiopClientRequest request = new GiopClientRequest(requestMessage, conDesc, interceptionOptions);
            if (request.IsAsyncRequest) {
                try {
                    // with respec to interception, this is a new request -> call again send_request interception before reply
                    request.PrepareSecondAscyncInterception();
                    request.InterceptSendRequest();
                } catch (Exception ex) {
                    request.Reply = new ReturnMessage(ex, requestMessage);
                    Exception newException = request.InterceptReceiveException(ex);
                    if (newException == ex) {
                        throw;
                    } else {
                        throw newException; // exeption has been changed by interception point
                    }
                }
            }
            // deserialize message body            
            IMessage result = m_ser.DeserialiseReply(msgBody, msgInput.Header.Version, request,
                                                     conDesc);
            if (!(result is LocationForwardMessage)) {
                // a standard return message
                return result;
            } else {
                // location-fwd
                // reissue request to new target
                return ForwardRequest(requestMessage, (LocationForwardMessage)result);
            }
                                                                                                            
        }
        
        /// <summary>
        /// creates a return message for a return value and possible out/ref args among the sent arguments
        /// </summary>
        private ReturnMessage CreateReturnMsgForValues(object retVal, object[] reqArgs,
                                                       IMethodCallMessage request) {
            // find out args
            MethodInfo targetMethod = (MethodInfo)request.MethodBase;
            ParameterInfo[] parameters = targetMethod.GetParameters();

            bool outArgFound = false;
            ArrayList outArgsList = new ArrayList();
            for (int i = 0; i < parameters.Length; i++) {
                if (ParameterMarshaller.IsOutParam(parameters[i]) || 
                    ParameterMarshaller.IsRefParam(parameters[i])) {
                    outArgsList.Add(reqArgs[i]); // i-th argument is an out/ref param
                    outArgFound = true;
                } else {
                    outArgsList.Add(null); // for an in param null must be added to out-args
                }
            }
            
            object[] outArgs = outArgsList.ToArray();
            if ((!outArgFound) || (outArgs == null)) { 
                outArgs = new object[0]; 
            }
            // create the return message
            return new ReturnMessage(retVal, outArgs, outArgs.Length, null, request); 
        }        
        
        private IMessage ForwardRequest(IMethodCallMessage request,
                                        LocationForwardMessage target) {
            object[] reqArgs = new object[request.Args.Length];
            request.Args.CopyTo(reqArgs, 0);
            object retVal = request.MethodBase.Invoke(target.FwdToProxy, reqArgs);
            return CreateReturnMsgForValues(retVal, reqArgs, 
                                            request);
        }        

        /// <summary>reads an incoming Giop request-message from the Stream sourceStream</summary>
        /// <returns>the .NET request message created from this Giop-message</returns>
        internal IMessage ParseIncomingRequestMessage(Stream sourceStream, 
                                                    GiopConnectionDesc conDesc, IInterceptionOption[] interceptionOptions) {
            CdrMessageInputStream msgInput = new CdrMessageInputStream(sourceStream);
            CdrInputStream msgBody = msgInput.GetMessageContentReadingStream();
            // deserialize the message body (the GIOP-request id is included in this message)
            return m_ser.DeserialiseRequest(msgBody, msgInput.Header.Version,
                                            conDesc, interceptionOptions);
        }

        /// <summary>serialises an outgoing .NET request Message on client side</summary>
        internal void SerialiseOutgoingRequestMessage(IMessage msg, IIorProfile target, GiopClientConnectionDesc conDesc,
                                                    Stream targetStream, uint requestId, IInterceptionOption[] interceptionOptions) {
            if (msg is IConstructionCallMessage) {
                // not supported in CORBA, TBD: replace through do nothing instead of exception
                throw new NotSupportedException("client activated objects are not supported with this channel");
            } else if (msg is IMethodCallMessage) {
                GiopVersion version = target.Version;
                // write a CORBA request message into the stream targetStream
                GiopHeader header = new GiopHeader(version.Major, version.Minor,
                                                   0, GiopMsgTypes.Request);
                CdrMessageOutputStream msgOutput = new CdrMessageOutputStream(targetStream, header);
                // serialize the message, this insert some data into msg, e.g. request-id
                msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY] = requestId; // set request-id
                msg.Properties[SimpleGiopMsg.TARGET_PROFILE_KEY] = target;
                GiopClientRequest request = new GiopClientRequest((IMethodCallMessage)msg, conDesc, interceptionOptions);
                m_ser.SerialiseRequest(request, 
                                       msgOutput.GetMessageContentWritingStream(),
                                       target, conDesc);
                msgOutput.CloseStream();
                if ((request.IsAsyncRequest) || (request.IsOneWayCall)) {
                    // after successful serialisation, call for oneway and async requests receive other, 
                    // see corba 2.6, page 21-12.
                    request.InterceptReceiveOther();
                }
            } else {
                throw new NotImplementedException("handling for this type of .NET message is not implemented at the moment, type: " +
                                                  msg.GetType());
            }
        }

        /// <summary>serialises an outgoing .NET reply Message on server side</summary>
        internal void SerialiseOutgoingReplyMessage(IMessage replyMsg, IMessage requestMsg, GiopVersion version,
                                                    Stream targetStream, GiopConnectionDesc conDesc,
                                                    IInterceptionOption[] interceptionOptions) {
            if (replyMsg is ReturnMessage) {
                // write a CORBA response message into the stream targetStream
                GiopHeader header = new GiopHeader(version.Major, version.Minor, 0, GiopMsgTypes.Reply);
                CdrMessageOutputStream msgOutput = new CdrMessageOutputStream(targetStream, header);
                GiopServerRequest request = new GiopServerRequest(requestMsg, 
                                                                  (ReturnMessage)replyMsg, conDesc, 
                                                                  interceptionOptions);
                // serialize the message
                m_ser.SerialiseReply(request, msgOutput.GetMessageContentWritingStream(), 
                                     version, conDesc);
                msgOutput.CloseStream(); // write to the stream
            } else {
                throw new NotImplementedException("handling for this type of .NET message is not implemented at the moment, type: " +
                                                  replyMsg.GetType());
            }
        }

        /// <summary>
        /// reads a locate-request message and formulates an answer.
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        internal Stream HandleIncomingLocateRequestMessage(Stream sourceStream) {
            Debug.WriteLine("receive locate request message");
            CdrMessageInputStream msgInput = new CdrMessageInputStream(sourceStream);
            CdrInputStream msgBody = msgInput.GetMessageContentReadingStream();
            // deserialize message body
            
            uint forRequestId;
            string targetUri = m_ser.DeserialiseLocateRequest(msgBody, msgInput.Header.Version, out forRequestId);
            Debug.WriteLine("locate request for target-uri: " + targetUri);
            Stream targetStream = new MemoryStream();
            GiopVersion version = msgInput.Header.Version;
            GiopHeader header = new GiopHeader(version.Major, version.Minor, 0, GiopMsgTypes.LocateReply);
            CdrMessageOutputStream msgOutput = new CdrMessageOutputStream(targetStream, header);
            // serialize the message
            m_ser.SerialiseLocateReply(msgOutput.GetMessageContentWritingStream(), version, forRequestId, 
                                       LocateStatus.OBJECT_HERE, null); // for the moment, do not try to find object, because forward is not possibly for IIOP.NET server
            msgOutput.CloseStream(); // write to the stream 
            return targetStream;
        }            
        
        internal Stream PrepareMessageErrorMessage(GiopVersion version) {
            Debug.WriteLine("create a message error message");
            Stream targetStream = new MemoryStream();            
            GiopHeader header = new GiopHeader(version.Major, version.Minor, 0, GiopMsgTypes.MessageError);
            header.WriteToStream(targetStream, 0);
            targetStream.Seek(0, SeekOrigin.Begin);
            return targetStream;
        }
        
        /// <summary>
        /// create a close connection message
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        internal Stream PrepareMessageCloseMessage(GiopVersion version) {
            Debug.WriteLine("create a close connection message");
            Stream targetStream = new MemoryStream();            
            GiopHeader header = new GiopHeader(version.Major, version.Minor, 0, GiopMsgTypes.CloseConnection);
            header.WriteToStream(targetStream, 0);
            targetStream.Seek(0, SeekOrigin.Begin);
            return targetStream;            
        }

        #endregion IMethods

    }
}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.Runtime.Remoting.Channels;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Idl;
    using Ch.Elca.Iiop.Util;
    using Ch.Elca.Iiop.MessageHandling;
    using Ch.Elca.Iiop.Cdr;
    using Ch.Elca.Iiop.Interception;
    using omg.org.CORBA;


    public class TestService : MarshalByRefObject {
    
        public int Add(Int32 arg1, Int32 arg2) {
            return arg1 + arg2;
        }
    
    }

    public class TestMessage : IMethodCallMessage {
    
        private IDictionary m_props = new Hashtable();

        private MethodInfo m_methodToCall;
        
        private string m_uri;
        
        private object[] m_args;
        
        private bool m_hasVarArgs = false;
    
        public TestMessage(MethodInfo methodToCall, object[] args, string uri) {
            m_methodToCall = methodToCall;
            m_uri = uri;
            m_args = args;
        }
        
        public IDictionary Properties {
            get {
                return m_props;
            }
        }
        
        public string Uri {
            get {
                return m_uri;
            }
        }
        
        public string TypeName {
            get {
                return m_methodToCall.DeclaringType.FullName;
            }
        }
        
        public string MethodName {
            get {
                return m_methodToCall.Name;
            }
        }
               
        public MethodBase MethodBase {
            get {
                return m_methodToCall;
            }
        }
        
        public object MethodSignature {
            get {
                if (MethodBase != null) {
                    return ReflectionHelper.GenerateSigForMethod(MethodBase);
                } else {
                    return null;
                }
                    
            }
        }
        
        public int ArgCount {
            get {
                return m_args.Length;
            }
        }
        
        public int InArgCount {
            get {
                throw new NotImplementedException();
            }
        }
        
        public object[] Args {
            get {
                return m_args;
            }
        }
        
        public object[] InArgs {
            get {
                throw new NotImplementedException();
            }
        }
        
        public bool HasVarArgs {
            get {
                return m_hasVarArgs;
            }
            set {
                m_hasVarArgs = value;
            }
        }
        
        public LogicalCallContext LogicalCallContext {
            get {
                return null;
            }
        }
        
        public object GetArg(int nr) {
            return m_args[nr];
        }
        
        public object GetInArg(int nr) {
            throw new NotImplementedException();
        }
        
        public string GetArgName(int nr) {
            throw new NotImplementedException();
        }
        
        public string GetInArgName(int nr) {
            throw new NotImplementedException();
        }
                                    
    }


    /// <summary>
    /// Unit-tests for testing request/reply serialisation/deserialisation
    /// </summary>
    public class RequestReplySerialisationTest : TestCase {
    
        private byte[] m_giopMagic = { 71, 73, 79, 80 };    
                
        /// <summary>
        /// asserts that the expected byte sequence is following in the stream
        /// </summary>
        private void AssertBytesFollowing(byte[] expected, CdrInputStream cdrIn) {
            for (int i = 0; i < expected.Length; i++) {
                byte data = (byte) cdrIn.ReadOctet();
                Assertion.AssertEquals(expected[i], data);
            }
        }
        
        /// <summary>
        /// skips the service contexts in a request / reply msg
        /// </summary>
        private void SkipServiceContexts(CdrInputStream cdrIn) {
            uint nrOfContexts = cdrIn.ReadULong();
            // Skip service contexts: not part of this test            
            for (uint i = 0; i < nrOfContexts; i++) {
                uint contextId = cdrIn.ReadULong();
                uint lengthOfContext = cdrIn.ReadULong();
                cdrIn.ReadPadding(lengthOfContext);
            }
        }
        
        public void TestRequestSerialisation() {
            // prepare message
            MethodInfo methodToCall = typeof(TestService).GetMethod("Add");
            object[] args = new object[] { ((Int32) 1), ((Int32) 2) };
            string uri = "iiop://localhost:8087/testuri"; // Giop 1.2 will be used because no version spec in uri
            Ior target = IiopUrlUtil.CreateIorForUrl(uri, "");
            TestMessage msg = new TestMessage(methodToCall, args, uri);
            // prepare connection context
            GiopClientConnectionDesc conDesc = new GiopClientConnectionDesc(null, null, 
                                                                            new GiopRequestNumberGenerator(), null);

            // serialise            
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            MemoryStream targetStream = new MemoryStream();
            
            uint reqId = 5;
            handler.SerialiseOutgoingRequestMessage(msg, target.Profiles[0], conDesc, targetStream, reqId,
                                                    InterceptorManager.EmptyInterceptorOptions);
            
            // check to serialised stream
            targetStream.Seek(0, SeekOrigin.Begin);
            
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(targetStream);
            cdrIn.ConfigStream(0, new GiopVersion(1, 2));
            
            // first is Giop-magic                        
            byte data;
            AssertBytesFollowing(m_giopMagic, cdrIn);
            // Giop version
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(1, data);
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(2, data);
            // flags: big-endian, no fragements
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(0, data);
            // Giop Msg type: request
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(0, data);
            // Giop Msg length
            uint msgLength = cdrIn.ReadULong();
            cdrIn.SetMaxLength(msgLength);
            // req-id
            Assertion.AssertEquals(reqId, cdrIn.ReadULong());
            // response flags
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(3, data);
            cdrIn.ReadPadding(3); // reserved
            // target
            Assertion.AssertEquals(0, cdrIn.ReadUShort());
            // target must be testuri encoded as ascii-characters
            Assertion.AssertEquals(7 , cdrIn.ReadULong());
            AssertBytesFollowing(
                new byte[] { 116, 101, 115, 116, 117, 114, 105 }, 
                cdrIn);
            // now the target method follows: Add (string is terminated by a zero)
            Assertion.AssertEquals(4, cdrIn.ReadULong());
            AssertBytesFollowing(new byte[] { 65, 100, 100, 0}, cdrIn);
            // now service contexts are following
            SkipServiceContexts(cdrIn);
            // Giop 1.2, must be aligned on 8
            cdrIn.ForceReadAlign(Aligns.Align8);
            // now params are following
            Assertion.AssertEquals(1, cdrIn.ReadLong());
            Assertion.AssertEquals(2, cdrIn.ReadLong());
        }
        
        public void TestReplySerialisation() {
            // request msg the reply is for
            MethodInfo methodToCall = typeof(TestService).GetMethod("Add");
            object[] args = new object[] { ((Int32) 1), ((Int32) 2) };
            string uri = "iiop://localhost:8087/testuri"; // Giop 1.2 will be used because no version spec in uri
            GiopVersion version = new GiopVersion(1, 2);
            TestMessage msg = new TestMessage(methodToCall, args, uri);
            msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY] = (uint)5;
            msg.Properties[SimpleGiopMsg.GIOP_VERSION_KEY] = version;
            msg.Properties[SimpleGiopMsg.CALLED_METHOD_KEY] = methodToCall;
            // create a connection context
            GiopConnectionDesc conDesc = new GiopConnectionDesc(null, null);

            // create the reply
            ReturnMessage retMsg = new ReturnMessage((Int32) 3, new object[0], 0, null, msg);            
            
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            MemoryStream targetStream = new MemoryStream();
            
            handler.SerialiseOutgoingReplyMessage(retMsg, msg, version, 
                                                  targetStream, conDesc, InterceptorManager.EmptyInterceptorOptions);
            
            // check to serialised stream
            targetStream.Seek(0, SeekOrigin.Begin);

            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(targetStream);
            cdrIn.ConfigStream(0, new GiopVersion(1, 2));
            
            // first is Giop-magic
            byte data;
            AssertBytesFollowing(m_giopMagic, cdrIn);
            // Giop version
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(1, data);
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(2, data);
            // flags: big-endian, no fragements
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(0, data);
            // Giop Msg type: reply
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(1, data);
            // Giop Msg length
            uint msgLength = cdrIn.ReadULong();
            cdrIn.SetMaxLength(msgLength);
            // req-id
            Assertion.AssertEquals(5, cdrIn.ReadULong());
            // response status: NO_EXCEPTION
            Assertion.AssertEquals(0, cdrIn.ReadULong());
            // ignore service contexts
            SkipServiceContexts(cdrIn);
            // Giop 1.2, must be aligned on 8
            cdrIn.ForceReadAlign(Aligns.Align8);
            // now return value is following
            Assertion.AssertEquals(3, cdrIn.ReadLong());
        }
        
        public void TestRequestDeserialisation() {          
            MemoryStream sourceStream = new MemoryStream();
            // prepare msg
            uint requestId = 5;
            byte responseFlags = 3;
            string methodName = "Add";
            int nrOfArgs = 2;
            int arg1 = 1;
            int arg2 = 2;
            GiopVersion version = new GiopVersion(1, 2);
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(sourceStream, 0, 
                                                                 version);
            cdrOut.WriteOpaque(m_giopMagic);
            // version
            cdrOut.WriteOctet(version.Major);
            cdrOut.WriteOctet(version.Minor);
            // flags
            cdrOut.WriteOctet(0);
            // msg-type: request
            cdrOut.WriteOctet(0);
            // msg-length
            cdrOut.WriteULong(68);
            // request-id
            cdrOut.WriteULong(requestId);
            // response-flags
            cdrOut.WriteOctet(responseFlags);
            cdrOut.WritePadding(3);
            // target: key type
            cdrOut.WriteULong(0);
            cdrOut.WriteULong(10); // key length
            cdrOut.WriteOpaque(new byte[] { 116, 101, 115, 116, 111, 98, 106, 101, 99, 116 }); // testobject
            // method name
            cdrOut.WriteString(methodName);
            // no service contexts
            cdrOut.WriteULong(0);
            cdrOut.ForceWriteAlign(Aligns.Align8);
            // parameters
            cdrOut.WriteLong(arg1);
            cdrOut.WriteLong(arg2);

            // create a connection context: this is needed for request deserialisation
            GiopConnectionDesc conDesc = new GiopConnectionDesc(null, null);

            // go to stream begin
            sourceStream.Seek(0, SeekOrigin.Begin);
 
            IMessage result = null;
            TestService service = new TestService();
            try {
                // object which should be called
                string uri = "testobject";
                RemotingServices.Marshal(service, uri);

                // deserialise request message
                GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
                result = handler.ParseIncomingRequestMessage(sourceStream, conDesc, InterceptorManager.EmptyInterceptorOptions);
            } catch (RequestDeserializationException e) {
                Console.WriteLine("Request deser exception, reason: " + e.Reason);
                throw e;
            } finally {
                RemotingServices.Disconnect(service);
            }

            // now check if values are correct
            Assertion.Assert("deserialised message is null", result != null);
            Assertion.AssertEquals(requestId, result.Properties[SimpleGiopMsg.REQUEST_ID_KEY]);
            Assertion.AssertEquals(version, result.Properties[SimpleGiopMsg.GIOP_VERSION_KEY]);
            Assertion.AssertEquals(responseFlags, result.Properties[SimpleGiopMsg.RESPONSE_FLAGS_KEY]);
            Assertion.AssertEquals("testobject", result.Properties[SimpleGiopMsg.URI_KEY]);
            Assertion.AssertEquals("Ch.Elca.Iiop.Tests.TestService", result.Properties[SimpleGiopMsg.TYPENAME_KEY]);
            Assertion.AssertEquals(methodName, result.Properties[SimpleGiopMsg.METHODNAME_KEY]);
            object[] args = (object[])result.Properties[SimpleGiopMsg.ARGS_KEY];
            Assertion.Assert("args is null", args != null);
            Assertion.AssertEquals(nrOfArgs, args.Length);
            Assertion.AssertEquals(arg1, args[0]);
            Assertion.AssertEquals(arg2, args[1]);
        }
        
        public void TestReplyDeserialisation() {
            // request msg the reply is for
            MethodInfo methodToCall = typeof(TestService).GetMethod("Add");
            object[] args = new object[] { ((Int32) 1), ((Int32) 2) };
            string uri = "iiop://localhost:8087/testuri"; // Giop 1.2 will be used because no version spec in uri
            TestMessage requestMsg = new TestMessage(methodToCall, args, uri);
            // prepare connection desc
            GiopClientConnectionDesc conDesc = new GiopClientConnectionDesc(null, null, new GiopRequestNumberGenerator(), null);
            // create the reply
            MemoryStream sourceStream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(sourceStream, 0, new GiopVersion(1, 2));
            cdrOut.WriteOpaque(m_giopMagic);
            // version
            cdrOut.WriteOctet(1);
            cdrOut.WriteOctet(2);
            // flags
            cdrOut.WriteOctet(0);
            // msg-type: reply
            cdrOut.WriteOctet(1);
            // msg-length
            cdrOut.WriteULong(16);
            // request-id
            cdrOut.WriteULong(5);
            // reply-status: no-exception
            cdrOut.WriteULong(0);
            // no service contexts
            cdrOut.WriteULong(0);
            // body: 8 aligned
            cdrOut.ForceWriteAlign(Aligns.Align8); 
            // result
            cdrOut.WriteLong(3);
            // check deser of msg:
            sourceStream.Seek(0, SeekOrigin.Begin);
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            ReturnMessage result = (ReturnMessage) handler.ParseIncomingReplyMessage(sourceStream, requestMsg, conDesc, InterceptorManager.EmptyInterceptorOptions);
            Assertion.AssertEquals(3, result.ReturnValue);
            Assertion.AssertEquals(0, result.OutArgCount);
        }                
                
        //[Ignore("can prevent the test domain from unloading, find a solution for this before adding definitively")]
        public void TestLocationForward() {
            IiopChannel chan = new IiopChannel(8090);
            ChannelServices.RegisterChannel(chan);
            // publish location fwd target
            TestService target = new TestService();            
            string fwdTargetUri = "testuriFwd";
            RemotingServices.Marshal(target, fwdTargetUri);
            
            // request msg the reply is for
            MethodInfo methodToCall = typeof(TestService).GetMethod("Add");
            object[] args = new object[] { ((Int32) 1), ((Int32) 2) };
            string origUrl = "iiop://localhost:8090/testuri"; // Giop 1.2 will be used because no version spec in uri
            TestMessage requestMsg = new TestMessage(methodToCall, args, origUrl);
            // prepare connection desc
            GiopClientConnectionDesc conDesc = new GiopClientConnectionDesc(null, null, new GiopRequestNumberGenerator(), null);
            
            try {
                Stream locFwdStream = PrepareLocationFwdStream("localhost", 8090,
                                                               target);
                GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
                ReturnMessage result = 
                    (ReturnMessage) handler.ParseIncomingReplyMessage(locFwdStream, requestMsg, conDesc, InterceptorManager.EmptyInterceptorOptions);
                Assertion.AssertEquals(3, result.ReturnValue);
                Assertion.AssertEquals(0, result.OutArgCount);                
            } finally {
                // unpublish target + channel
                RemotingServices.Disconnect(target);
                chan.StopListening(null);
                ChannelServices.UnregisterChannel(chan);            
            }
        }
                
        //[Ignore("can prevent the test domain from unloading, find a solution for this before adding definitively")]
        public void TestLocationForwardOnIsA() {
            // tests location forward, if we forward on is_a call            
            IiopChannel chan = new IiopChannel(8090);
            ChannelServices.RegisterChannel(chan);
            // publish location fwd target
            TestService target = new TestService();            
            string fwdTargetUri = "testuriFwdForIsA";
            RemotingServices.Marshal(target, fwdTargetUri);
            
            // request msg the reply is for
            MethodInfo methodToCall = typeof(omg.org.CORBA.IObject).GetMethod("_is_a");
            object[] args = new object[] { "IDL:Ch/Elca/Iiop/Tests/TestService:1.0" };
            string origUrl = "iiop://localhost:8090/testuri"; // Giop 1.2 will be used because no version spec in uri
            TestMessage requestMsg = new TestMessage(methodToCall, args, origUrl);
            // prepare connection desc
            GiopClientConnectionDesc conDesc = new GiopClientConnectionDesc(null, null, new GiopRequestNumberGenerator(), null);
            
            try {
                Stream locFwdStream = PrepareLocationFwdStream("localhost", 8090,
                                                               target);
                GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
                ReturnMessage result = 
                    (ReturnMessage) handler.ParseIncomingReplyMessage(locFwdStream, requestMsg, conDesc, InterceptorManager.EmptyInterceptorOptions);
                Assertion.AssertEquals(true, result.ReturnValue);
                Assertion.AssertEquals(0, result.OutArgCount);                
            } finally {
                // unpublish target + channel
                RemotingServices.Disconnect(target);
                chan.StopListening(null);
                ChannelServices.UnregisterChannel(chan);            
            }

            
            
        }
        
        private Stream PrepareLocationFwdStream(string host, short port,
                                                MarshalByRefObject target) {
                       
            // loc fwd ior
            byte[] objectKey = IiopUrlUtil.GetObjectKeyForObj(target);
            string repositoryID = Repository.GetRepositoryID(target.GetType());
            // this server support GIOP 1.2 --> create an GIOP 1.2 profile
            InternetIiopProfile profile = new InternetIiopProfile(new GiopVersion(1, 2), host,
                                                                  port, objectKey);                           
            Ior locFwdTarget = new Ior(repositoryID, new IorProfile[] { profile });
            CdrOutputStreamImpl iorStream = new CdrOutputStreamImpl(new MemoryStream(), 
                                                                    0, new GiopVersion(1, 2));
            locFwdTarget.WriteToStream(iorStream);            
            uint encodedIorLength = (uint)iorStream.GetPosition();
            
            // create the location fwd reply
            MemoryStream sourceStream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(sourceStream, 0, new GiopVersion(1, 2));
            cdrOut.WriteOpaque(m_giopMagic);
            // version
            cdrOut.WriteOctet(1);
            cdrOut.WriteOctet(2);
            // flags
            cdrOut.WriteOctet(0);
            // msg-type: reply
            cdrOut.WriteOctet(1);
            
            // msg-length
            cdrOut.WriteULong(28 + encodedIorLength);
            // request-id
            cdrOut.WriteULong(5);
            // reply-status: location fwd
            cdrOut.WriteULong(3);
            // one service context to enforce alignement requirement for giop 1.2
            cdrOut.WriteULong(1);
            cdrOut.WriteULong(162739); // service context id
            cdrOut.WriteULong(2); // length of svc context
            cdrOut.WriteBool(true);
            cdrOut.WriteBool(false);            
            // svc context end            
            // body: 8 aligned
            cdrOut.ForceWriteAlign(Aligns.Align8);

            locFwdTarget.WriteToStream(cdrOut);
                
            sourceStream.Seek(0, SeekOrigin.Begin);                
            return sourceStream;            
        }

    
    }

}

#endif
