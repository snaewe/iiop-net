/* DotNetToIDLMapper.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 30.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.ComponentModel;
using System.Diagnostics;
using Ch.Elca.Iiop.Util;
using Corba;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Idl {

    
    /// <summary>
    /// the actions which can be taken, when the CLS type is mapped to the specified IDL-construct
    /// </summary>
    public interface MappingAction {

        #region IMethods

        /// <summary>the CLS-type is mapped to an IDL struct</summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlStruct(Type clsType);

        /// <summary>the CLS-type is mapped to an IDL union</summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlUnion(Type clsType);

        /// <summary>the CLS-type is mapped to an IDL abstract interface</summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlAbstractInterface(Type clsType);

        /// <summary>the CLS-type is mapped to an IDL concrete interface</summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlConcreteInterface(Type clsNetType);
        
        /// <summary>the CLS-type is mapped to an IDL concrete interface</summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlLocalInterface(Type clsNetType);

        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlConcreateValueType(Type clsType);

        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlAbstractValueType(Type clsType);

        /// <returns>an optional result of the mapping, null may be possible</returns>
        /// <param name="isAlreadyBoxed">tells, if the dotNetType is boxed in a boxed value type, or if a native Boxed value type is mapped</param>
        object MapToIdlBoxedValueType(Type clsType, AttributeExtCollection attributes, bool isAlreadyBoxed);

        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlSequence(Type clsType);

        /// <summary>map to the IDL-type any</summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlAny(Type clsType);

        /// <summary>map to the CORBA standard type CORBA::AbstractBase</summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToAbstractBase(Type clsType);

        /// <summary>map to the CORBA standard type CORBA::ValueBase</summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToValueBase(Type clsType);
        
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapException(Type clsType);
        
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlEnum(Type clsType);

        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToWStringValue(Type clsType);

        object MapToStringValue(Type clsType);

        /// <summary>map to the special type TypeDesc, which is defined for mapping System.Type</summary>
        object MapToTypeDesc(Type clsType);

        /// <summary>map to the special type CORBA::TypeCode, which is defined for mapping CORBA::TypeCodeImpl</summary>
        object MapToTypeCode(Type clsType);

        #region basetypes
        /// <returns>an optional result of the mapping, null may be possible</returns>        
        object MapToIdlBoolean(Type clsType);
        
        object MapToIdlFloat(Type clsType);

        object MapToIdlDouble(Type clsType);

        object MapToIdlShort(Type clsType);

        object MapToIdlUShort(Type clsType);

        object MapToIdlLong(Type clsType);

        object MapToIdlULong(Type clsType);
        
        object MapToIdlLongLong(Type clsType);

        object MapToIdlULongLong(Type clsType);

        object MapToIdlOctet(Type clsType);

        object MapToIdlVoid(Type clsType);
        
        object MapToIdlWChar(Type clsType);
        
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlWString(Type clsType);

        object MapToIdlChar(Type clsType);
        
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlString(Type clsType);

        #endregion basetypes

        #endregion IMethods

    }
    
    /// <summary>
    /// This class contains the mapping information for mapping CLS to IDL.
    /// </summary>
    public class ClsToIdlMapper {

        #region SFields

        /// <summary>
        /// the singleton instance of the CLSToIDLMapper
        /// </summary>
        private static ClsToIdlMapper s_singleton = new ClsToIdlMapper();
        private static CheckMappedToAbstractVlaue s_abstrValueMapHelper = new CheckMappedToAbstractVlaue();


        // the following expressions are evaluated here for efficiency reasons
        private static Type s_int16Type = typeof(System.Int16);
        private static Type s_int32Type = typeof(System.Int32);
        private static Type s_int64Type = typeof(System.Int64);
        private static Type s_byteType = typeof(System.Byte);
        private static Type s_booleanType = typeof(System.Boolean);
        private static Type s_voidType = typeof(void);
        private static Type s_singleType = typeof(System.Single);
        private static Type s_doubleType = typeof(System.Double);
        private static Type s_charType = typeof(System.Char);
        private static Type s_stringType = typeof(System.String);
        private static Type s_objectType = typeof(System.Object);
        private static Type s_exceptType = typeof(System.Exception);
        private static Type s_typeType = typeof(System.Type);

        private static Type s_mByValComponentType = typeof(MarshalByValueComponent);

        private static Type s_boxedValBaseType = typeof(BoxedValueBase);

        private static Type s_idlStructAttrType = typeof(IdlStructAttribute);
        private static Type s_idlUnionAttrType = typeof(IdlUnionAttribute);
        private static Type s_idlSequenceAttrType = typeof(IdlSequenceAttribute);
        private static Type s_boxedValAttrType = typeof(BoxedValueAttribute);
        private static Type s_widecharAttrType = typeof(WideCharAttribute);
        private static Type s_stringValueAttrType = typeof(StringValueAttribute);
        private static Type s_objectIdlTypeAttrType = typeof(ObjectIdlTypeAttribute);
        private static Type s_interfaceTypeAttrType = typeof(InterfaceTypeAttribute);

        private static Type s_idlEntityType = typeof(IIdlEntity);

        private static Type s_corbaTypeCodeImplType = typeof(TypeCodeImpl);
        private static Type s_corbaTypeCodeType = typeof(omg.org.CORBA.TypeCode);

        #endregion SFields
        #region IConstructors

        private ClsToIdlMapper() {
        }

        #endregion IConstructors
        #region SMethods

        /// <summary>returns the singleton instance</summary>
        public static ClsToIdlMapper GetSingleton() {
            return s_singleton;
        }

        /// <summary>
        /// checks if the CLS type is an interface
        /// </summary>
        public static bool IsInterface(Type type) {
            return type.IsInterface;
        }

        /// <summary>
        /// checks, if the type is MarshalByRefObject or a subclass
        /// </summary>
        public static bool IsMarshalByRef(Type type) {
            return type.IsMarshalByRef;
        }

        /// <summary>
        /// checks, if the type is an enumeration
        /// </summary>
        public static bool IsEnum(Type type) {
            return (type.IsEnum);
        }

        /// <summary>
        /// checks, if the type is a CLS-array
        /// </summary>
        public static bool IsArray(Type type) {
            return (type.IsArray);
        }

        /// <summary>
        /// Checks, if the CLS type is mapped to an IDL value type.
        /// </summary>
        public static bool IsDefaultMarshalByVal(Type type) {
            if (type.IsPrimitive) { 
                return false; 
            } // for primitive types a dedicated serializer exists, if it is serializable
            if (IsMarshalledAsStruct(type)) { 
                return false; 
            } // marshalled as IDL-struct, no CORBA-value type
            if (IsEnum(type)) { 
                return false; 
            } // enums are handled specially
            if (IsArray(type)) { 
                return false; 
            } // arrays are handled specially
            if ((!(s_idlEntityType.IsAssignableFrom(type))) && (type.IsAbstract)) { 
                return false; 
            } // abstract native CLS types do not belong to the defaultMarhsalByVal Types; this is no criteria for types created by the IDL to CLS compiler
            return ((type.IsSerializable) || 
                    (type.IsSubclassOf(s_mByValComponentType)) ||
                     type.Equals(s_mByValComponentType));
        }

        /// <summary>
        /// checks if the CLS type belongs to the mappable primitive types
        /// </summary>
        public static bool IsMappablePrimitiveType(Type clsType) {
            if (clsType.Equals(s_int16Type) ||
                clsType.Equals(s_int32Type) ||
                clsType.Equals(s_int64Type) ||
                clsType.Equals(s_byteType) ||
                clsType.Equals(s_booleanType) ||
                clsType.Equals(s_voidType) ||
                clsType.Equals(s_singleType) ||
                clsType.Equals(s_doubleType) ||
                clsType.Equals(s_charType) ||
                clsType.Equals(s_stringType)) { 
                return true; 
            } else {
                return false;
            }
        }

        /// <summary>determines, if the CLS-type is mapped to an IDL-struct</summary>
        public static bool IsMarshalledAsStruct(Type clsType) {
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(
                                               clsType.GetCustomAttributes(true));
            if (attrs.IsInCollection(s_idlStructAttrType)) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>determines, if the CLS-type is mapped to an IDL-union</summary>
        public static bool IsMarshalledAsUnion(Type clsType) {
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(
                clsType.GetCustomAttributes(true));
            if (attrs.IsInCollection(s_idlUnionAttrType)) {
                return true;
            } else {
                return false;
            }
        }

        
        /// <summary>determines, if a CLS Type is mapped to an IDL abstract value type</summary>
        public static bool IsMappedToAbstractValueType(Type clsType, AttributeExtCollection attributes) {
            return (bool) s_singleton.MapClsType(clsType, attributes, s_abstrValueMapHelper);
        }

        /// <summary>checks, if the type is unmappable</summary>
        public static bool UnmappableType(Type clsType) {
            if (clsType.Equals(typeof(System.IntPtr)) || clsType.Equals(typeof(System.UInt16)) ||
                clsType.Equals(typeof(System.UInt32)) || clsType.Equals(typeof(System.UInt64)) ||
                clsType.Equals(typeof(System.UIntPtr))) {
                return true; 
            }
            return false;
        }

        #endregion SMethods
        #region IMethods
        
        /// <summary>uses MappingAction action while mapping a CLS-type to an IDL-type</summary>
        /// <param name="clsType">the type to map</param>
        /// <param name="action">the action to take for the determined mapping</param>
        /// <param name="attributes">the attributes on the param, field, return value</param>
        public object MapClsType(Type clsType, AttributeExtCollection attributes, MappingAction action) {
            return MapClsType(ref clsType, attributes, action);
        }
        
        /// <summary>uses MappingAction action while mapping a CLS-type to an IDL-type</summary>
        /// <param name="clsType">the type to map. The mapper can decide to transform the type during the mapping, the result of the transformation is returned. Transformation occurs, for example because of attributes</param>
        /// <param name="action">the action to take for the determined mapping</param>
        /// <param name="attributes">the attributes on the param, field, return value</param>
        public object MapClsType(ref Type clsType, AttributeExtCollection attributes, MappingAction action) {
            // handle out, ref types correctly: no other action needs to be taken than for in-types
            if (clsType.IsByRef) { 
                clsType = clsType.GetElementType(); 
            }
            
            // check for plugged special mappings, e.g. CLS ArrayList -> java.util.ArrayList
            CustomMapperRegistry cReg = CustomMapperRegistry.GetSingleton();
            if (cReg.IsCustomMappingPresentForCls(clsType)) {
                clsType = cReg.GetMappingForCls(clsType).IdlType;
            }

            // check some standard cases
            if (attributes.IsInCollection(s_boxedValAttrType)) { 
                // load the boxed value-type for this attribute
                Type boxed = Repository.GetBoxedValueType((BoxedValueAttribute)
                                                          attributes.GetAttributeForType(s_boxedValAttrType));
                if (boxed == null) { 
                    Trace.WriteLine("boxed type not found for boxed value attribute"); 
                    throw new NO_IMPLEMENT(10001, CompletionStatus.Completed_MayBe);
                }
                clsType = boxed; // transformation
                return action.MapToIdlBoxedValueType(boxed, attributes, false);
            } else if (IsInterface(clsType) && !(clsType.Equals(s_corbaTypeCodeType))) {
                return CallActionForDNInterface(ref clsType, action);
            } else if (IsMarshalByRef(clsType)) {
                return action.MapToIdlConcreteInterface(clsType);
            } else if (IsMappablePrimitiveType(clsType)) {
                return CallActionForDNPrimitveType(ref clsType, attributes, action);
            } else if (IsEnum(clsType)) { 
                return action.MapToIdlEnum(clsType);
            } else if (IsArray(clsType)) { 
                return CallActionForDNArray(ref clsType, attributes, action);
            } else if (clsType.IsSubclassOf(s_boxedValBaseType)) {
                // a boxed value type, which needs not to be boxed/unboxed but should be handled like a normal value type
                return action.MapToIdlBoxedValueType(clsType, attributes, true);
            } else if (clsType.Equals(s_objectType)) {
                return CallActionForDNObject(ref clsType, attributes, action);
            } else if (clsType.IsSubclassOf(s_exceptType) || clsType.Equals(s_exceptType)) {
                return action.MapException(clsType);
            } else if (clsType.Equals(s_typeType) || clsType.IsSubclassOf(s_typeType)) {
                return action.MapToTypeDesc(clsType);
            } else if (clsType.Equals(s_corbaTypeCodeImplType) || clsType.IsSubclassOf(s_corbaTypeCodeImplType) ||
                       clsType.Equals(s_corbaTypeCodeType)) {
                return action.MapToTypeCode(clsType);
            } else if (IsMarshalledAsStruct(clsType)) {
                 return action.MapToIdlStruct(clsType);
            } else if (IsMarshalledAsUnion(clsType)) {
                return action.MapToIdlUnion(clsType);
            } else if (IsDefaultMarshalByVal(clsType)) {
                return action.MapToIdlConcreateValueType(clsType);
            } else if (!UnmappableType(clsType)) {
                // other types are mapped to an abstract value type
                return action.MapToIdlAbstractValueType(clsType);
            } else {
                // not mappable: clsType
                throw new BAD_PARAM(18800, CompletionStatus.Completed_MayBe);
            }
        }

        /// <summary>determines the mapping for primitive types</summary>
        private object CallActionForDNPrimitveType(ref Type clsType, AttributeExtCollection attributes, MappingAction action) {
            if (clsType.Equals(s_int16Type)) {
                return action.MapToIdlShort(clsType);
            } else if (clsType.Equals(s_int32Type)) {
                return action.MapToIdlLong(clsType);
            } else if (clsType.Equals(s_int64Type)) {
                return action.MapToIdlLongLong(clsType);
            } else if (clsType.Equals(s_booleanType)) {
                return action.MapToIdlBoolean(clsType);
            } else if (clsType.Equals(s_byteType)) {
                return action.MapToIdlOctet(clsType);
            } else if (clsType.Equals(s_stringType)) {
                // distinguish cases
                return CallActionForDNString(ref clsType, attributes, action);
            } else if (clsType.Equals(s_charType)) {
                // distinguish cases
                bool useWide = UseWideOk(attributes);
                if (useWide) {
                    return action.MapToIdlWChar(clsType);
                } else {
                    return action.MapToIdlChar(clsType);
                }
            } else if (clsType.Equals(s_doubleType)) {
                return action.MapToIdlDouble(clsType);
            } else if (clsType.Equals(s_singleType)) {
                return action.MapToIdlFloat(clsType);
            } else if (clsType.Equals(s_voidType)) {
                return action.MapToIdlVoid(clsType);
            } else {
                // not mappable as primitive type: clsType
                throw new INTERNAL(18801, CompletionStatus.Completed_MayBe);
            }
        }

        /// <summary>
        /// helper for string/char mapping
        /// </summary>
        private bool UseWideOk(AttributeExtCollection attributes) {
            bool useWide = true;
            WideCharAttribute wideAttr = (WideCharAttribute)attributes.GetAttributeForType(s_widecharAttrType);
            if (wideAttr != null) {
                useWide = wideAttr.IsAllowed;
            }
            return useWide;
        }

        /// <summary>helper to determine if the string is mapped as a normal primitive value or as boxed value type</summary>
        private bool MapStringAsValueType(AttributeExtCollection attributes) {
            return attributes.IsInCollection(s_stringValueAttrType);
        }

        private object CallActionForDNString(ref Type clsType, AttributeExtCollection attributes, MappingAction action) {
            bool useWide = UseWideOk(attributes);
            if (MapStringAsValueType(attributes)) {    // distinguish between mapping as IDL primitive or as IDL boxed value
                if (useWide) {
                    return action.MapToIdlWString(clsType);
                } else {
                    return action.MapToIdlString(clsType);
                }
            } else {
                // as WStringValue / StringValue
                if (useWide) {
                    Type boxed = typeof(omg.org.CORBA.WStringValue);
                    clsType = boxed; // transform
                    return action.MapToWStringValue(boxed);
                } else {
                    Type boxed = typeof(omg.org.CORBA.StringValue);
                    clsType = boxed; // transform
                    return action.MapToStringValue(boxed);
                }
            }
        }
        
        /// <summary>
        /// call the appropriate mapping action, if the CLS type is System.Object
        /// </summary>
        /// <returns>
        /// an optional result of the mapping, some implementation of MappingAction will return null here
        /// </returns>
        private object CallActionForDNObject(ref Type clsType, AttributeExtCollection attributes, MappingAction action) {
            // distinguis the different cases here
            ObjectIdlTypeAttribute typeAttr = (ObjectIdlTypeAttribute) attributes.GetAttributeForType(
                                                                            s_objectIdlTypeAttrType);
            IdlTypeObject oType = IdlTypeObject.Any;
            if (typeAttr != null) { 
                oType = typeAttr.IdlType; 
            }
            switch (oType) {
                case IdlTypeObject.Any: 
                    return action.MapToIdlAny(clsType);
                case IdlTypeObject.AbstractBase:
                    return action.MapToAbstractBase(clsType);
                case IdlTypeObject.ValueBase:
                    return action.MapToValueBase(clsType);
                default: 
                    // unknown object attribute value: oType
                    throw new MARSHAL(18807, CompletionStatus.Completed_MayBe);
            }
        }
        
        /// <summary>
        /// call the appropriate mapping action for a CLSType array
        /// </summary>
        private object CallActionForDNArray(ref Type clsType, AttributeExtCollection attributes, MappingAction action) {
            // distinguish the different cases here
            if (attributes.IsInCollection(s_idlSequenceAttrType)) {
                return action.MapToIdlSequence(clsType);
            } else {
                Type boxed = Repository.GetBoxedArrayType(clsType);
                clsType = boxed; // transform
                return action.MapToIdlBoxedValueType(boxed, attributes, false);
            }
        }
        
        /// <summary>determines the mapping for the case, where clsType is a CLS-interface</summary>
        private object CallActionForDNInterface(ref Type clsType, MappingAction action) {
            // distinguish the different cases here
            object[] attrs = clsType.GetCustomAttributes(s_interfaceTypeAttrType, true);
            if (attrs.Length > 1) { 
                // only one InterfaceTypeAttribute for an interface allowed
                throw new INTERNAL(18811, CompletionStatus.Completed_MayBe);
            }
        
            InterfaceTypeAttribute interfaceAttr = null;
            if (attrs.Length > 0) { 
                interfaceAttr = (InterfaceTypeAttribute) attrs[0]; 
            }
            if ((interfaceAttr == null) || (interfaceAttr.IdlType.Equals(IdlTypeInterface.AbstractInterface))) {
                return action.MapToIdlAbstractInterface(clsType);
            } else if (interfaceAttr.IdlType.Equals(IdlTypeInterface.ConcreteInterface)) {
                return action.MapToIdlConcreteInterface(clsType);
            } else if (interfaceAttr.IdlType.Equals(IdlTypeInterface.AbstractValueType)) {
                return action.MapToIdlAbstractValueType(clsType);
            } else if (interfaceAttr.IdlType.Equals(IdlTypeInterface.LocalInterface)) {
                return action.MapToIdlLocalInterface(clsType);
            } else  {
                // ttributte IntrerfaceTypeAttribute had an unknown value for IDLType: interfaceAttr.IdlType
                throw new MARSHAL(18809, CompletionStatus.Completed_MayBe);
            }
        }

        #endregion IMethods

    }


    /// <summary>
    /// helper class to check, if a cls type is mapped to an abstract value type
    /// </summary>
    internal class CheckMappedToAbstractVlaue : MappingAction {
        
        #region IMethods

        public object MapToIdlUnion(System.Type clsType) {
            return false;
        }
        public object MapToIdlStruct(System.Type clsType) {
            return false;
        }
        public object MapToIdlAbstractInterface(System.Type clsType) {
            return false;
        }
        public object MapToIdlConcreteInterface(System.Type clsType) {
            return false;
        }
        public object MapToIdlLocalInterface(System.Type clsType) {
            return false;
        }
        public object MapToIdlConcreateValueType(System.Type clsType) {
            return false;
        }
        public object MapToIdlAbstractValueType(System.Type clsType) {
            return true;
        }
        public object MapToIdlBoxedValueType(System.Type clsType, AttributeExtCollection attributes, bool isAlreadyBoxed) {
            return false;
        }
        public object MapToIdlSequence(System.Type clsType) {
            return false;
        }
        public object MapToIdlAny(System.Type clsType) {
            return false;
        }
        public object MapToAbstractBase(System.Type clsType) {
            return false;
        }
        public object MapToValueBase(System.Type clsType) {
            return false;
        }
        public object MapException(System.Type clsType) {
            return false;
        }
        public object MapToIdlEnum(System.Type clsType) {
            return false;
        }
        public object MapToWStringValue(System.Type clsType) {
            return false;
        }
        public object MapToStringValue(System.Type clsType) {
            return false;
        }
        public object MapToTypeDesc(System.Type clsType) {
            return false;
        }
        public object MapToTypeCode(System.Type clsType) {
            return false;
        }
        public object MapToIdlBoolean(System.Type clsType) {
            return false;
        }
        public object MapToIdlFloat(System.Type clsType) {
            return false;
        }
        public object MapToIdlDouble(System.Type clsType) {
            return false;
        }
        public object MapToIdlShort(System.Type clsType) {
            return false;
        }
        public object MapToIdlUShort(System.Type clsType) {
            return false;
        }
        public object MapToIdlLong(System.Type clsType) {
            return false;
        }
        public object MapToIdlULong(System.Type clsType) {
            return false;
        }
        public object MapToIdlLongLong(System.Type clsType) {
            return false;
        }
        public object MapToIdlULongLong(System.Type clsType) {
            return false;
        }
        public object MapToIdlOctet(System.Type clsType) {
            return false;
        }
        public object MapToIdlVoid(System.Type clsType) {
            return false;
        }
        public object MapToIdlWChar(System.Type clsType) {
            return false;
        }
        public object MapToIdlWString(System.Type clsType) {
            return false;
        }
        public object MapToIdlChar(System.Type clsType) {
            return false;
        }
        public object MapToIdlString(System.Type clsType) {
            return false;
        }
            
        #endregion IMethods

    }
}




#if UnitTest

namespace Ch.Elca.Iiop.Tests {
	
    using NUnit.Framework;
    using Ch.Elca.Iiop.Idl;
    using Ch.Elca.Iiop.Util;
    using omg.org.CORBA;
    
    public enum MappingToResult {
        IdlStruct, IdlUnion, IdlAbstractIf, IdlConcreteIf, IdlLocalIf, IdlConcreteValue, IdlAbstractValue, 
        IdlBoxedValue, IdlSequence, IdlAny, IdlAbstractBase, IdlValueBase,
        IdlException, IdlEnum, IdlWstringValue, IdlStringValue, IdlTypeCode,
        IdlTypeDesc, IdlBool, IdlFloat, IdlDouble, IdlShort, IdlUShort, IdlLong, IdlULong,
        IdlLongLong, IdlULongLong, IdlOctet, IdlVoid, IdlChar, IdlWChar, IdlString, IdlWString
    }

    public enum TestEnum { 
        a, b, c 
    }

    /// <summary>
    /// Test class for helping to test ClsToIdlMapper
    /// </summary>
    public class TestMappingAction : MappingAction {
        
        #region IMethods
        public object MapToIdlUnion(System.Type clsType) {
            return MappingToResult.IdlUnion;
        }
        public object MapToIdlStruct(System.Type clsType) {
            return MappingToResult.IdlStruct;
        }
        public object MapToIdlAbstractInterface(System.Type clsType) {
            return MappingToResult.IdlAbstractIf;
        }
        public object MapToIdlConcreteInterface(System.Type clsType) {
            return MappingToResult.IdlConcreteIf;
        }
        public object MapToIdlLocalInterface(System.Type clsType) {
            return MappingToResult.IdlLocalIf;
        }
        public object MapToIdlConcreateValueType(System.Type clsType) {
            return MappingToResult.IdlConcreteValue;
        }
        public object MapToIdlAbstractValueType(System.Type clsType) {
            return MappingToResult.IdlAbstractValue;
        }
        public object MapToIdlBoxedValueType(System.Type clsType, AttributeExtCollection attributes, bool isAlreadyBoxed) {
            return MappingToResult.IdlBoxedValue;
        }
        public object MapToIdlSequence(System.Type clsType) {
            return MappingToResult.IdlSequence;
        }
        public object MapToIdlAny(System.Type clsType) {
            return MappingToResult.IdlAny;
        }
        public object MapToAbstractBase(System.Type clsType) {
            return MappingToResult.IdlAbstractBase;
        }
        public object MapToValueBase(System.Type clsType) {
            return MappingToResult.IdlValueBase;
        }
        public object MapException(System.Type clsType) {
            return MappingToResult.IdlException;
        }
        public object MapToIdlEnum(System.Type clsType) {
            return MappingToResult.IdlEnum;
        }
        public object MapToWStringValue(System.Type clsType) {
            return MappingToResult.IdlWstringValue;
        }
        public object MapToStringValue(System.Type clsType) {
            return MappingToResult.IdlStringValue;
        }
        public object MapToTypeCode(System.Type clsType) {
            return MappingToResult.IdlTypeCode;
        }
        public object MapToTypeDesc(System.Type clsType) {
            return MappingToResult.IdlTypeDesc;
        }
        public object MapToIdlBoolean(System.Type clsType) {
            return MappingToResult.IdlBool;
        }
        public object MapToIdlFloat(System.Type clsType) {
            return MappingToResult.IdlFloat;
        }
        public object MapToIdlDouble(System.Type clsType) {
            return MappingToResult.IdlDouble;
        }
        public object MapToIdlShort(System.Type clsType) {
            return MappingToResult.IdlShort;
        }
        public object MapToIdlUShort(System.Type clsType) {
            return MappingToResult.IdlUShort;
        }
        public object MapToIdlLong(System.Type clsType) {
            return MappingToResult.IdlLong;
        }
        public object MapToIdlULong(System.Type clsType) {
            return MappingToResult.IdlULong;
        }
        public object MapToIdlLongLong(System.Type clsType) {
            return MappingToResult.IdlLongLong;
        }
        public object MapToIdlULongLong(System.Type clsType) {
            return MappingToResult.IdlULongLong;
        }
        public object MapToIdlOctet(System.Type clsType) {
            return MappingToResult.IdlOctet;
        }
        public object MapToIdlVoid(System.Type clsType) {
            return MappingToResult.IdlVoid;
        }
        public object MapToIdlChar(System.Type clsType) {
            return MappingToResult.IdlChar;
        }
        public object MapToIdlWChar(System.Type clsType) {
            return MappingToResult.IdlWChar;
        }
        public object MapToIdlString(System.Type clsType) {
            return MappingToResult.IdlString;
        }
        public object MapToIdlWString(System.Type clsType) {
            return MappingToResult.IdlWString;
        }            
        #endregion

    }    
    
    [IdlUnion]   
    [Serializable]
    public struct TestIdlUnion : IIdlEntity {
        private System.Int32 m_discriminator;
        private System.Int16 m_val0;
        private System.Int32 m_val1;
        private System.Boolean m_val2;

        public System.Int32 Discriminator {
            get {
                return m_discriminator;
            }
        }

        public System.Int16 Getval0() {
            // ...
            return m_val0;
        }

        public System.Int32 Getval1() {
            // ...
            return m_val1;
        }

        public System.Boolean Getval2() {
            // ...
            return m_val2;
        }

        public void Setval0(System.Int16 val) {
            // ...
            m_discriminator = 0;
            m_val0 = val;
        }

        public void Setval1(System.Int32 val, System.Int32 discrVal) {
            // ...
            m_discriminator = discrVal;
            m_val1 = val;
        }

        public void Setval2(System.Boolean val, System.Int32 discrVal) {
            // ...
            m_discriminator = discrVal;
            m_val2 = val;
        }

        // other parts: left out
    }

    [IdlStruct]
    [Serializable]
    public struct TestIdlStruct : IIdlEntity {
    }
    
    [Serializable]
    public struct TestClsSerializableStruct {
    }
    
    [Serializable]
    public class TestClsSerializableClass {    
    }
    
    public struct TestClsNonSerializableStruct {
    }
    
    public class TestClsNonSerializableClass {
    }

    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/TestBoxedVal:1.0")]
    [Serializable]
    public class TestBoxedVal : BoxedValueBase, IIdlEntity {

        private Int16 m_val;

        public TestBoxedVal() {
        }

        public TestBoxedVal(Int16 val) {
            m_val = val;
        }

        protected override object GetValue() {
            return m_val;
        }
        
    }
   
    [InterfaceTypeAttribute(IdlTypeInterface.AbstractInterface)]
    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/TestAbsInterface:1.0")]
    public interface TestAbsInterface {
    }

    [InterfaceTypeAttribute(IdlTypeInterface.AbstractValueType)]
    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/TestAbsValInterface:1.0")]
    public interface TestAbsValInterface {
    }

    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/TestConInterface:1.0")]
    public interface TestConInterface {
    }
    
    [InterfaceTypeAttribute(IdlTypeInterface.LocalInterface)]
    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/TestLocalInterface:1.0")]
    public interface TestLocalInterface {
    }


    public class TestRemotableByRef : MarshalByRefObject {
    }
  
    /// <summary>
    /// Unit-tests for testing the ClsToIdlMapper
    /// </summary>
    public class ClsToIdlMapperTest : TestCase {
        
        #region SFields
        
        private static TestMappingAction s_testAction = new TestMappingAction();
        
        #endregion
        #region IConstructors
        
        public ClsToIdlMapperTest() {
        }

        #endregion
        #region IMethods

        public void TestMapToIdlVoid() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(void), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlVoid, mapResult);        
        }

        public void TestMapToIdlOctet() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Byte), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlOctet, mapResult);
        }
        
        public void TestMapToIdlShort() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Int16), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlShort, mapResult);        	
        }

        public void TestMapToIdlLong() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Int32), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlLong, mapResult);
        }
        
        public void TestMapToIdlLongLong() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Int64), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlLongLong, mapResult);
        }

        [ExpectedException(typeof(BAD_PARAM))]
        public void TestMapUInt16() {
        	// System.UInt16 is not mappable, because UInt16 is not CLS compatible
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(UInt16), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
        }

        [ExpectedException(typeof(BAD_PARAM))]
        public void TestMapUInt32() {
            // System.UInt32 is not mappable, because UInt32 is not CLS compatible
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(UInt32), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
        }

        [ExpectedException(typeof(BAD_PARAM))]
        public void TestMapUInt64() {
            // System.UInt64 is not mappable, because UInt64 is not CLS compatible
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(UInt64), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
        }
                        
        public void TestMapToIdlBoolean() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Boolean), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlBool, mapResult);
        }
        
        public void TestMapToIdlFloat() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Single), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlFloat, mapResult);        
        }
        
        public void TestMapToIdlDouble() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Double), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlDouble, mapResult);
        }

        public void TestMapToIdlString() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(String), 
                                                                           new AttributeExtCollection(new Attribute[] { new StringValueAttribute(), new WideCharAttribute(false) }),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlString, mapResult);
        }

        public void TestMapToIdlWString() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(String), 
                                                                           new AttributeExtCollection(new Attribute[] { new StringValueAttribute(), new WideCharAttribute(true) }),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlWString, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(String), 
                                                           new AttributeExtCollection(new Attribute[] { new StringValueAttribute() }),
                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlWString, mapResult);
        }

        public void TestMapToIdlStringValue() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(String), 
                                                                           new AttributeExtCollection(new Attribute[] { new WideCharAttribute(false) }),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlStringValue, mapResult);
        }

        public void TestMapToIdlWStringValue() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(String), 
                                                                           new AttributeExtCollection(new Attribute[] { new WideCharAttribute(true) }),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlWstringValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(String), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlWstringValue, mapResult);
        }

        public void TestMapToIdlChar() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Char), 
                                                                           new AttributeExtCollection(new Attribute[] { new WideCharAttribute(false) }),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlChar, mapResult);
        }

        public void TestMapToIdlWChar() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Char), 
                                                                           new AttributeExtCollection(new Attribute[] { new WideCharAttribute(true) }),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlWChar, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(Char), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlWChar, mapResult);
        }

        public void TestMapToIdlSequence() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(int[]), 
                                                                           new AttributeExtCollection(new Attribute[] { new IdlSequenceAttribute() } ),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlSequence, mapResult);
        }

        public void TestMapToIdlAny() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(object), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlAny, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(object), 
                                                           new AttributeExtCollection(new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.Any) }),
                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlAny, mapResult);
        }

        public void TestMapToIdlEnum() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnum), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlEnum, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnum), 
                                                           new AttributeExtCollection(new Attribute[] { new IdlEnumAttribute() }),
                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlEnum, mapResult);            
        }
        
        public void TestMapToIdlStruct() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestIdlStruct), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlStruct, mapResult);
        }

        public void TestMapToIdlUnion() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestIdlUnion), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlUnion, mapResult);
        }
        
        public void TestMapToIdlConcreteValueType() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestClsSerializableStruct), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlConcreteValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestClsSerializableClass), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlConcreteValue, mapResult);						
        }
        
        public void TestMapToIdlAbstractValueType() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestClsNonSerializableStruct), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlAbstractValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestClsNonSerializableClass), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlAbstractValue, mapResult);	
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestAbsValInterface), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
			Assertion.AssertEquals(MappingToResult.IdlAbstractValue, mapResult);	
        }

        public void TestMapToIdlBoxedValueType() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Int16[]), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlBoxedValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestBoxedVal ), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlBoxedValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(Int16), 
                                                           new AttributeExtCollection(new Attribute[] { new BoxedValueAttribute("IDL:Ch/Elca/Iiop/Tests/TestBoxedVal:1.0") }),
                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlBoxedValue, mapResult);
        }

        public void TestMapToIdlConcreteInterface() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestRemotableByRef), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlConcreteIf, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestConInterface), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlConcreteIf, mapResult);            
        }

        public void TestMapToIdlAbstractInterface() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestAbsInterface), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlAbstractIf, mapResult);
        }
        
        public void TestMapToIdlLocalInterface() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestLocalInterface), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlLocalIf, mapResult);
        }
        
        public void TestMapToIdlException() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Exception), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlException, mapResult);
        }
        
        public void TestMapToIdlAbstractBase() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(object), 
                                                                           new AttributeExtCollection(new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase) }),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlAbstractBase, mapResult);
        }
        
        public void TestMapToIdlValueBase() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(object), 
                                                                           new AttributeExtCollection(new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.ValueBase) }),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlValueBase, mapResult);
        }
        
        public void TestMapToIdlTypeDesc() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Type), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlTypeDesc, mapResult);
        }
        
        public void TestMapToIdlTypeCode() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TypeCodeImpl), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlTypeCode, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(omg.org.CORBA.TypeCode), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assertion.AssertEquals(MappingToResult.IdlTypeCode, mapResult);            
        }
        
        #endregion
        
    }

}

#endif