/* UnionGenerationHelper.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 02.10.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
    /// helper class to collect data about a parameter of an operation
    /// </summary>
    public class ParameterSpec {
        
        #region Types
        
        /// <summary>helper class for specifying parameter directions</summary>
        public class ParameterDirection {
        
            #region Constants

            private const int ParamDir_IN = 0;
            private const int ParamDir_OUT = 1;
            private const int ParamDir_INOUT = 2;

            #endregion Constants
            #region SFields
            
            public static readonly ParameterDirection s_inout = new ParameterDirection(ParamDir_INOUT);
            public static readonly ParameterDirection s_in = new ParameterDirection(ParamDir_IN);
            public static readonly ParameterDirection s_out = new ParameterDirection(ParamDir_OUT);
            
            #endregion SFields
            #region IFields
            
            private int m_direction;
            
            #endregion IFields
            #region IConstructors
                
            private ParameterDirection(int direction) {
                m_direction = direction;
            }
            
            #endregion IConstructors
            #region IMethods
            
            public bool IsInOut() {
                return (m_direction == ParamDir_INOUT);
            }

            public bool IsIn() {
                return (m_direction == ParamDir_IN);
            }

            public bool IsOut() {
                return (m_direction == ParamDir_OUT);
            }
            
            #endregion IMethods
            
        }
        
        #endregion Types
        #region IFields

        private TypeContainer m_paramType;
        private String m_paramName;
        private ParameterDirection m_direction;

        #endregion IFields
        #region IConstructors
        
        public ParameterSpec(String paramName, TypeContainer paramType, 
            ParameterDirection direction) {
            m_paramName = paramName;
            m_paramType = paramType;
            m_direction = direction;
        }
        
        /// <summary>creates an in parameterspec</summary>
        public ParameterSpec(String paramName, Type clsType) {
            m_paramName = paramName;
            m_paramType = new TypeContainer(clsType);
            m_direction = ParameterDirection.s_in;
        }
        
        /// <summary>creates a ParameterSpec for a ParameterInfo</summary>
        public ParameterSpec(ParameterInfo forParamInfo) {
            if (forParamInfo.IsOut) {
                m_direction = ParameterDirection.s_out;
            } else if (forParamInfo.ParameterType.IsByRef) {
                m_direction = ParameterDirection.s_inout;
            } else {
                m_direction = ParameterDirection.s_in;
            }            
            m_paramName = forParamInfo.Name;
            
            // custom attributes
            System.Object[] attrs = forParamInfo.GetCustomAttributes(false);
            m_paramType = new TypeContainer(forParamInfo.ParameterType,
                                            AttributeExtCollection.
                                                ConvertToAttributeCollection(attrs), 
                                            true);
        }

        #endregion IConstructors
        #region IMethods

        public String GetPramName() {
            return m_paramName;
        }
        
        public TypeContainer GetParamType() {
            return m_paramType;
        }

        /// <summary>
        /// merges the separated cls type with param direction:
        /// for inout/out parameters a ....& type is needed.
        /// </summary>
        /// <returns></returns>
        public Type GetParamTypeMergedDirection() {
            // get correct type for param direction:
            if (IsIn()) {
                return m_paramType.GetSeparatedClsType();
            } else { // out or inout parameter
                // need a type which represents a reference to the parametertype
                return ReflectionHelper.GetByRefTypeFor(m_paramType.GetSeparatedClsType());
            }
        }

        public ParameterDirection GetParamDirection() {
            return m_direction;
        }

        public bool IsInOut() {
            return m_direction.IsInOut();
        }

        public bool IsIn() {
            return m_direction.IsIn();
        }

        public bool IsOut() {
            return m_direction.IsOut();
        }

        #endregion IMethods

    }

    
    /// <summary>
    /// provides some help in generating methods / fields / ...
    /// </summary>
    public class IlEmitHelper {

        #region SFields    

        private static IlEmitHelper s_singleton = new IlEmitHelper();

        ///<summary>reference to one of the internal constructor of class ParameterInfo. 
        /// Used for assigning custom attributes to the return parameter</summary>
        private static ConstructorInfo s_paramBuildConstr;
    
        #endregion SFields
        #region IFields

        
        #endregion IFields
        #region SConstructor

        static IlEmitHelper() {
            // work around: need a way to define attributes on return parameter
            // TBD: search better way
            Type paramBuildType = typeof(ParameterBuilder);
            s_paramBuildConstr = paramBuildType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                               new Type[] { typeof(MethodBuilder), 
                                                                            ReflectionHelper.Int32Type, 
                                                                            typeof(ParameterAttributes),
                                                                            ReflectionHelper.StringType }, 
                                                               null);
        }

        #endregion SConstructor
        #region IConsturctors
    
        private IlEmitHelper() {
        }
    
        #endregion IConstructors
        #region SMethods
    
        public static IlEmitHelper GetSingleton() {
            return s_singleton;
        }
    
        #endregion SMethods
        #region IMethods
        
        private void AddFromIdlNameAttribute(MethodBuilder methodBuild, string forIdlMethodName) {
            methodBuild.SetCustomAttribute(
                new FromIdlNameAttribute(forIdlMethodName).CreateAttributeBuilder());
        }        
        
        private void AddFromIdlNameAttribute(PropertyBuilder propBuild, string forIdlAttributeName) {
            propBuild.SetCustomAttribute(
                new FromIdlNameAttribute(forIdlAttributeName).CreateAttributeBuilder());
        }        


        /// <summary>adds a method to a type, setting the attributes on the parameters</summary>
        /// <remarks>forgeign method: should be better on TypeBuilder, but not possible</remarks>
        /// <returns>the MethodBuilder for the method created</returns>
        public MethodBuilder AddMethod(TypeBuilder builder, string methodName, ParameterSpec[] parameters, 
                                       TypeContainer returnType, MethodAttributes attrs) {
        
            Type[] paramTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) { 
                paramTypes[i] = parameters[i].GetParamTypeMergedDirection();
            }
        
            MethodBuilder methodBuild = builder.DefineMethod(methodName, attrs, 
                                                             returnType.GetSeparatedClsType(),
                                                             paramTypes);
            // define the paramter-names / attributes
            for (int i = 0; i < parameters.Length; i++) {
                DefineParameter(methodBuild, parameters[i], i+1);
            }
            // add custom attributes for the return type
            ParameterBuilder paramBuild = CreateParamBuilderForRetParam(methodBuild);
            for (int i = 0; i < returnType.GetSeparatedAttrs().Length; i++) {
                paramBuild.SetCustomAttribute(returnType.GetSeparatedAttrs()[i]);
            }
            return methodBuild;
        }
        
        /// <summary>
        /// Like <see cref="Ch.Elca.Iiop.Idl.IlEmitHelper.AddMethod(TypeBuilder, string, ParameterSpec[], TypeContainer, MethodAttributes)"/>,
        /// but adds additionally a FromIdlName attribute to the method
        /// </summary>
        public MethodBuilder AddMethod(TypeBuilder builder, string clsMethodName,
                                       string forIdlMethodName,
                                       ParameterSpec[] parameters,
                                       TypeContainer returnType, MethodAttributes attrs) {
            MethodBuilder methodBuild = AddMethod(builder, clsMethodName, parameters,
                                                  returnType, attrs);
            AddFromIdlNameAttribute(methodBuild, forIdlMethodName);
            return methodBuild;
        }
        
        /// <summary>adds a constructor to a type, setting the attributes on the parameters</summary>
        /// <remarks>forgeign method: should be better on TypeBuilder, but not possible</remarks>
        /// <returns>the ConstructorBuilder for the method created</returns>
        public ConstructorBuilder AddConstructor(TypeBuilder builder, ParameterSpec[] parameters, 
                                                 MethodAttributes attrs) {
        
            Type[] paramTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) { 
                paramTypes[i] = parameters[i].GetParamTypeMergedDirection();
            }
        
            ConstructorBuilder constrBuild = 
                builder.DefineConstructor(attrs, CallingConventions.Standard,
                                          paramTypes);
            // define the paramter-names / attributes
            for (int i = 0; i < parameters.Length; i++) {
                ParameterAttributes paramAttr = ParameterAttributes.None;
                ParameterBuilder paramBuild = 
                    constrBuild.DefineParameter(i + 1, paramAttr, 
                                                parameters[i].GetPramName());
                // custom attribute spec
                TypeContainer specType = parameters[i].GetParamType();
                for (int j = 0; j < specType.GetSeparatedAttrs().Length; j++) {
                    paramBuild.SetCustomAttribute(specType.GetSeparatedAttrs()[j]);    
                }                
            }
            
            return constrBuild;
        }
        
        /// <summary>
        /// add a no arg constructor, which calls the default constructor of the supertype.
        /// </summary>
        public void AddDefaultConstructor(TypeBuilder builder,
                                          MethodAttributes attrs) {
            builder.DefineDefaultConstructor(attrs);
        }
        
        /// <summary>adds a field to a type, including the custom attributes needed</summary>
        /// <remarks>forgeign method: should be better on TypeBuilder, but not possible</remarks>
        /// <returns>the FieldBuilder for the field created</returns>
        public FieldBuilder AddFieldWithCustomAttrs(TypeBuilder builder, string fieldName, 
                                                    TypeContainer fieldType, FieldAttributes attrs) {
            // consider custom mappings
            Type clsType = fieldType.GetSeparatedClsType();
            FieldBuilder fieldBuild = builder.DefineField(fieldName, clsType, attrs);
            // add custom attributes
            for (int j = 0; j < fieldType.GetSeparatedAttrs().Length; j++) {
                fieldBuild.SetCustomAttribute(fieldType.GetSeparatedAttrs()[j]);
            }
            return fieldBuild;
        }


        /// <summary>
        /// adds a property setter method.
        /// </summary>
        /// <param name="attrs">MethodAttributes, automatically adds HideBySig and SpecialName</param>
        public MethodBuilder AddPropertySetter(TypeBuilder builder, string propertyName,
                                               TypeContainer propertyType, MethodAttributes attrs) {
            return AddPropertySetterInternal(builder, propertyName, null, propertyType, attrs);
        }
        
        /// <summary>
        /// adds a property setter method with a FromIdlName attribute 
        /// (based on the name of the property, this setter is defined for)
        /// </summary>
        /// <param name="attrs">MethodAttributes, automatically adds HideBySig and SpecialName</param>
        public MethodBuilder AddPropertySetter(TypeBuilder builder, string propertyName, 
                                               string forIdlSetterName,
                                               TypeContainer propertyType, MethodAttributes attrs) {
            return AddPropertySetterInternal(builder, propertyName, forIdlSetterName, 
                                             propertyType, attrs);
        }        
        
        /// <summary>
        /// adds a property setter method; optinally adds a FromIdlNameAttribute,
        /// if forIdlAttributeName is != null.
        /// </summary>
        private MethodBuilder AddPropertySetterInternal(TypeBuilder builder,
                                                        string propertyName,
                                                        string forIdlSetterName, 
                                                        TypeContainer propertyType, 
                                                        MethodAttributes attrs) {
            Type propTypeCls = propertyType.GetSeparatedClsType();
            MethodBuilder setAccessor = builder.DefineMethod("set_" + propertyName, 
                                                             attrs | MethodAttributes.HideBySig | MethodAttributes.SpecialName, 
                                                             null, new System.Type[] { propTypeCls });
            
            ParameterBuilder valParam = setAccessor.DefineParameter(1, ParameterAttributes.None, "value"); 
            // add custom attributes
            for (int j = 0; j < propertyType.GetSeparatedAttrs().Length; j++) {
                valParam.SetCustomAttribute(propertyType.GetSeparatedAttrs()[j]);
            }
            
            if (forIdlSetterName != null) {
                AddFromIdlNameAttribute(setAccessor, forIdlSetterName);    
            }            
            return setAccessor;                                                                                                                                    
        }

        /// <summary>
        /// adds a property getter method.
        /// </summary>
        /// <param name="attrs">MethodAttributes, automatically adds HideBySig and SpecialName</param>
        public MethodBuilder AddPropertyGetter(TypeBuilder builder, string propertyName,
                                               TypeContainer propertyType, MethodAttributes attrs) {
            return AddPropertyGetterInternal(builder, propertyName, null, propertyType, attrs);
        }
       
        /// <summary>
        /// adds a property getter method with a FromIdlName attribute 
        /// (based on the name of the property, this setter is defined for)
        /// </summary>
        /// <param name="attrs">MethodAttributes, automatically adds HideBySig and SpecialName</param>
        public MethodBuilder AddPropertyGetter(TypeBuilder builder, string propertyName,
                                               string forIdlGetterName,
                                               TypeContainer propertyType, MethodAttributes attrs) {
            return AddPropertyGetterInternal(builder, propertyName, forIdlGetterName, 
                                             propertyType, attrs);
        }

        
        private MethodBuilder AddPropertyGetterInternal(TypeBuilder builder, string propertyName,
                                                       string forIdlGetterName, 
                                                       TypeContainer propertyType, MethodAttributes attrs) {
            Type propTypeCls = propertyType.GetSeparatedClsType();
            MethodBuilder getAccessor = builder.DefineMethod("get_" + propertyName, 
                                                             attrs | MethodAttributes.HideBySig | MethodAttributes.SpecialName, 
                                                             propTypeCls, System.Type.EmptyTypes);
            
            ParameterBuilder retParamGet = CreateParamBuilderForRetParam(getAccessor);
            // add custom attributes
            for (int j = 0; j < propertyType.GetSeparatedAttrs().Length; j++) {                
                retParamGet.SetCustomAttribute(propertyType.GetSeparatedAttrs()[j]);
            }
            if (forIdlGetterName != null) {
                AddFromIdlNameAttribute(getAccessor, forIdlGetterName);    
            }
            return getAccessor;                                                           
        }

        /// <summary>
        /// adds a property to a type, including the custom attributes needed.
        /// </summary>
        public PropertyBuilder AddProperty(TypeBuilder builder, string propertyName, 
                                           TypeContainer propertyType, 
                                           MethodBuilder getAccessor, MethodBuilder setAccessor) {
            Type propTypeCls = propertyType.GetSeparatedClsType();
            PropertyBuilder propBuild = builder.DefineProperty(propertyName, PropertyAttributes.None, 
                                                               propTypeCls, System.Type.EmptyTypes);
            // add accessor methods
            if (getAccessor != null) {
                propBuild.SetGetMethod(getAccessor);
            }            
            if (setAccessor != null) {
                propBuild.SetSetMethod(setAccessor);
            }
            // define custom attributes
            for (int j = 0; j < propertyType.GetSeparatedAttrs().Length; j++) {
                propBuild.SetCustomAttribute(propertyType.GetSeparatedAttrs()[j]);                
            }
            return propBuild;
        }
        
        /// <summary>
        /// Like <see cref="Ch.Elca.Iiop.Idl.IlEmitHelper.AddProperty(TypeBuilder, string, TypeContainer, MethodBuilder, MethodBuilder)"/>,
        /// but adds additionally a FromIdlName attribute to the property
        /// </summary>
        public PropertyBuilder AddProperty(TypeBuilder builder, string clsPropertyName,
                                           string forIdlAttributeName,                                       
                                           TypeContainer propertyType, 
                                           MethodBuilder getAccessor, MethodBuilder setAccessor) {
            PropertyBuilder propBuild = AddProperty(builder, clsPropertyName,
                                                    propertyType, 
                                                    getAccessor, setAccessor);
            AddFromIdlNameAttribute(propBuild, forIdlAttributeName);
            return propBuild;
        }

        /// <summary>
        /// reefines a prameter; not possible for return parameter, ...? TODO: refact ...
        /// </summary>
        /// <param name="methodBuild"></param>
        /// <param name="spec"></param>
        /// <param name="paramNr"></param>
        private void DefineParameter(MethodBuilder methodBuild, ParameterSpec spec, int paramNr) {
            ParameterAttributes paramAttr = ParameterAttributes.None;
            if (spec.IsOut()) { 
                paramAttr = paramAttr | ParameterAttributes.Out; 
            }
            ParameterBuilder paramBuild = methodBuild.DefineParameter(paramNr, paramAttr, spec.GetPramName());
            // custom attribute spec
            TypeContainer specType = spec.GetParamType();
            for (int i = 0; i < specType.GetSeparatedAttrs().Length; i++) {
                paramBuild.SetCustomAttribute(specType.GetSeparatedAttrs()[i]);    
            }
        }

        /// <summary>
        /// need this, because define-parameter prevent creating a parameterbuilder for param-0, the ret param.
        /// For defining custom attributes on the ret-param, a parambuilder is however needed
        /// TBD: search nicer solution for this 
        /// </summary>
        /// <remarks>should be on MethodBuilder, but not possible to change MethodBuilder-class</remarks>
        private ParameterBuilder CreateParamBuilderForRetParam(MethodBuilder forMethod) {
            ParameterBuilder result = null;
            try {
                // mono allows to create the ParameterBuilder on return parameter by calling DefineParameter on MethodBuilder
                result = forMethod.DefineParameter(0, ParameterAttributes.None, null);
            } catch (ArgumentOutOfRangeException) {
                // workaround for .NET: create a new ParameterBuilder unsing reflection:
                // constructor is non-public
                result = (ParameterBuilder) s_paramBuildConstr.Invoke(new Object[] { forMethod, (System.Int32) 0,
                                                                                     ParameterAttributes.None, null } );
            }
            return result;
        }

        /// <summary>
        /// adds a serializable attribute to the type in construction
        /// </summary>        
        public void AddSerializableAttribute(TypeBuilder typebuild) {
            Type attrType = typeof(System.SerializableAttribute);
            ConstructorInfo attrConstr = attrType.GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder serAttr = new CustomAttributeBuilder(attrConstr, new Object[0]);    
            typebuild.SetCustomAttribute(serAttr);
        }        
        
        /// <summary>
        /// adds a repository id attribute to the type in construction.
        /// </summary>
        public void AddRepositoryIDAttribute(TypeBuilder typebuild, string id) {
            RepositoryIDAttribute repIdAttr = new RepositoryIDAttribute(id);
            typebuild.SetCustomAttribute(repIdAttr.CreateAttributeBuilder());            
        }
        
        /// <summary>generates il to cast/unbox a reference to the targetType
        public void GenerateCastObjectToType(ILGenerator gen, Type targetType) {
            if (targetType.IsValueType) {
                gen.Emit(OpCodes.Unbox, targetType); // get addr of value
                // for ints and floats: ldind may be used too as shortcut for ldobj, but for simplicity don't use it
                gen.Emit(OpCodes.Ldobj, targetType); // load value onto stack
            } else {
                gen.Emit(OpCodes.Castclass, targetType); // cast the reference to the correct return value
            }
        }
        
        #endregion IMethods
    
    }


}
