// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Guild
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Utils;

	public class GuildService : ServiceBase
	{
		public override async Task Initialize()
		{
			await base.Initialize();
		}

		[Command("Oldest", Permissions.Everyone, "Lists the oldest Discord members")]
		public Task<Embed> Oldest(CommandMessage message)
		{
			if (message.Channel is SocketGuildChannel guildChannel)
			{
				IEnumerable<SocketGuildUser> users = guildChannel.Guild.Users.OrderBy(x => x.JoinedAt).Take(1);

				string oldestString = string.Empty;
				int order = 1;

				foreach (SocketGuildUser user in users)
				{
					oldestString += string.Format("{0}. {1}\n", order, user.GetName());
				}

				return Task.FromResult(this.GetEmbed("Oldest Members", oldestString));
			}

			return Task.FromResult(this.GetEmbed("You're the oldest!", string.Empty));
		}

		[Command("Oldest", Permissions.Everyone, "Lists the oldest Discord members")]
		public Task<Embed> Oldest(CommandMessage message, int numberToReturn)
		{
			if (message.Channel is SocketGuildChannel guildChannel)
			{
				// Put a cap on the return amount for now
				numberToReturn = numberToReturn > 15 ? 15 : numberToReturn;

				IEnumerable<SocketGuildUser> users = guildChannel.Guild.Users.OrderBy(x => x.JoinedAt).Take(numberToReturn);

				string oldestString = string.Empty;
				int order = 1;

				foreach (SocketGuildUser user in users)
				{
					oldestString += string.Format("{0}. {1}\n", order++, user.GetName());
				}

				return Task.FromResult(this.GetEmbed("Oldest Members", oldestString));
			}

			return Task.FromResult(this.GetEmbed("You're the oldest!", string.Empty));
		}

		[Command("Newest", Permissions.Everyone, "Lists the newest Discord members")]
		public Task<Embed> Newest(CommandMessage message)
		{
			if (message.Channel is SocketGuildChannel guildChannel)
			{
				IEnumerable<SocketGuildUser> users = guildChannel.Guild.Users.OrderByDescending(x => x.JoinedAt).Take(1);

				string oldestString = string.Empty;
				int order = 1;

				foreach (SocketGuildUser user in users)
				{
					oldestString += string.Format("{0}. {1}\n", order++, user.GetName());
				}

				return Task.FromResult(this.GetEmbed("Newest Members", oldestString));
			}

			return Task.FromResult(this.GetEmbed("You're the youngest!", string.Empty));
		}

		[Command("Newest", Permissions.Everyone, "Lists the newest Discord members")]
		public Task<Embed> Newest(CommandMessage message, int numberToReturn)
		{
			if (message.Channel is SocketGuildChannel guildChannel)
			{
				// Put a cap on the return amount for now
				numberToReturn = numberToReturn > 15 ? 15 : numberToReturn;

				IEnumerable<SocketGuildUser> users = guildChannel.Guild.Users.OrderByDescending(x => x.JoinedAt).Take(numberToReturn);

				string oldestString = string.Empty;
				int order = 1;

				foreach (SocketGuildUser user in users)
				{
					oldestString += string.Format("{0}. {1}\n", order++, user.GetName());
				}

				return Task.FromResult(this.GetEmbed("Newest Members", oldestString));
			}

			return Task.FromResult(this.GetEmbed("You're the youngest!", string.Empty));
		}

		private Embed GetEmbed(string title, string description)
		{
			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = title;
			builder.Description = description;

			return builder.Build();
		}
	}
}
