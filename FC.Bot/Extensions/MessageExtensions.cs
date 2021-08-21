﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot
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

		public static async Task<Dictionary<IEmote, int>> GetReactions(this IUserMessage? self)
		{
			Dictionary<IEmote, int> results = new Dictionary<IEmote, int>();

			if (self == null)
				return results;

			foreach ((IEmote emote, ReactionMetadata _) in self.Reactions)
			{
				IEnumerable<IUser> users = await self.GetReactionUsersAsync(emote, 999).FlattenAsync();

				int count = 0;
				foreach (IUser user in users)
					count++;

				if (!results.ContainsKey(emote))
					results.Add(emote, 0);

				results[emote] += count;
			}

			return results;
		}
	}
}
