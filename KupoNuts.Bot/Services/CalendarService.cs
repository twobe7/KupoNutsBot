// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Events;
	using KupoNuts.Events;
	using KupoNuts.Utils;
	using NodaTime;

	public class CalendarService : ServiceBase
	{
		public override async Task Initialize()
		{
			ScheduleService.RunOnSchedule(this.Update, 15);
			await this.Update();
		}

		[Command("Calendar", Permissions.Administrators, "Updates the calendar")]
		public async Task Update()
		{
			Log.Write("Updating Calendar", "Bot");
			Settings db = Settings.Load();

			ulong channelId = 0;
			ulong weekMessageID = 0;
			ulong futureMessageID = 0;

			ulong.TryParse(db.CalendarChannel, out channelId);
			ulong.TryParse(db.CalendarMessage, out weekMessageID);
			ulong.TryParse(db.CalendarMessage2, out futureMessageID);

			if (channelId == 0)
				return;

			weekMessageID = await this.Update(channelId, weekMessageID, "Events in the next week", 0, 7);
			futureMessageID = await this.Update(channelId, futureMessageID, "Events in the future", 7, 30);

			db.CalendarMessage = weekMessageID.ToString();
			db.CalendarMessage2 = futureMessageID.ToString();
			db.Save();
		}

		private string GetEventString(Event evt, int daysTill, Instant occurrence)
		{
			StringBuilder builder = new StringBuilder();
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
				builder.Append(" - *");
				builder.Append(evt.ShortDescription);
				builder.Append("*");
			}

			// Today
			if (daysTill == 0)
			{
				builder.Append(" - ");
				builder.Append(evt.GetWhenString());
			}

			/*else
			{
				builder.AppendLine();
				builder.Append(Utils.Characters.Tab);
				builder.AppendLine(TimeUtils.GetTimeString(occurrence));
			}*/

			return builder.ToString();
		}

		private async Task<ulong> Update(ulong channelId, ulong messageId, string header, int minDays, int maxDays)
		{
			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);

			StringBuilder builder = new StringBuilder();
			/*builder.Append("**");
			builder.Append(header);
			builder.AppendLine("**");
			builder.AppendLine();*/

			Dictionary<int, List<(Event, Instant)>> eventSchedule = new Dictionary<int, List<(Event, Instant)>>();

			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

			List<Event> events = await EventsService.EventsDatabase.LoadAll();
			foreach (Event evt in events)
			{
				List<Event.Occurance> occurances = evt.GetNextOccurances();

				foreach (Event.Occurance occurance in occurances)
				{
					Instant instant = occurance.GetInstant();
					int days = TimeUtils.GetDaysTill(instant, zone);

					if (!eventSchedule.ContainsKey(days))
						eventSchedule.Add(days, new List<(Event, Instant)>());

					eventSchedule[days].Add((evt, instant));
				}
			}

			int count = 0;
			for (int i = minDays; i < maxDays; i++)
			{
				if (!eventSchedule.ContainsKey(i))
					continue;

				List<(Event, Instant)> eventsDay = eventSchedule[i];

				builder.Append("__");
				builder.Append(TimeUtils.GetDayName(i));
				builder.AppendLine("__");

				foreach ((Event evt, Instant occurance) in eventsDay)
				{
					builder.AppendLine(this.GetEventString(evt, i, occurance));
					count++;
				}

				builder.AppendLine();
			}

			if (count == 0)
				builder.AppendLine("None");

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Description = builder.ToString();
			embedBuilder.Color = Color.Blue;

			RestUserMessage? message = null;

			if (messageId != 0)
				message = (RestUserMessage)await channel.GetMessageAsync(messageId);

			if (message == null)
			{
				message = await channel.SendMessageAsync(header, false, embedBuilder.Build());
				messageId = message.Id;
			}
			else
			{
				await message.ModifyAsync(x =>
				{
					x.Content = header;
					x.Embed = embedBuilder.Build();
				});
			}

			return messageId;
		}
	}
}
