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
	using Discord.Interactions;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Characters;
	using FC.Bot.Services;

	[Group("rpg", "Profile games")]
	public class RPGService : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient;

		private const double GenerationChance = 0.2;

		private readonly List<ulong> blockedChannels = new ()
		{
			837682896805691473,
			838350357074935838,
			838350425593479208,
			838350518853566474,
		};

		public RPGService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			this.DiscordClient.MessageReceived += this.OnMessageReceived;
		}

		public override Task Shutdown()
		{
			this.DiscordClient.MessageReceived -= this.OnMessageReceived;
			return base.Shutdown();
		}

		[SlashCommand("level", "Shows your level and current xp")]
		public async Task ShowUserLevel()
		{
			await this.DeferAsync();

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			// Get user information
			User user = await UserService.GetUser(guildUser);
			string userName = guildUser.GetName();

			EmbedBuilder builder = new EmbedBuilder()
				.WithTitle($"{userName}'s Level");

			StringBuilder description = new StringBuilder()
				.AppendLine("Current Level: " + user.Level)
				.AppendLine("Total EXP: " + user.TotalXPCurrent);
			builder.Description = description.ToString();

			await this.FollowupAsync(embeds: new Embed[] { builder.Build() });
		}

		[SlashCommand("levels", "Shows the level leaderboards")]
		public async Task ShowLevelLeaders()
		{
			await this.DeferAsync();

			List<User> users = await UserService.GetAllUsersForGuild(this.Context.Guild.Id);

			users.Sort((User a, User b) =>
			{
				return -a.TotalXPCurrent.CompareTo(b.TotalXPCurrent);
			});

			StringBuilder builder = new ();

			int count = 1;
			foreach (User user in users)
			{
				if (count > 10)
					break;

				if (user.Id == null)
					continue;

				IGuildUser discordUser = await this.Context.Guild.GetUserAsync(user.DiscordUserId);

				if (discordUser == null)
					continue;

				builder.AppendLine($"> {count}. {discordUser.GetName()}");
				builder.AppendLine($"> Level: **{user.Level}** ({user.TotalXPCurrent} xp)");
				builder.AppendLine();

				count++;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Level Leaderboard")
				.WithDescription(builder.ToString())
				.WithColor(Color.Blue);

			await this.FollowupAsync(embeds: new Embed[] { embedBuilder.Build() });
		}

		[SlashCommand("reputation-leaderboard", "Shows the reputation leaderboards")]
		public async Task ShowRepLeaders()
		{
			await this.DeferAsync();

			List<User> users = await UserService.GetAllUsersForGuild(this.Context.Guild.Id);

			users.Sort((User a, User b) =>
			{
				return -a.Reputation.CompareTo(b.Reputation);
			});

			StringBuilder builder = new ();

			int count = 1;
			foreach (User user in users)
			{
				if (count > 10)
					break;

				if (user.Id == null)
					continue;

				IGuildUser discordUser = await this.Context.Guild.GetUserAsync(user.DiscordUserId);

				if (discordUser == null)
					continue;

				builder.AppendLine($"> {count}. {discordUser.GetName()}");
				builder.AppendLine($"> Reputation: **{user.Reputation}**");
				builder.AppendLine();

				count++;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Reputation Leaderboard")
				.WithDescription(builder.ToString())
				.WithColor(Color.Blue);

			await this.FollowupAsync(embeds: new Embed[] { embedBuilder.Build() });
		}

		[SlashCommand("give-rep", "Show someone you think they're neat.")]
		public async Task GiveReputation(SocketGuildUser user)
		{
			await this.DeferAsync();

			// Handle bots
			if (user.IsBot)
			{
				// Thank user if trying to rep Kupo Nuts or tell them no if repping a different bot
				string botMessage = user.Id == this.DiscordClient.CurrentUser.Id
					? string.Format("I think you're pretty neat too, _kupo!_")
					: string.Format("They wouldn't understand...");

				await this.FollowupAsync(botMessage);
				return;
			}

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			// Get leaderboard settings
			LeaderboardSettings settings = await LeaderboardSettingsService.GetSettings<LeaderboardSettings>(this.Context.Guild.Id);

			if (settings.ReputationAddRole == "0")
			{
				// Rep disabled on server
				await this.FollowupAsync("Rep has been disabled by Server Admin");
				return;
			}
			else if (settings.ReputationAddRole != "1")
			{
				// Rep role required to add
				if (ulong.TryParse(settings.ReputationAddRole, out ulong repRole) && !guildUser.RoleIds.Contains(repRole))
				{
					// Rep Role required to add not assigned to user
					await this.FollowupAsync("You do not have permission to add Rep.");
					return;
				}
			}

			// Get sending user information
			User fromUser = await UserService.GetUser(guildUser);
			string fromUserName = guildUser.GetName();

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

			await this.FollowupAsync(postBackMessage);
		}

		[SlashCommand("remove-rep", "Removes Rep from User.")]
		public async Task RemoveReputation(SocketGuildUser user)
		{
			await this.DeferAsync();

			// Handle bots
			if (user.IsBot)
			{
				await this.FollowupAsync("Bots cannot gain reputation.");
				return;
			}

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			// Get leaderboard settings
			LeaderboardSettings settings = await LeaderboardSettingsService.GetSettings<LeaderboardSettings>(this.Context.Guild.Id);

			if (settings.ReputationRemoveRole == "0")
			{
				// Rep disabled on server
				await this.FollowupAsync("Rep Removal has been disabled by Server Admin");
				return;
			}
			else if (settings.ReputationRemoveRole != "1")
			{
				// Rep role required to remove
				if (ulong.TryParse(settings.ReputationRemoveRole, out ulong repRole) && !guildUser.RoleIds.Contains(repRole))
				{
					// Rep Role required to remove not assigned to user
					await this.FollowupAsync("You do not have permission to remove Rep.");
					return;
				}
			}

			// Get sending user information
			User fromUser = await UserService.GetUser(guildUser);
			string fromUserName = guildUser.GetName();

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

			await this.FollowupAsync(postBackMessage);
		}

		[SlashCommand("profile", "View your profile.")]
		public async Task ShowProfile()
		{
			await this.DeferAsync();

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			User user = await UserService.GetUser(guildUser);

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle(guildUser.GetName())
				.WithThumbnailUrl(guildUser.GetAvatarUrl())
				.WithColor(Color.Teal);

			// Default character
			User.Character? defaultCharacter = user.GetDefaultCharacter();
			if (defaultCharacter != null && !string.IsNullOrWhiteSpace(defaultCharacter.CharacterName))
			{
				// Character
				EmbedFieldBuilder character = new EmbedFieldBuilder()
					.WithName("Default Character")
					.WithValue($"**{defaultCharacter.CharacterName}**");
				embed.AddField(character);
			}

			// Nuts
			EmbedFieldBuilder nuts = new EmbedFieldBuilder()
				.WithName("Nuts")
				.WithValue($"**{user.TotalKupoNutsCurrent}** (Total Held: {user.TotalKupoNutsReceived})");
			embed.AddField(nuts);

			// Level
			EmbedFieldBuilder level = new EmbedFieldBuilder()
				.WithName("Level")
				.WithValue($"**{user.Level}** (Total XP: {user.TotalXPCurrent})");
			embed.AddField(level);

			// Rep
			EmbedFieldBuilder rep = new EmbedFieldBuilder()
				.WithName("Reputation")
				.WithValue($"**{user.Reputation}**");
			embed.AddField(rep);

			// Joined At
			EmbedFieldBuilder joined = new EmbedFieldBuilder()
				.WithName("Joined")
				.WithValue($"{guildUser.JoinedAt?.ToString("dd MMM yy")} ({(DateTime.Now.Date - (guildUser.JoinedAt?.Date ?? DateTime.Now.Date)).TotalDays} days ago)");
			embed.AddField(joined);

			await this.FollowupAsync(embeds: new Embed[] { embed.Build() });
		}

		private static async void GainXP(User user, int? xpGained = null)
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

				if (message.Author.Id == this.DiscordClient.CurrentUser.Id)
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
							GainXP(user);
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