// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using FC.Data;
	using FC.Events;
	using FC.Manager.Server.RPC;

	public class EventsService : ServiceBase
	{
		[GuildRpc]
		public async Task<List<Event>> GetEvents(string guildId)
		{
			Table<Event> eventsDb = Table<Event>.Create("KupoNuts_Events", 1);
			await eventsDb.Connect();

			Dictionary<string, object> search = new Dictionary<string, object>();
			search.Add("ServerIdStr", guildId);
			return await eventsDb.LoadAll(search);
		}
	}
}
