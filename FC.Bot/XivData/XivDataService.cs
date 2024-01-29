// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Mounts;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FC.API;
using FC.Bot.Items;
using FC.Bot.Services;
using FC.Bot.XivData;
using FC.XIVData;
using FFXIVCollect;
using Universalis;
using XIVAPI;

[Group("xiv-data", "Queries data in FFXIV")]
public class XivDataService : ServiceBase
{
	public readonly DiscordSocketClient DiscordClient;

	public XivDataService(DiscordSocketClient discordClient)
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

			List<Result> results = await XIVAPI.SearchAPI.Search(search, "Action");

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

	[SlashCommand("item-search", "Gets information on an item")]
	public async Task GetItem(
			[Autocomplete(typeof(ItemAutocompleteHandler))]
			[Summary("search", "Name or Item Id")]
			string search)
	{
		await this.DeferAsync();

		if (ulong.TryParse(search, out ulong searchAsUlong))
		{
			await this.FollowupAsync(embed: await GetItem(searchAsUlong));
			return;
		}

		List<Result> results = await XIVAPI.SearchAPI.Search(search, "Item");

		if (results.Count <= 0)
		{
			await this.FollowupAsync(text: "I couldn't find any items that match that search.");
			return;
		}

		if (results.Count > 1)
		{
			EmbedBuilder embed = new ();

			StringBuilder description = new ();
			for (int i = 0; i < Math.Min(results.Count, 10); i++)
			{
				description.AppendLine(results[i].ID + " - " + results[i].Name);
			}

			embed.Title = $"{results.Count} results found for \"{search}\"";
			embed.Description = description.ToString();

			await this.FollowupAsync(embed: embed.Build());
			return;
		}

		ulong? id = results[0].ID;

		if (id == null)
		{
			await this.FollowupAsync(text: "The returned item did not have an Id. Weird.");
			return;
		}

		await this.FollowupAsync(embed: await GetItem((ulong)id));
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

			List<Result> results = await FFXIVCollect.SearchAPI.Search(FFXIVCollect.SearchAPI.SearchType.Mounts, search);

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

	private static async Task<Embed> GetItem(ulong itemId)
	{
		Item item = await ItemAPI.Get(itemId);

		EmbedBuilder embed = item.ToEmbed();

		if (item.IsUntradable != 1)
		{
			// TODO: Add default data centre here
			(MarketAPI.History? hq, MarketAPI.History? nm) = await MarketAPI.GetBestPriceHistory("Materia", itemId);

			if (hq != null | nm != null)
			{
				StringBuilder builder = new ();
				if (hq != null)
					builder.Append(hq.ToStringEx());

				if (nm != null)
					builder.Append(nm.ToStringEx());

				embed.AddField("Best Market Board Prices", builder.ToString());
			}
		}

		return embed.Build();
	}
}