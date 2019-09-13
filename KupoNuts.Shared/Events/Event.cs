// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class Event : EntryBase
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

		public string ServerId { get; set; } = "391492798353768449";

		public string? ChannelId { get; set; }

		public string? Name { get; set; }

		public string? ShortDescription { get; set; }

		public string? Description { get; set; }

		public string? Image { get; set; }

		public Colors Color { get; set; }

		public string? DateTime { get; set; }

		public Days Repeats { get; set; }

		public string? Duration { get; set; }

		public string? RemindMeEmote { get; set; }

		public string? NotifyDuration { get; set; }

		public List<Status> Statuses { get; set; } = new List<Status>();

		public Notification? Notify { get; set; }

		public override string ToString()
		{
			return "Event: " + this.Name;
		}

		public class Status
		{
			public Status()
			{
			}

			public Status(string emote, string? display = null)
			{
				this.EmoteString = emote;
				this.Display = display;
			}

			public string? EmoteString { get; set; }

			public string? Display { get; set; }
		}

		public class Notification
		{
			public string? MessageId { get; set; }

			public List<Attendee> Attendees { get; set; } = new List<Attendee>();

			public class Attendee
			{
				public string? UserId { get; set; }

				public int? Status { get; set; }

				public string? RemindTime { get; set; }
			}
		}
	}

	#pragma warning disable SA1402, SA1204
	public static class DaysUtil
	{
		public static Event.Days[] AllDays = new List<Event.Days>()
		{
			Event.Days.Monday,
			Event.Days.Tuesday,
			Event.Days.Wednesday,
			Event.Days.Thursday,
			Event.Days.Friday,
			Event.Days.Saturday,
			Event.Days.Sunday,
		}.ToArray();
	}
}
