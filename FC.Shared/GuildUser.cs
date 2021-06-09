// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;

	[Serializable]
	public class GuildUser
	{
		public GuildUser()
		{
		}

		public GuildUser(ulong userId, string name)
		{
			this.UserId = userId.ToString();
			this.Name = name;
		}

		public GuildUser(ulong userId, string name, int nuts)
		{
			this.UserId = userId.ToString();
			this.Name = name;
			this.Nuts = nuts;
		}

		public GuildUser(ulong userId, string name, int nuts, int xp, int level, int rep)
		{
			this.UserId = userId.ToString();
			this.Name = name;
			this.Nuts = nuts;
			this.TotalXPCurrent = xp;
			this.Level = level;
			this.Rep = rep;
		}

		public string UserId { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public int Nuts { get; set; }

		public int TotalXPCurrent { get; set; }
		public int Level { get; set; }

		public int Rep { get; set; }
	}
}
