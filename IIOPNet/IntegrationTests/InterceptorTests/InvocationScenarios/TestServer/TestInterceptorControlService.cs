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

    [Serializable]
    public enum ServerInterceptor {
        InterceptorA, InterceptorB, InterceptorC
    }

    [Serializable]
    public enum ServerInterceptionPoint {
        ReceiveSvcContext, ReceiveRequest, SendResponse
    }


    public class TestInterceptorControlService : MarshalByRefObject {

        private TestInterceptorInit m_interceptorInit;

        public TestInterceptorControlService(TestInterceptorInit interceptorInit) {
            m_interceptorInit = interceptorInit;
        }


        private TestInterceptor GetInterceptor(ServerInterceptor interceptor) {
            TestInterceptor result;
            switch (interceptor) {
                case ServerInterceptor.InterceptorA:
                    result = m_interceptorInit.A;
                    break;
                case ServerInterceptor.InterceptorB:
                    result = m_interceptorInit.B;
                    break;
                case ServerInterceptor.InterceptorC:
                    result = m_interceptorInit.C;
                    break;
                default:
                    throw new BAD_PARAM(3000, CompletionStatus.Completed_Yes);
            }
            return result;
        }

        public bool IsReceiveSvcContextCalled(ServerInterceptor interceptor) {
            return GetInterceptor(interceptor).InvokedOnInPathReceiveSvcContext;
        }

        public bool IsReceiveRequestCalled(ServerInterceptor interceptor) {
            return GetInterceptor(interceptor).InvokedOnInPathReceive;
        }

        public OutPathResult GetOutPathResult(ServerInterceptor interceptor) {
            return GetInterceptor(interceptor).OutPathResult;
        }

        public void SetThrowException(ServerInterceptor interceptor, ServerInterceptionPoint point) {
            TestInterceptor toModify = GetInterceptor(interceptor);
            switch (point) {
                case ServerInterceptionPoint.ReceiveSvcContext:
                    toModify.SetExceptionOnInPathSvcContext(new BAD_PARAM(2000, CompletionStatus.Completed_No));
                    break;
                case ServerInterceptionPoint.ReceiveRequest:
                    toModify.SetExceptionOnInPathRequest(new BAD_PARAM(2000, CompletionStatus.Completed_No));
                    break;
                case ServerInterceptionPoint.SendResponse:
                    toModify.SetExceptionOnOutPath(new BAD_PARAM(2000, CompletionStatus.Completed_Yes));
                    break;
            }
        }

        public void ClearInterceptorHistory() {
            m_interceptorInit.A.ClearInvocationHistory();
            m_interceptorInit.B.ClearInvocationHistory();
            m_interceptorInit.C.ClearInvocationHistory();
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }        
        
    }

}
