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


namespace omg.org.CORBA {
	
			
	/// <remarks>contains only the CORBA Orb operations supported by IIOP.NET</remarks>
	public interface ORB {
		
		/// <summary>takes an IOR or a corbaloc and returns a proxy</summary>
		object string_to_object([StringValue] string obj);
		
		/// <summary>takes a proxy and returns the IOR / corbaloc / ...</summary>
		string object_to_string(object obj);
		
		#region Typecode creation operations
		
		TypeCode create_string_tc(int bound);
		
		TypeCode create_wstring_tc(int bound);
				
		#endregion TypeCode creation operations
		
	}
	
	public interface IOrbServices : ORB {
		
		/// <summary>takes an object an returns the typecode for it</summary>
		TypeCode create_tc_for(object forObject);
		
		#region Pseudo object operation helpers
				
		/// <summary>checks, if object supports the specified interface</summary>
		bool is_a(object proxy, Type type);
        
        /// <summary>checks, if object supports the specified interface</summary>
        bool is_a(object proxy, string repId);
		
		/// <summary>checks, if the object is existing</summary>
		bool non_existent(object proxy);
		
		#endregion Pseude object operation helpers

	}
	
	
	/// <summary>implementation of the Orb interface methods supported by IIOP.NET</summary>
	public sealed class OrbServices : IOrbServices {
	
		#region SFields
		
		private static OrbServices s_singleton = new OrbServices();		
		
		#endregion SFields
		#region IConstructors
		
		private OrbServices() {			
		}
		
		#endregion IConstructors
		#region SMethods
		
		public static OrbServices GetSingleton() {
			return s_singleton;
		}
		
		#endregion SMethods
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
		
		#region Typecode creation operations
		
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
		
		/// <summary>takes an object an returns the typecode for it</summary>
		public TypeCode create_tc_for(object forObject) {
			if (!(forObject == null)) {
				return Repository.CreateTypeCodeForType(forObject.GetType(), AttributeExtCollection.EmptyCollection);
			} else {
				return new NullTC();
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
		
		#endregion IMethods
	
	}
	
	
}


