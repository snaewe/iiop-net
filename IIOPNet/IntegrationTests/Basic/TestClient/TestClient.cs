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
using System.Runtime.Remoting.Messaging;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.IntegrationTests {


    [TestFixture]
    public class TestClient {

        #region IFields

        private IiopClientChannel m_channel;

        private TestService m_testService;
        private TestExceptionService m_testExService;
        private ISimpleTestInterface m_svcSingleCall;
        private ISimpleTestInterface m_svcSingletonCall;
        private ISimpleTestInterface m_contextBound;
        private TestIdlTypesService  m_testIdlTypesService;
        private TestOneWayService m_testOneWayService;

        #endregion IFields
        #region IMethods

        private NamingContext GetNameService() {
            // access COS nameing service
            CorbaInit init = CorbaInit.GetInit();
            NamingContext nameService = init.GetNameService("localhost", 8087);
            return nameService;
        }

        private static T Connect<T>(string uri) {
            return (T)RemotingServices.Connect(typeof(T), uri);
        }

        [TestFixtureSetUp]
        public void SetupEnvironment() {
            // register the channel
            IDictionary properties = new Hashtable();
            properties[IiopClientChannel.ALLOW_REQUEST_MULTIPLEX_KEY] = false;
            m_channel = new IiopClientChannel(properties);
            ChannelServices.RegisterChannel(m_channel, false);

            // get the reference to the test-service
            m_testService = Connect<TestService>("corbaloc:iiop:1.2@localhost:8087/test");
            m_testExService = Connect<TestExceptionService>("corbaloc:iiop:1.2@localhost:8087/testExService");

            m_svcSingleCall = Connect<ISimpleTestInterface>("corbaloc:iiop:1.2@localhost:8087/testSingleCall");
            m_svcSingletonCall = Connect<ISimpleTestInterface>("corbaloc:iiop:1.2@localhost:8087/testSingletonCall");
            m_contextBound = Connect<ISimpleTestInterface>("corbaloc:iiop:1.2@localhost:8087/testContextBound");
            m_testIdlTypesService = Connect<TestIdlTypesService>("corbaloc:iiop:1.2@localhost:8087/testIdlTypesService");
            m_testOneWayService = Connect<TestOneWayService>("corbaloc:iiop:1.2@localhost:8087/testOneWayService");
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
        public void TestSByte() {
            System.SByte arg = -2;
            System.SByte result = (System.SByte)m_testService.TestIncSByte((byte)arg);
            Assert.AreEqual((System.SByte)(arg + 1), result);
            arg = 2;
            result = (System.SByte)m_testService.TestIncSByte((byte)arg);
            Assert.AreEqual((System.SByte)(arg + 1), result);
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
        public void TestUInt16() {
            ushort arg = 1;
            ushort result = (ushort)m_testService.TestIncUInt16((short)arg);
            Assert.AreEqual(((ushort)(arg + 1)), result);
            arg = System.UInt16.MaxValue - 1;
            result = (ushort)m_testService.TestIncUInt16((short)arg);
            Assert.AreEqual(((ushort)(arg + 1)), result);
        }

        [Test]
        public void TestUInt32() {
            uint arg = 1;
            uint result = (uint)m_testService.TestIncUInt32((int)arg);
            Assert.AreEqual(((uint)(arg + 1)), result);
            arg = System.UInt32.MaxValue - 1;
            result = (uint)m_testService.TestIncUInt32((int)arg);
            Assert.AreEqual(((uint)(arg + 1)), result);
        }

        [Test]
        public void TestUInt64() {
            ulong arg = 1;
            ulong result = (ulong)m_testService.TestIncUInt64((long)arg);
            Assert.AreEqual((ulong)(arg + 1), result);
            arg = System.UInt64.MaxValue - 1;
            result = (ulong)m_testService.TestIncUInt64((long)arg);
            Assert.AreEqual((ulong)(arg + 1), result);
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
            TestEnum arg = TestEnum.TestEnum_A;
            TestEnum result = m_testService.TestEchoEnumVal(arg);
            Assert.AreEqual(arg, result);
            arg = TestEnum.TestEnum_D;
            result = m_testService.TestEchoEnumVal(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestFlags() {
            int arg = 1;
            int result = m_testService.TestEchoFlagsVal(arg);
            Assert.AreEqual(arg, result);
            arg = 3;
            result = m_testService.TestEchoFlagsVal(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumBI16Val() {
            TestEnumBI16 arg = TestEnumBI16.TestEnumBI16_B1;
            TestEnumBI16 result = m_testService.TestEchoEnumI16Val(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumBUI32Val() {
            TestEnumUI32 arg = TestEnumUI32.TestEnumUI32_C2;
            TestEnumUI32 result = m_testService.TestEchoEnumUI32Val(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumBI64Val() {
            TestEnumBI64 arg = TestEnumBI64.TestEnumBI64_AL;
            TestEnumBI64 result = m_testService.TestEchoEnumI64Val(arg);
            Assert.AreEqual(arg, result);

            arg = TestEnumBI64.TestEnumBI64_BL;
            result = m_testService.TestEchoEnumI64Val(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestEnumAsAny() {
            TestEnum arg = TestEnum.TestEnum_A;
            TestEnum result = (TestEnum)m_testService.EchoAnything(arg);
            Assert.AreEqual(arg, result);
            arg = TestEnum.TestEnum_D;
            result = (TestEnum)m_testService.EchoAnything(arg);
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
            Assert.IsNull(result4[0]);
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
            System.Int32[][] arg1 = new System.Int32[2][];
            arg1[0] = new System.Int32[] { 1 };
            arg1[1] = new System.Int32[] { 2 };
            System.Int32[][] result1 = m_testService.EchoMultiDimIntArray(arg1);
            Assert.AreEqual(2, result1.Length);
            Assert.NotNull(result1[0]);
            Assert.NotNull(result1[1]);
            Assert.AreEqual(arg1[0][0], result1[0][0]);
            Assert.AreEqual(arg1[1][0], result1[1][0]);

            System.Byte[][][] arg2 = new System.Byte[3][][];
            arg2[0] = new System.Byte[][] { new System.Byte[] { 1 } };
            arg2[1] = new System.Byte[][] { new System.Byte[] { 2 } };
            arg2[2] = new System.Byte[][] { new System.Byte[] { 3 } };
            System.Byte[][][] result2 = m_testService.EchoMultiDimByteArray(arg2);
            Assert.AreEqual(3, result2.Length);
            Assert.NotNull(result2[0]);
            Assert.NotNull(result2[1]);
            Assert.NotNull(result2[2]);
            Assert.AreEqual(arg2[0][0][0], result2[0][0][0]);

        }

        [Test]
        public void TestMutlidimStringArrays() {
            System.String[][] arg1 = new System.String[2][];
            arg1[0] = new System.String[] { "test" };
            arg1[1] = new System.String[] { "test2" };
            System.String[][] result1 = m_testService.EchoMultiDimStringArray(arg1);
            Assert.AreEqual(2, result1.Length);
            Assert.NotNull(result1[0]);
            Assert.NotNull(result1[1]);
            Assert.AreEqual(arg1[0][0], result1[0][0]);
            Assert.AreEqual(arg1[1][0], result1[1][0]);
        }


        [Test]
        public void TestEchoIdlLongSequence() {
            int[] arg = new int[] { 1, 2, 3};
            int[] result = m_testService.EchoIdlLongSequence(arg);
            Assert.NotNull(result);
            Assert.AreEqual(arg.Length, result.Length);
            Assert.AreEqual(arg[0], result[0]);
            Assert.AreEqual(arg[1], result[1]);
            Assert.AreEqual(arg[2], result[2]);
        }

        [Test]
        public void TestEchoIdlLongSequenceOfSequence() {
            int[][] arg = new int[2][];
            arg[0] = new int[] { 1, 2};
            arg[1] = new int[] { 4};

            int[][] result = m_testService.EchoIdlLongSequenceOfSequence(arg);
            Assert.NotNull(result);
            Assert.AreEqual(arg.Length, result.Length);
            Assert.AreEqual(arg[0].Length, result[0].Length);
            Assert.AreEqual(arg[0][0], result[0][0]);
            Assert.AreEqual(arg[0][1], result[0][1]);
            Assert.AreEqual(arg[1].Length, result[1].Length);
            Assert.AreEqual(arg[1][0], result[1][0]);

            // check with bounded
            int[][] arg2 = new int[2][];
            arg2[0] = new int[] { 1, 2, 3};
            arg2[1] = new int[] { 4};

            int[][] result2 = m_testService.EchoIdlLongSequenceOfBoundedSequence(arg2);
            Assert.NotNull(result);
            Assert.AreEqual(arg2.Length, result2.Length);
            Assert.AreEqual(arg2[0].Length, result2[0].Length);
            Assert.AreEqual(arg2[0][0], result2[0][0]);
            Assert.AreEqual(arg2[0][1], result2[0][1]);
            Assert.AreEqual(arg2[0][2], result2[0][2]);
            Assert.AreEqual(arg2[1].Length, result2[1].Length);
            Assert.AreEqual(arg2[1][0], result2[1][0]);

            // over the bound
            int[][] arg3 = new int[2][];
            arg3[0] = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            arg3[1] = new int[] { 4};
            try {
                int[][] result3 = m_testService.EchoIdlLongSequenceOfBoundedSequence(arg3);
                Assert.Fail("possible to pass a too long idl sequence");
            } catch (omg.org.CORBA.BAD_PARAM) {
                // ok
            }
        }

        [Test]
        public void TestIdlLongSequenceAppend() {
            int[] argSeq = new int[] { 1, 2, 3};
            int argElem = 4;
            int[] result = m_testService.AppendToIdlLongSequence(argSeq, argElem);
            Assert.NotNull(result);
            Assert.AreEqual(argSeq.Length + 1, result.Length);
            Assert.AreEqual(argSeq[0], result[0]);
            Assert.AreEqual(argSeq[1], result[1]);
            Assert.AreEqual(argSeq[2], result[2]);
            Assert.AreEqual(argElem, result[3]);
        }

        [Test]
        public void TestEchoIdlStringSequence() {
            string[] arg = new string[] { "1", "2", "3"};
            string[] result = m_testService.EchoIdlStringSequence(arg);
            Assert.NotNull(result);
            Assert.AreEqual(arg.Length, result.Length);
            Assert.AreEqual(arg[0], result[0]);
            Assert.AreEqual(arg[1], result[1]);
            Assert.AreEqual(arg[2], result[2]);

            string[] arg2 = new string[] { "1", "2", "3", "4"};
            string[] result2 = m_testService.EchoIdlWStringSequence(arg2);
            Assert.NotNull(result2);
            Assert.AreEqual(arg2.Length, result2.Length);
            Assert.AreEqual(arg2[0], result2[0]);
            Assert.AreEqual(arg2[1], result2[1]);
            Assert.AreEqual(arg2[2], result2[2]);
        }

        [Test]
        public void TestIdlStringSequenceAppend() {
            string[] argSeq = new string[] { "1", "2", "3"};
            string argElem = "4";
            string[] result = m_testService.AppendToIdlStringSequence(argSeq, argElem);
            Assert.NotNull(result);
            Assert.AreEqual(argSeq.Length + 1, result.Length);
            Assert.AreEqual(argSeq[0], result[0]);
            Assert.AreEqual(argSeq[1], result[1]);
            Assert.AreEqual(argSeq[2], result[2]);
            Assert.AreEqual(argElem, result[3]);
        }

        [Test]
        public void TestIdlIntOneDimArray() {
            int[] testArray = new int[] { 1, 2, 3, 4, 5 };
            int[] result = m_testService.EchoIdlLongArrayFixedSize5(testArray);
            Assert.AreEqual(testArray.Length, result.Length);
            for (int i = 0; i < testArray.Length; i++) {
                Assert.AreEqual(testArray[i], result[i]);
            }
        }

        [Test]
        public void TestIdlIntTwoDimArray() {
            int[,] testArray = new int[,] { {1, 2, 3}, {4, 5, 6}, {7, 8, 9}, {10, 11, 12}, {13, 14, 15} };
            int[,] result = m_testService.EchoIdlLongArray5times3(testArray);
            Assert.AreEqual(testArray.Rank, result.Rank);
            Assert.AreEqual(testArray.GetLength(0), result.GetLength(0));
            Assert.AreEqual(testArray.GetLength(1), result.GetLength(1));
            for (int i = 0; i < testArray.GetLength(0); i++) {
                for (int j = 0; j < testArray.GetLength(1); j++) {
                    Assert.AreEqual(testArray[i,j], result[i,j]);
                }
            }
        }

        [Test]
        public void TestIdlArrayInsideStruct() {
            IdlArrayContainer container = new IdlArrayContainer();
            container.OneDimIntArray5 = new int[] { 1, 2, 3, 4, 5 };
            container.TwoDimIntArray2x2 = new int[,] { { 1, 2 }, { 3, 4 } };
            IdlArrayContainer result = m_testService.EchoIdlArrayContainer(container);
            Assert.AreEqual(container.OneDimIntArray5.Length, result.OneDimIntArray5.Length);
            Assert.AreEqual(container.TwoDimIntArray2x2.GetLength(0), result.TwoDimIntArray2x2.GetLength(0));
            Assert.AreEqual(container.TwoDimIntArray2x2.GetLength(1), result.TwoDimIntArray2x2.GetLength(1));

            for (int i = 0; i < container.OneDimIntArray5.Length; i++) {
                Assert.AreEqual(container.OneDimIntArray5[i], result.OneDimIntArray5[i]);
            }

            for (int i = 0; i < container.TwoDimIntArray2x2.GetLength(0); i++) {
                for (int j = 0; j < container.TwoDimIntArray2x2.GetLength(1); j++) {
                    Assert.AreEqual(container.TwoDimIntArray2x2[i,j], result.TwoDimIntArray2x2[i,j]);
                }
            }
        }

        [Test]
        public void TestIdlArraysAsAny() {
            IdlArrayContainer container = new IdlArrayContainer();
            container.OneDimIntArray5 = new int[] { 1, 2, 3, 4, 5 };
            container.TwoDimIntArray2x2 = new int[,] { { 1, 2 }, { 3, 4 } };
            IdlArrayContainer result =
                (IdlArrayContainer)m_testService.EchoAnything(container);
            Assert.AreEqual(container.OneDimIntArray5.Length, result.OneDimIntArray5.Length);
            Assert.AreEqual(container.TwoDimIntArray2x2.GetLength(0), result.TwoDimIntArray2x2.GetLength(0));
            Assert.AreEqual(container.TwoDimIntArray2x2.GetLength(1), result.TwoDimIntArray2x2.GetLength(1));

            for (int i = 0; i < container.OneDimIntArray5.Length; i++) {
                Assert.AreEqual(container.OneDimIntArray5[i], result.OneDimIntArray5[i]);
            }

            for (int i = 0; i < container.TwoDimIntArray2x2.GetLength(0); i++) {
                for (int j = 0; j < container.TwoDimIntArray2x2.GetLength(1); j++) {
                    Assert.AreEqual(container.TwoDimIntArray2x2[i,j], result.TwoDimIntArray2x2[i,j]);
                }
            }

            // test with any container
            int[] arg1Dim = new int[] { 1, 2, 3, 4, 5 };
            int[] result1Dim = (int[])m_testService.RetrieveIdlIntArrayAsAny(arg1Dim);
            for (int i = 0; i < arg1Dim.Length; i++) {
                Assert.AreEqual(arg1Dim[i], result1Dim[i]);
            }


            int[,] arg2Dim = new int[,] { { 1,2 }, {3, 4} };
            int[,] result2Dim = (int[,])m_testService.RetrieveIdlInt2DimArray2x2AsAny(arg2Dim);
            for (int i = 0; i < arg2Dim.GetLength(0); i++) {
                for (int j = 0; j < arg2Dim.GetLength(1); j++) {
                    Assert.AreEqual(arg2Dim[i,j], result2Dim[i,j]);
                }
            }

            int[,,] arg3Dim = new int[2,2,3];
            arg3Dim[0,0,0] = 1;
            arg3Dim[0,0,1] = 2;
            arg3Dim[0,0,2] = 3;
            arg3Dim[0,1,0] = 4;
            arg3Dim[0,1,1] = 5;
            arg3Dim[0,1,2] = 6;
            arg3Dim[1,0,0] = 7;
            arg3Dim[1,0,1] = 8;
            arg3Dim[1,0,2] = 9;
            arg3Dim[1,1,0] = 10;
            arg3Dim[1,1,1] = 11;
            arg3Dim[1,1,2] = 12;
            int[,,] result3Dim = (int[,,])m_testService.RetrieveIdlInt3DimArray2x2x3AsAny(arg3Dim);
            Assert.AreEqual(arg3Dim.GetLength(0), result3Dim.GetLength(0));
            Assert.AreEqual(arg3Dim.GetLength(1), result3Dim.GetLength(1));
            Assert.AreEqual(arg3Dim.GetLength(2), result3Dim.GetLength(2));
            for (int i = 0; i < arg3Dim.GetLength(0); i++) {
                for (int j = 0; j < arg3Dim.GetLength(1); j++) {
                    for (int k = 0; k < arg3Dim.GetLength(2); k++) {
                        Assert.AreEqual(arg3Dim[i,j,k], result3Dim[i,j,k]);
                    }
                }
            }
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
        public void TestRemoteObjectPassedAsAny() {
            Object adderAsObject = m_testService.RetrieveAdderAsAny();
            Adder adder = (Adder)adderAsObject;
            System.Int32 arg1 = 1;
            System.Int32 arg2 = 2;
            System.Int32 result = adder.Add(1, 2);
            Assert.AreEqual((System.Int32) arg1 + arg2, result);
        }

        [Test]
        public void TestRemoteObjectPassedForAbstractBase() {
            Object adderAsAbstractBase = m_testService.RetrieveAdderForAbstractInterfaceBase();
            Adder adder = (Adder)adderAsAbstractBase;
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
            TestStructA arg = new TestStructAImpl();
            arg.X = 11;
            arg.Y = -15;
            TestStructA result = m_testService.TestEchoStruct(arg);
            Assert.AreEqual(arg.X, result.X);
            Assert.AreEqual(arg.Y, result.Y);
        }

        [Test]
        public void TestStructIdl() {
            TestStructAIdl arg = new TestStructAIdl();
            arg.X = 11;
            arg.Y = -15;
            TestStructAIdl result = m_testService.TestEchoIdlStruct(arg);
            Assert.AreEqual(arg.X, result.X);
            Assert.AreEqual(arg.Y, result.Y);
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
            Assert.AreEqual(result.Msg, arg.Msg);
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
            Assert.AreEqual(newDetail, result.DetailedMsg);
            Assert.AreEqual(arg.Msg, result.Msg);
        }

        /// <summary>
        /// Checks, if a formal parameter type, which is not Serilizable works correctly,
        /// if an instance of a Serializable subclass is passed.
        /// </summary>
        [Test]
        public void TestNonSerilizableFormalParam() {
            TestNonSerializableBaseClass arg = new TestSerializableClassCImpl();
            TestNonSerializableBaseClass result = m_testService.TestAbstractValueTypeEcho(arg);
            Assert.AreEqual(typeof(TestSerializableClassCImpl), result.GetType());
        }

        [Test]
        public void TestBaseTypeNonSerializableParam() {
            TestSerializableClassCImpl arg = new TestSerializableClassCImpl();
            arg.Msg = "test";
            TestSerializableClassC result = m_testService.TestEchoSerializableC(arg);
            Assert.AreEqual(arg.Msg, result.Msg);
            // check method implementation called
            Assert.AreEqual(result.Msg, result.Format());
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
            Assert.AreEqual(newMsg, result.val1.Msg);
            Assert.AreEqual(result.val1, result.val2);
            Assert.AreEqual(result.val1.Msg, result.val2.Msg);
        }

        /// <summary>
        /// checks, if recursive values are serialised using an indirection
        /// </summary>
        [Test]
        public void TestRecursiveValueType() {
            TestSerializableClassE arg = new TestSerializableClassEImpl();
            arg.RecArrEntry = new TestSerializableClassE[1];
            arg.RecArrEntry[0] = arg;
            TestSerializableClassE result = m_testService.TestEchoSerializableE(arg);
            Assert.NotNull(result);
            Assert.NotNull(result.RecArrEntry);
            Assert.AreEqual(arg.RecArrEntry.Length, result.RecArrEntry.Length);
            Assert.IsTrue(result == result.RecArrEntry[0], "invalid entry in recArrEntry");
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
        [Test]
        public void TestReceivingUnknownInterfaceImplementor() {
            TestEchoInterface result = m_testService.RetrieveUnknownEchoInterfaceImplementor();
            // result is a proxy
            Assert.AreEqual(true, RemotingServices.IsTransparentProxy(result));
            System.Int32 arg = 23;
            System.Int32 echo = result.EchoInt(arg);
            Assert.AreEqual(arg, echo);
        }




        /// <summary>
        /// Checks unknown implementation class of interface as any
        /// </summary>
        [Test]
        public void TestReceivingUnknownInterfaceImplementorAsAny() {
            object resultAsAny = m_testService.RetrieveUnknownEchoInterfaceImplementorAsAny();
            TestEchoInterface result = (TestEchoInterface)resultAsAny;
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
        public void TestPassingForFormalParamObjectComplexTypes() {
            System.String arg1 = "test";
            System.String result1 = (System.String) m_testService.EchoAnything(arg1);
            Assert.AreEqual(arg1, result1);

            TestSerializableClassB1 arg2 = new TestSerializableClassB1Impl();
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

            TestJaggedArrays(); // problems with name mapping possible -> make sure to check afterwards
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
        public void TestAsyncCallInParallel() {
            System.Boolean arg = true;
            TestNegateBooleanDelegate nbd = new TestNegateBooleanDelegate(m_testService.TestNegateBoolean);
            IAsyncResult[] callResults = new IAsyncResult[50];
            // async calls
            for (int i = 0; i < callResults.Length; i++) {
                callResults[i] = nbd.BeginInvoke(arg, null, null);
            }
            // wait for responses
            for (int i = 0; i < callResults.Length; i++) {
                System.Boolean result = nbd.EndInvoke(callResults[i]);
                Assert.AreEqual(false, result);
            }
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
        public void TestOutArgs() {
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
        public void TestInRefOutArgsMixed() {
            System.Int32 inarg = 10;
            System.Int32 inarg2 = 20;
            System.Int32 outArg;
            System.Int32 outArg2;
            System.Int32 inoutArgBefore = 3;
            System.Int32 inoutArg = inoutArgBefore;
            System.Int32 inoutArgBefore2 = 5;
            System.Int32 inoutArg2 = inoutArgBefore2;

            System.Int32 result = m_testService.TestInOutRef(inarg, out outArg, ref inoutArg,
                                                             inarg2, ref inoutArg2, out outArg2);

            Assert.AreEqual(inarg + inarg2, result);
            Assert.AreEqual(inarg + 1, outArg);
            Assert.AreEqual(inarg2 + 1, outArg2);
            Assert.AreEqual(inoutArgBefore * 2, inoutArg);
            Assert.AreEqual(inoutArgBefore2 * 2, inoutArg2);
        }

        [Test]
        public void TestOverloadedMethods() {
            System.Int32 arg1int = 1;
            System.Int32 arg2int = 2;
            System.Int32 arg3int = 2;

            System.Double arg1double = 1.0;
            System.Double arg2double = 2.0;

            System.Int32 result1 = m_testService.AddOverloaded__long__long(arg1int, arg2int);
            Assert.AreEqual((System.Int32)(arg1int + arg2int), result1);
            System.Int32 result2 = m_testService.AddOverloaded__long__long__long(arg1int, arg2int, arg3int);
            Assert.AreEqual((System.Int32)(arg1int + arg2int + arg3int), result2);
            System.Double result3 = m_testService.AddOverloaded__double__double(arg1double, arg2double);
            Assert.AreEqual((System.Double)(arg1double + arg2double), result3);
        }

        [Test]
        public void TestNameClashes() {
            System.Int32 arg = 89;
            System.Int32 result = m_testService._custom(arg);
            Assert.AreEqual(arg, result);

            m_testService._context = arg;
            Assert.AreEqual(arg, m_testService._context);
        }

        [Test]
        public void TestNamesStartingWithUnderScore() {
            System.Int32 arg = 99;
            System.Int32 result = m_testService.N_echoInt(arg);
            Assert.AreEqual(arg, result);
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
        }

        [ExpectedException(typeof(omg.org.CosNaming.NamingContext_package.NotFound))]
        [Test]
        public void TestNonExistent() {
            NamingContext nameService = GetNameService();
            NameComponent[] name = new NameComponent[] { new NameComponent("testXYZ", "") };
            // get the reference to non-existent service
            m_testService = (TestService)nameService.resolve(name);
        }

        [Test]
        public void TestRaisesClauseUserException() {
            // interface was mapped from idl to cls on the server side
            try {
                m_testExService.ThrowTestException();
            } catch (TestException) {
                // ok, expected this exception
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
            // interface is defined in CLS on the server side
            try {
                m_testService.ThrowKnownException();
            } catch (TestNetExceptionMappedToIdl tex) {
                Assert.AreEqual(10, tex.code, "wrong code");
                // ok, expected this exception
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
        }

        [Test]
        public void TestRaisesClauseSystemException() {
            // interface was mapped from idl to cls on the server side
            try {
                m_testExService.ThrowSystemException();
            } catch (omg.org.CORBA.NO_IMPLEMENT nex) {
                // ok, expected this exception
                Assert.AreEqual(9, nex.Minor, "wrong minor code");
                Assert.AreEqual(omg.org.CORBA.CompletionStatus.Completed_Yes, nex.Status, "wrong status");
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
        }

        [Test]
        public void TestRaisesClauseNotIncludedUserException() {
            // interface was mapped from idl to cls on the server side
            try {
                m_testExService.ThrowDotNetException();
            } catch (omg.org.CORBA.UNKNOWN uex) {
                // ok, expected this exception
                Assert.AreEqual(189, uex.Minor, "wrong minor code");
                Assert.AreEqual(omg.org.CORBA.CompletionStatus.Completed_Yes, uex.Status, "wrong status");
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
            // interface is defined in CLS on the server side
            try {
                m_testService.ThrowUnKnownException();
            } catch (GenericUserException) {
                // ok, expected this exception
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
        }


        [Test]
        public void TestAttributesThrowingExceptions() {
            // interface was mapped from idl to cls on the server side
            try {
                bool result = m_testExService.TestAttrWithException;
            } catch (omg.org.CORBA.UNKNOWN uex) {
                Assert.AreEqual(190, uex.Minor, "wrong minor code");
                Assert.AreEqual(omg.org.CORBA.CompletionStatus.Completed_Yes, uex.Status, "wrong status");
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
            try {
                m_testExService.TestAttrWithException = true;
            } catch (omg.org.CORBA.NO_IMPLEMENT nex) {
                Assert.AreEqual(10, nex.Minor, "wrong minor code");
                Assert.AreEqual(omg.org.CORBA.CompletionStatus.Completed_Yes, nex.Status, "wrong status");
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
            // interface is defined in CLS on the server side
            try {
                bool result2 = m_testService.TestPropWithGetUserException;
            } catch (omg.org.CORBA.UNKNOWN uex) {
                Assert.AreEqual(190, uex.Minor, "wrong minor code");
                Assert.AreEqual(omg.org.CORBA.CompletionStatus.Completed_Yes, uex.Status, "wrong status");
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
            try {
                bool result3 =m_testService.TestPropWithGetSystemException;
            } catch (omg.org.CORBA.INTERNAL iex) {
                Assert.AreEqual(29, iex.Minor, "wrong minor code");
                Assert.AreEqual(omg.org.CORBA.CompletionStatus.Completed_Yes, iex.Status, "wrong status");
            } catch (Exception ex) {
                Assert.Fail("wrong exception type: " + ex.GetType());
            }
        }

        [Test]
        public void TestExceptionFromIdlNetSerializable() {
            // check, that the generated exception is serializable also with other formatters, i.e. implements ISerializable correctly
            TestException ex = new TestException();
            ex.Code = 2;
            ex.Msg = "msg";
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.MemoryStream serialised = new System.IO.MemoryStream();
            try {
                formatter.Serialize(serialised, ex);

                serialised.Seek(0, System.IO.SeekOrigin.Begin);
                formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                TestException deser = (TestException)formatter.Deserialize(serialised);
                Assert.AreEqual(ex.Code, deser.Code, "ex.Code");
                Assert.AreEqual(ex.Msg, deser.Msg, "ex.Msg");
            } finally {
                serialised.Close();
            }

        }

        [Test]
        public void TestSystemType() {
            IOrbServices orb = OrbServices.GetSingleton();

            Type arg1 = typeof(System.Int32);
            omg.org.CORBA.TypeCode arg1Tc = orb.create_tc_for_type(arg1);
            omg.org.CORBA.TypeCode result1TC = m_testService.EchoType(arg1Tc);
            Type result1 = orb.get_type_for_tc(result1TC);
            Assert.AreEqual(arg1, result1, "wrong type for int32 echo");

            Type arg2 = typeof(System.Boolean);
            omg.org.CORBA.TypeCode arg2Tc = orb.create_tc_for_type(arg2);
            omg.org.CORBA.TypeCode result2TC = m_testService.EchoType(arg2Tc);
            Type result2 = orb.get_type_for_tc(result2TC);
            Assert.AreEqual(arg2, result2, "wrong type for Boolean echo");

            Type arg3 = typeof(TestService);
            omg.org.CORBA.TypeCode arg3Tc = orb.create_tc_for_type(arg3);
            omg.org.CORBA.TypeCode result3TC = m_testService.EchoType(arg3Tc);
            Type result3 = orb.get_type_for_tc(result3TC);
            Assert.AreEqual(arg3, result3, "wrong type for testService type echo");

            Type arg4 = null;
            omg.org.CORBA.TypeCode arg4Tc = orb.create_tc_for_type(arg4);
            omg.org.CORBA.TypeCode result4TC = m_testService.EchoType(arg4Tc);
            Type result4 = orb.get_type_for_tc(result4TC);
            Assert.AreEqual(arg4, result4, "wrong type for null type echo");

        }

        [Test]
        public void TestSeqOfSeqByRefBugReport() {
            SSensi arg0Elem = new SSensi();
            arg0Elem.ICode = 1;
            arg0Elem.IDev = 1;
            arg0Elem.Sensibilites = new Int64[] { 1L, 2L };
            SSensi arg1Elem = new SSensi();
            arg1Elem.ICode = 2;
            arg1Elem.IDev = 2;
            arg1Elem.Sensibilites = new Int64[0];
            SSensi[] arg = new SSensi[] { arg0Elem, arg1Elem } ;
            int argLengthBefore = arg.Length;
            m_testService.TestDuplicateSeqOfSeqInOut(ref arg);
            Assert.AreEqual(argLengthBefore * 2, arg.Length, "wrong length");
            CheckSSensiEquality(arg0Elem, arg[0]);
            CheckSSensiEquality(arg0Elem, arg[1]);
            CheckSSensiEquality(arg1Elem, arg[2]);
            CheckSSensiEquality(arg1Elem, arg[3]);
        }

        private void CheckSSensiEquality(SSensi expected, SSensi compare) {
            Assert.AreEqual(expected.ICode, compare.ICode, "wrong ICode");
            Assert.AreEqual(expected.IDev, compare.IDev, "wrong IDev");
            Assert.NotNull(expected.Sensibilites, "wrong Sensibilities");
            Assert.NotNull(compare.Sensibilites, "wrong Sensibilities");
            Assert.AreEqual(expected.Sensibilites.Length, compare.Sensibilites.Length,
                            "wrong number of entries in Sensibilities");
            for (int i = 0; i < expected.Sensibilites.Length; i++) {
                Assert.AreEqual(expected.Sensibilites[i], compare.Sensibilites[i],
                                "wrong sensibilities element " + i);
            }
        }

        [Test]
        public void TestWellKnownServiceType() {
            CheckWellKnownService(m_svcSingletonCall, true);
            CheckWellKnownService(m_svcSingleCall, false);
        }

        [Test]
        public void TestContextBoundServiceType() {
            CheckWellKnownService(m_contextBound, true);
        }

        private void CheckWellKnownService(ISimpleTestInterface svcToCheck, bool stateShouldBeKept) {
            Int32 arg1 = 1;
            Int32 arg2 = 2;
            Assert.AreEqual(arg1 + arg2, svcToCheck.Add(arg1, arg2));

            Int32 stateSet = 8;
            Int32 stateSet2 = 10;
            svcToCheck.TestValue = stateSet;
            if (stateShouldBeKept) {
                Assert.AreEqual(stateSet, svcToCheck.TestValue, "set 1 failed");
                svcToCheck.TestValue = stateSet2;
                Assert.AreEqual(stateSet2, svcToCheck.TestValue, "set 2 failed");
            } else {
                Assert.AreEqual(svcToCheck.InitialValue, svcToCheck.TestValue, "set 1 failed");
                svcToCheck.TestValue = stateSet2;
                Assert.AreEqual(svcToCheck.InitialValue, svcToCheck.TestValue, "set 2 failed");
            }
        }

        [Test]
        public void TestMBRTypesWithReservedNameCollisions() {
            CCE._Assembly asm = m_testService.CreateAsm();
            Assert.NotNull(asm, "asm not created");

            CCE.N_Assembly _asm = m_testService.Create_Asm();
            Assert.NotNull(_asm, "_asm not created");
        }


        [Test]
        public void TestContextElements() {
            string arg = "test-Arg";
            string entryName = "element1";
            CorbaContextElement entry = new CorbaContextElement(arg);
            CallContext.SetData(entryName, entry);
            try {
                string extractedElem = m_testService.TestContextElementPassing();
                Assert.AreEqual(arg, extractedElem, "wrong entry extracted from callcontext");
            } finally {
                CallContext.FreeNamedDataSlot(entryName);
            }
        }

        [Test]
        public void TestBoxedValuetypes() {
            string arg1 = "test-Arg";
            string result1 = m_testIdlTypesService.EchoBoxedString(arg1);
            Assert.AreEqual(arg1, result1, "wrong boxed string returned");

            TestStructWB arg2 = new TestStructWB();
            arg2.a = "a";
            arg2.b = "b";
            TestStructWB result2 = m_testIdlTypesService.EchoBoxedStruct(arg2);
            Assert.AreEqual(arg2, result2, "wrong boxed struct returned");
        }

        [Test]
        public void TestUnions() {
            TestUnionLD arg = new TestUnionLD();
            short argVal = 12;
            arg.Setval0(argVal);
            TestUnionLD result = m_testIdlTypesService.EchoLDUnion(arg);
            Assert.AreEqual(arg.Discriminator, result.Discriminator, "Wrong disc val returned");
            Assert.AreEqual(argVal, result.Getval0(), "wrong union val returned");
        }


        /// regression for bug 2355283
        [Test]
        public void TestEmptySeqAlign() {
            int a = 0;
            long[] b, c;
            string[] d;

            m_testIdlTypesService.TestEmptySeqAlignment(ref a, out b, out c, out d);
            Assert.AreEqual(42, a, "Wrong a val returned");
            Assert.AreEqual(0, b.Length, "Wrong array b val returned");
            Assert.AreEqual(0, c.Length, "Wrong array c val returned");
            Assert.AreEqual(0, d.Length, "Wrong array d val returned");

            a = 1;
            m_testIdlTypesService.TestEmptySeqAlignment(ref a, out b, out c, out d);
            Assert.AreEqual(42, a, "Wrong a val returned");
            Assert.AreEqual(1, b.Length, "Wrong array b val returned");
            Assert.AreEqual(1, c.Length, "Wrong array c val returned");
            Assert.AreEqual(1, d.Length, "Wrong array d val returned");
        }

        [Test]
        public void TestDetectIncompatibleTargetIf() {
            // get the reference to the test-service
            TestService testService = (TestService)
                RemotingServices.Connect(typeof(TestService), "corbaloc:iiop:1.2@localhost:8087/test");
            TestService testService2 = (TestService)
                RemotingServices.Connect(typeof(TestService), "corbaloc:iiop:1.2@localhost:8087/testExService");
            Assert.AreEqual(true, testService.TestNegateBoolean(false));
            try {
                testService2.TestNegateBoolean(false);
            } catch (omg.org.CORBA.BAD_PARAM bpEx) {
                Assert.AreEqual(20010, bpEx.Minor);
            }
        }

        [Test]
        public void TestEchoBoxedSeq() {
            int[] arg = new int[] { 1, 2 };
            int[] result = m_testIdlTypesService.EchoBoxedSeq(arg);
            Assert.NotNull(result);
            Assert.AreEqual(arg.Length, result.Length);
            Assert.AreEqual(arg[0], result[0]);
            Assert.AreEqual(arg[1], result[1]);
        }

        [Test]
        public void TestEchoObjectArray() {
            object[] arg = new object[] { "abc", (int)123 };
            object[] result = m_testService.EchoObjectArray(arg);
            Assert.NotNull(result);
            Assert.AreEqual(arg.Length, result.Length);
            Assert.AreEqual(arg[0], result[0]);
            Assert.AreEqual(arg[1], result[1]);
        }


        [Test]
        public void TestVoidFromIdl() {
            int arg = 11;
            m_testOneWayService.SetArgumentVoid(arg);
            Assert.AreEqual(arg, m_testOneWayService.GetArgumentVoid());
        }

        [Test]
        public void TestOneWayFromIdl() {
            int arg = 12;
            m_testOneWayService.SetArgumentOneWay(arg);
            int i = 0;
            int result = 0;
            while (result != arg && i < 100) {
                result = m_testOneWayService.GetArgumentOneWay();
                i++;
            }
            Assert.AreEqual(arg, result);
            arg = 13;
            m_testOneWayService.SetArgumentOneWay(arg);

            i = 0;
            result = 0;
            while (result != arg && i < 100) {
                result = m_testOneWayService.GetArgumentOneWay();
                i++;
            }
            Assert.AreEqual(arg, result);
        }

        #endregion IMethods


    }

    [TestFixture]
    public class TestClientWithCallback {

        #region IFields

        private IiopChannel m_channel;

        private TestServiceWithCallback1 m_testServiceWithCallback1;
        // private TestServiceWithCallback2 m_testServiceWithCallback2;

        #endregion IFields

        private static T Connect<T>(string uri) {
            return (T)RemotingServices.Connect(typeof(T), uri);
        }

        [TestFixtureSetUp]
        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopChannel(0);
            ChannelServices.RegisterChannel(m_channel, false);

            // get the reference to the test-service
            m_testServiceWithCallback1 = Connect<TestServiceWithCallback1>("corbaloc:iiop:1.2@localhost:8087/testServiceWithCallback");
            // m_testServiceWithCallback2 = Connect<TestServiceWithCallback2>("corbaloc:iiop:1.2@localhost:8087/testServiceWithCallback");
        }

        [TestFixtureTearDown]
        public void TearDownEnvironment() {
            // unregister the channel
            ChannelServices.UnregisterChannel(m_channel);
        }

        [Test]
        public void TestServiceWithCallback() {
            CallbackImpl callback = new CallbackImpl();
            m_testServiceWithCallback1.Ping1(3, callback);
            int result;
            Assert.IsTrue(callback.Pop(TimeSpan.Zero, out result));
            Assert.AreEqual(3, result);
            m_testServiceWithCallback1.AsyncPing(1, callback);
            Assert.IsTrue(callback.Pop(TimeSpan.FromSeconds(2), out result));
            Assert.AreEqual(0, result);

            CallbackNestedImpl1 nestedCallback1 = new CallbackNestedImpl1();
            m_testServiceWithCallback1.Ping1(3, nestedCallback1);
            Assert.IsTrue(nestedCallback1.Pop1(out result));
            Assert.AreEqual(3, result);
        }

        [Test]
        public void TestServiceWithAmbiguousCallback() {
            int result;
            CallbackNestedImpl12 nestedCallback12 = new CallbackNestedImpl12();
            m_testServiceWithCallback1.Ping1(3, nestedCallback12);
            Assert.IsFalse(nestedCallback12.Pop2(out result), "Unexpected callback on Callback2 received");
            Assert.IsTrue(nestedCallback12.Pop1(out result), "No callback on Callback1 received");
            Assert.AreEqual(3, result);
            m_testServiceWithCallback1.Ping2(5, nestedCallback12);
            Assert.IsFalse(nestedCallback12.Pop1(out result), "Unexpected callback on Callback1 received");
            Assert.IsTrue(nestedCallback12.Pop2(out result), "No callback on Callback2 received");
            Assert.AreEqual(5, result);
        }

        private sealed class CallbackNestedImpl1 : MarshalByRefObject, Callback1 {
            private readonly Queue<int> pongs1;

            public CallbackNestedImpl1() {
                this.pongs1 = new Queue<int>();
            }

            void Callback1.Pong(int code) {
                this.pongs1.Enqueue(code);
            }

            public bool Pop1(out int code) {
                return Pop(this.pongs1, out code);
            }

            private static bool Pop(Queue<int> pongs, out int code) {
                if (pongs.Count == 0) {
                    code = 0;
                    return false;
                }
                code = pongs.Dequeue();
                return true;
            }

            public override object InitializeLifetimeService() {
                // live forever
                return null;
            }
        }

        private sealed class CallbackNestedImpl12 : MarshalByRefObject, Callback1, Callback2 {
            private readonly Queue<int> pongs1;
            private readonly Queue<int> pongs2;

            public CallbackNestedImpl12() {
                this.pongs1 = new Queue<int>();
                this.pongs2 = new Queue<int>();
            }

            void Callback1.Pong(int code) {
                this.pongs1.Enqueue(code);
            }

            void Callback2.Pong(int code) {
                this.pongs2.Enqueue(code);
            }

            public bool Pop1(out int code) {
                return Pop(this.pongs1, out code);
            }

            public bool Pop2(out int code) {
                return Pop(this.pongs2, out code);
            }

            private static bool Pop(Queue<int> pongs, out int code) {
                if (pongs.Count == 0) {
                    code = 0;
                    return false;
                }
                code = pongs.Dequeue();
                return true;
            }

            public override object InitializeLifetimeService() {
                // live forever
                return null;
            }
        }
    }

    internal sealed class CallbackImpl : MarshalByRefObject, Callback1 {
        private readonly Queue<int> pongs;

        public CallbackImpl() {
            this.pongs = new Queue<int>();
        }

        public void Pong(int code) {
            lock (this.pongs) {
                this.pongs.Enqueue(code);
                Monitor.Pulse(this.pongs);
            }
        }

        public bool Pop(TimeSpan timeout, out int code) {
            lock (this.pongs) {
                if (this.pongs.Count == 0 &&
                    !Monitor.Wait(this.pongs, timeout)) {
                    code = 0;
                    return false;
                }
                code = this.pongs.Dequeue();
                return true;
            }
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }
    }
}
