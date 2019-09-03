// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;

	[Serializable]
	public class Channel : EntryBase
	{
		public Channel()
		{
		}

		public Channel(ulong discordId, string name, Types type)
		{
			this.DiscordId = discordId.ToString();
			this.Name = name;
			this.Type = type;
		}

		public enum Types
		{
			Unknown,
			Text,
			Voice,
		}

		public string? DiscordId { get; set; }

		public string? Name { get; set; }

		public Types Type { get; set; }
	}
}
