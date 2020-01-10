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
	using Microsoft.AspNetCore.Components;

	public static class Channels
	{
		public static async Task<List<Channel>> GetChannels(HttpClient http, Channel.Types type)
		{
			List<Channel> allChanels = await http.GetJsonAsync<List<Channel>>("ChannelsAPI");
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
