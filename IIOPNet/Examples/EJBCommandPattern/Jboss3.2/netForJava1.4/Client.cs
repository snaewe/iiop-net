/* Client.cs
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 14.03.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace ch.elca.iiop.demo.ejbCommand {

    public class Client {

        [STAThread]
        public static void Main(string[] args) {
            try {                
                string nameserviceLoc = "corbaloc::localhost:3528/JBoss/Naming/root";
                if (args.Length > 0) {
                	nameserviceLoc = args[0];
                }                
            
                IiopClientChannel channel = new IiopClientChannel();
                ChannelServices.RegisterChannel(channel, false);

                NamingContext nameService = (NamingContext)RemotingServices.Connect(typeof(NamingContext), nameserviceLoc);

                NameComponent[] name = new NameComponent[] { new NameComponent("demo", ""),
                                                             new NameComponent("commandTargetHome", "") };
                // get the command target home interface
                CommandTargetHome home = (CommandTargetHome) nameService.resolve(name);
                CommandTarget commandTarget = home.create();

                Application.Run(new Commandform(commandTarget));

            } catch (Exception e) {
                Console.WriteLine("exception: " + e);
            }
        }
    }
}
