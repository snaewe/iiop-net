/* NClient.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel Tutorial
 * 
 * WHEN      RESPONSIBLE
 * 29.12.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.IO;
using System.Collections;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Security.Ssl;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Tutorial.GettingStarted {

    public class NClient {

        [STAThread]
        public static void Main(string[] args) {
            try {
                Console.WriteLine("input the two summands");
                Console.WriteLine("sum1:");
                double sum1 = Double.Parse(Console.ReadLine());
                Console.WriteLine("sum2:");
                double sum2 = Double.Parse(Console.ReadLine());

                string fileName = @"..\server\ior";
                if (args.Length > 0) {
                    fileName = args[0];
                }
                TextReader reader = new StreamReader(fileName);
                string ior = reader.ReadLine();
                reader.Close();
                Console.WriteLine("use ior: " + ior.ToString());

                IDictionary props = new Hashtable();
                props[IiopChannel.CHANNEL_NAME_KEY] = "IiopClientChannelSsl";
                props[IiopChannel.TRANSPORT_FACTORY_KEY] =
                   "Ch.Elca.Iiop.Security.Ssl.SslTransportFactory,SSLPlugin";
            
                props[SslTransportFactory.CLIENT_AUTHENTICATION] = 
                    "Ch.Elca.Iiop.Security.Ssl.ClientMutualAuthenticationSuitableFromStore,SSLPlugin";
                // take certificates from the windows certificate store of the current user
                props[ClientMutualAuthenticationSuitableFromStore.STORE_LOCATION] =
                    "CurrentUser";
                // the expected CN property of the server key
                props[DefaultClientAuthenticationImpl.EXPECTED_SERVER_CERTIFICATE_CName] = 
                    "IIOP.NET demo server";
            
                // register the channel
                IiopClientChannel channel = new IiopClientChannel(props);
                ChannelServices.RegisterChannel(channel, false);

                // get the reference to the adder
                Adder adder = (Adder)RemotingServices.Connect(typeof(Adder), ior);
                // call add
                double result = adder.add(sum1, sum2);
                Console.WriteLine("result: " + result);
            } catch (Exception e) {
                Console.WriteLine("exception: " + e);
            }
        }

    }
}