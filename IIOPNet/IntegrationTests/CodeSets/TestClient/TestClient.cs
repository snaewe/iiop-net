/* TestClient.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 18.09.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using omg.org.CORBA;
using Ch.Elca.Iiop.CorbaObjRef;

namespace Ch.Elca.Iiop.IntegrationTests {


    [TestFixture]
    public class TestClient {

        #region IFields

        private IiopClientChannel m_channel;

        private TestService m_testService;

        private TestService m_newTestService;

        #endregion IFields
        #region IMethods

        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            IDictionary properties = new Hashtable();
            properties[IiopClientChannel.ALLOW_REQUEST_MULTIPLEX_KEY] = false;
            m_channel = new IiopClientChannel(properties);
            ChannelServices.RegisterChannel(m_channel, false);

            // get the reference to the test-service
            m_testService = (TestService)RemotingServices.Connect(typeof(TestService), "corbaloc:iiop:1.2@localhost:8087/test");

            m_newTestService = m_testService.ReturnNewTestService();
        }

        [TearDown]
        public void TearDownEnvironment() {
            m_testService = null;
            // unregister the channel
            ChannelServices.UnregisterChannel(m_channel);
        }

        [Test]
        public void TestChar() {
            System.Char arg = 'a';
            System.Char result = m_testService.TestEchoChar(arg);
            Assert.AreEqual(arg, result);
            arg = '0';
            result = m_testService.TestEchoChar(arg);
            Assert.AreEqual(arg, result);

            arg = '0';
            result = m_newTestService.TestEchoChar(arg);
            Assert.AreEqual(arg, result);
        }
        
        [Test]
        public void TestString() {
            System.String arg = "test";
            System.String toAppend = "toAppend";
            System.String result = m_testService.TestAppendString(arg, toAppend);
            Assert.AreEqual(arg + toAppend, result);

            System.String result2 = m_newTestService.TestAppendString(arg, toAppend);
            Assert.AreEqual(arg + toAppend, result2);
        }       

        [Test]
        public void TestWChar() {
            System.Char arg = 'a';
            System.Char result = m_testService.TestEchoWChar(arg);
            Assert.AreEqual(arg, result);
            arg = '0';
            result = m_testService.TestEchoWChar(arg);
            Assert.AreEqual(arg, result);

            arg = '0';
            result = m_newTestService.TestEchoWChar(arg);
            Assert.AreEqual(arg, result);
        }
        
        [Test]
        public void TestWString() {
            System.String arg = "test";
            System.String toAppend = "toAppend";
            System.String result = m_testService.TestAppendWString(arg, toAppend);
            Assert.AreEqual(arg + toAppend, result);

            System.String result2 = m_newTestService.TestAppendWString(arg, toAppend);
            Assert.AreEqual(arg + toAppend, result2);
        }       

        [Test]
        public void TestCodeSetComponent() {
            IOrbServices orb = OrbServices.GetSingleton();
            string iorString = orb.object_to_string(m_newTestService);
            Ior ior = new Ior(iorString);
            Assert.IsTrue(ior.Profiles.Length > 0, "nr of profiles");
            IIorProfile profile = ior.Profiles[0];
            omg.org.IOP.CodecFactory codecFactory = (omg.org.IOP.CodecFactory)
                orb.resolve_initial_references("CodecFactory");
            object codeset = 
                profile.TaggedComponents.GetComponentData(1, codecFactory.create_codec(new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal, 1, 2)),
                                                          CodeSetComponentData.TypeCode);
            Assert.NotNull(codeset);
            Assert.AreEqual((int)CharSet.UTF8, ((CodeSetComponentData)codeset).NativeCharSet);
            Assert.AreEqual((int)WCharSet.UTF16, ((CodeSetComponentData)codeset).NativeWCharSet);            
        }


        #endregion IMethods


    }

}
