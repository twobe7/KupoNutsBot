// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot
{
	using Discord;

	public static class Emotes
	{
		public static IEmote Yes = Emote.Parse(@"<:yes:669831908003282944>");
		public static IEmote No = Emote.Parse(@"<:no:669831924679835648>");
		public static IEmote Maybe = Emote.Parse(@"<:maybe:669831957634351115>");
		public static IEmote Bell = new Emoji(@"🔔");
		public static IEmote Tank = Emote.Parse(@"<:tank:669841051292270606>");
		public static IEmote Healer = Emote.Parse(@"<:healer:669841051711700992>");
		public static IEmote DPS = Emote.Parse(@"<:dps:669841051703443476>");

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
