// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
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
	using FC.Bot.Commands;
	using FC.Bot.Currency;
	using FC.Bot.Extensions;
	using FC.Data;
	using FC.Shop;
	using Microsoft.Extensions.DependencyInjection;

	[Group("currency", "Currency based commands including games")]
	public class CurrencyService : ServiceBase
	{
		public const int DefaultBetAmount = 10;

		public readonly DiscordSocketClient DiscordClient;

		private const double GenerationChance = 0.05;
		private const int GameCoolDownInSeconds = 30;

		private static readonly Table<ShopItem> ShopItemDatabase = new ("KupoNuts_ShopItems", 0);

		// TODO: Move into Guild Settings - allow user to provide list of channels to be blocked
		private readonly List<ulong> blockedChannels = new ()
		{
			837682896805691473,
			838350357074935838,
			838350425593479208,
			838350518853566474,
		};

		// Run times
		private readonly Dictionary<ulong, DateTime?> slotsLastRunTime;
		private readonly Dictionary<ulong, DateTime?> activeInventoryWindows;
		private readonly Dictionary<ulong, DateTime?> blackjackLastRunTime;
		private readonly Dictionary<ulong, uint> userDailyGameCount;

		public CurrencyService(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
		{
			this.DiscordClient = discordClient;

			// Set run times
			var runTimes = serviceProvider.GetRequiredService<CurrencyRunTimes>();
			this.slotsLastRunTime = runTimes.SlotsLastRunTime;
			this.activeInventoryWindows = runTimes.ActiveInventoryWindows;
			this.blackjackLastRunTime = runTimes.BlackjackLastRunTime;
			this.userDailyGameCount = runTimes.UserDailyGameCount;
		}

		private enum CurrencyGame
		{
			Slots,
			Blackjack,
			ConnectFour,
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			await ShopItemDatabase.Connect();

			this.DiscordClient.MessageReceived += this.OnMessageReceived;

			ScheduleService.RunOnSchedule(this.ClearDailyGameCount, 60);
		}

		public override Task Shutdown()
		{
			this.DiscordClient.MessageReceived -= this.OnMessageReceived;
			return base.Shutdown();
		}

		[SlashCommand("nut-leaderboard", "Shows the kupo nut leader boards")]
		public async Task ShowLeaders()
		{
			await this.DeferAsync();

			IGuild guild = this.Context.Guild;

			List<User> users = await UserService.GetAllUsersForGuild(guild.Id);

			users.Sort((User a, User b) =>
			{
				return -a.TotalKupoNutsCurrent.CompareTo(b.TotalKupoNutsCurrent);
			});

			StringBuilder builder = new ();

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

				builder.Append(user.TotalKupoNutsCurrent);
				builder.Append(" - ");
				builder.AppendLine(discordUser.GetName());

				count++;
			}

			EmbedBuilder embedBuilder = new ()
			{
				Title = "Kupo Nut Leaderboard",
				Description = builder.ToString(),
			};

			await this.FollowupAsync(embed: embedBuilder.Build());
		}

		[SlashCommand("nuts", "Check your pile of Kupo Nuts")]
		public async Task Nuts()
		{
			// Get user information
			var guildUser = await this.Context.Guild.GetUserAsync(this.Context.User.Id);
			User user = await UserService.GetUser(guildUser);
			string userName = guildUser.GetName();

			EmbedBuilder builder = new () { Title = userName + "'s Kupo Nut Stash!" };

			StringBuilder description = new StringBuilder();
			description.AppendLine("Current Total: " + user.TotalKupoNutsCurrent);
			description.AppendLine("Total Held: " + user.TotalKupoNutsReceived);
			builder.Description = description.ToString();

			await this.RespondAsync(embeds: new Embed[] { builder.Build() });
		}

		[SlashCommand("dailynuts", "Get your daily nut")]
		public async Task DailyNuts()
		{
			// Get user information
			var guildUser = await this.Context.Guild.GetUserAsync(this.Context.User.Id);
			User user = await UserService.GetUser(guildUser);
			string userName = guildUser.GetName();

			string postBackMessage;

			if (user.LastDailyNut.HasValue)
			{
				// Daily reset at 8AM
				DateTime dailyReset = DateTime.Now.Date.AddHours(8);

				// Check if it's too early
				if (DateTime.Now < dailyReset)
					dailyReset = dailyReset.AddDays(-1);

				// If the user has already nutted, tell them off
				if (user.LastDailyNut.Value > dailyReset)
				{
					TimeSpan timeToWait = dailyReset.AddDays(1) - DateTime.Now;
					postBackMessage = $"Greedy {userName}! You can have more Kupo Nuts in {timeToWait.ToLongString()}, _kupo!_";

					await this.RespondAsync(postBackMessage);

					await Task.Delay(10000);

					await this.DeleteOriginalResponseAsync();

					return;
				}
			}

			// Generate random nuts
			int nutsReceieved = new Random().Next(10, 26);

			// Save to user
			await user.UpdateTotalKupoNuts(nutsReceieved, true);

			postBackMessage = string.Format("{0}, please enjoy these {1} Kupo Nuts! _Kupo!_", userName, nutsReceieved);

			// Send message
			await this.RespondAsync(postBackMessage);
		}

		[SlashCommand("givenuts", "Share your Kupo Nuts with someone")]
		public async Task GiveNuts(int numberOfNuts, IGuildUser user)
		{
			await this.DeferAsync(ephemeral: true);

			// Message for return
			string postBackMessage;

			// Get sender user name
			var guildUser = await this.Context.Guild.GetUserAsync(this.Context.User.Id);
			string fromUserName = guildUser.GetName();

			// Handle bots
			if (user.IsBot)
			{
				postBackMessage = user.Id == this.DiscordClient.CurrentUser.Id
					? string.Format("Thanks {0} but I have plenty of Kupo Nuts. You hang onto these, _kupo!_", fromUserName)
					: string.Format("That Bot has no need for delicious Kupo Nuts!");

				await this.FollowupAsync(postBackMessage);
			}

			// Get sending user information
			User fromUser = await UserService.GetUser(guildUser);

			// Get receiving user information
			User toUser = await UserService.GetUser(user);
			string toUserName = user.GetName();

			if (fromUser.TotalKupoNutsCurrent >= numberOfNuts)
			{
				if (fromUser.Id == toUser.Id)
				{
					postBackMessage = string.Format("You can't give Kupo Nuts to yourself, _kupo!_");
				}
				else
				{
					await fromUser.UpdateTotalKupoNuts(-numberOfNuts);
					await toUser.UpdateTotalKupoNuts(numberOfNuts);

					postBackMessage = string.Format("Lucky {0}, {1} has given you {2} Kupo Nuts!", toUserName, fromUserName, numberOfNuts);
				}
			}
			else
			{
				postBackMessage = string.Format("You only have {0} Kupo Nuts to give!", fromUser.TotalKupoNutsCurrent);
			}

			await this.FollowupAsync(postBackMessage);
		}

		[Command("Inventory", Permissions.Everyone, "Shows your current inventory", CommandCategory.Currency)]
		public async Task ShowInventory(CommandMessage message)
		{
			// Shop items
			List<ShopItem> items = new List<ShopItem>();

			User user = await UserService.GetUser(message.Author);

			EmbedBuilder embed = new EmbedBuilder();
			embed.Title = message.Author.GetName() + "'s Inventory";
			embed.Color = Color.Blue;

			StringBuilder desc = new StringBuilder();
			if (user.Inventory.Count == 0)
			{
				desc.AppendLine("Nothing to show here. Visit our shop, _kupo!_");
			}
			else
			{
				// Load shop items
				items = await ShopItemDatabase.LoadAll(new Dictionary<string, object>()
				{
					{ "GuildId", message.Guild.Id },
				});

				// Restrict items to only held
				items = items.Where(x => user.Inventory.ContainsKey(x.Name)).ToList();

				foreach (KeyValuePair<string, int> item in user.Inventory)
				{
					desc.AppendLine(item.Value + "x - " + item.Key);
				}
			}

			desc.AppendLine(Utils.Characters.Tab);

			embed.Description = desc.ToString();

			string prefix = CommandsService.GetPrefix(message.Guild.Id);
			embed.WithFooter("Use " + prefix + "shop to see items to buy. Select a reaction to redeem.");

			RestUserMessage userMessage = await message.Channel.SendMessageAsync(embed: embed.Build(), messageReference: message.MessageReference);

			// Add reacts
			await userMessage.AddReactionsAsync(items.Select(x => x.ReactionEmote).ToArray());

			// Handle reacts
			this.DiscordClient.ReactionAdded += this.OnReactionAddedInventory;

			// Add to active windows
			this.activeInventoryWindows.Add(userMessage.Id, DateTime.Now);

			// Clear reacts
			_ = Task.Run(async () => await this.StopInventoryListener(userMessage));
		}

		[Command("Shop", Permissions.Everyone, "Spend those hard earned nuts, _kupo!_", CommandCategory.Currency, showWait: false)]
		public async Task<Task> ShowShop(CommandMessage message)
		{
			// Load shop items
			List<ShopItem> items = await ShopItemDatabase.LoadAll(new Dictionary<string, object>()
			{
				{ "GuildId", message.Guild.Id },
			});

			return await new Shop(items).DisplayShop(message);
		}

		[SlashCommand("slots", "Spin it to win it!")]
		public async Task Slots()
		{
			await this.DeferAsync();

			if (await this.ValidateLastRunTime(this.Context, CurrencyGame.Slots))
			{
				if (await this.ValidateDailyCurrencyGameAllowance(this.Context))
				{
					User user = await UserService.GetUser(this.Context.Guild.Id, this.Context.User.Id);
					if (user.TotalKupoNutsCurrent > DefaultBetAmount)
					{
						await user.UpdateTotalKupoNuts(-DefaultBetAmount);

						this.UpdateLastRunTime(CurrencyGame.Slots, this.Context.User.Id, this.Context.Guild.Id);

						await new Slots().StartSlot(this.Context);
					}
					else
					{
						await this.FollowupAsync($"You must have {DefaultBetAmount} Kupo Nuts to play the Slots, _kupo!_");

						await Task.Delay(2000);

						await this.DeleteOriginalResponseAsync();
					}
				}
			}
		}

		[SlashCommand("blackjack", "Play a hand of blackjack!")]
		public async Task Blackjack(uint bet = DefaultBetAmount)
		{
			await this.DeferAsync();

			if (await this.ValidateLastRunTime(this.Context, CurrencyGame.Blackjack))
			{
				if (await this.ValidateDailyCurrencyGameAllowance(this.Context))
				{
					User user = await UserService.GetUser(this.Context.Guild.Id, this.Context.User.Id);
					if (user.TotalKupoNutsCurrent >= bet && bet < int.MaxValue)
					{
						int nutsToSubtract = Convert.ToInt32(bet) * -1;
						await user.UpdateTotalKupoNuts(nutsToSubtract);

						this.UpdateLastRunTime(CurrencyGame.Blackjack, this.Context.User.Id, this.Context.Guild.Id);

						await new Blackjack(bet).StartBlackjack(this.Context);
					}
					else
					{
						await this.FollowupAsync($"You don't have {bet} Kupo Nuts to play Blackjack, _kupo!_");

						await Task.Delay(2000);

						await this.DeleteOriginalResponseAsync();
					}
				}
			}
		}

		[SlashCommand("blackjack-stop", "Ends the current hand of blackjack")]
		public async Task StopBlackjack()
		{
			await this.DeferAsync();
			await new Blackjack().EndBlackjack(this.Context);
		}

		[SlashCommand("connect-four", "Start a game of Connect 4!")]
		public async Task ConnectFour(IGuildUser user)
		{
			await this.DeferAsync();

			await new ConnectFour().StartGame(this.Context, user);
			////if (await this.ValidateLastRunTime(this.Context, CurrencyGame.ConnectFour))
			////{
			////	if (await this.ValidateDailyCurrencyGameAllowance(this.Context))
			////	{
			////		User user = await UserService.GetUser(this.Context.Guild.Id, this.Context.User.Id);
			////		if (user.TotalKupoNutsCurrent > DefaultBetAmount)
			////		{
			////			await user.UpdateTotalKupoNuts(-DefaultBetAmount);

			////			this.UpdateLastRunTime(CurrencyGame.ConnectFour, this.Context.User.Id, this.Context.Guild.Id);

			////			await new ConnectFour().StartGame(this.Context);
			////		}
			////		else
			////		{
			////			await this.FollowupAsync($"You must have {DefaultBetAmount} Kupo Nuts to play the Slots, _kupo!_");

			////			await Task.Delay(2000);

			////			await this.DeleteOriginalResponseAsync();
			////		}
			////	}
			////}
		}

		[ComponentInteraction("connectFourResponse-*", true)]
		public async Task ConnectFourButtonHandler()
		{
			if (this.Context is SocketInteractionContext ctx)
			{
				if (int.TryParse(ctx.SegmentMatches.FirstOrDefault()?.Value, out int column))
					await new ConnectFour().Play(ctx, (byte)column);
			}
		}

		private async Task<bool> ValidateLastRunTime(IInteractionContext context, CurrencyGame gameType)
		{
			// Set variables
			DateTime? guildLastRunTime = null;
			bool runTimeContainsGuild = this.LastRunTimeForType(gameType).ContainsKey(context.Guild.Id);

			if (runTimeContainsGuild)
				guildLastRunTime = this.LastRunTimeForType(gameType)[context.Guild.Id];

			if (runTimeContainsGuild
				&& guildLastRunTime.HasValue
				&& (DateTime.Now - guildLastRunTime.Value).TotalSeconds < GameCoolDownInSeconds)
			{
				double timeToWait = GameCoolDownInSeconds - (DateTime.Now - guildLastRunTime.Value).TotalSeconds;

				await context.Interaction.FollowupAsync("Sorry, you need to wait another " + Math.Floor(timeToWait) + " seconds, _kupo!_");

				await Task.Delay(2000);

				await context.Interaction.DeleteOriginalResponseAsync();

				return false;
			}

			return true;
		}

		private async Task<bool> ValidateDailyCurrencyGameAllowance(IInteractionContext context)
		{
			// Get settings and user
			LeaderboardSettings settings = await LeaderboardSettingsService.GetSettings<LeaderboardSettings>(context.Guild.Id);

			// No games allowed
			if (settings.CurrencyGamesAllowedPerDay == 0)
			{
				await context.Interaction.FollowupAsync("Currency games have been disabled by Server Admin.");

				await Task.Delay(2000);

				await context.Interaction.DeleteOriginalResponseAsync();

				return false;
			}

			// Get count of games played for user
			if (!this.userDailyGameCount.TryGetValue(context.User.Id, out uint gameCount))
				gameCount = 0;

			if (settings.CurrencyGamesAllowedPerDay <= 0 || gameCount >= settings.CurrencyGamesAllowedPerDay)
			{
				// User has reached max games allowed today
				await context.Interaction.FollowupAsync("You cannot play any more games today, _kupo!_");

				await Task.Delay(2000);

				await context.Interaction.DeleteOriginalResponseAsync();

				return false;
			}

			return true;
		}

		private async void UpdateLastRunTime(CurrencyGame gameType, ulong userId, ulong guildId)
		{
			this.LastRunTimeForType(gameType).UpdateOrAdd(guildId, DateTime.Now);

			// Update game count if restricted
			LeaderboardSettings settings = await LeaderboardSettingsService.GetSettings<LeaderboardSettings>(guildId);
			if (settings.CurrencyGamesAllowedPerDay > 0 && settings.CurrencyGamesAllowedPerDay < 2880)
			{
				if (!this.userDailyGameCount.TryGetValue(userId, out uint gameCount))
					gameCount = 0;

				this.userDailyGameCount.UpdateOrAdd(userId, ++gameCount);
			}
		}

		private Dictionary<ulong, DateTime?> LastRunTimeForType(CurrencyGame gameType)
		{
			return gameType switch
			{
				CurrencyGame.Slots => this.slotsLastRunTime,
				CurrencyGame.Blackjack => this.blackjackLastRunTime,
				_ => new Dictionary<ulong, DateTime?>(),
			};
		}

		private async Task<Task> StopInventoryListener(RestUserMessage message)
		{
			// Give the user time to select item
			await Task.Delay(20000);

			// Remove Reacts
			await message.RemoveAllReactionsAsync();

			// Clear handler if no more active inventories
			this.activeInventoryWindows.Remove(message.Id);
			if (this.activeInventoryWindows.Count == 0)
				this.DiscordClient.ReactionAdded -= this.OnReactionAddedInventory;

			return Task.CompletedTask;
		}

		private async Task OnReactionAddedInventory(Cacheable<IUserMessage, ulong> incomingMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			try
			{
				// Don't react to your own reacts!
				if (reaction.UserId == this.DiscordClient.CurrentUser.Id)
					return;

				// Get message and reference (so we know who the message belongs to)
				IUserMessage message = await incomingMessage.DownloadAsync();
				IUserMessage userMessage = message.ReferencedMessage;

				// Only handle reacts from the original user, remove the reaction
				if (userMessage.Author.Id != reaction.UserId)
				{
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
					return;
				}

				// Load shop items
				List<ShopItem> items = await ShopItemDatabase.LoadAll(new Dictionary<string, object>()
				{
					{ "GuildId", userMessage.GetGuild().Id },
					{ "Reaction", reaction.Emote.GetString() },
				});

				// Get shop item
				ShopItem? itemToRedeem = items.FirstOrDefault();

				// Only handle relevant reacts
				if (itemToRedeem == null)
				{
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
					return;
				}

				if (channel.Value is SocketGuildChannel guildChannel)
				{
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					// Get user
					User user = await UserService.GetUser(userMessage.GetGuild().Id, userMessage.Author.Id);

					if (user.Inventory.ContainsKey(itemToRedeem.Name))
					{
						// Remove item from inventory
						await user.UpdateInventory(itemToRedeem.Name, -1);

						// Get the embed and duplicate
						IEmbed? embed = message.Embeds.FirstOrDefault();
						EmbedBuilder redeemEmbed = new EmbedBuilder()
							.WithTitle(embed?.Title)
							.WithColor(embed?.Color ?? Color.Blue)
							.WithDescription($"{itemToRedeem.Name} redeemed, _kupo!_");

						// Modify embed and remove reactions
						await message.ModifyAsync(x => x.Embed = redeemEmbed.Build());
						await message.RemoveAllReactionsAsync();

						// Get role to mention if added
						string mentionedRole = string.Empty;
						if (ulong.TryParse(itemToRedeem.Role, out ulong itemRoleToMention))
						{
							if (itemRoleToMention != 0)
								await message.Channel.SendMessageAsync($"Please attend to this, <@&{itemToRedeem.Role}>", allowedMentions: AllowedMentions.All);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			try
			{
				// Temp removal from pokemeow channel
				if (this.blockedChannels.Contains(message.Channel.Id))
					return;

				if (message.Author.Id == this.DiscordClient.CurrentUser.Id)
					return;

				if (message.Author.IsBot)
					return;

				LeaderboardSettings settings = await LeaderboardSettingsService.GetSettings<LeaderboardSettings>(message.GetGuild().Id);
				if (settings.AllowPassiveCurrencyGain)
				{
					await Task.Run(async () =>
					{
						double roll = new Random().NextDouble();
						if (roll < GenerationChance)
						{
							IMessage iMessage = await message.Channel.GetMessageAsync(message.Id);
							if (iMessage is RestUserMessage restMessage)
							{
								IGuildUser toUser = message.GetAuthor();

								Log.Write(toUser.GetName() + " Found a Kupo Nut with message: \"" + message.Content + "\"", "Bot");

								User user = await UserService.GetUser(toUser);
								await user.UpdateTotalKupoNuts(1);
							}
						}
					});
				}
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

		private Task ClearDailyGameCount()
		{
			// Reset day at UTC Midnight
			TimeSpan now = DateTime.UtcNow.TimeOfDay;
			if (now >= new TimeSpan(23, 50, 0) && now <= new TimeSpan(1, 0, 0))
				this.userDailyGameCount.Select(x => x.Value == 0);

			return Task.CompletedTask;
		}
	}
}
