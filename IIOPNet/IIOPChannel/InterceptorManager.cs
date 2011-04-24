/* InterceptorManager.cs
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
using System.Collections;
using Ch.Elca.Iiop.Idl;
using omg.org.PortableInterceptor;
using omg.org.CORBA;
using omg.org.IOP;


namespace Ch.Elca.Iiop.Interception {

 
    /// <summary>
    /// manages the corba portable interceptors.
    /// </summary>
    internal sealed class InterceptorManager {
 
        #region SFields
 
        private static ClientRequestInterceptor[] s_emptyClientRequestInterceptors = new ClientRequestInterceptor[0];
        private static ServerRequestInterceptor[] s_emptyServerRequestInterceptors = new ServerRequestInterceptor[0];
        private static IORInterceptor[] s_emptyIorInterceptors = new IORInterceptor[0];
 
        internal static IInterceptionOption[] EmptyInterceptorOptions = new IInterceptionOption[0];
 
        #endregion SFields
        #region IFields
 
        private volatile bool m_interceptionRegistrationComplete;
        private IDictionary m_namedClientRequestInterceptors = new Hashtable();
        private IList m_unnamedClientRequestInterceptors = new ArrayList();
 
        private IDictionary m_namedServerRequestInterceptors = new Hashtable();
        private IList m_unnamedServerRequestInterceptors = new ArrayList();

        private IDictionary m_namedIorInterceptors = new Hashtable();
        private IList m_unnamedIorInterceptors = new ArrayList();
 
        // the initalized interceptors
 
        private ClientRequestInterceptor[] m_clientRequestInterceptorsInitalized;
        private ServerRequestInterceptor[] m_serverRequestInterceptorsInitalized;
        private IORInterceptor[] m_iorInterceptorsInitalized;
 
        private OrbServices m_orb;
 
        #endregion IFields
        #region IConstructors
 
        internal InterceptorManager(OrbServices orb) {
            m_interceptionRegistrationComplete = false;
            m_orb = orb;
        }
 
        #endregion IConstructors
        #region IProperties
 
        /// <summary>
        /// is the registration of interceptors completed.
        /// </summary>
        internal bool RegistrationComplete {
            get {
                lock(this) {
                    return m_interceptionRegistrationComplete;
                }
            }
        }
 
        #endregion IProperties
        #region IMethods
 
        /// <summary>
        /// the active client request interceptors
        /// </summary>
        internal ClientRequestInterceptor[] GetClientRequestInterceptors(params IInterceptionOption[] options) {
            ClientRequestInterceptor[] result;
            if (m_interceptionRegistrationComplete) {
                result = m_clientRequestInterceptorsInitalized;
            } else {
                // registration incomplete.
                result = s_emptyClientRequestInterceptors;
            }
            if (options.Length > 0) {
                ClientRequestInterceptor[] oldResult = result;
                result = new ClientRequestInterceptor[oldResult.Length + options.Length];
                for (int i = 0; i < oldResult.Length; i++) {
                    result[i] = oldResult[i];
                }
                for (int i = 0; i < options.Length; i++) {
                    result[oldResult.Length + i] = options[i].GetClientRequestInterceptor(m_orb);
                }
            }
            return result;
        }

        /// <summary>
        /// the active server request interceptors
        /// </summary>
        internal ServerRequestInterceptor[] GetServerRequestInterceptors(params IInterceptionOption[] options) {
            ServerRequestInterceptor[] result;
            if (m_interceptionRegistrationComplete) {
                result = m_serverRequestInterceptorsInitalized;
            } else {
                // registration incomplete.
                result = s_emptyServerRequestInterceptors;
            }
            if (options.Length > 0) {
                ServerRequestInterceptor[] oldResult = result;
                result = new ServerRequestInterceptor[oldResult.Length + options.Length];
                for (int i = 0; i < oldResult.Length; i++) {
                    result[i] = oldResult[i];
                }
                for (int i = 0; i < options.Length; i++) {
                    result[oldResult.Length + i] = options[i].GetServerRequestInterceptor(m_orb);
                }
            }
            return result;
        }

        /// <summary>
        /// the active ior interceptors
        /// </summary>
        internal IORInterceptor[] GetIorInterceptors() {
            if (m_interceptionRegistrationComplete) {
                return m_iorInterceptorsInitalized;
            } else {
                // registration incomplete.
                return s_emptyIorInterceptors;
            }
        }
 
        /// <summary>
        /// complete the registration of interceptors.
        /// </summary>
        /// <param name="orb_initalizers"></param>
        internal void CompleteInterceptorRegistration(IList /* orb_initalizers */ orbInitalizers) {
            lock(this) {
                try {
                    if (m_interceptionRegistrationComplete) {
                        throw new BAD_INV_ORDER(700, CompletionStatus.Completed_MayBe);
                    }
                    // call all registered orb initalizers.
                    ORBInitInfoImpl info = new ORBInitInfoImpl(m_orb);
                    foreach (ORBInitializer init in orbInitalizers) {
                        init.pre_init(info);
                    }
                    foreach (ORBInitializer init in orbInitalizers) {
                        init.post_init(info);
                    }
                    InstallInterceptors();
                } finally {
                    m_interceptionRegistrationComplete = true;
                }
            }
        }
 
        /// <summary>
        /// copy the registered interceptors to the list of active interceptors. They become active just
        /// after m_interceptionRegistrationComplete has been set to true.
        /// </summary>
        /// <param name="interceptor"></param>
        private void InstallInterceptors() {
            m_clientRequestInterceptorsInitalized = new ClientRequestInterceptor[m_namedClientRequestInterceptors.Count +
                                                                                 m_unnamedClientRequestInterceptors.Count];
            m_unnamedClientRequestInterceptors.CopyTo(m_clientRequestInterceptorsInitalized, 0);
            m_namedClientRequestInterceptors.Values.CopyTo(m_clientRequestInterceptorsInitalized,
                                                           m_unnamedClientRequestInterceptors.Count);
 
            m_serverRequestInterceptorsInitalized = new ServerRequestInterceptor[m_namedServerRequestInterceptors.Count +
                                                                                 m_unnamedServerRequestInterceptors.Count];
            m_unnamedServerRequestInterceptors.CopyTo(m_serverRequestInterceptorsInitalized, 0);
            m_namedServerRequestInterceptors.Values.CopyTo(m_serverRequestInterceptorsInitalized,
                                                           m_unnamedServerRequestInterceptors.Count);
 
            m_iorInterceptorsInitalized = new IORInterceptor[m_namedIorInterceptors.Count +
                                                             m_unnamedIorInterceptors.Count];
            m_unnamedIorInterceptors.CopyTo(m_iorInterceptorsInitalized, 0);
            m_namedIorInterceptors.Values.CopyTo(m_iorInterceptorsInitalized,
                                                 m_unnamedIorInterceptors.Count);
        }
 
        /// <summary>
        /// adds a client request interceptor to the inactive client request interceptors. Not possible any more
        /// after registration completed.
        /// </summary>
        internal void add_client_request_interceptor(ClientRequestInterceptor interceptor) {
            lock(this) {
                if (m_interceptionRegistrationComplete) {
                    throw new BAD_INV_ORDER(701, CompletionStatus.Completed_No);
                }
                if (interceptor.Name != null && interceptor.Name != String.Empty) {
                    if (!m_namedClientRequestInterceptors.Contains(interceptor.Name)) {
                        m_namedClientRequestInterceptors[interceptor.Name] = interceptor;
                    } else {
                        throw new DuplicateName(interceptor.Name);
                    }
                } else {
                    m_unnamedClientRequestInterceptors.Add(interceptor);
                }
            }
 
        }
 
        /// <summary>
        /// adds a server request interceptor to the inactive serverrequest interceptors. Not possible any more
        /// after registration completed.
        /// </summary>
        internal void add_server_request_interceptor(ServerRequestInterceptor interceptor) {
            lock(this) {
                if (m_interceptionRegistrationComplete) {
                    throw new BAD_INV_ORDER(701, CompletionStatus.Completed_No);
                }
                if (interceptor.Name != null && interceptor.Name != String.Empty) {
                    if (!m_namedServerRequestInterceptors.Contains(interceptor.Name)) {
                        m_namedServerRequestInterceptors[interceptor.Name] = interceptor;
                    } else {
                        throw new DuplicateName(interceptor.Name);
                    }
                } else {
                    m_unnamedServerRequestInterceptors.Add(interceptor);
                }
            }
        }
 
        /// <summary>
        /// adds a ior interceptor to the inactive ior interceptors. Not possible any more
        /// after registration completed.
        /// </summary>
        internal void add_ior_interceptor(IORInterceptor interceptor) {
            lock(this) {
                if (m_interceptionRegistrationComplete) {
                    throw new BAD_INV_ORDER(701, CompletionStatus.Completed_No);
                }
                if (interceptor.Name != null && interceptor.Name != String.Empty) {
                    if (!m_namedIorInterceptors.Contains(interceptor.Name)) {
                        m_namedIorInterceptors[interceptor.Name] = interceptor;
                    } else {
                        throw new DuplicateName(interceptor.Name);
                    }
                } else {
                    m_unnamedIorInterceptors.Add(interceptor);
                }
            }
        }
 
        #endregion IMethods
 
 
    }
 
 
    /// <summary>
    /// default implemention of ORBInitInfo interface.
    /// </summary>
    internal sealed class ORBInitInfoImpl : ORBInitInfo {
 
        private InterceptorManager m_manager;
        private CodecFactory m_codecFactory;
        private OrbServices m_orb;
 
        internal ORBInitInfoImpl(OrbServices orb) {
            m_orb = orb;
            m_manager = orb.InterceptorManager;
            m_codecFactory = orb.CodecFactory;
        }
 
 
        /// <summary><see cref="omg.org.IOP.ORBInitInfo.orb_id"></see></summary>
        [StringValue()]
        [WideChar(false)]
        public string orb_id {
            get {
                return "IIOP.NET";
            }
        }
 
 
        /// <summary><see cref="omg.org.IOP.ORBInitInfo.codec_factory"></see></summary>
        public omg.org.IOP.CodecFactory codec_factory {
            get {
                return m_codecFactory;
            }
        }
 
 
        public void add_client_request_interceptor(ClientRequestInterceptor interceptor) {
            if (m_manager.RegistrationComplete) {
                throw new OBJECT_NOT_EXIST(701, CompletionStatus.Completed_No);
            }
            m_manager.add_client_request_interceptor(interceptor);
        }
 
        public void add_server_request_interceptor(ServerRequestInterceptor interceptor) {
            if (m_manager.RegistrationComplete) {
                throw new OBJECT_NOT_EXIST(701, CompletionStatus.Completed_No);
            }
            m_manager.add_server_request_interceptor(interceptor);
        }
 
        public void add_ior_interceptor(IORInterceptor interceptor) {
            if (m_manager.RegistrationComplete) {
                throw new OBJECT_NOT_EXIST(701, CompletionStatus.Completed_No);
            }
            m_manager.add_ior_interceptor(interceptor);
        }
 
        /// <summary>
        /// <see cref="omg.org.IOP.ORBInitInfo.allocate_slot_id"></see>
        /// </summary>
        public int allocate_slot_id() {
            return m_orb.PICurrentManager.AllocateSlotId();
        }

 
    }
 
 
    /// <summary>
    /// interface for specifying interceptors, which are activated optionally by the IIOP.NET infrastructure
    /// </summary>
    internal interface IInterceptionOption {
 
        /// <summary>
        /// returns the optional interceptor or null, if not present for this option.
        /// </summary>
        ServerRequestInterceptor GetServerRequestInterceptor(OrbServices orb);
 
        /// <summary>
        /// returns the optional interceptor or null, if not present for this option.
        /// </summary>
        ClientRequestInterceptor GetClientRequestInterceptor(OrbServices orb);
 
    }

}
