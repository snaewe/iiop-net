/* NClient.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel Tutorial
 * 
 * Copyright 2004 Patrik Reali
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
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;
using tutorial;

namespace Tutorial {

    public class NClient {

        [STAThread]
        public static void Main(string[] args) {
            try {
                string nameServiceHost = "localhost";
                int nameServicePort = 1050;
                parseArgs(ref nameServiceHost, ref nameServicePort, args);

                // register the channel
                IiopClientChannel channel = new IiopClientChannel();
                ChannelServices.RegisterChannel(channel, false);

                // access COS naming service
                CorbaInit init = CorbaInit.GetInit();
                NamingContext nameService = init.GetNameService(nameServiceHost, nameServicePort);
                NameComponent[] name = new NameComponent[] { new NameComponent("service", "") };
                // get the reference to the adder
                Service service = (Service)nameService.resolve(name);
                // call fail
                service.fail();
	    } catch (CustomEx je) {
                Console.WriteLine("Java-side exception: {0}\nReason: {1}", je.value.message, je.value.reason);
            } catch (Exception e) {
                Console.WriteLine("exception: " + e);
            }
        }

        private static void parseArgs(ref string host, ref int port, string[] args) {
            if (args.Length > 0) {
                host = args[0];
            }
            if (args.Length > 1) {
                port = Int32.Parse(args[1]);
            }
        }

    }
}
