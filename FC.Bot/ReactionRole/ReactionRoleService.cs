// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Data;
	using FC.Events;
	using NodaTime;

	public class ReactionRoleService : ServiceBase
	{
		public static Table<ReactionRoleHeader> ReactionRoleHeaderDatabase = new Table<ReactionRoleHeader>("KupoNuts_ReactionRoleHeader", 0);
		public static Table<ReactionRole> ReactionRoleDatabase = new Table<ReactionRole>("KupoNuts_ReactionRole", 0);

		private static ReactionRoleService? instance;
		private Dictionary<ulong, string> messageReactionRoleLookup = new Dictionary<ulong, string>();

		public static ReactionRoleService Instance
		{
			get
			{
				if (instance is null)
					throw new Exception("No Reaction Role Service");

				return instance;
			}
		}

		public override async Task Initialize()
		{
			instance = this;
			await ReactionRoleHeaderDatabase.Connect();
			await ReactionRoleDatabase.Connect();

			ScheduleService.RunOnSchedule(this.Update, 15);
			await this.Update();

			Program.DiscordClient.ReactionAdded += this.ReactionAdded;
			Program.DiscordClient.ReactionRemoved += this.ReactionRemoved;
		}

		public override Task Shutdown()
		{
			instance = null;
			Program.DiscordClient.ReactionAdded -= this.ReactionAdded;
			Program.DiscordClient.ReactionRemoved -= this.ReactionRemoved;

			return Task.CompletedTask;
		}

		[Command("ReactionRoles", Permissions.Administrators, "Updates reaction roles")]
		public async Task Update()
		{
			// Load current reaction roles into lookup
			List<ReactionRoleHeader> reactionRoles = await ReactionRoleHeaderDatabase.LoadAll();

			// Remove any lookups where reaction role has been removed
			foreach (KeyValuePair<ulong, string> lookup in this.messageReactionRoleLookup)
			{
				if (reactionRoles.FirstOrDefault(x => x.MessageId == lookup.Key) == null)
					this.messageReactionRoleLookup.Remove(lookup.Key);
			}

			// Add new reaction roles to lookup
			foreach (ReactionRoleHeader rr in reactionRoles)
			{
				if (rr.Id == null)
					continue;

				this.messageReactionRoleLookup.TryAdd(rr.MessageId, rr.Id);
			}
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				// Try get reaction role
				ReactionRole reactionRole = await this.GetReactionRoleIfValid(message, channel, reaction);

				// Reaction Role not found or RoleId is null - skip
				if (reactionRole == null || reactionRole.RoleId == null)
					return;

				IUserMessage userMessage = await message.DownloadAsync();
				IGuild guild = userMessage.GetGuild();
				IGuildUser user = await guild.GetUserAsync(reaction.UserId);

				IRole role = guild.GetRole(reactionRole.RoleId.Value);
				if (role != null)
				{
					if (!user.RoleIds.Contains(role.Id))
						await user.AddRoleAsync(role);
				}

				await user.AddRoleAsync(role);
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				// Try get reaction role
				ReactionRole reactionRole = await this.GetReactionRoleIfValid(message, channel, reaction);

				// Reaction Role not found or RoleId is null - skip
				if (reactionRole == null || reactionRole.RoleId == null)
					return;

				IUserMessage userMessage = await message.DownloadAsync();
				IGuild guild = userMessage.GetGuild();
				IGuildUser user = await guild.GetUserAsync(reaction.UserId);

				IRole role = guild.GetRole(reactionRole.RoleId.Value);
				if (role != null)
				{
					if (user.RoleIds.Contains(role.Id))
						await user.RemoveRoleAsync(role);
				}

				await user.AddRoleAsync(role);
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task<ReactionRole> GetReactionRoleIfValid(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			// Don't modify bot roles
			if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
				return new ReactionRole();

			// Don't process if message is not a reaction role message
			if (!this.messageReactionRoleLookup.ContainsKey(message.Id))
				return new ReactionRole();

			// Load Reaction Roles for Message
			List<ReactionRole> reactionRoles = await ReactionRoleDatabase.LoadAll(new Dictionary<string, object> { { "MessageId", message.Id } });

			// Get first reaction role (should only be one for each message id)
			ReactionRole reactionRole = reactionRoles.GetFirst();

			return reactionRole;
		}
	}
}
