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
        /// <summary>wirtes the value val, takes as formal parameter the actual type of val</summary>
        void write_ValueOfActualType(object val);
        /// <summary>
        /// writes a corba wstring value
        /// </summary>
        void write_WStringValue(string arg);
        /// <summary>
        /// writes a corba string value
        /// </summary>
        void write_StringValue(string arg);
        /// <summary>writes boxed values</summary>
        void write_boxed(object val, BoxedValueAttribute attr);
        void write_TypeCode(omg.org.CORBA.TypeCode val);
        void write_any_array([IdlSequenceAttribute(0L)][ObjectIdlType(IdlTypeObject.Any)] object[] seq,
                             int offset, int length);
        void write_boolean_array([IdlSequenceAttribute(0L)] bool[] seq, int offset, int length);
        void write_char_array([IdlSequenceAttribute(0L)] [WideCharAttribute(false)] char[] seq,
                              int offset, int length);
        void write_wchar_array([IdlSequenceAttribute(0L)] [WideCharAttribute(true)] char[]seq,
                               int offset, int length);
        void write_octet_array([IdlSequenceAttribute(0L)] byte[] seq, int offset, int length);
        void write_short_array([IdlSequenceAttribute(0L)] short[] seq, int offset, int length);
        void write_ushort_array([IdlSequenceAttribute(0L)] short[] seq, int offset, int length);
        void write_long_array([IdlSequenceAttribute(0L)] int[] seq, int offset, int length);
        void write_ulong_array([IdlSequenceAttribute(0L)] int[] seq, int offset, int length);
        void write_ulonglong_array([IdlSequenceAttribute(0L)] long[] seq, int offset, int length);
        void write_longlong_array([IdlSequenceAttribute(0L)] long[] seq, int offset, int length);
        void write_float_array([IdlSequenceAttribute(0L)] float[] seq, int offset, int length);
        void write_double_array([IdlSequenceAttribute(0L)] double[] seq, int offset, int length);

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
        string read_WStringValue();
        /// <summary>
        /// reads a corba string value
        /// </summary>
        /// <returns>the unboxed string</returns>
        string read_StringValue();
        /// <summary>boxed values are not handable with read-value</summary>
        /// <param name="boxedType">the boxed type, which is not itself a boxed type</param>
        object read_boxed(BoxedValueAttribute attr, Type boxedType, AttributeExtCollection boxedTypeAttrs);
        omg.org.CORBA.TypeCode read_TypeCode();

        void read_any_array([IdlSequenceAttribute(0L)][ObjectIdlTypeAttribute(IdlTypeObject.Any)] ref object[] seq,
                            int offset, int length);
        void read_boolean_array([IdlSequenceAttribute(0L)] ref bool[] seq, int offset, int length);
        void read_char_array([IdlSequenceAttribute(0L)] [WideCharAttribute(false)] ref char[] seq,
                             int offset, int length);
        void read_wchar_array([IdlSequenceAttribute(0L)] [WideCharAttribute(true)] ref char[]seq,
                              int offset, int length);
        void read_octet_array([IdlSequenceAttribute(0L)] ref byte[] seq, int offset, int length);
        byte[] read_octet_array();
        void read_short_array([IdlSequenceAttribute(0L)] ref short[] seq, int offset, int length);
        void read_ushort_array([IdlSequenceAttribute(0L)] ref short[] seq, int offset, int length);
        void read_long_array([IdlSequenceAttribute(0L)] ref int[] seq, int offset, int length);
        void read_ulong_array([IdlSequenceAttribute(0L)] ref int[] seq, int offset, int length);
        void read_ulonglong_array([IdlSequenceAttribute(0L)] ref long[] seq, int offset, int length);
        void read_longlong_array([IdlSequenceAttribute(0L)] ref long[] seq, int offset, int length);
        void read_float_array([IdlSequenceAttribute(0L)] ref float[] seq, int offset, int length);
        void read_double_array([IdlSequenceAttribute(0L)] ref double[] seq, int offset, int length);

        #endregion IMethods

    }


    /// <summary>
    /// inplementation of the DataOutputStream interface
    /// </summary>
    internal class DataOutputStreamImpl : DataOutputStream {

        #region Types
 
        /// <summary>dummy typed needed to serialize null as true value type, not as any
        /// see write_ValueOfActualType
        /// </summary>
        [Serializable]
        [ExplicitSerializationOrdered()]
        internal class DummyValTypeForNull {
        }
 
        #endregion Types
        #region SFields
 
        private static Type s_dummyValType = typeof(DummyValTypeForNull);

        private static Type s_wstringValueType = ReflectionHelper.WStringValueType;
        private static Type s_stringValueType = ReflectionHelper.StringValueType;

        #endregion SFields
        #region IFields

        private SerializerFactory m_serFactory;

        private CdrOutputStream m_cdrOut;

        #endregion IFields
        #region IConstructors

        public DataOutputStreamImpl(CdrOutputStream cdrOut, SerializerFactory serFactory) {
            m_cdrOut = cdrOut;
            m_serFactory = serFactory;
        }

        #endregion IConstructors
        #region IMethods
 
        private void Marshal(Type type, AttributeExtCollection attributes,
                             object val, CdrOutputStream cdrOut) {
            Serializer ser = m_serFactory.Create(type, attributes);
            ser.Serialize(val, cdrOut);
 
        }

        #region Implementation of DataOutputStream
        public void write_any([ObjectIdlType(IdlTypeObject.Any)] object val) {
            Marshal(ReflectionHelper.ObjectType,
                    new AttributeExtCollection(new Attribute[] {
                        new ObjectIdlTypeAttribute(IdlTypeObject.Any)}),
                    val, m_cdrOut);
        }

        public void write_boolean(bool val)    {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_char([WideCharAttribute(false)]char val) {
            Marshal(val.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new WideCharAttribute(false) }),
                    val, m_cdrOut);
        }

        public void write_wchar([WideCharAttribute(true)]char val) {
            Marshal(val.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new WideCharAttribute(true) }),
                    val, m_cdrOut);
        }

        public void write_octet(byte val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_short(short val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_ushort(short val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_long(int val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_ulong(int val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_longlong(long val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_ulonglong(long val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_float(float val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_double(double val) {
            Marshal(val.GetType(), AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_string([WideCharAttribute(false)]string val) {
            Marshal(val.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new WideCharAttribute(false) }),
                    val, m_cdrOut);
        }
 
        public void write_wstring([WideCharAttribute(true)]string val) {
            Marshal(val.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new WideCharAttribute(true) }),
                    val, m_cdrOut);
        }

        public void write_Object(System.MarshalByRefObject val) {
            Marshal(ReflectionHelper.MarshalByRefObjectType,
                    AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_Abstract([ObjectIdlType(IdlTypeObject.AbstractBase)]object val) {
            Marshal(ReflectionHelper.ObjectType,
                    new AttributeExtCollection(new Attribute[] {
                        new ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase) } ),
                    val, m_cdrOut);
        }

        public void write_Value([ObjectIdlType(IdlTypeObject.ValueBase)]object val) {
            Marshal(ReflectionHelper.ObjectType,
                    new AttributeExtCollection(new Attribute[] {
                        new ObjectIdlTypeAttribute(IdlTypeObject.ValueBase) } ),
                    val, m_cdrOut);
        }
 
        public void write_ValueOfActualType(object val) {
            if (val != null) {
                Marshal(val.GetType(),
                        AttributeExtCollection.EmptyCollection,
                        val, m_cdrOut);
            } else {
                Marshal(s_dummyValType,
                        AttributeExtCollection.EmptyCollection,
                        val, m_cdrOut);
            }
        }

        public void write_WStringValue(string val) {
            object boxed = new WStringValue(val);
            Marshal(s_wstringValueType, AttributeExtCollection.EmptyCollection,
                    boxed, m_cdrOut);
        }

        public void write_StringValue(string val) {
            object boxed = new StringValue(val);
            Marshal(s_stringValueType, AttributeExtCollection.EmptyCollection,
                    boxed, m_cdrOut);
        }


        public void write_boxed(object val, BoxedValueAttribute attr) {
            Marshal(val.GetType(),
                    new AttributeExtCollection(new Attribute[] { attr } ),
                    val, m_cdrOut);
        }


        public void write_TypeCode(omg.org.CORBA.TypeCode val) {
            Marshal(ReflectionHelper.CorbaTypeCodeType,
                    AttributeExtCollection.EmptyCollection,
                    val, m_cdrOut);
        }

        public void write_any_array([IdlSequenceAttribute(0L)][ObjectIdlType(IdlTypeObject.Any)] object[] seq,
                                    int offset, int length) {
            Marshal(ReflectionHelper.ObjectArrayType,
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L),
                        new ObjectIdlTypeAttribute(IdlTypeObject.Any) } ),
                    seq, m_cdrOut);
        }

        public void write_boolean_array([IdlSequenceAttribute(0L)] bool[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }

        public void write_char_array([IdlSequenceAttribute(0L)] [WideCharAttribute(false)] char[] seq,
                                     int offset, int length)  {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L), new WideCharAttribute(false) } ),
                    seq, m_cdrOut);
        }

        public void write_wchar_array([IdlSequenceAttribute(0L)] [WideCharAttribute(true)] char[]seq,
                                      int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L), new WideCharAttribute(true) } ),
                    seq, m_cdrOut);
        }

        public void write_octet_array([IdlSequenceAttribute(0L)] byte[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }

        public void write_short_array([IdlSequenceAttribute(0L)] short[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }

        public void write_ushort_array([IdlSequenceAttribute(0L)] short[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }

        public void write_long_array([IdlSequenceAttribute(0L)] int[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }

        public void write_ulong_array([IdlSequenceAttribute(0L)] int[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }

        public void write_ulonglong_array([IdlSequenceAttribute(0L)] long[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }

        public void write_longlong_array([IdlSequenceAttribute(0L)] long[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }
 
        public void write_float_array([IdlSequenceAttribute(0L)] float[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
                    seq, m_cdrOut);
        }
        public void write_double_array([IdlSequenceAttribute(0L)] double[] seq, int offset, int length) {
            Marshal(seq.GetType(),
                    new AttributeExtCollection(new Attribute[] {
                        new IdlSequenceAttribute(0L) } ),
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

        private static Type s_wstringValueType = ReflectionHelper.WStringValueType;
        private static Type s_stringValueType = ReflectionHelper.StringValueType;

        #endregion SFields
        #region IFields

        private SerializerFactory m_serFactory;

        private CdrInputStream m_cdrIn;

        #endregion IFields
        #region IConstructors

        public DataInputStreamImpl(CdrInputStream cdrIn, SerializerFactory serFactory) {
            m_cdrIn = cdrIn;
            m_serFactory = serFactory;
        }

        #endregion IConstructors
        #region IMethods

        #region Implementation of DataInputStream
 
        private object Unmarshal(Type type, AttributeExtCollection attributes,
                                 CdrInputStream cdrIn) {
            Serializer ser = m_serFactory.Create(type, attributes);
            return ser.Deserialize(cdrIn);
        }
 
        [return:ObjectIdlTypeAttribute(IdlTypeObject.Any)]
        public object read_any() {
            return Unmarshal(ReflectionHelper.ObjectType,
                             new AttributeExtCollection(new Attribute[] {
                                 new ObjectIdlTypeAttribute(IdlTypeObject.Any) } ),
                             m_cdrIn);
        }

        public bool read_boolean() {
            return (bool)Unmarshal(ReflectionHelper.BooleanType,
                                   AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        [return:WideCharAttribute(false)]
        public char read_char() {
            return (char)Unmarshal(ReflectionHelper.CharType,
                                   new AttributeExtCollection(new Attribute[] {
                                       new WideCharAttribute(false) } ),
                                   m_cdrIn);
        }

        [return:WideCharAttribute(true)]
        public char read_wchar() {
            return (char)Unmarshal(ReflectionHelper.CharType,
                                   new AttributeExtCollection(new Attribute[] {
                                       new WideCharAttribute(true) } ),
                                   m_cdrIn);
        }

        public byte read_octet() {
            return (byte)Unmarshal(ReflectionHelper.ByteType,
                                   AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        public short read_short() {
            return (short)Unmarshal(ReflectionHelper.Int16Type, AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        public short read_ushort() {
            return (short)Unmarshal(ReflectionHelper.Int16Type, AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        public int read_long() {
            return (int)Unmarshal(ReflectionHelper.Int32Type, AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        public int read_ulong() {
            return (int)Unmarshal(ReflectionHelper.Int32Type, AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        public long read_longlong() {
            return (long)Unmarshal(ReflectionHelper.Int64Type, AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        public long read_ulonglong() {
            return (long)Unmarshal(ReflectionHelper.Int64Type, AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        public float read_float() {
            return (float)Unmarshal(ReflectionHelper.SingleType, AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        public double read_double() {
            return (double)Unmarshal(ReflectionHelper.DoubleType, AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        [return:WideCharAttribute(false)]
        public string read_string() {
            return (string)Unmarshal(ReflectionHelper.StringType,
                                     new AttributeExtCollection(new Attribute[] {
                                         new WideCharAttribute(false) } ),
                                     m_cdrIn);
        }

        [return:WideCharAttribute(true)]
        public string read_wstring() {
            return (string)Unmarshal(ReflectionHelper.StringType,
                                     new AttributeExtCollection(new Attribute[] {
                                         new WideCharAttribute(false) } ),
                                     m_cdrIn);
        }

        public System.MarshalByRefObject read_Object() {
            return (MarshalByRefObject)Unmarshal(ReflectionHelper.MarshalByRefObjectType,
                                                 AttributeExtCollection.EmptyCollection, m_cdrIn);
        }

        [return:ObjectIdlType(IdlTypeObject.AbstractBase)]
        public object read_Abstract() {
            return Unmarshal(ReflectionHelper.ObjectType,
                             new AttributeExtCollection(new Attribute[] {
                                 new ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase) } ),
                             m_cdrIn);
        }

        [return:ObjectIdlType(IdlTypeObject.ValueBase)]
        public object read_Value() {
            return Unmarshal(ReflectionHelper.ObjectType,
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
            return Unmarshal(formal,
                             AttributeExtCollection.EmptyCollection,
                             m_cdrIn);
        }

        /// <summary>
        /// reads a corba wstring value
        /// </summary>
        /// <returns>the unboxed string</returns>
        public string read_WStringValue() {
            WStringValue result = (WStringValue)Unmarshal(s_wstringValueType,
                                                    AttributeExtCollection.EmptyCollection,
                                                    m_cdrIn);
            return (string)result.Unbox();
        }

        /// <summary>
        /// reads a corba string value
        /// </summary>
        /// <returns>the unboxed string</returns>
        public string read_StringValue() {
            StringValue result = (StringValue)Unmarshal(s_stringValueType,
                                                  AttributeExtCollection.EmptyCollection,
                                                  m_cdrIn);
            return (string)result.Unbox();
        }

        public object read_boxed(BoxedValueAttribute attr, Type boxedType, AttributeExtCollection boxedTypeAttrs) {
            if (boxedTypeAttrs == null) {
                boxedTypeAttrs = AttributeExtCollection.EmptyCollection;
            }
            boxedTypeAttrs = boxedTypeAttrs.MergeAttribute(attr);
            return Unmarshal(boxedType, boxedTypeAttrs, m_cdrIn);
        }

        public omg.org.CORBA.TypeCode read_TypeCode() {
            return null;
        }

        public void read_any_array([IdlSequenceAttribute(0L)][ObjectIdlType(IdlTypeObject.Any)] ref object[] seq, int offset, int length) {
            object[] res = (object[])Unmarshal(ReflectionHelper.ObjectArrayType,
                                               new AttributeExtCollection(new Attribute[] {
                                                   new IdlSequenceAttribute(0L),
                                                   new ObjectIdlTypeAttribute(IdlTypeObject.Any) } ),
                                               m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_boolean_array([IdlSequenceAttribute(0L)] ref bool[] seq, int offset, int length) {
            bool[] res = (bool[])Unmarshal(seq.GetType(),
                                           new AttributeExtCollection(new Attribute[] {
                                               new IdlSequenceAttribute(0L) } ),
                                           m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_char_array([IdlSequenceAttribute(0L)] [WideCharAttribute(false)] ref char[] seq, int offset, int length) {
            char[] res = (char[])Unmarshal(seq.GetType(),
                                           new AttributeExtCollection(new Attribute[] {
                                               new IdlSequenceAttribute(0L),
                                               new WideCharAttribute(false) } ),
                                           m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_wchar_array([IdlSequenceAttribute(0L)] [WideCharAttribute(true)] ref char[]seq, int offset, int length) {
            char[] res = (char[])Unmarshal(seq.GetType(),
                                           new AttributeExtCollection(new Attribute[] {
                                               new IdlSequenceAttribute(0L),
                                               new WideCharAttribute(true) } ),
                                           m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_octet_array([IdlSequenceAttribute(0L)] ref byte[] seq, int offset, int length) {
            byte[] res = read_octet_array();
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public byte[] read_octet_array() {
            byte[] res = (byte[])Unmarshal(typeof(byte[]),
                                           new AttributeExtCollection(new Attribute[] {
                                               new IdlSequenceAttribute(0L) } ),
                                           m_cdrIn);
            return res;
        }


        public void read_short_array([IdlSequenceAttribute(0L)] ref short[] seq, int offset, int length) {
            short[] res = (short[])Unmarshal(seq.GetType(),
                                             new AttributeExtCollection(new Attribute[] {
                                                 new IdlSequenceAttribute(0L) } ),
                                             m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_ushort_array([IdlSequenceAttribute(0L)] ref short[] seq, int offset, int length) {
            short[] res = (short[])Unmarshal(seq.GetType(),
                                             new AttributeExtCollection(new Attribute[] {
                                                 new IdlSequenceAttribute(0L) } ),
                                             m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_long_array([IdlSequenceAttribute(0L)] ref int[] seq, int offset, int length) {
            int[] res = (int[])Unmarshal(seq.GetType(),
                                         new AttributeExtCollection(new Attribute[] {
                                             new IdlSequenceAttribute(0L) } ),
                                         m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_ulong_array([IdlSequenceAttribute(0L)] ref int[] seq, int offset, int length) {
            int[] res = (int[])Unmarshal(seq.GetType(),
                                         new AttributeExtCollection(new Attribute[] {
                                             new IdlSequenceAttribute(0L) } ),
                                         m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_ulonglong_array([IdlSequenceAttribute(0L)] ref long[] seq, int offset, int length) {
            long[] res = (long[])Unmarshal(seq.GetType(),
                                           new AttributeExtCollection(new Attribute[] {
                                               new IdlSequenceAttribute(0L) } ),
                                           m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_longlong_array([IdlSequenceAttribute(0L)] ref long[] seq, int offset, int length) {
            long[] res = (long[])Unmarshal(seq.GetType(),
                                           new AttributeExtCollection(new Attribute[] {
                                               new IdlSequenceAttribute(0L) } ),
                                           m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_float_array([IdlSequenceAttribute(0L)] ref float[] seq, int offset, int length) {
            float[] res = (float[])Unmarshal(seq.GetType(),
                                             new AttributeExtCollection(new Attribute[] {
                                                 new IdlSequenceAttribute(0L) } ), m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }

        public void read_double_array([IdlSequenceAttribute(0L)] ref double[] seq, int offset, int length) {
            double[] res = (double[])Unmarshal(seq.GetType(),
                                               new AttributeExtCollection(new Attribute[] {
                                                   new IdlSequenceAttribute(0L) } ),
                                               m_cdrIn);
            Array.Copy((Array)res, 0, (Array)seq, offset, length);
        }
 
        #endregion Implementation of DataInputStream

        #endregion IMethods

    }

}

#if UnitTest

namespace Ch.Elca.Iiop.Tests {
 
    using System;
    using System.Reflection;
    using System.IO;
    using NUnit.Framework;
    using omg.org.CORBA;
    using omg.org.IOP;
    using Corba;
    using Ch.Elca.Iiop.Interception;
 
    /// <summary>
    /// Unit-tests for testing DataInputStream
    /// </summary>
    [TestFixture]
    public class DataInputStreamTest {
 
        private SerializerFactory m_serFactory;
 
        [SetUp]
        public void SetUp() {
            m_serFactory = new SerializerFactory();
            CodecFactory codecFactory =
                new CodecFactoryImpl(m_serFactory);
            Codec codec =
                codecFactory.create_codec(
                    new Encoding(ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_serFactory.Initalize(new SerializerFactoryConfig(), IiopUrlUtil.Create(codec));
        }
 
        private DataInputStream CreateInputStream(byte[] content) {
            MemoryStream contentStream = new MemoryStream(content);
            CdrInputStreamImpl inputStream = new CdrInputStreamImpl(contentStream);
            inputStream.ConfigStream(0, new GiopVersion(1, 2));
            inputStream.SetMaxLength((uint)content.Length);
            DataInputStreamImpl di =
                new DataInputStreamImpl(inputStream, m_serFactory);
            return di;
        }
 
        [Test]
        public void TestReadOctet() {
            byte val = 1;
            DataInputStream inputStream = CreateInputStream(new byte[] { val });
            byte read = inputStream.read_octet();
            Assert.AreEqual(val, read, "read");
        }
 
    }
 
    /// <summary>
    /// Unit-tests for testing DataOutputStream
    /// </summary>
    [TestFixture]
    public class DataOutputStreamTest {

        private SerializerFactory m_serFactory;
 
        [SetUp]
        public void SetUp() {
            m_serFactory = new SerializerFactory();
            CodecFactory codecFactory =
                new CodecFactoryImpl(m_serFactory);
            Codec codec =
                codecFactory.create_codec(
                    new Encoding(ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_serFactory.Initalize(new SerializerFactoryConfig(), IiopUrlUtil.Create(codec));
        }
 
        [Test]
        public void TestWriteOctet() {
            byte val = 1;
            MemoryStream outputStream = new MemoryStream();
            CdrOutputStream cdrOut = new CdrOutputStreamImpl(outputStream, 0, new GiopVersion(1,2));
            DataOutputStream doStream = new DataOutputStreamImpl(cdrOut,
                                                                 m_serFactory);
            doStream.write_octet(val);
            outputStream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(val, outputStream.ReadByte(), "written");
        }
 
    }

}

#endif
