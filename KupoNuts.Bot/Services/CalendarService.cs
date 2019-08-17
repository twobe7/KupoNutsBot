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
			CommandsService.BindCommand("calendar", this.Update, Permissions.Administrators, "Updates the calendar");

			Scheduler.RunOnSchedule(this.Update, 15);
			await this.Update();
		}

		public override Task Shutdown()
		{
			CommandsService.ClearCommand("calendar");
			return Task.CompletedTask;
		}

		private async Task Update()
		{
			Database db = Database.Load();

			ulong channelId = 0;
			ulong weekMessageID = 0;
			ulong futureMessageID = 0;

			ulong.TryParse(db.Settings.CalendarChannel, out channelId);
			ulong.TryParse(db.CalendarMessage, out weekMessageID);
			ulong.TryParse(db.CalendarMessage2, out futureMessageID);

			if (channelId == 0)
				return;

			weekMessageID = await this.Update(channelId, weekMessageID, "Events in the next week", 0, 7);
			futureMessageID = await this.Update(channelId, futureMessageID, "Events in the future", 7, 30);

			db = Database.Load();
			db.CalendarMessage = weekMessageID.ToString();
			db.CalendarMessage2 = futureMessageID.ToString();
			db.Save();
		}

		private string GetEventString(Event evt, int daysTill)
		{
			StringBuilder builder = new StringBuilder();

			Database db = Database.Load();
			Notification notify = db.GetNotification(evt.Id);
			if (notify != null)
			{
				builder.Append(" - ");
				builder.Append(notify.GetLink());
			}
			else
			{
				builder.Append(" - ");
				builder.Append(evt.Name);
			}

			// Today
			if (daysTill == 0)
			{
				builder.Append(" - ");
				builder.Append(evt.GetWhenString());
			}

			return builder.ToString();
		}

		private async Task<ulong> Update(ulong channelId, ulong messageId, string header, int minDays, int maxDays)
		{
			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);

			StringBuilder builder = new StringBuilder();
			builder.Append("**");
			builder.Append(header);
			builder.AppendLine("**");
			builder.AppendLine();

			Dictionary<int, List<Event>> eventSchedule = new Dictionary<int, List<Event>>();

			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			Database db = Database.Load();
			foreach (Event evt in db.Events)
			{
				List<Instant> occurances = evt.GetNextOccurances(zone);

				foreach (Instant occurance in occurances)
				{
					int days = TimeUtils.GetDaysTill(occurance, zone);

					if (!eventSchedule.ContainsKey(days))
						eventSchedule.Add(days, new List<Event>());

					eventSchedule[days].Add(evt);
				}
			}

			int count = 0;
			for (int i = minDays; i < maxDays; i++)
			{
				if (!eventSchedule.ContainsKey(i))
					continue;

				List<Event> events = eventSchedule[i];

				builder.AppendLine(TimeUtils.GetDayName(i));

				foreach (Event evt in events)
				{
					builder.AppendLine(this.GetEventString(evt, i));
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
				message = await channel.SendMessageAsync(null, false, embedBuilder.Build());
				messageId = message.Id;
			}
			else
			{
				await message.ModifyAsync(x =>
				{
					x.Embed = embedBuilder.Build();
				});
			}

			return messageId;
		}
	}
}
