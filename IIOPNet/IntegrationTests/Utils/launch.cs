/* Launch.cs
 * 
 * Project: IIOP.NET
 * Utils
 * 
 * WHEN      RESPONSIBLE
 * 20.08.03  Patrik Reali (PRR), patrik.reali -at- elca.ch
 * 
 * Copyright 2003 Patrik Reali
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
using System.Diagnostics;

public class Launch {
	public static void Main(String[] args) {
		ProcessStartInfo startInfo = null;
		if (args.Length <= 0) {
			Console.WriteLine("Usage:");
			Console.WriteLine("Lauch name [args]");
			Environment.Exit(2);
		} else if (args.Length == 1) {
			startInfo = new ProcessStartInfo(args[0]);
		} else {
			String parameters = String.Join(" ", args, 1, args.Length-1);
			startInfo = new ProcessStartInfo(args[0], parameters);
		}
		startInfo.WindowStyle = ProcessWindowStyle.Minimized;
		Process p = Process.Start(startInfo);
		Console.WriteLine(p.Id.ToString());

	}
}
