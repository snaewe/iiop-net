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
                // extract the GIOP-version out of the message-target
                GiopVersion version = ExtractGiopVersion(msg as IMethodCallMessage);
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
                                     version, requestId);
                msgOutput.CloseStream();
            } else {
                throw new NotImplementedException("handling for this type of .NET message is not implemented at the moment, type: " +
                                                  msg.GetType());
            }
        }

        /// <summary>extracts the GIOP-version out of the target information</summary>
        private GiopVersion ExtractGiopVersion(IMethodCallMessage msg) {
            GiopVersion result;
            string objectURI = msg.Uri;
            if (IiopUrlUtil.IsUrl(msg.Uri)) {
                IiopUrlUtil.ParseUrl(msg.Uri, out objectURI);
            }
            IiopUrlUtil.GetObjectInfoForObjUri(objectURI, out result);
            return result;
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
