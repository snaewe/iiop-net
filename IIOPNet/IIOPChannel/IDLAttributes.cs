/* IDLAttributes.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dul@elca.ch
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

namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// enumeration of the IDL-types, which are mapped to a .NET interface
    /// </summary>
    public enum IdlTypeInterface {
        ConcreteInterface,
        AbstractInterface,
        AbstractValueType
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
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { typeof(string) } );
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
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { typeof(Type) } );
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
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { typeof(string) } );
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
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { typeof(string) } );
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
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Field | AttributeTargets.Property, 
                    AllowMultiple = false)]
    public sealed class IdlSequenceAttribute : Attribute, IIdlAttribute {
        
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
            ConstructorInfo attrConstr = attrType.GetConstructor(new Type[] { typeof(bool) } );
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

}
