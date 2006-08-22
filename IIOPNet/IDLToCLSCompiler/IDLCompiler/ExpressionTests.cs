/* ExpressionTests.cs
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
    /// Unit-tests for testing generation
    /// for IDL constant expressions.
    /// </summary>
    [TestFixture]
    public class ExpressionTest : CompilerTestsBase {
                
        private StreamWriter m_writer;
        
        [SetUp]
        public void SetUp() {
            MemoryStream testSource = new MemoryStream();
            m_writer = CreateSourceWriter(testSource);
        }
        
        [TearDown]
        public void TearDown() {
            m_writer.Close();
        }
        
        private void CheckConstantValue(string constTypeName, Assembly asm,
                                          object expected) {
            object val = GetConstantValue(constTypeName, asm);
            Assertion.AssertEquals("field value", expected, val);
        }
        
        private object GetConstantValue(string constTypeName, Assembly asm) {
            Type constType = asm.GetType(constTypeName, false);
            Assertion.AssertNotNull("const type null?", constType);            
            FieldInfo field = constType.GetField("ConstVal", BindingFlags.Public | BindingFlags.Static);
            Assertion.AssertNotNull("const field", field);
            return field.GetValue(null);
        }
       
        [Test]
        public void TestAddInteger() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestAddInteger = 1 + 2;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestAddInteger"));
                                   
            CheckConstantValue("testmod.TestAddInteger", result, (int)3);
        }
        
        [Test]
        public void TestAddFloat() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const double TestAddFloat = 1.0 + 2.0 + 3.0;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestAddFloat"));
                                   
            CheckConstantValue("testmod.TestAddFloat", result, (double)6);
        }        
        
        [Test]
        [ExpectedException(typeof(InvalidOperandInExpressionException))]
        public void TestAddFloatAndInt() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const double TestAddFloatAndInt = 1.0 + 2;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestAddFloatAndInt"));            
        }                
        
        [Test]
        [ExpectedException(typeof(InvalidOperandInExpressionException))]
        public void TestAddIntAndFloat() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestAddIntAndFloat = 1 + 2.0;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestAddIntAndFloat"));            
        }

        [Test]
        public void TestSubInteger() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestSubInteger = 2 - 1;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestSubInteger"));
                                   
            CheckConstantValue("testmod.TestSubInteger", result, (int)1);
        }
        
        [Test]
        public void TestSubFloat() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const double TestSubFloat = 2.0 - 1.0;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestSubFloat"));
                                   
            CheckConstantValue("testmod.TestSubFloat", result, (double)1);
        }        
        
        [Test]
        [ExpectedException(typeof(InvalidOperandInExpressionException))]
        public void TestSubFloatAndInt() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const double TestSubFloatAndInt = 2.0 - 1;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestSubFloatAndInt"));            
        }                
        
        [Test]
        [ExpectedException(typeof(InvalidOperandInExpressionException))]
        public void TestSubIntAndFloat() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestSubIntAndFloat = 2 - 1.0;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestSubIntAndFloat"));            
        }
        
        [Test]
        public void TestAddAndSubInteger() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestAddAndSubInteger = 4 - 3 + 2 - 1;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestAddAndSubInteger"));
                                   
            CheckConstantValue("testmod.TestAddAndSubInteger", result, (int)2);
        }
        
        [Test]
        public void TestBitwiseOr() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestBitwiseOr = 0xFF0000 | 0x1;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestBitwiseOr"));
                                   
            CheckConstantValue("testmod.TestBitwiseOr", result, 
                               (int)(0xFF0000 | 0x1));
        }        
        
        [Test]
        public void TestBitwiseXor() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestBitwiseXor = 0xFF0000 ^ 0x1 ^ 0x10;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestBitwiseXor"));
                                   
            CheckConstantValue("testmod.TestBitwiseXor", result, 
                               (int)(0xFF0000 ^ 0x1 ^ 0x10));
        }        
        
        [Test]
        public void TestBitwiseXorUint64() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const unsigned long long TestBitwiseXorUint64 = 0xFFFFFFFFFFFFFFFF ^ 0xFFFFFFFFFFFFFFF0;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestBitwiseXorUint64"));
            Assertion.Assert( "uint 64 bigger than int64.max?",
                             (0xFFFFFFFFFFFFFFFF > Int64.MaxValue));
            CheckConstantValue("testmod.TestBitwiseXorUint64", result, 
                               (ulong)(0xFFFFFFFFFFFFFFFF ^ 0xFFFFFFFFFFFFFFF0));
        }        
        
        [Test]
        public void TestBitwiseAnd() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestBitwiseAnd = 0xFF0011 & 0x101 & 0x11;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestBitwiseAnd"));
                                   
            CheckConstantValue("testmod.TestBitwiseAnd", result, 
                               (int)(0xFF0011 & 0x101 & 0x11));
        }        
        
        [Test]
        public void TestShiftRight() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestShiftRight = 0xFF >> 4;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestShiftRight"));
                                   
            CheckConstantValue("testmod.TestShiftRight", result, 
                               (int)(0xFF >> 4));
        }        
        
        [Test]
        public void TestShiftLeft() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestShiftLeft = 0xFF << 4;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestShiftLeft"));
                                   
            CheckConstantValue("testmod.TestShiftLeft", result, 
                               (int)(0xFF << 4));
        }                
        
        [Test]
        public void TestShiftRightAndLeft() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestShiftRightAndLeft = 0xFF << 4 >> 2;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestShiftRightAndLeft"));
                                   
            CheckConstantValue("testmod.TestShiftRightAndLeft", result, 
                               (int)(0xFF << 4 >> 2));
        }          
        
        [Test]        
        [ExpectedException(typeof(InvalidOperandInExpressionException))]
        public void TestTooBigShiftLeft() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestTooBigShiftLeft = 0xFF << 64;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestTooBigShiftLeft"));            
        }
        
        [Test]        
        [ExpectedException(typeof(InvalidOperandInExpressionException))]
        public void TestTooSmallShiftLeft() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestTooSmallShiftLeft = 0xFF << -1;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestTooSmallShiftLeft"));            
        }        
        
        [Test]        
        [ExpectedException(typeof(InvalidOperandInExpressionException))]        
        public void TestTooBigShiftRight() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestTooBigShiftRight = 0xFF >> 64;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestTooBigShiftRight"));            
        }
        
        [Test]        
        [ExpectedException(typeof(InvalidOperandInExpressionException))]
        public void TestTooSmallShiftRight() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestTooSmallShiftRight = 0xFF >> -1;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestTooSmallShiftRight"));            
        }        
        
        [Test]
        public void TestMultInteger() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestMultInteger = 2 * 3;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestMultInteger"));
                                   
            CheckConstantValue("testmod.TestMultInteger", result, (int)6);
        }
        
        [Test]
        public void TestMultFloat() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const double TestMultFloat = 1.5 * 2.0 * 3.0;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestMultFloat"));
                                   
            CheckConstantValue("testmod.TestMultFloat", result, (double)9);
        }        
        
        [Test]
        public void TestDivInteger() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestDivInteger = 5 / 2;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestDivInteger"));
                                   
            CheckConstantValue("testmod.TestDivInteger", result, (int)5 / 2);
        }
        
        [Test]
        public void TestDivFloat() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const double TestDivFloat = 10.0 / 2.0 / 2.0;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestDivFloat"));
                                   
            CheckConstantValue("testmod.TestDivFloat", result, (double)2.5);
        }        

        [Test]
        public void TestModInteger() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestModInteger = 5 % 2;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestModInteger"));
                                   
            CheckConstantValue("testmod.TestModInteger", result, (int)5 % 2);
        }
        
        [Test]
        public void TestModFloat() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const double TestModFloat = 20.0 % 11.0 % 2.0;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestModFloat"));
                                   
            CheckConstantValue("testmod.TestModFloat", result, (double) 20.0 % 11.0 % 2.0);
        }        
        
        [Test]
        public void TestNegateInt() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestNegateInt = ~5;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestNegateInt"));
                                   
            CheckConstantValue("testmod.TestNegateInt", result, (int)~5);
        }
        
        [Test]
        public void TestNegateBoolean() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const boolean TestNegateBoolean = ~TRUE;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestNegateBoolean"));
                                   
            CheckConstantValue("testmod.TestNegateBoolean", result, false);                
        }

        
        [Test]
        public void TestUnaryMinus() {            
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestUnaryMinus = -1;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestUnaryMinus"));
                                   
            CheckConstantValue("testmod.TestUnaryMinus", result, (int)-1);
        }
        
        [Test]
        public void TestUnaryPlus() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long TestUnaryPlus = +1;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestUnaryPlus"));
                                   
            CheckConstantValue("testmod.TestUnaryPlus", result, (int)+1);
        }        
        
        [Test]
        public void TestMixedExpression1() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long long TestMixedExpression1a = 1;");
            m_writer.WriteLine("const long long TestMixedExpression1b = (TestMixedExpression1a << 16) | 0xFFFF00000;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestMixedExpression1"));
                                   
            CheckConstantValue("testmod.TestMixedExpression1b", result, (long)((1 << 16) | 0xFFFF00000));
        }
        
        [Test]
        public void TestAssignInt64() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const long long TestAssignInt64_Max = " + Int64.MaxValue + ";");
            m_writer.WriteLine("const long long TestAssignInt64_Min = " + Int64.MinValue + ";");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestAssignInt64"));
                                   
            CheckConstantValue("testmod.TestAssignInt64_Max", result, Int64.MaxValue);
            CheckConstantValue("testmod.TestAssignInt64_Min", result, Int64.MinValue);
        }
        
        [Test]
        public void TestAssignUInt64() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const unsigned long long TestAssignUInt64_Max = " + UInt64.MaxValue + ";");
            m_writer.WriteLine("const unsigned long long TestAssignUInt64_Min = " + UInt64.MinValue + ";");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestAssignUInt64"));

            Int64 constVal = (Int64)GetConstantValue("testmod.TestAssignUInt64_Max", result);
            Assertion.AssertEquals("value", UInt64.MaxValue, unchecked((UInt64)constVal));
            CheckConstantValue("testmod.TestAssignUInt64_Min", result, UInt64.MinValue);
        }                
        
        [Test]
        public void TestFloatInifinity() {
            // idl:
            m_writer.WriteLine("module testmod {");
            m_writer.WriteLine("const double TestFloatInifinity_Plus = Infinity;");
            m_writer.WriteLine("const double TestFloatInifinity_Minus = - Infinity;");
            m_writer.WriteLine("};");
            m_writer.Flush();
            m_writer.BaseStream.Seek(0, SeekOrigin.Begin);
            Assembly result = 
                CreateIdl(m_writer.BaseStream, GetAssemblyName("ExpressionTest_TestFloatInifinity"));
                                   
            CheckConstantValue("testmod.TestFloatInifinity_Plus", result, Double.PositiveInfinity);
            CheckConstantValue("testmod.TestFloatInifinity_Minus", result, Double.NegativeInfinity);
        
            
        
        }
    
    }
    
    
}

#endif
