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
using System.Globalization;
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
        private static Type s_stringType = typeof(System.String);
        
        private static Type s_javaBoxLong = typeof(java.lang._Long);
        private static Type s_javaBoxInteger = typeof(java.lang._Integer);
        private static Type s_javaBoxShort = typeof(java.lang._Short);
        private static Type s_javaBoxByte = typeof(java.lang._Byte);
        private static Type s_javaBoxDouble = typeof(java.lang._Double);
        private static Type s_javaBoxFloat = typeof(java.lang._Float);
        private static Type s_javaBoxCharacter = typeof(java.lang.Character);
        private static Type s_javaBoxBoolean = typeof(java.lang._Boolean);

        
        /// <summary>boxes an instance of a cls base type into a java primitive box; for non-primitives: 
        /// returns instance</summary>      
        protected object BoxClsInstanceIfNeeded(object clsInstance) {
            if (s_int16Type.IsInstanceOfType(clsInstance)) {
                return new java.lang._ShortImpl((System.Int16)clsInstance);
            } else if (s_int32Type.IsInstanceOfType(clsInstance)) {
                return new java.lang._IntegerImpl((System.Int32)clsInstance);
            } else if (s_int64Type.IsInstanceOfType(clsInstance)) {
                return new java.lang._LongImpl((System.Int64)clsInstance);
            } else if (s_byteType.IsInstanceOfType(clsInstance)) {
                return new java.lang._ByteImpl((System.Byte)clsInstance);
            } else if (s_singleType.IsInstanceOfType(clsInstance)) {
                return new java.lang._FloatImpl((System.Single)clsInstance);
            } else if (s_doubleType.IsInstanceOfType(clsInstance)) {
                return new java.lang._DoubleImpl((System.Double)clsInstance);
            } else if (s_booleanType.IsInstanceOfType(clsInstance)) {
                return new java.lang._BooleanImpl((System.Boolean)clsInstance);
            } else if (s_charType.IsInstanceOfType(clsInstance)) {
                return new java.lang.CharacterImpl((System.Char)clsInstance);
            } else {
                return clsInstance;
            }
        }       
                
        /// <summary>unboxes a java primitive box to a cls base type; otherwise return the instance unmodified</summary>
        /// <returns>for a boxed java primitive, the unboxed version; otherwise instance</returns>        
        protected object UnboxJavaInstanceIfNeeded(object javaInstance) {
            if (s_javaBoxInteger.IsInstanceOfType(javaInstance)) {
                return ((java.lang._IntegerImpl)javaInstance).intValue();
            } else if (s_javaBoxLong.IsInstanceOfType(javaInstance)) {
                return ((java.lang._LongImpl)javaInstance).longValue();
            } else if (s_javaBoxShort.IsInstanceOfType(javaInstance)) {
                return ((java.lang._ShortImpl)javaInstance).shortValue();
            } else if (s_javaBoxByte.IsInstanceOfType(javaInstance)) {
                return ((java.lang._ByteImpl)javaInstance).byteValue();
            } else if (s_javaBoxDouble.IsInstanceOfType(javaInstance)) {
                return ((java.lang._DoubleImpl)javaInstance).doubleValue();
            } else if (s_javaBoxFloat.IsInstanceOfType(javaInstance)) {
                return ((java.lang._FloatImpl)javaInstance).floatValue();
            } else if (s_javaBoxBoolean.IsInstanceOfType(javaInstance)) {
                return ((java.lang._BooleanImpl)javaInstance).booleanValue();
            } else if (s_javaBoxCharacter.IsInstanceOfType(javaInstance)) {
                return ((java.lang.CharacterImpl)javaInstance).charValue();
            } else {
                return javaInstance;
            }
        }
        
    }


    /// <summary>
    /// maps instances of java.util.ArrayListImpl to instances 
    /// of System.Collections.ArrayList and vice versa.
    /// </summary>
    public class ArrayListMapper : CollectionMapperBase, ICustomMapper {
     

        public object CreateClsForIdlInstance(object idlInstance) {
            java.util.ArrayListImpl source = (java.util.ArrayListImpl)idlInstance;
            System.Collections.ArrayList result = new System.Collections.ArrayList();
            result.Capacity = source.Capacity;
            object[] elements = source.GetElements();
            // check for boxed java base types
            for (int i = 0; i < elements.Length; i++) {
                elements[i] = UnboxJavaInstanceIfNeeded(elements[i]);
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
                elements[i] = BoxClsInstanceIfNeeded(elements[i]);
            }
            result.SetElements(elements);
               
            return result;
        }

    }


    /// <summary>
    /// maps instances of java.util.HashMapImpl to instances 
    /// of System.Collections.Hashtable and vice versa.
    /// </summary>
    public class HashMapMapper : CollectionMapperBase, ICustomMapper {
     

        public object CreateClsForIdlInstance(object idlInstance) {
            java.util.HashMapImpl source = (java.util.HashMapImpl)idlInstance;
            System.Collections.Hashtable result = new System.Collections.Hashtable();
            
            System.Collections.DictionaryEntry[] buckets = source.GetBuckets();
            for (int i = 0; i < buckets.Length; i++) {                
                object key = UnboxJavaInstanceIfNeeded(buckets[i].Key);
                object val = UnboxJavaInstanceIfNeeded(buckets[i].Value);
                result.Add(key, val);
            }
            
            return result;
        }

        public object CreateIdlForClsInstance(object clsInstance) {
            java.util.HashMapImpl result = new java.util.HashMapImpl();
            System.Collections.Hashtable source = (System.Collections.Hashtable)clsInstance;

            result.Capacity = (int)((source.Count + 1) / result.LoadFactor) + 1;
            System.Collections.DictionaryEntry[] buckets = new System.Collections.DictionaryEntry[source.Count];
            int i = 0;
            foreach (System.Collections.DictionaryEntry entry in source) {
                object key = BoxClsInstanceIfNeeded(entry.Key);
                object val = BoxClsInstanceIfNeeded(entry.Value);
                buckets[i] = new System.Collections.DictionaryEntry(key, val);
                i++;
            }
            result.SetBuckets(buckets);
               
            return result;
        }

    }
    
    public class DateMapper : ICustomMapper {
        
        private static DateTime s_javaOffsetBase;

        static DateMapper() {
            CultureInfo gmtCulture = new CultureInfo("en-GB");
            s_javaOffsetBase  = new DateTime(1970, 1, 1, 0, 0, 0, 
                                             gmtCulture.Calendar);            
        }

    
        public object CreateClsForIdlInstance(object idlInstance) {
            java.util._DateImpl source = (java.util._DateImpl)idlInstance;
            long offsetTicks = source.Offset * TimeSpan.TicksPerMillisecond;      
            System.DateTime result;
            if (offsetTicks >= 0) {      
                 result = s_javaOffsetBase + new TimeSpan(offsetTicks);
            } else {
                 result = s_javaOffsetBase - new TimeSpan(offsetTicks * -1);
            }
            return result.ToLocalTime();
        }
        
        public object CreateIdlForClsInstance(object clsInstance) {
            java.util._DateImpl result = new java.util._DateImpl();
            System.DateTime source = (System.DateTime)clsInstance;
            source = source.ToUniversalTime(); // convert to GMT for offset creation
             
            long tickOffset;           
            if (source >= s_javaOffsetBase) {
                tickOffset = (source - s_javaOffsetBase).Ticks;
            } else {
                tickOffset = -1 * ((s_javaOffsetBase - source).Ticks);
            }
            result.Offset = tickOffset / TimeSpan.TicksPerMillisecond;
            return result;
        }
        
        
    }


}
