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
using Ch.Elca.Iiop.Interception;
using omg.org.CosNaming;
using omg.org.CORBA;
using omg.org.PortableInterceptor;

namespace Ch.Elca.Iiop.IntegrationTests {


    [TestFixture]
    public class TestClient {

        #region IFields

        private IiopClientChannel m_channel;

        private TestService m_testService;
        private TestInterceptorControlService m_interceptorControl;

        private TestInterceptorInit m_testInterceptorInit;        

        #endregion IFields
        #region IMethods


        private void RegisterInterceptors() {
            IOrbServices orb = OrbServices.GetSingleton();
            m_testInterceptorInit = new TestInterceptorInit();
            orb.RegisterPortableInterceptorInitalizer(m_testInterceptorInit);
            orb.CompleteInterceptorRegistration();
        }


        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            if (m_channel == null) {
                m_channel = new IiopClientChannel();
                ChannelServices.RegisterChannel(m_channel, false);

                RegisterInterceptors();

                // get the reference to the test-service
                m_testService = (TestService)RemotingServices.Connect(typeof(TestService), "corbaloc:iiop:1.2@localhost:8087/test");

                m_interceptorControl = (TestInterceptorControlService)RemotingServices.Connect(typeof(TestInterceptorControlService),
                                                                                               "corbaloc:iiop:1.2@localhost:8087/interceptorControl");
            }
        }

        [TearDown]
        public void TearDownEnvironment() {
        }
        
        [Test]
        public void TestContextNoException() {
            try {
                int contextEntryVal = 4;
                m_testInterceptorInit.RequestIntercept.ContextEntryBegin = contextEntryVal;

                System.Byte arg = 1;
                System.Byte result = m_testService.TestIncByte(arg);
                Assertion.AssertEquals((System.Byte)(arg + 1), result);

                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.RequestIntercept.InvokedOnOutPath);
                
                Assertion.AssertEquals("a on in path called (reply)", 
                                       InPathResult.Reply, m_testInterceptorInit.RequestIntercept.InPathResult);

                Assertion.Assert("expected server side: rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled());

                Assertion.Assert("expected server side: rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled());

                Assertion.AssertEquals("expected server side: send on out path called (reply)", 
                                       OutPathResult.OutPathResult_Reply, 
                                       m_interceptorControl.GetOutPathResult());

                Assertion.Assert("service context not present", m_testInterceptorInit.RequestIntercept.HasReceivedContextElement);
                Assertion.AssertEquals("service context content", contextEntryVal,
                                       m_testInterceptorInit.RequestIntercept.ContextElement.TestEntry);              

            } finally {
                m_testInterceptorInit.RequestIntercept.ClearInvocationHistory();
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }            
        }

        [Test]
        public void TestTaggedComponentAvailableNoException() {
            try {
                // the server side includes the component into the ior; ior interceptor is on server side ->
                // for this test, can't use an ior created from a corbaloc on client side.
                TestService serverSideRef = m_testService.GetReferenceToThis();                

                System.Byte arg = 1;
                System.Byte result = serverSideRef.TestIncByte(arg);
                Assertion.AssertEquals((System.Byte)(arg + 1), result);

                Assertion.Assert("tagged component", m_testInterceptorInit.RequestIntercept.HasTaggedComponet);
                Assertion.AssertEquals("tagged component value", 1, 
                                       m_testInterceptorInit.RequestIntercept.TaggedComponent.TestEntry);
            } finally {
                m_testInterceptorInit.RequestIntercept.ClearInvocationHistory();
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }            
        }


        #endregion IMethods

    }

}
