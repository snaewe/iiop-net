/* InstanceMappers.cs
 * 
 * Project: IIOP.NET
 * Mapping-Plugin
 * 
 * WHEN      RESPONSIBLE
 * 17.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.JavaCollectionMappers {


    /// <summary>base class supporting the mapping of collection instances</summary>
    public class CollectionMapperBase {    	
    	
        private static Type s_int16Type = typeof(System.Int16);
        private static Type s_int32Type = typeof(System.Int32);
        private static Type s_int64Type = typeof(System.Int64);
        private static Type s_byteType = typeof(System.Byte);
        private static Type s_booleanType = typeof(System.Boolean);
        private static Type s_singleType = typeof(System.Single);
        private static Type s_doubleType = typeof(System.Double);
        private static Type s_charType = typeof(System.Char);
        
        private static Type s_javaBoxInteger = typeof(java.lang.Integer);
        private static Type s_javaBoxShort = typeof(java.lang._Short);
        private static Type s_javaBoxByte = typeof(java.lang._Byte);
        private static Type s_javaBoxDouble = typeof(java.lang._Double);
        private static Type s_javaBoxFloat = typeof(java.lang._Float);
        private static Type s_javaBoxCharacter = typeof(java.lang.Character);
        private static Type s_javaBoxBoolean = typeof(java.lang._Boolean);

        /// <summary>checks, if clsPrimitive is one of the cls primitive types, which must be boxed to java boxes
        protected bool IsClsPrimitive(object clsPrimitive) {
    	    if ((clsPrimitive != null) && (ClsToIdlMapper.IsMappablePrimitiveType(clsPrimitive.GetType()))) {
    	    	return true;
    	    } else {
    	        return false;
    	    }
        }

    	/// <summary>boxes a cls base type into a java primitive box</summary>
    	/// <remarks>clsPrimitive must be one of the mappable cls primitive types, else throws exception</remarks>
    	protected object BoxBaseType(object clsPrimitive) {
    		if (s_int16Type.IsInstanceOfType(clsPrimitive)) {
                    return new java.lang._ShortImpl((System.Int16)clsPrimitive);
    		} else if (s_int32Type.IsInstanceOfType(clsPrimitive)) {
    		    return new java.lang.IntegerImpl((System.Int32)clsPrimitive);
    		} else if (s_byteType.IsInstanceOfType(clsPrimitive)) {
                    return new java.lang._ByteImpl((System.Byte)clsPrimitive);
       		} else if (s_singleType.IsInstanceOfType(clsPrimitive)) {
   		    return new java.lang._FloatImpl((System.Single)clsPrimitive);
    		} else if (s_doubleType.IsInstanceOfType(clsPrimitive)) {
   		    return new java.lang._DoubleImpl((System.Double)clsPrimitive);
    		} else if (s_booleanType.IsInstanceOfType(clsPrimitive)) {
   		    return new java.lang._BooleanImpl((System.Boolean)clsPrimitive);
    		} else if (s_charType.IsInstanceOfType(clsPrimitive)) {
   		    return new java.lang.CharacterImpl((System.Char)clsPrimitive);
    		} else {
    		    throw new BAD_PARAM(7456, CompletionStatus.Completed_MayBe);
    		}
    	}
    	

        /// <summary>checks, if javaInstance is one of the java boxes
    	protected bool IsJavaBaseTypeBox(object javaInstance) {
    	    if (s_javaBoxInteger.IsInstanceOfType(javaInstance) || 
    	        s_javaBoxShort.IsInstanceOfType(javaInstance) ||
    	        s_javaBoxByte.IsInstanceOfType(javaInstance) ||
    	        s_javaBoxDouble.IsInstanceOfType(javaInstance) ||
       	        s_javaBoxFloat.IsInstanceOfType(javaInstance) ||
       	        s_javaBoxBoolean.IsInstanceOfType(javaInstance) ||
       	        s_javaBoxCharacter.IsInstanceOfType(javaInstance)
    	    ) {
    	    	return true;
    	    } else {
    	        return false;
    	    }
    	}
    	
        /// <summary>unboxes a java primitive box to a cls base type</summary>
        /// <remarks>javaInstance must be one of java primitive boxes, else throws exception</remarks>        	
    	protected object UnboxBaseType(object javaInstance) {
    	    if (s_javaBoxInteger.IsInstanceOfType(javaInstance)) {
    	    	return ((java.lang.Integer)javaInstance).intValue();
    	    } else if (s_javaBoxShort.IsInstanceOfType(javaInstance)) {
    	    	return ((java.lang._Short)javaInstance).shortValue();
    	    } else if (s_javaBoxByte.IsInstanceOfType(javaInstance)) {
    	    	return ((java.lang._Byte)javaInstance).byteValue();
    	    } else if (s_javaBoxDouble.IsInstanceOfType(javaInstance)) {
    	    	return ((java.lang._Double)javaInstance).doubleValue();
    	    } else if (s_javaBoxFloat.IsInstanceOfType(javaInstance)) {
    	        return ((java.lang._Float)javaInstance).floatValue();
      	    } else if (s_javaBoxBoolean.IsInstanceOfType(javaInstance)) {
    	        return ((java.lang._Boolean)javaInstance).booleanValue();
      	    } else if (s_javaBoxCharacter.IsInstanceOfType(javaInstance)) {
    	        return ((java.lang.Character)javaInstance).charValue();
    	    } else {
                throw new BAD_PARAM(7456, CompletionStatus.Completed_MayBe);
    	    }
    	}
        
    }


    /// <summary>
    /// maps instances of java.util.ArrayListImpl to instances 
    ///	of System.Collections.ArrayList and vice versa.
    /// </summary>
    public class ArrayListMapper : CollectionMapperBase, ICustomMapper {
     

        public object CreateClsForIdlInstance(object idlInstance) {
            java.util.ArrayListImpl source = (java.util.ArrayListImpl)idlInstance;
            System.Collections.ArrayList result = new System.Collections.ArrayList();
            result.Capacity = source.Capacity;
            object[] elements = source.GetElements();
            // check for boxed java base types
            for (int i = 0; i < elements.Length; i++) {
                if (IsJavaBaseTypeBox(elements[i])) {
                    elements[i] = UnboxBaseType(elements[i]);
                }
            }
            result.AddRange(elements);
            return result;
        }

        public object CreateIdlForClsInstance(object clsInstance) {
            java.util.ArrayListImpl result = new java.util.ArrayListImpl();
            System.Collections.ArrayList source = (System.Collections.ArrayList)clsInstance;
            result.Capacity = source.Capacity;
            object[] elements = source.ToArray();
            for (int i = 0; i < elements.Length; i++) {
                if (IsClsPrimitive(elements[i])) {
                    elements[i] = BoxBaseType(elements[i]);
                }
            }
            result.SetElements(elements);
               
            return result;
        }

    }

}
