// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;

	[Serializable]
	public class Channel
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

		public string DiscordId { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public Types Type { get; set; }
	}
}
