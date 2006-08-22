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
using omg.org.IOP;

namespace Ch.Elca.Iiop.IntegrationTests {


    /// <summary>
    /// adds the test interceptors.
    /// </summary>
    public class TestInterceptorInit : ORBInitializer {

        private TestRequestInterceptor m_requestIntercept;

        public TestRequestInterceptor RequestIntercept {
            get {
                return m_requestIntercept;
            }
        }

        public void pre_init(ORBInitInfo info) {
            int slotId = info.allocate_slot_id();
            Codec codec = info.codec_factory.create_codec(
                              new Encoding(ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_requestIntercept = new TestRequestInterceptor(String.Empty, "request", codec, slotId);
            info.add_server_request_interceptor(m_requestIntercept);
        }
        
        public void post_init(ORBInitInfo info) {
            // nothing to do
        }


    }


}