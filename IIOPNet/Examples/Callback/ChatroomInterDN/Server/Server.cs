/* Server.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel Tutorial
 * 
 * WHEN      RESPONSIBLE
 * 09.09.03  Dominic Ullmann (DUL), dul@elca.ch
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using Ch.Elca.Iiop;


namespace Ch.Elca.Iiop.Demo.Chatroom {

    /// <summary>
    /// publishes the service.
    /// </summary>
    public class Server {

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args) {
			// register the channel
			int port = 8087;
                        if (args.Length > 0) {
                            port = Int32.Parse(args[0]);
                        }
			IiopChannel chan = new IiopChannel(port);
			ChannelServices.RegisterChannel(chan, false);
		
			ChatroomImpl chatroom = new ChatroomImpl();
			string objectURI = "chatroom";
			RemotingServices.Marshal(chatroom, objectURI);
			
			Console.WriteLine("server running");
			Console.ReadLine();
		}
    }

}

