/* IIOPMessageHandler.cs
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
using System.Diagnostics;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.CorbaObjRef;

namespace Ch.Elca.Iiop.MessageHandling {


    /// <summary>
    /// This class handles Giop-Messages
    /// </summary>
    /// <remarks>
    /// This class is a helper class for the formatter
    /// </remarks>
    public class GiopMessageHandler {

        #region SFields

        private static GiopMessageHandler s_handler = new GiopMessageHandler();

        #endregion SFields
        #region IConstructors

        private GiopMessageHandler() {
        }

        #endregion IConstructors
        #region SMethods

        public static GiopMessageHandler GetSingleton() {
            return s_handler;
        }

        #endregion SMethods
        #region IMethods

        /// <summary>reads an incoming Giop reply message from the Stream sourceStream</summary>
        /// <remarks>Precondition: sourceStream contains a Giop reply Msg</remarks>
        /// <returns>the .NET reply Msg created from the Giop Reply</returns>
        public IMessage ParseIncomingReplyMessage(Stream sourceStream, 
                                                  IMethodCallMessage requestMessage) {
            Debug.WriteLine("receive reply message at client side");            
            CdrMessageInputStream msgInput = new CdrMessageInputStream(sourceStream);
            CdrInputStream msgBody = msgInput.GetMessageContentReadingStream();
            // deserialize message body
            GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();
            return ser.DeserialiseReply(msgBody, msgInput.Header.Version, requestMessage);
        }

        /// <summary>reads an incoming Giop request-message from the Stream sourceStream</summary>
        /// <returns>the .NET request message created from this Giop-message</returns>
        public IMessage ParseIncomingRequestMessage(Stream sourceStream) {
            CdrMessageInputStream msgInput = new CdrMessageInputStream(sourceStream);
            CdrInputStream msgBody = msgInput.GetMessageContentReadingStream();
            // deserialize the message body (the GIOP-request id is included in this message)
            GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();
            return ser.DeserialiseRequest(msgBody, msgInput.Header.Version);
        }

        /// <summary>serialises an outgoing .NET request Message on client side</summary>
        public void SerialiseOutgoingRequestMessage(IMessage msg, Stream targetStream, 
                                                    GiopRequestNumberGenerator reqNumberGen) {
            if (msg is IConstructionCallMessage) {
                // not supported in CORBA, TBD: replace through do nothing instead of exception
                throw new NotSupportedException("client activated objects are not supported with this channel");
            } else if (msg is IMethodCallMessage) {
                // extract IOR out of the target addr
                Ior ior = IiopUrlUtil.CreateIorForUrl((msg as IMethodCallMessage).Uri, "");
                GiopVersion version = ior.Version;
                // write a CORBA request message into the stream targetStream
                GiopHeader header = new GiopHeader(version.Major, version.Minor,
                                                   0, GiopMsgTypes.Request);
                CdrMessageOutputStream msgOutput = new CdrMessageOutputStream(targetStream, header);
                // serialize the message, this insert some data into msg, e.g. request-id
                GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();
                uint requestId = reqNumberGen.GenerateRequestId();
                msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY] = requestId; // set request-id
                ser.SerialiseRequest(msg as IMethodCallMessage, 
                                     msgOutput.GetMessageContentWritingStream(),
                                     ior, requestId);
                msgOutput.CloseStream();
            } else {
                throw new NotImplementedException("handling for this type of .NET message is not implemented at the moment, type: " +
                                                  msg.GetType());
            }
        }

        /// <summary>serialises an outgoing .NET reply Message on server side</summary>
        public void SerialiseOutgoingReplyMessage(IMessage msg, GiopVersion version, uint forRequstId,
                                                   Stream targetStream) {
            if (msg is ReturnMessage) {
                // write a CORBA response message into the stream targetStream
                GiopHeader header = new GiopHeader(version.Major, version.Minor, 0, GiopMsgTypes.Reply);
                CdrMessageOutputStream msgOutput = new CdrMessageOutputStream(targetStream, header);
                // serialize the message
                GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();
                ser.SerialiseReply((ReturnMessage)msg, msgOutput.GetMessageContentWritingStream(), 
                                   version, forRequstId);
                msgOutput.CloseStream(); // write to the stream
            } else {
                throw new NotImplementedException("handling for this type of .NET message is not implemented at the moment, type: " +
                                                  msg.GetType());
            }
        }

        #endregion IMethods

    }
}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
	
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Idl;
    using Ch.Elca.Iiop.Util;
    using Ch.Elca.Iiop.MessageHandling;
    using Ch.Elca.Iiop.Cdr;
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
                throw new NotImplementedException();
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
            TestMessage msg = new TestMessage(methodToCall, args, uri);
            // prepare connection context
            IiopClientConnectionManager manager = IiopClientConnectionManager.GetManager();
			GiopClientConnectionContext context = manager.AllocateConnection(msg);

            // serialise            
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            MemoryStream targetStream = new MemoryStream();

            handler.SerialiseOutgoingRequestMessage(msg, targetStream, new GiopRequestNumberGenerator());
            
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
            Assertion.AssertEquals(5, cdrIn.ReadULong());
            // response flags
            data = (byte) cdrIn.ReadOctet();
            Assertion.AssertEquals(3, data);
            cdrIn.ReadPadding(3); // reserved
            // target
            Assertion.AssertEquals(0, cdrIn.ReadUShort());
            // target must be testuri encoded as wide-characters followed by nonStringifyTag ( 44, 115, 116, 114, 61, 102 );
            Assertion.AssertEquals(20 , cdrIn.ReadULong());
            AssertBytesFollowing(
                new byte[] { 0, 116, 0, 101, 0, 115, 0, 116, 0, 117, 0, 114, 0, 105, 44, 115, 116, 114, 61, 102 }, 
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
            // release connection context
            manager.ReleaseConnection(msg);
        }
        
        public void TestReplySerialisation() {
            // request msg the reply is for
            MethodInfo methodToCall = typeof(TestService).GetMethod("Add");
            object[] args = new object[] { ((Int32) 1), ((Int32) 2) };
            string uri = "iiop://localhost:8087/testuri"; // Giop 1.2 will be used because no version spec in uri
            TestMessage msg = new TestMessage(methodToCall, args, uri);
            // create the reply
            ReturnMessage retMsg = new ReturnMessage((Int32) 3, new object[0], 0, null, msg);
            
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            MemoryStream targetStream = new MemoryStream();

            handler.SerialiseOutgoingReplyMessage(retMsg, new GiopVersion(1, 2), 5, targetStream);
            
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
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(sourceStream, 0, new GiopVersion(1, 2));
            cdrOut.WriteOpaque(m_giopMagic);
            // version
            cdrOut.WriteOctet(1);
            cdrOut.WriteOctet(2);
            // flags
            cdrOut.WriteOctet(0);
            // msg-type: request
            cdrOut.WriteOctet(0);
            // msg-length
            cdrOut.WriteULong(68);
            // request-id
            cdrOut.WriteULong(5);
            // response-flags
            cdrOut.WriteOctet(3);
            cdrOut.WritePadding(3);
            // target: key type
            cdrOut.WriteULong(0);
            cdrOut.WriteULong(26); // key length
            cdrOut.WriteOpaque(new byte[] { 0, 116, 0, 101, 0, 115, 0, 116, 0, 111, 0, 98, 0, 106, 0, 101, 0, 99, 0, 116, 44, 115, 116, 114, 61, 102 }); // testobject,str=f
            // method name
            cdrOut.WriteString("Add");
            // no service contexts
            cdrOut.WriteULong(0);
            cdrOut.ForceWriteAlign(Aligns.Align8);
            // parameters
            cdrOut.WriteLong(1);
            cdrOut.WriteLong(2);

            // create a connection context: this is needed for request deserialisation
            IiopServerConnectionManager.GetManager().RegisterActiveConnection();

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
                result = handler.ParseIncomingRequestMessage(sourceStream);
            } catch (RequestDeserializationException e) {
                Console.WriteLine("Request deser exception, reason: " + e.Reason);
                throw e;
            } finally {
                RemotingServices.Disconnect(service);
            }

            // now check if values are correct
            Assertion.Assert("deserialised message is null", result != null);
            Assertion.AssertEquals(5, result.Properties[SimpleGiopMsg.REQUEST_ID_KEY]);
            Assertion.AssertEquals(new GiopVersion(1, 2), result.Properties[SimpleGiopMsg.GIOP_VERSION_KEY]);
            Assertion.AssertEquals(3, result.Properties[SimpleGiopMsg.RESPONSE_FLAGS_KEY]);
            Assertion.AssertEquals("testobject", result.Properties[SimpleGiopMsg.URI_KEY]);
            Assertion.AssertEquals("Ch.Elca.Iiop.Tests.TestService", result.Properties[SimpleGiopMsg.TYPENAME_KEY]);
            Assertion.AssertEquals("Add", result.Properties[SimpleGiopMsg.METHODNAME_KEY]);
            object[] args = (object[])result.Properties[SimpleGiopMsg.ARGS_KEY];
            Assertion.Assert("args is null", args != null);
            Assertion.AssertEquals(2, args.Length);
            Assertion.AssertEquals(1, args[0]);
            Assertion.AssertEquals(2, args[1]);
            // release connection
            IiopServerConnectionManager.GetManager().UnregisterActiveConnection();
        }
        
        public void TestReplyDeserialisation() {
            // request msg the reply is for
            MethodInfo methodToCall = typeof(TestService).GetMethod("Add");
            object[] args = new object[] { ((Int32) 1), ((Int32) 2) };
            string uri = "iiop://localhost:8087/testuri"; // Giop 1.2 will be used because no version spec in uri
            TestMessage requestMsg = new TestMessage(methodToCall, args, uri);
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
            ReturnMessage result = (ReturnMessage) handler.ParseIncomingReplyMessage(sourceStream, requestMsg);
            Assertion.AssertEquals(3, result.ReturnValue);
            Assertion.AssertEquals(0, result.OutArgCount);
        }
    
    }

}

#endif