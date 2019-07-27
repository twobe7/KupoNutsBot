// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class Log
	{
		public static void Write(string message)
		{
			Console.WriteLine(message);
		}

		public static void Write(Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
