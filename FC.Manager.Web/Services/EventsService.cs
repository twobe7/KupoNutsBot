// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Data;
	using FC.Events;
	using FC.Manager.Web.RPC;

	public class EventsService : ServiceBase
	{
		private Table<Event> eventsDb = new Table<Event>("KupoNuts_Events", 1);

		public override async Task Initialize()
		{
			await this.eventsDb.Connect();
		}

		[GuildRpc]
		public async Task<List<Event>> GetEvents()
		{
			Dictionary<string, object> search = new()
			{
				{ "ServerIdStr", RPCService.GuildId.ToString() }
			};
			return await this.eventsDb.LoadAll(search);
		}

		[GuildRpc]
		public async Task DeleteEvent(string eventId)
		{
			await this.eventsDb.Delete(eventId);
		}

		[GuildRpc]
		public async Task UpdateEvent(Event evt)
		{
			evt.ServerIdStr = RPCService.GuildId.ToString();
			await this.eventsDb.Save(evt);
		}

		[GuildRpc]
		public Task<EventsSettings> GetSettings()
		{
			return SettingsService.GetSettings<EventsSettings>(RPCService.GuildId);
		}

		[GuildRpc]
		public Task SaveSettings(EventsSettings settings)
		{
			// Don't let clients change this!
			settings.Guild = RPCService.GuildId;

			return SettingsService.SaveSettings(settings);
		}
	}
}
