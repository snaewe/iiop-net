/* TestClient.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 08.10.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
            NamingContext nameService = (NamingContext)RemotingServices.Connect(typeof(NamingContext), 
                                                                                "corbaloc::localhost:11356/NameService");
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
        public void TestSimpleUnion() {
            TestUnion arg = new TestUnion();
            short case0Val = 11;
            arg.Setval0(case0Val);
            TestUnion result = m_testService.EchoTestUnion(arg);
            Assertion.AssertEquals(case0Val, result.Getval0());
            Assertion.AssertEquals(0, result.Discriminator);

	    TestUnion arg2 = new TestUnion();
            int case1Val = 12;
            arg2.Setval1(case1Val, 2);
            TestUnion result2 = m_testService.EchoTestUnion(arg2);
            Assertion.AssertEquals(case1Val, result2.Getval1());
            Assertion.AssertEquals(2, result2.Discriminator);

	    TestUnion arg3 = new TestUnion();
            bool case2Val = true;
            arg3.Setval2(case2Val, 7);
            TestUnion result3 = m_testService.EchoTestUnion(arg3);
            Assertion.AssertEquals(case2Val, result3.Getval2());
            Assertion.AssertEquals(7, result3.Discriminator);            
        }

        [Test]
        public void TestEnumBasedUnionNoExceptions() {
            TestUnionE arg = new TestUnionE();
            short case0Val = 11;
            arg.SetvalE0(case0Val);
            TestUnionE result = m_testService.EchoTestUnionE(arg);
            Assertion.AssertEquals(case0Val, result.GetvalE0());
            Assertion.AssertEquals(TestEnumForU.A, result.Discriminator);

	    TestUnionE arg2 = new TestUnionE();
            TestEnumForU case1Val = TestEnumForU.A;
            arg2.SetvalE1(case1Val, TestEnumForU.B);
            TestUnionE result2 = m_testService.EchoTestUnionE(arg2);
            Assertion.AssertEquals(case1Val, result2.GetvalE1());
            Assertion.AssertEquals(TestEnumForU.B, result2.Discriminator);
        }

        [Test]
        public void TestPassingUnionsAsAny() {
            TestUnion arg = new TestUnion();
            short case0Val = 11;
            arg.Setval0(case0Val);
            TestUnion result = (TestUnion)m_testService.EchoAny(arg);
            Assertion.AssertEquals(case0Val, result.Getval0());
            Assertion.AssertEquals(0, result.Discriminator);

            TestUnionE arg2 = new TestUnionE();
            TestEnumForU case1Val = TestEnumForU.A;
            arg2.SetvalE1(case1Val, TestEnumForU.B);
            TestUnionE result2 = (TestUnionE)m_testService.EchoAny(arg2);
            Assertion.AssertEquals(case1Val, result2.GetvalE1());
            Assertion.AssertEquals(TestEnumForU.B, result2.Discriminator);
        }

//        [Test]
//        public void TestPassingStringAsAny() {
//            string arg = "test";
//            string result = (string)m_testService.EchoAny(arg);
//            Assertion.AssertEquals(arg, result);
//        }

        [Test]
        public void TestReceivingStringAsAny() {
            string arg = "test";
            string result = (string)m_testService.RetrieveWStringAsAny(arg);
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestPassingSimpleAsAny() {
            int arg = 21;
            int result = (int)m_testService.EchoAny(arg);
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestReceivingUnknownUnionsAsAny() {
	        object result = m_testService.RetrieveUnknownUnion();
            Assertion.AssertNotNull("union not retrieved", result);
            Assertion.AssertEquals("type name", "TestUnionE2", result.GetType().FullName);
        }

        [Test]
        public void TestWStringSeq() {
            string elem = "seqElem";
            int nrOfElems = 5;

            string[] retrieved = m_testService.RetrieveWstringSeq(elem, nrOfElems);
            Assertion.AssertNotNull("wstring seq not retrieved", retrieved);            
            Assertion.AssertEquals(nrOfElems, retrieved.Length);
            for (int i = 0; i < retrieved.Length; i++) {
                Assertion.AssertEquals("array element i:" + i + " not ok; " + retrieved[i],
                                       elem, retrieved[i]);            
            }

            string[] arg = new string[] { "Nr1", "Nr2", "Nr3" };
            string[] result = m_testService.EchoWstringSeq(arg);
            Assertion.AssertNotNull("wstring seq not retrieved", result);
            Assertion.AssertEquals(arg.Length, result.Length);
            for (int i = 0; i < arg.Length; i++) {
                Assertion.AssertEquals("array element i:" + i + " not ok; " + result[i], arg[i], result[i]);
            }
        }

        [Test]
        public void TestSeqOfWStringSeq() {
            int nrOfOuterElems = 2;
            int nrOfInnerElems = 5;
            string elem = "seqElem";
            string[][] arg = new string[nrOfOuterElems][];
            for (int i = 0; i < arg.Length; i++) {
                arg[i] = new string[nrOfInnerElems];
                for (int j = 0; j < nrOfInnerElems; j++) {
                    arg[i][j] = elem;
                }
            }
            string[][] result = m_testService.EchoSeqOfWStringSeq(arg);
            Assertion.AssertNotNull(result);
            Assertion.AssertEquals(arg.Length, result.Length);
            for (int i = 0; i < nrOfOuterElems; i++) {
                Assertion.AssertNotNull(result[i]);
                Assertion.AssertEquals(arg[i].Length, result[i].Length);
                for (int j = 0; j < nrOfInnerElems; j++) {
                    Assertion.AssertEquals(arg[i][j], result[i][j]);
                }
            }
            
        }

        [Test]
        public void TestNestedStructTypes() {
            TestService_package.InnerStruct arg = new TestService_package.InnerStruct();
            arg.Field1 = 21;
            TestService_package.InnerStruct result = m_testService.EchoInnerStruct(arg);
            Assertion.AssertNotNull(result);
            Assertion.AssertEquals(arg.Field1, result.Field1);
        }

        [Test]
        public void TestRecStruct() {
            RecStruct arg = new RecStruct();
            arg.seq = new RecStruct[1];
            arg.seq[0].seq = new RecStruct[0]; // a null sequence is not allowed
            RecStruct result = m_testService.EchoRecStruct(arg);
            Assertion.AssertNotNull(result);
            Assertion.AssertNotNull(result.seq);
            Assertion.AssertEquals(arg.seq.Length, result.seq.Length);
            Assertion.AssertNotNull(result.seq[0]);
        }


        [Test]
        public void TestWChar() {
            char arg = 'a';
            char result = m_testService.EchoWChar(arg);
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestWString() {
            string arg = "test";
            string result = m_testService.EchoWString(arg);
            Assertion.AssertEquals(arg, result);
        }


    }

}