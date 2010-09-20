/* Client.cs
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 24.06.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;
using ch.elca.iiop.demo.ejbAdder;

namespace Ch.Elca.Iiop.Demo.EjbAdder {

    public class Client {

        [STAThread]
        public static void Main(string[] args) {
            try {
                string nameServiceHost = "localhost";
                int nameServicePort = 1050;
                parseArgs(ref nameServiceHost, ref nameServicePort, args);

                Console.WriteLine("input the two summands");
                Console.WriteLine("sum1:");
                double sum1 = Double.Parse(Console.ReadLine());
                Console.WriteLine("sum2:");
                double sum2 = Double.Parse(Console.ReadLine());

                // register the channel
                IiopClientChannel channel = new IiopClientChannel();
                ChannelServices.RegisterChannel(channel, false);

                // access COS nameing service
                RmiIiopInit init = new RmiIiopInit(nameServiceHost, nameServicePort);
                NamingContext nameService = init.GetNameService();
                NameComponent[] name = new NameComponent[] { new NameComponent("ch.elca.iiop.demo.ejbAdder.AdderHome", "") };
                // get the reference to the adder-home
                AdderHome adderHome = (AdderHome)nameService.resolve(name);
                // create Adder
                Adder adder = adderHome.create();
                // call add
                double result = adder.add(sum1, sum2);
                Console.WriteLine("result: " + result);
                // dispose the ejb
		adder.remove();
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