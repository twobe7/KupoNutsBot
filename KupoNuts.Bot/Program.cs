// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Characters;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Events;
	using KupoNuts.Bot.Lodestone;
	using KupoNuts.Bot.Polls;
	using KupoNuts.Bot.Services;
	using KupoNuts.Bot.Status;

	public class Program
	{
		private static List<ServiceBase> services = new List<ServiceBase>();
		private static bool exiting = false;

		private static DiscordSocketClient? client;

		public static DiscordSocketClient DiscordClient
		{
			get
			{
				if (client is null)
					throw new Exception("No Discord client");

				return client;
			}
		}

		public static bool Running
		{
			get;
			private set;
		}

		public static Task Run(string[] args)
		{
			Log.ExceptionLogged += Log_ExceptionLogged;

			Program prog = new Program();
			return prog.DoRun();
		}

		public static async Task Exit()
		{
			exiting = true;

			while (Running)
			{
				await Task.Yield();
			}
		}

		protected virtual async Task AddServices()
		{
			try
			{
				await this.AddService<LogService>();
				await this.AddService<EventsService>();
				await this.AddService<CalendarService>();
				await this.AddService<CommandsService>();
				await this.AddService<HelpService>();
				await this.AddService<DebugService>();
				await this.AddService<StatusService>();
				await this.AddService<ReminderService>();
				await this.AddService<EchoService>();
				await this.AddService<KarmaService>();
				await this.AddService<PollService>();
				await this.AddService<CharacterService>();
				await this.AddService<LodestoneService>();
				await this.AddService<QuoteService>();
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private static void Log_ExceptionLogged(string exceptionLog)
		{
			if (Program.DiscordClient != null)
			{
				try
				{
					string? idStr = Settings.Load().LogChannel;
					if (idStr != null)
					{
						ulong id = ulong.Parse(idStr);
						SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(id);
						EmbedBuilder enbedBuilder = new EmbedBuilder();
						enbedBuilder.Color = Color.Red;
						enbedBuilder.Title = "Kupo Nut Bot encountered an error";
						enbedBuilder.Description = exceptionLog;
						enbedBuilder.Timestamp = DateTimeOffset.UtcNow;
						channel.SendMessageAsync(null, false, enbedBuilder.Build());
					}
				}
				catch (Exception)
				{
				}
			}
		}

		private async Task DoRun()
		{
			try
			{
				Running = true;
				Log.Write("Kupo Nuts Bot booting..");

				string? token = Settings.Load().Token;

				if (string.IsNullOrEmpty(token))
				{
					Log.Write("No token set. Please set a token in the Database file");
				}
				else
				{
					client = new DiscordSocketClient();

					bool ready = false;
					DiscordClient.Log += this.LogAsync;

					DiscordClient.Ready += () =>
					{
						ready = true;
						return Task.CompletedTask;
					};

					await DiscordClient.LoginAsync(TokenType.Bot, token);
					await DiscordClient.StartAsync();

					while (!ready)
						await Task.Yield();

					// Write channels to the database
					Database<Channel> channelsDb = new Database<Channel>("Channels", 1);
					await channelsDb.Connect();
					foreach (SocketGuild guild in DiscordClient.Guilds)
					{
						foreach (SocketGuildChannel channel in guild.Channels)
						{
							Channel.Types type = Channel.Types.Unknown;
							if (channel is SocketTextChannel)
								type = Channel.Types.Text;

							if (channel is SocketVoiceChannel)
								type = Channel.Types.Voice;

							Channel record = await channelsDb.CreateEntry(guild.Id + "_" + channel.Id);
							record.DiscordId = channel.Id.ToString();
							record.Name = channel.Name;
							record.Type = type;
							await channelsDb.Save(record);
						}
					}

					// boot the rest of the bot
					await this.AddServices();

					while (!exiting)
					{
						await Task.Yield();
						Thread.Sleep(100);
					}
				}

				Log.Write("shutting down");

				if (services != null)
				{
					foreach (ServiceBase service in services)
					{
						await service.Shutdown();
					}
				}

				DiscordClient?.Dispose();
				Running = false;
				Log.Write("shutdown complete");
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task AddService<T>()
			where T : ServiceBase
		{
			T service = Activator.CreateInstance<T>();
			await service.Initialize();
			services.Add(service);
		}

		private Task LogAsync(LogMessage log)
		{
			Log.Write(log.ToString());
			return Task.CompletedTask;
		}
	}
}
