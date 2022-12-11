// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Utils;
	using NodaTime;

	public class DebugService : ServiceBase
	{
		private static readonly List<string> GoodBotResponses = new List<string>()
		{
			"Thanks!",
			"Thanks, you're a good human!",
			"Sure, I guess...",
			@"yaay!",
			"I try so hard!",
			"It's nice to get some recognition around here.",
		};

		public override Task Initialize()
		{
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		{
			return Task.CompletedTask;
		}

		[Command("Developer", Permissions.Everyone, "Join the developer discord!")]
		public Task<string> Developer()
		{
			return Task.FromResult("Join in on the developer discussion: https://discord.gg/z8G2T8GdDD");
		}

		[Command("GoodBot", Permissions.Everyone, "Praise me")]
		public Task<string> GoodBot()
		{
			Random rn = new Random();
			return Task.FromResult(GoodBotResponses[rn.Next(0, GoodBotResponses.Count)]);
		}

		[Command("Time", Permissions.Everyone, "Shows the current time in multiple time zones.")]
		public async Task Time(CommandMessage message)
		{
			long now = SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds();
			await message.Channel.SendMessageAsync(text: $"<t:{now}:f>", messageReference: message.MessageReference);

			return;
		}

		[Command("Time", Permissions.Everyone, "Shows the current time in the given timezone")]
		public Task<string> Time(string timezone)
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

		[Command("Unix", Permissions.Everyone, "Converts string to unix time", CommandCategory.Novelty, "ConvertToUnixTime")]
		[Command("ConvertToUnixTime", Permissions.Everyone, "Converts string to unix time")]
		public Task<string> ToUnixTime(string time, string timezone)
		{
			return this.ToUnixTime(DateTime.Now.Date.ToString("dd/MM/yyyy"), time, timezone);
		}

		[Command("Unix", Permissions.Everyone, "Converts string to unix time", CommandCategory.Novelty, "ConvertToUnixTime")]
		[Command("ConvertToUnixTime", Permissions.Everyone, "Converts string to unix time")]
		public Task<string> ToUnixTime(string date, string time, string timezone)
		{
			string response;

			if (DateTime.TryParse($"{date} {time}", out DateTime parsedDateTime))
			{
				ICollection<string>? tzAbbr = TimeZoneNames.TZNames.GetTimeZonesForCountry("au", "en-au").Keys;
				foreach (string? tzId in tzAbbr)
				{
					int offsetInHours = 0;

					TimeZoneNames.TimeZoneValues? abbr = TimeZoneNames.TZNames.GetAbbreviationsForTimeZone(tzId, "en-au");

					// Increase by 1 hour if Daylight
					if (abbr.Daylight == timezone)
						offsetInHours += 1;

					if (abbr.Generic == timezone || abbr.Daylight == timezone || abbr.Standard == timezone)
					{
						DateTimeZone? dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
						if (dateTimeZone != null)
						{
							ZonedDateTime zoned = default(Instant).InZone(dateTimeZone);

							offsetInHours += zoned.Offset.Seconds / 60 / 60;

							TimeSpan diffTS = new TimeSpan(offsetInHours, 0, 0);

							parsedDateTime = parsedDateTime.Add(-diffTS);

							TimeSpan timespan = parsedDateTime.TimeOfDay;

							Instant tt = Instant.FromUtc(parsedDateTime.Year, parsedDateTime.Month, parsedDateTime.Day, timespan.Hours, timespan.Minutes);

							response = tt.ToUnixTimeSeconds().ToString();

							return Task.FromResult($"<t:{response}:f>");
						}
					}
				}
			}

			return Task.FromResult("I'm unable to convert that.");
		}

		[Command("Blame", Permissions.Everyone, "Blames someone", CommandCategory.Novelty)]
		public Task<string> Blame(CommandMessage message)
		{
			if (message.Channel is SocketGuildChannel guildChannel)
			{
				List<IGuildUser> targets = new List<IGuildUser>();
				targets.Add(message.Author);

				foreach (SocketGuildUser tTarget in guildChannel.Guild.Users)
				{
					if (CommandsService.GetPermissions(tTarget) != Permissions.Administrators)
						continue;

					targets.Add(tTarget);
				}

				Random rnd = new Random();
				int val = rnd.Next(targets.Count);
				IGuildUser target = targets[val];

				if (target.Id == Program.DiscordClient.CurrentUser.Id)
					return Task.FromResult("This is my fault. =(");

				return Task.FromResult("This is your fault, " + target.GetName() + ".");
			}

			return Task.FromResult("This is your fault, " + message.Author.GetName() + ".");
		}

		[Command("GetChannelName", Permissions.Administrators, "do test")]
		public Task<string> Test(CommandMessage message, SocketTextChannel channel)
		{
			return Task.FromResult(channel.Name);
		}

		[Command("ThrowError", Permissions.Administrators, "Throws an error")]
		public Task ThrowError()
		{
			throw new Exception("Test exception");
		}
	}
}
