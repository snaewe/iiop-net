/* Connection.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 30.04.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2003 Dominic Ullmann
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
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using Ch.Elca.Iiop.Services;
using omg.org.CORBA;

namespace Ch.Elca.Iiop
{


    /// <summary>
    /// Stores information associated with a GIOP connection,
    /// e.g. the Codesets chosen
    /// </summary>
    public class GiopConnectionDesc
    {

        #region IFields

        private int m_charSetChosen;
        private int m_wcharSetChosen;

        private bool m_codeSetNegotiated = false;
        private bool m_isCodeSetDefined = false;

        private GiopClientConnectionManager m_conManager;
        private GiopTransportMessageHandler m_transportHandler;

        #endregion IFields
        #region IConstructors

        internal GiopConnectionDesc(GiopClientConnectionManager conManager,
                                   GiopTransportMessageHandler transportHandler)
        {
            m_conManager = conManager;
            m_transportHandler = transportHandler;
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// the CharSet selected; or throwing an INTERNAL exception, if not
        /// user specified.
        /// </summary>
        public int CharSet
        {
            get
            {
                AssertCodeSetDefined();
                return m_charSetChosen;
            }
        }

        /// <summary>
        /// the WCharSet selected; or throwing an INTERNAL exception, if not
        /// user specified.
        /// </summary>        
        public int WCharSet
        {
            get
            {
                AssertCodeSetDefined();
                return m_wcharSetChosen;
            }
        }

        /// <summary>
        /// a client connection manager responsible for client connections (may be null).
        /// </summary>
        internal GiopClientConnectionManager ConnectionManager
        {
            get
            {
                return m_conManager;
            }
        }

        /// <summary>
        /// the transport handler responsible for the associated connection
        /// </summary>
        internal GiopTransportMessageHandler TransportHandler
        {
            get
            {
                if (m_transportHandler != null)
                {
                    return m_transportHandler;
                }
                else
                {
                    throw new omg.org.CORBA.BAD_INV_ORDER(998, omg.org.CORBA.CompletionStatus.Completed_MayBe);
                }
            }
        }

        #endregion IProperties
        #region IMethods

        private void AssertCodeSetDefined()
        {
            if (!IsCodeSetDefined())
            {
                throw new INTERNAL(433, CompletionStatus.Completed_MayBe);
            }
        }

        /// <summary>
        /// is the codeset already negotiated?
        /// </summary>
        public bool IsCodeSetNegotiated()
        {
            return m_codeSetNegotiated;
        }

        /// <summary>
        /// the codeset is already negotiated.
        /// </summary>
        public void SetCodeSetNegotiated()
        {
            m_codeSetNegotiated = true;
        }

        public void SetNegotiatedCodeSets(int charSet, int wcharSet)
        {
            m_charSetChosen = charSet;
            m_wcharSetChosen = wcharSet;
            SetCodeSetNegotiated();
            SetCodeSetDefined();
        }

        /// <summary>
        /// Returns true, if a specific char and wchar set has been selected.
        /// Selecting a char/wchar set is done by calling 
        /// <see cref="GiopConnectionDesc.SetNegotiatedCodeSets"/>.
        /// </summary>
        public bool IsCodeSetDefined()
        {
            return m_isCodeSetDefined;
        }

        private void SetCodeSetDefined()
        {
            m_isCodeSetDefined = true;
        }

        #endregion IMethods

    }

    /// <summary>the connection context for the client side</summary>
    internal class GiopClientConnectionDesc : GiopConnectionDesc
    {

        #region Constants

        internal const string CLIENT_TR_HEADER_KEY = "_client_giop_con_desc_";

        #endregion Constants
        #region IConstructors

        internal GiopClientConnectionDesc(GiopClientConnectionManager conManager, GiopClientConnection connection,
                                          GiopRequestNumberGenerator reqNumberGen,
                                          GiopTransportMessageHandler transportHandler)
            : base(conManager, transportHandler)
        {
            m_reqNumGen = reqNumberGen;
            m_connection = connection;
        }

        #endregion IConstructors
        #region IFields

        private GiopRequestNumberGenerator m_reqNumGen;

        private GiopClientConnection m_connection;

        #endregion IFields
        #region IProperties

        internal GiopRequestNumberGenerator ReqNumberGen
        {
            get
            {
                return m_reqNumGen;
            }
        }

        internal GiopClientConnection Connection
        {
            get
            {
                return m_connection;
            }
        }

        #endregion IProporties
    }


    /// <summary>
    /// the connection context for the server side
    /// </summary>
    public class GiopServerConnection
    {

        #region Constants

        internal const string SERVER_TR_HEADER_KEY = "_server_giop_transport_";

        #endregion Constants
        #region IFields

        private GiopReceivedRequestMessageDispatcher m_msgDispatcher;
        private GiopConnectionDesc m_conDesc;

        #endregion IFields
        #region IConstructors

        internal GiopServerConnection(GiopConnectionDesc conDesc,
                                      GiopReceivedRequestMessageDispatcher msgDispatcher)
        {
            m_conDesc = conDesc;
            m_msgDispatcher = msgDispatcher;
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// the connection description.
        /// </summary>
        internal GiopConnectionDesc ConDesc
        {
            get
            {
                return m_conDesc;
            }
        }

        /// <summary>
        /// the message transport handler
        /// </summary>
        internal GiopTransportMessageHandler TransportHandler
        {
            get
            {
                return m_conDesc.TransportHandler;
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>notifies the transport about deserialisation complete.
        /// </summary>
        /// <remarks>it's now safe to read next request from transport, 
        /// because message ordering constraints are fullfilled.
        /// Because of e.g. codeset negotiation and other session based mechanism, 
        /// the request deserialisation must be done serialised; 
        /// the servant dispatch can be done in parallel.</remarks> 
        internal void NotifyDeserialiseRequestComplete()
        {
            m_msgDispatcher.NotifyDeserialiseRequestComplete();
        }

        #endregion IMethods

    }

    /// <summary>
    /// stores the relevant information of an IIOP client side
    /// connection
    /// </summary>
    internal abstract class GiopClientConnection
    {

        #region IFields

        private GiopClientConnectionDesc m_assocDesc;

        private string m_connectionKey;

        protected GiopTransportMessageHandler m_transportHandler;

        private DateTime m_lastUsed = DateTime.Now;

        private int m_numberOfRequestsOnCon;

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// used for inheritors, calling initalize themselves.
        /// </summary>
        protected GiopClientConnection()
        {
        }

        #endregion IConstructors
        #region IProperties

        internal GiopClientConnectionDesc Desc
        {
            get
            {
                return m_assocDesc;
            }
        }

        internal string ConnectionKey
        {
            get
            {
                return m_connectionKey;
            }
        }

        internal GiopTransportMessageHandler TransportHandler
        {
            get
            {
                return m_transportHandler;
            }
        }

        internal int NumberOfRequestsOnConnection
        {
            get
            {
                return m_numberOfRequestsOnCon;
            }
        }

        #endregion IProperties
        #region IMethods

        protected void Initalize(string connectionKey, GiopTransportMessageHandler transportHandler,
                                 GiopClientConnectionDesc assocDesc)
        {
            m_connectionKey = connectionKey;
            m_assocDesc = assocDesc;
            m_transportHandler = transportHandler;
        }

        internal bool CheckConnected()
        {
            return m_transportHandler.Transport.IsConnectionOpen();
        }

        /// <summary>
        /// is this connection closable in client role.
        /// </summary>        
        internal abstract bool CanCloseConnection();

        /// <summary>
        /// closes the connection.
        /// </summary>
        /// <remarks>this method must only be called by the ConnectionManager.</remarks>
        internal abstract void CloseConnection();

        /// <summary>
        /// is this connection initiated in this appdomain.
        /// </summary>
        internal abstract bool IsInitiatedLocal();

        /// <summary>
        /// Notifies the connection, that a request has been completelty sent on the connection.
        /// </summary>
        internal void NotifyRequestSentCompleted()
        {
            m_assocDesc.ConnectionManager.RequestOnConnectionSent(this);
        }

        /// <summary>
        /// returns ture, if the connection is not use for at least the specified time; otherwise false.
        /// </summary>
        internal bool IsNotUsedForAtLeast(TimeSpan idleTime)
        {
            return (m_lastUsed + idleTime < DateTime.Now);
        }

        /// <summary>
        /// if a connection is currently not in use and has not been used for a long time, the
        /// connection can be closed.
        /// </summary>
        internal bool CanBeClosedAsIdle(TimeSpan idleTime)
        {
            return (IsNotUsedForAtLeast(idleTime) && (!HasPendingRequests()) && CanCloseConnection());
        }

        /// <summary>
        /// updates the time, this connection has been used last.
        /// </summary>
        internal void UpdateLastUsedTime()
        {
            m_lastUsed = DateTime.Now;
        }

        internal void IncrementNumberOfRequests()
        {
            m_numberOfRequestsOnCon++;
        }

        internal void DecrementNumberOfRequests()
        {
            m_numberOfRequestsOnCon--;
        }

        internal bool HasPendingRequests()
        {
            return NumberOfRequestsOnConnection > 0;
        }

        /// <summary>
        /// returns true, if the connection is usable for a next request.
        /// </summary>
        internal virtual bool CanBeUsedForNextRequest()
        {
            return (CheckConnected() && (Desc.ReqNumberGen.IsAbleToGenerateNext()));
        }

        #endregion IMethods


    }


    /// <summary>
    /// a connection, which is initiated in the current appdomain.
    /// </summary>
    internal class GiopClientInitiatedConnection : GiopClientConnection
    {

        #region IFields

        private IClientTransport m_clientTransport;

        #endregion IFields
        #region IConstructors

        /// <param name="connectionKey">the key describing the connection</param>
        /// <param name="transport">a not yet connected client transport</param>
        /// <param name="headerFlags">the header flags to use for transport related giop messages.</param>
        internal GiopClientInitiatedConnection(string connectionKey, IClientTransport transport,
                                               MessageTimeout requestTimeOut, GiopClientConnectionManager conManager,
                                               bool supportBidir, byte headerFlags)
        {
            GiopRequestNumberGenerator reqNumberGen =
                (!supportBidir ? new GiopRequestNumberGenerator() : new GiopRequestNumberGenerator(true));
            GiopTransportMessageHandler handler =
                      new GiopTransportMessageHandler(transport, requestTimeOut, headerFlags);
            GiopClientConnectionDesc conDesc = new GiopClientConnectionDesc(conManager, this, reqNumberGen, handler);
            m_clientTransport = transport;
            Initalize(connectionKey, handler, conDesc);
        }

        #endregion IConstructors
        #region IMethods

        internal override void CloseConnection()
        {
            try
            {
                m_transportHandler.ForceCloseConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception while closing connection: " + ex);
            }
        }

        /// <summary>
        /// opens the connection and begins listening for messages.
        /// </summary>
        internal void OpenConnection()
        {
            m_clientTransport.OpenConnection();
            this.TransportHandler.StartMessageReception(); // begin listening for messages
        }

        internal override bool CanCloseConnection()
        {
            return true;
        }

        internal override bool IsInitiatedLocal()
        {
            return true;
        }

        #endregion IMethods

    }


    /// <summary>
    /// a connection, which is initiated in another appdomain. This connection is used in bidir mode
    /// for callback.
    /// </summary>    
    internal class GiopBidirInitiatedConnection : GiopClientConnection
    {

        #region IFields

        private bool m_canNoLongerBeUsed;

        #endregion IFields
        #region IConstructors

        /// <param name="connectionKey">the key describing the connection</param>        
        internal GiopBidirInitiatedConnection(string connectionKey, GiopTransportMessageHandler transportHandler,
                                              GiopClientConnectionManager conManager)
        {
            GiopRequestNumberGenerator reqNumberGen =
                    new GiopRequestNumberGenerator(false); // not connection originator -> create non-even req. numbers
            GiopClientConnectionDesc conDesc = new GiopClientConnectionDesc(conManager, this, reqNumberGen,
                                                                            transportHandler);
            Initalize(connectionKey, transportHandler, conDesc);
        }

        #endregion IConstructors
        #region IMethods

        internal override void CloseConnection()
        {
            throw new omg.org.CORBA.BAD_OPERATION(765, omg.org.CORBA.CompletionStatus.Completed_MayBe);
        }

        internal override bool CanCloseConnection()
        {
            return false;
        }

        internal override bool IsInitiatedLocal()
        {
            return false;
        }

        internal override bool CanBeUsedForNextRequest()
        {
            return (!m_canNoLongerBeUsed) && base.CanBeUsedForNextRequest();
        }

        internal void SetConnectionUnusable()
        {
            m_canNoLongerBeUsed = true;
        }

        #endregion IMethods

    }


}


#if UnitTest

namespace Ch.Elca.Iiop.Tests
{

    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using omg.org.CORBA;

    /// <summary>
    /// Tests the CdrInputStream
    /// </summary>
    [TestFixture]
    public class ConnectionTests
    {

        [Test]
        public void TestGiopConnectionDescCodeSetNotSet()
        {
            GiopConnectionDesc desc =
                new GiopConnectionDesc(null, null);
            Assert.IsTrue(
                             !desc.IsCodeSetNegotiated(), "Codeset not negotiated at construction time");
            Assert.IsTrue(
                             !desc.IsCodeSetDefined(), "No codeset user defined at construction time");
        }

        [Test]
        public void TestGiopConnectionDescCodeSetNotSetAccess()
        {
            GiopConnectionDesc desc =
                new GiopConnectionDesc(null, null);
            Assert.IsTrue(
                             !desc.IsCodeSetDefined(), "No codeset user defined at construction time");
            try
            {
                int charSet = desc.CharSet;
                Assert.Fail("Expected expection, when accessing charset, although not set");
            }
            catch (INTERNAL)
            {
                // expected.
            }
            try
            {
                int wcharSet = desc.WCharSet;
                Assert.Fail("Expected expection, when accessing charset, although not set");
            }
            catch (INTERNAL)
            {
                // expected.
            }
        }

        [Test]
        public void TestGiopConnectionDescSetCodeSet()
        {
            int charSet = 0x5010001;
            int wcharSet = 0x10100;
            GiopConnectionDesc desc =
                new GiopConnectionDesc(null, null);
            desc.SetNegotiatedCodeSets(charSet, wcharSet);
            Assert.IsTrue(
                             desc.IsCodeSetNegotiated(),"Codeset negotiated");
            Assert.AreEqual(charSet, desc.CharSet,"char set");
            Assert.AreEqual(wcharSet, desc.WCharSet,"wchar set");
            Assert.IsTrue(desc.IsCodeSetDefined(),"Codeset user defined");

        }

        [Test]
        public void TestGiopConnectionDescSetCodeSetNegotiated()
        {
            GiopConnectionDesc desc =
                new GiopConnectionDesc(null, null);
            desc.SetCodeSetNegotiated();
            Assert.IsTrue(
                             desc.IsCodeSetNegotiated(),"Codeset negotiated");
            Assert.IsTrue(
                             !desc.IsCodeSetDefined(),"Codeset not user defined");
        }



    }

}

#endif
