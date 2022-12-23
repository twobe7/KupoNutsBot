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
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using XIVAPI;

	public class ActionService : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient;
		public ActionService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		[Command("ASearch", Permissions.Everyone, "Gets information on an action", CommandCategory.XIVData, "ActionSearch")]
		[Command("ActionSearch", Permissions.Everyone, "Gets information on an action", CommandCategory.XIVData)]
		public async Task<Embed> GetAction(ulong itemId)
		{
			XIVAPI.Action action = await ActionAPI.Get(itemId);

			EmbedBuilder embed = action.ToEmbed();

			return embed.Build();
		}

		[Command("ASearch", Permissions.Everyone, "Gets information on an action", CommandCategory.XIVData, "ActionSearch")]
		[Command("ActionSearch", Permissions.Everyone, "Gets information on an action", CommandCategory.XIVData)]
		public async Task<Embed> GetAction(string search)
		{
			List<SearchAPI.Result> results = await SearchAPI.Search(search, "Action");

			if (results.Count <= 0)
				throw new UserException("I couldn't find any items that match that search.");

			if (results.Count > 1)
			{
				EmbedBuilder embed = new EmbedBuilder();

				StringBuilder description = new StringBuilder();
				for (int i = 0; i < Math.Min(results.Count, 10); i++)
				{
					description.AppendLine(results[i].ID + " - " + results[i].Name);
				}

				embed.Title = results.Count + " results found";
				embed.Description = description.ToString();
				return embed.Build();
			}

			ulong? id = results[0].ID;

			if (id == null)
				throw new Exception("No Id in item");

			return await this.GetAction(id.Value);
		}
	}
}
