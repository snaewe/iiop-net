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
using System.Net;
using System.Collections;
using Ch.Elca.Iiop.CorbaObjRef;
using omg.org.IOP;


namespace Ch.Elca.Iiop {


    /// <summary>specifies the interface, which must be implemented 
    /// by every tranport-wrapper</summary>
    public interface ITransport {
 
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
 
        /// <summary>is the connection to the peer open</summary>
        bool IsConnectionOpen();
 
        IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state);

        IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state);
 
        int EndRead(IAsyncResult asyncResult);
 
        void EndWrite(IAsyncResult asyncResult);

        int Read(byte[] buffer, int offset, int size);

        void Write(byte[] buffer, int offset, int size);

        /// <summary>returns the ip address of the client</summary>
        IPAddress GetPeerAddress();
 
        #endregion IMethods
 
    }
 
 
    /// <summary>the interface to implement by a client side transport wrapper</summary>
    public interface IClientTransport : ITransport {
 
        /// <summary>opens a connection to the target, if not already open (when open do nothing)</summary>
        void OpenConnection();
 
        /// <summary>
        /// the receive timeout of the client connection (in ms); 0 means infinite timeout.
        /// </summary>
        int ReceiveTimeOut {
            get;
            set;
        }
 
        /// <summary>
        /// the send timeout of the client connection (in ms); 0 means infinite timeout.
        /// </summary>
        int SendTimeOut {
            get;
            set;
        }
 
    }
 
    // /// <summary>waits for a client to connect to the server</summary>
    // void WaitForClientConnection();
 
 
 
    /// <summary>the interfce to implement by a server side transport wrapper</summary>
    public interface IServerTransport : ITransport {
 
        /// <summary>Results the given exception from a connection close on client side</summary>
        bool IsConnectionCloseException(Exception e);
 
    }

    /// <summary>
    /// Base interface for client and server transport factories.
    /// </summary>
    public interface ICommonTransportFactory {

        /// <summary>
        /// The codec to use for creating tagged components.
        /// This property is injected by the IIOPChannel.
        /// </summary>
        omg.org.IOP.Codec Codec {
            set;
        }
 
    }
 
 
    /// <summary>
    /// creates client transports
    /// </summary>
    public interface IClientTransportFactory : ICommonTransportFactory {
 
        /// <summary>creates a client transport to the target</summary>
        IClientTransport CreateTransport(IIorProfile target);
 
        /// <summary>creates a key identifying an endpoint; if two keys for two IORs are equal,
        /// then the transport can be used to connect to both.</summary>
        string GetEndpointKey(IIorProfile target);
 
        /// <summary>
        /// creates a key identifying an endpoint received over a bidir connection; if two keys for two IORs are equal,
        /// then the transport can be used to connect to both.
        /// </summary>
        string GetEndPointKeyForBidirEndpoint(object endPoint);
 
        /// <summary>returns true, if this transport factory can create a transport for the given IOR, otherwise false</summary>
        bool CanCreateTranporForIor(Ior target);
 
        /// <summary>returns true, if this transport factory can connect with this profile, otherwise false.</summary>
        bool CanUseProfile(IIorProfile profile);
 
        /// <summary>
        /// extract options, which are specific to the transport factory
        /// </summary>
        void SetupClientOptions(IDictionary options);
 
        /// <summary>
        /// This timeout-options should be handled by all transport factories.
        /// </summary>
        /// <param name="receiveTimeOut">the receive-timeout for the connection in milliseconds; 0 means inifinite timeout</param>
        /// <param name="sendTimeOut">the send-timeout for the connection in milliseconds; 0 means inifinite timeout</param>
        void SetClientTimeOut(int receiveTimeOut, int sendTimeOut);
 
    }
 
    /// <summary>creates server transports</summary>
    public interface IServerTransportFactory : ICommonTransportFactory {
 
        /// <summary>creates a connecton listener, which notifies about new clients
        /// using clientAcceptCallback</summary>
        IServerConnectionListener CreateConnectionListener(ClientAccepted clientAcceptCallBack);
 
        /// <summary>
        /// extract options, which are specific to the transport factory
        /// </summary>
        void SetupServerOptions(IDictionary options);
 
        /// <summary>
        /// creates an array of listen points for the given channel data. This listen points are
        /// sent to a foreign instance as endpoints for bidirectional connections.
        /// </summary>
        object[] GetListenPoints(IiopChannelData chanData);
    }
 
    /// <summary>creates client and server transports</summary>
    public interface ITransportFactory : IClientTransportFactory, IServerTransportFactory {
 
    }
 
    /// <summary>delegate to a method, which should be called, when a client connection is accepted</summary>
    public delegate void ClientAccepted(IServerTransport acceptedTransport);
 
    /// <summary>implementers wait for and accept client connections on their supported transport mechanism.
    /// </summary>
    public interface IServerConnectionListener {
 
        #region IMethods
 
        /// <summary>initalizes the listener, must only be called once</summary>
        void Setup(ClientAccepted clientAcceptCallback);
 
        /// <summary>has setup already been called</summary>
        bool IsInitalized();
 
        /// <summary>starts the listing for clients; calls ClientAccepted callback, when a client is accepted.</summary>
        /// <returns>the port the listener is listening on; may be different from listeningPortSuggestion</returns>
        /// <param name="bindTo">specifies, to which address the listener should be bound to; if unimportant, use IPAddress.Any</param>
        /// <param name="additionalTaggedComponents">gives back the additional components to add to an IOR;
        /// Those additional components hold the additional information needed by clients to connect to this listener.
        /// Can contain an array with 0 elements, if default information in the IOR is enough.
        /// </param>
        int StartListening(IPAddress bindTo, int listeningPortSuggestion, out TaggedComponent[] additionalTaggedComponents);
 
        /// <summary>is this listener active</summary>
        bool IsListening();
 
        /// <summary>stops accepting client connections</summary>
        void StopListening();
 
        #endregion IMethods
 
    }

}
