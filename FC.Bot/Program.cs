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
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Actions;
	using FC.Bot.Characters;
	using FC.Bot.CommandModules;
	using FC.Bot.Commands;
	using FC.Bot.ContentCreator;
	using FC.Bot.Currency;
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
	using FC.Bot.Utils;
	using FC.XIVData;
	using Microsoft.Extensions.DependencyInjection;

	public class Program
	{
		private static List<ServiceBase> services = new List<ServiceBase>();
		private static bool exiting = false;
		private static DiscordSocketClient? client;

		private readonly IServiceProvider serviceProvider;

		public Program()
		{
			this.serviceProvider = CreateProvider();
		}

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
			Program prog = new ();
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

		public static IServiceProvider CreateProvider()
		{
			var config = new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
				AlwaysDownloadUsers = true,
			};

			var servConfig = new InteractionServiceConfig
			{
				AutoServiceScopes = false,
				LogLevel = LogSeverity.Info,
				DefaultRunMode = RunMode.Async,
			};

			var collection = new ServiceCollection()
				.AddSingleton(config)
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton(servConfig)
				.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
				.AddSingleton<SettingsService>()
				.AddSingleton<LeaderboardSettingsService>()
				.AddSingleton<CommandsService>()
				.AddSingleton<ScheduleService>()
				.AddSingleton<EventsService>()
				.AddSingleton<Eventsv2.EventsService>()
				.AddSingleton<CalendarService>()
				.AddSingleton<UserService>()
				.AddSingleton<ActionService>()
				.AddSingleton<CharacterService>()
				.AddSingleton<ChannelService>()
				.AddSingleton<ChannelOptInService>()
				.AddSingleton<ContentCreatorService>()
				.AddSingleton<EchoService>()
				.AddSingleton<FashionReportService>()
				.AddSingleton<GuildService>()
				.AddSingleton<HelpService>()
				.AddSingleton<HousingService>()
				.AddSingleton<ItemService>()
				.AddSingleton<LodestoneService>()
				.AddSingleton<LogService>()
				.AddSingleton<MeService>()
				.AddSingleton<ModerationService>()
				.AddSingleton<MountService>()
				.AddSingleton<NoveltyService>()
				.AddSingleton<PollService>()
				.AddSingleton<QuoteService>()
				.AddSingleton<ReactionRoleService>()
				.AddSingleton<RPGService>()
				.AddSingleton<StatusService>()
				.AddSingleton<VoiceService>()
				.AddSingleton<DebugModule>()
				.AddSingleton<CurrencyRunTimes>()
				.AddSingleton<CurrencyService>()
				.AddSingleton<XIVData.Items>()
				.AddSingleton<ItemAutocompleteHandler>()
				;

			return collection.BuildServiceProvider();
		}

		////public static async Task<IGuildUser> GetBotUserForGuild(IGuild guild)
		////{
		////	return await guild.GetUserAsync(DiscordClient.CurrentUser.Id);
		////}

		protected virtual async Task AddServices()
		{
			try
			{
				// Dependencies - must be added in order
				await this.AddService<SettingsService>();
				await this.AddService<LeaderboardSettingsService>();
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

		////private static void Log_ExceptionLogged(string exceptionLog)
		////{
		////	if (client != null)
		////	{
		////		try
		////		{
		////			string? idStr = Settings.Load().LogChannel;
		////			if (idStr != null)
		////			{
		////				ulong id = ulong.Parse(idStr);
		////				SocketTextChannel channel = (SocketTextChannel)DiscordClient.GetChannel(id);
		////				EmbedBuilder embedBuilder = new EmbedBuilder
		////				{
		////					Color = Color.Red,
		////					Title = "Kupo Nut Bot encountered an error",
		////					Description = exceptionLog,
		////					Timestamp = DateTimeOffset.UtcNow,
		////				};
		////				channel.SendMessageAsync(null, false, embedBuilder.Build());
		////			}
		////		}
		////		catch (Exception)
		////		{
		////		}
		////	}
		////}

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
					bool ready = false;
					client = this.serviceProvider.GetRequiredService<DiscordSocketClient>();

					client.Log += this.LogAsync;
					client.SlashCommandExecuted += this.OnSlashCommandExecuted;
					client.AutocompleteExecuted += this.OnClientAutoCompleteExecuted;

					client.Ready += () =>
					{
						ready = true;
						return Task.CompletedTask;
					};

					await client.LoginAsync(TokenType.Bot, token);
					await client.StartAsync();

					while (!ready)
						await Task.Yield();

					// boot the rest of the bot
					await this.AddServices();

					// Register Slash commands
					var interactionService = this.serviceProvider.GetRequiredService<InteractionService>();
					await interactionService.AddModuleAsync(typeof(DebugModule), this.serviceProvider);
#if DEBUG
					Settings settings = Settings.Load();
					if (!string.IsNullOrWhiteSpace(settings?.BotDiscordServer))
					{
						// Get the guild
						SocketGuild botGuild = client.GetGuild(ulong.Parse(settings.BotDiscordServer));
						if (botGuild == null)
							throw new Exception("Unable to access guild");

						await interactionService.RegisterCommandsToGuildAsync(botGuild.Id);
					}
#else
					await interactionService.RegisterCommandsGloballyAsync();
#endif

					interactionService.Log += this.LogAsync;
					interactionService.SlashCommandExecuted += this.InteractionSlashCommandExecuted;
					////interactionService.AutocompleteHandlerExecuted += this.OnAutocompleteHandlerExecuted;
					////interactionService.AutocompleteCommandExecuted += this.OnAutocompleteCommandExecuted;

					// Initialise the item autocomplete
					this.serviceProvider.GetRequiredService<XIVData.Items>();

					Initializing = false;
					Log.Write("Initialization complete", "Bot");

					while (!exiting)
					{
						await Task.Yield();
						Thread.Sleep(100);
					}
				}

				Log.Write("Shutting down", "Bot");

				Program.DiscordClient.SlashCommandExecuted -= this.OnSlashCommandExecuted;

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
			T service = this.serviceProvider.GetRequiredService<T>();
			CommandsService.BindCommands(service);
			await service.Initialize();
			services.Add(service);
		}

		private Task LogAsync(LogMessage log)
		{
			Log.Write(log.ToString(), "Bot");
			return Task.CompletedTask;
		}

		private async Task OnSlashCommandExecuted(SocketSlashCommand slashCommand)
		{
			if (Program.Initializing)
			{
				await slashCommand.DeferAsync(true);

				while (Program.Initializing)
				{
					await Task.Delay(1000);
				}
			}

			var interactionService = this.serviceProvider.GetRequiredService<InteractionService>();
			var ctx = new SocketInteractionContext(client, slashCommand);

			await interactionService.ExecuteCommandAsync(ctx, this.serviceProvider);
		}

		private async Task InteractionSlashCommandExecuted(SlashCommandInfo info, IInteractionContext context, IResult result)
		{
			if (info == null)
				return;

			if (Program.Initializing)
			{
				await context.Interaction.DeferAsync(true);

				while (Program.Initializing)
				{
					await Task.Delay(1000);
				}
			}

			var innerResult = await info.CheckPreconditionsAsync(context, this.serviceProvider);
			if (!innerResult.IsSuccess)
			{
				var errorMessage = $"```diff\n- {innerResult.ErrorReason}\n```";
				if (context.Interaction.HasResponded)
				{
					await context.Interaction.FollowupAsync(text: errorMessage, ephemeral: true);
				}
				else
				{
					await context.Interaction.RespondAsync(text: errorMessage, ephemeral: true);
				}
			}
		}

		////private async Task OnAutocompleteHandlerExecuted(IAutocompleteHandler handler, IInteractionContext context, IResult result)
		////{
		////	await Task.Delay(1);
		////	return;
		////}

		////private async Task OnAutocompleteCommandExecuted(AutocompleteCommandInfo info, IInteractionContext context, IResult result)
		////{
		////	await Task.Delay(1);
		////	return;
		////}

		private async Task OnClientAutoCompleteExecuted(SocketAutocompleteInteraction socketAutocompleteInteraction)
		{
			try
			{
				var interactionService = this.serviceProvider.GetRequiredService<InteractionService>();
				var ctx = new SocketInteractionContext(client, socketAutocompleteInteraction);

				await interactionService.ExecuteCommandAsync(ctx, this.serviceProvider);
			}
			catch (Exception ex)
			{
				await Logger.LogExceptionToDiscordChannel(ex, "Error in Autocomplete");
			}

			return;
		}
	}
}
