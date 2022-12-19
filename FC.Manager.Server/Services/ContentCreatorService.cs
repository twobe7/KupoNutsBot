// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using FC.ContentCreator;
	using FC.Data;
	using FC.Manager.Server.RPC;

	public class ContentCreatorService : ServiceBase
	{
		private Table<ContentCreator> contentCreatorDb = new Table<ContentCreator>("KupoNuts_ContentCreator", 0);

		public override async Task Initialize()
		{
			await this.contentCreatorDb.Connect();
		}

		[GuildRpc]
		public async Task<List<ContentCreator>> GetContentCreators(ulong guildId)
		{
			Dictionary<string, object> search = new Dictionary<string, object>();
			search.Add("DiscordGuildId", guildId);
			List<ContentCreator> items = await this.contentCreatorDb.LoadAll(search);
			return items.OrderBy(x => x.GuildNickName).ToList();
		}

		[GuildRpc]
		public async Task DeleteContentCreator(ulong guildId, string contentCreatorId)
		{
			ContentCreator item = await this.contentCreatorDb.Load(contentCreatorId);
			if (item == null)
				return;

			////if (item.GuildId != guildId)
			////	throw new Exception("Attempt to delete another guilds shop item");

			await this.contentCreatorDb.Delete(contentCreatorId);
		}

		////[GuildRpc]
		////public Task UpdateContentCreator(ulong guildId, ContentCreator item)
		////{
		////	return Task.CompletedTask;
		////}
	}
}
