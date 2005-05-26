/* TestInterceptorControlService.cs
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
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;
using omg.org.PortableInterceptor;

namespace Ch.Elca.Iiop.IntegrationTests {


    public class TestInterceptorControlService : MarshalByRefObject {

        private TestInterceptorInit m_interceptorInit;

        public TestInterceptorControlService(TestInterceptorInit interceptorInit) {
            m_interceptorInit = interceptorInit;
        }


        public bool IsReceiveSvcContextCalled() {
            return m_interceptorInit.RequestIntercept.InvokedOnInPathReceiveSvcContext;
        }

        public bool IsReceiveRequestCalled() {
            return m_interceptorInit.RequestIntercept.InvokedOnInPathReceive;
        }

        public OutPathResult GetOutPathResult() {
            return m_interceptorInit.RequestIntercept.OutPathResult;
        }

        public void ClearInterceptorHistory() {
            m_interceptorInit.RequestIntercept.ClearInvocationHistory();
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }        
        
    }

}
