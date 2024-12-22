// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Guild
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Services;

	[Group("guild", "Shows guild related commands")]
	public class GuildService : ServiceBase
	{
		public GuildService(DiscordSocketClient discordClient)
		{
		}

		public override async Task Initialize()
		{
			await base.Initialize();
		}

		[SlashCommand("oldest", "Lists the oldest Discord members")]
		public async Task Oldest(
			[Summary("numberToReturn", "Number of members to display (max 15)")]
			int numberToReturn = 1)
		{
			await this.DeferAsync();

			if (this.Context.Channel is SocketGuildChannel guildChannel)
			{
				// Put a cap on the return amount for now
				numberToReturn = numberToReturn > 15 ? 15 : numberToReturn;

				IEnumerable<SocketGuildUser> users = guildChannel.Guild.Users.OrderBy(x => x.JoinedAt).Take(numberToReturn);

				string oldestString = string.Empty;
				int order = 1;

				foreach (SocketGuildUser user in users)
				{
					oldestString += $"{order++}. {user.GetName()}\n";
				}

				await this.FollowupAsync(embeds: [GetEmbed("Oldest Members", oldestString, this.Context.Guild.IconUrl)]);
				return;
			}

			await this.FollowupAsync(embeds: [GetEmbed("You're the oldest!", string.Empty)]);
		}

		[SlashCommand("newest", "Lists the newest Discord members")]
		public async Task Newest(
			[Summary("numberToReturn", "Number of members to display (max 15)")]
			int numberToReturn = 1)
		{
			await this.DeferAsync();

			if (this.Context.Channel is SocketGuildChannel guildChannel)
			{
				// Put a cap on the return amount for now
				numberToReturn = numberToReturn > 15 ? 15 : numberToReturn;

				IEnumerable<SocketGuildUser> users = guildChannel.Guild.Users.OrderByDescending(x => x.JoinedAt).Take(numberToReturn);

				string members = string.Empty;
				int order = 1;

				foreach (SocketGuildUser user in users)
				{
					members += $"{order++}. {user.GetName()}\n";
				}

				await this.FollowupAsync(embeds: [GetEmbed("Newest Members", members, this.Context.Guild.IconUrl)]);
				return;
			}

			await this.FollowupAsync(embeds: [GetEmbed("You're the youngest!", string.Empty)]);
		}

		private static Embed GetEmbed(string title, string description, string? iconUrl = null)
		{
			EmbedBuilder builder = new EmbedBuilder()
				.WithTitle(title)
				.WithDescription(description);

			if (!string.IsNullOrWhiteSpace(iconUrl))
				builder.AddThumbnail(iconUrl);

			return builder.Build();
		}
	}
}
