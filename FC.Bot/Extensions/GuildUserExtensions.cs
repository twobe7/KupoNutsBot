// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot
{
	using Discord;

	public static class GuildUserExtensions
	{
		public static string GetName(this IGuildUser self)
		{
			if (string.IsNullOrEmpty(self.Nickname))
				return self.Username;

			return self.Nickname;
		}
	}
}
