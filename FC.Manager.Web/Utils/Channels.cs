// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web
{
	using FC.Manager.Web.Services;
	using System.Collections.Generic;

	public static class Channels
	{
		public static List<Channel> GetChannels(
			GuildService guildService,
			Channel.Types type = Channel.Types.Text)
		{
			List<Channel> allChanels = guildService.GetChannels(RPCService.GuildId);

			List<Channel> channels = [];
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
