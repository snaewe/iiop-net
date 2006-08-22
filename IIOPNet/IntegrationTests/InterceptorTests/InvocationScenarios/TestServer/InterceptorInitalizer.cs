/* InterceptorInitalizer.cs
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

namespace Ch.Elca.Iiop.IntegrationTests {


    /// <summary>
    /// adds the test interceptors.
    /// </summary>
    public class TestInterceptorInit : ORBInitializer {

        private TestInterceptor m_a;
        private TestInterceptor m_b;
        private TestInterceptor m_c;

        public TestInterceptor A {
            get {
                return m_a;
            }
        }

        public TestInterceptor B {
            get {
                return m_b;
            }
        }

        public TestInterceptor C {
            get {
                return m_c;
            }
        }

        public void pre_init(ORBInitInfo info) {
            m_a = new TestInterceptor(String.Empty, "A");
            m_b = new TestInterceptor(String.Empty, "B");
            m_c = new TestInterceptor(String.Empty, "C");
            // WARNING: uses implementation detail to register interceptors in the order A, B, C. 
            // that's not guaranteed to work for non-test-cases. (not specified by standard).
            // if implementation changes, test-case must be updated.
            info.add_server_request_interceptor(m_a);
            info.add_server_request_interceptor(m_b);
            info.add_server_request_interceptor(m_c);
        }
        
        public void post_init(ORBInitInfo info) {
            // nothing to do
        }


    }


}