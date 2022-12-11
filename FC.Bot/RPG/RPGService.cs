// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.RPG
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Characters;
	using FC.Bot.Commands;
	using FC.Bot.Services;

	public class RPGService : ServiceBase
	{
		private const double GenerationChance = 0.2;

		private readonly List<ulong> blockedChannels = new List<ulong>()
		{
			837682896805691473,
			838350357074935838,
			838350425593479208,
			838350518853566474,
		};

		public override async Task Initialize()
		{
			await base.Initialize();

			Program.DiscordClient.MessageReceived += this.OnMessageReceived;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.MessageReceived -= this.OnMessageReceived;
			return base.Shutdown();
		}

		[Command("Level", Permissions.Everyone, "Shows your level and current xp", CommandCategory.Novelty)]
		public async Task<Embed> ShowUserLevel(CommandMessage message)
		{
			// Get user information
			User user = await UserService.GetUser(message.Author);
			string userName = message.Author.GetName();

			EmbedBuilder builder = new EmbedBuilder
			{
				Title = userName + "'s Level",
			};

			StringBuilder description = new StringBuilder();
			description.AppendLine("Current Level: " + user.Level);
			description.AppendLine("Total EXP: " + user.TotalXPCurrent);
			builder.Description = description.ToString();

			// Remove calling command
			await message.Channel.DeleteMessageAsync(message.Message);

			return builder.Build();
		}

		[Command("Levels", Permissions.Everyone, "Shows the level leaderboards", CommandCategory.Novelty)]
		public async Task<Embed> ShowLevelLeaders(CommandMessage message)
		{
			IGuild guild = message.Guild;

			List<User> users = await UserService.GetAllUsersForGuild(guild.Id);

			users.Sort((User a, User b) =>
			{
				return -a.TotalXPCurrent.CompareTo(b.TotalXPCurrent);
			});

			StringBuilder builder = new StringBuilder();

			int count = 1;
			foreach (User user in users)
			{
				if (count > 10)
					break;

				if (user.Id == null)
					continue;

				IGuildUser discordUser = await guild.GetUserAsync(user.DiscordUserId);

				if (discordUser == null)
					continue;

				builder.AppendLine("> " + count + ". " + discordUser.GetName());
				builder.AppendLine("> Level: **" + user.Level + "** (" + user.TotalXPCurrent + " xp)");
				builder.AppendLine();

				count++;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder
			{
				Title = "Level Leaderboard",
				Description = builder.ToString(),
				Color = Color.Blue,
			};
			return embedBuilder.Build();
		}

		[Command("RL", Permissions.Everyone, "Shows the reputation leaderboards", CommandCategory.Novelty, "ReputationLeaderboard")]
		[Command("RepLeaderboard", Permissions.Everyone, "Shows the reputation leaderboards", CommandCategory.Novelty, "ReputationLeaderboard")]
		[Command("ReputationLeaderboard", Permissions.Everyone, "Shows the reputation leaderboards", CommandCategory.Novelty)]
		public async Task<Embed> ShowRepLeaders(CommandMessage message)
		{
			IGuild guild = message.Guild;

			List<User> users = await UserService.GetAllUsersForGuild(guild.Id);

			users.Sort((User a, User b) =>
			{
				return -a.Reputation.CompareTo(b.Reputation);
			});

			StringBuilder builder = new StringBuilder();

			int count = 1;
			foreach (User user in users)
			{
				if (count > 10)
					break;

				if (user.Id == null)
					continue;

				IGuildUser discordUser = await guild.GetUserAsync(user.DiscordUserId);

				if (discordUser == null)
					continue;

				builder.AppendLine("> " + count + ". " + discordUser.GetName());
				builder.AppendLine("> Reputation: **" + user.Reputation + "**");
				builder.AppendLine();

				count++;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder
			{
				Title = "Reputation Leaderboard",
				Description = builder.ToString(),
				Color = Color.Blue,
			};
			return embedBuilder.Build();
		}

		[Command("Rep", Permissions.Everyone, "Show someone you think they're neat.", CommandCategory.Novelty, "GiveRep")]
		[Command("GiveRep", Permissions.Everyone, "Show someone you think they're neat.", CommandCategory.Novelty)]
		public async Task GiveReputation(CommandMessage message, IGuildUser user)
		{
			// Handle bots
			if (user.IsBot)
			{
				// Thank user if trying to rep Kupo Nuts or tell them no if repping a different bot
				string botMessage = user.Id == Program.DiscordClient.CurrentUser.Id
					? string.Format("I think you're pretty neat too, _kupo!_")
					: string.Format("They wouldn't understand...");

				await message.Channel.SendMessageAsync(botMessage, messageReference: message.MessageReference);
				return;
			}

			// Get leaderboard settings
			LeaderboardSettings settings = await LeaderboardSettingsService.GetSettings<LeaderboardSettings>(message.Guild.Id);

			if (settings.ReputationAddRole == "0")
			{
				// Rep disabled on server
				await message.Channel.SendMessageAsync("Rep has been disabled by Server Admin", messageReference: message.MessageReference);
				return;
			}
			else if (settings.ReputationAddRole != "1")
			{
				// Rep role required to add
				if (ulong.TryParse(settings.ReputationAddRole, out ulong repRole) && !message.Author.RoleIds.Contains(repRole))
				{
					// Rep Role required to add not assigned to user
					await message.Channel.SendMessageAsync("You do not have permission to add Rep.", messageReference: message.MessageReference);
					return;
				}
			}

			// Get sending user information
			User fromUser = await UserService.GetUser(message.Author);
			string fromUserName = message.Author.GetName();

			// Get receiving user information
			User toUser = await UserService.GetUser(user);
			string toUserName = user.GetName();

			// Daily reset at 8AM
			DateTime dailyReset = DateTime.Now.Date.AddHours(8);

			// Check if it's too early
			if (DateTime.Now < dailyReset)
				dailyReset = dailyReset.AddDays(-1);

			// Message for return
			string postBackMessage;

			// If rep limit is disabled or user has not repped today
			if (!settings.LimitReputationPerDay || !fromUser.LastRepGiven.HasValue || fromUser.LastRepGiven < dailyReset)
			{
				if (fromUser.Id == toUser.Id)
				{
					postBackMessage = "You can't rep yourself, _kupo!_";
				}
				else
				{
					// Update user information
					fromUser.LastRepGiven = DateTime.Now;
					_ = UserService.SaveUser(fromUser);

					toUser.Reputation += 1;
					_ = UserService.SaveUser(toUser);

					postBackMessage = $"Hey {toUserName}, {fromUserName} thinks you're pretty neat, _kupo!_\nYour rep is now: {toUser.Reputation}";
				}
			}
			else
			{
				postBackMessage = "You have already given your rep today!";
			}

			await message.Channel.SendMessageAsync(postBackMessage, messageReference: message.MessageReference);
			return;
		}

		[Command("Rep", Permissions.Everyone, "Show someone you think they're neat.", CommandCategory.Novelty, "GiveRep")]
		[Command("GiveRep", Permissions.Everyone, "Show someone you think they're neat.", CommandCategory.Novelty)]
		public async Task GiveReputationByUserString(CommandMessage message, string user)
		{
			// Get guild users by name
			List<IGuildUser> userToRep = await UserService.GetUsersByNickName(message.Guild, user);

			if (userToRep.Count == 1)
			{
				await this.GiveReputation(message, userToRep.First());
			}
			else
			{
				RestUserMessage response = await message.Channel.SendMessageAsync("I'm sorry, I'm not sure who you mean. Try mentioning them, _kupo!_", messageReference: message.MessageReference);

				// Wait, then delete both messages
				await Task.Delay(2000);

				await message.Channel.DeleteMessageAsync(response.Id);
				await message.Channel.DeleteMessageAsync(message.Id);
			}

			return;
		}

		[Command("RemoveRep", Permissions.Everyone, "Removes Rep from User.", CommandCategory.Novelty)]
		public async Task RemoveReputation(CommandMessage message, IGuildUser user)
		{
			// Handle bots
			if (user.IsBot)
			{
				await message.Channel.SendMessageAsync("Bots cannot gain reputation.", messageReference: message.MessageReference);
				return;
			}

			// Get leaderboard settings
			LeaderboardSettings settings = await LeaderboardSettingsService.GetSettings<LeaderboardSettings>(message.Guild.Id);

			if (settings.ReputationRemoveRole == "0")
			{
				// Rep disabled on server
				await message.Channel.SendMessageAsync("Rep Removal has been disabled by Server Admin", messageReference: message.MessageReference);
				return;
			}
			else if (settings.ReputationRemoveRole != "1")
			{
				// Rep role required to remove
				if (ulong.TryParse(settings.ReputationRemoveRole, out ulong repRole) && !message.Author.RoleIds.Contains(repRole))
				{
					// Rep Role required to remove not assigned to user
					await message.Channel.SendMessageAsync("You do not have permission to remove Rep.", messageReference: message.MessageReference);
					return;
				}
			}

			// Get sending user information
			User fromUser = await UserService.GetUser(message.Author);
			string fromUserName = message.Author.GetName();

			// Get receiving user information
			User toUser = await UserService.GetUser(user);
			string toUserName = user.GetName();

			// Message for return
			string postBackMessage;

			if (fromUser.Id == toUser.Id)
			{
				postBackMessage = "You can't remove your own rep, _kupo!_";
			}
			else
			{
				// Update user information
				toUser.Reputation -= 1;
				_ = UserService.SaveUser(toUser);

				postBackMessage = $"Hey {toUserName}, {fromUserName} thinks you've done bad, _kupo!_\nYour rep is now: {toUser.Reputation}";
			}

			await message.Channel.SendMessageAsync(postBackMessage, messageReference: message.MessageReference);
			return;
		}

		[Command("RemoveRep", Permissions.Everyone, "Removes Rep from User.", CommandCategory.Novelty)]
		public async Task RemoveReputationByUserString(CommandMessage message, string user)
		{
			// Get guild users by name
			List<IGuildUser> userToRep = await UserService.GetUsersByNickName(message.Guild, user);

			if (userToRep.Count == 1)
			{
				await this.RemoveReputation(message, userToRep.First());
			}
			else
			{
				RestUserMessage response = await message.Channel.SendMessageAsync("I'm sorry, I'm not sure who you mean. Try mentioning them, _kupo!_", messageReference: message.MessageReference);

				// Wait, then delete both messages
				await Task.Delay(2000);

				await message.Channel.DeleteMessageAsync(response.Id);
				await message.Channel.DeleteMessageAsync(message.Id);
			}

			return;
		}

		[Command("Profile", Permissions.Everyone, "View your profile.", CommandCategory.Novelty)]
		public async Task<Embed> ShowProfile(CommandMessage message)
		{
			EmbedBuilder embed = new EmbedBuilder();

			User user = await UserService.GetUser(message.Author);

			embed.Title = message.Author.GetName();
			embed.ThumbnailUrl = message.Author.GetAvatarUrl();
			embed.Color = Color.Teal;

			// Default character
			User.Character? defaultCharacter = user.GetDefaultCharacter();
			if (defaultCharacter != null && !string.IsNullOrWhiteSpace(defaultCharacter.CharacterName))
			{
				// Character
				EmbedFieldBuilder character = new EmbedFieldBuilder()
					.WithName("Default Character")
					.WithValue("**" + defaultCharacter.CharacterName + "**");
				embed.AddField(character);
			}

			// Nuts
			EmbedFieldBuilder nuts = new EmbedFieldBuilder()
				.WithName("Nuts")
				.WithValue("**" + user.TotalKupoNutsCurrent + "** (Total Held: " + user.TotalKupoNutsReceived + ")");
			embed.AddField(nuts);

			// Level
			EmbedFieldBuilder level = new EmbedFieldBuilder()
				.WithName("Level")
				.WithValue("**" + user.Level + "** (Total XP: " + user.TotalXPCurrent + ")");
			embed.AddField(level);

			// Rep
			EmbedFieldBuilder rep = new EmbedFieldBuilder()
				.WithName("Reputation")
				.WithValue("**" + user.Reputation + "**");
			embed.AddField(rep);

			// Joined At
			EmbedFieldBuilder joined = new EmbedFieldBuilder()
				.WithName("Joined")
				.WithValue(message.Author.JoinedAt?.ToString("dd MMM yy") + " (" + (DateTime.Now.Date - (message.Author?.JoinedAt?.Date ?? DateTime.Now.Date)).TotalDays + " days ago)");
			embed.AddField(joined);

			return embed.Build();
		}

		private async void GainXP(User user, int? xpGained = null)
		{
			// Add gained XP provided or generate random XP
			user.TotalXPCurrent += xpGained ?? new Random().Next(1, 5);
			await UserService.SaveUser(user);
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			try
			{
				// Temp removal from pokemeow channels
				if (this.blockedChannels.Contains(message.Channel.Id))
					return;

				if (message.Author.Id == Program.DiscordClient.CurrentUser.Id)
					return;

				if (message.Author.IsBot)
					return;

				await Task.Run(async () =>
				{
					double roll = new Random().NextDouble();
					if (roll < GenerationChance)
					{
						IMessage iMessage = await message.Channel.GetMessageAsync(message.Id);

						if (iMessage is RestUserMessage restMessage)
						{
							IGuildUser toUser = message.GetAuthor();

							Log.Write(toUser.GetName() + " Gained XP with message: \"" + message.Content + "\"", "Bot");

							User user = await UserService.GetUser(toUser);
							this.GainXP(user);
						}
					}
				});
			}
			catch (Discord.Net.HttpException)
			{
				// in case the discord request has failed, just abort out.
				// this can happen if a message is deleted or edited before we call GetMessageAsync.
				return;
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}