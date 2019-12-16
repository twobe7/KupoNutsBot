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
	using Discord.WebSocket;
	using KupoNuts.Data;

	public class UserService : ServiceBase
	{
		private static UserService? instance;

		private Dictionary<ulong, Dictionary<ulong, string>> userIdLookup = new Dictionary<ulong, Dictionary<ulong, string>>();
		private Table<User> userDb = Table<User>.Create("Users", 0);

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

		#pragma warning disable SA1516
		[Serializable]
		public class User : EntryBase
		{
			public ulong DiscordUserId { get; set; }
			public ulong DiscordGuildId { get; set; }
			public uint FFXIVCharacterId { get; set; } = 0;
			public int Level { get; set; } = 0;
			public int Nuts { get; set; } = 10;

			public bool Banned { get; set; } = false;
			public List<Warning> Warnings { get; set; } = new List<Warning>();

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
		}
	}
}
