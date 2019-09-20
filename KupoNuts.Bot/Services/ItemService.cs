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

			if (results.Count > 10)
				throw new UserException("I found too many items that match that search!");

			EmbedBuilder embed = new EmbedBuilder();
			if (results.Count > 1)
			{
				StringBuilder description = new StringBuilder();
				foreach (SearchAPI.Result result in results)
				{
					description.AppendLine(result.ID + " - " + result.Name);
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
