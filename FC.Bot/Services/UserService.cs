// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Amazon.DynamoDBv2.DataModel;
	using Amazon.DynamoDBv2.DocumentModel;
	using Discord;
	using Discord.WebSocket;
	using FC.Data;

	public class UserService : ServiceBase
	{
		private static UserService? instance;

		private Dictionary<ulong, Dictionary<ulong, string>> userIdLookup = new Dictionary<ulong, Dictionary<ulong, string>>();
		private Table<User> userDb = new Table<User>("KupoNuts_Users", 0);

		private Dictionary<ulong, Dictionary<ulong, Dictionary<uint, string>>> userCharacterIdLookup = new Dictionary<ulong, Dictionary<ulong, Dictionary<uint, string>>>();

		public static async Task<User> GetUser(IGuildUser user)
		{
			return await GetUser(user.GuildId, user.Id);
		}

		public static async Task<User> GetUser(ulong guildId, ulong userId)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			return await instance.GetUserImp(guildId, userId);
		}

		public static async Task SaveUser(User user)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			await instance.userDb.Save(user);
		}

		public static async Task<List<User>> GetAllUsers()
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			return await instance.userDb.LoadAll();
		}

		public static async Task<User.Character> GetUserCharacter(IGuildUser user, string characterName, string serverName)
		{
			return await GetUserCharacter(user.GuildId, user.Id, characterName, serverName);
		}

		public static async Task<User.Character> GetUserCharacter(ulong guildId, ulong userId, string characterName, string serverName)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			User user = await instance.GetUserWithCharacter(guildId, userId);

			User.Character? character = user.Characters.Find(x => x.CharacterName?.ToLower() == characterName.ToLower() && x.ServerName?.ToLower() == serverName.ToLower());
			if (character == null)
			{
				throw new UserException("No character with that name and server found");
			}

			return character;
		}

		public static async Task<User.Character> GetUserCharacter(IGuildUser user, uint ffxivCharacterId)
		{
			return await GetUserCharacter(user.GuildId, user.Id, ffxivCharacterId);
		}

		public static async Task<User.Character> GetUserCharacter(ulong guildId, ulong userId, uint ffxivCharacterId)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			User user = await instance.GetUserWithCharacter(guildId, userId);

			User.Character? character = user.Characters.Find(x => x.FFXIVCharacterId == ffxivCharacterId);
			if (character == null)
			{
				character = new User.Character();
				character.FFXIVCharacterId = ffxivCharacterId;

				user.Characters.Add(character);
				await SaveUser(user);
			}

			return character;
		}

		public static async Task<List<User.Character>> GetAllUserCharacters(IGuildUser user)
		{
			return await GetAllUserCharacters(user.GuildId, user.Id);
		}

		public static async Task<List<User.Character>> GetAllUserCharacters(ulong guildId, ulong userId)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			User user = await instance.GetUserWithCharacter(guildId, userId);

			return user.Characters;
		}

		public static async Task SaveUserCharacter(IGuildUser user, User.Character userCharacter)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			User userEntry = await instance.GetUserWithCharacter(user.GuildId, user.Id);

			// First character is going to be default
			if (userEntry.Characters.Count == 0)
			{
				userCharacter.IsDefaultCharacter = true;
			}
			else
			{
				User.Character? character = userEntry.Characters.Find(x => x.FFXIVCharacterId == userCharacter.FFXIVCharacterId);
				if (character != null)
				{
					userEntry.Characters.Remove(character);
				}
			}

			userEntry.Characters.Add(userCharacter);

			await SaveUser(userEntry);
		}

		public static async Task RemoveUserCharacter(IGuildUser user, string characterName, string? serverName = null)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			User userEntry = await instance.GetUserWithCharacter(user.GuildId, user.Id);

			List<User.Character> characters = userEntry.Characters.FindAll(x => x.CharacterName?.ToLower() == characterName.ToLower());

			if (characters.Count == 1)
			{
				userEntry.Characters.Remove(characters[0]);
				await SaveUser(userEntry);
			}
			else if (characters.Count > 1)
			{
				if (!string.IsNullOrEmpty(serverName))
				{
					User.Character? character = characters.Find(x => x.ServerName?.ToLower() == serverName.ToLower());
					if (character != null)
					{
						userEntry.Characters.Remove(characters[0]);
						await SaveUser(userEntry);
						return;
					}
				}
				else
				{
					throw new UserException("Multiple characters with same name found. Please specify a server.");
				}
			}

			throw new UserException("No linked character found with that name.");
		}

		public static async Task SetDefaultUserCharacter(IGuildUser user, string characterName, string? serverName = null)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			User userEntry = await instance.GetUserWithCharacter(user.GuildId, user.Id);

			List<User.Character> characters = userEntry.Characters.FindAll(x => x.CharacterName?.ToLower() == characterName);
			if (characters.Count == 1)
			{
				User.Character? defaultCharacter = userEntry.Characters.Find(x => x.IsDefaultCharacter);
				if (defaultCharacter != null)
				{
					defaultCharacter.IsDefaultCharacter = false;
				}

				characters[0].IsDefaultCharacter = true;
				await SaveUser(userEntry);
				return;
			}
			else if (characters.Count > 1)
			{
				if (!string.IsNullOrEmpty(serverName))
				{
					User.Character? character = characters.Find(x => x.ServerName?.ToLower() == serverName.ToLower());
					if (character != null)
					{
						User.Character? defaultCharacter = userEntry.Characters.Find(x => x.IsDefaultCharacter);
						if (defaultCharacter != null)
						{
							defaultCharacter.IsDefaultCharacter = false;
						}

						character.IsDefaultCharacter = true;
						await SaveUser(userEntry);
						return;
					}
				}
				else
				{
					throw new UserException("Multiple characters with same name found. Please specify a server.");
				}
			}

			throw new UserException("No linked character found with that name.");
		}

		public override async Task Initialize()
		{
			instance = this;
			await base.Initialize();
			await this.userDb.Connect();

			Program.DiscordClient.UserJoined += this.DiscordClient_UserJoined;
		}

		private async Task DiscordClient_UserJoined(SocketGuildUser arg)
		{
			User user = await this.GetUserImp(arg.Guild.Id, arg.Id);
			if (user.Banned)
			{
				await arg.Guild.AddBanAsync(arg);
			}
		}

		private async Task<User> GetUserImp(ulong guildId, ulong userId)
		{
			if (!this.userIdLookup.ContainsKey(guildId))
				this.userIdLookup.Add(guildId, new Dictionary<ulong, string>());

			User? userEntry = null;

			if (!this.userIdLookup[guildId].ContainsKey(userId))
			{
				List<User> users = await this.userDb.LoadAll(
					new Dictionary<string, object>()
					{
						{ "DiscordUserId",  userId },
						{ "DiscordGuildId", guildId },
					});

				if (users.Count > 1)
					throw new Exception("Multiple users with same discord user and guild!");

				if (users.Count == 1)
				{
					this.userIdLookup[guildId].Add(userId, users[0].Id);
					return users[0];
				}
			}
			else
			{
				userEntry = await this.userDb.Load(this.userIdLookup[guildId][userId]);
				if (userEntry != null)
				{
					return userEntry;
				}
			}

			userEntry = await this.userDb.CreateEntry();
			userEntry.DiscordGuildId = guildId;
			userEntry.DiscordUserId = userId;
			await this.userDb.Save(userEntry);
			this.userIdLookup[guildId].Add(userId, userEntry.Id);
			return userEntry;
		}

		private async Task<User> GetUserWithCharacter(ulong guildId, ulong userId)
		{
			User user = await this.GetUserImp(guildId, userId);

			if (user.FFXIVCharacterId != 0)
			{
				if (user.Characters.Find(x => x.FFXIVCharacterId == user.FFXIVCharacterId) == null)
				{
					// Create character from user ffxiv id and add to user
					user.Characters.Add(new User.Character()
					{
						FFXIVCharacterId = user.FFXIVCharacterId,
						Verified = true,
					});
				}

				user.FFXIVCharacterId = 0;

				await SaveUser(user);
			}

			return user;
		}

#pragma warning disable SA1516
		[Serializable]
		public class User : EntryBase
		{
			public ulong DiscordUserId { get; set; }
			public ulong DiscordGuildId { get; set; }
			public uint FFXIVCharacterId { get; set; } = 0;
			public bool Banned { get; set; } = false;
			public List<Warning> Warnings { get; set; } = new List<Warning>();
			public List<Character> Characters { get; set; } = new List<Character>();

			[Serializable]
			public class Warning
			{
				public enum Actions
				{
					Unknown,
					PostRemoved,
					Warned,
				}

				public Actions Action { get; set; } = Actions.Unknown;
				public ulong ChannelId { get; set; } = 0;
				public string Comment { get; set; } = string.Empty;
			}

			[Serializable]
			public class Character
			{
				public uint FFXIVCharacterId { get; set; } = 0;
				public string? CharacterName { get; set; }
				public string? ServerName { get; set; }
				public string? FFXIVCharacterVerification { get; set; }
				public bool IsDefaultCharacter { get; set; }
				public bool Verified { get; set; }
			}
		}
	}
}
