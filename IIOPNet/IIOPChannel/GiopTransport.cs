/* GiopTransport.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 17.01.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.IO;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.MessageHandling;
using omg.org.CORBA;


namespace Ch.Elca.Iiop {

    
    /// <summary>This class is responsible for Receiving/Sending Giop
    /// Messages on a client. Fragmented Messages are Reassembled</summary>
    internal sealed class GiopTransportClientMsgHandler {
        
        
        #region IFields
        
        private Stream m_transportStream;
        
        private FragmentedMsgAssembler m_fragmentAssembler =
            new FragmentedMsgAssembler();
        
        private GiopConnectionDesc m_conDesc;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>default constructor</summary>
        public GiopTransportClientMsgHandler(Stream transportStream,
                                             GiopConnectionDesc conDesc) {
            m_transportStream = transportStream;
            m_conDesc = conDesc;
        }
        
        #endregion IConstructor
        #region IProperties
        
        internal Stream TransportStream {
            get {
                return m_transportStream;
            }
        }        
        
        internal GiopConnectionDesc ConDesc {
            get {
                return m_conDesc;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>processes a request synchronous</summary>
        /// <param name="reqId">the request id of the message</param>
        /// <returns>the response message stream (already fully assembled message)</returns>
        public Stream ProcessRequestSynchronous(Stream msgStream, uint reqId,
                                                out ITransportHeaders responseHeaders) {
            ProcessRequest(msgStream);            
            return ProcessResponse(reqId, out responseHeaders);
                        
        }
        
        /// <summary>
        /// send the formatted request to the target
        /// </summary>
        public void ProcessRequest(Stream requestStream) {                                 
            requestStream.Seek(0, SeekOrigin.Begin); // go to the beginning of the stream
            
            Debug.WriteLine("sending an IIOP message in the Client side Transport sink");
            SendRequestMessage(requestStream);
            Debug.WriteLine("message sent");
        }

        
        /// <summary>
        /// process the response stream, before forwarding it to the formatter
        /// </summary>
        /// <remarks>
        /// Precondition: in the networkStream, the message following is not a response
        /// to another request, than the request sent by ProcessMessage
        /// </remarks>
        public Stream ProcessResponse(uint forReqId,                                     
                                      out ITransportHeaders responseHeaders) {
            responseHeaders = new TransportHeaders();
            responseHeaders[GiopConnectionDesc.CLIENT_TR_HEADER_KEY]= m_conDesc; // add to response headers
            Stream responseStream = null;
                      
            Debug.WriteLine("receiving an IIOP message in the Client side Transport sink");
            responseStream = ReceiveResponseMessage(forReqId);
            Debug.WriteLine("message received");
            responseStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
            return responseStream;
        }        
        
        /// <summary>
        /// sends the message from messageStream
        /// to the transport Stream
        /// </summary>
        private void SendRequestMessage(Stream msgStream) {            
            
#if DEBUG            
            msgStream.Seek(0, SeekOrigin.Begin); // go to the beginning of the stream
            byte[] data = new byte[msgStream.Length];
            msgStream.Read(data, 0, (int)msgStream.Length);
            OutputHelper.DebugBuffer(data);
#endif
            
            msgStream.Seek(0, SeekOrigin.Begin); // go to the beginning of the stream
                        
            IoUtil.StreamCopyExactly(msgStream, m_transportStream, 
                                     (int)msgStream.Length);                        
        }
        
        /// <summary>receives the response for request reqNr,
        /// which starts at the current position
        /// of the transport stream.
        /// </summary>
        /// <returns>the response message for request reqNr or        
        /// IOException if something goes wrong while reading or message
        /// received is not a reply for the request.</returns>
        private Stream ReceiveResponseMessage(uint reqNr) {
            
            bool responseMessageFound = false;
            Stream responseStream = null;
            while (!responseMessageFound) {
                responseStream = ReadResponseMessage();

                // find request_id in message
                uint msgReqId = FindRequestIdInReply(responseStream);
                if (msgReqId == reqNr) {
                    responseMessageFound = true;        
                } else if (msgReqId > reqNr) {
                    Trace.WriteLine("reply of sequence: " + msgReqId + "; expected was: " + reqNr);
                    throw new COMM_FAILURE(154, CompletionStatus.Completed_MayBe);
                } else if (msgReqId < reqNr) {
                    Trace.WriteLine("ignoring reply of sequence (older reply again): " + msgReqId + "; expected was: " + reqNr);
                    // ignore: received an older reply again
                }
            }
            
            responseStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
            return responseStream;            
        }
        
        private Stream ReadResponseMessage() {
            Stream responseStream = null;
            bool fullyRead = false;
            
            while (!fullyRead) {
                // create a stream for reading a new message
                CdrInputStreamImpl reader = new CdrInputStreamImpl(m_transportStream);
                GiopHeader msgHeader = new GiopHeader(reader);
             
                switch(msgHeader.GiopType) {
                    case GiopMsgTypes.Reply:
                        if ((msgHeader.GiopFlags & GiopHeader.FRAGMENT_MASK) > 0) {
                            m_fragmentAssembler.StartFragment(reader, msgHeader);
                        } else {
                            // no fragmentation
                            responseStream = new MemoryStream();
                            msgHeader.WriteToStream(responseStream,
                                                    msgHeader.ContentMsgLength);
                            IoUtil.StreamCopyExactly(m_transportStream, 
                                                     responseStream,
                                                     (int)msgHeader.ContentMsgLength);
                            fullyRead = true; // no more fragments
                        }                    
                        break;
                    case GiopMsgTypes.Fragment:
                        if (!(m_fragmentAssembler.IsLastFragment(msgHeader))) {
                            m_fragmentAssembler.AddFragment(reader, msgHeader);
                        } else {
                            responseStream = m_fragmentAssembler.FinishFragmentedMsg(reader, 
                                                                                     msgHeader);
                            fullyRead = true; // no more fragments
                        }
                        break;
                    default:
                        Trace.WriteLine("unsupported GIOP-msg received: " + msgHeader.GiopType);
                        throw new INTERNAL(155, CompletionStatus.Completed_MayBe);
                }
                
            } // end while (!fullyRead)

                      
#if DEBUG            
            responseStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning
            byte[] data = new byte[responseStream.Length];
            responseStream.Read(data, 0, (int)responseStream.Length);
            OutputHelper.DebugBuffer(data);
#endif
            return responseStream;
        }
        
        
        /// <summary>extract the requestid from the message</summary>
        /// <param name="responseStream">Stream, containing a reply message</param>
        private uint FindRequestIdInReply(Stream responseStream) {
            responseStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
            
            CdrInputStreamImpl reader = new CdrInputStreamImpl(responseStream);
            GiopHeader msgHeader = new GiopHeader(reader);
            Debug.Assert(msgHeader.GiopType == GiopMsgTypes.Reply);
            
            if ((msgHeader.Version.Major == 1) && (msgHeader.Version.Minor <= 1)) {
                // GIOP 1.0 / 1.1, the service context collection preceeds the id
                SkipServiceContexts(reader);
                return reader.ReadULong();
            } else {
                return reader.ReadULong();                
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

        #endregion IMethods
                
    }
    
        
    /// <summary>This class is responsible for Receiving/Sending Giop
    /// Messages on a server. Fragmented Messages are Reassembled</summary>
    internal sealed class GiopTransportServerMsgHandler {
        
        #region Types
        
        internal enum HandlingResult {
            ConnectionClose, ReplyOk, AsyncReply
        }
        
        #endregion Types
        #region IFields

        // create a connection desc for the server connection                    
        private GiopConnectionDesc m_conDesc = new GiopConnectionDesc();
        
        private FragmentedMsgAssembler m_fragmentAssembler =
            new FragmentedMsgAssembler();
        
        private Stream m_transportStream;
        private IServerChannelSink m_transportSink;
        private IServerTransport m_transport;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>default constructor</summary>
        public GiopTransportServerMsgHandler(IServerTransport transport, 
                                             IServerChannelSink transportSink) {
            m_transport = transport;
            m_transportStream = transport.TransportStream;
            m_transportSink = transportSink;
        }                       

        #endregion IConstructors
        #region IMethods
        
        /// <summary>
        /// sends the message from messageStream
        /// to the transport Stream
        /// </summary>
        public void SendResponseMessage(Stream msgStream) {            
            
            Debug.WriteLine("Send response message at server side");
            
#if DEBUG            
            msgStream.Seek(0, SeekOrigin.Begin); // go to the beginning of the stream
            byte[] data = new byte[msgStream.Length];
            msgStream.Read(data, 0, (int)msgStream.Length);
            OutputHelper.DebugBuffer(data);
#endif
            
            msgStream.Seek(0, SeekOrigin.Begin); // go to the beginning of the stream
                        
            IoUtil.StreamCopyExactly(msgStream, m_transportStream, 
                                     (int)msgStream.Length);                        
            
            m_conDesc.MessagesAlreadyExchanged |= true;
            
            Debug.WriteLine("Send response message complete");
        }
        
        /// <summary>
        /// handles a Locate request msg: sends a LocateReply message as a result.
        /// </summary>
        private HandlingResult ProcessLocateRequest(Stream msgStream) {                        
            
            Trace.WriteLine("Process Locate request");
#if DEBUG
            msgStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
            byte[] data = new byte[msgStream.Length];
            msgStream.Read(data, 0, (int)msgStream.Length);
            OutputHelper.DebugBuffer(data);            
#endif                        

            msgStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in GiopMessageHandler

            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            Stream resultMsgStream = handler.HandleIncomingLocateRequestMessage(msgStream);
            SendResponseMessage(resultMsgStream);
            
            Trace.WriteLine("Locate request processed");
            return HandlingResult.ReplyOk;
        }
        
        /// <summary>
        /// handles a request msg: sends a Reply message as a result.
        /// </summary>
        /// <param name="msgStream">the request msg</param>        
        private HandlingResult ProcessRequest(Stream msgStream) {

#if DEBUG
            msgStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
            byte[] data = new byte[msgStream.Length];
            msgStream.Read(data, 0, (int)msgStream.Length);
            OutputHelper.DebugBuffer(data);
#endif

            msgStream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning in formatter
            
            // the out params returned form later sinks
            IMessage responseMsg;
            ITransportHeaders responseHeaders;
            Stream responseStream;
            
            // create the sink stack for async processing of message
            ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
            sinkStack.Push(m_transportSink, m_transportStream);
            // empty transport headers for this protocol
            ITransportHeaders requestHeaders = new TransportHeaders();
            requestHeaders[GiopConnectionDesc.SERVER_TR_HEADER_KEY] = m_conDesc;
            requestHeaders[CommonTransportKeys.IPAddress] = m_transport.GetClientAddress();
            
            // next sink will process the request-message
            ServerProcessing result = 
                m_transportSink.NextChannelSink.ProcessMessage(sinkStack, null, /* no RequestMessage in transport handler */
                                                             requestHeaders, msgStream, 
                                                             out responseMsg, out responseHeaders,
                                                             out responseStream);
            switch (result) {
                case ServerProcessing.Complete :
                    try { 
                        sinkStack.Pop(m_transportSink); 
                    } catch (Exception) { }
                    SendResponseMessage(responseStream);
                    return HandlingResult.ReplyOk;
                case ServerProcessing.Async : 
                    sinkStack.StoreAndDispatch(m_transportSink, this); // this sink wants to handle response
                    return HandlingResult.AsyncReply;
                case ServerProcessing.OneWay :
                    try { 
                        sinkStack.Pop(m_transportSink); 
                    } catch (Exception) { }
                    // no message to send
                    return HandlingResult.ReplyOk;
                default:
                    // should not arrive here
                    throw new Exception("invalid processing state: " + result);
            }

        }

        /// <summary>processes an incoming message at the server side.</summary>
        /// <returns>
        /// Returns HandlingResult expressing result of processing.
        /// Throws an IOException, if something goes wrong at the server side
        /// </returns>
        public HandlingResult ProcessIncomingMsg() {
                              
            while (true) {
                
                CdrInputStreamImpl reader = new CdrInputStreamImpl(m_transportStream);
                GiopHeader msgHeader = new GiopHeader(reader);
            
                switch(msgHeader.GiopType) {
                    case GiopMsgTypes.Request:
                        if ((msgHeader.GiopFlags & GiopHeader.FRAGMENT_MASK) > 0) {
                            m_fragmentAssembler.StartFragment(reader, msgHeader);
                            // more fragments
                            break; // wait for next fragment
                        } else {
                            // no fragmentation
                            Stream msgStream = new MemoryStream();
                            msgHeader.WriteToStream(msgStream,
                                                    msgHeader.ContentMsgLength);
                            IoUtil.StreamCopyExactly(m_transportStream, 
                                                     msgStream,
                                                     (int)msgHeader.ContentMsgLength);                            

                            return ProcessRequest(msgStream); // create and send the response
                        }
                    case GiopMsgTypes.Fragment:
                        if (!(m_fragmentAssembler.IsLastFragment(msgHeader))) {
                            m_fragmentAssembler.AddFragment(reader, msgHeader);
                            // more fragments to follow
                            break; // wait for next fragment
                        } else {
                            Stream msgStream = m_fragmentAssembler.FinishFragmentedMsg(reader, 
                                                                                       ref msgHeader);
                            if (msgHeader.GiopType.Equals(GiopMsgTypes.Request)) {
                                return ProcessRequest(msgStream); // create and send the response
                            } else if (msgHeader.GiopType.Equals(GiopMsgTypes.LocateRequest)) {
                                return ProcessLocateRequest(msgStream); // create and send the response
                            } else {
                                throw new IOException("unsupported Giop message : " +
                                                      msgHeader.GiopType);
                            }
                        }                        
                    case GiopMsgTypes.CloseConnection:
                        return HandlingResult.ConnectionClose;
                    case GiopMsgTypes.LocateRequest:
                        // read the message, may be fragmented in GIOP 1.2
                        if ((msgHeader.GiopFlags & GiopHeader.FRAGMENT_MASK) > 0) {
                            m_fragmentAssembler.StartFragment(reader, msgHeader);
                            // more fragments
                            break; // wait for next fragment
                        } else {
                            Stream msgStream = new MemoryStream();
                            msgHeader.WriteToStream(msgStream,
                                                    msgHeader.ContentMsgLength);
                            IoUtil.StreamCopyExactly(m_transportStream, 
                                                     msgStream,
                                                     (int)msgHeader.ContentMsgLength);

                            return ProcessLocateRequest(msgStream);
                        }                        
                    default:
                        throw new IOException("unsupported Giop message : " +
                                             msgHeader.GiopType);
                }
                
            }
            // can not reach here            
        }
        
        #endregion IMethods
      
    }



}
