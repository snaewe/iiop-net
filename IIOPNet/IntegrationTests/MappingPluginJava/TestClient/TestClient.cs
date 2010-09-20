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
using System.Collections;
using System.IO;
using NUnit.Framework;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Idl;
using omg.org.CosNaming;

namespace Ch.Elca.Iiop.IntegrationTests.MappingPlugin {

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
            ChannelServices.RegisterChannel(m_channel, false);

            // access COS nameing service
            CorbaInit init = CorbaInit.GetInit();
            NamingContext nameService = init.GetNameService("localhost", 1050);
            NameComponent[] name = new NameComponent[] { new NameComponent("testPlugin", "") };
            // get the reference to the test-service
            m_testService = (TestService)nameService.resolve(name);
            
            try {
               CustomMapperRegistry reg = CustomMapperRegistry.GetSingleton();
               reg.AddMappingsFromFile(new FileInfo("customMapping.xml"));
               reg.AddMappingsFromFile(new FileInfo("customMappingTest.xml"));
            } catch (Exception e) {
                Console.WriteLine("custom mapper not loadable: " + e);
                throw e;
            }
        }

        [TearDown]
        public void TearDownEnvironment() {
            m_testService = null;
            // unregister the channel            
            ChannelServices.UnregisterChannel(m_channel);
        }

        private void CheckArrayListElems(ArrayList resultList, object expectedValues, int expectedNrOfElems) {
            Assertion.AssertEquals(expectedNrOfElems, resultList.Count);
            for (int i = 0; i < expectedNrOfElems; i++) {
                Assertion.AssertEquals(expectedValues, resultList[i]);
            }
        }

        private void CheckHashtableElems(Hashtable resultTable, object expectedValues, int expectedNrOfElems) {
            Assertion.AssertEquals(expectedNrOfElems, resultTable.Count);
            for (int i = 0; i < expectedNrOfElems; i++) {
                if (!resultTable.ContainsKey(i)) {
                    Assertion.Fail("key missing in Hashtable:" + i);
                }
                Assertion.AssertEquals("wrong value " + resultTable[i] + " for key: " + i, expectedValues, resultTable[i]);
            }
        }


        [Test]
        public void TestDoubleArrayList() {
            double val = 2.3;
            int nrOfElems = 5;
            ArrayList result = m_testService.createDoubleList(val, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
            result.Add(val + 1);
            ArrayList result3 = m_testService.echoList(result);
            Assertion.AssertEquals(val + 1, result3[result3.Count - 1]);
        }

        [Test]
        public void TestFloatArrayList() {
            float val = 3.3F;
            int nrOfElems = 5;
            ArrayList result = m_testService.createFloatList(val, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
        }
        
        [Test]
        public void TestByteArrayList() {
            System.Byte val = 4;
            int nrOfElems = 4;
            ArrayList result = m_testService.createByteList(val, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestInt16ArrayList() {
            System.Int16 val = 8;
            int nrOfElems = 4;
            ArrayList result = m_testService.createShortList(val, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestInt32ArrayList() {
            System.Int32 val = 82997;
            int nrOfElems = 4;
            ArrayList result = m_testService.createIntList(val, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
        }
        
        [Test]
        public void TestInt64ArrayList() {
            System.Int64 val = 782997;
            int nrOfElems = 4;
            ArrayList result = m_testService.createLongList(val, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestBooleanArrayList() {
            System.Boolean val = true;
            int nrOfElems = 4;
            ArrayList result = m_testService.createBooleanList(val, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestEmptyArrayList() {
            ArrayList arg = new ArrayList();
            ArrayList result = m_testService.echoList(arg);
            Assertion.AssertEquals(0, result.Count);
        }
        
        [Test]
        public void TestCharArrayList() {
            System.Char val = 'a';
            int nrOfElems = 4;
            ArrayList result = m_testService.createCharList(val, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
        }
        
        [Test]
        public void TestByRefArrayList() {
            int nrOfElems = 4;
            ArrayList result = m_testService.createByRefTypeList(nrOfElems);
            Assertion.AssertEquals(nrOfElems, result.Count);
            for (int i = 0; i < nrOfElems; i++) {
                Assertion.AssertEquals(true, result[i].GetType().IsMarshalByRef);
                Assertion.AssertEquals(true, RemotingServices.IsTransparentProxy(result[i]));
            }
            ArrayList result2 = m_testService.echoList(result);
            Assertion.AssertEquals(nrOfElems, result2.Count);
            for (int i = 0; i < nrOfElems; i++) {
                Assertion.AssertEquals(true, result2[i].GetType().IsMarshalByRef);
                Assertion.AssertEquals(true, RemotingServices.IsTransparentProxy(result2[i]));
            }
        }

        [Test]
        public void TestValTypeArrayList() {
            string msg = "msg";
            int nrOfElems = 10;
            TestSerializableClassB1Impl val = new TestSerializableClassB1Impl();
            val.Msg = msg;
            ArrayList result = m_testService.createValTypeList(msg, nrOfElems);
            CheckArrayListElems(result, val, nrOfElems);
            result.Add(val);
            ArrayList result2 = m_testService.echoList(result);
            CheckArrayListElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestEmptyHashMap() {
            Hashtable arg = new Hashtable();
            Hashtable result = m_testService.echoHashMap(arg);
            Assertion.AssertEquals(0, result.Count);
        }

        [Test]
        public void TestHashMapWithDoubleVals() {
            double val = 2.3;
            int nrOfElems = 25;
            Hashtable result = m_testService.createHashMapWithDoubleVals(val, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
            result[nrOfElems + 1] = val + 1;
            Hashtable result3 = m_testService.echoHashMap(result);
            Assertion.AssertEquals(val + 1, result3[nrOfElems + 1]);
        }

        [Test]
        public void TestHashMapWithFloatVals() {
            float val = 3.3F;
            int nrOfElems = 1;
            Hashtable result = m_testService.createHashMapWithFloatVals(val, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestHashMapWithByteVals() {
            System.Byte val = 4;
            int nrOfElems = 4;
            Hashtable result = m_testService.createHashMapWithByteVals(val, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
        }


        [Test]
        public void TestHashMapWithInt16Vals() {
            System.Int16 val = 8;
            int nrOfElems = 4;
            Hashtable result = m_testService.createHashMapWithShortVals(val, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestHashMapWithInt32Vals() {
            System.Int32 val = 82997;
            int nrOfElems = 4;
            Hashtable result = m_testService.createHashMapWithIntVals(val, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
        }
        
        [Test]
        public void TestHashMapWithInt64Vals() {
            System.Int64 val = 782997;
            int nrOfElems = 4;
            Hashtable result = m_testService.createHashMapWithLongVals(val, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestHashMapWithBooleanVals() {
            System.Boolean val = true;
            int nrOfElems = 24;
            Hashtable result = m_testService.createHashMapWithBooleanVals(val, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
        }


        [Test]
        public void TestHashMapWithCharVals() {
            System.Char val = 'a';
            int nrOfElems = 40;
            Hashtable result = m_testService.createHashMapWithCharVals(val, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
        }


        [Test]
        public void TestHashMapWithByRefVals() {
            int nrOfElems = 4;
            Hashtable result = m_testService.createHashMapWithByRefVals(nrOfElems);
            Assertion.AssertEquals(nrOfElems, result.Count);
            for (int i = 0; i < nrOfElems; i++) {
                Assertion.AssertEquals(true, result[i].GetType().IsMarshalByRef);
                Assertion.AssertEquals(true, RemotingServices.IsTransparentProxy(result[i]));
            }
            Hashtable result2 = m_testService.echoHashMap(result);
            Assertion.AssertEquals(nrOfElems, result2.Count);
            for (int i = 0; i < nrOfElems; i++) {
                Assertion.AssertEquals(true, result2[i].GetType().IsMarshalByRef);
                Assertion.AssertEquals(true, RemotingServices.IsTransparentProxy(result2[i]));
            }
        }

        [Test]
        public void TestHashMapWithValTypeVals() {
            string msg = "msg";
            int nrOfElems = 10;
            TestSerializableClassB1Impl val = new TestSerializableClassB1Impl();
            val.Msg = msg;
            Hashtable result = m_testService.createHashMapWithValTypeVals(msg, nrOfElems);
            CheckHashtableElems(result, val, nrOfElems);
            result[nrOfElems] = val;
            Hashtable result2 = m_testService.echoHashMap(result);
            CheckHashtableElems(result2, val, nrOfElems + 1);
        }

        [Test]
        public void TestDateTime() {
            DateTime arg = DateTime.Now;
            DateTime result = m_testService.echoDate(arg);
            // check date equality up to the milliseconds
            Assertion.AssertEquals("current date (year) not echoed correctly", arg.Year, result.Year);
            Assertion.AssertEquals("current date (month) not echoed correctly", arg.Month, result.Month);
            Assertion.AssertEquals("current date (day) not echoed correctly", arg.Day, result.Day);
            Assertion.AssertEquals("current date (hour) not echoed correctly", arg.Hour, result.Hour);
            Assertion.AssertEquals("current date (minutes) not echoed correctly", arg.Minute, result.Minute);
            Assertion.AssertEquals("current date (seconds) not echoed correctly", arg.Second, result.Second);
            Assertion.AssertEquals("current date (milliseconds) not echoed correctly", arg.Millisecond, result.Millisecond);            
        }

        [Test]
        public void TestGetDateTime() {
            DateTime now = DateTime.Now;
            DateTime result = m_testService.receiveCurrentDate();
            // check date difference is less than two minutes
            Assertion.Assert("date difference too big", (result - now).TotalSeconds < 120);
        }
        
        [Test]
        public void TestCustomMappedSerializable() {
            CustomMappedSerializableCls arg = new CustomMappedSerializableCls("Test");
            CustomMappedSerializableCls result = m_testService.echoCustomMappedSer(arg);
            Assertion.AssertNotNull(result);
            Assertion.AssertEquals(arg.Message, result.Message);
        }        

        
    }


}
