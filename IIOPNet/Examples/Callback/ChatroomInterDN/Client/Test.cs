/* Test.cs
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 19.02.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 *
 * Copyright 2003 ELCA Informatique SA
 * Av. de la Harpe 22-24, 1000 Lausanne 13, Switzerland
 * www.elca.ch
 *
 * Copyright 2006 Dominic Ullmann
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
using System.Windows.Forms;
using System.Threading;
using Ch.Elca.Iiop.Demo.Chatroom;
using NUnit.Framework;
using NUnit.Extensions.Forms;

namespace Ch.Elca.Iiop.Demo.Chatroom.Test {

    public class ChatAppTest : NUnitFormTest {

        private Client m_client;
        private Chatform m_chatform;

        private TextBoxTester m_usernameTextbox;
        private ButtonTester m_connectButton;
        private ButtonTester m_disconnectbutton;
        private TextBoxTester m_messageTextbox;
        private ButtonTester m_sendMsgButton;
        private ButtonTester m_sendMsgAsyncButton;
        private LabelTester m_connectedLabel;
        private ListBoxTester m_messagesTester;

        public override bool UseHidden {
            get {
                return false;
            }
        }

        public override void Setup() {
            base.Setup();

            m_usernameTextbox = new TextBoxTester("m_usernameTextbox");
	    m_connectButton = new ButtonTester("m_connectButton");
	    m_disconnectbutton = new ButtonTester("m_disconnectbutton");
	    m_sendMsgButton = new ButtonTester("m_sendMsgButton");
	    m_sendMsgAsyncButton = new ButtonTester("m_sendMsgAsyncButton");
            m_connectedLabel = new LabelTester("m_constatus");
            m_messageTextbox = new TextBoxTester("m_messageTextbox");
            m_messagesTester = new ListBoxTester("m_messages");
            m_client = new Client("localhost", 8087, 0);
            m_chatform = m_client.CreateChatForm();
            m_chatform.Show();
            m_usernameTextbox.Enter("test");
	    m_connectButton.Click();
        }

        public override void TearDown() {
            m_disconnectbutton.Click();            
            m_chatform = null;
            m_client.TearDown();
            base.TearDown();
        }


        [Test]
        public void TestConnectDisconnect() {
            Assert.AreEqual(Chatform.CONNECTED_INFO, m_connectedLabel.Text,
                            "not correctly connected");
            m_disconnectbutton.Click();
            Assert.AreEqual(Chatform.NOT_CONNECTED_INFO, m_connectedLabel.Text,
                            "not correctly disconnected");
	    m_connectButton.Click();
        }

        [Test]
        public void TestSendMessage() {
            string msg = "abcd";
            m_messageTextbox.Enter(msg);
	    m_sendMsgButton.Click();
            // can't verify the result; BeginInvoke in message listener seems not to work with NUnit Forms
            /* Thread.Sleep(2000);
            ListBox.ObjectCollection items =
                (ListBox.ObjectCollection)m_messagesTester.Properties.Items;
            Assert.AreEqual(1, items.Count, "number of items");
            Assert.AreEqual(items[0], "wrong item", msg); */
            
        }

        [Test]
        public void TestSendMessageAsync() {
            string msg = "abcde";
            m_messageTextbox.Enter(msg);
            m_sendMsgAsyncButton.Click();            
            // can't verify the result; BeginInvoke in message listener seems not to work with NUnit Forms             
            /* Thread.Sleep(2000);
            ListBox.ObjectCollection items =
                (ListBox.ObjectCollection)m_messagesTester.Properties.Items;
            Assert.AreEqual(1, items.Count, "number of items");
            Assert.AreEqual(msg, items[0], "wrong item"); */
        }

    }

}

