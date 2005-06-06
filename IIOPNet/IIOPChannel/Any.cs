/* Any.cs
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
	
		
	/// <summary>used, if more control over any serialisation is needed than 
	/// by using the automatic IIOP.NET mechanism.</summary>
	/// <remarks>pass an instance of this container object instead of the object itself,
	/// if you need to control the typecode sent by IIOP.NET for the object instance.
	/// example: 
	/// A remote object provides in it's interface the following method:
	/// <code>
	///     void Test(object arg);
	/// </code>
	/// When passing an instance of System.String to this method, IIOP.NET automatically 
	/// passing the string as boxed value with type code WStringValueTC.
	/// If the string should be passed instead as wstring, do the following:
	/// <code>
	///     OrbServices orb = OrbServices.GetSingleton();    
	///     TypeCode wstringTC = orb.create_wstring_tc(0);
	///     Any any = new Any("myString", wstringTC);
	///     myObject.Test(any);
	/// </code>
	/// 
	/// </remarks>
	[Serializable]
	public sealed class Any : IIdlEntity {
		
		
		#region IFields
		
		private TypeCode m_typeCode;
		private object m_value;
		
		
		#endregion IFields
		#region IConstructors
		
		public Any(object obj, TypeCode type) {
			if (type == null) {
				throw new BAD_PARAM(456, CompletionStatus.Completed_MayBe);
			}			
			if (obj != null) {
		        // precodition: type is an instance of TypeCodeImpl
			    Type requiredObjectType = Repository.GetTypeForTypeCode(type);
			    if (!requiredObjectType.IsAssignableFrom(obj.GetType())) {
			        throw new BAD_PARAM(456, CompletionStatus.Completed_MayBe);	
			    }
			}
			m_value = obj;
			m_typeCode = type;
		}
		
		public Any(object obj) {
			m_value = obj;
			m_typeCode = OrbServices.GetSingleton().create_tc_for(obj);
			
		}
		
		#endregion IConstructors
		#region IProperties
		
		public object Value {
			get {
				return m_value;
			}
		}
		
		public TypeCode Type {
			get {
				return m_typeCode;
			}
		}
		
		#endregion IProperties
		
	}
	
}
