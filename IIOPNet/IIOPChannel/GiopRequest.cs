/* GiopRequest.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 29.03.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Interception;
using omg.org.CORBA;
using omg.org.IOP;
using omg.org.PortableInterceptor;

namespace Ch.Elca.Iiop.MessageHandling {
          

    /// <summary>
    /// base class for GiopClientRequest and GiopServerRequest.
    /// </summary>
    internal abstract class AbstractGiopRequest {
    
        #region IProperties
        
        /// <summary>
        /// the request id
        /// </summary>
        internal abstract uint RequestId {
            get;
            set;
        }
        
        /// <summary>
        /// the name of the target method for this request
        /// </summary>
        internal abstract string RequestMethodName {
            get;
            set;
        }
        
        /// <summary>
        /// the .net uri, describing the target object.
        /// </summary>
        internal abstract string CalledUri {
            get;
            set;
        }
        
        
        /// <summary>
        /// the service context of the request.
        /// </summary>                
        internal abstract ServiceContextList RequestServiceContext {
            get;
            set;
        }
        
        /// <summary>
        /// the service context of the reply.
        /// </summary>        
        internal abstract ServiceContextList ResponseServiceContext {
            get;
            set;
        }
        
        /// <summary>
        /// the request scoped PICurrent.
        /// </summary>
        internal abstract PICurrentImpl PICurrent {
            get;
        }
        
        #endregion IProperties
        
    }
    
    
    /// <summary>
    /// gives access to corba relevant parts of a .NET message for the client side request processing
    /// </summary>
    internal class GiopClientRequest : AbstractGiopRequest {
        
        #region IFields
        
        private IMethodCallMessage m_requestMessage;
        private IMessage m_replyMessage;
        
        private ClientRequestInterceptionFlow m_interceptionFlow;
        private ClientRequestInfoImpl m_clientRequestInfo;   
        
        private GiopClientConnectionDesc m_conDesc;
        
        #endregion IFields
        #region IConstructors
         
        internal GiopClientRequest(IMethodCallMessage requestMsg, GiopClientConnectionDesc conDesc,
                                   IInterceptionOption[] interceptionOptions) {
            m_requestMessage = requestMsg;
            m_conDesc = conDesc;
            IntializeForInterception(interceptionOptions);
        }
        
        #endregion IConstructors
        #region IProperties
        
        /// <summary>
        /// is this request a one way call.
        /// </summary>
        internal bool IsOneWayCall {
            get{
                return RemotingServices.IsOneWay(m_requestMessage.MethodBase);
            }
        }        
        
        /// <summary>
        /// is this request sent asynchornously
        /// </summary>
        internal bool IsAsyncRequest {
            get {
                return SimpleGiopMsg.IsMessageAsyncRequest(m_requestMessage);
            }
        }

        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.RequestId"/>.
        /// </summary>
        internal override uint RequestId {
            get {
                return (uint)m_requestMessage.Properties[SimpleGiopMsg.REQUEST_ID_KEY];
            }
            set {
                m_requestMessage.Properties[SimpleGiopMsg.REQUEST_ID_KEY] = value;
            }
        }    
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.RequestMethodName"/>.
        /// </summary>
        internal override string RequestMethodName {
            get {
                string result = (string)
                    m_requestMessage.Properties[SimpleGiopMsg.IDL_METHOD_NAME_KEY];
                if (result != null) {
                return result;
                } else {
                    throw new BAD_INV_ORDER(200, CompletionStatus.Completed_MayBe);
                }
            }
            set {
                if (value == null) {
                    throw new INTERNAL(200, CompletionStatus.Completed_MayBe);
                }
                m_requestMessage.Properties[SimpleGiopMsg.IDL_METHOD_NAME_KEY] =
                        value;
            }        
        }
        
        /// <summary>
        /// the request arguments
        /// </summary>
        internal object[] RequestArguments {
            get {
                return m_requestMessage.Args;
            }
        }
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.CalledUri"/>.
        /// </summary>
        internal override string CalledUri {
            get {
                return m_requestMessage.Uri;
            }
            set {
                // not changable, but needed to implement interface
                throw new BAD_OPERATION(200, CompletionStatus.Completed_MayBe);

            }
        }
        
        /// <summary>
        /// the profile selected for the connection.
        /// </summary>
        internal Ch.Elca.Iiop.CorbaObjRef.IIorProfile SelectedProfile {
            get {
                return (Ch.Elca.Iiop.CorbaObjRef.IIorProfile)
                    m_requestMessage.Properties[SimpleGiopMsg.TARGET_PROFILE_KEY];
            }
        }
        
        /// <summary>
        /// the MethodInfo of the request target method
        /// </summary>
        internal MethodInfo MethodToCall {
            get {
                return (MethodInfo)m_requestMessage.MethodBase;
            }
        }
        
        /// <summary>
        /// the request call context
        /// </summary>        
        internal LogicalCallContext RequestCallContext {
            get {
                return m_requestMessage.LogicalCallContext;
            }
        }
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.RequestServiceContext"/>.
        /// </summary>
        /// <remarks>if not yet available, creates one.</remarks>
        internal override ServiceContextList RequestServiceContext {
            get {
                ServiceContextList list = SimpleGiopMsg.GetServiceContextFromMessage(m_requestMessage);
                if (list == null) {
                    list = new ServiceContextList();
                    SimpleGiopMsg.SetServiceContextInMessage(m_requestMessage, list);
                }
                return list;
            }
            set {
                ServiceContextList list = SimpleGiopMsg.GetServiceContextFromMessage(m_requestMessage);
                if (list == null) {
                    SimpleGiopMsg.SetServiceContextInMessage(m_requestMessage, value);                    
                } else {
                    // at most settable once
                    throw new BAD_OPERATION(200, CompletionStatus.Completed_MayBe);
                }
            }
        }

        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.ResponseServiceContext"/>.
        /// </summary>        
        /// <remarks>must be set after deserialisation.</remarks>
        internal override ServiceContextList ResponseServiceContext {
            get {
                ServiceContextList list = null;
                if (m_replyMessage != null) {
                    list = SimpleGiopMsg.GetServiceContextFromMessage(m_replyMessage);
                } else {
                    throw new BAD_INV_ORDER(301, CompletionStatus.Completed_MayBe);
                }
                if (list != null) {
                    return list;
                } else {
                    throw new BAD_INV_ORDER(10, CompletionStatus.Completed_MayBe);
                }                
            }
            set {
                ServiceContextList list = SimpleGiopMsg.GetServiceContextFromMessage(m_replyMessage);
                if (list == null) {
                    SimpleGiopMsg.SetServiceContextInMessage(m_replyMessage, value);
                } else {
                    // at most settable once
                    throw new BAD_OPERATION(200, CompletionStatus.Completed_MayBe);
                }                
            }
        }
        
        /// <summary>the .NET remoting request</summary>
        internal IMethodCallMessage Request {
            get {
                return m_requestMessage;
            }
        }
        
        /// <summary>
        /// the reply for this request; set this after deserialisation.
        /// </summary>
        internal IMessage Reply {
            get {
                return m_replyMessage;                
            }
            set {
                m_replyMessage = value;
            }
        }   
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.PICurrent"/>.
        /// </summary>
        internal override PICurrentImpl PICurrent {
            get {
                PICurrentImpl piCurrent =
                    (PICurrentImpl)SimpleGiopMsg.GetPICurrent(m_requestMessage);
                if (piCurrent == null) {
                    piCurrent = OrbServices.GetSingleton().PICurrentManager.CreateEmptyRequestScope();
                    SimpleGiopMsg.SetPICurrent(m_requestMessage, piCurrent);
                }
                return piCurrent;
            }
        }

        /// <summary>
        /// the connection description.
        /// </summary>
        internal GiopClientConnectionDesc ConnectionDesc {
            get {
                return m_conDesc;
            }
        }
                
        #endregion IProperties
        #region IMethods
                
        private void IntializeForInterception(IInterceptionOption[] interceptionOptions) {
            // flow lifetime is bound to message lifetime, GiopClientRequest is only a wrapper around message and
            // can be recreated during message lifetime.
            m_interceptionFlow =
                (ClientRequestInterceptionFlow)SimpleGiopMsg.GetInterceptionFlow(m_requestMessage);
            if (m_interceptionFlow ==  null) {
                ClientRequestInterceptor[] interceptors = 
                    OrbServices.GetSingleton().InterceptorManager.GetClientRequestInterceptors(interceptionOptions);
                if (interceptors.Length == 0) {
                    m_interceptionFlow = new ClientRequestInterceptionFlow();
                } else {
                    m_interceptionFlow = new ClientRequestInterceptionFlow(interceptors);
                }
                SimpleGiopMsg.SetInterceptionFlow(m_requestMessage, m_interceptionFlow);
            }
            if (m_interceptionFlow.NeedsRequestInfo()) {
                // optimization: needs not be created, if non-intercepted.
                m_clientRequestInfo = new ClientRequestInfoImpl(this);
            }            
        }
        
        /// <summary>
        /// updates the picurrent from the thread scope PICurrent
        /// </summary>
        internal void SetRequestPICurrentFromThreadScopeCurrent() {
            PICurrentImpl piCurrent = OrbServices.GetSingleton().PICurrentManager.CreateRequestScopeFromThreadScope();
            SimpleGiopMsg.SetPICurrent(m_requestMessage, piCurrent);
        }                
                        
        /// <summary>
        /// portable interception point: send request
        /// </summary>
        /// <remarks>throws exception, if a problem occurs during call of send request interception points.
        /// Client need to handle exception by calling InterceptReceiveException at the appropriate time and
        /// pass the exception on to the client.</remarks>
        internal void InterceptSendRequest() {            
            m_interceptionFlow.SendRequest(m_clientRequestInfo);            
        }
        
        /// <summary>
        /// portable interception point: receive reply
        /// </summary>        
        /// <remarks>in case of interception point throwing an excpetion: pass the exception through
        /// the remaining interception points by calling receive exception. The exception is at the
        /// end thrown to the caller for further handling.</remarks>
        internal void InterceptReceiveReply() {            
            m_interceptionFlow.SwitchToReplyDirection(); // make sure, flow is in reply direction
            try {
                m_interceptionFlow.ReceiveReply(m_clientRequestInfo);
            } catch (Exception ex) {
                throw m_interceptionFlow.ReceiveException(m_clientRequestInfo, ex);
            }
        }

        /// <summary>
        /// portable interception point: receive exception
        /// </summary>        
        /// <returns>the modified or unmodified receivedException, depending on the interception chain:
        /// the interception chain may change the resulting exception.</returns>
        /// <remarks>unexpected exceptions during interception chain processing are thrown to the caller.</remarks>
        internal Exception InterceptReceiveException(Exception receivedException) {            
            m_interceptionFlow.SwitchToReplyDirection(); // make sure, flow is in reply direction
            return m_interceptionFlow.ReceiveException(m_clientRequestInfo, receivedException);            
        }

        /// <summary>
        /// portable interception point: receive other
        /// </summary>
        /// <remarks>in case of interception point throwing an excpetion: pass the exception through
        /// the remaining interception points by calling receive exception. The exception is at the
        /// end thrown to the caller for further handling.</remarks>
        internal void InterceptReceiveOther() {            
            m_interceptionFlow.SwitchToReplyDirection(); // make sure, flow is in reply direction
            try {
                m_interceptionFlow.ReceiveOther(m_clientRequestInfo);
            } catch (Exception ex) {
                throw m_interceptionFlow.ReceiveException(m_clientRequestInfo, ex);
            }            
        }
        
        /// <summary>
        /// returns false, if reply interception chain has not yet been completed; otherwise true.
        /// </summary>
        internal bool IsReplyInterceptionChainCompleted() {            
            return (m_interceptionFlow.IsInReplyDirection() && !(m_interceptionFlow.HasNextInterceptor()));
        }
        
        /// <summary>
        /// because for async calls, we need two passes through the interception chain, this methods resets the
        /// chain for the second pass.
        /// </summary>
        internal void PrepareSecondAscyncInterception() {
            m_interceptionFlow.SwitchToRequestDirection();
            m_interceptionFlow.ResetToStart();
        }
                
        #endregion IMethods
         
    }
    
    
    /// <summary>
    /// gives access to corba relevant parts of a .NET message for the sever side request processing
    /// </summary>    
    internal class GiopServerRequest : AbstractGiopRequest {
        
        #region IFields
        
        private IMessage m_requestMessage;
        /// <summary>
        /// the request as IMethodCallMessage; Is null on the request path
        /// </summary>
        private IMethodCallMessage m_requestCallMessage;
        private ReturnMessage m_replyMessage;
        
        private ServerRequestInterceptionFlow m_interceptionFlow;
        private ServerRequestInfoImpl m_serverRequestInfo;                
        
        private GiopConnectionDesc m_conDesc;
        
        #endregion IFields
        #region IConstructors
    
        /// <summary>
        /// constructor for the in direction.
        /// </summary>
        internal GiopServerRequest(GiopConnectionDesc conDesc,
                                   IInterceptionOption[] interceptionOptions) {
            m_requestMessage = new SimpleGiopMsg();
            m_requestCallMessage = null; // not yet created; will be created from requestMessage later.
            m_replyMessage = null; // not yet available
            m_conDesc = conDesc;
            InitalizeForInterception(interceptionOptions);
        }
        
        /// <summary>
        /// constructor for the out-direction
        /// </summary>
        /// <param name="request">the request message, may be null</param>
        /// <param name="reply">the reply message</param>
        internal GiopServerRequest(IMessage request, ReturnMessage reply, GiopConnectionDesc conDesc,
                                   IInterceptionOption[] interceptionOptions) {
            if (request is IMethodCallMessage) {                
                m_requestCallMessage = (IMethodCallMessage)request;
            }
            m_requestMessage = request;
            m_replyMessage = reply;
            m_conDesc = conDesc;
            InitalizeForInterception(interceptionOptions);
        }
        
        #endregion IConstructors
        #region IProperties

        private object Get(string propertyName, int minor) {
            object ret = m_requestMessage.Properties[propertyName];
            if (ret != null) {
                return ret;
            }
            throw new BAD_INV_ORDER(minor, CompletionStatus.Completed_MayBe);
        }

        private void Set(string propertyName, object value, int minor) {
            if (m_requestCallMessage == null) {
                m_requestMessage.Properties[propertyName] = value;
            } else {
                throw new BAD_OPERATION(minor, CompletionStatus.Completed_MayBe);
            }
        }

        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.RequestId"/>.
        /// </summary>
        internal override uint RequestId {
            get { return (uint)Get(SimpleGiopMsg.REQUEST_ID_KEY, 200); }
            set { Set(SimpleGiopMsg.REQUEST_ID_KEY, value, 200); }
        }        
                
        /// <summary>
        /// the giop version this message is encoded with
        /// </summary>
        internal GiopVersion Version {
            get { return (GiopVersion)Get(SimpleGiopMsg.GIOP_VERSION_KEY, 201); }
            set { Set(SimpleGiopMsg.GIOP_VERSION_KEY, value, 201); }
        }
        
        /// <summary>
        /// the response flags for this request
        /// </summary>
        internal byte ResponseFlags {
            get { return (byte)Get(SimpleGiopMsg.RESPONSE_FLAGS_KEY, 202); }
            set { Set(SimpleGiopMsg.RESPONSE_FLAGS_KEY, value, 202); }
        }
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.RequestMethodName"/>.
        /// </summary>
        internal override string RequestMethodName {
            get { return (string)Get(SimpleGiopMsg.IDL_METHOD_NAME_KEY, 203); }
            set { Set(SimpleGiopMsg.IDL_METHOD_NAME_KEY, value, 203); }
        }
        
        /// <summary>
        /// the key of the target object
        /// </summary>
        internal byte[] ObjectKey {
            get { return (byte[])Get(SimpleGiopMsg.REQUESTED_OBJECT_KEY, 10); }
            set { Set(SimpleGiopMsg.REQUESTED_OBJECT_KEY, value, 216); }
        }        
        
        /// <summary>
        /// the uri requested by the client; may be or may not be the uri, which is called at the end 
        /// (because of redirections of some request to other objects).
        /// </summary>
        internal string RequestUri {
            get { return (string)Get(SimpleGiopMsg.REQUESTED_URI_KEY, 204); }
            set { Set(SimpleGiopMsg.REQUESTED_URI_KEY, value, 204); }
        }
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.CalledUri"/>.
        /// </summary>
        internal override string CalledUri {
            get {
                if (m_requestCallMessage == null) {
                    return (string)Get(SimpleGiopMsg.URI_KEY, 205);
                } else {
                    return m_requestCallMessage.Uri;
                }
            }
            set { Set(SimpleGiopMsg.URI_KEY, value, 205); }
        }
        
        /// <summary>
        /// the signature of the called .net method
        /// </summary>
        private Type[] CalledMethodSignature {
            get {
                if (m_requestCallMessage == null) {
                    return (Type[])Get(SimpleGiopMsg.METHOD_SIG_KEY, 206);
                } else {
                    return (Type[])m_requestCallMessage.MethodSignature;
                }
            }
            set { Set(SimpleGiopMsg.METHOD_SIG_KEY, value, 206); }
        }
        
        /// <summary>the name of the called .net method; can be different from RequestMethodName.</summary>
        internal string CalledMethodName {
            get {
                if (m_requestCallMessage == null) {
                    return (string)Get(SimpleGiopMsg.METHODNAME_KEY, 207);
                } else {
                    return m_requestCallMessage.MethodName;
                }
            }
            set { Set(SimpleGiopMsg.METHODNAME_KEY, value, 207); }
        }
        
        /// <summary>the info of the called .net method.</summary>
        internal MethodInfo CalledMethod {
            get { return (MethodInfo)Get(SimpleGiopMsg.CALLED_METHOD_KEY, 208); }
            set { Set(SimpleGiopMsg.CALLED_METHOD_KEY, value, 208); }
        }        
        
        /// <summary>
        /// is one of the standard corba operaton like is_a called, or a regular operation.
        /// </summary>
        internal bool IsStandardCorbaOperation {
            get { return (bool)Get(SimpleGiopMsg.IS_STANDARD_CORBA_OP_KEY, 209); }
            set { Set(SimpleGiopMsg.IS_STANDARD_CORBA_OP_KEY, value, 209); }
        }
        
        /// <summary>
        /// the full name of the type of .NET object, servicing this request.
        /// </summary>
        internal string ServerTypeName {
            get {
                if (m_requestCallMessage == null) {
                    return (string)Get(SimpleGiopMsg.TYPENAME_KEY, 210);
                } else {
                    return m_requestCallMessage.TypeName;
                }
            }
            set { Set(SimpleGiopMsg.TYPENAME_KEY, value, 210); }
        }
        
        /// <summary>
        /// the type of .NET object, servicing this request.
        /// </summary>
        internal Type ServerTypeType {
            get { return (Type)Get(SimpleGiopMsg.TARGET_TYPE_KEY, 210); }
            set { Set(SimpleGiopMsg.TARGET_TYPE_KEY, value, 210); }
        }        
        
        /// <summary>
        /// the request arguments
        /// </summary>
        internal object[] RequestArgs {
            get {
                if (m_requestCallMessage == null) {
                    return (object[])Get(SimpleGiopMsg.ARGS_KEY, 211);
                } else {
                    return m_requestCallMessage.Args;
                }
            }
            set { Set(SimpleGiopMsg.ARGS_KEY, value, 211); }
        }
        
        /// <summary>
        /// the reply out arguments
        /// </summary>
        internal object[] OutArgs {
            get {
                if (m_replyMessage != null) {
                    return m_replyMessage.OutArgs;
                } else {
                    throw new BAD_INV_ORDER(212, CompletionStatus.Completed_MayBe);
                }
            }
        }
        
        /// <summary>
        /// the reply value
        /// </summary>
        internal object ReturnValue {
            get {
                if (m_replyMessage != null) {
                    return m_replyMessage.ReturnValue;
                } else {
                    throw new BAD_INV_ORDER(213, CompletionStatus.Completed_MayBe);
                }                
            }
        }
        
        /// <summary>
        /// is the reply an exception or not
        /// </summary>
        internal bool IsExceptionReply {
            get {
                if (m_replyMessage != null) {
                    return m_replyMessage.Exception != null;
                } else {
                    throw new BAD_INV_ORDER(214, CompletionStatus.Completed_MayBe);
                }
            }
        }
        
        /// <summary>
        /// the exception thrown by the invocation or null, if no exception encountered.
        /// </summary>
        internal Exception Exception {
            get {
                if (m_replyMessage != null) {
                    return m_replyMessage.Exception;
                } else {
                    throw new BAD_INV_ORDER(215, CompletionStatus.Completed_MayBe);
                }                
            }
        }
        
        /// <summary>
        /// the exception to pass to the client or null, if no exception encountered.
        /// </summary>
        internal Exception IdlException {
            get {
                Exception ex = Exception;
                if (ex != null) {
                    MethodBase calledMethod = GetCalledMethodInternal();
                    return DetermineExceptionToThrow(ex, calledMethod);
                } else {
                    return null;
                }
            }
        }                

        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.RequestServiceContext"/>.
        /// </summary>
        /// <remarks>must be set after deserialisation using the setter method</remarks>        
        internal override ServiceContextList RequestServiceContext {
            get {
                ServiceContextList list = SimpleGiopMsg.GetServiceContextFromMessage(m_requestMessage);
                if (list != null) {
                    return list;
                } else {
                    throw new BAD_INV_ORDER(10, CompletionStatus.Completed_MayBe);
                }                
            }
            set {
                ServiceContextList list = SimpleGiopMsg.GetServiceContextFromMessage(m_requestMessage);
                if (list == null) {
                    SimpleGiopMsg.SetServiceContextInMessage(m_requestMessage, value);
                } else {
                    // at most settable once
                    throw new BAD_OPERATION(200, CompletionStatus.Completed_MayBe);
                }                
            }
        }

        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.ResponseServiceContext"/>.
        /// </summary>        
        /// <remarks>if not yet available, creates one.</remarks>
        internal override ServiceContextList ResponseServiceContext {
            get {
                ServiceContextList list = null;
                if (m_replyMessage != null) {
                    list = SimpleGiopMsg.GetServiceContextFromMessage(m_replyMessage);
                } else {
                    throw new BAD_INV_ORDER(301, CompletionStatus.Completed_MayBe);
                }
                if (list == null) {
                    list = new ServiceContextList();
                    SimpleGiopMsg.SetServiceContextInMessage(m_replyMessage, list);                    
                }
                return list;
            }            
            set {
                ServiceContextList list = null;
                if (m_replyMessage != null) {
                    list = SimpleGiopMsg.GetServiceContextFromMessage(m_replyMessage);
                } else {
                    throw new BAD_INV_ORDER(301, CompletionStatus.Completed_MayBe);
                }
                if (list == null) {
                    SimpleGiopMsg.SetServiceContextInMessage(m_replyMessage, value);
                } else {
                    // at most settable once
                    throw new BAD_OPERATION(200, CompletionStatus.Completed_MayBe);
                }                
            }
        }        
        
        /// <summary>the .NET remoting request</summary>
        internal IMessage Request {
            get {
                return m_requestMessage;
            }
        }    
        
        /// <summary>the .NET remoting reply</summary>
        internal ReturnMessage Reply {
            get {
                return m_replyMessage;
            }
        }
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.MessageHandling.AbstractGiopRequest.PICurrent"/>.
        /// </summary>
        internal override PICurrentImpl PICurrent {
            get {
                PICurrentImpl piCurrent = 
                    (PICurrentImpl)SimpleGiopMsg.GetPICurrent(m_requestMessage);
                if (piCurrent == null) {
                    piCurrent = OrbServices.GetSingleton().PICurrentManager.CreateEmptyRequestScope();
                    SimpleGiopMsg.SetPICurrent(m_requestMessage, piCurrent);
                }
                return piCurrent;
            }
        }                
        
        /// <summary>
        /// the connection description.
        /// </summary>
        internal GiopConnectionDesc ConnectionDesc {
            get {
                return m_conDesc;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        private void InitalizeForInterception(IInterceptionOption[] interceptionOptions) {
            // flow lifetime is bound to message lifetime, GiopServerRequest is only a wrapper around message and
            // can be recreated during message lifetime.
            m_interceptionFlow = (ServerRequestInterceptionFlow)SimpleGiopMsg.GetInterceptionFlow(m_requestMessage);
            if (m_interceptionFlow ==  null) {
                ServerRequestInterceptor[] interceptors = 
                    OrbServices.GetSingleton().InterceptorManager.GetServerRequestInterceptors(interceptionOptions);
                if (interceptors.Length == 0) {
                    m_interceptionFlow = new ServerRequestInterceptionFlow();
                } else {                    
                    m_interceptionFlow = new ServerRequestInterceptionFlow(interceptors);
                }
                SimpleGiopMsg.SetInterceptionFlow(m_requestMessage, m_interceptionFlow);
            }
            if (m_interceptionFlow.NeedsRequestInfo()) {
                m_serverRequestInfo = new ServerRequestInfoImpl(this);
            }            
        }
        
        /// <summary>
        /// updates the picurrent from the thread scope PICurrent
        /// </summary>
        internal void SetRequestPICurrentFromThreadScopeCurrent() {
            PICurrentImpl piCurrent = OrbServices.GetSingleton().PICurrentManager.CreateRequestScopeFromThreadScope();
            SimpleGiopMsg.SetPICurrent(m_requestMessage, piCurrent);
        }
        
        /// <summary>
        /// updates the thread scope PICurrent from thread scope
        /// </summary>
        internal void SetThreadScopeCurrentFromPICurrent() {
            PICurrentImpl current = PICurrent;
            OrbServices.GetSingleton().PICurrentManager.SetFromRequestScope(current);
        }

        /// <summary>
        /// clears thread scope picurrent.
        /// </summary>
        internal void ClearThreadScopePICurrent() {
            OrbServices.GetSingleton().PICurrentManager.ClearThreadScope();
        }        
                
        /// <summary>
        /// returns the idl method name if available or null, if not yet available.
        /// </summary>
        /// <returns>the requested method name</returns>
        internal string GetRequestedMethodNameInternal() {
            return (string)m_requestMessage.Properties[SimpleGiopMsg.IDL_METHOD_NAME_KEY];
        }
        
        /// <summary>
        /// extracts the called method from the request message. Returns null, if not yet determined.
        /// </summary>
        /// <returns></returns>
        private MethodInfo GetCalledMethodInternal() {
            return (MethodInfo)m_requestMessage.Properties[SimpleGiopMsg.CALLED_METHOD_KEY];
        }
        
        /// <summary>
        /// aquire the information needed to call a standard corba operation, which is possible for every object
        /// </summary>
        /// <param name="methodName">the name of the method called</param>
        /// <returns>the method-info of the method which describes signature for deserialisation</returns>
        private MethodInfo DecodeStandardOperation(string methodName) {
            Type serverType = StandardCorbaOps.s_type; // generic handler
            MethodInfo calledMethodInfo = serverType.GetMethod(methodName); // for parameter unmarshalling, use info of the signature method
            if (calledMethodInfo == null) { 
                // unexpected exception: can't load method of type StandardCorbaOps
                throw new INTERNAL(2801, CompletionStatus.Completed_MayBe);
            }
            return calledMethodInfo;
        }

        /// <summary>
        /// Resolve the target type for this request.
        /// </summary>
        internal void ResolveTargetType() {
            if (m_requestCallMessage != null) { // is fixed
                throw new BAD_OPERATION(300, CompletionStatus.Completed_MayBe);
            }
            
            Type serverType;
            bool regularOp;            
            string calledUri = RequestUri;
                                                
            if (!StandardCorbaOps.CheckIfStandardOp(RequestMethodName)) {
                regularOp = true; // non-pseude op
                serverType = RemotingServices.GetServerTypeForUri(calledUri);
                if (serverType == null) {
                    throw new OBJECT_NOT_EXIST(0, CompletionStatus.Completed_No); 
                }
            } else {
                regularOp = false; // pseude-object op
                calledUri = StandardCorbaOps.WELLKNOWN_URI; // change object-uri
                serverType = StandardCorbaOps.s_type;
            }
            IsStandardCorbaOperation = !regularOp;
            ServerTypeName = serverType.FullName;
            ServerTypeType = serverType;
            CalledUri = calledUri;
        }
        
        /// <summary>
        /// resolve the call to a .net method according to the properties already set.
        /// As a result, update the request message.
        /// </summary>
        /// <remarks>ResolveTargetType must have been called before</remarks>
        internal void ResolveCalledMethod(MethodInfo forRequestMethodName) {
            if (ServerTypeType == null) {
                // ResolveTargetType not called before
                throw new BAD_OPERATION(305, CompletionStatus.Completed_MayBe);
            }
            MethodInfo callForMethod = forRequestMethodName;
            string internalMethodName; // the implementation method name
            if (!IsStandardCorbaOperation) {
                // The called might be on an explicitely implemented interface, it is this
                // interface type that must be presented to RemotingServices for method call
                // to work:
                ServerTypeName = callForMethod.DeclaringType.FullName;
                ServerTypeType = callForMethod.DeclaringType;

                // handle object specific-ops
                internalMethodName = callForMethod.Name;
                // to handle overloads correctly, add signature info:
                CalledMethodSignature = ReflectionHelper.GenerateSigForMethod(callForMethod);                
            } else {
                // handle standard corba-ops like _is_a
                callForMethod = DecodeStandardOperation(RequestMethodName);
                MethodInfo internalCall = 
                    StandardCorbaOps.GetMethodToCallForStandardMethod(callForMethod.Name);
                if (internalCall == null) {
                    throw new INTERNAL(2802, CompletionStatus.Completed_MayBe);    
                }
                internalMethodName = internalCall.Name;
            }            
            CalledMethodName = internalMethodName;            
            CalledMethod = callForMethod;
        }                
                
        /// <summary>
        /// for methods mapped from idl, check if exception is allowed to throw
        /// according to throws clause and if not creae a unknown exception instead.
        /// </summary>
        private Exception DetermineIdlExceptionToThrow(Exception thrown, MethodInfo thrower) {
            // for idl interfaces, check if thrown exception is in the raises clause;
            // if not, throw an unknown system exception
            if (ReflectionHelper.IsExceptionInRaiseAttributes(thrown, thrower) && (thrown is AbstractUserException)) {
                return thrown;
            } else {
                return new UNKNOWN(189, CompletionStatus.Completed_Yes); // if not in raises clause
            }
        }
        
        /// <summary>
        /// determines, which exception to return to the client based on
        /// the called method/attribute and on the Exception thrown.
        /// Make sure to return only exceptions, which are allowed for the thrower; e.g.
        /// only those specified in the interface for methods and for attributes only system exceptions.
        /// </summary>
        private Exception DetermineExceptionToThrow(Exception thrown, MethodBase thrower) {
            if (thrown is omg.org.CORBA.AbstractCORBASystemException) {
                return thrown; // system exceptions are not wrapped or transformed
            }
            Exception exceptionToThrow;
            if ((thrower is MethodInfo) && (!((MethodInfo)thrower).IsSpecialName)) { // is a normal method (i.e. no property accessor, ...)
                if (ReflectionHelper.IIdlEntityType.IsAssignableFrom(thrower.DeclaringType)) { 
                    exceptionToThrow = DetermineIdlExceptionToThrow(thrown,
                                                                    (MethodInfo)thrower);
                } else {
                    if (ReflectionHelper.IsExceptionInRaiseAttributes(thrown, (MethodInfo)thrower) &&
                        (thrown is AbstractUserException)) {
                        exceptionToThrow = thrown; // a .NET method could also use ThrowsIdlException attribute to return non-wrapped exceptions
                    } else {
                        // wrap into generic user exception, because CLS to IDL gen adds this exception to
                        // all methods
                        exceptionToThrow = new GenericUserException(thrown);
                    }
                }
            } else if ((thrower is MethodInfo) && (((MethodInfo)thrower).IsSpecialName)) { // is a special method (i.e. a property accessor, ...) 
                exceptionToThrow = new UNKNOWN(190, CompletionStatus.Completed_Yes);
            } else {
                // thrower == null means here, that the target method was not determined,
                // i.e. the request deserialisation was not ok
                Debug.WriteLine("target method unknown, can't determine what exception client accepts; thrown was: " + 
                                thrown);
                exceptionToThrow = new UNKNOWN(201, CompletionStatus.Completed_No);
            }
            return exceptionToThrow;
        }        
        
        /// <summary>
        /// set the final .net request compiled at the end of deserialisation.
        /// </summary>
        /// <param name="requestCallMessage"></param>
        internal void UpdateWithFinalRequest(IMethodCallMessage requestCallMessage) {
            m_requestCallMessage = requestCallMessage;            
            m_requestMessage = requestCallMessage;               
        }
                
        /// <summary>
        /// portable interception point: receive request service contexts
        /// </summary>
        /// <remarks>throws exception, if a problem occurs during call of  receive request service contexts interception points.
        /// Client need to handle exception by calling InterceptSendException at the appropriate time and
        /// pass the exception on to the client.</remarks>
        internal void InterceptReceiveRequestServiceContexts() {            
            m_interceptionFlow.ReceiveRequestServiceContexts(m_serverRequestInfo);
        }

        /// <summary>
        /// portable interception point: receive request
        /// </summary>
        internal void InterceptReceiveRequest() {            
            try {
                m_interceptionFlow.ResetToStart(); // reset to the first element, because positioned at the end after receive request service contexts.
                m_interceptionFlow.ReceiveRequest(m_serverRequestInfo);
            } catch (Exception) {
                // swith to reply direction and reset to first, because all Receive service contexts 
                // interception points completed -> exception reply must pass all interception points.
                m_interceptionFlow.SwitchToReplyDirection();
                m_interceptionFlow.ResetToStart();
                throw; // exception response
            }
            
        }
        
        /// <summary>
        /// portable interception point: send exception
        /// </summary>
        /// <returns>the modified or unmodified exception after the interception chain has completed.</returns>
        internal Exception InterceptSendException(Exception ex) {            
            m_interceptionFlow.SwitchToReplyDirection(); // make sure, flow is in reply direction
            return m_interceptionFlow.SendException(m_serverRequestInfo, ex);
        }        
        
        /// <summary>
        /// portable interception point: send reply
        /// </summary>
        internal void InterceptSendReply() {            
            m_interceptionFlow.SwitchToReplyDirection(); // make sure, flow is in reply direction
            try {
                m_interceptionFlow.SendReply(m_serverRequestInfo);
            } catch (Exception ex) {
                throw m_interceptionFlow.SendException(m_serverRequestInfo, ex);
            }            
        }                
        
        #endregion IMethods
        
    }
     
}
