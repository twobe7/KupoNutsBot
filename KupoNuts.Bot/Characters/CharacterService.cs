// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using XIVAPI;

	public class CharacterService : ServiceBase
	{
		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by Lodestone Id")]
		public async Task<Embed> WhoIs(uint characterId)
		{
			CharacterAPI.GetResponse response = await CharacterAPI.Get(characterId);

			if (response.Character == null)
				throw new UserException("I couldn't find that character.");

			return response.Character.BuildEmbed();
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by character name")]
		public async Task<Embed> WhoIs(string characterName)
		{
			return await this.WhoIs(characterName, null);
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by character name")]
		public async Task<Embed> WhoIs(string characterName, string? serverName)
		{
			CharacterAPI.SearchResponse response = await CharacterAPI.Search(characterName, serverName);

			if (response.Pagination == null)
				throw new Exception("No Pagination");

			if (response.Results == null)
			{
				throw new Exception("No Results");
			}
			else if (response.Results.Count == 1)
			{
				CharacterAPI.GetResponse getResponse = await CharacterAPI.Get(response.Results[0].ID);

				if (getResponse.Character == null)
					throw new UserException("I couldn't find that character.");

				return getResponse.Character.BuildEmbed();
			}
			else
			{
				EmbedBuilder embed = new EmbedBuilder();

				StringBuilder description = new StringBuilder();
				for (int i = 0; i < Math.Min(response.Results.Count, 10); i++)
				{
					description.AppendLine(response.Results[i].ID + " - " + response.Results[i].Name);
				}

				embed.Title = response.Results.Count + " results found";
				embed.Description = description.ToString();
				Embed embedAc = embed.Build();

				return embedAc;
			}
		}
	}
}
