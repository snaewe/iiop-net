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

        private IiopChannel m_channel;

        private TestService m_testService;

        #endregion IFields
        #region IMethods

        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            if (m_channel == null) {
                // the remote proxy for this url is bound to a certain sink chain for some time ->
                // don't recreate channel; otherwise, is no more bidirectional for another run.
                IDictionary props = new Hashtable();
                props[IiopServerChannel.PORT_KEY] = 0;
                props[IiopChannel.BIDIR_KEY] = true;
                m_channel = new IiopChannel(props);
            }
            ChannelServices.RegisterChannel(m_channel, false);
            // get the reference to the test-service
            m_testService = (TestService)RemotingServices.Connect(typeof(TestService), "corbaloc:iiop:1.2@localhost:8087/test");
        }

        [TearDown]
        public void TearDownEnvironment() {
            m_testService = null;
            ChannelServices.UnregisterChannel(m_channel);
        }
        
        [Test]
        public void Test() {
            System.Byte arg = 1;
            System.Byte result = m_testService.TestIncByte(arg);
            Assert.AreEqual((System.Byte)(arg + 1), result);

            CallbackIntIncrementerImpl callback = new CallbackIntIncrementerImpl();
            m_testService.RegisterCallbackIntIncrementer(callback);
            
            System.Int32 arg1 = 1;
            System.Int32 result1 = m_testService.IncrementWithCallbackIncrementer(arg1);
            Assert.AreEqual((System.Int32)(arg1 + 1), result1);                
        }

        #endregion IMethods


    }


    public class CallbackIntIncrementerImpl : MarshalByRefObject, CallbackIntIncrementer {

        public System.Int32 TestIncInt32(System.Int32 arg) {
            return arg + 1;
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }        

    }

}
