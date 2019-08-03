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
	using KupoNuts.Bot.Events;
	using KupoNuts.Events;
	using KupoNuts.Utils;
	using NodaTime;

	public class CalendarService : ServiceBase
	{
		private bool online = false;

		public override async Task Initialize()
		{
			this.online = true;

			await this.Update();
			_ = Task.Factory.StartNew(this.AutoUpdate, TaskCreationOptions.LongRunning);
		}

		public override Task Shutdown()
		{
			this.online = false;
			return Task.CompletedTask;
		}

		private async Task AutoUpdate()
		{
			while (this.online)
			{
				int minutes = DateTime.UtcNow.Minute;
				int delay = 15 - minutes;
				while (delay < 0)
					delay += 15;

				await Task.Delay(new TimeSpan(0, delay, 0));

				await this.Update();

				await Task.Delay(new TimeSpan(0, 2, 0));
			}
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

		private string GetEventString(Event evt)
		{
			// should come from notification, probably.
			string serverId = "391492798353768449";

			StringBuilder builder = new StringBuilder();

			Database db = Database.Load();
			Notification notify = db.GetNotification(evt.Id);
			if (notify != null)
			{
				builder.Append(" - [");
				builder.Append(evt.Name);
				builder.Append("](");
				builder.Append("https://discordapp.com/channels/");
				builder.Append(serverId);
				builder.Append("/");
				builder.Append(notify.ChannelId);
				builder.Append("/");
				builder.Append(notify.MessageId);
				builder.Append(")");
			}
			else
			{
				builder.Append(" - ");
				builder.Append(evt.Name);
			}

			// Today
			int days = evt.GetDaysTill();
			if (days == 0)
			{
				builder.Append(" - In ");
				builder.Append(TimeUtils.GetDurationString(evt.GetDurationTill()));
			}

			return builder.ToString();
		}

		// "**Events in the next week**", 0, 7
		private async Task<ulong> Update(ulong channelId, ulong messageId, string header, int minDays, int maxDays)
		{
			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);

			StringBuilder builder = new StringBuilder();
			builder.Append("**");
			builder.Append(header);
			builder.AppendLine("**");
			builder.AppendLine();

			Dictionary<int, List<Event>> weekEvents = new Dictionary<int, List<Event>>();

			Database db = Database.Load();
			foreach (Event evt in db.Events)
			{
				int days = evt.GetDaysTill();

				if (days < minDays || days >= maxDays)
					continue;

				if (!weekEvents.ContainsKey(days))
					weekEvents.Add(days, new List<Event>());

				weekEvents[days].Add(evt);
			}

			int count = 0;
			foreach ((int days, List<Event> events) in weekEvents)
			{
				builder.AppendLine(TimeUtils.GetDayName(days));

				foreach (Event evt in events)
				{
					builder.AppendLine(this.GetEventString(evt));
					builder.AppendLine();
					count++;
				}
			}

			if (count == 0)
				builder.AppendLine("None");

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Description = builder.ToString();
			embedBuilder.Color = Color.Blue;

			if (messageId == 0)
			{
				RestUserMessage message = await channel.SendMessageAsync(null, false, embedBuilder.Build());
				messageId = message.Id;
			}
			else
			{
				RestUserMessage message = (RestUserMessage)await channel.GetMessageAsync(messageId);
				await message.ModifyAsync(x =>
				{
					x.Embed = embedBuilder.Build();
				});
			}

			return messageId;
		}
	}
}
