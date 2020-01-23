// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Discord.WebSocket;

	public static class DiscrodClientExtensions
	{
		public static SocketGuild GetGuild(this DiscordSocketClient self, string guildId)
		{
			return self.GetGuild(ulong.Parse(guildId));
		}
	}
}
