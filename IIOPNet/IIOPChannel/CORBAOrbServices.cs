/* CORBAOrbServices.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 11.04.04  Dominic Ullmann (DUL), dul@elca.ch
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
using System.Runtime.Remoting;
using System.Collections;
using System.Diagnostics;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Interception;
using omg.org.PortableInterceptor;
using omg.org.IOP;


namespace omg.org.CORBA {
    
            
    /// <remarks>contains only the CORBA Orb operations supported by IIOP.NET</remarks>
    public interface ORB {
        
        /// <summary>takes an IOR or a corbaloc and returns a proxy</summary>
        object string_to_object([StringValue] string obj);
        
        /// <summary>takes a proxy and returns the IOR / corbaloc / ...</summary>
        string object_to_string(object obj);

		
        /// <summary>allows to access a small set of well defined local objects.></summary>
        /// <remarks>currently supported are: CodecFactory and PICurrent.</remarks>
        [ThrowsIdlException(typeof(omg.org.CORBA.ORB_package.InvalidName))]
        object resolve_initial_references ([StringValue()][WideChar(false)] string identifier);
        
        #region Typecode creation operations

        TypeCode create_interface_tc ([StringValue] [WideChar(false)] string id,
                                      [StringValue] [WideChar(false)] string name);
        
        TypeCode create_string_tc(int bound);
        
        TypeCode create_wstring_tc(int bound);
        
        TypeCode create_ulong_tc();
        
        TypeCode create_ushort_tc();
        
        TypeCode create_ulonglong_tc();    

        TypeCode create_array_tc (int length,
                                  TypeCode element_type);

        TypeCode create_alias_tc ([StringValue] [WideChar(false)] string id, [StringValue] [WideChar(false)] string name, 
                                  TypeCode original_type);
                
        #endregion TypeCode creation operations
        
    }
    
    public interface IOrbServices : ORB {
        
        /// <summary>takes an object an returns the typecode for it</summary>
        TypeCode create_tc_for(object forObject);
        
        /// <summary>takes a type an returns the typecode for it</summary>
        TypeCode create_tc_for_type(Type forType);      
        
        /// <summary>
        /// retrieves a type corresponding to the given typecode.
        /// </summary>
        Type get_type_for_tc(TypeCode tc);
        
        #region Pseudo object operation helpers
                
        /// <summary>checks, if object supports the specified interface</summary>
        bool is_a(object proxy, Type type);
      
        /// <summary>checks, if object supports the specified interface</summary>
        bool is_a(object proxy, string repId);
        
        /// <summary>checks, if the object is existing</summary>
        bool non_existent(object proxy);
        
        #endregion Pseude object operation helpers
        #region Portable Interceptors
	    
        /// <summary>registers an initalizer for portable interceptors. The interceptors are
        /// enabled by calling CompleteInterceptorRegistration.</summary>
        void RegisterPortableInterceptorInitalizer(ORBInitalizer initalizer);
	    
        /// <summary>
        /// completes registration of interceptors. 
        /// Afterwards, the interceptors are enabled and are called during processing.
        /// </summary>
        void CompleteInterceptorRegistration();
	    
        #endregion Protable Interceptors

    }
    
    
    /// <summary>implementation of the Orb interface methods supported by IIOP.NET</summary>
    public sealed class OrbServices : IOrbServices {
    
        #region SFields
        
        private static OrbServices s_singleton = new OrbServices();     
        
        #endregion SFields
        #region IFields
		
        private IList m_orbInitalizers; 
        private InterceptorManager m_interceptorManager;
        private CodecFactory m_codecFactory;
        private Ch.Elca.Iiop.Interception.PICurrentManager m_piCurrentManager;
		
        #endregion IFields
        #region IConstructors
        
        private OrbServices() {         
            m_orbInitalizers = new ArrayList();
            m_codecFactory = new CodecFactoryImpl();
            m_piCurrentManager = new PICurrentManager();
            m_interceptorManager = new InterceptorManager(this);
        }
        
        #endregion IConstructors
        #region SMethods
        
        public static OrbServices GetSingleton() {
            return s_singleton;
        }
        
        #endregion SMethods
        #region IProperties
		
        /// <summary>
        /// the manager responsible for managing the interceptors.
        /// </summary>
        internal InterceptorManager InterceptorManager {
            get {
                return m_interceptorManager;
            }
        }
		
        /// <summary>
        /// returns the instance of the codec factory.
        /// </summary>
        internal CodecFactory CodecFactory {
            get {
                return m_codecFactory;
            }
        }
		
        /// <summary>
        /// returns the thread-scoped instance of picurrent.
        /// </summary>
        internal Ch.Elca.Iiop.Interception.PICurrentImpl PICurrent {
            get {
                return m_piCurrentManager.GetThreadScopedCurrent();
            }
        }
		
        /// <summary>
        /// returns the manager responsible for PICurrents.
        /// </summary>
        internal Ch.Elca.Iiop.Interception.PICurrentManager PICurrentManager {
            get {
                return m_piCurrentManager;
            }
        }
		
        #endregion IProperties
        #region IMethods
        
        
        private void CheckIsValidUri(string uri) {
            if (!IiopUrlUtil.IsUrl(uri)) {
                throw new BAD_PARAM(264, CompletionStatus.Completed_Yes);
            }
        }
        
        private void CheckIsProxy(MarshalByRefObject mbrProxy) {
            if ((mbrProxy == null) || (!RemotingServices.IsTransparentProxy(mbrProxy))) {
                // argument is not a proxy
                throw new BAD_PARAM(265, CompletionStatus.Completed_Yes);
            }
        }

        
        /// <summary>takes an IOR or a corbaloc and returns a proxy</summary>
        public object string_to_object([StringValue] string uri) {
            CheckIsValidUri(uri);
            
            Ior ior = IiopUrlUtil.CreateIorForUrl(uri, "");
            // performance opt: if an ior passed in, use it
            string iorString = uri;         
            if (!IiopUrlUtil.IsIorString(uri)) {
                iorString = ior.ToString();
            }
                
            return RemotingServices.Connect(ior.Type, ior.ToString());
        }
        
        /// <summary>takes a proxy and returns the IOR / corbaloc / ...</summary>
        public string object_to_string(object obj) {
            MarshalByRefObject mbr = obj as MarshalByRefObject;
            if (mbr == null) {
                throw new BAD_PARAM(265, CompletionStatus.Completed_Yes);
            }
            if (RemotingServices.IsTransparentProxy(mbr)) {
            
                string uri = RemotingServices.GetObjectUri(mbr);
                CheckIsValidUri(uri);
                if (IiopUrlUtil.IsIorString(uri)) {
                    return uri;
                } else {
                    // create an IOR assuming type is CORBA::Object
                    return IiopUrlUtil.CreateIorForUrl(uri, "").ToString();
                }
            } else {
                // local object
                return IiopUrlUtil.CreateIorForObjectFromThisDomain(mbr).ToString();
            }
        }

        /// <summary>
        /// <see cref="omg.org.CORBA.ORB.resolve_initial_references"/>
        /// </summary>
        public object resolve_initial_references ([StringValue()][WideChar(false)] string identifier) {
            if (identifier == "CodecFactory") {
                return CodecFactory;
            } else if (identifier == "PICurrent") {
                return PICurrent;
            } else {
                throw new omg.org.CORBA.ORB_package.InvalidName();
            }
        }
        
        #region Typecode creation operations

        public TypeCode create_interface_tc ([StringValue] [WideChar(false)] string id,
                                             [StringValue] [WideChar(false)] string name) {
            return new ObjRefTC(id, name);
        }
        
        public TypeCode create_ulong_tc() {
            return new ULongTC();
        }
        
        public TypeCode create_ushort_tc() {
            return new UShortTC();
        }
        
        public TypeCode create_ulonglong_tc() {
            return new ULongLongTC();
        }
        
        public TypeCode create_string_tc(int bound) {
            return new StringTC(bound);
        }
        
        public TypeCode create_wstring_tc(int bound) {
            return new WStringTC(bound);
        }

        public TypeCode create_array_tc (int length,
                                         TypeCode element_type) {
            return new ArrayTC(element_type, length);
        }

        public TypeCode create_alias_tc ([StringValue] [WideChar(false)] string id, [StringValue] [WideChar(false)] string name, 
                                         TypeCode original_type) {
            return new AliasTC(id, name, original_type);
        }

        
        /// <summary>takes an object an returns the typecode for it</summary>
        public TypeCode create_tc_for(object forObject) {
            if (!(forObject == null)) {
                return Repository.CreateTypeCodeForType(forObject.GetType(), AttributeExtCollection.EmptyCollection);
            } else {
                return new NullTC();
            }
        }

        public TypeCode create_tc_for_type(Type forType) {
            return Repository.CreateTypeCodeForType(forType, AttributeExtCollection.EmptyCollection);
        }

        public Type get_type_for_tc(TypeCode tc) {
            if (!(tc is NullTC)) {
                return Repository.GetTypeForTypeCode(tc);
            } else {
                return null;
            }
        }
                
        #endregion TypeCode creation operations     
        
        #region Pseudo object operation helpers
                
        public bool is_a(object proxy, Type type) {
            if (type == null) {
                throw new ArgumentException("type must be != null");
            }
            string repId = Repository.GetRepositoryID(type);
            return is_a(proxy, repId);
            
        }
        
        public bool is_a(object proxy, string repId) {
            if (proxy == null) {
                throw new ArgumentException("proxy must be != null");
            } 
            CheckIsProxy(proxy as MarshalByRefObject);
            if (repId == null) {
                throw new ArgumentException("repId must be != null");
            }           
            
            if (repId.Equals("IDL:omg.org/CORBA/Object:1.0") ||
                repId.Equals(String.Empty)) {
                // always true
                return true;
            }
            
            // perform remote call to check for is_a
            return ((IObject)proxy)._is_a(repId);           
        }
        
        public bool non_existent(object proxy) {
            CheckIsProxy(proxy as MarshalByRefObject);
            
            return ((IObject)proxy)._non_existent();
        }
        
        #endregion Pseude object operation helpers
        #region Portable Interceptors
	    
        /// <summary>see <see cref="omg.org.CORBA.IOrbServices.RegisterPortableInterceptorInitalizer"</summary>
        public void RegisterPortableInterceptorInitalizer(ORBInitalizer initalizer) {
            lock(m_orbInitalizers.SyncRoot) {
                m_orbInitalizers.Add(initalizer);
            }
        }
	    
        /// <summary>see <see cref="omg.org.CORBA.IOrbServices.CompleteInterceptorRegistration"</summary>
        public void CompleteInterceptorRegistration() {
            lock(m_orbInitalizers.SyncRoot) {
                try {
                    m_interceptorManager.CompleteInterceptorRegistration(m_orbInitalizers);
                } finally {
                    // not needed any more
                    m_orbInitalizers.Clear();
                }
            }	        
        }
	    
        #endregion Protable Interceptors
        
        #endregion IMethods
    
    }        						

}


namespace omg.org.CORBA.ORB_package {
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/ORB/InvalidName:1.0")]
    [Serializable]
    public class InvalidName : AbstractUserException {
        
        #region IConstructors

        /// <summary>constructor needed for deserialisation</summary>
        public InvalidName() { }

        #endregion IConstructors

    }    
    
}


