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
using System.Text;
using NUnit.Framework;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;
using omg.org.CORBA;
using Ch.Elca.Iiop.CorbaObjRef;

namespace Ch.Elca.Iiop.IntegrationTests {

    [TestFixture]
    public class TestClient {

        #region IFields

        private IiopClientChannel m_channel;

        private TestService m_testService;


        #endregion IFields

        [TestFixtureSetUp]
        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopClientChannel();
            ChannelServices.RegisterChannel(m_channel, false);

            // get the reference to the test-service
            m_testService = (TestService)RemotingServices.Connect(typeof(TestService), "corbaloc:iiop:1.2@localhost:8087/test");
        }

        [TestFixtureTearDown]
        public void TearDownEnvironment() {
            m_testService = null;
            // unregister the channel
            ChannelServices.UnregisterChannel(m_channel);
        }

        [Test]
        public void TestDouble() {
            System.Double arg = 1.23;
            System.Double result = m_testService.TestIncDouble(arg);
            Assert.AreEqual((System.Double)(arg + 1), result);
        }

        [Test]
        public void TestFloat() {
            System.Single arg = 1.23f;
            System.Single result = m_testService.TestIncFloat(arg);
            Assert.AreEqual((System.Single)(arg + 1), result);
        }
        
        [Test]
        public void TestByte() {
            System.Byte arg = 1;
            System.Byte result = m_testService.TestIncByte(arg);
            Assert.AreEqual((System.Byte)(arg + 1), result);
        }

        [Test]
        public void TestInt16() {
            System.Int16 arg = 1;
            System.Int16 result = m_testService.TestIncInt16(arg);
            Assert.AreEqual((System.Int16)(arg + 1), result);
            arg = -11;
            result = m_testService.TestIncInt16(arg);
            Assert.AreEqual((System.Int16)(arg + 1), result);
        }

        [Test]
        public void TestInt32() {
            System.Int32 arg = 1;
            System.Int32 result = m_testService.TestIncInt32(arg);
            Assert.AreEqual((System.Int32)(arg + 1), result);
            arg = -11;
            result = m_testService.TestIncInt32(arg);
            Assert.AreEqual((System.Int32)(arg + 1), result);
        }

        [Test]
        public void TestInt64() {
            System.Int64 arg = 1;
            System.Int64 result = m_testService.TestIncInt64(arg);
            Assert.AreEqual((System.Int64)(arg + 1), result);
            arg = -11;
            result = m_testService.TestIncInt64(arg);
            Assert.AreEqual((System.Int64)(arg + 1), result);
        }

        [Test]
        public void TestSByte() {
            System.SByte arg = 1;
            System.SByte result = m_testService.TestIncSByte(arg);
            Assert.AreEqual((System.SByte)(arg + 1), result);
            arg = -2;
            result = m_testService.TestIncSByte(arg);
            Assert.AreEqual((System.SByte)(arg + 1), result);
        }

        [Test]
        public void TestSByteAsAny() {
            System.SByte arg = 1;
            System.SByte result = (System.SByte)
                ((System.Byte)m_testService.EchoAnything(arg));
            Assert.AreEqual((System.SByte)arg, result);
            arg = -2;
            result = (System.SByte)
                ((System.Byte)m_testService.EchoAnything(arg));
            Assert.AreEqual((System.SByte)arg, result);

            Any argAny = new Any(arg);
            Any resultAny = m_testService.EchoAnythingContainer(argAny);
            result = (System.SByte)
                ((System.Byte)resultAny.Value);
            Assert.AreEqual((System.SByte)arg, result);
        }

        [Test]
        public void TestUInt16() {
            System.UInt16 arg = 1;
            System.UInt16 result = m_testService.TestIncUInt16(arg);
            Assert.AreEqual((System.Int16)(arg + 1), result);
            arg = System.UInt16.MaxValue - (System.UInt16)1;
            result = m_testService.TestIncUInt16(arg);
            Assert.AreEqual((System.UInt16)(arg + 1), result);
        }

        [Test]
        public void TestUInt32() {
            System.UInt32 arg = 1;
            System.UInt32 result = m_testService.TestIncUInt32(arg);
            Assert.AreEqual((System.UInt32)(arg + 1), result);
            arg = System.UInt32.MaxValue - (System.UInt32)1;
            result = m_testService.TestIncUInt32(arg);
            Assert.AreEqual((System.UInt32)(arg + 1), result);
        }

        [Test]
        public void TestUInt64() {
            System.UInt64 arg = 1;
            System.UInt64 result = m_testService.TestIncUInt64(arg);
            Assert.AreEqual((System.UInt64)(arg + 1), result);
            arg = System.UInt64.MaxValue - 1;
            result = m_testService.TestIncUInt64(arg);
            Assert.AreEqual((System.UInt64)(arg + 1), result);
        }

        [Test]
        public void TestUInt64AsAny() {
            System.UInt64 arg = 1;
            System.UInt64 result = (System.UInt64)
                ((System.Int64)m_testService.EchoAnything(arg));
            Assert.AreEqual(arg, result);
            arg = System.UInt64.MaxValue - 1;
            result = (System.UInt64)
                ((System.Int64)m_testService.EchoAnything(arg));
            Assert.AreEqual(arg, result);
            
            Any argAny = new Any(arg);
            Any resultAny = m_testService.EchoAnythingContainer(argAny);
            result = (System.UInt64)resultAny.Value;
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestBoolean() {
            System.Boolean arg = true;
            System.Boolean result = m_testService.TestNegateBoolean(arg);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestVoid() {
            m_testService.TestVoid();
        }
        
        [Test]
        public void TestChar() {
            System.Char arg = 'a';
            System.Char result = m_testService.TestEchoChar(arg);
            Assert.AreEqual(arg, result);
            arg = '0';
            result = m_testService.TestEchoChar(arg);
            Assert.AreEqual(arg, result);
        }
        
        [Test]
        public void TestString() {
            System.String arg = "test";
            System.String toAppend = "toAppend";
            System.String result = m_testService.TestAppendString(arg, toAppend);
            Assert.AreEqual(arg + toAppend, result);
            arg = "test";
            toAppend = null;
            result = m_testService.TestAppendString(arg, toAppend);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumeration() {
            TestEnum arg = TestEnum.A;
            TestEnum result = m_testService.TestEchoEnumVal(arg);
            Assert.AreEqual(arg, result);
            arg = TestEnum.D;
            result = m_testService.TestEchoEnumVal(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestFlagsArguments() {
            TestFlags arg = TestFlags.A1;
            TestFlags result = m_testService.TestEchoFlagsVal(arg);
            Assert.AreEqual(arg, result);
            arg = TestFlags.All;
            result = m_testService.TestEchoFlagsVal(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumBI16Val() {
            TestEnumBI16 arg = TestEnumBI16.B1;
            TestEnumBI16 result = m_testService.TestEchoEnumI16Val(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumBUI32Val() {
            TestEnumUI32 arg = TestEnumUI32.C2;
            TestEnumUI32 result = m_testService.TestEchoEnumUI32Val(arg);
            Assert.AreEqual(arg, result);
            arg = TestEnumUI32.A2;
            result = m_testService.TestEchoEnumUI32Val(arg);
            Assert.AreEqual(arg, result);
            arg = TestEnumUI32.B2;
            result = m_testService.TestEchoEnumUI32Val(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumBI64Val() {
            TestEnumBI64 arg = TestEnumBI64.AL;
            TestEnumBI64 result = m_testService.TestEchoEnumI64Val(arg);
            Assert.AreEqual(arg, result);

            arg = TestEnumBI64.BL;
            result = m_testService.TestEchoEnumI64Val(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumAsAny() {
            TestEnum arg = TestEnum.A;
            TestEnum result = (TestEnum)m_testService.EchoAnything(arg);
            Assert.AreEqual(arg, result);
            arg = TestEnum.D;
            result = (TestEnum)m_testService.EchoAnything(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestFlagsAsAny() {
            TestFlags arg = TestFlags.A1;
            TestFlags result = (TestFlags)m_testService.EchoAnything(arg);
            Assert.AreEqual(arg, result);
            arg = TestFlags.All;
            result = (TestFlags)m_testService.EchoAnything(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestByteArray() {
            System.Byte[] arg = new System.Byte[1];
            arg[0] = 1;
            System.Byte toAppend = 2;
            System.Byte[] result = m_testService.TestAppendElementToByteArray(arg, toAppend);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual((System.Byte) 1, result[0]);
            Assert.AreEqual((System.Byte) 2, result[1]);

            arg = null;
            toAppend = 3;
            result = m_testService.TestAppendElementToByteArray(arg, toAppend);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual((System.Byte) 3, result[0]);
        }

        [Test]
        public void TestStringArray() {
            System.String arg1 = "abc";
            System.String arg2 = "def";
            System.String[] result = m_testService.CreateTwoElemStringArray(arg1, arg2);
            Assert.AreEqual(arg1, result[0]);
            Assert.AreEqual(arg2, result[1]);
            
            System.String[] arg = new System.String[1];
            arg[0] = "abc";
            System.String toAppend = "def";
            result = m_testService.TestAppendElementToStringArray(arg, toAppend);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("abc", result[0]);
            Assert.AreEqual("def", result[1]);

            arg = null;
            toAppend = "hik";
            result = m_testService.TestAppendElementToStringArray(arg, toAppend);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("hik", result[0]);
        }
        
        [Test]
        public void TestJaggedArrays() {
            System.Int32[][] arg1 = new System.Int32[2][];
            arg1[0] = new System.Int32[] { 1 };
            arg1[1] = new System.Int32[] { 2, 3 };
            System.Int32[][] result1 = m_testService.EchoJaggedIntArray(arg1);
            Assert.AreEqual(2, result1.Length);
            Assert.NotNull(result1[0]);
            Assert.NotNull(result1[1]);
            Assert.AreEqual(arg1[0][0], result1[0][0]);
            Assert.AreEqual(arg1[1][0], result1[1][0]);
            Assert.AreEqual(arg1[1][1], result1[1][1]);
            
            System.Byte[][][] arg2 = new System.Byte[3][][];
            arg2[0] = new System.Byte[][] { new System.Byte[] { 1 } };
            arg2[1] = new System.Byte[][] { new System.Byte[0] };
            arg2[2] = new System.Byte[0][];
            System.Byte[][][] result2 = m_testService.EchoJaggedByteArray(arg2);
            Assert.AreEqual(3, result2.Length);
            Assert.NotNull(result2[0]);
            Assert.NotNull(result2[1]);
            Assert.NotNull(result2[2]);
            Assert.AreEqual(arg2[0][0][0], result2[0][0][0]);
        }

        [Test]
        public void TestJaggedArraysWithNullElems() {
        System.Int32[][] arg1 = null;
            System.Int32[][] result1 = m_testService.EchoJaggedIntArray(arg1);
            Assert.AreEqual(arg1, result1);

            System.Int32[][] arg2 = new System.Int32[2][];
            System.Int32[][] result2 = m_testService.EchoJaggedIntArray(arg2);
            Assert.NotNull(result2);

            System.String[][] arg3 = null;
            System.String[][] result3 = m_testService.EchoJaggedStringArray(arg3);
            Assert.AreEqual(arg3, result3);

            System.String[][] arg4 = new System.String[][] { null, new System.String[] { "abc", "def" } };
            System.String[][] result4 = m_testService.EchoJaggedStringArray(arg4);
            Assert.NotNull(result4);
            Assert.Null(result4[0]);
            Assert.NotNull(result4[1]);
            Assert.AreEqual(result4[1][0], arg4[1][0]);
            Assert.AreEqual(result4[1][1], arg4[1][1]);
        }

        
        [Test]
        public void TestJaggedStringArrays() {
            System.String[][] arg1 = new System.String[2][];
            arg1[0] = new System.String[] { "test" };
            arg1[1] = new System.String[] { "test2", "test3" };
            System.String[][] result1 = m_testService.EchoJaggedStringArray(arg1);
            Assert.AreEqual(2, result1.Length);
            Assert.NotNull(result1[0]);
            Assert.NotNull(result1[1]);
            Assert.AreEqual(arg1[0][0], result1[0][0]);
            Assert.AreEqual(arg1[1][0], result1[1][0]);
            Assert.AreEqual(arg1[1][1], result1[1][1]);
        }
        
        [Test]
        public void TestMultidimArrays() {
            System.Int32[,] arg1 = new System.Int32[2,2];
            arg1[0,0] = 1;
            arg1[0,1] = 2;
            arg1[1,0] = 3;
            arg1[1,1] = 4;

            System.Int32[,] result1 = m_testService.EchoMultiDimIntArray(arg1);
            Assert.AreEqual(arg1[0,0], result1[0,0]);
            Assert.AreEqual(arg1[1,0], result1[1,0]);
            Assert.AreEqual(arg1[0,1], result1[0,1]);
            Assert.AreEqual(arg1[1,1], result1[1,1]);
        }
        
        [Test]
        public void TestMutlidimStringArrays() {
            System.String[,] arg1 = new System.String[2,2];
            arg1[0,0] = "test0";
            arg1[0,1] = "test1";
            arg1[1,0] = "test2";
            arg1[1,1] = "test3";
            System.String[,] result1 = m_testService.EchoMultiDimStringArray(arg1);
            Assert.AreEqual(arg1[0,0], result1[0,0]);
            Assert.AreEqual(arg1[0,1], result1[0,1]);
            Assert.AreEqual(arg1[1,0], result1[1,0]);
            Assert.AreEqual(arg1[1,1], result1[1,1]);
        }

        [Test]
        public void TestRemoteObjects() {
            Adder adder = m_testService.RetrieveAdder();
            System.Int32 arg1 = 1;
            System.Int32 arg2 = 2;
            System.Int32 result = adder.Add(1, 2);
            Assert.AreEqual((System.Int32) arg1 + arg2, result);
        }

        [Test]
        public void TestSendRefOfAProxy() {
            Adder adder = m_testService.RetrieveAdder();
            System.Int32 arg1 = 1;
            System.Int32 arg2 = 2;
            System.Int32 result = m_testService.AddWithAdder(adder, arg1, arg2);
            Assert.AreEqual((System.Int32) arg1 + arg2, result);
        }

        [Test]
        public void TestStruct() {
            TestStructA arg = new TestStructA();
            arg.X = 11;
            arg.Y = -15;
            TestStructA result = m_testService.TestEchoStruct(arg);
            Assert.AreEqual(arg.X, result.X);
            Assert.AreEqual(arg.Y, result.Y);
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
            Assert.AreEqual(result.Msg, arg.Msg);
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
            Assert.AreEqual(newDetail, result.DetailedMsg);
            Assert.AreEqual(arg.Msg, result.Msg);
        }

        /// <summary>
        /// Checks, if a formal parameter type, which is not Serilizable works correctly,
        /// if an instance of a Serializable subclass is passed.
        /// </summary>
        [Test]
        public void TestNonSerilizableFormalParam() {
            TestNonSerializableBaseClass arg = new TestSerializableClassC();
            TestNonSerializableBaseClass result = m_testService.TestAbstractValueTypeEcho(arg);
            Assert.AreEqual(typeof(TestSerializableClassC), result.GetType());
        }

        [Test]
        public void TestBaseTypeNonSerializableParam() {
            TestSerializableClassC arg = new TestSerializableClassC();
            arg.Msg = "test";
            TestSerializableClassC result = m_testService.TestEchoSerializableC(arg);
            Assert.AreEqual(arg.Msg, result.Msg);
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
            Assert.AreEqual(newMsg, result.val1.Msg);
            Assert.AreEqual(result.val1, result.val2);
            Assert.AreEqual(result.val1.Msg, result.val2.Msg);
        }
        
        [Test]
        public void TestRecursiveValueType() {
            TestSerializableClassE arg = new TestSerializableClassE();
            arg.RecArrEntry = new TestSerializableClassE[1];
            arg.RecArrEntry[0] = arg;
            TestSerializableClassE result = m_testService.TestEchoSerializableE(arg);
            Assert.NotNull(result);
            Assert.NotNull(result.RecArrEntry);
            Assert.AreEqual(arg.RecArrEntry.Length, result.RecArrEntry.Length);
            Assert.IsTrue((result == result.RecArrEntry[0]), "invalid entry in recArrEntry");
            
        }

        /// <summary>
        /// Checks if a ByRef actual value for a formal parameter interface is passed correctly
        /// </summary>
        [Test]
        public void TestInterfacePassingByRef() {
            TestEchoInterface result = m_testService.RetrieveEchoInterfaceImplementor();
            // result is a proxy
            Assert.AreEqual(true, RemotingServices.IsTransparentProxy(result));
            System.Int32 arg = 23;
            System.Int32 echo = result.EchoInt(arg);
            Assert.AreEqual(arg, echo);
        }

        /// <summary>
        /// Checks if a ByVal actual value for a formal parameter interface is passed correctly
        /// </summary>
        [Test]
        public void TestInterfacePassingByVal() {
            System.String initialMsg = "initial";
            TestInterfaceA result = m_testService.RetrieveTestInterfaceAImplementor(initialMsg);
            Assert.AreEqual(initialMsg, result.Msg);

            System.String passedBack = m_testService.ExtractMsgFromInterfaceAImplmentor(result);
            Assert.AreEqual(initialMsg, passedBack);
        }

        [Test]
        public void TestInheritanceFromInterfaceForValueType() {
            System.String initialMsg = "initial";
            TestAbstrInterfaceImplByMarshalByVal impl = m_testService.RetriveTestInterfaceAImplemtorTheImpl(initialMsg);
            Assert.NotNull(impl as TestInterfaceA, "cast to Interface TestInterfaceA failed");
            Assert.AreEqual(initialMsg, impl.Msg);
        }

        [Test]
        public void TestWritableProperty() {
            System.Double arg = 1.2;
            m_testService.TestProperty = arg;
            System.Double result = m_testService.TestProperty;
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestReadOnlyProperty() {
            System.Double result = m_testService.TestReadOnlyPropertyReturningZero;
            Assert.AreEqual((System.Double) 0, result);
            PropertyInfo prop = typeof(TestService).GetProperty("TestReadOnlyPropertyReturningZero");
            Assert.NotNull(prop);
            Assert.AreEqual(false, prop.CanWrite);
            Assert.AreEqual(true, prop.CanRead);
        }

        [Test]
        public void TestPassingNullForFormalParamObjectAndAny() {
            object arg1 = null;
            object result1 = m_testService.EchoAnything(arg1);
            Assert.AreEqual(arg1, result1);
            
            Any any = new Any(null);
            Any result = m_testService.EchoAnythingContainer(any);
            Assert.AreEqual(any.Value, result.Value);
        }

        
        /// <summary>
        /// Test passing instances, if formal parameter is System.Object
        /// </summary>
        [Test]
        public void TestPassingForFormalParamObjectSimpleTypes() {
            System.Double arg1 = 1.23;
            System.Double result1 = (System.Double) m_testService.EchoAnything(arg1);
            Assert.AreEqual(arg1, result1);

            System.Char arg2 = 'a';
            System.Char result2 = (System.Char) m_testService.EchoAnything(arg2);
            Assert.AreEqual(arg2, result2);

            System.Boolean arg3 = true;
            System.Boolean result3 = (System.Boolean) m_testService.EchoAnything(arg3);
            Assert.AreEqual(arg3, result3);

            System.Int32 arg4 = 89;
            System.Int32 result4 = (System.Int32) m_testService.EchoAnything(arg4);
            Assert.AreEqual(arg4, result4);
        }
        
        [Test]
        public void TestCustomAnyTypeCode() {
            System.String testString = "abcd";
            OrbServices orb = OrbServices.GetSingleton();
            omg.org.CORBA.TypeCode wstringTc = orb.create_wstring_tc(0);
            Any any = new Any(testString, wstringTc);
            System.String echo = (System.String)m_testService.EchoAnything(any);
            Assert.AreEqual(testString, echo);
        }

        [Test]
        public void TestPassingForFormalParamObjectComplexTypes() {
            System.String arg1 = "test";
            System.String result1 = (System.String) m_testService.EchoAnything(arg1);
            Assert.AreEqual(arg1, result1);
            
            TestSerializableClassB1 arg2 = new TestSerializableClassB1();
            arg2.Msg = "msg";
            TestSerializableClassB1 result2 = (TestSerializableClassB1) m_testService.EchoAnything(arg2);
            Assert.AreEqual(arg2.Msg, result2.Msg);
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
            Assert.AreEqual(arg3[0], result3[0]);

            System.Int32[] arg4 = new System.Int32[1];
            arg4[0] = 1;
            System.Int32[] result4 = (System.Int32[]) m_testService.EchoAnything(arg4);
            Assert.AreEqual(arg4[0], result4[0]);
        }

        [Test]
        public void TestAnyContainer() {
            System.String testString = "abcd";
            OrbServices orb = OrbServices.GetSingleton();
            omg.org.CORBA.TypeCode wstringTc = orb.create_wstring_tc(0);
            Any any = new Any(testString, wstringTc);
            Any result = m_testService.EchoAnythingContainer(any);
            Assert.AreEqual(any.Value, result.Value);
        }

        // this test is a replacement for the next one, until behaviour is decided
        [Test]
        public void TestCallEqualityServerAndProxy() {
            m_testService.CheckEqualityWithServiceV2((TestService)m_testService);
            m_testService.CheckEqualityWithService((MarshalByRefObject)m_testService);
        }
                        
        [Ignore("Not yet decided, what behaviour should be supported by IIOP.NET")]
        [Test]
        public void TestEqualityServerAndProxy() {
            bool result = m_testService.CheckEqualityWithServiceV2((TestService)m_testService);
            Assert.AreEqual(true, result);
            result = m_testService.CheckEqualityWithService((MarshalByRefObject)m_testService);
            Assert.AreEqual(true, result);
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
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestRefArgs() {
            System.Int32 argInit = 1;
            System.Int32 arg = argInit;
            System.Int32 result = m_testService.TestRef(ref arg);
            Assert.AreEqual(arg, result);
            Assert.AreEqual(argInit + 1, arg);
        }

        delegate System.Int32 TestOutArgsDelegate(System.Int32 arg, out System.Int32 argOut);

        [Test]
        public void TestOutArgsMixed() {
            System.Int32 argOut;
            System.Int32 arg = 1;
            System.Int32 result = m_testService.TestOut(arg, out argOut);
            Assert.AreEqual(arg, argOut);
            Assert.AreEqual(arg, result);

            System.Int32 argOut2;
            TestOutArgsDelegate oad = new TestOutArgsDelegate(m_testService.TestOut);
            // async call
            IAsyncResult ar = oad.BeginInvoke(arg, out argOut2, null, null);
            // wait for response
            System.Int32 result2 = oad.EndInvoke(out argOut2, ar);
            Assert.AreEqual(arg, argOut2);
            Assert.AreEqual(arg, result2);
        }
        
        [Test]
        public void TestOutArgAlone() {
            System.Int32 result;
            m_testService.Assign5ToOut(out result);
            Assert.AreEqual(5, result);
        }
        
        [Test]
        public void TestOverloadedMethods() {
            System.Int32 arg1int = 1;
            System.Int32 arg2int = 2;
            System.Int32 arg3int = 2;

            System.Double arg1double = 1.0;
            System.Double arg2double = 2.0;

            System.Int32 result1 = m_testService.AddOverloaded(arg1int, arg2int);
            Assert.AreEqual((System.Int32)(arg1int + arg2int), result1);
            System.Int32 result2 = m_testService.AddOverloaded(arg1int, arg2int, arg3int);
            Assert.AreEqual((System.Int32)(arg1int + arg2int + arg3int), result2);
            System.Double result3 = m_testService.AddOverloaded(arg1double, arg2double);
            Assert.AreEqual((System.Double)(arg1double + arg2double), result3);
        }

        [Test]
        public void TestNameClashes() {
            System.Int32 arg = 89;
            System.Int32 result = m_testService.custom(arg);
            Assert.AreEqual(arg, result);
           
            m_testService.context = arg;
            Assert.AreEqual(arg, m_testService.context);
        }

        [Test]
        public void TestNamesStartingWithUnderScore() {
            System.Int32 arg = 99;
            System.Int32 result = m_testService._echoInt(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestCheckParamAttrs() {
            System.String arg = "testArg";
            System.String result = m_testService.CheckParamAttrs(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestSimpleUnionNoExceptions() {
            TestUnion arg = new TestUnion();
            short case0Val = 11;
            arg.Setval0(case0Val);
            TestUnion result = m_testService.EchoUnion(arg);
            Assert.AreEqual(case0Val, result.Getval0());
            Assert.AreEqual(0, result.Discriminator);

            TestUnion arg2 = new TestUnion();
            int case1Val = 12;
            arg2.Setval1(case1Val, 2);
            TestUnion result2 = m_testService.EchoUnion(arg2);
            Assert.AreEqual(case1Val, result2.Getval1());
            Assert.AreEqual(2, result2.Discriminator);

            TestUnion arg3 = new TestUnion();
            bool case2Val = true;
            arg3.Setval2(case2Val, 7);
            TestUnion result3 = m_testService.EchoUnion(arg3);
            Assert.AreEqual(case2Val, result3.Getval2());
            Assert.AreEqual(7, result3.Discriminator);

            TestUnionULong arg4 = new TestUnionULong();
            int case1Val2 = 13;
            arg4.Setval1(case1Val2);
            TestUnionULong result4 = m_testService.EchoUnionULong(arg4);
            Assert.AreEqual(case1Val2, result4.Getval1());
            uint case1DiscrVal = 0x80000000;
            Assert.AreEqual((int)case1DiscrVal, result4.Discriminator);

        }

        [Test]
        public void TestEnumBasedUnionNoExceptions() {
            TestUnionE arg = new TestUnionE();
            short case0Val = 11;
            arg.SetvalE0(case0Val);
            TestUnionE result = m_testService.EchoUnionE(arg);
            Assert.AreEqual(case0Val, result.GetvalE0());
            Assert.AreEqual(TestEnumForU.A, result.Discriminator);

        TestUnionE arg2 = new TestUnionE();
            TestEnumForU case1Val = TestEnumForU.A;
            arg2.SetvalE1(case1Val, TestEnumForU.B);
            TestUnionE result2 = m_testService.EchoUnionE(arg2);
            Assert.AreEqual(case1Val, result2.GetvalE1());
            Assert.AreEqual(TestEnumForU.B, result2.Discriminator);
        }

        [Test]
        public void TestUnionExceptions() {
            try {
                TestUnion arg = new TestUnion();
                arg.Getval0();
                Assert.Fail("exception not thrown for getting value from non-initalized union");
            } catch (omg.org.CORBA.BAD_OPERATION) {
            }
            try {
                TestUnion arg = new TestUnion();
                arg.Setval0(11);
                arg.Getval1();
                Assert.Fail("exception not thrown for getting value from non-initalized union");
            } catch (omg.org.CORBA.BAD_OPERATION) {
            }
            try {
                TestUnion arg1 = new TestUnion();
                arg1.Setval1(11, 7);
                Assert.Fail("exception not thrown on wrong discriminator value.");
            } catch (omg.org.CORBA.BAD_PARAM) {
            }
            try {
                TestUnion arg2 = new TestUnion();
                arg2.Setval2(false, 0);
                Assert.Fail("exception not thrown on wrong discriminator value.");
            } catch (omg.org.CORBA.BAD_PARAM) {
            }
        }

        [Test]
        public void TestConstantRegression() {
            Int32 constVal = MyConstant.ConstVal;
            Assert.AreEqual(11, constVal, "wrong constant value");
            
            Int64 maxIntVal = Max_int.ConstVal;
            Assert.AreEqual(Int64.MaxValue, maxIntVal, "wrong constant value");
            
            // regression test for BUG #909562
            Int64 minIntVal = Min_int.ConstVal;
            Assert.AreEqual(Int64.MinValue, minIntVal, "wrong constant value");
                                              
            Int64 zeroIntVal = Zero_val.ConstVal;
            Assert.AreEqual(0, zeroIntVal, "wrong constant value");

            Int64 zeroFromHex = Zero_from_hex.ConstVal;
            Assert.AreEqual(0, zeroFromHex, "wrong constant value");
            
            Int64 oneFromHex = One_from_hex.ConstVal;
            Assert.AreEqual(1, oneFromHex, "wrong constant value");
            
            Int64 minusOneFromHex = Minus_one_from_hex.ConstVal;
            Assert.AreEqual(-1, minusOneFromHex, "wrong constant value");
    
            Single zeroValFloat = Zero_val_float.ConstVal;
            Assert.AreEqual(0.0, zeroValFloat, "wrong constant value");
    
            Single minusOneFloat = Minus_one_float.ConstVal;
            Assert.AreEqual(-1.0, minusOneFloat, "wrong constant value");
            
            Single plusOneFloat = Plus_one_float.ConstVal;
            Assert.AreEqual(1.0, plusOneFloat, "wrong constant value");
            
            Single plus_inf = Plus_Inf.ConstVal;
            Assert.AreEqual(Single.PositiveInfinity, plus_inf, "wrong constant value");
            Single minus_inf = Minus_Inf.ConstVal;
            Assert.AreEqual(Single.NegativeInfinity, minus_inf, "wrong constant value");

            UInt16 expectedValUShort = 0x8000;
            Assert.AreEqual((Int16)expectedValUShort, UShort_BiggerThanShort.ConstVal, "wrong constant value");
            UInt32 expectedValULong = 0x80000000;
            Assert.AreEqual((Int32)expectedValULong, ULong_BiggerThanLong.ConstVal, "wrong constant value");
            UInt64 expectedValULongLong = 0x8000000000000000;
            Assert.AreEqual((Int64)expectedValULongLong, ULongLong_BiggerThanLongLong.ConstVal, "wrong constant value");
            
        }

        [Test]
        public void TestConstantAllKinds() {
            Assert.AreEqual(-29, A_SHORT_CONST.ConstVal, "wrong const val short");
            Assert.AreEqual(A_SHORT_CONST.ConstVal, VAL_OF_A_SHORT_CONST.ConstVal, "wrong const val short other const");
            Assert.AreEqual(30, A_LONG_CONST.ConstVal, "wrong const val long");
            Assert.AreEqual(-31, A_LONG_LONG_CONST.ConstVal, "wrong const val long long");

            Assert.AreEqual(81, A_UNSIGNED_SHORT_CONST.ConstVal, "wrong const val ushort");
            Assert.AreEqual(101, A_UNSIGNED_LONG_CONST.ConstVal, "wrong const val ulong");
            Assert.AreEqual(102, A_UNSIGNED_LONG_LONG_CONST.ConstVal, "wrong const val ulong long");

            Assert.AreEqual('C', A_CHAR_CONST.ConstVal, "wrong const val char");
            Assert.AreEqual(A_CHAR_CONST.ConstVal, VAL_OF_A_CHAR_CONST.ConstVal, "wrong const val char other const");

            Assert.AreEqual('D', A_WCHAR_CONST.ConstVal, "wrong const val wchar");

            Assert.AreEqual(true, A_BOOLEAN_CONST_TRUE.ConstVal, "wrong const val boolean true");
            Assert.AreEqual(false, A_BOOLEAN_CONST_FALSE.ConstVal, "wrong const val boolean false");

            Assert.AreEqual((Single)1.1, A_FLOAT_CONST.ConstVal, "wrong const val float");
            Assert.AreEqual((Double)6.7E8, A_DOUBLE_CONST.ConstVal, "wrong const val double");

            Assert.AreEqual("test", A_STRING_CONST.ConstVal, "wrong const val string");
            Assert.AreEqual("test-b", A_STRING_CONST_BOUNDED.ConstVal, "wrong const val string bounded");

            Assert.AreEqual("w-test", A_WSTRING_CONST.ConstVal, "wrong const val wstring");
            Assert.AreEqual("w-test-b", A_WSTRING_CONST_BOUNDED.ConstVal, "wrong const val wstring bounded");

            Assert.AreEqual(10, SCOPED_NAME_CONST_LONGTD.ConstVal, "wrong const val typedef long");

            Assert.AreEqual(A_ENUM_FOR_CONST.CV1, SCOPED_NAME_CONST_ENUM.ConstVal, "wrong const val enum");

            Assert.AreEqual(8, A_OCTET_CONST.ConstVal, "wrong const val octet");
        }

        [Test]
        public void TestConstValueAndSwitch() {
            // check, if switch is possbile with constant values
            int testValue = A_LONG_CONST.ConstVal;
            switch(testValue) {
                case A_LONG_CONST.ConstVal:
                    // ok
                    break;
                default:
                    Assert.Fail("wrong value: " + testValue + "; should be: " + A_LONG_CONST.ConstVal);
                    break;
            }
        }

        [Test]
        public void TestPassingUnionsAsAny() {
            TestUnion arg = new TestUnion();
            short case0Val = 11;
            arg.Setval0(case0Val);
            TestUnion result = (TestUnion)m_testService.EchoAnything(arg);
            Assert.AreEqual(case0Val, result.Getval0());
            Assert.AreEqual(0, result.Discriminator);

        TestUnionE arg2 = new TestUnionE();
            TestEnumForU case1Val = TestEnumForU.A;
            arg2.SetvalE1(case1Val, TestEnumForU.B);
            TestUnionE result2 = (TestUnionE)m_testService.EchoAnything(arg2);
            Assert.AreEqual(case1Val, result2.GetvalE1());
            Assert.AreEqual(TestEnumForU.B, result2.Discriminator);
        }

        [Test]
        public void TestReceivingUnknownUnionsAsAny() {
            object result = m_testService.RetrieveUnknownUnionAsAny();
            Assert.NotNull(result, "union not retrieved");
            Assert.AreEqual("Ch.Elca.Iiop.IntegrationTests.TestUnionE2", result.GetType().FullName, "type name");
        }


        [Test]
        public void TestReferenceOtherConstant() {
            Int32 constValA = AVAL.ConstVal;
            Assert.AreEqual(1, constValA, "wrong constant value");
            Int32 constValB = BVAL.ConstVal;
            Assert.AreEqual(1, constValB, "wrong constant value");
        }


        [Test]
        public void TestCharacterConstant() {
            Char constValNonEscapeCharConst = NonEscapeCharConst.ConstVal;
            Assert.AreEqual('a', constValNonEscapeCharConst, "wrong constant value");

            Char constValUnicodeEscapeCharConst1 = UnicodeEscapeCharConst1.ConstVal;
            Assert.AreEqual('\u0062', constValUnicodeEscapeCharConst1, "wrong constant value");

            Char constValUnicodeEscapeCharConst2 = UnicodeEscapeCharConst2.ConstVal;
            Assert.AreEqual('\uFFFF', constValUnicodeEscapeCharConst2, "wrong constant value");

            Char constValHexEscapeCharConst = HexEscapeCharConst.ConstVal;
            Assert.AreEqual('\u0062', constValHexEscapeCharConst, "wrong constant value");

            Char constValDecEscapeCharConst1 = DecEscapeCharConst1.ConstVal;
            Assert.AreEqual('a', constValDecEscapeCharConst1, "wrong constant value");

            Char constValDecEscapeCharConst2 = DecEscapeCharConst2.ConstVal;
            Assert.AreEqual('\u0000', constValDecEscapeCharConst2, "wrong constant value");
        }

        [Test]
        public void TestCharacterConstantBugReport841774() {
            Char constValStandAlone = STAND_ALONE_TEST.ConstVal;
            Assert.AreEqual('1', constValStandAlone, "wrong constant value");
            Char constValNetWork = NETWORK_TEST.ConstVal;
            Assert.AreEqual('2', constValNetWork, "wrong constant value");
            Char constValProduction = PRODUCTION.ConstVal;
            Assert.AreEqual('3', constValProduction, "wrong constant value");
        }

        [Test]
        public void TestWstringLiteralBugReport906401() {
            String val_a = COMP_NAME_A.ConstVal;
            Assert.AreEqual("test", val_a, "wrong constant value");
            String val_b = COMP_NAME_B.ConstVal;
            Assert.AreEqual("java:comp/env/ejb/Fibo", val_b, "wrong constant value");
        }

        /// <summary>checks, if channel uses is_a to check interface compatiblity on IOR deser,
        /// if other checks don't work</summary>
        [Test]
        public void TestInterfaceCompMbrDeser() {
            TestSimpleInterface1 proxy1 = (TestSimpleInterface1)m_testService.GetSimpleService1();
            Assert.NotNull(proxy1, "testSimpleService1 ref not received");
            Assert.AreEqual(true, proxy1.ReturnTrue());

            TestSimpleInterface2 proxy2 = (TestSimpleInterface2)m_testService.GetSimpleService2();
            Assert.NotNull(proxy2, "testSimpleService2 ref not received");
            Assert.AreEqual(false, proxy2.ReturnFalse());

            TestSimpleInterface1 proxy3 = (TestSimpleInterface1)m_testService.GetWhenSuppIfMissing();
            Assert.NotNull(proxy3, "testSimpleService1 ref not received");
            Assert.AreEqual(true, proxy3.ReturnTrue());

        }

        [Test]
        public void TestIsACall() {
            omg.org.CORBA.IObject proxy1 = (omg.org.CORBA.IObject)m_testService.GetSimpleService1();
            Assert.NotNull(proxy1, "testSimpleService1 ref not received");
            Assert.IsTrue(proxy1._is_a("IDL:Ch/Elca/Iiop/IntegrationTests/TestSimpleInterface1:1.0"));
            
            omg.org.CORBA.IObject proxy2 = (omg.org.CORBA.IObject)m_testService.GetSimpleService2();
            Assert.NotNull(proxy2, "testSimpleService2 ref not received");
            Assert.IsTrue(proxy2._is_a("IDL:Ch/Elca/Iiop/IntegrationTests/TestSimpleInterface2:1.0"));
            
            
            // test using ORBServices
            omg.org.CORBA.OrbServices orb = omg.org.CORBA.OrbServices.GetSingleton();
            Assert.IsTrue(orb.is_a(proxy1, typeof(TestSimpleInterface1)));
            Assert.IsTrue(orb.is_a(proxy2, typeof(TestSimpleInterface2)));
            // target object implements both interfaces
            Assert.IsTrue(orb.is_a(proxy1, typeof(TestSimpleInterface2)));
            Assert.IsTrue(orb.is_a(proxy2, typeof(TestSimpleInterface1)));
            
            Assert.IsFalse(orb.is_a(m_testService, typeof(TestSimpleInterface1)));
            Assert.IsFalse(orb.is_a(m_testService, typeof(TestSimpleInterface2)));
        }
        
        [Test]
        public void TestNonExistentCall() {
            // test using ORBServices
            omg.org.CORBA.OrbServices orb = omg.org.CORBA.OrbServices.GetSingleton();

            Assert.AreEqual(false, orb.non_existent(m_testService));
            object nonExObject = orb.string_to_object("iiop://localhost:8087/someNonExistingObject");
            Assert.AreEqual(true, orb.non_existent(nonExObject));
        }
        
        [Test]
        public void TestUserIdForMbr() {
            string id = "myAdderId";
            Adder adder = m_testService.CreateNewWithUserID(id);
            string marshalUrl = RemotingServices.GetObjectUri(adder);
            Ior adderIor = new Ior(marshalUrl);
            IInternetIiopProfile prof = adderIor.FindInternetIiopProfile();
            byte[] objectKey = prof.ObjectKey;
            ASCIIEncoding enc = new ASCIIEncoding();
            string marshalUri = new String(enc.GetChars(objectKey));
            Assert.AreEqual(id, marshalUri, "wrong user id");
            
            // check if callable
            int arg1 = 1;
            int arg2 = 2;
            Assert.AreEqual(arg1 + arg2, adder.Add(arg1, arg2), "wrong adder result");
        }
        
        [Test]
        public void TestSystemIdForMbr() {
            Adder adder = m_testService.CreateNewWithSystemID();
            string marshalUrl = RemotingServices.GetObjectUri(adder);
            Ior adderIor = new Ior(marshalUrl);
            IInternetIiopProfile prof = adderIor.FindInternetIiopProfile();
            byte[] objectKey = prof.ObjectKey;
            ASCIIEncoding enc = new ASCIIEncoding();
            string marshalUri = new String(enc.GetChars(objectKey));
            if (marshalUri.StartsWith("/")) {
                marshalUri = marshalUri.Substring(1);
            }
            Assert.IsTrue(marshalUri.IndexOf("/") > 0, "no appdomain-guid");
            string guid_string = marshalUri.Substring(0, marshalUri.IndexOf("/"));
            guid_string = guid_string.Replace("_", "-");
            try {
                Guid guid = new Guid(guid_string);
            } catch (Exception ex) {
                Assert.Fail("guid not in uri: " + ex);
            }
            
            // check if callable
            int arg1 = 1;
            int arg2 = 2;
            Assert.AreEqual(arg1 + arg2, adder.Add(arg1, arg2), "wrong adder result");
        }

        [Test]
        public void TestObjectToString() {
            OrbServices orbServices = OrbServices.GetSingleton();
            string id = "myAdderId2";
            Adder adder = m_testService.CreateNewWithUserID(id);
            string iorString = orbServices.object_to_string(adder);
            Ior adderIor = new Ior(iorString);
            IInternetIiopProfile prof = adderIor.FindInternetIiopProfile();
            Assert.AreEqual(8087, prof.Port);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(2, prof.Version.Minor);
            
            byte[] oid = { 0x6d, 0x79, 0x41, 0x64, 0x64, 0x65, 0x72, 0x49, 0x64, 0x32 };
            CheckIorKey(oid, prof.ObjectKey);

            string testServiceIorString = m_testService.GetIorStringForThisObject();
            Ior testServiceIor = new Ior(testServiceIorString);
            IInternetIiopProfile profSvcIor = testServiceIor.FindInternetIiopProfile();
            Assert.AreEqual(8087, profSvcIor.Port);
            Assert.AreEqual(1, profSvcIor.Version.Major);
            Assert.AreEqual(2, profSvcIor.Version.Minor);
            
            byte[] oidTestService = { 0x74, 0x65, 0x73, 0x74 };
            CheckIorKey(oidTestService, profSvcIor.ObjectKey);


        }

        private void CheckIorKey(byte[] expected, byte[] actual) {
            Assert.AreEqual(expected.Length, actual.Length, "wrong id length");
            for (int i = 0; i <expected.Length; i++) {
                Assert.AreEqual(expected[i], actual[i], "wrong element nr " + i);
            }
        }
        
        [Test]
        public void TestIdsIncludingNonAscii() {
            string id = "myAdderId" + '\u0765' + "1" + @"\uA";
            string expectedMarshalledId = @"myAdderId\u07651\\uA";
            Adder adder = m_testService.CreateNewWithUserID(id);
            string marshalUrl = RemotingServices.GetObjectUri(adder);
            Ior adderIor = new Ior(marshalUrl);
            IInternetIiopProfile prof = adderIor.FindInternetIiopProfile();
            byte[] objectKey = prof.ObjectKey;
            ASCIIEncoding enc = new ASCIIEncoding();
            string marshalUri = new String(enc.GetChars(objectKey));
            Assert.AreEqual(expectedMarshalledId, marshalUri, "wrong user id");
            
            // check if callable
            int arg1 = 1;
            int arg2 = 2;
            Assert.AreEqual(arg1 + arg2, adder.Add(arg1, arg2), "wrong adder result");
        }

        [Test]
        public void TestErrorReportBadOperation() {
            m_testService.GetAllUsagerType();
        }

        [Test]
        public void TestSystemType() {
            Type arg1 = typeof(System.Int32);
            Type result1 = m_testService.EchoType(arg1);
            Assert.AreEqual(arg1, result1, "wrong type for int32 echo");

            Type arg2 = typeof(System.Boolean);
            Type result2 = m_testService.EchoType(arg2);
            Assert.AreEqual(arg2, result2, "wrong type for Boolean echo");

            Type arg3 = typeof(TestService);
            Type result3 = m_testService.EchoType(arg3);
            Assert.AreEqual(arg3, result3, "wrong type for testService type echo");

            Type arg4 = null;
            Type result4 = m_testService.EchoType(arg4);
            Assert.AreEqual(arg4, result4, "wrong type for null type echo");
        }

        [Test]
        public void TestUnionNetSerializableOptimized() {
            // check, that the generated union is serializable also with other formatters optimized (not all fields, but only needed ones)
            TestUnion arg = new TestUnion();
            int case1Val = 12;
            arg.Setval1(case1Val, 2);

            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.MemoryStream serialised = new System.IO.MemoryStream();
            try {
                formatter.Serialize(serialised, arg);

                serialised.Seek(0, System.IO.SeekOrigin.Begin);
                formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                TestUnion deser = (TestUnion)formatter.Deserialize(serialised);

                Assert.AreEqual(2, deser.Discriminator);
                Assert.AreEqual(case1Val, deser.Getval1());
            } finally {
                serialised.Close();
            }
        }

    }

}
