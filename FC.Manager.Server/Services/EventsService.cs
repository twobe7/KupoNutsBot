// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using FC.Events;
	using FC.Manager.Server.RPC;

	public class EventsService : ServiceBase
	{
		[GuildRpc]
		public async Task<List<Event>> GetEvents(string guildId)
		{
			await Task.Delay(0);
			return null;
		}
	}
}
