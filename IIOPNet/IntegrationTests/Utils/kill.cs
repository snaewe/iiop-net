using System;
using System.Diagnostics;

public class Kill {
	public static void Main(String[] args) {
		if (args.Length != 1) {
			Console.WriteLine("Usage:");
			Console.WriteLine("Kill pid");
			Environment.Exit(2);
		} else {
			Process p = Process.GetProcessById(Int32.Parse(args[0]));
			p.Kill();
		}
	}
}
