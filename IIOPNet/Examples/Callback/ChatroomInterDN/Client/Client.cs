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

        [STAThread]
        public static void Main(string[] args) {
            try {

                string nameServiceHost = "localhost";
                int nameServicePort = 8087;
                if (args.Length > 0) {
                    nameServiceHost = args[0];
                }
                if (args.Length > 1) {
                    nameServicePort = Int32.Parse(args[1]);
                }

                // the port the callback is listening on
                int callbackPort = 0; // auto assign
                if (args.Length > 2) {
                    callbackPort = Int32.Parse(args[2]);
                }
            
                IiopChannel channel = new IiopChannel(callbackPort);
                ChannelServices.RegisterChannel(channel);

                CorbaInit init = CorbaInit.GetInit();
                NamingContext nameService = (NamingContext)init.GetNameService(nameServiceHost, nameServicePort);

                NameComponent[] name = new NameComponent[] { new NameComponent("chatroom", "") };
                // get the chatroom
                IChatroom chatroom = (IChatroom) nameService.resolve(name);

                Application.Run(new Chatform(chatroom));

            } catch (Exception e) {
                Console.WriteLine("exception: " + e);
            }
        }
    }
}
