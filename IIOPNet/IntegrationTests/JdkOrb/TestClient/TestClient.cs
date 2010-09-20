/* TestClient.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 25.03.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
        public void TestInt32() {
            System.Int32 arg = 1;
            System.Int32 result = m_testService.EchoLong(arg);
            Assertion.AssertEquals(arg, result);
        }

        [Test]
        public void TestStringArrayAsAny() {
            string arg1 = "s1";
            string arg2 = "s2";
            object result = m_testService.StringArrayAsAny(arg1, arg2);
            Assertion.AssertNotNull(result);
            string[] resAsStrArray = (string[])result;
            Assertion.AssertEquals(2, resAsStrArray.Length);
            Assertion.AssertEquals(arg1, resAsStrArray[0]);
            Assertion.AssertEquals(arg2, resAsStrArray[1]);
        }



        
    }

}