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
using omg.org.CORBA;

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
            TestEnum arg = TestEnum.A;
            TestEnum result = m_testService.TestEchoEnumVal(arg);
            Assertion.AssertEquals(arg, result);
            arg = TestEnum.D;
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
            System.Int32[,] arg1 = new System.Int32[2,2];
            arg1[0,0] = 1;
            arg1[0,1] = 2;
            arg1[1,0] = 3;
            arg1[1,1] = 4;

            System.Int32[,] result1 = m_testService.EchoMultiDimIntArray(arg1);
            Assertion.AssertEquals(arg1[0,0], result1[0,0]);
            Assertion.AssertEquals(arg1[1,0], result1[1,0]);
            Assertion.AssertEquals(arg1[0,1], result1[0,1]);
            Assertion.AssertEquals(arg1[1,1], result1[1,1]);            
        }
        
        [Test]
        public void TestMutlidimStringArrays() {
            System.String[,] arg1 = new System.String[2,2];
            arg1[0,0] = "test0";
            arg1[0,1] = "test1";
            arg1[1,0] = "test2";
            arg1[1,1] = "test3";
            System.String[,] result1 = m_testService.EchoMultiDimStringArray(arg1);
            Assertion.AssertEquals(arg1[0,0], result1[0,0]);
            Assertion.AssertEquals(arg1[0,1], result1[0,1]);
            Assertion.AssertEquals(arg1[1,0], result1[1,0]);
            Assertion.AssertEquals(arg1[1,1], result1[1,1]);
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
            TestStructA arg = new TestStructA();
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
            TestSerializableClassB2 arg = new TestSerializableClassB2();
            arg.Msg = "msg";            
            TestSerializableClassB2 result = m_testService.TestChangeSerializableB2(arg, arg.DetailedMsg);
            Assertion.AssertEquals(result.Msg, arg.Msg);
        }
        
        /// <summary>
        /// Checks, if the fields of a super-type are serilised too
        /// </summary>
        [Test]
        public void TestValueTypeInheritance() {
            TestSerializableClassB2 arg = new TestSerializableClassB2();
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
            TestNonSerializableBaseClass arg = new TestSerializableClassC();
            TestNonSerializableBaseClass result = m_testService.TestAbstractValueTypeEcho(arg);
            Assertion.AssertEquals(typeof(TestSerializableClassC), result.GetType());
        }

        [Test]
        public void TestBaseTypeNonSerializableParam() {
            TestSerializableClassC arg = new TestSerializableClassC();
            arg.Msg = "test";
            TestSerializableClassC result = m_testService.TestEchoSerializableC(arg);
            Assertion.AssertEquals(arg.Msg, result.Msg);
        }

        /// <summary>
        /// Checks, if fields with reference semantics retain their semantic during serialisation / deserialisation
        /// </summary>
        [Test]
        public void TestReferenceSematicForValueTypeField() {
            TestSerializableClassD arg = new TestSerializableClassD();
            arg.val1 = new TestSerializableClassB1();
            arg.val1.Msg = "test";
            arg.val2 = arg.val1;
            System.String newMsg = "test-new";
            TestSerializableClassD result = m_testService.TestChangeSerilizableD(arg, newMsg);
            Assertion.AssertEquals(newMsg, result.val1.Msg);
            Assertion.AssertEquals(result.val1, result.val2);
            Assertion.AssertEquals(result.val1.Msg, result.val2.Msg);
        }
        
        [Test]
        public void TestRecursiveValueType() {            
            TestSerializableClassE arg = new TestSerializableClassE();
            arg.RecArrEntry = new TestSerializableClassE[1];
            arg.RecArrEntry[0] = arg;
            TestSerializableClassE result = m_testService.TestEchoSerializableE(arg);
            Assertion.AssertNotNull(result);
            Assertion.AssertNotNull(result.RecArrEntry);
            Assertion.AssertEquals(arg.RecArrEntry.Length, result.RecArrEntry.Length);
            Assertion.Assert("invalid entry in recArrEntry", (result == result.RecArrEntry[0]));
            
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
        public void TestCustomAnyTypeCode() {
            System.String testString = "abcd";
            OrbServices orb = OrbServices.GetSingleton();
            omg.org.CORBA.TypeCode wstringTc = orb.create_wstring_tc(0);
            Any any = new Any(testString, wstringTc);
            System.String echo = (System.String)m_testService.EchoAnything(any);
            Assertion.AssertEquals(testString, echo);
        }

        [Test]
        public void TestPassingForFormalParamObjectComplexTypes() {
            System.String arg1 = "test";
            System.String result1 = (System.String) m_testService.EchoAnything(arg1);
            Assertion.AssertEquals(arg1, result1);
            
            TestSerializableClassB1 arg2 = new TestSerializableClassB1();
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

            System.Int32 result1 = m_testService.AddOverloaded(arg1int, arg2int);
            Assertion.AssertEquals((System.Int32)(arg1int + arg2int), result1);
            System.Int32 result2 = m_testService.AddOverloaded(arg1int, arg2int, arg3int);
            Assertion.AssertEquals((System.Int32)(arg1int + arg2int + arg3int), result2);
            System.Double result3 = m_testService.AddOverloaded(arg1double, arg2double);
            Assertion.AssertEquals((System.Double)(arg1double + arg2double), result3);
        }

        [Test]
        public void TestNameClashes() {
            System.Int32 arg = 89;
            System.Int32 result = m_testService.custom(arg);
            Assertion.AssertEquals(arg, result);
           
            m_testService.context = arg;
            Assertion.AssertEquals(arg, m_testService.context);
        }

        [Test]
        public void TestNamesStartingWithUnderScore() {
            System.Int32 arg = 99;
            System.Int32 result = m_testService._echoInt(arg);
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestCheckParamAttrs() {
            System.String arg = "testArg";
            System.String result = m_testService.CheckParamAttrs(arg);
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestSimpleUnionNoExceptions() {
            TestUnion arg = new TestUnion();
            short case0Val = 11;
            arg.Setval0(case0Val);
            TestUnion result = m_testService.EchoUnion(arg);
            Assertion.AssertEquals(case0Val, result.Getval0());
            Assertion.AssertEquals(0, result.Discriminator);

        TestUnion arg2 = new TestUnion();
            int case1Val = 12;
            arg2.Setval1(case1Val, 2);
            TestUnion result2 = m_testService.EchoUnion(arg2);
            Assertion.AssertEquals(case1Val, result2.Getval1());
            Assertion.AssertEquals(2, result2.Discriminator);

        TestUnion arg3 = new TestUnion();
            bool case2Val = true;
            arg3.Setval2(case2Val, 7);
            TestUnion result3 = m_testService.EchoUnion(arg3);
            Assertion.AssertEquals(case2Val, result3.Getval2());
            Assertion.AssertEquals(7, result3.Discriminator);            

        }

        [Test]
        public void TestEnumBasedUnionNoExceptions() {
            TestUnionE arg = new TestUnionE();
            short case0Val = 11;
            arg.SetvalE0(case0Val);
            TestUnionE result = m_testService.EchoUnionE(arg);
            Assertion.AssertEquals(case0Val, result.GetvalE0());
            Assertion.AssertEquals(TestEnumForU.A, result.Discriminator);

        TestUnionE arg2 = new TestUnionE();
            TestEnumForU case1Val = TestEnumForU.A;
            arg2.SetvalE1(case1Val, TestEnumForU.B);
            TestUnionE result2 = m_testService.EchoUnionE(arg2);
            Assertion.AssertEquals(case1Val, result2.GetvalE1());
            Assertion.AssertEquals(TestEnumForU.B, result2.Discriminator);
        }

        [Test]
        public void TestUnionExceptions() {
            try {
                TestUnion arg = new TestUnion();
                arg.Getval0();
                Assertion.Fail("exception not thrown for getting value from non-initalized union");
            } catch (omg.org.CORBA.BAD_OPERATION) {
            }
            try {
                TestUnion arg = new TestUnion();
                arg.Setval0(11);
                arg.Getval1();
                Assertion.Fail("exception not thrown for getting value from non-initalized union");
            } catch (omg.org.CORBA.BAD_OPERATION) {
            }
            try {
                TestUnion arg1 = new TestUnion();
                arg1.Setval1(11, 7);
                Assertion.Fail("exception not thrown on wrong discriminator value.");
            } catch (omg.org.CORBA.BAD_PARAM) {
            }
            try {
                TestUnion arg2 = new TestUnion();
                arg2.Setval2(false, 0);
                Assertion.Fail("exception not thrown on wrong discriminator value.");
            } catch (omg.org.CORBA.BAD_PARAM) {
            }
        }

        [Test]
        public void TestConstant() {
            Int32 constVal = MyConstant.ConstVal;
            Assertion.AssertEquals("wrong constant value", 11, constVal);
            
            Int64 maxIntVal = Max_int.ConstVal;
            Assertion.AssertEquals("wrong constant value", 
                                   Int64.MaxValue, maxIntVal);            
            
            // regression test for BUG #909562
            Int64 minIntVal = Min_int.ConstVal;
            Assertion.AssertEquals("wrong constant value", 
                                   Int64.MinValue, minIntVal);
                                              
            Int64 zeroIntVal = Zero_val.ConstVal;
            Assertion.AssertEquals("wrong constant value", 0, zeroIntVal);

            Int64 zeroFromHex = Zero_from_hex.ConstVal;             
            Assertion.AssertEquals("wrong constant value", 0, zeroFromHex);
            
            Int64 oneFromHex = One_from_hex.ConstVal;           
            Assertion.AssertEquals("wrong constant value", 1, oneFromHex);
            
            Int64 minusOneFromHex = Minus_one_from_hex.ConstVal;            
            Assertion.AssertEquals("wrong constant value", -1, minusOneFromHex);
    
            Single zeroValFloat = Zero_val_float.ConstVal;
            Assertion.AssertEquals("wrong constant value", 0.0, zeroValFloat);
    
            Single minusOneFloat = Minus_one_float.ConstVal;
            Assertion.AssertEquals("wrong constant value", -1.0, minusOneFloat);
            
            Single plusOneFloat = Plus_one_float.ConstVal;
            Assertion.AssertEquals("wrong constant value", 1.0, plusOneFloat);
            
            Single plus_inf = Plus_Inf.ConstVal;
            Assertion.AssertEquals("wrong constant value", Single.PositiveInfinity, plus_inf);
            Single minus_inf = Minus_Inf.ConstVal;
            Assertion.AssertEquals("wrong constant value", Single.NegativeInfinity, minus_inf);
            
            
        }

        [Test]
        public void TestPassingUnionsAsAny() {
            TestUnion arg = new TestUnion();
            short case0Val = 11;
            arg.Setval0(case0Val);
            TestUnion result = (TestUnion)m_testService.EchoAnything(arg);
            Assertion.AssertEquals(case0Val, result.Getval0());
            Assertion.AssertEquals(0, result.Discriminator);

        TestUnionE arg2 = new TestUnionE();
            TestEnumForU case1Val = TestEnumForU.A;
            arg2.SetvalE1(case1Val, TestEnumForU.B);
            TestUnionE result2 = (TestUnionE)m_testService.EchoAnything(arg2);
            Assertion.AssertEquals(case1Val, result2.GetvalE1());
            Assertion.AssertEquals(TestEnumForU.B, result2.Discriminator);
        }

        [Test]
        public void TestReceivingUnknownUnionsAsAny() {
        object result = m_testService.RetrieveUnknownUnionAsAny();
            Assertion.AssertNotNull("union not retrieved", result);
            Assertion.AssertEquals("type name", "Ch.Elca.Iiop.IntegrationTests.TestUnionE2", result.GetType().FullName);
        }


        [Test]
        public void TestReferenceOtherConstant() {
            Int32 constValA = AVAL.ConstVal;
            Assertion.AssertEquals("wrong constant value", 1, constValA);
            Int32 constValB = BVAL.ConstVal;
            Assertion.AssertEquals("wrong constant value", 1, constValB);
        }


        [Test]
        public void TestCharacterConstant() {
            Char constValNonEscapeCharConst = NonEscapeCharConst.ConstVal;
            Assertion.AssertEquals("wrong constant value", 'a', constValNonEscapeCharConst);

            Char constValUnicodeEscapeCharConst1 = UnicodeEscapeCharConst1.ConstVal;
            Assertion.AssertEquals("wrong constant value", '\u0062', constValUnicodeEscapeCharConst1);

            Char constValUnicodeEscapeCharConst2 = UnicodeEscapeCharConst2.ConstVal;
            Assertion.AssertEquals("wrong constant value", '\uFFFF', constValUnicodeEscapeCharConst2);

            Char constValHexEscapeCharConst = HexEscapeCharConst.ConstVal;
            Assertion.AssertEquals("wrong constant value", '\u0062', constValHexEscapeCharConst);

            Char constValDecEscapeCharConst1 = DecEscapeCharConst1.ConstVal;
            Assertion.AssertEquals("wrong constant value", 'a', constValDecEscapeCharConst1);

            Char constValDecEscapeCharConst2 = DecEscapeCharConst2.ConstVal;
            Assertion.AssertEquals("wrong constant value", '\u0000', constValDecEscapeCharConst2);
        }

        [Test]
        public void TestCharacterConstantBugReport841774() {
            Char constValStandAlone = STAND_ALONE_TEST.ConstVal;
            Assertion.AssertEquals("wrong constant value", '1', constValStandAlone);
            Char constValNetWork = NETWORK_TEST.ConstVal;
            Assertion.AssertEquals("wrong constant value", '2', constValNetWork);
            Char constValProduction = PRODUCTION.ConstVal;
            Assertion.AssertEquals("wrong constant value", '3', constValProduction);
        }

        [Test]
        public void TestWstringLiteralBugReport906401() {
            String val_a = COMP_NAME_A.ConstVal;
            Assertion.AssertEquals("wrong constant value", "test", val_a);
            String val_b = COMP_NAME_B.ConstVal;
            Assertion.AssertEquals("wrong constant value", "java:comp/env/ejb/Fibo", val_b);
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

            TestSimpleInterface1 proxy3 = (TestSimpleInterface1)m_testService.GetWhenSuppIfMissing();
            Assertion.AssertNotNull("testSimpleService1 ref not received", proxy3);
            Assertion.AssertEquals(true, proxy3.ReturnTrue());
                        
        }

        [Test]
        public void TestIsACall() {
            omg.org.CORBA.IObject proxy1 = (omg.org.CORBA.IObject)m_testService.GetSimpleService1();
            Assertion.AssertNotNull("testSimpleService1 ref not received", proxy1);            
            Assertion.AssertEquals(true, proxy1._is_a("IDL:Ch/Elca/Iiop/IntegrationTests/TestSimpleInterface1:1.0"));
            
            omg.org.CORBA.IObject proxy2 = (omg.org.CORBA.IObject)m_testService.GetSimpleService2();
            Assertion.AssertNotNull("testSimpleService2 ref not received", proxy2);
            Assertion.AssertEquals(true, proxy2._is_a("IDL:Ch/Elca/Iiop/IntegrationTests/TestSimpleInterface2:1.0"));
            
            
            // test using ORBServices
            omg.org.CORBA.OrbServices orb = omg.org.CORBA.OrbServices.GetSingleton();
            Assertion.AssertEquals(true, orb.is_a(proxy1, typeof(TestSimpleInterface1)));
            Assertion.AssertEquals(true, orb.is_a(proxy2, typeof(TestSimpleInterface2)));
            // target object implements both interfaces
            Assertion.AssertEquals(true, orb.is_a(proxy1, typeof(TestSimpleInterface2)));
            Assertion.AssertEquals(true, orb.is_a(proxy2, typeof(TestSimpleInterface1)));
            
            Assertion.AssertEquals(false, orb.is_a(m_testService, typeof(TestSimpleInterface1)));
            Assertion.AssertEquals(false, orb.is_a(m_testService, typeof(TestSimpleInterface2)));
        }
        
        [Test]
        public void TestNonExistentCall() {         
            // test using ORBServices
            omg.org.CORBA.OrbServices orb = omg.org.CORBA.OrbServices.GetSingleton();

            Assertion.AssertEquals(false, orb.non_existent(m_testService));
            object nonExObject = orb.string_to_object("iiop://localhost:8087/someNonExistingObject");
            Assertion.AssertEquals(true, orb.non_existent(nonExObject));
        }
        

    }

}
