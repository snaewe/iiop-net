using System;
using System.Diagnostics;

public class Launch {
	public static void Main(String[] args) {
		if (args.Length <= 0) {
			Console.WriteLine("Usage:");
			Console.WriteLine("Lauch name [args]");
			Environment.Exit(2);
		} else if (args.Length == 1) {
			Process p = Process.Start(args[0]);
			Console.WriteLine(p.Id.ToString());
		} else {
			String parameters = String.Join(" ", args, 1, args.Length-1);
			Process p = Process.Start(args[0], parameters);
			Console.WriteLine(p.Id.ToString());
		}
	}
}
