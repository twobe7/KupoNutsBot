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

		public static async Task<List<User>> GetAllUsersForGuild(ulong guildId)
		{
			if (instance == null)
				throw new Exception("Attempt to access user service before it is initialized");

			Dictionary<string, object> filters = new Dictionary<string, object>()
			{
				{ "DiscordGuildId", guildId },
			};

			return await instance.userDb.LoadAll(filters);
		}

		public static async Task<List<IGuildUser>> GetUsersByNickName(IGuild guild, string name)
		{
			IReadOnlyCollection<IGuildUser> guildUsers = await guild.GetUsersAsync();

			List<IGuildUser> userToRep = new List<IGuildUser>();

			// Remove spaces in input
			name = name.Replace(" ", string.Empty);

			foreach (IGuildUser gUser in guildUsers)
			{
				if (!string.IsNullOrWhiteSpace(gUser.Nickname) && FC.Utils.StringUtils.ComputeLevenshtein(gUser.Nickname, name) < 3)
				{
					userToRep.Add(gUser);
				}
				else if (!string.IsNullOrWhiteSpace(gUser.Username) && FC.Utils.StringUtils.ComputeLevenshtein(gUser.Username, name) < 3)
				{
					userToRep.Add(gUser);
				}
			}

			return userToRep;
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

			User? userEntry;

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
					return userEntry;
			}

			userEntry = await this.userDb.CreateEntry();
			userEntry.DiscordGuildId = guildId;
			userEntry.DiscordUserId = userId;
			await this.userDb.Save(userEntry);
			this.userIdLookup[guildId].Add(userId, userEntry.Id);
			return userEntry;
		}
	}
}
