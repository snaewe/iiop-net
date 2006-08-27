/* GiopRequest.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 05.05.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2005 Dominic Ullmann
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
using Ch.Elca.Iiop.Idl;
using omg.org.PortableInterceptor;
using omg.org.IOP;
 
namespace omg.org.IIOP {

    [RepositoryID("IDL:omg.org/IIOP/ListenPoint:1.0")]
    [IdlStruct]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public struct ListenPoint {
    
        #region IFields
        
        [ExplicitSerializationOrderNr(0)]
        [StringValue()]
        [WideChar(false)]
        public string host;
        
        [ExplicitSerializationOrderNr(1)]
        public short port;
        
        #endregion IFields
        #region IConstructors
        
        public ListenPoint(string host, short port) {
            this.port = port;
            this.host = host;
        }
        
        #endregion IConstructors
        #region IProperties
        
        public int ListenPort {
            get {
                return ((ushort)port); // mapped from ushort -> convert first to ushort.
            }
        }
        
        public string ListenHost {
            get {
                return host;
            }
        }
        
        #endregion IProperties
        
    }
    
    
    [RepositoryID("IDL:omg.org/IIOP/BiDirIIOPServiceContext:1.0")]
    [IdlStruct]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public struct BiDirIIOPServiceContext {
    
        public BiDirIIOPServiceContext(ListenPoint[] listenPoints) {
            listen_points = listenPoints;
        }
        
        [ExplicitSerializationOrderNr(0)]
        [IdlSequence(0L)]
        public ListenPoint[] listen_points;
    
    }
    
    /// <summary>
    /// the service id for bidirection iiop service context.
    /// </summary>
    public sealed class BI_DIR_IIOP {
                                             
        #region Constants
        
        public const int ConstVal = 5;

        #endregion Constants
        #region IConstructors
        
        private BI_DIR_IIOP() {
        }
        
        #endregion IConstructors
        
    }
            
     
}

namespace Ch.Elca.Iiop.Interception {
    
    
    using omg.org.IIOP;
    using omg.org.CORBA;
    
    /// <summary>
    /// client channel interceptor to handle bidirectional connections. 
    /// </summary>
    /// <remarks>Installed by BiDirInterceptionOption.</remarks>    
    public class BiDirIiopClientInterceptor : ClientRequestInterceptor {
        
        #region IFields
        
        private Codec m_codec;                
        
        #endregion IFields
        #region IConstructors
        
        public BiDirIiopClientInterceptor(OrbServices orb) {
            m_codec = orb.CodecFactory.create_codec(
                          new Encoding(ENCODING_CDR_ENCAPS.ConstVal, 1, 2));            
        }
        
        #endregion IConstructors
        #region IProperties
                
        public string Name {
            get {
                return "BiDirClientInterceptor";
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        private ListenPoint[] ConvertToListenPoints(object[] endPoints) {            
            int nrOfIiopPoints = 0;
            for (int i = 0; i < endPoints.Length; i++) {
                if (endPoints[i] is ListenPoint) {
                    nrOfIiopPoints++;
                }
            }
            ListenPoint[] result = new ListenPoint[nrOfIiopPoints];
            int j = 0;
            for (int i = 0; i < endPoints.Length; i++) {
                if (endPoints[i] is ListenPoint) {
                    result[j] = (ListenPoint)endPoints[i];
                    j++;
                }
            }
            return result;
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ClientRequestInterceptor.send_request"></see>
        /// </summary>        
        public void send_request(ClientRequestInfo ri) {
            ClientRequestInfoImpl internalInfo = (ClientRequestInfoImpl)ri; // need access to connection information
            
            if ((internalInfo.ConnectionDesc.Connection.IsInitiatedLocal()) && // initiated in this appdomain
                (internalInfo.ConnectionDesc.ConnectionManager.SupportBiDir())) {
                                                    
                GiopBidirectionalConnectionManager biDirConManager =
                    (GiopBidirectionalConnectionManager)internalInfo.ConnectionDesc.ConnectionManager;
                
                ListenPoint[] listenPointsEntry = ConvertToListenPoints(biDirConManager.GetOwnListenPoints());
                BiDirIIOPServiceContext contextEntry = new BiDirIIOPServiceContext(listenPointsEntry);
                
                ServiceContext svcContext = new ServiceContext(BI_DIR_IIOP.ConstVal, 
                                                               m_codec.encode_value(contextEntry));                
                ri.add_request_service_context(svcContext, true);
                
                biDirConManager.SetupConnectionForBidirReception(internalInfo.ConnectionDesc);            
            }
            
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ClientRequestInterceptor.send_poll"></see>
        /// </summary>        
        public void send_poll(ClientRequestInfo ri) {
            // nothing to do
        }
               
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ClientRequestInterceptor.receive_reply"></see>
        /// </summary>        
        public void receive_reply(ClientRequestInfo ri) {
            // nothing to do
        }

        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ClientRequestInterceptor.receive_exception"></see>
        /// </summary>        
        public void receive_exception(ClientRequestInfo ri) {
            // nothing to do
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ClientRequestInterceptor.receive_other"></see>
        /// </summary>        
        public void receive_other(ClientRequestInfo ri) {
            // nothing to do
        }
        
        #endregion IMethods
        
    }
    
    /// <summary>
    /// server channel interceptor to handle bidirectional connections. 
    /// </summary>
    /// <remarks>Installed by BiDirInterceptionOption.</remarks>
    public class BiDirIiopServerInterceptor : ServerRequestInterceptor {

        #region IFields
        
        private Codec m_codec;
        private omg.org.CORBA.TypeCode m_svcContextTypeCode;
        
        #endregion IFields
        #region IConstructors
        
        public BiDirIiopServerInterceptor(OrbServices orb) {
            m_codec = orb.CodecFactory.create_codec(
                          new Encoding(ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_svcContextTypeCode = orb.create_tc_for_type(typeof(BiDirIIOPServiceContext));
        }
        
        #endregion IConstructors
        #region IProperties
        
        public string Name {
            get {
                return "BiDirServerInterceptor";
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ServerRequestInterceptor.receive_request_service_contexts"></see>
        /// </summary>        
        public void receive_request_service_contexts(ServerRequestInfo ri) {
            object svcCtx = null;            
            try {
                svcCtx = ri.get_request_service_context(BI_DIR_IIOP.ConstVal);
            } catch (omg.org.CORBA.BAD_PARAM) {
                // context not found
            }
            
            if (svcCtx != null) {
            
                BiDirIIOPServiceContext receivedCtx = 
                    (BiDirIIOPServiceContext)m_codec.decode_value(((ServiceContext)svcCtx).context_data, 
                                                                  m_svcContextTypeCode);
                
                ServerRequestInfoImpl internalInfo = (ServerRequestInfoImpl)ri; // need access to connection information
                GiopBidirectionalConnectionManager biDirConManager =
                    (GiopBidirectionalConnectionManager)internalInfo.ConnectionDesc.ConnectionManager;
                biDirConManager.RegisterBidirectionalConnection(internalInfo.ConnectionDesc,
                                                                receivedCtx.listen_points);
            }
            
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ServerRequestInterceptor.receive_request"></see>
        /// </summary>        
        public void receive_request(ServerRequestInfo ri) {
            // nothing to do
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ServerRequestInterceptor.send_reply"></see>
        /// </summary>        
        public void send_reply(ServerRequestInfo ri) {
            // nothing to do
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ServerRequestInterceptor.send_exception"></see>
        /// </summary>        
        public void send_exception(ServerRequestInfo ri) {
            // nothing to do
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.ServerRequestInterceptor.send_other"></see>
        /// </summary>        
        public void send_other(ServerRequestInfo ri) {
            // nothing to do
        }
        
        #endregion IMethods
        
    }
    
    /// <summary>
    /// used to install bidir client and server interceptor in a channel scoped manner.
    /// </summary>
    public class BiDirIiopInterceptionOption : IInterceptionOption {
        
        #region IFields
        
        private BiDirIiopClientInterceptor m_clientInterceptor;
        private BiDirIiopServerInterceptor m_serverInterceptor;
        
        #endregion IFields
        #region IConstructors
        
        public ClientRequestInterceptor GetClientRequestInterceptor(OrbServices orb) {
            lock(this) {
                if (m_clientInterceptor == null) {
                    m_clientInterceptor = new BiDirIiopClientInterceptor(orb);
                }
                return m_clientInterceptor;
            }            
        }

        public ServerRequestInterceptor GetServerRequestInterceptor(OrbServices orb) {
            lock(this) {
                if (m_serverInterceptor == null) {
                    m_serverInterceptor = new BiDirIiopServerInterceptor(orb);
                }
                return m_serverInterceptor;
            }            
        }
        
        #endregion IConstructors
        
    }
    
}
