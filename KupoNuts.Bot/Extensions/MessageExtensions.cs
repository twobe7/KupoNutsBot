// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot
{
	using System;
	using Discord;
	using Discord.WebSocket;

	public static class MessageExtensions
	{
		public static IGuildUser GetAuthor(this IMessage self)
		{
			if (self.Author is SocketGuildUser guildUser)
				return guildUser;

			if (self.Channel is SocketGuildChannel guildChannel)
			{
				return guildChannel.Guild.GetUser(self.Author.Id);
			}
			else
			{
				throw new Exception("Message was not in a guild channel.");
			}
		}

		public static IGuild GetGuild(this IMessage self)
		{
			if (self.Channel is SocketGuildChannel guildChannel)
			{
				return guildChannel.Guild;
			}
			else
			{
				throw new Exception("Message was not in a guild channel.");
			}
		}
	}
}
