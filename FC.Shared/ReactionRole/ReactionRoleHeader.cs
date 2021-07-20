// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Events
{
	using System;

	[Serializable]
	public class ReactionRoleHeader : EntryBase
	{
		public string GuildId { get; set; } = "0";
		public ulong MessageId { get; set; } = 0;
	}
}
