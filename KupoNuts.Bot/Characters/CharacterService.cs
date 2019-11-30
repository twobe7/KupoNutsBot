// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Services;
	using KupoNuts.Utils;

	#pragma warning disable
	public class CharacterService : ServiceBase
	{
		public override async Task Initialize()
		{
			await base.Initialize();
		}

		public async Task<bool> WhoIs(CommandMessage message, uint characterId)
		{
			// Sepcial case to jsut laod Kupo Nuts' portrait from disk.
			if (characterId == 24960538)
			{
				await message.Channel.SendMessageAsync("Thats me!");
				await message.Channel.SendFileAsync(PathUtils.Current + "/Assets/self.png");
				return true;
			}

			XIVAPI.CharacterAPI.GetResponse response = await XIVAPI.CharacterAPI.Get(characterId, XIVAPI.CharacterAPI.CharacterData.FreeCompany);

			if (response.Character == null)
				throw new UserException("I couldn't find that character.");

			FFXIVCollect.CharacterAPI.Character? collectChar = await FFXIVCollect.CharacterAPI.Get(characterId);

			string file = await PortraitDrawer.Draw(response.Character, response.FreeCompany, collectChar);
			await message.Channel.SendFileAsync(file);
			return true;
		}

		[Command("IAm", Permissions.Everyone, "Records your character for use with the 'WhoIs' and 'WhoAmI' commands")]
		public async Task<Embed> IAm(CommandMessage message, string characterName)
		{
			return await this.IAm(message, characterName, null);
		}

		[Command("IAm", Permissions.Everyone, "Records your character for use with the 'WhoIs' and 'WhoAmI' commands")]
		public async Task<Embed> IAm(CommandMessage message, string characterName, string? serverName)
		{
			XIVAPI.CharacterAPI.SearchResponse response = await XIVAPI.CharacterAPI.Search(characterName, serverName);

			if (response.Pagination == null)
				throw new Exception("No Pagination");

			if (response.Results == null)
			{
				throw new Exception("No Results");
			}
			else if (response.Results.Count == 1)
			{
				uint id = response.Results[0].ID;
				return await this.IAm(message, id);
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

		[Command("IAm", Permissions.Everyone, "Records your character for use with the 'WhoIs' and 'WhoAmI' commands")]
		public async Task<Embed> IAm(CommandMessage message, uint characterId)
		{
			UserService.User userEntry = await this.GetuserEntry(message.Author, true);
			userEntry.FFXIVCharacterId = characterId;
			await UserService.SaveUser(userEntry);

			EmbedBuilder embed = new EmbedBuilder();
			embed.Description = "Character userEntryed!";
			return embed.Build();
		}

		[Command("WhoAmI", Permissions.Everyone, "displays your userEntryed character")]
		public async Task<bool> WhoAmI(CommandMessage message)
		{
			IGuildUser user = message.Author;
			IGuild guild = message.Guild;

			UserService.User userEntry = await this.GetuserEntry(user, false);
			return await this.WhoIs(message, userEntry.FFXIVCharacterId);
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a userEntryed character")]
		public async Task<bool> WhoIs(CommandMessage message, IGuildUser user)
		{
			// Special case to handle ?WhoIs @KupoNuts to resolve her own character.
			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				return await this.WhoIs(message, 24960538);
			}

			UserService.User userEntry = await this.GetuserEntry(user, false);
			return await this.WhoIs(message, userEntry.FFXIVCharacterId);
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by character name")]
		public async Task<bool> WhoIs(CommandMessage message, string characterName)
		{
			return await this.WhoIs(message, characterName, null);
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by character name")]
		public async Task<bool> WhoIs(CommandMessage message, string characterName, string? serverName)
		{
			XIVAPI.CharacterAPI.SearchResponse response = await XIVAPI.CharacterAPI.Search(characterName, serverName);

			if (response.Pagination == null)
				throw new Exception("No Pagination");

			if (response.Results == null)
			{
				throw new Exception("No Results");
			}
			else if (response.Results.Count == 1)
			{
				uint id = response.Results[0].ID;
				return await this.WhoIs(message, id);
			}
			else
			{
				Embed embed = this.GetTooManyResultsEmbed(response);
				await message.Channel.SendMessageAsync(null, false, embed);
				return true;
			}
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Gear(CommandMessage message, string characterName)
		{
			return await this.Gear(message, characterName, null);
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Gear(CommandMessage message, IGuildUser user)
		{
			UserService.User userEntry = await this.GetuserEntry(user, false);
			return await this.Gear(message, userEntry.FFXIVCharacterId);
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Gear(CommandMessage message)
		{
			UserService.User userEntry = await this.GetuserEntry(message.Author, false);
			return await this.Gear(message, userEntry.FFXIVCharacterId);
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Gear(CommandMessage message, string characterName, string? serverName)
		{
			XIVAPI.CharacterAPI.SearchResponse response = await XIVAPI.CharacterAPI.Search(characterName, serverName);

			if (response.Pagination == null)
				throw new Exception("No Pagination");

			if (response.Results == null)
			{
				throw new Exception("No Results");
			}
			else if (response.Results.Count == 1)
			{
				uint id = response.Results[0].ID;
				return await this.Gear(message, id);
			}
			else
			{
				return this.GetTooManyResultsEmbed(response);
			}
		}

		public async Task<Embed> Gear(CommandMessage message, uint characterId)
		{
			XIVAPI.CharacterAPI.GetResponse response = await XIVAPI.CharacterAPI.Get(characterId, XIVAPI.CharacterAPI.CharacterData.FreeCompany);

			if (response.Character == null)
				throw new UserException("I couldn't find that character.");

			return response.Character.GetGear();
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Stats(CommandMessage message, string characterName)
		{
			return await this.Stats(message, characterName, null);
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Stats(CommandMessage message, IGuildUser user)
		{
			UserService.User userEntry = await this.GetuserEntry(user, false);
			return await this.Stats(message, userEntry.FFXIVCharacterId);
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Stats(CommandMessage message)
		{
			UserService.User userEntry = await this.GetuserEntry(message.Author, false);
			return await this.Stats(message, userEntry.FFXIVCharacterId);
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Stats(CommandMessage message, string characterName, string? serverName)
		{
			XIVAPI.CharacterAPI.SearchResponse response = await XIVAPI.CharacterAPI.Search(characterName, serverName);

			if (response.Pagination == null)
				throw new Exception("No Pagination");

			if (response.Results == null)
			{
				throw new Exception("No Results");
			}
			else if (response.Results.Count == 1)
			{
				uint id = response.Results[0].ID;
				return await this.Stats(message, id);
			}
			else
			{
				return this.GetTooManyResultsEmbed(response);
			}
		}

		public async Task<Embed> Stats(CommandMessage message, uint characterId)
		{
			XIVAPI.CharacterAPI.GetResponse response = await XIVAPI.CharacterAPI.Get(characterId, XIVAPI.CharacterAPI.CharacterData.FreeCompany);

			if (response.Character == null)
				throw new UserException("I couldn't find that character.");

			return response.Character.GetAttributtes();
		}

		private Embed GetTooManyResultsEmbed(XIVAPI.CharacterAPI.SearchResponse response)
		{
			if (response.Pagination == null)
				throw new Exception("No Pagination");

			if (response.Results == null)
				throw new Exception("No Results");

			EmbedBuilder embed = new EmbedBuilder();

			StringBuilder description = new StringBuilder();
			for (int i = 0; i < Math.Min(response.Results.Count, 10); i++)
			{
				description.AppendLine(response.Results[i].ID + " - " + response.Results[i].Name);
			}

			embed.Title = response.Results.Count + " results found";
			embed.Description = description.ToString();
			return embed.Build();
		}

		private async Task<UserService.User> GetuserEntry(IGuildUser user, bool create)
		{
			UserService.User userEntry = await UserService.GetUser(user);

			if (userEntry.FFXIVCharacterId == 0 && !create)
				throw new UserException("No character userEntryed! Use `IAm` to link your character.");

			return userEntry;
		}
	}
}
