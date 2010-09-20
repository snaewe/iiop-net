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
        public void TestNoException() {
            try {
                System.Byte arg = 1;
                System.Byte result = m_testService.TestIncByte(arg);
                Assertion.AssertEquals((System.Byte)(arg + 1), result);

                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.A.InvokedOnOutPath);
                Assertion.Assert("expected: b on out path called", m_testInterceptorInit.B.InvokedOnOutPath);
                Assertion.Assert("expected: c on out path called", m_testInterceptorInit.C.InvokedOnOutPath);

                Assertion.AssertEquals("a on in path called (reply)", 
                                       InPathResult.Reply, m_testInterceptorInit.A.InPathResult);
                Assertion.AssertEquals("b on in path called (reply)",
                                       InPathResult.Reply, m_testInterceptorInit.B.InPathResult);
                Assertion.AssertEquals("c on in path called (reply)", 
                                       InPathResult.Reply, m_testInterceptorInit.C.InPathResult);
            } finally {
                m_testInterceptorInit.A.ClearInvocationHistory();
                m_testInterceptorInit.B.ClearInvocationHistory();
                m_testInterceptorInit.C.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestServerUserExceptionNoInterceptorException() {
            try {
                try {
                    m_testService.TestThrowException();
                    Assertion.Fail("no exception");
                } catch (TestServerSideException) {
                    // ok, expected
                }

                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.A.InvokedOnOutPath);
                Assertion.Assert("expected: b on out path called", m_testInterceptorInit.B.InvokedOnOutPath);
                Assertion.Assert("expected: c on out path called", m_testInterceptorInit.C.InvokedOnOutPath);

                Assertion.AssertEquals("a on in path called (exception)", 
                                       InPathResult.Exception, m_testInterceptorInit.A.InPathResult);
                Assertion.AssertEquals("b on in path called (exception)",
                                       InPathResult.Exception, m_testInterceptorInit.B.InPathResult);
                Assertion.AssertEquals("c on in path called (exception)", 
                                       InPathResult.Exception, m_testInterceptorInit.C.InPathResult);
            } finally {
                m_testInterceptorInit.A.ClearInvocationHistory();
                m_testInterceptorInit.B.ClearInvocationHistory();
                m_testInterceptorInit.C.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestExceptionClientOutPath() {
            try {
                m_testInterceptorInit.B.SetExceptionOnOutPath(new BAD_OPERATION(1000, CompletionStatus.Completed_No));
                try {
                    System.Byte arg = 1;
                    System.Byte result = m_testService.TestIncByte(arg);
                    Assertion.Fail("no exception");
                } catch (BAD_OPERATION) {
                    // ok, expected
                }

                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.A.InvokedOnOutPath);
                Assertion.Assert("expected: b on out path called", m_testInterceptorInit.B.InvokedOnOutPath);
                Assertion.Assert("expected: c on out path not called", !m_testInterceptorInit.C.InvokedOnOutPath);

                Assertion.AssertEquals("a on in path called (exception)", 
                                       InPathResult.Exception, m_testInterceptorInit.A.InPathResult);
                Assertion.AssertEquals("b on in path called",
                                       InPathResult.NotCalled, m_testInterceptorInit.B.InPathResult);
                Assertion.AssertEquals("c on in path called", 
                                       InPathResult.NotCalled, m_testInterceptorInit.C.InPathResult);
            } finally {
                m_testInterceptorInit.A.ClearInvocationHistory();
                m_testInterceptorInit.B.ClearInvocationHistory();
                m_testInterceptorInit.C.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestExceptionClientInPathAfterNormalReply() {
            try {
                m_testInterceptorInit.B.SetExceptionOnInPath(new BAD_OPERATION(1000, CompletionStatus.Completed_Yes));
                try {
                    System.Byte arg = 1;
                    System.Byte result = m_testService.TestIncByte(arg);
                    Assertion.Fail("no exception");
                } catch (BAD_OPERATION) {
                    // ok, expected
                }

                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.A.InvokedOnOutPath);
                Assertion.Assert("expected: b on out path called", m_testInterceptorInit.B.InvokedOnOutPath);
                Assertion.Assert("expected: c on out path called", m_testInterceptorInit.C.InvokedOnOutPath);

                Assertion.AssertEquals("a on in path called (exception)", 
                                       InPathResult.Exception, m_testInterceptorInit.A.InPathResult);
                Assertion.AssertEquals("b on in path called (reply)",
                                       InPathResult.Reply, m_testInterceptorInit.B.InPathResult);
                Assertion.AssertEquals("c on in path called (reply)", 
                                       InPathResult.Reply, m_testInterceptorInit.C.InPathResult);
            } finally {
                m_testInterceptorInit.A.ClearInvocationHistory();
                m_testInterceptorInit.B.ClearInvocationHistory();
                m_testInterceptorInit.C.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestExceptionClientInPathAfterServerExceptionReply() {
            try {
                m_testInterceptorInit.B.SetExceptionOnInPath(new BAD_OPERATION(1000, CompletionStatus.Completed_Yes));
                try {
                    m_testService.TestThrowException();
                    Assertion.Fail("no exception");
                } catch (BAD_OPERATION) {
                    // ok, expected
                }

                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.A.InvokedOnOutPath);
                Assertion.Assert("expected: b on out path called", m_testInterceptorInit.B.InvokedOnOutPath);
                Assertion.Assert("expected: c on out path called", m_testInterceptorInit.C.InvokedOnOutPath);

                Assertion.AssertEquals("a on in path called (exception)", 
                                       InPathResult.Exception, m_testInterceptorInit.A.InPathResult);
                Assertion.AssertEquals("b on in path called (exception)",
                                       InPathResult.Exception, m_testInterceptorInit.B.InPathResult);
                Assertion.AssertEquals("c on in path called (exception)", 
                                       InPathResult.Exception, m_testInterceptorInit.C.InPathResult);
            } finally {
                m_testInterceptorInit.A.ClearInvocationHistory();
                m_testInterceptorInit.B.ClearInvocationHistory();
                m_testInterceptorInit.C.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestBypassServerInterceptors() {
            try {
                // call should be non-intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();

                // the following calls should be non-intercepted; special handling in interceptors
                Assertion.Assert("not expected: a rec. svc. context on in path called", 
                                 !m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("not expected: b rec. svc. context on in path called", 
                                 !m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("not expected: c rec. svc. context on in path called", 
                                 !m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("not expected: a rec. on in path called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("not expected: b rec. on in path called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("not expected: c rec. on in path called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path shouldn't be called", 
                                       OutPathResult.OutPathResult_NotCalled, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path shouldn't be called", 
                                       OutPathResult.OutPathResult_NotCalled, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path shouldn't be called", 
                                       OutPathResult.OutPathResult_NotCalled, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));                

            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();                
            }
        }

        [Test]
        public void TestCheckServerInterceptorsNormalReply() {
            try {
                System.Byte arg = 1;
                System.Byte result = m_testService.TestIncByte(arg);
                Assertion.AssertEquals((System.Byte)(arg + 1), result);

                // the following calls are non-intercepted; special handling in interceptors
                Assertion.Assert("expected: a rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("expected: a rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path called (reply)", 
                                       OutPathResult.OutPathResult_Reply, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path called (reply)", 
                                       OutPathResult.OutPathResult_Reply, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path called (reply)", 
                                       OutPathResult.OutPathResult_Reply, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));               
            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }
        }        


        [Test]
        public void TestCheckServerInterceptorsUserExceptionReply() {
            try {
                try {
                    m_testService.TestThrowException();
                    Assertion.Fail("no exception");
                } catch (TestServerSideException) {
                    // ok, expected
                }

                // the following calls are non-intercepted; special handling in interceptors
                Assertion.Assert("expected: a rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("expected: a rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path called (exception)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path called (exception)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path called (reply)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));               
            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }
        }

        [Test]
        public void TestCheckServerInterceptorsReceiveSvcContextException() {
            try {
                // non-intercepted
                m_interceptorControl.SetThrowException(ServerInterceptor.ServerInterceptor_InterceptorB, 
                                                       ServerInterceptionPoint.ServerInterceptionPoint_ReceiveSvcContext);
                try {
                    System.Byte arg = 1;
                    System.Byte result = m_testService.TestIncByte(arg);
                    Assertion.Fail("no exception");
                } catch (BAD_PARAM) {
                    // ok, expected
                }

                // the following calls are non-intercepted; special handling in interceptors
                Assertion.Assert("expected: a rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. svc. context on in path not called", 
                                 !m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("expected: a rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path called (exception)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path not called", 
                                       OutPathResult.OutPathResult_NotCalled, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path not called", 
                                       OutPathResult.OutPathResult_NotCalled, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));               
            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }

            try {
                // non-intercepted
                m_interceptorControl.SetThrowException(ServerInterceptor.ServerInterceptor_InterceptorA, 
                                                       ServerInterceptionPoint.ServerInterceptionPoint_ReceiveSvcContext);
                try {
                    System.Byte arg = 1;
                    System.Byte result = m_testService.TestIncByte(arg);
                    Assertion.Fail("no exception");
                } catch (BAD_PARAM) {
                    // ok, expected
                }

                // the following calls are non-intercepted; special handling in interceptors
                Assertion.Assert("expected: a rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. svc. context on in path not called", 
                                 !m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. svc. context on in path not called", 
                                 !m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("expected: a rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path not called", 
                                       OutPathResult.OutPathResult_NotCalled, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path not called", 
                                       OutPathResult.OutPathResult_NotCalled, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path not called", 
                                       OutPathResult.OutPathResult_NotCalled, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));               
            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }
        }

        [Test]
        public void TestCheckServerInterceptorsReceiveRequestException() {
            try {
                // non-intercepted
                m_interceptorControl.SetThrowException(ServerInterceptor.ServerInterceptor_InterceptorB, 
                                                       ServerInterceptionPoint.ServerInterceptionPoint_ReceiveRequest);
                try {
                    System.Byte arg = 1;
                    // intercepted
                    System.Byte result = m_testService.TestIncByte(arg);
                    Assertion.Fail("no exception");
                } catch (BAD_PARAM) {
                    // ok, expected
                }

                // the following calls are non-intercepted; special handling in interceptors
                Assertion.Assert("expected: a rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("expected: a rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path called (exception)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path called (excpetion)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path called (excpetion)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));               
            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }

            try {
                // non-intercepted
                m_interceptorControl.SetThrowException(ServerInterceptor.ServerInterceptor_InterceptorA, 
                                                       ServerInterceptionPoint.ServerInterceptionPoint_ReceiveRequest);
                try {
                    System.Byte arg = 1;
                    // intercepted
                    System.Byte result = m_testService.TestIncByte(arg);
                    Assertion.Fail("no exception");
                } catch (BAD_PARAM) {
                    // ok, expected
                }

                // the following calls are non-intercepted; special handling in interceptors
                Assertion.Assert("expected: a rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("expected: a rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. on in path not called", 
                                 !m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path called (exception)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path called (excpetion)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path called (excpetion)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));               
            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }
        }

        [Test]
        public void TestCheckServerInterceptorsSendResponseExceptionAfterNormalReply() {
            try {
                // non-intercepted
                m_interceptorControl.SetThrowException(ServerInterceptor.ServerInterceptor_InterceptorB, 
                                                       ServerInterceptionPoint.ServerInterceptionPoint_SendResponse);
                try {
                    System.Byte arg = 1;
                    // intercepted
                    System.Byte result = m_testService.TestIncByte(arg);
                    Assertion.Fail("no exception");
                } catch (BAD_PARAM) {
                    // ok, expected
                }

                // the following calls are non-intercepted; special handling in interceptors
                Assertion.Assert("expected: a rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("expected: a rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. on in path not called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path called (exception)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path called (normal reply)", 
                                       OutPathResult.OutPathResult_Reply, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path called (normal reply)", 
                                       OutPathResult.OutPathResult_Reply, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));               
            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }

        }

        [Test]
        public void TestCheckServerInterceptorsSendResponseExceptionAfterExceptionReply() {
            try {
                // non-intercepted
                m_interceptorControl.SetThrowException(ServerInterceptor.ServerInterceptor_InterceptorB, 
                                                       ServerInterceptionPoint.ServerInterceptionPoint_SendResponse);
                try {
                    // intercepted
                    m_testService.TestThrowException();
                    Assertion.Fail("no exception");
                } catch (BAD_PARAM) {
                    // ok, expected
                }

                // the following calls are non-intercepted; special handling in interceptors
                Assertion.Assert("expected: a rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. svc. context on in path called", 
                                 m_interceptorControl.IsReceiveSvcContextCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.Assert("expected: a rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.Assert("expected: b rec. on in path called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.Assert("expected: c rec. on in path not called", 
                                 m_interceptorControl.IsReceiveRequestCalled(ServerInterceptor.ServerInterceptor_InterceptorC));

                Assertion.AssertEquals("a on out path called (exception)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorA));
                Assertion.AssertEquals("b on out path called (normal reply)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorB));
                Assertion.AssertEquals("c on out path called (normal reply)", 
                                       OutPathResult.OutPathResult_Exception, 
                                       m_interceptorControl.GetOutPathResult(ServerInterceptor.ServerInterceptor_InterceptorC));               
            } finally {
                // call is not intercepted on server side
                m_interceptorControl.ClearInterceptorHistory();
            }

        }


        [Test]
        public void TestOneWayCallNoException() {
            try {
                m_testService.OneWayCall();

                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.A.InvokedOnOutPath);
                Assertion.Assert("expected: b on out path called", m_testInterceptorInit.B.InvokedOnOutPath);
                Assertion.Assert("expected: c on out path called", m_testInterceptorInit.C.InvokedOnOutPath);

                Assertion.AssertEquals("a on in path called (other)", 
                                       InPathResult.Other, m_testInterceptorInit.A.InPathResult);
                Assertion.AssertEquals("b on in path called (other)",
                                       InPathResult.Other, m_testInterceptorInit.B.InPathResult);
                Assertion.AssertEquals("c on in path called (other)", 
                                       InPathResult.Other, m_testInterceptorInit.C.InPathResult);
            } finally {
                m_testInterceptorInit.A.ClearInvocationHistory();
                m_testInterceptorInit.B.ClearInvocationHistory();
                m_testInterceptorInit.C.ClearInvocationHistory();
            }            
        }
        
        [Test]
        public void TestOneWayCallExceptionClientOutPath() {
            try {
                m_testInterceptorInit.B.SetExceptionOnOutPath(new BAD_OPERATION(1000, CompletionStatus.Completed_No));
                m_testService.OneWayCall(); // one way call, no result
 
                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.A.InvokedOnOutPath);
                Assertion.Assert("expected: b on out path called", m_testInterceptorInit.B.InvokedOnOutPath);
                Assertion.Assert("expected: c on out path not called", !m_testInterceptorInit.C.InvokedOnOutPath);

                Assertion.AssertEquals("a on in path called (exception)", 
                                       InPathResult.Exception, m_testInterceptorInit.A.InPathResult);
                Assertion.AssertEquals("b on in path called",
                                       InPathResult.NotCalled, m_testInterceptorInit.B.InPathResult);
                Assertion.AssertEquals("c on in path called", 
                                       InPathResult.NotCalled, m_testInterceptorInit.C.InPathResult);
            } finally {
                m_testInterceptorInit.A.ClearInvocationHistory();
                m_testInterceptorInit.B.ClearInvocationHistory();
                m_testInterceptorInit.C.ClearInvocationHistory();
            }            
        }

        [Test]
        public void TestOneWayCallExceptionClientInPath() {
            try {
                m_testInterceptorInit.B.SetExceptionOnInPath(new BAD_OPERATION(1000, CompletionStatus.Completed_Yes));
                m_testService.OneWayCall(); // one way call, no result

                Assertion.Assert("expected: a on out path called", m_testInterceptorInit.A.InvokedOnOutPath);
                Assertion.Assert("expected: b on out path called", m_testInterceptorInit.B.InvokedOnOutPath);
                Assertion.Assert("expected: c on out path called", m_testInterceptorInit.C.InvokedOnOutPath);

                Assertion.AssertEquals("a on in path called (exception)", 
                                       InPathResult.Exception, m_testInterceptorInit.A.InPathResult);
                Assertion.AssertEquals("b on in path called (other)",
                                       InPathResult.Other, m_testInterceptorInit.B.InPathResult);
                Assertion.AssertEquals("c on in path called (other)", 
                                       InPathResult.Other, m_testInterceptorInit.C.InPathResult);
            } finally {
                m_testInterceptorInit.A.ClearInvocationHistory();
                m_testInterceptorInit.B.ClearInvocationHistory();
                m_testInterceptorInit.C.ClearInvocationHistory();
            }            
        }

        #endregion IMethods


    }

}
