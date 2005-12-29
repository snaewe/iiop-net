/* UnionGenerationHelper.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 01.10.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using omg.org.CORBA;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Idl {

    [Serializable]
    public class InvalidUnionDiscriminatorValue : Exception {
        
        public InvalidUnionDiscriminatorValue(object incompatibleValue, Type expectedType) :
            base("invalid discriminator value: " + incompatibleValue + " for type: " + expectedType) {            
        }
        
        public InvalidUnionDiscriminatorValue() {
        }
        
        protected InvalidUnionDiscriminatorValue(System.Runtime.Serialization.SerializationInfo info,
                                                 System.Runtime.Serialization.StreamingContext context) : base(info, context) {            
        }
        
        public InvalidUnionDiscriminatorValue(string reason) : base(reason) {
        }        
        
    } 
    
    public class UnionGenerationHelper {

        #region Types

        private class SwitchCase {
            #region IFields
            
            private object[] m_discriminatorValues;
            private TypeContainer m_elemType;
            private string m_elemName;           
            private FieldBuilder m_elemField = null;

            private Label m_ilLabel;

            #endregion IFields
            #region IConstrutors

            public SwitchCase(TypeContainer elemType, string elemName, object[] discriminatorValues, 
                              FieldBuilder elemField) {
                m_elemType = elemType;
                m_elemName = elemName;
                m_elemField = elemField;
                m_discriminatorValues = discriminatorValues;
            }

            #endregion IConstructors
            #region IProperties

            public TypeContainer ElemType {
                get {
                    return m_elemType;
                }
            }

            public FieldBuilder ElemField {
                get {
                    return m_elemField;
                }
            }

            public string ElemName {
                get {
                    return m_elemName;
                }
            }

            public object[] DiscriminatorValues {
                get {
                    return m_discriminatorValues;
                }
            }

            public Label IlLabelAssigned {
                get {
                    return m_ilLabel;
                }
            }

            #endregion IProperties
            #region IMethods

            public bool IsDefaultCase() {
                return IsDefaultCase(m_discriminatorValues);
            }            

            public static bool IsDefaultCase(object[] discrimValues) {
                return ((discrimValues.Length == 1) &&
                    (discrimValues[0].Equals(s_defaultCaseDiscriminator)));
            }

            public bool HasMoreThanOneDiscrValue() {
                return IsDefaultCase() || (m_discriminatorValues.Length > 1);
            }

            /// <summary>
            /// creates an IL label for this case
            /// </summary>
            /// <remarks>is not called for a default case.</remarks>
            public void AssignIlLabel(ILGenerator gen) {
                m_ilLabel = gen.DefineLabel();
            }

            #endregion IMethods
        }

        /// <summary>
        /// different actions for a switch structure for the discriminator values
        /// </summary>
        private interface GenerateUnionCaseAction {           
            /// <summary>
            /// generate code to execute for a switch case including default case
            /// </summary>                        
            void GenerateCaseAction(ILGenerator gen, SwitchCase forCase, bool isDefaultCase);
            /// <summary>
            /// generate code to execute for no default case present (instead of default case code)
            /// </summary>            
            void GenerateNoDefaultCasePresent(ILGenerator gen);      
            /// <summary>
            /// generate code to load the current discriminator value
            /// </summary>
            void GenerateLoadDiscValue(ILGenerator gen);
        }
        
        private class GenerateGetFieldInfoForDiscAction : GenerateUnionCaseAction {
            
            private UnionGenerationHelper m_ugh;
            
            internal GenerateGetFieldInfoForDiscAction(UnionGenerationHelper ugh) {
                m_ugh = ugh;
            }
            
            public void GenerateCaseAction(ILGenerator gen, SwitchCase forCase, bool isDefaultCase) {
                m_ugh.GenerateGetFieldOfUnionType(gen, forCase.ElemField);
            }
                
            public void GenerateNoDefaultCasePresent(ILGenerator gen) {
                gen.Emit(OpCodes.Ldnull);
            }
            
            public void GenerateLoadDiscValue(ILGenerator gen) {
                gen.Emit(OpCodes.Ldarg_0);
            }
            
        }
        
        private class GenerateAssignFromInfoForDiscAction : GenerateUnionCaseAction {
            
            private UnionGenerationHelper m_ugh;
            private MethodInfo m_getValueMethod;
            private MethodInfo m_getTypeFromH;            
            
            internal GenerateAssignFromInfoForDiscAction(UnionGenerationHelper ugh,
                                                         MethodInfo getValueMethod,
                                                         MethodInfo getTypeFromH) {
                m_ugh = ugh;                
                m_getValueMethod = getValueMethod;
                m_getTypeFromH = getTypeFromH;                
            }
            
            public void GenerateCaseAction(ILGenerator gen, SwitchCase forCase, bool isDefaultCase) {
                m_ugh.GenerateGetFieldFromObjectData(forCase.ElemField, gen, m_getValueMethod, m_getTypeFromH);                
            }
            public void GenerateNoDefaultCasePresent(ILGenerator gen) {
                // nothing to do
            }
            
            public void GenerateLoadDiscValue(ILGenerator gen) {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, m_ugh.m_discrField);                
            }
            
        }
        
        private class GenerateInsertIntoInfoForDiscAction : GenerateUnionCaseAction {
            
            private UnionGenerationHelper m_ugh;
            private MethodInfo m_addValueMethod;
            private MethodInfo m_getTypeFromH;
            
            internal GenerateInsertIntoInfoForDiscAction(UnionGenerationHelper ugh,
                                                         MethodInfo addValueMethod,
                                                         MethodInfo getTypeFromH) {
                m_ugh = ugh;    
                m_addValueMethod = addValueMethod;
                m_getTypeFromH = getTypeFromH;
            }            
            
            public void GenerateCaseAction(ILGenerator gen, SwitchCase forCase, bool isDefaultCase) {
                m_ugh.GenerateAddFieldToObjectData(forCase.ElemField, gen, m_addValueMethod, m_getTypeFromH);
            }
            public void GenerateNoDefaultCasePresent(ILGenerator gen) {
                // nothing to do
            }
            
            public void GenerateLoadDiscValue(ILGenerator gen) {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, m_ugh.m_discrField);
            }            
            
        }            
        
        #endregion Types        
        #region Constants

        internal const string GET_FIELD_FOR_DISCR_METHOD = "GetFieldForDiscriminator";
        internal const string GET_COVERED_DISCR_VALUES = "GetCoveredDiscrValues";
        internal const string GET_DEFAULT_FIELD = "GetDefaultField";
        internal const string DISCR_FIELD_NAME = "m_discriminator";
        internal const string ISINIT_PROPERTY_NAME = "IsInitalized";
        internal const string DISCR_PROPERTY_NAME = "Discriminator";
        internal const string INIT_FIELD_NAME = "m_initalized";

        #endregion Constants
        #region SFields

        private static object s_defaultCaseDiscriminator = new object();

        private static ConstructorInfo s_BadParamConstr;
        private static ConstructorInfo s_BadOperationConstr;

        #endregion SFields
        #region IFields

        private TypeBuilder m_builder;

        private FieldBuilder m_discrField;
        private TypeContainer m_discrType;

        private FieldBuilder m_initalizedField;
        private FieldBuilder m_unionTypeCache;
               
        private ArrayList m_switchCases = new ArrayList();

        private ArrayList m_coveredDiscrs = new ArrayList();

        private IlEmitHelper m_ilEmitHelper = IlEmitHelper.GetSingleton();

        #endregion IFields
        #region SConstructor

        static UnionGenerationHelper() {
            Type badParamType = typeof(BAD_PARAM);
            Type badOperationType = typeof(BAD_OPERATION);
            s_BadParamConstr = badParamType.GetConstructor(new Type[] { ReflectionHelper.Int32Type, typeof(CompletionStatus) });
            s_BadOperationConstr = badOperationType.GetConstructor(new Type[] { ReflectionHelper.Int32Type, typeof(CompletionStatus) });            
        }

        #endregion SConstructor
        #region IConstructors

        /// <summary>
        /// creates the union in the module represented by modBuilder.
        /// </summary>
        /// <param name="visibility">specifies the visiblity of resulting type</param>
        public UnionGenerationHelper(ModuleBuilder modBuilder, string fullName, 
                                     TypeAttributes visibility) {
            TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Serializable | 
                                       TypeAttributes.BeforeFieldInit | /* TypeAttributes.SequentialLayout | */
                                       TypeAttributes.Sealed | visibility;

            m_builder = modBuilder.DefineType(fullName, typeAttrs, ReflectionHelper.ValueTypeType,
                                              new System.Type[] { ReflectionHelper.IIdlEntityType });
            m_builder.AddInterfaceImplementation(ReflectionHelper.ISerializableType); // optimization for inter .NET communication
            BeginType();
        }
        
        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// should only be called by UnionBuildInfo constructor.
        /// </summary>
        public TypeBuilder Builder {
            get {
                return m_builder;
            }
        }

        public TypeContainer DiscriminatorType {
            get {
                return m_discrType;
            }
        }

        public static object DefaultCaseDiscriminator {
            get {
                return s_defaultCaseDiscriminator;
            }
        }

        #endregion IProperties
        #region IMethods
        
        public void AddDiscriminatorFieldAndProperty(TypeContainer discrType, ArrayList coveredDiscrRange) {
            if ((m_discrType != null) || (m_coveredDiscrs == null)) {
                throw new INTERNAL(899, CompletionStatus.Completed_MayBe);
            }
            m_discrType = discrType;
            m_coveredDiscrs = coveredDiscrRange;
            m_discrField = m_ilEmitHelper.AddFieldWithCustomAttrs(m_builder, DISCR_FIELD_NAME, m_discrType, 
                                                                  FieldAttributes.Private);
            // Property for discriminiator
            String propName = DISCR_PROPERTY_NAME;
            // set the methods for the property
            MethodBuilder getAccessor = m_ilEmitHelper.AddPropertyGetter(m_builder, propName, m_discrType,
                                                                         MethodAttributes.Public);
            ILGenerator gen = getAccessor.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, m_discrField);
            gen.Emit(OpCodes.Ret);
            MethodBuilder setAccessor = null;
            m_ilEmitHelper.AddProperty(m_builder, propName, m_discrType, getAccessor, setAccessor);
            // add a method, which returns an array of covered discriminators
            AddCoveredDiscrsGetter();
        }

        private void AddInitalizedFieldAndProperty() {
            // used to detect uninitalized unions, field is automatically initalized to false
            m_initalizedField = m_ilEmitHelper.AddFieldWithCustomAttrs(m_builder, INIT_FIELD_NAME, 
                                                                       new TypeContainer(ReflectionHelper.BooleanType), 
                                                                       FieldAttributes.Private);
            // Property for initalized -> allows to check easyily, if union is initalized
            String propName = ISINIT_PROPERTY_NAME;
            // set the methods for the property
            MethodBuilder getAccessor = 
                m_ilEmitHelper.AddPropertyGetter(m_builder, propName,
                                                 new TypeContainer(ReflectionHelper.BooleanType),
                                                 MethodAttributes.Public);
            ILGenerator gen = getAccessor.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, m_initalizedField);
            gen.Emit(OpCodes.Ret);
            MethodBuilder setAccessor = null;
            m_ilEmitHelper.AddProperty(m_builder, propName,
                                       new TypeContainer(ReflectionHelper.BooleanType), 
                                       getAccessor, setAccessor);
        }

        /// <summary>
        /// this field caches typeof(union-type)
        /// </summary>
        private void AddTypeField() {
            m_unionTypeCache = m_ilEmitHelper.AddFieldWithCustomAttrs(m_builder, "s_type", 
                                                                      new TypeContainer(ReflectionHelper.TypeType), 
                                                                      FieldAttributes.Public | FieldAttributes.Static);
        }

        /// <summary>
        /// add static constructor
        /// </summary>
        private void AddStaticConstructor() {
            ConstructorBuilder staticConstr = m_builder.DefineConstructor(MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig,
                                                                          CallingConventions.Standard,
                                                                          Type.EmptyTypes);
            ILGenerator staticConstrIl = staticConstr.GetILGenerator();
            staticConstrIl.Emit(OpCodes.Ldtoken, m_builder);
            MethodInfo getTypeFromH = ReflectionHelper.TypeType.GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            staticConstrIl.Emit(OpCodes.Call, getTypeFromH);
            // store Type in cache field
            staticConstrIl.Emit(OpCodes.Stsfld, m_unionTypeCache);
            staticConstrIl.Emit(OpCodes.Ret);
        }

        private void BeginType() {                        
            // add IdlUnion attribute
            m_builder.SetCustomAttribute(new IdlUnionAttribute().CreateAttributeBuilder());
            IlEmitHelper.GetSingleton().AddSerializableAttribute(m_builder);
            
            AddTypeField();
            AddInitalizedFieldAndProperty();
            AddStaticConstructor(); // add the static constructor
        }

        public Type FinalizeType() {
            AddOwnDefaultCaseIfNeeded();
            GenerateSerialisationHelpers();
            try {
                Type t = m_builder.CreateType();
                return t;
            } catch(InvalidOperationException ioe) {
                throw new NotSupportedException("Error in union " + m_builder.AssemblyQualifiedName, ioe);
            } catch(NotSupportedException nse) {                
                throw new NotSupportedException("Error in union " + m_builder.AssemblyQualifiedName, nse);
            }
        }

        public void GenerateSwitchCase(TypeContainer elemType, string elemDeclIdent, object[] discriminatorValues) {
            
            // generate val-field for this switch-case
            FieldBuilder elemField = m_ilEmitHelper.AddFieldWithCustomAttrs(m_builder, "m_" + elemDeclIdent,
                                                                            elemType, FieldAttributes.Private);
            SwitchCase switchCase = new SwitchCase(elemType, elemDeclIdent, discriminatorValues,
                                                   elemField);            
            // AMELIORATION possiblity: check range conflict with existing cases, before adding case
            m_switchCases.Add(switchCase);
            // generate accessor and modifier methods
            GenerateAccessorMethod(switchCase);
            GenerateModifierMethod(switchCase);
        }

        private void GenerateIsInitalized(ILGenerator gen, Label jumpOnOk) {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, m_initalizedField); // fieldvalue
            gen.Emit(OpCodes.Ldc_I4_1); // true
            gen.Emit(OpCodes.Beq, jumpOnOk);
            
            gen.Emit(OpCodes.Ldc_I4, 34);
            gen.Emit(OpCodes.Ldc_I4, (int)CompletionStatus.Completed_MayBe);
            gen.Emit(OpCodes.Newobj, s_BadOperationConstr);
            gen.Emit(OpCodes.Throw);
        }

        private void GenerateDiscrValueOkTest(ILGenerator gen, SwitchCase switchCase, Label jumpOnOk,
                                              ConstructorInfo exceptionToThrowConstr, int exceptionMinorCode) {
            // check if discr value ok, special for default case
            if (!(SwitchCase.IsDefaultCase(switchCase.DiscriminatorValues))) {
                for (int i = 0; i < switchCase.DiscriminatorValues.Length; i++) {
                    // generate compare + branch on ok to jumpOnOk
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, m_discrField); // fieldvalue of discriminator field                
                    PushDiscriminatorValueToStack(gen, switchCase.DiscriminatorValues[i]);
                    gen.Emit(OpCodes.Beq, jumpOnOk);
                }
            } else {                
                Label exceptionLabel = gen.DefineLabel();
                // compare current discr val with all covered discr val, if matching -> not ok
                foreach (object discrVal in m_coveredDiscrs) {
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, m_discrField); // fieldvalue of discriminator field                
                    PushDiscriminatorValueToStack(gen, discrVal);
                    gen.Emit(OpCodes.Beq, exceptionLabel);
                }
                // after all tests, no forbidden value for default case used -> jump to ok
                gen.Emit(OpCodes.Br, jumpOnOk);
                gen.MarkLabel(exceptionLabel);
            }
            
            // exception thrown if not ok
            gen.Emit(OpCodes.Ldc_I4, exceptionMinorCode);
            gen.Emit(OpCodes.Ldc_I4, (int)CompletionStatus.Completed_MayBe);
            gen.Emit(OpCodes.Newobj, exceptionToThrowConstr);
            gen.Emit(OpCodes.Throw);
        }

        private void GenerateAccessorMethod(SwitchCase switchCase) {
            MethodBuilder accessor = m_ilEmitHelper.AddMethod(m_builder, "Get" + switchCase.ElemName, 
                                                              new ParameterSpec[0], switchCase.ElemType, 
                                                              MethodAttributes.Public | MethodAttributes.HideBySig);
            ILGenerator gen = accessor.GetILGenerator();
            Label checkInitOk = gen.DefineLabel();
            Label checkDiscrValOk = gen.DefineLabel();
            // check if initalized
            GenerateIsInitalized(gen, checkInitOk);
            gen.MarkLabel(checkInitOk);
            GenerateDiscrValueOkTest(gen, switchCase, checkDiscrValOk, s_BadOperationConstr, 34);
            gen.MarkLabel(checkDiscrValOk);
         
            // load value and return
            gen.Emit(OpCodes.Ldarg_0); // load union this reference
            gen.Emit(OpCodes.Ldfld, switchCase.ElemField); //load the value of the union for this switch-case
            gen.Emit(OpCodes.Ret);
        }

        private void GenerateModifierMethod(SwitchCase switchCase) {
            ParameterSpec[] parameters;
            ParameterSpec valArg = new ParameterSpec("val", switchCase.ElemType, 
                                                     ParameterSpec.ParameterDirection.s_in);
            if (switchCase.HasMoreThanOneDiscrValue()) {
                // need an additional parameter
                ParameterSpec discrArg = new ParameterSpec("discrVal", DiscriminatorType, 
                                                           ParameterSpec.ParameterDirection.s_in);
                parameters = new ParameterSpec[] {  valArg, discrArg};
            } else {
                // don't need an additional parameter
                parameters = new ParameterSpec[] {  valArg };
            }
            
            MethodBuilder modifier = m_ilEmitHelper.AddMethod(m_builder, "Set" + switchCase.ElemName, 
                                                              parameters, new TypeContainer(ReflectionHelper.VoidType), 
                                                              MethodAttributes.Public | MethodAttributes.HideBySig);

            ILGenerator gen = modifier.GetILGenerator();
            
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Stfld, m_initalizedField); // store initalizedfield
            
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, switchCase.ElemField); // store value field

            gen.Emit(OpCodes.Ldarg_0);
            if (switchCase.HasMoreThanOneDiscrValue()) {
                gen.Emit(OpCodes.Ldarg_2);
            } else {
                PushDiscriminatorValueToStack(gen, switchCase.DiscriminatorValues[0]);
            }
            gen.Emit(OpCodes.Stfld, m_discrField); // store discriminator field
            
            if (switchCase.HasMoreThanOneDiscrValue()) {            
                // check, if discrvalue assigned is ok
                Label endMethodLabel = gen.DefineLabel();
                GenerateDiscrValueOkTest(gen, switchCase, endMethodLabel, s_BadParamConstr, 34);            
                gen.MarkLabel(endMethodLabel);            
            }
            gen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// generates the serialisation helper methods
        /// </summary>
        private void GenerateSerialisationHelpers() {
            GenerateGetFieldInfoForDiscriminator();
            AddGetSpecifiedDefaultCaseFieldGetter();
            GenerateGetObjectDataMethod();
            GenerateISerializableDeserializationConstructor();
        }
        
        private void GenerateSwitchForDiscriminator(ILGenerator gen, GenerateUnionCaseAction action,
                                                    Label afterSwitchBlock) {
            
            // process all switch cases stored ...

            // generate the following structure:
            //        ldarg.0 // start case-1 test
            //        push first discr constant for switch case considered
            //        beq case1
            //        ldarg.0
            //        push next (if present) discr constant for switch case considered
            //        beq case1
            //                    
            //        ldarg.0 // start case-2 test
            //        push first discr constant for switch case considered
            //        beq case2
            //          ...
            //        br default
            // case1 :
            //          ...
            //          br end
            // case2 :
            //          ...
            //          br end
            // default:
            //          ...
            Label defaultCaseLabel = gen.DefineLabel(); // make sure to have default case, even if no default is specified in IDL
            // part1: compare and jump            
            foreach (SwitchCase switchCase in m_switchCases) {
                if (switchCase.IsDefaultCase()) {
                    // generate the default case at the end, no comparison needed for default case
                    continue;
                }
                // generate and assign label to the current case
                switchCase.AssignIlLabel(gen);
                
                for (int j = 0; j < switchCase.DiscriminatorValues.Length; j++) {
                    // generate tests
                    action.GenerateLoadDiscValue(gen);
                    PushDiscriminatorValueToStack(gen, switchCase.DiscriminatorValues[j]);
                    gen.Emit(OpCodes.Beq, switchCase.IlLabelAssigned);
                }
            }
            // if nothing found, jump to default case
            gen.Emit(OpCodes.Br, defaultCaseLabel);
            
            // part2: code for cases (jump-targets)
            SwitchCase defaultCaseFound = null;
            foreach (SwitchCase switchCase in m_switchCases) {
                if (switchCase.IsDefaultCase()) {
                    // make sure to generate the default case at the end, not in between
                    defaultCaseFound = switchCase;
                    continue;
                }
                // set position for the case label
                gen.MarkLabel(switchCase.IlLabelAssigned);
                // the field to return
                action.GenerateCaseAction(gen, switchCase, false);
                gen.Emit(OpCodes.Br, afterSwitchBlock); // jump to exit point
            }

            // the default case
            gen.MarkLabel(defaultCaseLabel);
            if (defaultCaseFound != null) {
                // a default case present
                action.GenerateCaseAction(gen, defaultCaseFound, true);
            } else {
                action.GenerateNoDefaultCasePresent(gen);
            }
            gen.Emit(OpCodes.Br_S, afterSwitchBlock); // jump to exit point            
        }        
        
        private void GenerateGetFieldInfoForDiscriminator() {
            MethodBuilder getFieldForDiscrMethod = m_ilEmitHelper.AddMethod(m_builder, GET_FIELD_FOR_DISCR_METHOD,
                                                                   new ParameterSpec[] { 
                                                                        new ParameterSpec("discrVal", 
                                                                                          m_discrType, 
                                                                                          ParameterSpec.ParameterDirection.s_in) 
                                                                   },
                                                                   new TypeContainer(typeof(FieldInfo)), 
                                                                   MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static);
            ILGenerator gen = getFieldForDiscrMethod.GetILGenerator();            
            GenerateGetFieldInfoForDiscAction action = new GenerateGetFieldInfoForDiscAction(this);
            Label endOfMethod = gen.DefineLabel();            
            GenerateSwitchForDiscriminator(gen, action, endOfMethod);
            gen.MarkLabel(endOfMethod);
            gen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// generates a call to get a field of the union-type via reflection
        /// </summary>
        private void GenerateGetFieldOfUnionType(ILGenerator gen, FieldInfo elemField) {
            gen.Emit(OpCodes.Ldsfld, m_unionTypeCache); // load the typecachefield
            gen.Emit(OpCodes.Ldstr, elemField.Name);
            gen.Emit(OpCodes.Ldc_I4, Convert.ToInt32(BindingFlags.NonPublic | BindingFlags.Instance));
            MethodInfo getField = ReflectionHelper.TypeType.GetMethod("GetField", 
                                                         BindingFlags.Public | BindingFlags.Instance, 
                                                         null, new Type[] { ReflectionHelper.StringType, typeof(BindingFlags) }, null);
            gen.Emit(OpCodes.Callvirt, getField); // call getField to retrieve the fieldInfo: pushed result on stack
        }
        
        /// <summary>
        /// checks, if a default case is present
        /// </summary>
        /// <returns></returns>
        private bool IsDefaultCasePresent() {
            foreach (SwitchCase switchCase in m_switchCases) {                
                if (switchCase.IsDefaultCase()) {
                    return true;
                }
            }
            return false;
        }
        
        private void AddOwnDefaultCaseDiscriminatorSetter() {
            
            // discr val paramter
            ParameterSpec discrArg = new ParameterSpec("discrVal", DiscriminatorType, 
                                                       ParameterSpec.ParameterDirection.s_in);
            ParameterSpec[] parameters = new ParameterSpec[] { discrArg };
            
            MethodBuilder modifier = m_ilEmitHelper.AddMethod(m_builder, "SetDefault", 
                                                              parameters, new TypeContainer(ReflectionHelper.VoidType), 
                                                              MethodAttributes.Public | MethodAttributes.HideBySig);

            ILGenerator gen = modifier.GetILGenerator();
            
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Stfld, m_initalizedField); // store initalizedfield
            
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, m_discrField); // store discriminator field
            
            // check, if discrvalue assigned is ok
            Label endMethodLabel = gen.DefineLabel();
            SwitchCase ownDefault = new SwitchCase(null, null, new object[] { s_defaultCaseDiscriminator }, 
                                                   null);
            GenerateDiscrValueOkTest(gen, ownDefault, endMethodLabel, s_BadParamConstr, 34);            
            gen.MarkLabel(endMethodLabel);            
            gen.Emit(OpCodes.Ret);            
        }

        /// <summary>
        /// checks if a default case is present and adds an own special default case, if not.        
        /// </summary>
        private void AddOwnDefaultCaseIfNeeded() {
            if (IsDefaultCasePresent()) {
                // not needed
                return;
            }
            AddOwnDefaultCaseDiscriminatorSetter();
        }

        /// <summary>
        /// adds a method, which returns the field for the default case, or null if no default case 
        /// was specified in IDL.
        /// This method is used for TypeCodeCreation from the Type.
        /// </summary>
        private void AddGetSpecifiedDefaultCaseFieldGetter() {
            MethodBuilder getDefaultCase = m_ilEmitHelper.AddMethod(m_builder, GET_DEFAULT_FIELD,
                                                                    new ParameterSpec[0],
                                                                    new TypeContainer(typeof(FieldInfo)), 
                                                                    MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static);
            ILGenerator gen = getDefaultCase.GetILGenerator();
            foreach (SwitchCase switchCase in m_switchCases) {
                if (switchCase.IsDefaultCase()) {
                    GenerateGetFieldOfUnionType(gen, switchCase.ElemField);
                    gen.Emit(OpCodes.Ret);
                    return;
                }
            }
            // none found
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// adds a static method, which returns all used discriminator values.
        /// </summary>
        private void AddCoveredDiscrsGetter() {
            Type discrTypeCls = m_discrType.GetCompactClsType();
            MethodBuilder methodToBuild = m_ilEmitHelper.AddMethod(m_builder, GET_COVERED_DISCR_VALUES,
                                                                   new ParameterSpec[0],
                                                                   new TypeContainer(ReflectionHelper.ObjectArrayType),
                                                                   MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static);
            ILGenerator gen = methodToBuild.GetILGenerator();
            LocalBuilder resultRef = gen.DeclareLocal(ReflectionHelper.ObjectArrayType);

            gen.Emit(OpCodes.Ldc_I4, m_coveredDiscrs.Count);
            gen.Emit(OpCodes.Newarr, ReflectionHelper.ObjectType);
            gen.Emit(OpCodes.Stloc_0);
            
            for (int i = 0; i < m_coveredDiscrs.Count; i++) {                
                gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Ldc_I4, i); // element nr
                PushDiscriminatorValueToStack(gen, m_coveredDiscrs[i]);
                gen.Emit(OpCodes.Box, discrTypeCls);
                gen.Emit(OpCodes.Stelem_Ref);
            }            
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
        }
        
        /// <summary>
        /// helper method for implementing ISerializable
        /// </summary>
        private void GenerateAddFieldToObjectData(FieldInfo field, ILGenerator body,
                                                  MethodInfo addValueMethod, MethodInfo getTypeFromH) {
            body.Emit(OpCodes.Ldarg_1); // info
            body.Emit(OpCodes.Ldstr, field.Name); // arg1
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, field);
            if (field.FieldType.IsValueType) {
                body.Emit(OpCodes.Box, field.FieldType); // arg2: field value; need to box because value type
            }
            body.Emit(OpCodes.Ldtoken, field.FieldType);
            body.Emit(OpCodes.Call, getTypeFromH); // load the type, the third argument of AddValue
            body.Emit(OpCodes.Callvirt, addValueMethod); // finally add the value to the info            
        }
        
        private void GenerateGetObjectDataMethod() {
            ParameterSpec[] getObjDataParams = new ParameterSpec[] { 
                new ParameterSpec("info", typeof(System.Runtime.Serialization.SerializationInfo)), 
                new ParameterSpec("context", typeof(System.Runtime.Serialization.StreamingContext)) };
            MethodBuilder getObjectDataMethod =
                m_ilEmitHelper.AddMethod(m_builder, "GetObjectData", getObjDataParams,
                                         new TypeContainer(typeof(void)),
                                         MethodAttributes.Virtual | MethodAttributes.Public |
                                         MethodAttributes.HideBySig);
            ILGenerator body = 
                getObjectDataMethod.GetILGenerator();

            MethodInfo addValueMethod =
                typeof(System.Runtime.Serialization.SerializationInfo).GetMethod("AddValue",  BindingFlags.Public | BindingFlags.Instance,
                                                                                 null,
                                                                                 new Type[] { ReflectionHelper.StringType,
                                                                                              ReflectionHelper.ObjectType,
                                                                                              ReflectionHelper.TypeType },
                                                                                 new ParameterModifier[0]);
            MethodInfo getTypeFromH = 
                ReflectionHelper.TypeType.GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);            
            Label beforeReturn = body.DefineLabel();
            // serialize initalized field           
            GenerateAddFieldToObjectData(m_initalizedField, body, addValueMethod, getTypeFromH);
            // nothing more to do, if not initalized, therefore check here
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, m_initalizedField); // fieldvalue
            body.Emit(OpCodes.Brfalse, beforeReturn);
            // initalized -> serialise discriminator and corresponding value
            GenerateAddFieldToObjectData(m_discrField, body, addValueMethod, getTypeFromH);
                        
            // now serialize field corresponding to discriminator value
            // do this by using a switch structure
            GenerateInsertIntoInfoForDiscAction action = new GenerateInsertIntoInfoForDiscAction(this, addValueMethod, getTypeFromH);
            GenerateSwitchForDiscriminator(body, action, beforeReturn);
                                                
            body.MarkLabel(beforeReturn);
            body.Emit(OpCodes.Ret);
        }
                
        private void GenerateGetFieldFromObjectData(FieldInfo field, ILGenerator body,
                                                    MethodInfo getValueMethod, MethodInfo getTypeFromH) {
            body.Emit(OpCodes.Ldarg_0); // this for calling store field after GetValue
            body.Emit(OpCodes.Ldarg_1); // info
            body.Emit(OpCodes.Ldstr, field.Name); // ld the first arg of GetValue
            body.Emit(OpCodes.Ldtoken, field.FieldType);
            body.Emit(OpCodes.Call, getTypeFromH); // ld the 2nd arg of GetValue
            body.Emit(OpCodes.Callvirt, getValueMethod); // call info.GetValue
            // now store result in the corresponding field
            m_ilEmitHelper.GenerateCastObjectToType(body, field.FieldType);
            body.Emit(OpCodes.Stfld, field);
        }
        
        private void GenerateISerializableDeserializationConstructor() {
            // deserialisation constructor
            ParameterSpec[] constrParams = new ParameterSpec[] {
                new ParameterSpec("info", typeof(System.Runtime.Serialization.SerializationInfo)), 
                new ParameterSpec("context", typeof(System.Runtime.Serialization.StreamingContext)) };
            ConstructorBuilder constrBuilder =
                m_ilEmitHelper.AddConstructor(m_builder, constrParams,
                                              MethodAttributes.Family | MethodAttributes.HideBySig);
            ILGenerator body = constrBuilder.GetILGenerator();
            // value type constructors don't call base class constructors                        
            // directly start with deserialisation                     
            MethodInfo getValueMethod = 
                typeof(System.Runtime.Serialization.SerializationInfo).GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public,
                                                                                 null,
                                                                                 new Type[] { typeof(string), typeof(Type) },
                                                                                 new ParameterModifier[0]);
            MethodInfo getTypeFromH = 
                ReflectionHelper.TypeType.GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);            
            
            Label beforeReturn = body.DefineLabel();
            // get the value of the init field
            GenerateGetFieldFromObjectData(m_initalizedField, body, getValueMethod, getTypeFromH);
            // nothing more to do, if not initalized, therefore check here
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, m_initalizedField); // fieldvalue
            body.Emit(OpCodes.Brfalse, beforeReturn);
            // initalized -> deserialise discriminator and corresponding value
            GenerateGetFieldFromObjectData(m_discrField, body, getValueMethod, getTypeFromH);
                        
            // now deserialize field corresponding to discriminator value
            GenerateAssignFromInfoForDiscAction action = 
                new GenerateAssignFromInfoForDiscAction(this, getValueMethod, getTypeFromH);
            GenerateSwitchForDiscriminator(body, action, beforeReturn);
            
            body.MarkLabel(beforeReturn);
            body.Emit(OpCodes.Ret);
        }        

        private void PushBooleanDiscriminatorValueToStack(ILGenerator gen, object discVal) {
            if (!(discVal is System.Boolean)) {
                throw new InvalidUnionDiscriminatorValue(discVal, m_discrType.GetCompactClsType());
            }
            gen.Emit(OpCodes.Ldc_I4, Convert.ToInt32(discVal));            
        }
        
        private void PushInt16DiscriminatorValueToStack(ILGenerator gen, object discVal) {
            Int32 val;
            decimal discValAsDecimal = Convert.ToDecimal(discVal);
            if (m_discrType.GetAssignableFromType().Equals(typeof(System.UInt16))) {
                // handling to uint -> int conversion of the mapping
                if ((discValAsDecimal < UInt16.MinValue) || (discValAsDecimal > UInt16.MaxValue)) {
                    throw new InvalidUnionDiscriminatorValue(discVal, m_discrType.GetCompactClsType());
                }
                System.UInt16 asUint16 = Convert.ToUInt16(discVal);
                val = (System.Int16)asUint16; // cast to int16
            } else {
                if ((discValAsDecimal < Int16.MinValue) || (discValAsDecimal > Int16.MaxValue)) {
                    throw new InvalidUnionDiscriminatorValue(discVal, m_discrType.GetCompactClsType());
                }    
                val = Convert.ToInt32(discVal);
            }                        
            gen.Emit(OpCodes.Ldc_I4, val);            
        }
        
        private void PushInt32DiscriminatorValueToStack(ILGenerator gen, object discVal) {
            System.Int32 val;
            decimal discValAsDecimal = Convert.ToDecimal(discVal);
            if (m_discrType.GetAssignableFromType().Equals(typeof(System.UInt32))) {
                // handling to uint -> int conversion of the mapping
                if ((discValAsDecimal < UInt32.MinValue) || (discValAsDecimal > UInt32.MaxValue)) {
                    throw new InvalidUnionDiscriminatorValue(discVal, m_discrType.GetCompactClsType());
                }    
                UInt32 asUint32 = Convert.ToUInt32(discVal);
                val = (System.Int32)asUint32; // cast to int32
            } else {
                if ((discValAsDecimal < Int32.MinValue) || (discValAsDecimal > Int32.MaxValue)) {
                    throw new InvalidUnionDiscriminatorValue(discVal, m_discrType.GetCompactClsType());
                }
                val = Convert.ToInt32(discVal);
            }
            gen.Emit(OpCodes.Ldc_I4, val);
        }
        
        private void PushInt64DiscriminatorValueToStack(ILGenerator gen, object discVal) {
            decimal discValAsDecimal = Convert.ToDecimal(discVal);
            if (m_discrType.GetAssignableFromType().Equals(typeof(System.UInt64))) {
                if ((discValAsDecimal < UInt64.MinValue) || (discValAsDecimal > UInt64.MaxValue)) {
                    throw new InvalidUnionDiscriminatorValue(discVal, m_discrType.GetCompactClsType());
                }                
                UInt64 asUint64 = Convert.ToUInt64(discVal);
                gen.Emit(OpCodes.Ldc_I8, (Int64)asUint64);
            } else {
                if ((discValAsDecimal < Int64.MinValue) || (discValAsDecimal > Int64.MaxValue)) {
                    throw new InvalidUnionDiscriminatorValue(discVal, m_discrType.GetCompactClsType());
                }                
                gen.Emit(OpCodes.Ldc_I8, Convert.ToInt64(discVal));
            }
        }

        /// <summary>pushes the constant value for the discriminator to the stack</summary>
        /// <remarks>discriminator must be already generated</remarks>
        private void PushDiscriminatorValueToStack(ILGenerator gen, object discVal) {
            if (m_discrType == null) {
                throw new INTERNAL(899, CompletionStatus.Completed_MayBe);
            }
            
            Type discrTypeCls = m_discrType.GetCompactClsType(); // for discriminator do not split boxed value types, because only the listed unseparable types usable
            if (discrTypeCls.Equals(ReflectionHelper.BooleanType)) {
                PushBooleanDiscriminatorValueToStack(gen, discVal);
            } else if (discrTypeCls.Equals(ReflectionHelper.Int16Type)) {
                PushInt16DiscriminatorValueToStack(gen, discVal);
            } else if (discrTypeCls.Equals(ReflectionHelper.Int32Type)) {
                PushInt32DiscriminatorValueToStack(gen, discVal);
            } else if (discrTypeCls.Equals(ReflectionHelper.Int64Type)) {
                PushInt64DiscriminatorValueToStack(gen, discVal);
            } else if (discrTypeCls.Equals(ReflectionHelper.CharType)) {
                if (!(discVal is System.Char)) {
                    throw new InvalidUnionDiscriminatorValue(discVal, discrTypeCls);
                }
                gen.Emit(OpCodes.Ldc_I4, Convert.ToInt32(discVal));
            } else if (discrTypeCls.IsEnum) {
                if (!discVal.GetType().IsEnum) {
                    throw new InvalidUnionDiscriminatorValue(discVal, discrTypeCls);
                }
                // get the int value for the idl enum
                gen.Emit(OpCodes.Ldc_I4, (System.Int32)discVal);
            } else {
                throw new INTERNAL(899, CompletionStatus.Completed_MayBe);
            }
        }

        #endregion IMethods

    }

}
