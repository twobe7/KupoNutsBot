﻿// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Events;
	using KupoNuts.Events;
	using KupoNuts.Utils;
	using NodaTime;

	public class ReminderService : ServiceBase
	{
		private static ReminderService? instance;

		private static IEmote emoteCancel = Emote.Parse("<:No:604942582589423618>");
		private static IEmote emote15mins = Emote.Parse("<:15mins:604947121786978308>");
		private static IEmote emote30mins = Emote.Parse("<:30mins:604947121921064960>");
		private static IEmote emote1hour = Emote.Parse("<:1hour:604947121778720815>");
		private static IEmote emote1day = Emote.Parse("<:1day:604947121380261899>");
		private static IEmote emoteYes = Emote.Parse("<:Yes:604942582866247690>");

		private Dictionary<ulong, PendingReminder> pendingReminderLookup = new Dictionary<ulong, PendingReminder>();
		private HashSet<ulong> activeReminderLookup = new HashSet<ulong>();

		public static void SetReminder(Event evt, Event.Notification.Attendee attendee)
		{
			if (instance == null)
				throw new Exception("No Reminder Service");

			_ = Task.Run(async () => { await instance.ConfirmReminder(evt, attendee); });
		}

		public override async Task Initialize()
		{
			instance = this;

			Program.DiscordClient.ReactionAdded += this.ReactionAdded;
			Scheduler.RunOnSchedule(this.Update, 15);
			await this.Update();
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;

			return Task.CompletedTask;
		}

		private static Duration? GetDelaytime(IEmote emote)
		{
			if (emote.Name == emote15mins.Name)
				return Duration.FromMinutes(15);

			if (emote.Name == emote30mins.Name)
				return Duration.FromMinutes(30);

			if (emote.Name == emote1hour.Name)
				return Duration.FromHours(1);

			if (emote.Name == emote1day.Name)
				return Duration.FromDays(1);

			return null;
		}

		private async Task Update()
		{
			List<Event> events = await EventsService.EventsDatabase.LoadAll();
			foreach (Event evt in events)
			{
				if (evt.Id == null)
					continue;

				Event.Occurance? nextOccurance = evt.GetNextOccurance();
				if (nextOccurance == null)
					continue;

				List<Event.Notification.Attendee>? attendees = evt.GetAttendees();
				if (attendees != null)
				{
					foreach (Event.Notification.Attendee attendee in attendees)
					{
						if (attendee.UserId == null)
							continue;

						Duration? remindTime = attendee.GetRemindTime();
						if (remindTime == null)
							continue;

						Instant? remindInstant = nextOccurance.GetInstant() - remindTime;

						if (remindInstant.Value < TimeUtils.Now)
						{
							await this.Remind(attendee, evt);
						}
					}
				}
			}
		}

		private async Task Remind(Event.Notification.Attendee attendee, Event evt)
		{
			if (attendee.UserId == null)
				return;

			if (evt.Notify == null)
				return;

			SocketUser user = Program.DiscordClient.GetUser(ulong.Parse(attendee.UserId));

			EmbedBuilder builder = new EmbedBuilder();

			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append("Hey ");
			messageBuilder.Append(user.Username);
			messageBuilder.AppendLine("!");
			messageBuilder.Append("You asked me to remind you about the event, ");
			messageBuilder.Append(evt.Notify.GetLink(evt));
			messageBuilder.Append(", that starts in");
			messageBuilder.Append(TimeUtils.GetDurationString(attendee.GetRemindTime()));
			messageBuilder.AppendLine(".");

			builder.Description = messageBuilder.ToString();

			attendee.RemindTime = null;
			await EventsService.EventsDatabase.Save(evt);

			IUserMessage message = await user.SendMessageAsync(null, false, builder.Build());
			await message.AddReactionAsync(emoteYes);

			this.activeReminderLookup.Add(message.Id);
		}

		private async Task ConfirmReminder(Event evt, Event.Notification.Attendee attendee)
		{
			if (attendee.UserId == null)
				return;

			SocketUser user = Program.DiscordClient.GetUser(ulong.Parse(attendee.UserId));

			string? eventName = evt.Name;
			if (evt.Notify != null)
				eventName = evt.Notify.GetLink(evt);

			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append("Hey ");
			messageBuilder.Append(user.Username);
			messageBuilder.AppendLine("!");

			if (attendee.RemindTime != null)
			{
				messageBuilder.Append("You're already set to recieve a reminder");
				messageBuilder.Append(TimeUtils.GetDurationString(attendee.GetRemindTime()));
				messageBuilder.Append(" before the event ");
				messageBuilder.Append(eventName);
				messageBuilder.AppendLine(" begins.");
			}
			else
			{
				messageBuilder.Append("I can remind you about the event, ");
				messageBuilder.Append(eventName);
				messageBuilder.AppendLine(", before it begins.");
			}

			messageBuilder.AppendLine();
			messageBuilder.AppendLine("How long before the event begins should I remind you?");

			EmbedBuilder builder = new EmbedBuilder();
			builder.Description = messageBuilder.ToString();

			IUserMessage message = await user.SendMessageAsync(null, false, builder.Build());

			this.pendingReminderLookup.Add(message.Id, new PendingReminder(evt, attendee));

			List<IEmote> reactions = new List<IEmote>();

			reactions.Add(emoteCancel);
			reactions.Add(emote15mins);
			reactions.Add(emote30mins);
			reactions.Add(emote1hour);
			reactions.Add(emote1day);

			await message.AddReactionsAsync(reactions.ToArray());
		}

		private Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return Task.CompletedTask;

				_ = Task.Run(async () =>
				{
					IUserMessage initialMessage = await message.GetOrDownloadAsync();
					await this.HandleResponse(initialMessage, reaction.Emote, reaction.UserId);
				});
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}

			return Task.CompletedTask;
		}

		private async Task HandleResponse(IUserMessage message, IEmote emote, ulong userId)
		{
			if (this.activeReminderLookup.Contains(message.Id))
			{
				this.activeReminderLookup.Remove(message.Id);

				await message.DeleteAsync();
			}
			else if (this.pendingReminderLookup.ContainsKey(message.Id))
			{
				PendingReminder reminder = this.pendingReminderLookup[message.Id];
				Duration? time = GetDelaytime(emote);
				await reminder.SetDelay(time);

				SocketUser user = Program.DiscordClient.GetUser(userId);
				IUserMessage replyMessage;
				EmbedBuilder builder = new EmbedBuilder();
				StringBuilder messageBuilder = new StringBuilder();

				if (time == null)
				{
					messageBuilder.Append("Got it, I wont remind you.");
				}
				else
				{
					messageBuilder.Append("Got it, I'll let you know");
					messageBuilder.Append(TimeUtils.GetDurationString((Duration)time));
					messageBuilder.AppendLine(" before the event starts!");
				}

				messageBuilder.AppendLine();
				messageBuilder.Append("(This message will self-destruct in 5 seconds!)");

				builder.Description = messageBuilder.ToString();
				replyMessage = await user.SendMessageAsync(null, false, builder.Build());

				await Task.Delay(5000);

				await message.DeleteAsync();
				await replyMessage.DeleteAsync();

				this.pendingReminderLookup.Remove(message.Id);
			}
		}

		public class PendingReminder
		{
			public string EventId;
			public ulong UserId;

			public PendingReminder(Event evt, Event.Notification.Attendee attendee)
			{
				if (evt.Id == null)
					throw new ArgumentNullException("evt.Id");

				if (attendee.UserId == null)
					throw new ArgumentNullException("attendee.Id");

				this.EventId = evt.Id;
				this.UserId = ulong.Parse(attendee.UserId);
			}

			public async Task SetDelay(Duration? time)
			{
				Event? evt = await EventsService.EventsDatabase.Load(this.EventId);

				if (evt is null)
					return;

				Event.Notification.Attendee? attendee = evt.GetAttendee(this.UserId);

				if (attendee is null)
					return;

				attendee.SetRemindTime(time);
				await EventsService.EventsDatabase.Save(evt);
			}
		}
	}
}
