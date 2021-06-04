// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Currency
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
	using FC.Bot.Services;
	using FC.Data;
	using FC.Shop;

	public class Shop
	{
		public const string StoreTitle = "Welcome to the Kupo Nut Store!";
		public const string KupoNut = @"<:kupo_nut:815575569482776607>";
		public const string GoldKupoNut = @"<:kupo_nut_gold:820579501469597697>";
		public const string Chub = @"<:Chub:820582269596073984>";

		////public static IEmote NutEmote = Emote.Parse(KupoNut);
		////public static IEmote GoldNutEmote = Emote.Parse(GoldKupoNut);
		////public static IEmote ChubEmote = Emote.Parse(Chub);

		private static List<ShopItem> shopItems = new List<ShopItem>();
		private static List<IEmote> shopEmotes = new List<IEmote>();

		private static Dictionary<ulong, ulong> activeShops = new Dictionary<ulong, ulong>();

		public Shop(List<ShopItem> items)
		{
			shopItems = new List<ShopItem>();

			// Default items
			shopItems.AddRange(new List<ShopItem>()
			{
				new ShopItem("Kupo Nut", "A favourite of the Moogles, _kupo!_", 1, KupoNut),
				new ShopItem("Chub", "Are you sure you want this, _kupo?_", 50, Chub),
				new ShopItem("Gold Kupo Nut", "So shiny!", 200, GoldKupoNut),
				////new Item("Test Item", "Testing", 999999, new Emoji("🚧")),
			});

			// Guild items
			shopItems.AddRange(items);

			// Get emotes
			shopEmotes = shopItems.Select(x => x.ReactionEmote).ToList();
		}

		public async Task<Task> DisplayShop(CommandMessage message)
		{
			// Initial builder information
			EmbedBuilder embed = new EmbedBuilder();
			embed.Title = StoreTitle;
			embed.Color = Color.LightOrange;

			// Build store front
			StringBuilder builder = new StringBuilder();

			foreach (ShopItem item in shopItems)
			{
				// Add Item name
				builder.Append(item.ReactionEmote.GetString() + " " + "**" + item.Name + "**");

				// Add Cost
				builder.AppendLine(" - " + item.Cost + " " + KupoNut);

				// Add description
				builder.Append(Utils.Characters.Tab);
				builder.AppendLine("*" + item.Description + "*");
				builder.AppendLine();
			}

			builder.AppendLine(Utils.Characters.Tab);

			embed.Description = builder.ToString();

			User user = await UserService.GetUser(message.Author);
			embed.WithFooter("You have " + user.TotalKupoNutsCurrent + " Kupo Nuts to spend");

			RestUserMessage userMessage = await message.Channel.SendMessageAsync(embed: embed.Build(), messageReference: message.MessageReference);

			// Add message to active shops
			activeShops.Add(userMessage.Id, message.Author.Id);

			await userMessage.AddReactionsAsync(shopItems.Select(x => x.ReactionEmote).ToArray());

			Program.DiscordClient.ReactionAdded += OnReactionAdded;

			// Stop shop after delay
			_ = Task.Run(async () => await StopShopListener(userMessage));

			return Task.CompletedTask;
		}

		private static async Task<Task> StopShopListener(RestUserMessage message)
		{
			// TODO: Eventually there will be multiple pages for items
			// TODO: This will have to be updated to refresh the time delay if user is navigating

			// Give the user time to select item
			await Task.Delay(10000);

			// Remove from active shops
			activeShops.Remove(message.Id);

			if (activeShops.Count == 0)
				Program.DiscordClient.ReactionAdded -= OnReactionAdded;

			// Remove reactions and replace with success
			Embed embed = GetSuccessEmbed();
			await message.RemoveAllReactionsAsync();
			await message.ModifyAsync(x => x.Embed = embed);

			return Task.CompletedTask;
		}

		private static Embed GetSuccessEmbed()
		{
			EmbedBuilder embed = new EmbedBuilder();

			embed.Title = StoreTitle;
			embed.WithDescription("Thanks for shopping!");

			return embed.Build();
		}

		private static Embed GetFailureEmbed(string userName, string itemName = "this")
		{
			EmbedBuilder embed = new EmbedBuilder();

			embed.Title = StoreTitle;
			embed.WithDescription("Sorry, " + userName + "! You do not have enough Kupo Nuts for " + itemName + ".");

			return embed.Build();
		}

		private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> incomingMessage, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				// Don't react to your own reacts!
				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return;

				// Only handle reacts to shop embed
				if (!activeShops.ContainsKey(incomingMessage.Id))
					return;

				// Only handle reacts from the original user, remove the reaction
				if (activeShops[incomingMessage.Id] != reaction.UserId)
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					return;
				}

				// Only handle relevant reacts
				if (!shopEmotes.Contains(reaction.Emote))
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					return;
				}

				if (channel is SocketGuildChannel guildChannel)
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					// Try to get the purchasing item
					ShopItem itemToBuy = shopItems.FirstOrDefault(x => x.ReactionEmote.GetString() == reaction.Emote.GetString());

					User user = await UserService.GetUser(guildChannel.Guild.Id, reaction.UserId);

					if (user.TotalKupoNutsCurrent >= itemToBuy.Cost)
					{
						// Take payment
						user.UpdateTotalKupoNuts(-itemToBuy.Cost);

						// Add to inventory
						user.UpdateInventory(itemToBuy.Name, 1);

						// Convert to success embed
						await message.ModifyAsync(x => x.Embed = GetSuccessEmbed());

						await Task.Delay(5000);

						// Delete shop message
						await message.DeleteAsync();

						// Delete calling command
						if (message.ReferencedMessage != null)
							await message.ReferencedMessage.DeleteAsync();
					}
					else
					{
						SocketGuildUser failUser = guildChannel.GetUser(reaction.UserId);

						// Convert message to failure embed
						Embed embed = GetFailureEmbed(failUser.GetName(), "a " + itemToBuy.Name);
						await message.ModifyAsync(x => x.Embed = embed);

						await Task.Delay(5000);
						await message.DeleteAsync();
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
