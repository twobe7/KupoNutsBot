// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using KupoNuts.Bot.Commands;
	using XIVAPI;

	public class ItemService : ServiceBase
	{
		[Command("ItemSearch", Permissions.Everyone, "Gets information on an item")]
		public async Task<Embed> GetItem(string search)
		{
			List<SearchAPI.Result> results = await SearchAPI.Search(search, ContentAPI.ContentType.Item);

			EmbedBuilder embed = new EmbedBuilder();
			if (results.Count > 1)
			{
				StringBuilder description = new StringBuilder();
				for (int i = 0; i < Math.Min(results.Count, 10); i++)
				{
					description.AppendLine(results[i].ID + " - " + results[i].Name);
				}

				embed.Title = results.Count + " results found";
				embed.Description = description.ToString();
			}
			else
			{
				throw new NotImplementedException();
			}

			return embed.Build();
		}
	}
}
