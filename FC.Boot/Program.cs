// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Boot
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using FC.Bot;
	using FC.Events;
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
			Log.Write("Initializing... press [ESC] to shutdown", "Boot");

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
						Log.Write("Shutting down...", "Boot");
						await Bot.Program.Exit();
						await Manager.Server.Program.Exit();
					}
				}
			}
			catch (Exception? ex)
			{
				Log.Write(ex);
			}

			Log.Write("Shutdown complete", "Boot");
		}
	}
}
