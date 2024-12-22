// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Characters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Services;
	using FC.Utils;
	using FC.XIVData;
	using NetStone.Model.Parseables.Search.Character;

	[Group("character", "FFXIV Character commands")]
	public class CharacterService(DiscordSocketClient discordClient) : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient = discordClient;

		public override async Task Initialize()
		{
			await base.Initialize();
		}

		[SlashCommand("iam", "Records your character for use with the 'WhoIs' and 'WhoAmI' commands")]
		[RequireNameOrId]
		public async Task IAm(
			[Autocomplete(typeof(EnumAutoCompleteHandler<XivWorld>))]
			[Summary("serverName", "Name of Character Server")]
			string? serverName = null,
			[Summary("characterName", "Name of Character")]
			string? characterName = null,
			[Summary("characterId", "Lodestone Id of Character")]
			uint? characterId = null)
		{
			await this.DeferAsync();

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			CharacterInfo? character = await this.GetCharacter(serverName, characterName, characterId);

			User user = await UserService.GetUser(guildUser);
			await this.FollowupAsync(embeds: [await this.RecordCharacter(user, character)]);
		}

		[SlashCommand("who-am-i", "Displays your linked character")]
		public async Task WhoAmI([Summary("characterIndex", "Index of character set via `iam` command")] int? characterIndex = null)
		{
			await this.DeferAsync();

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			User user = await UserService.GetUser(guildUser);
			await this.PostWhoIsInteractionResponse(this.Context, user, characterIndex);
		}

		[SlashCommand("i-am-not", "Removes your linked lodestone character")]
		public async Task IAmNot([Summary("characterIndex", "Index of character set via `iam` command")] int characterIndex)
		{
			await this.DeferAsync(ephemeral: true);

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			User user = await UserService.GetUser(guildUser);

			var character = user.Characters.ElementAtOrDefault(characterIndex - 1);

			if (character == null)
			{
				await this.FollowupAsync("I couldn't find a character at index: " + characterIndex);
				return;
			}

			var components = new ComponentBuilder()
				.WithButton(label: "Cancel", customId: "iAmNot-cancel-0")
				.WithButton(label: "Confirm", customId: $"iAmNot-confirm-{characterIndex}");

			await this.FollowupAsync(
				embed: new EmbedBuilder().WithTitle("Unlink Character").WithDescription($"{character.CharacterName}").Build(),
				ephemeral: true,
				components: components.Build());
		}

		[ComponentInteraction("iAmNot-*-*", true)]
		public async Task IAmNotButtonHandler()
		{
			if (this.Context is SocketInteractionContext ctx)
			{
				if (ctx.SegmentMatches.FirstOrDefault()?.Value == "confirm")
				{
					if (int.TryParse(ctx.SegmentMatches.ElementAtOrDefault(1)?.Value, out int characterIndex))
					{
						if (ctx.User is not IGuildUser guildUser)
							throw new UserException("Unable to process user.");

						User user = await UserService.GetUser(guildUser);

						// Remove user and save
						user.RemoveCharacter(characterIndex);
						await UserService.SaveUser(user);

						// Send update to Discord
						await ctx.Interaction.ModifyOriginalResponseAsync(x =>
						{
							x.Content = "Character unlinked!";
							x.Embed = null;
							x.Components = null;
						});

						return;
					}
				}

				await ctx.Interaction.ModifyOriginalResponseAsync(x =>
				{
					x.Content = "Character not unlinked!";
					x.Embed = null;
					x.Components = null;
				});
			}
		}

		[SlashCommand("i-am-usually", "Sets the linked lodestone character as your default")]
		public async Task IAmUsually([Summary("characterIndex", "Index of character set via `iam` command")] int characterIndex)
		{
			await this.DeferAsync(ephemeral: true);

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			User user = await UserService.GetUser(guildUser);
			user.SetDefaultCharacter(characterIndex);
			await UserService.SaveUser(user);

			await this.FollowupAsync("Default character updated!", ephemeral: true);
		}

		[SlashCommand("who-is", "Looks up a linked character")]
		public async Task WhoIs(
			[Summary("user", "Name of user to search, leave empty for your character")]
			SocketGuildUser user,
			[Summary("characterIndex", "Index of character set via `iam` command")]
			int? characterIndex = null)
		{
			await this.DeferAsync();

			User userEntry = await UserService.GetUser(user);
			await this.PostWhoIsInteractionResponse(this.Context, userEntry, characterIndex);
		}

		[SlashCommand("who-is-other", "Looks up a character profile by character and server name")]
		[RequireNameOrId]
		public async Task WhoIs(
			[Autocomplete(typeof(EnumAutoCompleteHandler<XivWorld>))]
			[Summary("serverName", "Name of Character Server")]
			string? serverName = null,
			[Summary("characterName", "Name of Character")]
			string? characterName = null,
			[Summary("characterId", "Lodestone Id of Character")]
			uint? characterId = null)
		{
			await this.DeferAsync();

			CharacterInfo? character = await this.GetCharacter(serverName, characterName, characterId);

			string file = await CharacterCard.Draw(character);
			await this.FollowupWithFileAsync(file);
		}

		[SlashCommand("portrait", "Shows your linked character portrait")]
		public async Task Portrait(
			[Summary("user", "Name of user to search, leave empty for your character")]
			SocketGuildUser? user = null,
			[Summary("characterIndex", "Index of character set via `iam` command")]
			int? characterIndex = null)
		{
			await this.DeferAsync();

			if (this.Context.User is not SocketGuildUser guildUser)
				throw new UserException("Unable to process user.");

			user ??= guildUser;

			CharacterInfo character = await this.GetCharacterInfo(user, characterIndex);
			string file = await CharacterPortrait.Draw(character);

			await this.FollowupWithFileAsync(file);
		}

		[SlashCommand("portrait-other", "Looks up a character profile by character name and server name")]
		[RequireNameOrId]
		public async Task Portrait(
			[Autocomplete(typeof(EnumAutoCompleteHandler<XivWorld>))]
			[Summary("serverName", "Name of Character Server")]
			string? serverName = null,
			[Summary("characterName", "Name of Character")]
			string? characterName = null,
			[Summary("characterId", "Lodestone Id of Character")]
			uint? characterId = null)
		{
			await this.DeferAsync();

			CharacterInfo? character = await this.GetCharacter(serverName, characterName, characterId);

			string file = await CharacterPortrait.Draw(character);
			await this.FollowupWithFileAsync(file);
		}

		[SlashCommand("gear", "Shows the current gear and stats of a character")]
		public async Task Gear(
			[Summary("user", "Name of user to search, leave empty for your character")]
			SocketGuildUser? user = null,
			[Summary("characterIndex", "Index of character set via `iam` command")]
			int? characterIndex = null)
		{
			await this.DeferAsync();

			if (this.Context.User is not SocketGuildUser guildUser)
				throw new UserException("Unable to process user.");

			user ??= guildUser;

			CharacterInfo info = await this.GetCharacterInfo(user, characterIndex);

			await this.FollowupAsync(embeds: [info.GetGearEmbed()]);
		}

		[SlashCommand("gear-other", "Shows the current gear and stats of a character")]
		[RequireNameOrId]
		public async Task Gear(
			[Autocomplete(typeof(EnumAutoCompleteHandler<XivWorld>))]
			[Summary("serverName", "Name of Character Server")]
			string? serverName = null,
			[Summary("characterName", "Name of Character")]
			string? characterName = null,
			[Summary("characterId", "Lodestone Id of Character")]
			uint? characterId = null)
		{
			await this.DeferAsync();

			CharacterInfo? character = await this.GetCharacter(serverName, characterName, characterId);

			await this.FollowupAsync(embeds: [character.GetGearEmbed()]);
		}

		[SlashCommand("stats", "Shows the current gear and stats of a character")]
		public async Task Stats(
			[Summary("user", "Name of user to search, leave empty for your character")]
			SocketGuildUser? user = null,
			[Summary("characterIndex", "Index of character set via `iam` command")]
			int? characterIndex = null)
		{
			await this.DeferAsync();

			if (this.Context.User is not SocketGuildUser guildUser)
				throw new UserException("Unable to process user.");

			user ??= guildUser;

			CharacterInfo info = await this.GetCharacterInfo(user, characterIndex);

			await this.FollowupAsync(embeds: [info.GetAttributesEmbed()]);
		}

		[SlashCommand("stats-other", "Shows the current gear and stats of a character")]
		[RequireNameOrId]
		public async Task Stats(
			[Autocomplete(typeof(EnumAutoCompleteHandler<XivWorld>))]
			[Summary("serverName", "Name of Character Server")]
			string? serverName = null,
			[Summary("characterName", "Name of Character")]
			string? characterName = null,
			[Summary("characterId", "Lodestone Id of Character")]
			uint? characterId = null)
		{
			await this.DeferAsync();

			CharacterInfo? character = await this.GetCharacter(serverName, characterName, characterId);

			await this.FollowupAsync(embeds: [character.GetAttributesEmbed()]);
		}

		[SlashCommand("elemental-level", "Shows current Elemental Level of a character")]
		public async Task ElementalLevel([Summary("characterIndex", "Index of character set via `iam` command")] int? characterIndex = null)
		{
			await this.DeferAsync();

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			CharacterInfo info = await this.GetCharacterInfo(guildUser, characterIndex);
			await this.FollowupAsync(embeds: [await info.GetElementalLevelEmbed()]);
		}

		[SlashCommand("resistance-rank", "Shows current Resistance Rank of a character")]
		public async Task ResistanceRank([Summary("characterIndex", "Index of character set via `iam` command")] int? characterIndex = null)
		{
			await this.DeferAsync();

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			CharacterInfo info = await this.GetCharacterInfo(guildUser, characterIndex);
			await this.FollowupAsync(embeds: [await info.GetResistanceRankEmbed()]);
		}

		private async Task<CharacterInfo> GetCharacter(
			string? serverName = null,
			string? characterName = null,
			uint? characterId = null,
			bool updateCollect = false)
		{
			CharacterInfo? character = null;
			if (characterId != null)
			{
				character = await this.UpdateAndGetCharacterInfo(characterId.Value, updateCollect);
			}
			else if (characterName != null && serverName != null)
			{
				character = await this.GetCharacterInfo(characterName, serverName, updateCollect);
			}

			if (character == null)
				throw new UserException("Unable to get character");

			return character;
		}

		private async Task PostWhoIsInteractionResponse(IInteractionContext ctx, User user, int? characterIndex = null)
		{
			// Special case to just load Kupo Nuts' portrait from disk.
			if (user.DiscordUserId == this.DiscordClient.CurrentUser.Id)
			{
				await this.FollowupWithFileAsync($"{PathUtils.Current}/Assets/self.png", text: "Thats me!");
				return;
			}

			User.Character? character = user.GetDefaultCharacter();

			if (character == null)
			{
				await this.FollowupAsync("No characters linked! Use `IAm` to link a character");
				return;
			}

			if (characterIndex != null)
			{
				character = user.Characters.ElementAtOrDefault(characterIndex.Value - 1)
					?? throw new UserException("I couldn't find a character at index: " + characterIndex);
			}
			else
			{
				// Set Character Index to 1 for default selection
				characterIndex = 1;
			}

			// Hold all embeds to update single response message
			List<Embed> responseEmbeds = [];

			// Default character
			CharacterInfo defaultCharacterInfo = await this.UpdateAndGetCharacterInfo(character.FFXIVCharacterId, true);
			await this.FollowupWithFileAsync(await CharacterCard.Draw(defaultCharacterInfo));

			// Get current Verification status
			bool oldIsVerified = character.IsVerified;

			if (!character.CheckVerification(defaultCharacterInfo))
			{
				EmbedBuilder builder = new()
				{
					Description = "This character has not been verified.",
					Color = Color.Gold,
				};

				// If this is the requesting users character, give instructions on how to verify
				if (ctx.User.Id == user.DiscordUserId)
				{
					builder.Title = "This character has not been verified";
					builder.Description =
						"To verify this character, enter the following verification code " +
						"in your [lodestone profile](https://na.finalfantasyxiv.com/lodestone/my/setting/profile/): " +
						$"{character.FFXIVCharacterVerification}";
				}

				responseEmbeds.Add(builder.Build());
				await this.ModifyOriginalResponseAsync(x => x.Embeds = responseEmbeds.ToArray());
			}

			if (!defaultCharacterInfo.HasCollect)
			{
				var builder = new EmbedBuilder()
					.WithDescription("To show Minions, Mounts, and Achievements, please link your character at [FFXIV Collect](https://ffxivcollect.com/)");

				responseEmbeds.Add(builder.Build());
				await this.ModifyOriginalResponseAsync(x => x.Embeds = responseEmbeds.ToArray());
			}

			// While building the AKA, we can confirm if a name/server change has occured
			// for queried character and update DB
			// Initial value used to see if verification has been updated
			bool hasChanges = oldIsVerified != character.IsVerified;

			// AKA
			StringBuilder akaDescBuilder = new();
			int index = 0;
			foreach (User.Character altCharacter in user.Characters)
			{
				// For the queried character, check if the name/server has changed and update
				if (altCharacter.FFXIVCharacterId == defaultCharacterInfo.Id
					&& ((!string.IsNullOrWhiteSpace(defaultCharacterInfo.Name) && altCharacter.CharacterName != defaultCharacterInfo.Name)
						|| (!string.IsNullOrWhiteSpace(defaultCharacterInfo.Server) && altCharacter.ServerName != defaultCharacterInfo.Server)))
				{
					hasChanges = true;
					altCharacter.CharacterName = defaultCharacterInfo.Name;
					altCharacter.ServerName = defaultCharacterInfo.Server;
				}

				index++;

				// Add bold text if selected character
				var indexAndCharacterString = characterIndex == index
					? $"**{index}) {altCharacter.CharacterName}**"
					: $"{index}) {altCharacter.CharacterName}";

				akaDescBuilder.Append($"{indexAndCharacterString} ({Emotes.Home.GetString()} {altCharacter.ServerName})");

				if (!altCharacter.IsVerified)
					akaDescBuilder.Append(" *(Not Verified)*");

				akaDescBuilder.AppendLine();
			}

			// Character change detected, save user information
			if (hasChanges)
				_ = UserService.SaveUser(user);

			if (index > 1)
			{
				EmbedBuilder builder = new()
				{
					Description = akaDescBuilder.ToString(),
					Title = "Also known as:",
				};
				responseEmbeds.Add(builder.Build());
				await this.ModifyOriginalResponseAsync(x => x.Embeds = responseEmbeds.ToArray());
			}
		}

		private async Task<CharacterInfo> GetCharacterInfo(IGuildUser guildUser, int? characterIndex, bool updateCollect = false)
		{
			User user = await UserService.GetUser(guildUser);
			User.Character? character = user.GetDefaultCharacter()
				?? throw new UserException("No characters linked! Use `IAm` to link a character");

			if (characterIndex != null)
			{
				character = user.Characters.ElementAtOrDefault(characterIndex.Value - 1);

				if (character is null)
					throw new UserException("I couldn't find a character at index: " + characterIndex);
			}

			return await this.UpdateAndGetCharacterInfo(character.FFXIVCharacterId, updateCollect);
		}

		private async Task<Embed> RecordCharacter(User user, CharacterInfo character)
		{
			EmbedBuilder embed = new();

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
					embed.Description =
						"To verify this character, enter the following verification code " +
						"in your [lodestone profile](https://na.finalfantasyxiv.com/lodestone/my/setting/profile/): " +
						$"{userCharacter.FFXIVCharacterVerification}";
					return embed.Build();
				}
			}
		}

		private EmbedBuilder GetMultipleCharacterResponseEmbedBuilder(IEnumerable<CharacterSearchEntry> results)
		{
			EmbedBuilder builder = new EmbedBuilder()
					.WithColor(Color.Red)
					.WithTitle($"Multiple Characters Found");

			StringBuilder stringBuilder = new StringBuilder()
				.AppendLine("```")
				.AppendLine($"| **Id**{Utils.Characters.Tab}|{Utils.Characters.DoubleSpace}**Character Name**")
				.AppendLine("| --------- | ------------------");

			foreach (var character in results)
			{
				stringBuilder.Append($"| {character.Id}");

				var rightPad = 10 - character.Id?.ToString().Length;
				for (var i = 0; i < rightPad; i++)
					stringBuilder.Append(Utils.Characters.Space);

				stringBuilder.AppendLine($"|{Utils.Characters.DoubleSpace}{character.Name}");
			}

			stringBuilder.AppendLine("```");

			builder.Description = stringBuilder.ToString();

			return builder;
		}

		private async Task<CharacterInfo> GetCharacterInfo(string characterName, string serverName, bool updateCollect = false)
		{
			if (!string.IsNullOrEmpty(serverName))
			{
				var txtInfo = new System.Globalization.CultureInfo("en-au", false).TextInfo;
				serverName = txtInfo.ToTitleCase(serverName.ToLower());
			}

			NetStone.LodestoneClient client = await NetStone.LodestoneClient.GetClientAsync();
			var response = await client.SearchCharacter(new NetStone.Search.Character.CharacterSearchQuery
			{
				CharacterName = characterName,
				World = serverName,
			});

			////CharacterAPI.SearchResponse response = await CharacterAPI.Search(characterName, serverName);

			////if (response.Pagination == null)
			////	throw new Exception("No Pagination");

			if (response?.Results == null)
			{
				throw new Exception("No Results");
			}
			else if (!response.Results.Any())
			{
				throw new UserException("I couldn't find a character with that name.");
			}
			else if (response.Results.Count() != 1)
			{
				EmbedBuilder builder = this.GetMultipleCharacterResponseEmbedBuilder(response.Results);

				await this.FollowupAsync(embeds: [builder.Build()]);

				throw new UserException($"Found {response.Results.Count()} characters with that name.");
			}
			else
			{
				if (!uint.TryParse(response.Results.First().Id, out uint charId))
				{
					throw new UserException($"I couldn't find a character with that name.");
				}

				return await this.UpdateAndGetCharacterInfo(charId, updateCollect);
			}
		}

		private async Task<CharacterInfo> UpdateAndGetCharacterInfo(uint id, bool updateCollect = false)
		{
			CharacterInfo info = new(id);
			await info.Update(updateCollect);
			return info;
		}
	}
}
