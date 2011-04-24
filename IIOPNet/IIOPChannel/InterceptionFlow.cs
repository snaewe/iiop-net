/* InterceptionFlow.cs
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

namespace Ch.Elca.Iiop.Interception {
 
 
    /// <summary>
    /// Base class of all interception flows, i.e. clientrequest, serverrequest and ior
    /// </summary>
    internal abstract class InterceptionFlow {
 
        #region SFields
 
        private static Interceptor[] s_emptyInterceptionList = new Interceptor[0];
 
        #endregion SFields
        #region IFields
 
        private Interceptor[] m_interceptors;
        private int m_currentInterceptor;
        private int m_increment;
 
        #endregion IFields
        #region IConstructors
 
        internal InterceptionFlow() : this(s_emptyInterceptionList) {
        }
 
        internal InterceptionFlow(Interceptor[] interceptors) {
            m_increment = 1;
            m_interceptors = interceptors;
            ResetToStart();
        }
 
        #endregion IConstructors
        #region IProperties
 
        protected int Increment {
            get {
                return m_increment;
            }
            set {
                m_increment = value;
            }
        }
 
        #endregion IProperties
        #region IMethods
 
        /// <summary>
        /// are there any interceptors?
        /// </summary>
        protected bool HasInterceptors() {
            return m_interceptors.Length > 0;
        }
 
        protected Interceptor GetCurrent() {
            return m_interceptors[m_currentInterceptor];
        }
 
        internal bool HasNextInterceptor() {
            return ((m_currentInterceptor + m_increment >= 0) &&
                    (m_currentInterceptor + m_increment < m_interceptors.Length));
        }
 
        /// <summary>
        /// sets the flow to the next interceptor in the flow; at the beginning; positioned before the
        /// first element.
        /// </summary>
        /// <returns>true, if positioned on a new valid element; false if no new element, i.e. end reached.</returns>
        internal bool ProceedToNextInterceptor() {
            if (HasNextInterceptor()) {
                m_currentInterceptor +=  m_increment;
                return true;
            } else {
                if (m_increment > 0) {
                    m_currentInterceptor = m_interceptors.Length; // position on element after last
                } else {
                    m_currentInterceptor = -1; // position on element after last
                }
                return false;
            }
        }
 
        /// <summary>
        /// position before the first interception point in the flow.
        /// </summary>
        internal void ResetToStart() {
            if (m_increment > 0) {
                m_currentInterceptor = -1;
            } else {
                m_currentInterceptor = m_interceptors.Length;
            }
        }
 
        protected void SwitchDirection() {
            m_increment = -1 * m_increment;
        }
 
        #endregion IMethods
 
    }
 
 
    /// <summary>
    /// Base class of all request interception flows, i.e. clientrequest, serverrequest
    /// </summary>
    internal abstract class RequestInterceptionFlow : InterceptionFlow {
 
        #region IConstructors
 
        /// <summary>
        /// for empty interception list.
        /// </summary>
        internal RequestInterceptionFlow() : base() {
        }
 
        internal RequestInterceptionFlow(Interceptor[] interceptors) : base(interceptors) {
        }
 
        #endregion IConstructors
        #region IMethods
 
        /// <summary>
        /// after the last interception starting points have been called,
        /// swith to the direction for reply processing.
        /// </summary>
        internal void SwitchToReplyDirection() {
            if (!IsInReplyDirection()) {
                SwitchDirection();
            }
        }
 
        internal void SwitchToRequestDirection() {
            if (!IsInRequestDirection()) {
                SwitchDirection();
            }
        }
 
        internal bool IsInReplyDirection() {
            return Increment < 0;
        }
 
        internal bool IsInRequestDirection() {
            return Increment > 0;
        }
 
        /// <summary>
        /// optimization:
        /// should a non-null request info be passed into send/receive methods, or is null ok.
        /// </summary>
        internal bool NeedsRequestInfo() {
            return HasInterceptors();
        }
 
        #endregion IMethods
 
    }
 
 
    /// <summary>
    /// client request interception flow
    /// </summary>
    internal class ClientRequestInterceptionFlow : RequestInterceptionFlow {
 
        #region IConstructors

        /// <summary>
        /// for empty interception list.
        /// </summary>
        internal ClientRequestInterceptionFlow() : base() {
        }

 
        internal ClientRequestInterceptionFlow(ClientRequestInterceptor[] interceptors) : base(interceptors) {
        }
 
        #endregion IConstructors
        #region IMethods
 
        /// <summary>
        /// returns the current interceptor to handle.
        /// </summary>
        private ClientRequestInterceptor GetCurrentInterceptor() {
            return (ClientRequestInterceptor)GetCurrent();
        }
 
        /// <summary>
        /// calls send request interception point. Throws an exception, if an interception point throws
        /// an exception.
        /// </summary>
        internal void SendRequest(ClientRequestInfoImpl clientInfo) {
            while (ProceedToNextInterceptor()) {
                GetCurrentInterceptor().send_request(clientInfo);
            }
        }
 
        /// <summary>
        /// calls receive reply interception point. Throws an exception, if an interception point throws
        /// an exception.
        /// </summary>
        internal void ReceiveReply(ClientRequestInfoImpl clientInfo) {
            while (ProceedToNextInterceptor()) {
                ClientRequestInterceptor current = GetCurrentInterceptor();
                current.receive_reply(clientInfo);
            }
        }
 
        /// <summary>
        /// calls receive reply interception point. Throws an exception, if an interception point throws
        /// an exception.
        /// </summary>
        internal void ReceiveOther(ClientRequestInfoImpl clientInfo) {
            while (ProceedToNextInterceptor()) {
                ClientRequestInterceptor current = GetCurrentInterceptor();
                current.receive_other(clientInfo);
            }
        }
 
        /// <summary>
        /// calls receive exception interception point;
        /// Don't throw exception,if an interception point throws an exception.
        /// Instead, pass the exception on to the next interception point with receive_excpetion.
        /// </summary>
        /// <param name="receivedException"></param>
        /// <returns></returns>
        internal Exception ReceiveException(ClientRequestInfoImpl clientInfo, Exception receivedException) {
            Exception result = receivedException;
            if (clientInfo != null) { // can be null, if no interception chain available -> don't set in this case
                // update exception in requestInfo
                clientInfo.SetReceivedException(receivedException);
            }
            while (ProceedToNextInterceptor()) { // proceed either to the begin element in reply chain, or skip failing element
                ClientRequestInterceptor current = GetCurrentInterceptor();
                try {
                    current.receive_exception(clientInfo);
                } catch (Exception ex) {
                    result = ex;
                    // update exception in requestInfo
                    clientInfo.SetReceivedException(ex);
                }
            }
            return result;
        }
 
        #endregion IMethods

 
    }
 
    /// <summary>
    /// server request interception flow
    /// </summary>
    internal class ServerRequestInterceptionFlow : RequestInterceptionFlow {

 
        #region IConstructors

        /// <summary>
        /// for empty interception list.
        /// </summary>
        internal ServerRequestInterceptionFlow() : base() {
        }

 
        internal ServerRequestInterceptionFlow(ServerRequestInterceptor[] interceptors) : base(interceptors) {
        }
 
        #endregion IConstructors
        #region IMethods
 
        /// <summary>
        /// returns the current interceptor to handle.
        /// </summary>
        private ServerRequestInterceptor GetCurrentInterceptor() {
            return (ServerRequestInterceptor)GetCurrent();
        }
 
        /// <summary>
        /// calls receive request service contexts interception point. Throws an exception, if an interception point throws
        /// an exception.
        /// </summary>
        internal void ReceiveRequestServiceContexts(ServerRequestInfoImpl serverInfo) {
            while (ProceedToNextInterceptor()) {
                ServerRequestInterceptor current = GetCurrentInterceptor();
                current.receive_request_service_contexts(serverInfo);
            }
        }

        /// <summary>
        /// calls receive request interception point. Throws an exception, if an interception point throws
        /// an exception.
        internal void ReceiveRequest(ServerRequestInfoImpl serverInfo) {
            while (ProceedToNextInterceptor()) {
                ServerRequestInterceptor current = GetCurrentInterceptor();
                current.receive_request(serverInfo);
            }
        }
 
        /// <summary>
        /// calls send reply interception point. Throws an exception, if an interception point throws
        /// an exception.
        /// </summary>
        internal void SendReply(ServerRequestInfoImpl serverInfo) {
            while (ProceedToNextInterceptor()) {
                ServerRequestInterceptor current = GetCurrentInterceptor();
                current.send_reply(serverInfo);
            }
        }
 
        /// <summary>
        /// calls send exception interception point;
        /// Don't throw exception,if an interception point throws an exception.
        /// Instead, pass the exception on to the next interception point with send_excpetion.
        /// </summary>
        internal Exception SendException(ServerRequestInfoImpl serverInfo, Exception sentException) {
            Exception result = sentException;
            if (serverInfo != null) { // can be null, if no interception chain available -> don't set in this case
                // update exception in requestInfo
                serverInfo.SetSentException(sentException);
            }
            while (ProceedToNextInterceptor()) { // proceed either to the begin element in reply chain, or skip failing element
                ServerRequestInterceptor current = GetCurrentInterceptor();
                try {
                    current.send_exception(serverInfo);
                } catch (Exception ex) {
                    result = ex;
                    // update exception in requestInfo
                    serverInfo.SetSentException(ex);
                }
            }
            return result;
        }
 
        #endregion IMethods
 
    }

}
