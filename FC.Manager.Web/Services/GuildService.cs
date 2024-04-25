// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using FC.Bot.Services;
	using FC.Manager.Web.RPC;

	public class GuildService : ServiceBase
	{
		public static ulong GuildId;
		public static bool CanManageGuild;

		[RPC]
		public bool IsInGuild(ulong guildId)
		{
			SocketGuild guild = DiscordService.DiscordClient.GetGuild(guildId);
			return guild != null;
		}

		[GuildRpc]
		public List<Channel> GetChannels(ulong guildId)
		{
			SocketGuild guild = DiscordService.DiscordClient.GetGuild(guildId)
				?? throw new Exception("Unable to access guild");

			List<Channel> results = [];
			foreach (SocketGuildChannel guildChannel in guild.Channels)
			{
				Channel.Types type = Channel.Types.Unknown;

				if (guildChannel is SocketTextChannel)
					type = Channel.Types.Text;

				if (guildChannel is SocketVoiceChannel)
					type = Channel.Types.Voice;

				results.Add(new Channel(guildChannel.Id, guildChannel.Name, type));
			}

			return results;
		}

		[GuildRpc]
		public List<Role> GetRoles(ulong guildId)
		{
			SocketGuild guild = DiscordService.DiscordClient.GetGuild(guildId)
				?? throw new Exception("Unable to access guild");

			List<Role> results = [];
			foreach (SocketRole guildRole in guild.Roles)
			{
				results.Add(new Role(guildRole.Id, guildRole.Name));
			}

			return results;
		}

		[GuildRpc]
		public async Task<List<GuildUser>> GetGuildUsers()
		{
			var guildId = RPCService.GuildId;

			SocketGuild guild = DiscordService.DiscordClient.GetGuild(guildId)
				?? throw new Exception("Unable to access guild");

			// Get the guild users
			IReadOnlyCollection<Discord.IGuildUser> guildUsers = await guild.GetUsersAsync().FirstOrDefaultAsync() ?? [];

			// Get database users
			List<User> users = await UserService.GetAllUsersForGuild(guildId);

			List<GuildUser> results = [];
			foreach (User guildUser in users)
			{
				Discord.IGuildUser? user = guildUsers.FirstOrDefault(x => x.Id == guildUser.DiscordUserId);
				results.Add(new GuildUser(
					guildUser.DiscordUserId,
					user == null ? "Unknown" : (user.Nickname ?? user.Username),
					guildUser.TotalKupoNutsCurrent,
					guildUser.TotalXPCurrent,
					guildUser.Level,
					guildUser.Reputation));
			}

			return results;
		}

		[GuildRpc]
		public async Task<GuildSettings> GetSettings(ulong guildId)
		{
			return await SettingsService.GetSettings<GuildSettings>(guildId);
		}

		[GuildRpc]
		public async Task SetSettings(ulong guildId, GuildSettings settings)
		{
			settings.Guild = guildId;
			await SettingsService.SaveSettings(settings);
		}

		[GuildRpc]
		public async Task<List<string>> GetTimezonesFromSettings(ulong guildId)
		{
			GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guildId);
			return settings.TimeZone;
		}

		[GuildRpc]
		public void ResetGuildUserNuts(ulong guildId)
		{
			// Get database users
			List<User> users = UserService.GetAllUsersForGuild(guildId).Result;

			foreach (User u in users)
				u.ClearTotalKupoNuts();

			return;
		}
	}
}
