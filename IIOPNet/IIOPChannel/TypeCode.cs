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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;

namespace omg.org.CORBA {

    /// <summary>the type code enumeration</summary>
    /// <remarks>IDL enums are mapped to CLS with an int32 base type</remarks>
    [IdlEnumAttribute]
    public enum TCKind : int {
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
        tk_local_interface = 33        
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

        /// <summary>get the number of members for kinds: tk_struct, tk_value, tk_enum, tk_union ...</summary>
        int member_count();
        [return:StringValueAttribute()]
        string member_name(int index);

        // for union:        
        object member_label (int index);        
        TypeCode discriminator_type();
        int default_index();

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

    internal struct UnionSwitchCase {
        #region IFields

        internal object DiscriminatorValue;
        internal omg.org.CORBA.TypeCode ElementType;
        internal string ElementName;

        #endregion IFields

        public UnionSwitchCase(object discriminatorValue, string elementName, TypeCode elementType) {
            DiscriminatorValue = discriminatorValue;
            ElementType = elementType;
            ElementName = elementName;
        }
    }


    internal abstract class TypeCodeImpl : TypeCode, IIdlEntity {
    
        #region IFields
        
        /// <summary>the kind of this TypeCode</summary>
        private TCKind m_kind;

        #endregion IFields
        #region IConstructors
        
        public TypeCodeImpl(TCKind kind) {
            m_kind = kind;
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>serialize the whole type-code to the stream</summary>
        /// <param name="cdrStream"></param>
        internal virtual void WriteToStream(CdrOutputStream cdrStream) {
            StreamPosition indirPos = new StreamPosition(cdrStream); 
            uint val = Convert.ToUInt32(kind());
            cdrStream.WriteULong(val);
            cdrStream.StoreIndirection(this, 
                                       new IndirectionInfo(indirPos.Position, 
                                                           IndirectionType.TypeCode,
                                                           IndirectionUsage.TypeCode));
        }        

        /// <summary>reads the type-code content from the stream, without the TCKind at the beginning</summary>
        /// <remarks>helper which is used by the constructor with arg CdrInputStream</remarks>
        internal virtual void ReadFromStream(CdrInputStream cdrStream) { }
        
        protected string ReadRepositoryId(CdrInputStream cdrStream) {
            return cdrStream.ReadString();
        }

        internal abstract Type GetClsForTypeCode();
        

        /// <summary>
        /// returns the CLS attributes, which descirbes the type together with cls-type
        /// </summary>
        /// <returns>a collection of Attributes</returns>
        internal virtual AttributeExtCollection GetClsAttributesForTypeCode() {
            return new AttributeExtCollection();
        }

        
        /// <summary>
        /// returns the CLS attributes, which descirbes the type together with cls-type
        /// </summary>
        /// <returns>a collection of attribute-builders</returns>
        internal virtual CustomAttributeBuilder[] GetAttributes() {
            return new CustomAttributeBuilder[0];
        }
        
        /// <summary>
        /// for some type-codes, the type represented by the type-code must be created, if not present
        /// </summary>
        /// <returns>If overriden, the type created. Default is: Exception</returns>
        internal virtual Type CreateType(ModuleBuilder modBuilder, string fullTypeName) {
            throw new INTERNAL(129, CompletionStatus.Completed_MayBe);
        }

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
        public virtual object member_label (int index) {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual omg.org.CORBA.TypeCode discriminator_type() {
            throw new BAD_OPERATION(0, CompletionStatus.Completed_No);
        }
        public virtual int default_index() {
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
               
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal InterfaceTC(TCKind kind) : base(kind) {
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

        internal override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = ReadRepositoryId(encap);
            m_name = encap.ReadString();
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0, cdrStream);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            encap.WriteToTargetStream();
        }

        internal override Type GetClsForTypeCode() {
            Type result = Repository.GetTypeForId(m_id);
            if (result == null) {
                // create the type represented by this typeCode
                string typeName = Repository.GetTypeNameForId(m_id);
                result = TypeFromTypeCodeRuntimeGenerator.GetSingleton().CreateOrGetType(typeName, this);
            }
            return result;
        }

        #endregion IMethods

    }

    internal class ObjRefTC : InterfaceTC {
        
        #region IConstructors
        
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal ObjRefTC() : base(TCKind.tk_objref) {
        }     
        
        public ObjRefTC(string repositoryID, string name) : base(repositoryID, name, TCKind.tk_objref) { }

        #endregion IConstructors
    }

    internal class AbstractIfTC : InterfaceTC {
        
        #region IConstructors
       
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal AbstractIfTC() : base(TCKind.tk_abstract_interface) {
        }     
        
        public AbstractIfTC(string repositoryID, string name) : base(repositoryID, name, TCKind.tk_abstract_interface) { }        

        #endregion IConstructors
    }

    internal class LocalIfTC : InterfaceTC {
        
        #region IConstructors        
        
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal LocalIfTC() : base(TCKind.tk_local_interface) {
        }     
        
        public LocalIfTC(string repositoryID, string name) : base(repositoryID, name, TCKind.tk_local_interface) { }

        #endregion IConstructors
    }

    internal class NullTC : TypeCodeImpl {
        
        #region IConstructors
        
        public NullTC() : base(TCKind.tk_null) { }

        #endregion IConstructors
        #region IMethods
        
        internal override Type GetClsForTypeCode() {
            // this operation is not supported for a NullTC
            throw new BAD_OPERATION(478, CompletionStatus.Completed_MayBe);
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
            return ReflectionHelper.Int16Type;
        }

        #endregion IMethods
    }

    internal class LongTC : TypeCodeImpl {
        
        #region IConstructors
        
        public LongTC() : base(TCKind.tk_long) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return ReflectionHelper.Int32Type;
        }

        #endregion IMethods

    }

    internal class UShortTC : TypeCodeImpl { 
        
        #region IConstructors
        
        public UShortTC() : base(TCKind.tk_ushort) { }

        #endregion IConstructors
        #region IMethods
        
        internal override Type GetClsForTypeCode() {
            return ReflectionHelper.Int16Type;
        }

        #endregion IMethods

    }

    internal class ULongTC : TypeCodeImpl {

        #region IConstructors
        
        public ULongTC() : base(TCKind.tk_ulong) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return ReflectionHelper.Int32Type;
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
            return ReflectionHelper.BooleanType;
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
        
        internal override AttributeExtCollection GetClsAttributesForTypeCode() {
            return new AttributeExtCollection(new Attribute[] { new WideCharAttribute(false) });
        }

        internal override CustomAttributeBuilder[] GetAttributes() {
            return new CustomAttributeBuilder[] { new WideCharAttribute(false).CreateAttributeBuilder() };
        }

        #endregion IMethods

    }

    internal class OctetTC : TypeCodeImpl {
        
        #region IConstructors
        
        public OctetTC() : base(TCKind.tk_octet) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return ReflectionHelper.ByteType;
        }

        #endregion IMethods

    }
    
    /// <summary>typecode used for a typedef</summary>
    internal class AliasTC : TypeCodeImpl {

        #region IFields

        private string m_id;
        private string m_name;
        private TypeCode m_aliased;

        #endregion IFields
        #region IConstructors
        
        public AliasTC() : base(TCKind.tk_alias) { }
        
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
            return m_aliased;
        }

        internal override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = ReadRepositoryId(encap);
            m_name = encap.ReadString();
            TypeCodeSerializer ser = new TypeCodeSerializer();
            m_aliased = (TypeCode) ser.Deserialise(typeof(TypeCode), 
                                                   new AttributeExtCollection(), encap);
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0, cdrStream);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            TypeCodeSerializer ser = new TypeCodeSerializer();
            ser.Serialise(typeof(TypeCode), m_aliased, new AttributeExtCollection(), encap);
            encap.WriteToTargetStream();
        }

        internal override Type GetClsForTypeCode() {
            // resolve typedef
            return ((TypeCodeImpl)m_aliased).GetClsForTypeCode();
        }        
        
        internal override AttributeExtCollection GetClsAttributesForTypeCode() {
            // get attributes for typedefed type
            return ((TypeCodeImpl)m_aliased).GetClsAttributesForTypeCode();
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
        
        public WCharTC() : base(TCKind.tk_wchar) { }

        #endregion IConstructors
        #region IMethods
        
        internal override Type GetClsForTypeCode() {
            return typeof(System.Char);
        }
        
        internal override AttributeExtCollection GetClsAttributesForTypeCode() {
            return new AttributeExtCollection(new Attribute[] { new WideCharAttribute(true) });
        }

        internal override CustomAttributeBuilder[] GetAttributes() {
            return new CustomAttributeBuilder[] { new WideCharAttribute(true).CreateAttributeBuilder() };
        }

        #endregion IMethods
    }

    internal class StringTC : TypeCodeImpl {
        
        #region IFields

        private int m_length = 0;

        #endregion IFields
        #region IConstructors

        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal StringTC() : base(TCKind.tk_string) {
        }     

        public StringTC(int length) : base(TCKind.tk_string) {
            m_length = length;
        }

        #endregion IConstructors
        #region IMethods

        public override int length() {
            return m_length;
        }
        
        internal override void ReadFromStream(CdrInputStream cdrStream) {
            m_length = (int)cdrStream.ReadULong();
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            cdrStream.WriteULong((uint)m_length);
        }
    
        internal override Type GetClsForTypeCode() {
            return typeof(System.String);
        }
        
        internal override AttributeExtCollection GetClsAttributesForTypeCode() {
            return new AttributeExtCollection(new Attribute[] { new WideCharAttribute(false), 
                                                                new StringValueAttribute() });
        }

        internal override CustomAttributeBuilder[] GetAttributes() {
            return new CustomAttributeBuilder[] { new WideCharAttribute(false).CreateAttributeBuilder(),
                                                  new StringValueAttribute().CreateAttributeBuilder() };
        }


        #endregion IMethods

    }

    internal class WStringTC : TypeCodeImpl {
        
        #region IFields
        
        private int m_length = 0;

        #endregion IFields
        #region IConstructors
        
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal WStringTC() : base(TCKind.tk_wstring) {
        }     
        
        public WStringTC(int length) : base(TCKind.tk_wstring) {
            m_length = length;
        }

        #endregion IConstructors
        #region IMethods

        public override int length() {
            return m_length;
        }
        
        internal override void ReadFromStream(CdrInputStream cdrStream) {
            m_length = (int)cdrStream.ReadULong();
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            cdrStream.WriteULong((uint)m_length);
        }
    
        internal override Type GetClsForTypeCode() {
            return typeof(System.String);
        }
        
        internal override AttributeExtCollection GetClsAttributesForTypeCode() {
            return new AttributeExtCollection(new Attribute[] { new WideCharAttribute(true), 
                                                                new StringValueAttribute() });
        }

        internal override CustomAttributeBuilder[] GetAttributes() {
            return new CustomAttributeBuilder[] { new WideCharAttribute(true).CreateAttributeBuilder(),
                                                  new StringValueAttribute().CreateAttributeBuilder() };
        }


        #endregion IMethods

    }

    internal class LongLongTC : TypeCodeImpl {
        
        #region IConstructors
        
        public LongLongTC() : base(TCKind.tk_longlong) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return ReflectionHelper.Int64Type;
        }

        #endregion IMethods

    }

    internal class ULongLongTC : TypeCodeImpl {
        
        #region IConstructors
        
        public ULongLongTC() : base(TCKind.tk_ulonglong) { }

        #endregion IConstructors
        #region IMethods

        internal override Type GetClsForTypeCode() {
            return ReflectionHelper.Int64Type;
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
               
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal EnumTC() : base(TCKind.tk_enum) {
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

        internal override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = ReadRepositoryId(encap);
            m_name = encap.ReadString();
            uint length = encap.ReadULong();
            m_members = new string[length];
            for (int i = 0; i < length; i++) {
                m_members[i] = encap.ReadString();
            }
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0, cdrStream);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            encap.WriteULong((uint)m_members.Length);
            foreach(string member in m_members) {
                encap.WriteString(member);
            }
            encap.WriteToTargetStream();
        }

        internal override Type GetClsForTypeCode() {
            Type result = Repository.GetTypeForId(m_id);
            if (result == null) {
                // create the type represented by this typeCode
                string typeName = Repository.GetTypeNameForId(m_id);
                result = TypeFromTypeCodeRuntimeGenerator.GetSingleton().CreateOrGetType(typeName, this);
            }
            return result;
        }

        internal override Type CreateType(ModuleBuilder modBuilder, string fullTypeName) {            
            TypeBuilder result = modBuilder.DefineType(fullTypeName, 
                                                       TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                                                       typeof(System.Enum));
            result.DefineField("value__", ReflectionHelper.Int32Type, 
                               FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
        
            // add enum entries
            for (int i = 0; i < m_members.Length; i++) {
                String enumeratorId = m_members[i];
                FieldBuilder enumVal = result.DefineField(enumeratorId, result, 
                                                          FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
                enumVal.SetConstant((System.Int32) i);
            }

            // add IDLEnum attribute
            result.SetCustomAttribute(new IdlEnumAttribute().CreateAttributeBuilder());
        
            // create the type
            return result.CreateType();            
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
                       
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal ValueBoxTC() : base(TCKind.tk_value_box) {
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

        internal override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = ReadRepositoryId(encap);
            m_name = encap.ReadString();
            TypeCodeSerializer ser = new TypeCodeSerializer();
            m_boxed = (TypeCode) ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), encap);
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0, cdrStream);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            TypeCodeSerializer ser = new TypeCodeSerializer();
            ser.Serialise(typeof(TypeCode), m_boxed, new AttributeExtCollection(new Attribute[0]), encap);
            encap.WriteToTargetStream();
        }

        internal override Type GetClsForTypeCode() {
            Type result = Repository.GetTypeForId(m_id);
            if (result == null) {
                // create the type represented by this typeCode
                string typeName = Repository.GetTypeNameForId(m_id);
                result = TypeFromTypeCodeRuntimeGenerator.GetSingleton().CreateOrGetType(typeName, this);
            }
            return result;
        }

        internal override Type CreateType(ModuleBuilder modBuilder, string fullTypeName) {
            Type boxedType = ((TypeCodeImpl) m_boxed).GetClsForTypeCode();
            CustomAttributeBuilder[] attrs = ((TypeCodeImpl) m_boxed).GetAttributes();
            // create the type
            BoxedValueTypeGenerator generator = new BoxedValueTypeGenerator();
            TypeBuilder result = generator.CreateBoxedType(boxedType, modBuilder, fullTypeName, attrs);
            return result.CreateType();
        }


        #endregion IMethods

    }

    internal class SequenceTC : TypeCodeImpl {
        
        #region IFields

        private int m_length;
        private TypeCode m_seqType;

        #endregion IFields
        #region IConstructors
        
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal SequenceTC() : base(TCKind.tk_sequence) {
        }     


        public SequenceTC(TypeCode seqType, int length) : base(TCKind.tk_sequence) {
            Initalize(seqType, length);
        }

        #endregion IConstructors
        #region IMethods
        
        private void Initalize(TypeCode seqType, int length) {
            m_seqType = seqType;    
            m_length = length;            
        }

        public override int length() {
            return m_length;
        }
    
        public override TypeCode content_type() {
            return m_seqType;
        }

        internal override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            TypeCodeSerializer ser = new TypeCodeSerializer();
            m_seqType = (TypeCode) ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), encap);
            m_length = (int)encap.ReadULong();
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0, cdrStream);
            TypeCodeSerializer ser = new TypeCodeSerializer();
            ser.Serialise(typeof(TypeCode), m_seqType, new AttributeExtCollection(new Attribute[0]), encap);
            encap.WriteULong((uint)m_length);
            encap.WriteToTargetStream();
        }

        internal override Type GetClsForTypeCode() {
            Type elemType = ((TypeCodeImpl)m_seqType).GetClsForTypeCode();
            Type arrayType;
            // handle types in creation correctly
            if (elemType is TypeBuilder) {
                Module declModule = ((TypeBuilder)elemType).Module;
                arrayType = declModule.GetType(elemType.FullName + "[]"); // not nice, better solution ?
            } else {
                Assembly declAssembly = elemType.Assembly;
                arrayType = declAssembly.GetType(elemType.FullName + "[]"); // not nice, better solution ?
            }                                    
            return arrayType;
        }

        internal override AttributeExtCollection GetClsAttributesForTypeCode() {
            if (m_length == 0) {
                return new AttributeExtCollection(new Attribute[] { new IdlSequenceAttribute() });
            } else {
                return new AttributeExtCollection(new Attribute[] { new IdlSequenceAttribute(m_length) });
            }
        }

        internal override CustomAttributeBuilder[] GetAttributes() {
            if (m_length == 0) {
                return new CustomAttributeBuilder[] { new IdlSequenceAttribute().CreateAttributeBuilder() };
            } else {
                return new CustomAttributeBuilder[] { new IdlSequenceAttribute(m_length).CreateAttributeBuilder() };
            }
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
               
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal BaseStructTC(TCKind kind) : base(kind) {
        }     

        public BaseStructTC(string repositoryID, string name, StructMember[] members, TCKind kind) : base(kind) {
            Initalize(repositoryID, name, members);
        }

        #endregion IConstructors
        #region IMethods
        
        public void Initalize(string repositoryID, string name, StructMember[] members) {
            m_id = repositoryID;
            m_name = name;
            m_members = members;
            if (m_members == null) { m_members = new StructMember[0]; }            
        }
        

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

        internal override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = ReadRepositoryId(encap);
            m_name = encap.ReadString();
            uint length = encap.ReadULong();
            m_members = new StructMember[length];
            TypeCodeSerializer ser = new TypeCodeSerializer();
            for (int i = 0; i < length; i++) {
                string memberName = encap.ReadString();
                TypeCode memberType = (TypeCode)ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), encap);    
                StructMember member = new StructMember(memberName, memberType);
                m_members[i] = member;
            }
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0, cdrStream);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            encap.WriteULong((uint)m_members.Length);
            TypeCodeSerializer ser = new TypeCodeSerializer();            
            foreach(StructMember member in m_members) {
                encap.WriteString(member.m_name);
                ser.Serialise(typeof(TypeCode), member.m_type, new AttributeExtCollection(new Attribute[0]), encap);
            }
            encap.WriteToTargetStream();
        }

        internal override Type GetClsForTypeCode() {
            Type result = Repository.GetTypeForId(m_id);
            if (result == null) {
                // create the type represented by this typeCode
                string typeName = Repository.GetTypeNameForId(m_id);
                result = TypeFromTypeCodeRuntimeGenerator.GetSingleton().CreateOrGetType(typeName, this);
            }
            return result;

        }                

        #endregion IMethods

    }

    internal class StructTC : BaseStructTC {
        
        #region IConstructors
        
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal StructTC() : base(TCKind.tk_struct) {
        }     

        public StructTC(string repositoryID, string name, StructMember[] members) : base(repositoryID, name, members, TCKind.tk_struct) { }        
        
        #endregion IConstructors

    }

    internal class UnionTC : TypeCodeImpl {

        #region Types

        /// <summary>
        /// represents a switch-case in the following form:
        /// element name / type
        /// corresponding discriminator values
        /// </summary>
        struct ElementCase {
            
            #region IFields
            public string ElemName;
            public TypeContainer ElemType;
            private ArrayList discriminatorValues;
            #endregion IFields
            #region IConstrutors

            public ElementCase(string elemName, TypeContainer elemType) {
                ElemName = elemName;
                ElemType = elemType;
                discriminatorValues = new ArrayList();
            }

            public void AddDiscriminatorValue(object discValue) {
                discriminatorValues.Add(discValue);
            }

            public object[] GetDiscriminatorValues() {
                return (object[])discriminatorValues.ToArray(typeof(object));
            }

            #endregion IConstructors
        }

        #endregion Types
        #region IFields

        private string m_id;
        private string m_name;
        
        private omg.org.CORBA.TypeCode m_discriminatorType;

        private int m_defaultCase;

        private UnionSwitchCase[] m_members = new UnionSwitchCase[0];

        #endregion IFields
        #region IConstructors
                
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal UnionTC() : base(TCKind.tk_union) {
        }     

        public UnionTC(string repositoryID, string name, 
                       omg.org.CORBA.TypeCode discriminatorType, int defaultCase,
                       UnionSwitchCase[] switchCases) : base(TCKind.tk_union) {
            Initalize(repositoryID, name, discriminatorType, defaultCase,
                      switchCases);
        }

        #endregion IConstructors
        #region IMethods
        
        public void Initalize(string repositoryID, string name,
                              omg.org.CORBA.TypeCode discriminatorType, int defaultCase,
                              UnionSwitchCase[] switchCases) {
            m_id = repositoryID;
            m_name = name;
            m_discriminatorType = discriminatorType;
            m_defaultCase = defaultCase;
            m_members = switchCases;                                      
        }
                              

        [return:StringValueAttribute()]
        public override string id() {
            return m_id;
        }

        [return:StringValueAttribute()]
        public override string name() {
            return m_name;
        }

        public override object member_label (int index) {
            return m_members[index].DiscriminatorValue;
        }
        
        public override omg.org.CORBA.TypeCode discriminator_type() {
            return m_discriminatorType;
        }

        public override int default_index() {
            return m_defaultCase;
        }

        public override int member_count() {
            return m_members.Length;
        }

        [return:StringValueAttribute()]
        public override string member_name(int index) {
            return m_members[index].ElementName;
        }
        public override TypeCode member_type(int index) {
            return m_members[index].ElementType;
        }

        internal override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();
            m_id = ReadRepositoryId(encap);
            m_name = encap.ReadString();
            Marshaller marshaller = Marshaller.GetSingleton();
            TypeCodeSerializer ser = new TypeCodeSerializer();
            m_discriminatorType = (omg.org.CORBA.TypeCode)ser.Deserialise(typeof(omg.org.CORBA.TypeCode), 
                                                                          new AttributeExtCollection(new Attribute[0]), 
                                                                          encap);
            Type discrTypeCls = ((TypeCodeImpl)m_discriminatorType).GetClsForTypeCode();
            m_defaultCase = encap.ReadLong();
            
            uint length = encap.ReadULong();
            m_members = new UnionSwitchCase[length];            
            for (int i = 0; i < length; i++) {
                object discrLabel = marshaller.Unmarshal(discrTypeCls, new AttributeExtCollection(new Attribute[0]), 
                                                         encap);
                string memberName = encap.ReadString();                
                omg.org.CORBA.TypeCode memberType = (omg.org.CORBA.TypeCode)ser.Deserialise(typeof(omg.org.CORBA.TypeCode), 
                                                                                            new AttributeExtCollection(new Attribute[0]), 
                                                                                            encap);    
                UnionSwitchCase member = new UnionSwitchCase(discrLabel, memberName, memberType);
                m_members[i] = member;
            }
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            // write common part: typecode nr
            base.WriteToStream(cdrStream);
            // complex type-code: in encapsulation
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0, cdrStream);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            Marshaller marshaller = Marshaller.GetSingleton();
            TypeCodeSerializer ser = new TypeCodeSerializer();
            Type discrTypeCls = ((TypeCodeImpl)m_discriminatorType).GetClsForTypeCode();
            ser.Serialise(typeof(omg.org.CORBA.TypeCode), m_discriminatorType, 
                          new AttributeExtCollection(new Attribute[0]), encap);
            encap.WriteLong(m_defaultCase);
            
            encap.WriteULong((uint)m_members.Length);
            for (int i = 0; i < m_members.Length; i++) {
                marshaller.Marshal(discrTypeCls, new AttributeExtCollection(new Attribute[0]), 
                                   m_members[i].DiscriminatorValue, encap);
                encap.WriteString(m_members[i].ElementName);
                ser.Serialise(typeof(omg.org.CORBA.TypeCode), m_members[i].ElementType,
                             new AttributeExtCollection(new Attribute[0]), encap);
            }            

            encap.WriteToTargetStream();
        }

        internal override Type GetClsForTypeCode() {
            Type result = Repository.GetTypeForId(m_id);
            if (result == null) {
                // create the type represented by this typeCode
                string typeName = Repository.GetTypeNameForId(m_id);
                result = TypeFromTypeCodeRuntimeGenerator.GetSingleton().CreateOrGetType(typeName, this);
            }
            return result;

        }

        private ArrayList CoveredDiscriminatorRange() {
            ArrayList result = new ArrayList();
            for (int i = 0; i < m_members.Length; i++) {
                UnionSwitchCase switchCase = m_members[i];
                if (i == m_defaultCase) {
                    continue;
                }
                if (result.Contains(switchCase.DiscriminatorValue)) {
                    throw new MARSHAL(880, CompletionStatus.Completed_MayBe);
                }
                result.Add(switchCase.DiscriminatorValue);
            }
            return result;
        }

        /// <summary>
        /// collects all disriminator values, which uses the same element.
        /// </summary>
        private Hashtable CollectCases() {
            Hashtable result = new Hashtable();
            for (int i = 0; i < m_members.Length; i++) {
                if (i == m_defaultCase) {
                    if (result.Contains(m_members[i].ElementName)) {
                        throw new MARSHAL(881, CompletionStatus.Completed_MayBe);
                    }
                    ElementCase elemCase = new ElementCase(m_members[i].ElementName,
                                                           new TypeContainer(((TypeCodeImpl)m_members[i].ElementType).GetClsForTypeCode(),
                                                                             ((TypeCodeImpl)m_members[i].ElementType).GetAttributes()));
                    elemCase.AddDiscriminatorValue(UnionGenerationHelper.DefaultCaseDiscriminator);
                    result[m_members[i].ElementName] = elemCase;
                } else {
                    if (result.Contains(m_members[i].ElementName)) {
                        ElementCase current = (ElementCase) result[m_members[i].ElementName];
                        current.AddDiscriminatorValue(m_members[i].DiscriminatorValue);
                    } else {
                        ElementCase elemCase = new ElementCase(m_members[i].ElementName,
                                                               new TypeContainer(((TypeCodeImpl)m_members[i].ElementType).GetClsForTypeCode(),
                                                                                 ((TypeCodeImpl)m_members[i].ElementType).GetAttributes()));
                        elemCase.AddDiscriminatorValue(m_members[i].DiscriminatorValue);
                        result[m_members[i].ElementName] = elemCase;
                    }
                }
            }
            return result;
        }

        internal override Type CreateType(ModuleBuilder modBuilder, string fullTypeName) {
            UnionGenerationHelper genHelper = new UnionGenerationHelper(modBuilder, fullTypeName, 
                                                                        TypeAttributes.Public);
            
            TypeContainer discrType = new TypeContainer(((TypeCodeImpl)m_discriminatorType).GetClsForTypeCode(), 
                                                        ((TypeCodeImpl)m_discriminatorType).GetAttributes());
            // extract covered discr range from m_members
            ArrayList coveredDiscriminatorRange = CoveredDiscriminatorRange();             
            genHelper.AddDiscriminatorFieldAndProperty(discrType, coveredDiscriminatorRange);
            
            Hashtable cases = CollectCases();
            foreach (ElementCase elemCase in cases.Values) {                                
                genHelper.GenerateSwitchCase(elemCase.ElemType, elemCase.ElemName, 
                                             elemCase.GetDiscriminatorValues());                
            }                       
            
            // create the resulting type
            return genHelper.FinalizeType();
        }

        #endregion IMethods

    }

    internal class ExceptTC : BaseStructTC {

        #region IConstructors
        
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal ExceptTC() : base(TCKind.tk_except) {
        }
        
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
               
        /// <summary>constructor used during deserialisation; call ReadFromStream afterwards</summary>
        internal ValueTypeTC() : base(TCKind.tk_value) {
        }     

        public ValueTypeTC(string repositoryID, string name, ValueTypeMember[] members, TypeCode baseClass, short typeMod) : base(TCKind.tk_value) {
            Initalize(repositoryID, name, members, baseClass, typeMod);
        }

        #endregion IConstructors
        #region IMethods
        
        public void Initalize(string repositoryID, string name, ValueTypeMember[] members, TypeCode baseClass, short typeMod) {
            m_id = repositoryID;
            m_name = name;
            m_members = members;
            if (m_members == null) { m_members = new ValueTypeMember[0]; }
            m_baseClass = baseClass;
            m_typeMod = typeMod;            
        }
        

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


        internal override void ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();    
            m_id = ReadRepositoryId(encap);
            m_name = encap.ReadString();
            m_typeMod = encap.ReadShort();
            TypeCodeSerializer ser = new TypeCodeSerializer();
            m_baseClass = (TypeCode)ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), encap);    
            // deser members
            uint length = encap.ReadULong();
            m_members = new ValueTypeMember[length];
            for (int i = 0; i < length; i++) {
                string memberName = encap.ReadString();
                TypeCode memberType = (TypeCode)ser.Deserialise(typeof(TypeCode), new AttributeExtCollection(new Attribute[0]), encap);    
                short visibility = encap.ReadShort();
                ValueTypeMember member = new ValueTypeMember(memberName, memberType, visibility);
                m_members[i] = member;
            }
        }

        internal override void WriteToStream(CdrOutputStream cdrStream) {
            base.WriteToStream(cdrStream);
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0, cdrStream);
            encap.WriteString(m_id);
            encap.WriteString(m_name);
            encap.WriteShort(m_typeMod);
            TypeCodeSerializer ser = new TypeCodeSerializer();
            // ser baseclass type
            ser.Serialise(typeof(TypeCode), m_baseClass, new AttributeExtCollection(new Attribute[0]), encap);            
            // ser members
            encap.WriteULong((uint)m_members.Length);
            foreach(ValueTypeMember member in m_members) {
                encap.WriteString(member.m_name);
                ser.Serialise(typeof(TypeCode), member.m_type, new AttributeExtCollection(new Attribute[0]), encap);
                encap.WriteShort(member.m_visibility);
            }
            encap.WriteToTargetStream();
        }

        internal override Type GetClsForTypeCode() {
            Type result = Repository.GetTypeForId(m_id);
            if (result == null) {
                // create the type represented by this typeCode
                string typeName = Repository.GetTypeNameForId(m_id);
                result = TypeFromTypeCodeRuntimeGenerator.GetSingleton().CreateOrGetType(typeName, this);
            }
            return result;
        }

        internal override Type CreateType(ModuleBuilder modBuilder, string fullTypeName) {
            Type baseType = typeof(object);
            if (!(m_baseClass is NullTC)) {
                baseType = ((TypeCodeImpl)m_baseClass).GetClsForTypeCode();
            }
            string typeName = Repository.GetTypeNameForId(m_id);
            TypeAttributes attrs = TypeAttributes.Class | TypeAttributes.Serializable;
            attrs = attrs | TypeAttributes.Public;
            TypeBuilder result = modBuilder.DefineType(typeName, attrs ,baseType);
            // add rep-id Attr
            RepositoryIDAttribute repIdAttr = new RepositoryIDAttribute(m_id);
            result.SetCustomAttribute(repIdAttr.CreateAttributeBuilder());
            // define members
            foreach (ValueTypeMember member in m_members) {
                Type memberType = ((TypeCodeImpl) (member.m_type)).GetClsForTypeCode();                
                FieldAttributes fieldAttrs = FieldAttributes.Public;
                FieldBuilder field = result.DefineField(member.m_name, memberType, fieldAttrs);
                CustomAttributeBuilder[] cAttrs = ((TypeCodeImpl) (member.m_type)).GetAttributes();
                foreach (CustomAttributeBuilder cAttr in cAttrs) {
                    field.SetCustomAttribute(cAttr);
                }
            }
            // create the type
            return result.CreateType();
        }

        #endregion IMethods

    }
    
}

namespace Ch.Elca.Iiop.Marshalling {
    

    /// <summary>serializes a typecode</summary>
    internal class TypeCodeSerializer : Serialiser {
        
        #region IMethods

        public override object Deserialise(System.Type formal, AttributeExtCollection attributes, CdrInputStream sourceStream) {
            // indirPos will contain the next aligned position in the base stream, after next read-op is performed
            StreamPosition indirPos = new StreamPosition(sourceStream);
            
            uint kindVal = (uint)sourceStream.ReadULong();
            if (kindVal != CdrStreamHelper.INDIRECTION_TAG) {
            
                omg.org.CORBA.TCKind kind = (omg.org.CORBA.TCKind)Enum.ToObject(typeof(omg.org.CORBA.TCKind),
                                                                                (int)kindVal);
                omg.org.CORBA.TypeCodeImpl result;
                switch(kind) {
                    case omg.org.CORBA.TCKind.tk_abstract_interface :
                        result = new omg.org.CORBA.AbstractIfTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_alias:
                        result = new omg.org.CORBA.AliasTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_any:
                        result = new omg.org.CORBA.AnyTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_array:
                        throw new NotImplementedException("array not implemented");
                    case omg.org.CORBA.TCKind.tk_boolean:
                        result = new omg.org.CORBA.BooleanTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_char:
                        result = new omg.org.CORBA.CharTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_double:
                        result = new omg.org.CORBA.DoubleTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_enum:
                        result = new omg.org.CORBA.EnumTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_except:
                        result = new omg.org.CORBA.ExceptTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_fixed:
                        throw new NotImplementedException("fixed not implemented");
                    case omg.org.CORBA.TCKind.tk_float:
                        result = new omg.org.CORBA.FloatTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_local_interface :
                        result = new omg.org.CORBA.LocalIfTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_long:
                        result = new omg.org.CORBA.LongTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_longdouble:
                        throw new NotImplementedException("long double not implemented");
                    case omg.org.CORBA.TCKind.tk_longlong:
                        result = new omg.org.CORBA.LongLongTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_native:
                        throw new NotSupportedException("native not supported");
                    case omg.org.CORBA.TCKind.tk_null:
                        result = new omg.org.CORBA.NullTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_objref:
                        result = new omg.org.CORBA.ObjRefTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_octet:
                        result = new omg.org.CORBA.OctetTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_Principal:
                        throw new NotImplementedException("Principal not implemented");
                    case omg.org.CORBA.TCKind.tk_sequence:
                        result = new omg.org.CORBA.SequenceTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_short:
                        result = new omg.org.CORBA.ShortTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_string:
                        result = new omg.org.CORBA.StringTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_struct:
                        result = new omg.org.CORBA.StructTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_TypeCode:
                        result = new omg.org.CORBA.TypeCodeTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_ulong:
                        result = new omg.org.CORBA.ULongTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_ulonglong:
                        result = new omg.org.CORBA.ULongLongTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_union:
                        result = new omg.org.CORBA.UnionTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_ushort:
                        result = new omg.org.CORBA.UShortTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_value:
                        result = new omg.org.CORBA.ValueTypeTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_value_box:
                        result = new omg.org.CORBA.ValueBoxTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_void:
                        result = new omg.org.CORBA.VoidTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_wchar:
                        result = new omg.org.CORBA.WCharTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_wstring:
                        result = new omg.org.CORBA.WStringTC();                                    
                        break;
                    default:
                        // unknown typecode: kind
                        throw new omg.org.CORBA.BAD_PARAM(1504, 
                                                          omg.org.CORBA.CompletionStatus.Completed_MayBe);
                }
                // store indirection
                IndirectionInfo indirInfo = new IndirectionInfo(indirPos.Position, 
                                                                IndirectionType.TypeCode,
                                                                IndirectionUsage.TypeCode);
                sourceStream.StoreIndirection(indirInfo, result);
                // read additional parts of typecode, if present
                result.ReadFromStream(sourceStream);                                
                return result;
            } else {
                // resolve indirection:
                long indirectionOffset = sourceStream.ReadLong();
                sourceStream.CheckIndirectionResolvable(indirectionOffset, true,
                                                        IndirectionType.TypeCode,
                                                        IndirectionUsage.TypeCode);
                return sourceStream.GetObjectForIndir(indirectionOffset, true,
                                                      IndirectionType.TypeCode,
                                                      IndirectionUsage.TypeCode);
            }
        }

        public override void Serialise(System.Type formal, object actual, AttributeExtCollection attributes, CdrOutputStream targetStream) {
            if (!(actual is omg.org.CORBA.TypeCodeImpl)) { 
                // typecode not serializable
                throw new omg.org.CORBA.INTERNAL(1654, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            omg.org.CORBA.TypeCodeImpl tcImpl = actual as omg.org.CORBA.TypeCodeImpl;
            if (!targetStream.IsPreviouslyMarshalled(tcImpl, IndirectionType.TypeCode, IndirectionUsage.TypeCode)) {
                tcImpl.WriteToStream(targetStream);
            } else {
                targetStream.WriteIndirection(tcImpl);
            }
        }

        #endregion IMethods

    }



}
