// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class Event : EntryBase
	{
		public string ServerId = "391492798353768449";
		public string? ChannelId;
		public string? Name;
		public string? Description;
		public string? Image;
		public Colors Color;
		public string? DateTime;
		public Days Repeats;
		public string? Duration;
		public string? RemindMeEmote;
		public string? NotifyDuration;
		public List<Status> Statuses = new List<Status>();
		public Notification? Notify;

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

		public class Status
		{
			public string? EmoteString;
			public string? Display;

			public Status()
			{
			}

			public Status(string emote, string? display = null)
			{
				this.EmoteString = emote;
				this.Display = display;
			}
		}

		public class Notification
		{
			public ulong? MessageId;
			public List<Attendee> Attendees = new List<Attendee>();

			public class Attendee
			{
				public ulong? UserId;
				public int? Status;
				public string? RemindTime;
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
