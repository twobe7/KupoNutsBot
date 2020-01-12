// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;
	using Microsoft.AspNetCore.Components;

	public static class Channels
	{
		public static async Task<List<Channel>> GetChannels(Channel.Types type = Channel.Types.Text)
		{
			List<Channel> allChanels = await RPCService.Invoke<List<Channel>>("GuildService.GetChannels");

			List<Channel> channels = new List<Channel>();
			foreach (Channel channel in allChanels)
			{
				if (channel.Type != type)
					continue;

				channels.Add(channel);
			}

			channels.Sort((Channel a, Channel b) =>
			{
				return a.Name.CompareTo(b.Name);
			});

			return channels;
		}
	}
}
