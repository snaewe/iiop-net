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

        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopClientChannel();
            ChannelServices.RegisterChannel(m_channel);

            // access COS nameing service
            CorbaInit init = CorbaInit.GetInit();
            NamingContext nameService = init.GetNameService("localhost", 8087);
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
        public void TestEnumeration() {
            TestEnum arg = TestEnum.TestEnum_A;
            TestEnum result = m_testService.TestEchoEnumVal(arg);
            Assertion.AssertEquals(arg, result);
            arg = TestEnum.TestEnum_D;
            result = m_testService.TestEchoEnumVal(arg);
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestByteArray() {
            System.Byte[] arg = new System.Byte[1];
            arg[0] = 1;
            System.Byte toAppend = 2;
            System.Byte[] result = m_testService.TestAppendElementToByteArray(arg, toAppend);
            Assertion.AssertEquals(2, result.Length);
            Assertion.AssertEquals((System.Byte) 1, result[0]);
            Assertion.AssertEquals((System.Byte) 2, result[1]);

            arg = null;
            toAppend = 3;
            result = m_testService.TestAppendElementToByteArray(arg, toAppend);
            Assertion.AssertEquals(1, result.Length);
            Assertion.AssertEquals((System.Byte) 3, result[0]);
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
        public void TestMultidimArrays() {
            System.Int32[][] arg1 = new System.Int32[2][];
            arg1[0] = new System.Int32[] { 1 };
            arg1[1] = new System.Int32[] { 2 };
            System.Int32[][] result1 = m_testService.EchoMultiDimIntArray(arg1);
            Assertion.AssertEquals(2, result1.Length);
            Assertion.AssertNotNull(result1[0]);
            Assertion.AssertNotNull(result1[1]);
            Assertion.AssertEquals(arg1[0][0], result1[0][0]);
            Assertion.AssertEquals(arg1[1][0], result1[1][0]);
            
            System.Byte[][][] arg2 = new System.Byte[3][][];
            arg2[0] = new System.Byte[][] { new System.Byte[] { 1 } };
            arg2[1] = new System.Byte[][] { new System.Byte[] { 2 } };
            arg2[2] = new System.Byte[][] { new System.Byte[] { 3 } };
            System.Byte[][][] result2 = m_testService.EchoMultiDimByteArray(arg2);
            Assertion.AssertEquals(3, result2.Length);
            Assertion.AssertNotNull(result2[0]);
            Assertion.AssertNotNull(result2[1]);
            Assertion.AssertNotNull(result2[2]);
            Assertion.AssertEquals(arg2[0][0][0], result2[0][0][0]);

        }
        
        [Test]
        public void TestMutlidimStringArrays() {
            System.String[][] arg1 = new System.String[2][];
            arg1[0] = new System.String[] { "test" };
            arg1[1] = new System.String[] { "test2" };
            System.String[][] result1 = m_testService.EchoMultiDimStringArray(arg1);
            Assertion.AssertEquals(2, result1.Length);
            Assertion.AssertNotNull(result1[0]);
            Assertion.AssertNotNull(result1[1]);
            Assertion.AssertEquals(arg1[0][0], result1[0][0]);
            Assertion.AssertEquals(arg1[1][0], result1[1][0]);
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

        [Test]
        public void TestStruct() {
            TestStructA arg = new TestStructAImpl();
            arg.X = 11;
            arg.Y = -15;
            TestStructA result = m_testService.TestEchoStruct(arg);
            Assertion.AssertEquals(arg.X, result.X);
            Assertion.AssertEquals(arg.Y, result.Y);
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
        /// Checks if a ByRef actual value for a formal parameter interface is passed correctly
        /// </summary>
        [Test]
        public void TestInterfacePassingByRef() {
            TestEchoInterface result = m_testService.RetrieveEchoInterfaceImplementor();
            // result is a proxy
            Assertion.AssertEquals(true, RemotingServices.IsTransparentProxy(result));
            System.Int32 arg = 23;
            System.Int32 echo = result.EchoInt(arg);
            Assertion.AssertEquals(arg, echo);
        }

        /// <summary>
        /// Checks if a ByVal actual value for a formal parameter interface is passed correctly
        /// </summary>        
        [Test]
        public void TestInterfacePassingByVal() {
            System.String initialMsg = "initial";
            TestInterfaceA result = m_testService.RetrieveTestInterfaceAImplementor(initialMsg);
            Assertion.AssertEquals(initialMsg, result.Msg);

            System.String passedBack = m_testService.ExtractMsgFromInterfaceAImplmentor(result);
            Assertion.AssertEquals(initialMsg, passedBack);
        }

        [Test]
        public void TestInheritanceFromInterfaceForValueType() {
            System.String initialMsg = "initial";
            TestAbstrInterfaceImplByMarshalByVal impl = m_testService.RetriveTestInterfaceAImplemtorTheImpl(initialMsg);            
            Assertion.Assert("cast to Interface TestInterfaceA failed", (impl as TestInterfaceA) != null);
            Assertion.AssertEquals(initialMsg, impl.Msg);
        }

        [Test]
        public void TestWritableProperty() {
            System.Double arg = 1.2;
            m_testService.TestProperty = arg;
            System.Double result = m_testService.TestProperty;
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestReadOnlyProperty() {
            System.Double result = m_testService.TestReadOnlyPropertyReturningZero;
            Assertion.AssertEquals((System.Double) 0, result);
            PropertyInfo prop = typeof(TestService).GetProperty("TestReadOnlyPropertyReturningZero");
            Assertion.AssertNotNull(prop);
            Assertion.AssertEquals(false, prop.CanWrite);
            Assertion.AssertEquals(true, prop.CanRead);
        }
        
        /// <summary>
        /// Test passing instances, if formal parameter is System.Object
        /// </summary>
        [Test]
        public void TestPassingForFormalParamObjectSimpleTypes() {
            System.Double arg1 = 1.23;
            System.Double result1 = (System.Double) m_testService.EchoAnything(arg1);
            Assertion.AssertEquals(arg1, result1);

            System.Char arg2 = 'a';
            System.Char result2 = (System.Char) m_testService.EchoAnything(arg2);
            Assertion.AssertEquals(arg2, result2);

            System.Boolean arg3 = true;
            System.Boolean result3 = (System.Boolean) m_testService.EchoAnything(arg3);
            Assertion.AssertEquals(arg3, result3);

            System.Int32 arg4 = 89;
            System.Int32 result4 = (System.Int32) m_testService.EchoAnything(arg4);
            Assertion.AssertEquals(arg4, result4);
        }

        [Test]
        public void TestPassingForFormalParamObjectComplexTypes() {
            System.String arg1 = "test";
            System.String result1 = (System.String) m_testService.EchoAnything(arg1);
            Assertion.AssertEquals(arg1, result1);
            
            TestSerializableClassB1 arg2 = new TestSerializableClassB1Impl();
            arg2.Msg = "msg";
            TestSerializableClassB1 result2 = (TestSerializableClassB1) m_testService.EchoAnything(arg2);
            Assertion.AssertEquals(arg2.Msg, result2.Msg);
        }

        /// <summary>
        /// Checks if arrays can be passed for formal parameter object.
        /// </summary>
        /// <remarks>
        /// Difficulty here is, that at the server, boxed type may not exist yet for array type and must be created on deserialising
        /// any!
        /// </remarks>
        [Test]
        public void TestPassingForFormalParamObjectArrays() {
            System.Byte[] arg3 = new System.Byte[1];
            arg3[0] = 1;
            System.Byte[] result3 = (System.Byte[]) m_testService.EchoAnything(arg3);
            Assertion.AssertEquals(arg3[0], result3[0]);

            System.Int32[] arg4 = new System.Int32[1];
            arg4[0] = 1;
            System.Int32[] result4 = (System.Int32[]) m_testService.EchoAnything(arg4);
            Assertion.AssertEquals(arg4[0], result4[0]);
        }
                
        [Test]
        public void TestEqualityServerAndProxy() {
            bool result = m_testService.CheckEqualityWithServiceV2((TestService)m_testService);
            Assertion.AssertEquals(true, result);
            result = m_testService.CheckEqualityWithService((MarshalByRefObject)m_testService);
            Assertion.AssertEquals(true, result);
        }

        delegate System.Boolean TestNegateBooleanDelegate(System.Boolean arg);

        [Test]
        public void TestAsyncCall() {
            System.Boolean arg = true;
            TestNegateBooleanDelegate nbd = new TestNegateBooleanDelegate(m_testService.TestNegateBoolean);
            // async call
            IAsyncResult ar = nbd.BeginInvoke(arg, null, null);
            // wait for response
            System.Boolean result = nbd.EndInvoke(ar);
            Assertion.AssertEquals(false, result);
        }

        [Test]
        public void TestRefArgs() {
            System.Int32 argInit = 1;
            System.Int32 arg = argInit;
            System.Int32 result = m_testService.TestRef(ref arg);
            Assertion.AssertEquals(arg, result);
            Assertion.AssertEquals(argInit + 1, arg);
        }

        delegate System.Int32 TestOutArgsDelegate(System.Int32 arg, out System.Int32 argOut);

        [Test]
        public void TestOutArgs() {
            System.Int32 argOut;
            System.Int32 arg = 1;
            System.Int32 result = m_testService.TestOut(arg, out argOut);
            Assertion.AssertEquals(arg, argOut);
            Assertion.AssertEquals(arg, result);

            System.Int32 argOut2;
            TestOutArgsDelegate oad = new TestOutArgsDelegate(m_testService.TestOut);
            // async call
            IAsyncResult ar = oad.BeginInvoke(arg, out argOut2, null, null);
            // wait for response
            System.Int32 result2 = oad.EndInvoke(out argOut2, ar);
            Assertion.AssertEquals(arg, argOut2);
            Assertion.AssertEquals(arg, result2);
        }

        [Test]
        public void TestOverloadedMethods() {
            System.Int32 arg1int = 1;
            System.Int32 arg2int = 2;
            System.Int32 arg3int = 2;

            System.Double arg1double = 1.0;
            System.Double arg2double = 2.0;

            System.Int32 result1 = m_testService.AddOverloaded__long__long(arg1int, arg2int);
            Assertion.AssertEquals((System.Int32)(arg1int + arg2int), result1);
            System.Int32 result2 = m_testService.AddOverloaded__long__long__long(arg1int, arg2int, arg3int);
            Assertion.AssertEquals((System.Int32)(arg1int + arg2int + arg3int), result2);
            System.Double result3 = m_testService.AddOverloaded__double__double(arg1double, arg2double);
            Assertion.AssertEquals((System.Double)(arg1double + arg2double), result3);
        }

        [Test]
        public void TestNameClashes() {
            System.Int32 arg = 89;
            System.Int32 result = m_testService._custom(arg);
            Assertion.AssertEquals(arg, result);
           
            m_testService._context = arg;
            Assertion.AssertEquals(arg, m_testService._context);
        }

        [Test]
        public void TestNamesStartingWithUnderScore() {
            System.Int32 arg = 99;
            System.Int32 result = m_testService.N_echoInt(arg);
            Assertion.AssertEquals(arg, result);
        }


    }

}