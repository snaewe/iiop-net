/* Connection.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 30.04.03  Dominic Ullmann (DUL), dul@elca.ch
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
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Ch.Elca.Iiop.Services;

namespace Ch.Elca.Iiop {


    /// <summary>
    /// Stores information associated with a GIOP connection,
    /// e.g. the Codesets chosen
    /// </summary>
    internal class GiopConnectionContext {

        #region IFields

        private IiopConnection m_assocConnection;

        private uint m_charSetChosen = CodeSetService.DEFAULT_CHAR_SET;
        private uint m_wcharSetChosen = CodeSetService.DEFAULT_WCHAR_SET;

        #endregion IFields
        #region IConstructors

        public GiopConnectionContext(IiopConnection con) {
            m_assocConnection = con;
        }

        #endregion IConstructors
        #region IProperties

        internal IiopConnection Connection {
            get {
                return m_assocConnection;
            }
        }

        internal uint CharSet {
            get {
                return m_charSetChosen;
            }
            set {
                m_charSetChosen = value;
            }
        }

        internal uint WCharSet {
            get {
                return m_wcharSetChosen;
            }
            set {
                m_charSetChosen = value;
            }
        }

        #endregion IProperties

    }

    /// <summary>
    /// Encapsulates a Tcp/ip Connection
    /// </summary>
    public abstract class IiopConnection {

        /// <summary>
        /// helper class for waiting for fragments
        /// </summary>
        class FragmentWaiter {
            
            #region IFields
            
            private uint m_bytesToFollow = 0;
            private byte m_flags = 0;

            #endregion IFields
            #region IProperties

            public uint BytesToFollow {
                get { 
                    return m_bytesToFollow; 
                }
                set { 
                    m_bytesToFollow = value; 
                }
            }
            
            public byte Flags {
                get { 
                    return m_flags; 
                }
                set { 
                    m_flags = value; 
                }
            }

            #endregion IProperties
        }

        #region IFields

        private Hashtable m_waitingThreads = Hashtable.Synchronized(new Hashtable());        

        #endregion IFields
        #region IConstructors

        public IiopConnection() {
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// specifies how to receive a message after the main request / reply message:
        /// this is used for example for fragments
        /// </summary>
        public abstract void ReceiveNextMessage();

        /// <summary>wait for the next fragment</summary>
        /// <returns>bytes in the fragmen</returns>
        public uint WaitForFragment(uint requestId, out byte newFlags) {
            FragmentWaiter waiter = new FragmentWaiter(); // create an object for receiving the signal
            m_waitingThreads.Add(requestId, waiter);

            lock (waiter) {
                // create a new Thread for handling next message 
                // using the ReceiveNextMessage method
                ThreadStart start = new ThreadStart(this.ReceiveNextMessage);
                Thread handleThread = new Thread(start);
                handleThread.Start();
                Monitor.Wait(waiter); // wait for signal on waiter
                newFlags = waiter.Flags;
                return waiter.BytesToFollow;
            }
        }

        /// <summary>is called if a fragment is received</summary>
        public void FragmetReceived(uint requestId, uint bytesToFollow, byte newFlags) {
            FragmentWaiter waiting = (FragmentWaiter) m_waitingThreads[requestId];
            if (waiting == null) { 
                throw new omg.org.CORBA.INV_FLAG(0, omg.org.CORBA.CompletionStatus.Completed_No); 
            }
            // signal
            lock(waiting) {
                waiting.BytesToFollow = bytesToFollow;
                waiting.Flags = newFlags;
                m_waitingThreads.Remove(requestId);
                Monitor.Pulse(waiting);
            }
        }

        #endregion IMethods

    }


    public class IiopServerConnection : IiopConnection {
    
        #region IFields

        private IiopServerTransportSink m_serverSink;
        private NetworkStream m_stream;

        #endregion IFiels
        #region IConstructors

        public IiopServerConnection(IiopServerTransportSink serverSink, NetworkStream stream) {
            m_serverSink = serverSink;
            m_stream = stream;
        }

        #endregion IConstructors
        #region IProperties

        internal IiopServerTransportSink TransportSink {
            get {
                return m_serverSink;
            }
        }

        internal NetworkStream Stream {
            get {
                return m_stream;
            }
        }

        #endregion IProperties
        #region IMethods

        public override void ReceiveNextMessage() {
            GiopConnectionContext context = IiopConnectionManager.GetCurrentConnectionContext();
            IiopServerConnection con = (IiopServerConnection)context.Connection;
            con.TransportSink.Process(con.Stream);
        }

        #endregion IMethods

    }

    
    public class IiopClientConnection : IiopConnection {

        #region IFields

        private GiopRequestNumberGenerator m_reqNumGen = new GiopRequestNumberGenerator();

        private IiopClientFormatterSink m_formatterSink;

        private GiopConnectionContext m_assocContext;

        private string m_chanUri;

        private TcpClient m_socket;
        private string m_host;
        private int m_port;
        private ProtectedNetworkStream m_stream;

        #endregion IFields
        #region IConstructors

        public IiopClientConnection(IiopClientFormatterSink sink, string chanUri) {
            m_formatterSink = sink;
            m_chanUri = chanUri;
        }

        #endregion IConstructors
        #region IProperties

        public GiopRequestNumberGenerator ReqNumberGen {
            get {
                return m_reqNumGen;
            }
        }

        internal GiopConnectionContext Context {
            get {
                return m_assocContext;
            }
            set {
                m_assocContext = value;
            }
        }

        internal string ChanUri {
            get {
                return m_chanUri;
            }
        }

        internal TcpClient ClientSocket {
            get {
                return m_socket;
            }
        }

        #endregion IProperties
        #region IMethods


        internal void ConnectTo(string host, int port) {
            TcpClient tcpSocket = new TcpClient(host, port);
            tcpSocket.NoDelay = false; // what is better here ?
            m_socket = tcpSocket;
            m_host = host;
            m_port = port;
            m_stream = new ProtectedNetworkStream(tcpSocket.GetStream());
        }

        internal bool CheckConnected() {
            try {
                m_socket.GetStream();
                return true;
            } catch (Exception) {
                // not connected any more
                return false;
            }
        }

        internal void CloseConnection() {
            try {
                m_socket.Close();
            } catch (Exception) { }
        }

        public override void ReceiveNextMessage() {
            IMessage result;
            try {
                IiopConnectionManager.SetCurrentConnectionContext(m_assocContext); // set the connection context for the invoking thread
                m_formatterSink.DeserialiseResponse(m_stream, null, null, out result);
            } 
            catch (Exception) {
                // TODO
            }
        }

        #endregion IMethods

    
    }


}
