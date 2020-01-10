// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;

	[Serializable]
	public class ChannelData : EntryBase
	{
		public OptInInfo? OptIn { get; set; }

		public string GuildId { get; set; } = string.Empty;

		public string ChannelId { get; set; } = string.Empty;

		[Serializable]
		public class OptInInfo
		{
			public string Emote { get; set; } = string.Empty;

			public string Role { get; set; } = string.Empty;
		}
	}
}
