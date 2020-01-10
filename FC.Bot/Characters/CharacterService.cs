// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot.Characters
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Bot.Utils;
	using FC.Utils;
	using SixLabors.ImageSharp;
	using SixLabors.ImageSharp.PixelFormats;
	using SixLabors.ImageSharp.Processing;
	using XIVAPI;

	using FFXIVCollectCharacter = FFXIVCollect.CharacterAPI.Character;
	using Image = SixLabors.ImageSharp.Image;
	using XIVAPICharacter = XIVAPI.Character;

	public class CharacterService : ServiceBase
	{
		public override async Task Initialize()
		{
			await base.Initialize();
		}

		[Command("Lodestone", Permissions.Everyone, "links to your lodestone page")]
		public async Task<string> Lodestone(CommandMessage message)
		{
			UserService.User userEntry = await this.GetUserEntry(message.Author);
			return "https://eu.finalfantasyxiv.com/lodestone/character/" + userEntry.FFXIVCharacterId + "/";
		}

		[Command("Lodestone", Permissions.Everyone, "links to a users lodestone page")]
		public async Task<string> Lodestone(IGuildUser user)
		{
			UserService.User userEntry = await this.GetUserEntry(user);
			return "https://eu.finalfantasyxiv.com/lodestone/character/" + userEntry.FFXIVCharacterId + "/";
		}

		[Command("Lodestone", Permissions.Everyone, "links to a characters lodestone page")]
		public async Task<string> Lodestone(CommandMessage message, string characterName, string serverName)
		{
			CharacterInfo character = await this.GetCharacterInfo(characterName, serverName);
			return "https://eu.finalfantasyxiv.com/lodestone/character/" + character.Id + "/";
		}

		[Command("IAm", Permissions.Everyone, "Records your character for use with the 'WhoIs' and 'WhoAmI' commands")]
		public async Task<string> IAm(CommandMessage message, string characterName, string serverName)
		{
			CharacterInfo character = await this.GetCharacterInfo(characterName, serverName);

			UserService.User userEntry = await UserService.GetUser(message.Author);
			bool hadCharacter = userEntry.FFXIVCharacterId != 0;
			userEntry.FFXIVCharacterId = character.Id;
			await UserService.SaveUser(userEntry);

			return hadCharacter ? "Character updated!" : "Character linked!";
		}

		[Command("WhoAmI", Permissions.Everyone, "displays your linked character")]
		public async Task WhoAmI(CommandMessage message)
		{
			UserService.User userEntry = await this.GetUserEntry(message.Author);
			await this.WhoIs(message, userEntry.FFXIVCharacterId);
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a linked character")]
		public async Task WhoIs(CommandMessage message, IGuildUser user)
		{
			// Special case to handle ?WhoIs @FC to resolve her own character.
			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				await this.WhoIs(message, 24960538);
			}
			else
			{
				UserService.User userEntry = await this.GetUserEntry(user);
				await this.WhoIs(message, userEntry.FFXIVCharacterId);
			}
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by character and server name")]
		public async Task WhoIs(CommandMessage message, string characterName, string serverName)
		{
			CharacterInfo character = await this.GetCharacterInfo(characterName, serverName);
			string file = await CharacterCard.Draw(character);
			await message.Channel.SendFileAsync(file);
		}

		public async Task<bool> WhoIs(CommandMessage message, uint characterId)
		{
			// Special case to just load Kupo Nuts' portrait from disk.
			if (characterId == 24960538)
			{
				await message.Channel.SendMessageAsync("Thats me!");
				await message.Channel.SendFileAsync(PathUtils.Current + "/Assets/self.png");
				return true;
			}

			CharacterInfo character = await this.GetCharacterInfo(characterId);
			string file = await CharacterCard.Draw(character);
			await message.Channel.SendFileAsync(file);
			return true;
		}

		[Command("ClearCustomPortrait", Permissions.Everyone, "clears the custom portrait image for your linked character")]
		public async Task<string> SetCustomPortrait(CommandMessage message)
		{
			UserService.User userEntry = await this.GetUserEntry(message.Author);

			string path = "CustomPortraits/" + userEntry.FFXIVCharacterId + ".png";

			if (!File.Exists(path))
				throw new UserException("No custom portrait set. Use \"" + CommandsService.CommandPrefix + "CustomPortrait\" as the comment on an uploaded image to set a custom portrait.");

			File.Delete(path);
			return "Custom portrait cleared.";
		}

		[Command("CustomPortrait", Permissions.Everyone, "Sets a custom portrait image for your linked character (for best results: 375x512 " + @"png)")]
		public async Task<string> SetCustomPortrait(CommandMessage message, Attachment file)
		{
			UserService.User userEntry = await this.GetUserEntry(message.Author);

			string temp = "Temp/" + file.Filename;
			string path = "CustomPortraits/" + userEntry.FFXIVCharacterId + ".png";

			if (!Directory.Exists("CustomPortraits/"))
				Directory.CreateDirectory("CustomPortraits/");

			await FileDownloader.Download(file.Url, temp);

			Image<Rgba32> charImg = Image.Load<Rgba32>(temp);
			charImg.Mutate(x => x.Resize(375, 512));
			charImg.Save(path);

			File.Delete(temp);

			return "Portrait updated.";
		}

		[Command("Portrait", Permissions.Everyone, "Shows your linked character portrait")]
		public async Task Portrait(CommandMessage message)
		{
			CharacterInfo character = await this.GetCharacterInfo(message.Author);
			string file = await CharacterPortrait.Draw(character);

			await message.Channel.SendFileAsync(file);
		}

		[Command("Portrait", Permissions.Everyone, "Shows another user's linked character portrait")]
		public async Task Portrait(CommandMessage message, IGuildUser user)
		{
			CharacterInfo character = await this.GetCharacterInfo(user);
			string file = await CharacterPortrait.Draw(character);

			await message.Channel.SendFileAsync(file);
		}

		[Command("Portrait", Permissions.Everyone, "Looks up a character profile by character name and server name")]
		public async Task Portrait(CommandMessage message, string characterName, string serverName)
		{
			CharacterInfo character = await this.GetCharacterInfo(characterName, serverName);
			string file = await CharacterPortrait.Draw(character);
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Gear(CommandMessage message, IGuildUser user)
		{
			CharacterInfo info = await this.GetCharacterInfo(user);
			return info.GetGearEmbed();
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Gear(CommandMessage message)
		{
			CharacterInfo info = await this.GetCharacterInfo(message.Author);
			return info.GetGearEmbed();
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Gear(CommandMessage message, string characterName, string serverName)
		{
			CharacterInfo info = await this.GetCharacterInfo(characterName, serverName);
			return info.GetGearEmbed();
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Stats(IGuildUser user)
		{
			CharacterInfo info = await this.GetCharacterInfo(user);
			return info.GetAttributesEmbed();
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of your linked character")]
		public async Task<Embed> Stats(CommandMessage message)
		{
			CharacterInfo info = await this.GetCharacterInfo(message.Author);
			return info.GetAttributesEmbed();
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of a character")]
		public async Task<Embed> Stats(string characterName, string serverName)
		{
			CharacterInfo info = await this.GetCharacterInfo(characterName, serverName);
			return info.GetAttributesEmbed();
		}

		private async Task<CharacterInfo> GetCharacterInfo(IGuildUser guildUser)
		{
			UserService.User user = await this.GetUserEntry(guildUser);
			return await this.GetCharacterInfo(user.FFXIVCharacterId);
		}

		private async Task<CharacterInfo> GetCharacterInfo(UserService.User user)
		{
			return await this.GetCharacterInfo(user.FFXIVCharacterId);
		}

		private async Task<CharacterInfo> GetCharacterInfo(string characterName, string serverName)
		{
			XIVAPI.CharacterAPI.SearchResponse response = await XIVAPI.CharacterAPI.Search(characterName, serverName);

			if (response.Pagination == null)
				throw new Exception("No Pagination");

			if (response.Results == null)
			{
				throw new Exception("No Results");
			}
			else if (response.Results.Count == 0)
			{
				throw new UserException("I couldn't find a character with that name.");
			}
			else if (response.Results.Count != 1)
			{
				throw new UserException("I found " + response.Results.Count + " characters with that name.");
			}
			else
			{
				return await this.GetCharacterInfo(response.Results[0].ID);
			}
		}

		private async Task<CharacterInfo> GetCharacterInfo(uint id)
		{
			CharacterInfo info = new CharacterInfo(id);
			await info.Update();
			return info;
		}

		private async Task<UserService.User> GetUserEntry(IGuildUser user)
		{
			UserService.User userEntry = await UserService.GetUser(user);

			if (userEntry.FFXIVCharacterId == 0)
				throw new UserException("No character linked! Use `IAm` to link your character.");

			return userEntry;
		}
	}
}
