// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNutsBot.Events;
	using KupoNutsBot.Services;
	using KupoNutsBot.Status;

	public class Program
	{
		private static List<ServiceBase> services = new List<ServiceBase>();

		public static DiscordSocketClient DiscordClient
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

		protected virtual async Task AddServices()
		{
			try
			{
				await AddService<StatusService>();
				await AddService<CommandsService>();
				await AddService<DebugService>();
				await AddService<EventsService>();
				await AddService<EchoService>();
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task Run()
		{
			Log.Write("Kupo Nuts Bot booting.. Press [ESC] to shutdown");

			Database.Load();

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

			bool quit = false;
			while (!quit)
			{
				await Task.Yield();

				if (Console.ReadKey(true).Key == ConsoleKey.Escape)
				{
					Log.Write("Kupo Nuts Bot is shutting down");
					quit = true;
				}
			}

			foreach (ServiceBase service in services)
			{
				await service.Shutdown();
			}

			DiscordClient.Dispose();
		}

		private Task LogAsync(LogMessage log)
		{
			Log.Write(log.ToString());
			return Task.CompletedTask;
		}
	}
}
