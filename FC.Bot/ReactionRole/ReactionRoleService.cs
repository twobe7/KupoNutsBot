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
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using FC.Data;
	using FC.ReactionRoles;

	public class ReactionRoleService : ServiceBase
	{
		public static Table<ReactionRoleHeader> ReactionRoleHeaderDatabase = new Table<ReactionRoleHeader>("KupoNuts_ReactionRoleHeader", 0);
		public static Table<ReactionRole> ReactionRoleDatabase = new Table<ReactionRole>("KupoNuts_ReactionRole", 0);
		public static Table<ReactionRoleItem> ReactionRoleItemDatabase = new Table<ReactionRoleItem>("KupoNuts_ReactionRoleItem", 0);

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
			await ReactionRoleItemDatabase.Connect();

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
		public async Task ManualUpdate(CommandMessage message)
		{
			await this.Update();
			await message.Message.DeleteAsync();
		}

		public async Task Update()
		{
			// Load current reaction roles into lookup
			List<ReactionRoleHeader> roleHeaders = await ReactionRoleHeaderDatabase.LoadAll();

			// Remove any lookups where reaction role has been removed
			foreach (KeyValuePair<ulong, string> lookup in this.messageReactionRoleLookup)
			{
				if (roleHeaders.FirstOrDefault(x => x.MessageId == lookup.Key) == null)
					this.messageReactionRoleLookup.Remove(lookup.Key);
			}

			// Add new reaction roles to lookup
			foreach (ReactionRoleHeader rr in roleHeaders)
			{
				if (rr.Id == null || rr.ChannelId == null)
					continue;

				// Ensure there is a valid guild
				SocketGuild? guild = Program.DiscordClient.GetGuild(rr.GuildId);
				if (guild == null)
					continue;

				// Ensure we have a valid channel
				ISocketMessageChannel? channel = (ISocketMessageChannel?)guild.GetChannel(rr.ChannelId.Value);
				if (channel == null)
					continue;

				bool postMessageRequired = !rr.MessageId.HasValue;

				if (rr.MessageId.HasValue)
				{
					// Attempt to add to lookup
					this.messageReactionRoleLookup.TryAdd(rr.MessageId.Value, rr.Id);

					IMessage message = await channel.GetMessageAsync(rr.MessageId.Value);

					if (message == null)
						postMessageRequired = true;

					if (message != null && rr.Updated.HasValue
						&& ((!message.EditedTimestamp.HasValue && message.CreatedAt < rr.Updated.Value)
							|| (message.EditedTimestamp.HasValue && message.EditedTimestamp < rr.Updated.Value)))
					{
						if (message is RestUserMessage restUserMessage)
						{
							ReactionRole? reactionRole = await ReactionRoleDatabase.Load(rr.Id);
							if (reactionRole != null)
							{
								// Add reaction items to role object
								reactionRole.Reactions = await ReactionRoleItemDatabase.LoadAll(new Dictionary<string, object>
								{
									{ "ReactionRoleId", reactionRole.Id },
								});

								// Restrict to reactions with reactions
								reactionRole.Reactions = reactionRole.Reactions.Where(x => !string.IsNullOrWhiteSpace(x.Reaction)).ToList();

								// Update the embed
								await restUserMessage.ModifyAsync(x => x.Embed = reactionRole.ToEmbed());

								// Check reactions
								Dictionary<IEmote, int>? messageReactions = await restUserMessage.GetReactions();

								// Add missing reactions
								foreach (ReactionRoleItem emote in reactionRole.Reactions)
								{
									if (!string.IsNullOrWhiteSpace(emote.Reaction) && !messageReactions.TryGetValue(emote.ReactionEmote, out int _))
										await restUserMessage.AddReactionAsync(emote.ReactionEmote);
								}

								// Remove deleted reactions
								foreach (KeyValuePair<IEmote, int> react in messageReactions)
								{
									if (!reactionRole.Reactions.Any(x => x.ReactionEmote?.Name == react.Key?.Name))
									{
										try
										{
											await restUserMessage.RemoveAllReactionsForEmoteAsync(react.Key);
										}
										catch (Exception ex)
										{
											await Utils.Logger.LogExceptionToDiscordChannel(ex, "Error removing reactions to Role Reactions message.", guild.Id.ToString());
										}
									}
								}
							}
						}
					}
				}

				if (postMessageRequired)
				{
					ReactionRole? reactionRole = await ReactionRoleDatabase.Load(rr.Id);
					if (reactionRole != null)
					{
						// Add reaction items to role object
						reactionRole.Reactions = await ReactionRoleItemDatabase.LoadAll(new Dictionary<string, object>
						{
							{ "ReactionRoleId", reactionRole.Id },
						});

						// Post message
						RestUserMessage? reactionRoleMessage = await channel.SendMessageAsync(embed: reactionRole.ToEmbed());

						// Add reactions
						await reactionRoleMessage.AddReactionsAsync(reactionRole.ReactionsArray());

						// Add message id to ReactionRole
						reactionRole.MessageId = reactionRoleMessage.Id;

						// Add message Id to lookup
						this.messageReactionRoleLookup.TryAdd(reactionRole.MessageId.Value, rr.Id);

						// Clear reactions so they don't save to DB
						reactionRole.Reactions = new List<ReactionRoleItem>();

						// Save back to db
						await ReactionRoleHeaderDatabase.Save(new ReactionRoleHeader(reactionRole));
						await ReactionRoleDatabase.Save(reactionRole);
					}
				}
			}
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				// Try get reaction role
				ReactionRole reactionRole = await this.GetReactionRoleIfValid(message, channel, reaction);

				// Reaction Role not found or no Reactions - skip
				if (reactionRole == null || !reactionRole.Reactions.Any())
					return;

				// If Item matching added Reaction doesn't exist for reaction role - skip
				ReactionRoleItem item = reactionRole.Reactions.FirstOrDefault(x => x.ReactionEmote.Name == reaction.Emote.Name && x.Role != null);
				if (item == null)
					return;

				// Need to fetch message and guild to get user and role
				IUserMessage userMessage = await message.DownloadAsync();
				IGuild guild = userMessage.GetGuild();
				IGuildUser user = await guild.GetUserAsync(reaction.UserId);

				IRole role = guild.GetRole(item.Role.GetValueOrDefault());
				if (role != null)
				{
					if (!user.RoleIds.Contains(role.Id))
						await user.AddRoleAsync(role);
				}
			}
			catch (Exception ex)
			{
				await Utils.Logger.LogExceptionToDiscordChannel(
					ex,
					$"Role Reaction Added - MessageId: {message.Id}",
					(channel as IGuildChannel)?.GuildId.ToString(),
					(reaction.User.GetValueOrDefault() as IGuildUser)?.GetName());
			}
		}

		private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				// Try get reaction role
				ReactionRole reactionRole = await this.GetReactionRoleIfValid(message, channel, reaction);

				// Reaction Role not found or no Reactions - skip
				if (reactionRole == null || !reactionRole.Reactions.Any())
					return;

				// If Item matching added Reaction doesn't exist for reaction role - skip
				ReactionRoleItem item = reactionRole.Reactions.FirstOrDefault(x => x.ReactionEmote.Name == reaction.Emote.Name && x.Role != null);
				if (item == null)
					return;

				IUserMessage userMessage = await message.DownloadAsync();
				IGuild guild = userMessage.GetGuild();
				IGuildUser user = await guild.GetUserAsync(reaction.UserId);

				IRole role = guild.GetRole(item.Role.GetValueOrDefault());
				if (role != null)
				{
					if (user.RoleIds.Contains(role.Id))
						await user.RemoveRoleAsync(role);
				}
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

			// Load Reaction Role itemsfor Message
			reactionRole.Reactions = await ReactionRoleItemDatabase.LoadAll(new Dictionary<string, object>
			{
				{ "ReactionRoleId", reactionRole.Id },
			});

			return reactionRole;
		}
	}
}
