/* NAdderServer.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel Tutorial
 * 
 * WHEN      RESPONSIBLE
 * 14.09.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2005 Dominic Ullmann
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
using omg.org.CosNaming;


namespace Ch.Elca.Iiop.Tutorial.GettingStarted {

    /// <summary>
    /// publishes the service.
    /// </summary>
    public class NAdderServer {

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args) {
                        if (args.Length != 1) {
                            Console.WriteLine("Please specify the nameservice url either as ior or corbaloc");
                        }
                        string nameServiceUrl = args[0];

			// register the channel
			int port = 8087;
			IiopChannel chan = new IiopChannel(port);
			ChannelServices.RegisterChannel(chan, false);
		
			AdderImpl adder = new AdderImpl();
			string objectURI = "adder";
			RemotingServices.Marshal(adder, objectURI);

			// publish the adder with an external name service
                        NamingContext nameService = (NamingContext)RemotingServices.Connect(typeof(NamingContext),
                                                                                            nameServiceUrl);
			NameComponent[] name = new NameComponent[] { new NameComponent("adder") };
			nameService.bind(name, adder);
			Console.WriteLine("server running");
			Console.ReadLine();

			// unpublish with external name service
                        nameService.unbind(name);
		}
    }

}

