// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Items
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using XIVAPI;

	public class ItemService : ServiceBase
	{
		[Command("ISearch", Permissions.Everyone, "Gets information on an item")]
		[Command("ItemSearch", Permissions.Everyone, "Gets information on an item")]
		public async Task<Embed> GetItem(string search)
		{
			List<SearchAPI.Result> results = await SearchAPI.Search(search, "Item");

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

			return await this.GetItem((ulong)id);
		}

		[Command("ISearch", Permissions.Everyone, "Gets information on an item")]
		[Command("ItemSearch", Permissions.Everyone, "Gets information on an item")]
		public async Task<Embed> GetItem(ulong itemID)
		{
			ItemAPI.Item item = await ItemAPI.Get(itemID);
			return item.ToEmbed();
		}
	}
}
