// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

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
