/* TestClient.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 11.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

        private TestFwd m_test;

        private NamingContext m_nameService;


        #endregion IFields

        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopClientChannel();
            ChannelServices.RegisterChannel(m_channel);

            // access COS nameing service
            string nameserviceLoc = "corbaloc::localhost:3528/JBoss/Naming/root";
            m_nameService = (NamingContext)RemotingServices.Connect(typeof(NamingContext), nameserviceLoc);

            NameComponent[] name = new NameComponent[] { new NameComponent("IntegrationTest", ""), 
                                                         new NameComponent("testForwarder", "") };
            // get the reference to the test-home
            TestHomeFwd testhome = (TestHomeFwd) m_nameService.resolve(name);
            m_test = testhome.create();
        }

        [TearDown]
        public void TearDownEnvironment() {
            if (m_test != null) {
                m_test.remove();
                m_test = null;
            }
            m_nameService = null;
            // unregister the channel            
            ChannelServices.UnregisterChannel(m_channel);
        }

        [Test]
        public void TestVoidForward() {
            m_test.TestVoid();
        }

        [Test]
        public void TestByteArrayInContainerForward() {
            System.Byte[] arg = new System.Byte[2];
            arg[0] = 1;
            arg[1] = 2;
            System.Byte[] result = m_test.TestFwdContainer(arg);
            Assertion.AssertEquals(2, result.Length);
            Assertion.AssertEquals((System.Byte) 1, result[0]);
            Assertion.AssertEquals((System.Byte) 2, result[1]);
        }

    }

}