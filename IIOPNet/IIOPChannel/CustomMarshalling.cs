/* CustomMarshalling.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 05.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Marshalling {

    /// <summary>This interface must be implemented by custom marshalled value types</summary>
    public interface ICustomMarshalled {
        
        #region IMethods
        
        /// <summary>serializes the state of this custom marshalled value type</summary>
        void Serialize(Corba.DataOutputStream stream);
        /// <summary>deserialises the state of this custom marshalled value type</summary>
        void Deserialise(Corba.DataInputStream stream);

        #endregion IMethods

    }

}


namespace Corba {

    /// <summary>
    /// interface used for custom marshalling
    /// </summary>
    // Coding convention (identifier names) violated, because mapped from IDL (by hand)
    public interface DataOutputStream : IIdlEntity {
        
        #region IMethods
        
        void write_any([ObjectIdlType(IdlTypeObject.Any)] object val);
        void write_boolean(bool val);
        void write_char([WideCharAttribute(false)]char val);
        void write_wchar([WideCharAttribute(true)]char val);
        void write_octet(byte val);
        void write_short(short val);
        void write_ushort(short val);
        void write_long(int val);
        void write_ulong(int val);
        void write_longlong(long val);
        void write_ulonglong(long val);
        void write_float(float val);
        void write_double(double val);
        void write_string([WideCharAttribute(false)]string val);
        void write_wstring([WideCharAttribute(true)]string val);
        void write_Object(MarshalByRefObject val);
        void write_Abstract([ObjectIdlType(IdlTypeObject.AbstractBase)]object val);
        /// <param name="val">no boxed value types, no primitive types and no arrays are supported here</param>
        void write_Value([ObjectIdlType(IdlTypeObject.ValueBase)] object val);
        /// <summary>writes boxed values</summary>
        void write_boxed(object val, BoxedValueAttribute attr);
        void write_TypeCode(omg.org.CORBA.TypeCode val);
        void write_any_array([IdlSequenceAttribute][ObjectIdlType(IdlTypeObject.Any)] object[] seq,
                             int offset, int length);
        void write_boolean_array([IdlSequenceAttribute] bool[] seq, int offset, int length);
        void write_char_array([IdlSequenceAttribute] [WideCharAttribute(false)] char[] seq,
                              int offset, int length);
        void write_wchar_array([IdlSequenceAttribute] [WideCharAttribute(true)] char[]seq,
                               int offset, int length);
        void write_octet_array([IdlSequenceAttribute] byte[] seq, int offset, int length);
        void write_short_array([IdlSequenceAttribute] short[] seq, int offset, int length);
        void write_ushort_array([IdlSequenceAttribute] short[] seq, int offset, int length);
        void write_long_array([IdlSequenceAttribute] int[] seq, int offset, int length);
        void write_ulong_array([IdlSequenceAttribute] int[] seq, int offset, int length);
        void write_ulonglong_array([IdlSequenceAttribute] long[] seq, int offset, int length);
        void write_longlong_array([IdlSequenceAttribute] long[] seq, int offset, int length);
        void write_float_array([IdlSequenceAttribute] float[] seq, int offset, int length);
        void write_double_array([IdlSequenceAttribute] double[] seq, int offset, int length);

        #endregion IMethods

    }

    /// <summary>
    /// interface used for custom marshalling
    /// </summary>
    // Coding convention (identifier names) violated, because mapped from IDL (by hand)
    public interface DataInputStream : IIdlEntity {
        
        #region IMethods
        
        [return:ObjectIdlType(IdlTypeObject.Any)]
        object read_any();
        bool read_boolean();
        [return:WideCharAttribute(false)]
        char read_char();
        [return:WideCharAttribute(true)]
        char read_wchar();
        byte read_octet();
        short read_short();
        short read_ushort();
        int read_long();
        int read_ulong();
        long read_longlong();
        long read_ulonglong();
        float read_float();
        double read_double();
        [return:WideCharAttribute(false)]
        string read_string();
        [return:WideCharAttribute(true)]
        string read_wstring();
        MarshalByRefObject read_Object();
        [return:ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase)]
        object read_Abstract();
        [return:ObjectIdlTypeAttribute(IdlTypeObject.ValueBase)]
        object read_Value();
        /// <summary>
        /// reads a value type, which is of the given formal type
        /// </summary>
        /// <param name="formal"></param>
        /// <returns></returns>
        object read_ValueOfType(Type formal);
        /// <summary>
        /// reads a corba wstring value
        /// </summary>
        /// <returns>the unboxed string</returns>
        object read_WStringValue();
        /// <summary>
        /// reads a corba string value
        /// </summary>
        /// <returns>the unboxed string</returns>
        object read_StringValue();
        /// <summary>boxed values are not handable with read-value</summary>
        /// <param name="boxedType">the boxed type, which is not itself a boxed type</param>
        object read_boxed(BoxedValueAttribute attr, Type boxedType, AttributeExtCollection boxedTypeAttrs);
        omg.org.CORBA.TypeCode read_TypeCode();

        void read_any_array([IdlSequenceAttribute][ObjectIdlTypeAttribute(IdlTypeObject.Any)] ref object[] seq,
                            int offset, int length);
        void read_boolean_array([IdlSequenceAttribute] ref bool[] seq, int offset, int length);
        void read_char_array([IdlSequenceAttribute] [WideCharAttribute(false)] ref char[] seq,
                             int offset, int length);
        void read_wchar_array([IdlSequenceAttribute] [WideCharAttribute(true)] ref char[]seq,
                              int offset, int length);
        void read_octet_array([IdlSequenceAttribute] ref byte[] seq, int offset, int length);
        void read_short_array([IdlSequenceAttribute] ref short[] seq, int offset, int length);
        void read_ushort_array([IdlSequenceAttribute] ref short[] seq, int offset, int length);
        void read_long_array([IdlSequenceAttribute] ref int[] seq, int offset, int length);
        void read_ulong_array([IdlSequenceAttribute] ref int[] seq, int offset, int length);
        void read_ulonglong_array([IdlSequenceAttribute] ref long[] seq, int offset, int length);
        void read_longlong_array([IdlSequenceAttribute] ref long[] seq, int offset, int length);
        void read_float_array([IdlSequenceAttribute] ref float[] seq, int offset, int length);
        void read_double_array([IdlSequenceAttribute] ref double[] seq, int offset, int length);    

        #endregion IMethods

    }


    /// <summary>
    /// inplementation of the DataOutputStream interface
    /// </summary>
    internal class DataOutputStreamImpl : DataOutputStream {

        #region SFields

        private static Type s_objectType = typeof(object);
        private static Type s_mByRefType = typeof(MarshalByRefObject);

        #endregion SFields
        #region IFields

        private Marshaller m_marshaller = Marshaller.GetSingleton();

        private CdrOutputStream m_cdrOut;

        #endregion IFields
        #region IConstructors

        public DataOutputStreamImpl(CdrOutputStream cdrOut) {
            m_cdrOut = cdrOut;
        }

        #endregion IConstructors
        #region IMethods

        #region Implementation of DataOutputStream
        public void write_any([ObjectIdlType(IdlTypeObject.Any)] object val) {
            m_marshaller.Marshal(typeof(object), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new ObjectIdlTypeAttribute(IdlTypeObject.Any)}),
                                 val, m_cdrOut);
        }

        public void write_boolean(bool val)    {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_char([WideCharAttribute(false)]char val) {
            m_marshaller.Marshal(val.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new WideCharAttribute(false) }), 
                                 val, m_cdrOut);
        }

        public void write_wchar([WideCharAttribute(true)]char val) {
            m_marshaller.Marshal(val.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new WideCharAttribute(true) }), 
                                 val, m_cdrOut);
        }

        public void write_octet(byte val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_short(short val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_ushort(short val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_long(int val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_ulong(int val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_longlong(long val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_ulonglong(long val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_float(float val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_double(double val) {
            m_marshaller.Marshal(val.GetType(), new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_string([WideCharAttribute(false)]string val) {
            m_marshaller.Marshal(val.GetType(), 
                                 new AttributeExtCollection(new Attribute[] {
                                         new WideCharAttribute(false) }),
                                 val, m_cdrOut);            
        }
        
        public void write_wstring([WideCharAttribute(true)]string val) {
            m_marshaller.Marshal(val.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new WideCharAttribute(true) }), 
                                 val, m_cdrOut);            
        }

        public void write_Object(System.MarshalByRefObject val) {
            m_marshaller.Marshal(s_mByRefType, new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_Abstract([ObjectIdlType(IdlTypeObject.AbstractBase)]object val) {
            m_marshaller.Marshal(s_objectType, 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase) } ),
                                 val, m_cdrOut);
        }

        public void write_Value([ObjectIdlType(IdlTypeObject.ValueBase)]object val) {
            m_marshaller.Marshal(s_objectType, 
                                 new AttributeExtCollection(new Attribute[] { 
                                          new ObjectIdlTypeAttribute(IdlTypeObject.ValueBase) } ),
                                 val, m_cdrOut);
        }

        public void write_boxed(object val, BoxedValueAttribute attr) {
            m_marshaller.Marshal(val.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { attr } ),
                                 val, m_cdrOut);
        }


        public void write_TypeCode(omg.org.CORBA.TypeCode val) {
            m_marshaller.Marshal(typeof(omg.org.CORBA.TypeCode),
                                 new AttributeExtCollection(),
                                 val, m_cdrOut);
        }

        public void write_any_array([IdlSequenceAttribute][ObjectIdlType(IdlTypeObject.Any)] object[] seq,
                                    int offset, int length) {
            m_marshaller.Marshal(typeof(object[]), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute(), 
                                         new ObjectIdlTypeAttribute(IdlTypeObject.Any) } ),
                                 seq, m_cdrOut);
        }

        public void write_boolean_array([IdlSequenceAttribute] bool[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ),
                                 seq, m_cdrOut);
        }

        public void write_char_array([IdlSequenceAttribute] [WideCharAttribute(false)] char[] seq,
                                     int offset, int length)  {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute(), new WideCharAttribute(false) } ),
                                 seq, m_cdrOut);
        }

        public void write_wchar_array([IdlSequenceAttribute] [WideCharAttribute(true)] char[]seq,
                                      int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute(), new WideCharAttribute(true) } ),
                                 seq, m_cdrOut);
        }

        public void write_octet_array([IdlSequenceAttribute] byte[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ),
                                 seq, m_cdrOut);
        }

        public void write_short_array([IdlSequenceAttribute] short[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ),
                                 seq, m_cdrOut);
        }

        public void write_ushort_array([IdlSequenceAttribute] short[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ),
                                 seq, m_cdrOut);
        }

        public void write_long_array([IdlSequenceAttribute] int[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(),
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ),
                                 seq, m_cdrOut);
        }

        public void write_ulong_array([IdlSequenceAttribute] int[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ), 
                                 seq, m_cdrOut);
        }

        public void write_ulonglong_array([IdlSequenceAttribute] long[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ), 
                                 seq, m_cdrOut);
        }

        public void write_longlong_array([IdlSequenceAttribute] long[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ),
                                 seq, m_cdrOut);
        }
        
        public void write_float_array([IdlSequenceAttribute] float[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] { 
                                         new IdlSequenceAttribute() } ), 
                                 seq, m_cdrOut);
        }
        public void write_double_array([IdlSequenceAttribute] double[] seq, int offset, int length) {
            m_marshaller.Marshal(seq.GetType(), 
                                 new AttributeExtCollection(new Attribute[] {
                                         new IdlSequenceAttribute() } ), 
                                 seq, m_cdrOut);
        }

        #endregion

        #endregion IMethods
    
    }
    
    /// <summary>
    /// implementation of the DataInputStream Interface
    /// </summary>
    internal class DataInputStreamImpl : DataInputStream {
        
        #region SFields

        private static Type s_objectType = typeof(object);
        private static Type s_stringType = typeof(string);
        private static Type s_mByRefType = typeof(MarshalByRefObject);

        private static Type s_wstringValueType = typeof(WStringValue);
        private static Type s_stringValueType = typeof(StringValue);

        #endregion SFields
        #region IFields

        private Marshaller m_marshaller = Marshaller.GetSingleton();

        private CdrInputStream m_cdrIn;

        #endregion IFields
        #region IConstructors

        public DataInputStreamImpl(CdrInputStream cdrIn) {
            m_cdrIn = cdrIn;
        }

        #endregion IConstructors
        #region IMethods

        #region Implementation of DataInputStream
        
        [return:ObjectIdlTypeAttribute(IdlTypeObject.Any)]
        public object read_any() {
            return m_marshaller.Unmarshal(s_objectType, 
                                          new AttributeExtCollection(new Attribute[] { 
                                                  new ObjectIdlTypeAttribute(IdlTypeObject.Any) } ),
                                          m_cdrIn);
        }

        public bool read_boolean() {
            return (bool)m_marshaller.Unmarshal(typeof(bool), new AttributeExtCollection(), m_cdrIn);
        }

        [return:WideCharAttribute(false)]
        public char read_char() {
            return (char)m_marshaller.Unmarshal(typeof(char), 
                                                new AttributeExtCollection(new Attribute[] { 
                                                        new WideCharAttribute(false) } ),
                                                m_cdrIn);            
        }

        [return:WideCharAttribute(true)]
        public char read_wchar() {
            return (char)m_marshaller.Unmarshal(typeof(char),
                                                new AttributeExtCollection(new Attribute[] { 
                                                        new WideCharAttribute(true) } ), 
                                                m_cdrIn);
        }

        public byte read_octet() {
            return (byte)m_marshaller.Unmarshal(typeof(byte), new AttributeExtCollection(), m_cdrIn);            
        }

        public short read_short() {
            return (short)m_marshaller.Unmarshal(typeof(short), new AttributeExtCollection(), m_cdrIn);
        }

        public short read_ushort() {
            return (short)m_marshaller.Unmarshal(typeof(short), new AttributeExtCollection(), m_cdrIn);
        }

        public int read_long() {
            return (int)m_marshaller.Unmarshal(typeof(int), new AttributeExtCollection(), m_cdrIn);
        }

        public int read_ulong() {
            return (int)m_marshaller.Unmarshal(typeof(int), new AttributeExtCollection(), m_cdrIn);
        }

        public long read_longlong() {
            return (long)m_marshaller.Unmarshal(typeof(long), new AttributeExtCollection(), m_cdrIn);
        }

        public long read_ulonglong() {
            return (long)m_marshaller.Unmarshal(typeof(long), new AttributeExtCollection(), m_cdrIn);
        }

        public float read_float() {
            return (float)m_marshaller.Unmarshal(typeof(float), new AttributeExtCollection(), m_cdrIn);
        }

        public double read_double() {
            return (double)m_marshaller.Unmarshal(typeof(double), new AttributeExtCollection(), m_cdrIn);
        }

        [return:WideCharAttribute(false)]
        public string read_string() {
            return (string)m_marshaller.Unmarshal(s_stringType, 
                                                  new AttributeExtCollection(new Attribute[] { 
                                                          new WideCharAttribute(false) } ),
                                                  m_cdrIn);
        }

        [return:WideCharAttribute(true)]
        public string read_wstring() {
            return (string)m_marshaller.Unmarshal(s_stringType, 
                                                  new AttributeExtCollection(new Attribute[] { 
                                                          new WideCharAttribute(false) } ),
                                                  m_cdrIn);
        }

        public System.MarshalByRefObject read_Object() {
            return (MarshalByRefObject)m_marshaller.Unmarshal(s_mByRefType,
                                                              new AttributeExtCollection(), m_cdrIn);
        }

        [return:ObjectIdlType(IdlTypeObject.AbstractBase)]
        public object read_Abstract() {
            return m_marshaller.Unmarshal(s_objectType, 
                                          new AttributeExtCollection(new Attribute[] { 
                                                  new ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase) } ),
                                          m_cdrIn);
        }

        [return:ObjectIdlType(IdlTypeObject.ValueBase)]
        public object read_Value() {
            return m_marshaller.Unmarshal(s_objectType, 
                                          new AttributeExtCollection(new Attribute[] { 
                                                  new ObjectIdlTypeAttribute(IdlTypeObject.ValueBase) } ),
                                          m_cdrIn);
        }

        /// <summary>
        /// reads a value type, which is of the given formal type
        /// </summary>
        /// <param name="formal"></param>
        /// <returns></returns>
        public object read_ValueOfType(Type formal) {
            return m_marshaller.Unmarshal(formal, 
                                          new AttributeExtCollection(),
                                          m_cdrIn);
        }

        /// <summary>
        /// reads a corba wstring value
        /// </summary>
        /// <returns>the unboxed string</returns>
        public object read_WStringValue() {
            WStringValue result = (WStringValue)m_marshaller.Unmarshal(s_wstringValueType, 
                                                                       new AttributeExtCollection(),
                                                                       m_cdrIn);
            return result.Unbox();
        }

        /// <summary>
        /// reads a corba string value
        /// </summary>
        /// <returns>the unboxed string</returns>
        public object read_StringValue() {
            StringValue result = (StringValue)m_marshaller.Unmarshal(s_stringValueType, 
                                                                     new AttributeExtCollection(),
                                                                     m_cdrIn);
            return result.Unbox();
        }

        public object read_boxed(BoxedValueAttribute attr, Type boxedType, AttributeExtCollection boxedTypeAttrs) {
            if (boxedTypeAttrs == null) { boxedTypeAttrs = new AttributeExtCollection(); }
            boxedTypeAttrs.InsertAttribute(attr);
            return m_marshaller.Unmarshal(boxedType, boxedTypeAttrs, m_cdrIn);
        }

        public omg.org.CORBA.TypeCode read_TypeCode() {
            return null;
        }

        public void read_any_array([IdlSequenceAttribute][ObjectIdlType(IdlTypeObject.Any)] ref object[] seq, int offset, int length) {
            object[] res = (object[])m_marshaller.Unmarshal(typeof(object[]), 
                                                            new AttributeExtCollection(new Attribute[] { 
                                                                    new IdlSequenceAttribute(),
                                                                    new ObjectIdlTypeAttribute(IdlTypeObject.Any) } ),
                                                            m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_boolean_array([IdlSequenceAttribute] ref bool[] seq, int offset, int length) {
            bool[] res = (bool[])m_marshaller.Unmarshal(seq.GetType(), 
                                                        new AttributeExtCollection(new Attribute[] { 
                                                                new IdlSequenceAttribute() } ), 
                                                        m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_char_array([IdlSequenceAttribute] [WideCharAttribute(false)] ref char[] seq, int offset, int length) {
            char[] res = (char[])m_marshaller.Unmarshal(seq.GetType(), 
                                                        new AttributeExtCollection(new Attribute[] { 
                                                                new IdlSequenceAttribute(), 
                                                                new WideCharAttribute(false) } ),
                                                        m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_wchar_array([IdlSequenceAttribute] [WideCharAttribute(true)] ref char[]seq, int offset, int length) { 
            char[] res = (char[])m_marshaller.Unmarshal(seq.GetType(),
                                                        new AttributeExtCollection(new Attribute[] { 
                                                                new IdlSequenceAttribute(), 
                                                                new WideCharAttribute(true) } ),
                                                        m_cdrIn);            
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_octet_array([IdlSequenceAttribute] ref byte[] seq, int offset, int length) {
            byte[] res = (byte[])m_marshaller.Unmarshal(seq.GetType(),
                                                        new AttributeExtCollection(new Attribute[] { 
                                                                new IdlSequenceAttribute() } ),
                                                        m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_short_array([IdlSequenceAttribute] ref short[] seq, int offset, int length) {
            short[] res = (short[])m_marshaller.Unmarshal(seq.GetType(), 
                                                          new AttributeExtCollection(new Attribute[] { 
                                                                  new IdlSequenceAttribute() } ),
                                                          m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_ushort_array([IdlSequenceAttribute] ref short[] seq, int offset, int length) {
            short[] res = (short[])m_marshaller.Unmarshal(seq.GetType(),
                                                          new AttributeExtCollection(new Attribute[] { 
                                                                  new IdlSequenceAttribute() } ),
                                                          m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_long_array([IdlSequenceAttribute] ref int[] seq, int offset, int length) {
            int[] res = (int[])m_marshaller.Unmarshal(seq.GetType(), 
                                                      new AttributeExtCollection(new Attribute[] { 
                                                              new IdlSequenceAttribute() } ),
                                                      m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_ulong_array([IdlSequenceAttribute] ref int[] seq, int offset, int length) {
            int[] res = (int[])m_marshaller.Unmarshal(seq.GetType(), 
                                                      new AttributeExtCollection(new Attribute[] { 
                                                              new IdlSequenceAttribute() } ),
                                                      m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_ulonglong_array([IdlSequenceAttribute] ref long[] seq, int offset, int length) {
            long[] res = (long[])m_marshaller.Unmarshal(seq.GetType(),
                                                        new AttributeExtCollection(new Attribute[] {
                                                                new IdlSequenceAttribute() } ),
                                                        m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_longlong_array([IdlSequenceAttribute] ref long[] seq, int offset, int length) {
            long[] res = (long[])m_marshaller.Unmarshal(seq.GetType(),
                                                        new AttributeExtCollection(new Attribute[] { 
                                                                new IdlSequenceAttribute() } ),
                                                        m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_float_array([IdlSequenceAttribute] ref float[] seq, int offset, int length) {
            float[] res = (float[])m_marshaller.Unmarshal(seq.GetType(),
                                                          new AttributeExtCollection(new Attribute[] { 
                                                                  new IdlSequenceAttribute() } ), m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_double_array([IdlSequenceAttribute] ref double[] seq, int offset, int length) {
            double[] res = (double[])m_marshaller.Unmarshal(seq.GetType(), 
                                                            new AttributeExtCollection(new Attribute[] { 
                                                                    new IdlSequenceAttribute() } ),
                                                            m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }
        
        #endregion Implementation of DataInputStream

        #endregion IMethods

    }

}
