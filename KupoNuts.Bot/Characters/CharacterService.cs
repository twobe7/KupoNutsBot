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
	using KupoNuts.Characters;

	public class CharacterService : ServiceBase
	{
		private Database<CharacterLink> characterDb = new Database<CharacterLink>("Characters", 0);

		public override async Task Initialize()
		{
			await base.Initialize();
			await this.characterDb.Connect();
		}

		public async Task<bool> WhoIs(CommandMessage message, uint characterId)
		{
			XIVAPI.CharacterAPI.GetResponse response = await XIVAPI.CharacterAPI.Get(characterId, XIVAPI.CharacterAPI.CharacterData.FreeCompany);

			if (response.Character == null)
				throw new UserException("I couldn't find that character.");

			FFXIVCollect.CharacterAPI.Character? collectChar = await FFXIVCollect.CharacterAPI.Get(characterId);

			string file = await PortraitDrawer.Draw(response.Character, response.FreeCompany, collectChar);
			await message.Channel.SendFileAsync(file);
			return true;
		}

		[Command("IAm", Permissions.Everyone, "looks up a character profile by character name")]
		public async Task<Embed> IAm(CommandMessage message, string characterName)
		{
			return await this.IAm(message, characterName, null);
		}

		[Command("IAm", Permissions.Everyone, "Records who your character is")]
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

		[Command("IAm", Permissions.Everyone, "Records who your character is")]
		public async Task<Embed> IAm(CommandMessage message, uint characterId)
		{
			IGuildUser user = message.Author;
			IGuild guild = message.Guild;

			CharacterLink link = await this.GetLink(user, guild, true);
			link.UserId = user.Id;
			link.GuildId = guild.Id;
			link.CharacterId = characterId;
			await this.characterDb.Save(link);

			EmbedBuilder embed = new EmbedBuilder();
			embed.Description = "Character linked!";
			return embed.Build();
		}

		[Command("WhoAmI", Permissions.Everyone, "displays your character")]
		public async Task<bool> WhoAmI(CommandMessage message)
		{
			IGuildUser user = message.Author;
			IGuild guild = message.Guild;

			CharacterLink link = await this.GetLink(user, guild, false);
			return await this.WhoIs(message, link.CharacterId);
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a linked character")]
		public async Task<bool> WhoIs(CommandMessage message, IGuildUser user)
		{
			CharacterLink link = await this.GetLink(user, message.Guild, false);
			return await this.WhoIs(message, link.CharacterId);
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
				EmbedBuilder embed = new EmbedBuilder();

				StringBuilder description = new StringBuilder();
				for (int i = 0; i < Math.Min(response.Results.Count, 10); i++)
				{
					description.AppendLine(response.Results[i].ID + " - " + response.Results[i].Name);
				}

				embed.Title = response.Results.Count + " results found";
				embed.Description = description.ToString();
				Embed embedAc = embed.Build();

				await message.Channel.SendMessageAsync(null, false, embedAc);

				return true;
			}
		}

		private async Task<CharacterLink> GetLink(IGuildUser user, IGuild guild, bool create)
		{
			string id = "Guild:" + guild.Id.ToString() + "_User:" + user.Id;
			CharacterLink? link = await this.characterDb.Load(id);

			if (link is null)
			{
				if (!create)
				{
					throw new UserException("No character linked! Use `IAm` to link your character.");
				}
				else
				{
					link = await this.characterDb.CreateEntry(id);
				}
			}

			return link;
		}
	}
}
