// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Discord;
	using Discord.WebSocket;

	public static class EmoteExtensions
	{
		public static bool IsAvailable(this Emote self)
		{
			foreach (SocketGuild guild in Program.DiscordClient.Guilds)
			{
				foreach (GuildEmote emote in guild.Emotes)
				{
					if (emote.Id == self.Id)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
