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

namespace Ch.Elca.Iiop.IntegrationTests {


    public enum InPathResult {
        NotCalled, Reply, Exception, Other
    }


    /// <summary>
    /// adds the test interceptors.
    /// </summary>
    public class TestInterceptor : ClientRequestInterceptor {


        private string m_name;
        private string m_testId;
        private bool m_invokedOnOutPath = false;
        private InPathResult m_inPathResult = InPathResult.NotCalled;
        private Exception m_throwExceptionOutPath = null;
        private Exception m_throwExceptionInPath = null;


        public TestInterceptor(string name, string testId) {
            m_name = name;
            m_testId = testId;
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

        public void ClearInvocationHistory() {
            m_invokedOnOutPath = false;
            m_inPathResult = InPathResult.NotCalled;
            m_throwExceptionOutPath = null;
            m_throwExceptionInPath = null;
        }

        public void send_request(ClientRequestInfo ri) {
            m_invokedOnOutPath = true;
            if (m_throwExceptionOutPath != null) {
                throw m_throwExceptionOutPath;
            }
        }

        public void send_poll(ClientRequestInfo ri) {
            // never called by IIOP.NET
        }

        public void receive_reply(ClientRequestInfo ri) {
            m_inPathResult = InPathResult.Reply;
            if (m_throwExceptionInPath != null) {
                throw m_throwExceptionInPath;
            }
        }

        public void receive_exception(ClientRequestInfo ri) {
            m_inPathResult = InPathResult.Exception;
            if (m_throwExceptionInPath != null) {
                throw m_throwExceptionInPath;
            }
        }

        public void receive_other(ClientRequestInfo ri) {
            m_inPathResult = InPathResult.Other;
            if (m_throwExceptionInPath != null) {
                throw m_throwExceptionInPath;
            }
        }

        public void SetExceptionOnOutPath(Exception ex) {
            m_throwExceptionOutPath = ex;
        }

        public void SetExceptionOnInPath(Exception ex) {
            m_throwExceptionInPath = ex;
        }

    }


}