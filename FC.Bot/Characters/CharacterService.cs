// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

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

			UserService.User.Character userCharacter = await UserService.GetUserCharacter(message.Author, character.Id);

			if (userCharacter.Verified)
			{
				if (string.IsNullOrEmpty(userCharacter.CharacterName))
				{
					userCharacter.CharacterName = character.Name;
					userCharacter.ServerName = character.Server;
					await UserService.SaveUserCharacter(message.Author, userCharacter);

					return "Character Name/Server updated!";
				}

				return "Character already linked!";
			}
			else if (userCharacter.FFXIVCharacterVerification == null)
			{
				userCharacter.FFXIVCharacterVerification = Guid.NewGuid().ToString();
				await UserService.SaveUserCharacter(message.Author, userCharacter);

				return "To confirm that you're " + character.Name + ", please update the Lodestone Character Profile with the following Verification Id: `" + userCharacter.FFXIVCharacterVerification + "`";
			}
			else
			{
				if (character.Bio?.Contains(userCharacter.FFXIVCharacterVerification) == true)
				{
					userCharacter.FFXIVCharacterId = character.Id;
					userCharacter.CharacterName = character.Name;
					userCharacter.ServerName = character.Server;
					userCharacter.FFXIVCharacterVerification = null;
					userCharacter.Verified = true;
					await UserService.SaveUserCharacter(message.Author, userCharacter);

					return "Character linked! (You can now remove the Verification Id from your Character Profile)";
				}
				else
				{
					return "Character not linked. Unable to find Verification Id within Character Profile: `" + userCharacter.FFXIVCharacterVerification + "`";
				}
			}
		}

		[Command("IAmNot", Permissions.Everyone, "Removes your linked lodestone character")]
		public async Task<string> IAmNot(CommandMessage message, string characterName)
		{
			await UserService.RemoveUserCharacter(message.Author, characterName);
			return "Character unlinked!";
		}

		[Command("IAmNot", Permissions.Everyone, "Removes your linked lodestone character")]
		public async Task<string> IAmNot(CommandMessage message, string characterName, string serverName)
		{
			await UserService.RemoveUserCharacter(message.Author, characterName, serverName);
			return "Character unlinked!";
		}

		[Command("IAmUsually", Permissions.Everyone, "Sets the linked lodestone character as your default")]
		public async Task<string> IAmUsually(CommandMessage message, string characterName)
		{
			await UserService.SetDefaultUserCharacter(message.Author, characterName);
			return "Default character updated!";
		}

		[Command("IAmUsually", Permissions.Everyone, "Sets the linked lodestone character as your default")]
		public async Task<string> IAmUsually(CommandMessage message, string characterName, string serverName)
		{
			await UserService.SetDefaultUserCharacter(message.Author, characterName, serverName);
			return "Default character updated!";
		}

		[Command("WhoAmI", Permissions.Everyone, "Displays your linked character")]
		public async Task WhoAmI(CommandMessage message)
		{
			bool defaultCharacterShown = false;

			List<UserService.User.Character> userCharacters = await UserService.GetAllUserCharacters(message.Author);

			if (userCharacters.Count == 0)
			{
				throw new UserException("No character linked! Use `IAm` to link your character.");
			}

			UserService.User.Character? defaultCharacter = userCharacters.Find(x => x.IsDefaultCharacter);
			if (defaultCharacter != null)
			{
				await this.WhoIs(message, defaultCharacter.FFXIVCharacterId);
				defaultCharacterShown = true;

				userCharacters.Remove(defaultCharacter);
			}

			if (!defaultCharacterShown && userCharacters.Count == 1)
			{
				await this.WhoIs(message, userCharacters[0].FFXIVCharacterId);
			}
			else
			{
				userCharacters.Sort((x, y) =>
				{
					return x.FFXIVCharacterId.CompareTo(y.FFXIVCharacterId);
				});

				IGuildUser guildUser = message.Author;

				int increment = 1;

				StringBuilder characterList = new StringBuilder();
				foreach (UserService.User.Character character in userCharacters)
				{
					characterList.Append(increment++);
					characterList.Append(" - ");
					characterList.AppendLine(!string.IsNullOrEmpty(character.CharacterName)
												? string.Format("{0} ({1})", character.CharacterName, character.ServerName)
												: "??? (Name not recorded, perform the `IAm` command to fix this)");
				}

				EmbedBuilder builder = new EmbedBuilder();
				builder.Author = new EmbedAuthorBuilder();
				builder.Author.Name = defaultCharacterShown ? "Also Known As:" : "Linked Characters:";
				builder.Description = characterList.ToString();

				await message.Channel.SendMessageAsync(embed: builder.Build());
			}
		}

		[Command("WhoAmI", Permissions.Everyone, "Displays your linked character")]
		public async Task WhoAmI(CommandMessage message, int characterToReturn)
		{
			List<UserService.User.Character> userCharacters = await UserService.GetAllUserCharacters(message.Author);

			if (userCharacters.Count == 0)
			{
				throw new UserException("No character linked! Use `IAm` to link your character.");
			}

			UserService.User.Character? defaultCharacter = userCharacters.Find(x => x.IsDefaultCharacter);
			if (defaultCharacter != null)
			{
				userCharacters.Remove(defaultCharacter);
			}

			userCharacters.Sort((x, y) =>
			{
				return x.FFXIVCharacterId.CompareTo(y.FFXIVCharacterId);
			});

			await this.WhoIs(message, userCharacters[characterToReturn - 1].FFXIVCharacterId);
		}

		[Command("WhoIs", Permissions.Everyone, "Looks up a linked character")]
		public async Task WhoIs(CommandMessage message, IGuildUser user)
		{
			// Special case to handle ?WhoIs @FC to resolve her own character.
			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				await this.WhoIs(message, 24960538);
			}
			else
			{
				UserService.User.Character characterEntry = await this.GetUserCharacterEntry(user);
				await this.WhoIs(message, characterEntry.FFXIVCharacterId);
			}
		}

		[Command("WhoIs", Permissions.Everyone, "looks up a character profile by character and server name")]
		public async Task WhoIs(CommandMessage message, string characterName, string serverName)
		{
			CharacterInfo character = await this.GetCharacterInfo(characterName, serverName);
			string file = await CharacterCard.Draw(character);
			await message.Channel.SendFileAsync(file);
			await this.PostCollectLink(message, character);
		}

		public async Task<bool> WhoIs(CommandMessage message, uint characterId)
		{
			CharacterInfo character = await this.GetCharacterInfo(characterId);
			string file = await CharacterCard.Draw(character);
			await message.Channel.SendFileAsync(file);
			await this.PostCollectLink(message, character);
			return true;
		}

		[Command("ClearCustomPortrait", Permissions.Everyone, "clears the custom portrait image for your linked character")]
		public async Task<string> ClearCustomPortrait(CommandMessage message)
		{
			UserService.User.Character characterEntry = await this.GetUserCharacterEntry(message.Author);

			string path = "CustomPortraits/" + characterEntry.FFXIVCharacterId + ".png";

			if (!File.Exists(path))
				throw new UserException("No custom portrait set. Use \"" + CommandsService.GetPrefix(message.Guild) + "CustomPortrait\" as the comment on an uploaded image to set a custom portrait.");

			File.Delete(path);
			return "Custom portrait cleared.";
		}

		[Command("CustomPortrait", Permissions.Everyone, "Sets a custom portrait image for your linked character (for best results: 375x512 " + @"png)")]
		public async Task<string> SetCustomPortrait(CommandMessage message, Attachment file)
		{
			UserService.User.Character characterEntry = await this.GetUserCharacterEntry(message.Author);

			string temp = "Temp/" + file.Filename;
			string path = "CustomPortraits/" + characterEntry.FFXIVCharacterId + ".png";

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
			UserService.User.Character character = await this.GetUserCharacterEntry(guildUser);
			return await this.GetCharacterInfo(character.FFXIVCharacterId);
		}

		[Obsolete("Not used - user doesn't hold ffxiv character id")]
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

		private async Task<UserService.User.Character> GetUserCharacterEntry(IGuildUser user)
		{
			List<UserService.User.Character> characterEntries = await UserService.GetAllUserCharacters(user);

			if (characterEntries.Count == 0)
				throw new UserException("No character linked! Use `IAm` to link your character.");

			UserService.User.Character? defaultCharacter = characterEntries.Find(x => x.IsDefaultCharacter);
			if (defaultCharacter != null)
			{
				return defaultCharacter;
			}
			else
			{
				characterEntries.Sort((x, y) =>
				{
					return x.FFXIVCharacterId.CompareTo(y.FFXIVCharacterId);
				});

				return characterEntries[0];
			}
		}

		private async Task PostCollectLink(CommandMessage message, CharacterInfo info)
		{
			if (!info.HasMinions && !info.HasMounts && !info.HasAchievements)
			{
				EmbedBuilder builder = new EmbedBuilder();
				builder.Description = "To show Minions, Mounts, and Achievements, please link your character at [FFXIV Collect](https://ffxivcollect.com/)";
				await message.Channel.SendMessageAsync(null, false, builder.Build());
			}
		}
	}
}
