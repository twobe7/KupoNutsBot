// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Events
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
	using FC.Data;
	using FC.Events;
	using NodaTime;

	public class EventsService : ServiceBase
	{
		public static Table<Event> EventsDatabase = Table<Event>.Create("Events", 1);

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

			ScheduleService.RunOnSchedule(this.Update, 15);
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
			// TODO: store event ID's in a separate table so we aren't scanning the entire events db each update.
			List<Event> events = await EventsDatabase.LoadAll();
			foreach (Event evt in events)
			{
				if (evt.Id == null)
					continue;

				// delete non repeating events that have passed
				if (!evt.DoesRepeat())
				{
					Event.Occurance? next = evt.GetNextOccurance();

					// will never occur (past event)
					if (next == null)
					{
						Log.Write("Delete Event: \"" + evt.Name + "\" (" + evt.Id + ")", "Bot");
						await EventsDatabase.Delete(evt.Id);

						if (evt.Notify != null)
						{
							await evt.Notify.Delete(evt);
						}

						continue;
					}
				}

				SocketTextChannel? channel = evt.GetChannel();

				if (channel is null)
					continue;

				if (this.ShouldNotify(evt))
				{
					if (evt.Notify == null)
						evt.Notify = new Event.Notification();

					this.Watch(evt);

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
			if (evt.GetIsOccuring())
				return true;

			Duration? timeTillEvent = evt.GetDurationTill();

			// Event in the past.
			if (timeTillEvent == null)
				return false;

			Duration? notifyDuration = evt.NotifyDuration;

			// never notify
			if (notifyDuration == null)
				return false;

			// instant notify
			if (notifyDuration.Value.TotalSeconds == 0)
				return true;

			return timeTillEvent.Value <= notifyDuration.Value;
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				if (!this.messageEventLookup.ContainsKey(message.Id.ToString()))
					return;

				// don't mark yourself as attending!
				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return;

				string eventId = this.messageEventLookup[message.Id.ToString()];
				Event? evt = await EventsDatabase.Load(eventId);

				if (evt is null)
				{
					// this event was deleted while the notification was up.
					// we need to detect this case in the 'Update' loop to clear old notifications.
					// but for now, we'll handle it when someone reacts.
					this.messageEventLookup.Remove(message.Id.ToString());
					await channel.DeleteMessageAsync(message.Value);
					return;
				}

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
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
