/* ConnectionManager.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 28.04.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using omg.org.CORBA;

using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.CorbaObjRef;

namespace Ch.Elca.Iiop {


    /// <summary>this class manages outgoing client side connections</summary>
    internal class GiopClientConnectionManager : IDisposable {
        
        #region Types
        
        /// <summary>Encapsulates a connections, used for connection management</summary>
        protected class ConnectionDescription {
            
            #region IFields

            private GiopClientConnection m_connection;
            private DateTime m_lastUsed;            
            private bool m_isInUse;            

            #endregion IFields
            #region IConstructors

            public ConnectionDescription(GiopClientConnection connection) {
                m_lastUsed = DateTime.Now;
                m_connection = connection;                
            }

            #endregion IConstructors
            #region IProperties

            /// <summary>
            /// the encapsulated connection
            /// </summary>
            public GiopClientConnection Connection {
                get { 
                    return m_connection; 
                }
                set { 
                    m_connection = value; 
                }
            }
            
            /// <summary>
            /// can this connection be closed
            /// </summary>
            public bool IsAllowedToBeClosed {
                get {
                    return m_connection.CanCloseConnection();
                }
            }
            
            /// <summary>
            /// is this connection currenty used for a sending/receiving a giop message.
            /// </summary>
            public bool IsInUse {
                get {
                    return m_isInUse;
                }
                set {
                    m_isInUse = value;
                }
            }

            #endregion IProperties
            #region IMethods
            
            /// <summary>
            /// returns ture, if the connection is not use for at least the specified time; otherwise false.
            /// </summary>
            public bool IsNotUsedForAtLeast(TimeSpan idleTime) {
                return (m_lastUsed + idleTime < DateTime.Now);
            }
            
            public bool CanBeClosedAsIdle(TimeSpan idleTime) {
                return (IsNotUsedForAtLeast(idleTime) && IsAllowedToBeClosed &&
                        !IsInUse);
            }
            
            /// <summary>
            /// updates the time, this connection has been used last.
            /// </summary>
            public void UpdateLastUsedTime() {
                m_lastUsed = DateTime.Now;
            }
            
            #endregion IMethods

        }
        
        #endregion Types
        #region IFields
        
        protected IClientTransportFactory m_transportFactory;

        /// <summary>contains all connections opened by the client; key is the target of the connection; 
        /// value is the ConnectionDesc instance</summary>
        private Hashtable m_allClientConnections /* target, ConnectionDescription */ = new Hashtable();
    	
    	/// <summary>
    	///  contains the allocated connections. key is the message, which will be sent
    	/// with the connection, value is a ConnectionDesc instance
    	/// </summary>
    	private Hashtable /* IMessage, ConnectionDescription */ m_allocatedConnections = new Hashtable();
    	
    	private MessageTimeout m_requestTimeOut;
    	
    	private Timer m_destroyTimer;
    	private TimeSpan m_connectionLifeTime;
        
        #endregion IFields
        #region IConstructors                        
        
        internal GiopClientConnectionManager(IClientTransportFactory transportFactory, MessageTimeout requestTimeOut,
                                             int unusedKeepAliveTime) {
            Initalize(transportFactory, requestTimeOut, unusedKeepAliveTime);
        }
        
        internal GiopClientConnectionManager(IClientTransportFactory transportFactory, int unusedKeepAliveTime) :
            this(transportFactory, MessageTimeout.Infinite, unusedKeepAliveTime) {
        }
        
        #endregion IConstructors
        
        ~GiopClientConnectionManager() {
            CleanUp();
        }
        
        #region IMethods
        
        private void Initalize(IClientTransportFactory transportFactory, MessageTimeout requestTimeOut,
                               int unusedKeepAliveTime) {
            m_transportFactory = transportFactory;
            m_requestTimeOut = requestTimeOut;
            if (unusedKeepAliveTime != Timeout.Infinite) {
                m_connectionLifeTime = TimeSpan.FromMilliseconds(unusedKeepAliveTime);
                TimerCallback timerDelegate = new TimerCallback(DestroyUnusedConnections);
                // Create a timer which invokes the session destroyer every unusedKeepAliveTime
                m_destroyTimer = new Timer(timerDelegate, null, 2 * unusedKeepAliveTime, unusedKeepAliveTime);
            }
        }
        
        
        public void Dispose() {
            CleanUp();
            GC.SuppressFinalize(this);
        }
        
        private void CleanUp() {
            if (m_destroyTimer != null) {
                m_destroyTimer.Dispose();
                m_destroyTimer = null;
            }
            CloseAllConnections();
        }
        
        /// <summary>checks, if availabe connections contain one, which is usable</summary>
        /// <returns>the connection, if found, otherwise null.</returns>
        protected virtual ConnectionDescription GetFromAvailable(string connectionKey) {
            ConnectionDescription result = null;
            ConnectionDescription con = (ConnectionDescription)m_allClientConnections[connectionKey];
            if (con != null) {
                if (con.Connection.CheckConnected() && (con.Connection.Desc.ReqNumberGen.IsAbleToGenerateNext())) {
                    result = con;
                } else {
                    try {
                        con.Connection.CloseConnection();
                    } catch (Exception) {                            
                    } finally {
                        m_allClientConnections.Remove(connectionKey);
                    }
                }                
            }
            return result;
        }
        
        /// <summary>
        /// checks, if this connection manager is able to build up a connection to the given target ior
        /// </summary>
        internal bool CanConnectToIor(Ior target) {            
            return m_transportFactory.CanCreateTranporForIor(target);
        }
        
        /// <summary>
        /// checks, if this connection manager is able to build up a connection with the given target profile.
        /// </summary>
        internal bool CanConnectWithProfile(IIorProfile targetProfile) {
            return m_transportFactory.CanUseProfile(targetProfile);
        }
        
        private ConnectionDescription CreateAndRegisterNewConnection(IIorProfile target, string targetKey) {
            ConnectionDescription result;
            IClientTransport transport =
                m_transportFactory.CreateTransport(target);
            // already open connection here, because GetConnectionFor 
            // should returns an open connection (if not closed meanwhile)
            transport.OpenConnection();
            result = new ConnectionDescription(
                         CreateClientConnection(targetKey, transport, m_requestTimeOut));
            m_allClientConnections[targetKey] = result;
            return result;
        }      
        
        protected virtual GiopClientConnection CreateClientConnection(string targetKey, IClientTransport transport,
                                                              MessageTimeout requestTimeOut) {
            return new GiopClientInitiatedConnection(targetKey, transport, requestTimeOut, this, false);
        }
        
        /// <summary>allocation a connection and reqNr on connection for the message.</summary>
        internal GiopClientConnectionDesc AllocateConnectionFor(IMessage msg, IIorProfile target,
                                                                out uint requestNr) {
            ConnectionDescription result = null;
            
            if (target != null) {
                string targetKey = m_transportFactory.GetEndpointKey(target);
                if (targetKey == null) {
                    throw new BAD_PARAM(1178, CompletionStatus.Completed_MayBe);
                }
                lock(this) {
                    result = GetFromAvailable(targetKey);

                    // if no usable connection, create new one
                    if (result == null) {
                        result = CreateAndRegisterNewConnection(target, targetKey);
                    }
                    result.IsInUse = true;
                    m_allocatedConnections[msg] = result;
                    requestNr = result.Connection.Desc.ReqNumberGen.GenerateRequestId();
                }
            } else {
                // should not occur
                throw new BAD_PARAM(995,
                                    omg.org.CORBA.CompletionStatus.Completed_No);
            }
            return result.Connection.Desc;
        }
        
        internal void ReleaseConnectionFor(IMessage msg) {
            lock(this) {
                ConnectionDescription connection = 
                    (ConnectionDescription)m_allocatedConnections[msg];

                if (connection == null) {
                    throw new INTERNAL(11111, 
                                       CompletionStatus.Completed_MayBe);
                }
                connection.UpdateLastUsedTime();
                connection.IsInUse = false;
                // remove from allocated connections
                m_allocatedConnections.Remove(msg);
            }
        }
        
        /// <summary>get the reserved connection for the message forMessage</summary>
    	/// <remarks>Prescondition: AllocateConnectionFor is already called for msg</remarks>
    	/// <returns>a client connection; for connection oriented transports, 
    	/// the transport has already been connected by the con-manager.</returns>
    	internal GiopClientConnection GetConnectionFor(IMessage forMessage) {
    		lock(this) {
    	        return ((ConnectionDescription) m_allocatedConnections[forMessage]).Connection;
    		}
    	}
                        
        private void DestroyUnusedConnections(Object state) {
            lock(this) {
                ArrayList toClose = new ArrayList();
                foreach (DictionaryEntry de in m_allClientConnections) {
                    if (((ConnectionDescription)de.Value).CanBeClosedAsIdle(m_connectionLifeTime)) {
                        toClose.Add(de.Key);
                    }
                }
                foreach (object key in toClose) {
                    ConnectionDescription conDesc = (ConnectionDescription)m_allClientConnections[key];
                    m_allClientConnections.Remove(key);
                    try {
                        conDesc.Connection.CloseConnection();
                    } catch (Exception) {
                        // ignore
                    }
                }                
            }            
        }
        
        private void CloseAllConnections() {
            lock(this) {
                foreach (ConnectionDescription conDesc in m_allClientConnections.Values) {
                    try {
                        conDesc.Connection.CloseConnection();
                    } catch (Exception) {                
                    }
                }
            }
            m_allClientConnections.Clear();
        }
        
        /// <summary>
        /// supports this connection manager bidir connections.
        /// </summary>        
        internal virtual bool SupportBiDir() {
            return false;
        }
                
        #endregion IMethods
        
    }

    
    /// <summary>
    /// A connection manager, which is suitable for bidirectional channels.
    /// This connection manager handles two cases:
    /// - it manages bidir connections for callbacks (1)
    /// - it allows to setup client initiated connections for receiving callbacks. (2)
    /// </summary>
    internal class GiopBidirectionalConnectionManager : GiopClientConnectionManager {
        
        #region IFields
        
        /// <summary>contains all bidir connections (from this instance (server) to client); 
        /// key is the target of the connection; 
        /// value is the ConnectionDesc instance</summary>
        private Hashtable m_bidirConnections /* target, ConnectionDescription */ = new Hashtable();
        
        private object[] m_ownListenPoints = new object[0];
        
        private IGiopRequestMessageReceiver m_receptionHandler = null;

        #endregion IFields
        #region IConstructors
        
        internal GiopBidirectionalConnectionManager(IClientTransportFactory transportFactory, MessageTimeout requestTimeOut,
                                                    int unusedKeepAliveTime) : base(transportFactory, requestTimeOut, unusedKeepAliveTime) {
        }
        
        internal GiopBidirectionalConnectionManager(IClientTransportFactory transportFactory, int unusedKeepAliveTime) :
            this(transportFactory, MessageTimeout.Infinite, unusedKeepAliveTime) {
        }
        
        #endregion IConstructors
        #region IMethods
        
        #region UseCaseConForCallBack
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.GiopClientConnectionManager.GetFromAvailable"></see>
        /// </summary>
        /// <remarks>for use case (1)</remarks>
        protected override ConnectionDescription GetFromAvailable(string connectionKey) {            
            ConnectionDescription con;
            lock(this) {
                con = (ConnectionDescription)m_bidirConnections[connectionKey];
            }
            if ((con != null) &&
                (con.Connection.Desc.ReqNumberGen.IsAbleToGenerateNext())) {
                Trace.WriteLine(String.Format("GetConnection to {0}; using bidirectional connection", connectionKey));
                return con;
            } else {
                Trace.WriteLine(String.Format("GetConnection to {0}; create new connection", connectionKey));
                return base.GetFromAvailable(connectionKey);
            }

        }
        
        
        /// <summary>registeres connections from received listen points. Those connections
        /// can be used for callbacks.</summary>
        /// <remarks>for use case (1)</remarks>
        internal void RegisterBidirectionalConnection(GiopConnectionDesc receivedOnDesc, 
                                                      Array receivedListenPoints) {
            // ask transport factory to create the connection key for the listenPoints
            for (int i = 0; i < receivedListenPoints.Length; i++) {
                string conKey = 
                    m_transportFactory.GetEndPointKeyForBidirEndpoint(receivedListenPoints.GetValue(i));
                if (conKey != null) {
                    lock(this) {
                        ConnectionDescription conDesc = (ConnectionDescription)m_bidirConnections[conKey];
                        if ((conDesc == null) || 
                            ((conDesc != null) && 
                             (conDesc.Connection.Desc.TransportHandler != receivedOnDesc.TransportHandler))) {
                            // new / different connection for listen-point
                            GiopBidirInitiatedConnection connection =
                                new GiopBidirInitiatedConnection(conKey, receivedOnDesc.TransportHandler,
                                                                 this);                    
                            Trace.WriteLine(String.Format("register bidirectional connection to {0}", conKey));
                            m_bidirConnections[conKey] =
                                new ConnectionDescription(connection);
                        } else {
                            Trace.WriteLine(String.Format("received listen points for already registered bidirectional connection to {0}", conKey));
                        }
                    }
                }
            }            
        }
        
        /// <summary>
        /// remove all bidirectional connections. This should be called, when the server channel stops listening.
        /// </summary>
        /// <remarks>for use case (1)</remarks>
        internal void RemoveAllBidirInitiated() {
            lock(this) {
                m_bidirConnections.Clear();
            }
        }
        
        #endregion UseCaseConForCallBack
        #region UseCaseConInitiatedSupportingReceiveBidir
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.GiopClientConnectionManager.CreateClientConnection"></see>
        /// </summary>
        /// <remarks>for use case (2)</remarks>
        protected override GiopClientConnection CreateClientConnection(string targetKey, IClientTransport transport,
                                                                       MessageTimeout requestTimeOut) {
            return new GiopClientInitiatedConnection(targetKey, transport, requestTimeOut, this, true);
        }        
        
        /// <summary>sets the listen points, which can be sent to the other side for connecting back to
        /// this side endpoints.</summary>
        /// <remarks>for use case (2)</remarks>
        internal void SetOwnListenPoints(object[] ownListenPoints) {
            lock(this) {
                m_ownListenPoints = ownListenPoints;
            }
        }

        /// <summary>
        /// gets the own listen points, which can be sent to the other side for connecting back to this
        /// side endpoints.
        /// </summary>
        /// <remarks>for use case (2)</remarks>        
        internal object[] GetOwnListenPoints() {
            lock(this) {
                return m_ownListenPoints;
            }
        }
        
        /// <summary>the server channel entry point, which should be invoked, if a request is
        /// received on a bidir connection on the client side.</summary>
        /// <remarks>for use case (2)</remarks>
        internal void RegisterMessageReceptionHandler(IGiopRequestMessageReceiver receptionHandler) {
            m_receptionHandler = receptionHandler;
        }
        
        /// <summary>configures a client initiated connection to receive callbacks.</summary>
        /// <remarks>for use case (2)</remarks>
        internal void SetupConnectionForBidirReception(GiopClientConnectionDesc conDesc) {
            if (m_receptionHandler != null) {                
                if (conDesc.Connection is GiopClientInitiatedConnection) {
                    GiopTransportMessageHandler handler = 
                        conDesc.Connection.TransportHandler;
                    handler.InstallReceiver(m_receptionHandler, conDesc); // set, if not yet set.
                } else {
                    throw new INTERNAL(545, CompletionStatus.Completed_MayBe);
                }                
            }  else {
                throw new INTERNAL(544, CompletionStatus.Completed_MayBe);
            }
        }
        
        #endregion UseCaseConInitiatedSupportingReceiveBidir        
        
        /// <summary>
        /// supports this connection manager bidir connections.
        /// </summary>        
        internal override bool SupportBiDir() {
            return true;
        }        
        
        #endregion IMethods

        
    }


}
