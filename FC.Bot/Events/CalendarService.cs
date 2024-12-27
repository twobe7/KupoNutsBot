// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Events.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Events;
	using FC.Bot.Services;
	using FC.Events;
	using FC.Utils;
	using NodaTime;

	public class CalendarService : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient;
		public CalendarService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		public override async Task Initialize()
		{
			ScheduleService.RunOnSchedule(this.Update, 15);
			await this.Update();
		}

		[Command("Calendar", Permissions.Administrators, "Updates the calendar")]
		public async Task Update()
		{
			Log.Write("Updating Calendar", "Bot");

			foreach (SocketGuild guild in this.DiscordClient.Guilds)
			{
				EventsSettings settings = await SettingsService.GetSettings<EventsSettings>(guild.Id);
				if (settings == null)
					continue;

				if (settings.CalendarChannel == null)
					continue;

				_ = ulong.TryParse(settings.CalendarChannel, out ulong channelId);
				_ = ulong.TryParse(settings.CalendarWeekMessageId, out ulong weekMessageID);
				_ = ulong.TryParse(settings.CalendarFutureMessageId, out ulong futureMessageID);

				if (channelId == 0)
					return;

				weekMessageID = await this.Update(guild.Id, channelId, weekMessageID, "Events in the next week", 0, 7);
				futureMessageID = await this.Update(guild.Id, channelId, futureMessageID, "Events in the future", 7, 30);

				settings.CalendarWeekMessageId = weekMessageID.ToString();
				settings.CalendarFutureMessageId = futureMessageID.ToString();
				await SettingsService.SaveSettings(settings);
			}
		}

		private string GetEventString(Event evt, int daysTill, Instant occurrence)
		{
			StringBuilder builder = new();
			builder.Append(Utils.Characters.Tab);

			if (evt.Notify != null)
			{
				builder.Append(evt.Notify.GetLink(evt));
			}
			else
			{
				builder.Append(evt.Name);
			}

			if (!string.IsNullOrEmpty(evt.ShortDescription))
			{
				builder.Append($" - *{evt.ShortDescription}*");
			}

			// Today
			if (daysTill == 0)
			{
				builder.Append($" - {evt.GetWhenString()}");
			}

			/*else
			{
				builder.AppendLine();
				builder.Append(Utils.Characters.Tab);
				builder.AppendLine(TimeUtils.GetTimeString(occurrence));
			}*/

			return builder.ToString();
		}

		private async Task<ulong> Update(ulong guildId, ulong channelId, ulong messageId, string header, int minDays, int maxDays)
		{
			SocketTextChannel channel = (SocketTextChannel)this.DiscordClient.GetChannel(channelId);

			StringBuilder builder = new();
			/*builder.Append("**");
			builder.Append(header);
			builder.AppendLine("**");
			builder.AppendLine();*/

			Dictionary<int, List<(Event, Instant)>> eventSchedule = [];

			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

			Dictionary<string, object> filters = new()
			{
				{ "ServerIdStr", guildId.ToString() },
			};

			List<Event> events = await EventsService.EventsDatabase.LoadAll(filters);
			foreach (Event evt in events)
			{
				List<Occurance> occurances = evt.GetNextOccurances();

				foreach (Occurance occurance in occurances)
				{
					Instant instant = occurance.GetInstant();
					int days = TimeUtils.GetDaysTill(instant, zone);

					if (!eventSchedule.ContainsKey(days))
						eventSchedule.Add(days, []);

					eventSchedule[days].Add((evt, instant));
				}
			}

			int count = 0;
			for (int i = minDays; i < maxDays; i++)
			{
				if (!eventSchedule.ContainsKey(i))
					continue;

				List<(Event, Instant)> eventsDay = eventSchedule[i];

				builder.AppendLine($"__{TimeUtils.GetDayName(i)}__");

				foreach ((Event evt, Instant occurance) in eventsDay)
				{
					builder.AppendLine(this.GetEventString(evt, i, occurance));
					count++;
				}

				builder.AppendLine();
			}

			if (count == 0)
				builder.AppendLine("None");

			EmbedBuilder embedBuilder = new()
			{
				Title = header,
				Description = builder.ToString(),
				Color = Color.Blue,
			};

			RestUserMessage? message = null;

			if (messageId != 0)
				message = (RestUserMessage)await channel.GetMessageAsync(messageId);

			if (message == null)
			{
				message = await channel.SendMessageAsync(null, false, embedBuilder.Build());
				messageId = message.Id;
			}
			else
			{
				if (message.Author.Id == this.DiscordClient.CurrentUser.Id)
				{
					await message.ModifyAsync(x =>
					{
						x.Content = null;
						x.Embed = embedBuilder.Build();
					});
				}
			}

			return messageId;
		}
	}
}
