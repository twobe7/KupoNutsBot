// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using KupoNuts.Utils;

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

		public string ChannelId { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public string Image { get; set; }

		public Colors Color { get; set; }

		public string DateTime { get; set; }

		public Days Repeats { get; set; }

		public string Duration { get; set; }

		public string RemindMeEmote { get; set; }

		public List<Status> Statuses { get; set; }

		public class Status
		{
			public Status()
			{
			}

			public Status(string emote, string display = null)
			{
				this.EmoteString = emote;
				this.Display = display;
			}

			public string EmoteString { get; set; }

			public string Display { get; set; }
		}
	}
}
