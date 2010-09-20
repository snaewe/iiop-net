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
using System.Collections;
using NUnit.Framework;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;
using omg.org.CORBA;


public class CallBackImpl : MarshalByRefObject, CallBack {

    private string m_msg;

    public string Msg {
        get {
            return m_msg;
        }
    }

    public void call_back(string mesg) {
        m_msg = mesg;
    }    

}

 

public class CallbackIntIncrementerImpl : MarshalByRefObject, CallbackIntIncrementer {

    public int TestIncInt32(int arg) {
        return arg + 1;
    }

    public override object InitializeLifetimeService() {
        // live forever
        return null;
    }

}



namespace Ch.Elca.Iiop.IntegrationTests {

    [TestFixture]
    public class TestClient {

        #region IFields

        private IiopChannel m_channel;

        private TestService m_testService;


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

            NamingContext nameService = GetNameService();
            NameComponent[] name = new NameComponent[] { new NameComponent("test", "") };
            // get the reference to the test-service
            m_testService = (TestService)nameService.resolve(name);
        }

        [TearDown]
        public void TearDownEnvironment() {
            m_testService = null;
            // unregister the channel, otherwise, get some problem with server channel data.       
            ChannelServices.UnregisterChannel(m_channel);
        }

        [Test]
        public void Test() {
            // to make sure, to use same channel, do everything in one test-method
            
            // without callback
            byte arg1 = 1;
            byte result1 = m_testService.TestIncByte(arg1);
            Assertion.AssertEquals("wrong result 1", ((System.Byte)arg1 + 1), result1);

            // for the following callbacks, should use already existing connection for callback (bidir)
            string arg2 = "test";
            CallBackImpl echoImpl = new CallBackImpl();
            m_testService.string_callback(echoImpl, arg2);
            Assertion.AssertEquals("wrong result 2", arg2, echoImpl.Msg);

            int arg3 = 3;
            CallbackIntIncrementerImpl incImpl = new CallbackIntIncrementerImpl();
            m_testService.RegisterCallbackIntIncrementer(incImpl);
            int result3 = m_testService.IncrementWithCallbackIncrementer(arg3);
            Assertion.AssertEquals("wrong result 3", arg3 + 1, result3);
            
        }

    }

}