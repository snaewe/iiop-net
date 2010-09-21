/* TypeCodeCreator.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 21.08.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2005 Dominic Ullmann
 *
 * Copyright 2005 ELCA Informatique SA
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
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;
 
namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// create a type-code for the cls-Type mapped to the specified IDL-type
    /// </summary>
    internal class TypeCodeCreater : MappingAction {
        
        #region Types
        
        internal class TypecodeForTypeKey {
        
            #region IFields
            
            private Type m_type;
            private AttributeExtCollection m_attributes;
            
            #endregion IFields
            #region IConstructors
        
            public TypecodeForTypeKey(Type forType, AttributeExtCollection
                                      attrs) {
                if ((forType == null) || (attrs == null)) {
                    throw new INTERNAL(801, CompletionStatus.Completed_MayBe);
                }
                m_type = forType;
                m_attributes = attrs;
            }
        
            #endregion IConstructors
            #region IProperties
            
            public Type ForType {
                get {
                    return m_type;                    
                }
            }
            
            public AttributeExtCollection Attributes {
                get {
                    return m_attributes;
                }
            }
            
            #endregion IProperties
            #region IMethods
        
            public override bool Equals(object other) {
                if (!(other is TypecodeForTypeKey)) {
                    return false;
                }
                return (m_type.Equals(((TypecodeForTypeKey)other).m_type) &&
                        m_attributes.Equals(((TypecodeForTypeKey)other).m_attributes));
            }
            
            public override int GetHashCode() {
                return m_type.GetHashCode() ^ 
                       m_attributes.GetHashCode();
            }
            
            #endregion IMethods
        
        }
        
        #endregion Types

        #region Constants

        private const short CONCRETE_VALUE_MOD = 0;
        private const short ABSTRACT_VALUE_MOD = 2;

        private const short VISIBILITY_PRIVATE = 0;
        private const short VISIBILITY_PUBLIC = 1;

        #endregion Constants
        #region IFields
        
        private IDictionary m_alreadyCreatedTypeCodes = new Hashtable();
        
        #endregion IFields
        #region IMethods
        
        private TypeCodeImpl CreateOrGetTypeCodeForType(Type forType,
                                                        AttributeExtCollection attributes) {
            TypecodeForTypeKey key = new TypecodeForTypeKey(forType, attributes);
            TypeCodeImpl result;

            lock(m_alreadyCreatedTypeCodes)
                result = m_alreadyCreatedTypeCodes[key] as TypeCodeImpl;

            if (result == null)
                result = Repository.CreateTypeCodeForTypeInternal(forType, attributes, this);

            return result;
        }
        
        /// <summary>
        /// used for recursive type code creation
        /// </summary>
        private void RegisterCreatedTypeCodeForType(Type forType,
                                                    AttributeExtCollection attributes,
                                                    TypeCodeImpl typeCode) {
            TypecodeForTypeKey key = new TypecodeForTypeKey(forType, attributes);
            lock(m_alreadyCreatedTypeCodes)
                m_alreadyCreatedTypeCodes[key] = typeCode;
        }
        
        private static IDictionary structTCs = new Hashtable();
        
        #region Implementation of MappingAction
        public object MapToIdlStruct(Type clsType) {
            lock(structTCs)
            {
                StructTC result = structTCs[clsType] as StructTC;
                if(result != null)
                    return result;
            
                result = new StructTC();
                RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                               result);
                
                FieldInfo[] members = ReflectionHelper.GetAllDeclaredInstanceFieldsOrdered(clsType);
                StructMember[] structMembers = new StructMember[members.Length];
                for (int i = 0; i < members.Length; i++) {                
                    omg.org.CORBA.TypeCode memberType = 
                        CreateOrGetTypeCodeForType(members[i].FieldType,
                                                   ReflectionHelper.GetCustomAttriutesForField(members[i], 
                                                                                               true));
                    structMembers[i] = new StructMember(members[i].Name, memberType);
                }
                result.Initalize(Repository.GetRepositoryID(clsType), 
                                 IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                                 structMembers);
                
                structTCs[clsType] = result;
                
                return result;
            }
        }
        public object MapToIdlUnion(Type clsType) {
            UnionTC result = new UnionTC();            
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            
            // first get discriminator type
            FieldInfo discriminator = clsType.GetField(UnionGenerationHelper.DISCR_FIELD_NAME, 
                                                       BindingFlags.Instance | 
                                                       BindingFlags.DeclaredOnly |
                                                       BindingFlags.NonPublic);
            omg.org.CORBA.TypeCode discrTypeCode = 
                CreateOrGetTypeCodeForType(discriminator.FieldType, 
                                           ReflectionHelper.GetCustomAttriutesForField(discriminator,
                                                                                       true));            
            // get the methods used for typecode creation
            MethodInfo getCoveredDiscrMethod = clsType.GetMethod(UnionGenerationHelper.GET_COVERED_DISCR_VALUES,
                                                                 BindingFlags.Static | BindingFlags.NonPublic |
                                                                 BindingFlags.DeclaredOnly);
            MethodInfo getDefaultFieldMethod = clsType.GetMethod(UnionGenerationHelper.GET_DEFAULT_FIELD,
                                                                 BindingFlags.Static | BindingFlags.NonPublic |
                                                                 BindingFlags.DeclaredOnly);
            MethodInfo getFieldForDiscrVal = clsType.GetMethod(UnionGenerationHelper.GET_FIELD_FOR_DISCR_METHOD,
                                                               BindingFlags.Static | BindingFlags.NonPublic |
                                                               BindingFlags.DeclaredOnly);

            // get all discriminator values used in switch-cases
            object[] coveredDiscrs = (object[])getCoveredDiscrMethod.Invoke(null, new object[0]);
            if (coveredDiscrs == null) {
                throw new INTERNAL(898, CompletionStatus.Completed_MayBe);
            }
                        
            FieldInfo defaultField = (FieldInfo)getDefaultFieldMethod.Invoke(null, new object[0]);
            
            UnionSwitchCase[] cases = null;
            int defaultCaseNumber = -1; // no default case
            if (defaultField != null) {
                cases = new UnionSwitchCase[coveredDiscrs.Length + 1];
                omg.org.CORBA.TypeCode elemType = 
                    CreateOrGetTypeCodeForType(defaultField.FieldType, 
                                               ReflectionHelper.GetCustomAttriutesForField(defaultField, 
                                                                                           true));
                // create a default value of type discriminiator type, because of possible discriminator types, this 
                // is possible with Activator.CreateInstance ...
                object dummyValue = null;
                try {
                    dummyValue = Activator.CreateInstance(discriminator.FieldType);
                } catch (Exception) {
                    throw new MARSHAL(881, CompletionStatus.Completed_MayBe);
                }
                cases[coveredDiscrs.Length] = new UnionSwitchCase(dummyValue, defaultField.Name.Substring(2),
                                                                  elemType);
                defaultCaseNumber = coveredDiscrs.Length;
            } else {
                cases = new UnionSwitchCase[coveredDiscrs.Length];
            }
            
            // add a UnionSwitchCase to typecode for every discriminator value used
            for (int i = 0; i < coveredDiscrs.Length; i++) {
                FieldInfo caseField = (FieldInfo)getFieldForDiscrVal.Invoke(null, new object[] { coveredDiscrs[i] });
                if (caseField == null) {
                    throw new INTERNAL(898, CompletionStatus.Completed_MayBe);
                }
                omg.org.CORBA.TypeCode elemType = 
                    CreateOrGetTypeCodeForType(caseField.FieldType, 
                                               ReflectionHelper.GetCustomAttriutesForField(caseField, true));
                // extract name of element field: strip m_
                UnionSwitchCase switchCase = new UnionSwitchCase(coveredDiscrs[i], caseField.Name.Substring(2),
                                                                 elemType);
                cases[i] = switchCase;
            }                                                                                                
            result.Initalize(Repository.GetRepositoryID(clsType),            
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                             discrTypeCode, defaultCaseNumber, cases);
            return result;
        }
        
        public object MapToIdlAbstractInterface(Type clsType) {
            AbstractIfTC result =
                new AbstractIfTC(Repository.GetRepositoryID(clsType), 
                                 IdlNaming.ReverseIdlToClsNameMapping(clsType.Name));
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            return result;
            
        }
        public object MapToIdlLocalInterface(Type clsType) {
            LocalIfTC result = new LocalIfTC(Repository.GetRepositoryID(clsType), 
                                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name));
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            return result;            
        }
        
        public object MapToIdlConcreteInterface(Type clsType) {           
            ObjRefTC result;
            if (!clsType.Equals(ReflectionHelper.MarshalByRefObjectType)) {
                result = new ObjRefTC(Repository.GetRepositoryID(clsType),
                                      IdlNaming.ReverseIdlToClsNameMapping(clsType.Name));
            } else {
                result = new ObjRefTC(String.Empty, String.Empty);
            }
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            return result;            
        }
        public object MapToIdlConcreateValueType(Type clsType) {
            omg.org.CORBA.TypeCode baseTypeCode;
            if (clsType.BaseType.Equals(ReflectionHelper.ObjectType) || 
                clsType.BaseType.Equals(typeof(System.ComponentModel.MarshalByValueComponent))) {
                baseTypeCode = new NullTC();
            } else {
                baseTypeCode = CreateOrGetTypeCodeForType(clsType.BaseType, 
                                                          AttributeExtCollection.EmptyCollection);
            }
            ValueTypeTC result = new ValueTypeTC();
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);                        
            
            // create the TypeCodes for the members
            FieldInfo[] members = ReflectionHelper.GetAllDeclaredInstanceFieldsOrdered(clsType);
            ValueMember[] valueMembers = new ValueMember[members.Length];
            for (int i = 0; i < members.Length; i++) {
                omg.org.CORBA.TypeCode memberType = CreateOrGetTypeCodeForType(members[i].FieldType, 
                                                        ReflectionHelper.GetCustomAttriutesForField(members[i], 
                                                                                                    true));
                short visibility;
                if (members[i].IsPrivate) { 
                    visibility = VISIBILITY_PRIVATE; 
                } else { 
                    visibility = VISIBILITY_PUBLIC; 
                }
                valueMembers[i] = new ValueMember(members[i].Name, memberType, visibility);
            }
            result.Initalize(Repository.GetRepositoryID(clsType),
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                             valueMembers, baseTypeCode, CONCRETE_VALUE_MOD);
            return result;
        }
        public object MapToIdlAbstractValueType(Type clsType) {                       
            omg.org.CORBA.TypeCode baseTypeCode;
            if (clsType.BaseType.Equals(ReflectionHelper.ObjectType) || 
                clsType.BaseType.Equals(typeof(System.ComponentModel.MarshalByValueComponent))) {
                baseTypeCode = new NullTC();
            } else {
                baseTypeCode = CreateOrGetTypeCodeForType(clsType.BaseType, 
                                   AttributeExtCollection.EmptyCollection);
            }
            ValueTypeTC result = new ValueTypeTC();            
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            result.Initalize(Repository.GetRepositoryID(clsType),
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name), 
                             new ValueMember[0],
                             baseTypeCode, ABSTRACT_VALUE_MOD);
            return result;
        }
        private object MapToIdlBoxedValueType(Type clsType,
                                              bool boxInAny) {
            // dotNetType is subclass of BoxedValueBase
            if (!clsType.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) {
                // mapper error: MapToIdlBoxedValue found incorrect type
                throw new INTERNAL(1929, CompletionStatus.Completed_MayBe);
            }
            Type boxedType;
            object[] attributesOnBoxed = new object[0];
            try {
                boxedType = (Type)clsType.InvokeMember(BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME,
                                                       BindingFlags.InvokeMethod | BindingFlags.Public |
                                                       BindingFlags.NonPublic | BindingFlags.Static | 
                                                       BindingFlags.DeclaredOnly, 
                                                       null, null, new object[0]);

                attributesOnBoxed = (object[])clsType.InvokeMember(BoxedValueBase.GET_BOXED_TYPE_ATTRIBUTES_METHOD_NAME,
                                                            BindingFlags.InvokeMethod | BindingFlags.Public |
                                                            BindingFlags.NonPublic | BindingFlags.Static | 
                                                            BindingFlags.DeclaredOnly, 
                                                            null, null, new object[0]);
            } 
            catch (Exception) {
                // invalid type: clsType
                // static method missing or not callable:
                // BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME
                throw new INTERNAL(1930, CompletionStatus.Completed_MayBe);
            }
            if (boxInAny) {
                omg.org.CORBA.TypeCode boxed = CreateOrGetTypeCodeForType(boxedType, 
                                                                          AttributeExtCollection.ConvertToAttributeCollection(attributesOnBoxed));
            
                ValueBoxTC result = new ValueBoxTC(Repository.GetRepositoryID(clsType), 
                                                   IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                                                   boxed);
                RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection, result);
                return result;                                
            } else {
                // don't use boxed form
                // therefore create a typecode for the type boxed inside
                // e.g. in case of a boxed sequence of int, there will be a idlsequence typecode with int as element type created.
                // e.g. in case, where a sequence of boxed valuetype is boxed, an idl sequence will be created containing a typecode
                // for the boxed type.
                omg.org.CORBA.TypeCodeImpl forBoxed = CreateOrGetTypeCodeForType(boxedType, 
                                                                                 AttributeExtCollection.ConvertToAttributeCollection(attributesOnBoxed));
                return forBoxed;                
            }                                                              
        }
        public object MapToIdlBoxedValueType(Type clsType, Type needsBoxingFrom) {
            return MapToIdlBoxedValueType(clsType,
                                          MappingConfiguration.Instance.UseBoxedInAny);
        }
        public object MapToIdlSequence(Type clsType, int bound, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes) {
            // sequence should not contain itself! -> do not register typecode
            omg.org.CORBA.TypeCode elementTC = CreateOrGetTypeCodeForType(clsType.GetElementType(),
                                                   elemTypeAttributes);
            return new SequenceTC(elementTC, bound);
        }        
        public object MapToIdlArray(Type clsType, int[] dimensions, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes) {
            // array should not contain itself! -> do not register typecode            
            // get the typecode for the array element type
            omg.org.CORBA.TypeCode elementTC = CreateOrGetTypeCodeForType(clsType.GetElementType(),
                                                   elemTypeAttributes);
            // for multidim arrays, nest array tcs
            ArrayTC arrayTC = new ArrayTC(elementTC, dimensions[dimensions.Length - 1]); // the innermost array tc for the rightmost dimension
            for (int i = dimensions.Length - 2; i >= 0; i--) {
                arrayTC = new ArrayTC(arrayTC, dimensions[i]);    
            }
            return arrayTC;
        }
        public object MapToIdlAny(Type clsType) {
            return new AnyTC();
        }
        public object MapToAbstractBase(Type clsType) {
            // no CLS type mapped to CORBA::AbstractBase
            throw new INTERNAL(1940, CompletionStatus.Completed_MayBe);
        }
        public object MapToValueBase(Type clsType) {
            // no CLS type mapped to CORBA::ValueBase
            throw new INTERNAL(1940, CompletionStatus.Completed_MayBe);
        }
        public object MapToWStringValue(Type clsType) {
            if (MappingConfiguration.Instance.UseBoxedInAny) {
                return MapToIdlBoxedValueType(clsType, true);
            } else {
                // don't use boxed form
                return new WStringTC(0);
            }
        }
        public object MapToStringValue(Type clsType) {
            if (MappingConfiguration.Instance.UseBoxedInAny) {
                return MapToIdlBoxedValueType(clsType, true);
            } else {                
                // don't use boxed form
                return new StringTC(0);
            }
        }
        public object MapException(Type clsType) {
            // TODO: check this, generic user exception handling ...
            ExceptTC result = new ExceptTC();
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            
            FieldInfo[] members = ReflectionHelper.GetAllDeclaredInstanceFieldsOrdered(clsType);
            StructMember[] exMembers = new StructMember[members.Length];
            for (int i = 0; i < members.Length; i++) {                
                omg.org.CORBA.TypeCode memberType = CreateOrGetTypeCodeForType(members[i].FieldType,
                                                        ReflectionHelper.GetCustomAttriutesForField(members[i], 
                                                                                                    true));
                exMembers[i] = new StructMember(members[i].Name, memberType);
            }
            result.Initalize(Repository.GetRepositoryID(clsType), 
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                             exMembers);
            return result;
        }
        public object MapToIdlEnum(Type clsType) {                        
            string[] names = Enum.GetNames(clsType);
            EnumTC result = new EnumTC(Repository.GetRepositoryID(clsType), 
                                       IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                                       names);
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);                        
            return result;

        }
        public object MapToIdlFlagsEquivalent(Type clsType) {
            Type underlyingType = Enum.GetUnderlyingType(clsType);
            return CreateOrGetTypeCodeForType(underlyingType, AttributeExtCollection.EmptyCollection);
        }
        public object MapToIdlBoolean(Type clsType) {
            return new BooleanTC();
        }
        public object MapToIdlFloat(Type clsType) {
            return new FloatTC();
        }
        public object MapToIdlDouble(Type clsType) {
            return new DoubleTC();
        }
        public object MapToIdlShort(Type clsType) {
            return new ShortTC();
        }
        public object MapToIdlUShort(Type clsType) {
            return new UShortTC();
        }
        public object MapToIdlLong(Type clsType) {
            return new LongTC();
        }
        public object MapToIdlULong(Type clsType) {
            return new ULongTC();
        }
        public object MapToIdlLongLong(Type clsType) {
            return new LongLongTC();
        }
        public object MapToIdlULongLong(Type clsType) {
            return new ULongLongTC();
        }
        public object MapToIdlOctet(Type clsType) {
            return new OctetTC();
        }
        public object MapToIdlSByteEquivalent(Type clsType) {
            return new OctetTC();
        }
        public object MapToIdlVoid(Type clsType) {
            return new VoidTC();
        }
        public object MapToIdlWChar(Type clsType) {
            return new WCharTC();
        }
        public object MapToIdlWString(Type clsType) {
            return new WStringTC(0); // no bound specified
        }
        public object MapToIdlChar(Type clsType) {
            return new CharTC();
        }
        /// <returns>an optional result of the mapping, null may be possible</returns>
        public object MapToIdlString(Type clsType) {
            return new StringTC(0); // no bound specified
        }
        
        public object MapToTypeDesc(Type clsType) {
            return new omg.org.CORBA.TypeCodeTC();
        }

        public object MapToTypeCode(Type clsType) {
            return new omg.org.CORBA.TypeCodeTC();
        }

        #endregion
        #endregion IMethods

    }
     
}
