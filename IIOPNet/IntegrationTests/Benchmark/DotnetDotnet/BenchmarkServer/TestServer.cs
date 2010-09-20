/* TestServer.cs
 *
 * Project: IIOP.NET
 * Benchmarks
 *
 * WHEN      RESPONSIBLE
 * 20.05.04  Patrik Reali (PRR), patrik.reali -at- elca.ch
 * 20.05.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace Ch.Elca.Iiop.Benchmarks {


    public class TestServer {

        public static void Main(String[] args) {
            // register the channel
            int port = 8087;
            IDictionary dict = new Hashtable();
            dict["port"] = port;
            string endian = "BigEndian";
            if (args.Length > 0) {
                endian = args[0];
            }
            dict["endian"] = endian;
            IiopChannel chan = new IiopChannel(dict);
            ChannelServices.RegisterChannel(chan, false);

            TestServiceImpl test = new TestServiceImpl();
            string objectURI = "test";
            RemotingServices.Marshal(test, objectURI);

            Console.WriteLine("server running");
            Thread.Sleep(Timeout.Infinite);
        }

    }

}

