// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot
{
	using Discord;

	public static class GuildUserExtensions
	{
		public static string GetName(this IGuildUser self)
		{
			if (self == null)
				return "Unknown User";

			if (string.IsNullOrEmpty(self.Nickname))
				return self.Username;

			return self.Nickname;
		}
	}
}
