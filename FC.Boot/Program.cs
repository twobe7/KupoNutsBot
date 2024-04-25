// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Boot;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
	private static Task? botTask;
	////private static Task? managerTask;

	private static bool Running
	{
		get
		{
			if (botTask == null || botTask.IsCompleted) // && (managerTask == null || managerTask.IsCompleted))
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
			////managerTask = Manager.Web.Run.RunApp(args);

			StringBuilder input = new ();
			while (Running)
			{
				await Task.Yield();
				Thread.Sleep(100);

				ConsoleKeyInfo consoleKey = Console.ReadKey(true);

				if (consoleKey.Key == ConsoleKey.Escape)
				{
					Log.Write("Shutting down...", "Boot");
					await Bot.Program.Exit();
					////await Manager.Server.Program.Exit();
				}
				else if (consoleKey.Key == ConsoleKey.Enter)
				{
					Console.WriteLine();
					await HandleInput(input.ToString());
					input.Clear();
				}
				else
				{
					input.Append(consoleKey.KeyChar);
					Console.Write(consoleKey.KeyChar);
				}
			}
		}
		catch (Exception? ex)
		{
			Log.Write(ex);
		}

		Log.Write("Shutdown complete", "Boot");
	}

	private static Task HandleInput(string input)
	{
		return Task.CompletedTask;
	}
}
