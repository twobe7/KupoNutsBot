// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
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

		public static async Task<Dictionary<string, int>> GetReactions(this IUserMessage? self)
		{
			Dictionary<string, int> results = new Dictionary<string, int>();

			if (self == null)
				return results;

			foreach ((IEmote emote, ReactionMetadata data) in self.Reactions)
			{
				IEnumerable<IUser> users = await self.GetReactionUsersAsync(emote, 999).FlattenAsync();

				int count = 0;
				foreach (IUser user in users)
					count++;

				if (!results.ContainsKey(emote.Name))
					results.Add(emote.Name, 0);

				results[emote.Name] += count;
			}

			return results;
		}
	}
}
