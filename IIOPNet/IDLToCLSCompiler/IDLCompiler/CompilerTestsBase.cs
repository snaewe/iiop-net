/* CompilerTestsBase.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 01.08.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

#if UnitTest

namespace Ch.Elca.Iiop.IdlCompiler.Tests {
	
    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.Text;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Idl;
    using parser;
    using Ch.Elca.Iiop.IdlCompiler.Action;
    using Ch.Elca.Iiop.IdlCompiler.Exceptions;
    using Ch.Elca.Iiop.Util;

    /// <summary>
    /// Base class for unit tests
    /// </summary>    
    public abstract class CompilerTestsBase {
        
        #region SFields
        
        private static Encoding s_latin1 = Encoding.GetEncoding("ISO-8859-1");
        
        #endregion
        #region IMethods
        
        
        /// <summary>
        /// Get an assembly name for a string name.
        /// </summary>
        protected AssemblyName GetAssemblyName(string name) {
            AssemblyName result = new AssemblyName();
            result.Name = name;
            return result;
        }
        
        /// <summary>
        /// Parses the idl and generates an assembly.
        /// </summary>
        protected Assembly CreateIdl(Stream source, AssemblyName targetName) {
            return CreateIdl(source, targetName, false, false);
        }                
        
        /// <summary>
        /// Parses the idl and generates an assembly.
        /// </summary>
        protected Assembly CreateIdl(Stream source, AssemblyName targetName,
                                  bool anyToAnyContainerMapping,
                                  bool makeInterfaceDisposable) {
            return CreateIdl(source, targetName, anyToAnyContainerMapping,
        	                 makeInterfaceDisposable, new ArrayList());
        }        
        
        protected Assembly CreateIdl(Stream source, AssemblyName targetName,
                                     bool anyToAnyContainerMapping, bool makeInterfaceDisposable,
                                     ArrayList refAssemblies) {
        	IDLParser parser = new IDLParser(source);
            ASTspecification spec = parser.specification();
            // now parsed representation can be visited with the visitors
            MetaDataGenerator generator = new MetaDataGenerator(targetName, ".", 
                                                                refAssemblies);
            generator.MapAnyToAnyContainer = anyToAnyContainerMapping;
            if(makeInterfaceDisposable) {
                generator.InheritedInterface = typeof(System.IDisposable);
            }
            generator.InitalizeForSource(parser.getSymbolTable());
            spec.jjtAccept(generator, null);
            Assembly result = generator.ResultAssembly;            
            return result;
        }
        
        
        /// <summary>
        /// Creates a stream writer for a stream
        /// to place the source into.
        /// </summary>
        protected StreamWriter CreateSourceWriter(Stream stream) {            
            StreamWriter writer = new StreamWriter(stream, s_latin1);            
            return writer;
        }
        
        /// <summary>
        /// Check, that the repository id attribute is present with
        /// the given id.
        /// </summary>
        protected void CheckRepId(Type testType, string expected) {
            object[] repAttrs = 
                testType.GetCustomAttributes(ReflectionHelper.RepositoryIDAttributeType,
                                             false);
            Assertion.AssertEquals("wrong number of RepIDAttrs", 1, repAttrs.Length);
            RepositoryIDAttribute repId = (RepositoryIDAttribute) repAttrs[0];
            Assertion.AssertEquals("wrong repId", expected,
                                   repId.Id);            
        }                
        
        /// <summary>
        /// Check, that the correct interface type attribute has been placed.
        /// </summary>
        protected void CheckInterfaceAttr(Type testType, IdlTypeInterface expected) {
            object[] ifAttrs = testType.GetCustomAttributes(typeof(InterfaceTypeAttribute), 
                                                            false);
            Assertion.AssertEquals("wrong number of InterfaceTypeAttribute", 1, ifAttrs.Length);
            InterfaceTypeAttribute ifAttr = (InterfaceTypeAttribute) ifAttrs[0];
            Assertion.AssertEquals("wrong ifattr", expected,
                                   ifAttr.IdlType);            
        }
        
        /// <summary>
        /// Check, that the impl class attribute has been placed specifying the correct class.
        /// </summary>
        protected void CheckImplClassAttr(Type toCheck, string implClassName) {
            object[] attrs = toCheck.GetCustomAttributes(ReflectionHelper.ImplClassAttributeType, 
                                                         false);
            Assertion.AssertEquals("wrong number of ImplClassAttribute", 1, attrs.Length);
            ImplClassAttribute attr = (ImplClassAttribute) attrs[0];
            Assertion.AssertEquals("wrong implclass attr", implClassName,
                                   attr.ImplClass);            
        }                
        
        /// <summary>
        /// Check, that an idl enum attribute has been placed.
        /// </summary>        
        protected void CheckIdlEnumAttributePresent(Type enumType) {
            object[] attrs = enumType.GetCustomAttributes(ReflectionHelper.IdlEnumAttributeType, 
                                                          false);
            Assertion.AssertEquals("wrong number of IdlEnumAttribute", 1, attrs.Length);
        }
        
        /// <summary>
        /// Check, that the idl struct attribute has been placed.
        /// </summary>
        protected void CheckIdlStructAttributePresent(Type structType) {
            object[] attrs = structType.GetCustomAttributes(ReflectionHelper.IdlStructAttributeType, 
                                                            false);
            Assertion.AssertEquals("wrong number of IdlStructAttribute", 1, attrs.Length);
        }
        
        /// <summary>
        /// Check, that the idl union attribute has been placed.
        /// </summary>
        protected void CheckIdlUnionAttributePresent(Type unionType) {
            object[] attrs = unionType.GetCustomAttributes(ReflectionHelper.IdlUnionAttributeType, 
                                                           false);
            Assertion.AssertEquals("wrong number of IdlUnionAttribute", 1, attrs.Length);
        }
        
        /// <summary>
        /// Check, that the class is serializable.
        /// </summary>
        protected void CheckSerializableAttributePresent(Type toCheck) {
            Assertion.AssertEquals("not serializable", true, toCheck.IsSerializable);
        }
        
        /// <summary>
        /// Check, that the ExplicitSerializationOrdered attribute has been placed.
        /// </summary>
        protected void CheckExplicitSerializationOrderedAttributePresent(Type type) {
            object[] attrs = type.GetCustomAttributes(ReflectionHelper.ExplicitSerializationOrderedType, 
                                                      false);
            Assertion.AssertNotNull("attrs null?", attrs);
            Assertion.AssertEquals("wrong number of ExplicitSerializationOrderedAttribute", 1, attrs.Length);
        }
        
        /// <summary>
        /// Check, that a given public method is present.
        /// </summary>
        protected void CheckPublicInstanceMethodPresent(Type testType, string methodName,
                                                      Type returnType, Type[] paramTypes) {
            CheckMethodPresent(testType, methodName, returnType, paramTypes,
                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        }
        
        /// <summary>
        /// Check, that a given method is present.
        /// </summary>
        protected void CheckMethodPresent(Type testType, string methodName,
                                        Type returnType, Type[] paramTypes, BindingFlags attrs) {
            MethodInfo testMethod = testType.GetMethod(methodName, 
                                                       attrs,
                                                       null, paramTypes, null);
            Assertion.AssertNotNull(String.Format("method {0} not found", methodName),
                                    testMethod);
            
            Assertion.AssertEquals(String.Format("wrong return type {0} in method {1}", testMethod.ReturnType, methodName),
                                   returnType, testMethod.ReturnType);                                            
        }
        
        /// <summary>
        /// Check, that a given property is present.
        /// </summary>
        protected void CheckPropertyPresent(Type testType, string propName, 
                                        Type propType, BindingFlags attrs) {            
            PropertyInfo testProp = testType.GetProperty(propName, attrs,
                                                       null, propType, Type.EmptyTypes,
                                                       null);
            Assertion.AssertNotNull(String.Format("property {0} not found", propName),
                                    testProp);
            
            Assertion.AssertEquals(String.Format("wrong type {0} in property {1}", testProp.PropertyType, propName),
                                   propType, testProp.PropertyType);                                            
        }
        
        /// <summary>
        /// Check, that a given field is present.
        /// </summary>
        protected void CheckFieldPresent(Type testType, string fieldName,
                                       Type fieldType, BindingFlags flags) {
            FieldInfo testField = testType.GetField(fieldName, flags);                                           
            Assertion.AssertNotNull(String.Format("field {0} not found in type {1}", fieldName, testType.FullName),
                                    testField);
            Assertion.AssertEquals(String.Format("wrong field type {0} in field {1}", 
                                                 testField.FieldType, testField.Name),
                                   fieldType, testField.FieldType);        
        }
        
        /// <summary>
        /// Check, that the given number of fields are present.
        /// </summary>
        protected void CheckNumberOfFields(Type testType, BindingFlags flags,
                                         System.Int32 expected) {
            FieldInfo[] fields = testType.GetFields(flags);
            Assertion.AssertEquals("wrong number of fields found in type: " + testType.FullName,
                                   expected, fields.Length);        
        }
        
        /// <summary>
        /// Check, that exactly one custom attribute is present in the given array.
        /// </summary>
        protected void CheckOnlySpecificCustomAttrInCollection(object[] testAttrs,
                                                             Type attrType) {
            Assertion.AssertEquals("wrong nr of custom attrs found",
                                   1, testAttrs.Length);
            Assertion.AssertEquals("wrong custom attr found",
                                   attrType,
                                   testAttrs[0].GetType());                                                             
        }
        
        /// <summary>
        /// Check, that the type implements IIdlEntity.
        /// </summary>
        protected void CheckIIdlEntityInheritance(Type testType) {
            Type idlEntityIf = testType.GetInterface("IIdlEntity");
            Assertion.AssertNotNull(String.Format("type {0} doesn't inherit from IIdlEntity", testType.FullName),
                                    idlEntityIf);
        }
        
        /// <summary>
        /// Check an enum field.
        /// </summary>
        protected void CheckEnumField(FieldInfo field, string idlEnumValName) {
            Type enumType = field.DeclaringType;
            Assertion.AssertEquals("wrong enum val field type", 
                                   enumType, field.FieldType);
            Assertion.AssertEquals("wrong enum val field name",
                                   idlEnumValName, field.Name);
        }
        

        
        #endregion

        
        
        
    
    }
    
    
}

#endif
