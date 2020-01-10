// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot.Utils
{
	using Discord;

	public static class EmoteUtility
	{
		public static IEmote Parse(string str)
		{
			Emote result;
			if (Emote.TryParse(str, out result))
				return result;

			return new Emoji(str);
		}
	}
}
