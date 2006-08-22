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
using System.Collections;
using System.Diagnostics;
using System.Reflection;
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
		
		private TypeCodeImpl m_typeCode;
		private object m_value;
		
		
		#endregion IFields
		#region IConstructors
		
		public Any(object obj, TypeCode type) {
			if (type == null) {
				throw new BAD_PARAM(456, CompletionStatus.Completed_MayBe);
			}
		    SetTypeCode(type);
		    m_value = m_typeCode.ConvertToAssignable(obj);
		}
		
		public Any(object obj) {
		    SetTypeCode(		        
                OrbServices.GetSingleton().create_tc_for(obj));
		    // because serialization is done based on the typecode type, make sure
		    // that the value is assignable to the typecode.
		    // For cases, where the .NET type can't be mapped directly to idl (like SByte),
		    // this would otherwise lead to problems in Serializer.
		    m_value = m_typeCode.ConvertToAssignable(obj);
		}
				
		#endregion IConstructors
		#region IProperties
		
		/// <summary>
		/// The value assignable to the typecode -> i.e. the internal representation.
		/// </summary>		
		internal object ValueInternalRepresenation {
		    get {
		        return m_value;
		    }
		}
		
		/// <summary>
		/// the value best corresponding to the typecode specified.
		/// </summary>
		public object Value {
			get {
				return m_typeCode.ConvertToExternalRepresentation(m_value, false);
			}
		}
		
		/// <summary>
		/// the cls compliant value corresponding to the typecode specified.
		/// </summary>
		public object ClsValue {
		    get {
		        // return m_typeCode.ConvertToClsValue();
		        return m_typeCode.ConvertToExternalRepresentation(m_value, true);
		    }
		}
		
		/// <summary>
		/// the typecode of this any container
		/// </summary>
		public TypeCode Type {
			get {
				return m_typeCode;
			}
		}
		
		#endregion IProperties
		#region IMethods

		private void SetTypeCode(TypeCode typeCode) {
            if (!(typeCode is omg.org.CORBA.TypeCodeImpl)) { 
                throw new INTERNAL(567, CompletionStatus.Completed_MayBe); 
		    }
		    m_typeCode = (TypeCodeImpl)typeCode;
		}		
		
		public override bool Equals(object obj) {
		    Any other = obj as Any;
		    if (other == null) {
		        return false;
		    }
		    return (other.Type.equal(Type) && 
		            (other.m_value != null ? other.m_value.Equals(m_value) :
		                                   m_value == null));
		}
		
		public override int GetHashCode() {
		    return m_typeCode.GetHashCode() ^
		        (m_value != null ? m_value.GetHashCode() : 0);
		}
		
		#endregion IMethods
		
	}
	
}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System;
    using System.Reflection;
    using NUnit.Framework;
    using omg.org.CORBA;
    
    /// <summary>
    /// Unit-tests for testing Any container
    /// </summary>
    [TestFixture]
    public class AnyContainerTest {
    
        
        [Test]
        public void BoxOctet() {
            byte val = 11;
            omg.org.CORBA.TypeCode tc = new OctetTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", val, anyContainer.ClsValue);
        }
        
        [Test]
        public void BoxIncompatibleType() {
            try {
                byte val = 11;
                omg.org.CORBA.TypeCode tc = new LongTC();
                Any anyContainer = new Any(val, tc);
                Assertion.Fail("expected exception");
            } catch (BAD_PARAM bp) {
                Assertion.AssertEquals(456, bp.Minor);
            }
        }
        
        [Test]
        public void BoxULong() {
            uint val = 11;
            omg.org.CORBA.TypeCode tc = new ULongTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", (int)val, anyContainer.ClsValue);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.Int32Type, 
                                   anyContainer.ClsValue.GetType());            
        }
        
        [Test]
        public void BoxULongFromCls() {
            int val = 11;
            omg.org.CORBA.TypeCode tc = new ULongTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", (uint)val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", ((uint)val).GetType(), anyContainer.Value.GetType());
            Assertion.AssertEquals("wrong val", val, anyContainer.ClsValue);
        }           

        [Test]
        public void BoxULongFromClsOutsideRange() {
            int val = -11;
            omg.org.CORBA.TypeCode tc = new ULongTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            // do an unchecked cast, overflow no issue here
            Assertion.AssertEquals("wrong val", unchecked((uint)val), anyContainer.Value);
            Assertion.AssertEquals("wrong val", unchecked((uint)val).GetType(), anyContainer.Value.GetType());
            Assertion.AssertEquals("wrong val", val, anyContainer.ClsValue);
        }       
        
        [Test]
        public void BoxLong() {
            int val = 11;
            omg.org.CORBA.TypeCode tc = new LongTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", val, anyContainer.ClsValue);
        }
        
        [Test]
        public void BoxULongLong() {
            ulong val = 11;
            omg.org.CORBA.TypeCode tc = new ULongLongTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", (long)val, anyContainer.ClsValue);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.Int64Type, 
                                   anyContainer.ClsValue.GetType());
        }
        
        [Test]
        public void BoxULongLongFromCls() {
            long val = 11;
            omg.org.CORBA.TypeCode tc = new ULongLongTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", (ulong)val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", ((ulong)val).GetType(), anyContainer.Value.GetType());
            Assertion.AssertEquals("wrong val", val, anyContainer.ClsValue);
        }        
        
        [Test]
        public void BoxULongLongFromClsOutsideRange() {
            long val = -11;
            omg.org.CORBA.TypeCode tc = new ULongLongTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            // do an unchecked cast, overflow no issue here
            Assertion.AssertEquals("wrong val", unchecked((ulong)val), anyContainer.Value);
            Assertion.AssertEquals("wrong val", unchecked((ulong)val).GetType(), anyContainer.Value.GetType());
            Assertion.AssertEquals("wrong val", val, anyContainer.ClsValue);
        }        
        
        [Test]
        public void BoxUShort() {
            ushort val = 11;
            omg.org.CORBA.TypeCode tc = new UShortTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", (short)val, anyContainer.ClsValue);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.Int16Type, 
                                   anyContainer.ClsValue.GetType());
        }
        
        [Test]
        public void BoxUShortFromCls() {
            short val = 11;
            omg.org.CORBA.TypeCode tc = new UShortTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", (ushort)val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", ((ushort)val).GetType(), anyContainer.Value.GetType());
            Assertion.AssertEquals("wrong val", val, anyContainer.ClsValue);
        }        
        
        [Test]
        public void BoxUShortFromClsOutsideRange() {
            short val = -11;
            omg.org.CORBA.TypeCode tc = new UShortTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            // do an unchecked cast, overflow no issue here
            Assertion.AssertEquals("wrong val", unchecked((ushort)val), anyContainer.Value);
            Assertion.AssertEquals("wrong val", unchecked((ushort)val).GetType(), anyContainer.Value.GetType());
            Assertion.AssertEquals("wrong val", val, anyContainer.ClsValue);
        }                
        
        [Test]
        public void BoxSByteToOctet() {
            sbyte val = 11;
            omg.org.CORBA.TypeCode tc = new OctetTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", (byte)val, anyContainer.Value);
            Assertion.AssertEquals("wrong val", (byte)val, anyContainer.ClsValue);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.ByteType, 
                                   anyContainer.ClsValue.GetType());
        }        
        
        [Test]
        public void BoxSByteToOctetOutsideRange() {
            sbyte val = -11;
            omg.org.CORBA.TypeCode tc = new OctetTC();
            Any anyContainer = new Any(val, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            // do an unchecked cast, overflow no issue here
            Assertion.AssertEquals("wrong val", unchecked((byte)val), anyContainer.Value);
            Assertion.AssertEquals("wrong val", unchecked((byte)val), anyContainer.ClsValue);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.ByteType, 
                                   anyContainer.ClsValue.GetType());
        }        
        
        [Test]
        public void BoxBoxedValueTypeFromUnboxed() {
            omg.org.CORBA.TypeCode tc = 
                new ValueBoxTC("IDL:omg.org/CORBA/StringValue:1.0", "StringValue",
                               new StringTC());
            string toBoxInto = "test";
            Any anyContainer = new Any(toBoxInto, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.StringValueType,
                                   anyContainer.ValueInternalRepresenation.GetType());
            
            Assertion.AssertEquals("wrong val", toBoxInto,
                                   anyContainer.Value);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.StringType,
                                   anyContainer.Value.GetType());            
            
        }
        
        [Test]
        public void BoxBoxedValueTypeFromBoxed() {
            omg.org.CORBA.TypeCode tc = 
                new ValueBoxTC("IDL:omg.org/CORBA/StringValue:1.0", "StringValue",
                               new StringTC());
            StringValue toBoxInto = new StringValue("test");
            Any anyContainer = new Any(toBoxInto, tc);
            Assertion.AssertEquals("wrong tc", tc, anyContainer.Type);
            Assertion.AssertEquals("wrong val", toBoxInto,
                                   anyContainer.ValueInternalRepresenation);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.StringValueType,
                                   anyContainer.ValueInternalRepresenation.GetType());
            
            Assertion.AssertEquals("wrong val", toBoxInto.Unbox(),
                                   anyContainer.Value);
            Assertion.AssertEquals("wrong val type", ReflectionHelper.StringType,
                                   anyContainer.Value.GetType());            
        }        
        
        [Test]
        public void TestNonAssignableException() {
        	try {
        		Any any = new Any("1.0", new OctetTC());
        		Assertion.Fail("assignement possible, but shouldn't");
        	} catch (BAD_PARAM bpEx) {
        		Assertion.Assert("exception message", 
        		                 bpEx.Message.StartsWith("CORBA system exception : omg.org.CORBA.BAD_PARAM [The given instance 1.0 of type"));
        	}
        }
        
    }
    
}
    
#endif

