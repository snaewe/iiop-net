/* ReflectUtil.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 10.09.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Idl;
using System.Collections;
using System.Collections.Generic;

namespace Ch.Elca.Iiop.Util {
    
    /// <summary>
    /// Adds some missing reflection functionalty.
    /// </summary>
    public sealed class ReflectionHelper {
        
        #region SFields
        
        private static Type s_iIdlEntityType = typeof(IIdlEntity);
        private static Type s_iCustomMarshalledType = typeof(Ch.Elca.Iiop.Marshalling.ICustomMarshalled);
        private static Type s_corbaTypeCodeType = typeof(omg.org.CORBA.TypeCode);
        private static Type s_boxedValueBaseType = typeof(BoxedValueBase);
        private static Type s_iobjectType = typeof(omg.org.CORBA.IObject);
        private static Type s_IorProfileType = typeof(Ch.Elca.Iiop.CorbaObjRef.IorProfile);
        private static Type s_iSerializableType = typeof(System.Runtime.Serialization.ISerializable);
        private static Type s_anyType = typeof(omg.org.CORBA.Any);
        private static Type s_stringValueType = typeof(omg.org.CORBA.StringValue);
        private static Type s_wstringValueType = typeof(omg.org.CORBA.WStringValue);
        
        private static Type s_idlEnumAttributeType = typeof(IdlEnumAttribute);
        private static Type s_implClassAttributeType = typeof(ImplClassAttribute);
        private static Type s_idlSequenceAttributeType = typeof(IdlSequenceAttribute);
        private static Type s_idlArrayAttributeType = typeof(IdlArrayAttribute);
        private static Type s_idlStructAttrType = typeof(IdlStructAttribute);
        private static Type s_idlUnionAttrType = typeof(IdlUnionAttribute);
        private static Type s_wideCharAttrType = typeof(WideCharAttribute);
        private static Type s_stringValueAttrType = typeof(StringValueAttribute);
        private static Type s_fromIdlNameAttributeType = typeof(FromIdlNameAttribute);
        private static Type s_throwsIdlExceptionAttributeType = typeof(ThrowsIdlExceptionAttribute);
        private static Type s_oneWayAttributeType = typeof(System.Runtime.Remoting.Messaging.OneWayAttribute);
        private static Type s_iOrderedAttributeType = typeof(IOrderedAttribute);
        private static Type s_contextElementAttributeType = typeof(ContextElementAttribute);
        private static Type s_supportedInterfaceAttributeType = typeof(SupportedInterfaceAttribute);
        private static Type s_repositoryIdAttributeType = typeof(RepositoryIDAttribute);
        private static Type s_flagsAttributeType = typeof(FlagsAttribute);
        private static Type s_explicitSerializationOrderNrType = typeof(ExplicitSerializationOrderNr);
        private static Type s_explicitSerializationOrderedType = typeof(ExplicitSerializationOrdered);

        private static Type s_voidType = typeof(void);
        private static Type s_stringType = typeof(System.String);
        private static Type s_charType = typeof(System.Char);
        private static Type s_int16Type = typeof(System.Int16);
        private static Type s_int32Type = typeof(System.Int32);
        private static Type s_int64Type = typeof(System.Int64);
        private static Type s_uInt16Type = typeof(System.UInt16);
        private static Type s_uInt32Type = typeof(System.UInt32);
        private static Type s_uInt64Type = typeof(System.UInt64);
        private static Type s_booleanType = typeof(System.Boolean);
        private static Type s_byteType = typeof(System.Byte);
        private static Type s_sByteType = typeof(System.SByte);
        private static Type s_singleType = typeof(System.Single);
        private static Type s_doubleType = typeof(System.Double);
        private static Type s_dateTimeType = typeof(System.DateTime);

        private static Type s_objectType = typeof(System.Object);
        private static Type s_objectArrayType = typeof(System.Object[]);
        private static Type s_valueTypeType = typeof(System.ValueType);
        private static Type s_marshalByRefType = typeof(MarshalByRefObject);
        private static Type s_typeType = typeof(System.Type);

        
        #endregion SFields
        #region IConstructors

        private ReflectionHelper() {
        }

        #endregion IConstructors
        #region SProperties
        
        /// <summary>caches typeof(IIdlEntity)</summary>
        public static Type IIdlEntityType {
            get {
                return s_iIdlEntityType;
            }
        }
        
        /// <summary>caches typeof(Ch.Elca.Iiop.Marshalling.ICustomMarshalled)</summary>
        public static Type ICustomMarshalledType {
            get {
                return s_iCustomMarshalledType;
            }
        }
               
        /// <summary>caches typeof(omg.org.CORBA.TypeCode)</summary>
        public static Type CorbaTypeCodeType {
            get {
                return s_corbaTypeCodeType;
            }
        }
        
        /// <summary>caches typeof(Ch.Elca.Iiop.Idl.BoxedValueBase)</summary>
        public static Type BoxedValueBaseType {
            get {
                return s_boxedValueBaseType;
            }
        }
        
        /// <summary>caches typeof(omg.org.CORBA.IObject)</summary>
        public static Type IObjectType {
            get {
                return s_iobjectType;
            }
        }
        
        /// <summary>caches typeof(Ch.Elca.Iiop.CorbaObjRef.IorProfile)</summary>
        public static Type IorProfileType {
            get {
                return s_IorProfileType;
            }
        }
        
        /// <summary>caches typeof(System.Runtime.Serialization.ISerializable)</summary>
        public static Type ISerializableType {
            get {
                return s_iSerializableType;
            }
        }

        /// <summary>caches typeof(omg.org.CORBA.Any)</summary>
        public static Type AnyType {
            get {
                return s_anyType;
            }
        }                

        /// <summary>caches typeof(omg.org.CORBA.StringValue)</summary>
        public static Type StringValueType {
            get {
                return s_stringValueType;
            }
        }
        
        /// <summary>caches typeof(omg.org.CORBA.WStringValue)</summary>
        public static Type WStringValueType {
            get {
                return s_wstringValueType;
            }
        }
        
        /// <summary>caches typeof(IdlEnumAttribute)</summary>        
        public static Type IdlEnumAttributeType {
            get {
                return s_idlEnumAttributeType;
            }
        }
        
        /// <summary>caches typeof(ImplClassAttribute)</summary>        
        public static Type ImplClassAttributeType {
            get {
                return s_implClassAttributeType;
            }
        }
        
        /// <summary>caches typeof(IdlSequenceAttribute)</summary>        
        public static Type IdlSequenceAttributeType {
            get {
                return s_idlSequenceAttributeType;
            }
        }

        /// <summary>caches typeof(IdlArrayAttribute)</summary>        
        public static Type IdlArrayAttributeType {
            get {
                return s_idlArrayAttributeType;
            }
        }
        
        /// <summary>caches typeof(IdlStructAttribute)</summary>        
        public static Type IdlStructAttributeType {
            get {
                return s_idlStructAttrType;
            }
        }
        
        /// <summary>caches typeof(IdlUnionAttribute)</summary>
        public static Type IdlUnionAttributeType {
            get {
                return s_idlUnionAttrType;
            }
        }
        
        /// <summary>caches typeof(WideCharAttribute)</summary>
        public static Type WideCharAttributeType {
            get {
                return s_wideCharAttrType;
            }
        }
        
        /// <summary>caches typeof(StringValueAttribute)</summary>
        public static Type StringValueAttributeType {
            get {
                return s_stringValueAttrType;
            }
        }
        
        /// <summary>caches typeof(FromIdlNameAttribute)</summary>        
        public static Type FromIdlNameAttributeType {
            get {
                return s_fromIdlNameAttributeType;
            }
        }
        
        /// <summary>caches typeof(ThrowsIdlExceptionAttribute)</summary>
        public static Type ThrowsIdlExceptionAttributeType {
            get {
                return s_throwsIdlExceptionAttributeType;
            }
        }
        
        /// <summary>caches typeof(System.Runtime.Remoting.Messaging.OneWayAttribute)</summary>
        public static Type OneWayAttributeType {
            get {
                return s_oneWayAttributeType;
            }
        }
        
        /// <summary>chaches typeof(IOrderedAttribute)</summary>
        public static Type IOrderedAttributeType {
            get {
                return s_iOrderedAttributeType;
            }
        }


        public static Type ContextElementAttributeType {
            get {
                return s_contextElementAttributeType;
            }
        }
        
        /// <summary>
        /// caches typeof(Ch.Elca.Iiop.Idl.SupportedInterfaceAttribute)
        /// </summary>
        public static Type SupportedInterfaceAttributeType {
            get {
                return s_supportedInterfaceAttributeType;
            }
        }
        
        /// <summary>
        /// caches typeof(Ch.Elca.Iiop.Idl.RepositoryIDAttribute)
        /// </summary>        
        public static Type RepositoryIDAttributeType {
            get {
                return s_repositoryIdAttributeType;
            }
        }        

        /// <summary>
        /// caches typeof(FlagsAttribute)
        /// </summary>        
        public static Type FlagsAttributeType {
            get {
                return s_flagsAttributeType;
            }
        }
        
        /// <summary>
        /// caches typeof(ExplicitSerializationOrderNr)
        /// </summary>
        public static Type ExplicitSerializationOrderNrType {
            get {        
                return s_explicitSerializationOrderNrType;
            }
        }
        
        /// <summary>
        /// caches typeof(ExplicitSerializationOrdered)
        /// </summary>
        public static Type ExplicitSerializationOrderedType {
            get {        
                return s_explicitSerializationOrderedType;
            }
        }

        /// <summary>caches typeof(void)</summary>
        public static Type VoidType {
            get {
                return s_voidType;               
            }
        }
        
        /// <summary>caches typeof(System.Char)</summary>
        public static Type CharType {
            get {
                return s_charType;
            }
        }
                
        /// <summary>caches typeof(System.String)</summary>
        public static Type StringType {
            get {
                return s_stringType;
            }
        }                
        
        /// <summary>caches typeof(Int16)</summary>
        public static Type Int16Type {
            get {
                return s_int16Type;
            }
        }
        
        /// <summary>caches typeof(Int32)</summary>
        public static Type Int32Type {
            get {
                return s_int32Type;
            }
        }
        
        /// <summary>caches typeof(Int64)</summary>
        public static Type Int64Type {
            get {
                return s_int64Type;
            }
        }

        /// <summary>caches typeof(UInt16)</summary>
        public static Type UInt16Type {
            get {
                return s_uInt16Type;
            }
        }

        /// <summary>caches typeof(UInt32)</summary>
        public static Type UInt32Type {
            get {
                return s_uInt32Type;
            }
        }

        /// <summary>caches typeof(UInt64)</summary>
        public static Type UInt64Type {
            get {
                return s_uInt64Type;
            }
        }

        /// <summary>caches typeof(Byte)</summary>
        public static Type ByteType {
            get {
                return s_byteType;
            }
        }

        /// <summary>caches typeof(SByte)</summary>
        public static Type SByteType {
            get {
                return s_sByteType;
            }
        }

        /// <summary>caches typeof(Boolean)</summary>
        public static Type BooleanType {
            get {
                return s_booleanType;
            }
        }
        
        /// <summary>caches typeof(Single)</summary>
        public static Type SingleType {
            get {
                return s_singleType;
            }
        }
        
        /// <summary>caches typof(Double)</summary>
        public static Type DoubleType {
            get {
                return s_doubleType;
            }
        }
        
        /// <summary>caches typeof(Object)</summary>
        public static Type ObjectType {
            get {
                return s_objectType;
            }
        }
        
        /// <summary>caches typeof(object[])</summary>
        public static Type ObjectArrayType {
            get {
                return s_objectArrayType;
            }
        }
        
        /// <summary>caches typeof(ValueType)</summary>
        public static Type ValueTypeType {
            get {
                return s_valueTypeType;
            }
        }
        
        /// <summary>caches typeof(MarshalByRefObject)</summary>
        public static Type MarshalByRefObjectType {
            get {
                return s_marshalByRefType;
            }
        }
        
        /// <summary>caches typeof(Type)</summary>
        public static Type TypeType {
            get {
                return s_typeType;
            }
        }

        /// <summary>caches typeof(DateTime)</summary>
        public static Type DateTimeType {
            get {
                return s_dateTimeType;
            }
        }
        
        #endregion SProperties        
        #region SMethods
        
        /// <summary>
        /// gets all the public instance methods for Type type.
        /// </summary>
        /// <param name="type">The type to get the methods for</param>
        /// <param name="declaredOnly">return only methods directly declared in type</param>
        /// <returns></returns>
        public static MethodInfo[] GetPublicInstanceMethods(Type type, bool declaredOnly) {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            if (declaredOnly) {
                flags |= BindingFlags.DeclaredOnly;    
            }
            return type.GetMethods(flags);
        }
        
        /// <summary>
        /// get all the instance fields directly declared in Type type.
        /// </summary>
        /// <param name="type">The type to get the fields for</param>
        /// <returns></returns>
        private static FieldInfo[] GetAllDeclaredInstanceFields(Type type) {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                                 BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetFields(flags);
        }  
        
        private static Dictionary<Type, FieldInfo[]> orderedFields = new Dictionary<Type, FieldInfo[]>();
        
        /// <summary>
        /// Get all the instance fields directly declared in Type type in the serialization order.
        /// </summary>
        /// <param name="type">The type to get the fields for</param>
        /// <remarks>the order is either defined by the ExplicitSerializationOrderNr attributes or
        /// by the field name, if implicitely ordered.</remarks>
        public static FieldInfo[] GetAllDeclaredInstanceFieldsOrdered(Type type) {
            lock(orderedFields) {
                if(orderedFields.ContainsKey(type)) {
                    return orderedFields[type];
                }
                FieldInfo[] fields = GetAllDeclaredInstanceFields(type);
                IComparer comparer;
                if (type.IsDefined(ReflectionHelper.ExplicitSerializationOrderedType, false)) {
                    comparer = ExplicitOrderFieldInfoComparer.Instance;    
                } else {
                     comparer = ImplicitOrderFieldInfoComparer.Instance;
                }
                Array.Sort(fields, comparer);            
                orderedFields.Add(type, fields);
                return fields;
            }
        }
        
        /// <summary>
        /// get the custom attributes for the Type type.
        /// </summary>
        /// <param name="type">the type to get the attributes for</param>
        /// <param name="inherit">should attributes on inherited type also be returned</param>
        /// <returns></returns>
        public static AttributeExtCollection GetCustomAttributesForType(Type type, bool inherit) {
            object[] attributes = type.GetCustomAttributes(inherit);
            return AttributeExtCollection.ConvertToAttributeCollection(attributes);
        }
        
        public static AttributeExtCollection GetCustomAttriutesForField(FieldInfo member, bool inherit) {
            object[] attributes = member.GetCustomAttributes(inherit);
            return AttributeExtCollection.ConvertToAttributeCollection(attributes);    
        }
                
        public static AttributeExtCollection GetCustomAttriutesForMethod(MethodInfo member, bool inherit) {
            return GetCustomAttriutesForMethod(member, inherit, null);
        }

        public static AttributeExtCollection GetCustomAttriutesForMethod(MethodInfo member, bool inherit,
                                                                         Type attributeType) {
            AttributeExtCollection result;
            object[] attributes;
            if (attributeType == null) {
                attributes = member.GetCustomAttributes(inherit);
            } else {
                attributes = member.GetCustomAttributes(attributeType, inherit);
            }
            result = AttributeExtCollection.ConvertToAttributeCollection(attributes);           
            if (inherit) {
                // check also for interface methods ...
                if (member.IsVirtual) {
                    // check also for attributes on interface method, if it's a implementation of an interface method
                    Type declaringType = member.DeclaringType;
                    // search interfaces for method definition
                    Type[] interfaces = declaringType.GetInterfaces();
                    foreach (Type interf in interfaces) {
                        MethodInfo found = IsMethodDefinedInInterface(member, interf);
                        if (found != null) {
                            // add attributes from interface definition if not already present                                                
                            object[] inheritedAttributes;
                            if (attributeType == null) {
                                inheritedAttributes = found.GetCustomAttributes(inherit);
                            } else {
                                inheritedAttributes = found.GetCustomAttributes(attributeType, inherit);
                            }
                            result = result.MergeMissingAttributes(inheritedAttributes);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// collects the custom attributes on the current parameter and from
        /// the paramters from inherited methods.
        /// </summary>
        /// <param name="paramInfo">the parameter to check</param>
        /// <returns>a collection of attributes</returns>
        public static AttributeExtCollection CollectParameterAttributes(ParameterInfo paramInfo, MethodInfo paramInMethod) {
            AttributeExtCollection result = AttributeExtCollection.ConvertToAttributeCollection(
                paramInfo.GetCustomAttributes(true));
            if (!paramInMethod.IsVirtual) {
                return result;
            }

            MethodInfo baseDecl = paramInMethod.GetBaseDefinition();
            if (!baseDecl.Equals(paramInMethod)) {
                // add param attributes from base definition if not already present
                ParameterInfo[] baseParams = baseDecl.GetParameters();
                ParameterInfo baseParamToConsider = baseParams[paramInfo.Position];
                result = result.MergeMissingAttributes(baseParamToConsider.GetCustomAttributes(true));
            }
            
            Type declaringType = paramInMethod.DeclaringType;
            // search interfaces for method definition
            Type[] interfaces = declaringType.GetInterfaces();
            foreach (Type interf in interfaces) {
                MethodInfo found = IsMethodDefinedInInterface(paramInMethod, interf);
                if (found != null) {
                    // add param attributes from interface definition if not already present
                    ParameterInfo[] ifParams = found.GetParameters();
                    ParameterInfo ifParamToConsider = ifParams[paramInfo.Position];
                    result = result.MergeMissingAttributes(ifParamToConsider.GetCustomAttributes(true));
                }
            }

            return result;
        }


        /// <summary>
        /// collects the custom attributes on the return parameter and from
        /// the return paramters from inherited methods.
        /// </summary>
        /// <returns>a collection of attributes</returns>
        public static AttributeExtCollection CollectReturnParameterAttributes(MethodInfo method) {
            AttributeExtCollection result = AttributeExtCollection.ConvertToAttributeCollection(
                method.ReturnTypeCustomAttributes.GetCustomAttributes(true));

            if (!method.IsVirtual) {
                return result;
            }
            
            MethodInfo baseDecl = method.GetBaseDefinition();
            if (!baseDecl.Equals(method)) {
                // add return param attributes from base definition if not already present               
                result = result.MergeMissingAttributes(baseDecl.ReturnTypeCustomAttributes.GetCustomAttributes(true));
            }
            
            Type declaringType = method.DeclaringType;
            // search interfaces for method definition
            Type[] interfaces = declaringType.GetInterfaces();
            foreach (Type interf in interfaces) {
                MethodInfo found = IsMethodDefinedInInterface(method, interf);
                if (found != null) {
                    // add return param attributes from interface definition if not already present
                    result = result.MergeMissingAttributes(found.ReturnTypeCustomAttributes.GetCustomAttributes(true));
                }
            }

            return result;
                               
        }
        
        /// <summary>
        /// returns true, if a parameter is an out parameter
        /// </summary>
        public static bool IsOutParam(ParameterInfo paramInfo) {
            return paramInfo.IsOut;
        }

        /// <summary>
        /// returns true, if a parameter is a ref parameter
        /// </summary>
        public static bool IsRefParam(ParameterInfo paramInfo) {
            if (!paramInfo.ParameterType.IsByRef) { return false; }
            return (!paramInfo.IsOut) || (paramInfo.IsOut && paramInfo.IsIn);
        }

        /// <summary>
        /// returns true, if a parameter is an in parameter
        /// </summary>
        public static bool IsInParam(ParameterInfo paramInfo) {
            if (paramInfo.ParameterType.IsByRef) { return false; } // only out/ref params have a byRef type
            return ((paramInfo.IsIn) || 
                    ((!(paramInfo.IsOut)) && (!(paramInfo.IsRetval))));
        }        

        /// <summary>
        /// checks, if a similar method is defined in the interface specified;
        /// returns its MethodInfo if true, else returns null;
        /// </summary>
        /// <param name="method"></param>
        /// <param name="ifType"></param>
        /// <returns></returns>
        private static MethodInfo IsMethodDefinedInInterface(MethodInfo method, Type ifType) {            
            try {
                MethodInfo found = ifType.GetMethod(method.Name, ExtractMethodTypes(method));
                return found;
            } catch (Exception) {
                return null;
            }
        }

        private static Type[] ExtractMethodTypes(MethodInfo method) {
            ParameterInfo[] parameters = method.GetParameters();
            Type[] result = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                result[i] = parameters[i].ParameterType;
            }
            return result;
        }
        
        /// <summary>checks, if a method matching the MethodInfo method is defined on the type</summary>
        public static bool IsMethodDefinedOnType(MethodInfo method, Type type,
                                                 BindingFlags flags) {            
            MethodInfo foundMethod = type.GetMethod(method.Name, flags, null, ExtractMethodTypes(method),
                                                    null);
            return (foundMethod != null);            
        }
        
        public static bool IsPropertyDefinedOnType(PropertyInfo property, Type type,
                                                   BindingFlags flags) {
            PropertyInfo foundProperty = type.GetProperty(property.Name, flags,
                                                          null, property.PropertyType,
                                                          Type.EmptyTypes, null);
            return (foundProperty != null);
        }
        
        /// <summary>checks, if the cls method is overloaded seen from type inType</summary>
        public static bool IsMethodOverloaded(MethodInfo method, Type inType) {
            MethodInfo[] methods = inType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            int nrOfOverloads = 0;
            foreach (MethodInfo methodFound in methods) {
                if (methodFound.Name.Equals(method.Name)) {
                    nrOfOverloads++;
                }
            }
            return (nrOfOverloads > 1);
        }        
        
        /// <summary>
        /// checks, if the mehtod is already defined in a base class or an interface.
        /// </summary>
        /// <returns>true, if contained in a base class or interface, else returns false</returns>
        public static bool CheckIsMethodInInterfaceOrBase(Type type, MethodInfo method, BindingFlags flags) {
            bool result = false;
            Type baseType = type.BaseType;
            if (baseType != null) {
                result = IsMethodDefinedOnType(method, baseType, flags);
            }
            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++) {
                result = result || IsMethodDefinedOnType(method, interfaces[i], flags);
            }
            return result;
        }

        /// <summary>
        /// checks, if the property is already defined in a base class or an interface. If so, don't map it again
        /// </summary>
        /// <returns>true, if contained in a base class or interface, else returns false</returns>
        public static bool CheckIsPropertyInInterfaceOrBase(Type typeToMap, PropertyInfo prop, BindingFlags flags) {
            bool result = false;
            Type baseType = typeToMap.BaseType;
            if (baseType != null) {
                result = IsPropertyDefinedOnType(prop, baseType, flags);
            }
            Type[] interfaces = typeToMap.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++) {
                result = result || IsPropertyDefinedOnType(prop, interfaces[i], flags);
            }
            return result;
        }
        
        /// <summary>
        /// checks, if thrown is part of the raises attributes (the ThrowsIdlException attributes) of thrower
        /// </summary>        
        public static bool IsExceptionInRaiseAttributes(Exception thrown, MethodInfo thrower) {
            AttributeExtCollection methodAttributes =
                ReflectionHelper.GetCustomAttriutesForMethod(thrower, true);
            foreach (Attribute attr in methodAttributes) {
                if (ReflectionHelper.ThrowsIdlExceptionAttributeType.
                    IsAssignableFrom(attr.GetType())) {
                        if (((ThrowsIdlExceptionAttribute)attr).ExceptionType.Equals(thrown.GetType())) {
                            return true;
                        }
                    }
            }
            return false;
        }    
        
        /// <summary>generate the signature info for the method</summary>
        public static Type[] GenerateSigForMethod(MethodBase method) {
            ParameterInfo[] parameters = method.GetParameters();
            Type[] result = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {                             
                result[i] = parameters[i].ParameterType;
            }
            return result;
        }        
        
        /// <summary>
        /// gets the by ref Type for a type.
        /// </summary>
        public static Type GetByRefTypeFor(Type type) {
            // use module and not assembly here, because not fully completed types are possible here too
            return type.Module.GetType(type.FullName + "&"); // not nice, better solution ?
        }
        
        #endregion SMethods

    }


}
