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
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Bot.Currency;
	using FC.Data;
	using FC.Shop;

	public class CurrencyService : ServiceBase
	{
		public const string KupoNut = "<:kupo_nut:815575569482776607>";

		public static Table<ShopItem> ShopItemDatabase = new Table<ShopItem>("KupoNuts_ShopItems", 0);

		public static IEmote NutEmote = Emote.Parse(KupoNut);

		private const double GenerationChance = 0.05;

		private readonly List<ulong> blockedChannels = new List<ulong>()
		{
			837682896805691473,
			838350357074935838,
			838350425593479208,
			838350518853566474,
		};

		private Dictionary<ulong, DateTime?> activeInventoryWindows = new Dictionary<ulong, DateTime?>();

		// Run times
		private Dictionary<ulong, DateTime?> slotsLastRunTime = new Dictionary<ulong, DateTime?>();
		private Dictionary<ulong, DateTime?> blackjackLastRunTime = new Dictionary<ulong, DateTime?>();

		public override async Task Initialize()
		{
			await base.Initialize();

			await ShopItemDatabase.Connect();

			Program.DiscordClient.MessageReceived += this.OnMessageReceived;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.MessageReceived -= this.OnMessageReceived;
			return base.Shutdown();
		}

		[Command("NL", Permissions.Everyone, "Shows the kupo nut leader boards", CommandCategory.Currency, "NutLeadboard")]
		[Command("NutLeadboard", Permissions.Everyone, "Shows the kupo nut leader boards", CommandCategory.Currency)]
		public async Task<Embed> ShowLeaders(CommandMessage message)
		{
			IGuild guild = message.Guild;

			List<User> users = await UserService.GetAllUsersForGuild(guild.Id);

			users.Sort((User a, User b) =>
			{
				return -a.TotalKupoNutsCurrent.CompareTo(b.TotalKupoNutsCurrent);
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

				builder.Append(user.TotalKupoNutsCurrent);
				builder.Append(" - ");
				builder.AppendLine(discordUser.GetName());

				count++;
			}

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Title = "Kupo Nut Leaderboard";
			embedBuilder.Description = builder.ToString();
			return embedBuilder.Build();
		}

		[Command("Nuts", Permissions.Everyone, "Check your pile of Kupo Nuts", CommandCategory.Currency)]
		public async Task<Embed> Nuts(CommandMessage message)
		{
			// Get user information
			User user = await UserService.GetUser(message.Author);
			string userName = message.Author.GetName();

			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = userName + "'s Kupo Nut Stash!";

			StringBuilder description = new StringBuilder();
			description.AppendLine("Current Total: " + user.TotalKupoNutsCurrent);
			description.AppendLine("Total Held: " + user.TotalKupoNutsReceived);
			builder.Description = description.ToString();

			// Remove calling command
			await message.Channel.DeleteMessageAsync(message.Message);

			return builder.Build();
		}

		[Command("DN", Permissions.Everyone, "Get your daily nut.", CommandCategory.Currency, "DailyNuts")]
		[Command("DailyNut", Permissions.Everyone, "Get your daily nut.", CommandCategory.Currency, "DailyNuts")]
		[Command("DailyNuts", Permissions.Everyone, "Get your daily nut.", CommandCategory.Currency)]
		public async void DailyNuts(CommandMessage message)
		{
			// Get user information
			User user = await UserService.GetUser(message.Author);
			string userName = message.Author.GetName();

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
					TimeSpan timeToWait = dailyReset.AddDays(1) - user.LastDailyNut.Value;
					postBackMessage = string.Format("Greedy {0}! You can have more Kupo Nuts in {1} minutes, {2} seconds, _kupo!_", userName, Math.Floor(timeToWait.TotalMinutes), timeToWait.Seconds);

					// Delete command message
					await message.Channel.DeleteMessageAsync(message.Message);

					RestUserMessage failedMessage = await message.Channel.SendMessageAsync(postBackMessage);

					await Task.Delay(10000);

					await failedMessage.DeleteAsync();

					return;
				}
			}

			// Generate random nuts
			int nutsReceieved = new Random().Next(10, 26);

			// Save to user
			user.UpdateTotalKupoNuts(nutsReceieved, true);

			postBackMessage = string.Format("{0}, please enjoy these {1} Kupo Nuts! _Kupo!_", userName, nutsReceieved);

			// Delete command message
			await message.Channel.DeleteMessageAsync(message.Message);

			// Send message
			await message.Channel.SendMessageAsync(postBackMessage);
		}

		[Command("GiveNuts", Permissions.Everyone, "Share your Kupo Nuts with someone.", CommandCategory.Currency)]
		public async Task<string> GiveNuts(CommandMessage message, int numberOfNuts, IGuildUser user)
		{
			// Message for return
			string postBackMessage = string.Empty;

			// Get sender user name
			string fromUserName = message.Author.GetName();

			// Handle bots
			if (user.IsBot)
			{
				if (user.Id == Program.DiscordClient.CurrentUser.Id)
				{
					postBackMessage = string.Format("Thanks {0} but I have plenty of Kupo Nuts. You hang onto these, _kupo!_", fromUserName);
				}
				else
				{
					postBackMessage = string.Format("That Bot has no need for delicious Kupo Nuts!");
				}

				return await Task.FromResult(postBackMessage);
			}

			// Get sending user information
			User fromUser = await UserService.GetUser(message.Author);

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
					fromUser.UpdateTotalKupoNuts(-numberOfNuts);
					toUser.UpdateTotalKupoNuts(numberOfNuts);

					postBackMessage = string.Format("Lucky {0}, {1} has given you {2} Kupo Nuts!", toUserName, fromUserName, numberOfNuts);
				}
			}
			else
			{
				postBackMessage = string.Format("You only have {0} Kupo Nuts to give!", fromUser.TotalKupoNutsCurrent);
			}

			return await Task.FromResult(postBackMessage);
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
			Program.DiscordClient.ReactionAdded += this.OnReactionAddedInventory;

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

		[Command("Slots", Permissions.Everyone, "Spin it to win it!", CommandCategory.Currency, showWait: false)]
		public async Task<Task> Slots(CommandMessage message)
		{
			// Get last run for guild
			DateTime? guildLastRunTime = null;
			if (this.slotsLastRunTime.ContainsKey(message.Guild.Id))
			{
				guildLastRunTime = this.slotsLastRunTime[message.Guild.Id];
			}

			if (this.slotsLastRunTime.ContainsKey(message.Guild.Id)
				&& guildLastRunTime.HasValue
				&& (DateTime.Now - guildLastRunTime.Value).TotalMinutes < 1)
			{
				double timeToWait = 60 - (DateTime.Now - guildLastRunTime.Value).TotalSeconds;

				IUserMessage response = await message.Channel.SendMessageAsync("Sorry, you need to wait another " + Math.Floor(timeToWait) + " seconds, _kupo!_");

				await Task.Delay(2000);

				await message.Channel.DeleteMessageAsync(message.Id);
				await response.DeleteAsync();
			}
			else
			{
				User user = await UserService.GetUser(message.Author);
				if (user.TotalKupoNutsCurrent > 10)
				{
					user.UpdateTotalKupoNuts(-10);

					if (!this.slotsLastRunTime.ContainsKey(message.Guild.Id))
					{
						this.slotsLastRunTime.Add(message.Guild.Id, DateTime.Now);
					}
					else
					{
						this.slotsLastRunTime[message.Guild.Id] = DateTime.Now;
					}

					return await new Slots().StartSlot(message);
				}
				else
				{
					IUserMessage response = await message.Channel.SendMessageAsync("You must have 10 Kupo Nuts to play the Slots, _kupo!_");

					await Task.Delay(2000);

					await message.Channel.DeleteMessageAsync(message.Id);
					await response.DeleteAsync();
				}
			}

			return Task.CompletedTask;
		}

		[Command("BJ", Permissions.Everyone, "Play a hand of blackjack!", CommandCategory.Currency, "Blackjack", showWait: false)]
		[Command("Bjack", Permissions.Everyone, "Play a hand of blackjack!", CommandCategory.Currency, "Blackjack", showWait: false)]
		[Command("Blackjack", Permissions.Everyone, "Play a hand of blackjack!", CommandCategory.Currency, showWait: false)]
		public async Task<Task> Blackjack(CommandMessage message)
		{
			// Get last run for guild
			DateTime? guildLastRunTime = null;
			if (this.blackjackLastRunTime.ContainsKey(message.Guild.Id))
			{
				guildLastRunTime = this.blackjackLastRunTime[message.Guild.Id];
			}

			if (this.blackjackLastRunTime.ContainsKey(message.Guild.Id)
				&& guildLastRunTime.HasValue
				&& (DateTime.Now - guildLastRunTime.Value).TotalSeconds < 30)
			{
				double timeToWait = 30 - (DateTime.Now - guildLastRunTime.Value).TotalSeconds;

				IUserMessage response = await message.Channel.SendMessageAsync("Sorry, you need to wait another " + Math.Floor(timeToWait) + " seconds, _kupo!_");

				await Task.Delay(2000);

				await message.Channel.DeleteMessageAsync(message.Id);
				await response.DeleteAsync();
			}
			else
			{
				User user = await UserService.GetUser(message.Author);
				if (user.TotalKupoNutsCurrent >= 10)
				{
					user.UpdateTotalKupoNuts(-10);

					if (!this.blackjackLastRunTime.ContainsKey(message.Guild.Id))
					{
						this.blackjackLastRunTime.Add(message.Guild.Id, DateTime.Now);
					}
					else
					{
						this.blackjackLastRunTime[message.Guild.Id] = DateTime.Now;
					}

					return await new Blackjack(10).StartBlackjack(message);
				}
				else
				{
					IUserMessage response = await message.Channel.SendMessageAsync("You must have 10 Kupo Nuts to play Blackjack, _kupo!_");

					await Task.Delay(2000);

					await message.Channel.DeleteMessageAsync(message.Id);
					await response.DeleteAsync();
				}
			}

			return Task.CompletedTask;
		}

		[Command("BJ", Permissions.Everyone, "Play a hand of blackjack!", CommandCategory.Currency, "Blackjack", showWait: false)]
		[Command("Bjack", Permissions.Everyone, "Play a hand of blackjack!", CommandCategory.Currency, "Blackjack", showWait: false)]
		[Command("Blackjack", Permissions.Everyone, "Play a hand of blackjack!", CommandCategory.Currency, showWait: false)]
		public async Task<Task> Blackjack(CommandMessage message, uint bet)
		{
			// Get last run for guild
			DateTime? guildLastRunTime = null;
			if (this.blackjackLastRunTime.ContainsKey(message.Guild.Id))
			{
				guildLastRunTime = this.blackjackLastRunTime[message.Guild.Id];
			}

			if (this.blackjackLastRunTime.ContainsKey(message.Guild.Id)
				&& guildLastRunTime.HasValue
				&& (DateTime.Now - guildLastRunTime.Value).TotalSeconds < 30)
			{
				double timeToWait = 30 - (DateTime.Now - guildLastRunTime.Value).TotalSeconds;

				IUserMessage response = await message.Channel.SendMessageAsync("Sorry, you need to wait another " + Math.Floor(timeToWait) + " seconds, _kupo!_");

				await Task.Delay(2000);

				await message.Channel.DeleteMessageAsync(message.Id);
				await response.DeleteAsync();
			}
			else
			{
				User user = await UserService.GetUser(message.Author);
				if (user.TotalKupoNutsCurrent >= bet && bet < int.MaxValue)
				{
					int nutsToSubtract = Convert.ToInt32(bet) * -1;
					user.UpdateTotalKupoNuts(nutsToSubtract);

					if (!this.blackjackLastRunTime.ContainsKey(message.Guild.Id))
					{
						this.blackjackLastRunTime.Add(message.Guild.Id, DateTime.Now);
					}
					else
					{
						this.blackjackLastRunTime[message.Guild.Id] = DateTime.Now;
					}

					return await new Blackjack(bet).StartBlackjack(message);
				}
				else
				{
					IUserMessage response = await message.Channel.SendMessageAsync($"You don't have {bet} Kupo Nuts to play Blackjack, _kupo!_");

					await Task.Delay(2000);

					await message.Channel.DeleteMessageAsync(message.Id);
					await response.DeleteAsync();
				}
			}

			return Task.CompletedTask;
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
				Program.DiscordClient.ReactionAdded -= this.OnReactionAddedInventory;

			return Task.CompletedTask;
		}

		private async Task OnReactionAddedInventory(Cacheable<IUserMessage, ulong> incomingMessage, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				// Don't react to your own reacts!
				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
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
				ShopItem itemToRedeem = items.FirstOrDefault();

				// Only handle relevant reacts
				if (itemToRedeem == null)
				{
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
					return;
				}

				if (channel is SocketGuildChannel guildChannel)
				{
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					// Get user
					User user = await UserService.GetUser(userMessage.GetGuild().Id, userMessage.Author.Id);

					if (user.Inventory.ContainsKey(itemToRedeem.Name))
					{
						// Remove item from inventory
						user.UpdateInventory(itemToRedeem.Name, -1);

						// Get the embed and duplicate
						IEmbed embed = message.Embeds.FirstOrDefault();
						EmbedBuilder redeemEmbed = new EmbedBuilder()
							.WithTitle(embed.Title)
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

							////mentionedRole = $"Let <@&{itemToRedeem.Role}> know about it!";
							////IRole role = message.GetGuild().GetRole(itemRoleToMention));
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

							Log.Write(toUser.GetName() + " Found a Kupo Nut with message: \"" + message.Content + "\"", "Bot");

							User user = await UserService.GetUser(toUser);
							user.UpdateTotalKupoNuts(1);
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
