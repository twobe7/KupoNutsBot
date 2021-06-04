// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot
{
	using Discord;

	public static class Emotes
	{
		public static IEmote Yes = Emote.Parse(@"<:Yes:835536105586950185>");
		public static IEmote No = Emote.Parse(@"<:No:835536115981221960>");
		public static IEmote Maybe = Emote.Parse(@"<:Maybe:835536125887119431>");
		public static IEmote Bell = new Emoji(@"🔔");
		public static IEmote Tank = Emote.Parse(@"<:Tank:835536215574970378>");
		public static IEmote Healer = Emote.Parse(@"<:Healer:835536228001906720>");
		public static IEmote DPS = Emote.Parse(@"<:Dps:835536256674431006>");
		public static IEmote Home = Emote.Parse(@"<:home:800382941834117151>");

		public static bool IsEmote(IEmote a, IEmote b)
		{
			if (a == null)
				return b == null;

			if (b == null)
				return a == null;

			return a.Name == b.Name;
		}

		public static string GetString(this IEmote self)
		{
			if (self is Emote emote)
			{
				return "<:" + emote.Name + ":" + emote.Id + ">";
			}

			return self.Name;
		}
	}
}
