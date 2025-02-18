﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.CommandModules
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Utils;
	using FC.Utils;
	using Microsoft.Extensions.DependencyInjection;
	using NodaTime;

	[Group("debug", "Commands relating to the usage of the bot")]
	public class DebugModule : InteractionModuleBase
	{
		private static readonly List<string> GoodBotResponses =
		[
			"Thanks!",
			"Thanks, you're a good human!",
			"Sure, I guess...",
			@"yaay!",
			"I try so hard!",
			"It's nice to get some recognition around here.",
		];

		private readonly IServiceProvider serviceProvider;

		public DebugModule(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		[SlashCommand("goodbot", "Praise me")]
		public async Task GoodBot() => await this.RespondAsync(GoodBotResponses[new Random().Next(0, GoodBotResponses.Count)]);

		[SlashCommand("developer", "Join the developer discord!")]
		public async Task Developer() => await this.RespondAsync("Join in on the developer discussion: https://discord.gg/z8G2T8GdDD");

		[SlashCommand("time", "Shows the current time")]
		public async Task Time([Summary(description: "Timezone to display (AEST, NZST)")]string? timezone = null)
		{
			if (timezone == null)
			{
				await this.RespondAsync(text: $"<t:{SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds()}:f>");
				return;
			}

			await this.DeferAsync();

			// Get timezones for relevant countries
			var tzAbbr = this.GetTimezones();

			foreach (string? tzId in tzAbbr)
			{
				TimeZoneNames.TimeZoneValues? abbr = TimeZoneNames.TZNames.GetAbbreviationsForTimeZone(tzId, "en-au");

				// Check if a timezone name matches given timezone
				if (abbr.Generic == timezone || abbr.Daylight == timezone || abbr.Standard == timezone)
				{
					DateTimeZone? dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
					if (dateTimeZone != null)
					{
						var currentInstance = SystemClock.Instance.GetCurrentInstant()
							.InZone(dateTimeZone)
							.ToInstant();

						await this.FollowupAsync(text: "The time is: " + TimeUtils.GetDateTimeString(currentInstance, dateTimeZone));
						return;
					}
				}
			}

			await this.FollowupAsync(text: "Unable to find given timezone");
		}

		public Task<string> TimeOld(string timezone)
		{
			DateTimeZone dtz;

			try
			{
				dtz = TimeUtils.GetTimeZone(timezone);
			}
			catch (Exception)
			{
				throw new UserException("I couldn't find that timezone! Try \"Australia/Sydney\"");
			}

			Instant now = SystemClock.Instance.GetCurrentInstant();
			return Task.FromResult("The time is: " + TimeUtils.GetDateTimeString(now, dtz));
		}

		[SlashCommand("convert-to-unix-time", "Converts string to unix time")]
		public async Task ToUnixTime(string? date = null, string? time = null, string? timezone = "AEST")
		{
			await this.DeferAsync();

			var unixTimeString = this.ToUnixTimeString(date, time, timezone);
			if (unixTimeString != null)
			{
				await this.FollowupAsync(text: unixTimeString);
				return;
			}

			await this.FollowupAsync(text: "I'm unable to convert that.");
		}

		public string? ToUnixTimeString(string? date = null, string? time = null, string? timezone = "AEST", bool timeOnly = false)
		{
			string response;

			date ??= DateTime.Now.Date.ToString("dd/MM/yyyy");
			time ??= DateTime.Now.Date.ToString("HH:mm");

			if (DateTime.TryParse($"{date} {time}", out DateTime parsedDateTime))
			{
				timezone = timezone?.ToLower();

				var tzAbbr = this.GetTimezones();
				foreach (string? tzId in tzAbbr)
				{
					int offsetInHours = 0;

					TimeZoneNames.TimeZoneValues? abbr = TimeZoneNames.TZNames.GetAbbreviationsForTimeZone(tzId, "en-au");

					// Increase by 1 hour if Daylight
					if (abbr?.Daylight?.ToLower() == timezone)
						offsetInHours += 1;

					if (abbr?.Generic?.ToLower() == timezone || abbr?.Daylight?.ToLower() == timezone || abbr?.Standard?.ToLower() == timezone)
					{
						DateTimeZone? dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
						if (dateTimeZone != null)
						{
							ZonedDateTime zoned = default(Instant).InZone(dateTimeZone);

							offsetInHours += zoned.Offset.Seconds / 60 / 60;

							TimeSpan diffTS = new(offsetInHours, 0, 0);

							parsedDateTime = parsedDateTime.Add(-diffTS);

							TimeSpan timespan = parsedDateTime.TimeOfDay;

							Instant tt = Instant.FromUtc(parsedDateTime.Year, parsedDateTime.Month, parsedDateTime.Day, timespan.Hours, timespan.Minutes);

							response = tt.ToUnixTimeSeconds().ToString();

							return timeOnly ? $"<t:{response}:t>" : $"<t:{response}:f>";
						}
					}
				}
			}

			return null;
		}

		[RequireUserPermission(GuildPermission.Administrator)]
		[SlashCommand("getchannelname", "do test")]
		public async Task GetChannelName(SocketTextChannel channel)
		{
			await this.RespondAsync(channel.Name);
		}

		[RequireUserPermission(GuildPermission.Administrator)]
		[SlashCommand("throwerror", "Throws an error")]
		public Task ThrowError()
		{
			throw new Exception("Test exception");
		}

		[RequireOwner]
		[SlashCommand("update-slash-commands", "Updates the Slash Commands", true)]
		public async Task UpdateSlashCommands()
		{
			if (this.Context != null && !this.Context.Interaction.HasResponded)
			{
				await this.DeferAsync(ephemeral: true);
			}

			// Register Slash commands
			var interactionService = this.serviceProvider.GetRequiredService<InteractionService>();

			// Remove existing slash commands
			foreach (var cmd in interactionService.SlashCommands)
			{
				await interactionService.RemoveModuleAsync(cmd.Module);
			}

			// Re-add the modules
			try
			{
				await interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), this.serviceProvider);

#if !DEBUG
				// DebugModule has already been registered
				await interactionService.RemoveModuleAsync(typeof(DebugModule));
#endif

			}
			catch (Exception ex)
			{
				await Logger.LogExceptionToDiscordChannel(ex, "AddModulesAsync - Error in update slash commands");
			}
#if DEBUG
			Settings settings = Settings.Load();
			if (!string.IsNullOrWhiteSpace(settings?.BotDiscordServer))
			{
				// Get the guild
				SocketGuild botGuild = Program.DiscordClient.GetGuild(ulong.Parse(settings.BotDiscordServer))
					?? throw new Exception("Unable to access guild");

				try
				{
					await interactionService.RegisterCommandsToGuildAsync(botGuild.Id);
				}
				catch (Exception ex)
				{
					await Logger.LogExceptionToDiscordChannel(ex, "RegisterCommandsToGuildAsync - Error in update slash commands");
				}
			}
#else
			await interactionService.RegisterCommandsGloballyAsync();
#endif

			if (this.Context != null)
				await this.FollowupAsync(text: "Slash commands registered", ephemeral: true);
		}

		/// <summary>
		/// Get Timezones for specified countries.
		/// </summary>
		/// <returns>IEnumerable of timezones.</returns>
		private IEnumerable<string> GetTimezones()
		{
			return
				TimeZoneNames.TZNames.GetTimeZonesForCountry("au", "en-au").Keys
					.Concat(TimeZoneNames.TZNames.GetTimeZonesForCountry("nz", "en-au").Keys);
		}
	}
}
