// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Actions;
	using FC.Bot.Characters;
	using FC.Bot.Commands;
	using FC.Bot.ContentCreators;
	using FC.Bot.Events;
	using FC.Bot.Events.Services;
	using FC.Bot.Guild;
	using FC.Bot.Housing;
	using FC.Bot.Items;
	using FC.Bot.Lodestone;
	using FC.Bot.Mounts;
	using FC.Bot.Polls;
	using FC.Bot.Quotes;
	using FC.Bot.RPG;
	using FC.Bot.Services;
	using FC.Bot.Status;

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

		public static bool Initializing
		{
			get;
			private set;
		}

		public static Task Run(string[] args)
		{
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

		public static async Task<IGuildUser> GetBotUserForGuild(IGuild guild)
		{
			return await guild.GetUserAsync(DiscordClient.CurrentUser.Id);
		}

		protected virtual async Task AddServices()
		{
			try
			{
				// Dependencies - must be added in order
				await this.AddService<SettingsService>();
				await this.AddService<CommandsService>();
				await this.AddService<ScheduleService>();
				await this.AddService<EventsService>();
				await this.AddService<Eventsv2.EventsService>();
				await this.AddService<CalendarService>();
				await this.AddService<UserService>();

				// No dependencies
				await this.AddService<ActionService>();
				await this.AddService<CharacterService>();
				await this.AddService<ChannelService>();
				await this.AddService<ChannelOptInService>();
				await this.AddService<ContentCreatorService>();
				await this.AddService<CurrencyService>();
				await this.AddService<DebugService>();
				await this.AddService<EchoService>();
				await this.AddService<FashionReportService>();
				await this.AddService<GuildService>();
				await this.AddService<HelpService>();
				await this.AddService<HousingService>();
				await this.AddService<ItemService>();
				await this.AddService<LodestoneService>();
				await this.AddService<LogService>();
				await this.AddService<MeService>();
				await this.AddService<ModerationService>();
				await this.AddService<MountService>();
				await this.AddService<NoveltyService>();
				await this.AddService<PollService>();
				await this.AddService<QuoteService>();
				await this.AddService<ReactionRoleService>();
				await this.AddService<RPGService>();
				await this.AddService<StatusService>();
				await this.AddService<VoiceService>();
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private static void Log_ExceptionLogged(string exceptionLog)
		{
			if (client != null)
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
				Initializing = true;
				Log.Write("FCChan Bot booting...", "Bot");

				string? token = Settings.Load().Token;

				if (string.IsNullOrEmpty(token))
				{
					Log.Write(new Exception("No token set. Please set a token in the Settings file"));
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

					// boot the rest of the bot
					await this.AddServices();

					// Update all users in guilds
					await this.PopulateGuildUsers();

					Initializing = false;
					Log.Write("Initialization complete", "Bot");

					while (!exiting)
					{
						await Task.Yield();
						Thread.Sleep(100);
					}
				}

				Log.Write("Shutting down", "Bot");

				if (services != null)
				{
					foreach (ServiceBase service in services)
					{
						await service.Shutdown();
					}
				}

				client?.Dispose();
				Running = false;
				Log.Write("Shutdown complete", "Bot");
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

		private async Task PopulateGuildUsers()
		{
			// Had issue where commands like Rep didn't recognise users
			// Using GetUsersAsync returned null
			// This worked in dev so we'll populate the user list on boot
			foreach (IGuild guild in Program.DiscordClient.Guilds)
			{
				await guild.DownloadUsersAsync();
			}
		}

		private Task LogAsync(LogMessage log)
		{
			Log.Write(log.ToString(), "Bot");
			return Task.CompletedTask;
		}
	}
}
