/* BoxedValueTypeGenerator.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 08.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using omg.org.CORBA;
using Ch.Elca.Iiop.Util;


namespace Ch.Elca.Iiop.Idl {


    /// <summary>
    /// generates boxed value types, for received CORBA requests / for CORBA replys to send
    /// It is responsible to create boxed-value types for CLS types, which are mapped to an IDL boxed value type, e.g. int[]
    /// and which are not created by the IDL to CLS compiler
    /// </summary>
    internal class BoxedValueRuntimeTypeGenerator {
        
        #region IFields

        private BoxedValueTypeGenerator m_boxedValGen = new BoxedValueTypeGenerator();
        private static BoxedValueRuntimeTypeGenerator s_runtimeGen = new BoxedValueRuntimeTypeGenerator();
        
        private AssemblyBuilder m_asmBuilder;
        private ModuleBuilder m_modBuilder;
        
        private Hashtable m_repIdsForBoxedArrays = new Hashtable();

        #endregion IFields
        #region IConstructors
        
        private BoxedValueRuntimeTypeGenerator() {
            Initalize();
        }

        #endregion IConstructors
        #region SMethods
        
        public static BoxedValueRuntimeTypeGenerator GetSingleton() {
            return s_runtimeGen;
        }

        #endregion SMethods
        #region IMethods

        private void Initalize() {
            AssemblyName asmname = new AssemblyName();
            asmname.Name = "dynBoxed";
            asmname.Version = new Version(0, 0, 0, 0);
            asmname.CultureInfo = CultureInfo.InvariantCulture;
            asmname.SetPublicKeyToken(new byte[0]);
            m_asmBuilder = System.Threading.Thread.GetDomain().
                DefineDynamicAssembly(asmname, AssemblyBuilderAccess.Run);
            m_modBuilder = m_asmBuilder.DefineDynamicModule("dynBoxed.netmodule");
        }

        /// <summar>check if the type with the name fullname is defined among the generated boxed value
        /// type. If so, return the type</summary>
        internal Type RetrieveType(string fullname) {
            lock(this) {
                return m_asmBuilder.GetType(fullname);
            }
        }

        /// <summary>get or create the boxed value type(s) for a .NET arrayType</summary>
        internal Type GetOrCreateBoxedTypeForArray(Type arrayType) {
            if (!arrayType.IsArray) { 
                // an array-type is required for calling GetOrCreateBoxedTypeForArray
                throw new INTERNAL(10050, CompletionStatus.Completed_MayBe);
            }
            lock(this) {
                // for the .NET / .NET case, check if already a boxed type has been created by the idl to cls compiler
                // the repository id, which will identify the boxed value type generated for clsArrayType
                // will be used to check, if this boxed value type has already been created by the idl to cls mapping.
                string repIdForType = (string)m_repIdsForBoxedArrays[arrayType];
                if (repIdForType == null) {
                    repIdForType = m_boxedValGen.GetRepositoryIDForBoxedArrayType(arrayType);
                    m_repIdsForBoxedArrays[arrayType] = repIdForType;
                }
                // repository knows all the types, ask for type for rep-id
                Type result = Repository.GetTypeForId(repIdForType);
                if (result == null) {
                    // no type found,
                    // create the boxed value type(s) for the array
                    TypeBuilder resultBuild = m_boxedValGen.CreateBoxedTypeForArray(arrayType, m_modBuilder, this);
                    result = resultBuild.CreateType();
                    Repository.RegisterDynamicallyCreatedType(result);
                    
                }
                return result;
            }
        }

        #endregion IMethods

    }

    
    /// <remarks>
    /// uses reflection emit to accomplish it's task
    /// </remarks>
    public class BoxedValueTypeGenerator {
        
        #region IMethods

        /// <summary>
        /// creates a boxed value type for boxed types other than native CLS arrays 
        /// (IDL Sequences are allowed)
        /// </summary>
        /// <returns>
        /// the TypeBuilder for the boxedValue-type (CreateType is not called, to allow further modifications)
        /// </returns>
        public TypeBuilder CreateBoxedType(Type toBox, ModuleBuilder modBuilder, string fullyQualifiedName,
                                           CustomAttributeBuilder[] attrsOnBoxedType) {
            TypeBuilder boxBuilder = modBuilder.DefineType(fullyQualifiedName, 
                                                           TypeAttributes.Class | TypeAttributes.Public,
                                                           ReflectionHelper.BoxedValueBaseType);
            DefineBoxedType(boxBuilder, toBox, attrsOnBoxedType);
            return boxBuilder;
        }
        
        /// <summary>create the boxed value type(s) for a .NET arrayType</summary>
        internal TypeBuilder CreateBoxedTypeForArray(Type arrayType, ModuleBuilder modBuilder,
                                                     BoxedValueRuntimeTypeGenerator gen) {
            // create the boxed value type(s) for the array
            string boxedTypeFullName = CreateBoxedArrayFullTypeName(arrayType);
            TypeBuilder boxBuilder = modBuilder.DefineType(boxedTypeFullName,
                                                           TypeAttributes.Class | TypeAttributes.Public,
                                                           ReflectionHelper.BoxedValueBaseType);
            DefineBoxedTypeForCLSArray(boxBuilder, arrayType, gen);
            return boxBuilder;
        }
        
        /// <summary>
        /// returns the repository id, which will be assigned to a boxed value type generated at runtime
        /// for a .NET array type.
        /// </summary>
        internal string GetRepositoryIDForBoxedArrayType(Type arrayType) {
            string typeName = CreateBoxedArrayFullTypeName(arrayType);
            return IdlNaming.MapFullTypeNameToIdlRepId(typeName);
        }
        
        /// <summary>creates the fully qualified type name for the box</summary>
        private string CreateBoxedArrayFullTypeName(Type arrayType) {
            Type arrayElemType = DetermineInnermostArrayElemType(arrayType);
            // unqualified name of the innermost element type of the array
            string arrayElemTypeName = IdlNaming.MapShortTypeNameToIdl(arrayElemType); // need the mapped name for identifier
            arrayElemTypeName = arrayElemTypeName.Replace(" ", "_");
            string boxUnqual = "seq" + DetermineArrayRank(arrayType) + "_" + arrayElemTypeName;
            
            string namespaceBox = arrayElemType.Namespace;
            if ((namespaceBox != null) && (namespaceBox.Length > 0)) {
                namespaceBox = "org.omg.BoxedArray." + namespaceBox;
            } else {
                namespaceBox = "org.omg.BoxedArray";
            }
            
            return namespaceBox + "." + boxUnqual;
        }
        
        /// <summary>determines the innermost array type</summary>
        private Type DetermineInnermostArrayElemType(Type arrayType) {
            Type elemType = arrayType.GetElementType();
            while (elemType.IsArray) {
                elemType = elemType.GetElementType();
            }
            return elemType;
        }

        private int DetermineArrayRank(Type arrayType) {
            // true multi-dim array
            if (!arrayType.GetElementType().IsArray) { 
                return arrayType.GetArrayRank(); 
            }
            // array of array of array of ...
            Type elemType = arrayType.GetElementType();
            int rank = 1;
            while (elemType.IsArray) {
                elemType = elemType.GetElementType();
                rank++;
            }
            return rank;
        }

        
        /// <summary>define the box-Type for the CLS arrayType</summary>
        private void DefineBoxedTypeForCLSArray(TypeBuilder boxBuilder, Type arrayType,
                                                BoxedValueRuntimeTypeGenerator gen) {            
            IlEmitHelper.GetSingleton().AddSerializableAttribute(boxBuilder);
            // add the field for the boxed value content
            FieldBuilder valField = DefineBoxedField(boxBuilder, arrayType, gen);
            // define getValue method
            DefineGetValue(boxBuilder, valField);
            // define getBoxed type methods:
            DefineGetBoxedType(boxBuilder, valField);
            DefineGetBoxedTypeAttributes(boxBuilder, valField);
            DefineGetFirstNonBoxedType(boxBuilder, arrayType);
            DefineGetFirstNonBoxedTypeName(boxBuilder, arrayType);
            // define the constructors
            DefineEmptyDefaultConstr(boxBuilder);
            // define the constructor which sets the valField directly
            DefineAssignConstr(boxBuilder, valField);
            // define the constructor which transforms a .NET array to the form assignable to the valField, if types are different
            if (!valField.FieldType.Equals(arrayType)) {
                // need a constructor which transforms instance before assigning
                DefineTransformAndAssignConstrForArray(boxBuilder, valField, arrayType);
            }    
        }
        
        /// <summary>define the box-Type for the boxed type other than a CLS array</summary>
        private void DefineBoxedType(TypeBuilder boxBuilder, Type boxedType, 
                                     CustomAttributeBuilder[] attrsOnBoxedType) {
            IlEmitHelper.GetSingleton().AddSerializableAttribute(boxBuilder);
            // add the field for the boxed value content
            FieldBuilder valField = DefineBoxedField(boxBuilder, boxedType, attrsOnBoxedType);
            // define getValue method
            DefineGetValue(boxBuilder, valField);
            // define getBoxed type methods:
            DefineGetBoxedType(boxBuilder, valField);
            DefineGetBoxedTypeAttributes(boxBuilder, valField);
            Type fullUnboxed = boxedType;
            if ((fullUnboxed.IsArray) && (fullUnboxed.GetElementType().IsSubclassOf(ReflectionHelper.BoxedValueBaseType))) {
                // call GetFirstNonBoxed static method on element type 
                try {
                    Type unboxedElemType = (Type)fullUnboxed.GetElementType().InvokeMember(
                        BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME, 
                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Static | BindingFlags.DeclaredOnly,
                        null, null, new object[0]);
                    Array dummyArray = Array.CreateInstance(unboxedElemType, 0);
                    fullUnboxed = dummyArray.GetType();
                } catch (Exception) {
                    // invalid type found in boxed value creation: " + fullUnboxed
                    throw new INTERNAL(10045, CompletionStatus.Completed_MayBe);
                }
                
            } else if (fullUnboxed.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) {
                // call GetFirstNonBoxed static method on type 
                try {
                    fullUnboxed = (Type)fullUnboxed.InvokeMember(
                        BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME, 
                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Static | BindingFlags.DeclaredOnly, 
                        null, null, new object[0]);
                } catch (Exception) {
                    // invalid type found: fullUnboxed,
                    // static method missing or not callable:
                    // BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME
                    throw new INTERNAL(10044, CompletionStatus.Completed_MayBe);
                }
            }

            if ((fullUnboxed.IsArray) && (fullUnboxed.GetElementType().IsArray))  { 
                // add a constructor, which takes a CLS array (with an element type which is also an array) and creates the boxed value type for the instance
                // for arrays with an element type, which is not an array, such a constructor already exists
                DefineTransformAndAssignConstrForArray(boxBuilder, valField, fullUnboxed);
            }
            
            if ((boxedType.IsArray) && (!boxedType.GetElementType().IsArray) && 
                (boxedType.GetElementType().IsSubclassOf(ReflectionHelper.BoxedValueBaseType))) {
                
                Type boxedElemType;
                try {
                    // get the type boxed in the element
                    boxedElemType = (Type)boxedType.GetElementType().InvokeMember(
                        BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME, 
                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Static | BindingFlags.DeclaredOnly, 
                        null, null, new object[0]);
                } catch (Exception) {
                    // invalid type found: boxedType.GetElementType(),
                    // static method missing or not callable:
                    // BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME
                    throw new INTERNAL(10044, CompletionStatus.Completed_MayBe);
                }

                if (!boxedElemType.IsArray) {
                    // The boxed type which should be defined, has type of the following form boxed:
                    // a sequence of BoxedValues, but these boxed values do not box arrays itself 
                    // for this boxed type an additional transform constructor is needed, which boxes the elements
                    // of the sequence
                    Array boxedInnerArray = Array.CreateInstance(boxedElemType, 0);
                    DefineTransformAndAssignConstrForArray(boxBuilder, valField, boxedInnerArray.GetType());
                }

            }

            if ((!boxedType.IsArray) && (boxedType.IsSubclassOf(ReflectionHelper.BoxedValueBaseType))) {
                // The boxed value type boxes another boxed value type --> need a transform constructor,
                // which takes an unboxed value and boxes it, before assigning it to the field
                // TODO: implement this
                throw new NO_IMPLEMENT(12345, CompletionStatus.Completed_MayBe);
            }

            DefineGetFirstNonBoxedType(boxBuilder, fullUnboxed);
            DefineGetFirstNonBoxedTypeName(boxBuilder, fullUnboxed);
            // define the constructors
            DefineEmptyDefaultConstr(boxBuilder);
            // define the constructor which sets the valField directly
            DefineAssignConstr(boxBuilder, valField);
        }

        
        /// <summary>
        /// used while generating a boxed value type for a native cls type, i.e.
        /// no attributes other than a single IdlSequenceAttribute is needed on boxed field.
        /// </summary>
        private FieldBuilder DefineBoxedField(TypeBuilder boxBuilder, Type boxedType,
                                              BoxedValueRuntimeTypeGenerator gen) {
            Type fieldType = boxedType; 
            if (boxedType.IsArray) {
                if (boxedType.GetElementType().IsArray) {
                    // recursive boxing needed: fieldType is an array of boxed values
                    fieldType = Array.CreateInstance(gen.GetOrCreateBoxedTypeForArray(boxedType.GetElementType()), 0).GetType();
                } else {
                    // last step, can box directly
                    fieldType = boxedType;
                }
            }
            // create the field for the boxed value
            FieldBuilder fieldBuild = boxBuilder.DefineField("m_val", fieldType, FieldAttributes.Private);
            if (boxedType.IsArray) {
                CustomAttributeBuilder attrBuilder = (new IdlSequenceAttribute(0)).CreateAttributeBuilder();
                fieldBuild.SetCustomAttribute(attrBuilder);
            }
            return fieldBuild;
        }
        
        /// <summary>adds the field for the boxed value content</summary>
        /// <returns>the resulting fieldBuilder</returns>
        private FieldBuilder DefineBoxedField(TypeBuilder boxBuilder, Type boxedType, CustomAttributeBuilder[] attrsOnBoxedType) {
            Type fieldType = boxedType; 
            if (boxedType.IsArray) {
                if (boxedType.GetElementType().IsArray) {
                    // boxed Type not supported:  boxedType, because it's a nested array
                    throw new INTERNAL(10052, CompletionStatus.Completed_MayBe);
                }
            }
            // create the field for the boxed value
            FieldBuilder fieldBuild = boxBuilder.DefineField("m_val", fieldType, FieldAttributes.Private);
            for (int i = 0; i < attrsOnBoxedType.Length; i++) {
                fieldBuild.SetCustomAttribute(attrsOnBoxedType[i]);
            }
            return fieldBuild;
        }

        /// <summary>adds the getValue method needed by the generic unbox operation</summary>
        private void DefineGetValue(TypeBuilder builder, FieldBuilder valField) {
            MethodBuilder getMethodBuilder = builder.DefineMethod("GetValue", 
                                                                  MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, 
                                                                  ReflectionHelper.ObjectType, new Type[0]);
            ILGenerator bodyGen = getMethodBuilder.GetILGenerator();
            bodyGen.Emit(OpCodes.Ldarg_0); // load this
            bodyGen.Emit(OpCodes.Ldfld, valField); // load the field m_val
            if (valField.FieldType.IsValueType) {
                // need to box, because formal return parameter is object
                bodyGen.Emit(OpCodes.Box, valField.FieldType);
            }            
            bodyGen.Emit(OpCodes.Ret);
            // DefineMethodOverride is not callable, override is default, DefineMethodOverride is only used with interfaces to specify the method to override --> DefineMethodOveride is only useful, when no entry in the vtable in a base class exists for the method
        }

        /// <summary>define the static method, which returns the type of the boxed value in this boxed value type</summary>
        private void DefineGetBoxedType(TypeBuilder boxBuilder, FieldBuilder valField) {
            MethodBuilder getMethodBuilder = boxBuilder.DefineMethod(BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME,
                                                                     MethodAttributes.Static | MethodAttributes.Public |
                                                                        MethodAttributes.HideBySig,
                                                                     ReflectionHelper.ObjectType, new Type[0]);
            ILGenerator bodyGen = getMethodBuilder.GetILGenerator();
            bodyGen.Emit(OpCodes.Ldtoken, valField.FieldType); // load token for the field-type
            // now use Type.GetTypeFromHandle to get a Type-object
            MethodInfo getTypeFromH = ReflectionHelper.TypeType.GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            bodyGen.Emit(OpCodes.Call, getTypeFromH); // call the static method --> therefore no need to push this to the stack
            bodyGen.Emit(OpCodes.Ret); // return the type
        }

        /// <summary>define the static method, which returns the full unboxed type of the boxed value in this boxed value type</summary>
        private void DefineGetFirstNonBoxedType(TypeBuilder boxBuilder, Type fullUnboxed) {
            MethodBuilder getMethodBuilder = boxBuilder.DefineMethod(BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME,
                                                                     MethodAttributes.Static | MethodAttributes.Public |
                                                                        MethodAttributes.HideBySig,
                                                                     ReflectionHelper.ObjectType, new Type[0]);
            ILGenerator bodyGen = getMethodBuilder.GetILGenerator();
            bodyGen.Emit(OpCodes.Ldtoken, fullUnboxed); // load token for the field-type
            // now use Type.GetTypeFromHandle to get a Type-object
            MethodInfo getTypeFromH = ReflectionHelper.TypeType.GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            bodyGen.Emit(OpCodes.Call, getTypeFromH); // call the static method --> therefore no need to push this to the stack
            bodyGen.Emit(OpCodes.Ret); // return the type                
        }
        
        /// <summary>define the static method, which returns the full unboxed type name of the boxed value in this boxed value type</summary>
        private void DefineGetFirstNonBoxedTypeName(TypeBuilder boxBuilder, Type fullUnboxed) {
            MethodBuilder getMethodBuilder = boxBuilder.DefineMethod(BoxedValueBase.GET_FIRST_NONBOXED_TYPENAME_METHODNAME,
                                                                     MethodAttributes.Static | MethodAttributes.Public |
                                                                        MethodAttributes.HideBySig,
                                                                     ReflectionHelper.StringType, new Type[0]);
            ILGenerator bodyGen = getMethodBuilder.GetILGenerator();
            bodyGen.Emit(OpCodes.Ldstr, fullUnboxed.FullName); // load name for the field-type
            bodyGen.Emit(OpCodes.Ret); // return the type-name
        }

        /// <summary>
        /// creates a static method, which returns an object[] of attributes of the boxed type
        /// </summary>
        /// <param name="boxBuilder">the boxed type builder</param>
        /// <param name="valField">the field containing the boxed instance</param>
        private void DefineGetBoxedTypeAttributes(TypeBuilder boxBuilder, FieldBuilder valField) {
            MethodBuilder getMethodBuilder = boxBuilder.DefineMethod(BoxedValueBase.GET_BOXED_TYPE_ATTRIBUTES_METHOD_NAME,
                                                                     MethodAttributes.Static | MethodAttributes.Public |
                                                                        MethodAttributes.HideBySig,
                                                                     ReflectionHelper.ObjectArrayType, new Type[0]);
            ILGenerator bodyGen = getMethodBuilder.GetILGenerator();
            bodyGen.Emit(OpCodes.Ldtoken, boxBuilder); // load token for the boxed type
            // now use Type.GetTypeFromHandle to get a Type-object
            MethodInfo getTypeFromH = ReflectionHelper.TypeType.GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            bodyGen.Emit(OpCodes.Call, getTypeFromH); // call the static method --> therefore no need to push this to the stack
            // now use GetField to get the field type, this is on stack: the Type-object
            MethodInfo getFieldMethod = ReflectionHelper.TypeType.GetMethod("GetField", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                                                               new Type[] { ReflectionHelper.StringType, typeof(BindingFlags) }, null);
            bodyGen.Emit(OpCodes.Ldstr, valField.Name);
            bodyGen.Emit(OpCodes.Ldc_I4, (Int32)(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            bodyGen.EmitCall(OpCodes.Callvirt, getFieldMethod, null);
            // now call GetCustomAttributes on field, args: methodInfo, true
            bodyGen.Emit(OpCodes.Ldc_I4_1);
            MethodInfo getCustomAttrsMethod = typeof(FieldInfo).GetMethod("GetCustomAttributes", BindingFlags.Public | BindingFlags.Instance, null, 
                                                                          new Type[] { ReflectionHelper.BooleanType }, null);
            bodyGen.EmitCall(OpCodes.Callvirt, getCustomAttrsMethod, null);
            bodyGen.Emit(OpCodes.Ret); // return the array of attributes
        }


        /// <summary>adds an empty default constructor</summary>
        private void DefineEmptyDefaultConstr(TypeBuilder boxBuilder) {
            boxBuilder.DefineDefaultConstructor(MethodAttributes.Public);
        }

        /// <summary>defines the constructor, which sets the valField</summary>
        private void DefineAssignConstr(TypeBuilder boxBuilder, FieldBuilder valField) {
            ConstructorBuilder assignConstrBuilder = boxBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { valField.FieldType } );
            ILGenerator bodyGen = assignConstrBuilder.GetILGenerator();
            // call the base constructor with no args
            bodyGen.Emit(OpCodes.Ldarg_0); // load this
            ConstructorInfo baseConstr = ReflectionHelper.BoxedValueBaseType.GetConstructor(Type.EmptyTypes);
            bodyGen.Emit(OpCodes.Call, baseConstr);
            // check if arg is null (if it's a reference type) --> ArgumentException
            if (!valField.FieldType.IsValueType) {
                Label afterNullTest = bodyGen.DefineLabel();
                bodyGen.Emit(OpCodes.Ldarg_1); // load the parameter
                bodyGen.Emit(OpCodes.Brtrue_S, afterNullTest); // branch to after the null test if not null
                bodyGen.Emit(OpCodes.Ldstr, "boxed-value-constr: arg may not be null");
                ConstructorInfo argExConstr = typeof(ArgumentException).GetConstructor(new Type[] { ReflectionHelper.StringType } );
                bodyGen.Emit(OpCodes.Newobj, argExConstr); // create an argument exception instance
                bodyGen.Emit(OpCodes.Throw); // throw the exception
                bodyGen.MarkLabel(afterNullTest); // set the afterNullTest Label position to after the null test block
            }
            // set the field
            bodyGen.Emit(OpCodes.Ldarg_0); // load this
            bodyGen.Emit(OpCodes.Ldarg_1); // load the arg, which should be stored in the field valField
            bodyGen.Emit(OpCodes.Stfld, valField); // store
            bodyGen.Emit(OpCodes.Ret); // return
        }

        /// <summary>defines a constructor which takes a .NET array and transforms it to an instance of type assignable to the valField</summary>
        /// <remarks>this constructor is needed for automatic boxing support while serializing, e.g. an int[][] should be boxed: 
        /// in this case, a tansformation is needed: box the inner arrays in seq1_long --> seq1_long[] </remarks>
        private void DefineTransformAndAssignConstrForArray(TypeBuilder boxBuilder, FieldBuilder valField, Type arrayType) {
            ConstructorBuilder assignConstrBuilder = boxBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { arrayType } );
            ILGenerator bodyGen = assignConstrBuilder.GetILGenerator();
            // define one local variable:
            bodyGen.DeclareLocal(ReflectionHelper.ObjectType);
            // call the base constructor with no args
            bodyGen.Emit(OpCodes.Ldarg_0); // load this
            ConstructorInfo baseConstr = ReflectionHelper.BoxedValueBaseType.GetConstructor(Type.EmptyTypes);
            bodyGen.Emit(OpCodes.Call, baseConstr);
                
            // check if arg is null --> ArgumentException
            Label afterNullTest = bodyGen.DefineLabel();
            bodyGen.Emit(OpCodes.Ldarg_1); // load the parameter
            bodyGen.Emit(OpCodes.Brtrue_S, afterNullTest); // branch to after the null test if not null
            bodyGen.Emit(OpCodes.Ldstr, "boxed-value-constr: arg may not be null");
            ConstructorInfo argExConstr = typeof(ArgumentException).GetConstructor(new Type[] { ReflectionHelper.StringType } );
            bodyGen.Emit(OpCodes.Newobj, argExConstr); // create an argument exception instance
            bodyGen.Emit(OpCodes.Throw); // throw the exception
            bodyGen.MarkLabel(afterNullTest); // set the afterNullTest Label position to after the null test block
            // transform the arg
            // prepare arguments for BoxedArrayHelper.boxOneDimArray, first argument is type of the value box, in which the array should be boxed: --> val=this.GetType()
            bodyGen.Emit(OpCodes.Ldarg_0); // load this: hidden paramter for GetType()
            MethodInfo getTypeInfo = ReflectionHelper.ObjectType.GetMethod("GetType", BindingFlags.Public | BindingFlags.Instance);
            bodyGen.Emit(OpCodes.Call, getTypeInfo); // call this.GetType()
            // second argument for BoxedArrayHelper.boxOneDimArray: the array to box
            bodyGen.Emit(OpCodes.Ldarg_1); // load the constr. arg
            // call BoxedArrayHelper.boxOneDimArray
            MethodInfo boxOneDim = typeof(BoxedArrayHelper).GetMethod(BoxedArrayHelper.BOX_ONEDIM_ARRAY_METHODNAME,
                                                                      BindingFlags.Public | BindingFlags.NonPublic |
                                                                          BindingFlags.Static | BindingFlags.DeclaredOnly,
                                                                      null, new Type[] { ReflectionHelper.TypeType, ReflectionHelper.ObjectType },
                                                                      new ParameterModifier[0]);
            bodyGen.Emit(OpCodes.Call, boxOneDim); // call the static method
            // store result in local.0
            bodyGen.Emit(OpCodes.Stloc_0);
            
            bodyGen.Emit(OpCodes.Ldarg_0); // load this
            bodyGen.Emit(OpCodes.Ldloc_0); // load loc.0 for casting and setting
            // cast to the type of the field:
            bodyGen.Emit(OpCodes.Castclass, valField.FieldType);
            // set the field
            bodyGen.Emit(OpCodes.Stfld, valField); // store
            bodyGen.Emit(OpCodes.Ret); // return
        }

        #endregion IMethods

    }

}
