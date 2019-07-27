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
			await AddService<StatusService>();
			await AddService<EventsService>();
		}

		private async Task Run()
		{
			Console.WriteLine("Kupo Nuts Bot booting.. Press [ESC] to shutdown");

			Database.Load();

			DiscordClient = new DiscordSocketClient();

			bool ready = false;
			DiscordClient.Log += this.LogAsync;
			DiscordClient.MessageReceived += this.MessageReceivedAsync;

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
			Console.WriteLine("Connected:");
			foreach (SocketGuild guild in DiscordClient.Guilds)
			{
				Console.WriteLine("    " + guild.Name);
				foreach (SocketGuildChannel channel in guild.Channels)
				{
					if (channel is SocketTextChannel textChannel)
					{
						Console.WriteLine("         " + channel.Name + ":" + channel.Id);
					}
				}
			}

			bool quit = false;
			while (!quit)
			{
				await Task.Yield();

				if (Console.ReadKey(true).Key == ConsoleKey.Escape)
				{
					Console.WriteLine("Kupo Nuts Bot is shutting down");
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
			Console.WriteLine(log.ToString());
			return Task.CompletedTask;
		}

		// This is not the recommended way to write a bot - consider
		// reading over the Commands Framework sample.
		private async Task MessageReceivedAsync(SocketMessage message)
		{
			// Ignore messages that did not come from users
			if (!(message is SocketUserMessage))
				return;

			// The bot should never respond to itself.
			if (message.Author.Id == DiscordClient.CurrentUser.Id)
				return;

			if (message.Content == "*kupo bind")
			{
				Console.WriteLine("Binding Chanel: " + message.Channel.Id);
				await message.Channel.SendMessageAsync("pong!");
			}
		}
	}
}
