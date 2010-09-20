/* Client.cs
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 22.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace ch.elca.iiop.demo.ejbChatroom {

    public class Client {

        [STAThread]
        public static void Main(string[] args) {
            try {                
                string nameserviceLoc = "corbaloc::localhost:3528/JBoss/Naming/root";
                // the port the callback is listening on
                int callbackPort = 0; // auto assign
                if (args.Length > 0) {
                	nameserviceLoc = args[0];
                }
                if (args.Length > 1) {
                    callbackPort = Int32.Parse(args[1]);
                }
            
                IiopChannel channel = new IiopChannel(callbackPort);
                ChannelServices.RegisterChannel(channel, false);

                NamingContext nameService = (NamingContext)RemotingServices.Connect(typeof(NamingContext), nameserviceLoc);

                NameComponent[] name = new NameComponent[] { new NameComponent("demo", ""),
                                                             new NameComponent("chatroomHome", "") };
                // get the chatroom home interface
                ChatroomHome home = (ChatroomHome) nameService.resolve(name);
                Chatroom chatroom = home.create();

	        Application.Run(new Chatform(chatroom));

            } catch (Exception e) {
                Console.WriteLine("exception: " + e);
            }
        }
    }
}
