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
using omg.org.CORBA;

namespace Ch.Elca.Iiop.IntegrationTests {

    [TestFixture]
    public class TestClient {

        #region IFields

        private IiopClientChannel m_channel;

        private TestService m_testService;


        #endregion IFields

        private NamingContext GetNameService() {
            // access COS nameing service
            return (NamingContext)RemotingServices.Connect(typeof(NamingContext), 
                                                           "corbaloc::localhost:11356/NameService");
        }

        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopClientChannel();
            ChannelServices.RegisterChannel(m_channel);

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

        [Test]
        public void TestPassingStringAsAny() {
            OrbServices orb = OrbServices.GetSingleton();
            string arg = "test";
            omg.org.CORBA.TypeCode wstringTC = orb.create_wstring_tc(0);
            Any any = new Any(arg, wstringTC);
            
            string result = (string)m_testService.EchoAny(any);
            Assertion.AssertEquals(arg, result);
        }

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
        public void TestRecursiveAny() {
            TestUnionE3 arg = new TestUnionE3();
            TestEnumForU3 case0Val = TestEnumForU3.A3;
            arg.SetvalE0(case0Val);
            TestUnionE3 result = (TestUnionE3)m_testService.EchoAny(arg);
            Assertion.AssertEquals(case0Val, result.GetvalE0());
            Assertion.AssertEquals(TestEnumForU3.A3, result.Discriminator);            
        }

        [Test]
        public void TestReceivingUnknownUnionsAsAny() {
            object result = m_testService.RetrieveUnknownUnion();
            Assertion.AssertNotNull("union not retrieved", result);
            Assertion.AssertEquals("type name", "TestUnionE2", result.GetType().FullName);
        }
        
        [Test]
        public void TestTypeDefInAny() {            
            int memberElem = 5;
            object result = 
                m_testService.RetrieveStructWithTypedefMember(memberElem);
            Assertion.AssertNotNull("test struct with typedef null", result);
            Assertion.AssertEquals(typeof(StructWithTypedefMember), result.GetType());            
            Assertion.AssertEquals(memberElem, ((StructWithTypedefMember)result).longtdField);
            // test to receive a typedefed type directly
            int nrOfElems = 1;
            object result2 = m_testService.RetrieveTypedefedSeq(nrOfElems, memberElem);
            Assertion.AssertNotNull("typedefed seq null", result2);
            Assertion.AssertEquals(typeof(int[]), result2.GetType());
            Assertion.AssertEquals(nrOfElems, ((int[])result2).Length);
            Assertion.AssertEquals(memberElem, ((int[])result2)[0]);
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
        public void TestBoundedSeq() {
            int[] lengthOk = new int[] { 1, 2, 3 };
            int[] result = m_testService.EchoBoundedSeq(lengthOk);
            Assertion.AssertNotNull(result);
            Assertion.AssertEquals(lengthOk.Length, result.Length);
            for (int i = 0; i < result.Length; i++) {
                Assertion.AssertEquals(lengthOk[i], result[i]);
            }
            
            int[] tooLong = new int[] { 1, 2, 3, 4 };
            try {
                m_testService.EchoBoundedSeq(tooLong);
                Assertion.Fail("expected BAD_PARAM exception, because sequence too long, but not thrown");
            } catch (BAD_PARAM badParamE) {
                Assertion.AssertEquals(badParamE.Minor, 3434);
            }

        }

        [Test]
        public void TestSeqOfBoundedSeq() {
            byte[] innerSeqLengthOk = new byte[] { 1, 2, 3 };
            byte[][] arg = new byte[20][];
            for (int i = 0; i < arg.Length; i++) {
                arg[i] = innerSeqLengthOk;
            }
            byte[][] result = m_testService.EchoUuids(arg);
            Assertion.AssertNotNull(result);
            Assertion.AssertEquals(arg.Length, result.Length);
            for (int i = 0; i < result.Length; i++) {
                Assertion.AssertEquals(arg[i].Length, result[i].Length);
                for (int j = 0; j < result[i].Length; j++) {
                    Assertion.AssertEquals(arg[i][j], result[i][j]);
                }
            }          

            // too long test:
            byte[] innerSeqLengthTooBig = new byte[20];
            byte[][] argNotOk = new byte[10][];
            for (int i = 0; i < argNotOk.Length; i++) {
                argNotOk[i] = innerSeqLengthTooBig;
            }
            try {
                byte[][] result2 = m_testService.EchoUuids(argNotOk);
                Assertion.Fail("expected BAD_PARAM exception, because sequence too long, but not thrown");
            } catch(BAD_PARAM badParamE) {
                Assertion.AssertEquals(badParamE.Minor, 3434);
            }
        }

        [Test]
        public void TestRetrieveSeqOfBoundedSeqAsAny() {
            int outerLength = 20;
            int innerLength = 4;
            byte elemVal = 2;
            byte[][] result = (byte[][])m_testService.RetrieveUuidAsAny(outerLength, innerLength, elemVal);
            Assertion.AssertNotNull(result);
            Assertion.AssertEquals("wrong nr of Uuid elements", outerLength, result.Length);
            for (int i = 0; i < result.Length; i++) {
                Assertion.AssertEquals(innerLength, result[i].Length);
                for (int j = 0; j < result[i].Length; j++) {
                    Assertion.AssertEquals(elemVal, result[i][j]);
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
        
        [Test]
        public void TestIdlKeyWordMethodNames() {
            byte arg = 39;
            byte result = m_testService._octet(arg);
            Assertion.AssertEquals("wrong result octet-call", arg, result);
        }

        [Test]
        public void TestULongAsAny() {
            int arg = 74;
            int result = (int) m_testService.RetrieveULongAsAny(arg);
            Assertion.AssertEquals("wrong result of retrieveULongAsAny", arg, result);
            
            OrbServices orb = OrbServices.GetSingleton();
            int arg2 = 89;
            omg.org.CORBA.TypeCode ulongTC = orb.create_ulong_tc();
            Any any = new Any(arg2, ulongTC);
            int result2 = m_testService.ExtractFromULongAny(any);
            Assertion.AssertEquals("wrong result of ExtractFromULongAny", arg2, result2);
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