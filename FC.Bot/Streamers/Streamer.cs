// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;

	[Serializable]
	public class Streamer : EntryBase
	{
		public ulong DiscordUserId { get; set; }
		public ulong DiscordGuildId { get; set; }
		public string? GuildNickName { get; set; }
		public ContentInfo? Twitch { get; set; }
		public ContentInfo? Youtube { get; set; }

		[Serializable]
		public class ContentInfo
		{
			public ContentInfo(string username)
			{
				this.UserName = username;
			}

			public string? UserName { get; set; }
			public string? LastStreamId { get; set; }
		}
	}
}
