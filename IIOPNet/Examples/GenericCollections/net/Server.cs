/* Server.cs
 * 
 * Project: IIOP.NET
 * Examples
 * 
 * WHEN      RESPONSIBLE
 * 25.05.03  Patrik Reali (PRR), patrik.reali -at- elca.ch
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

namespace Ch.Elca.Iiop.Demo.StorageSystem {

public class Server {

        [STAThread]
        public static void Main(string[] args) {
            // register the channel
            int port = 8087;
            IiopChannel chan = new IiopChannel(port);
            ChannelServices.RegisterChannel(chan, false);

            // publish the storage service manager
            Manager manager = new Manager();
            string objectURI = "storagemanager";
            RemotingServices.Marshal(manager, objectURI);
			
            Console.WriteLine("server running");
            Console.WriteLine("press any key to terminate....");
            Console.ReadLine();
        }

    }	

}
