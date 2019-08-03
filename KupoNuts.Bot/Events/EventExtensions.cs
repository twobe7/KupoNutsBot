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

		public static void SetAttendeeStatus(this Event self, ulong userId, int status)
		{
			Database db = Database.Load();

			Attendee attendee = self.GetAttendee(db, userId);
			attendee.Status = status;

			db.Save();
		}

		public static Attendee GetAttendee(this Event self, ulong userId)
		{
			Database db = Database.Load();
			return self.GetAttendee(db, userId);
		}

		public static Attendee GetAttendee(this Event self, Database db, ulong userId)
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

		public static string GetAttendeeString(this Event self, int statusIndex, out int count)
		{
			StringBuilder builder = new StringBuilder();

			Database db = Database.Load();
			count = 0;
			foreach (Attendee attendee in db.Attendees)
			{
				if (attendee.EventId != self.Id)
					continue;

				if (attendee.Status == statusIndex)
				{
					count++;
					builder.AppendLine(attendee.GetMention());
				}
			}

			if (count <= 0)
				builder.Append("No one yet");

			if (count > 8)
			{
				builder.Clear();
				builder.Append(count + " people");
			}

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

		public static Instant GetDateTime(this Event self)
		{
			if (string.IsNullOrEmpty(self.DateTime))
				return Instant.FromJulianDate(0);

			return InstantPattern.ExtendedIso.Parse(self.DateTime).Value;
		}

		public static Duration? GetDurationTill(this Event self)
		{
			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			return self.GetNextOccurance(zone) - TimeUtils.Now;
		}

		public static int GetDaysTill(this Event self)
		{
			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			Instant? nextOccurance = self.GetNextOccurance(zone);

			if (nextOccurance == null)
				return -1;

			ZonedDateTime zdt = TimeUtils.Now.InZone(zone);
			LocalDateTime ldt = zdt.LocalDateTime;
			ldt = ldt.Date.AtMidnight();
			zdt = ldt.InZoneLeniently(zone);

			Duration duration = ((Instant)nextOccurance) - zdt.ToInstant();

			return (int)Math.Floor(duration.TotalDays);
		}

		public static Instant? GetNextOccurance(this Event self, DateTimeZone zone)
		{
			Instant eventDateTime = self.GetDateTime();

			Instant now = SystemClock.Instance.GetCurrentInstant();
			if (eventDateTime < now)
			{
				if (self.Repeats == 0)
				{
					// This event has already happened.
					return null;
				}

				LocalDate nextDate;
				LocalDateTime dateTime = eventDateTime.InZone(zone).LocalDateTime;

				LocalDate date = dateTime.Date;
				LocalDate todaysDate = TimeUtils.Now.InZone(zone).LocalDateTime.Date;

				List<LocalDate> dates = new List<LocalDate>();
				foreach (Event.Days day in Enum.GetValues(typeof(Event.Days)))
				{
					if (!FlagsUtils.IsSet(self.Repeats, day))
						continue;

					nextDate = todaysDate.Next(TimeUtils.ToIsoDay(day));
					dates.Add(nextDate);
				}

				if (dates.Count <= 0)
					return eventDateTime;

				dates.Sort();
				nextDate = dates[0];

				Period dateOffset = nextDate - date;
				dateTime = dateTime + dateOffset;

				Instant nextInstant = dateTime.InZoneLeniently(zone).ToInstant();
				return nextInstant;
			}

			return eventDateTime;
		}
	}
}
