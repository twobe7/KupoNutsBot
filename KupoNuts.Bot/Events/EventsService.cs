// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
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
	using KupoNuts.Bot.Services;
	using KupoNuts.Events;
	using NodaTime;

	public class EventsService : ServiceBase
	{
		private Dictionary<ulong, Event> messageLookup = new Dictionary<ulong, Event>();

		public static EventsService Instance
		{
			get;
			private set;
		}

		public override async Task Initialize()
		{
			Instance = this;

			CommandsService.BindCommand("events", this.Update, Permissions.Administrators, "Checks event notifications");

			Scheduler.RunOnSchedule(this.Update, 15);
			await this.Update();

			Program.DiscordClient.ReactionAdded += this.ReactionAdded;
		}

		public override Task Shutdown()
		{
			Instance = null;

			CommandsService.ClearCommand("events");

			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;

			return Task.CompletedTask;
		}

		public void Watch(RestUserMessage message, Event evt)
		{
			if (message == null)
				return;

			if (this.messageLookup.ContainsKey(message.Id))
				return;

			this.messageLookup.Add(message.Id, evt);
		}

		private async Task Update()
		{
			Database db = Database.Load();
			db.SanatiseAttendees();
			db.Save();

			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

			for (int i = db.Events.Count - 1; i >= 0; i--)
			{
				Event evt = db.Events[i];

				// dont delete repeating events
				if (evt.Repeats != 0)
					continue;

				Instant? next = evt.GetNextOccurance(zone);

				// will never occur (past event)
				if (next == null)
				{
					db.DeleteEvent(evt.Id);
				}
			}

			db.Save();
			db = Database.Load();

			for (int i = db.Notifications.Count - 1; i >= 0; i--)
			{
				RestUserMessage message = await db.Notifications[i].GetMessage();
				Event evt = db.GetEvent(db.Notifications[i].EventId);

				// dead reminder.
				if (message is null || evt is null || !this.ShouldNotify(evt))
				{
					evt.ClearAttendees(db);
					await db.Notifications[i].Delete();
					db.Notifications.RemoveAt(i);
					db.Save();
					continue;
				}

				this.Watch(message, evt);
			}

			db = Database.Load();
			foreach (Event evt in db.Events)
			{
				SocketTextChannel channel = evt.GetChannel();

				if (channel == null)
					continue;

				if (db.GetNotification(evt.Id) == null && this.ShouldNotify(evt))
				{
					await evt.Post();
				}

				await evt.CheckReactions();
			}
		}

		private bool ShouldNotify(Event evt)
		{
			Duration? timeTillEvent = evt.GetDurationTill();

			// Event in the past.
			if (timeTillEvent == null)
				return false;

			Duration? notifyDuration = evt.GetNotifyDuration();

			// never notify
			if (notifyDuration == null)
				return false;

			// instant notify
			if (notifyDuration.Value.TotalSeconds == 0)
				return true;

			return timeTillEvent.Value < notifyDuration.Value;
		}

		private async Task Notify(string[] args, SocketMessage message)
		{
			if (args.Length <= 0)
			{
				foreach (Event evt in Database.Load().Events)
				{
					await evt.Post();
				}
			}
			else
			{
				await message.Channel.SendMessageAsync("I'm sorry, I cant notify specific events yet.");
			}
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!this.messageLookup.ContainsKey(message.Id))
				return;

			// dont mark yourself as attending!
			if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
				return;

			Event evt = this.messageLookup[message.Id];
			Attendee attendee = evt.GetAttendee(reaction.UserId.ToString());

			if (!string.IsNullOrEmpty(evt.RemindMeEmote))
			{
				if (reaction.Emote.Name == evt.GetRemindMeEmote().Name)
				{
					ReminderService.SetReminder(evt, attendee);
				}
			}

			for (int i = 0; i < evt.Statuses.Count; i++)
			{
				Event.Status status = evt.Statuses[i];

				if (reaction.Emote.Name == status.GetEmote().Name)
				{
					evt.SetAttendeeStatus(reaction.UserId.ToString(), i);
				}
			}

			await evt.UpdateNotifications();

			RestUserMessage userMessage = (RestUserMessage)await channel.GetMessageAsync(message.Id);
			SocketUser user = Program.DiscordClient.GetUser(reaction.UserId);
			await userMessage.RemoveReactionAsync(reaction.Emote, user);
		}
	}
}
