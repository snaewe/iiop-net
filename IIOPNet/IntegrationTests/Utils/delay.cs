using System;
using System.Threading;

public class Delay {
	public static void Main(String[] args) {
		if (args.Length != 1) {
			Console.WriteLine("Usage:");
			Console.WriteLine("Delay seconds");
			Environment.Exit(2);
		} else {
			int s = Int32.Parse(args[0]);
			if (s <= 0) {
				Console.WriteLine("Usage:");
				Console.WriteLine("Delay seconds");
				Environment.Exit(2);
			} else {
				Thread.Sleep(TimeSpan.FromSeconds(s));
			}
		}
	}
}

