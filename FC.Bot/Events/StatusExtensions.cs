// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Discord;
	using FC.Events;

	public static class StatusExtensions
	{
		public static IEmote GetEmote(this Event.Status self)
		{
			return Emote.Parse(self.EmoteString);
		}
	}
}
