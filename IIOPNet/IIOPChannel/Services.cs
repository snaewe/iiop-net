/* Services.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 17.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using Ch.Elca.Iiop.Cdr;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using Ch.Elca.Iiop.CorbaObjRef;

namespace Ch.Elca.Iiop.Services {

    /// <summary>
    /// This class represents the collection of service contexts in request / response messages
    /// </summary>
    public class ServiceContextCollection {
        
        #region IFields

        private Hashtable m_contexts = new Hashtable();

        #endregion IFields
        #region IConstructors

        public ServiceContextCollection() {
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>the nr of service contexts</summary>
        public ulong Count {
            get {
                return (ulong)m_contexts.Count;
            }
        }

        #endregion IProperties
        #region IMethods

        public void AddServiceContext(ServiceContext context) {
            m_contexts.Add(context.ServiceID, context);
        }

        public ServiceContext GetContext(uint serviceId) {
            return (ServiceContext)m_contexts[serviceId];
        }
        
        public bool ContainsContextForService(uint serviceId) {
            return (GetContext(serviceId) != null);
        }

        /// <summary>enumarate over the service contexts</summary>
        public IEnumerator GetEnumerator() {
            return m_contexts.Values.GetEnumerator();
        }

        public override string ToString() {
            string result = "\n" + Count.ToString() + " ServiceContexts:\n";
            IEnumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext()) {
                result += (enumerator.Current + "\n");
            }
            return result;
        }

        #endregion IMethods

    }


    /// <summary>
    /// this class represents a service context
    /// </summary>
    public class ServiceContext {

        #region IFields
        
        private uint m_serviceId;
        private byte[] m_contextData;

        #endregion IFields
        #region IConstructors

        protected ServiceContext(uint serviceId) : this(serviceId, new byte[0]) {
        }

        public ServiceContext(uint serviceId, byte[] contextData) {
            m_serviceId = serviceId;
            m_contextData = contextData;
            if (m_contextData == null) { 
                m_contextData = new byte[0]; 
            }
        }

        public ServiceContext(CdrEncapsulationInputStream encap, uint serviceId) {
            m_serviceId = serviceId;
            Deserialize(encap);
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>the service id for this context</summary>
        public uint ServiceID {
            get { 
                return m_serviceId; 
            }
        }

        public byte[] ContextData {
            get { 
                return m_contextData; 
            }
        }

        #endregion IProperties
        #region IMethods

        protected void SetContextData(byte[] data) {
            m_contextData = data;
        }

        public override string ToString() {
            return "service-id: " + m_serviceId + ", data length: " + m_contextData.Length;
        }

        public virtual void Serialize(CdrOutputStream stream) {
            stream.WriteULong(m_serviceId);
            CdrEncapsulationOutputStream encapStream = new CdrEncapsulationOutputStream(0);
            stream.WriteOpaque(m_contextData);
            stream.WriteEncapsulation(encapStream);
        }

        public virtual void Deserialize(CdrEncapsulationInputStream encap) {
            m_contextData = encap.ReadRestOpaque();
        }

        #endregion IMethods

    }


    /// <summary>
    /// this class is a base class for a CORBA service
    /// </summary>
    public abstract class CorbaService {
        
        #region IMethods

        /// <summary>
        /// get the servie id for this service
        /// </summary>
        public abstract uint GetServiceId();

        /// <summary>
        /// Deserialises the ServiceContext from the encapsulated service context data
        /// </summary>
        /// <returns>The deserialised context</returns>
        public abstract ServiceContext DeserialiseContext(CdrEncapsulationInputStream encap);

        /// <summary>
        /// This method is called when a request is received
        /// </summary>
        public abstract void HandleContextForReceivedRequest(ServiceContext context);
        
        /// <summary>
        /// This method is called when a reply is received
        /// </summary>
        public abstract void HandleContextForReceivedReply(ServiceContext context);
        
        /// <summary>
        /// This method is called when a request is sent
        /// </summary>
        /// <returns>The ServiceContext to include or null</returns>
        public abstract ServiceContext InsertContextForRequestToSend(IMethodCallMessage msg, Ior targetIor,
                                                                     GiopConnectionContext conContext);

        /// <summary>
        /// This method is called, when a reply is sent
        /// </summary>
        /// <returns>The ServiceContext to include or null</returns>
        public abstract ServiceContext InsertContextForReplyToSend();


        #endregion IMethods

    }

    
    /// <summary>
    /// this class represents an unknown service
    /// </summary>    
    public class UnknownService : CorbaService {
        
        #region IFields

        private uint m_serviceId;
        private ServiceContext m_context;

        #endregion IFields
        #region IConsturctors

        public UnknownService(uint serviceId) {
            m_serviceId = serviceId;
            m_context = new ServiceContext(serviceId, new byte[0]);
        }

        #endregion IConstructors
        #region IMethods
        
        
        public override ServiceContext DeserialiseContext(CdrEncapsulationInputStream encap) {
            ServiceContext cntx = new ServiceContext(encap, m_serviceId);
            return cntx;
        }

        public override void HandleContextForReceivedRequest(ServiceContext context) {
            // nothing to do
        }
        
        public override void HandleContextForReceivedReply(ServiceContext context) {
            // nothing to do
        }
        
        public override ServiceContext InsertContextForRequestToSend(IMethodCallMessage msg, Ior targetIor,
                                                                     GiopConnectionContext conContext) {
            // nothing to do
            return null;
        }

        public override ServiceContext InsertContextForReplyToSend() {
            // nothing to do
            return null;
        }
        
        public override uint GetServiceId() {
            return m_serviceId;
        }

        #endregion IMethods

    }    

        
    /// <summary>
    /// This class provides supporting functionality for Corba Object services
    /// </summary>
    public class CosServices {
        
        #region IFields

        /// <summary>
        /// the registered services
        /// </summary>
        private Hashtable m_services = new Hashtable();

        #endregion IFields
        #region SFields

        private static CosServices s_singleton = new CosServices();

        #endregion SFields
        #region IConstructors

        private CosServices() {
            RegisterDefaultServices();
        }

        #endregion IConstructors
        #region SMethods

        public static CosServices GetSingleton() {
            return s_singleton;
        }

        #endregion SMethods
        #region IMethods

        /// <summary>
        /// register the default Cos Services
        /// </summary>
        private void RegisterDefaultServices() {
            RegisterService(new CodeSetService());
        }

        /// <summary>
        /// register a CosService.
        /// </summary>
        /// <remarks>
        /// This method is useful for Services, which wants to access / send a ServiceContext.
        /// </remarks>
        public void RegisterService(CorbaService service) {
            lock (m_services.SyncRoot) {
                m_services.Add(service.GetServiceId(), service);
            }
        }

        /// <summary>
        /// get the serive for the service-id
        /// </summary>
        public CorbaService GetForServiceId(uint serviceId) {
            CorbaService result = null;
            lock (m_services.SyncRoot) {
                result = (CorbaService)m_services[serviceId];
            }
            if (result != null) {
                return result;
            } else {
                return new UnknownService(serviceId);
            }
        }

        /// <summary>
        /// Inform the registered interceptors: a request was received
        /// </summary>
        internal void InformInterceptorsReceivedRequest(ServiceContextCollection contexts) {
            lock (m_services.SyncRoot) {
                IEnumerator enumerator = m_services.Values.GetEnumerator();
                while (enumerator.MoveNext()) {
                    CorbaService service = (CorbaService) enumerator.Current;
                    ServiceContext cntx = contexts.GetContext(service.GetServiceId());
                    service.HandleContextForReceivedRequest(cntx);
                }
            }
        }

        /// <summary>
        /// Inform the registered interceptors: a reply was received
        /// </summary>
        internal void InformInterceptorsReceivedReply(ServiceContextCollection contexts) {
            lock (m_services.SyncRoot) {
                IEnumerator enumerator = m_services.Values.GetEnumerator();
                while (enumerator.MoveNext()) {
                    CorbaService service = (CorbaService) enumerator.Current;
                    ServiceContext cntx = contexts.GetContext(service.GetServiceId());
                    service.HandleContextForReceivedReply(cntx);
                }
            }
        }

        /// <summary>
        /// Inform the registered interceptors: a request will be sent
        /// </summary>
        /// <returns>The collected contexts</returns>
        internal ServiceContextCollection InformInterceptorsRequestToSend(IMethodCallMessage msg, Ior targetIor,
                                                                          GiopConnectionContext conContext) {
            ServiceContextCollection cntxColl = new ServiceContextCollection();
            lock (m_services.SyncRoot) {
                IEnumerator enumerator = m_services.Values.GetEnumerator();
                while (enumerator.MoveNext()) {
                    CorbaService service = (CorbaService) enumerator.Current;
                    ServiceContext cntx = service.InsertContextForRequestToSend(msg, targetIor, conContext);
                    if (cntx != null) {
                        cntxColl.AddServiceContext(cntx);
                    }
                }
            }
            return cntxColl;
        }

        /// <summary>
        /// Inform the registered interceptors: a reply will be sent
        /// </summary>
        /// <returns>The collected contexts</returns>
        internal ServiceContextCollection InformInterceptorsReplyToSend() {
            ServiceContextCollection cntxColl = new ServiceContextCollection();
            lock (m_services.SyncRoot) {
                IEnumerator enumerator = m_services.Values.GetEnumerator();
                while (enumerator.MoveNext()) {
                    CorbaService service = (CorbaService) enumerator.Current;
                    ServiceContext cntx = service.InsertContextForReplyToSend();
                    if (cntx != null) {
                        cntxColl.AddServiceContext(cntx);
                    }
                }
            }
            return cntxColl;
        }

        #endregion IMethods

    }

   
}
