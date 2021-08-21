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
	using FC.ReactionRoles;

	public class ReactionRoleService : ServiceBase
	{
		private readonly Table<ReactionRoleHeader> reactionRoleHeaderDb = new Table<ReactionRoleHeader>("KupoNuts_ReactionRoleHeader", 0);
		private readonly Table<ReactionRole> reactionRoleDb = new Table<ReactionRole>("KupoNuts_ReactionRole", 0);
		private readonly Table<ReactionRoleItem> reactionRoleItemDb = new Table<ReactionRoleItem>("KupoNuts_ReactionRoleItem", 0);

		public override async Task Initialize()
		{
			await this.reactionRoleHeaderDb.Connect();
			await this.reactionRoleDb.Connect();
			await this.reactionRoleItemDb.Connect();
		}

		[GuildRpc]
		public async Task<List<ReactionRole>> GetReactionRoles(ulong guildId)
		{
			Dictionary<string, object> search = new Dictionary<string, object>
			{
				{ "GuildId", guildId },
			};

			List<ReactionRole> items = await this.reactionRoleDb.LoadAll(search);

			// Get Items
			foreach (ReactionRole role in items)
			{
				role.Reactions = await this.reactionRoleItemDb.LoadAll(new Dictionary<string, object>
				{
					{ "ReactionRoleId", role.Id },
				});
			}

			return items.OrderBy(x => x.Name).ToList();
		}

		[GuildRpc]
		public async Task DeleteReactionRole(ulong guildId, string itemId)
		{
			ReactionRole item = await this.reactionRoleDb.Load(itemId);
			if (item == null)
				return;

			if (item.GuildId != guildId)
				throw new Exception("Attempt to delete another guilds reaction role");

			await this.reactionRoleDb.Delete(itemId);
			await this.reactionRoleHeaderDb.Delete(itemId);
		}

		[GuildRpc]
		public async Task UpdateReactionRole(ulong guildId, ReactionRole reactionRole)
		{
			reactionRole.GuildId = guildId;
			await this.reactionRoleDb.Save(reactionRole);

			// Save header
			if (reactionRole.ChannelId.HasValue)
				await this.reactionRoleHeaderDb.Save(new ReactionRoleHeader(reactionRole));
		}

		[GuildRpc]
		public async Task DeleteReactionItem(ulong guildId, string itemId)
		{
			ReactionRoleItem item = await this.reactionRoleItemDb.Load(itemId);
			if (item == null)
				return;

			if (item.GuildId != guildId)
				throw new Exception("Attempt to delete another guilds reaction role");

			await this.reactionRoleItemDb.Delete(itemId);

			await this.UpdateReactionRoleAndHeader(item.ReactionRoleId);
		}

		[GuildRpc]
		public async Task UpdateReactionItem(ulong guildId, ReactionRoleItem item, Emote test)
		{
			// Update item
			item.GuildId = guildId;
			await this.reactionRoleItemDb.Save(item);

			await this.UpdateReactionRoleAndHeader(item.ReactionRoleId);
		}

		private async Task UpdateReactionRoleAndHeader(string roleId)
		{
			// Update Role Updated value
			ReactionRole role = await this.reactionRoleDb.Load(roleId);
			await this.reactionRoleDb.Save(role);

			// Update Role Header Updated value
			ReactionRoleHeader roleHeader = await this.reactionRoleHeaderDb.Load(roleId);
			await this.reactionRoleHeaderDb.Save(roleHeader);
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
