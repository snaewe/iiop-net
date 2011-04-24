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
 
        void ProcessRequest(Stream requestStream, GiopServerConnection serverConnection);
 
        void ProcessLocateRequest(Stream requestStream, GiopServerConnection serverConnection);
 
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
                m_onTransport.Write(m_buffer, 0, bytesToSendInProgress);
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
 
        /// <summary>
        /// delegate determining the siganture of the closed event.
        /// </summary>
        public delegate void ConnectionClosedDelegate(GiopTransportMessageHandler sender,
                                                      EventArgs args);
 
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
        private byte m_headerFlags;
 
        /// <summary>
        /// This event informs about the closing of the underlying transport connection.
        /// </summary>
        internal event ConnectionClosedDelegate ConnectionClosed;
 
        #endregion IFields
        #region IConstructors
 
        /// <summary>creates a giop transport message handler, which doesn't accept request messages</summary>
        internal GiopTransportMessageHandler(ITransport transport, byte headerFlags) : this(transport, MessageTimeout.Infinite, headerFlags) {
        }
 
        /// <summary>creates a giop transport message handler, which doesn't accept request messages.
        /// A receiver must be installed first.</summary>
        /// <param name="transport">the transport implementation</param>
        /// <param name="timeout">the client side timeout for a request</param>
        /// <param name="headerFlags">the header flags to use for message created by giop transport.</param>
        internal GiopTransportMessageHandler(ITransport transport, MessageTimeout timeout,
                                             byte headerFlags) {
            Initalize(transport, timeout, headerFlags);
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
 
        private void Initalize(ITransport transport, MessageTimeout timeout,
                               byte headerFlags) {
            m_transport = transport;
            m_timeout = timeout;
            m_headerFlags = headerFlags;
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
        /// closes the connection.
        /// </summary>
        /// <remarks>Catches all exceptions during close.</remarks>
        private void CloseConnection() {
            try {
                m_transport.CloseConnection();
            } catch (Exception ex) {
                Trace.WriteLine("problem to close connection: " + ex);
            }
        }
 
        /// <summary>
        /// close a connection, make sure, that no read task is pending.
        /// </summary>
        internal void ForceCloseConnection() {
            CloseConnection();
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
            try {
                lock (m_waitingForResponse.SyncRoot) {
                    foreach (DictionaryEntry entry in m_waitingForResponse) {
                        try {
                            IResponseWaiter waiter = (IResponseWaiter)entry.Value;
                            waiter.Problem =
                                new omg.org.CORBA.COMM_FAILURE(
                                        CorbaSystemExceptionCodes.COMM_FAILURE_CONNECTION_DROPPED,
                                        CompletionStatus.Completed_MayBe);
                            waiter.Notify();
                        } catch (Exception ex) {
                            Debug.WriteLine("exception while aborting message: " + ex);
                        }
                    }
                    m_waitingForResponse.Clear();
                }
            } catch (Exception) {
                // ignore
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
 
        private Stream PrepareMessageErrorMessage(GiopVersion version) {
            Debug.WriteLine("create a message error message");
            Stream targetStream = new MemoryStream();
            GiopHeader header = new GiopHeader(version.Major, version.Minor, m_headerFlags, GiopMsgTypes.MessageError);
            header.WriteToStream(targetStream, 0);
            targetStream.Seek(0, SeekOrigin.Begin);
            return targetStream;
        }
 
        /// <summary>
        /// create a close connection message
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private Stream PrepareMessageCloseMessage(GiopVersion version) {
            Debug.WriteLine("create a close connection message");
            Stream targetStream = new MemoryStream();
            GiopHeader header = new GiopHeader(version.Major, version.Minor, m_headerFlags, GiopMsgTypes.CloseConnection);
            header.WriteToStream(targetStream, 0);
            targetStream.Seek(0, SeekOrigin.Begin);
            return targetStream;
        }
 
        /// <summary>
        /// sends a giop error message as result of a problematic message
        /// </summary>
        internal void SendErrorResponseMessage() {
            GiopVersion version = new GiopVersion(1, 2); // use highest number supported
            Stream messageErrorStream = PrepareMessageErrorMessage(version);
            SendResponse(messageErrorStream);
        }
 
        /// <summary>
        /// send a close connection message to the peer.
        /// </summary>
        internal void SendConnectionCloseMessage() {
            GiopVersion version = new GiopVersion(1, 0);
            Stream messageCloseStream = PrepareMessageCloseMessage(version);
            SendMessage(messageCloseStream);
        }
 
        /// <summary>
        /// sends the request and blocks the thread until the response message
        /// has arravied or a timeout has occured.
        /// </summary>
        /// <returns>the response stream</returns>
        internal Stream SendRequestSynchronous(Stream requestStream, uint requestId,
                                               GiopClientConnection connection) {
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
            connection.NotifyRequestSentCompleted();
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
 
        internal void SendRequestMessageOneWay(Stream requestStream, uint requestId,
                                               GiopClientConnection connection) {
            SendMessage(requestStream);
            // no answer expected.
            connection.NotifyRequestSentCompleted();
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
            connection.NotifyRequestSentCompleted();
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
                    ProcessRequestMessage(messageStream, messageReceived); // process this message
                    // don't receive next message here, new message reception is started by dispatcher at appropriate time
                    break;
                case GiopMsgTypes.LocateRequest:
                    ProcessLocateRequestMessage(messageStream, messageReceived); // process this message
                    // don't receive next message here, new message reception is started by dispatcher at appropriate time
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
                        } else {
                            Debug.WriteLine("received not expected reply for request with id " + replyForRequestId);
                        }
                    }
 
                    messageReceived.StartReceiveMessage(); // receive next message
                    break;
                case GiopMsgTypes.LocateReply:
                    // ignore, not interesting
                    messageReceived.StartReceiveMessage(); // receive next message
                    break;
                case GiopMsgTypes.CloseConnection:
                    CloseConnection();
                    AbortAllPendingRequestsWaiting(); // if requests are waiting for a reply, abort them
                    RaiseConnectionClosedEvent(); // inform about connection closure
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
                    RaiseConnectionClosedEvent(); // inform about connection closure
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
            RaiseConnectionClosedEvent(); // inform about connection closure
        }
 
        /// <summary>
        /// called, when the connection has been closed while receiving a message
        /// </summary>
        internal void MsgReceivedConnectionClosedException() {
            Trace.WriteLine("connection closed while trying to read a message");
            CloseConnection();
            AbortAllPendingRequestsWaiting(); // if requests are waiting for a reply, abort them
            RaiseConnectionClosedEvent(); // inform about closure
        }
 
        /// <summary>
        /// Notifies all intersted parties, that the connection has been closed and
        /// that the handler is no longer usable.
        /// </summary>
        private void RaiseConnectionClosedEvent() {
            try {
                ConnectionClosedDelegate toNotify = ConnectionClosed;
                if (toNotify != null) {
                    toNotify(this, EventArgs.Empty);
                }
            } catch (Exception ex) {
                // ignore this issue.
                Debug.WriteLine("issue while notifying about connection closed event: " + ex);
            }
        }
 
        /// <summary>
        /// allows to install a receiver when ready to process messages.
        /// </summary>
        /// <param name="serverThreadsMaxPerConnection">
        /// the maximum number of server threads used for processing requests on a multiplexed client connection.
        /// </param>
        /// <remarks>used for standalone server side and used for bidirectional communication.</remarks>
        internal void InstallReceiver(IGiopRequestMessageReceiver receiver, GiopConnectionDesc receiverConDesc,
                                      int serverThreadsMaxPerConnection) {
            lock(this) {
                if (m_reiceivedRequestDispatcher == null) {
                    m_reiceivedRequestDispatcher =
                        new GiopReceivedRequestMessageDispatcher(receiver, this, receiverConDesc,
                                                                 serverThreadsMaxPerConnection);
                }
            }
        }

 
        /// <summary>
        /// processes this request
        /// </summary>
        private void ProcessRequestMessage(Stream requestStream, MessageReceiveTask receiveTask) {
            lock(this) {
                if (m_reiceivedRequestDispatcher == null) {
                    SendErrorResponseMessage(); // can't be handled, no dispatcher
                    return;
                }
            }
            m_reiceivedRequestDispatcher.ProcessRequestMessage(requestStream, receiveTask);
        }

        /// <summary>
        /// processes this locate reqeuest message
        /// </summary>
        private void ProcessLocateRequestMessage(Stream requestStream, MessageReceiveTask receiveTask) {
            lock(this) {
                if (m_reiceivedRequestDispatcher == null) {
                    SendErrorResponseMessage(); // can't be handled, no dispatcher
                    return;
                }
            }
            m_reiceivedRequestDispatcher.ProcessLocateRequestMessage(requestStream, receiveTask);
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
                cdrIn.ReadULong(); // context id
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
 
        #region Constants
 
        private const int NO_RECEIVE_PENDING = 0;
        private const int RECEIVE_PENDING = 1;
 
        #endregion Constants
        #region IFields
 
        private IGiopRequestMessageReceiver m_receiver;
        private GiopServerConnection m_serverCon;
 
        private GiopTransportMessageHandler m_msgHandler;
 
        private MessageReceiveTask m_msgReceiveTask;
 
        private int m_receivePending = NO_RECEIVE_PENDING; // to be usable with interlocked no bool, but int
        private int m_requestsInProgress;
        /// <summary>
        /// how many requests are allowed in parallel.
        /// </summary>
        private int m_maxRequestsAllowedInParallel = 25;
 
        #endregion IFields
        #region IConstructors
 
        /// <summary>creates a giop transport message handler, which accept request messages by delegating to receiver</summary>
        /// <param name="serverThreadsMaxPerConnection">
        /// the maximum number of server threads used for processing requests on a multiplexed client connection.
        /// </param>
        internal GiopReceivedRequestMessageDispatcher(IGiopRequestMessageReceiver receiver, GiopTransportMessageHandler msgHandler,
                                                      GiopConnectionDesc conDesc, int serverThreadsMaxPerConnection) {
            m_serverCon = new GiopServerConnection(conDesc, this);
            if (receiver != null) {
                m_receiver = receiver;
            } else {
                throw new BAD_PARAM(400, CompletionStatus.Completed_MayBe);
            }
            m_msgHandler = msgHandler;
            if (serverThreadsMaxPerConnection < 1) {
                throw new BAD_PARAM(401, CompletionStatus.Completed_MayBe);
            }
            m_maxRequestsAllowedInParallel = serverThreadsMaxPerConnection;
        }

        #endregion IConstructors
        #region IMethods
 
        private void StartReadNextMessage() {
            // read next message
            m_msgReceiveTask.StartReceiveMessage();
            // don't perform anything after StartReceiveMessage, because a received message may
            // already be in ProcessRequestMessage now
        }
 
        /// <summary>notification from formatter; deserialise request has been completed.</summary>
        /// <remarks>it's now safe to read next request from transport,
        /// because message ordering constraints are fullfilled.
        /// Because of e.g. codeset negotiation and other session based mechanism,
        /// the request deserialisation must be done serialised;
        /// the servant dispatch can be done in parallel.</remarks>
        internal void NotifyDeserialiseRequestComplete() {
            if (m_requestsInProgress < m_maxRequestsAllowedInParallel) {
                StartReadNextMessage();
            } else {
                Interlocked.Exchange(ref m_receivePending, RECEIVE_PENDING);
            }
        }
 
        internal void HandleUnexpectedProcessingException() {
            // unexpected exception -> processing problem on this connection, close connection...
            try {
                m_msgHandler.SendConnectionCloseMessage();
            } finally {
                m_msgHandler.ForceCloseConnection();
            }
        }
 
        internal void ProcessRequestMessage(Stream requestStream, MessageReceiveTask receiveTask) {
            // invariant: only one thread processing the first 3 statments at one instance in time
            // (statement nr 3 can lead to a next read allowing another i/o completion thread
            // to call this method)
            try {
                // call is serialised, no need for a lock; at most one thread is reading messages from transport.
                Interlocked.Increment(ref m_requestsInProgress);
                m_msgReceiveTask = receiveTask;
                m_receiver.ProcessRequest(requestStream, m_serverCon);
                // in the mean time, deserialise request notification may have been sent.
                // -> multiple requests are possible in progress and execute the following code
                int receivePending = Interlocked.Exchange(ref m_receivePending, NO_RECEIVE_PENDING);
                Interlocked.Decrement(ref m_requestsInProgress);
                // ReceivePending will be again set to true, when a request has been deserialised
                // and too many requests are processing
                if (receivePending == RECEIVE_PENDING) {
                    // too many requests in parallel last time when trying to start receive
                    // in NotifyDeserialiseRequestComplete -> do it now
                    StartReadNextMessage();
                }
            } catch (Exception ex) {
                HandleUnexpectedProcessingException();
                Trace.WriteLine("stopped processing on server connection after unexpected exception: " + ex);
            }
        }
 
        internal void ProcessLocateRequestMessage(Stream requestStream, MessageReceiveTask receiveTask) {
            // this called is completely handled by IIOP.NET
            // -> no servant will be called and therefore whole request is processed before reading next message
            try {
                m_receiver.ProcessLocateRequest(requestStream, m_serverCon);
                receiveTask.StartReceiveMessage();
            } catch (Exception ex) {
                HandleUnexpectedProcessingException();
                Trace.WriteLine("stopped processing on server connection after unexpected exception: " + ex);
            }
        }

        #endregion IMethods

    }

}
