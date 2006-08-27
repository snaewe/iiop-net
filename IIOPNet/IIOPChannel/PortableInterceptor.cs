/* PortableInterceptor.cs
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
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;
using omg.org.IOP;


namespace omg.org.Dynamic {
            
    [IdlStruct]
    [RepositoryID("IDL:omg.org/Dynamic/Parameter:1.0")]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public struct Parameter {
                        
        [ExplicitSerializationOrderNr(0)]
        [ObjectIdlTypeAttribute(IdlTypeObject.Any)]
        public object argument;
        // TODO:
        // ParameterMode mode;
    }
    
}

namespace omg.org.Messaging {

    /// <summary>
    /// control returns to the client, before message has been delivered to the client side transport.
    /// </summary>
    public sealed class SYNC_NONE {
        
        #region Constants
        
        public const short ConstVal = 0;
        
        #endregion Constants
        #region IConstructors
        
        private SYNC_NONE() {
        }
        
        #endregion IConstructors        
        
    }
    
    /// <summary>
    /// control returns to the client, after message has been delivered to the client side transport.
    /// </summary>
    public sealed class SYNC_WITH_TRANSPORT {
        
        #region Constants
        
        public const short ConstVal = 1;
        
        #endregion Constants
        #region IConstructors
        
        private SYNC_WITH_TRANSPORT() {        
        }
        
        #endregion IConstructors
        
    }    
    
    public sealed class SYNC_WITH_SERVER {
        
        #region Constants
        
        public const short ConstVal = 2;

        #endregion Constants
        #region IConstructors
        
        private SYNC_WITH_SERVER() {
        }
        
        #endregion IConstructors
        
    }    

    public sealed class SYNC_WITH_TARGET {
        
        #region Constants
        
        public const short ConstVal = 3;
        
        #endregion Constants
        #region IConstructors

        private SYNC_WITH_TARGET() {
        }        
        
        #endregion IConstructors
        
    }    

}

    
namespace omg.org.PortableInterceptor {

    /// <summary>the reply status in request-info</summary>
    public sealed class SUCCESSFUL {
        
        #region Constants
        
        public const short ConstVal = 0;
        
        #endregion Constants
        #region IConstructors

        private SUCCESSFUL() {
        }        
        
        #endregion IConstructors
        
    }    
    
    /// <summary>the reply status in request-info</summary>
    public sealed class SYSTEM_EXCEPTION {
        
        #region Constants
        
        public const short ConstVal = 1;
        
        #endregion Constants
        #region IConstructors

        private SYSTEM_EXCEPTION() {
        }        
        
        #endregion IConstructors
        
    }        

    /// <summary>the reply status in request-info</summary>
    public sealed class USER_EXCEPTION {
        
        #region Constants
        
        public const short ConstVal = 2;
        
        #endregion Constants
        #region IConstructors

        private USER_EXCEPTION() {
        }        
        
        #endregion IConstructors
        
    }    
    
    /// <summary>the reply status in request-info</summary>
    public sealed class LOCATION_FORWARD {
        
        #region Constants
        
        public const short ConstVal = 3;
        
        #endregion Constants
        #region IConstructors

        private LOCATION_FORWARD() {
        }        
        
        #endregion IConstructors
        
    }    

    /// <summary>the reply status in request-info</summary>
    public sealed class TRANSPORT_RETRY {
        
        #region Constants
        
        public const short ConstVal = 4;
        
        #endregion Constants
        #region IConstructors

        private TRANSPORT_RETRY() {
        }        
        
        #endregion IConstructors
        
    }        
        
    
    [RepositoryID("IDL:omg.org/PortableInterceptor/InvalidSlot:1.0")]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public class InvalidSlot : AbstractUserException {

        #region IConstructors
        
        public InvalidSlot() {
        }
        
        public InvalidSlot(string reason) : base(reason) {
        }
        
        protected InvalidSlot(System.Runtime.Serialization.SerializationInfo info,
                              System.Runtime.Serialization.StreamingContext context) : base(info, context) {            
        }                
        
        #endregion IConstructors
    }

    
    [RepositoryID("IDL:omg.org/PortableInterceptor/ForwardRequest:1.0")]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public class ForwardRequest : AbstractUserException {

        #region IFields
        
        [ExplicitSerializationOrderNr(0)]
        private MarshalByRefObject m_forward;
        
        #endregion IFields
        #region IConstructors
        
        public ForwardRequest() {            
        }
        
        public ForwardRequest(MarshalByRefObject forward) {
            m_forward = forward;
        }
        
        protected ForwardRequest(System.Runtime.Serialization.SerializationInfo info,
                                 System.Runtime.Serialization.StreamingContext context) : base(info, context) {            
            this.m_forward = (MarshalByRefObject)info.GetValue("forwardTo", typeof(MarshalByRefObject));
        }        
        
        #endregion IConstructors
        #region IProperties
        
        public MarshalByRefObject ForwardTo {
            get {
                return m_forward;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info,
                                           System.Runtime.Serialization.StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("forwardTo", m_forward);
        }        
        
        #endregion IMethods        
        
    }
    
    
    /// <summary>
    /// Base interface for all portable interceptors.
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/Interceptor:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface Interceptor {
    
    
        /// <summary>
        /// Each interceptor may have a name that may be used administratively to order the lists of Interceptors.
        /// Only one interceptor of a given name can be registered for each interceptor type except for 
        /// anonymous interceptors (i.e. name empty string): they may be registered more than once.
        /// </summary>
        [StringValue()]
        [WideChar(false)]
        string Name {
            get;
        }
        
        // no ORB.destroy -> the following method is not useful
        // void destroy();
        
    }
    
    
    /// <summary>
    /// Interface to be implemented by a client side request interceptor. 
    /// The client side interceptors intercepts the request/reply sequence at specific points
    /// on the client side.        
    /// </summary>
    /// <remarks>
    /// The interceptor list is traversed in order on sending interception points and
    /// in reverse order on the receiving interception points.
    /// </remarks>
    [RepositoryID("IDL:omg.org/PortableInterceptor/ClientRequestInterceptor:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface ClientRequestInterceptor : Interceptor {
        
        
        /// <summary>
        /// this interception point allows an interceptor to quest request information
        /// and modify the service context before the request is sent to the server.
        /// This point may raise a system exception. If it does, no other interceptors send_request operations 
        /// are called.
        /// The interceptors already on the flow stack are popped, and their receive_exception point are called.
        /// </summary>
        /// <param name="ri"></param>        
        /// <remarks>Interceptors shall follow completion_status semantics if they raise a system exception from
        /// this point: The status shall be COMPLETED_NO</remarks>
        // TODO: forwardrequest description        
        void send_request(ClientRequestInfo ri);
        
        /// <summary>
        /// This interception point allows an Interceptor to query information during a 
        /// TII polling get reply sequency.
        /// </summary>
        /// <remarks>this interception point is never called by IIOP.NET</remarks>
        void send_poll(ClientRequestInfo ri);
        
        // not supported, because no locate request message may be sent
        // void send_poll(ClientRequestInfo ri);
        
        /// <summary>
        /// This interception point allows an interceptor to query the information on a reply after
        /// it is returned from the server and before control is returned to the client.
        /// This point may raise a system exception. If it does, no other interceptors receive_reply operations 
        /// are called.
        /// The remaining interceptors in the flow stack shall have their receive exception interception 
        /// point called.
        /// </summary>
        /// <param name="ri"></param>
        /// <remarks>Interceptors shall follow completion_status semantics if they raise a system exception from
        /// this point: The status shall be COMPLETED_YES</remarks>
        void receive_reply(ClientRequestInfo ri);
        
        /// <summary>
        /// When an exception occurs, this interception point is called. It allows an interceptor
        /// to query the exception's information before it is raised to the client.
        /// </summary>
        /// <param name="ri"></param>
        void receive_exception(ClientRequestInfo ri);
        
        /// <summary>
        /// This interception point allows an Interceptor to query the information available, when
        /// a request results in something other than a normal reply or exception. For example, a request
        /// could result in a retry (e.g. on LOCATION_FORWARD status); or for an asynchronous call
        /// the reply does not follow immediately the request, but control shall return to client and
        /// an ending interception point shall be called.
        /// Asynchronous requests are simply two separate requests: The first received no reply.
        /// The second receives a normal reply. So the normal (no exception) flow is:
        /// send_request followed by receive_other. second: send_request followed by receive_reply.
        /// 
        /// This interception point is also called for oneway requests.
        /// </summary>
        /// <param name="ri"></param>
        void receive_other(ClientRequestInfo ri);
    }    
    
    
    /// <summary>
    /// Interface to be implemented by a server side request interceptor. 
    /// The server side interceptors intercepts the request/reply sequence at specific points
    /// on the server side.        
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/ServerRequestInterceptor:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface ServerRequestInterceptor : Interceptor {
        
        /// <summary>
        /// At this interception point, interceptors must get their service context information
        /// from the incoming request and transfer it to PortableInterceptor::Current's slots.
        /// Hint: Operation parameters are not yet available at this point.
        /// </summary>
        /// <param name="ri"></param>
        /// <remarks>On throwing system exception: completion status is Completed_NO</remarks>
        void receive_request_service_contexts(ServerRequestInfo ri);
        
        /// <summary>
        /// This interception point allos an interceptor to query information after all the information,
        /// including operation parameters are available. 
        /// </summary>
        /// <param name="ri"></param>
        /// <remarks>On throwing system exception: completion status is Completed_NO</remarks>
        void receive_request(ServerRequestInfo ri);
        
        /// <summary>
        /// this interception point allows an interceptor to query reply information and modfy 
        /// the reply service context after the target operation has been invoked and before the 
        /// reply is returned to the client.
        /// </summary>
        /// <param name="ri"></param>
        /// <remarks>On throwing system exception: completion status is Completed_YES</remarks>
        void send_reply(ServerRequestInfo ri);
        
        /// <summary>
        /// When an exception occurs, this interception point is called. It allows an interceptor
        /// to query the exception information and modify the reply service context before
        /// the exception is raised to the client.
        /// </summary>
        /// <param name="ri"></param>
        void send_exception(ServerRequestInfo ri);
        
        /// <summary>
        /// This interception point allows an interceptor to query the information available
        /// when a request results in something other than a normal reply or exception.
        /// </summary>
        /// <param name="ri"></param>
        /// <remarks>On throwing system exception: completion status is Completed_NO</remarks>
        void send_other(ServerRequestInfo ri);
                
    }
    
    

    /// <summary>
    /// base interface for ServerRequestInfo and ClientRequestInfo.
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/RequestInfo:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface RequestInfo {

        #region IProperties
        
        /// <summary>
        /// request id, which identifies the request/reply sequence.
        /// </summary>
        int request_id {
            get;
        }
        
        /// <summary>
        /// The name of the operation being invoked.
        /// </summary>
        [StringValue()]
        [WideChar(false)]        
        string operation {
            get;
        }
        
        /// <summary>the paramter list of the operation.</summary>
        /// <remarks>if the operation has no arguments, this returns a zero length sequence.</remarks>
        [IdlSequence(0L)]
        omg.org.Dynamic.Parameter[] arguments {
            get;
        }
        
        /// <summary>the list of user exceptions, this operation may raise.</summary>
        /// <remarks>if the operation raises no user exceptions, this returns a zero length sequence.</remarks>
        [IdlSequence(0L)]
        omg.org.CORBA.TypeCode[] exceptions {
            get;
        }
        
        /// <summary>the list of contexts that may be passed on invocation.</summary>
        /// <remarks>if the operation supports no context information, this returns a zero length sequence.</remarks>
        [StringValue()]
        [WideChar(false)]
        [IdlSequence(0L)]
        string[] contexts {
            get;
        }

        /// <summary>the contexts being sent for this invocation.</summary>
        /// <remarks>if no context information is sent, this returns a zero length sequence.</remarks>
        [StringValue()]
        [WideChar(false)]
        [IdlSequence(0L)]        
        string[] operation_context {
            get;
        }
        
        /// <summary>
        /// contains the result of the invocation.
        /// </summary>
        object result {
            get;
        }
        
        /// <summary>
        /// indicates, wheter a reponse is expected.
        /// </summary>
        bool response_expected {
            get;
        }
        
        /// <summary>
        /// returns for non-synchronous request, the level of syncronisation with the target.        
        /// </summary>
        short sync_scope {
            get;
        }
        
        /// <summary>
        /// indicates the state of the result of the invocation.
        /// </summary>
        short reply_status {
            get;
        }        
        
        /// <summary>
        /// if reply status is location_forward, this property will contain
        /// the forward target.
        /// </summary>
        MarshalByRefObject forward_reference {
            get;
        }                
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>
        /// This operation returns the data from the given slot of the PortableInterceptor::Current, 
        /// that is in the scope of the request.
        /// </summary>
        [ThrowsIdlException(typeof(InvalidSlot))]
        object get_slot(int id);
        
        /// <summary>
        /// This operation returns a copy of the service context with the given ID that is associated
        /// with the request.
        /// </summary>
        ServiceContext get_request_service_context(int id);
        
        /// <summary>
        /// This operation returns a copy of the service context with the given ID that is associated
        /// with the reply.
        /// </summary>
        ServiceContext get_reply_service_context(int id);
        
        
        #endregion IMethods
    }
    
    
    /// <summary>
    /// used in client side request interceptors to pass information to interception points
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/ClientRequestInfo:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface ClientRequestInfo : RequestInfo {
        
        #region IProperties
        
        /// <summary>the object, the client called to perform the operation</summary>
        MarshalByRefObject target {
            get;
        }
        
        /// <summary>the actual object, the operation will be invoked on.</summary>
        MarshalByRefObject effective_target {
            get;
        }
        
        /// <summary>the profile, which will be used to send the request.</summary>
        TaggedProfile effective_profile {
             get;
        }
        
        /// <summary>the exception to be returned to the client.</summary>
        object received_exception {
            get;
        }
        
        /// <summary>the repository id of the exception returned to the client.</summary>
        [StringValue()]
        [WideChar(false)]
        string received_exception_id {
            get;
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>
        /// gets the component from the profile selected for this request.
        /// </summary>
        TaggedComponent get_effective_component(int id);
        
        /// <summary>
        /// gets the components from the profile selected for this request.
        /// </summary>
        [return: IdlSequence(0L)]
        TaggedComponent[] get_effective_components(int id);
        
        /// <summary>
        /// returns the policy in effect for this request. If policy type not valid, throws INV_POLICY with
        /// minor code 1.
        /// </summary>
        Policy get_request_policy(int type);
        
        /// <summary>
        /// allows interceptor to add service context to the request. if replace is true, an 
        /// already existing context is replaced by this one.
        /// </summary>
        void add_request_service_context(ServiceContext service_context, bool replace);
        
        #endregion IMethods
        
        
    }
    
    
    /// <summary>
    /// used in server side request interceptors to pass information to interception points
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/ServerRequestInfo:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface ServerRequestInfo : RequestInfo {

        #region IProperties
        
        /// <summary>the exception to be returned to the client.</summary>
        object sending_exception {
            get;
        }
        
        /// <summary>the opaque id, describing the target of the operation invocation.</summary>
        [IdlSequence(0L)]
        byte[] object_id {
            get;
        }
        
        /// <summary>This attribute is the opaque identifier for the object-adapter.</summary>
        [IdlSequence(0L)]
        byte[] adapter_id {
            get;
        }
        
        /// <summary>the repository id of the most derived interface of the servant.</summary>
        [StringValue()]
        [WideChar(false)]
        string target_most_derived_interface {
            get;
        }
        
        #endregion IProperties
        #region IMethods
                
        /// <summary>returns the policy in effect for the given policy type. If policy was not registered
        /// via register_policy_factory, a INV_POLICY with minor code 2 is returned.</summary>        
        Policy get_server_policy(int type);
        
        /// <summary>
        /// set a slot in the PI::Current, which is in the scope fo the request. if data already existing in
        /// the slot, it is overwritten. InvalidSlot is raised, if slot was not allocated.
        /// </summary>        
        [ThrowsIdlException(typeof(InvalidSlot))]
        void set_slot(int id, object data);
        
        /// <summary>
        /// returns true, if the servant is the given repository id.
        /// </summary>
        bool target_is_a([StringValue()][WideChar(false)] string id);
        
        /// <summary>allows interceptors to add service contexts to the reply.</summary>
        void add_reply_service_context(ServiceContext service_context, bool replace);
        
        #endregion IMethods
        
    }
    
    
    /// <summary>
    /// A portable service implementation may add information to ior's (tagged components)
    /// in order that client side service works correctly.
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/IORInterceptor:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface IORInterceptor : Interceptor {
        
        /// <summary>
        /// establishes tagged components in the profiles within an IOR.
        /// </summary>
        /// <param name="info"></param>
        void establish_components (IORInfo info);
        
    }
    
    
    /// <summary>
    /// The IORInfo allows IORInterceptor (on the server side) to components
    /// to an ior profile.
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/IORInfo:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface IORInfo {
        
        /// <summary>
        /// gets the server side policy for the given type. Throws INV_POLICY with a minor code 2,
        /// if policy not knwon.
        /// </summary>
        Policy get_effective_policy(int type);
        
        /// <summary>
        /// adds the specified tagged component to all profiles.
        /// </summary>        
        void add_ior_component(TaggedComponent component);
        
        /// <summary>
        /// adds the specified tagged component to the profile with the given id.
        /// </summary>        
        void add_ior_component_to_profile(TaggedComponent component, int profileId);
    }
    
    
    /// <summary>
    /// Interface implemented by an orb initalizer, which is used to register portable interceptors.
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/OrbInitalizer:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]    
    public interface ORBInitializer {
        
        void pre_init(ORBInitInfo info);
        
        void post_init(ORBInitInfo info);        
        
    }
    
    
    [RepositoryIDAttribute("IDL:omg.org/PortableInterceptor/DuplicateName:1.0")]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public class DuplicateName : AbstractUserException {
        
        #region IFields
        
        [ExplicitSerializationOrderNr(0)]
        public string name;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>default constructor</summary>
        public DuplicateName(string name) : base("duplicate name : " + name) {
            this.name = name;
        }

        /// <summary>constructor needed for deserialisation</summary>
        public DuplicateName() {
        }
        
        protected DuplicateName(System.Runtime.Serialization.SerializationInfo info,
                                System.Runtime.Serialization.StreamingContext context) : base(info, context) {            
            this.name = info.GetString("name");
        }
        
        #endregion IConstructors
        #region IMethods
        
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info,
                                           System.Runtime.Serialization.StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("name", name);
        }        
        
        #endregion IMethods
    }    
    
    /// <summary>
    /// Interface usable to register interceptors.
    /// </summary>
    [RepositoryID("IDL:omg.org/PortableInterceptor/ORBInitInfo:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]        
    public interface ORBInitInfo {
        
        
        /// <summary>the id of the ORB being intalized</summary>
        [StringValue()]
        [WideChar(false)]
        string orb_id {
            get;
        }
        
        
        /// <summary>a mean for getting a codec during initalization.</summary>
        CodecFactory codec_factory {
            get;
        }
        
        [ThrowsIdlException(typeof(omg.org.PortableInterceptor.DuplicateName))]
        void add_client_request_interceptor(ClientRequestInterceptor interceptor);
        
        [ThrowsIdlException(typeof(omg.org.PortableInterceptor.DuplicateName))]
        void add_server_request_interceptor(ServerRequestInterceptor interceptor);
        
        [ThrowsIdlException(typeof(omg.org.PortableInterceptor.DuplicateName))]
        void add_ior_interceptor(IORInterceptor interceptor);
        
        /// <summary>
        /// allocates a slot on PortableInterceptor::Current
        /// </summary>
        int allocate_slot_id();        
        
    }
    
    [RepositoryID("IDL:omg.org/PortableInterceptor/Current:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]    
    public interface Current : omg.org.CORBA.Current {
        
        /// <summary>
        /// A service can get the slot date it set in PICurrent with this method.
        /// </summary>
        [ThrowsIdlException(typeof(InvalidSlot))]
        object get_slot(int id);

        /// <summary>
        /// A service sets data in a slot via this method. If data already exists, it's overriden.
        /// If set_slot is called on a slot, which is not allocated, InvalidSlot is raised.
        /// set_slot must not be called from withing a ORB initalizer.
        /// </summary>        
        [ThrowsIdlException(typeof(InvalidSlot))]
        void set_slot(int id, object data);
        
    }
      
}
