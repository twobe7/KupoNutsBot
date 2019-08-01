// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using KupoNuts.Utils;
	using NodaTime;

	[Serializable]
	public class Event
	{
		[Flags]
		public enum Days
		{
			None = 0,

			Monday = 1,
			Tuesday = 2,
			Wednesday = 4,
			Thursday = 8,
			Friday = 16,
			Saturday = 32,
			Sunday = 64,
		}

		public enum Colors
		{
			Default,
			DarkerGrey,
			DarkGrey,
			LighterGrey,
			DarkRed,
			Red,
			DarkOrange,
			Orange,
			LightOrange,
			Gold,
			LightGrey,
			Magenta,
			DarkPurple,
			Purple,
			DarkBlue,
			Blue,
			DarkGreen,
			Green,
			DarkTeal,
			Teal,
			DarkMagenta,
		}

		public string Id { get; set; }

		public ulong ChannelId { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public string Image { get; set; }

		public Colors Color { get; set; }

		public Instant DateTime { get; set; }

		public Days Repeats { get; set; }

		public Duration Duration { get; set; }

		public string RemindMeEmote { get; set; }

		public List<Status> Statuses { get; set; } = new List<Status>();

		public List<Attendee> Attendees { get; set; } = new List<Attendee>();

		public List<Notification> Notifications { get; set; } = new List<Notification>();

		public Attendee GetAttendee(ulong userId)
		{
			foreach (Attendee attendee in this.Attendees)
			{
				if (attendee.UserId == userId)
				{
					return attendee;
				}
			}

			Attendee newAttendee = new Attendee();
			newAttendee.UserId = userId;
			this.Attendees.Add(newAttendee);
			return newAttendee;
		}

		public Instant NextOccurance(DateTimeZone zone)
		{
			Instant now = SystemClock.Instance.GetCurrentInstant();
			if (this.DateTime < now && this.Repeats != 0)
			{
				LocalDate nextDate;
				LocalDateTime dateTime = this.DateTime.InZone(zone).LocalDateTime;

				LocalDate date = dateTime.Date;
				LocalDate todaysDate = TimeUtils.Now.InZone(zone).LocalDateTime.Date;

				List<LocalDate> dates = new List<LocalDate>();
				foreach (Days day in Enum.GetValues(typeof(Days)))
				{
					if (!FlagsUtils.IsSet(this.Repeats, day))
						continue;

					nextDate = todaysDate.Next(TimeUtils.ToIsoDay(day));
					dates.Add(nextDate);
				}

				if (dates.Count <= 0)
					return this.DateTime;

				dates.Sort();
				nextDate = dates[0];

				Period dateOffset = nextDate - date;
				dateTime = dateTime + dateOffset;

				Instant nextInstant = dateTime.InZoneLeniently(zone).ToInstant();
				return nextInstant;
			}

			return this.DateTime;
		}

		public class Status
		{
			public string EmoteString;
			public string Display;

			public Status()
			{
			}

			public Status(string emote, string display = null)
			{
				this.EmoteString = emote;
				this.Display = display;
			}
		}

		public class Attendee
		{
			public ulong UserId;
			public int Status;
			public Duration? RemindTime;
		}

		[Serializable]
		public class Notification
		{
			public ulong MessageId;
		}
	}
}
