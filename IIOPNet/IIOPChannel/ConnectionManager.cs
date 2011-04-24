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
 
        #region IFields
 
        protected IClientTransportFactory m_transportFactory;

        /// <summary>contains all connections opened by the client; key is the target of the connection;
        /// value is the ConnectionDesc instance</summary>
        private Hashtable m_allClientConnections /* string (target), List of GiopClientConnection */ = new Hashtable();
 
        /// <summary>contains all connections, which are currently availabe for sending a request.</summary>
        private Hashtable m_availableConnections /* string (target), List of GiopClientConnection */ = new Hashtable();
 
        /// <summary>
        ///  contains the allocated connections. key is the message, which will be sent
        /// with the connection, value is a ConnectionDesc instance
        /// </summary>
        private Hashtable /* IMessage, GiopClientConnection */ m_allocatedConnections = new Hashtable();
 
        private MessageTimeout m_requestTimeOut;
 
        private Timer m_destroyTimer;
        private TimeSpan m_connectionLifeTime;
        /// <summary>
        ///  the max. number of concurrent connections to the same target.
        /// </summary>
        private int m_maxNumberOfConnections;
        /// <summary>
        /// allow or disallow multiplexing of requests on the same connections, i.e. send another request also
        /// a previous request has not been completed yet (no response yet).
        /// </summary>
        private bool m_allowMultiplex;
 
        /// <summary>
        /// the number of requests, which should be at most multiplexed on one connection. If too many
        /// requests are multiplexed, try to wait for some to complete.
        /// </summary>
        private int m_maxNumberOfMultiplexedRequests;
 
        /// <summary>
        /// The default header flags used when creating transport related messages.
        /// </summary>
        protected byte m_headerFlags;
 
        #endregion IFields
        #region IConstructors
 
        internal GiopClientConnectionManager(IClientTransportFactory transportFactory, MessageTimeout requestTimeOut,
                                             int unusedKeepAliveTime, int maxNumberOfConnections, bool allowMultiplex,
                                             int maxNumberOfMultplexedRequests, byte headerFlags) {
            Initalize(transportFactory, requestTimeOut, unusedKeepAliveTime,
                      maxNumberOfConnections, allowMultiplex, maxNumberOfMultplexedRequests,
                      headerFlags);
        }
 
        #endregion IConstructors
 
        ~GiopClientConnectionManager() {
            CleanUp();
        }
 
        #region IMethods
 
        private void Initalize(IClientTransportFactory transportFactory, MessageTimeout requestTimeOut,
                               int unusedKeepAliveTime, int maxNumberOfConnections, bool allowMultiplex,
                               int maxNumberOfMultplexedRequests, byte headerFlags) {
            m_transportFactory = transportFactory;
            m_requestTimeOut = requestTimeOut;
            if (unusedKeepAliveTime != Timeout.Infinite) {
                m_connectionLifeTime = TimeSpan.FromMilliseconds(unusedKeepAliveTime);
                TimerCallback timerDelegate = new TimerCallback(DestroyUnusedConnections);
                // Create a timer which invokes the session destroyer every unusedKeepAliveTime
                m_destroyTimer = new Timer(timerDelegate, null, 2 * unusedKeepAliveTime, unusedKeepAliveTime);
            }
            m_maxNumberOfConnections = maxNumberOfConnections; // the max. number of concurrent connections to the same target.
            if (m_maxNumberOfConnections < 1) {
                throw new BAD_PARAM(579, CompletionStatus.Completed_MayBe);
            }
            m_allowMultiplex = allowMultiplex;
            m_maxNumberOfMultiplexedRequests = maxNumberOfMultplexedRequests;
            m_headerFlags = headerFlags;
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
 
        /// <summary>checks, if availabe connections contain one, which is usable. If yes, removes the connection
        /// from the available and returns it. </summary>
        /// <remarks>If unusable connections are found, closes and removes them from available and all connections.</remarks>
        /// <returns>the connection, if found, otherwise null.</returns>
        protected virtual GiopClientConnection GetFromAvailable(string connectionKey) {
            GiopClientConnection result = null;
            IList available = (IList)m_availableConnections[connectionKey];
            while ((available != null) && (available.Count > 0)) {
                GiopClientConnection con = (GiopClientConnection)available[available.Count - 1];
                available.RemoveAt(available.Count - 1);
                if (con.CanBeUsedForNextRequest()) {
                    result = con;
                    break;
                } else {
                    try {
                        if (con.CanCloseConnection()) {
                            con.CloseConnection();
                        }
                    } catch (Exception) {
                    } finally {
                        UnregisterConnection(connectionKey, con);
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
 
        protected virtual GiopClientInitiatedConnection CreateClientConnection(string targetKey, IClientTransport transport,
                                                              MessageTimeout requestTimeOut) {
            return new GiopClientInitiatedConnection(targetKey, transport, requestTimeOut, this, false,
                                                     m_headerFlags);
        }
 
        /// <summary>
        /// register a connections among the list of all connections. Must be called only, when having
        /// the lock on the connection manager instance.
        /// </summary>
        protected void RegisterConnection(string targetKey, GiopClientConnection con) {
            IList connections = (IList)m_allClientConnections[targetKey];
            if (connections == null) {
                connections = new ArrayList();
                m_allClientConnections[targetKey] = connections;
            }
            connections.Add(con);
        }
 
        /// <summary>
        /// unregister a connections among the list of all connections. Must be called only, when having
        /// the lock on the connection manager instance.
        /// </summary>
        protected virtual void UnregisterConnection(string targetKey, GiopClientConnection con) {
            IList connections = (IList)m_allClientConnections[targetKey];
            if (connections != null) {
                connections.Remove(con);
            }
        }
 
        /// <summary>
        /// register the connection as available for a new request. Must be called only, when having
        /// the lock on the connection manager instance, i.e. in a block lock(this).
        /// </summary>
        protected void SetConnectionAvailable(string targetKey, GiopClientConnection con) {
            IList available = (IList)m_availableConnections[targetKey];
            if (available == null) {
                available = new ArrayList();
                m_availableConnections[targetKey] = available;
            }
            if (!available.Contains(con)) { // possible, that this is called also if the connection is already available
                available.Add(con);
                Monitor.Pulse(this); // inform the next object waiting on connections becoming available.
            }
        }
 
        /// <summary>
        /// removes the connection from the available connections.
        /// </summary>
        protected void RemoveConnectionAvailable(string targetKey, GiopClientConnection con) {
            IList available = (IList)m_availableConnections[targetKey];
            if (available != null) {
                available.Remove(con);
            }
        }
 
        protected virtual bool CanInitiateNewConnection(string targetKey) {
            IList connectionsPresent = (IList)m_allClientConnections[targetKey];
            if (connectionsPresent == null) {
                return true;
            } else {
                return connectionsPresent.Count < m_maxNumberOfConnections;
            }
        }
 
        /// <summary>
        /// Allocate a connection for the message to the given target.
        /// </summary>
        private GiopClientConnection AllocateConnectionForTarget(IMessage msg, IIorProfile target,
                                                                 string targetKey,
                                                                 out uint requestNr) {
            GiopClientConnection result = null;
            GiopClientInitiatedConnection newConnection = null; // contains the new connection, if one is created
            lock(this) {
                while (result == null) {
                    result = GetFromAvailable(targetKey);
                    if (result != null) {
                        break;
                    } else {
                        // if no usable connection, create new one (if possible)
                        if (CanInitiateNewConnection(targetKey)) {
                            IClientTransport transport =
                                m_transportFactory.CreateTransport(target);
                            newConnection = CreateClientConnection(targetKey, transport, m_requestTimeOut);
                            result = newConnection;
                        } else {
                            // wait for connections to become available
                            Monitor.Wait(this); // wait for a new connection to become available.
                        }
                    }
                }
                result.IncrementNumberOfRequests();
                requestNr = result.Desc.ReqNumberGen.GenerateRequestId();
                m_allocatedConnections[msg] = result;
                if (newConnection != null) {
                    // Register the new connection, if everything went ok
                    RegisterConnection(targetKey, result);
                }
            }
            if (newConnection != null) {
                // open the connection now outside the locked session,
                // to allow other threads to access connection manager during
                // this lenghty operation.
                try {
                    newConnection.OpenConnection();
                } catch(Exception) {
                    lock(this) {
                        // clean up dead connection
                        UnregisterConnection(targetKey, newConnection);
                    }
                    throw;
                }
            }
            return result;
        }
 
        /// <summary>allocation a connection and reqNr on connection for the message.</summary>
        internal GiopClientConnectionDesc AllocateConnectionFor(IMessage msg, IIorProfile target,
                                                                out uint requestNr) {
            if (target != null) {
                string targetKey = m_transportFactory.GetEndpointKey(target);
                if (targetKey == null) {
                    throw new BAD_PARAM(1178, CompletionStatus.Completed_MayBe);
                }
                return AllocateConnectionForTarget(msg, target, targetKey, out requestNr).Desc;
            } else {
                // should not occur
                throw new BAD_PARAM(995,
                                    omg.org.CORBA.CompletionStatus.Completed_No);
            }
        }
 
        /// <summary>
        /// Notifies the connection manager, that a request has been completely sent on the connection.
        /// For non oneway messages, a reply is required before RequestOnConnectionCompleted is called.
        /// </summary>
        /// <remarks>if multiplexing is allowed, the connection can now be reused for a next request. This
        /// guarantuees, that the session based services like codeset work correctly.</remarks>
        internal void RequestOnConnectionSent(GiopClientConnection con) {
            if (m_allowMultiplex) {
                lock(this) {
                    if (con.NumberOfRequestsOnConnection < m_maxNumberOfMultiplexedRequests) {
                        // if currently not too many requests on the same connection, register already
                        // as available.
                        SetConnectionAvailable(con.ConnectionKey, con);
                    }
                }
            } // else: nothing to do, wait for request completion.
        }
 
        /// <summary>
        /// Notifies the connection manager, that the connection is no longer needed by the request, because
        /// the reply has been successfully received or an exception has occured.
        /// </summary>
        /// <remarks>if multiplexing is not allowed, the connection can now be reused for a next request.</remarks>
        internal void RequestOnConnectionCompleted(IMessage msg) {
            lock(this) {
                GiopClientConnection connection =
                    (GiopClientConnection)m_allocatedConnections[msg];
                if (connection != null) {
                    connection.UpdateLastUsedTime();
                    connection.DecrementNumberOfRequests();
                    // remove from allocated connections
                    m_allocatedConnections.Remove(msg);
                    // make sure, that connection is available again (must be called here in every case, because
                    // RequestOnConnectionSent has not set it for sure also for mutex allowed).
                    SetConnectionAvailable(connection.ConnectionKey, connection);
                }
                // else: nothing to do, because failed to register connection correctly
                // -> for simpler error handling, call this also, if something during allocation went wrong
            }
        }
 
        /// <summary>get the reserved connection for the message forMessage</summary>
        /// <remarks>Prescondition: AllocateConnectionFor is already called for msg</remarks>
        /// <returns>a client connection; for connection oriented transports,
        /// the transport has already been connected by the con-manager.</returns>
        internal GiopClientConnection GetConnectionFor(IMessage forMessage) {
            lock(this) {
                return ((GiopClientConnection) m_allocatedConnections[forMessage]);
            }
        }
 
        /// <summary>chooses a non-used connection to destroy, if there is at least one</summary>
        private GiopClientConnection GetConToDestroy(IList availableForTarget) {
            for (int i = 0; i< availableForTarget.Count; i++) {
                if (((GiopClientConnection)availableForTarget[i]).CanBeClosedAsIdle(m_connectionLifeTime)) {
                    return ((GiopClientConnection)availableForTarget[i]);
                }
            }
            return null;
        }
 
        private void DestroyUnusedConnections(Object state) {
            lock(this) {
                bool hasDestroyedConnections = false;
                foreach (DictionaryEntry de in m_availableConnections) {
                    IList list = (IList) de.Value;
                    GiopClientConnection toDestroy = GetConToDestroy(list);
                    if (toDestroy != null) {
                        list.Remove(toDestroy);
                        UnregisterConnection((string)de.Key, toDestroy);
                        try {
                            toDestroy.CloseConnection();
                        } catch (ThreadAbortException) {
                            throw;
                        } catch (Exception) {
                            // ignore
                        }
                        hasDestroyedConnections = true;
                    }
                }
                if (hasDestroyedConnections) {
                    Monitor.PulseAll(this); // inform all threads waiting on changes in connections
                }
            }
        }
 
        /// <summary>
        /// brutally close all currently open connections.
        /// </summary>
        protected virtual void CloseAllConnections() {
            lock(this) {
                foreach (IList consToTarget in m_allClientConnections.Values) {
                    if (consToTarget != null) {
                        foreach (GiopClientConnection con in consToTarget) {
                            try {
                                if (con.CanCloseConnection()) {
                                    con.CloseConnection();
                                }
                            } catch (Exception) {
                            }
                        }
                    }
                }
                m_allClientConnections.Clear();
                m_availableConnections.Clear();
            }
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
 
        /// <summary>contains all bidir connections (from this instance (server) to client); </summary>
        private Hashtable m_bidirConnections /* string (target), List of GiopClientConnection */ = new Hashtable();
 
        private object[] m_ownListenPoints = new object[0];
 
        private IGiopRequestMessageReceiver m_receptionHandler = null;
        private int m_serverThreadsMaxPerConnection;

        #endregion IFields
        #region IConstructors
 
        internal GiopBidirectionalConnectionManager(IClientTransportFactory transportFactory, MessageTimeout requestTimeOut,
                                                    int unusedKeepAliveTime,
                                                    int maxNumberOfConnections, bool allowMultiplex,
                                                    int maxNumberOfMultplexedRequests, byte headerFlags) :
            base(transportFactory, requestTimeOut, unusedKeepAliveTime, maxNumberOfConnections,
                 allowMultiplex, maxNumberOfMultplexedRequests, headerFlags) {
        }
 
        #endregion IConstructors
        #region IMethods
 
        #region UseCaseConForCallBack
 
        private bool IsBidirConnectionAlreadyRegistered(string conKey, GiopConnectionDesc receivedOnDesc) {
            IList known = (IList)m_bidirConnections[conKey];
            if (known != null) {
                foreach (GiopClientConnection con in known) {
                    if (con.Desc.TransportHandler == receivedOnDesc.TransportHandler) {
                        return true; // received on same transport handler to the same target -> same connection.
                    }
                }
            }
            return false;
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
                        if (!IsBidirConnectionAlreadyRegistered(conKey, receivedOnDesc)) {
                            // new / different connection for listen-point
                            GiopBidirInitiatedConnection connection =
                                new GiopBidirInitiatedConnection(conKey, receivedOnDesc.TransportHandler,
                                                                 this);
                            Trace.WriteLine(String.Format("register bidirectional connection to {0}", conKey));
                            IList cons = (IList)m_bidirConnections[conKey];
                            if (cons == null) {
                                cons = new ArrayList();
                                m_bidirConnections[conKey] = cons;
                            }
                            cons.Add(connection);
                            RegisterConnection(conKey, connection); // register the new connection
                            SetConnectionAvailable(conKey, connection);
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
                foreach (IList cons in m_bidirConnections.Values) {
                    if (cons != null) {
                        foreach (GiopBidirInitiatedConnection con in cons) {
                            con.SetConnectionUnusable(); // don't use this connection for new requests any more
                            RemoveConnectionAvailable(con.ConnectionKey, con);
                            UnregisterConnection(con.ConnectionKey, con);
                        }
                    }
                }
            }
        }
 
        protected override void UnregisterConnection(string targetKey, GiopClientConnection con) {
            base.UnregisterConnection(targetKey, con);
            IList known = (IList)m_bidirConnections[targetKey];
            if (known != null) {
                known.Remove(con); // remove connection, if it's among bidir.
            }
        }
 
        protected override bool CanInitiateNewConnection(string targetKey) {
            IList bidirKnown = (IList)m_bidirConnections[targetKey];
            return (((bidirKnown == null) || (bidirKnown.Count == 0)) // only initiate new connections, if bidir is not available
                    && base.CanInitiateNewConnection(targetKey)); // and base says ok
        }
 
        protected override void CloseAllConnections() {
            lock(this) {
                base.CloseAllConnections();
                m_bidirConnections.Clear();
            }
        }
 
        #endregion UseCaseConForCallBack
        #region UseCaseConInitiatedSupportingReceiveBidir
 
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.GiopClientConnectionManager.CreateClientConnection"></see>
        /// </summary>
        /// <remarks>for use case (2)</remarks>
        protected override GiopClientInitiatedConnection CreateClientConnection(string targetKey, IClientTransport transport,
                                                                                MessageTimeout requestTimeOut) {
            return new GiopClientInitiatedConnection(targetKey, transport, requestTimeOut, this, true,
                                                     m_headerFlags);
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
        internal void RegisterMessageReceptionHandler(IGiopRequestMessageReceiver receptionHandler,
                                                      int serverThreadsMaxPerConnection) {
            m_receptionHandler = receptionHandler;
            m_serverThreadsMaxPerConnection = serverThreadsMaxPerConnection;
        }
 
        /// <summary>configures a client initiated connection to receive callbacks.</summary>
        /// <remarks>for use case (2)</remarks>
        internal void SetupConnectionForBidirReception(GiopClientConnectionDesc conDesc) {
            if (m_receptionHandler != null) {
                if (conDesc.Connection is GiopClientInitiatedConnection) {
                    GiopTransportMessageHandler handler =
                        conDesc.Connection.TransportHandler;
                    handler.InstallReceiver(m_receptionHandler, conDesc,
                                            m_serverThreadsMaxPerConnection); // set, if not yet set.
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
