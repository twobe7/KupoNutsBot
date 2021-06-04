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
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FFXIVCollect;

	public class MountService : ServiceBase
	{
		[Command("MSearch", Permissions.Everyone, "Gets information on a mount", CommandCategory.XIVData, "MountSearch")]
		[Command("MountSearch", Permissions.Everyone, "Gets information on a mount", CommandCategory.XIVData)]
		public async Task<Embed> GetMount(string search)
		{
			List<SearchAPI.Result> results = await SearchAPI.Search(SearchAPI.SearchType.Mounts, search);

			if (results.Count <= 0)
				throw new UserException("I couldn't find any mounts that match that search.");

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
				throw new Exception("No Id in mount");

			return await this.GetMount(id.Value);
		}

		[Command("MSearch", Permissions.Everyone, "Gets information on a mount", CommandCategory.XIVData, "MountSearch")]
		[Command("MountSearch", Permissions.Everyone, "Gets information on a mount", CommandCategory.XIVData)]
		public async Task<Embed> GetMount(ulong itemId)
		{
			MountAPI.Mount mount = await MountAPI.Get(itemId);

			EmbedBuilder embed = mount.ToEmbed();

			return embed.Build();
		}
	}
}
