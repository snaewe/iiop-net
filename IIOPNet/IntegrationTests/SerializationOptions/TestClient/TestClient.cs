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

        private bool m_isConfigured; // = false;

        #endregion IFields
        #region IMethods

        [SetUp]
        public void SetupEnvironment() {
            if (!m_isConfigured) {
                OrbServices orb = OrbServices.GetSingleton();
                orb.SerializerFactoryConfig.StringSerializationAllowNull = true;
                orb.SerializerFactoryConfig.SequenceSerializationAllowNull = true;
                orb.SerializerFactoryConfig.ArraySerializationAllowNull = true;
                m_isConfigured = true;
            }
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
        public void TestEchoStringNotNull() {
            System.String testString = "abcd";
            System.String result = m_testService.EchoString(testString);
            Assertion.AssertEquals("result", testString, result);
        }

        [Test]
        public void TestEchoStringNull() {
            System.String testString = null;
            System.String result = m_testService.EchoString(testString);
            Assertion.AssertEquals("result", String.Empty, result);
        }

        [Test]
        public void TestEchoIntSeqNotNull() {
            int[] arg = new int[] { 1 };
            int[] result = m_testService.EchoSequence(arg);
            Assertion.AssertEquals("result length", arg.Length, result.Length);
            Assertion.AssertEquals("result element 0", arg[0], result[0]);
        }

        [Test]
        public void TestEchoIntSeqNull() {
            int[] arg = null;
            int[] result = m_testService.EchoSequence(arg);
            Assertion.AssertEquals("result length", 0, result.Length);
        }

        [Test]
        public void TestEchoArrayNotNull() {
            int[,] arg = new int[,] { {1, 2, 3}, { 4, 5, 6} };
            int[,] result = m_testService.EchoArray(arg);
            Assertion.AssertEquals("result 0 length", arg.GetLength(0), result.GetLength(0));
            Assertion.AssertEquals("result 1 length", arg.GetLength(1), result.GetLength(1));
            Assertion.AssertEquals("result element 0/0", arg[0, 0], result[0, 0]);
            Assertion.AssertEquals("result element 1/0", arg[1, 0], result[1, 0]);
            Assertion.AssertEquals("result element 0/1", arg[0, 1], result[0, 1]);
            Assertion.AssertEquals("result element 1/1", arg[1, 1], result[1, 1]);
            Assertion.AssertEquals("result element 0/2", arg[0, 2], result[0, 2]);
            Assertion.AssertEquals("result element 1/2", arg[1, 2], result[1, 2]);
        }

        [Test]
        public void TestEchoArrayNull() {
            int[,] arg = null;
            int[,] result = m_testService.EchoArray(arg);
            Assertion.AssertEquals("result 0 length", 2, result.GetLength(0));
            Assertion.AssertEquals("result 1 length", 3, result.GetLength(1));
            Assertion.AssertEquals("result element 0/0", 0, result[0, 0]);
            Assertion.AssertEquals("result element 1/0", 0, result[1, 0]);
            Assertion.AssertEquals("result element 0/1", 0, result[0, 1]);
            Assertion.AssertEquals("result element 1/1", 0, result[1, 1]);
            Assertion.AssertEquals("result element 0/2", 0, result[0, 2]);
            Assertion.AssertEquals("result element 1/2", 0, result[1, 2]);
        }

        #endregion IMethods


    }

}
