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
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.MessageHandling;
using omg.org.CORBA;


namespace Ch.Elca.Iiop {
    
    
    /// <summary>
    /// represents a message timeout (can be infinite or finite)
    /// </summary>
    internal class MessageTimeout {
        
    	#region SFields
    	
    	private static MessageTimeout s_infiniteTimeOut = new MessageTimeout();
    	
    	#endregion SFields
    	#region IFields
    	
        private object m_timeOut = null;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>
        /// infinite connection timeout
        /// </summary>
        private MessageTimeout() {
            m_timeOut = null;
        }
        
        /// <summary>
        /// timeout set to the argument parameter.
        /// </summary>
        internal MessageTimeout(TimeSpan timeOut) {
            m_timeOut = timeOut;
        }

        #endregion IConstructors
        #region SProperties

        /// <summary>
        /// returns an instance of MessageTimeout with infinite timeout.
        /// </summary>
        internal static MessageTimeout Infinite {
        	get {
        		return s_infiniteTimeOut;
        	}
        }
        
        #endregion SProperties        
        #region IProperties
        
        internal TimeSpan TimeOut {
            get {
                if (m_timeOut != null) {
                    return (TimeSpan)m_timeOut;
                } else {
                    throw new BAD_OPERATION(109, CompletionStatus.Completed_MayBe);
                }
            }
        }
        
        /// <summary>
        /// is no timeout defined ?
        /// </summary>
        internal bool IsUnlimited {
            get {
                return m_timeOut == null;
            }
        }                
        
        #endregion IProperties
                                
    }
    
    
    /// <summary>
    /// inteface of a giop request message receiver;    
    /// </summary>
    /// <remarks>the methods of this interface are called in a ThreadPool thread</remarks>
    internal interface IGiopRequestMessageReceiver {
                
        void ProcessRequest(Stream requestStream, GiopTransportMessageHandler transportHandler,
                            GiopConnectionDesc conDesc);
        
        void ProcessLocateRequest(Stream requestStream, GiopTransportMessageHandler transportHandler,
                                  GiopConnectionDesc conDesc);
        
    }        
    
    /// <summary>
    /// encapsulates the logic to send a message.
    /// </summary>
    internal class MessageSendTask {
        
        #region Constants
        
        /// <summary>
        /// specifies, how much should be read in one step from message to send
        /// </summary>
        private const int READ_CHUNK_SIZE = 8192;

        #endregion Constants
        #region Fields
        
        private ITransport m_onTransport;
        private GiopTransportMessageHandler m_messageHandler;        
        
        private byte[] m_buffer = new byte[READ_CHUNK_SIZE];
        
        #endregion Fields
        #region IConstructors
        
        public MessageSendTask(ITransport onTransport,
                               GiopTransportMessageHandler messageHandler) {
            Initalize(onTransport, messageHandler);
        }

                
        #endregion IConstructors
        
        private void Initalize(ITransport onTransport,
                               GiopTransportMessageHandler messageHandler) {
            m_onTransport = onTransport;           
            m_messageHandler = messageHandler;
        }
                
        /// <summary>
        /// begins the send of the next message part on the transport;
        /// notifies callback about progress
        /// </summary>
        internal void Send(Stream stream, long bytesToSend) {
            if (bytesToSend <= 0) {
                throw new omg.org.CORBA.INTERNAL(87, CompletionStatus.Completed_MayBe);
            }

            long bytesAlreadySent = 0;
            while (HasDataToSend(bytesAlreadySent, bytesToSend)) {
                // need more data
                long nrOfBytesToRead = bytesToSend - bytesAlreadySent;
                int toRead = (int)Math.Min(m_buffer.Length,
                                           nrOfBytesToRead);
                
                // read either the whole buffer length or
                // the remaining nr of bytes: nrOfBytesToRead - bytesRead
                int bytesToSendInProgress = stream.Read(m_buffer, 0, toRead);
                if (bytesToSendInProgress <= 0) {
                    // underlying stream not enough data
                    throw new omg.org.CORBA.INTERNAL(88, CompletionStatus.Completed_MayBe);
                }
                bytesAlreadySent += bytesToSendInProgress;
                IAsyncResult res =
                    m_onTransport.BeginWrite(m_buffer, 0, bytesToSendInProgress, null, null);
                bool waitOk = m_messageHandler.WaitForEvent(res.AsyncWaitHandle);
                if (!waitOk) {
                    throw new omg.org.CORBA.TIMEOUT(37, CompletionStatus.Completed_MayBe);
                }
                m_onTransport.EndWrite(res);
            }
            // write complete
        }                
        
        /// <summary>
        /// returns true, if another part should be sent, else 
        /// returns false to indicate that the message is completely sent
        /// </summary>
        /// <returns></returns>
        private bool HasDataToSend(long bytesAlreadySent, long bytesToSend) {
            return bytesAlreadySent < bytesToSend;
        }
                        
    }            
    
    /// <summary>
    /// encapsulates a message to receive and keeps track of what has already been received
    /// </summary>
    internal class MessageReceiveTask {

        #region Constants
        
        /// <summary>
        /// specifies, how much should be read in one step from message to send
        /// </summary>
        private const int READ_CHUNK_SIZE = 8192;

        #endregion Constants
        #region Fields

        private ITransport m_onTransport;
        private GiopTransportMessageHandler m_messageHandler;
        
        private GiopHeader m_header = null;
        private Stream m_messageToReceive;        
        private int m_expectedMessageLength;
        private int m_bytesRead;
        
        private byte[] m_buffer = new byte[READ_CHUNK_SIZE];
        private byte[] m_giopHeaderBuffer = new byte[GiopHeader.HEADER_LENGTH];
        
        #endregion Fields
        #region IConstructors
        
        public MessageReceiveTask(ITransport onTransport,
                                  GiopTransportMessageHandler messageHandler) {            
            m_onTransport = onTransport;
            m_messageHandler = messageHandler;
        }
        
        #endregion IConstructors
        #region IProperties
                
        public Stream MessageStream {
            get {
                return m_messageToReceive;
            }
        }             
        
        public GiopHeader Header {
            get {
                if (m_header == null) {
                    throw new INTERNAL(100, CompletionStatus.Completed_MayBe);
                }
                return m_header;
            }
        }
                
        #endregion IProperties
        #region IMethods
        
        /// <summary>
        /// begins receiving a new message from transport; can be called again, after message has been completed.
        /// </summary>
        public void StartReceiveMessage() {
            m_messageToReceive = new MemoryStream();
            m_header = null;
            m_expectedMessageLength = GiopHeader.HEADER_LENGTH; // giop header-length
            m_bytesRead = 0;

            StartReceiveNextMessagePart();
        }
        
        private void StartReceiveNextMessagePart() {
            int toRead = Math.Min(READ_CHUNK_SIZE,
                                       m_expectedMessageLength - m_bytesRead);
            m_onTransport.BeginRead(m_buffer, 0, toRead, new AsyncCallback(this.HandleReadCompleted), this);
        }
        

        private void HandleReadCompleted(IAsyncResult ar) {
            try {            
                int read = m_onTransport.EndRead(ar);
                if (read <= 0) {
                    // connection has been closed by the other end
                    m_messageHandler.MsgReceivedConnectionClosedException();
                    return;
                }
                int offset = m_bytesRead;
                m_bytesRead += read;
                // copy to message stream
                m_messageToReceive.Write(m_buffer, 0, read);
                // handle header
                if (m_header == null) {
                    // copy to giop-header buffer
                    Array.Copy(m_buffer, 0, m_giopHeaderBuffer, offset, read);
                    if (m_bytesRead == 12) {
                        m_header = new GiopHeader(m_giopHeaderBuffer);
                        m_expectedMessageLength = (int)(m_expectedMessageLength + m_header.ContentMsgLength);
                    }
                }
                if (HasNextMessagePart()) {
                    StartReceiveNextMessagePart();
                } else {
                    // completed
                    m_messageToReceive.Seek(0, SeekOrigin.Begin);
                    m_messageHandler.MsgReceivedCallback(this);
                }
            } catch (Exception ex) {
                m_messageHandler.MsgReceivedCallbackException(ex);
            }
        }
                
        private bool HasNextMessagePart() {
            return m_bytesRead < m_expectedMessageLength;
        }
                
        #endregion IMethods                
        
    }
    
    
    /// <summary>
    /// this class is responsible for reading/writing giop messages
    /// </summary>
    public class GiopTransportMessageHandler {                
        
        #region ResponseWaiter-Types
        
        /// <summary>
        /// inteface for a helper class, which waits for a response
        /// </summary>
        internal interface IResponseWaiter {
            Stream Response {
                get;
                set;
            }
            Exception Problem {
                get;
                set;
            }
            
            /// <summary>
            /// is called by MessageHandler, if the suitable response has been received
            /// </summary>
            void Notify();
            
            /// <summary>
            /// prepare for receiving notify; can block the current thread (notify will then unblock)
            /// </summary>
            /// <returns>false, if the wait has been interrupted, otherwise true</returns>
            bool StartWaiting();
            
            
            /// <summary>
            /// is notified, when the response waiter is no longer needed; after completed,
            /// the instance must not be used any more
            /// </summary>
            void Completed();
        }        
        
        internal class SynchronousResponseWaiter : IResponseWaiter {
            
            private Stream m_response;
            private Exception m_problem;
            private ManualResetEvent m_waiter;            
            private GiopTransportMessageHandler m_handler;
            
            public SynchronousResponseWaiter(GiopTransportMessageHandler handler) {   
                m_waiter = new ManualResetEvent(false);
                m_handler = handler;
            }
            
            /// <summary>
            /// the response if successfully completed, otherwise null.
            /// </summary>
            public Stream Response {
                get {
                    return m_response;
                }
                set {
                    m_response = value;
                }
            }
            
            /// <summary>
            /// the problem, if one has occured
            /// </summary>
            public Exception Problem {
                get {
                    return m_problem;    
                }
                set {
                    m_problem = value;
                }
            }
            
            public void Notify() {
                m_waiter.Set();
            }
            
            public bool StartWaiting() {
                return m_handler.WaitForEvent(m_waiter);
            }
            
            public void Completed() {
                // dispose this handle
                m_waiter.Close();
            }
            
        }
    
        internal class AsynchronousResponseWaiter : IResponseWaiter {
            
            private GiopTransportMessageHandler m_transportHandler;
            private Stream m_response;
            private Exception m_problem;
            private AsyncResponseAvailableCallBack m_callback;
            private IClientChannelSinkStack m_clientSinkStack;
            private GiopClientConnection m_clientConnection;
            private Timer m_timer;
            private MessageTimeout m_timeOut;
            private volatile bool m_alreadyNotified;
            private uint m_requestId;
                        
            internal AsynchronousResponseWaiter(GiopTransportMessageHandler transportHandler,
                                                uint requestId,
                                                AsyncResponseAvailableCallBack callback,
                                                IClientChannelSinkStack clientSinkStack,
                                                GiopClientConnection connection, 
                                                MessageTimeout timeOut) {
                Initalize(transportHandler, requestId, callback, clientSinkStack, connection, timeOut);
            }            
                        
            
            /// <summary>
            /// the response if successfully completed, otherwise null.
            /// </summary>
            public Stream Response {
                get {
                    return m_response;
                }
                set {
                    m_response = value;
                }
            }
            
            /// <summary>
            /// the problem, if one has occured
            /// </summary>
            public Exception Problem {
                get {
                    return m_problem;    
                }
                set {
                    m_problem = value;
                }
            }            
            
            private void Initalize(GiopTransportMessageHandler transportHandler,
                                   uint requestId,
                                   AsyncResponseAvailableCallBack callback,
                                   IClientChannelSinkStack clientSinkStack,
                                   GiopClientConnection connection, 
                                   MessageTimeout timeOutMillis) {
                m_alreadyNotified = false;                
                m_transportHandler = transportHandler;
                m_requestId = requestId;
                m_callback = callback;
                m_clientConnection = connection;
                m_clientSinkStack = clientSinkStack;
                m_timeOut = timeOutMillis;                
            }                            
            
            public void Notify() {
                if (!m_alreadyNotified) {
                    lock(this) {
                        if (!m_alreadyNotified) {
                            m_alreadyNotified = true;
                            Completed();
                            m_callback(m_clientSinkStack, m_clientConnection,
                                       Response, Problem);
                        }
                    }
                }
                
            }
            
            public bool StartWaiting() {
                if (!m_timeOut.IsUnlimited) {
                    m_timer = new Timer(new TimerCallback(this.TimeOutCallback),
                                        null,
                                        (int)Math.Round(m_timeOut.TimeOut.TotalMilliseconds),
                                        Timeout.Infinite);                    
                }
                return true;
            }
            
            public void Completed() {
                if (m_timer != null) {                    
                    m_timer.Dispose();
                }
                // nothing special to do
            }            
            
            private void TimeOutCallback(object state) {
                if (!m_alreadyNotified) {
                    lock(this) {
                        if (!m_alreadyNotified) {
                            m_alreadyNotified = true;                            
                            m_transportHandler.CancelWaitForResponseMessage(m_requestId);
                            Completed();
                            m_transportHandler.ForceCloseConnection(); // close the connection after a timeout
                            m_callback(m_clientSinkStack, m_clientConnection,
                                       null, new TIMEOUT(32, CompletionStatus.Completed_MayBe));
                        }
                    }
                }                
            }

            
        }
        
        #endregion ResponseWaiter-Types
        #region IFields
                
        private ITransport m_transport;
        private MessageTimeout m_timeout;
        private AutoResetEvent m_writeLock;
        
        private IDictionary m_waitingForResponse = new ListDictionary();
        private FragmentedMessageAssembler m_fragmentAssembler =
            new FragmentedMessageAssembler();

        private MessageReceiveTask m_messageReceiveTask;
        private MessageSendTask m_messageSendTask;
        
        private GiopReceivedRequestMessageDispatcher m_reiceivedRequestDispatcher;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>creates a giop transport message handler, which doesn't accept request messages</summary>
        internal GiopTransportMessageHandler(ITransport transport) : this(transport, MessageTimeout.Infinite) {            
        }
        
        /// <summary>creates a giop transport message handler, which doesn't accept request messages</summary>
        internal GiopTransportMessageHandler(ITransport transport, MessageTimeout timeout) {            
            Initalize(transport, timeout);
        }
        
        #endregion IConstructors
        #region IProperties
        
        internal ITransport Transport {
            get {
                return m_transport;
            }
        }
        
        #endregion IProperties
        #region IMethods        
        
        private void Initalize(ITransport transport, MessageTimeout timeout) {
            m_transport = transport;            
            m_timeout = timeout;            
            m_writeLock = new AutoResetEvent(true);
            m_reiceivedRequestDispatcher = null;
            m_messageSendTask = new MessageSendTask(m_transport, this);
        }    
                
        #region Synchronization
        
        /// <summary>
        /// wait for an event, considering timeout.
        /// </summary>
        /// <returns>true, if ok, false if timeout occured</returns>
        internal bool WaitForEvent(WaitHandle waiter) {
            if (!m_timeout.IsUnlimited) {
                return waiter.WaitOne(m_timeout.TimeOut, false);
            } else {
                return waiter.WaitOne();
            }
        }
        
        private bool WaitForWriteLock() {
            return WaitForEvent(m_writeLock); // wait for the right to write to the stream
        }
        
        #endregion Synchronization
        #region ConnectionClose
        
        /// <summary>
        /// close a connection, make sure, that no read task is pending.
        /// </summary>
        internal void ForceCloseConnection() {
            try {
                m_transport.CloseConnection();                
            } catch (Exception ex) {
                Trace.WriteLine("problem to close connection: " + ex);
            }
            try {
                StopMessageReception();
            } catch (Exception ex) {
                Trace.WriteLine("problem to stop message reception: " + ex);
            }
        }
        
        #endregion ConnectionClose
        #region Exception handling
        
        private void CloseConnectionAfterTimeout() {
            try {
                Trace.WriteLine("closing connection because of timeout");
                ForceCloseConnection();
            } catch (Exception ex) {
                Debug.WriteLine("exception while trying to close connection after a timeout: " + ex);
            }
        }
        
        private void CloseConnectionAfterUnexpectedException(Exception uex) {
            try {
                Trace.WriteLine("closing connection because of unexpected exception: " + uex);
                ForceCloseConnection();
            } catch (Exception ex) {
                Debug.WriteLine("exception while trying to close connection: " + ex);
            }
        }
        
        #endregion Exception handling
        #region Cancel Pending
        
        /// <summary>abort all requests, which wait for a reply</summary>
        private void AbortAllPendingRequestsWaiting() {
            lock (m_waitingForResponse.SyncRoot) {
                try {
                    foreach (DictionaryEntry entry in m_waitingForResponse) {
                        try {
                            IResponseWaiter waiter = (IResponseWaiter)entry.Value;
                            waiter.Problem = new omg.org.CORBA.COMM_FAILURE(209, CompletionStatus.Completed_MayBe);
                            waiter.Notify();
                        } catch (Exception ex) {
                            Debug.WriteLine("exception while aborting message: " + ex);
                        }
                    }
                    m_waitingForResponse.Clear();
                } catch (Exception) {
                    // ignore
                }
            }
        }
        
        /// <summary>
        /// deregister the waiter for the given requestId
        /// </summary>        
        internal void CancelWaitForResponseMessage(uint requestId) {
            lock (m_waitingForResponse.SyncRoot) {
                // deregister waiter
                m_waitingForResponse.Remove(requestId);
            }
        }        
        
        #endregion Cancel Pending
        #region Sending messages

        /// <summary>
        /// sets the stream to offset 0. Returns the length of the stream.
        /// </summary>                
        private long PrepareStreamToSend(Stream stream) {
            stream.Seek(0, SeekOrigin.Begin);
            return stream.Length;
        }
                        
        private void SendMessage(Stream message) {            
            long bytesToSend = PrepareStreamToSend(message);
            bool gotLock = WaitForWriteLock();
            if (!gotLock) {
                CloseConnectionAfterTimeout();
                Debug.WriteLine("failed to send async request message due to timeout while trying to start writing");
                throw new TIMEOUT(32, CompletionStatus.Completed_No);
            }
            try {
                m_messageSendTask.Send(message, bytesToSend);
            } catch (Exception ex) {
                Trace.WriteLine("problem while writing message: " + ex);
                // close connection
                CloseConnectionAfterUnexpectedException(ex);
            } finally {
                try {
                    // message send completed, signal availability of write lock
                    m_writeLock.Set();
                } catch (Exception ex) {
                    CloseConnectionAfterUnexpectedException(ex);
                }
            }
        }
        
        /// <summary>
        /// send a message as a result to an incoming message
        /// </summary>
        internal void SendResponse(Stream responseStream) {
            SendMessage(responseStream);
        }
        
        /// <summary>
        /// sends a giop error message as result of a problematic message
        /// </summary>
        internal void SendErrorResponseMessage() {
            GiopVersion version = new GiopVersion(1, 2); // use highest number supported
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            Stream messageErrorStream = handler.PrepareMessageErrorMessage(version);
            SendResponse(messageErrorStream);
        }
        
        /// <summary>
        /// send a close connection message to the peer.
        /// </summary>
        internal void SendConnectionCloseMessage() {
            GiopVersion version = new GiopVersion(1, 0);
            GiopMessageHandler handler = GiopMessageHandler.GetSingleton();
            Stream messageCloseStream = handler.PrepareMessageCloseMessage(version);
            SendMessage(messageCloseStream);
        }
                
        /// <summary>
        /// sends the request and blocks the thread until the response message
        /// has arravied or a timeout has occured.
        /// </summary>
        /// <returns>the response stream</returns>
        internal Stream SendRequestSynchronous(Stream requestStream, uint requestId) {            
            // interested in a response -> register to receive response.
            // this must be done before sending the message, because otherwise,
            // it would be possible, that a response arrives before being registered
            IResponseWaiter waiter;
            lock (m_waitingForResponse.SyncRoot) {
                // create and register wait handle
                waiter = new SynchronousResponseWaiter(this);
                if (!m_waitingForResponse.Contains(requestId)) {
                    m_waitingForResponse[requestId] = waiter;
                } else {
                    throw new omg.org.CORBA.INTERNAL(40, CompletionStatus.Completed_No);
                }
            }            
            SendMessage(requestStream);
            // wait for completion or timeout                
            bool received = waiter.StartWaiting();
            waiter.Completed();
            if (received) {
                // get and return the message
                if (waiter.Problem != null) {
                    throw waiter.Problem;
                } else if (waiter.Response != null) {
                    return waiter.Response;
                } else {
                    throw new INTERNAL(41, CompletionStatus.Completed_MayBe);
                }
            } else {
                CancelWaitForResponseMessage(requestId);
                CloseConnectionAfterTimeout();
                throw new omg.org.CORBA.TIMEOUT(31, CompletionStatus.Completed_MayBe);
            }            
        }
        
        internal void SendRequestMessageOneWay(Stream requestStream, uint requestId) {
            SendMessage(requestStream);
            // no answer expected.
        }
        
        internal void SendRequestMessageAsync(Stream requestStream, uint requestId,
                                              AsyncResponseAvailableCallBack callback,
                                              IClientChannelSinkStack clientSinkStack,
                                              GiopClientConnection connection) {            
            // interested in a response -> register to receive response.
            // this must be done before sending the message, because otherwise,
            // it would be possible, that a response arrives before being registered            
            IResponseWaiter waiter;
            lock (m_waitingForResponse.SyncRoot) {
                // create and register wait handle                
                waiter = new AsynchronousResponseWaiter(this, requestId, callback, clientSinkStack, connection,
                                                        m_timeout);
                if (!m_waitingForResponse.Contains(requestId)) {
                    m_waitingForResponse[requestId] = waiter;
                } else {
                    throw new omg.org.CORBA.INTERNAL(40, CompletionStatus.Completed_No);
                }
            }            
            SendMessage(requestStream);
            // wait for completion or timeout
            waiter.StartWaiting(); // notify the waiter, that the time for the request starts; is non-blocking
        }
                                
        #endregion Sending messages
        #region Receiving messages          
                
        /// <summary>
        /// begins receiving messages asynchronously
        /// </summary>
        internal void StartMessageReception() {
            lock(this) {
                if (m_messageReceiveTask == null) {
                    m_messageReceiveTask = new MessageReceiveTask(m_transport, this);
                    m_messageReceiveTask.StartReceiveMessage();
                } // else ignore, already started
            }
        }
        
        /// <summary>
        /// abort receiving messages
        /// </summary>
        private void StopMessageReception() {
            lock(this) {
                m_messageReceiveTask = null;
            }
        }
                
        internal void MsgReceivedCallback(MessageReceiveTask messageReceived) {
            Stream messageStream = messageReceived.MessageStream;
            GiopHeader header = messageReceived.Header;
            if (FragmentedMessageAssembler.IsFragmentedMessage(header)) {
                // defragment
                if (FragmentedMessageAssembler.IsStartFragment(header)) {
                    m_fragmentAssembler.StartFragment(messageStream);
                    messageReceived.StartReceiveMessage(); // receive next message
                    return; // wait for next callback
                } else if (!FragmentedMessageAssembler.IsLastFragment(header)) {
                    m_fragmentAssembler.AddFragment(messageStream);
                    messageReceived.StartReceiveMessage(); // receive next message
                    return; // wait for next callback                    
                } else {
                    messageStream = m_fragmentAssembler.FinishFragmentedMsg(messageStream, out header);
                }                
            }
            
            // here, the message is no longer fragmented, don't check for fragment here
            switch (header.GiopType) {
                case GiopMsgTypes.Request:
                    EnqueueRequestMessage(messageStream); // put message into processing queue
                    messageReceived.StartReceiveMessage(); // receive next message (non_blocking)
                    ProcessQueuedRequests(); // make sure that somebody is processing requests; otherwise do it
                    break;
                case GiopMsgTypes.LocateRequest:
                    EnqueueLocateRequestMessage(messageStream); // put message into processing queue
                    messageReceived.StartReceiveMessage(); // receive next message (non-blocking)
                    ProcessQueuedRequests(); // make sure that somebody is processing requests; otherwise do it
                    break;
                case GiopMsgTypes.Reply:
                    // see, if somebody is interested in the response
                    lock (m_waitingForResponse.SyncRoot) {
                        uint replyForRequestId = ExtractRequestIdFromReplyMessage(messageStream);
                        IResponseWaiter waiter = (IResponseWaiter)m_waitingForResponse[replyForRequestId];
                        if (waiter != null) {
                            m_waitingForResponse.Remove(replyForRequestId);
                            waiter.Response = messageStream;
                            waiter.Notify();
                        }
                    }
                    
                    messageReceived.StartReceiveMessage(); // receive next message
                    break;
                case GiopMsgTypes.LocateReply:
                    // ignore, not interesting
                    messageReceived.StartReceiveMessage(); // receive next message
                    break;
                case GiopMsgTypes.CloseConnection:
                    m_transport.CloseConnection();
                    AbortAllPendingRequestsWaiting(); // if requests are waiting for a reply, abort them
                    break;
                case GiopMsgTypes.CancelRequest:
                    CdrInputStreamImpl input = new CdrInputStreamImpl(messageStream);
                    GiopHeader cancelHeader = new GiopHeader(input);
                    uint requestIdToCancel = input.ReadULong();
                    m_fragmentAssembler.CancelFragmentsIfInProgress(requestIdToCancel);
                    messageReceived.StartReceiveMessage(); // receive next message
                    break;                
                case GiopMsgTypes.MessageError:                    
                    CloseConnectionAfterUnexpectedException(new MARSHAL(16, CompletionStatus.Completed_MayBe));
                    AbortAllPendingRequestsWaiting(); // if requests are waiting for a reply, abort them
                    break;
                default:
                    // should not occur; 
                    // hint: fragment is also considered as error here,
                    // because fragment should be handled before this loop                    
                    
                    // send message error
                    SendErrorResponseMessage();
                    messageReceived.StartReceiveMessage(); // receive next message
                    break;
            }                                    
        }                
        
        internal void MsgReceivedCallbackException(Exception ex) {
            try {                
                if (ex is omg.org.CORBA.MARSHAL) {
                    // Giop header was not ok
                    // send a message error, something wrong with the message format
                    SendErrorResponseMessage();
                }                
                CloseConnectionAfterUnexpectedException(ex);
            } catch (Exception) {                
            }                 
            AbortAllPendingRequestsWaiting(); // if requests are waiting for a reply, abort them            
        }        
        
        /// <summary>
        /// called, when the connection has been closed while receiving a message
        /// </summary>        
        internal void MsgReceivedConnectionClosedException() {
            Trace.WriteLine("connection closed while trying to read a message");
            try {
                m_transport.CloseConnection();
            } catch (Exception) {                
            }
            AbortAllPendingRequestsWaiting(); // if requests are waiting for a reply, abort them
        }
        
        
        /// <summary>
        /// allows to install a receiver when ready to process messages.
        /// </summary>        
        /// <remarks>used for bidirectional communication.</remarks>
        internal void InstallReceiver(IGiopRequestMessageReceiver receiver, GiopConnectionDesc receiverConDesc) {
            lock(this) {
                if (m_reiceivedRequestDispatcher == null) {
                    m_reiceivedRequestDispatcher = 
                        new GiopReceivedRequestMessageDispatcher(receiver, this, receiverConDesc);
                }
            }
        }

        
        /// <summary>
        /// adds this request to the currently pending ones.
        /// </summary>
        private void EnqueueRequestMessage(Stream requestStream) {
            lock(this) {
                if (m_reiceivedRequestDispatcher == null) {
                    SendErrorResponseMessage(); // can't be handled, no dispatcher
                    return;
                }
            }
            m_reiceivedRequestDispatcher.EnqueueRequestMessage(requestStream);
        }

        /// <summary>
        /// adds this request to the currently pending ones.
        /// </summary>
        private void EnqueueLocateRequestMessage(Stream requestStream) {
            lock(this) {
                if (m_reiceivedRequestDispatcher == null) {
                    SendErrorResponseMessage(); // can't be handled, no dispatcher
                    return;
                }
            }
            m_reiceivedRequestDispatcher.EnqueueLocateRequestMessage(requestStream);
        }
        
        /// <summary>
        /// makes sure, that the queued requests get processed. If currently nobody is processing the requests,
        /// the caller is promoted to the requests processor. When the queue gets emtpy again, the caller is 
        /// released. When the next requests arrives, a new thread is promoted to request processor.
        /// </summary>
        private void ProcessQueuedRequests() {
            lock(this) {                
                if (m_reiceivedRequestDispatcher == null) {
                    return;
                }
            }
            m_reiceivedRequestDispatcher.ProcessQueuedRequests();
        }
        
        /// <summary>
        /// extracts the request id from a non-fragmented reply message
        /// </summary>
        /// <param name="replyMessage"></param>
        private uint ExtractRequestIdFromReplyMessage(Stream replyMessage) {
            replyMessage.Seek(0, SeekOrigin.Begin);
            CdrInputStreamImpl reader = new CdrInputStreamImpl(replyMessage);
            GiopHeader msgHeader = new GiopHeader(reader);
            
            if (msgHeader.Version.IsBeforeGiop1_2()) {
                // GIOP 1.0 / 1.1, the service context collection preceeds the id
                SkipServiceContexts(reader);
            }
            return reader.ReadULong();
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
        
        #endregion Receiving messages        
        
        internal IPAddress GetPeerAddress() {
            return m_transport.GetPeerAddress();
        }        
        
        #endregion IMethods
    }
    
    
    
    /// <summary>
    /// stores and dispatches reveiced giop requests (Request, LocateRequest).
    /// </summary>
    public class GiopReceivedRequestMessageDispatcher {
        
        #region Types
               
        /// <summary>
        /// encapsulates a request message to process.
        /// </summary>
        private abstract class MessageToProcess {
            
            private Stream m_messageStream;            
            
            protected MessageToProcess(Stream messageStream) {
                m_messageStream = messageStream;
            }
            
            protected Stream MessageStream {
                get {
                    return m_messageStream;
                }
            }
            
            /// <summary>processes this request</summary>
            internal abstract void Process(IGiopRequestMessageReceiver receiver, 
                                           GiopTransportMessageHandler transportHandler,
                                           GiopConnectionDesc connectionDesc);
            
        }
        
        private class Request : MessageToProcess {

            internal Request(Stream messageStream) : base(messageStream) {                
            }
                        
            internal override void Process(IGiopRequestMessageReceiver receiver, 
                                           GiopTransportMessageHandler transportHandler,
                                           GiopConnectionDesc connectionDesc) {
                receiver.ProcessRequest(MessageStream, transportHandler, connectionDesc);
            }

            
        }
        
        private class LocateRequest : MessageToProcess {

            internal LocateRequest(Stream messageStream) : base(messageStream) {                
            }            
            
            internal override void Process(IGiopRequestMessageReceiver receiver,
                                           GiopTransportMessageHandler transportHandler,
                                           GiopConnectionDesc connectionDesc) {
                receiver.ProcessLocateRequest(MessageStream, transportHandler, connectionDesc);
            }
            
        }
        
        #endregion Types
        #region IFields
        
        private IGiopRequestMessageReceiver m_receiver;
        // the connection desc for the handled connection.
        private GiopConnectionDesc m_conDesc;
        
        private GiopTransportMessageHandler m_msgHandler;
        
        /// <summary>
        /// the buffered requests.
        /// </summary>
        private Queue m_requestQueue = new Queue();
        
        /// <summary>
        /// is currently a requestProcessing in progress.
        /// </summary>
        private bool m_processing = false;        
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>creates a giop transport message handler, which accept request messages by delegating to receiver</summary>
        internal GiopReceivedRequestMessageDispatcher(IGiopRequestMessageReceiver receiver, GiopTransportMessageHandler msgHandler,
                                                      GiopConnectionDesc conDesc) {
            m_conDesc = conDesc;
            if (receiver != null) {
                m_receiver = receiver;
            } else {
                throw new BAD_PARAM(400, CompletionStatus.Completed_MayBe);
            }
            m_msgHandler = msgHandler;
        }        
                
        #endregion IConstructors
        #region IMethods
                               
        /// <summary>
        /// enqueues a request message for dispatching.
        /// </summary>        
        internal void EnqueueRequestMessage(Stream requestStream) {
            lock(this) {
                MessageToProcess req = new Request(requestStream);
                m_requestQueue.Enqueue(req);
            }
        }

        /// <summary>
        /// enqueues a locate request message for dispatching.
        /// </summary>                
        internal void EnqueueLocateRequestMessage(Stream requestStream) {
            lock(this) {
                MessageToProcess req = new LocateRequest(requestStream);
                m_requestQueue.Enqueue(req);
            }
        }
        
        internal void ProcessQueuedRequests() {
            lock(this) {
                if (!m_processing) {
                    // start processing: promote this thread to request processor.
                    m_processing = true;
                } else {
                    return; // already processing -> return
                }
            }
            Process(); // promote this thread to request processor, because currently nobody processing.
        }        
                        
        /// <summary>
        /// processes requests incoming from one connection in order (otherwise problems with codeset establishment).
        /// </summary>        
        /// <remarks>
        /// - Reduces the number of thread switches if many requests are concurrently arriving.
        /// - If no more requests are pending, release thread for other tasks.
        /// - called by the thread-pool
        /// </remarks>
        private void Process() {
            try {
                while (true) {
                    MessageToProcess req;
                    lock(this) {
                        // get next request to process.
                        if (!(m_requestQueue.Count > 0)) {
                            m_processing = false; // use a new thread for messages arraving from now on ...
                            break; // nothing more to process
                        }
                        req = (MessageToProcess)m_requestQueue.Dequeue();
                    }
                    // process next request
                    req.Process(m_receiver, m_msgHandler, m_conDesc );
                }
            } catch (Exception) {
                // unexpected exception -> processing problem on this connection, close connection...
                try {
                    m_msgHandler.SendConnectionCloseMessage();
                } finally {
                    m_msgHandler.ForceCloseConnection();
                }
                // after this, no new thread should be started to process next requests, because
                // connection closed -> don't set m_processing to false here
            }
        }
                               
        #endregion IMethods
        
    }    
    
}
