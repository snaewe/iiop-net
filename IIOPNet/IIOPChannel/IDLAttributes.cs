/* IDLAttributes.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// enumeration of the IDL-types, which are mapped to a .NET interface
    /// </summary>
    public enum IdlTypeInterface {
        ConcreteInterface,
        AbstractInterface,
        LocalInterface,
        AbstractValueType,
    }


    /// <summary>
    /// enumeration of the IDL-types, which are mapped to object
    /// </summary>
    public enum IdlTypeObject {
        Any,
        AbstractBase,
        ValueBase
    }


    /// <summary>
    /// marker interface to mark IDL to CLS mapped types
    /// </summary>
    public interface IIdlEntity {
    }

    
    public interface IIdlAttribute {
        
        #region IMethods
        
        CustomAttributeBuilder CreateAttributeBuilder();
        
        #endregion IMethods

    }
    
    /// <summary>
    /// for attributes, which can be present multiple times on the same construct,
    /// this number specifies, in which order the attributes should be considered
    /// by the serialiser / deserialiser.
    /// </summary>
    public interface IOrderedAttribute {
        
        #region IProperties
        
        /// <summary>
        /// the number in the ordered collection of these attributes.
        /// </summary>
        long OrderNr {
            get;
        }
        
        #endregion IProperties
        
    }


    /// <summary>implemeneted by attributes, which are associated to another attribute</summary>
    public interface IAssociatedAttribute {

        #region IProperties
        
        /// <summary>
        /// the key number of the attribute, this one is associated to.
        /// </summary>
        long AssociatedToAttributeWithKey {
            get;
        }        
        
        #endregion IProperties        

    }


    /// <summary>
    /// this attribute specifies the repository id used in the IDL.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum, 
                    AllowMultiple = false)]
    public sealed class RepositoryIDAttribute : Attribute, IIdlAttribute {
        
        #region IFields
        
        private string m_id;

        #endregion
        #region IConstructors

        public RepositoryIDAttribute(string id) {
            m_id = id;
        }

        #endregion IConstructors
        #region IProperties

        public string Id {
            get { 
                return m_id; 
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.StringType } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[] { m_id });
            return result;
        }

        #endregion IMethods

    }


    /// <summary>
    /// this attribute specifies that the repository id should be taken from the specified type instead of this type.
    /// </summary>
    /// <remarks>
    /// do not combine this attribute with RepositoryIDAttribute
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SupportedInterfaceAttribute : Attribute, IIdlAttribute {
        
        #region IFields
        
        private Type m_type;
        
        #endregion IFields
        #region IConstructors
       
        public SupportedInterfaceAttribute(Type type) {
            m_type = type;
        }

        #endregion IConstructors
        #region IProperties
        
        public Type FromType {
            get { 
                return m_type; 
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.TypeType } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[] { m_type });
            return result;
        }

        #endregion IMethods

    }


    /// <summary>
    /// this attribute is used to specify an implementation class for a value type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ImplClassAttribute : Attribute, IIdlAttribute {
        
        #region IFields
        
        private string m_implClass;
        
        #endregion IFields
        #region IConstructors
        
        public ImplClassAttribute(string implClass) {
            m_implClass = implClass;    
        }

        #endregion IConstructors
        #region IProperties

        public string ImplClass {
            get { 
                return m_implClass; 
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.StringType } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[] { m_implClass });
            return result;
        }

        #endregion IMethods
    }

    /// <summary>
    /// this attribute is used to specify, that a struct is mapped from the IDL-struct type
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class IdlStructAttribute : Attribute, IIdlAttribute {
        
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[0]);
            return result;
        }

        #endregion IMethods

    }

    /// <summary>
    /// this attribute is used to specify, that an enum is mapped from the IDL-enum type
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public sealed class IdlEnumAttribute : Attribute, IIdlAttribute {
        
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[0]);
            return result;
        }

        #endregion IMethods

    }
    
    /// <summary>
    /// this attribute is used to specify, that a struct is mapped from the IDL-union type
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class IdlUnionAttribute : Attribute, IIdlAttribute {
        
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[0]);
            return result;
        }

        #endregion IMethods

    }
    
    /// <summary>
    /// this attribute is used to indicate a mapping from an IDL boxed value type
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field | AttributeTargets.Property, 
                    AllowMultiple = false)]
    public sealed class BoxedValueAttribute : Attribute, IIdlAttribute {
        
        #region IFields
        
        private string m_repositoryId = null;
        
        #endregion IFields
        #region IConstructors
        
        /// <summary>
        /// Associate the CLS type with the outermost boxed value type, the mapping is done from
        /// </summary>
        /// <param name="repositoryId">the repository id of the outermost boxed value type</param>
        public BoxedValueAttribute(string repositoryId) {
            m_repositoryId = repositoryId;
        }

        #endregion IConstructors
        #region IProperties
        
        public string RepositoryId {
            get { 
                return m_repositoryId; 
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.StringType } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, 
                                                                       new Object[] { m_repositoryId });
            return result;
        }

        #endregion IMethods

    }

    /// <summary>
    /// this attribute is used to indicate a mapping from an IDL sequence type
    /// </summary>
    /// <remarks>
    /// IDL-sequences are mapped to .NET arrays. Because .NET arrays are not mapped to sequences, but instead
    /// to boxed value types, this attribute is used to distingish these cases.
    /// Together with the sequence attribute for the sequence, all attributes for the element type are also added.
    /// For sequences of sequences, this means, that a sequence attribute is added for the sequence itself and also
    /// for the inner sequence.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field | AttributeTargets.Property, 
                    AllowMultiple = true)]
    public sealed class IdlSequenceAttribute : Attribute, IIdlAttribute, IOrderedAttribute {
        
        #region IFields

        /// <summary>
        /// for bounded sequences > 0 (max number of elements), for unbounded = 0
        /// </summary>
        private long m_bound = 0;
        
        private long m_orderNr;

        #endregion IFields
        #region IConsturctors

        /// <summary>
        /// Constructor for unbounded sequences
        /// </summary>
        public IdlSequenceAttribute(long orderNr) : this(orderNr, 0) {            
        }

        /// <summary>
        /// constructor for bounded sequences
        /// </summary>
        /// <param name="bound">max nr of elements</param>
        public IdlSequenceAttribute(long orderNr, long bound) {
            m_bound = bound;
            m_orderNr = orderNr;
        }

        #endregion IConstructors
        #region IProperties

        public long Bound {
            get {
                return m_bound;
            }
        }
        
        public long OrderNr {
            get {
                return m_orderNr;
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            if (!IsBounded()) {
                ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.Int64Type });
                return new CustomAttributeBuilder(attrConstr, new Object[] { m_orderNr });
            } else {
                ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.Int64Type, 
                                                                                  ReflectionHelper.Int64Type });
                return new CustomAttributeBuilder(attrConstr, new Object[] { m_orderNr, m_bound } );
            }
        }

        /// <summary>
        /// is the sequence bounded or not
        /// </summary>
        /// <returns>bounded or not</returns>
        public bool IsBounded() {
            return IsBounded(m_bound);
        }

        #endregion IMethods
        #region SMethods
        
        public static long DetermineSequenceAttributeOrderNr(AttributeExtCollection elemTypeAttributes) {            
            Attribute idlorderAttr = elemTypeAttributes.GetHighestOrderAttribute();
            if (idlorderAttr != null) {
                return ((IOrderedAttribute)idlorderAttr).OrderNr + 1;
            } else {
                return 0;
            }
        }

        /// <summary>
        /// checks, if the bound does limit the sequence size or not; returns false for unbounded seqeuences
        /// </summary>
        public static bool IsBounded(long bound) {
            return bound > 0;
        }
        
        #endregion SMethods

    }


    /// <summary>
    /// this attribute is used to indicate a mapping from an IDL array type (fixed size)
    /// </summary>
    /// <remarks>
    /// IDL-arrays are mapped to .NET arrays. Because .NET arrays are not mapped to idl arrays, but instead
    /// to boxed value types, this attribute is used to distingish these cases.
    /// Together with the array attribute for the array, all attributes for the element type are also added.
    /// For array of arrays, this means, that a array attribute is added for the array itself and also
    /// for the inner array.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field | AttributeTargets.Property, 
                    AllowMultiple = true)]
    public sealed class IdlArrayAttribute : Attribute, IIdlAttribute, IOrderedAttribute {
        
        #region IFields
        /// <summary>
        /// the first array dimension
        /// </summary>
        private int m_firstDimensionSize;
        
        private long m_orderNr;        

        #endregion IFields
        #region IConsturctors

        /// <summary>
        /// constructor taking the order nr and the first dimension of the array
        /// </summary>
        /// <param name="firstDimension">the size of the first dimension of the array</param>
        public IdlArrayAttribute(long orderNr, int firstDimensionSize) {
            m_firstDimensionSize = firstDimensionSize;        
            m_orderNr = orderNr;
        }

        #endregion IConstructors
        #region IProperties

        public int FirstDimensionSize {
            get {
                return m_firstDimensionSize;
            }
        }        
        
        public long OrderNr {
            get {
                return m_orderNr;
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.Int64Type,
                                                                              ReflectionHelper.Int32Type });
            return new CustomAttributeBuilder(attrConstr, new Object[] { m_orderNr, m_firstDimensionSize } );
        }

        #endregion IMethods
        #region SMethods
        
        public static long DetermineArrayAttributeOrderNr(AttributeExtCollection elemTypeAttributes) {            
            Attribute idlorderAttr = elemTypeAttributes.GetHighestOrderAttribute();
            if (idlorderAttr != null) {
                return ((IOrderedAttribute)idlorderAttr).OrderNr + 1;
            } else {
                return 0;
            }
        }        
        
        #endregion SMethods

    }

    /// <summary>
    /// this attribute is used to provide the size of a dimension for a fixed size idl array
    /// </summary>
    /// <remarks>
    /// For multi dimension idl arrays, the higher dimension size can't be directly added to IdlArrayAttribute, 
    /// because the constructor of .NET attributes may not take an array for CLS Compliance.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field | AttributeTargets.Property, 
                    AllowMultiple = true)]
    public sealed class IdlArrayDimensionAttribute : Attribute, IIdlAttribute, IAssociatedAttribute {

        #region IFields

        private long m_associatedTo;
        private int m_dimensionSize;
        private int m_dimensionNr;

        #endregion IFields
        #region IConstructors

        public IdlArrayDimensionAttribute(long associatedTo, int dimensionNr, int dimensionSize) {
            m_associatedTo = associatedTo;
            m_dimensionNr = dimensionNr;
            m_dimensionSize = dimensionSize;
        }

        #endregion IConstructors
        #region IProperties
        
        /// <summary>
        /// the key number of the attribute, this one is associated to.
        /// </summary>
        public long AssociatedToAttributeWithKey {
            get {
                return m_associatedTo;
            }
        }    

        /// <summary>
        /// the dimension, this attribute describes
        /// </summary>
        public int DimensionNr {
            get {
                return m_dimensionNr;
            }
        }

        /// <summary>
        /// the size of this dimension.
        /// </summary>
        public int DimensionSize {
            get {
                return m_dimensionSize;
            }
        }    
        
        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.Int64Type,
                                                                              ReflectionHelper.Int32Type,
                                                                              ReflectionHelper.Int32Type });
            return new CustomAttributeBuilder(attrConstr, new Object[] { m_associatedTo, m_dimensionNr, m_dimensionSize } );
        }

        #endregion IMethods    

    }


    /// <summary>
    /// this attribute is used to describe the IDL-type from which the .NET interface is mapped from
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class InterfaceTypeAttribute : Attribute, IIdlAttribute {
        
        #region IFields
        
        private IdlTypeInterface m_idlType;

        #endregion IFields
        #region IConstructors
        
        public InterfaceTypeAttribute(IdlTypeInterface idlType) {
            m_idlType = idlType;
        }

        #endregion IConstructors
        #region IProperties

        public IdlTypeInterface IdlType {
            get { 
                return m_idlType; 
            }
        }
        
        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { typeof(IdlTypeInterface) } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[] { m_idlType });
            return result;
        }

        #endregion IMethods

    }
    

    /// <summary>
    /// this attribute is used to describe the IDL-type from which a parameter, field, retval of type object is mapped from
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field | AttributeTargets.Property, 
                    AllowMultiple = false)]
    public sealed class ObjectIdlTypeAttribute : Attribute, IIdlAttribute {
        
        #region IFields
        
        private IdlTypeObject m_idlType;

        #endregion IFields
        #region IConstructors
        
        public ObjectIdlTypeAttribute(IdlTypeObject idlType) {
            m_idlType = idlType;
        }

        #endregion IConstructors
        #region IProperties
        
        public IdlTypeObject IdlType {
            get { 
                return m_idlType; 
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { typeof(IdlTypeObject) } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[] { m_idlType });
            return result;
        }

        #endregion IMethods

    }


    /// <summary>
    /// are wide characters allowed.
    /// </summary>
    /// <remarks>
    /// IDL-type string and wstring are both mapped to System.String, containing wide chars. -->
    /// wide characters are not allowed for IDL-type string.
    /// IDL-type char and wchar are both mapped to System.Char, which is a wide char -->
    /// wide-chars are not alloewed for IDL-type char.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field | AttributeTargets.Property, 
                    AllowMultiple = false)]
    public sealed class WideCharAttribute : Attribute, IIdlAttribute {
        
        #region IFields

        private bool m_isAllowed;

        #endregion IFields
        #region IConstructors
        
        public WideCharAttribute(bool isAllowed) {
            m_isAllowed = isAllowed;
        }

        #endregion IConstructors
        #region IProperties
        
        public bool IsAllowed {
            get { return m_isAllowed; }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.BooleanType } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[] { m_isAllowed });
            return result;
        }

        #endregion IMethods

    }


    /// <summary>
    /// specifies, if string should be handled as primitive type instead of reference type.
    /// In .NET String is not a value-type. In IDL it is a primitive value type. Therefore the value null
    /// for a .NET string variable could not be mapped. Therefore the normal mapping uses a boxed value type,
    /// boxing the string value.
    /// For preventing this standard mapping (is needed if mapping from IDL-string / IDL-wstring to .NET), this
    /// attribute is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field | AttributeTargets.Property, 
                    AllowMultiple = false)]
    public sealed class StringValueAttribute : Attribute, IIdlAttribute {

        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, 
                                                                       new Object[0]);
            return result;
        }

        #endregion IMethods
    }

    
    /// <summary>
    /// this attribute specifies the name of the idl entity mapped to the idl entity;
    /// this is used currently for properties/methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, 
                    AllowMultiple = false)]
    public sealed class FromIdlNameAttribute : Attribute, IIdlAttribute {
        
        #region IFields
        
        private string m_idlName;

        #endregion
        #region IConstructors

        public FromIdlNameAttribute(string idlName) {
            m_idlName = idlName;
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// the name of the idl entity, the cls entity is mapped from
        /// </summary>
        public string IdlName {
            get { 
                return m_idlName; 
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.StringType } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[] { m_idlName });
            return result;
        }

        #endregion IMethods

    }
    
    /// <summary>
    /// this attribute specifies what exceptions may be thrown by a method; 
    /// not usable for properties, because properties are mapped to idl attributes and attributes can
    /// only return system exceptions. (CORBA 2.6; chapter 3.13)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, 
                    AllowMultiple = true)]
    public sealed class ThrowsIdlExceptionAttribute : Attribute, IIdlAttribute {
        
        #region IFields
        
        private Type m_exceptionType;

        #endregion
        #region IConstructors

        public ThrowsIdlExceptionAttribute(Type exceptionType) {
            m_exceptionType = exceptionType;
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// the type of the exception, which may be thrown
        /// </summary>
        public Type ExceptionType {
            get { 
                return m_exceptionType;
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>creates an attribute builder for this custom attribute</summary>
        public CustomAttributeBuilder CreateAttributeBuilder() {
            Type attrType = this.GetType();
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { ReflectionHelper.TypeType } );
            CustomAttributeBuilder result = new CustomAttributeBuilder(attrConstr, new Object[] { m_exceptionType });
            return result;
        }

        #endregion IMethods

    }

}
