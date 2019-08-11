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
			await notify.Post(self.Id);

			Database db = Database.Load();
			db.Notifications.Add(notify);
			db.Save();
		}

		public static async Task UpdateNotifications(this Event self)
		{
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

		public static SocketTextChannel GetChannel(this Event self)
		{
			if (string.IsNullOrEmpty(self.ChannelId))
				return null;

			ulong id = ulong.Parse(self.ChannelId);

			SocketChannel channel = Program.DiscordClient.GetChannel(id);

			if (channel is SocketTextChannel textChannel)
				return textChannel;

			throw new Exception("Channel: \"" + self.ChannelId + "\" is not a text channel");
		}

		public static string GetRepeatsString(this Event self)
		{
			if (self.Repeats == Event.Days.None)
				return null;

			StringBuilder builder = new StringBuilder();
			builder.Append("Every ");

			int count = 0;
			foreach (Event.Days day in Enum.GetValues(typeof(Event.Days)))
			{
				if (FlagsUtils.IsSet(self.Repeats, day))
				{
					if (count > 0)
						builder.Append(", ");

					count++;
					builder.Append(day);
				}
			}

			if (count == 7)
			{
				builder.Clear();
				builder.Append("Every day");
			}

			return builder.ToString();
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

						builder.Append(attendee.GetMention());
					}
					else
					{
						builder.AppendLine(attendee.GetMention());
					}
				}
			}

			if (total <= 0)
				builder.Append("No one yet");

			return builder.ToString();
		}

		public static Duration? GetNotifyDuration(this Event self)
		{
			if (string.IsNullOrEmpty(self.NotifyDuration))
				return null;

			return DurationPattern.Roundtrip.Parse(self.NotifyDuration).Value;
		}

		public static Duration GetDuration(this Event self)
		{
			if (string.IsNullOrEmpty(self.Duration))
				return Duration.FromSeconds(0);

			return DurationPattern.Roundtrip.Parse(self.Duration).Value;
		}

		public static string GetWhenString(this Event self)
		{
			Duration? durationTill = self.GetDurationTill();

			if (durationTill == null)
				return "Never";

			Duration time = (Duration)durationTill;

			if (time.TotalSeconds < 0)
			{
				Instant now = TimeUtils.RoundInstant(TimeUtils.Now);
				Instant instant = now + time + self.GetDuration();
				Duration endsIn = instant - now;
				return "Ends in" + TimeUtils.GetDurationString(endsIn);
			}

			return "In" + TimeUtils.GetDurationString(time);
		}

		public static Instant GetDateTime(this Event self)
		{
			if (string.IsNullOrEmpty(self.DateTime))
				return Instant.FromJulianDate(0);

			return InstantPattern.ExtendedIso.Parse(self.DateTime).Value;
		}

		public static Duration? GetDurationTill(this Event self)
		{
			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			Duration? duration = self.GetNextOccurance(zone) - TimeUtils.RoundInstant(TimeUtils.Now);
			return duration;
		}

		public static List<Instant> GetNextOccurances(this Event self, DateTimeZone zone)
		{
			Instant eventDateTime = self.GetDateTime();
			Duration eventDuration = self.GetDuration();
			Instant now = SystemClock.Instance.GetCurrentInstant();

			List<Instant> occurances = new List<Instant>();

			if (self.Repeats == 0)
			{
				if (eventDateTime > now)
				{
					occurances.Add(eventDateTime);
				}
			}
			else
			{
				LocalDateTime dateTime = eventDateTime.InZone(zone).LocalDateTime;

				LocalDate date = dateTime.Date;
				LocalDate todaysDate = TimeUtils.Now.InZone(zone).LocalDateTime.Date;

				List<LocalDate> dates = new List<LocalDate>();
				foreach (Event.Days day in Enum.GetValues(typeof(Event.Days)))
				{
					if (!FlagsUtils.IsSet(self.Repeats, day))
						continue;

					IsoDayOfWeek dayOfWeek = TimeUtils.ToIsoDay(day);

					if (todaysDate.DayOfWeek == dayOfWeek)
					{
						dates.Add(todaysDate);
					}
					else
					{
						dates.Add(todaysDate.Next(dayOfWeek));
					}
				}

				dates.Sort();

				Period dayOffset = dateTime - date.AtMidnight();

				foreach (LocalDate nextDate in dates)
				{
					LocalDateTime nextDateTime = nextDate.AtMidnight() + dayOffset;
					Instant occurance = nextDateTime.InZoneLeniently(zone).ToInstant();

					if (occurance + eventDuration < now)
						continue;

					occurances.Add(occurance);
				}
			}

			return occurances;
		}

		public static Instant? GetNextOccurance(this Event self, DateTimeZone zone)
		{
			List<Instant> occurances = self.GetNextOccurances(zone);
			if (occurances == null || occurances.Count <= 0)
				return null;

			return occurances[0];
		}
	}
}
