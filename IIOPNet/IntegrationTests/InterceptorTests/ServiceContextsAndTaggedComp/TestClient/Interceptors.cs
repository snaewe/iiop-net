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


    public enum InPathResult {
        NotCalled, Reply, Exception, Other
    }


    /// <summary>
    /// adds the test interceptors.
    /// </summary>
    public class TestRequestInterceptor : ClientRequestInterceptor {


        private string m_name;
        private string m_testId;
        private bool m_invokedOnOutPath = false;
        private InPathResult m_inPathResult = InPathResult.NotCalled;

        private object m_lastContextElement;
        private object m_taggedComponent;
        private Codec m_codec;
        private int m_contextEntryBegin;


        public TestRequestInterceptor(string name, string testId, Codec codec) {
            m_name = name;
            m_testId = testId;
            m_codec = codec;
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

        public bool InvokedOnOutPath {
            get {
                return m_invokedOnOutPath;
            }
        }

        public InPathResult InPathResult {
            get {
                return m_inPathResult;
            }
        }

        public bool HasReceivedContextElement {
            get {
                return m_lastContextElement is TestServiceContext;
            }
        }

        public TestServiceContext ContextElement {
            get {
                return (TestServiceContext)m_lastContextElement;
            }
        }

        public bool HasTaggedComponet {
            get {
                return m_taggedComponent is TestComponent;
            }
        }

        public TestComponent TaggedComponent {
            get {
                return (TestComponent)m_taggedComponent;
            }
        }

        public int ContextEntryBegin {
            get {
                return m_contextEntryBegin;
            }
            set {
                m_contextEntryBegin = value;
            }
        }

        /// <summary>don't intercept calls for interception control service</summary>
        private bool MustNonInterceptCall(ClientRequestInfo ri) {
           return ri.operation.Equals("IsReceiveSvcContextCalled") ||
                  ri.operation.Equals("IsReceiveRequestCalled") ||
                  ri.operation.Equals("GetOutPathResult") ||                  
                  ri.operation.Equals("ClearInterceptorHistory");
        }

        public void ClearInvocationHistory() {
            m_invokedOnOutPath = false;
            m_inPathResult = InPathResult.NotCalled;
            m_lastContextElement = null;
            m_taggedComponent = null;
        }

        public void send_request(ClientRequestInfo ri) {            
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_invokedOnOutPath = true;
            TestServiceContext contextEntry = new TestServiceContext();
            contextEntry.TestEntry = m_contextEntryBegin;
            ServiceContext context = new ServiceContext(1000,
                                                        m_codec.encode(contextEntry));
            ri.add_request_service_context(context, true);
            try {
                TaggedComponent taggedComponentEnc = ri.get_effective_component(1000);
                omg.org.CORBA.TypeCode typeCode = 
                    omg.org.CORBA.OrbServices.GetSingleton().create_tc_for_type(typeof(TestComponent));
                m_taggedComponent = m_codec.decode_value(taggedComponentEnc.component_data, typeCode);
            } catch (BAD_PARAM) {
                m_taggedComponent = null;
            }
        }

        public void send_poll(ClientRequestInfo ri) {
            // never called by IIOP.NET
        }

        public void receive_reply(ClientRequestInfo ri) {
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_inPathResult = InPathResult.Reply;
            try {
                ServiceContext context = ri.get_reply_service_context(1000);
                m_lastContextElement = (TestServiceContext)m_codec.decode(context.context_data);    
            } catch (BAD_PARAM) {
                m_lastContextElement = null; // not found
            }
        }

        public void receive_exception(ClientRequestInfo ri) {
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_inPathResult = InPathResult.Exception;
        }

        public void receive_other(ClientRequestInfo ri) {
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_inPathResult = InPathResult.Other;
        }


    }


}