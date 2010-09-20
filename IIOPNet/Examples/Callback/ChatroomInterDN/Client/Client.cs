/* Client.cs
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 09.09.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Windows.Forms;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;

namespace Ch.Elca.Iiop.Demo.Chatroom {

    public class Client {

        #region IFields

        private IChatroom m_chatroom;
        private IiopChannel m_channel;

        private int m_callbackPort;
        private string m_nameServiceHost;
        private int m_nameServicePort;

        #endregion IFields
        #region IConstructors

        public Client(string nameServiceHost, int nameServicePort,
                      int callbackPort) {
            m_nameServiceHost = nameServiceHost;
            m_nameServicePort = nameServicePort;
            m_callbackPort = callbackPort;
            Setup();
        }

        public Client(string[] args) {
            ParseArgs(args);
            Setup();
        }

        #endregion IConstructors
        #region IProperties
        
        protected IChatroom Chatroom {
            get {
                return m_chatroom;
            }
        }

        #endregion IProperties
        #region IMethods

        private void SetupChannel(int callbackPort) {
            m_channel = new IiopChannel(callbackPort);
            ChannelServices.RegisterChannel(m_channel, false);
        }

        private void RetrieveChatRoom(string nameServiceHost, 
                                      int nameServicePort) {
            CorbaInit init = CorbaInit.GetInit();
            NamingContext nameService = (NamingContext)init.GetNameService(nameServiceHost, nameServicePort);

            NameComponent[] name = new NameComponent[] { new NameComponent("chatroom", "") };
            // get the chatroom
            m_chatroom = (IChatroom) nameService.resolve(name);
        }

        private void ParseArgs(string[] args) {
            m_nameServiceHost = "localhost";
            m_nameServicePort = 8087;
            if (args.Length > 0) {
                m_nameServiceHost = args[0];
            }
            if (args.Length > 1) {
                m_nameServicePort = Int32.Parse(args[1]);
            }
            // the port the callback is listening on
            m_callbackPort = 0; // auto assign
            if (args.Length > 2) {
                m_callbackPort = Int32.Parse(args[2]);
            }
        }

        private void Setup() {
            SetupChannel(m_callbackPort);
            RetrieveChatRoom(m_nameServiceHost, m_nameServicePort);
        }

        public void TearDown() {
            if (m_channel != null) {
                ChannelServices.UnregisterChannel(m_channel);
                m_channel = null;
            }
        }

        public Chatform CreateChatForm() {
            Chatform form = new Chatform();
            form.Chatroom = Chatroom;            
            return form;
        }

        public void Run() {
            try {
                Chatform form = CreateChatForm();
                Application.Run(form);
            } finally {
                TearDown();
            }
        }

        #endregion IMethods
        #region SMethods

        [STAThread]
        public static void Main(string[] args) {
            try {           
                Client client = new Client(args);
                client.Run();
            } catch (Exception e) {
                Console.WriteLine("exception: " + e);
            }
        }

        #endregion SMethods

    }
}
