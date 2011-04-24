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
using System.IO;

public class Launch {
    public static void Main(String[] args) {        
        try {
            ProcessStartInfo startInfo = ParseArgs(args);
        
            Process p = Process.Start(startInfo);
            Console.WriteLine(p.Id.ToString());
        } catch (Exception e) {
            Console.WriteLine("Exception while trying to start app: " + e);
        }
    }
    
    public static ProcessStartInfo ParseArgs(String[] args) {
        int i = 0;
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;

        while ((i < args.Length) && (args[i].StartsWith("-"))) {
            if (args[i].Equals("-h")) {
                HowTo();
                Environment.Exit(0);
            } else if (args[i].Equals("-d")) {
                i++;
                if (i == args.Length) {
                    Error("Error: option argument missing for -d option");
                }
                startInfo.WorkingDirectory = new DirectoryInfo(args[i++]).FullName;
            } else if (args[i].Equals("-w")) {
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                i++;
            } else {
                Error(String.Format("Error: invalid option {0}", args[i]));
            }
        }

        if (args.Length - i <= 0) {
            Error("Error: programm to launch is missing");
        } else if (args.Length - i == 1) {
            startInfo.FileName = args[i];
        } else {
            startInfo.FileName = args[i];
            i++;
            String parameters = String.Join(" ", args, i, (args.Length-i));
            startInfo.Arguments = parameters;
        }

        return startInfo;
    
    }
    
    public static void HowTo() {
        Console.WriteLine("Usage:");
        Console.WriteLine("Lauch [options] name [args]");
        Console.WriteLine();
        Console.WriteLine("options are:");
        Console.WriteLine("-h              help");
        Console.WriteLine("-d directory    the working directory for programm to start");
        Console.WriteLine("-w              start not minimized, but normal");
    }
    
    public static void Error(String message) {
        Console.WriteLine(message);
        Console.WriteLine();
        HowTo();
        Environment.Exit(2);
    }
}
