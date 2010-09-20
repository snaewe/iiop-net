/* TestClient.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 04.06.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopClientChannel();
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
        public void TestAnyContainer() {
            System.String testString = "abcd";
            OrbServices orb = OrbServices.GetSingleton();
            omg.org.CORBA.TypeCode wstringTc = orb.create_wstring_tc(0);
            Any any = new Any(testString, wstringTc);
            Any result = m_testService.EchoAny(any);
            Assertion.AssertEquals(any.Value, result.Value);
        }

        #endregion IMethods


    }

}
