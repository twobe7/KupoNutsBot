// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Data;
	using FC.Events;
	using FC.Manager.Server.RPC;

	public class EventsService : ServiceBase
	{
		private Table<Event> eventsDb = new Table<Event>("KupoNuts_Events", 1);

		public override async Task Initialize()
		{
			await this.eventsDb.Connect();
		}

		[GuildRpc]
		public async Task<List<Event>> GetEvents(string guildId)
		{
			Dictionary<string, object> search = new Dictionary<string, object>();
			search.Add("ServerIdStr", guildId);
			return await this.eventsDb.LoadAll(search);
		}

		[GuildRpc]
		public async Task DeleteEvent(string guildId, string eventId)
		{
			await this.eventsDb.Delete(eventId);
		}

		[GuildRpc]
		public async Task UpdateEvent(string guildId, Event evt)
		{
			evt.ServerIdStr = guildId;
			await this.eventsDb.Save(evt);
		}

		[GuildRpc]
		public Task<EventsSettings> GetSettings(string guildId)
		{
			return SettingsService.GetSettings<EventsSettings>(guildId);
		}

		[GuildRpc]
		public Task SaveSettings(string guildId, EventsSettings settings)
		{
			// Don't let clients change this!
			settings.GuildId = guildId;

			return SettingsService.SaveSettings(settings);
		}
	}
}
