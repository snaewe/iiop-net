/* TypeCode.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 24.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace omg.org.CORBA {

    /// <summary>the type code enumeration</summary>
    [IdlEnumAttribute]
    public enum TCKind : long {
        tk_null = 0, 
        tk_void = 1,
        tk_short = 2,
        tk_long = 3,
        tk_ushort = 4,
        tk_ulong = 5,
        tk_float = 6,
        tk_double = 7,
        tk_boolean = 8,
        tk_char = 9,
        tk_octet = 10,
        tk_any = 11,
        tk_TypeCode = 12,
        tk_Principal = 13,
        tk_objref = 14,
        tk_struct = 15,
        tk_union = 16,
        tk_enum = 17,
        tk_string = 18,
        tk_sequence = 19,
        tk_array = 20,
        tk_alias = 21,
        tk_except = 22,
        tk_longlong = 23,
        tk_ulonglong = 24,
        tk_longdouble = 25,
        tk_wchar = 26,
        tk_wstring = 27,
        tk_fixed = 28,
        tk_value = 29,
        tk_value_box = 30,
        tk_native = 31,
        tk_abstract_interface = 32,
    }

    
    /// <summary>the typecode interface</summary>
    /// <remarks>mapped by hand from IDL</remarks>
    // Codeing Convention violated (identifiers), because this interface is mapped according the IDL <-> CLS mapping specification
    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContextExt:1.0")]
    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    public interface TypeCode : IIdlEntity {
        
        #region IMethods

        bool equal(TypeCode tc);

        bool equivalent(TypeCode tc);

        TypeCode get_compact_typecode();

        TCKind kind();

        /// <summary>get the repository id for kinds: objref, struct, union, enum, value, value-box, abstract-interface, ...</summary>
        [return:StringValueAttribute()]
        string id();
        
        [return:StringValueAttribute()]
        string name();

        /// <summary>get the number of members for kinds: tk_struct, tk_value, tk_enum, ...</summary>
        int member_count();
        [return:StringValueAttribute()]
        string member_name(int index);

        // for union: not mapped

        /// <summary>for kinds: string, sequence</summary>
        int length();

        /// <summary>for sequence, array, value_box</summary>
        TypeCode content_type();

        /// <summary>for kinds: value</summary>
        short member_visibility(int index);
        short type_modifier();
        TypeCode concrete_base_type();

        #endregion IMethods

    }


    internal struct StructMember {
        
        #region IFields

        internal string m_name;
        internal TypeCode m_type;

        #endregion IFileds
        #region IConstructors
        
        public StructMember(string name, TypeCode type) {
            m_name = name;
            m_type = type;
        }

        #endregion IConstructors

    }


    internal struct ValueTypeMember {

        #region IFields

        internal string m_name;
        internal TypeCode m_type;
        internal short m_visibility;

        #endregion IFields
        #region IConstructors

        public ValueTypeMember(string name, TypeCode type, short visibility) {
            m_name = name;
            m_type = type;
            m_visibility = visibility;
        }

        #endregion IConstructors

    }


    internal abstract class TypeCodeImpl : TypeCode, IIdlEntity {
    
        #region IFields
        
        /// <summary>the kind of this TypeCode</summary>
        private TCKind m_kind;

        #endregion IFields
        #region IConstructors


        protected TypeCodeImpl(TCKind kind) {
            m_kind = kind;
        }

        /// <summary>create a TypeCode-Impl from a CDRStream</summary>
        public TypeCodeImpl(CdrInputStream cdrStream, TCKind kind) {
            m_kind = kind;
            ReadFromStream(cdrStream);
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>serialize the whole type-code to the stream</summary>
        /// <param name="cdrStream"></param>
        internal virtual void WriteToStream(CdrOutputStream cdrStream) {
            uint val = Convert.ToUInt32(kind());
            cdrStream.WriteULong(val);
        }

        /// <summary>reads the type-code content from the stream, without the TCKind at the beginning</summary>
        /// <remarks>helper which is used by the constructor with arg CdrInputStream</remarks>
        protected virtual void ReadFromStream(CdrInputStream cdrStream) { }

        internal abstract Type GetClsForTypeCode();

        #region Implementation of TypeCode
        public bool equal(omg.org.CORBA.TypeCode tc) {
            if (tc.kind() != kind()) {
                return false;
            }
            return true;
        }
        public bool equivalent(omg.org.CORBA.TypeCode tc) {
            return true;
        }

        public virtual omg.org.CORBA.TypeCode get_compact_typecode() {
            return this;
        }
        public TCKind kind() {
            return m_kind;
        }
        [return:StringValueAttribute()]
        public virtual string id() {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        [return:StringValueAttribute()]
        public virtual string name() {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual int member_count() {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        [return:StringValueAttribute()]
        public virtual string member_name(int index) {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual omg.org.CORBA.TypeCode member_type(int index)     {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual int length() {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual omg.org.CORBA.TypeCode content_type() {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual short member_visibility(int index) {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual short type_modifier() {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual omg.org.CORBA.TypeCode concrete_base_type() {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
    
        #endregion Implementation of TypeCode

        #endregion IMethods

    }

    
    /// <summary>represents a TC for abstract and concrete interface</summary>
    internal abstract class InterfaceTC : TypeCodeImpl {
        
        #region IFields

        private string m_id;
        private string m_name;

        #endregion IFields
        #region IConstructors
        
        public InterfaceTC(CdrInputStream cdrStream, TCKind kind) : base(cdrStream, kind) {
        }     
        
        public InterfaceTC(string repositoryId, string name, TCKind kind) : base(kind) {
            m_id = repositoryId;
            m_name = name;
        }
        #endregion IConstructors
        #region IMethods
        
        [return:StringValueAttribute()]
        public override string id() {
            return m_id;
        }

        [return:StringValueAttribute()]
        public override string name() {
            return m_name;
        }

        protected override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = encap.ReadString();
            m_name = encap.ReadString();
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            cdrStream.WriteEncapsulation(encap);
        }

        internal override Type GetClsForTypeCode() {
            return Repository.GetTypeForId(m_id);
        }

        #endregion IMethods

    }

    internal class ObjRefTC : InterfaceTC {
        
        #region IConstructors

        public ObjRefTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_objref) { }
        public ObjRefTC(string repositoryID, string name) : base(repositoryID, name, TCKind.tk_objref) { }

        #endregion IConstructors
    }

    internal class AbstractIfTC : InterfaceTC {
        
        #region IConstructors

        public AbstractIfTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_abstract_interface) { }
        public AbstractIfTC(string repositoryID, string name) : base(repositoryID, name, TCKind.tk_abstract_interface) { }

        #endregion IConstructors
    }

    internal class NullTC : TypeCodeImpl {
        
        #region IConstructors
        
        public NullTC() : base(TCKind.tk_null) { }

        #endregion IConstructors
        #region IMethods
        
        internal override Type GetClsForTypeCode() {
            throw new NotSupportedException("this operation is not supported for a NullTC");
        }

        #endregion IMethods
    }

    internal class VoidTC : TypeCodeImpl {

        #region IConstructors

        public VoidTC() : base(TCKind.tk_void) { }

        #endregion IConstructors
        #region IMethods
        
        internal override Type GetClsForTypeCode() {
            return typeof(void);
        }

        #endregion IMethods
    }

    internal class ShortTC : TypeCodeImpl {
        
        #region IConstructors
        
        public ShortTC() : base(TCKind.tk_short) { }

        #endregion IConstructors
        #region IMethods
        
        internal override Type GetClsForTypeCode() {
            return typeof(System.Int16);
        }

        #endregion IMethods
    }

    internal class LongTC : TypeCodeImpl {
        
        #region IConstructors
        
        public LongTC() : base(TCKind.tk_long) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Int32);
        }

        #endregion IMethods

    }

    internal class UShortTC : TypeCodeImpl { 
        
        #region IConstructors
        
        public UShortTC() : base(TCKind.tk_ushort) { }

        #endregion IConstructors
        #region IMethods
        
        internal override Type GetClsForTypeCode() {
            return typeof(System.Int16);
        }

        #endregion IMethods

    }

    internal class ULongTC : TypeCodeImpl {

        #region IConstructors
        
        public ULongTC() : base(TCKind.tk_ulong) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Int32);
        }

        #endregion IMethods

    }

    internal class FloatTC : TypeCodeImpl {
        
        #region IConstructors
        
        public FloatTC() : base(TCKind.tk_float) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Single);
        }

        #endregion IMethods

    }

    internal class DoubleTC : TypeCodeImpl {
        
        #region IConstructors
        
        public DoubleTC() : base(TCKind.tk_double) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Double);
        }

        #endregion IMethods

    }

    internal class BooleanTC : TypeCodeImpl {
        
        #region IConstructors
        
        public BooleanTC() : base(TCKind.tk_boolean) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Boolean);
        }

        #endregion IMethods

    }

    internal class CharTC : TypeCodeImpl {

        #region IConstructors
        
        public CharTC() : base(TCKind.tk_char) { }
        
        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Char);
        }

        #endregion IMethods

    }

    internal class OctetTC : TypeCodeImpl {
        
        #region IConstructors
        
        public OctetTC() : base(TCKind.tk_octet) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Byte);
        }

        #endregion IMethods

    }

    internal class AnyTC : TypeCodeImpl {
        
        #region IConstructors
        
        public AnyTC() : base(TCKind.tk_any) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Object);
        }

        #endregion IMethods

    }

    internal class TypeCodeTC : TypeCodeImpl {
        
        #region IConstructors
        
        public TypeCodeTC() : base(TCKind.tk_TypeCode) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(omg.org.CORBA.TypeCode);
        }

        #endregion IMethods

    }

    internal class WCharTC : TypeCodeImpl {
        
        #region IConstructors
        
        public WCharTC() : base(TCKind.tk_char) { }

        #endregion IConstructors
        #region IMethods
        
        internal override Type GetClsForTypeCode() {
            return typeof(System.Char);
        }

        #endregion IMethods
    }

    internal class StringTC : TypeCodeImpl {
        
        #region IFields

        private int m_length;

        #endregion IFields
        #region IConstructors

        public StringTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_string) { }
        public StringTC(int length) : base(TCKind.tk_string) {
            m_length = length;
        }

        #endregion IConstructors
        #region IMethods

        public override int length() {
            return m_length;
        }
    
        internal override Type GetClsForTypeCode() {
            return typeof(System.String);
        }

        #endregion IMethods

    }

    internal class WStringTC : TypeCodeImpl {
        
        #region IFields
        
        private int m_length;

        #endregion IFields
        #region IConstructors
        
        public WStringTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_wstring) { }
        public WStringTC(int length) : base(TCKind.tk_string) {
            m_length = length;
        }

        #endregion IConstructors
        #region IMethods

        public override int length() {
            return m_length;
        }
    
        internal override Type GetClsForTypeCode() {
            return typeof(System.String);
        }

        #endregion IMethods

    }

    internal class LongLongTC : TypeCodeImpl {
        
        #region IConstructors
        
        public LongLongTC() : base(TCKind.tk_longlong) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Int64);
        }

        #endregion IMethods

    }

    internal class ULongLongTC : TypeCodeImpl {
        
        #region IConstructors
        
        public ULongLongTC() : base(TCKind.tk_ulonglong) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return typeof(System.Int64);
        }

        #endregion IMethods

    }

    internal class EnumTC : TypeCodeImpl {
        
        #region IFields

        private string m_id;
        private string m_name;

        private string[] m_members;

        #endregion IFields
        #region IConstructors
        
        public EnumTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_enum) {
        }

        public EnumTC(string repositoryID, string name, string[] members) : base(TCKind.tk_enum) {
            m_id = repositoryID;
            m_name = name;
            m_members = members;
            if (m_members == null) { m_members = new string[0]; }
        }

        #endregion IConstructors
        #region IMethods

        [return:StringValueAttribute()]
        public override string id() {
            return m_id;
        }

        [return:StringValueAttribute()]
        public override string name() {
            return m_name;
        }

        public override int member_count() {
            return m_members.Length;
        }
        [return:StringValueAttribute()]
        public override string member_name(int index) {
            return m_members[index];
        }

        protected override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = encap.ReadString();
            m_name = encap.ReadString();
            uint length = encap.ReadULong();
            m_members = new string[length];
            for (int i = 0; i < length; i++) {
                m_members[i] = encap.ReadString();
            }
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            encap.WriteULong((uint)m_members.Length);
            foreach(string member in m_members) {
                encap.WriteString(member);
            }
            cdrStream.WriteEncapsulation(encap);
        }

        internal override Type GetClsForTypeCode() {
            return Repository.GetTypeForId(m_id);
        }

        #endregion IMethods

    }

    internal class ValueBoxTC : TypeCodeImpl {
        
        #region IFields

        private string m_id;
        private string m_name;
        private TypeCode m_boxed;

        #endregion IFields
        #region IConstructors
        
        
        public ValueBoxTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_value_box) {
        }

        public ValueBoxTC(string repositoryID, string name, TypeCode boxed) : base(TCKind.tk_value_box) {
            m_id = repositoryID;
            m_name = name;
            m_boxed = boxed;    
        }

        #endregion IConstructors
        #region IMethods

        [return:StringValueAttribute()]
        public override string id() {
            return m_id;
        }

        [return:StringValueAttribute()]
        public override string name() {
            return m_name;
        }

        public override TypeCode content_type() {
            return m_boxed;
        }

        protected override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = encap.ReadString();
            m_name = encap.ReadString();
            TypeCodeSerializer ser = new TypeCodeSerializer();
            m_boxed = (TypeCode) ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), cdrStream);
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            TypeCodeSerializer ser = new TypeCodeSerializer();
            ser.Serialise(typeof(TypeCode), m_boxed, new AttributeExtCollection(new Attribute[0]), cdrStream);
            cdrStream.WriteEncapsulation(encap);
        }

        internal override Type GetClsForTypeCode() {
            return Repository.GetTypeForId(m_id);
        }

        #endregion IMethods

    }

    internal class SequenceTC : TypeCodeImpl {
        
        #region IFields

        private int m_length;
        private TypeCode m_seqType;

        #endregion IFields
        #region IConstructors

        public SequenceTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_sequence) {
        }

        public SequenceTC(TypeCode seqType, int length) : base(TCKind.tk_sequence) {
            m_seqType = seqType;    
            m_length = length;
        }

        #endregion IConstructors
        #region IMethods

        public override int length() {
            return m_length;
        }
    
        public override TypeCode content_type() {
            return m_seqType;
        }

        protected override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            TypeCodeSerializer ser = new TypeCodeSerializer();
            m_seqType = (TypeCode) ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), cdrStream);
            m_length = (int)encap.ReadULong();
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0);
            TypeCodeSerializer ser = new TypeCodeSerializer();
            ser.Serialise(typeof(TypeCode), m_seqType, new AttributeExtCollection(new Attribute[0]), cdrStream);
            encap.WriteULong((uint)m_length);
            cdrStream.WriteEncapsulation(encap);
        }

        internal override Type GetClsForTypeCode() {
            Type arrayType = Array.CreateInstance(((TypeCodeImpl)m_seqType).GetClsForTypeCode(), 0).GetType();
            return arrayType;
        }

        #endregion IMethods

    }


    /// <summary>base class for struct and exception</summary>
    internal abstract class BaseStructTC : TypeCodeImpl {

        #region IFields
        
        private string m_id;
        private string m_name;

        private StructMember[] m_members;

        #endregion IFields
        #region IConstructors
        
        public BaseStructTC(CdrInputStream cdrStream, TCKind kind) : base(cdrStream, kind) {
        }

        public BaseStructTC(string repositoryID, string name, StructMember[] members, TCKind kind) : base(kind) {
            m_id = repositoryID;
            m_name = name;
            m_members = members;
            if (m_members == null) { m_members = new StructMember[0]; }
        }

        #endregion IConstructors
        #region IMethods

        [return:StringValueAttribute()]
        public override string id() {
            return m_id;
        }

        [return:StringValueAttribute()]
        public override string name() {
            return m_name;
        }

        public override int member_count() {
            return m_members.Length;
        }
        [return:StringValueAttribute()]
        public override string member_name(int index) {
            return m_members[index].m_name;
        }
        public override TypeCode member_type(int index)     {
            return m_members[index].m_type;            
        }

        protected override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = encap.ReadString();
            m_name = encap.ReadString();
            uint length = encap.ReadULong();
            m_members = new StructMember[length];
            TypeCodeSerializer ser = new TypeCodeSerializer();
            for (int i = 0; i < length; i++) {
                string memberName = encap.ReadString();
                TypeCode memberType = (TypeCode)ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), cdrStream);    
                StructMember member = new StructMember(memberName, memberType);
                m_members[i] = member;
            }
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            encap.WriteULong((uint)m_members.Length);
            TypeCodeSerializer ser = new TypeCodeSerializer();            
            foreach(StructMember member in m_members) {
                encap.WriteString(member.m_name);
                ser.Serialise(typeof(TypeCode), member.m_type, new AttributeExtCollection(new Attribute[0]), cdrStream);
            }
            cdrStream.WriteEncapsulation(encap);
        }

        internal override Type GetClsForTypeCode() {
            return Repository.GetTypeForId(m_id);
        }

        #endregion IMethods

    }

    internal class StructTC : BaseStructTC {
        
        #region IConstructors
        
        public StructTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_struct) { }
        public StructTC(string repositoryID, string name, StructMember[] members) : base(repositoryID, name, members, TCKind.tk_struct) { }

        #endregion IConstructors

    }

    internal class ExceptTC : BaseStructTC {

        #region IConstructors

        public ExceptTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_except) { }
        public ExceptTC(string repositoryID, string name, StructMember[] members) : base(repositoryID, name, members, TCKind.tk_except) { }

        #endregion IConstrutors

    }
    
    
    internal class ValueTypeTC : TypeCodeImpl {

        #region IFields

        private string m_id;
        private string m_name;
        private ValueTypeMember[] m_members;
        private short m_typeMod;
        private TypeCode m_baseClass;

        #endregion IFields
        #region IConstructors
        
        public ValueTypeTC(CdrInputStream cdrStream) : base(cdrStream, TCKind.tk_value) {
        }

        public ValueTypeTC(string repositoryID, string name, ValueTypeMember[] members, TypeCode baseClass, short typeMod) : base(TCKind.tk_value) {
            m_id = repositoryID;
            m_name = name;
            m_members = members;
            if (m_members == null) { m_members = new ValueTypeMember[0]; }
            m_baseClass = baseClass;
            m_typeMod = typeMod;
        }

        #endregion IConstructors
        #region IMethods

        [return:StringValueAttribute()]
        public override string id() {
            return m_id;
        }

        [return:StringValueAttribute()]
        public override string name() {
            return m_name;
        }

        public override int member_count() {
            return m_members.Length;
        }
        [return:StringValueAttribute()]
        public override string member_name(int index) {
            return m_members[index].m_name;
        }
        public override TypeCode member_type(int index)     {
            return m_members[index].m_type;            
        }

        public override short member_visibility(int index) {
            return m_members[index].m_visibility;
        }
        public override short type_modifier() {
            return m_typeMod;
        }
        public override omg.org.CORBA.TypeCode concrete_base_type() {
            return m_baseClass;
        }


        protected override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = encap.ReadString();
            m_name = encap.ReadString();
            m_typeMod = encap.ReadShort();
            TypeCodeSerializer ser = new TypeCodeSerializer();
            m_baseClass = (TypeCode)ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), cdrStream);    
            // deser members
            uint length = encap.ReadULong();
            m_members = new ValueTypeMember[length];
            for (int i = 0; i < length; i++) {
                string memberName = encap.ReadString();
                TypeCode memberType = (TypeCode)ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), cdrStream);    
                short visibility = encap.ReadShort();
                ValueTypeMember member = new ValueTypeMember(memberName, memberType, visibility);
                m_members[i] = member;
            }
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            encap.WriteShort(m_typeMod);
            TypeCodeSerializer ser = new TypeCodeSerializer();
            // ser baseclass type
            ser.Serialise(typeof(TypeCode), m_baseClass, new AttributeExtCollection(new Attribute[0]), cdrStream);            
            // ser members
            encap.WriteULong((uint)m_members.Length);
            foreach(ValueTypeMember member in m_members) {
                encap.WriteString(member.m_name);
                ser.Serialise(typeof(TypeCode), member.m_type, new AttributeExtCollection(new Attribute[0]), cdrStream);
                encap.WriteShort(member.m_visibility);
            }
            cdrStream.WriteEncapsulation(encap);
        }

        internal override Type GetClsForTypeCode() {
            return Repository.GetTypeForId(m_id);
        }

        #endregion IMethods

    }
    
}

namespace Ch.Elca.Iiop.Marshalling {
    

    /// <summary>serializes a typecode</summary>
    internal class TypeCodeSerializer : Serialiser {
        
        #region IMethods

        public override object Deserialise(System.Type formal, AttributeExtCollection attributes, CdrInputStream sourceStream) {
            long kindVal = (long)sourceStream.ReadULong();
            omg.org.CORBA.TCKind kind = (omg.org.CORBA.TCKind)Enum.ToObject(typeof(omg.org.CORBA.TCKind),
                                                                            kindVal);
            switch(kind) {
                case omg.org.CORBA.TCKind.tk_abstract_interface :
                    return new omg.org.CORBA.AbstractIfTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_alias:
                    throw new NotImplementedException("alias not implemented");
                case omg.org.CORBA.TCKind.tk_any:
                    return new omg.org.CORBA.AnyTC();
                case omg.org.CORBA.TCKind.tk_array:
                    throw new NotImplementedException("array not implemented");
                case omg.org.CORBA.TCKind.tk_boolean:
                    return new omg.org.CORBA.BooleanTC();
                case omg.org.CORBA.TCKind.tk_char:
                    return new omg.org.CORBA.CharTC();
                case omg.org.CORBA.TCKind.tk_double:
                    return new omg.org.CORBA.DoubleTC();
                case omg.org.CORBA.TCKind.tk_enum:
                    return new omg.org.CORBA.EnumTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_except:
                    return new omg.org.CORBA.ExceptTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_fixed:
                    throw new NotImplementedException("fixed not implemented");
                case omg.org.CORBA.TCKind.tk_float:
                    return new omg.org.CORBA.FloatTC();
                case omg.org.CORBA.TCKind.tk_long:
                    return new omg.org.CORBA.LongTC();
                case omg.org.CORBA.TCKind.tk_longdouble:
                    throw new NotImplementedException("long double not implemented");
                case omg.org.CORBA.TCKind.tk_longlong:
                    return new omg.org.CORBA.LongLongTC();
                case omg.org.CORBA.TCKind.tk_native:
                    throw new NotSupportedException("native not supported");
                case omg.org.CORBA.TCKind.tk_null:
                    return new omg.org.CORBA.NullTC();
                case omg.org.CORBA.TCKind.tk_objref:
                    return new omg.org.CORBA.ObjRefTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_octet:
                    return new omg.org.CORBA.OctetTC();
                case omg.org.CORBA.TCKind.tk_Principal:
                    throw new NotImplementedException("Principal not implemented");
                case omg.org.CORBA.TCKind.tk_sequence:
                    return new omg.org.CORBA.SequenceTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_short:
                    return new omg.org.CORBA.ShortTC();
                case omg.org.CORBA.TCKind.tk_string:
                    return new omg.org.CORBA.StringTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_struct:
                    return new omg.org.CORBA.StructTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_TypeCode:
                    return new omg.org.CORBA.TypeCodeTC();
                case omg.org.CORBA.TCKind.tk_ulong:
                    return new omg.org.CORBA.ULongTC();
                case omg.org.CORBA.TCKind.tk_ulonglong:
                    return new omg.org.CORBA.ULongLongTC();
                case omg.org.CORBA.TCKind.tk_union:
                    throw new NotSupportedException("Union not supported");
                case omg.org.CORBA.TCKind.tk_ushort:
                    return new omg.org.CORBA.UShortTC();
                case omg.org.CORBA.TCKind.tk_value:
                    return new omg.org.CORBA.ValueTypeTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_value_box:
                    return new omg.org.CORBA.ValueBoxTC(sourceStream);
                case omg.org.CORBA.TCKind.tk_void:
                    return new omg.org.CORBA.VoidTC();
                case omg.org.CORBA.TCKind.tk_wchar:
                    return new omg.org.CORBA.WCharTC();
                case omg.org.CORBA.TCKind.tk_wstring:
                    return new omg.org.CORBA.WStringTC(sourceStream);
                default:
                    // unknown typecode: kind
                    throw new omg.org.CORBA.BAD_PARAM(1504, 
                                                      omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
        }

        public override void Serialise(System.Type formal, object actual, AttributeExtCollection attributes, CdrOutputStream targetStream) {
            if (!(actual is omg.org.CORBA.TypeCodeImpl)) { 
                // typecode not serializable
                throw new omg.org.CORBA.INTERNAL(1654, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            omg.org.CORBA.TypeCodeImpl tcImpl = actual as omg.org.CORBA.TypeCodeImpl;
            tcImpl.WriteToStream(targetStream);
        }

        #endregion IMethods

    }



}
