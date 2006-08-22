/* Messages.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 15.01.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2006 Dominic Ullmann
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
using System.Runtime.Remoting.Messaging;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Interception;
using omg.org.IOP;

namespace Ch.Elca.Iiop.MessageHandling {
    
    /// <summary>
    /// used to specify the LocateStatus for a locate reply
    /// </summary>
    public enum LocateStatus {
        UNKNOWN_OBJECT = 0,
        OBJECT_HERE = 1,
        OBJECT_FORWARD = 2,
        SYSTEM_EXCEPTION = 4 // new for giop 1.2
    }
    
    
    /// <summary>
    /// simple implementation of the IMessage interface
    /// </summary>
    public class SimpleGiopMsg : IMessage {
        
        #region Constants

        /// <summary>the key to access the requestId in the message properties</summary>
        public const string REQUEST_ID_KEY = "_request_ID";
        /// <summary>the key to access the giop-version in the message properties</summary>
        public const string GIOP_VERSION_KEY = "_giop_version";
        /// <summary>the key to access the response-flags in the message properties</summary>
        public const string RESPONSE_FLAGS_KEY = "_response_flags";
        /// <summary>the key to access the idl method name in the message properties</summary>
        public const string IDL_METHOD_NAME_KEY = "_idl_method_name";        
        /// <summary>the key to access the flag, specifying if one of the corba standard ops, like is_a is called or a regular object operation, in the message properties</summary>
        public const string IS_STANDARD_CORBA_OP_KEY = "_is_standard_corba_op";           
        /// <summary>the key to access the client side requested uri in the message properties</summary>
        public const string REQUESTED_URI_KEY = "_requested_uri_op";                   
        /// <summary>the key to access the called method info</summary>
        public const string CALLED_METHOD_KEY = "_called_method";
        /// <summary>the key to access the service context for this message (either request or reply context, depending on the message</summary>
        public const string SERVICE_CONTEXT = "_service_context";
        /// <summary>the key to access the piCurrent request scoped slots for this message</summary>
        public const string PI_CURRENT_SLOTS = "_piCurrent_slots_";
        /// <summary>the key to access the interception flow instance in this message</summary>
        public const string INTERCEPTION_FLOW = "_interception_flow";
        /// <summary>the key to access the isAsyncMessage property in the message properties.</summary>
        public const string IS_ASYNC_REQUEST = "_is_async_request";
        /// <summary>the key to access the selected profile for connection on client side.</summary>
        public const string TARGET_PROFILE_KEY = "_target_profile_key";
        /// <summary>the key used to access the object key in the message properties (only server side)</summary>
        public const string REQUESTED_OBJECT_KEY = "__ObjectKey";        
        /// <summary>the key used to access the uri-property in messages</summary>         
        public const string URI_KEY = "__Uri";
        /// <summary>the key used to access the typename-property in messages</summary>
        public const string TYPENAME_KEY = "__TypeName";
        /// <summary>the key used to access the target type property in messages</summary>
        public const string TARGET_TYPE_KEY = "_target_type_Key";        
        /// <summary>the key used to access the methodname-property in messages</summary>
        public const string METHODNAME_KEY = "__MethodName";
        /// <summary>the key used to access the argument-property in messages</summary>
        public const string ARGS_KEY = "__Args";
        /// <summary>the key used to access the method-signature property in messages</summary>
        public const string METHOD_SIG_KEY = "__MethodSignature";

        #endregion Constants
        #region IFields
        
        private Hashtable m_properties = new Hashtable();

        #endregion IFields
        #region IProperties
    
        #region Implementation of IMessage
        
        public IDictionary Properties {
            get {
                return m_properties;
            }
        }
        
        #endregion

        #endregion IProperties
        #region SMethods
        
        /// <summary>
        /// helper method, which sets the service context inside the message
        /// </summary>
        internal static void SetServiceContextInMessage(IMessage msg, ServiceContextList svcContext) {
            msg.Properties[SimpleGiopMsg.SERVICE_CONTEXT] = svcContext;                        
        }

        /// <summary>
        /// helper method, which gets the service context from the message
        /// </summary>        
        internal static ServiceContextList GetServiceContextFromMessage(IMessage msg) {
            return (ServiceContextList)msg.Properties[SimpleGiopMsg.SERVICE_CONTEXT];
        }
        
        /// <summary>
        /// helper method to the the interception flow  inside a message
        /// </summary>
        internal static void SetInterceptionFlow(IMessage msg, RequestInterceptionFlow flow) {
            msg.Properties[SimpleGiopMsg.INTERCEPTION_FLOW] = flow;
        }
        
        /// <summary>
        /// helper method to extract the interception flow from inside a message
        /// </summary>
        internal static RequestInterceptionFlow GetInterceptionFlow(IMessage msg) {
            return (RequestInterceptionFlow)msg.Properties[SimpleGiopMsg.INTERCEPTION_FLOW];
        }
        
        /// <summary>
        /// helper method to extract the pi current from inside a message
        /// </summary>
        internal static PICurrentImpl GetPICurrent(IMessage msg) {
            return (PICurrentImpl)msg.Properties[SimpleGiopMsg.PI_CURRENT_SLOTS];
        }
        
        /// <summary>
        /// helper method to set the picurrent inside a message.
        /// </summary>
        internal static void SetPICurrent(IMessage msg, PICurrentImpl current) {
            msg.Properties[SimpleGiopMsg.PI_CURRENT_SLOTS] = current;
        }                

        /// <summary>
        /// helper method, which sets the is_async to true for message
        /// </summary>
        internal static void SetMessageAsyncRequest(IMessage msg) {
            msg.Properties[SimpleGiopMsg.IS_ASYNC_REQUEST] = true;
        }                        
        
        /// <summary>
        /// helper method, which gets the is_async for the message
        /// </summary>
        internal static bool IsMessageAsyncRequest(IMessage msg) {
            if (msg.Properties.Contains(SimpleGiopMsg.IS_ASYNC_REQUEST)) {
                return (bool)msg.Properties[SimpleGiopMsg.IS_ASYNC_REQUEST];
            } else {
                return false;
            }
        }
        
        #endregion SMethods        

    }
    
    /// <summary>contains the information for a location forward reply</summary>
    internal class LocationForwardMessage : IMessage {
        
        #region Constants
        
        internal const string FWD_PROXY_KEY = "__FwdToProxy";
        
        #endregion Constants
        #region IFields        
        
        private IDictionary m_properties = new Hashtable();
        
        #endregion IFields
        #region IConstructors
        
        internal LocationForwardMessage(MarshalByRefObject toProxy) {
            m_properties[FWD_PROXY_KEY] = toProxy;            
        }
        
        #endregion IConstructors
        #region IProperties
        
        public IDictionary Properties {
            get {
                return m_properties;
            }
        }
        
        internal MarshalByRefObject FwdToProxy {
            get {
                return (MarshalByRefObject)m_properties[FWD_PROXY_KEY];
            }
        }
        
        #endregion IProperties
        
    }
    
    /// <summary>contains the information for a locate request message</summary>
    internal class LocateRequestMessage : IMessage {
        
        #region Constants
        
        internal const string OBJECT_KEY_KEY = "_ObjectKey";
        internal const string TARGET_URI_KEY = "_TargetUriKey";
        internal const string REQUEST_ID_KEY = "_RequestIdKey";
        
        #endregion Constants
        #region IFields        
        
        private IDictionary m_properties = new Hashtable();
        
        #endregion IFields
        #region IConstructors
        
        internal LocateRequestMessage(uint requestId, byte[] objectKey, string targetUri) {
            m_properties[OBJECT_KEY_KEY] = objectKey;
            m_properties[TARGET_URI_KEY] = targetUri;
            m_properties[REQUEST_ID_KEY] = requestId;
        }
        
        #endregion IConstructors
        #region IProperties
        
        public IDictionary Properties {
            get {
                return m_properties;
            }
        }
        
        internal uint RequestId {
            get {
                return (uint)m_properties[REQUEST_ID_KEY];
            }
        }
        
        internal byte[] ObjectKey {
            get {
                return (byte[])m_properties[OBJECT_KEY_KEY];
            }
        }
        
        internal string TargetUri {
            get {
                return (string)m_properties[TARGET_URI_KEY];
            }
        }
        
        #endregion IProperties        
        
    }
    
    /// <summary>contains the information for a locate request message</summary>
    internal class LocateReplyMessage : IMessage {
        
        #region Constants
        
        internal const string STATUS_KEY = "_StatusKey";
        internal const string FWD_KEY = "_FwdToKey";
        internal const string EXCEPTION_KEY = "_ExceptionKey";
        
        #endregion Constants
        #region IFields        
        
        private IDictionary m_properties = new Hashtable();
        
        #endregion IFields
        #region IConstructors
        
        internal LocateReplyMessage(LocateStatus status) {
            m_properties[STATUS_KEY] = status;            
        }
        
        internal LocateReplyMessage(Exception ex) : this(LocateStatus.SYSTEM_EXCEPTION) {
            m_properties[EXCEPTION_KEY] = ex;
        }
        
        internal LocateReplyMessage(Ior fwdTo) : this(LocateStatus.OBJECT_FORWARD) {
            m_properties[FWD_KEY] = fwdTo;
        }
        
        #endregion IConstructors
        #region IProperties
        
        public IDictionary Properties {
            get {
                return m_properties;
            }
        }
        
        internal LocateStatus Status {
            get {
                return (LocateStatus)m_properties[STATUS_KEY];
            }
        }
        
        internal Exception Exception {
            get {
                return (Exception)m_properties[EXCEPTION_KEY];
            }
        }

        internal Ior FwdTo {
            get {
                return (Ior)m_properties[FWD_KEY];
            }
        }        
        
        #endregion IProperties
        
    }
    
    
}
