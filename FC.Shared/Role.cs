// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;

	[Serializable]
	public class Role
	{
		public Role()
		{
		}

		public Role(ulong discordId, string name)
		{
			this.DiscordId = discordId.ToString();
			this.Name = name;
		}

		public string DiscordId { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;
	}
}
