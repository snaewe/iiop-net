/* TransportWrapper.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 18.01.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.CorbaObjRef;


namespace Ch.Elca.Iiop {


    /// <summary>specifies the interface, which must be implemented 
    /// by every tranport-wrapper</summary>
    internal interface ITransport {
        
        #region IProperties
        
        /// <summary>The stream to access this transport</summary>
        Stream TransportStream {
            get;
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>is data available for reading</summary>
        bool IsDataAvailable();
        
        /// <summary>closes the connection</summary>
        void CloseConnection();
        
        #endregion IMethods                
        
    }
    
    
    /// <summary>the interface to implement by a client side transport wrapper</summary>
    internal interface IClientTransport : ITransport {
        
        /// <summary>opens a connection to the target, if not already open (when open do nothing)</summary>
        void OpenConnection();
                
        /// <summary>is the connection to the target open</summary>
        bool IsConnectionOpen();
                
    }
    
    // /// <summary>waits for a client to connect to the server</summary>
    // void WaitForClientConnection();
    
    
    
    /// <summary>the interfce to implement by a server side transport wrapper</summary>
    internal interface IServerTransport : ITransport {
                
        /// <summary>Results the given exception from a connection close on client side</summary>
        bool IsConnectionCloseException(Exception e);
        
    }
            
    
    /// <summary>
    /// creates client transports
    /// </summary>
    internal interface IClientTransportFactory {
        
        /// <summary>creates a client transport to the target</summary>
        IClientTransport CreateTransport(Ior target);
        
        /// <summary>creates a key identifying an endpoint; if two keys for two IORs are equal, 
        /// then the transport can be used to connect to both.</summary>
        string GetEndpointKey(Ior target);
                
    }
    
    
    
    /// <summary>delegate to a method, which should be called, when a client connection is accepted</summary>
    delegate void ClientAccepted(IServerTransport acceptedTransport);
    
    /// <summary>implementers wait for and accept client connections on their supported transport mechanism.
    /// </summary>
    interface IServerConnectionListener {
        
        #region IMethods
        
        /// <summary>initalizes the listener, must only be called once</summary>
        void Setup(ClientAccepted clientAcceptCallback);
        
        /// <summary>has setup already been called</summary>
        bool IsInitalized();
        
        /// <summary>starts the listing for clients; calls ClientAccepted callback, when a client is accepted.</summary>
        /// <returns>the port the listener is listening on; may be different from listeningPortSuggestion</returns>
        int StartListening(int listeningPortSuggestion);
        
        /// <summary>is this listener active</summary>
        bool IsListening();
        
        /// <summary>stops accepting client connections</summary>
        void StopListening();
        
        #endregion IMethods
        
    }


}
