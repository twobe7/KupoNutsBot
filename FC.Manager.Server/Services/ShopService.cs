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
	using FC.Manager.Server.RPC;
	using FC.Shop;

	public class ShopService : ServiceBase
	{
		private Table<ShopItem> shopItemsDb = new Table<ShopItem>("KupoNuts_ShopItems", 0);

		public override async Task Initialize()
		{
			await this.shopItemsDb.Connect();
		}

		[GuildRpc]
		public async Task<List<ShopItem>> GetShopItems(ulong guildId)
		{
			Dictionary<string, object> search = new Dictionary<string, object>();
			search.Add("GuildId", guildId);
			List<ShopItem> items = await this.shopItemsDb.LoadAll(search);
			return items.OrderBy(x => x.Name).ToList();
		}

		[GuildRpc]
		public async Task DeleteItem(ulong guildId, string itemId)
		{
			ShopItem item = await this.shopItemsDb.Load(itemId);
			if (item == null)
				return;

			if (item.GuildId != guildId)
				throw new Exception("Attempt to delete another guilds shop item");

			await this.shopItemsDb.Delete(itemId);
		}

		[GuildRpc]
		public async Task UpdateShopItem(ulong guildId, ShopItem item)
		{
			item.GuildId = guildId;
			await this.shopItemsDb.Save(item);
		}

		////[GuildRpc]
		////public Task<FC.Events.EventsSettings> GetSettings(ulong guildId)
		////{
		////	return SettingsService.GetSettings<FC.Events.EventsSettings>(guildId);
		////}

		////[GuildRpc]
		////public Task SaveSettings(ulong guildId, FC.Events.EventsSettings settings)
		////{
		////	// Don't let clients change this!
		////	settings.Guild = guildId;

		////	return SettingsService.SaveSettings(settings);
		////}
	}
}
