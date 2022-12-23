// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using FC.Data;

	public class ChannelService : ServiceBase
	{
		private static Table<ChannelData> channelTable = new Table<ChannelData>("KupoNuts_Channels", 0);

		public ChannelService(DiscordSocketClient discordClient)
		{
		}

		public static async Task<ChannelData> GetChannelData(ulong guild, ulong channel)
		{
			string id = guild.ToString() + "_" + channel.ToString();

			ChannelData? data = await channelTable.Load(id);

			if (data == null)
			{
				data = await channelTable.CreateEntry(id);
				data.GuildId = guild.ToString();
				data.ChannelId = channel.ToString();
				await channelTable.Save(data);
			}

			return data;
		}

		public static async Task<List<ChannelData>> GetAllChannelData()
		{
			return await channelTable.LoadAll();
		}

		public static async Task SaveChannelData(ChannelData data)
		{
			await channelTable.Save(data);
		}

		public override async Task Initialize()
		{
			await base.Initialize();
			await channelTable.Connect();
		}
	}
}
