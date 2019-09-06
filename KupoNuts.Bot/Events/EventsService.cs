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
		public static Database<Event> EventsDatabase = new Database<Event>("Events", 1);

		private static EventsService? instance;
		private Dictionary<string, string> messageEventLookup = new Dictionary<string, string>();

		public static EventsService Instance
		{
			get
			{
				if (instance is null)
					throw new Exception("No Events Service");

				return instance;
			}
		}

		public override async Task Initialize()
		{
			instance = this;

			await EventsDatabase.Connect();

			Scheduler.RunOnSchedule(this.Update, 15);
			await this.Update();

			Program.DiscordClient.ReactionAdded += this.ReactionAdded;
		}

		public override Task Shutdown()
		{
			instance = null;
			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;

			return Task.CompletedTask;
		}

		public void Watch(Event evt)
		{
			if (evt.Notify == null || evt.Notify.MessageId == null || evt.Id == null)
				return;

			if (this.messageEventLookup.ContainsKey(evt.Notify.MessageId))
				return;

			this.messageEventLookup.Add(evt.Notify.MessageId, evt.Id);
		}

		[Command("Events", Permissions.Administrators, "Checks event notifications")]
		public async Task Update()
		{
			// todo: store event ID's in a seperate table so we aren't scanning the entire events db each update.
			List<Event> events = await EventsDatabase.LoadAll();
			foreach (Event evt in events)
			{
				if (evt.Id == null)
					continue;

				// delete non repeating events that have passed
				if (evt.Repeats == 0)
				{
					Instant? next = evt.GetNextOccurance();

					// will never occur (past event)
					if (next == null)
					{
						Log.Write("Delete Event: \"" + evt.Name + "\" (" + evt.Id + ")");
						await EventsDatabase.Delete(evt.Id);

						if (evt.Notify != null)
						{
							await evt.Notify.Delete(evt);
						}
					}

					continue;
				}

				SocketTextChannel? channel = evt.GetChannel();

				if (channel is null)
					continue;

				if (this.ShouldNotify(evt))
				{
					if (evt.Notify == null)
						evt.Notify = new Event.Notification();

					await evt.CheckReactions();

					await evt.Notify.Post(evt);
					await EventsDatabase.Save(evt);
				}
				else
				{
					if (evt.Notify != null)
					{
						// remove notification
						await evt.Notify.Delete(evt);
						evt.Notify = null;
						await EventsDatabase.Save(evt);
					}
				}
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

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!this.messageEventLookup.ContainsKey(message.Id.ToString()))
				return;

			// dont mark yourself as attending!
			if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
				return;

			Event evt = await EventsDatabase.Load(this.messageEventLookup[message.Id.ToString()]);

			if (evt.Notify == null)
				return;

			Event.Notification.Attendee? attendee = evt.GetAttendee(reaction.UserId);

			if (attendee == null)
			{
				attendee = new Event.Notification.Attendee();
				attendee.UserId = reaction.UserId.ToString();
				evt.Notify.Attendees.Add(attendee);
				await EventsDatabase.Save(evt);
			}

			if (!string.IsNullOrEmpty(evt.RemindMeEmote))
			{
				if (reaction.Emote.Name == evt.GetRemindMeEmote().Name)
				{
					ReminderService.SetReminder(evt, attendee);
				}
			}

			if (evt.Statuses != null)
			{
				for (int i = 0; i < evt.Statuses.Count; i++)
				{
					Event.Status status = evt.Statuses[i];

					if (reaction.Emote.Name == status.GetEmote().Name)
					{
						evt.SetAttendeeStatus(reaction.UserId, i);
						await EventsDatabase.Save(evt);
					}
				}
			}

			await evt.Notify.Post(evt);

			RestUserMessage userMessage = (RestUserMessage)await channel.GetMessageAsync(message.Id);
			SocketUser user = Program.DiscordClient.GetUser(reaction.UserId);
			await userMessage.RemoveReactionAsync(reaction.Emote, user);
		}
	}
}
