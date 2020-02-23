// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Data;
	using FC.Eventsv2;
	using FC.Manager.Server.RPC;

	public class EventsV2Service : ServiceBase
	{
		private Table<Event> eventsDb = new Table<Event>("Events", 2);

		public override async Task Initialize()
		{
			await this.eventsDb.Connect();
		}

		[GuildRpc]
		public async Task<List<Event>> GetEvents(ulong guildId)
		{
			Dictionary<string, object> search = new Dictionary<string, object>();
			search.Add("GuildId", guildId);
			return await this.eventsDb.LoadAll(search);
		}

		[GuildRpc]
		public async Task DeleteEvent(ulong guildId, string eventId)
		{
			Event evt = await this.eventsDb.Load(eventId);
			if (evt == null)
				return;

			if (evt.GuildId != guildId)
				throw new Exception("Attempt to delete another guilds event");

			await this.eventsDb.Delete(eventId);
		}

		[GuildRpc]
		public async Task UpdateEvent(ulong guildId, Event evt)
		{
			evt.GuildId = guildId;
			await this.eventsDb.Save(evt);
		}

		[GuildRpc]
		public Task<FC.Events.EventsSettings> GetSettings(ulong guildId)
		{
			return SettingsService.GetSettings<FC.Events.EventsSettings>(guildId);
		}

		[GuildRpc]
		public Task SaveSettings(ulong guildId, FC.Events.EventsSettings settings)
		{
			// Don't let clients change this!
			settings.Guild = guildId;

			return SettingsService.SaveSettings(settings);
		}
	}
}
