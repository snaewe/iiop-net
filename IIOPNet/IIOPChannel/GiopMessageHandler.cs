/* IIOPMessageHandler.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 17.01.03  Dominic Ullmann (DUL), dul@elca.ch
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

namespace Ch.Elca.Iiop.MessageHandling {

    /// <summary>gives information to caller how to proceed after message receiving</summary>
    public enum IncomingHandlingStatus {
        normal, fragment, close
    }

    /// <summary>
    /// This class handles IIOP-Messages
    /// </summary>
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

        private IncomingHandlingStatus HandleFragment(CdrInputStream msgBody, GiopHeader header) {
            GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();
            uint moreBytesToFollow = 0;
            uint reqId = ser.DeserialiseFragment(msgBody, header, out moreBytesToFollow);
            // inform connection manager
            GiopConnectionContext context = IiopConnectionManager.GetCurrentConnectionContext();
            IiopConnection con = context.Connection;
            con.FragmetReceived(reqId, moreBytesToFollow, header.GiopFlags);
            return IncomingHandlingStatus.fragment;
        }


        /// <summary>reads an incoming IIOP-message from the Stream sourceStream on the client side</summary>
        /// <param name="resultMsg">the .NET message creates from this IIOP-message</param>
        /// <returns>status</returns>
        public IncomingHandlingStatus ParseIncomingClientMessage(Stream sourceStream, 
                                                                 IMethodCallMessage requestMessage,
                                                                 out IMessage resultMsg) {
            Debug.WriteLine("receive message at client side");            
            CdrMessageInputStream msgInput = new CdrMessageInputStream(sourceStream);
            IiopMsgTypes msgType = msgInput.Header.GIOP_Type;
            Debug.WriteLine("message-type: " + msgType);
            CdrInputStream msgBody = msgInput.GetMessageContentReadingStream();
            // deserialize message body
            switch (msgType) {
                case IiopMsgTypes.Reply:
                    GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();
                    resultMsg = ser.DeserialiseReply(msgInput, msgBody, msgInput.Header.Version, requestMessage);
                    return IncomingHandlingStatus.normal;
                case IiopMsgTypes.Fragment:
                    resultMsg = null;
                    return HandleFragment(msgBody, msgInput.Header);
                default:
                    throw new NotImplementedException("message handling not yet implemented for type: " + msgType);
            }
        }

        /// <summary>reads an incoming IIOP-message from the Stream sourceStream on the server side</summary>
        /// <returns>the .NET message created from this IIOP-message</returns>
        public IncomingHandlingStatus ParseIncomingServerMessage(Stream sourceStream, out IMessage resultMsg) {
            CdrMessageInputStream msgInput = new CdrMessageInputStream(sourceStream);
            IiopMsgTypes msgType = msgInput.Header.GIOP_Type;
            CdrInputStream msgBody = msgInput.GetMessageContentReadingStream();
            // deserialize the message body (the GIOP-request id is included in this message)
            switch (msgType) {
                case IiopMsgTypes.Request:
                    GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();
                    resultMsg = ser.DeserialiseRequest(msgInput, msgBody, msgInput.Header.Version);
                    return IncomingHandlingStatus.normal;
                case IiopMsgTypes.Fragment:
                    resultMsg = null;
                    return HandleFragment(msgBody, msgInput.Header);
                default:
                    throw new NotImplementedException("message handling not yet implemented for type: " + msgType);
            }
        }

        /// <summary>serialises an outgoing .NET Message on client side</summary>
        public void SerialiseOutgoingClientMessage(IMessage msg, Stream targetStream, 
                                                   GiopRequestNumberGenerator reqNumberGen) {
            if (msg is IConstructionCallMessage) {
                // not supported in CORBA, TBD: replace through do nothing instead of exception
                throw new NotSupportedException("client activated objects are not supported with this channel");
            } else if (msg is IMethodCallMessage) {
                // extract the GIOP-version out of the message-target
                GiopVersion version = ExtractGIOPVersion(msg as IMethodCallMessage);
                // write a CORBA request message into the stream targetStream
                GiopHeader header = new GiopHeader(version.Major, version.Minor, 0, IiopMsgTypes.Request);
                CdrMessageOutputStream msgOutput = new CdrMessageOutputStream(targetStream, header);
                // serialize the message, this insert some data into msg, e.g. request-id
                GiopMessageBodySerialiser ser = GiopMessageBodySerialiser.GetSingleton();
                uint requestId = reqNumberGen.GenerateRequestId();
                msg.Properties[SimpleGiopMsg.REQUEST_ID_KEY] = requestId; // set request-id
                ser.SerialiseRequest(msg, msgOutput.GetMessageContentWritingStream(),
                                     version, requestId);
                msgOutput.CloseStream();
            } else {
                throw new NotImplementedException("handling for this type of .NET message is not implemented at the moment, type: " +
                                                  msg.GetType());
            }
        }

        /// <summary>extracts the GIOP-version out of the target information</summary>
        private GiopVersion ExtractGIOPVersion(IMethodCallMessage msg) {
            GiopVersion result;
            string objectURI = msg.Uri;
            if (IiopUrlUtil.IsUrl(msg.Uri)) {
                IiopUrlUtil.ParseUrl(msg.Uri, out objectURI);
            }
            IiopUrlUtil.GetObjectInfoForObjUri(objectURI, out result);
            return result;
        }

        /// <summary>serialises an outgoing .NET Message on server side</summary>
        public void SerialiseOutgoingServerMessage(IMessage msg, GiopVersion version, uint forRequstId,
                                                   Stream targetStream) {
            if (msg is ReturnMessage) {
                // write a CORBA response message into the stream targetStream
                GiopHeader header = new GiopHeader(version.Major, version.Minor, 0, IiopMsgTypes.Reply);
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
