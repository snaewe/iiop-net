/* Interceptors.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 10.04.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Interception;
using omg.org.CosNaming;
using omg.org.CORBA;
using omg.org.PortableInterceptor;
using omg.org.IOP;

namespace Ch.Elca.Iiop.IntegrationTests {


    /// <summary>
    /// adds the test interceptors.
    /// </summary>
    public class TestRequestInterceptor : ServerRequestInterceptor {


        private string m_name;
        private string m_testId;
        private Codec m_codec;        
        private int m_slotId;


        public TestRequestInterceptor(string name, string testId,
                                      Codec codec, int slotId) {
            m_name = name;
            m_testId = testId;
            m_codec = codec;
            m_slotId = slotId;
        }

        public string Name {
            get {
                return m_name;
            }
        }

        /// <summary>for debugging purposes</summary>
        public string TestId {
            get {
                return m_testId;
            }
        }

        /// <summary>the allocated slot_id</summary>
        public int SlotId {
            get {
                return m_slotId;
            }
        }

        public void receive_request_service_contexts(ServerRequestInfo ri) {
            object contextAsObject;
            try {
                contextAsObject = ri.get_request_service_context(1000);
            } catch (BAD_PARAM) {
                contextAsObject = null;
            }
            if (contextAsObject != null) {
                ServiceContext context = (ServiceContext)contextAsObject;
                TestServiceContext contextReceived = (TestServiceContext)m_codec.decode(context.context_data);
                ri.set_slot(m_slotId, contextReceived.TestEntry);
            }
        }
                
        public void receive_request(ServerRequestInfo ri) {
            // modify request scope after copy to the thread scope -> must not be propagated to the thread scope.
            if (ri.operation == "TestReceiveReqNotChangeThreadScope") {
                ri.set_slot(m_slotId, 2 * (int)ri.get_slot(m_slotId));
            } else if (ri.operation == "TestReceiveReqChangeThreadScope") {
                ORB orb = OrbServices.GetSingleton();
                omg.org.PortableInterceptor.Current current = 
                    (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
                current.set_slot(m_slotId, 3 * (int)current.get_slot(m_slotId));
            }
        }
        
        public void send_reply(ServerRequestInfo ri) {
            object testEntryAsObject = ri.get_slot(m_slotId);
            if (testEntryAsObject != null) {
                int entryResult = (int)testEntryAsObject;
                TestServiceContext resultContextEntry = 
                    new TestServiceContext(entryResult);
                ServiceContext context = new ServiceContext(1000, m_codec.encode(resultContextEntry));
                ri.add_reply_service_context(context, true);
            }
        }
        
        public void send_exception(ServerRequestInfo ri) {
        }
        
        public void send_other(ServerRequestInfo ri) {
        }

    }


}