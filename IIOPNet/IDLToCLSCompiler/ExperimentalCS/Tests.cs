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
        
        private void CheckMethodPresent(Type testType, string methodName, 
                                        Type returnType, Type[] paramTypes) {
            MethodInfo testMethod = testType.GetMethod(methodName, 
                                                       BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                                                       null, paramTypes, null);
            Assertion.AssertNotNull(String.Format("method {0} not found", methodName),
                                    testMethod);
            
            Assertion.AssertEquals(String.Format("wrong return type {0} in method {1}", testMethod.ReturnType, methodName),
                                   returnType, testMethod.ReturnType);                                            
        }
        
        private void CheckIIdlEntityInheritance(Type testType) {
            Type idlEntityIf = testType.GetInterface("IIdlEntity");
            Assertion.AssertNotNull(String.Format("type {0} doesn't inherit from IIdlEntity", testType.FullName),
                                    idlEntityIf);
        }
        
        
        [Test]
        public void TestConcreteInterfaces() {
            MemoryStream testSource = new MemoryStream();
            StreamWriter writer = new StreamWriter(testSource, s_latin1);
            
            // idl:
            writer.WriteLine("module testmod {");
            writer.WriteLine("    interface Test {");
            writer.WriteLine("        octet EchoOctet(in octet arg);");
            writer.WriteLine("    };");
            writer.WriteLine("    #pragma ID Test \"IDL:testmod/Test:1.0\"");
            writer.WriteLine("};");
            
            writer.Flush();
            testSource.Seek(0, SeekOrigin.Begin);
            Assembly result = CreateIdl(testSource);
                       
            // check if interface is correctly created
            Type ifType = result.GetType("testmod.Test", true);
            CheckMethodPresent(ifType, "EchoOctet", 
                               typeof(System.Byte), new Type[] { typeof(System.Byte) });
            CheckRepId(ifType, "IDL:testmod/Test:1.0");
            CheckInterfaceAttr(ifType, IdlTypeInterface.ConcreteInterface);
            CheckIIdlEntityInheritance(ifType);            
            
            writer.Close();
        }
                
        #endregion IMethods
    
        
    }
        
}


#endif
