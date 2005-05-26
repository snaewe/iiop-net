/* InterceptionInfo.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 13.02.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2005 Dominic Ullmann
 *
 * Copyright 2005 ELCA Informatique SA
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
using omg.org.CORBA;
using omg.org.PortableInterceptor;
using omg.org.IOP;
using Ch.Elca.Iiop.MessageHandling;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Idl;

namespace Ch.Elca.Iiop.Interception {
    
    
    /// <summary>
    /// implementation of RequestInfo interface
    /// </summary>
    internal abstract class RequestInfoImpl : RequestInfo {
        
        #region IFields
        
        private AbstractGiopRequest m_giopRequest;
        
        #endregion IFields
        #region IConstructors
        
        internal RequestInfoImpl(AbstractGiopRequest giopRequest) {            
            m_giopRequest = giopRequest;
        }
        
        #endregion IConstructors
        #region IProperties
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.request_id"></see>
        /// </summary>
        public int request_id {
            get {
                return (int)m_giopRequest.RequestId; // use giop as mechanism -> return request id.
            }
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.operation"></see>
        /// </summary>
        public string operation {
            get {
                return m_giopRequest.RequestMethodName;
            }
        }

        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.arguments"></see>
        /// </summary>        
        [IdlSequence(0L)]
        public omg.org.Dynamic.Parameter[] arguments {
            get {
                // not mandatory, for the beginning, don't implement.                
                throw new NO_RESOURCES(1, CompletionStatus.Completed_MayBe);                
            }
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.exception_list"></see>
        /// </summary>        
        [IdlSequence(0L)]
        public omg.org.CORBA.TypeCode[] exceptions {
            get {
                // not mandatory, for the beginning, don't implement.                
                throw new NO_RESOURCES(1, CompletionStatus.Completed_MayBe);                
            }
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.contexts"></see>
        /// </summary>        
        [StringValue()]
        [WideChar(false)]
        [IdlSequence(0L)]
        public string[] contexts {
            get {
                // not mandatory, for the beginning, don't implement.                
                throw new NO_RESOURCES(1, CompletionStatus.Completed_MayBe);                
            }
        }

        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.operation_context"></see>
        /// </summary>        
        [StringValue()]
        [WideChar(false)]
        [IdlSequence(0L)]        
        public string[] operation_context {
            get {
                // not mandatory, for the beginning, don't implement.                
                throw new NO_RESOURCES(1, CompletionStatus.Completed_MayBe);                
            }
        }        

        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.result"></see>
        /// </summary>
        public object result {
            get {
                // not mandatory, for the beginning, don't implement.                
                throw new NO_RESOURCES(1, CompletionStatus.Completed_MayBe);
            }
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.response_expected"></see>
        /// </summary>
        public bool response_expected {
            get {
                // TODO
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.forward_reference"></see>
        /// </summary>
        public MarshalByRefObject forward_reference {
            get {
                // TODO
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.sync_scope"></see>        
        /// </summary>        
        /// <remarks>for this imlementation, alwasy SYNC_WITH_TRANSPORT</remarks>
        public short sync_scope {
            get {
                return omg.org.Messaging.SYNC_WITH_TRANSPORT.ConstVal;
            }
        }        
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.reply_status"></see>        
        /// </summary>        
        public short reply_status {
            get {
                // TODO
                throw new NotImplementedException();
            }
        }
                
        #endregion IProperties       
        #region IMethods
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.get_slot()"></see>
        /// </summary>
        [ThrowsIdlException(typeof(InvalidSlot))]
        public object get_slot(int id) {
            return m_giopRequest.PICurrent.get_slot(id);
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.get_request_service_context(int)"></see>
        /// </summary>
        public ServiceContext get_request_service_context(int id) {
            // service context is a struct -> a copy is automatically returned.
            if (m_giopRequest.RequestServiceContext.ContainsServiceContext(id)) {
                return m_giopRequest.RequestServiceContext.GetServiceContext(id);
            } else {
                throw new BAD_PARAM(23, CompletionStatus.Completed_MayBe);
            }
        }
        
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.RequestInfo.get_reply_service_context(int)"></see>
        /// </summary>
        public ServiceContext get_reply_service_context(int id) {
            // service context is a struct -> a copy is automatically returned.
            if (m_giopRequest.ResponseServiceContext.ContainsServiceContext(id)) {
                return m_giopRequest.ResponseServiceContext.GetServiceContext(id);
            } else {
                throw new BAD_PARAM(23, CompletionStatus.Completed_MayBe);
            }            
        }
        
        #endregion IMethods
        
    }
    
    /// <summary>
    /// implementation of ClientRequestInfo interface used for client side interception
    /// </summary>    
    internal class ClientRequestInfoImpl : RequestInfoImpl, ClientRequestInfo {
        
        #region IFields
        
        private GiopClientRequest m_clientRequest;
        private Exception m_receivedException = null;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>
        /// construct a client request info based on the ClientRequest data.        
        /// </summary>
        /// <remarks>delegates client requests normally to the serverRequest instance.</remarks>
        internal ClientRequestInfoImpl(GiopClientRequest clientRequest) : base(clientRequest) {
            m_clientRequest = clientRequest;
        }
        
        #endregion IConstructors
        #region IProperties
                
        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.target"></see></summary>
        public MarshalByRefObject target {
            get {
                throw new NotImplementedException();
            }
        }

        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.effective_target"></see></summary>        
        public MarshalByRefObject effective_target {
            get {
                throw new NotImplementedException();
            }
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.effective_target"></see></summary>
        public TaggedProfile effective_profile {
            get {
                IIorProfile selectedProfile = m_clientRequest.SelectedProfile;
                return selectedProfile.CreateTaggedProfile();
            }
        }        
        
        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.received_exception"></see></summary>        
        public object received_exception {
            get {
                throw new NotImplementedException();
            }
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.received_exception_id"></see></summary>        
        [StringValue()]
        [WideChar(false)]
        public string received_exception_id {
            get {
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// the connection description.
        /// </summary>
        internal GiopClientConnectionDesc ConnectionDesc {
            get {
                return m_clientRequest.ConnectionDesc;
            }
        }        
        
        #endregion IProperties
        #region IMethods
                
        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.get_effective_component"></see></summary>
        public TaggedComponent get_effective_component(int id) {
            IIorProfile selectedProfile = m_clientRequest.SelectedProfile;
            if (selectedProfile.ContainsTaggedComponent(id)) {
                return selectedProfile.GetTaggedComponent(id);                
            }            
            throw new BAD_PARAM(25, CompletionStatus.Completed_MayBe);
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.get_effective_components"></see></summary>
        [return: IdlSequence(0L)]
        public TaggedComponent[] get_effective_components(int id) {
            IIorProfile selectedProfile = m_clientRequest.SelectedProfile;            
            if (selectedProfile.ContainsTaggedComponent(id)) {
                return selectedProfile.GetTaggedComponents(id);
            }            
            throw new BAD_PARAM(25, CompletionStatus.Completed_MayBe);
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.get_request_policy"></see></summary>
        /// <remarks>always throws INV_POLICY, because policies not yet supported.</remarks>
        public Policy get_request_policy(int type) {
            throw new INV_POLICY(1, CompletionStatus.Completed_MayBe);
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ClientRequestInfo.add_request_service_context"></see></summary>
        public void add_request_service_context(ServiceContext service_context, bool replace) {
            if ((!m_clientRequest.RequestServiceContext.ContainsServiceContext(service_context.context_id)) ||
                replace) {
                m_clientRequest.RequestServiceContext.AddServiceContext(service_context);
            } else {                
                throw new BAD_INV_ORDER(11, CompletionStatus.Completed_MayBe);
            }            
        }        
        
        /// <summary>sets the received exception to the given one.</summary>
        internal void SetReceivedException(Exception ex) {
            m_receivedException = ex;    
        }
        
        #endregion IMethods
        
    }
    
    /// <summary>
    /// implementation of ServerRequestInfo interface used for client side interception
    /// </summary>    
    internal class ServerRequestInfoImpl : RequestInfoImpl, ServerRequestInfo {
    
        #region IFields
        
        private GiopServerRequest m_serverRequest;
        private Exception m_sentException = null;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>
        /// construct a server request info based on the ServerRequest data.        
        /// </summary>
        /// <remarks>delegates client requests normally to the serverRequest instance.</remarks>
        internal ServerRequestInfoImpl(GiopServerRequest serverRequest) : base(serverRequest) {
            m_serverRequest = serverRequest;
        }
        
        #endregion IConstructors
        #region IProperties
        
        /// <summary><see cref="omg.org.PortableInterceptor.ServerRequestInfo.sending_exception"></see></summary>
        public object sending_exception {
            get {
                throw new NotImplementedException();
            }
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ServerRequestInfo.object_id"></see></summary>
        public byte[] object_id {
            get {
                return m_serverRequest.ObjectKey;
            }
        }        
        
        /// <summary><see cref="omg.org.PortableInterceptor.ServerRequestInfo.adapter_id"></see></summary>
        [IdlSequence(0L)]
        public byte[] adapter_id {
            get {
                throw new System.NotImplementedException();
            }
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ServerRequestInfo.target_most_derived_interface"></see></summary>
        [StringValue()]
        [WideChar(false)]
        public string target_most_derived_interface {
            get {
                throw new NotImplementedException();
            }
        }                                

        /// <summary>
        /// the connection description.
        /// </summary>
        internal GiopConnectionDesc ConnectionDesc {
            get {
                return m_serverRequest.ConnectionDesc;
            }
        }                
        
        #endregion IProperties
        #region IMethods
        
        
        /// <summary><see cref="omg.org.PortableInterceptor.ServerRequestInfo.get_server_policy"></see></summary>
        /// <remarks>always throws INV_POLICY, because policies not yet supported.</remarks>
        public Policy get_server_policy(int type) {
            throw new INV_POLICY(2, CompletionStatus.Completed_MayBe);
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ServerRequestInfo.set_slot"></see></summary>
        [ThrowsIdlException(typeof(InvalidSlot))]
        public void set_slot(int id, object data) {
            m_serverRequest.PICurrent.set_slot(id, data);
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ServerRequestInfo.target_is_a"></see></summary>
        public bool target_is_a([StringValue()][WideChar(false)] string id) {
            throw new System.NotImplementedException();
        }
        
        /// <summary><see cref="omg.org.PortableInterceptor.ServerRequestInfo.add_reply_service_context"></see></summary>
        public void add_reply_service_context(ServiceContext service_context, bool replace) {
            if ((!m_serverRequest.ResponseServiceContext.ContainsServiceContext(service_context.context_id)) ||
                replace) {
                m_serverRequest.ResponseServiceContext.AddServiceContext(service_context);
            } else {                
                throw new BAD_INV_ORDER(11, CompletionStatus.Completed_MayBe);
            }
        }        
        
        /// <summary>sets the sent exception to the given one.</summary>
        internal void SetSentException(Exception ex) {
            m_sentException = ex;
        }
        
        #endregion IMethods
        
    }
    
    
    /// <summary>
    /// implementation of IORInfo interface used for ior interception.
    /// </summary>
    internal class IORInfoImpl : IORInfo {
    
        #region IFields
        
        private InternetIiopProfile[] m_profiles;
        
        #endregion IFields
        #region IConstructors
        
        public IORInfoImpl(InternetIiopProfile[] profiles) {
            m_profiles = profiles;
        }
        
        public IORInfoImpl(InternetIiopProfile profile) : this(new InternetIiopProfile[] { profile }) {            
        }
        
        #endregion IConstructors
        #region IMethods
                
        /// <summary>
        /// gets the server side policy for the given type. Throws INV_POLICY with a minor code 2,
        /// if policy not knwon.
        /// </summary>
        /// <remarks>policies not yet supported, throws always INV_POLICY exception.</remarks>
        public Policy get_effective_policy(int type) {
            throw new INV_POLICY(2, CompletionStatus.Completed_MayBe);
        }
        
        /// <summary>
        /// adds the specified tagged component to all profiles.
        /// </summary>        
        public void add_ior_component(TaggedComponent component) {
            for (int i = 0; i < m_profiles.Length; i++) {
                m_profiles[i].AddTaggedComponent(component);
            }
        }
        
        /// <summary>
        /// adds the specified tagged component to the profile with the given id.
        /// </summary>        
        public void add_ior_component_to_profile(TaggedComponent component, int profileId) {
            for (int i = 0; i < m_profiles.Length; i++) {
                if (m_profiles[i].ProfileId == profileId) {
                    m_profiles[i].AddTaggedComponent(component);
                }
            }
        }
        
        #endregion IMethods
        
    }
    
}
