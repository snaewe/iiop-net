/* ConnectionManager.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 28.04.03  Dominic Ullmann (DUL), dul@elca.ch
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

using Ch.Elca.Iiop.Services;

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
            CallContext.FreeNamedDataSlot(CONNECTION_CONTEXT_KEY);
        }

        /// <summary>
        /// sets the active connection context
        /// </summary>
        internal static void SetCurrentConnectionContext(GiopConnectionContext context) {
            CallContext.SetData(CONNECTION_CONTEXT_KEY, context);
        }

        /// <summary>
        /// gets the active connection context
        /// </summary>
        /// <remarks>
        /// is only != null after SetCurrentConnectionContext is called
        /// </remarks>
        /// <returns></returns>
        public static GiopConnectionContext GetCurrentConnectionContext() {
            return (GiopConnectionContext)CallContext.GetData(CONNECTION_CONTEXT_KEY);
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
    
        internal IiopServerConnection RegisterActiveConnection(IiopServerTransportSink serverSink, NetworkStream stream) {
            IiopServerConnection con = new IiopServerConnection(serverSink, stream);
            GiopConnectionContext context = new GiopConnectionContext(con);
            SetCurrentConnectionContext(context);
            return con;
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

        private class ConnectionUsageDescription {
            
            #region IFields

            private GiopConnectionContext m_connectionDesc;
            private bool m_accessedSinceCheck;

            #endregion IFields
            #region IConstructors

            public ConnectionUsageDescription(GiopConnectionContext connectionDesc) {
                m_accessedSinceCheck = true;
                m_connectionDesc = connectionDesc;
            }

            #endregion IConstructors
            #region IProperties

            public GiopConnectionContext ConnectionDesc {
                get { 
                    return m_connectionDesc; 
                }
                set { 
                    m_connectionDesc = value; 
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

        #region SFields

        private static IiopClientConnectionManager s_singleton = new IiopClientConnectionManager();

        #endregion SFields
        #region IFields
        
        private Timer m_destroyTimer;

        /** contains the available client connections */
        private Hashtable m_availableclientConnections = new Hashtable();

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
        
        public IiopClientConnection CreateOrGetClientConnection(IiopClientFormatterSink clientSink, string targetUri) {
            lock(this) {
                GiopConnectionContext result = null;

                if ((targetUri != null) && (IiopUrlUtil.IsUrl(targetUri)))    {
                    string objectUri;
                    string chanUri = IiopUrlUtil.ParseUrl(targetUri, out objectUri);
                        
                    ArrayList avConnections = (ArrayList) m_availableclientConnections[chanUri];
                    if ((avConnections != null) && (avConnections.Count > 0)) { // lock not needed for avConnections, because all using methods exclusive
                        // connection must not be available for other clients if used by this one
                        ConnectionUsageDescription connectionDesc = (ConnectionUsageDescription) avConnections[0];
                        avConnections.Remove(connectionDesc);
                        if (((IiopClientConnection)connectionDesc.ConnectionDesc.Connection).CheckConnected()) {
                            result = connectionDesc.ConnectionDesc;
                            connectionDesc.Accessed = true;
                        }
                    }
                        
                    if (result == null) {
                        result = CreateClientConnection(clientSink, chanUri);
                    }
                } else {
                    result = CreateClientConnection(clientSink, null);
                }
                SetCurrentConnectionContext(result);
                return (IiopClientConnection)result.Connection;
            }
        }

        private GiopConnectionContext CreateClientConnection(IiopClientFormatterSink clientSink, string chanUri) {
            IiopClientConnection con = new IiopClientConnection(clientSink, chanUri);
            GiopConnectionContext context = new GiopConnectionContext(con);
            con.Context = context; // set context of this connection
            return context;
        }
        
        /// <summary>tells the connection manager, that the active connection is not used any more</summary>
        public void ReleaseClientConnection() {
            GiopConnectionContext context = GetCurrentConnectionContext();
            IiopClientConnection connection = (IiopClientConnection) context.Connection;
            lock(this) {
                if (connection.ChanUri != null) {
                    ArrayList avConnections = (ArrayList) m_availableclientConnections[connection.ChanUri];
                    if (avConnections == null) {
                        avConnections = new ArrayList();
                        m_availableclientConnections.Add(connection.ChanUri, avConnections);
                    }
                    ConnectionUsageDescription desc = new ConnectionUsageDescription(context);
                    avConnections.Add(desc);
                } else {
                    connection.CloseConnection(); // not usable further, because connection information not gettable
                }
            }
            // remove the connection context
            RemoveCurrentConnectionContext();
        }

        internal void DestroyUnusedConnections(Object state) {
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
                            ((IiopClientConnection)desc.ConnectionDesc.Connection).CloseConnection();
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
