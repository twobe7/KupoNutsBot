// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.ReactionRoles
{
	using System;

	[Serializable]
	public class ReactionRoleHeader : EntryBase
	{
		public ReactionRoleHeader()
		{
		}

		public ReactionRoleHeader(ReactionRole rr)
		{
			this.Id = rr.Id;
			this.GuildId = rr.GuildId;
			this.ChannelId = rr.ChannelId;

			if (rr.MessageId.HasValue)
				this.MessageId = rr.MessageId;
		}

		public ulong GuildId { get; set; } = 0;
		public ulong? ChannelId { get; set; }
		public ulong? MessageId { get; set; }
	}
}
