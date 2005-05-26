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


    [Serializable]
    public enum OutPathResult {
        NotCalled, Reply, Exception, Other
    }


    /// <summary>
    /// adds the test interceptors.
    /// </summary>
    public class TestRequestInterceptor : ServerRequestInterceptor {


        private string m_name;
        private string m_testId;
        private bool m_invokedOnInPathReceiveSvcContext = false;
        private bool m_invokedOnInPathReceive = false;
        private OutPathResult m_outPathResult = OutPathResult.NotCalled;
        private Codec m_codec;
        private object m_contextReceived;


        public TestRequestInterceptor(string name, string testId,
                                      Codec codec) {
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

        public bool InvokedOnInPathReceiveSvcContext {
            get {
                return m_invokedOnInPathReceiveSvcContext;
            }
        }

        public bool InvokedOnInPathReceive {
            get {
                return m_invokedOnInPathReceive;
            }
        }

        public OutPathResult OutPathResult {
            get {                
                return m_outPathResult;
            }
        }

        /// <summary>don't intercept calls for interception control service</summary>
        private bool MustNonInterceptCall(ServerRequestInfo ri) {
           return ri.operation.Equals("IsReceiveSvcContextCalled") ||
                  ri.operation.Equals("IsReceiveRequestCalled") ||
                  ri.operation.Equals("GetOutPathResult") ||                  
                  ri.operation.Equals("ClearInterceptorHistory");
        }

        public void ClearInvocationHistory() {
              m_invokedOnInPathReceiveSvcContext = false;
              m_invokedOnInPathReceive = false;
              m_outPathResult = OutPathResult.NotCalled;
              m_contextReceived = null;
        }

        public void receive_request_service_contexts(ServerRequestInfo ri) {
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_invokedOnInPathReceiveSvcContext = true;
            ServiceContext context = ri.get_request_service_context(1000);
            m_contextReceived = (TestServiceContext)m_codec.decode(context.context_data);
        }
                
        public void receive_request(ServerRequestInfo ri) {
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_invokedOnInPathReceive = true;            
        }
        
        public void send_reply(ServerRequestInfo ri) {
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_outPathResult = OutPathResult.Reply;
            if (m_contextReceived != null) {
                TestServiceContext resultContextEntry = 
                    new TestServiceContext(((TestServiceContext)m_contextReceived).TestEntry);
                ServiceContext context = new ServiceContext(1000, m_codec.encode(resultContextEntry));
                ri.add_reply_service_context(context, true);
            }
        }
        
        public void send_exception(ServerRequestInfo ri) {
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_outPathResult = OutPathResult.Exception;
        }
        
        public void send_other(ServerRequestInfo ri) {
            if (MustNonInterceptCall(ri)) {
                return;
            }
            m_outPathResult = OutPathResult.Other;
        }

    }

    public class TestIORInterceptor : IORInterceptor {

        private string m_name;
        private string m_testId;
        private TaggedComponent m_tc;

        public TestIORInterceptor(string name, string testId,
                                  Codec codec) {
            m_name = name;
            m_testId = testId;
            m_tc = new TaggedComponent(1000, codec.encode_value(new TestComponent(1)));            
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

        public void establish_components(omg.org.PortableInterceptor.IORInfo iorInfo) {
            iorInfo.add_ior_component(m_tc);
        }
        

    }

}