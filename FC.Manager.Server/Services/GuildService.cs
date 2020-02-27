// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using FC.Data;
	using FC.Manager.Server.RPC;

	public class GuildService : ServiceBase
	{
		[RPC]
		public bool IsInGuild(ulong guildId)
		{
			SocketGuild guild = DiscordService.DiscordClient.GetGuild(guildId);
			return guild != null;
		}

		[GuildRpc]
		public List<Channel> GetChannels(ulong guildId)
		{
			SocketGuild guild = DiscordService.DiscordClient.GetGuild(guildId);

			if (guild == null)
				throw new Exception("Unable to access guild");

			List<Channel> results = new List<Channel>();
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
	}
}
