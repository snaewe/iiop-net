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
        private TestSimpleServicePublic m_testServiceInternalIf;

        private IOrbServices m_orb;

        #endregion IFields

        private NamingContext GetNameService() {
            // access COS nameing service
            return (NamingContext)RemotingServices.Connect(typeof(NamingContext), 
                                                           "corbaloc::localhost:11356/NameService");
        }

        [SetUp]
        public void SetupEnvironment() {
            MappingConfiguration.Instance.UseBoxedInAny = false; // disable boxing of string/arrays in any's
            // register the channel
            m_channel = new IiopClientChannel();
            ChannelServices.RegisterChannel(m_channel, false);

            NamingContext nameService = GetNameService();
            NameComponent[] name = new NameComponent[] { new NameComponent("test", "") };
            // get the reference to the test-service
            m_testService = (TestService)nameService.resolve(name);

            NameComponent[] nameInternal = 
                new NameComponent[] { new NameComponent("testInternal", "") };
            // get the reference to a service with a server-side only interface inherited from a public one
            m_testServiceInternalIf = 
               (TestSimpleServicePublic)nameService.resolve(nameInternal);

            m_orb = OrbServices.GetSingleton();
        }

        [TearDown]
        public void TearDownEnvironment() {
            m_testService = null;
            m_testServiceInternalIf = null;
            // unregister the channel            
            ChannelServices.UnregisterChannel(m_channel);
        }

        [Test]
        public void TestSimpleUnion() {
            TestUnion arg = new TestUnion();
            short case0Val = 11;
            arg.Setval0(case0Val);
            TestUnion result = m_testService.EchoTestUnion(arg);
            Assert.AreEqual(case0Val, result.Getval0());
            Assert.AreEqual(0, result.Discriminator);

            TestUnion arg2 = new TestUnion();
            int case1Val = 12;
            arg2.Setval1(case1Val, 2);
            TestUnion result2 = m_testService.EchoTestUnion(arg2);
            Assert.AreEqual(case1Val, result2.Getval1());
            Assert.AreEqual(2, result2.Discriminator);

            TestUnion arg3 = new TestUnion();
            bool case2Val = true;
            arg3.Setval2(case2Val, 7);
            TestUnion result3 = m_testService.EchoTestUnion(arg3);
            Assert.AreEqual(case2Val, result3.Getval2());
            Assert.AreEqual(7, result3.Discriminator);            
        }

        [Test]
        public void TestEnumBasedUnionNoExceptions() {
            TestUnionE arg = new TestUnionE();
            short case0Val = 11;
            arg.SetvalE0(case0Val);
            TestUnionE result = m_testService.EchoTestUnionE(arg);
            Assert.AreEqual(case0Val, result.GetvalE0());
            Assert.AreEqual(TestEnumForU.A, result.Discriminator);

            TestUnionE arg2 = new TestUnionE();
            TestEnumForU case1Val = TestEnumForU.A;
            arg2.SetvalE1(case1Val, TestEnumForU.B);
            TestUnionE result2 = m_testService.EchoTestUnionE(arg2);
            Assert.AreEqual(case1Val, result2.GetvalE1());
            Assert.AreEqual(TestEnumForU.B, result2.Discriminator);
        }

        [Test]
        public void TestPassingUnionsAsAny() {
            TestUnion arg = new TestUnion();
            short case0Val = 11;
            arg.Setval0(case0Val);
            TestUnion result = (TestUnion)m_testService.EchoAny(arg);
            Assert.AreEqual(case0Val, result.Getval0());
            Assert.AreEqual(0, result.Discriminator);

            TestUnionE arg2 = new TestUnionE();
            TestEnumForU case1Val = TestEnumForU.A;
            arg2.SetvalE1(case1Val, TestEnumForU.B);
            TestUnionE result2 = (TestUnionE)m_testService.EchoAny(arg2);
            Assert.AreEqual(case1Val, result2.GetvalE1());
            Assert.AreEqual(TestEnumForU.B, result2.Discriminator);
        }

        [Test]
        public void TestPassingWStringAsAny() {
            // explicit mapping
            OrbServices orb = OrbServices.GetSingleton();
            string arg = "test";
            omg.org.CORBA.TypeCode wstringTC = orb.create_wstring_tc(0);
            Any any = new Any(arg, wstringTC);
            
            string result = (string)m_testService.EchoAny(any);
            Assert.AreEqual(arg, result);

            // improved implicit mapping (in case of any, don't map to boxed wstringvalue.
            string result2 = (string)m_testService.EchoAny(arg);
            Assert.AreEqual(arg, result2);

            // check extraction on server side with implicit mapping
            string result3 = m_testService.ExtractFromWStringAny(arg);
            Assert.AreEqual(arg, result3);
        }

        [Test]
        public void TestPassingStringAsAny() {
            // explicit mapping
            OrbServices orb = OrbServices.GetSingleton();
            string arg = "test";
            omg.org.CORBA.TypeCode stringTC = orb.create_string_tc(0);
            Any any = new Any(arg, stringTC);
            
            string result = (string)m_testService.EchoAny(any);
            Assert.AreEqual(arg, result);

            // check extraction on server side with explicit mapping
            string result3 = m_testService.ExtractFromStringAny(any);
            Assert.AreEqual(arg, result3);
        }


        [Test]
        public void TestReceivingWStringAsAny() {
            string arg = "test";
            string result = (string)m_testService.RetrieveWStringAsAny(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestReceivingStringAsAny() {
            string arg = "test";
            string result = (string)m_testService.RetrieveStringAsAny(arg);
            Assert.AreEqual(arg, result);
            string arg2 = "test2";
            string result2 = (string)m_testService.RetrieveStringAsAny(arg2);
            Assert.AreEqual(arg2, result2);
        }

        [Test]
        public void TestOctetOfOctetArrayAsAny() {
            byte[][] arg = new byte[2][];
            arg[0] = new byte[] { 1 };
            arg[1] = new byte[] { 2 };

            byte[][] result = (byte[][])m_testService.EchoAny(arg);
            Assert.IsNotNull(result);
            Assert.AreEqual(arg.Length, result.Length);
            Assert.AreEqual(arg[0].Length, result[0].Length);
            Assert.AreEqual(arg[1].Length, result[1].Length);
            Assert.AreEqual(arg[0][0], result[0][0]);
            Assert.AreEqual(arg[1][0], result[1][0]);

            byte[][] result2 = m_testService.ExtractFromOctetOfOctetSeqAny(arg);
            Assert.IsNotNull(result2);
            Assert.AreEqual(arg.Length, result2.Length);
            Assert.AreEqual(arg[0].Length, result2[0].Length);
            Assert.AreEqual(arg[1].Length, result2[1].Length);
            Assert.AreEqual(arg[0][0], result2[0][0]);
            Assert.AreEqual(arg[1][0], result2[1][0]);
        }

        [Test]
        public void TestPassingSimpleAsAny() {
            int arg = 21;
            int result = (int)m_testService.EchoAny(arg);
            Assert.AreEqual(arg, result);
        }
        
        [Test]
        public void TestRecursiveAny() {
            TestUnionE3 arg = new TestUnionE3();
            TestEnumForU3 case0Val = TestEnumForU3.A3;
            arg.SetvalE0(case0Val);
            TestUnionE3 result = (TestUnionE3)m_testService.EchoAny(arg);
            Assert.AreEqual(case0Val, result.GetvalE0());
            Assert.AreEqual(TestEnumForU3.A3, result.Discriminator);            
        }

        [Test]
        public void TestReceivingUnknownUnionsAsAny() {
            object result = m_testService.RetrieveUnknownUnion();
            Assert.IsNotNull(result, "union not retrieved");
            Assert.AreEqual("TestUnionE2", result.GetType().FullName, "type name");
        }
        
        [Test]
        public void TestTypeDefInAny() {            
            int memberElem = 5;
            object result = 
                m_testService.RetrieveStructWithTypedefMember(memberElem);
            Assert.IsNotNull(result, "test struct with typedef null");
            Assert.AreEqual(typeof(StructWithTypedefMember), result.GetType());            
            Assert.AreEqual(memberElem, ((StructWithTypedefMember)result).longtdField);
            // test to receive a typedefed type directly
            int nrOfElems = 1;
            object result2 = m_testService.RetrieveTypedefedSeq(nrOfElems, memberElem);
            Assert.IsNotNull(result2, "typedefed seq null");
            Assert.AreEqual(typeof(int[]), result2.GetType());
            Assert.AreEqual(nrOfElems, ((int[])result2).Length);
            Assert.AreEqual(memberElem, ((int[])result2)[0]);
        }

        [Test]
        public void TestWStringSeq() {
            string elem = "seqElem";
            int nrOfElems = 5;

            string[] retrieved = m_testService.RetrieveWstringSeq(elem, nrOfElems);
            Assert.IsNotNull(retrieved, "wstring seq not retrieved");            
            Assert.AreEqual(nrOfElems, retrieved.Length);
            for (int i = 0; i < retrieved.Length; i++) {
                Assert.AreEqual(elem, retrieved[i], "array element i:" + i + " not ok; " + retrieved[i]);            
            }

            string[] arg = new string[] { "Nr1", "Nr2", "Nr3" };
            string[] result = m_testService.EchoWstringSeq(arg);
            Assert.IsNotNull(result, "wstring seq not retrieved");
            Assert.AreEqual(arg.Length, result.Length);
            for (int i = 0; i < arg.Length; i++) {
                Assert.AreEqual(arg[i], result[i], "array element i:" + i + " not ok; " + result[i]);
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
            Assert.IsNotNull(result);
            Assert.AreEqual(arg.Length, result.Length);
            for (int i = 0; i < nrOfOuterElems; i++) {
                Assert.IsNotNull(result[i]);
                Assert.AreEqual(arg[i].Length, result[i].Length);
                for (int j = 0; j < nrOfInnerElems; j++) {
                    Assert.AreEqual(arg[i][j], result[i][j]);
                }
            }
            
        }

        [Test]
        public void TestBoundedSeq() {
            int[] lengthOk = new int[] { 1, 2, 3 };
            int[] result = m_testService.EchoBoundedSeq(lengthOk);
            Assert.IsNotNull(result);
            Assert.AreEqual(lengthOk.Length, result.Length);
            for (int i = 0; i < result.Length; i++) {
                Assert.AreEqual(lengthOk[i], result[i]);
            }
            
            int[] tooLong = new int[] { 1, 2, 3, 4 };
            try {
                m_testService.EchoBoundedSeq(tooLong);
                Assert.Fail("expected BAD_PARAM exception, because sequence too long, but not thrown");
            } catch (BAD_PARAM badParamE) {
                Assert.AreEqual(badParamE.Minor, 3434);
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
            Assert.IsNotNull(result);
            Assert.AreEqual(arg.Length, result.Length);
            for (int i = 0; i < result.Length; i++) {
                Assert.AreEqual(arg[i].Length, result[i].Length);
                for (int j = 0; j < result[i].Length; j++) {
                    Assert.AreEqual(arg[i][j], result[i][j]);
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
                Assert.Fail("expected BAD_PARAM exception, because sequence too long, but not thrown");
            } catch(BAD_PARAM badParamE) {
                Assert.AreEqual(badParamE.Minor, 3434);
            }
        }

        [Test]
        public void TestRetrieveSeqOfBoundedSeqAsAny() {
            int outerLength = 20;
            int innerLength = 4;
            byte elemVal = 2;
            byte[][] result = (byte[][])m_testService.RetrieveUuidAsAny(outerLength, innerLength, elemVal);
            Assert.IsNotNull(result);
            Assert.AreEqual(outerLength, result.Length, "wrong nr of Uuid elements");
            for (int i = 0; i < result.Length; i++) {
                Assert.AreEqual(innerLength, result[i].Length);
                for (int j = 0; j < result[i].Length; j++) {
                    Assert.AreEqual(elemVal, result[i][j]);
                }
            }
        }

        [Test]
        public void TestNestedStructTypes() {
            TestService_package.InnerStruct arg = new TestService_package.InnerStruct();
            arg.Field1 = 21;
            TestService_package.InnerStruct result = m_testService.EchoInnerStruct(arg);
            Assert.IsNotNull(result);
            Assert.AreEqual(arg.Field1, result.Field1);
        }

        [Test]
        public void TestRecStruct() {
            RecStruct arg = new RecStruct();
            arg.seq = new RecStruct[1];
            arg.seq[0].seq = new RecStruct[0]; // a null sequence is not allowed
            RecStruct result = m_testService.EchoRecStruct(arg);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.seq);
            Assert.AreEqual(arg.seq.Length, result.seq.Length);
            Assert.IsNotNull(result.seq[0]);
        }

        [Test]
        public void TestStructConstructors() {
            SimpleStruct st1 = new SimpleStruct();
            st1.code = 1;
            st1.msg = "msg";
            st1.sType = SimpleEnum.S2;
            SimpleStruct res1 = m_testService.EchoSimpleStruct(st1);
            Assert.AreEqual(st1.code, res1.code, "st1.code");
            Assert.AreEqual(st1.msg, res1.msg, "st1.msg");
            Assert.AreEqual(st1.sType, res1.sType, "st1.sType");
            SimpleStruct st2 = new SimpleStruct(2, "msg2", SimpleEnum.S3);
            SimpleStruct res2 = m_testService.EchoSimpleStruct(st2);
            Assert.AreEqual(st2.code, res2.code, "st2.code");
            Assert.AreEqual(st2.msg, res2.msg, "st2.msg");
            Assert.AreEqual(st2.sType, res2.sType, "st2.sType");            
        }


        [Test]
        public void TestWChar() {
            char arg = 'a';
            char result = m_testService.EchoWChar(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestWString() {
            string arg = "test";
            string result = m_testService.EchoWString(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestString() {
            string arg = "test";
            string result = m_testService.EchoString(arg);
            Assert.AreEqual(arg, result);
        }
        
        [Test]
        public void TestIdlKeyWordPropertyNames() {
            int[] argSeq = new int[] { 1, 2, 3 };
            m_testService._sequence = argSeq;
            int[] resultSeq = m_testService._sequence;
            Assert.IsNotNull(resultSeq, "property int seq null");
            Assert.AreEqual(argSeq.Length, resultSeq.Length);
            for (int i = 0; i < argSeq.Length; i++) {
                Assert.AreEqual(argSeq[i], resultSeq[i], "wrong seq entry");
            }            
        }
        
        [Test]
        public void TestIdlKeyWordMethodNames() {
            byte arg = 39;
            byte result = m_testService._octet(arg);
            Assert.AreEqual(arg, result, "wrong result octet-call");
        }

        [Test]
        public void TestULongAsAny() {
            int arg = 74;
            int result = (int) m_testService.RetrieveULongAsAny(arg);
            Assert.AreEqual(arg, result, "wrong result of retrieveULongAsAny");
            
            OrbServices orb = OrbServices.GetSingleton();
            int arg2 = 89;
            omg.org.CORBA.TypeCode ulongTC = orb.create_ulong_tc();
            Any any = new Any(arg2, ulongTC);
            int result2 = m_testService.ExtractFromULongAny(any);
            Assert.AreEqual(arg2, result2, "wrong result of ExtractFromULongAny");
        }

        [Test]
        public void TestLongTypeDefAsAny() {
            OrbServices orb = OrbServices.GetSingleton();
            int arg = 74;
            int result = (int)m_testService.RetrieveLongTypeDefAsAny(arg);
            Assert.AreEqual(arg, result, "result of RetrieveLongTypeDefAsAny");

            int arg2 = 91;
            omg.org.CORBA.TypeCode argTC = orb.create_tc_for(arg2);
            omg.org.CORBA.TypeCode longTD_TC = orb.create_alias_tc("IDL:longTD:1.0", "longTD", argTC);
            Any any = new Any(arg2, longTD_TC);
            int result2 = m_testService.ExtractFromLongTypeDef(any);
            Assert.AreEqual(arg2, result2, "result of ExtractFromLongTypeDef");
        }

        [Test]
        public void TestNullAsAny() {
            object result = m_testService.EchoAny(null);
            Assert.IsNull(result, "result not null");
        }

        [Test]
        public void TestNilReferenceAsAny() {
            OrbServices orb = OrbServices.GetSingleton();
            omg.org.CORBA.TypeCode nilRefTC = orb.create_tc_for_type(typeof(System.MarshalByRefObject));
            Any nilRefAny = new Any(null, nilRefTC);
            object result = m_testService.EchoAny(nilRefAny);
            Assert.IsNull(result, "result not null");

            Any nilRefAny2 = new Any(null, orb.create_interface_tc(String.Empty, String.Empty));
            object result2 = m_testService.EchoAny(nilRefAny2);
            Assert.IsNull(result2, "result not null");
        }

        [Test]
        public void TestNameserviceList() {
            NamingContext nameService = GetNameService();

            Binding[] bindings;
            BindingIterator bindingIterator;
            nameService.list(10, out bindings, out bindingIterator);
            Assert.IsTrue((bindings.Length > 0), "nr of bindings too small");

            bool found  = false;
            foreach (Binding binding in bindings) {
                NameComponent[] name = binding.binding_name;                
                if ((name.Length > 0) && (name[0].id.Equals("test"))) {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "service not found");
        }

        [Test]
        public void TestOneDimIdlIntArray() {
            int[] arg = new int[] { 1, 2, 3, 4, 5 };
            int[] result = m_testService.EchoIntList5(arg);
            Assert.AreEqual(arg.Length, result.Length);
            for (int i = 0; i < arg.Length; i++) {
                Assert.AreEqual(arg[i], result[i]);
            }            
        }

        [Test]
        public void TestOneDimIdlStringArray() {
            string[] arg = new string[] { "1", "2", "3", "4", "5" };
            string[] result = m_testService.EchoStringList5(arg);
            Assert.AreEqual(arg.Length, result.Length);
            for (int i = 0; i < arg.Length; i++) {
                Assert.AreEqual(arg[i], result[i]);
            }            
        }

        [Test]
        public void TestTwoDimIdlIntArray() {
            int[,] arg = new int[,] { {1, 2}, {3, 4} };
            int[,] result = m_testService.EchoInt2Dim2x2(arg);
            Assert.AreEqual(arg.GetLength(0), result.GetLength(0));
            Assert.AreEqual(arg.GetLength(1), result.GetLength(1));
            for (int i = 0; i < arg.GetLength(0); i++) {
                for (int j = 0; j < arg.GetLength(1); j++) {
                    Assert.AreEqual(arg[i,j], result[i,j]);
                }
            }            
        }

        [Test]
        public void TestStructContainingArray() {
            BlobData arg = new BlobData();
            arg.ident = 1;
            arg.data = new int[] { 1, 2, 3 };
            BlobData result = m_testService.EchoBlobData(arg);    
            Assert.AreEqual(arg.ident, result.ident);
            Assert.AreEqual(arg.data.Length, result.data.Length);
            for (int i = 0; i < arg.data.Length; i++) {
                Assert.AreEqual(arg.data[i], result.data[i]);
            }            
        }

        [Test]
        public void TestInnerStructAsAny() {
            TestService_package.InnerStruct arg = new TestService_package.InnerStruct(1);
            TestService_package.InnerStruct result = 
                (TestService_package.InnerStruct)m_testService.EchoAny(arg);
            Assert.AreEqual(arg.Field1, result.Field1, "arg.Field1");

            TestService_package.InnerStruct result2 = 
                (TestService_package.InnerStruct)m_testService.RetrieveInnerStructAsAny(arg);
            Assert.AreEqual(arg.Field1, result2.Field1, "arg.Field1");

            TestService_package._Event argEvent = new TestService_package._Event(1);
            TestService_package._Event resultEvent = 
                (TestService_package._Event)m_testService.EchoAny(argEvent);
            Assert.AreEqual(argEvent.EventId, resultEvent.EventId, "argEvent.EventId");

            TestService_package._Event resultEvent2 = 
                (TestService_package._Event)m_testService.RetrieveEventAsAny(argEvent);
            Assert.AreEqual(argEvent.EventId, resultEvent2.EventId, "argEvent.EventId");            
        }
        
        [Test]
        public void TestMBRTypesWithReservedNameCollisions() {
            CCE._Assembly asm = m_testService.CreateAsm();
            Assert.IsNotNull(asm, "asm not created");

/*            CCE.N_Assembly _asm = m_testService.Create_Asm();
            Assert.IsNotNull("_asm not created", _asm); */
        }

        [Test]
        public void TestIsAServiceWithServerSideIf() {
            // most specific interface of the remote object is known on 
            // the server side only
            Assert.IsTrue(
                      m_orb.is_a(m_testServiceInternalIf,
                                 "IDL:TestSimpleServicePublic:1.0"), "wrong public type info");

            Assert.IsTrue(
                      m_orb.is_a(m_testServiceInternalIf,
                                 "IDL:TestSimpleServicePublic:1.0"), "wrong server only type info");

        }

        [Test]
        public void TestCallServiceWithServerSideIf() {
            // most specific interface of the remote object is known on 
            // the server side only
            int arg = 1;
            int result = 
                m_testServiceInternalIf.EchoLong(arg);
            Assert.AreEqual(arg, result);
        }

        [Test]
        public void TestMostSpecTypeSrvWithServerSideIf() {
            string objToString = 
                m_orb.object_to_string(m_testServiceInternalIf);
            Ch.Elca.Iiop.CorbaObjRef.Ior forObj = new 
                Ch.Elca.Iiop.CorbaObjRef.Ior(objToString);
            Assert.AreEqual("IDL:Internal/TestSimpleServiceInternal:1.0",
                                   forObj.TypID);
        }

    }

}