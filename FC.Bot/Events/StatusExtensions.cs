// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

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
