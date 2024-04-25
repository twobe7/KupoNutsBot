// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Discord.WebSocket;
	using FC.Manager.Web.RPC;

	public class EmoteService : ServiceBase
	{
		[RPC]
		public bool IsInGuild(ulong guildId)
		{
			SocketGuild guild = DiscordService.DiscordClient.GetGuild(guildId);
			return guild != null;
		}

		[GuildRpc]
		public List<Emote> GetEmotes(ulong guildId)
		{
			List<Emote> results = new List<Emote>();

			// Get current guild emotes
			SocketGuild guild = DiscordService.DiscordClient.GetGuild(guildId);

			if (guild == null)
				throw new Exception("Unable to access guild");

			foreach (Discord.GuildEmote guildEmote in guild.Emotes.OrderBy(x => x.Name))
			{
				results.Add(new Emote(guildEmote.Id, guildEmote.Name, guildEmote.Url, guildEmote.RequireColons, false));
			}

			// Get emotes from Bot Discord Server
			string discordServerId = Settings.Load().BotDiscordServer;
			if (!string.IsNullOrEmpty(discordServerId) && guildId != ulong.Parse(discordServerId))
			{
				SocketGuild kupoNutsGuild = DiscordService.DiscordClient.GetGuild(ulong.Parse(discordServerId));

				if (kupoNutsGuild == null)
					throw new Exception("Unable to access guild");

				foreach (Discord.GuildEmote guildEmote in kupoNutsGuild.Emotes.OrderBy(x => x.Name))
				{
					results.Add(new Emote(guildEmote.Id, guildEmote.Name, guildEmote.Url, guildEmote.RequireColons, false));
				}
			}

			// Get basic emojis
			////Emotes emotesClass = new Emotes();
			////foreach (KeyValuePair<string, string> emote in emotesClass.GetAllEmotes())
			////{
			////	results.Add(new Emote(emote.Key, emote.Value));
			////}

			return results;
		}
	}
}
