/* Tests.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 28.09.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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


namespace Ch.Elca.Iiop.IDLCompiler.Tests {
	
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

    /// <summary>
    /// Unit-tests for testing assembly generation
    /// for IDL
    /// </summary>
    [TestFixture]
    public class CLSForIDLGenerationTest {
            
        #region SFields
        
        private static Encoding s_latin1 = Encoding.GetEncoding("ISO-8859-1");
        
        #endregion SFields
        #region IFields


        #endregion IFields
        #region IMethods

        [SetUp]
        public void SetupEnvironment() {
        
        }

        [TearDown]
        public void TearDownEnvironment() {
        
        }
        
        public Assembly CreateIdl(Stream source) {            
            IDLParser parser = new IDLParser(source);
            ASTspecification spec = parser.specification();
            // now parsed representation can be visited with the visitors
            MetaDataGenerator generator = new MetaDataGenerator("testAsm", ".", 
                                                                new ArrayList());
            generator.InitalizeForSource(parser.getSymbolTable());
            spec.jjtAccept(generator, null);
            return generator.GetResultAssembly();
        }                
        
        private void CheckRepId(Type testType, string expected) {
            object[] repAttrs = testType.GetCustomAttributes(typeof(RepositoryIDAttribute), 
                                                             false);
            Assertion.AssertEquals("wrong number of RepIDAttrs", 1, repAttrs.Length);
            RepositoryIDAttribute repId = (RepositoryIDAttribute) repAttrs[0];
            Assertion.AssertEquals("wrong repId", expected,
                                   repId.Id);            
        }                
        
        private void CheckInterfaceAttr(Type testType, IdlTypeInterface expected) {
            object[] ifAttrs = testType.GetCustomAttributes(typeof(InterfaceTypeAttribute), 
                                                            false);
            Assertion.AssertEquals("wrong number of InterfaceTypeAttribute", 1, ifAttrs.Length);
            InterfaceTypeAttribute ifAttr = (InterfaceTypeAttribute) ifAttrs[0];
            Assertion.AssertEquals("wrong ifattr", expected,
                                   ifAttr.IdlType);            
        }
        
        private void CheckImplClassAttr(Type toCheck, string implClassName) {
            object[] attrs = toCheck.GetCustomAttributes(typeof(ImplClassAttribute), 
                                                         false);
            Assertion.AssertEquals("wrong number of ImplClassAttribute", 1, attrs.Length);
            ImplClassAttribute attr = (ImplClassAttribute) attrs[0];
            Assertion.AssertEquals("wrong implclass attr", implClassName,
                                   attr.ImplClass);            
        }        
        
        private void CheckIdlEnumAttributePresent(Type enumType) {
            object[] attrs = enumType.GetCustomAttributes(typeof(IdlEnumAttribute), 
                                                          false);
            Assertion.AssertEquals("wrong number of IdlEnumAttribute", 1, attrs.Length);
        }
        
        private void CheckIdlStructAttributePresent(Type structType) {
            object[] attrs = structType.GetCustomAttributes(typeof(IdlStructAttribute), 
                                                            false);
            Assertion.AssertEquals("wrong number of IdlStructAttribute", 1, attrs.Length);
        }
        
        private void CheckIdlUnionAttributePresent(Type unionType) {
            object[] attrs = unionType.GetCustomAttributes(typeof(IdlUnionAttribute), 
                                                           false);
            Assertion.AssertEquals("wrong number of IdlUnionAttribute", 1, attrs.Length);
        }
        
        private void CheckSerializableAttributePresent(Type toCheck) {
            Assertion.AssertEquals("not serializable", true, toCheck.IsSerializable);
        }

        private void CheckPublicInstanceMethodPresent(Type testType, string methodName, 
                                                      Type returnType, Type[] paramTypes) {
            CheckMethodPresent(testType, methodName, returnType, paramTypes,
                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        }
        
        private void CheckMethodPresent(Type testType, string methodName, 
                                        Type returnType, Type[] paramTypes, BindingFlags attrs) {
            MethodInfo testMethod = testType.GetMethod(methodName, 
                                                       attrs,
                                                       null, paramTypes, null);
            Assertion.AssertNotNull(String.Format("method {0} not found", methodName),
                                    testMethod);
            
            Assertion.AssertEquals(String.Format("wrong return type {0} in method {1}", testMethod.ReturnType, methodName),
                                   returnType, testMethod.ReturnType);                                            
        }
        
        private void CheckFieldPresent(Type testType, string fieldName,
                                       Type fieldType, BindingFlags flags) {
            FieldInfo testField = testType.GetField(fieldName, flags);                                           
            Assertion.AssertNotNull(String.Format("field {0} not found in type {1}", fieldName, testType.FullName),
                                    testField);
            Assertion.AssertEquals(String.Format("wrong field type {0} in field {1}", 
                                                 testField.FieldType, testField.Name),
                                   fieldType, testField.FieldType);        
        }
        
        private void CheckNumberOfFields(Type testType, BindingFlags flags, 
                                         System.Int32 expected) {
            FieldInfo[] fields = testType.GetFields(flags);
            Assertion.AssertEquals("wrong number of fields found in type: " + testType.FullName,
                                   expected, fields.Length);        
        }
        
        private void CheckOnlySpecificCustomAttrInCollection(object[] testAttrs, 
                                                             Type attrType) {
            Assertion.AssertEquals("wrong nr of custom attrs found",
                                   1, testAttrs.Length);
            Assertion.AssertEquals("wrong custom attr found",
                                   attrType,
                                   testAttrs[0].GetType());                                                             
        }
        
        private void CheckIIdlEntityInheritance(Type testType) {
            Type idlEntityIf = testType.GetInterface("IIdlEntity");
            Assertion.AssertNotNull(String.Format("type {0} doesn't inherit from IIdlEntity", testType.FullName),
                                    idlEntityIf);
        }
        
        private void CheckEnumField(FieldInfo field, string idlEnumValName) {
            Type enumType = field.DeclaringType;
            Assertion.AssertEquals("wrong enum val field type", 
                                   enumType, field.FieldType);
            Assertion.AssertEquals("wrong enum val field name",
                                   idlEnumValName, field.Name);
        }
        
        private void CheckInterfaceDefinitions(string ifModifier, 
                                               IdlTypeInterface ifAttrVal) {
            MemoryStream testSource = new MemoryStream();
            StreamWriter writer = new StreamWriter(testSource, s_latin1);
            
            // idl:
            writer.WriteLine("module testmod {");
            writer.WriteLine("    " + ifModifier + " interface Test {");
            writer.WriteLine("        octet EchoOctet(in octet arg);");
            writer.WriteLine("    };");
            writer.WriteLine("    #pragma ID Test \"IDL:testmod/Test:1.0\"");
            writer.WriteLine("};");
            
            writer.Flush();
            testSource.Seek(0, SeekOrigin.Begin);
            Assembly result = CreateIdl(testSource);
                       
            // check if interface is correctly created
            Type ifType = result.GetType("testmod.Test", true);
            CheckPublicInstanceMethodPresent(ifType, "EchoOctet", 
                                             typeof(System.Byte), new Type[] { typeof(System.Byte) });
            CheckRepId(ifType, "IDL:testmod/Test:1.0");
            CheckInterfaceAttr(ifType, ifAttrVal);
            CheckIIdlEntityInheritance(ifType);            
            
            writer.Close();
        }
        
        
        [Test]
        public void TestConcreteInterfaces() {
            CheckInterfaceDefinitions("", IdlTypeInterface.ConcreteInterface);
        }
        
        [Test]
        public void TestAbstractInterfaces() {
            CheckInterfaceDefinitions("abstract", IdlTypeInterface.AbstractInterface);
        }
        
        [Test]
        public void TestLocalInterfaces() {
            CheckInterfaceDefinitions("local", IdlTypeInterface.LocalInterface);
        } 
        
        [Test]
        public void TestEnum() {
            MemoryStream testSource = new MemoryStream();
            StreamWriter writer = new StreamWriter(testSource, s_latin1);
            
            // idl:
            writer.WriteLine("module testmod {");
            writer.WriteLine("    enum Test {");
            writer.WriteLine("        A, B, C");
            writer.WriteLine("    };");
            writer.WriteLine("};");
            
            writer.Flush();
            testSource.Seek(0, SeekOrigin.Begin);
            Assembly result = CreateIdl(testSource);
                       
            // check if enum is correctly created
            Type enumType = result.GetType("testmod.Test", true);
            CheckIdlEnumAttributePresent(enumType);
            
            // check enum val
            FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | 
                                                    BindingFlags.Static | 
                                                    BindingFlags.DeclaredOnly);
            Assertion.AssertEquals("wrong number of fields in enum", 
                                   3, fields.Length);
            
            CheckEnumField(fields[0], "A");
            CheckEnumField(fields[1], "B");
            CheckEnumField(fields[2], "C");
            
            writer.Close();            
        }
        
        [Test]
        public void TestConcreteValueType() {
            MemoryStream testSource = new MemoryStream();
            StreamWriter writer = new StreamWriter(testSource, s_latin1);
            
            // idl:
            writer.WriteLine("module testmod {");
            writer.WriteLine("    valuetype Test {");
            writer.WriteLine("        private octet x;");
            writer.WriteLine("        octet EchoOctet(in octet arg);");
            writer.WriteLine("        attribute octet attr;");
            writer.WriteLine("    };");
            writer.WriteLine("    #pragma ID Test \"IDL:testmod/Test:1.0\"");
            writer.WriteLine("};");
            
            writer.Flush();
            testSource.Seek(0, SeekOrigin.Begin);
            Assembly result = CreateIdl(testSource);
                       
            // check if val-type is correctly created
            Type valType = result.GetType("testmod.Test", true);
                       
            CheckImplClassAttr(valType, "testmod.TestImpl");
            CheckSerializableAttributePresent(valType);
            CheckRepId(valType, "IDL:testmod/Test:1.0");
            
            CheckIIdlEntityInheritance(valType);
            
            CheckPublicInstanceMethodPresent(valType, "EchoOctet",
                                             typeof(System.Byte), new Type[] { typeof(System.Byte) });
            CheckFieldPresent(valType, "m_x", typeof(System.Byte), 
                              BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            CheckNumberOfFields(valType, BindingFlags.Public | BindingFlags.NonPublic |
                                         BindingFlags.Instance | BindingFlags.DeclaredOnly,
                                1);                                    
            writer.Close();
        }
        
        [Test]
        public void TestStruct() {
            MemoryStream testSource = new MemoryStream();
            StreamWriter writer = new StreamWriter(testSource, s_latin1);
            
            // idl:
            writer.WriteLine("module testmod {");
            writer.WriteLine("    struct Test {");
            writer.WriteLine("        long a;");
            writer.WriteLine("    };");
            writer.WriteLine("};");
            
            writer.Flush();
            testSource.Seek(0, SeekOrigin.Begin);
            Assembly result = CreateIdl(testSource);
                       
            // check if struct is correctly created
            Type structType = result.GetType("testmod.Test", true);
            // must be a struct
            Assertion.Assert("is a struct", structType.IsValueType);
            CheckIdlStructAttributePresent(structType);
            CheckSerializableAttributePresent(structType);
            
            CheckFieldPresent(structType, "a", typeof(System.Int32), 
                              BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);            
            CheckNumberOfFields(structType, BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Instance | BindingFlags.DeclaredOnly,
                                1);
            writer.Close();
        }
        
        [Test]
        public void TestUnion() {
            MemoryStream testSource = new MemoryStream();
            StreamWriter writer = new StreamWriter(testSource, s_latin1);
            
            // idl:
            writer.WriteLine("module testmod {");
            writer.WriteLine("    union Test switch(long) {");
            writer.WriteLine("        case 0: short val0;");
            writer.WriteLine("        case 1: ");
            writer.WriteLine("        case 2: long val1;");
			writer.WriteLine("        default: boolean val2;");
            writer.WriteLine("    };");
            writer.WriteLine("};");
            
            writer.Flush();
            testSource.Seek(0, SeekOrigin.Begin);
            Assembly result = CreateIdl(testSource);
                       
            // check if union is correctly created
            Type unionType = result.GetType("testmod.Test", true);
            // must be a struct
            Assertion.Assert("is a struct", unionType.IsValueType);
            CheckIdlUnionAttributePresent(unionType);

            CheckFieldPresent(unionType, "m_discriminator", typeof(System.Int32), 
                              BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            CheckFieldPresent(unionType, "m_val0", typeof(System.Int16), 
                              BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            CheckFieldPresent(unionType, "m_val1", typeof(System.Int32), 
                              BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            CheckFieldPresent(unionType, "m_val2", typeof(System.Boolean), 
                              BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            CheckPublicInstanceMethodPresent(unionType, "Getval0",
                                             typeof(System.Int16), Type.EmptyTypes);
            CheckPublicInstanceMethodPresent(unionType, "Getval1",
                                             typeof(System.Int32), Type.EmptyTypes);
            CheckPublicInstanceMethodPresent(unionType, "Getval2",
                                             typeof(System.Boolean), Type.EmptyTypes);

            CheckPublicInstanceMethodPresent(unionType, "Setval0",
                                             typeof(void), new Type[] { typeof(System.Int16) });
            CheckPublicInstanceMethodPresent(unionType, "Setval1",
                                             typeof(void), new Type[] { typeof(System.Int32), typeof(System.Int32) } );
            CheckPublicInstanceMethodPresent(unionType, "Setval2",
                                             typeof(void), new Type[] { typeof(System.Boolean), typeof(System.Int32) } );            

            CheckMethodPresent(unionType, "GetFieldForDiscriminator", 
                               typeof(FieldInfo), new Type[] { typeof(System.Int32) },
                               BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Static);
            
            writer.Close();
        }          
        
        [Test]
        public void TestIdlSequenceParamters() {
            MemoryStream testSource = new MemoryStream();
            StreamWriter writer = new StreamWriter(testSource, s_latin1);
            
            // idl:
            writer.WriteLine("module testmod {");
            writer.WriteLine("    typedef sequence<long> seqLong;");
            writer.WriteLine("    interface Test {");
            writer.WriteLine("        seqLong EchoSeqLong(in seqLong arg);");
            writer.WriteLine("    };");
            writer.WriteLine("};");
            
            writer.Flush();
            testSource.Seek(0, SeekOrigin.Begin);
            Assembly result = CreateIdl(testSource);
                       
            // check if sequence as method parameters is created correctly
            Type ifContainerType = result.GetType("testmod.Test", true);
            
            MethodInfo seqMethod = ifContainerType.GetMethod("EchoSeqLong",
                                                             BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);                            
            Assertion.AssertNotNull("method not found in seqTest", seqMethod);
            ParameterInfo[] parameters = seqMethod.GetParameters();
            Assertion.AssertEquals("wrong number of paramters; seqTestMethod", 
                                   1, parameters.Length);
            Assertion.AssertEquals("wrong parameter type; seqTestMethod",
                                   typeof(int[]), parameters[0].ParameterType);
            Assertion.AssertEquals("wrong return type; seqTestMethod",
                                   typeof(int[]), seqMethod.ReturnType);
            object[] paramAttrs = parameters[0].GetCustomAttributes(false);
            CheckOnlySpecificCustomAttrInCollection(paramAttrs, typeof(IdlSequenceAttribute));
            object[] returnAttrs = seqMethod.ReturnTypeCustomAttributes.GetCustomAttributes(false);
            CheckOnlySpecificCustomAttrInCollection(returnAttrs, typeof(IdlSequenceAttribute));            
        }

        public void TestConstants() {
            MemoryStream testSource = new MemoryStream();
            StreamWriter writer = new StreamWriter(testSource, s_latin1);
            
            // idl:
            writer.WriteLine("module testmod {");
            writer.WriteLine("    interface Test {");
            writer.WriteLine("        const long MyConstant = 11;");
            writer.WriteLine("    };");
            writer.WriteLine("const long MyOutsideTypeConstant = 13;");
            writer.WriteLine("};");
            writer.WriteLine("const long MyOutsideAllConstant = 19;");
            writer.Flush();
            testSource.Seek(0, SeekOrigin.Begin);
            Assembly result = CreateIdl(testSource);
                       
            // check if classes for constants were created correctly
            Type const1Type = result.GetType("testmod.Test_package.MyConstant", true);
            Type const2Type = result.GetType("testmod.MyOutsideTypeConstant", true);
            Type const3Type = result.GetType("MyOutsideAllConstant", true);

            CheckFieldPresent(const1Type, "ConstVal", typeof(System.Int32), 
                              BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            CheckNumberOfFields(const1Type, BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly,
                                1);                                    
            
            CheckFieldPresent(const2Type, "ConstVal", typeof(System.Int32), 
                              BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            CheckNumberOfFields(const2Type, BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly,
                                1);

            CheckFieldPresent(const3Type, "ConstVal", typeof(System.Int32), 
                              BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            CheckNumberOfFields(const3Type, BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly,
                                1);            
        }

        #endregion IMethods     
        
    }
        
}


#endif
