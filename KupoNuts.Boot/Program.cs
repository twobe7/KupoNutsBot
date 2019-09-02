// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Boot
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using KupoNuts.Bot;
	using XIVAPI;

	public class Program
	{
		private static Task? botTask;
		private static Task? managerTask;

		private static bool Running
		{
			get
			{
				if ((botTask == null || botTask.IsCompleted) && (managerTask == null || managerTask.IsCompleted))
					return false;

				return true;
			}
		}

		public static void Main(string[] args)
		{
			Task task = Task.Run(async () => { await MainAsync(args); });
			task.Wait();
		}

		private static async Task MainAsync(string[] args)
		{
			Console.WriteLine("[Boot] Initializing... press [ESC] to shutdown");

			try
			{
				botTask = Bot.Program.Run(args);
				managerTask = Manager.Server.Program.Run(args);

				while (Running)
				{
					await Task.Yield();
					Thread.Sleep(100);

					if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
					{
						Console.WriteLine("[Boot] Shutting down...");
						await Bot.Program.Exit();
						await Manager.Server.Program.Exit();
					}
				}
			}
			catch (Exception? ex)
			{
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
			}

			Console.WriteLine("[Boot] Shutdown complete");
		}
	}
}
