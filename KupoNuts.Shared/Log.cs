// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.Text;

	public static class Log
	{
		public delegate void LogEvent(string str);

		public static event LogEvent? MessageLogged;

		public static event LogEvent? ExceptionLogged;

		public static void Write(string message, string category = "Bot")
		{
			string str = "[" + DateTime.Now.ToString("HH:mm:ss") + "][" + category + "] " + message;
			Console.WriteLine(str);
			MessageLogged?.Invoke(str);
		}

		public static void Write(Exception? ex)
		{
			if (ex == null)
				return;

			StringBuilder builder = new StringBuilder();
			while (ex != null)
			{
				builder.Append(ex.GetType());
				builder.Append(" - ");
				builder.AppendLine(ex.Message);
				builder.AppendLine(ex.StackTrace);
				builder.AppendLine();

				ex = ex.InnerException;
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(builder.ToString());
			Console.ForegroundColor = ConsoleColor.White;

			ExceptionLogged?.Invoke(builder.ToString());
		}
	}
}
