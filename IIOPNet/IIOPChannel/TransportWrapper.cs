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
        
        /// <summary>opens a connection to the target</summary>
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
        
        /// <summary>creates a client transport to the targetHost/targetPort</summary>
        IClientTransport CreateTransport(string targetHost, int targetPort);
                
    }
    
    
    
    
    



}
