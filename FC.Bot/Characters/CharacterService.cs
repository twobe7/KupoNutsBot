// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Characters
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
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

		[Command("IAm", Permissions.Everyone, "Records your character for use with the 'WhoIs' and 'WhoAmI' commands", CommandCategory.Character)]
		public async Task<Embed> IAm(CommandMessage message, uint characterId)
		{
			User user = await UserService.GetUser(message.Author);
			CharacterInfo character = await this.GetCharacterInfo(characterId);

			return await this.RecordCharacter(user, character);
		}

		[Command("IAm", Permissions.Everyone, "Records your character for use with the 'WhoIs' and 'WhoAmI' commands", CommandCategory.Character, requiresQuotes: false)]
		public async Task<Embed> IAm(CommandMessage message, string serverName, string characterFirstName, string characterLastName)
		{
			User user = await UserService.GetUser(message.Author);
			CharacterInfo character = await this.GetCharacterInfo(this.GetCharacterFullName(characterFirstName, characterLastName), serverName);

			return await this.RecordCharacter(user, character);
		}

		[Command("IAmNot", Permissions.Everyone, "Removes your linked lodestone character", CommandCategory.Character)]
		public async Task<string> IAmNot(CommandMessage message, uint characterId)
		{
			User user = await UserService.GetUser(message.Author);
			user.RemoveCharacter(characterId);
			await UserService.SaveUser(user);
			return "Character unlinked!";
		}

		[Command("IAmNot", Permissions.Everyone, "Removes your linked lodestone character", CommandCategory.Character, requiresQuotes: false)]
		public async Task<string> IAmNot(CommandMessage message, string characterFirstName, string characterLastName)
		{
			User user = await UserService.GetUser(message.Author);
			user.RemoveCharacter(this.GetCharacterFullName(characterFirstName, characterLastName));
			await UserService.SaveUser(user);
			return "Character unlinked!";
		}

		[Command("IAmNot", Permissions.Everyone, "Removes your linked lodestone character", CommandCategory.Character, requiresQuotes: false)]
		public async Task<string> IAmNot(CommandMessage message, string serverName, string characterFirstName, string characterLastName)
		{
			User user = await UserService.GetUser(message.Author);
			user.RemoveCharacter(this.GetCharacterFullName(characterFirstName, characterLastName), serverName);
			await UserService.SaveUser(user);
			return "Character unlinked!";
		}

		[Command("IAmUsually", Permissions.Everyone, "Sets the linked lodestone character as your default", CommandCategory.Character, requiresQuotes: false)]
		public async Task<string> IAmUsually(CommandMessage message, string characterFirstName, string characterLastName)
		{
			User user = await UserService.GetUser(message.Author);
			user.SetDefaultCharacter(this.GetCharacterFullName(characterFirstName, characterLastName));
			await UserService.SaveUser(user);
			return "Default character updated!";
		}

		[Command("IAmUsually", Permissions.Everyone, "Sets the linked lodestone character as your default", CommandCategory.Character, requiresQuotes: false)]
		public async Task<string> IAmUsually(CommandMessage message, string serverName, string characterFirstName, string characterLastName)
		{
			User user = await UserService.GetUser(message.Author);
			user.SetDefaultCharacter(this.GetCharacterFullName(characterFirstName, characterLastName), serverName);
			await UserService.SaveUser(user);
			return "Default character updated!";
		}

		[Command("WhoAmI", Permissions.Everyone, "Displays your linked character", CommandCategory.Character)]
		public async Task WhoAmI(CommandMessage message)
		{
			User user = await UserService.GetUser(message.Author);
			await this.PostWhoIsResponse(message, user);
		}

		[Command("WhoAmI", Permissions.Everyone, "Displays your linked character", CommandCategory.Character)]
		public async Task WhoAmI(CommandMessage message, int characterIndex)
		{
			User user = await UserService.GetUser(message.Author);
			await this.PostWhoIsResponse(message, user, characterIndex);
		}

		[Command("WhoIs", Permissions.Everyone, "Looks up a linked character", CommandCategory.Character, requiresQuotes: false)]
		public async Task WhoIs(CommandMessage message, IGuildUser user)
		{
			User userEntry = await UserService.GetUser(user);
			await this.PostWhoIsResponse(message, userEntry);
		}

		[Command("WhoIs", Permissions.Everyone, "Looks up a linked character", CommandCategory.Character, requiresQuotes: false)]
		public async Task WhoIs(CommandMessage message, IGuildUser user, int characterIndex)
		{
			User userEntry = await UserService.GetUser(user);
			await this.PostWhoIsResponse(message, userEntry, characterIndex);
		}

		[Command("WhoIs", Permissions.Everyone, "Looks up a character profile by character and server name", CommandCategory.Character, requiresQuotes: false)]
		public async Task WhoIs(CommandMessage message, string serverName, string characterFirstName, string characterLastName)
		{
			CharacterInfo character = await this.GetCharacterInfo(characterFirstName + ' ' + characterLastName, serverName);
			string file = await CharacterCard.Draw(character);
			await message.Channel.SendFileAsync(file);
		}

		public async Task PostWhoIsResponse(CommandMessage message, User user, int? characterIndex = null)
		{
			User.Character? defaultCharacter = user.GetDefaultCharacter();
			if (defaultCharacter is null)
				throw new UserException("No characters linked! Use `IAm` to link a character");

			if (characterIndex != null)
			{
				try
				{
					defaultCharacter = user.Characters[characterIndex.Value - 1];
				}
				catch
				{
					throw new UserException("I couldn't find a character at index: " + characterIndex);
				}
			}

			// Default character
			CharacterInfo defaultCharacterInfo = await this.GetCharacterInfo(defaultCharacter.FFXIVCharacterId);
			string file = await CharacterCard.Draw(defaultCharacterInfo);
			await message.Channel.SendFileAsync(file);

			if (!defaultCharacter.IsVerified(defaultCharacterInfo))
			{
				EmbedBuilder builder = new EmbedBuilder
				{
					Description = "This character has not been verified.",
					Color = Discord.Color.Gold,
				};

				// If this is the requesting users character, give instructions on how to verify
				if (message.Author.Id == user.DiscordUserId)
				{
					builder.Title = "This character has not been verified";
					builder.Description = "To verify this character, enter the following verification code in your [lodestone profile](https://na.finalfantasyxiv.com/lodestone/my/setting/profile/): " + defaultCharacter.FFXIVCharacterVerification;
				}

				await message.Channel.SendMessageAsync(null, false, builder.Build());
			}

			if (!defaultCharacterInfo.HasMinions && !defaultCharacterInfo.HasMounts && !defaultCharacterInfo.HasAchievements)
			{
				EmbedBuilder builder = new EmbedBuilder();
				builder.Description = "To show Minions, Mounts, and Achievements, please link your character at [FFXIV Collect](https://ffxivcollect.com/)";
				await message.Channel.SendMessageAsync(null, false, builder.Build());
			}

			// While building the AKA, we can confirm if a name/server change has occured
			// for queried character and update DB
			bool hasChanges = false;

			// AKA
			StringBuilder akaDescBuilder = new StringBuilder();
			int index = 0;
			foreach (User.Character character in user.Characters)
			{
				// For the queried character, check if the name/server has changed and update
				if (character.FFXIVCharacterId == defaultCharacterInfo.Id
					&& ((!string.IsNullOrWhiteSpace(defaultCharacterInfo.Name) && character.CharacterName != defaultCharacterInfo.Name)
						|| (!string.IsNullOrWhiteSpace(defaultCharacterInfo.Server) && character.ServerName != defaultCharacterInfo.Server)))
				{
					hasChanges = true;
					character.CharacterName = defaultCharacterInfo.Name;
					character.ServerName = defaultCharacterInfo.Server;
				}

				index++;

				akaDescBuilder.Append(index);
				akaDescBuilder.Append(") ");
				akaDescBuilder.Append(character.CharacterName);
				akaDescBuilder.Append(" (");
				akaDescBuilder.Append(Emotes.Home.GetString());
				akaDescBuilder.Append(character.ServerName);
				akaDescBuilder.Append(")");

				if (!await character.IsVerified(user))
					akaDescBuilder.Append(" *(Not Verified)*");

				akaDescBuilder.AppendLine();
			}

			// Character change detected, save user information
			if (hasChanges)
				_ = UserService.SaveUser(user);

			if (index > 1)
			{
				EmbedBuilder builder = new EmbedBuilder
				{
					Description = akaDescBuilder.ToString(),
					Title = "Also known as:",
				};
				await message.Channel.SendMessageAsync(null, false, builder.Build());
			}
		}

		[Command("Portrait", Permissions.Everyone, "Shows your linked character portrait", CommandCategory.Character)]
		public async Task Portrait(CommandMessage message)
		{
			CharacterInfo character = await this.GetCharacterInfo(message.Author);
			string file = await CharacterPortrait.Draw(character);

			await message.Channel.SendFileAsync(file);
		}

		[Command("Portrait", Permissions.Everyone, "Shows another user's linked character portrait", CommandCategory.Character)]
		public async Task Portrait(CommandMessage message, IGuildUser user)
		{
			CharacterInfo character = await this.GetCharacterInfo(user);
			string file = await CharacterPortrait.Draw(character);

			await message.Channel.SendFileAsync(file);
		}

		[Command("Portrait", Permissions.Everyone, "Looks up a character profile by character name and server name", CommandCategory.Character)]
		public async Task Portrait(CommandMessage message, string serverName, string characterFirstName, string characterLastName)
		{
			CharacterInfo character = await this.GetCharacterInfo(this.GetCharacterFullName(characterFirstName, characterLastName), serverName);
			string file = await CharacterPortrait.Draw(character);
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character", CommandCategory.Character)]
		public async Task<Embed> Gear(CommandMessage message, IGuildUser user)
		{
			CharacterInfo info = await this.GetCharacterInfo(user);
			return info.GetGearEmbed();
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character", CommandCategory.Character)]
		public async Task<Embed> Gear(CommandMessage message)
		{
			CharacterInfo info = await this.GetCharacterInfo(message.Author);
			return info.GetGearEmbed();
		}

		[Command("Gear", Permissions.Everyone, "Shows the current gear and stats of a character", CommandCategory.Character)]
		public async Task<Embed> Gear(CommandMessage message, string serverName, string characterFirstName, string characterLastName)
		{
			CharacterInfo info = await this.GetCharacterInfo(this.GetCharacterFullName(characterFirstName, characterLastName), serverName);
			return info.GetGearEmbed();
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of a character", CommandCategory.Character)]
		public async Task<Embed> Stats(IGuildUser user)
		{
			CharacterInfo info = await this.GetCharacterInfo(user);
			return info.GetAttributesEmbed();
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of your linked character", CommandCategory.Character)]
		public async Task<Embed> Stats(CommandMessage message)
		{
			CharacterInfo info = await this.GetCharacterInfo(message.Author);
			return info.GetAttributesEmbed();
		}

		[Command("Stats", Permissions.Everyone, "Shows the current gear and stats of a character", CommandCategory.Character)]
		public async Task<Embed> Stats(string serverName, string characterFirstName, string characterLastName)
		{
			CharacterInfo info = await this.GetCharacterInfo(this.GetCharacterFullName(characterFirstName, characterLastName), serverName);
			return info.GetAttributesEmbed();
		}

		[Command("EL", Permissions.Everyone, "Shows current Elemental Level of a character", CommandCategory.Character, commandParent: "ElementalLevel")]
		[Command("ElementalLevel", Permissions.Everyone, "Shows current Elemental Level of a character", CommandCategory.Character)]
		public async Task<Embed> ElementalLevel(CommandMessage message)
		{
			return await this.GetElementalLevel(message);
		}

		[Command("EL", Permissions.Everyone, "Shows current Elemental Level of a character", CommandCategory.Character, commandParent: "ElementalLevel")]
		[Command("ElementalLevel", Permissions.Everyone, "Shows current Elemental Level of a character", CommandCategory.Character)]
		public async Task<Embed> ElementalLevel(CommandMessage message, int characterIndex)
		{
			return await this.GetElementalLevel(message, characterIndex);
		}

		[Command("RR", Permissions.Everyone, "Shows current Resistance Rank of a character", commandParent: "ResistanceRank")]
		[Command("ResistanceRank", Permissions.Everyone, "Shows current Resistance Rank of a character", CommandCategory.Character)]
		public async Task<Embed> ResistanceRank(CommandMessage message)
		{
			return await this.GetResistanceRank(message);
		}

		[Command("RR", Permissions.Everyone, "Shows current Resistance Rank of a character", commandParent: "ResistanceRank")]
		[Command("ResistanceRank", Permissions.Everyone, "Shows current Resistance Rank of a character", CommandCategory.Character)]
		public async Task<Embed> ResistanceRank(CommandMessage message, int characterIndex)
		{
			return await this.GetResistanceRank(message, characterIndex);
		}

		[Command("Census", Permissions.Administrators, "Perform Census")]
		public async void GetCharacterCensus(CommandMessage message, ulong freeCompanyId)
		{
			// I am the only one who can run this command
			if (message.Author.Id != 294055671396302858)
				throw new UnauthorizedAccessException();

			Embed embed = await this.GetFreeCompanyCensus(freeCompanyId);
			await message.Channel.SendMessageAsync(embed: embed);
		}

		private async Task<Embed> GetElementalLevel(CommandMessage message, int? characterIndex = null)
		{
			User user = await UserService.GetUser(message.Author);
			User.Character? defaultCharacter = user.GetDefaultCharacter();
			if (defaultCharacter is null)
				throw new UserException("No characters linked! Use `IAm` to link a character");

			int index = 0;
			if (characterIndex != null)
			{
				defaultCharacter = null;
				foreach (User.Character character in user.Characters)
				{
					index++;

					if (index == characterIndex)
					{
						defaultCharacter = character;
					}
				}

				if (defaultCharacter is null)
				{
					throw new UserException("I couldn't find a character at index: " + characterIndex);
				}
			}

			CharacterInfo info = await this.GetCharacterInfo(defaultCharacter.FFXIVCharacterId);
			return await info.GetElementalLevelEmbed();
		}

		private async Task<Embed> GetResistanceRank(CommandMessage message, int? characterIndex = null)
		{
			User user = await UserService.GetUser(message.Author);
			User.Character? defaultCharacter = user.GetDefaultCharacter();
			if (defaultCharacter is null)
				throw new UserException("No characters linked! Use `IAm` to link a character");

			int index = 0;
			if (characterIndex != null)
			{
				defaultCharacter = null;
				foreach (User.Character character in user.Characters)
				{
					index++;

					if (index == characterIndex)
					{
						defaultCharacter = character;
					}
				}

				if (defaultCharacter is null)
				{
					throw new UserException("I couldn't find a character at index: " + characterIndex);
				}
			}

			CharacterInfo info = await this.GetCharacterInfo(defaultCharacter.FFXIVCharacterId);
			return await info.GetResistanceRankEmbed();
		}

		private async Task<Embed> RecordCharacter(User user, CharacterInfo character)
		{
			EmbedBuilder embed = new EmbedBuilder();

			User.Character? userCharacter = user.GetCharacter(character.Id);

			if (userCharacter == null)
			{
				userCharacter = new User.Character
				{
					FFXIVCharacterId = character.Id,
					CharacterName = character.Name,
					ServerName = character.Server,
					IsVerified = false,
				};
				user.Characters.Add(userCharacter);
				await UserService.SaveUser(user);
			}

			if (userCharacter.IsVerified)
			{
				userCharacter.CharacterName = character.Name;
				userCharacter.ServerName = character.Server;
				await UserService.SaveUser(user);

				embed.Description = "Character linked!";
				return embed.Build();
			}
			else
			{
				if (userCharacter.FFXIVCharacterVerification == null)
				{
					userCharacter.FFXIVCharacterVerification = Guid.NewGuid().ToString();
					await UserService.SaveUser(user);
				}

				if (character.Bio?.Contains(userCharacter.FFXIVCharacterVerification) == true)
				{
					userCharacter.FFXIVCharacterId = character.Id;
					userCharacter.CharacterName = character.Name;
					userCharacter.ServerName = character.Server;
					userCharacter.FFXIVCharacterVerification = null;
					userCharacter.IsVerified = true;
					await UserService.SaveUser(user);

					embed.Description = "Character linked! (You can now remove the Verification Id from your Character Profile)";
					return embed.Build();
				}
				else
				{
					embed.Description = "To verify character ownership, please place the following verification Id in your [lodestone profile](https://na.finalfantasyxiv.com/lodestone/my/setting/profile/): `" + userCharacter.FFXIVCharacterVerification + "`";
					return embed.Build();
				}
			}
		}

		private async Task<CharacterInfo> GetCharacterInfo(IGuildUser guildUser)
		{
			User user = await UserService.GetUser(guildUser);
			User.Character? character = user.GetDefaultCharacter();

			if (character is null)
				throw new UserException("No characters linked.");

			return await this.GetCharacterInfo(character.FFXIVCharacterId);
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

		private string GetCharacterFullName(string first, string last)
		{
			// I didn't want to write this for every function
			return first + " " + last;
		}

		private async Task<Embed> GetFreeCompanyCensus(ulong freeCompanyId)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.Title = "Free Company Census";

			FreeCompanyAPI.GetResponse response = await FreeCompanyAPI.GetFreeCompany(freeCompanyId);

			if (response.FreeCompanyMembers == null)
				return embed.WithDescription("No members found").Build();

			// Census variables
			List<CensusData> data = new List<CensusData>();
			DateTime startTime = new DateTime(1970, 1, 1);
			DateTime activeThreshold = DateTime.Now.Date.AddMonths(-3);

			// Loop members
			foreach (FreeCompanyAPI.Member member in response.FreeCompanyMembers)
			{
				FreeCompanyAPI.GetResponse responseCharacter = await FreeCompanyAPI.GetCharacter(member.ID, FreeCompanyAPI.CharacterData.None, "Character.Gender,Character.Race,Character.Tribe,Character.ParseDate");

				if (responseCharacter.Character == null)
					continue;

				FreeCompanyAPI.Character character = responseCharacter.Character;

				CensusData entry = new CensusData();

				// Race
				entry.Race = ((CharacterInfo.Races)character.Race).ToDisplayString();

				// Tribe
				entry.Tribe = ((CharacterInfo.Tribes)character.Tribe).ToDisplayString();

				// Male = 1, Female = 2
				entry.Gender = character.Gender;

				// Parse last seen date
				DateTime parsedDate = startTime.AddSeconds(Convert.ToDouble(character.ParseDate));

				// Member is active if seen in last 3 months
				if (parsedDate > activeThreshold)
					entry.Active = true;

				// Insert delay to try space out API request
				////await Task.Delay(250);
			}

			// Total Members
			embed.AddField("Total Members", data.Count, true);

			// Race ranking
			StringBuilder raceRanking = new StringBuilder();
			foreach (IGrouping<string, CensusData> raceGroup in data.GroupBy(x => x.Race))
			{
				// Tribes
				IEnumerable<IGrouping<string, CensusData>> x = raceGroup.GroupBy(x => x.Tribe);
				(string, int) tribeA = (x.FirstOrDefault().Key, x.FirstOrDefault().Count());
				(string, int) tribeB = (x.LastOrDefault().Key, x.LastOrDefault().Count());

				raceRanking.Append($"{raceGroup.Key} - {raceGroup.Count()} ");
				raceRanking.AppendLine($"{tribeA.Item1}: {tribeA.Item2}, {tribeB.Item1}: {tribeB.Item2})");
			}

			embed.AddField("Race", raceRanking);

			// Gender ranking
			StringBuilder genderRanking = new StringBuilder();
			foreach (IGrouping<uint, CensusData> genderGroup in data.GroupBy(x => x.Gender))
			{
				string gender = genderGroup.Key == 1 ? "Male" : "Female";
				genderRanking.AppendLine($"{gender} - {genderGroup.Count()}");
			}

			embed.AddField("Gender", genderRanking);

			// Only active
			List<CensusData> activeData = data.Where(x => x.Active).ToList();

			// Active Members
			embed.AddField("Active Members", activeData.Count, true);

			// Race ranking
			StringBuilder activeRaceRanking = new StringBuilder();
			foreach (IGrouping<string, CensusData> raceGroup in activeData.GroupBy(x => x.Race))
			{
				// Tribes
				IEnumerable<IGrouping<string, CensusData>> x = raceGroup.GroupBy(x => x.Tribe);
				(string, int) tribeA = (x.FirstOrDefault().Key, x.FirstOrDefault().Count());
				(string, int) tribeB = (x.LastOrDefault().Key, x.LastOrDefault().Count());

				activeRaceRanking.Append($"{raceGroup.Key} - {raceGroup.Count()} ");
				activeRaceRanking.AppendLine($"{tribeA.Item1}: {tribeA.Item2}, {tribeB.Item1}: {tribeB.Item2})");
			}

			embed.AddField("Active Race", activeRaceRanking);

			// Gender ranking
			StringBuilder activeGenderRanking = new StringBuilder();
			foreach (IGrouping<uint, CensusData> genderGroup in activeData.GroupBy(x => x.Gender))
			{
				string gender = genderGroup.Key == 1 ? "Male" : "Female";
				activeGenderRanking.AppendLine($"{gender} - {genderGroup.Count()}");
			}

			embed.AddField("Active Gender", activeGenderRanking);

			return embed.Build();
		}

		private class CensusData
		{
			public string Race { get; set; } = string.Empty;
			public string Tribe { get; set; } = string.Empty;
			public uint Gender { get; set; }
			public bool Active { get; set; } = false;
		}

		////private enum Races
		////{
		////	None = 0,
		////	Hyur = 1,
		////	Elezen = 2,
		////	Lalafell = 3,
		////	Miqo'te = 4,
		////	Roegadyn = 5,
		////	Au Ra = 6,
		////	Hrothgar = 7,
		////	Viera = 8,
		////}
	}
}
