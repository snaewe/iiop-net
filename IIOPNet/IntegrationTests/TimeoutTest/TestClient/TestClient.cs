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

        #endregion IFields
        #region IMethods

        private NamingContext GetNameService() {
            // access COS nameing service
            CorbaInit init = CorbaInit.GetInit();
            NamingContext nameService = init.GetNameService("localhost", 8087);            
            return nameService;
        }

        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            IDictionary props = new Hashtable();
            props[IiopClientChannel.CLIENT_REQUEST_TIMEOUT_KEY] = 1000;
            m_channel = new IiopClientChannel(props);
            ChannelServices.RegisterChannel(m_channel, false);

            // get the reference to the test-service
            m_testService = (TestService)RemotingServices.Connect(typeof(TestService), "corbaloc:iiop:1.2@localhost:8087/test");
        }

        [TearDown]
        public void TearDownEnvironment() {
            m_testService = null;
            // unregister the channel            
            ChannelServices.UnregisterChannel(m_channel);
        }
        
        [Test]
        public void TestWithoutTimeout() {
            System.Byte arg = 1;
            System.Byte result = m_testService.TestIncByte(arg);
            Assert.AreEqual((System.Byte)(arg + 1), result);

            System.Byte arg2 = 3;
            System.Byte result2 = m_testService.TestIncByteWithSleep(arg2, 100);
            Assert.AreEqual((System.Byte)(arg2 + 1), result2);
        }

        [Test]
        [ExpectedException(typeof(TIMEOUT))]
        public void TestWithTimeout() {
            System.Byte arg = 1;

                System.Byte result = m_testService.TestIncByteWithSleep(arg, 20000);
                Assert.Fail("timeout excpetion not thrown");

        }


        #endregion IMethods


    }

}
