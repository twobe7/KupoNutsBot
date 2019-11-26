// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Amazon.DynamoDBv2.DataModel;
	using Amazon.DynamoDBv2.DocumentModel;
	using Discord;

	public class UserService : ServiceBase
	{
		private static UserService? instance;

		private Dictionary<ulong, Dictionary<ulong, string>> userIdLookup = new Dictionary<ulong, Dictionary<ulong, string>>();
		private Database<User> userDb = new Database<User>("Users", 0);

		public static async Task<User> GetUser(IGuildUser user)
		{
			ulong guildId = user.GuildId;
			ulong userId = user.Id;

			return await GetUser(guildId, userId);
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

		public override async Task Initialize()
		{
			instance = this;
			await base.Initialize();
			await this.userDb.Connect();
		}

		private async Task<User> GetUserImp(ulong guildId, ulong userId)
		{
			if (!this.userIdLookup.ContainsKey(guildId))
				this.userIdLookup.Add(guildId, new Dictionary<ulong, string>());

			User? userEntry = null;

			if (!this.userIdLookup[guildId].ContainsKey(userId))
			{
				List<User> users = await this.userDb.LoadAll(
					new ScanCondition("DiscordUserId", ScanOperator.Equal, userId),
					new ScanCondition("DiscordGuildId", ScanOperator.Equal, guildId));

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
			this.userIdLookup[guildId].Add(userId, userEntry.Id);
			return userEntry;
		}

		#pragma warning disable SA1516
		[Serializable]
		public class User : EntryBase
		{
			public ulong DiscordUserId { get; set; }
			public ulong DiscordGuildId { get; set; }
			public uint FFXIVCharacterId { get; set; } = 0;
			public int Level { get; set; } = 0;
			public int Nuts { get; set; } = 10;
		}
	}
}
