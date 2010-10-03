/* CLSToIDLMapper.cs
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
using System.Collections;
using System.Collections.Generic;
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
        /// <param name="clsType">the .NET boxed value type type, inheriting from BoxedValueBase</param>
        /// <param name="needsBoxingFrom">tells, if the dotNetType is boxed in a boxed value type, or if a native Boxed value type is mapped;
        /// if needsBoxingFrom is != null, it's a dotnettype, which is boxed to clsType; if == null; it's a native boxed type, which needs no
        /// boxing.</param>
        object MapToIdlBoxedValueType(Type clsType, Type needsBoxingFrom);

        /// <param name="bound">for unbounded sequences: 0, else max nr of elems</param>
        /// <param name="allAttributes">the attributes including the IdlSequenceAttributes lead to calling this map action</param>
        /// <param name="elemTypeAttributes">the attributes of the sequence element</param>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlSequence(Type clsType, int bound, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes);

        /// <param name="dimensions">fixed size array: the dimensions of the array</param>
        /// <param name="allAttributes">the attributes including the IdlArrayAttributes lead to calling this map action</param>
        /// <param name="elemTypeAttributes">the attributes of the array element</param>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlArray(Type clsType, int[] dimensions, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes);

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
        
        /// <summary>
        /// This mapping method is used to map CLS enums, which are no flags to idl enum
        /// or to the underlying integral type for enums based on int64/uint64.
        /// </summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlEnum(Type clsType);
        
        /// <summary>
        /// This mapping method is used to map CLS flags to underlying integral type.
        /// Distinguish this from integral types to allow deserialization to Flags
        /// in a good way.
        /// </summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlFlagsEquivalent(Type clsType);

        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToWStringValue(Type clsType);

        object MapToStringValue(Type clsType);

        /// <summary>map to CORBA type-desc (typecode) for System.Type</summary>
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
        
        /// <summary>
        /// a mapping form system.SByte to an idl equivalent.
        /// Distinguish this from byte to octet mapping to allow deserialization to sbyte
        /// in a good way.
        /// </summary>
        /// <returns>an optional result of the mapping, null may be possible</returns>
        object MapToIdlSByteEquivalent(Type clsType);

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
        private static MappingToAction s_mappingToAction = new MappingToAction();

        // the following expressions are evaluated here for efficiency reasons
        private static Type s_exceptType = typeof(System.Exception);

        private static Type s_mByValComponentType = typeof(MarshalByValueComponent);

        private static Type s_boxedValAttrType = typeof(BoxedValueAttribute);
        private static Type s_objectIdlTypeAttrType = typeof(ObjectIdlTypeAttribute);
        private static Type s_interfaceTypeAttrType = typeof(InterfaceTypeAttribute);

        private static Type s_corbaTypeCodeImplType = typeof(TypeCodeImpl);
        
        private static Type s_anyType = typeof(omg.org.CORBA.Any);
        
        private static Type s_intPtrType = typeof(System.IntPtr);
        private static Type s_uintPtrType = typeof(System.UIntPtr);

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
        private static bool IsMarshalByRef(Type type) {
            return type.IsMarshalByRef;
        }

        /// <summary>
        /// checks, if the type is an enumeration
        /// </summary>
        public static bool IsEnum(Type type) {
            return (type.IsEnum);
        }        
        
        /// <summary>
        /// checks, if the enum type is a flags enumeration
        /// </summary>        
        private static bool HasEnumFlagsAttributes(Type enumType) {
            object[] flagsAttrs = 
                enumType.GetCustomAttributes(ReflectionHelper.FlagsAttributeType, true);
            return (flagsAttrs != null && flagsAttrs.Length > 0);          
        }
        
        /// <summary>
        /// Returns true, if the enum is mapped from idl to cls.
        /// </summary>
        internal static bool IsIdlEnum(Type enumType) {
            AttributeExtCollection attrs = ReflectionHelper.GetCustomAttributesForType(enumType, true);
            return (attrs.IsInCollection(ReflectionHelper.IdlEnumAttributeType));
        }        
        
        /// <summary>
        /// checks, if the type is a CLS-array
        /// </summary>
        public static bool IsArray(Type type) {
            return (type.IsArray);
        }
        
        public static bool IsException(Type type) {
            return (s_exceptType.IsAssignableFrom(type));
        }
        
        /// <summary>
        /// checks, if the conditions are fullfilled to map a value type to a concrete corba value type
        /// </summary>
        private static bool IsValueTypeConcrete(Type type) {
            if ((!(ReflectionHelper.IIdlEntityType.IsAssignableFrom(type))) && (type.IsAbstract)) { 
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
            if (clsType.Equals(ReflectionHelper.Int16Type) ||
                clsType.Equals(ReflectionHelper.Int32Type) ||
                clsType.Equals(ReflectionHelper.Int64Type) ||
                clsType.Equals(ReflectionHelper.ByteType) ||
                clsType.Equals(ReflectionHelper.BooleanType) ||
                clsType.Equals(ReflectionHelper.VoidType) ||
                clsType.Equals(ReflectionHelper.SingleType) ||
                clsType.Equals(ReflectionHelper.DoubleType) ||
                clsType.Equals(ReflectionHelper.CharType) ||
                clsType.Equals(ReflectionHelper.StringType) ||
                clsType.Equals(ReflectionHelper.UInt16Type) ||
                clsType.Equals(ReflectionHelper.UInt32Type) ||
                clsType.Equals(ReflectionHelper.UInt64Type) ||
                clsType.Equals(ReflectionHelper.SByteType)) {
                return true; 
            } else {
                return false;
            }
        }

        /// <summary>determines, if the CLS-type is mapped to an IDL-struct</summary>
        public static bool IsMarshalledAsStruct(Type clsType) {
            AttributeExtCollection attrs = ReflectionHelper.GetCustomAttributesForType(clsType, true);
            return attrs.IsInCollection(ReflectionHelper.IdlStructAttributeType);
        }

        /// <summary>determines, if the CLS-type is mapped to an IDL-union</summary>
        public static bool IsMarshalledAsUnion(Type clsType) {
            AttributeExtCollection attrs = ReflectionHelper.GetCustomAttributesForType(clsType, true);
            return attrs.IsInCollection(ReflectionHelper.IdlUnionAttributeType);
        }

        
        /// <summary>determines, if a CLS Type is mapped to an IDL abstract value type</summary>
        public static bool IsMappedToAbstractValueType(Type clsType, AttributeExtCollection attributes) {
            return (MappingToResult)s_singleton.MapClsType(clsType, attributes, s_mappingToAction) ==
                MappingToResult.IdlAbstractValue;
        }
        
        /// <summary>determines, if a CLS Type is mapped to an IDL abstract value type</summary>
        public static bool IsMappedToAbstractValueType(Type clsType) {
            return IsMappedToAbstractValueType(clsType, AttributeExtCollection.EmptyCollection);
        }        
        
        /// <summary>
        /// Checks, if the CLS type is mapped to an IDL concrete value type.
        /// </summary>
        public static bool IsMappedToConcreteValueType(Type clsType) {
            return IsMappedToConcreteValueType(clsType, AttributeExtCollection.EmptyCollection);
        }
        
        /// <summary>
        /// Checks, if the CLS type is mapped to an IDL concrete value type.
        /// </summary>
        public static bool IsMappedToConcreteValueType(Type clsType, AttributeExtCollection attributes) {
            return (MappingToResult) s_singleton.MapClsType(clsType, attributes, s_mappingToAction) ==
                MappingToResult.IdlConcreteValue;
        }        

        public static bool IsMappedToConcreteInterface(Type clsType) {
            return IsMappedToConcreteInterface(clsType, AttributeExtCollection.EmptyCollection);
        }        
        
        public static bool IsMappedToConcreteInterface(Type clsType, AttributeExtCollection attributes) {
            return (MappingToResult) s_singleton.MapClsType(clsType, attributes, s_mappingToAction) ==
                MappingToResult.IdlConcreteIf;
        }

        public static bool IsMappedToAbstractInterface(Type clsType) {
            return IsMappedToAbstractInterface(clsType, AttributeExtCollection.EmptyCollection);
        }        
        
        public static bool IsMappedToAbstractInterface(Type clsType, AttributeExtCollection attributes) {
            return (MappingToResult) s_singleton.MapClsType(clsType, attributes, s_mappingToAction) ==
                MappingToResult.IdlAbstractIf;
        }
        
        public static bool IsMappedToLocalInterface(Type clsType) {
            return IsMappedToLocalInterface(clsType, AttributeExtCollection.EmptyCollection);
        }        
        
        public static bool IsMappedToLocalInterface(Type clsType, AttributeExtCollection attributes) {
            return (MappingToResult) s_singleton.MapClsType(clsType, attributes, s_mappingToAction) ==
                MappingToResult.IdlLocalIf;
        }        
        
        public static bool IsMappedToBoxedValueType(Type clsType) {
            return IsMappedToBoxedValueType(clsType, AttributeExtCollection.EmptyCollection);
        }        
        
        public static bool IsMappedToBoxedValueType(Type clsType, AttributeExtCollection attributes) {
            return (MappingToResult) s_singleton.MapClsType(clsType, attributes, s_mappingToAction) ==
                MappingToResult.IdlBoxedValue;
        }        

        /// <summary>checks, if the type is unmappable</summary>
        public static bool UnmappableType(Type clsType) {
            if (clsType.Equals(s_intPtrType) ||                
                clsType.Equals(s_uintPtrType)) {
                return true; 
            }
            return false;
        }
        
        /// <summary>
        /// checks, if the interface impelemntation relation can be mapped from CLS to IDL. 
        /// </summary>
        /// <param name="interfaceType">the CLS interface to check for</param>
        /// <param name="implementorType">the CLS class/interface, which implements the interface</param>
        /// <returns></returns>
        public static bool MapInheritanceFromInterfaceToIdl(Type interfaceType, Type implementorType) {
            if (!interfaceType.IsInterface) {
                return false;
            }
            if (ClsToIdlMapper.IsMappedToAbstractInterface(implementorType)) {
                // an abstract interface child can only inherit from an abstract interface parent
                return ClsToIdlMapper.IsMappedToAbstractInterface(interfaceType);
            }
            if (ClsToIdlMapper.IsMappedToConcreteInterface(implementorType)) {
                // a concrete interface may inherit from abstract and concrete interfaces
                return ClsToIdlMapper.IsMappedToAbstractInterface(interfaceType) ||
                       ClsToIdlMapper.IsMappedToConcreteInterface(interfaceType);
            }
            if (ClsToIdlMapper.IsMappedToLocalInterface(implementorType)) {
                // a local interface may inherit from local, abstract and concrete interfaces
                return ClsToIdlMapper.IsMappedToAbstractInterface(interfaceType) ||
                       ClsToIdlMapper.IsMappedToConcreteInterface(interfaceType) ||
                       ClsToIdlMapper.IsMappedToLocalInterface(interfaceType);
            }
            if (ClsToIdlMapper.IsMappedToAbstractValueType(implementorType)) {
                // a abstract value type may inherit from local, abstract and concrete interfaces
                return ClsToIdlMapper.IsMappedToAbstractInterface(interfaceType) ||
                       ClsToIdlMapper.IsMappedToConcreteInterface(interfaceType) ||
                       ClsToIdlMapper.IsMappedToLocalInterface(interfaceType);              
            }
            if (ClsToIdlMapper.IsMappedToConcreteValueType(implementorType)) {
                // a abstract value type may inherit from local, abstract and concrete interfaces
                return ClsToIdlMapper.IsMappedToAbstractInterface(interfaceType) ||
                       ClsToIdlMapper.IsMappedToConcreteInterface(interfaceType) ||
                       ClsToIdlMapper.IsMappedToLocalInterface(interfaceType);              
            }
            return false;
        }
        
        /// <summary>
        /// loads the boxed value type for the BoxedValueAttribute
        /// </summary>
        private static Type GetBoxedValueType(BoxedValueAttribute attr) {
            string repId = attr.RepositoryId; 
            Debug.WriteLine("getting boxed value type: " + repId);
            Type resultType = Repository.GetTypeForId(repId);
            return resultType;
        }

        /// <summar>load or create a boxed value type for a .NET array, which is mapped to an IDL boxed value type through the CLS to IDL mapping</summary>
        /// <remarks>this method is not called for IDL Boxed value types, mapped to a CLS array, for those the getBoxedValueType method is responsible</remarks>
        private static Type GetBoxedArrayType(Type clsArrayType) {
            // convert a .NET true moredim array type to an array of array of ... type
            if (clsArrayType.GetArrayRank() > 1) {
                clsArrayType = BoxedArrayHelper.CreateNestedOneDimType(clsArrayType);
            }
            BoxedValueRuntimeTypeGenerator gen = BoxedValueRuntimeTypeGenerator.GetSingleton();
            return gen.GetOrCreateBoxedTypeForArray(clsArrayType);
        }               

        #endregion SMethods
        #region IMethods
        
        /// <summary>uses MappingAction action while mapping a CLS-type to an IDL-type</summary>
        /// <param name="clsType">the type to map</param>
        /// <param name="action">the action to take for the determined mapping</param>
        /// <param name="attributes">the attributes on the param, field, return value</param>
        public object MapClsType(Type clsType, AttributeExtCollection attributes, MappingAction action) {
            CustomMappingDesc usedCustomMapping;
            return MapClsTypeWithTransform(ref clsType, ref attributes, action, out usedCustomMapping);
        }
        
        /// <summary>uses MappingAction action while mapping a CLS-type to an IDL-type</summary>
        /// <param name="clsType">the type to map. The mapper can decide to transform the type during the mapping, the result of the transformation is returned. Transformation occurs, for example because of attributes</param>
        /// <param name="action">the action to take for the determined mapping</param>
        /// <param name="attributes">the attributes on the param, field, return value; a new collection without the considered attributes is returned</param>
        public object MapClsTypeWithTransform(ref Type clsType, ref AttributeExtCollection attributes, MappingAction action) {            
            CustomMappingDesc usedCustomMapping;
            return MapClsTypeWithTransform(ref clsType, ref attributes, action, out usedCustomMapping);
        }
        
        private enum MapType {
            Unknown,
            
            BoxedValueNotFound,
            BoxedValue,
            Interface,
            MarshalByRef,
            Primitive,
            Flags,
            Enum,
            Array,
            BoxedValue2,
            Exception,
            Struct,
            Union,
            Object,
            Any,
            TypeDesc,
            TypeCode,
            ConcreteValue,
            AbstractValue,
            Unmappable
        }
        
        private static Dictionary<TypeCodeCreater.TypecodeForTypeKey, MapType> mapTypes = new Dictionary<TypeCodeCreater.TypecodeForTypeKey, MapType>();
        
        /// <summary>uses MappingAction action while mapping a CLS-type to an IDL-type</summary>
        /// <param name="clsType">the type to map. The mapper can decide to transform the type during the mapping, the result of the transformation is returned. Transformation occurs, for example because of attributes</param>
        /// <param name="action">the action to take for the determined mapping</param>
        /// <param name="attributes">the attributes on the param, field, return value; a new collection without the considered attributes is returned</param>
        /// <param name="usedCustomMapping">the custom mapping used if any; otherwise null</param>
        public object MapClsTypeWithTransform(ref Type clsType, ref AttributeExtCollection attributes, MappingAction action,
                                              out CustomMappingDesc usedCustomMapping) {
                                              
            TypeCodeCreater.TypecodeForTypeKey key = new TypeCodeCreater.TypecodeForTypeKey(clsType, attributes);
            MapType mapType = MapType.Unknown;
            
            lock(mapTypes) {
                if(!mapTypes.TryGetValue(key, out mapType))
                    mapType = MapType.Unknown;
            }
            // handle out, ref types correctly: no other action needs to be taken than for in-types
            usedCustomMapping = null;
            AttributeExtCollection originalAttributes = attributes; // used to save reference to the passed in attributes
            if (clsType.IsByRef) {
                clsType = clsType.GetElementType(); 
            }
            
            // check for plugged special mappings, e.g. CLS ArrayList -> java.util.ArrayList
            CustomMapperRegistry cReg = CustomMapperRegistry.GetSingleton();
            if (cReg.IsCustomMappingPresentForCls(clsType)) {
                usedCustomMapping = cReg.GetMappingForCls(clsType);
                clsType = usedCustomMapping.IdlType;
            }

            // check some standard cases
            Attribute boxedValAttr;
            attributes = originalAttributes.RemoveAttributeOfType(s_boxedValAttrType, out boxedValAttr);
            
            if(mapType == MapType.Unknown)
            {
                if (boxedValAttr != null) { 
                    // load the boxed value-type for this attribute
                    Type boxed = GetBoxedValueType((BoxedValueAttribute)boxedValAttr);
                    if (boxed == null) { 
                        Trace.WriteLine("boxed type not found for boxed value attribute"); 
                        mapType = MapType.BoxedValueNotFound;
                    } else {
                        Type needsBoxingFrom = clsType;
                        clsType = boxed; // transformation
                        mapType = MapType.BoxedValue;
                    }
                } else if (IsInterface(clsType) && !(clsType.Equals(ReflectionHelper.CorbaTypeCodeType))) {
                    mapType = MapType.Interface;
                } else if (IsMarshalByRef(clsType)) {
                    mapType = MapType.MarshalByRef;
                } else if (IsMappablePrimitiveType(clsType)) {
                    mapType = MapType.Primitive;
                } else if (IsEnum(clsType)) { 
                    if (HasEnumFlagsAttributes(clsType)) {
                        mapType = MapType.Flags;
                    } else {
                        // enums with more than Int32.MaxValue elements can't be mapped to idl.
                        // but not possibly to check this, because in .NET 1.0, no support for Array.LongLength.
                        mapType = MapType.Enum;
                    }
                } else if (IsArray(clsType)) { 
                    mapType = MapType.Array;
                } else if (clsType.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) {
                    // a boxed value type, which needs not to be boxed/unboxed but should be handled like a normal value type
                    mapType = MapType.BoxedValue2;
                } else if (clsType.IsSubclassOf(s_exceptType) || clsType.Equals(s_exceptType)) {
                    mapType = MapType.Exception;
                } else if (IsMarshalledAsStruct(clsType)) {
                     mapType = MapType.Struct;
                } else if (IsMarshalledAsUnion(clsType)) {
                    mapType = MapType.Union;
                } else if (clsType.Equals(ReflectionHelper.ObjectType)) {
                    mapType = MapType.Object;
                } else if (clsType.Equals(s_anyType)) {
                    mapType = MapType.Any;
                } else if (clsType.Equals(ReflectionHelper.TypeType) || clsType.IsSubclassOf(ReflectionHelper.TypeType)) {
                    mapType = MapType.TypeDesc;
                } else if (clsType.Equals(s_corbaTypeCodeImplType) || clsType.IsSubclassOf(s_corbaTypeCodeImplType) ||
                           clsType.Equals(ReflectionHelper.CorbaTypeCodeType)) {
                    mapType = MapType.TypeCode;
                } else if (!UnmappableType(clsType)) {
                    if (IsValueTypeConcrete(clsType)) {
                        mapType = MapType.ConcreteValue;
                    } else {
                        // other types are mapped to an abstract value type
                        mapType = MapType.AbstractValue;
                    }
                } else {
                    // not mappable: clsType
                    mapType = MapType.Unmappable;
                }
                
                lock(mapTypes)
                    mapTypes[key] = mapType;
            }
            
            switch(mapType)
            {
                case MapType.BoxedValueNotFound:
                    Trace.WriteLine("boxed type not found for boxed value attribute"); 
                    throw new NO_IMPLEMENT(10001, CompletionStatus.Completed_MayBe);
                case MapType.BoxedValue:
                    Type boxed = GetBoxedValueType((BoxedValueAttribute)boxedValAttr);
                    Type needsBoxingFrom = clsType;
                    clsType = boxed; // transformation
                    return action.MapToIdlBoxedValueType(boxed, needsBoxingFrom);
                case MapType.Interface:
                    return CallActionForDNInterface(ref clsType, action);
                case MapType.MarshalByRef:
                    return action.MapToIdlConcreteInterface(clsType);
                case MapType.Primitive:
                    return CallActionForDNPrimitveType(ref clsType, ref attributes, action);
                case MapType.Flags:
                    return action.MapToIdlFlagsEquivalent(clsType);
                case MapType.Enum:
                    // enums with more than Int32.MaxValue elements can't be mapped to idl.
                    // but not possibly to check this, because in .NET 1.0, no support for Array.LongLength.
                    return action.MapToIdlEnum(clsType);
                case MapType.Array:
                    return CallActionForDNArray(ref clsType, ref attributes, originalAttributes, action);
                case MapType.BoxedValue2:
                    // a boxed value type, which needs not to be boxed/unboxed but should be handled like a normal value type
                    return action.MapToIdlBoxedValueType(clsType, null);
                case MapType.Exception:
                    return action.MapException(clsType);
                case MapType.Struct:
                     return action.MapToIdlStruct(clsType);
                case MapType.Union:
                    return action.MapToIdlUnion(clsType);
                case MapType.Object:
                    return CallActionForDNObject(ref clsType, ref attributes, action);
                case MapType.Any:
                    return action.MapToIdlAny(clsType);
                case MapType.TypeDesc:
                    return action.MapToTypeDesc(clsType);
                case MapType.TypeCode:
                    return action.MapToTypeCode(clsType);
                case MapType.ConcreteValue:
                    return action.MapToIdlConcreateValueType(clsType);
                case MapType.AbstractValue:
                    // other types are mapped to an abstract value type
                    return action.MapToIdlAbstractValueType(clsType);                   
            }
            // not mappable: clsType
            throw new BAD_PARAM(18800, CompletionStatus.Completed_MayBe, "The type " + clsType.AssemblyQualifiedName +
                               " is not mappable to idl");
        }

        /// <summary>determines the mapping for primitive types</summary>
        private object CallActionForDNPrimitveType(ref Type clsType, ref AttributeExtCollection modifiedAttributes, MappingAction action) {
            if (clsType.Equals(ReflectionHelper.Int16Type)) {
                return action.MapToIdlShort(clsType);
            } else if (clsType.Equals(ReflectionHelper.Int32Type)) {
                return action.MapToIdlLong(clsType);
            } else if (clsType.Equals(ReflectionHelper.Int64Type)) {
                return action.MapToIdlLongLong(clsType);
            } else if (clsType.Equals(ReflectionHelper.BooleanType)) {
                return action.MapToIdlBoolean(clsType);
            } else if (clsType.Equals(ReflectionHelper.ByteType)) {
                return action.MapToIdlOctet(clsType);
            } else if (clsType.Equals(ReflectionHelper.StringType)) {
                // distinguish cases
                return CallActionForDNString(ref clsType, ref modifiedAttributes, action);
            } else if (clsType.Equals(ReflectionHelper.CharType)) {
                // distinguish cases
                bool useWide = UseWideOk(ref modifiedAttributes);
                if (useWide) {
                    return action.MapToIdlWChar(clsType);
                } else {
                    return action.MapToIdlChar(clsType);
                }
            } else if (clsType.Equals(ReflectionHelper.DoubleType)) {
                return action.MapToIdlDouble(clsType);
            } else if (clsType.Equals(ReflectionHelper.SingleType)) {
                return action.MapToIdlFloat(clsType);
            } else if (clsType.Equals(ReflectionHelper.UInt16Type)) {
                return action.MapToIdlUShort(clsType);
            } else if (clsType.Equals(ReflectionHelper.UInt32Type)) {
                return action.MapToIdlULong(clsType);
            } else if (clsType.Equals(ReflectionHelper.UInt64Type)) {
                return action.MapToIdlULongLong(clsType);
            } else if (clsType.Equals(ReflectionHelper.SByteType)) {
                return action.MapToIdlSByteEquivalent(clsType);
            } else if (clsType.Equals(ReflectionHelper.VoidType)) {
                return action.MapToIdlVoid(clsType);
            } else {
                // not mappable as primitive type: clsType
                throw new INTERNAL(18801, CompletionStatus.Completed_MayBe);
            }
        }

        /// <summary>
        /// helper for string/char mapping; removes the wchar attribute if present
        /// </summary>
        private bool UseWideOk(ref AttributeExtCollection modifiedAttributes) {
            bool useWide = MappingConfiguration.Instance.UseWideCharByDefault;
            Attribute wideAttr;
            modifiedAttributes = modifiedAttributes.RemoveAttributeOfType(ReflectionHelper.WideCharAttributeType, out wideAttr);
            if (wideAttr != null) {
                useWide = ((WideCharAttribute)wideAttr).IsAllowed;
            }
            return useWide;
        }

        /// <summary>helper to determine if the string is mapped as a normal primitive value or as boxed value type</summary>
        private bool MapStringAsValueType(ref AttributeExtCollection modifiedAttributes) {
            Attribute mapAsWStringValueAttr;
            modifiedAttributes = modifiedAttributes.RemoveAttributeOfType(ReflectionHelper.StringValueAttributeType, out mapAsWStringValueAttr);
            return mapAsWStringValueAttr != null;
        }

        private object CallActionForDNString(ref Type clsType, ref AttributeExtCollection modifiedAttributes, MappingAction action) {
            bool useWide = UseWideOk(ref modifiedAttributes);
            if (MapStringAsValueType(ref modifiedAttributes)) {    // distinguish between mapping as IDL primitive or as IDL boxed value
                if (useWide) {
                    return action.MapToIdlWString(clsType);
                } else {
                    return action.MapToIdlString(clsType);
                }
            } else {
                // as WStringValue / StringValue
                if (useWide) {
                    Type boxed = ReflectionHelper.WStringValueType;
                    clsType = boxed; // transform
                    return action.MapToWStringValue(boxed);
                } else {
                    Type boxed = ReflectionHelper.StringValueType;
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
        private object CallActionForDNObject(ref Type clsType, ref AttributeExtCollection modifiedAttributes, MappingAction action) {
            // distinguis the different cases here
            Attribute typeAttr;
            modifiedAttributes = modifiedAttributes.RemoveAttributeOfType(s_objectIdlTypeAttrType, out typeAttr);               
            IdlTypeObject oType = IdlTypeObject.Any;
            if (typeAttr != null) { 
                oType = ((ObjectIdlTypeAttribute)typeAttr).IdlType;
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
        
        private int[] DetermineIdlArrayDimensions(Type clsType, IdlArrayAttribute arrayAttr,
                                                  ref AttributeExtCollection modifiedAttributes) {
            // get all the array dimensions, first is inside the IDLArrayAttribute; others are separate
            int[] dimensions = new int[clsType.GetArrayRank()];
            if (dimensions.Length < 1) {
                throw new INTERNAL(5643, CompletionStatus.Completed_MayBe); // should never occur
            }
            dimensions[0] = arrayAttr.FirstDimensionSize;
            if (dimensions.Length > 1) {
                IList dimensionAttrs;
                modifiedAttributes = 
                    modifiedAttributes.RemoveAssociatedAttributes(arrayAttr.OrderNr, out dimensionAttrs);
                if (dimensionAttrs.Count != dimensions.Length - 1) {
                    throw new INTERNAL(5644, CompletionStatus.Completed_MayBe); // should never occur
                }
                for (int i = 0; i < dimensionAttrs.Count; i++) {
                    IdlArrayDimensionAttribute arrayDim = ((IdlArrayDimensionAttribute)dimensionAttrs[i]);                    
                    dimensions[arrayDim.DimensionNr] = arrayDim.DimensionSize;
                }
            }
            return dimensions;
        }


        /// <summary>
        /// call the appropriate mapping action for a CLSType array
        /// </summary>
        private object CallActionForDNArray(ref Type clsType,
                                            ref AttributeExtCollection modifiedAttributes, 
                                            AttributeExtCollection allAttributes,                                            
                                            MappingAction action) {
            // distinguish the different cases here
            Attribute idlMappingAttr = modifiedAttributes.GetHighestOrderAttribute();
            if (idlMappingAttr is IdlSequenceAttribute) {
                modifiedAttributes = modifiedAttributes.RemoveAttribute(idlMappingAttr);
                int bound = (int)
                    ((IdlSequenceAttribute)idlMappingAttr).Bound;
                return action.MapToIdlSequence(clsType, bound, allAttributes, modifiedAttributes);
            } else if (idlMappingAttr is IdlArrayAttribute) {
                IdlArrayAttribute arrayAttr = (IdlArrayAttribute)idlMappingAttr;
                modifiedAttributes = modifiedAttributes.RemoveAttribute(idlMappingAttr);
                int[] dimensions = DetermineIdlArrayDimensions(clsType, arrayAttr, ref modifiedAttributes);
                return action.MapToIdlArray(clsType, dimensions, allAttributes, modifiedAttributes);
            } else {
                Type boxed = GetBoxedArrayType(clsType);
                Type needsBoxingFrom = clsType;
                clsType = boxed; // transform
                return action.MapToIdlBoxedValueType(boxed, needsBoxingFrom);
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
    /// used as result for MappingToAction
    /// </summary>
    internal enum MappingToResult {
        IdlStruct, IdlUnion, IdlAbstractIf, IdlConcreteIf, IdlLocalIf, IdlConcreteValue, IdlAbstractValue, 
        IdlBoxedValue, IdlSequence, IdlArray, IdlAny, IdlAbstractBase, IdlValueBase,
        IdlException, IdlEnum, IdlFlagsEquivalent, IdlWstringValue, IdlStringValue, IdlTypeCode,
        IdlTypeDesc, IdlBool, IdlFloat, IdlDouble, IdlShort, IdlUShort, IdlLong, IdlULong,
        IdlLongLong, IdlULongLong, IdlOctet, IdlSByteEquivalent, IdlVoid, 
        IdlChar, IdlWChar, IdlString, IdlWString
    }
    
    /// <summary>
    /// Test class for helping to test ClsToIdlMapper
    /// </summary>
    internal class MappingToAction : MappingAction {
        
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
        public object MapToIdlBoxedValueType(System.Type clsType, System.Type needsBoxingFrom) {
            return MappingToResult.IdlBoxedValue;
        }
        public object MapToIdlSequence(System.Type clsType, int bound, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes) {
            return MappingToResult.IdlSequence;
        }
        public object MapToIdlArray(System.Type clsType, int[] dimensions, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes) {
            return MappingToResult.IdlArray;
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
        public object MapToIdlFlagsEquivalent(Type clsType) {
            return MappingToResult.IdlFlagsEquivalent;
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
        public object MapToIdlSByteEquivalent(Type clsType) {
            return MappingToResult.IdlSByteEquivalent;
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
    
}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using NUnit.Framework;
    using Ch.Elca.Iiop.Idl;
    using Ch.Elca.Iiop.Util;
    using omg.org.CORBA;
        
    
    [IdlEnum]
    public enum TestIdlEnumBI32 {
        IDL_A, IDL_B, IDL_C
    }

    public enum TestEnumBI32 : int {
        a1, b1, c1 
    }
        
    public enum TestEnumBI16 : short {
        a2, b2, c2
    }    
    
    public enum TestEnumBI64 : long {
        a3, b3, c3
    }
    
    public enum TestEnumBB : byte {
        a4, b4, c4
    }
    
    [CLSCompliant(false)]
    public enum TestEnumBSB : sbyte {
        a5, b5, c5
    }
    
    [CLSCompliant(false)]
    public enum TestEnumBUI16 : ushort {
        a6, b6, c6
    }
    
    [CLSCompliant(false)]
    public enum TestEnumBUI32 : uint {
        a7, b7, c7
    }
    
    [CLSCompliant(false)]
    public enum TestEnumBUI64 : ulong {
        a8, b8, c8
    }    
    
    [Flags]
    public enum TestFlagsBI32 : int {
        a1, b1, c1 
    }
    
    [Flags]
    public enum TestFlagsBI16 : short {
        a2, b2, c2
    }    
    
    [Flags]
    public enum TestFlagsBI64 : long {
        a3, b3, c3
    }
    
    [Flags]
    public enum TestFlagsBB : byte {
        a4, b4, c4
    }
    
    [CLSCompliant(false)]
    [Flags]
    public enum TestFlagsBSB : sbyte {
        a5, b5, c5
    }
    
    [CLSCompliant(false)]
    [Flags]
    public enum TestFlagsBUI16 : ushort {
        a6, b6, c6
    }
    
    [CLSCompliant(false)]
    [Flags]
    public enum TestFlagsBUI32 : uint {
        a7, b7, c7
    }
    
    [CLSCompliant(false)]
    [Flags]
    public enum TestFlagsBUI64 : ulong {
        a8, b8, c8
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
    [TestFixture]
    public class ClsToIdlMapperTest {
        
        #region SFields
        
        private static MappingToAction s_testAction = new MappingToAction();
        
        #endregion
        #region IConstructors
        
        public ClsToIdlMapperTest() {
        }

        #endregion
        #region IMethods

        [Test]
        public void TestMapToIdlVoid() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.VoidType, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlVoid, mapResult);        
        }

        [Test]
        public void TestMapToIdlOctet() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.ByteType, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlOctet, mapResult);
        }
        
        [Test]
        public void TestMapSByteToIdlOctet() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.SByteType, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlSByteEquivalent, mapResult);
        }        
        
        [Test]
        public void TestMapToIdlShort() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.Int16Type, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlShort, mapResult);            
        }

        [Test]
        public void TestMapToIdlLong() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.Int32Type, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlLong, mapResult);
        }
        
        [Test]
        public void TestMapToIdlLongLong() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.Int64Type, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlLongLong, mapResult);
        }

        [Test]        
        public void TestMapUInt16() {
            // System.UInt16 is not CLS compliant, but if used in code map it to idl ushort
            // the idl to cls compiler will map idl ushort to cls short to prevent cls compliance problems
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(UInt16), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlUShort, mapResult);
        }

        [Test]
        public void TestMapUInt32() {
            // System.UInt32 is not CLS compliant, but if used in code map it to idl ulong
            // the idl to cls compiler will map idl ulong to cls int to prevent cls compliance problems
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(UInt32), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlULong, mapResult);
        }

        [Test]
        public void TestMapUInt64() {
            // System.UInt64 is not CLS compliant, but if used in code map it to idl ulonglong
            // the idl to cls compiler will map idl ulonglong to cls long to prevent cls compliance problems
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(UInt64), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlULongLong, mapResult);
        }
        
        [Test]
        public void TestUnmappableException() {
            try {
                // System.IntPtr is not mappable, because System.IntPtr doesn't make sense outside of process
                ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
                MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(System.UIntPtr), 
                                                                               new AttributeExtCollection(),
                                                                               s_testAction);
                Assert.Fail("no exception, but expected BAD_PARAM");
            } catch (BAD_PARAM ex) {
                Assert.IsTrue(
                                 ex.Message.IndexOf("System.UIntPtr") > 0,"exception message not enough detailed");
                Assert.IsTrue(
                                 ex.Message.IndexOf("is not mappable to idl") > 0, "exception message not enough detailed");
            }
            
        }
                        
        [Test]        
        public void TestMapToIdlBoolean() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.BooleanType, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlBool, mapResult);
        }
        
        [Test]
        public void TestMapToIdlFloat() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.SingleType, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFloat, mapResult);        
        }
        
        [Test]
        public void TestMapToIdlDouble() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.DoubleType, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlDouble, mapResult);
        }

        [Test]        
        public void TestMapToIdlString() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.StringType, 
                                                                           new AttributeExtCollection(new Attribute[] { new StringValueAttribute(), new WideCharAttribute(false) }),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlString, mapResult);
            if(!MappingConfiguration.Instance.UseWideCharByDefault) {
                mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.StringType, 
                                                               new AttributeExtCollection(new Attribute[] { new StringValueAttribute() }),
                                                               s_testAction);
                Assert.AreEqual(MappingToResult.IdlString, mapResult);
            }
        }

        [Test]
        public void TestMapToIdlWString() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.StringType, 
                                                                           new AttributeExtCollection(new Attribute[] { new StringValueAttribute(), new WideCharAttribute(true) }),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlWString, mapResult);
            if(MappingConfiguration.Instance.UseWideCharByDefault) {
                mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.StringType, 
                                                               new AttributeExtCollection(new Attribute[] { new StringValueAttribute() }),
                                                               s_testAction);
                Assert.AreEqual(MappingToResult.IdlWString, mapResult);
            }
        }

        [Test]
        public void TestMapToIdlStringValue() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.StringType, 
                                                                           new AttributeExtCollection(new Attribute[] { new WideCharAttribute(false) }),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlStringValue, mapResult);
            if(!MappingConfiguration.Instance.UseWideCharByDefault) {
                mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.StringType, 
                                                               new AttributeExtCollection(),
                                                               s_testAction);
                Assert.AreEqual(MappingToResult.IdlStringValue, mapResult);
            }
        }

        [Test]
        public void TestMapToIdlWStringValue() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.StringType, 
                                                                           new AttributeExtCollection(new Attribute[] { new WideCharAttribute(true) }),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlWstringValue, mapResult);
            if(MappingConfiguration.Instance.UseWideCharByDefault) {
                mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.StringType, 
                                                               new AttributeExtCollection(),
                                                               s_testAction);
                Assert.AreEqual(MappingToResult.IdlWstringValue, mapResult);
            }
        }

        [Test]
        public void TestMapToIdlChar() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.CharType, 
                                                                           new AttributeExtCollection(new Attribute[] { new WideCharAttribute(false) }),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlChar, mapResult);
            if(!MappingConfiguration.Instance.UseWideCharByDefault) {
                mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.CharType, 
                                                               new AttributeExtCollection(),
                                                               s_testAction);
                Assert.AreEqual(MappingToResult.IdlChar, mapResult);
            }
        }

        [Test]
        public void TestMapToIdlWChar() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.CharType, 
                                                                           new AttributeExtCollection(new Attribute[] { new WideCharAttribute(true) }),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlWChar, mapResult);
            if(MappingConfiguration.Instance.UseWideCharByDefault) {
                mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.CharType, 
                                                               new AttributeExtCollection(),
                                                               s_testAction);
                Assert.AreEqual(MappingToResult.IdlWChar, mapResult);
            }
        }

        [Test]
        public void TestMapToIdlSequence() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(int[]), 
                                                                           new AttributeExtCollection(new Attribute[] { new IdlSequenceAttribute(0) } ),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlSequence, mapResult);
        }

        [Test]
        public void TestMapToIdlArray() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(int[]), 
                                                                           new AttributeExtCollection(new Attribute[] { new IdlArrayAttribute(0, 10) } ),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlArray, mapResult);

            mapResult = (MappingToResult)mapper.MapClsType(typeof(int[,]), 
                                                           new AttributeExtCollection(new Attribute[] { new IdlArrayAttribute(0, 10), new IdlArrayDimensionAttribute(0, 1, 20) } ),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlArray, mapResult);

            mapResult = (MappingToResult)mapper.MapClsType(typeof(int[,,]), 
                                                           new AttributeExtCollection(new Attribute[] { new IdlArrayAttribute(0, 10), 
                                                                                                        new IdlArrayDimensionAttribute(0, 1, 20), new IdlArrayDimensionAttribute(0, 2, 5) } ),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlArray, mapResult);
        }

        [Test]
        public void TestMapToIdlAny() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.ObjectType, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlAny, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.ObjectType, 
                                                           new AttributeExtCollection(new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.Any) }),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlAny, mapResult);
        }

        [Test]
        public void TestMapToIdlEnumFromIdlEnum() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestIdlEnumBI32), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }
        
        [Test]
        public void TestMapToIdlEnumFromClsEnumBB() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnumBB), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }        
        
        [Test]
        public void TestMapToIdlEnumFromClsEnumI16() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnumBI16), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }        

        [Test]
        public void TestMapToIdlEnumFromClsEnumI32() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnumBI32), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }        
        
        [Test]
        public void TestMapToIdlEnumFromClsEnumI64() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnumBI64), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }
        
        [Test]
        public void TestMapToIdlEnumFromClsEnumSB() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnumBSB), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }        
        
        [Test]
        public void TestMapToIdlEnumFromClsEnumUI16() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnumBUI16), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }        

        [Test]
        public void TestMapToIdlEnumFromClsEnumUI32() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnumBUI32), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }        
        
        [Test]
        public void TestMapToIdlEnumFromClsEnumUI64() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestEnumBUI64), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlEnum, mapResult);
        }
        
        [Test]
        public void TestMapToFlagsFromClsFlagsBB() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestFlagsBB), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFlagsEquivalent, mapResult);
        }        
        
        [Test]
        public void TestMapToFlagsFromClsFlagsI16() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestFlagsBI16), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFlagsEquivalent, mapResult);
        }        

        [Test]
        public void TestMapToFlagsFromClsFlagsI32() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestFlagsBI32), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFlagsEquivalent, mapResult);
        }        
        
        [Test]
        public void TestMapToFlagsFromClsFlagsI64() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestFlagsBI64), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFlagsEquivalent, mapResult);
        }
        
        [Test]
        public void TestMapToFlagsFromClsFlagsSB() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestFlagsBSB), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFlagsEquivalent, mapResult);
        }        
        
        [Test]
        public void TestMapToFlagsFromClsFlagsUI16() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestFlagsBUI16), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFlagsEquivalent, mapResult);
        }        

        [Test]
        public void TestMapToFlagsFromClsFlagsUI32() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestFlagsBUI32), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFlagsEquivalent, mapResult);
        }        
        
        [Test]
        public void TestMapToFlagsFromClsFlagsUI64() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestFlagsBUI64), 
                                                                           AttributeExtCollection.EmptyCollection,
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlFlagsEquivalent, mapResult);
        }        
        
        [Test]
        public void TestMapToIdlStruct() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestIdlStruct), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlStruct, mapResult);
        }

        [Test]
        public void TestMapToIdlUnion() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestIdlUnion), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlUnion, mapResult);
        }
        
        [Test]
        public void TestMapToIdlConcreteValueType() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestClsSerializableStruct), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlConcreteValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestClsSerializableClass), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlConcreteValue, mapResult);                        
        }
        
        [Test]
        public void TestMapToIdlAbstractValueType() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestClsNonSerializableStruct), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlAbstractValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestClsNonSerializableClass), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlAbstractValue, mapResult);    
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestAbsValInterface), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlAbstractValue, mapResult);    
        }

        [Test]        
        public void TestMapToIdlBoxedValueType() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Int16[]), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlBoxedValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestBoxedVal ), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlBoxedValue, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.Int16Type, 
                                                           new AttributeExtCollection(new Attribute[] { new BoxedValueAttribute("IDL:Ch/Elca/Iiop/Tests/TestBoxedVal:1.0") }),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlBoxedValue, mapResult);
        }

        [Test]
        public void TestMapToIdlConcreteInterface() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestRemotableByRef), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlConcreteIf, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(typeof(TestConInterface), 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlConcreteIf, mapResult);            
        }

        [Test]
        public void TestMapToIdlAbstractInterface() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestAbsInterface), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlAbstractIf, mapResult);
        }
        
        [Test]
        public void TestMapToIdlLocalInterface() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TestLocalInterface), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlLocalIf, mapResult);
        }
        
        [Test]
        public void TestMapToIdlException() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(Exception), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlException, mapResult);
        }
        
        [Test]
        public void TestMapToIdlAbstractBase() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.ObjectType, 
                                                                           new AttributeExtCollection(new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase) }),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlAbstractBase, mapResult);
        }
        
        [Test]
        public void TestMapToIdlValueBase() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.ObjectType, 
                                                                           new AttributeExtCollection(new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.ValueBase) }),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlValueBase, mapResult);
        }
        
        [Test]
        public void TestMapToIdlTypeDesc() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.TypeType, 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlTypeDesc, mapResult);
        }
        
        [Test]
        public void TestMapToIdlTypeCode() {
            ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
            MappingToResult mapResult = (MappingToResult)mapper.MapClsType(typeof(TypeCodeImpl), 
                                                                           new AttributeExtCollection(),
                                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlTypeCode, mapResult);
            mapResult = (MappingToResult)mapper.MapClsType(ReflectionHelper.CorbaTypeCodeType, 
                                                           new AttributeExtCollection(),
                                                           s_testAction);
            Assert.AreEqual(MappingToResult.IdlTypeCode, mapResult);            
        }
        
        #endregion
        
    }

}

#endif
