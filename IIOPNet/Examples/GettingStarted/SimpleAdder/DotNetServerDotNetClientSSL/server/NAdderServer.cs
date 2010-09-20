/* NAdderServer.cs
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Collections;
using System.IO;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Security.Ssl;
using omg.org.CORBA;


namespace Ch.Elca.Iiop.Tutorial.GettingStarted {

    /// <summary>
    /// The adder implementation class
    /// </summary>
    public class AdderImpl : MarshalByRefObject, Adder {
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }
        
        public double add(double arg1, double arg2) {
            return arg1 + arg2;
        }

    }

    /// <summary>
    /// publishes the service.
    /// </summary>
    public class NAdderServer {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args) {
            // register the channel            
            IDictionary props = new Hashtable();
            props[IiopChannel.CHANNEL_NAME_KEY] = "securedServerIiopChannel";            
            props[IiopServerChannel.PORT_KEY] = "8087";            
            props[IiopChannel.TRANSPORT_FACTORY_KEY] = 
                "Ch.Elca.Iiop.Security.Ssl.SslTransportFactory,SSLPlugin";
 
            props[SslTransportFactory.SERVER_REQUIRED_OPTS] = "96";
            props[SslTransportFactory.SERVER_SUPPORTED_OPTS] = "96";
            props[SslTransportFactory.SERVER_AUTHENTICATION] = 
                "Ch.Elca.Iiop.Security.Ssl.DefaultServerAuthenticationImpl,SSLPlugin";

            props[DefaultServerAuthenticationImpl.SERVER_CERTIFICATE] = 
                "5f4abc1aad19e53857be2a4bbec9297091f0082c";
            props[DefaultServerAuthenticationImpl.STORE_LOCATION] = "CurrentUser";

            IiopChannel chan = new IiopChannel(props);
            ChannelServices.RegisterChannel(chan, false);
        
            AdderImpl adder = new AdderImpl();
            string objectURI = "adder";
            RemotingServices.Marshal(adder, objectURI);
            
            // write out ior to file
            OrbServices orb = OrbServices.GetSingleton();
            string ior = orb.object_to_string(adder);

            TextWriter writer = new StreamWriter(@"ior");
            writer.WriteLine(ior);
            writer.Close();
            Console.WriteLine("server ior: " + ior.ToString());

            Console.WriteLine("server running");
            Thread.Sleep(Timeout.Infinite);
        }
    }

}

