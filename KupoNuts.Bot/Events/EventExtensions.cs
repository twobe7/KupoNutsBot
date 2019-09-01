// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Events;
	using KupoNuts.Utils;
	using NodaTime;
	using NodaTime.Text;

	public static class EventExtensions
	{
		public static async Task Post(this Event self)
		{
			Notification notify = new Notification();
			notify.EventId = self.Id;
			notify.ChannelId = self.ChannelId;

			if (self.Id != null)
				await notify.Post(self.Id);

			Database db = Database.Load();
			db.Notifications.Add(notify);
			db.Save();
		}

		public static async Task UpdateNotifications(this Event self)
		{
			if (self.Id == null)
				throw new ArgumentNullException("Id");

			Database db = Database.Load();
			foreach (Notification notify in db.Notifications)
			{
				if (notify.EventId != self.Id)
					continue;

				await notify.Post(self.Id);
			}
		}

		public static async Task CheckReactions(this Event self)
		{
			Database db = Database.Load();
			foreach (Notification notify in db.Notifications)
			{
				if (notify.EventId != self.Id)
					continue;

				await notify.CheckReactions(self);
			}

			await self.UpdateNotifications();
		}

		public static Task Delete(this Event self)
		{
			Log.Write("Delete Event: \"" + self.Name + "\" (" + self.Id + ")");
			Database db = Database.Load();
			db.DeleteEvent(self.Id);
			db.Save();

			// TODO: Delete any event notifications that have been posted
			// TODO: Clear any watched messages from the EventsService
			return Task.CompletedTask;
		}

		public static IEmote GetRemindMeEmote(this Event self)
		{
			return Emote.Parse(self.RemindMeEmote);
		}

		public static SocketTextChannel? GetChannel(this Event self)
		{
			if (string.IsNullOrEmpty(self.ChannelId))
				return null;

			ulong id = ulong.Parse(self.ChannelId);

			SocketChannel channel = Program.DiscordClient.GetChannel(id);

			if (channel is SocketTextChannel textChannel)
				return textChannel;

			throw new Exception("Channel: \"" + self.ChannelId + "\" is not a text channel");
		}

		/// <summary>
		/// Deletes all attendees for the given event.
		/// </summary>
		public static void ClearAttendees(this Event self, Database db)
		{
			if (self == null)
				return;

			for (int i = db.Attendees.Count - 1; i >= 0; i--)
			{
				if (db.Attendees[i] == null)
					continue;

				if (db.Attendees[i].EventId == self.Id)
				{
					db.Attendees.RemoveAt(i);
				}
			}
		}

		public static void SetAttendeeStatus(this Event self, string userId, int status)
		{
			Database db = Database.Load();

			Attendee attendee = self.GetAttendee(db, userId);
			attendee.Status = status;

			db.Save();
		}

		public static List<Attendee> GetAttendees(this Event self)
		{
			List<Attendee> attendees = new List<Attendee>();
			Database db = Database.Load();

			foreach (Attendee attendee in db.Attendees)
			{
				if (attendee.EventId != self.Id)
					continue;

				attendees.Add(attendee);
			}

			return attendees;
		}

		public static Attendee GetAttendee(this Event self, string userId)
		{
			Database db = Database.Load();
			return self.GetAttendee(db, userId);
		}

		public static Attendee GetAttendee(this Event self, Database db, string userId)
		{
			if (self.Id == null)
				throw new ArgumentNullException("Id");

			foreach (Attendee attendee in db.Attendees)
			{
				if (attendee.EventId != self.Id)
					continue;

				if (!attendee.Is(userId))
					continue;

				return attendee;
			}

			Attendee newAttendee = new Attendee(self.Id, userId);
			db.Attendees.Add(newAttendee);
			db.Save();
			return newAttendee;
		}

		public static string GetAttendeeString(this Event self, int statusIndex, out int total)
		{
			StringBuilder builder = new StringBuilder();

			Database db = Database.Load();
			total = 0;
			foreach (Attendee attendee in db.Attendees)
			{
				if (attendee.EventId == self.Id && attendee.Status == statusIndex)
				{
					total++;
				}
			}

			int count = 0;
			foreach (Attendee attendee in db.Attendees)
			{
				if (attendee.EventId != self.Id)
					continue;

				if (attendee.Status == statusIndex)
				{
					count++;

					if (total > 8)
					{
						if (count > 1)
							builder.Append(", ");

						builder.Append(attendee.GetName());
					}
					else
					{
						builder.AppendLine(attendee.GetName());
					}
				}
			}

			if (total <= 0)
				builder.Append("No one yet");

			return builder.ToString();
		}
	}
}
