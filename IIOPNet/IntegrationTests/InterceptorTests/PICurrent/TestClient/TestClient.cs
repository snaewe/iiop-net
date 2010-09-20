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

            }
        }

        [TearDown]
        public void TearDownEnvironment() {
        }
        
        [Test]
        public void TestSlotModifyInClientRecContextAndServer() {
            try {
                int slotId = m_testInterceptorInit.RequestIntercept.SlotId;
                ORB orb = OrbServices.GetSingleton();
                omg.org.PortableInterceptor.Current current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                int contextEntryVal = 4;
                current.set_slot(slotId, contextEntryVal);

                System.Int32 arg = 1;
                System.Int32 result = m_testService.TestAddToContextData(arg);
                Assertion.AssertEquals(arg + contextEntryVal, result);

                Assertion.Assert("service context not present", m_testInterceptorInit.RequestIntercept.HasReceivedContextElement);
                Assertion.AssertEquals("service context content", arg + contextEntryVal,
                                       m_testInterceptorInit.RequestIntercept.ContextElement.TestEntry);              

                current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                Assertion.AssertEquals("slot was modified", contextEntryVal, current.get_slot(slotId));      


            } finally {
                m_testInterceptorInit.RequestIntercept.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestSlotModifyInClientRecContextReceiveRCSAndServer() {
            // receive request modifies the request scope slots
            try {
                int slotId = m_testInterceptorInit.RequestIntercept.SlotId;
                ORB orb = OrbServices.GetSingleton();
                omg.org.PortableInterceptor.Current current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                int contextEntryVal = 4;
                current.set_slot(slotId, contextEntryVal);

                System.Int32 arg = 1;
                System.Int32 result = m_testService.TestReceiveReqNotChangeThreadScope(arg);
                Assertion.AssertEquals(arg + contextEntryVal, result);

                Assertion.Assert("service context not present", m_testInterceptorInit.RequestIntercept.HasReceivedContextElement);
                Assertion.AssertEquals("service context content", arg + contextEntryVal,
                                       m_testInterceptorInit.RequestIntercept.ContextElement.TestEntry);              

                current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                Assertion.AssertEquals("slot was modified", contextEntryVal, current.get_slot(slotId));      

            } finally {
                m_testInterceptorInit.RequestIntercept.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestSlotModifyInClientRecContextReceiveTCSAndServer() {
            // receive request modifies the thread scope slots
            try {
                int slotId = m_testInterceptorInit.RequestIntercept.SlotId;
                ORB orb = OrbServices.GetSingleton();
                omg.org.PortableInterceptor.Current current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                int contextEntryVal = 4;
                current.set_slot(slotId, contextEntryVal);

                System.Int32 arg = 1;
                System.Int32 result = m_testService.TestReceiveReqChangeThreadScope(arg);
                Assertion.AssertEquals(arg + (3*contextEntryVal), result);

                Assertion.Assert("service context not present", m_testInterceptorInit.RequestIntercept.HasReceivedContextElement);
                Assertion.AssertEquals("service context content", arg + (3*contextEntryVal),
                                       m_testInterceptorInit.RequestIntercept.ContextElement.TestEntry);        

                current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                Assertion.AssertEquals("slot was modified", contextEntryVal, current.get_slot(slotId));      

            } finally {
                m_testInterceptorInit.RequestIntercept.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestNoSlotSet() {
            // receive request modifies the thread scope slots
            try {
                int slotId = m_testInterceptorInit.RequestIntercept.SlotId;
                ORB orb = OrbServices.GetSingleton();
                omg.org.PortableInterceptor.Current current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                current.set_slot(slotId, null);

                System.Boolean result = m_testService.NoValueInScope();
                Assertion.Assert("value in slot", result);

                Assertion.Assert("service context present", !m_testInterceptorInit.RequestIntercept.HasReceivedContextElement);

                current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                Assertion.AssertNull("slot was set", current.get_slot(slotId));

            } finally {
                m_testInterceptorInit.RequestIntercept.ClearInvocationHistory();
            }            
        }

        #endregion IMethods

    }

}
