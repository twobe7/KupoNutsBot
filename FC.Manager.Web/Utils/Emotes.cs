// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web
{
	using System.Collections.Generic;
	using FC.Manager.Web.Services;

	public static class Emotes
	{
		public static List<Emote> GetEmotes(
			EmoteService emoteService)
		{
			List<Emote> allEmotes = emoteService.GetEmotes(RPCService.GuildId);

			////allEmotes.Sort((Emote a, Emote b) =>
			////{
			////	return a.Name.CompareTo(b.Name);
			////});

			return allEmotes;
		}
	}
}
