/* TestServer.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 08.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Reflection;
using Ch.Elca.Iiop.Idl;

namespace Ch.Elca.Iiop.IntegrationTests.MappingPlugin {


    public class TestServer {

        public static void Main(String[] args) {
            InitCustomMapping();

            // register the channel
            int port = 8087;
            IiopChannel chan = new IiopChannel(port);
            ChannelServices.RegisterChannel(chan, false);

            TestServiceImpl test = new TestServiceImpl();
            string objectURI = "test";
            RemotingServices.Marshal(test, objectURI);

            Console.WriteLine("server running");
            Console.ReadLine();
        }
        
        /// <summary>
        /// initalizes the custom mapping from assembly manifest stream
        /// </summary>
        private static void InitCustomMapping() {
            string resourceName = "customMapping.xml";
            // load from asm
            Assembly asm = (typeof(TestServer)).Assembly;
            Stream configStream = asm.GetManifestResourceStream(resourceName);
            if (configStream == null) {
                Console.WriteLine("custom mapper not loadable, {0} not found in assembly", resourceName);
                Environment.Exit(2);
            }
            try {
                CustomMapperRegistry reg = CustomMapperRegistry.GetSingleton();
                reg.AddMappingFromStream(configStream);
            } catch (Exception e) {
                Console.WriteLine("custom mapper not loadable, exception: {0}", e);
                Environment.Exit(2);
            }        
        }

    }

}
