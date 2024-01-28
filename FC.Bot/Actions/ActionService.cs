// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Actions
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using XIVAPI;

	[Group("xiv-data", "Queries data in FFXIV")]
	public class ActionService : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient;
		public ActionService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		[SlashCommand("action-search", "Gets information on an action")]
		[RequireAny]

		public async Task GetAction(
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

				List<SearchAPI.Result> results = await SearchAPI.Search(search, "Action");

				if (results.Count <= 0)
				{
					await this.FollowupAsync("I couldn't find any actions that match that search.");
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

				itemId = results[0].ID ?? throw new Exception("No Id in item");
			}

			XIVAPI.Action action = await ActionAPI.Get(itemId.Value);

			await this.FollowupAsync(embeds: new Embed[] { action.ToEmbed().Build() });
		}
	}
}
