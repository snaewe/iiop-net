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

namespace Ch.Elca.Iiop {

    /// <summary>
    /// base class for client and server TcpConnectionManager
    /// </summary>
    internal abstract class IiopConnectionManager {

        #region Constants

        protected const string CONNECTION_CONTEXT_KEY = "_iiopConnectionContext";

        #endregion Constants
        #region IConstructors

        protected IiopConnectionManager() {
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// Remove the current connection context
        /// </summary>
        internal static void RemoveCurrentConnectionContext() {
            if (CallContext.GetData(CONNECTION_CONTEXT_KEY) != null) {
                Stack contexts = (Stack)CallContext.GetData(CONNECTION_CONTEXT_KEY);
                contexts.Pop(); // remove active context (=topmost context)
                if (contexts.Count == 0) {
                    CallContext.FreeNamedDataSlot(CONNECTION_CONTEXT_KEY);
                }
            }
        }

        /// <summary>
        /// sets the active connection context
        /// </summary>
        internal static void SetCurrentConnectionContext(GiopConnectionContext context) {
            if (CallContext.GetData(CONNECTION_CONTEXT_KEY) != null) {
                Stack contexts = (Stack)CallContext.GetData(CONNECTION_CONTEXT_KEY);
                contexts.Push(context);
            } else {
                Stack contexts = new Stack();
                contexts.Push(context);
                CallContext.SetData(CONNECTION_CONTEXT_KEY, contexts);
            }
        }

        /// <summary>
        /// gets the active connection context
        /// </summary>
        /// <remarks>
        /// is only != null after SetCurrentConnectionContext is called
        /// </remarks>
        /// <returns></returns>
        public static GiopConnectionContext GetCurrentConnectionContext() {
            if (CallContext.GetData(CONNECTION_CONTEXT_KEY) != null) {
                Stack contexts = (Stack)CallContext.GetData(CONNECTION_CONTEXT_KEY);
                if (!(contexts.Count == 0)) {
                    return (GiopConnectionContext)contexts.Peek(); // active=top elem
                } else {
                    throw new INTERNAL(111, CompletionStatus.Completed_MayBe);
                }
            } else {
                throw new INTERNAL(111, CompletionStatus.Completed_MayBe); 
            }
        }

        #endregion IMethods
    
    }

    /// <summary>
    /// This class is repsonsible for providing support task for Tcp/Ip connections
    /// on server side.
    /// </summary>
    internal class IiopServerConnectionManager : IiopConnectionManager {

        #region SFields

        private static IiopServerConnectionManager s_singleton = new IiopServerConnectionManager();

        #endregion SFields
        #region SMethods

        /// <summary>
        /// gets the singleton instance of the connection-manager
        /// </summary>
        public static IiopServerConnectionManager GetManager() {
            return s_singleton;
        }

        #endregion SMethods
        #region IMethods
    
        internal GiopConnectionContext RegisterActiveConnection() {
            GiopConnectionContext context = new GiopConnectionContext();
            SetCurrentConnectionContext(context);
        	return context;
        }

        internal void UnregisterActiveConnection() {
            RemoveCurrentConnectionContext();   
        }

        #endregion IMethods
    
    }


    /// <summary>
    /// This class is repsonsible for opening / closing and assigning Tcp/Ip Iiop connections
    /// on client side.
    /// </summary>
    internal class IiopClientConnectionManager : IiopConnectionManager {

        #region Types 
        
        private class ConnectionUsageDescription {
            
            #region IFields

            private IiopClientConnection m_connection;
            private bool m_accessedSinceCheck;

            #endregion IFields
            #region IConstructors

            public ConnectionUsageDescription(IiopClientConnection connection) {
                m_accessedSinceCheck = true;
                m_connection = connection;
            }

            #endregion IConstructors
            #region IProperties

            public IiopClientConnection Connection {
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
        #region Constants
        
        private const string MSG_URI_KEY = "__Uri";
        
        #endregion Constants
        #region SFields

        private static IiopClientConnectionManager s_singleton = new IiopClientConnectionManager();

        #endregion SFields
        #region IFields
        
        private Timer m_destroyTimer;

        /** contains the available client connections */
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
        
        private IiopClientConnectionManager() {
            TimerCallback timerDelegate = new TimerCallback(DestroyUnusedConnections);
            // Create a timer which invokes the session destroyer every second
            m_destroyTimer = new Timer(timerDelegate, null, 1000, 5000);
        }

        #endregion IConstructors
        
        ~IiopClientConnectionManager() {
            if (m_destroyTimer != null) { 
                m_destroyTimer.Dispose(); 
                m_destroyTimer = null;
            }
        }

        #region SMethods

        /// <summary>
        /// gets the singleton instance of the connection-manager
        /// </summary>
        public static IiopClientConnectionManager GetManager() {
            return s_singleton;
        }

        #endregion SMethods
        #region IMethods
               
        /// <summary>
        /// reserves a connection for the request to send
        /// </summary>
        /// <returns>
        /// The connection context for this connection
        /// </returns>
        internal GiopClientConnectionContext AllocateConnection(IMessage forMessage) {
        	IiopClientConnection result = null;
        	string targetUri = forMessage.Properties[MSG_URI_KEY] as string;
        	lock(this) {
        	    if ((targetUri != null) && (IiopUrlUtil.IsUrl(targetUri))) {
                    string objectUri;
                    string chanUri = IiopUrlUtil.ParseUrl(targetUri, out objectUri);
                        
                    ArrayList avConnections = (ArrayList) m_availableclientConnections[chanUri];
                    if ((avConnections != null) && (avConnections.Count > 0)) { // lock not needed for avConnections, because all using methods exclusive
                        // connection must not be available for other clients if used by this one
                        ConnectionUsageDescription connectionDesc = (ConnectionUsageDescription) avConnections[0];
                        avConnections.Remove(connectionDesc);
                        if (((IiopClientConnection)connectionDesc.Connection).CheckConnected()) {
                            result = connectionDesc.Connection;
                            connectionDesc.Accessed = true;
                        }
                    }
                    // if connection not reusable, create new one    
                    if (result == null) {
                        result = new IiopClientConnection(chanUri);
                    }
                } else {
                    result = new IiopClientConnection(null);
                }
                SetCurrentConnectionContext(result.Context);
        	}
        	m_allocatedConnections[forMessage] = result;
        	return result.Context;
        }
    	
    	
    	internal void ReleaseConnection(IMessage forMessage) {
    		lock(this) {
    			IiopClientConnection connection = 
    			    (IiopClientConnection)m_allocatedConnections[forMessage];
    			
    			if (connection == null) {
    				throw new INTERNAL(11111, 
    				                   CompletionStatus.Completed_MayBe);
    			}
    			// remove from allocated connections
    			m_allocatedConnections.Remove(forMessage);
    			
    			// check if reusable
    			if (connection.ChanUri != null) {
                    ArrayList avConnections = (ArrayList) m_availableclientConnections[connection.ChanUri];
                    if (avConnections == null) {
                        avConnections = new ArrayList();
                        m_availableclientConnections.Add(connection.ChanUri, avConnections);
                    }
                    ConnectionUsageDescription desc = new ConnectionUsageDescription(connection);
                    avConnections.Add(desc);
                } else {
                    connection.CloseConnection(); // not usable further, because connection information not gettable
                }
            	
            	// remove the connection context
            	RemoveCurrentConnectionContext();
    		}
    	}
    	
    	/// <summary>get the reserved connection for the message forMessage</summary>
    	/// <remarks>Prescondition: AllocateConnection is already called for msg</remarks>
    	internal IiopClientConnection GetConnectionFor(IMessage forMessage) {
    		lock(m_allocatedConnections.SyncRoot) {
    			return (IiopClientConnection) m_allocatedConnections[forMessage];
    		}
    	}

        private void DestroyUnusedConnections(Object state) {
            lock(this) {
                IEnumerator enumerator = m_availableclientConnections.Values.GetEnumerator();
                while (enumerator.MoveNext()) { // enumerator over all targets
                    ArrayList list = (ArrayList) enumerator.Current;
                    IEnumerator connectionEnum = list.GetEnumerator();
                    ArrayList connectionsToRemove = new ArrayList();
                    while (connectionEnum.MoveNext()) { // enumerate over all connections to target
                        ConnectionUsageDescription desc = (ConnectionUsageDescription) connectionEnum.Current;
                        if (!desc.Accessed) { // unused --> destroy
                            connectionsToRemove.Add(desc);    // remove session
                            ((IiopClientConnection)desc.Connection).CloseConnection();
                        } else {
                            desc.Accessed = false;
                        }
                    }
                    // remove the sessionDesc from the sessions to the current target
                    IEnumerator destroyEnum = connectionsToRemove.GetEnumerator();
                    while (destroyEnum.MoveNext()) {
                        list.Remove(destroyEnum.Current);
                    }
                }
            }
        }

        

        #endregion IMethods

    }



}
