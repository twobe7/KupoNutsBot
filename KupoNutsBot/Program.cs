// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNutsBot.Commands;
	using KupoNutsBot.Events;
	using KupoNutsBot.Services;
	using KupoNutsBot.Status;

	public class Program
	{
		private static List<ServiceBase> services = new List<ServiceBase>();
		private static bool exiting = false;

		public static DiscordSocketClient DiscordClient
		{
			get;
			private set;
		}

		public static bool Running
		{
			get;
			private set;
		}

		public static void Main(string[] args)
		{
			Program prog = new Program();
			Task task = Task.Run(prog.Run);
			task.Wait();
		}

		public static async Task AddService<T>()
			where T : ServiceBase
		{
			T service = Activator.CreateInstance<T>();
			await service.Initialize();
			services.Add(service);
		}

		public static void Exit()
		{
			exiting = true;
		}

		protected virtual async Task AddServices()
		{
			try
			{
				await AddService<ManagerService>();
				await AddService<CommandsService>();
				await AddService<UpdateService>();
				await AddService<DebugService>();
				await AddService<StatusService>();
				await AddService<EventsService>();
				await AddService<ReminderService>();
				await AddService<EchoService>();
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task Run()
		{
			Running = true;
			Log.Write("Kupo Nuts Bot booting.. Press [ESC] to shutdown");

			Database.Load();

			if (string.IsNullOrEmpty(Database.Instance.Token))
			{
				Log.Write("No token set. Please set a token in the Database file");
				return;
			}

			DiscordClient = new DiscordSocketClient();

			bool ready = false;
			DiscordClient.Log += this.LogAsync;

			DiscordClient.Ready += () =>
			{
				ready = true;
				return Task.CompletedTask;
			};

			await DiscordClient.LoginAsync(TokenType.Bot, Database.Instance.Token);
			await DiscordClient.StartAsync();

			while (!ready)
				await Task.Yield();

			await this.AddServices();

			// now we are ready to go
			await Task.Delay(100);
			Log.Write("Connected:");
			foreach (SocketGuild guild in DiscordClient.Guilds)
			{
				Log.Write("    " + guild.Name);
				foreach (SocketGuildChannel channel in guild.Channels)
				{
					if (channel is SocketTextChannel textChannel)
					{
						Log.Write("         " + channel.Name + ":" + channel.Id);
					}
				}
			}

			while (!exiting)
			{
				await Task.Yield();
				Thread.Sleep(100);

				if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
				{
					exiting = true;
				}
			}

			Log.Write("Kupo Nuts Bot is shutting down");

			foreach (ServiceBase service in services)
			{
				await service.Shutdown();
			}

			DiscordClient.Dispose();
			Running = false;
			Log.Write("Kupo Nuts Bot has shut down");
		}

		private Task LogAsync(LogMessage log)
		{
			Log.Write(log.ToString());
			return Task.CompletedTask;
		}
	}
}
