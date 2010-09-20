/* TestClient.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 08.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using NUnit.Framework;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;

namespace Ch.Elca.Iiop.IntegrationTests {

    [TestFixture]
    public class TestClient {

        #region IFields

        private IiopClientChannel m_channel;

        private TestService m_testService;


        #endregion IFields

        private NamingContext GetNameService() {
            // access COS nameing service
            CorbaInit init = CorbaInit.GetInit();
            NamingContext nameService = init.GetNameService("localhost", 1050);
            return nameService;            
        }


        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopClientChannel();
            ChannelServices.RegisterChannel(m_channel, false);

            NamingContext nameService = GetNameService();
            NameComponent[] name = new NameComponent[] { new NameComponent("test", "") };
            // get the reference to the test-service
            m_testService = (TestService)nameService.resolve(name);
        }

        [TearDown]
        public void TearDownEnvironment() {
            m_testService = null;
            // unregister the channel            
            ChannelServices.UnregisterChannel(m_channel);
        }

        [Test]
        public void TestDouble() {
            System.Double arg = 1.23;
            System.Double result = m_testService.TestIncDouble(arg);
            Assertion.AssertEquals((System.Double)(arg + 1), result);
        }

        [Test]
        public void TestFloat() {
            System.Single arg = 1.23f;
            System.Single result = m_testService.TestIncFloat(arg);
            Assertion.AssertEquals((System.Single)(arg + 1), result);
        }
        
        [Test]
        public void TestByte() {
            System.Byte arg = 1;
            System.Byte result = m_testService.TestIncByte(arg);
            Assertion.AssertEquals((System.Byte)(arg + 1), result);
        }

        [Test]
        public void TestInt16() {
            System.Int16 arg = 1;
            System.Int16 result = m_testService.TestIncInt16(arg);
            Assertion.AssertEquals((System.Int16)(arg + 1), result);
            arg = -11;
            result = m_testService.TestIncInt16(arg);
            Assertion.AssertEquals((System.Int16)(arg + 1), result);
        }

        [Test]
        public void TestInt32() {
            System.Int32 arg = 1;
            System.Int32 result = m_testService.TestIncInt32(arg);
            Assertion.AssertEquals((System.Int32)(arg + 1), result);
            arg = -11;
            result = m_testService.TestIncInt32(arg);
            Assertion.AssertEquals((System.Int32)(arg + 1), result);
        }

        [Test]
        public void TestInt64() {
            System.Int64 arg = 1;
            System.Int64 result = m_testService.TestIncInt64(arg);
            Assertion.AssertEquals((System.Int64)(arg + 1), result);
            arg = -11;
            result = m_testService.TestIncInt64(arg);
            Assertion.AssertEquals((System.Int64)(arg + 1), result);
        }

        [Test]
        public void TestBoolean() {
            System.Boolean arg = true;
            System.Boolean result = m_testService.TestNegateBoolean(arg);
            Assertion.AssertEquals(false, result);
        }

        [Test]
        public void TestVoid() {
            m_testService.TestVoid();
        }
        
        [Test]
        public void TestChar() {
            System.Char arg = 'a';
            System.Char result = m_testService.TestEchoChar(arg);
            Assertion.AssertEquals(arg, result);
            arg = '0';
            result = m_testService.TestEchoChar(arg);
            Assertion.AssertEquals(arg, result);
        }
        
        [Test]
        public void TestString() {
            System.String arg = "test";
            System.String toAppend = "toAppend";
            System.String result = m_testService.TestAppendString(arg, toAppend);
            Assertion.AssertEquals(arg + toAppend, result);
            arg = "test";
            toAppend = null;
            result = m_testService.TestAppendString(arg, toAppend);
            Assertion.AssertEquals(arg, result);
        }       

        [Test]
        public void TestByteArray() {
            System.Byte[] arg = new System.Byte[1];
            arg[0] = 1;
            System.Byte toAppend = 2;
            System.Byte[] result = m_testService.TestAppendElementToByteArray(arg, toAppend);
            Assertion.AssertEquals(2, result.Length);
            Assertion.AssertEquals((System.Byte) arg[0], result[0]);
            Assertion.AssertEquals((System.Byte) toAppend, result[1]);

            arg = null;
            toAppend = 3;
            result = m_testService.TestAppendElementToByteArray(arg, toAppend);
            Assertion.AssertEquals(1, result.Length);
            Assertion.AssertEquals((System.Byte) toAppend, result[0]);
        }

        [Test]
        public void TestLongArray() {
            System.Int64[] arg = new System.Int64[1];
            arg[0] = 134;
            System.Int64 toAppend = 1901;
            System.Int64[] result = m_testService.TestAppendElementToLongArray(arg, toAppend);
            Assertion.AssertEquals(2, result.Length);
            Assertion.AssertEquals((System.Int64) arg[0], result[0]);
            Assertion.AssertEquals((System.Int64) toAppend, result[1]);

            arg = null;
            toAppend = 3098;
            result = m_testService.TestAppendElementToLongArray(arg, toAppend);
            Assertion.AssertEquals(1, result.Length);
            Assertion.AssertEquals((System.Int64) toAppend, result[0]);
        }


        [Test]
        public void TestStringArray() {            
            System.String arg1 = "abc";
            System.String arg2 = "def";
            System.String[] result = m_testService.CreateTwoElemStringArray(arg1, arg2);
            Assertion.AssertEquals(arg1, result[0]);
            Assertion.AssertEquals(arg2, result[1]);
            
            System.String[] arg = new System.String[1];
            arg[0] = "abc";
            System.String toAppend = "def";
            result = m_testService.TestAppendElementToStringArray(arg, toAppend);
            Assertion.AssertEquals(2, result.Length);
            Assertion.AssertEquals("abc", result[0]);
            Assertion.AssertEquals("def", result[1]);

            arg = null;
            toAppend = "hik";
            result = m_testService.TestAppendElementToStringArray(arg, toAppend);
            Assertion.AssertEquals(1, result.Length);
            Assertion.AssertEquals("hik", result[0]);
        }
        
        [Test]
        public void TestJaggedArrays() {
            System.Int32[][] arg1 = new System.Int32[2][];
            arg1[0] = new System.Int32[] { 1 };
            arg1[1] = new System.Int32[] { 2, 3 };
            System.Int32[][] result1 = m_testService.EchoJaggedIntArray(arg1);
            Assertion.AssertEquals(2, result1.Length);
            Assertion.AssertNotNull(result1[0]);
            Assertion.AssertNotNull(result1[1]);
            Assertion.AssertEquals(arg1[0][0], result1[0][0]);
            Assertion.AssertEquals(arg1[1][0], result1[1][0]);
            Assertion.AssertEquals(arg1[1][1], result1[1][1]);
            
            System.Byte[][][] arg2 = new System.Byte[3][][];
            arg2[0] = new System.Byte[][] { new System.Byte[] { 1 } };
            arg2[1] = new System.Byte[][] { new System.Byte[0] };
            arg2[2] = new System.Byte[0][];
            System.Byte[][][] result2 = m_testService.EchoJaggedByteArray(arg2);
            Assertion.AssertEquals(3, result2.Length);
            Assertion.AssertNotNull(result2[0]);
            Assertion.AssertNotNull(result2[1]);
            Assertion.AssertNotNull(result2[2]);
            Assertion.AssertEquals(arg2[0][0][0], result2[0][0][0]);
        }

        [Test]
        public void TestJaggedArraysWithNullElems() {
        System.Int32[][] arg1 = null;
            System.Int32[][] result1 = m_testService.EchoJaggedIntArray(arg1);
            Assertion.AssertEquals(arg1, result1);

            System.Int32[][] arg2 = new System.Int32[2][];
            System.Int32[][] result2 = m_testService.EchoJaggedIntArray(arg2);
            Assertion.AssertNotNull(result2);

            System.String[][] arg3 = null;
            System.String[][] result3 = m_testService.EchoJaggedStringArray(arg3);
            Assertion.AssertEquals(arg3, result3);

            System.String[][] arg4 = new System.String[][] { null, new System.String[] { "abc", "def" } };
            System.String[][] result4 = m_testService.EchoJaggedStringArray(arg4);
            Assertion.AssertNotNull(result4);
            Assertion.AssertNull(result4[0]);
            Assertion.AssertNotNull(result4[1]);
            Assertion.AssertEquals(result4[1][0], arg4[1][0]);
            Assertion.AssertEquals(result4[1][1], arg4[1][1]);
        }

        
        [Test]
        public void TestJaggedStringArrays() {
            System.String[][] arg1 = new System.String[2][];
            arg1[0] = new System.String[] { "test" };
            arg1[1] = new System.String[] { "test2", "test3" };
            System.String[][] result1 = m_testService.EchoJaggedStringArray(arg1);
            Assertion.AssertEquals(2, result1.Length);
            Assertion.AssertNotNull(result1[0]);
            Assertion.AssertNotNull(result1[1]);
            Assertion.AssertEquals(arg1[0][0], result1[0][0]);
            Assertion.AssertEquals(arg1[1][0], result1[1][0]);
            Assertion.AssertEquals(arg1[1][1], result1[1][1]);                        
        }       

        [Test]
        public void TestInterfaceImplementingValueTypeArray() {
            NamedValue[] arg = new NamedValue[] { 
                new NamedValueImplImpl("name1", 1) };
            NamedValue toAppend = new NamedValueImplImpl("name2", 2);
            NamedValue[] result = m_testService.TestAppendElementToNamedValueArray(arg, toAppend);
            Assertion.AssertNotNull(result);
            Assertion.AssertEquals(arg.Length + 1, result.Length);
            for (int i = 0; i < arg.Length; i++) {
                Assertion.AssertEquals(arg[i], result[i]);
            }
            Assertion.AssertEquals(toAppend, result[arg.Length]);
        }
        
        [Test]
        public void TestRemoteObjects() {
            Adder adder = m_testService.RetrieveAdder();
            System.Int32 arg1 = 1;
            System.Int32 arg2 = 2;
            System.Int32 result = adder.Add(1, 2);
            Assertion.AssertEquals((System.Int32) arg1 + arg2, result);            
        }

        [Test]
        public void TestSendRefOfAProxy() {
            Adder adder = m_testService.RetrieveAdder();
            System.Int32 arg1 = 1;
            System.Int32 arg2 = 2;
            System.Int32 result = m_testService.AddWithAdder(adder, arg1, arg2);
            Assertion.AssertEquals((System.Int32) arg1 + arg2, result);
        }

        /// <summary>
        /// Checks, if the repository id of the value-type itself is used and not the rep-id 
        /// for the implementation class
        /// </summary>
        [Test]
        public void TestTypeOfValueTypePassed() {
            TestSerializableClassB2Impl arg = new TestSerializableClassB2Impl();
            arg.Msg = "msg";            
            TestSerializableClassB2 result = m_testService.TestChangeSerializableB2(arg, arg.DetailedMsg);
            Assertion.AssertEquals(result.Msg, arg.Msg);
        }
        
        /// <summary>
        /// Checks, if the fields of a super-type are serilised too
        /// </summary>
        [Test]
        public void TestValueTypeInheritance() {
            TestSerializableClassB2 arg = new TestSerializableClassB2Impl();
            arg.Msg = "msg";
            System.String newDetail = "new detail";
            TestSerializableClassB2 result = m_testService.TestChangeSerializableB2(arg, newDetail);
            Assertion.AssertEquals(newDetail, result.DetailedMsg);
            Assertion.AssertEquals(arg.Msg, result.Msg);
        }

        /// <summary>
        /// Checks, if a formal parameter type, which is not Serilizable works correctly,
        /// if an instance of a Serializable subclass is passed.
        /// </summary>
        [Test]
        public void TestNonSerilizableFormalParam() {
            TestNonSerializableBaseClass arg = new TestSerializableClassCImpl();
            TestNonSerializableBaseClass result = m_testService.TestAbstractValueTypeEcho(arg);
            Assertion.AssertEquals(typeof(TestSerializableClassCImpl), result.GetType());
        }

        [Test]
        public void TestBaseTypeNonSerializableParam() {
            TestSerializableClassCImpl arg = new TestSerializableClassCImpl();
            arg.Msg = "test";
            TestSerializableClassC result = m_testService.TestEchoSerializableC(arg);
            Assertion.AssertEquals(arg.Msg, result.Msg);
            // check method implementation called
            Assertion.AssertEquals(result.Msg, result.Format());
        }

        /// <summary>
        /// Checks, if fields with reference semantics retain their semantic during serialisation / deserialisation
        /// </summary>
        [Test]
        public void TestReferenceSematicForValueTypeField() {
            TestSerializableClassD arg = new TestSerializableClassDImpl();
            arg.val1 = new TestSerializableClassB1Impl();
            arg.val1.Msg = "test";
            arg.val2 = arg.val1;
            System.String newMsg = "test-new";
            TestSerializableClassD result = m_testService.TestChangeSerilizableD(arg, newMsg);
            Assertion.AssertEquals(newMsg, result.val1.Msg);
            Assertion.AssertEquals(result.val1, result.val2);
            Assertion.AssertEquals(result.val1.Msg, result.val2.Msg);
        }
        
        /// <summary>
        /// checks, if recursive values are serialised using an indirection
        /// </summary>
        [Test]
        public void TestRecursiveValueTypeInstance() {            
            TestSerializableClassE arg = new TestSerializableClassEImpl();
            arg.RecArrEntry = new TestSerializableClassE[1];
            arg.RecArrEntry[0] = arg;
            TestSerializableClassE result = m_testService.TestEchoSerializableE(arg);
            Assertion.AssertNotNull(result);
            Assertion.AssertNotNull(result.RecArrEntry);
            Assertion.AssertEquals(arg.RecArrEntry.Length, result.RecArrEntry.Length);
            Assertion.Assert("invalid entry in recArrEntry", (result == result.RecArrEntry[0]));            
        }

        [Test]
        public void TestValueTypeWithMixedContent() {
           System.Boolean arg1 = true;
           System.Int16 arg2 = 2;
           System.Int32 arg3 = 3;
           System.String arg4 = "test";
           TestSerializableMixedValAndBase result = m_testService.TestMixedSerType(arg1, arg2, arg3, arg4);
           Assertion.AssertEquals(arg1, result.basicVal1);
           Assertion.AssertEquals(arg2, result.basicVal2);
           Assertion.AssertEquals(arg3, result.basicVal3);
           Assertion.AssertEquals(arg4, result.val1.Msg);
           Assertion.AssertEquals(arg4, result.val2.Msg);
           Assertion.AssertEquals(arg4, result.val3.Msg);

           TestSerializableClassD result2 = m_testService.TestMixedSerTypeFormalIsBase(arg1, arg2, arg3, arg4);
           Assertion.AssertEquals(arg4, result.val1.Msg);
           Assertion.AssertEquals(arg4, result.val2.Msg);
        }

        [Test]
        public void TestInnerClassSerializable() {
           TestSerWithInner__AnInnerClass inner = new TestSerWithInner__AnInnerClassImpl();
           TestSerWithInner arg = new TestSerWithInnerImpl();
           arg.Field1 = inner;
           
           TestSerWithInner result = m_testService.TestEchoWithInner(arg);
           Assertion.AssertNotNull(result);
           Assertion.AssertNotNull(result.Field1);
           Assertion.AssertEquals(arg.Field1.InnerField1,  result.Field1.InnerField1);
           Assertion.AssertEquals(arg.Field1.InnerField2,  result.Field1.InnerField2);
        }

       
        /// <summary>
        /// Test receiving instances, if formal parameter is System.Object
        /// </summary>
        [Test]
        public void TestReceivingSimpleTypesAsAny() {
            System.Double arg = 1.23;
            object result = m_testService.GetDoubleAsAny(arg);
            Assertion.AssertEquals(result.GetType().FullName, "java.lang._Double");
        }
        
        [Test]
        public void TestStringAsAny() {
            string arg = "TestArg";
            string result = (string) m_testService.EchoAnything(arg);
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestPassingForFormalParamObjectComplexTypes() {           
            TestSerializableClassB1 arg2 = new TestSerializableClassB1Impl();
            arg2.Msg = "msg";
            TestSerializableClassB1 result2 = (TestSerializableClassB1) m_testService.EchoAnything(arg2);
            Assertion.AssertEquals(arg2.Msg, result2.Msg);
        }

        [Test]
        public void TestProperty() {
            System.Int32 arg = 10;
            m_testService.testProp = arg;
            System.Int32 newVal = m_testService.testProp;
            Assertion.AssertEquals(arg, newVal);
        }
        
        [Test]
        public void TestRecursiveValueType() {
            int nrOfChildren = 5;
            TestRecursiveValType result = m_testService.TestRecursiveValueType(nrOfChildren);
            Assertion.AssertNotNull(result);
            Assertion.AssertNotNull(result.children);
            Assertion.AssertEquals(nrOfChildren, result.children.Length);
        }

        [Test]
        public void TestArrayElemTypesWithSpecialMappedRmiNames() {
            int nrOfElems = 5;
            int val = 11;

            _In[] result = m_testService.TestArrayWithIdlConflictingElemType(nrOfElems, val);
            Assertion.AssertNotNull(result);
            Assertion.AssertEquals(nrOfElems, result.Length);
            Assertion.AssertEquals(val, result[0].Val);
            J_TestStartByUnderscore[] result2 = 
                m_testService.TestArrayWithElemTypeNameStartByUnderscore(nrOfElems, val);
            Assertion.AssertNotNull(result2);
            Assertion.AssertEquals(nrOfElems, result2.Length);
            Assertion.AssertEquals(val, result2[0].Val);
        }

        [Test]
        public void TestFragments() {
            // use a really big argument to force fragmentation at server side
            int size = 16000;
            byte[] hugeArg = new byte[size];
            for (int i = 0; i < size; i++) {
                hugeArg[i] = (byte)(i % 256);
            }
            
            byte[] result = m_testService.TestAppendElementToByteArray(hugeArg, (byte)(size % 256));
            Assertion.AssertEquals(result.Length, size + 1);
            for (int i = 0; i < size + 1; i++) {
                Assertion.AssertEquals((byte)(i % 256), result[i]);
            }
        }

        /// <summary>checks, if channel uses is_a to check interface compatiblity on IOR deser,
        /// if other checks don't work</summary>
        [Test]
        public void TestInterfaceCompMbrDeser() {
        TestSimpleInterface1 proxy1 = (TestSimpleInterface1)m_testService.GetSimpleService1();
            Assertion.AssertNotNull("testSimpleService1 ref not received", proxy1);
            Assertion.AssertEquals(true, proxy1.ReturnTrue());

        TestSimpleInterface2 proxy2 = (TestSimpleInterface2)m_testService.GetSimpleService2();
            Assertion.AssertNotNull("testSimpleService2 ref not received", proxy2);
            Assertion.AssertEquals(false, proxy2.ReturnFalse());                        
        }

        [Test]
        public void TestIsACall() {
        omg.org.CORBA.IObject proxy1 = (omg.org.CORBA.IObject)m_testService.GetSimpleService1();
            Assertion.AssertNotNull("testSimpleService1 ref not received", proxy1);            
            Assertion.AssertEquals(true, proxy1._is_a("RMI:Ch.Elca.Iiop.IntegrationTests.TestSimpleInterface1:0000000000000000"));
            
            omg.org.CORBA.IObject proxy2 = (omg.org.CORBA.IObject)m_testService.GetSimpleService2();
            Assertion.AssertNotNull("testSimpleService2 ref not received", proxy2);
            Assertion.AssertEquals(true, proxy2._is_a("RMI:Ch.Elca.Iiop.IntegrationTests.TestSimpleInterface2:0000000000000000"));
        }
        
        [Test]
        public void TestIdlKeyWordPropertyNames() {           
            int[] argSeq = new int[] { 1, 2, 3 };
            m_testService._sequence = argSeq;
            int[] resultSeq = m_testService._sequence;
            Assertion.AssertNotNull("property int seq null", resultSeq);
            Assertion.AssertEquals(argSeq.Length, resultSeq.Length);
            for (int i = 0; i < argSeq.Length; i++) {
                Assertion.AssertEquals("wrong seq entry", argSeq[i], resultSeq[i]);
            }            
        }
        
        [Ignore("doesn't work with sun jdk 1.4; it expects _!")]
        [Test]
        public void TestIdlKeyWordMethodNames() {
            byte arg = 39;
            byte result = m_testService._octet(arg);
            Assertion.AssertEquals("wrong result octet-call", arg, result);
        }

        [Test]
        public void TestNameserviceList() {
            NamingContext nameService = GetNameService();

            Binding[] bindings;
            BindingIterator bindingIterator;
            nameService.list(10, out bindings, out bindingIterator);
            Assertion.Assert("nr of bindings too small", (bindings.Length > 0));

            bool found  = false;
            foreach (Binding binding in bindings) {
                NameComponent[] name = binding.binding_name;                
                if ((name.Length > 0) && (name[0].id.Equals("test"))) {
                    found = true;
                    break;
                }
            }
            Assertion.Assert("service not found", found);
        }

        
    }

}