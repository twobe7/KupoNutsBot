// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using XIVAPI;

	public class CharacterService : ServiceBase
	{
		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by Lodestone Id")]
		public async Task WhoIs(CommandMessage message, uint characterId)
		{
			CharacterAPI.GetResponse response = await CharacterAPI.Get(characterId);

			if (response.Character == null)
				throw new UserException("Sorry! That character was blank.");

			Embed embed = response.Character.BuildEmbed();
			await message.Channel.SendMessageAsync(null, false, embed);
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by character name")]
		public async Task WhoIs(CommandMessage message, string characterName)
		{
			await this.WhoIs(message, characterName, null);
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by character name")]
		public async Task WhoIs(CommandMessage message, string characterName, string? serverName)
		{
			CharacterAPI.SearchResponse response = await CharacterAPI.Search(characterName, serverName);

			if (response.Pagination == null)
				throw new Exception("No Pagination");

			if (response.Pagination.ResultsTotal != 1)
			{
				if (response.Pagination.ResultsTotal >= 1000)
					throw new UserException("There are way too many characters that match that name!");

				throw new UserException("There are " + response.Pagination.ResultsTotal + " characters that match that name!");
			}

			if (response.Results == null || response.Results.Count != 1)
				throw new Exception("No Results");

			CharacterAPI.GetResponse getResponse = await CharacterAPI.Get(response.Results[0].ID);

			if (getResponse.Character == null)
				throw new UserException("Sorry! That character was blank.");

			Embed embed = getResponse.Character.BuildEmbed();
			await message.Channel.SendMessageAsync(null, false, embed);
		}
	}
}
