// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Mounts
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Services;
	using FFXIVCollect;

	[Group("xiv-data", "Queries data in FFXIV")]
	public class MountService : ServiceBase
	{
		public MountService(DiscordSocketClient discordClient)
		{
		}

		[SlashCommand("mount-search", "Gets information on a mount")]
		[RequireAny]
		public async Task GetMount(
			[Summary("search", "Search query")]
			string? search = null,
			[Summary("itemId", "Item Id to lookup")]
			ulong? itemId = null)
		{
			await this.DeferAsync();

			if (!itemId.HasValue)
			{
				if (search == null)
					throw new UserException("Something went wrong");

				List<SearchAPI.Result> results = await SearchAPI.Search(SearchAPI.SearchType.Mounts, search);

				if (results.Count <= 0)
				{
					await this.FollowupAsync("I couldn't find any mounts that match that search.");
				}

				if (results.Count > 1)
				{
					EmbedBuilder embed = new EmbedBuilder().WithTitle($"{results.Count} results found");
					StringBuilder description = new ();

					for (int i = 0; i < Math.Min(results.Count, 10); i++)
					{
						description.AppendLine($"{results[i].ID} - {results[i].Name}");
					}

					embed.Description = description.ToString();

					await this.FollowupAsync(embeds: new Embed[] { embed.Build() });
					return;
				}

				itemId = results[0].ID ?? throw new Exception("No Id in mount");
			}

			MountAPI.Mount mount = await MountAPI.Get(itemId.Value);

			await this.FollowupAsync(embeds: new Embed[] { mount.ToEmbed().Build() });
		}
	}
}
