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
using omg.org.CORBA;

using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.CorbaObjRef;

namespace Ch.Elca.Iiop {


    /// <summary>this class manages outgoing client side connections</summary>
    internal class GiopClientConnectionManager {
        
        #region Types 
        
        /// <summary>used for connection management</summary>
        private class ConnectionUsageDescription {
            
            #region IFields

            private GiopClientConnection m_connection;
            private bool m_accessedSinceCheck;

            #endregion IFields
            #region IConstructors

            public ConnectionUsageDescription(GiopClientConnection connection) {
                m_accessedSinceCheck = true;
                m_connection = connection;
            }

            #endregion IConstructors
            #region IProperties

            public GiopClientConnection Connection {
                get { 
                    return m_connection; 
                }
                set { 
                    m_connection = value; 
                }
            }

            /// <summary>is this session used since last check</summary>
            public bool Accessed {
                get { 
                    return m_accessedSinceCheck; 
                }
                set { 
                    m_accessedSinceCheck = value; 
                }
            }

            #endregion IProperties

        }
        
        #endregion Types
        #region IFields
        
        private IClientTransportFactory m_transportFactory;

        private Timer m_destroyTimer;

        /// <summary>contains the available client connections </summary>
        private Hashtable m_availableclientConnections = new Hashtable();
    	
    	/// <summary>
    	///  contains the allocated connections
    	/// </summary>
    	/// <remarks>
    	/// key is the message, which will be sent
    	/// with the connection
    	/// </remarks>
    	private Hashtable m_allocatedConnections = new Hashtable();                
        
        #endregion IFields
        #region IConstructors
        
        internal GiopClientConnectionManager(IClientTransportFactory transportFactory) {
            m_transportFactory = transportFactory;
            
            TimerCallback timerDelegate = new TimerCallback(DestroyUnusedConnections);
            // Create a timer which invokes the session destroyer every 5 seconds, first call in 10 seconds
            m_destroyTimer = new Timer(timerDelegate, null, 10000, 5000);
        }                
        
        #endregion IConstructors
        
        ~GiopClientConnectionManager() {
            if (m_destroyTimer != null) { 
                m_destroyTimer.Dispose(); 
                m_destroyTimer = null;
            }
        }
        
        #region IMethods
        
        /// <summary>checks, if availabe connections contain one, which is usable</summary>
        /// <returns>the connection, if found, otherwise null.</returns>
        private GiopClientConnection GetFromAvailable(string connectionKey) {
            if (connectionKey == null) {
                return null;
            }
            
            lock(this) {
            
            ArrayList avConnections = (ArrayList) m_availableclientConnections[connectionKey];
            while ((avConnections != null) && (avConnections.Count > 0)) { // lock not needed for avConnections, because all using methods exclusive
                // connection must not be available for other clients if used by this one
                ConnectionUsageDescription connectionDesc = (ConnectionUsageDescription) avConnections[0];
                avConnections.Remove(connectionDesc);
                // connection must be connected to be usable, otherwise do not use it
                if ((connectionDesc.Connection).CheckConnected()) {
                    GiopClientConnection result = connectionDesc.Connection;
                    connectionDesc.Accessed = true;
                    return result;
                }
            }
            return null;
            
            }
        }
        
        
        /// <summary>allocation a connection for the message.</summary>
        internal GiopClientConnectionDesc AllocateConnectionFor(IMessage msg, Ior target) {
            GiopClientConnection result = null;
            lock(this) {
                if (target != null) {
                    string targetKey = m_transportFactory.GetEndpointKey(target);
                    result = GetFromAvailable(targetKey);

                    // if connection not reusable, create new one
                    if (result == null) {
                        IClientTransport transport =
                            m_transportFactory.CreateTransport(target);
                        // already open connection here, because GetConnectionFor 
                        // should returns an open connection (if not closed meanwhile)
                        transport.OpenConnection();
                        result = new GiopClientConnection(targetKey, transport);
                    }
                } else {
                    // should not occur?
                    throw new omg.org.CORBA.INTERNAL(995,
                                                     omg.org.CORBA.CompletionStatus.Completed_No);
                }                
                m_allocatedConnections[msg] = result;
            }
            
            return result.Desc;

        }
        
        internal void ReleaseConnectionFor(IMessage msg) {
            lock(this) {
                GiopClientConnection connection = 
                    (GiopClientConnection)m_allocatedConnections[msg];

                if (connection == null) {
                    throw new INTERNAL(11111, 
                                       CompletionStatus.Completed_MayBe);
                }
                // remove from allocated connections
                m_allocatedConnections.Remove(msg);

                // check if reusable
                if ((connection.ConnectionKey != null) && connection.CheckConnected() && 
                    connection.Desc.ReqNumberGen.IsAbleToGenerateNext()) {
                    ArrayList avConnections = (ArrayList) m_availableclientConnections[connection.ConnectionKey];
                    if (avConnections == null) {
                        avConnections = new ArrayList();
                        m_availableclientConnections.Add(connection.ConnectionKey, avConnections);
                    }
                    ConnectionUsageDescription desc = new ConnectionUsageDescription(connection);
                    avConnections.Add(desc);
                } else {
                    connection.CloseConnection(); // not usable further, because connection information not gettable
                }            	            	
            }
        }
        
        /// <summary>get the reserved connection for the message forMessage</summary>
    	/// <remarks>Prescondition: AllocateConnectionFor is already called for msg</remarks>
    	/// <returns>a client connection; for connection oriented transports, 
    	/// the transport has already been connected by the con-manager.</returns>
    	internal GiopClientConnection GetConnectionFor(IMessage forMessage) {
    		lock(this) {
    			return (GiopClientConnection) m_allocatedConnections[forMessage];
    		}
    	}

        
        /// <summary>generates the request id to use for the given message</summary>
        internal uint GenerateRequestId(IMessage msg, GiopClientConnectionDesc allocatedCon) {
            return allocatedCon.ReqNumberGen.GenerateRequestId();
        }
        
        /// <summary>Mark the connections as non-used since last check</summary>
        private void MarkNonUsed(ArrayList availableForUri) {
            lock(this) {                
                for (int i = 0; i< availableForUri.Count; i++) {
                    ((ConnectionUsageDescription)availableForUri[i]).Accessed = false;
                }
            }
        }
        
        /// <summary>chooses a non-used connection to destroy, if there is at least one</summary>
        private ConnectionUsageDescription GetConToDestroy(ArrayList availableForUri) {
            lock(this) {                
                for (int i = 0; i< availableForUri.Count; i++) {
                    if (!((ConnectionUsageDescription)availableForUri[i]).Accessed) {
                        return ((ConnectionUsageDescription)availableForUri[i]);
                    }
                }                
            }        
            return null;
        }                        
        
        private void DestroyUnusedConnections(Object state) {
            lock(this) {
                IEnumerator enumerator = m_availableclientConnections.Values.GetEnumerator();
                while (enumerator.MoveNext()) { // enumerator over all targets
                    ArrayList list = (ArrayList) enumerator.Current;                    
                    ConnectionUsageDescription toDestroy = GetConToDestroy(list);
                    if (toDestroy != null) {
                        list.Remove(toDestroy);                    
                        toDestroy.Connection.CloseConnection();
                    }
                    MarkNonUsed(list);
                }
            }
        }
        
        #endregion IMethods
        
    }



}
